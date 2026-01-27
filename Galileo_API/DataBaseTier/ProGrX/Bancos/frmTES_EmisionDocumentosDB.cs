using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo.Models.TES;
using Galileo_API.Controllers.WFCSinpe;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesEmisionDocumentosDb
    {
        private readonly MTesoreria mTesoreria;
        private readonly VerificadorCoreFactory _factory;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly MReportingServicesDB mReporting;
        private readonly PortalDB _portalDB;
        private readonly MTesFuncionesDb mTesFunciones;

        private const string nSolicitudes = "solicitudes";
        private const string nFechas = "fechas";

        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(100);

        public FrmTesEmisionDocumentosDb(IConfiguration config)
        {
            mTesoreria = new MTesoreria(config);
            _factory = new VerificadorCoreFactory(config);
            _Security_MainDB = new MSecurityMainDb(config);
            mReporting = new MReportingServicesDB(config);
            _portalDB = new PortalDB(config);
            mTesFunciones = new MTesFuncionesDb(config);
        }

        #region ===== Helpers mínimos (lo que NO conviene mover) =====

        private static string LimpiarReporte(string nombre)
            => Regex.Replace(nombre ?? string.Empty, @"\.(rdl|rdlc)$", "", RegexOptions.IgnoreCase, RegexTimeout);

        private static (int? solInicio, int? solCorte, DateTime? fechaInicio, DateTime? fechaCorte) GetRangos(TesEmisionDocFiltros f)
            => MTesFuncionesDb.GetRangosEmision(f, nSolicitudes, nFechas);

        private static string SerializarResultadoReporte(IActionResult action)
            => MTesFuncionesDb.SerializarResultadoReporte(action);

        private static TesEmisionDocFiltros ParseFiltros(string filtros)
            => JsonConvert.DeserializeObject<TesEmisionDocFiltros>(filtros) ?? new TesEmisionDocFiltros();

        private static void NormalizarFiltroFechas(TesEmisionDocFiltros filtro)
        {
            if (!string.Equals(filtro.generarPor, nFechas, StringComparison.OrdinalIgnoreCase))
            {
                filtro.fecha_inicio = null;
                filtro.fecha_corte = null;
            }
        }

        #endregion

        #region ===== Modelos =====

        private sealed record QueryBuildResult(string QueryTransac, string BaseQuery, object Parametros);

        private sealed record EmisionContext(
            int CodEmpresa,
            TesEmisionDocFiltros Filtro,
            SqlConnection Conn,
            TesBancoDocsData BancoDocs,
            TesBancoData BancoData,
            int UsaFirmas,
            QueryBuildResult Q,
            TesArchivosEspecialesData? ChequesReport);

        private sealed class ClasificacionState
        {
            public List<TesTransaccionDto> ListaConFirmas { get; } = new();
            public List<TesTransaccionDto> ListaSinFirmas { get; } = new();

            public List<byte[]> PdfsBoleta { get; } = new();
            public FileContentResult? FileResultBoleta { get; set; }

            public string ReporteCkConFirmas { get; set; } = string.Empty;
            public string ReporteCkSinFirmas { get; set; } = string.Empty;
        }

        #endregion

        #region ===== Consultas / catálogo =====

        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumento_Formato_Obtener(int CodEmpresa, int banco)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"exec spTes_Formatos_Bancos @banco";
                return conn.Query(query, new { banco })
                    .Select(row => new DropDownListaGenericaModel { item = row.IDX, descripcion = row.ItmX })
                    .ToList();
            });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumento_Plan_Obtener(int CodEmpresa, int banco)
        {
            const string query = @"
select Bp.COD_PLAN as item, Bp.COD_PLAN as descripcion
from TES_BANCOS B
inner join TES_BANCO_PLANES_TE Bp on B.ID_BANCO = Bp.ID_BANCO
Where B.ID_BANCO = @banco And B.UTILIZA_PLAN = 1
order by Bp.COD_PLAN asc";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDB, CodEmpresa, query, new { banco });
        }

        public ErrorDto<TesTransaccionesData> TES_EmisionDocumento_Buscar(int CodEmpresa, string tipoDoc, int banco, string plan)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"
select isnull(count(*),0) as Total,
       isnull(Min(nsolicitud),0) as Minimo,
       isnull(Max(nsolicitud),0) as Maximo
from Tes_Transacciones
Where Estado='P' And Tipo = @tipoDoc and ID_Banco = @banco";

                var solicitudes = conn.QueryFirstOrDefault<TesTransaccionesData>(query, new { tipoDoc, banco })
                                 ?? new TesTransaccionesData();

                if (solicitudes.total == 0)
                {
                    solicitudes.minimo = 0;
                    solicitudes.maximo = 0;
                }

                solicitudes.docInicial = mTesoreria.fxTesTipoDocConsec(CodEmpresa, banco, tipoDoc, "/", plan).Result;

                var vDato = mTesoreria.fxTesTipoDocExtraeDato(CodEmpresa, banco, tipoDoc, "mod_consec").Result ?? "0";
                solicitudes.docBloqueo = vDato != "1";

                return solicitudes;
            });
        }

        #endregion

        #region ===== Solicitudes =====

        public ErrorDto<List<TesSolicitudesGenData>> TES_EmisionDocumento_Solicitudes_Obtener(int CodEmpresa, string filtros)
        {
            var filtro = ParseFiltros(filtros);
            NormalizarFiltroFechas(filtro);
            var (solInicio, solCorte, fechaInicio, fechaCorte) = GetRangos(filtro);

            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                var consecInt = mTesoreria.fxTesTipoDocConsecInterno(CodEmpresa, filtro.banco, filtro.tipoDoc, "/", filtro.plan).Result;

                var query = @"
Select TOP (@top) t.*,
        (select descripcion from SINPE_MOTIVOS where cod_motivo = t.id_rechazo ) as estadoSinpe,
       dbo.fxTes_Cuentas_Bancarias_Pass(id_Banco,Cta_Ahorros) as Pass
From Tes_Transacciones t
Where t.Estado='P' And t.Tipo = @tipoDoc
  And t.Id_Banco=@banco And t.Autoriza = 'S' and t.fecha_hold is null";

                if (string.Equals(filtro.generarPor, nSolicitudes, StringComparison.OrdinalIgnoreCase))
                    query += " And t.NSolicitud Between @minimo And @maximo";
                else if (string.Equals(filtro.generarPor, nFechas, StringComparison.OrdinalIgnoreCase))
                    query += " And t.Fecha_Solicitud Between @fechaInicio And @fechaCorte";

                query += " Order by t.NSolicitud";

                var result = conn.Query<TesSolicitudesGenData>(query, new
                {
                    top = filtro.cantidad,
                    tipoDoc = filtro.tipoDoc,
                    banco = filtro.banco,
                    minimo = solInicio,
                    maximo = solCorte,
                    fechaInicio,
                    fechaCorte
                }).ToList();

                var now = DateTime.Now;
                foreach (var item in result)
                {
                    item.documento = filtro.tipoDoc == "TE"
                        ? $"{filtro.docInicial:000}-{consecInt}"
                        : filtro.docInicial.ToString(CultureInfo.InvariantCulture);

                    item.fecha = now;
                    item.firmas = item.firmas_autoriza_fecha == null ? "No" : "Sí";
                }

                return result;
            });
        }

        public ErrorDto TES_EmisionDocumento_ValidaNumDocumento(int CodEmpresa, int banco, string tipoDoc, int docInicial, int cantidadList)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                var docFinal = docInicial + (cantidadList - 1);

                const string query = @"
SELECT ndocumento
FROM Tes_Transacciones
WHERE id_Banco = @banco
  AND ndocumento BETWEEN @docInicial AND @docFinal
  AND Tipo = @tipoDoc";

                var lista = conn.Query<int>(query, new { banco, docInicial, docFinal, tipoDoc }).ToList();
                var docExistente = lista.FirstOrDefault(nDoc => nDoc >= docInicial && nDoc <= docFinal);

                if (docExistente != 0)
                    return DbHelper.ErrorResponse($"\nYa existe un Documento asignado [{docExistente}] dentro del rango suministrado", -2);

                return DbHelper.CreateOkResponse();
            });
        }

        public ErrorDto TES_EmisionDocumento_RevisaCuentas_SP(int CodEmpresa, int banco)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                conn.Execute("exec spTes_Cuentas_Revisa @banco", new { banco });
                return DbHelper.OkResponse("Cuentas verificadas correctamente!");
            });
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TES_EmisionDocumento_CtasPuente_Obtener(int CodEmpresa, string Usuario)
        {
            const string query = @"
select 
    B.id_Banco as item,
    rtrim(B.descripcion) as descripcion
from Tes_Bancos B
inner join tes_Banco_ASG A 
    on B.id_Banco = A.id_Banco
   and A.nombre = @usuario
where B.estado = 'A'
  and B.puente = 1";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDB, CodEmpresa, query, new { usuario = Usuario });
        }

        public ErrorDto TES_EmisionDocumento_CtaPuente_Aplicar(int CodEmpresa, int Banco, string Usuario, string Solicitudes)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                var listaSolicitudes = JsonConvert.DeserializeObject<List<int>>(Solicitudes) ?? new List<int>();
                const string query = @"exec spTes_Traslados_Cuenta_Puente @solicitud, @banco, @usuario";

                foreach (var solicitud in listaSolicitudes)
                    conn.Execute(query, new { solicitud, banco = Banco, usuario = Usuario });

                return DbHelper.OkResponse("Solicitudes movidas correctamente");
            });
        }

        #endregion

        #region ===== Generación principal =====

        public ErrorDto<object> TES_EmisionDocumento_Generar(int codEmpresa, string filtros)
        {
            try
            {
                var filtro = ParseFiltros(filtros);
                NormalizarFiltroFechas(filtro);

                using var conn = _portalDB.CreateConnection(codEmpresa);

                var bancoDocs = LoadBancoDocs(conn, filtro);
                if (bancoDocs == null)
                    return DbHelper.CreateErrorResponse<object>("No existe configuración en tes_banco_docs para el banco/tipoDoc indicado.");

                var bancoData = LoadBancoData(conn, filtro);
                if (bancoData == null)
                    return DbHelper.CreateErrorResponse<object>("No existe configuración en Tes_Bancos para el banco indicado.");

                var usaFirmas = LoadFirmasAut(conn, filtro);
                var q = BuildQueries(filtro);

                TesArchivosEspecialesData? chequesReport = null;
                if (bancoDocs.comprobante is "01" or "02" or "03")
                    chequesReport = mTesoreria.sbCargaArchivosEspeciales(codEmpresa, filtro.banco).Result;

                var ctx = new EmisionContext(
                    CodEmpresa: codEmpresa,
                    Filtro: filtro,
                    Conn: conn,
                    BancoDocs: bancoDocs,
                    BancoData: bancoData,
                    UsaFirmas: usaFirmas,
                    Q: q,
                    ChequesReport: chequesReport
                );

                return bancoDocs.comprobante switch
                {
                    "01" or "02" or "03" => ProcesarChequesYBoletas(ctx),
                    "04" => ProcesarTransferencias(ctx),
                    _ => DbHelper.CreateErrorResponse<object>($"Comprobante '{bancoDocs.comprobante}' no soportado.")
                };
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }

        private ErrorDto<object> ProcesarTransferencias(EmisionContext ctx)
        {
            List<TesTransaccionDto> Trans()
                => ctx.Conn.Query<TesTransaccionDto>(ctx.Q.QueryTransac, ctx.Q.Parametros).ToList();

            return ctx.Filtro.formatoTE switch
            {
                "A" => mTesFunciones.SbTeBancoNacionalDesdeRangoSolicitudes(
                        conn: ctx.Conn,
                        codEmpresa: ctx.CodEmpresa,
                        bancoId: ctx.Filtro.banco,
                        tipoDoc: ctx.Filtro.tipoDoc,
                        qBaseIn: ctx.Q.BaseQuery,
                        qParams: ctx.Q.Parametros,
                        transaccionesList: Trans(),
                        resolveConsecutivo: () => mTesoreria.fxTesTipoDocConsec(ctx.CodEmpresa, ctx.Filtro.banco, ctx.Filtro.tipoDoc, "+").Result),

                "B" => MTesFuncionesDb.SbTeBancoPopularCore(
                        codEmpresa: ctx.CodEmpresa,
                        bancoId: ctx.Filtro.banco,
                        tipoDoc: ctx.Filtro.tipoDoc,
                        transaccionesList: Trans(),
                        resolveConsecutivo: () => mTesoreria.fxTesTipoDocConsec(ctx.CodEmpresa, ctx.Filtro.banco, ctx.Filtro.tipoDoc, "+").Result),

                "C" => mTesFunciones.SbTeBcrDesdeRangoSolicitudes(
                        conn: ctx.Conn,
                        codEmpresa: ctx.CodEmpresa,
                        bancoId: ctx.Filtro.banco,
                        tipoDoc: ctx.Filtro.tipoDoc,
                        qBaseIn: ctx.Q.BaseQuery,
                        qParams: ctx.Q.Parametros,
                        transaccionesList: Trans(),
                        resolveBancoConsec: () => mTesoreria.fxTesTipoDocConsec(ctx.CodEmpresa, ctx.Filtro.banco, ctx.Filtro.tipoDoc, "+").Result),

                "D" => mTesFunciones.SbTeBcrEmpresarialArchivo(
                        conn: ctx.Conn,
                        codEmpresa: ctx.CodEmpresa,
                        bancoId: ctx.Filtro.banco,
                        tipoDoc: ctx.Filtro.tipoDoc,
                        resolveBancoConsec: () => mTesoreria.fxTesTipoDocConsec(ctx.CodEmpresa, ctx.Filtro.banco, ctx.Filtro.tipoDoc, "+").Result),

                "F" => mTesFunciones.SbTeBcrComercialArchivo(
                        conn: ctx.Conn,
                        codEmpresa: ctx.CodEmpresa,
                        bancoId: ctx.Filtro.banco,
                        tipoDoc: ctx.Filtro.tipoDoc,
                        resolveBancoConsec: () => mTesoreria.fxTesTipoDocConsec(ctx.CodEmpresa, ctx.Filtro.banco, ctx.Filtro.tipoDoc, "+").Result),

                "E" => mTesFunciones.SbTeBctEnlaceArchivo(
                        conn: ctx.Conn,
                        codEmpresa: ctx.CodEmpresa,
                        bancoId: ctx.Filtro.banco,
                        tipoDoc: ctx.Filtro.tipoDoc,
                        filtros: ctx.Filtro),

                "G" => mTesFunciones.SbTeBncrSinpeArchivo(
                        conn: ctx.Conn,
                        codEmpresa: ctx.CodEmpresa,
                        bancoId: ctx.Filtro.banco,
                        tipoDoc: ctx.Filtro.tipoDoc,
                        filtros: ctx.Filtro),

                "DV1" or "DV2" => mTesFunciones.SbTeFormatoEstandarWrapper(
                        conn: ctx.Conn,
                        codEmpresa: ctx.CodEmpresa,
                        bancoId: ctx.Filtro.banco,
                        tipoDoc: ctx.Filtro.tipoDoc,
                        formato: ctx.Filtro.formatoTE ?? string.Empty,
                        plan: ctx.Filtro.plan),

                "S" => DbHelper.CreateErrorResponse<object>("No se pudo realizar la operación, debido a que la opción de SINPE se encuentra en espera"),

                "SG" => sbTeBancoSinpeGeneral(ctx.CodEmpresa, ctx.Filtro, Trans()),

                _ => mTesFunciones.SbTeFormatoEstandarWrapper(
                        conn: ctx.Conn,
                        codEmpresa: ctx.CodEmpresa,
                        bancoId: ctx.Filtro.banco,
                        tipoDoc: ctx.Filtro.tipoDoc,
                        formato: ctx.Filtro.formatoTE ?? string.Empty,
                        plan: ctx.Filtro.plan)
            };
        }

        #endregion

        #region ===== Cheques / boletas (mantengo igual) =====

        private ErrorDto<object> ProcesarChequesYBoletas(EmisionContext ctx)
        {
            if (ctx.ChequesReport == null)
                return DbHelper.CreateErrorResponse<object>("No se pudo cargar archivos especiales del banco.");

            var now = DateTime.Now;
            var consecutivo = ResolverConsecutivo(ctx);

            var transacciones = ctx.Conn.Query<TesTransaccionDto>(ctx.Q.QueryTransac, ctx.Q.Parametros).ToList();

            var state = new ClasificacionState();
            var reporteData = CrearReporteDataBase(ctx.CodEmpresa, ctx.Filtro.usuario);

            int contador = 0;

            foreach (var item in transacciones)
            {
                if (contador >= ctx.Filtro.verificacion)
                    break;

                var upd = ProcesarTransaccionEmitida(ctx, item, now, consecutivo);
                if (upd.Code != 0)
                    return upd;

                if (ctx.BancoDocs.doc_auto == 1)
                    consecutivo = mTesoreria.fxTesTipoDocConsec(ctx.CodEmpresa, ctx.Filtro.banco, ctx.Filtro.tipoDoc, "+").Result;

                var clas = ClasificarYGenerarBoletaSiAplica(ctx, state, reporteData, item);
                if (clas.Code != 0)
                    return clas;

                contador++;
            }

            var (ckConFirma, ckSinFirma) = GenerarReportesCheques(reporteData, state);

            var boletaReg = GenerarBoletaRegistro(state.PdfsBoleta, state.FileResultBoleta);
            if (boletaReg.Code != 0)
                return boletaReg;

            return DbHelper.CreateOkResponse<object>(JsonConvert.SerializeObject(new
            {
                archivo = new
                {
                    chequeConFirma = ckConFirma,
                    chequeSinFirma = ckSinFirma,
                    boletaRegisto = boletaReg.Result
                },
                strQuery = JsonConvert.SerializeObject(transacciones, Formatting.Indented),
                parametros = ctx.Q.Parametros,
                comprobante = ctx.BancoDocs.comprobante
            }, Formatting.Indented));
        }

        private long ResolverConsecutivo(EmisionContext ctx)
        {
            // Este también lo podrías mover a MTesFuncionesDb, pero lo dejo aquí
            // porque toca reglas de emisión (docBloqueo/docInicial) y Sonar suele
            // no marcarlo duplicado contra otros módulos.
            if (ctx.BancoDocs.doc_auto != 1)
                return 0;

            if (ctx.Filtro.docBloqueo == true)
                return mTesoreria.fxTesTipoDocConsec(ctx.CodEmpresa, ctx.Filtro.banco, ctx.Filtro.tipoDoc, "/").Result;

            if (ctx.Filtro.docInicial > 0)
            {
                const string queryUpdate = @"
update tes_banco_docs
set consecutivo = @consecutivo
where id_banco = @banco and tipo = @tipoDoc";

                ctx.Conn.Execute(queryUpdate, new { consecutivo = ctx.Filtro.docInicial, banco = ctx.Filtro.banco, tipoDoc = ctx.Filtro.tipoDoc });
                return ctx.Filtro.docInicial;
            }

            return mTesoreria.fxTesTipoDocConsec(ctx.CodEmpresa, ctx.Filtro.banco, ctx.Filtro.tipoDoc, "+").Result;
        }

        private ErrorDto<object> ProcesarTransaccionEmitida(EmisionContext ctx, TesTransaccionDto item, DateTime vFecha, long consecutivo)
        {
            var nsolicitud = item.nsolicitud;
            if (nsolicitud <= 0)
                return DbHelper.CreateErrorResponse<object>("NSolicitud inválida al procesar transacción.");

            var queryUpdate = new StringBuilder(@"
UPDATE Tes_Transacciones
SET Estado = 'I',
    Fecha_Emision = @vfecha,
    Ubicacion_Actual = 'T',
    FECHA_TRASLADO = @vfecha,
    User_Genera = @usuario");

            if (ctx.BancoDocs.doc_auto == 1)
                queryUpdate.Append(" ,NDocumento = @consecutivo");

            queryUpdate.Append(" where NSolicitud = @nsolicitud");

            ctx.Conn.Execute(queryUpdate.ToString(), new
            {
                vfecha = vFecha,
                usuario = ctx.Filtro.usuario,
                consecutivo,
                nsolicitud
            });

            mTesoreria.sbTesBancosAfectacion(ctx.CodEmpresa, nsolicitud, "E");
            mTesoreria.sbTesBitacoraEspecial(ctx.CodEmpresa, nsolicitud, "10", "", (ctx.Filtro.usuario ?? "").ToUpperInvariant());

            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = ctx.CodEmpresa,
                Usuario = (ctx.Filtro.usuario ?? "").ToUpperInvariant(),
                DetalleMovimiento = $"Genero Solicitud {nsolicitud}",
                Movimiento = "GENERA - WEB",
                Modulo = 9
            });

            mTesoreria.sbTESActualizaCC(ctx.CodEmpresa,
                new MTesoreria.ActualizaCCParams
                {
                    Codigo = string.IsNullOrEmpty(item.codigo) ? string.Empty : item.codigo.Trim(),
                    Tipo = item.tipo,
                    Documento = consecutivo.ToString(CultureInfo.InvariantCulture),
                    Banco = item.id_banco ?? 0,
                    OP = item.op ?? 0,
                    Modulo = item.modulo,
                    SubModulo = item.submodulo,
                    Referencia = item.referencia ?? 0
                });

            return DbHelper.CreateOkResponse<object>();
        }

        private ErrorDto<object> ClasificarYGenerarBoletaSiAplica(EmisionContext ctx, ClasificacionState state, FrmReporteGlobal reporteData, TesTransaccionDto item)
        {
            if (ctx.BancoDocs.comprobante == "01")
                return ClasificarChequeFormulaContinua(ctx, state, item);

            if (ctx.BancoDocs.comprobante is "02" or "03")
                return GenerarBoletaRegistroPorItem(state, reporteData, item);

            return DbHelper.CreateOkResponse<object>();
        }

        private static ErrorDto<object> ClasificarChequeFormulaContinua(EmisionContext ctx, ClasificacionState state, TesTransaccionDto item)
        {
            var rutaSinFirmas = ctx.ChequesReport?.chequesSinFirmas;
            if (string.IsNullOrWhiteSpace(rutaSinFirmas))
                return DbHelper.CreateErrorResponse<object>("No está configurada la ruta del reporte de cheques sin firmas.");

            if (ctx.UsaFirmas != 1)
            {
                state.ReporteCkSinFirmas = LimpiarReporte(rutaSinFirmas);
                state.ListaSinFirmas.Add(item);
                return DbHelper.CreateOkResponse<object>();
            }

            var rutaConFirmas = ctx.ChequesReport?.chequesFirmas;
            if (string.IsNullOrWhiteSpace(rutaConFirmas))
                return DbHelper.CreateErrorResponse<object>("No está configurada la ruta del reporte de cheques con firmas.");

            bool firmaAutorizada = item.firmas_autoriza_fecha != null;

            var desde = ctx.BancoData.firmas_desde;
            var hasta = ctx.BancoData.firmas_hasta;

            bool vaConFirmas = firmaAutorizada || (item.monto >= desde && item.monto <= hasta);

            if (vaConFirmas)
            {
                state.ReporteCkConFirmas = LimpiarReporte(rutaConFirmas);
                state.ListaConFirmas.Add(item);
            }
            else
            {
                state.ReporteCkSinFirmas = LimpiarReporte(rutaSinFirmas);
                state.ListaSinFirmas.Add(item);
            }

            return DbHelper.CreateOkResponse<object>();
        }

        private ErrorDto<object> GenerarBoletaRegistroPorItem(ClasificacionState state, FrmReporteGlobal reporteData, TesTransaccionDto item)
        {
            reporteData.nombreReporte = "Banking_BoletaRegistro";
            reporteData.parametros = JsonConvert.SerializeObject(new { nSolicitud = item.nsolicitud });

            var action = mReporting.ReporteRDLC_v2(reporteData);

            if (action is ObjectResult obj)
            {
                var jres = System.Text.Json.JsonSerializer.Serialize(obj.Value);
                var err = System.Text.Json.JsonSerializer.Deserialize<ErrorDto>(jres) ?? new ErrorDto();
                return DbHelper.CreateErrorResponse<object>(err.Description ?? $"Error al generar boleta para solicitud {item.nsolicitud}.");
            }

            state.FileResultBoleta = action as FileContentResult;

            if (state.FileResultBoleta?.FileContents is { Length: > 0 } bytes)
            {
                state.PdfsBoleta.Add(bytes);
                return DbHelper.CreateOkResponse<object>();
            }

            return DbHelper.CreateErrorResponse<object>($"Ocurrió un error al generar la boleta de la solicitud {item.nsolicitud}, contenido nulo o vacío.");
        }

        private (string ckConFirma, string ckSinFirma) GenerarReportesCheques(FrmReporteGlobal reporteData, ClasificacionState state)
        {
            string ckCon = string.Empty;
            string ckSin = string.Empty;

            if (state.ListaConFirmas.Count > 0 && !string.IsNullOrWhiteSpace(state.ReporteCkConFirmas))
            {
                reporteData.parametros = System.Text.Json.JsonSerializer.Serialize(state.ListaConFirmas);
                reporteData.nombreReporte = state.ReporteCkConFirmas;
                ckCon = SerializarResultadoReporte(mReporting.ReporteRDLC_v2(reporteData));
            }

            if (state.ListaSinFirmas.Count > 0 && !string.IsNullOrWhiteSpace(state.ReporteCkSinFirmas))
            {
                reporteData.parametros = System.Text.Json.JsonSerializer.Serialize(state.ListaSinFirmas);
                reporteData.nombreReporte = state.ReporteCkSinFirmas;
                ckSin = SerializarResultadoReporte(mReporting.ReporteRDLC_v2(reporteData));
            }

            return (ckCon, ckSin);
        }

        private static ErrorDto<object> GenerarBoletaRegistro(List<byte[]> pdfsBoleta, FileContentResult? fileResultBoleta)
            => MTesFuncionesDb.GenerarBoletaRegistroPdf(pdfsBoleta, fileResultBoleta);

        private static FrmReporteGlobal CrearReporteDataBase(int codEmpresa, string? usuario) => new()
        {
            codEmpresa = codEmpresa,
            parametros = null,
            nombreReporte = "",
            usuario = usuario ?? string.Empty,
            cod_reporte = "P",
            folder = "Bancos"
        };

        #endregion

        #region ===== Carga de datos / queries base =====

        private static TesBancoDocsData? LoadBancoDocs(SqlConnection connection, TesEmisionDocFiltros filtro)
            => MTesFuncionesDb.LoadBancoDocs(connection, filtro.banco, filtro.tipoDoc);

        private static TesBancoData? LoadBancoData(SqlConnection connection, TesEmisionDocFiltros filtro)
            => MTesFuncionesDb.LoadBancoData(connection, filtro.banco);

        private static int LoadFirmasAut(SqlConnection connection, TesEmisionDocFiltros filtro)
            => MTesFuncionesDb.LoadFirmasAut(connection, filtro.banco, filtro.usuario);

        private static QueryBuildResult BuildQueries(TesEmisionDocFiltros filtro)
        {
            var (solInicio, solCorte, fechaInicio, fechaCorte) = GetRangos(filtro);
            var (queryTransac, baseQuery, parametros) = MTesFuncionesDb.BuildQueriesEmision(
                banco: filtro.banco,
                tipoDoc: filtro.tipoDoc,
                cantidad: filtro.cantidad,
                generarPor: filtro.generarPor,
                minimo: solInicio,
                maximo: solCorte,
                fechaInicio: fechaInicio,
                fechaCorte: fechaCorte,
                nSolicitudes: nSolicitudes,
                nFechas: nFechas);

            return new QueryBuildResult(queryTransac, baseQuery, parametros);
        }

        #endregion

        #region ===== SINPE General =====

        private ErrorDto<object> sbTeBancoSinpeGeneral(int codEmpresa, TesEmisionDocFiltros filtro, List<TesTransaccionDto> transaccionesList)
        {
            if (!string.Equals(filtro.tipoDoc, "TS", StringComparison.OrdinalIgnoreCase))
                return DbHelper.CreateOkResponse<object>(JsonConvert.SerializeObject(new { results = Array.Empty<ErrorDto>() }, Formatting.Indented));

            if (transaccionesList == null || transaccionesList.Count == 0)
                return DbHelper.CreateOkResponse<object>(JsonConvert.SerializeObject(new { results = Array.Empty<ErrorDto>() }, Formatting.Indented));

            if (string.IsNullOrWhiteSpace(filtro.usuario))
                return DbHelper.CreateErrorResponse<object>("Usuario requerido para procesar SINPE.");

            try
            {
                var servicio = _factory.CrearServicio(codEmpresa, filtro.usuario);
                var results = new List<ErrorDto>(capacity: transaccionesList.Count);

                foreach (var trx in transaccionesList)
                    results.Add(EmitirSinpe(servicio, codEmpresa, filtro.usuario, trx));

                return DbHelper.CreateOkResponse<object>(JsonConvert.SerializeObject(new { results }, Formatting.Indented));
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }

        private static ErrorDto EmitirSinpe(IWfcSinpe servicio, int codEmpresa, string usuario, TesTransaccionDto trx)
            => MTesFuncionesDb.EmitirSinpe(servicio, codEmpresa, usuario, trx);

        #endregion
    }
}
