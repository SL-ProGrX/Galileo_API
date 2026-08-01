using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace Galileo_API.DataBaseTier
{
    public class MRecibos
    {
        private readonly PortalDB _portalDB;
        private readonly MReportingServicesDB _reportingServicesDB;
        private readonly IConfiguration _config;

        public MRecibos(IConfiguration config)
        {
            _config = config;
            _portalDB = new PortalDB(config);
            _reportingServicesDB = new MReportingServicesDB(config);
        }

        private static ErrorDto<object> CreateOkObjectResponse()
            => DbHelper.CreateOkResponse<object>(null);

        private static void ApplyException(ErrorDto<object> response, Exception ex)
        {
            response.Code = -1;
            response.Description = ex.Message;
        }

        private static void ApplyReporteActionResult(ErrorDto<object> response, object actionResult)
        {
            var objectResult = actionResult as ObjectResult;

            if (objectResult == null)
            {
                response.Result = JsonConvert.SerializeObject(actionResult, Formatting.Indented);
                return;
            }

            var res = objectResult.Value;
            var json = System.Text.Json.JsonSerializer.Serialize(res);
            var err = System.Text.Json.JsonSerializer.Deserialize<ErrorDto<object>>(json);

            if (err != null)
            {
                response.Code = err.Code;
                response.Description = err.Description;
                response.Result = err.Result;
            }
            else
            {
                response.Code = -1;
                response.Description = "ErrorDto deserialization returned null.";
            }
        }

        public long FxDocumentoConsecutivo(int codEmpresa, string vTipo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            const string qSys = "SELECT ISNULL(SysDocVersion,0) FROM SIF_EMPRESA WHERE PORTAL_ID = @codEmpresa;";
            int sysDocVersion = conn.QueryFirstOrDefault<int>(qSys, new { codEmpresa });

            string tipo = (vTipo ?? string.Empty).Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(tipo))
            {
                throw new InvalidOperationException("El tipo de documento es requerido.");
            }

            if (sysDocVersion == 1)
            {
                long consecutivo = ObtenerConsecutivoAse(conn, tipo);

                if (consecutivo <= 0)
                {
                    throw new InvalidOperationException(
                        $"No existe consecutivo configurado en ASE_CONSECUTIVOS para el tipo {tipo}.");
                }

                IncrementarConsecutivoAse(conn, tipo);
                return consecutivo;
            }

            const string sp = "exec dbo.spSIFDocsConsecutivo @Tipo;";
            long cons = conn.QueryFirstOrDefault<long>(sp, new
            {
                Tipo = tipo
            });

            if (cons <= 0)
            {
                throw new InvalidOperationException(
                    $"No se obtuvo consecutivo para el tipo de documento {tipo}.");
            }

            return cons;
        }

        private static long ObtenerConsecutivoAse(SqlConnection conn, string tipo)
        {
            return tipo switch
            {
                "RE" => conn.QueryFirstOrDefault<long>(
                    "select isnull(CS_RECIBO,0) as Consecutivo from ASE_CONSECUTIVOS"),
                "DP" => conn.QueryFirstOrDefault<long>(
                    "select isnull(CS_DEPOSITO,0) as Consecutivo from ASE_CONSECUTIVOS"),
                "ND" => conn.QueryFirstOrDefault<long>(
                    "select isnull(CS_NOTA_DEBITO,0) as Consecutivo from ASE_CONSECUTIVOS"),
                "NC" => conn.QueryFirstOrDefault<long>(
                    "select isnull(CS_NOTA_CREDITO,0) as Consecutivo from ASE_CONSECUTIVOS"),
                _ => throw new InvalidOperationException(
                    $"Tipo de documento no válido para SysDocVersion 1: {tipo}.")
            };
        }

        private static void IncrementarConsecutivoAse(SqlConnection conn, string tipo)
        {
            string sql = tipo switch
            {
                "RE" => "update ASE_CONSECUTIVOS set CS_RECIBO = isnull(CS_RECIBO,0) + 1",
                "DP" => "update ASE_CONSECUTIVOS set CS_DEPOSITO = isnull(CS_DEPOSITO,0) + 1",
                "ND" => "update ASE_CONSECUTIVOS set CS_NOTA_DEBITO = isnull(CS_NOTA_DEBITO,0) + 1",
                "NC" => "update ASE_CONSECUTIVOS set CS_NOTA_CREDITO = isnull(CS_NOTA_CREDITO,0) + 1",
                _ => throw new InvalidOperationException(
                    $"Tipo de documento no válido para SysDocVersion 1: {tipo}.")
            };

            conn.Execute(sql);
        }


        /// <summary>Genera el recibo usando la versión documental configurada.</summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="pDocumento">Número del documento.</param>
        /// <param name="pTipo">Tipo de documento.</param>
        /// <param name="Usuario">Usuario que solicita el reporte.</param>
        /// <param name="pReImprime">Indica si corresponde a una reimpresión.</param>
        /// <param name="pFolder">Carpeta lógica donde se encuentran los RDL.</param>
        /// <returns>Archivo generado o información del error.</returns>
        public ErrorDto<object> sbImprimeRecibo(
            int CodEmpresa,
            string pDocumento,
            string pTipo,
            string Usuario,
            bool pReImprime = false,
            string pFolder = "Sys")
        {
                var response = CreateOkObjectResponse();

                try
                {
                    var empresaEnlace = new MProGrxMain(_config).EmpresaEnlaceObtener();
                    int SysDocVersion = empresaEnlace?.FirstOrDefault()?.SysDocVersion ?? 0;

                    response = SysDocVersion == 1
                        ? sbImprimev1(CodEmpresa, pDocumento, pTipo, pReImprime, pFolder)
                        : sbImprimev2(CodEmpresa, pTipo, pDocumento, Usuario, pReImprime, pFolder);
                }
                catch (Exception ex)
                {
                    ApplyException(response, ex);
                }

                return response;
        }

        /// <summary>Genera un recibo con el esquema documental versión 1.</summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="pDocumento">Número del documento.</param>
        /// <param name="pTipo">Tipo de documento.</param>
        /// <param name="pReImprime">Indica si corresponde a una reimpresión.</param>
        /// <param name="pFolder">Carpeta lógica donde se encuentran los RDL.</param>
        /// <returns>Archivo generado o información del error.</returns>
        public ErrorDto<object> sbImprimev1(
            int CodEmpresa,
            string pDocumento,
            string pTipo,
            bool pReImprime = false,
            string pFolder = "Sys")
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = CreateOkObjectResponse();
            bool vFlat = false;
            try
            {
                pTipo = fxTipoASEDoc(pTipo);
                using var connection = new SqlConnection(stringConn);
                    var query = "select cs_utilizar_reciboFlat as Flat from ase_consecutivos";
                    vFlat = connection.QueryFirstOrDefault<string>(query) == "S";

                    query = "select nombre,cedula_juridica from sif_empresa";
                    var vEmpresa = connection.QueryFirstOrDefault(query);

                    if (!pReImprime)
                    {
                        query = "Update ASE_DOCUMENTOS set Estado='I' Where ID_DOCUMENTO = @pDocumento and tipo = @pTipo";
                        connection.Execute(query, new { pDocumento, pTipo });
                    }

                    //Imprime reporte
                    FrmReporteGlobal reporteData = new FrmReporteGlobal
                    {
                        codEmpresa = CodEmpresa,
                        cod_reporte = "P",
                        folder = pFolder
                    };

                    string selectionFormula = $" where ASE_DOCUMENTOS.ID_DOCUMENTO = {pDocumento} AND ASE_DOCUMENTOS.TIPO = '{pTipo}'";

                    if (pTipo == "RE")
                    {
                        if (vFlat)
                        {
                            reporteData.nombreReporte = "Sys_DocumentoFlat";
                            reporteData.parametros = JsonConvert.SerializeObject(
                                new {
                                    filtros = selectionFormula,
                                    Empresa = vEmpresa?.nombre ?? string.Empty,
                                    CedJur = vEmpresa?.cedula_juridica ?? string.Empty
                                });
                        }
                        else
                        {
                            reporteData.nombreReporte = "Sys_DocumentoCls";
                            reporteData.parametros = JsonConvert.SerializeObject(
                                new { filtros = selectionFormula });
                        }
                    } 
                    else
                    {
                        reporteData.nombreReporte = "Sys_DocumentoBoleta";
                        reporteData.parametros = JsonConvert.SerializeObject(
                                new { filtros = selectionFormula });
                    }
                    
                    var actionResult = _reportingServicesDB.ReporteRDLC_v2(reporteData);

                    ApplyReporteActionResult(response, actionResult);
            }
            catch (Exception ex)
            {
                ApplyException(response, ex);
            }
            return response;
        }

        /// <summary>Genera un recibo con el esquema documental versión 2.</summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="pTipo">Tipo de documento.</param>
        /// <param name="pTransaccion">Número de transacción.</param>
        /// <param name="Usuario">Usuario que solicita el reporte.</param>
        /// <param name="pReImprime">Indica si corresponde a una reimpresión.</param>
        /// <param name="pFolder">Carpeta lógica donde se encuentran los RDL.</param>
        /// <returns>Archivo generado o información del error.</returns>
        public ErrorDto<object> sbImprimev2(
            int CodEmpresa,
            string pTipo,
            string pTransaccion,
            string Usuario,
            bool pReImprime = false,
            string pFolder = "Sys")
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = CreateOkObjectResponse();
            try
            {
                using var connection = new SqlConnection(stringConn);

                var vDocumento = connection.QueryFirstOrDefault(
                    "select Tipo_Comprobante,isnull(ARCHIVO_PER,'') as 'ARCHIVO',FORMATO_SALIDA from sif_documentos where tipo_documento = @pTipo",
                    new { pTipo });

                 bool vFlat = vDocumento?.FORMATO_SALIDA == "01";
                string vArchivo = GetReportName(vDocumento);

                var vEmpresa = connection.QueryFirstOrDefault("select nombre,cedula_juridica from sif_empresa");
                string empresaNombre = vEmpresa?.nombre ?? string.Empty;
                string empresaCedJur = vEmpresa?.cedula_juridica ?? string.Empty;

                if (!pReImprime)
                {
                    connection.Execute(
                        "Update SIF_TRANSACCIONES set Estado='I' Where COD_TRANSACCION = @pTransaccion and tipo_documento = @pTipo",
                        new { pTransaccion, pTipo });
                }

                FrmReporteGlobal reporteData = new FrmReporteGlobal
                {
                    codEmpresa = CodEmpresa,
                    cod_reporte = "P",
                    folder = pFolder,
                    nombreReporte = vArchivo,
                    parametros = BuildReportParams(vFlat, pTransaccion, pTipo, empresaNombre, empresaCedJur, Usuario)
                };

                var actionResult = _reportingServicesDB.ReporteRDLC_v2(reporteData);
                ApplyReporteActionResult(response, actionResult);
            }
            catch (Exception ex)
            {
                ApplyException(response, ex);
            }
            return response;
        }

        private const string DefaultBoletaReport = "Sys_DocumentoBoletav2";

        private static string GetReportName(dynamic vDocumento)
        {
            if (vDocumento == null)
                return DefaultBoletaReport;

            switch (vDocumento.Tipo_Comprobante)
            {
                case "00":
                    return DefaultBoletaReport;
                case "01":
                    return "Sys_DocumentoFlatv2";
                case "02":
                    return !string.IsNullOrEmpty(vDocumento.archivo) ? vDocumento.archivo : DefaultBoletaReport;
                default:
                    return DefaultBoletaReport;
            }
        }

        private static string BuildReportParams(bool vFlat, string pTransaccion, string pTipo, string empresaNombre, string empresaCedJur, string Usuario)
        {
            string selectionFormula = $" where SIF_TRANSACCIONES.COD_TRANSACCION = '{pTransaccion}' AND SIF_TRANSACCIONES.TIPO_DOCUMENTO = '{pTipo}'";
            if (vFlat)
            {
                return JsonConvert.SerializeObject(new
                {
                    filtros = selectionFormula,
                    Empresa = empresaNombre,
                    Usuario,
                    CedJur = empresaCedJur
                });
            }
            else
            {
                return JsonConvert.SerializeObject(new
                {
                    filtros = selectionFormula,
                    Empresa = empresaNombre,
                    Usuario
                });
            }
        }

        public static string fxTipoASEDoc(string vTipo)
        {
            string tipo = vTipo.Trim().ToUpper();

            switch (tipo)
            {
                case "RECIBO" or "RECIBOS":
                    return "RE";
                case "DEPOSITO" or "DEPOSITOS":
                    return "DP";
                case "NOTA CREDITO" or "NOTA DE CRÉDITO":
                    return "NC";
                case "NOTA DEBITO" or "NOTA DE DÉBITO":
                    return "ND";
                default:
                    return vTipo;
            }
        }

        public string FxDocumentoCuenta(int codEmpresa, string vTipo)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            try
            {
                var empresaEnlace = new MProGrxMain(_config).EmpresaEnlaceObtener();
                int sysDocVersion = empresaEnlace?.FirstOrDefault()?.SysDocVersion ?? 0;

                string tipo = (vTipo ?? string.Empty).Trim().ToUpperInvariant();

                if (sysDocVersion == 1)
                {
                    string? sql = tipo switch
                    {
                        "RE" => "select CS_RE_CUENTA as cuenta from ase_consecutivos",
                        "DP" => "select CS_DP_CUENTA as cuenta from ase_consecutivos",
                        "ND" => "select CS_ND_CUENTA as cuenta from ase_consecutivos",
                        "NC" => "select CS_NC_CUENTA as cuenta from ase_consecutivos",
                        _ => null
                    };

                    if (string.IsNullOrWhiteSpace(sql))
                    {
                        return string.Empty;
                    }

                    return (conn.QueryFirstOrDefault<string>(sql) ?? string.Empty).Trim();
                }

                const string sqlV2 = @"
            SELECT COD_CUENTA
            FROM SIF_DOCUMENTOS
            WHERE tipo_documento = @tipo";

                return (conn.QueryFirstOrDefault<string>(sqlV2, new { tipo }) ?? string.Empty).Trim();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
