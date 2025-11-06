using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.Security;
using PgxAPI.Models.TES;
using System.Text;
using TesTransaccionDto = PgxAPI.Models.TES.TesTransaccionDto;

namespace PgxAPI.DataBaseTier
{
    public class frmTES_EmisionDocumentosDB
    {
        private readonly IConfiguration? _config;
        private readonly MTesoreria MTesoreria;
        private MSecurityMainDb DBBitacora;
        //private mReportingServicesDB mReporting;
        //private mServiciosWCFDB mServiciosWCF;
        private mProGrX_AuxiliarDB mAuxiliar;
        private string NumNegocio = "";
        private string CedulaReg = "";
        private string Razon = "";

        public frmTES_EmisionDocumentosDB(IConfiguration config)
        {
            _config = config;
            MTesoreria = new MTesoreria(config);
            DBBitacora = new MSecurityMainDb(_config);
            //mReporting = new mReportingServicesDB(_config);
            //mServiciosWCF = new mServiciosWCFDB(_config);
            mAuxiliar = new mProGrX_AuxiliarDB(_config);
            NumNegocio = _config.GetSection("BCRFormat").GetSection("NumNegocio").Value.ToString();
            CedulaReg = _config.GetSection("BCRFormat").GetSection("CedulaReg").Value.ToString();
            Razon = _config.GetSection("BCRFormat").GetSection("Razon").Value.ToString();
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return DBBitacora.Bitacora(data);
        }

        /// <summary>
        /// Obtener formatos de banco
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="banco"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumento_Formato_Obtener(int CodEmpresa, int banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"exec spTes_Formatos_Bancos @banco";
                    response.Result = connection.Query(query, new { banco = banco })
                        .Select(row => new DropDownListaGenericaModel
                        {
                            item = row.IDX,
                            descripcion = row.ItmX
                        }).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtener planes 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="banco"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumento_Plan_Obtener(int CodEmpresa, int banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select Bp.COD_PLAN as item, Bp.COD_PLAN as descripcion
                        from TES_BANCOS B inner join TES_BANCO_PLANES_TE Bp on B.ID_BANCO = Bp.ID_BANCO 
                        Where B.ID_BANCO = @banco And B.UTILIZA_PLAN = 1 order by Bp.COD_PLAN asc";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query,
                        new { banco = banco }).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
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
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TesTransaccionesData>
            {
                Code = 0,
                Result = new TesTransaccionesData()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select isnull(count(*),0) as Total,isnull(Min(nsolicitud),0) as Minimo,
                        isnull(Max(nsolicitud),0) as Maximo from Tes_Transacciones
                        Where Estado='P' And Tipo = @tipoDoc and ID_Banco = @banco";
                    var result = connection.QueryFirstOrDefault<TesTransaccionesData>(query,
                        new { tipoDoc = tipoDoc, banco = banco });

                    response.Result = result ?? new TesTransaccionesData();

                    // Si no hay solicitudes
                    if (response.Result.total == 0)
                    {
                        response.Result.minimo = 0;
                        response.Result.maximo = 0;
                    }

                    // Obtener consecutivo inicial
                    response.Result.docInicial = MTesoreria.fxTesTipoDocConsec(CodEmpresa, banco, tipoDoc, "/", plan).Result;

                    // Verificar si se puede modificar
                    string vDato = MTesoreria.fxTesTipoDocExtraeDato(CodEmpresa, banco, tipoDoc, "mod_consec").Result;
                    response.Result.docBloqueo = vDato != "1";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
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
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<TesSolicitudesGenData>>
            {
                Code = 0,
                Result = new List<TesSolicitudesGenData>()
            };
            long consecInt = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    consecInt = MTesoreria.fxTesTipoDocConsecInterno(CodEmpresa, filtro.banco, filtro.tipoDoc, "/", filtro.plan).Result;

                    var query = @$"Select TOP {filtro.cantidad} *, dbo.fxTes_Cuentas_Bancarias_Pass(id_Banco,Cta_Ahorros) as 'Pass'
                        From Tes_Transacciones Where Estado='P' And Tipo = @tipoDoc
                        And Id_Banco=@banco And Autoriza = 'S' and fecha_hold is null";

                    if (filtro.generarPor == "solicitudes")
                    {
                        query += " And NSolicitud Between @minimo And @maximo";
                    }
                    else if (filtro.generarPor == "fechas")
                    {
                        query += @" And Fecha_Solicitud Between @fechaInicio And @fechaCorte";
                    }
                    query += " Order by NSolicitud";

                    var fechaInicio = filtro.fecha_inicio?.Date;
                    var fechaCorte = filtro.fecha_corte?.Date.AddDays(1).AddTicks(-1);

                    var result = connection.Query<TesSolicitudesGenData>(query,
                            new {
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

                    response.Result = result ?? new List<TesSolicitudesGenData>();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
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
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = ""
            };
            try
            {
                int docFinal = docInicial + (cantidadList - 1);

                using var connection = new SqlConnection(stringConn);
                {
                    var query = @" SELECT ndocumento FROM Tes_Transacciones
                        WHERE id_Banco = @banco AND ndocumento BETWEEN @docInicial AND @docFinal
                        AND Tipo = @tipoDoc'";
                    var lista = connection.Query<int>(query,
                        new {
                            banco = banco,
                            docInicial = docInicial,
                            docFinal = docFinal,
                            tipoDoc = tipoDoc
                        }).ToList();

                    foreach (var nDoc in lista)
                    {
                        if (nDoc >= docInicial && nDoc <= docFinal)
                        {
                            response.Code = -2;
                            response.Description = $"\nYa existe un Documento asignado [{nDoc}] dentro del rango suministrado";
                            return response;
                        }
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

        /// <summary>
        /// Ejecuta un SP que Revisa Cuentas Bancarias de Solicitudes Pendientes de Emitir
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="banco"></param>
        /// <returns></returns>
        public ErrorDto TES_EmisionDocumento_RevisaCuentas_SP(int CodEmpresa, int banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto
            {
                Code = 0,
                Description = ""
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "exec spTes_Cuentas_Revisa @banco";
                    connection.Execute(query, new { banco = banco });

                    resp.Description = "Cuentas verificadas correctamente!";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Obtener solicitudes registradas en la Cuenta Bancaria [Puente]
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="banco"></param>
        /// <param name="tipoDoc"></param>
        /// <returns></returns>
        public ErrorDto<List<TesTransaccionDto>> TES_EmisionDocumento_SolicitudesCtaPuente_Obtener(int CodEmpresa, int banco, string tipoDoc)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<TesTransaccionDto>>
            {
                Code = 0,
                Description = ""
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select nsolicitud,codigo,beneficiario,monto,tipo,cta_Ahorros
                        from Tes_Transacciones where id_banco = @banco and  ESTADO = 'P' and Tipo = @tipo";
                    resp.Result = connection.Query<TesTransaccionDto>(query, new { banco = banco, tipo = tipoDoc }).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Obtener cuentas puente
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumento_CtasPuente_Obtener(int CodEmpresa, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = ""
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select B.id_Banco as item ,rtrim(B.descripcion) as descripcion 
                        from Tes_Bancos B inner join tes_Banco_ASG A on B.id_Banco = A.id_Banco 
                        and A.nombre = @usuario Where B.estado = 'A' and B.puente  = 1";
                    resp.Result = connection.Query<DropDownListaGenericaModel>(query, new { usuario = Usuario }).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Traslada las Solicitudes seleccionadas entre Cuentas (Puente)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Banco"></param>
        /// <param name="Usuario"></param>
        /// <param name="Solicitudes"></param>
        /// <returns></returns>
        public ErrorDto TES_EmisionDocumento_CtaPuente_Aplicar(int CodEmpresa, int Banco, string Usuario, string Solicitudes)
        {
            List<int> listaSolicitudes = JsonConvert.DeserializeObject<List<int>>(Solicitudes) ?? new List<int>();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto
            {
                Code = 0,
                Description = ""
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    foreach (var item in listaSolicitudes)
                    {
                        var query = @"exec spTes_Traslados_Cuenta_Puente @solicitud, @banco, @usuario";
                        connection.Execute(query, new { banco = Banco, usuario = Usuario, solicitud = item });
                    }
                    resp.Description = "Solicitudes movidas correctamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
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
            var query = "";

            var fechaInicio = filtro.fecha_inicio?.Date;
            var fechaCorte = filtro.fecha_corte?.Date.AddDays(1).AddTicks(-1);
            long vConsecutivo = 0;

            if (filtro.generarPor != "fechas")
            {
                filtro.fecha_inicio = null;
                filtro.fecha_corte = null;
            }

            DateTime vFecha = DateTime.Now; //Devuelve la fecha del servidor
            try
            {
                chequesReport = MTesoreria.sbCargaArchivosEspeciales(CodEmpresa, filtro.banco).Result;
                using var connection = new SqlConnection(stringConn);
                {
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
                    if (filtro.generarPor == "solicitudes")
                    {
                        queryTransac += " And NSolicitud Between @minimo And @maximo";
                    }
                    else if (filtro.generarPor == "fechas")
                    {
                        queryTransac += @" And Fecha_Solicitud Between @fechaInicio And @fechaCorte";
                    }
                    queryTransac += " Order by Nsolicitud";

                    //Lista de solo los nsolicitud de Tes_Transacciones
                    var baseQuery = $@"(SELECT TOP {filtro.cantidad} nsolicitud 
                                    FROM Tes_Transacciones  WHERE Estado = 'P' AND Tipo = @tipoDoc 
                                    AND ID_Banco = @banco AND Autoriza = 'S' AND fecha_hold IS NULL";
                    if (filtro.generarPor == "solicitudes")
                    {
                        baseQuery += " And NSolicitud Between @minimo And @maximo";
                    }
                    else if (filtro.generarPor == "fechas")
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

                    switch (bancoDocs.comprobante) {
                        case "01" or "02" or "03"://CK formula continua /CK Bloque / Registro Doc
                            if (bancoDocs.doc_auto == 1)
                            {
                                //Revisa que el Consecutivo, Sea Modificable o No, si lo es inicializar por el indicado por el usuario
                                if (filtro.docBloqueo != null && filtro.docBloqueo == true)
                                {
                                    vConsecutivo = MTesoreria.fxTesTipoDocConsec(CodEmpresa, filtro.banco, filtro.tipoDoc, "/").Result;
                                }
                                else
                                {
                                    if (vConsecutivo == 0) {
                                        vConsecutivo = filtro.docInicial;

                                        var queryUpdate = "update tes_banco_docs set consecutivo = @consecutivo where id_banco = @banco and tipo = @tipoDoc";
                                        connection.Execute(queryUpdate, new { consecutivo = vConsecutivo, banco = filtro.banco, tipoDoc = filtro.tipoDoc });
                                    }
                                    else
                                    {
                                        vConsecutivo = MTesoreria.fxTesTipoDocConsec(CodEmpresa, filtro.banco, filtro.tipoDoc, "+").Result;
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
                            FrmReporteGlobal reporteData = new FrmReporteGlobal();
                            reporteData.codEmpresa = CodEmpresa;
                            reporteData.parametros = null;
                            reporteData.nombreReporte = "";
                            reporteData.usuario = filtro.usuario;
                            reporteData.cod_reporte = "P";
                            reporteData.folder = "Bancos";

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

                                    MTesoreria.sbTesBancosAfectacion(CodEmpresa, item.nsolicitud, "E");
                                    MTesoreria.sbTesBitacoraEspecial(CodEmpresa, item.nsolicitud, "10", "", filtro.usuario.ToUpper());

                                    Bitacora(new BitacoraInsertarDto
                                    {
                                        EmpresaId = CodEmpresa,
                                        Usuario = filtro.usuario.ToUpper(),
                                        DetalleMovimiento = $"Genero Solicitud {item.nsolicitud}",
                                        Movimiento = "GENERA - WEB",
                                        Modulo = 9
                                    });

                                    //Actualiza Cuentas Corrientes
                                    MTesoreria.sbTESActualizaCC(
                                        CodEmpresa,
                                        item.codigo.Trim(),
                                        item.tipo,
                                        vConsecutivo.ToString(),
                                        item.id_banco,
                                        item.op == null ? 0 : item.op.Value,
                                        item.modulo,
                                        item.submodulo,
                                        item.referencia == null ? 0 : item.referencia.Value
                                    );

                                    if (bancoDocs.doc_auto == 1)
                                    {
                                        vConsecutivo = MTesoreria.fxTesTipoDocConsec(CodEmpresa, filtro.banco, filtro.tipoDoc, "+").Result;
                                    }

                                    //Identifica tipo de reporte
                                    switch (bancoDocs.comprobante)
                                    {
                                        case "01": //Cheques Formula Continua

                                            //Si utiliza firmas, preguntar por el rango en montos
                                            if (vFirmas == 1)
                                            {
                                                if (item.monto >= bancoData.firmas_desde && item.monto <= bancoData.firmas_hasta)
                                                {
                                                    reporteCkConFirmas = chequesReport.chequesFirmas.Replace(".rdl", "").Replace(".rdlc", "").Replace(".RDL", "").Replace(".RDLC", ""); //Reporte con Firmas
                                                    listaRecorridaConFirmas.Add(item);
                                                }
                                                else if (vFirmaAutorizada)
                                                {
                                                    reporteCkConFirmas = chequesReport.chequesFirmas.Replace(".rdl", "").Replace(".rdlc", "").Replace(".RDL", "").Replace(".RDLC", ""); //Reporte con Firmas
                                                    listaRecorridaConFirmas.Add(item);
                                                }
                                                else
                                                {
                                                    reporteCkSinFirmas = chequesReport.chequesSinFirmas.Replace(".rdl", "").Replace(".rdlc", "").Replace(".RDL", "").Replace(".RDLC", ""); //Reporte sin Firmas
                                                    listaRecorridaSinFirmas.Add(item);
                                                }
                                            }
                                            else
                                            {
                                                reporteCkSinFirmas = chequesReport.chequesSinFirmas.Replace(".rdl", "").Replace(".rdlc", "").Replace(".RDL", "").Replace(".RDLC", ""); //Reporte sin Firmas
                                                listaRecorridaSinFirmas.Add(item);
                                            }

                                            break;
                                        case "02" or "03": //Cheques Block / Boleta de Transaccion

                                            listaRecorridaBoleta.Add(item);

                                            //Genera reporte de Boleta de Transaccion
                                            reporteData.nombreReporte = "Banking_BoletaRegistro";
                                            reporteData.parametros = JsonConvert.SerializeObject(new { nSolicitud = item.nsolicitud });
                                            //var actionBoleta = mReporting.ReporteRDLC_v2(reporteData);

                                            //Valida respuesta de ReporteRDLC_v2
                                            //var objectResult = actionBoleta as ObjectResult;

                                            // if (objectResult == null)
                                            // {
                                            //     fileResultBoleta = actionBoleta as FileContentResult;

                                            //     if (fileResultBoleta != null && fileResultBoleta.FileContents != null && fileResultBoleta.FileContents.Length > 0)
                                            //     {
                                            //         pdfsBoleta.Add(fileResultBoleta.FileContents);
                                            //     }
                                            //     else
                                            //     {
                                            //         response.Code = -1;
                                            //         response.Description = "Ocurrió un error al generar la boleta de la solicitud "+ item.nsolicitud +", contenido es nulo o vacío";
                                            //         return response;
                                            //     }
                                            // }
                                            // else
                                            // {
                                            //     var res = objectResult.Value;
                                            //     //converte res a JSON
                                            //     var Jres = System.Text.Json.JsonSerializer.Serialize(res);
                                            //     // convierte JSON a ErrorDto
                                            //     var err = System.Text.Json.JsonSerializer.Deserialize<ErrorDto>(Jres);

                                            //     response.Code = -1;
                                            //     response.Description = err.Description ?? "Ocurrió un error al generar la boleta de la solicitud "+item.nsolicitud;
                                            //     return response;
                                            // }

                                            break;
                                        default:
                                            break;
                                    }

                                    contador++;

                                } else {
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
                                    string nsolicitudes = "";
                                    foreach (var item in listaRecorridaConFirmas)
                                    {
                                        nsolicitudes += item.nsolicitud + ",";
                                    }
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

                                    //var actionResult1 = mReporting.ReporteRDLC_v2(reporteData);

                                    //Valida respuesta de ReporteRDLC_v2
                                    // var objectResult = actionResult1 as ObjectResult;

                                    // if (objectResult == null)
                                    // {
                                    //     ckConFirma = JsonConvert.SerializeObject(actionResult1, Formatting.Indented);
                                    // }
                                    // else
                                    // {
                                    //     var res = objectResult.Value;
                                    //     //converto res a JSON
                                    //     var Jres = System.Text.Json.JsonSerializer.Serialize(res);

                                    //     // convierto JSON a ErrorDto
                                    //     var err = System.Text.Json.JsonSerializer.Deserialize<ErrorDto>(Jres);

                                    //     ckConFirma = JsonConvert.SerializeObject(err, Formatting.Indented);
                                    // }
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

                                   // var actionResult2 = mReporting.ReporteRDLC_v2(reporteData);

                                    //Valida respuesta de ReporteRDLC_v2
                                    // var objectResult = actionResult2 as ObjectResult;

                                    // if (objectResult == null)
                                    // {
                                    //     ckSinFirma = JsonConvert.SerializeObject(actionResult2, Formatting.Indented);
                                    // }
                                    // else
                                    // {
                                    //     var res = objectResult.Value;
                                    //     //converto res a JSON
                                    //     var Jres = System.Text.Json.JsonSerializer.Serialize(res);

                                    //     // convierto JSON a ErrorDto
                                    //     var err = System.Text.Json.JsonSerializer.Deserialize<ErrorDto>(Jres);

                                    //     ckSinFirma = JsonConvert.SerializeObject(err, Formatting.Indented);
                                    // }
                                }

                            }

                            //Boleta de Registro
                            string boletaReg = string.Empty;
                            if (pdfsBoleta.Count > 0 && fileResultBoleta != null)
                            {
                                fileResultBoleta.FileContents = mAuxiliar.CombinarBytesPdfSharp(pdfsBoleta.ToArray());
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
                                    response.Code = -1;
                                    response.Description = "No se pudo realizar la operación, debido a que la opción de SINPE se encuentra en espera";
                                    // Banco General SINPE
                                    //transaccionesList = connection.Query<TES_TransaccionDto>(queryTransac, parametros).ToList();
                                    //    response = sbTeBancoSinpeGeneral(CodEmpresa, filtro, transaccionesList);
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
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

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
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<object>
            {
                Code = 0,
                Description = ""
            };
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
                Seguridad_PortalDB seguridadPortal = new Seguridad_PortalDB(_config);
                string Empresa_Name = "TF " + seguridadPortal.SeleccionarPgxClientePorCodEmpresa(CodEmpresa).PGX_CORE_DB;
                string vConcepto = Empresa_Name.PadRight(30, ' ');

                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select Cta,codigo_Cliente from tes_Bancos Where id_Banco = @banco";
                    var bancoData = connection.QueryFirstOrDefault(query, new { banco = BancoID });
                    if (bancoData != null)
                    {
                        vCuentaEmpresa = bancoData.Cta;
                        vCuentaEmpresa = vCuentaEmpresa.ToString().Trim().Replace("-", "");
                        vNumCliente = bancoData.codigo_Cliente;
                        vNumCliente = vNumCliente.PadLeft(6, '0');
                    }
                    query = "select DESCRIPCION from tes_Bancos where ID_BANCO = @banco";
                    string BancoNombre = connection.QueryFirstOrDefault<string>(query, new { banco = BancoID }) ?? "";

                    //Inicializa Variables de Bancos y Consecutivo
                    string BancoTDoc = filtros.tipoDoc;
                    long BancoConsec = MTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+").Result;

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

                        string linea = "3"; //Credito
                        linea += cuenta.Substring(5, 3);   // Oficina apertura
                        linea += cuenta.Substring(0, 3);   // Tipo de cuenta (100 o 200)
                        linea += "01";                     // Moneda colones
                        linea += cuenta.Substring(cuenta.Length - 7); // 7 dígitos finales
                        
                        decimal monto = (decimal)item.monto;
                        curMonto2 += monto;
                        curCuentas += long.Parse(cuenta.Substring(cuenta.Length - 7, 6)); // sin dígito verificador

                        linea += i.ToString("D8"); //8d Numero Comprobante (Consecutivo Interno)

                        string strMontoDet = monto.ToString("0000000000.00").Replace(".", ""); //12d Monto sin el punto decimal

                        linea += strMontoDet;
                        linea += vConcepto; //30d Concepto de Pago
                        linea += "00"; //Fin de Linea
                        sb.AppendLine(linea);
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

                    resp.Result = JsonConvert.SerializeObject(archivo, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
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
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<object>
            {
                Code = 0,
                Description = ""
            };
            DateTime vFecha = DateTime.Now; //Devuelve la fecha del servidor
            string strCadena = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Inicializa Variables de Bancos y Consecutivo
                    int BancoID = filtros.banco;
                    string BancoTDoc = filtros.tipoDoc;
                    long BancoConsec = MTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+").Result;

                    // En vez de guardar el archivo, lo devuelve como string
                    var sb = new StringBuilder();

                    foreach (var item in transaccionesList)
                    {
                        switch (item.codigo!.Trim().Length)
                        {
                            case 8:
                                strCadena = "0" + item.codigo.Trim().Substring(0, 1) + "0" + item.codigo.Trim().Substring(1, 7);
                                break;
                            case 9:
                                strCadena = "0" + item.codigo.Trim();
                                break;
                            case < 8:
                                strCadena = Convert.ToInt64(item.codigo).ToString("D10"); 
                                break;
                            case > 10:
                                strCadena = item.codigo.Substring(0, 4) + "0" + item.codigo.Substring(5, 5);
                                break;
                            default:
                                strCadena = item.codigo.Trim();
                                break;
                        }

                        string strNombre = item.beneficiario!.Trim();

                        if(strNombre.Length > 30)
                        {
                            strNombre = strNombre.Substring(0, 30);
                        } 
                        else
                        {
                            strNombre = strNombre.PadRight(30, ' ');
                        }
                        strCadena += strNombre;

                        string strCuenta = item.cta_ahorros == null ? "0" : item.cta_ahorros.Trim();

                        if (strCuenta.Length > 13)
                        {
                            strCuenta = strCuenta.Substring(0, 13);
                        }
                        else
                        {
                            strCuenta = strCuenta.PadLeft(13, '0');
                        }
                        strCadena += strCuenta;

                        string strSelf = " ";
                        strCadena += strSelf;

                        decimal monto = (decimal)item.monto!;
                        string strMonto = monto.ToString("000000000.00").Replace(".", "");

                        strCadena += strMonto;

                        string strFecha = string.Format("{0:ddMMyyyy}", vFecha);
                        strCadena += strFecha;

                        string strTipo = "A";
                        strCadena += strTipo;

                        string strProducto = "06";
                        strCadena += strProducto;

                        string strEstado = "P";
                        strCadena += strEstado;

                        strCadena += strFecha;
                        strCadena += strMonto;

                        sb.AppendLine(strCadena);
                    }

                    // Devolver el contenido generado en el object
                    var archivo = new
                    {
                        bancoConsec = BancoConsec.ToString(),
                        extension = "txt",
                        contenido = sb.ToString()
                    };

                    resp.Result = JsonConvert.SerializeObject(archivo, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Generacion con Formatos Estandares de Transferencias Bancarias (Genera archivo)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private ErrorDto<object> sbTeFormatoEstandar(int CodEmpresa, TesEmisionDocFiltros filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
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
                using var connection = new SqlConnection(stringConn);
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

                    query = "select DESCRIPCION from tes_Bancos where ID_BANCO = @banco";
                    string BancoNombre = connection.QueryFirstOrDefault<string>(query, new { banco = BancoID }) ?? "";

                    //Inicializa Variables de Bancos y Consecutivo
                    string BancoTDoc = filtros.tipoDoc;
                    string BancoPlan = filtros.plan;
                    long BancoConsec = MTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+", BancoPlan).Result;

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

                    resp.Result = JsonConvert.SerializeObject(archivo, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
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
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<object>
            {
                Code = 0,
                Description = ""
            };
            DateTime vFecha = DateTime.Now; //Devuelve la fecha del servidor
            string strCadena = "";
            try
            {
                string vRazon = Razon.PadRight(30, ' ');
                string vNumNegocio = NumNegocio;
                string vCedulaReg = CedulaReg;

                using var connection = new SqlConnection(stringConn);
                {
                    //Calcular el Numero de Archivo , Numero de la Transferencia en el Dia
                    int i = 1;
                    var query = @"select documento_base,count(*) From Tes_Transacciones 
                    where id_banco = @banco and fecha_emision = @fecha
                    and estado = 'T' group by documento_base";
                    var resultados = connection.QueryFirstOrDefault(query,
                        new { banco = filtros.banco, fecha = vFecha });
                    if (resultados != null)
                    {
                        foreach (var row in resultados)
                        {
                            i++;
                        }
                    }
                    string vConArchivo = i.ToString("D3");

                    //Crear y Sacar la cuenta de Tes_Bancos, se Asume que esta cuenta tiene el digito verificador
                    query = @"select Cta from Tes_Bancos where id_Banco = @banco";
                    string vCuentaBanco = connection.QueryFirstOrDefault<string>(query, new { banco = filtros.banco });
                    //Se indica la oficina 001 de apertura por Omision
                    vCuentaBanco = "001" + int.Parse(vCuentaBanco).ToString("D8");

                    //Calcular TestKey Complementario (de la primera Linea)
                    query = @"select dbo.fxTESBCRTestkey(@cuentaBanco, @montoTotal) as TestKey";
                    int xTestKey = connection.QueryFirstOrDefault<int>(query, 
                        new { cuentaBanco = vCuentaBanco, montoTotal = vMontoTotal });
                    vTestKey = Math.Min(vTestKey + xTestKey, 2147483468);

                    //Validando Largo del TestKey  = 12
                    string vTesKeyCh = vTestKey.ToString().Trim();
                    if (vTesKeyCh.Length > 12)
                    {
                        vTestKey = long.Parse(vTesKeyCh[^12..]); 
                    }

                    //Inicializa Variables de Bancos y Consecutivo
                    int BancoID = filtros.banco;
                    string BancoTDoc = filtros.tipoDoc;
                    long BancoConsec = MTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, " + ").Result;

                    // En vez de guardar el archivo, lo devuelve como string
                    var sb = new StringBuilder();

                    //ENCABEZADO DEL FORMATO DE TRANSFERENCIA
                    strCadena = "000"; //Estado
                    strCadena += vNumNegocio;
                    strCadena += vConArchivo;
                    strCadena += "000000"; 
                    strCadena += vCedulaReg;
                    strCadena += Convert.ToInt64(vTestKey).ToString("D12"); //12 TestKey
                    strCadena += "000000";
                    strCadena += vFecha.Day.ToString("D2") + vFecha.Month.ToString("D2") + vFecha.Year.ToString("D4");
                    strCadena += new string(' ', 21);
                    strCadena += "Y"; //Señal de Y2k
                    sb.AppendLine(strCadena);

                    //DETALLE DE LA TRANSFERENCIA
                    //Linea 1 es la de Debito cuenta Bancaria
                    i = 1;
                    strCadena = "000"; //Estado Relleno con Ceros
                    strCadena += "1"; //Concepto 1 = Cuenta Corriente / 2 Cuenta Ahorro
                    strCadena += "00000"; //Filler 5
                    strCadena += vCuentaBanco.Trim().PadRight(11).Substring(0, 11); //Oficina -> 3c, Cuenta -> 7 + 1 Digito verificador
                    strCadena += "1"; //Moneda  1 = Colones, 2 = Dolares
                    strCadena += "4"; //2 -> Credito, 4 -> Debito
                    strCadena += "0000"; //Codigo de Causa
                    strCadena += BancoConsec.ToString("D4") + i.ToString("D4"); //Numero de Documento 8
                    strCadena += ((long)(vMontoTotal * 100)).ToString("D12"); //12 Sin Decimales
                    strCadena += vFecha.Day.ToString("D2") + vFecha.Month.ToString("D2") + vFecha.Year.ToString("D4"); 
                    strCadena += "0"; //Filler 1
                    strCadena += vRazon; //Razon de Transferencia (Detalle) 30
                    sb.AppendLine(strCadena);

                    foreach (var item in transaccionesList)
                    {
                        i = i + 1;
                        strCadena = "000"; //Estado Relleno con Ceros
                        strCadena += "2"; //Concepto 1 = Cuenta Corriente / 2 Cuenta Ahorro
                        strCadena += "00000"; //Filler 5
                        strCadena += item.cta_ahorros.PadRight(11).Substring(0, 11).Trim(); //Oficina -> 3c, Cuenta -> 7 + 1 Digito verificador
                        strCadena += "1"; //Moneda  1 = Colones, 2 = Dolares
                        strCadena += "2"; //2 -> Credito, 4 -> Debito
                        strCadena += "0000"; //Codigo de Causa
                        strCadena += BancoConsec.ToString("D4") + i.ToString("D4"); //Numero de Documento 8
                        strCadena += ((int)(item.monto * 100)).ToString("D12"); //12 Sin Decimales
                        strCadena += vFecha.Day.ToString("D2") + vFecha.Month.ToString("D2") + vFecha.Year.ToString("D4");
                        strCadena += "0"; //Filler 1
                        strCadena += vRazon; //Razon de Transferencia (Detalle) 30
                        sb.AppendLine(strCadena);
                    }

                    // Devolver el contenido generado en el object
                    var archivo = new
                    {
                        bancoConsec = BancoConsec.ToString(),
                        extension = "BCR",
                        contenido = sb.ToString()
                    };

                    resp.Result = JsonConvert.SerializeObject(archivo, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Procedimiento para crear el nuevo archivo del BCR, Banca Empresarial
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private ErrorDto<object> sbTeBCR_Empresarial(int CodEmpresa, TesEmisionDocFiltros filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<object>
            {
                Code = 0,
                Description = ""
            };
            DateTime vFecha = DateTime.Now; //Devuelve la fecha del servidor
            int txtCantidadSolicitudes = filtros.cantidad;
            var mFechaInicio = filtros.fecha_inicio?.Date;
            var mFechaCorte = filtros.fecha_corte?.Date.AddDays(1).AddTicks(-1);
            int? mSolInicio = null;
            int? mSolCorte = null;
            if (filtros.generarPor == "solicitudes")
            {
                mSolInicio = filtros.minimo;
                mSolCorte = filtros.maximo;
            }
            string vRazon = "";
            string vNumNegocio = "";
            string vCedulaReg = "";
            string strCadena = ""; 
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select REPLACE(cedula_juridica,'-','') as cedula_juridica, nombre From SIF_EMPRESA";
                    var empresa = connection.QueryFirstOrDefault(query);
                    vNumNegocio = empresa?.cedula_juridica.Trim() ?? string.Empty;
                    vCedulaReg = empresa?.cedula_juridica.Trim() ?? string.Empty;
                    vRazon = "TRANSFERENCIAS " + empresa?.nombre ?? string.Empty;

                    //Inicializa Variables de Bancos y Consecutivo
                    int BancoID = filtros.banco;
                    string BancoTDoc = filtros.tipoDoc;
                    long BancoConsec = MTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+").Result;

                    // En vez de guardar el archivo, lo devuelve como string
                    var sb = new StringBuilder();

                    //Calcular el Numero de Archivo , Numero de la Transferencia en el Dia
                    int i = 1;
                    query = @"select documento_base,count(*) From Tes_Transacciones 
                    where id_banco = @banco and fecha_emision = @fecha
                    and estado = 'T' group by documento_base";
                    var resultados = connection.QueryFirstOrDefault(query,
                        new { banco = BancoID, fecha = vFecha });
                    if (resultados != null)
                    {
                        foreach (var row in resultados)
                        {
                            i++;
                        }
                    }
                    string vConArchivo = i.ToString("D3");

                    query = @"select dbo.fxTesCantidadTEDiarias(@fecha ,@banco) as 'Cantidad'";
                    int iLineInicio = connection.QueryFirstOrDefault<int>(query,
                        new { banco = BancoID, fecha = vFecha });

                    //REGISTRO DE CONTROL
                    i = 1;
                    strCadena = "000"; //Estado 3
                    strCadena += (vCedulaReg ?? "").Trim().PadLeft(12, '0'); //Cedula Juridica 12
                    strCadena += vConArchivo; //Consecutivo Archivo 3
                    strCadena += vFecha.ToString("ddMMyyyy"); //Fecha Aplicacion 8
                    strCadena += "000000000000"; //Cedula de Registro 12
                    strCadena += "000000000000"; //12 TestKey  no se genera, se rellena con ceros
                    strCadena += "000000"; //6 Hora Estado Se rellena con ceros
                    strCadena += new string(' ', 6); //filler 6 espacios en blanco
                    strCadena += "TLB"; //Tipo de archivo
                    strCadena += new string(' ', 128); //filler 128 espacios en blanco
                    strCadena += "D"; //Tipo de movinento Debido
                    sb.AppendLine(strCadena);

                    //DEBITOS
                    query = @"exec spTES_BCR_Empresarial 2, @banco, @bancoTDoc, 
                        @numNegocio, @bancoConsec, @cantidad, 
                        @solInicio, @solCorte, @fechaInicio, @fechaCorte";
                    var Linea2 = connection.QueryFirstOrDefault<string>(query,
                        new { 
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
                    sb.AppendLine(Linea2);

                    //CREDITOS
                    query = @"exec spTES_BCR_Empresarial 3, @banco, @bancoTDoc, 
                        @numNegocio, @bancoConsec, @cantidad, 
                        @solInicio, @solCorte, @fechaInicio, @fechaCorte";
                    var Linea3 = connection.QueryFirstOrDefault<string>(query,
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
                    sb.AppendLine(Linea3);

                    // Devolver el contenido generado en el object
                    var archivo = new
                    {
                        bancoConsec = BancoConsec.ToString(),
                        extension = "txt",
                        contenido = sb.ToString()
                    };

                    resp.Result = JsonConvert.SerializeObject(archivo, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Procedimiento para crear el nuevo archivo del BCT
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private ErrorDto<object> sbTeBCT_Enlace(int CodEmpresa, TesEmisionDocFiltros filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<object>
            {
                Code = 0,
                Description = ""
            };
            DateTime vFecha = DateTime.Now; //Devuelve la fecha del servidor
            int txtCantidadSolicitudes = filtros.cantidad;
            var mFechaInicio = filtros.fecha_inicio?.Date;
            var mFechaCorte = filtros.fecha_corte?.Date.AddDays(1).AddTicks(-1);
            int? mSolInicio = null;
            int? mSolCorte = null;
            if (filtros.generarPor == "solicitudes")
            {
                mSolInicio = filtros.minimo;
                mSolCorte = filtros.maximo;
            }
            string strCadena = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Inicializa Variables de Bancos y Consecutivo
                    int BancoID = filtros.banco;
                    string BancoTDoc = filtros.tipoDoc;
                    long BancoConsec = MTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+").Result;

                    // En vez de guardar el archivo, lo devuelve como string
                    var sb = new StringBuilder();

                    //DETALLE DE LA TRANSFERENCIA
                    var query = @"exec spTES_BCT_Enlace @banco, @bancoTDoc, 
                        @bancoConsec, @cantidad, 
                        @solInicio, @solCorte, @fechaInicio, @fechaCorte";
                    var resultado = connection.QueryFirstOrDefault(query,
                        new
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

                    strCadena = resultado.Linea;
                    sb.AppendLine(strCadena);

                    // Devolver el contenido generado en el object
                    var archivo = new
                    {
                        bancoConsec = BancoConsec.ToString(),
                        extension = "txt",
                        contenido = sb.ToString()
                    };

                    resp.Result = JsonConvert.SerializeObject(archivo, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Procedimiento para crear el nuevo archivo del BCR, Banca Comercial
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private ErrorDto<object> sbTeBCR_Comercial(int CodEmpresa, TesEmisionDocFiltros filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<object>
            {
                Code = 0,
                Description = ""
            };
            DateTime vFecha = DateTime.Now; //Devuelve la fecha del servidor
            int txtCantidadSolicitudes = filtros.cantidad;
            var mFechaInicio = filtros.fecha_inicio?.Date;
            var mFechaCorte = filtros.fecha_corte?.Date.AddDays(1).AddTicks(-1);
            int? mSolInicio = null;
            int? mSolCorte = null;
            if (filtros.generarPor == "solicitudes")
            {
                mSolInicio = filtros.minimo;
                mSolCorte = filtros.maximo;
            }
            string vRazon = "";
            string vNumNegocio = "";
            string vCedulaReg = "";
            string strCadena = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select REPLACE(cedula_juridica,'-','') as cedula_juridica, nombre From SIF_EMPRESA";
                    var empresa = connection.QueryFirstOrDefault(query);
                    vNumNegocio = empresa?.cedula_juridica.Trim() ?? string.Empty;
                    vCedulaReg = empresa?.cedula_juridica.Trim() ?? string.Empty;
                    vRazon = "TRANSFERENCIAS " + empresa?.nombre ?? string.Empty;

                    //Inicializa Variables de Bancos y Consecutivo
                    int BancoID = filtros.banco;
                    string BancoTDoc = filtros.tipoDoc;
                    long BancoConsec = MTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+").Result;

                    // En vez de guardar el archivo, lo devuelve como string
                    var sb = new StringBuilder();

                    //Calcular el Numero de Archivo , Numero de la Transferencia en el Dia
                    int i = 1;
                    query = @"select documento_base,count(*) From Tes_Transacciones 
                    where id_banco = @banco and fecha_emision = @fecha
                    and estado = 'T' group by documento_base";
                    var resultados = connection.QueryFirstOrDefault(query,
                        new { banco = BancoID, fecha = vFecha });
                    if (resultados != null)
                    {
                        foreach (var row in resultados)
                        {
                            i++;
                        }
                    }
                    string vConArchivo = i.ToString("D3");

                    query = @"select dbo.fxTesCantidadTEDiarias(@fecha ,@banco) as 'Cantidad'";
                    int iLineInicio = connection.QueryFirstOrDefault<int>(query,
                        new { banco = BancoID, fecha = vFecha });

                    //REGISTRO DE CONTROL
                    i = 1;
                    strCadena = "000"; //Estado 3
                    strCadena += (vCedulaReg ?? "").Trim().PadLeft(12, '0'); //Cedula Juridica 12
                    strCadena += vConArchivo; //Consecutivo Archivo 3
                    strCadena += vFecha.ToString("ddMMyyyy"); //Fecha Aplicacion 8
                    strCadena += "000000000000"; //Cedula de Registro 12
                    strCadena += "000000000000"; //12 Filler con 0
                    strCadena += "000000"; //6 Hora Estado Se rellena con ceros
                    strCadena += "".PadRight(138, '0'); //filler 138 con 0
                    sb.AppendLine(strCadena);

                    //DEBITOS
                    query = @"exec spTES_BCR_Comercial 2, @banco, @bancoTDoc, 
                        @numNegocio, @bancoConsec, @cantidad, 
                        @solInicio, @solCorte, @fechaInicio, @fechaCorte";
                    var Linea2 = connection.QueryFirstOrDefault<string>(query,
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
                    sb.AppendLine(Linea2);

                    //CREDITOS
                    query = @"exec spTES_BCR_Comercial 3, @banco, @bancoTDoc, 
                        @numNegocio, @bancoConsec, @cantidad, 
                        @solInicio, @solCorte, @fechaInicio, @fechaCorte";
                    var Linea3 = connection.QueryFirstOrDefault<string>(query,
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
                    sb.AppendLine(Linea3);

                    // Devolver el contenido generado en el object
                    var archivo = new
                    {
                        bancoConsec = BancoConsec.ToString(),
                        extension = "txt",
                        contenido = sb.ToString()
                    };

                    resp.Result = JsonConvert.SerializeObject(archivo, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Emite la Transferencia en formato SINPE para el BNCR
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private ErrorDto<object> sbTeBNCR_Sinpe(int CodEmpresa, TesEmisionDocFiltros filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<object>
            {
                Code = 0,
                Description = ""
            };
            DateTime vFecha = DateTime.Now; //Devuelve la fecha del servidor
            int txtCantidadSolicitudes = filtros.cantidad;
            var mFechaInicio = filtros.fecha_inicio?.Date;
            var mFechaCorte = filtros.fecha_corte?.Date.AddDays(1).AddTicks(-1);
            int? mSolInicio = null;
            int? mSolCorte = null;
            if (filtros.generarPor == "solicitudes")
            {
                mSolInicio = filtros.minimo;
                mSolCorte = filtros.maximo;
            }
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Inicializa Variables de Bancos y Consecutivo
                    int BancoID = filtros.banco;
                    string BancoTDoc = filtros.tipoDoc;
                    long BancoConsec = MTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+").Result;

                    // En vez de guardar el archivo, lo devuelve como string
                    var sb = new StringBuilder();

                    //ENCABEZADO: LINEA 1
                    var query = @"exec spTES_BNCR_SINPE 1, @banco, @bancoTDoc, 
                        @bancoConsec, @cantidad, 
                        @solInicio, @solCorte, @fechaInicio, @fechaCorte";
                    var Linea1 = connection.QueryFirstOrDefault<string>(query,
                        new
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

                    sb.AppendLine(Linea1);

                    //DEBITOS
                    query = @"exec spTES_BNCR_SINPE 2, @banco, @bancoTDoc, 
                        @bancoConsec, @cantidad, 
                        @solInicio, @solCorte, @fechaInicio, @fechaCorte";
                    var Linea2 = connection.QueryFirstOrDefault<string>(query,
                        new
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
                    sb.AppendLine(Linea2);

                    //CREDITOS
                    query = @"exec spTES_BNCR_SINPE 3, @banco, @bancoTDoc, 
                        @bancoConsec, @cantidad, 
                        @solInicio, @solCorte, @fechaInicio, @fechaCorte";
                    var Linea3 = connection.QueryFirstOrDefault<string>(query,
                        new
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
                    sb.AppendLine(Linea3);

                    // Devolver el contenido generado en el object
                    var archivo = new
                    {
                        bancoConsec = BancoConsec.ToString(),
                        extension = "tef",
                        contenido = sb.ToString()
                    };

                    resp.Result = JsonConvert.SerializeObject(archivo, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        // private ErrorDto<object> sbTeBancoSinpeGeneral(int CodEmpresa, TesEmisionDocFiltros filtro, List<TesTransaccionDto> transaccionesList)
        // {
        //     string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
        //     var resp = new ErrorDto<object>
        //     {
        //         Code = 0,
        //         Description = ""
        //     };

        //     try
        //     {
        //         using var connection = new SqlConnection(stringConn);
        //         {
        //             if (filtro.tipoDoc == "TS")
        //             {
        //                 foreach (var sinpe in transaccionesList)
        //                 {
        //                     //Valida sinpe
        //                     var validaSinpe = mServiciosWCF.fxValidacionSinpe(CodEmpresa, sinpe.nsolicitud.ToString(), filtro.usuario);
        //                     if (validaSinpe != null)
        //                     {
        //                         resp.Code = validaSinpe.Code;
        //                         resp.Description = validaSinpe.Description;
        //                     }

        //                     switch (sinpe.tipo_girosinpe)
        //                     {
        //                         case "CD": //Credito Directo
        //                             mServiciosWCF.fxTesEmisionSinpeCreditoDirecto(
        //                                 CodEmpresa,
        //                                 sinpe.nsolicitud,
        //                                 DateTime.Now, filtro.usuario,
        //                                 0,
        //                                 0);
        //                             break;
        //                         case "TR": // Tiempo Real
        //                             mServiciosWCF.fxTesEmisionSinpeTiempoReal(
        //                                 CodEmpresa,
        //                                 sinpe.nsolicitud,
        //                                 DateTime.Now, filtro.usuario,
        //                                 0,
        //                                 0);
        //                             break;
        //                         default:
        //                             break;
        //                     }


        //                 }

        //             }
        //         }
        //     }
        //     catch (Exception)
        //     {

        //         throw;
        //     }
        //     return resp;
        // }


    }
}
