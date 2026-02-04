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
        private readonly IConfiguration _config;
        private readonly MReportingServicesDB _reportingServicesDB;

        private MReportingServicesDBDependencies? deps;

        public MRecibos(IConfiguration config)
        {
            _config = config;
            _reportingServicesDB = new MReportingServicesDB(config);
            deps = null;
        }


        public ErrorDto<object> sbImprimeRecibo(int CodEmpresa, string pDocumento, string pTipo, string Usuario, bool pReImprime = false)
        {
            var response = new ErrorDto<object>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };
            try
            {
                var empresaEnlace = new MProGrxMain(_config).EmpresaEnlaceObtener();
                int SysDocVersion = empresaEnlace?.FirstOrDefault()?.SysDocVersion ?? 0;
                if (SysDocVersion == 1)
                {
                    response = sbImprimev1(CodEmpresa, pDocumento, pTipo, pReImprime);
                } 
                else
                {
                    response = sbImprimev2(CodEmpresa, pTipo, pDocumento, Usuario, pReImprime);
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        public ErrorDto<object> sbImprimev1(int CodEmpresa, string pDocumento, string pTipo, bool pReImprime = false)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<object>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };
            bool vFlat = false;
            try
            {
                pTipo = fxTipoASEDoc(pTipo);
                using var connection = new SqlConnection(stringConn);
                {
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
                        folder = "Sys"
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
                                    Empresa = vEmpresa.nombre,
                                    CedJur = vEmpresa.cedula_juridica
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

                    //Valida respuesta de ReporteRDLC_v2
                    var objectResult = actionResult as ObjectResult;

                    if (objectResult == null)
                    {
                        response.Result = JsonConvert.SerializeObject(actionResult, Formatting.Indented);
                    }
                    else
                    {
                        var res = objectResult.Value;
                        //converto res a JSON
                        var Jres = System.Text.Json.JsonSerializer.Serialize(res);

                        // convierto JSON a ErrorDTO
                        var err = System.Text.Json.JsonSerializer.Deserialize<ErrorDto>(Jres);

                        response.Code = err.Code;
                        response.Description = err.Description;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        public ErrorDto<object> sbImprimev2(int CodEmpresa, string pTipo, string pTransaccion, string Usuario, bool pReImprime = false)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<object>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };
            bool vFlat = false;
            string vArchivo = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select Tipo_Comprobante,isnull(ARCHIVO_PER,'') as 'ARCHIVO',FORMATO_SALIDA from sif_documentos where tipo_documento = @pTipo";
                    var vDocumento = connection.QueryFirstOrDefault(query, new { pTipo }); 
                    vFlat = vDocumento.FORMATO_SALIDA == "01";

                    switch(vDocumento.Tipo_Comprobante)
                    {
                        case "00": //Boleta
                            vArchivo = "Sys_DocumentoBoletav2";
                            break;
                        case "01": //Recibo General
                            vArchivo = "Sys_DocumentoFlatv2";
                            break;
                        case "02": //Archivo Personalizado
                            if (vDocumento.archivo != "")
                            {
                                vArchivo = vDocumento.archivo;
                            } else {
                                vArchivo = "Sys_DocumentoBoletav2";
                            }
                            break;
                    }

                    query = "select nombre,cedula_juridica from sif_empresa";
                    var vEmpresa = connection.QueryFirstOrDefault(query);

                    if (!pReImprime)
                    {
                        query = "Update SIF_TRANSACCIONES set Estado='I' Where COD_TRANSACCION = @pTransaccion and tipo_documento = @pTipo";
                        connection.Execute(query, new { pTransaccion, pTipo });
                    }

                    //Imprime reporte
                    FrmReporteGlobal reporteData = new FrmReporteGlobal
                    {
                        codEmpresa = CodEmpresa,
                        cod_reporte = "P",
                        folder = "Sys",
                        nombreReporte = vArchivo
                    };

                    string selectionFormula = $" where SIF_TRANSACCIONES.COD_TRANSACCION = '{pTransaccion}' AND SIF_TRANSACCIONES.TIPO_DOCUMENTO = '{pTipo}'";
                    if (vFlat)
                    {
                        reporteData.parametros = JsonConvert.SerializeObject(
                                new
                                {
                                    filtros = selectionFormula,
                                    Empresa = vEmpresa.nombre,
                                    Usuario,
                                    CedJur = vEmpresa.cedula_juridica
                                });
                    }
                    else
                    {
                        reporteData.parametros = JsonConvert.SerializeObject(
                                new
                                {
                                    filtros = selectionFormula,
                                    Empresa = vEmpresa.nombre,
                                    Usuario
                                });
                    }

                    var actionResult = _reportingServicesDB.ReporteRDLC_v2(reporteData);

                    //Valida respuesta de ReporteRDLC_v2
                    var objectResult = actionResult as ObjectResult;

                    if (objectResult == null)
                    {
                        response.Result = JsonConvert.SerializeObject(actionResult, Formatting.Indented);
                    }
                    else
                    {
                        var res = objectResult.Value;
                        //converto res a JSON
                        var Jres = JsonConvert.SerializeObject(res, Formatting.Indented);

                        // convierto JSON a ErrorDTO
                        var err = System.Text.Json.JsonSerializer.Deserialize<ErrorDto>(Jres);

                        response.Code = err.Code;
                        response.Description = err.Description;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
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
   
   


        public long fxDocumentoConsecutivo(int codEmpresa, string vTipo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            using var conn = new SqlConnection(stringConn);
            try
            {
                string strCampo = "", strUpdate = "";
                var query = "SELECT SysDocVersion FROM SIF_EMPRESA WHERE PORTAL_ID = @codEmpresa";
                if(conn.Query<int>(query, codEmpresa).FirstOrDefault() == 1)
                {
                    switch (vTipo)
                    {
                        case "RE":
                            strCampo = "select CS_RECIBO as Consecutivo from ase_consecutivos";
                            strUpdate = "update ase_consecutivos set CS_RECIBO = CS_RECIBO + 1";
                            break;
                        case "DP":
                            strCampo = "select CS_DEPOSITO as Consecutivo from ase_consecutivos";
                            strUpdate = "update ase_consecutivos set CS_DEPOSITO = CS_DEPOSITO + 1";
                            break;
                        case "ND":
                            strCampo = "select CS_NOTA_DEBITO as Consecutivo from ase_consecutivos";
                            strUpdate = "update ase_consecutivos set CS_NOTA_DEBITO = CS_NOTA_DEBITO + 1";
                            break;
                        case "NC":
                            strCampo = "select CS_NOTA_CREDITO as Consecutivo from ase_consecutivos";
                            strUpdate = "update ase_consecutivos set CS_NOTA_CREDITO = CS_NOTA_CREDITO + 1";
                            break;
                    }

                    long consecutivo = conn.Query<long>(strCampo).FirstOrDefault();

                    if (consecutivo == 0)
                    {
                        return 0;
                    }
                    else
                    {
                        conn.Execute(strUpdate);
                    }

                        return consecutivo;
                }
                else
                {
                    strCampo = "exec spSIFDocsConsecutivo @vTipo";
                }

                return conn.Query<long>(strCampo, new {vTipo}).FirstOrDefault();

            }
            catch (Exception)
            {
                return 0;
            }

        }
    }
}
