using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo.Models.TES;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;
using static Org.BouncyCastle.Math.EC.ECCurve;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesEmisionDocumentosDb
    {
        private readonly IConfiguration? _config;
        private readonly MTesoreria mTesoreria;
        private readonly VerificadorCoreFactory _factory;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly MReportingServicesDB mReporting;
        private readonly PortalDB _portalDB;

        private readonly string nSolicitudes = "solicitudes";
        private readonly string nFechas = "fechas";

        public FrmTesEmisionDocumentosDb(IConfiguration config)
        {
            _config = config;
            mTesoreria = new MTesoreria(config);
            _factory = new VerificadorCoreFactory(config);
            _Security_MainDB = new MSecurityMainDb(config);
            mReporting = new MReportingServicesDB(config);
            _portalDB = new PortalDB(config);
        }

        private string GetParametro(int CodEmpresa, string codigo)
        {
            return mTesoreria.fxTesParametro(CodEmpresa, codigo);
        }

        private string LimpiarReporte(string nombre)
        {
            return Regex.Replace(nombre, @"\.(rdl|rdlc|RDL|RDLC)$", "", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Obtener formatos de banco
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="banco"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumento_Formato_Obtener(int CodEmpresa, int banco)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                var query = @"exec spTes_Formatos_Bancos @banco";
                var result = conn.Query(query, new { banco })
                    .Select(row => new DropDownListaGenericaModel
                    {
                        item = row.IDX,
                        descripcion = row.ItmX
                    }).ToList();
                return result;
            });
        }

        /// <summary>
        /// Obtener planes 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="banco"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumento_Plan_Obtener(int CodEmpresa, int banco)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                var query = @"select Bp.COD_PLAN as item, Bp.COD_PLAN as descripcion
                        from TES_BANCOS B inner join TES_BANCO_PLANES_TE Bp on B.ID_BANCO = Bp.ID_BANCO 
                        Where B.ID_BANCO = @banco And B.UTILIZA_PLAN = 1 order by Bp.COD_PLAN asc";
                return conn.Query<DropDownListaGenericaModel>(query, new { banco = banco }).ToList();
            });
        }

        /// <summary>
        /// Buscar información para emisión de documentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipoDoc"></param>
        /// <param name="banco"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        public ErrorDto<TesTransaccionesData> TES_EmisionDocumento_Buscar(int CodEmpresa, string tipoDoc, int banco, string plan)
        {

            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                var query = @"select isnull(count(*),0) as Total,isnull(Min(nsolicitud),0) as Minimo,
                        isnull(Max(nsolicitud),0) as Maximo from Tes_Transacciones
                        Where Estado='P' And Tipo = @tipoDoc and ID_Banco = @banco";
                var solicitudes = conn.QueryFirstOrDefault<TesTransaccionesData>(query, new { tipoDoc = tipoDoc, banco = banco }) ?? new TesTransaccionesData();

                // Si no hay solicitudes
                if (solicitudes.total == 0)
                {
                    solicitudes.minimo = 0;
                    solicitudes.maximo = 0;
                }

                // Obtener consecutivo inicial
                solicitudes.docInicial = mTesoreria.fxTesTipoDocConsec(CodEmpresa, banco, tipoDoc, "/", plan).Result;

                // Verificar si se puede modificar
                string vDato = mTesoreria.fxTesTipoDocExtraeDato(CodEmpresa, banco, tipoDoc, "mod_consec").Result ?? "0";
                solicitudes.docBloqueo = vDato != "1";

                return solicitudes;
            });
        }


        /// <summary>
        /// Despliega en pantalla las solicitudes pendientes que estan autorizadas 
        /// y que estan dentro del rango de parametros suministrado por el usuario.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<TesSolicitudesGenData>> TES_EmisionDocumento_Solicitudes_Obtener(int CodEmpresa, string filtros)
        {
            TesEmisionDocFiltros filtro = JsonConvert.DeserializeObject<TesEmisionDocFiltros>(filtros) ?? new TesEmisionDocFiltros();
            long consecInt = 0;
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                consecInt = mTesoreria.fxTesTipoDocConsecInterno(CodEmpresa, filtro.banco, filtro.tipoDoc, "/", filtro.plan).Result;

                var query = @$"Select TOP {filtro.cantidad} *, dbo.fxTes_Cuentas_Bancarias_Pass(id_Banco,Cta_Ahorros) as 'Pass'
                        From Tes_Transacciones Where Estado='P' And Tipo = @tipoDoc
                        And Id_Banco=@banco And Autoriza = 'S' and fecha_hold is null";

                if (filtro.generarPor == nSolicitudes)
                {
                    query += " And NSolicitud Between @minimo And @maximo";
                }
                else if (filtro.generarPor == nFechas)
                {
                    query += @" And Fecha_Solicitud Between @fechaInicio And @fechaCorte";
                }
                query += " Order by NSolicitud";

                var fechaInicio = filtro.fecha_inicio?.Date;
                var fechaCorte = filtro.fecha_corte?.Date.AddDays(1).AddTicks(-1);

                var result = conn.Query<TesSolicitudesGenData>(query,
                            new
                            {
                                tipoDoc = filtro.tipoDoc,
                                banco = filtro.banco,
                                minimo = filtro.minimo,
                                maximo = filtro.maximo,
                                fechaInicio = fechaInicio,
                                fechaCorte = fechaCorte,

                            }).ToList();

                foreach (var item in result)
                {
                    if (filtro.tipoDoc == "TE")
                    {
                        item.documento = $"{filtro.docInicial:000}-{consecInt}";
                    }
                    else
                    {
                        item.documento = filtro.docInicial.ToString();
                    }
                    item.fecha = DateTime.Now; //Devuelve la fecha del servidor
                    item.firmas = (item.firmas_autoriza_fecha == null) ? "No" : "Sí";
                }

                return result;
            });
        }

        /// <summary>
        /// Valida el numero de documento, si ya está asignado dentro del rango
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="banco"></param>
        /// <param name="tipoDoc"></param>
        /// <param name="docInicial"></param>
        /// <param name="cantidadList"></param>
        /// <returns></returns>
        public ErrorDto TES_EmisionDocumento_ValidaNumDocumento(int CodEmpresa, int banco, string tipoDoc, int docInicial, int cantidadList)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                int docFinal = docInicial + (cantidadList - 1);

                var query = @" SELECT ndocumento FROM Tes_Transacciones
                        WHERE id_Banco = @banco AND ndocumento BETWEEN @docInicial AND @docFinal
                        AND Tipo = @tipoDoc'";
                var lista = conn.Query<int>(query,
                    new
                    {
                        banco = banco,
                        docInicial = docInicial,
                        docFinal = docFinal,
                        tipoDoc = tipoDoc
                    }).ToList();

                var docExistente = lista.FirstOrDefault(nDoc => nDoc >= docInicial && nDoc <= docFinal);
                if (docExistente != 0)
                {
                    return DbHelper.ErrorResponse($"\nYa existe un Documento asignado [{docExistente}] dentro del rango suministrado", -2);
                }

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Ejecuta un SP que Revisa Cuentas Bancarias de Solicitudes Pendientes de Emitir
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="banco"></param>
        /// <returns></returns>
        public ErrorDto TES_EmisionDocumento_RevisaCuentas_SP(int CodEmpresa, int banco)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var query = "exec spTes_Cuentas_Revisa @banco";
                conn.Execute(query, new { banco = banco });
                return DbHelper.OkResponse("Cuentas verificadas correctamente!");

            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtener cuentas puente
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumento_CtasPuente_Obtener(
            int CodEmpresa,
            string Usuario)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                var query = @"
            select 
                B.id_Banco as item,
                rtrim(B.descripcion) as descripcion
            from Tes_Bancos B
            inner join tes_Banco_ASG A 
                on B.id_Banco = A.id_Banco
               and A.nombre = @usuario
            where B.estado = 'A'
              and B.puente = 1";

                return conn.Query<DropDownListaGenericaModel>(
                    query,
                    new { usuario = Usuario }
                ).ToList();
            });
        }

        /// <summary>
        /// Traslada las Solicitudes seleccionadas entre Cuentas (Puente)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Banco"></param>
        /// <param name="Usuario"></param>
        /// <param name="Solicitudes"></param>
        /// <returns></returns>
        public ErrorDto TES_EmisionDocumento_CtaPuente_Aplicar(
            int CodEmpresa,
            int Banco,
            string Usuario,
            string Solicitudes)
        {
            try
            {
                var listaSolicitudes =
                    JsonConvert.DeserializeObject<List<int>>(Solicitudes) ?? new List<int>();

                using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                foreach (var solicitud in listaSolicitudes)
                {
                    var query = @"exec spTes_Traslados_Cuenta_Puente 
                          @solicitud, @banco, @usuario";

                    connection.Execute(query, new
                    {
                        solicitud,
                        banco = Banco,
                        usuario = Usuario
                    });
                }

                return DbHelper.OkResponse("Solicitudes movidas correctamente");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Generar emision de documentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<object> TES_EmisionDocumento_Generar(int CodEmpresa, string filtros)
        {

            TesEmisionDocFiltros filtro = JsonConvert.DeserializeObject<TesEmisionDocFiltros>(filtros) ?? new TesEmisionDocFiltros();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<object>
            {
                Code = 0
            };
            var chequesReport = new TesArchivosEspecialesData();

            var fechaInicio = filtro.fecha_inicio?.Date;
            var fechaCorte = filtro.fecha_corte?.Date.AddDays(1).AddTicks(-1);
            long vConsecutivo = 0;

            if (filtro.generarPor != nFechas)
            {
                filtro.fecha_inicio = null;
                filtro.fecha_corte = null;
            }

            DateTime vFecha = DateTime.Now; //Devuelve la fecha del servidor
            try
            {
                chequesReport = mTesoreria.sbCargaArchivosEspeciales(CodEmpresa, filtro.banco).Result;
                using var connection = new SqlConnection(stringConn);

                var queryBDoc = @"select doc_auto,comprobante from tes_banco_docs 
                        where id_banco = @banco and tipo = @tipoDoc";
                var bancoDocs = connection.QueryFirstOrDefault<TesBancoDocsData>(queryBDoc,
                    new
                    {
                        banco = filtro.banco,
                        tipoDoc = filtro.tipoDoc
                    });

                var queryB = @"select firmas_desde,firmas_hasta,formato_transferencia,Lugar_Emision
                        from Tes_Bancos where id_banco = @banco";
                var bancoData = connection.QueryFirstOrDefault<TesBancoData>(queryB,
                    new { banco = filtro.banco });

                var queryBFAut = "select isnull(count(*),0) as Existe from TES_BANCO_FIRMASAUT where id_Banco = @banco and usuario = @usuario";
                var vFirmas = connection.QueryFirstOrDefault<int>(queryBFAut,
                    new { banco = filtro.banco, usuario = filtro.usuario });

                //Lista completa de Tes_Transacciones
                var queryTransac = @$"Select TOP {filtro.cantidad} * From Tes_Transacciones Where Estado = 'P' And Tipo = @tipoDoc
                        And ID_Banco= @banco And Autoriza='S' and fecha_hold is null";
                if (filtro.generarPor == nSolicitudes)
                {
                    queryTransac += " And NSolicitud Between @minimo And @maximo";
                }
                else if (filtro.generarPor == nFechas)
                {
                    queryTransac += @" And Fecha_Solicitud Between @fechaInicio And @fechaCorte";
                }
                queryTransac += " Order by Nsolicitud";

                //Lista de solo los nsolicitud de Tes_Transacciones
                var baseQuery = $@"(SELECT TOP {filtro.cantidad} nsolicitud 
                                    FROM Tes_Transacciones  WHERE Estado = 'P' AND Tipo = @tipoDoc 
                                    AND ID_Banco = @banco AND Autoriza = 'S' AND fecha_hold IS NULL";
                if (filtro.generarPor == nSolicitudes)
                {
                    baseQuery += " And NSolicitud Between @minimo And @maximo";
                }
                else if (filtro.generarPor == nFechas)
                {
                    baseQuery += @" And Fecha_Solicitud Between @fechaInicio And @fechaCorte";
                }
                baseQuery += " Order by Nsolicitud)";

                var parametros = new
                {
                    banco = filtro.banco,
                    tipoDoc = filtro.tipoDoc,
                    minimo = filtro.minimo,
                    maximo = filtro.maximo,
                    fechaInicio = fechaInicio,
                    fechaCorte = fechaCorte
                };

                var transaccionesList = new List<TesTransaccionDto>();

                switch (bancoDocs.comprobante)
                {
                    case "01" or "02" or "03"://CK formula continua /CK Bloque / Registro Doc
                        if (bancoDocs.doc_auto == 1)
                        {
                            //Revisa que el Consecutivo, Sea Modificable o No, si lo es inicializar por el indicado por el usuario
                            if (filtro.docBloqueo != null && filtro.docBloqueo == true)
                            {
                                vConsecutivo = mTesoreria.fxTesTipoDocConsec(CodEmpresa, filtro.banco, filtro.tipoDoc, "/").Result;
                            }
                            else
                            {
                                if (vConsecutivo == 0)
                                {
                                    vConsecutivo = filtro.docInicial;

                                    var queryUpdate = "update tes_banco_docs set consecutivo = @consecutivo where id_banco = @banco and tipo = @tipoDoc";
                                    connection.Execute(queryUpdate, new { consecutivo = vConsecutivo, banco = filtro.banco, tipoDoc = filtro.tipoDoc });
                                }
                                else
                                {
                                    vConsecutivo = mTesoreria.fxTesTipoDocConsec(CodEmpresa, filtro.banco, filtro.tipoDoc, "+").Result;
                                }
                            }
                        }

                        transaccionesList = connection.Query<TesTransaccionDto>(queryTransac, parametros).ToList();

                        int contador = 0;

                        //Cheques Formula Continua
                        var listaRecorridaConFirmas = new List<TesTransaccionDto>();
                        var listaRecorridaSinFirmas = new List<TesTransaccionDto>();
                        string reporteCkConFirmas = "", reporteCkSinFirmas = "";

                        //Boleta de Registro
                        var listaRecorridaBoleta = new List<TesTransaccionDto>();
                        var pdfsBoleta = new List<byte[]>();
                        FileContentResult fileResultBoleta = null;

                        //Imprime reporte
                        FrmReporteGlobal reporteData = new()
                        {
                            codEmpresa = CodEmpresa,
                            parametros = null,
                            nombreReporte = "",
                            usuario = filtro.usuario,
                            cod_reporte = "P",
                            folder = "Bancos"
                        };

                        foreach (var item in transaccionesList)
                        {
                            if (contador < filtro.verificacion)
                            {
                                //Indica que el documento esta autorizado para que se utilice firma electronica
                                bool vFirmaAutorizada = item.firmas_autoriza_fecha != null;

                                var queryUpdate = @"UPDATE Tes_Transacciones SET Estado = 'I', Fecha_Emision = @vfecha, Ubicacion_Actual = 'T', 
                                            FECHA_TRASLADO = @vfecha, User_Genera = @usuario";
                                if (bancoDocs.doc_auto == 1)
                                {
                                    queryUpdate += " ,NDocumento = @consecutivo";
                                }
                                queryUpdate += "  where NSolicitud= @nsolicitud";

                                connection.Execute(queryUpdate,
                                    new
                                    {
                                        vfecha = vFecha,
                                        usuario = filtro.usuario,
                                        consecutivo = vConsecutivo,
                                        nsolicitud = item.nsolicitud
                                    });

                                mTesoreria.sbTesBancosAfectacion(CodEmpresa, item.nsolicitud, "E");
                                mTesoreria.sbTesBitacoraEspecial(CodEmpresa, item.nsolicitud, "10", "", filtro.usuario.ToUpper());

                                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                                {
                                    EmpresaId = CodEmpresa,
                                    Usuario = filtro.usuario.ToUpper(),
                                    DetalleMovimiento = $"Genero Solicitud {item.nsolicitud}",
                                    Movimiento = "GENERA - WEB",
                                    Modulo = 9
                                });

                                //Actualiza Cuentas Corrientes
                                mTesoreria.sbTESActualizaCC(
                                    CodEmpresa,
                                    new MTesoreria.ActualizaCCParams
                                    {
                                        Codigo = item.codigo.Trim(),
                                        Tipo = item.tipo,
                                        Documento = vConsecutivo.ToString(),
                                        Banco = (int)item.id_banco,
                                        OP = item.op == null ? 0 : item.op.Value,
                                        Modulo = item.modulo,
                                        SubModulo = item.submodulo,
                                        Referencia = item.referencia == null ? 0 : item.referencia.Value
                                    }
                                );

                                if (bancoDocs.doc_auto == 1)
                                {
                                    vConsecutivo = mTesoreria.fxTesTipoDocConsec(CodEmpresa, filtro.banco, filtro.tipoDoc, "+").Result;
                                }

                                //Identifica tipo de reporte
                                switch (bancoDocs.comprobante)
                                {
                                    case "01": //Cheques Formula Continua
                                        string reporte = Path.GetFileNameWithoutExtension(chequesReport.chequesFirmas);
                                        //Si utiliza firmas, preguntar por el rango en montos
                                        if (vFirmas == 1)
                                        {
                                            if (item.monto >= bancoData.firmas_desde && item.monto <= bancoData.firmas_hasta)
                                            {

                                                reporteCkConFirmas = LimpiarReporte(chequesReport.chequesFirmas); //Reporte con Firmas
                                                listaRecorridaConFirmas.Add(item);
                                            }
                                            else if (vFirmaAutorizada)
                                            {
                                                reporteCkConFirmas = LimpiarReporte(chequesReport.chequesFirmas); //Reporte con Firmas
                                                listaRecorridaConFirmas.Add(item);
                                            }
                                            else
                                            {
                                                reporteCkSinFirmas = LimpiarReporte(chequesReport.chequesSinFirmas); //Reporte sin Firmas
                                                listaRecorridaSinFirmas.Add(item);
                                            }
                                        }
                                        else
                                        {
                                            reporteCkSinFirmas = LimpiarReporte(chequesReport.chequesSinFirmas); //Reporte sin Firmas
                                            listaRecorridaSinFirmas.Add(item);
                                        }

                                        break;
                                    case "02" or "03": //Cheques Block / Boleta de Transaccion

                                        listaRecorridaBoleta.Add(item);

                                        //Genera reporte de Boleta de Transaccion
                                        reporteData.nombreReporte = "Banking_BoletaRegistro";
                                        reporteData.parametros = JsonConvert.SerializeObject(new { nSolicitud = item.nsolicitud });
                                        var actionBoleta = mReporting.ReporteRDLC_v2(reporteData);

                                        //Valida respuesta de ReporteRDLC_v2
                                        var objectResult = actionBoleta as ObjectResult;

                                        if (objectResult == null)
                                        {
                                            fileResultBoleta = actionBoleta as FileContentResult;

                                            if (fileResultBoleta != null && fileResultBoleta.FileContents != null && fileResultBoleta.FileContents.Length > 0)
                                            {
                                                pdfsBoleta.Add(fileResultBoleta.FileContents);
                                            }
                                            else
                                            {
                                                response.Code = -1;
                                                response.Description = "Ocurrió un error al generar la boleta de la solicitud " + item.nsolicitud + ", contenido es nulo o vacío";
                                                return response;
                                            }
                                        }
                                        else
                                        {
                                            var res = objectResult.Value;
                                            //converte res a JSON
                                            var Jres = System.Text.Json.JsonSerializer.Serialize(res);
                                            // convierte JSON a ErrorDto
                                            var err = System.Text.Json.JsonSerializer.Deserialize<ErrorDto>(Jres);

                                            response.Code = -1;
                                            response.Description = err.Description ?? "Ocurrió un error al generar la boleta de la solicitud " + item.nsolicitud;
                                            return response;
                                        }

                                        break;
                                    default:
                                        break;
                                }

                                contador++;

                            }
                            else
                            {
                                break;
                            }
                        }

                        //Genera los reportes de Cheques Formula Continua
                        string ckConFirma = string.Empty;
                        string ckSinFirma = string.Empty;
                        if (listaRecorridaConFirmas.Count > 0 || listaRecorridaSinFirmas.Count > 0)
                        {

                            string vMesLetras = MTesoreria.fxTesMesDescripcion(DateTime.Now.Month);

                            if (listaRecorridaConFirmas.Count > 0)
                            {
                                var nsolicitudes = string.Join(",", listaRecorridaConFirmas.Select(x => x.nsolicitud));
                                nsolicitudes = nsolicitudes.TrimEnd(',');

                                var parametrosJsonConFirmas = new
                                {
                                    filtros = $@" WHERE CHEQUES.NSOLICITUD IN ({nsolicitudes})",
                                    Fecha = $@" {bancoData.lugar_Emision} DE {vMesLetras} DE {DateTime.Now.Year.ToString()} ",
                                    Año = DateTime.Now.Year.ToString(),
                                    Letras = ""
                                };

                                reporteData.parametros = System.Text.Json.JsonSerializer.Serialize(listaRecorridaConFirmas);
                                reporteData.nombreReporte = reporteCkConFirmas;

                                var actionResult1 = mReporting.ReporteRDLC_v2(reporteData);

                                //Valida respuesta de ReporteRDLC_v2
                                var objectResult = actionResult1 as ObjectResult;

                                if (objectResult == null)
                                {
                                    ckConFirma = JsonConvert.SerializeObject(actionResult1, Formatting.Indented);
                                }
                                else
                                {
                                    var res = objectResult.Value;
                                    //converto res a JSON
                                    var Jres = System.Text.Json.JsonSerializer.Serialize(res);

                                    // convierto JSON a ErrorDto
                                    var err = System.Text.Json.JsonSerializer.Deserialize<ErrorDto>(Jres);

                                    ckConFirma = JsonConvert.SerializeObject(err, Formatting.Indented);
                                }
                            }
                            else if (listaRecorridaSinFirmas.Count > 0)
                            {
                                string nsolicitudes = "";
                                foreach (var item in listaRecorridaSinFirmas)
                                {
                                    nsolicitudes += item.nsolicitud + ",";
                                }
                                nsolicitudes = nsolicitudes.TrimEnd(',');


                                var parametrosJsonSinFirmas = new
                                {
                                    filtros = $@" WHERE CHEQUES.NSOLICITUD IN ({nsolicitudes})",
                                    Fecha = $@" {bancoData.lugar_Emision} DE {vMesLetras} DE {DateTime.Now.Year.ToString()} ",
                                    Año = DateTime.Now.Year.ToString(),
                                    Letras = ""
                                };

                                reporteData.parametros = System.Text.Json.JsonSerializer.Serialize(parametrosJsonSinFirmas);
                                reporteData.nombreReporte = reporteCkSinFirmas;

                                var actionResult2 = mReporting.ReporteRDLC_v2(reporteData);

                                //Valida respuesta de ReporteRDLC_v2
                                var objectResult = actionResult2 as ObjectResult;

                                if (objectResult == null)
                                {
                                    ckSinFirma = JsonConvert.SerializeObject(actionResult2, Formatting.Indented);
                                }
                                else
                                {
                                    var res = objectResult.Value;
                                    //converto res a JSON
                                    var Jres = System.Text.Json.JsonSerializer.Serialize(res);

                                    // convierto JSON a ErrorDto
                                    var err = System.Text.Json.JsonSerializer.Deserialize<ErrorDto>(Jres);

                                    ckSinFirma = JsonConvert.SerializeObject(err, Formatting.Indented);
                                }
                            }

                        }

                        //Boleta de Registro
                        string boletaReg = string.Empty;
                        if (pdfsBoleta.Count > 0 && fileResultBoleta != null)
                        {
                            fileResultBoleta.FileContents = MProGrXAuxiliarDB.CombinarBytesPdfSharp(pdfsBoleta.ToArray());
                            boletaReg = JsonConvert.SerializeObject(fileResultBoleta, Formatting.Indented);
                        }

                        response.Result = new
                        {
                            chequeConFirma = ckConFirma,
                            chequeSinFirma = ckSinFirma,
                            boletaRegisto = boletaReg
                        };
                        queryTransac = JsonConvert.SerializeObject(transaccionesList, Formatting.Indented);
                        break;
                    case "04": //Transferencias Electrónicas
                        switch (filtro.formatoTE)
                        {
                            case "A": //A - BNCR. Internet Banking

                                var queryA = "select sum(monto) as PLx from Tes_Transacciones where nsolicitud in" + baseQuery;
                                var montoPL = connection.QueryFirstOrDefault<int>(queryA, parametros);

                                transaccionesList = connection.Query<TesTransaccionDto>(queryTransac, parametros).ToList();
                                response = sbTeBancoNacional(CodEmpresa, filtro, transaccionesList, montoPL);

                                break;
                            case "B": //B - Banco Popular

                                transaccionesList = connection.Query<TesTransaccionDto>(queryTransac, parametros).ToList();
                                response = sbTeBancoPopular(CodEmpresa, filtro, transaccionesList);

                                break;
                            case "C": //C - BCR. Planilla Empresarial

                                transaccionesList = connection.Query<TesTransaccionDto>(queryTransac, parametros).ToList();

                                var queryC = @"select sum(dbo.fxTESBCRTestkey(cta_ahorros,monto)) as TestKeyX, 
                                        sum(Monto) as Monto from Tes_Transacciones where nsolicitud in" + baseQuery;
                                var resultC = connection.QueryFirstOrDefault(queryC, parametros);

                                long xTestKey = 0;
                                decimal totalMonto = 0;
                                if (resultC != null)
                                {
                                    long testKeyX = resultC.TestKeyX ?? 0;
                                    xTestKey = testKeyX > 2147483468 ? 2147483468 : testKeyX;
                                    totalMonto = resultC.Monto ?? 0;
                                }
                                response = sbTeBCR_Planilla(CodEmpresa, filtro, transaccionesList, xTestKey, totalMonto);

                                break;
                            case "D": //D - BCR. Empresas

                                response = sbTeBCR_Empresarial(CodEmpresa, filtro);

                                break;
                            case "E": //E - BCT. Enlace

                                response = sbTeBCT_Enlace(CodEmpresa, filtro);

                                break;
                            case "F": //F - BCR. Comercial

                                response = sbTeBCR_Comercial(CodEmpresa, filtro);

                                break;
                            case "G": //G - BN Formato SINPE

                                response = sbTeBNCR_Sinpe(CodEmpresa, filtro);

                                break;
                            case "DV1" or "DV2":

                                response = sbTeFormatoEstandar(CodEmpresa, filtro);

                                break;
                            case "S":
                                response.Code = -1;
                                response.Description = "No se pudo realizar la operación, debido a que la opción de SINPE se encuentra en espera";
                                break;
                            case "SG":
                                //response.Code = -1;
                                //response.Description = "No se pudo realizar la operación, debido a que la opción de SINPE se encuentra en espera";
                                // Banco General SINPE
                                transaccionesList = connection.Query<TesTransaccionDto>(queryTransac, parametros).ToList();
                                response = sbTeBancoSinpeGeneral(CodEmpresa, filtro, transaccionesList);
                                break;
                            default:
                                response = sbTeFormatoEstandar(CodEmpresa, filtro);
                                break;
                        }

                        break;
                    default:
                        break;
                }

                var concatenado = new
                {
                    archivo = response.Result,
                    strQuery = queryTransac,
                    parametros = parametros,
                    comprobante = bancoDocs.comprobante
                };
                response.Result = JsonConvert.SerializeObject(concatenado, Formatting.Indented);

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        #region Formatos de Bancos

        /// <summary>
        /// Emite la Transferencia en formato para el Banco Nacional. (Genera archivo)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="transaccionesList"></param>
        /// <param name="curPlanilla"></param>
        /// <returns></returns>
        private ErrorDto<object> sbTeBancoNacional(int CodEmpresa, TesEmisionDocFiltros filtros, List<TesTransaccionDto> transaccionesList, int? curPlanilla)
        {

            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            int BancoID = filtros.banco;
            DateTime vFecha = DateTime.Now; //Devuelve la fecha del servidor
            decimal curMonto1 = curPlanilla ?? 0;
            string strMonto = curMonto1.ToString("0000000000.00").Replace(".", "");
            string vCuentaEmpresa = "";
            string vNumCliente = "";
            decimal curMonto2 = 0;
            long curCuentas = 0;
            try
            {
                SeguridadPortalDb seguridadPortal = new SeguridadPortalDb(_config);
                string Empresa_Name = "TF " + seguridadPortal.SeleccionarPgxClientePorCodEmpresa(CodEmpresa).PGX_CORE_DB;
                string vConcepto = Empresa_Name.PadRight(30, ' ');

                var query = "select Cta,codigo_Cliente from tes_Bancos Where id_Banco = @banco";
                var bancoData = connection.QueryFirstOrDefault(query, new { banco = BancoID });
                if (bancoData != null)
                {
                    vCuentaEmpresa = bancoData.Cta;
                    vCuentaEmpresa = vCuentaEmpresa.ToString().Trim().Replace("-", "");
                    vNumCliente = bancoData.codigo_Cliente;
                    vNumCliente = vNumCliente.PadLeft(6, '0');
                }
                
                //Inicializa Variables de Bancos y Consecutivo
                string BancoTDoc = filtros.tipoDoc;
                long BancoConsec = mTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+").Result;

                // En vez de guardar el archivo, lo devuelve como string
                var sb = new StringBuilder();

                //ENCABEZADO DEL FORMATO DE TRANSFERENCIA
                string strCadena = "1";
                strCadena += vNumCliente;
                strCadena += vFecha.Day.ToString("00") + vFecha.Month.ToString("00") + vFecha.Year.ToString("0000");
                strCadena += BancoID.ToString("D12"); // 12 dígitos con ceros a la izquierda
                strCadena += "10000";
                strCadena += strMonto;
                strCadena += "000000000000000000000000"; // 24 ceros
                sb.AppendLine(strCadena);

                //DETALLE DE LA TRANSFERENCIA
                int i = 0;

                foreach (var item in transaccionesList)
                {
                    i++;
                    string cuenta = (item.cta_ahorros ?? "").ToString().Replace("-", "").Trim();
                    var linea = new StringBuilder(120);
                    linea.Append('3');
                    linea.Append(cuenta.Substring(5, 3));
                    linea.Append(cuenta.Substring(0, 3));
                    linea.Append("01");
                    linea.Append(cuenta.Substring(cuenta.Length - 7));
                    linea.Append(i.ToString("D8"));

                    decimal monto = (decimal)item.monto;
                    string strMontoDet = monto.ToString("0000000000.00").Replace(".", ""); //12d Monto sin el punto decimal
                    linea.Append(strMontoDet);
                    linea.Append(vConcepto);
                    linea.Append("00");

                    sb.AppendLine(linea.ToString());
                }

                //CREA ULTIMA LINEA DE DETALLE CON EL DEBITO A LA EMPRESA 
                strCadena = "2";
                strCadena += vCuentaEmpresa.Substring(0, 3); // Movimiento de Debito, y 000 Sucursal de Apertura
                strCadena += "10001"; //Cuenta Corriente y Moneda en Colones
                strCadena += vCuentaEmpresa.Substring(vCuentaEmpresa.Length - 7); // 7 dígitos - Cuenta de la Empresa + Digito Verificador
                strCadena += (i + 1).ToString("D8"); //Numero Comprobante

                string strMontoEmpresa = curMonto2.ToString("0000000000.00").Replace(".", ""); //12d Monto sin el punto decimal
                strCadena += strMontoEmpresa; //Total de los Creditos para Debitar a esta cuenta
                strCadena += vConcepto; //30d Concepto de Pago
                strCadena += "00"; //Fin de Linea
                sb.AppendLine(strCadena);

                curCuentas += long.Parse(vCuentaEmpresa.Substring(vCuentaEmpresa.Length - 7, 6)); // sin verificador

                // REGISTRO DE CONTROL DEL ARCHIVO DE TRANSFERENCIA 
                string linea4 = "4"; //Codigo de Control de registro
                decimal montoControl = curMonto1 + curMonto2; //Suma Debitos y Creditos de la Transferencia
                string strMontoControl = montoControl.ToString("0000000000000.00").Replace(".", "");
                linea4 += strMontoControl;
                linea4 += curCuentas.ToString("D10"); //Sumatoria de Cuentas
                linea4 += "0000000000";
                linea4 += "000000000000";
                linea4 += "000000000000";
                linea4 += "00000000";
                sb.AppendLine(linea4);

                // Devolver el contenido generado en el object
                var archivo = new
                {
                    bancoConsec = BancoConsec.ToString(),
                    extension = "ENV",
                    contenido = sb.ToString()
                };


                var json = JsonConvert.SerializeObject(archivo, Formatting.Indented);
                return DbHelper.CreateOkResponse<object>(json);

            }
            catch (Exception ex)
            {
               return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }

        /// <summary>
        /// Emite la Transferencia en formato para el Banco Popular. (Genera archivo)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="transaccionesList"></param>
        /// <returns></returns>
        private ErrorDto<object> sbTeBancoPopular(int CodEmpresa, TesEmisionDocFiltros filtros, List<TesTransaccionDto> transaccionesList)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            DateTime vFecha = DateTime.Now; //Devuelve la fecha del servidor

            try
            {

                //Inicializa Variables de Bancos y Consecutivo
                int BancoID = filtros.banco;
                string BancoTDoc = filtros.tipoDoc;
                long BancoConsec = mTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+").Result;

                // En vez de guardar el archivo, lo devuelve como string
                var sb = new StringBuilder();

                foreach (var item in transaccionesList)
                {
                    // ---- Código (10 chars) ----
                    string codigo10;
                    var codigoTrim = item.codigo?.Trim() ?? string.Empty;

                    switch (codigoTrim.Length)
                    {
                        case 8:
                            // 0 + first + 0 + rest(7)  => 10
                            codigo10 = "0" + codigoTrim.Substring(0, 1) + "0" + codigoTrim.Substring(1, 7);
                            break;

                        case 9:
                            codigo10 = "0" + codigoTrim;
                            break;

                        case < 8:
                            // relleno a 10 con ceros
                            codigo10 = Convert.ToInt64(codigoTrim).ToString("D10");
                            break;

                        case > 10:
                            // patrón original: XXXX + 0 + XXXXX (saltando el char 4)
                            // (Mantengo tu lógica tal cual)
                            codigo10 = codigoTrim.Substring(0, 4) + "0" + codigoTrim.Substring(5, 5);
                            break;

                        default:
                            // 10 exactos o cualquier otro caso
                            codigo10 = codigoTrim;
                            break;
                    }

                    // ---- Nombre (30 chars) ----
                    string nombre = (item.beneficiario ?? string.Empty).Trim();
                    nombre = nombre.Length > 30 ? nombre.Substring(0, 30) : nombre.PadRight(30, ' ');

                    // ---- Cuenta (13 chars) ----
                    string cuenta = (item.cta_ahorros ?? "0").Trim();
                    cuenta = cuenta.Length > 13 ? cuenta.Substring(0, 13) : cuenta.PadLeft(13, '0');

                    // ---- Monto (11 + 2 decimales => "000000000.00" -> 12 sin punto) ----
                    decimal monto = (decimal)(item.monto ?? 0m);
                    string strMonto = monto.ToString("000000000.00").Replace(".", "");

                    // ---- Fecha ddMMyyyy ----
                    string strFecha = vFecha.ToString("ddMMyyyy");

                    // ---- Armar línea sin concatenación en loop (Sonar S1643) ----
                    var line = new StringBuilder(120);
                    line.Append(codigo10);
                    line.Append(nombre);
                    line.Append(cuenta);
                    line.Append(' ');       // strSelf
                    line.Append(strMonto);
                    line.Append(strFecha);
                    line.Append('A');       // strTipo
                    line.Append("06");      // strProducto
                    line.Append('P');       // strEstado
                    line.Append(strFecha);
                    line.Append(strMonto);

                    sb.AppendLine(line.ToString());
                }

                // Devolver el contenido generado en el object
                var archivo = new
                {
                    bancoConsec = BancoConsec.ToString(),
                    extension = "txt",
                    contenido = sb.ToString()
                };

                var json = JsonConvert.SerializeObject(archivo, Formatting.Indented);
                return DbHelper.CreateOkResponse<object>(json);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }

        /// <summary>
        /// Generacion con Formatos Estandares de Transferencias Bancarias (Genera archivo)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private ErrorDto<object> sbTeFormatoEstandar(int CodEmpresa, TesEmisionDocFiltros filtros)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var resp = new ErrorDto<object>
            {
                Code = 0,
                Description = ""
            };
            string pFormato = filtros.formatoTE;
            int BancoID = filtros.banco;
            DateTime vFecha = DateTime.Now; //Devuelve la fecha del servidor
            string vNumNegocio = "";
            string vCedulaReg = "";
            string vRazon = "";
            string vExtension = "";
            string vProcedimiento = "";
            try
            {

                var query = "select REPLACE(cedula_juridica,'-','') as 'cedula_Juridica',NOMBRE From SIF_EMPRESA";
                var empresaData = connection.QueryFirstOrDefault(query);
                if (empresaData != null)
                {
                    vNumNegocio = empresaData.cedula_Juridica;
                    vCedulaReg = empresaData.cedula_Juridica;
                    vRazon = "TRANSFERENCIAS " + empresaData.nombre;
                }

                query = "select Procedimiento,Extension from vTes_Formatos where cod_formato = @formato";
                var formatoData = connection.QueryFirstOrDefault(query, new { formato = pFormato });
                if (formatoData != null)
                {
                    vExtension = formatoData.Extension;
                    vProcedimiento = formatoData.Procedimiento;
                }

                //Inicializa Variables de Bancos y Consecutivo
                string BancoTDoc = filtros.tipoDoc;
                string BancoPlan = filtros.plan;
                long BancoConsec = mTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+", BancoPlan).Result;

                int i = 1;
                query = @"SELECT COUNT(DISTINCT documento_base)
                              FROM   Tes_Transacciones
                              WHERE  id_banco = @banco
                              AND    CONVERT(VARCHAR, fecha_emision, 106) = @fecha
                              AND    estado = 'T'";
                i = connection.QueryFirstOrDefault<int>(
                    query, new
                    {
                        banco = BancoID,
                        fecha = vFecha.ToString("yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture)
                    }) + 1;
                string vConArchivo = i.ToString("000");

                query = "SELECT dbo.fxTesCantidadTEDiarias(@fecha, @banco) AS Cantidad";
                int iLineInicio = connection.QueryFirstOrDefault<int>(
                    query, new
                    {
                        fecha = vFecha.ToString("yyyy/MM/dd"),
                        banco = BancoID
                    }
                );

                // En vez de guardar el archivo, se devuelve como string
                var sb = new StringBuilder();
                for (int numLinea = 1; numLinea <= 3; numLinea++)
                {
                    //numLinea donde 1 = LINEA CONTROL, 2 = DEBITOS y 3 = CREDITOS
                    var queryLinea = $@"EXEC {vProcedimiento} {numLinea}, @bancoID, @bancoTDoc, @numNegocio, @bancoConsec, 
                                        @cantidadSolicitudes, @mSolInicio, @mSolCorte, @mFechaInicio, @mFechaCorte";
                    if (BancoPlan != "-sp-")
                    {
                        queryLinea += ", @bancoPlan";
                    }

                    var parametros = new
                    {
                        bancoID = BancoID,
                        bancoTDoc = BancoTDoc,
                        numNegocio = vNumNegocio,
                        bancoConsec = BancoConsec,
                        cantidadSolicitudes = filtros.cantidad,
                        mSolInicio = filtros.minimo,
                        mSolCorte = filtros.maximo,
                        mFechaInicio = filtros.fecha_inicio?.ToString("yyyy/MM/dd"),
                        mFechaCorte = filtros.fecha_corte?.ToString("yyyy/MM/dd"),
                        bancoPlan = BancoPlan
                    };

                    var lineasList = connection.Query<string>(queryLinea, parametros).ToList();
                    foreach (var linea in lineasList)
                    {
                        if (!string.IsNullOrWhiteSpace(linea))
                        {
                            sb.AppendLine(linea);
                        }
                    }
                }

                // Devolver el contenido generado en el object
                var archivo = new
                {
                    bancoConsec = BancoConsec.ToString(),
                    extension = vExtension,
                    contenido = sb.ToString()

                };

                var json = JsonConvert.SerializeObject(archivo, Formatting.Indented);
                return DbHelper.CreateOkResponse<object>(json);

            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }

        /// <summary>
        /// Emite la Transferencia en formato para el Banco de Costa Rica. (Genera archivo)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="transaccionesList"></param>
        /// <param name="vTestKey"></param>
        /// <param name="vMontoTotal"></param>
        /// <returns></returns>
        private ErrorDto<object> sbTeBCR_Planilla(int CodEmpresa, TesEmisionDocFiltros filtros, List<TesTransaccionDto> transaccionesList, long vTestKey, decimal vMontoTotal)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa); 
            DateTime vFecha = DateTime.Now; //Devuelve la fecha del servidor

            try
            {
                string vRazon = GetParametro(CodEmpresa, "BCRFormat3").PadRight(30, ' ');
                string vNumNegocio = GetParametro(CodEmpresa, "BCRFormat1");
                string vCedulaReg = GetParametro(CodEmpresa, "BCRFormat2");

                // Calcular el Numero de Archivo (consecutivo de transferencias del día)
                int i = 1;
                var query = @"select documento_base,count(*) From Tes_Transacciones 
                      where id_banco = @banco and fecha_emision = @fecha
                      and estado = 'T' group by documento_base";
                var resultados = connection.QueryFirstOrDefault(query, new { banco = filtros.banco, fecha = vFecha });
                if (resultados != null)
                {
                    foreach (var _ in resultados)
                    {
                        i++;
                    }
                }
                string vConArchivo = i.ToString("D3");

                // Cuenta bancaria (asume que tiene dígito verificador)
                query = @"select Cta from Tes_Bancos where id_Banco = @banco";
                string vCuentaBanco = connection.QueryFirstOrDefault<string>(query, new { banco = filtros.banco }) ?? "0";
                vCuentaBanco = "001" + int.Parse(vCuentaBanco).ToString("D8"); // 001 + 8 dígitos

                // TestKey complementario (1era línea)
                query = @"select dbo.fxTESBCRTestkey(@cuentaBanco, @montoTotal) as TestKey";
                int xTestKey = connection.QueryFirstOrDefault<int>(query, new { cuentaBanco = vCuentaBanco, montoTotal = vMontoTotal });
                vTestKey = Math.Min(vTestKey + xTestKey, 2147483468);

                // Validando largo del TestKey = 12
                string vTesKeyCh = vTestKey.ToString().Trim();
                if (vTesKeyCh.Length > 12)
                {
                    vTestKey = long.Parse(vTesKeyCh[^12..]);
                }

                // Inicializa Variables de Bancos y Consecutivo
                int BancoID = filtros.banco;
                string BancoTDoc = filtros.tipoDoc;

                // Nota: en tu código original estaba " + " (con espacios).
                // Mantengo la intención de '+'.
                long BancoConsec = mTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+").Result;

                // Construcción del archivo
                var sb = new StringBuilder();

                // ENCABEZADO (registro control)
                {
                    var header = new StringBuilder(220);
                    header.Append("000");                                     // Estado
                    header.Append(vNumNegocio);                               // Num negocio
                    header.Append(vConArchivo);                               // Consecutivo archivo (3)
                    header.Append("000000");
                    header.Append(vCedulaReg);
                    header.Append(Convert.ToInt64(vTestKey).ToString("D12"));  // TestKey 12
                    header.Append("000000");
                    header.Append(vFecha.Day.ToString("D2"));
                    header.Append(vFecha.Month.ToString("D2"));
                    header.Append(vFecha.Year.ToString("D4"));
                    header.Append(new string(' ', 21));
                    header.Append('Y');                                       // Señal Y2K
                    sb.AppendLine(header.ToString());
                }

                // LINEA 1: DÉBITO a la cuenta bancaria de la empresa
                int lineaIndex = 1;
                {
                    var debito = new StringBuilder(220);
                    debito.Append("000");                                     // Estado
                    debito.Append('1');                                       // Concepto 1 = cta corriente
                    debito.Append("00000");                                   // Filler
                    debito.Append(vCuentaBanco.Trim().PadRight(11).Substring(0, 11)); // Oficina+cuenta+verificador (11)
                    debito.Append('1');                                       // Moneda 1=CRC
                    debito.Append('4');                                       // 4 = Débito
                    debito.Append("0000");                                    // Causa
                    debito.Append(BancoConsec.ToString("D4"));
                    debito.Append(lineaIndex.ToString("D4"));
                    debito.Append(((long)(vMontoTotal * 100)).ToString("D12")); // Monto sin decimales
                    debito.Append(vFecha.Day.ToString("D2"));
                    debito.Append(vFecha.Month.ToString("D2"));
                    debito.Append(vFecha.Year.ToString("D4"));
                    debito.Append('0');                                       // Filler
                    debito.Append(vRazon);                                    // Razon 30
                    sb.AppendLine(debito.ToString());
                }

                // CREDITOS (por cada transacción)
                foreach (var item in transaccionesList)
                {
                    lineaIndex++;

                    // (evita nulls / longitudes raras)
                    string cuenta = (item.cta_ahorros ?? string.Empty)
                        .PadRight(11)
                        .Substring(0, 11)
                        .Trim();

                    long montoCents = (long)Math.Round(((decimal)item.monto) * 100m, 0, MidpointRounding.AwayFromZero);

                    var credito = new StringBuilder(220);
                    credito.Append("000");                        // Estado
                    credito.Append('2');                          // Concepto 2 = cta ahorros
                    credito.Append("00000");                      // Filler
                    credito.Append(cuenta);                       // Oficina+cuenta+verificador (11)
                    credito.Append('1');                          // Moneda CRC
                    credito.Append('2');                          // 2 = Crédito
                    credito.Append("0000");                       // Causa
                    credito.Append(BancoConsec.ToString("D4"));
                    credito.Append(lineaIndex.ToString("D4"));
                    credito.Append(montoCents.ToString("D12"));   // Monto sin decimales
                    credito.Append(vFecha.Day.ToString("D2"));
                    credito.Append(vFecha.Month.ToString("D2"));
                    credito.Append(vFecha.Year.ToString("D4"));
                    credito.Append('0');                          // Filler
                    credito.Append(vRazon);                       // Razon 30

                    sb.AppendLine(credito.ToString());
                }

                // Resultado
                var archivo = new
                {
                    bancoConsec = BancoConsec.ToString(),
                    extension = "BCR",
                    contenido = sb.ToString()
                };

                var json = JsonConvert.SerializeObject(archivo, Formatting.Indented);
                return DbHelper.CreateOkResponse<object>(json);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }

        /// <summary>
        /// Procedimiento para crear el nuevo archivo del BCR, Banca Empresarial
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private ErrorDto<object> sbTeBCR_Empresarial(int CodEmpresa, TesEmisionDocFiltros filtros)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            DateTime vFecha = DateTime.Now; //Devuelve la fecha del servidor
            int txtCantidadSolicitudes = filtros.cantidad;
            var mFechaInicio = filtros.fecha_inicio?.Date;
            var mFechaCorte = filtros.fecha_corte?.Date.AddDays(1).AddTicks(-1);
            int? mSolInicio = null;
            int? mSolCorte = null;
            if (filtros.generarPor == nSolicitudes)
            {
                mSolInicio = filtros.minimo;
                mSolCorte = filtros.maximo;
            }

            try
            {
                // Empresa
                var empresa = connection.QueryFirstOrDefault(
                    "select REPLACE(cedula_juridica,'-','') as cedula_juridica, nombre From SIF_EMPRESA");

                string vNumNegocio = empresa?.cedula_juridica?.Trim() ?? string.Empty;
                string vCedulaReg = empresa?.cedula_juridica?.Trim() ?? string.Empty;
                string vRazon = "TRANSFERENCIAS " + (empresa?.nombre ?? string.Empty);

                // Consecutivo banco
                int BancoID = filtros.banco;
                string BancoTDoc = filtros.tipoDoc;
                long BancoConsec = mTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+").Result;

                var sb = new StringBuilder();

                // Calcular número de archivo (consecutivo del día)
                int i = 1;
                var query = @"select documento_base,count(*) From Tes_Transacciones 
                      where id_banco = @banco and fecha_emision = @fecha
                      and estado = 'T' group by documento_base";

                var resultados = connection.QueryFirstOrDefault(query, new { banco = BancoID, fecha = vFecha });

                if (resultados != null)
                {
                    foreach (var _ in resultados)
                    {
                        i++;
                    }
                }

                string vConArchivo = i.ToString("D3");

                // Cantidad TE diarias (se mantiene por compatibilidad, aunque no se use aquí)
                _ = connection.QueryFirstOrDefault<int>(
                    @"select dbo.fxTesCantidadTEDiarias(@fecha ,@banco) as 'Cantidad'",
                    new { banco = BancoID, fecha = vFecha });

                // REGISTRO DE CONTROL
                {
                    var control = new StringBuilder(200);
                    control.Append("000"); // Estado 3
                    control.Append((vCedulaReg ?? string.Empty).Trim().PadLeft(12, '0')); // Cedula juridica 12
                    control.Append(vConArchivo); // Consecutivo archivo 3
                    control.Append(vFecha.ToString("ddMMyyyy")); // Fecha aplicacion 8
                    control.Append("000000000000"); // Cedula de registro 12
                    control.Append("000000000000"); // TestKey 12 (ceros)
                    control.Append("000000"); // Hora estado 6 (ceros)
                    control.Append(new string(' ', 6)); // filler 6 espacios
                    control.Append("TLB"); // Tipo archivo
                    control.Append(new string(' ', 128)); // filler 128 espacios
                    control.Append('D'); // Tipo movimiento Debido

                    sb.AppendLine(control.ToString());
                }

                // DEBITOS
                {
                    var debitosQuery = @"exec spTES_BCR_Empresarial 2, @banco, @bancoTDoc, 
                                 @numNegocio, @bancoConsec, @cantidad, 
                                 @solInicio, @solCorte, @fechaInicio, @fechaCorte";

                    var linea2 = connection.QueryFirstOrDefault<string>(
                        debitosQuery,
                        new
                        {
                            banco = BancoID,
                            bancoTDoc = BancoTDoc,
                            numNegocio = vNumNegocio,
                            bancoConsec = BancoConsec,
                            cantidad = txtCantidadSolicitudes,
                            solInicio = mSolInicio,
                            solCorte = mSolCorte,
                            fechaInicio = mFechaInicio,
                            fechaCorte = mFechaCorte
                        });

                    if (!string.IsNullOrWhiteSpace(linea2))
                        sb.AppendLine(linea2);
                }

                // CREDITOS
                {
                    var creditosQuery = @"exec spTES_BCR_Empresarial 3, @banco, @bancoTDoc, 
                                  @numNegocio, @bancoConsec, @cantidad, 
                                  @solInicio, @solCorte, @fechaInicio, @fechaCorte";

                    var linea3 = connection.QueryFirstOrDefault<string>(
                        creditosQuery,
                        new
                        {
                            banco = BancoID,
                            bancoTDoc = BancoTDoc,
                            numNegocio = vNumNegocio,
                            bancoConsec = BancoConsec,
                            cantidad = txtCantidadSolicitudes,
                            solInicio = mSolInicio,
                            solCorte = mSolCorte,
                            fechaInicio = mFechaInicio,
                            fechaCorte = mFechaCorte
                        });

                    if (!string.IsNullOrWhiteSpace(linea3))
                        sb.AppendLine(linea3);
                }

                // Devolver el contenido generado en el object
                var archivo = new
                {
                    bancoConsec = BancoConsec.ToString(),
                    extension = "txt",
                    contenido = sb.ToString()
                };

                var json = JsonConvert.SerializeObject(archivo, Formatting.Indented);
                return DbHelper.CreateOkResponse<object>(json);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }

        /// <summary>
        /// Procedimiento para crear el nuevo archivo del BCT
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private ErrorDto<object> sbTeBCT_Enlace(int CodEmpresa, TesEmisionDocFiltros filtros)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            int txtCantidadSolicitudes = filtros.cantidad;
            var mFechaInicio = filtros.fecha_inicio?.Date;
            var mFechaCorte = filtros.fecha_corte?.Date.AddDays(1).AddTicks(-1);
            int? mSolInicio = null;
            int? mSolCorte = null;
            if (filtros.generarPor == nSolicitudes)
            {
                mSolInicio = filtros.minimo;
                mSolCorte = filtros.maximo;
            }
            try
            {
                // Inicializa Variables de Bancos y Consecutivo
                int BancoID = filtros.banco;
                string BancoTDoc = filtros.tipoDoc;
                long BancoConsec = mTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+").Result;

                // En vez de guardar el archivo, lo devuelve como string
                var sb = new StringBuilder();

                // DETALLE DE LA TRANSFERENCIA
                var query = @"exec spTES_BCT_Enlace 
                      @banco, @bancoTDoc, @bancoConsec, @cantidad, 
                      @solInicio, @solCorte, @fechaInicio, @fechaCorte";

                var resultado = connection.QueryFirstOrDefault(query, new
                {
                    banco = BancoID,
                    bancoTDoc = BancoTDoc,
                    bancoConsec = BancoConsec,
                    cantidad = txtCantidadSolicitudes,
                    solInicio = mSolInicio,
                    solCorte = mSolCorte,
                    fechaInicio = mFechaInicio,
                    fechaCorte = mFechaCorte
                });

                // En tu código original: resultado.Linea
                // Aquí lo manejamos seguro por si viene null o sin esa propiedad.
                string linea = resultado?.Linea?.ToString() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(linea))
                    sb.AppendLine(linea);

                // Devolver el contenido generado en el object
                var archivo = new
                {
                    bancoConsec = BancoConsec.ToString(),
                    extension = "txt",
                    contenido = sb.ToString()
                };

                var json = JsonConvert.SerializeObject(archivo, Formatting.Indented);
                return DbHelper.CreateOkResponse<object>(json);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }

        /// <summary>
        /// Procedimiento para crear el nuevo archivo del BCR, Banca Comercial
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private ErrorDto<object> sbTeBCR_Comercial(int CodEmpresa, TesEmisionDocFiltros filtros)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            DateTime vFecha = DateTime.Now; //Devuelve la fecha del servidor
            int txtCantidadSolicitudes = filtros.cantidad;
            var mFechaInicio = filtros.fecha_inicio?.Date;
            var mFechaCorte = filtros.fecha_corte?.Date.AddDays(1).AddTicks(-1);
            int? mSolInicio = null;
            int? mSolCorte = null;
            if (filtros.generarPor == nSolicitudes)
            {
                mSolInicio = filtros.minimo;
                mSolCorte = filtros.maximo;
            }
           
            try
            {
                // Empresa
                var empresa = connection.QueryFirstOrDefault(
                    "select REPLACE(cedula_juridica,'-','') as cedula_juridica, nombre From SIF_EMPRESA");

                string vNumNegocio = empresa?.cedula_juridica?.Trim() ?? string.Empty;
                string vCedulaReg = empresa?.cedula_juridica?.Trim() ?? string.Empty;
                

                // Inicializa Variables de Bancos y Consecutivo
                int BancoID = filtros.banco;
                string BancoTDoc = filtros.tipoDoc;
                long BancoConsec = mTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+").Result;

                var sb = new StringBuilder();

                // Calcular el Numero de Archivo (consecutivo del día)
                int i = 1;
                var query = @"select documento_base,count(*) From Tes_Transacciones 
                      where id_banco = @banco and fecha_emision = @fecha
                      and estado = 'T' group by documento_base";

                var resultados = connection.QueryFirstOrDefault(query, new { banco = BancoID, fecha = vFecha });
                if (resultados != null)
                {
                    foreach (var _ in resultados)
                    {
                        i++;
                    }
                }

                string vConArchivo = i.ToString("D3");

                // Cantidad TE diarias (se mantiene por compatibilidad, aunque no se use)
                _ = connection.QueryFirstOrDefault<int>(
                    @"select dbo.fxTesCantidadTEDiarias(@fecha ,@banco) as 'Cantidad'",
                    new { banco = BancoID, fecha = vFecha });

                // REGISTRO DE CONTROL
                
                    var control = new StringBuilder(200);
                    control.Append("000"); // Estado 3
                    control.Append((vCedulaReg ?? string.Empty).Trim().PadLeft(12, '0')); // Cedula juridica 12
                    control.Append(vConArchivo); // Consecutivo archivo 3
                    control.Append(vFecha.ToString("ddMMyyyy")); // Fecha aplicacion 8
                    control.Append("000000000000"); // Cedula de registro 12
                    control.Append("000000000000"); // Filler 12 con 0
                    control.Append("000000");       // Hora estado 6
                    control.Append(new string('0', 138)); // filler 138 con 0 (equivalente a "".PadRight(138,'0'))
                    sb.AppendLine(control.ToString());
                

                // DEBITOS
                
                    var debitosQuery = @"exec spTES_BCR_Comercial 2, @banco, @bancoTDoc, 
                                 @numNegocio, @bancoConsec, @cantidad, 
                                 @solInicio, @solCorte, @fechaInicio, @fechaCorte";

                    var linea2 = connection.QueryFirstOrDefault<string>(
                        debitosQuery,
                        new
                        {
                            banco = BancoID,
                            bancoTDoc = BancoTDoc,
                            numNegocio = vNumNegocio,
                            bancoConsec = BancoConsec,
                            cantidad = txtCantidadSolicitudes,
                            solInicio = mSolInicio,
                            solCorte = mSolCorte,
                            fechaInicio = mFechaInicio,
                            fechaCorte = mFechaCorte
                        });

                    if (!string.IsNullOrWhiteSpace(linea2))
                        sb.AppendLine(linea2);
                

                // CREDITOS
                
                    var creditosQuery = @"exec spTES_BCR_Comercial 3, @banco, @bancoTDoc, 
                                  @numNegocio, @bancoConsec, @cantidad, 
                                  @solInicio, @solCorte, @fechaInicio, @fechaCorte";

                    var linea3 = connection.QueryFirstOrDefault<string>(
                        creditosQuery,
                        new
                        {
                            banco = BancoID,
                            bancoTDoc = BancoTDoc,
                            numNegocio = vNumNegocio,
                            bancoConsec = BancoConsec,
                            cantidad = txtCantidadSolicitudes,
                            solInicio = mSolInicio,
                            solCorte = mSolCorte,
                            fechaInicio = mFechaInicio,
                            fechaCorte = mFechaCorte
                        });

                    if (!string.IsNullOrWhiteSpace(linea3))
                        sb.AppendLine(linea3);
                

                // Devolver el contenido generado en el object
                var archivo = new
                {
                    bancoConsec = BancoConsec.ToString(),
                    extension = "txt",
                    contenido = sb.ToString()
                };

                var json = JsonConvert.SerializeObject(archivo, Formatting.Indented);
                return DbHelper.CreateOkResponse<object>(json);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }

        /// <summary>
        /// Emite la Transferencia en formato SINPE para el BNCR
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private ErrorDto<object> sbTeBNCR_Sinpe(int CodEmpresa, TesEmisionDocFiltros filtros)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            int txtCantidadSolicitudes = filtros.cantidad;
            var mFechaInicio = filtros.fecha_inicio?.Date;
            var mFechaCorte = filtros.fecha_corte?.Date.AddDays(1).AddTicks(-1);
            int? mSolInicio = null;
            int? mSolCorte = null;
            if (filtros.generarPor == nSolicitudes)
            {
                mSolInicio = filtros.minimo;
                mSolCorte = filtros.maximo;
            }
            try
            {
                // Inicializa Variables de Bancos y Consecutivo
                int BancoID = filtros.banco;
                string BancoTDoc = filtros.tipoDoc;
                long BancoConsec = mTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+").Result;

                // En vez de guardar el archivo, lo devuelve como string
                var sb = new StringBuilder();

                const string sp = @"exec spTES_BNCR_SINPE 
                            @numLinea, @banco, @bancoTDoc, 
                            @bancoConsec, @cantidad, 
                            @solInicio, @solCorte, @fechaInicio, @fechaCorte";

                var parametrosBase = new
                {
                    banco = BancoID,
                    bancoTDoc = BancoTDoc,
                    bancoConsec = BancoConsec,
                    cantidad = txtCantidadSolicitudes,
                    solInicio = mSolInicio,
                    solCorte = mSolCorte,
                    fechaInicio = mFechaInicio,
                    fechaCorte = mFechaCorte
                };

                // Encabezado: línea 1
                var linea1 = connection.QueryFirstOrDefault<string>(sp, new
                {
                    numLinea = 1,
                    parametrosBase.banco,
                    parametrosBase.bancoTDoc,
                    parametrosBase.bancoConsec,
                    parametrosBase.cantidad,
                    parametrosBase.solInicio,
                    parametrosBase.solCorte,
                    parametrosBase.fechaInicio,
                    parametrosBase.fechaCorte
                });

                if (!string.IsNullOrWhiteSpace(linea1))
                    sb.AppendLine(linea1);

                // Débitos: línea 2
                var linea2 = connection.QueryFirstOrDefault<string>(sp, new
                {
                    numLinea = 2,
                    parametrosBase.banco,
                    parametrosBase.bancoTDoc,
                    parametrosBase.bancoConsec,
                    parametrosBase.cantidad,
                    parametrosBase.solInicio,
                    parametrosBase.solCorte,
                    parametrosBase.fechaInicio,
                    parametrosBase.fechaCorte
                });

                if (!string.IsNullOrWhiteSpace(linea2))
                    sb.AppendLine(linea2);

                // Créditos: línea 3
                var linea3 = connection.QueryFirstOrDefault<string>(sp, new
                {
                    numLinea = 3,
                    parametrosBase.banco,
                    parametrosBase.bancoTDoc,
                    parametrosBase.bancoConsec,
                    parametrosBase.cantidad,
                    parametrosBase.solInicio,
                    parametrosBase.solCorte,
                    parametrosBase.fechaInicio,
                    parametrosBase.fechaCorte
                });

                if (!string.IsNullOrWhiteSpace(linea3))
                    sb.AppendLine(linea3);

                // Devolver el contenido generado en el object
                var archivo = new
                {
                    bancoConsec = BancoConsec.ToString(),
                    extension = "tef",
                    contenido = sb.ToString()
                };

                var json = JsonConvert.SerializeObject(archivo, Formatting.Indented);
                return DbHelper.CreateOkResponse<object>(json);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }

        private ErrorDto<object> sbTeBancoSinpeGeneral(int CodEmpresa, TesEmisionDocFiltros filtro, List<TesTransaccionDto> transaccionesList)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<object>
            {
                Code = 0,
                Description = "",
                Result = new object()
            };

            var results = new List<ErrorDto>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                if (filtro.tipoDoc == "TS")
                {
                    foreach (var sinpe in transaccionesList)
                    {
                        //Valida sinpe
                        var validaSinpe = _factory.CrearServicio(CodEmpresa, filtro.usuario).fxValidacionSinpe(CodEmpresa, sinpe.nsolicitud.ToString(), filtro.usuario);

                        if (validaSinpe != null)
                        {
                            if (validaSinpe.Code == -1)
                            {
                                results.Add(new ErrorDto
                                {
                                    Code = -1,
                                    Description = "N° " + sinpe.nsolicitud + "-" + validaSinpe.Description,
                                });
                                continue;
                            }
                            else
                            {
                                results.Add(new ErrorDto
                                {
                                    Code = validaSinpe.Code,
                                    Description = "N° " + sinpe.nsolicitud + "-" + validaSinpe.Description,
                                });
                            }
                        }


                        if (CodEmpresa == 61)
                        {
                            switch (sinpe.tipo_girosinpe)
                            {
                                case "CD": //Credito Directo
                                    _factory.CrearServicio(CodEmpresa, filtro.usuario).fxTesEmisionSinpeCreditoDirecto(
                                        CodEmpresa,
                                        sinpe.nsolicitud,
                                        DateTime.Now, filtro.usuario,
                                        0,
                                        0);
                                    break;
                                case "TR": // Tiempo Real
                                    _factory.CrearServicio(CodEmpresa, filtro.usuario).fxTesEmisionSinpeTiempoReal(
                                        CodEmpresa,
                                        sinpe.nsolicitud,
                                        DateTime.Now, filtro.usuario,
                                        0,
                                        0);
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            _factory.CrearServicio(CodEmpresa, filtro.usuario).fxTesEmisionSinpeCreditoDirecto(
                                        CodEmpresa,
                                        sinpe.nsolicitud,
                                        DateTime.Now, filtro.usuario,
                                        0,
                                        0);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }
        #endregion

    }//End class
}
