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
        private readonly IConfiguration _config;
        private readonly MTesoreria mTesoreria;
        private readonly VerificadorCoreFactory _factory;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly MReportingServicesDB mReporting;
        private readonly PortalDB _portalDB;

        private const string nSolicitudes = "solicitudes";
        private const string nFechas = "fechas";
        private const string zero6Append = "000000";
        private const string zero12Append = "000000000000";
        private const string fechaFormat = "yyyy/MM/dd";
        private const string fechaFormat2 = "ddMMyyyy";

        // Sonar: regex sin timeout es hotspot
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(100);

        public FrmTesEmisionDocumentosDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            mTesoreria = new MTesoreria(config);
            _factory = new VerificadorCoreFactory(config);
            _Security_MainDB = new MSecurityMainDb(config);
            mReporting = new MReportingServicesDB(config);
            _portalDB = new PortalDB(config);
        }

        #region ===== Helpers comunes (reducción de duplicación / Sonar) =====

        private string GetParametro(int codEmpresa, string codigo)
            => mTesoreria.fxTesParametro(codEmpresa, codigo);

        private static string LimpiarReporte(string nombre)
            => Regex.Replace(nombre ?? string.Empty, @"\.(rdl|rdlc)$", "", RegexOptions.IgnoreCase, RegexTimeout);

        private static void AppendIfNotEmpty(StringBuilder sb, string? line)
        {
            if (!string.IsNullOrWhiteSpace(line))
                sb.AppendLine(line);
        }

        private static ErrorDto<object> OkJson(object payload)
            => DbHelper.CreateOkResponse<object>(JsonConvert.SerializeObject(payload, Formatting.Indented));

        private static ErrorDto<object> OkObj(object? result = null)
            => DbHelper.CreateOkResponse<object>(result!);

        private static ErrorDto<object> Err(string msg, int code = -1)
            => DbHelper.CreateErrorResponse<object>(msg, code, default!);

        /// <summary>
        /// { bancoConsec, extension, contenido } (evita duplicación en returns)
        /// </summary>
        private static ErrorDto<object> ArchivoResponse(long bancoConsec, string extension, StringBuilder sb)
            => OkJson(new
            {
                bancoConsec = bancoConsec.ToString(CultureInfo.InvariantCulture),
                extension,
                contenido = sb.ToString()
            });

        /// <summary>
        /// Consecutivo del día: 1 + count(distinct documento_base)
        /// </summary>
        private static int GetConsecutivoArchivoDelDia(SqlConnection connection, int bancoId, DateTime fechaEmision)
        {
            const string sql = @"
select count(distinct documento_base)
from Tes_Transacciones
where id_banco = @banco
  and fecha_emision = @fecha
  and estado = 'T'";

            var count = connection.QuerySingle<int>(sql, new { banco = bancoId, fecha = fechaEmision });
            return count + 1;
        }

        /// <summary>
        /// Obtiene num negocio/cedula reg desde SIF_EMPRESA.
        /// </summary>
        private static (string numNegocio, string cedulaReg) GetEmpresaNumNegocioYReg(SqlConnection connection)
        {
            const string sql = "select REPLACE(cedula_juridica,'-','') as cedula_juridica from SIF_EMPRESA";
            var empresa = connection.QueryFirstOrDefault(sql);
            var cedula = empresa?.cedula_juridica?.ToString()?.Trim() ?? string.Empty;
            return (cedula, cedula);
        }

        private static (int? solInicio, int? solCorte, DateTime? fechaInicio, DateTime? fechaCorte) GetRangos(TesEmisionDocFiltros f)
        {
            int? solInicio = string.Equals(f.generarPor, nSolicitudes, StringComparison.OrdinalIgnoreCase) ? f.minimo : null;
            int? solCorte = string.Equals(f.generarPor, nSolicitudes, StringComparison.OrdinalIgnoreCase) ? f.maximo : null;

            DateTime? fechaInicio = string.Equals(f.generarPor, nFechas, StringComparison.OrdinalIgnoreCase) ? f.fecha_inicio?.Date : null;
            DateTime? fechaCorte = string.Equals(f.generarPor, nFechas, StringComparison.OrdinalIgnoreCase) ? f.fecha_corte?.Date.AddDays(1).AddTicks(-1) : null;

            return (solInicio, solCorte, fechaInicio, fechaCorte);
        }

        private static string SerializarResultadoReporte(IActionResult action)
        {
            if (action is not ObjectResult obj)
                return JsonConvert.SerializeObject(action, Formatting.Indented);

            var jres = System.Text.Json.JsonSerializer.Serialize(obj.Value);
            var err = System.Text.Json.JsonSerializer.Deserialize<ErrorDto>(jres);
            return JsonConvert.SerializeObject(err, Formatting.Indented);
        }

        private static IEnumerable<string> ExecSP3Lineas(SqlConnection conn, string sp, object parametrosBase)
        {
            // Evita duplicación de l1/l2/l3 y también evita el mega anonymous object repetido.
            for (int numLinea = 1; numLinea <= 3; numLinea++)
            {
                var linea = conn.QueryFirstOrDefault<string>(sp, new { numLinea, parametrosBase });
                if (!string.IsNullOrWhiteSpace(linea))
                    yield return linea;
            }
        }

        #endregion

        #region ===== Modelos para bajar params (y duplicidad) =====

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
                    .Select(row => new DropDownListaGenericaModel
                    {
                        item = row.IDX,
                        descripcion = row.ItmX
                    })
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

                // Sonar: evitar TOP interpolado
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

                var result = conn.Query<TesSolicitudesGenData>(
                        query,
                        new
                        {
                            top = filtro.cantidad,
                            tipoDoc = filtro.tipoDoc,
                            banco = filtro.banco,
                            minimo = solInicio,
                            maximo = solCorte,
                            fechaInicio,
                            fechaCorte
                        })
                    .ToList();

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
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
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
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        public ErrorDto TES_EmisionDocumento_RevisaCuentas_SP(int CodEmpresa, int banco)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                conn.Execute("exec spTes_Cuentas_Revisa @banco", new { banco });
                return DbHelper.OkResponse("Cuentas verificadas correctamente!");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
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
            try
            {
                var listaSolicitudes = JsonConvert.DeserializeObject<List<int>>(Solicitudes) ?? new List<int>();
                using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                const string query = @"exec spTes_Traslados_Cuenta_Puente @solicitud, @banco, @usuario";

                foreach (var solicitud in listaSolicitudes)
                    connection.Execute(query, new { solicitud, banco = Banco, usuario = Usuario });

                return DbHelper.OkResponse("Solicitudes movidas correctamente");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
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
                    return Err("No existe configuración en tes_banco_docs para el banco/tipoDoc indicado.");

                var bancoData = LoadBancoData(conn, filtro);
                if (bancoData == null)
                    return Err("No existe configuración en Tes_Bancos para el banco indicado.");

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
                    _ => Err($"Comprobante '{bancoDocs.comprobante}' no soportado.")
                };
            }
            catch (Exception ex)
            {
                return Err(ex.Message);
            }
        }

        private ErrorDto<object> ProcesarChequesYBoletas(EmisionContext ctx)
        {
            if (ctx.ChequesReport == null)
                return Err("No se pudo cargar archivos especiales del banco.");

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

            return OkJson(new
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
            });
        }

        private ErrorDto<object> ProcesarTransferencias(EmisionContext ctx)
        {
            // Mantengo tu flujo, pero evitando duplicación de cargas de transacciones.
            List<TesTransaccionDto> Trans() => ctx.Conn.Query<TesTransaccionDto>(ctx.Q.QueryTransac, ctx.Q.Parametros).ToList();

            return ctx.Filtro.formatoTE switch
            {
                "A" => ProcesarTE_BNCR_InternetBanking(ctx.CodEmpresa, ctx.Filtro, ctx.Conn, ctx.Q),
                "B" => sbTeBancoPopular(ctx.CodEmpresa, ctx.Filtro, Trans()),
                "C" => ProcesarTE_BCR_Planilla(ctx.CodEmpresa, ctx.Filtro, ctx.Conn, ctx.Q),
                "D" => sbTeBCR_Empresarial(ctx.CodEmpresa, ctx.Filtro),
                "E" => sbTeBCT_Enlace(ctx.CodEmpresa, ctx.Filtro),
                "F" => sbTeBCR_Comercial(ctx.CodEmpresa, ctx.Filtro),
                "G" => sbTeBNCR_Sinpe(ctx.CodEmpresa, ctx.Filtro),
                "DV1" or "DV2" => sbTeFormatoEstandar(ctx.CodEmpresa, ctx.Filtro),
                "S" => Err("No se pudo realizar la operación, debido a que la opción de SINPE se encuentra en espera"),
                "SG" => sbTeBancoSinpeGeneral(ctx.CodEmpresa, ctx.Filtro, Trans()),
                _ => sbTeFormatoEstandar(ctx.CodEmpresa, ctx.Filtro)
            };
        }

        #endregion

        #region ===== Cheques / boletas =====

        private long ResolverConsecutivo(EmisionContext ctx)
        {
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
                return Err("NSolicitud inválida al procesar transacción.");

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

            return OkObj();
        }

        private ErrorDto<object> ClasificarYGenerarBoletaSiAplica(
            EmisionContext ctx,
            ClasificacionState state,
            FrmReporteGlobal reporteData,
            TesTransaccionDto item)
        {
            // Cheques fórmula continua
            if (ctx.BancoDocs.comprobante == "01")
                return ClasificarChequeFormulaContinua(ctx, state, item);

            // Boletas 02/03 (aquí solo recolectamos PDFs; el combinado se hace al final)
            if (ctx.BancoDocs.comprobante is "02" or "03")
                return GenerarBoletaRegistroPorItem(state, reporteData, item);

            return OkObj();
        }

        private static ErrorDto<object> ClasificarChequeFormulaContinua(EmisionContext ctx, ClasificacionState state, TesTransaccionDto item)
        {
            var rutaSinFirmas = ctx.ChequesReport?.chequesSinFirmas;
            if (string.IsNullOrWhiteSpace(rutaSinFirmas))
                return Err("No está configurada la ruta del reporte de cheques sin firmas.");

            if (ctx.UsaFirmas != 1)
            {
                state.ReporteCkSinFirmas = LimpiarReporte(rutaSinFirmas);
                state.ListaSinFirmas.Add(item);
                return OkObj();
            }

            var rutaConFirmas = ctx.ChequesReport?.chequesFirmas;
            if (string.IsNullOrWhiteSpace(rutaConFirmas))
                return Err("No está configurada la ruta del reporte de cheques con firmas.");

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

            return OkObj();
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
                return Err(err.Description ?? $"Error al generar boleta para solicitud {item.nsolicitud}.");
            }

            state.FileResultBoleta = action as FileContentResult;

            if (state.FileResultBoleta?.FileContents is { Length: > 0 } bytes)
            {
                state.PdfsBoleta.Add(bytes);
                return OkObj();
            }

            return Err($"Ocurrió un error al generar la boleta de la solicitud {item.nsolicitud}, contenido nulo o vacío.");
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
        {
            if (pdfsBoleta == null || pdfsBoleta.Count == 0)
                return OkObj(string.Empty);

            if (fileResultBoleta == null || string.IsNullOrWhiteSpace(fileResultBoleta.ContentType))
                return Err("No se pudo generar boleta: FileContentResult es nulo o inválido.");

            try
            {
                var combinado = MProGrXAuxiliarDB.CombinarBytesPdfSharp(pdfsBoleta.ToArray());
                if (combinado == null || combinado.Length == 0)
                    return Err("No se pudo generar boleta: el PDF combinado quedó vacío.");

                var resultFile = new FileContentResult(combinado, fileResultBoleta.ContentType)
                {
                    FileDownloadName = fileResultBoleta.FileDownloadName
                };

                // Contrato: ErrorDto<object> con Result = JSON del FileContentResult
                return OkJson(resultFile);
            }
            catch (Exception ex)
            {
                return Err(ex.Message);
            }
        }

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

        #region ===== Carga de datos / queries base (reduce duplicación) =====

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

        private static TesBancoDocsData? LoadBancoDocs(SqlConnection connection, TesEmisionDocFiltros filtro)
        {
            const string sql = @"
select doc_auto, comprobante
from tes_banco_docs
where id_banco = @banco and tipo = @tipoDoc";

            return connection.QueryFirstOrDefault<TesBancoDocsData>(sql, new { banco = filtro.banco, tipoDoc = filtro.tipoDoc });
        }

        private static TesBancoData? LoadBancoData(SqlConnection connection, TesEmisionDocFiltros filtro)
        {
            const string sql = @"
select firmas_desde, firmas_hasta, formato_transferencia, Lugar_Emision
from Tes_Bancos
where id_banco = @banco";

            return connection.QueryFirstOrDefault<TesBancoData>(sql, new { banco = filtro.banco });
        }

        private static int LoadFirmasAut(SqlConnection connection, TesEmisionDocFiltros filtro)
        {
            const string sql = @"
select isnull(count(*),0)
from TES_BANCO_FIRMASAUT
where id_Banco = @banco and usuario = @usuario";

            return connection.QueryFirstOrDefault<int>(sql, new { banco = filtro.banco, usuario = filtro.usuario });
        }

        private static QueryBuildResult BuildQueries(TesEmisionDocFiltros filtro)
        {
            var (solInicio, solCorte, fechaInicio, fechaCorte) = GetRangos(filtro);

            var queryTransac = @"
Select TOP (@top) *
From Tes_Transacciones
Where Estado = 'P' And Tipo = @tipoDoc
  And ID_Banco= @banco And Autoriza='S' and fecha_hold is null";

            var baseQuery = @"
(SELECT TOP (@top) nsolicitud
 FROM Tes_Transacciones
 WHERE Estado = 'P' AND Tipo = @tipoDoc
   AND ID_Banco = @banco AND Autoriza = 'S' AND fecha_hold IS NULL";

            if (string.Equals(filtro.generarPor, nSolicitudes, StringComparison.OrdinalIgnoreCase))
            {
                queryTransac += " And NSolicitud Between @minimo And @maximo";
                baseQuery += " And NSolicitud Between @minimo And @maximo";
            }
            else if (string.Equals(filtro.generarPor, nFechas, StringComparison.OrdinalIgnoreCase))
            {
                queryTransac += " And Fecha_Solicitud Between @fechaInicio And @fechaCorte";
                baseQuery += " And Fecha_Solicitud Between @fechaInicio And @fechaCorte";
            }

            queryTransac += " Order by Nsolicitud";
            baseQuery += " Order by Nsolicitud)";

            var parametros = new
            {
                top = filtro.cantidad,
                banco = filtro.banco,
                tipoDoc = filtro.tipoDoc,
                minimo = solInicio,
                maximo = solCorte,
                fechaInicio,
                fechaCorte
            };

            return new QueryBuildResult(queryTransac, baseQuery, parametros);
        }

        #endregion

        #region ===== Transferencias =====

        private ErrorDto<object> ProcesarTE_BNCR_InternetBanking(int codEmpresa, TesEmisionDocFiltros filtro, SqlConnection connection, QueryBuildResult q)
        {
            // Nota: baseQuery se construye del lado servidor (no input de usuario).
            // Si Sonar lo marca como hotspot, la solución definitiva es mover este SUM a un SP.
            var queryA = "select sum(monto) as PLx from Tes_Transacciones where nsolicitud in ";
            queryA += q.BaseQuery;
            var montoPL = connection.QueryFirstOrDefault<int>(queryA, q.Parametros);

            var transacciones = connection.Query<TesTransaccionDto>(q.QueryTransac, q.Parametros).ToList();
            return sbTeBancoNacional(codEmpresa, filtro, transacciones, montoPL);
        }

        private ErrorDto<object> ProcesarTE_BCR_Planilla(int codEmpresa, TesEmisionDocFiltros filtro, SqlConnection connection, QueryBuildResult q)
        {
            var transacciones = connection.Query<TesTransaccionDto>(q.QueryTransac, q.Parametros).ToList();

            var queryC = @"
select sum(dbo.fxTESBCRTestkey(cta_ahorros,monto)) as TestKeyX,
       sum(Monto) as Monto
from Tes_Transacciones
where nsolicitud in ";
            queryC += q.BaseQuery;

            var resultC = connection.QueryFirstOrDefault(queryC, q.Parametros);

            long xTestKey = 0;
            decimal totalMonto = 0;

            if (resultC != null)
            {
                xTestKey = (long?)resultC.TestKeyX ?? 0;
                totalMonto = (decimal?)resultC.Monto ?? 0m;
            }

            xTestKey = xTestKey > 2147483468 ? 2147483468 : xTestKey;
            return sbTeBCR_Planilla(codEmpresa, filtro, transacciones, xTestKey, totalMonto);
        }

        #endregion

        #region ===== Implementaciones existentes (con menos duplicación) =====

        private ErrorDto<object> sbTeBancoNacional(int CodEmpresa, TesEmisionDocFiltros filtros, List<TesTransaccionDto> transaccionesList, int? curPlanilla)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            int BancoID = filtros.banco;
            DateTime vFecha = DateTime.Now;

            decimal curMonto1 = curPlanilla ?? 0;
            string strMonto = curMonto1.ToString("0000000000.00", CultureInfo.InvariantCulture).Replace(".", "");
            string vCuentaEmpresa = "";
            string vNumCliente = "";
            decimal curMonto2 = 0;
            long curCuentas = 0;

            try
            {
                var seguridadPortal = new SeguridadPortalDb(_config);
                string Empresa_Name = "TF " + seguridadPortal.SeleccionarPgxClientePorCodEmpresa(CodEmpresa).PGX_CORE_DB;
                string vConcepto = Empresa_Name.PadRight(30, ' ');

                var bancoInfo = connection.QueryFirstOrDefault("select Cta,codigo_Cliente from tes_Bancos Where id_Banco = @banco",
                    new { banco = BancoID });

                if (bancoInfo != null)
                {
                    vCuentaEmpresa = (bancoInfo.Cta ?? "").ToString().Trim().Replace("-", "");
                    vNumCliente = (bancoInfo.codigo_Cliente ?? "").ToString().PadLeft(6, '0');
                }

                string BancoTDoc = filtros.tipoDoc;
                long BancoConsec = mTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+").Result;

                var sb = new StringBuilder();

                var header = new StringBuilder(120);
                header.Append('1');
                header.Append(vNumCliente);
                header.Append(vFecha.Day.ToString("00", CultureInfo.InvariantCulture));
                header.Append(vFecha.Month.ToString("00", CultureInfo.InvariantCulture));
                header.Append(vFecha.Year.ToString("0000", CultureInfo.InvariantCulture));
                header.Append(BancoID.ToString("D12", CultureInfo.InvariantCulture));
                header.Append("10000");
                header.Append(strMonto);
                header.Append("000000000000000000000000");
                sb.AppendLine(header.ToString());

                int i = 0;

                foreach (var item in transaccionesList)
                {
                    i++;

                    string cuenta = (item.cta_ahorros ?? "").Replace("-", "").Trim();
                    if (cuenta.Length < 12)
                        return Err($"Cuenta inválida en solicitud {item.nsolicitud}.");

                    var linea = new StringBuilder(120);
                    linea.Append('3');
                    linea.Append(cuenta.Substring(5, 3));
                    linea.Append(cuenta.Substring(0, 3));
                    linea.Append("01");
                    linea.Append(cuenta.Substring(cuenta.Length - 7));
                    linea.Append(i.ToString("D8", CultureInfo.InvariantCulture));

                    decimal monto = item.monto ?? 0m;
                    string strMontoDet = monto.ToString("0000000000.00", CultureInfo.InvariantCulture).Replace(".", "");
                    linea.Append(strMontoDet);
                    linea.Append(vConcepto);
                    linea.Append("00");

                    sb.AppendLine(linea.ToString());
                }

                if (string.IsNullOrWhiteSpace(vCuentaEmpresa) || vCuentaEmpresa.Length < 8)
                    return Err("Cuenta empresa inválida o no configurada.");

                var last = new StringBuilder(120);
                last.Append('2');
                last.Append(vCuentaEmpresa.Substring(0, 3));
                last.Append("10001");
                last.Append(vCuentaEmpresa.Substring(vCuentaEmpresa.Length - 7));
                last.Append((i + 1).ToString("D8", CultureInfo.InvariantCulture));

                string strMontoEmpresa = curMonto2.ToString("0000000000.00", CultureInfo.InvariantCulture).Replace(".", "");
                last.Append(strMontoEmpresa);
                last.Append(vConcepto);
                last.Append("00");
                sb.AppendLine(last.ToString());

                curCuentas += long.Parse(vCuentaEmpresa.Substring(vCuentaEmpresa.Length - 7, 6), CultureInfo.InvariantCulture);

                var linea4 = new StringBuilder(200);
                linea4.Append('4');
                decimal montoControl = curMonto1 + curMonto2;
                string strMontoControl = montoControl.ToString("0000000000000.00", CultureInfo.InvariantCulture).Replace(".", "");
                linea4.Append(strMontoControl);
                linea4.Append(curCuentas.ToString("D10", CultureInfo.InvariantCulture));
                linea4.Append("0000000000");
                linea4.Append(zero12Append);
                linea4.Append(zero12Append);
                linea4.Append("00000000");
                sb.AppendLine(linea4.ToString());

                return ArchivoResponse(BancoConsec, "ENV", sb);
            }
            catch (Exception ex)
            {
                return Err(ex.Message);
            }
        }

        private ErrorDto<object> sbTeBancoPopular(int CodEmpresa, TesEmisionDocFiltros filtros, List<TesTransaccionDto> transaccionesList)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            DateTime vFecha = DateTime.Now;

            try
            {
                int BancoID = filtros.banco;
                string BancoTDoc = filtros.tipoDoc;
                long BancoConsec = mTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+").Result;

                var sb = new StringBuilder();

                foreach (var item in transaccionesList)
                {
                    string codigoTrim = item.codigo?.Trim() ?? string.Empty;

                    string codigo10 = codigoTrim.Length switch
                    {
                        8 => "0" + codigoTrim.Substring(0, 1) + "0" + codigoTrim.Substring(1, 7),
                        9 => "0" + codigoTrim,
                        < 8 => Convert.ToInt64(string.IsNullOrWhiteSpace(codigoTrim) ? "0" : codigoTrim, CultureInfo.InvariantCulture)
                                    .ToString("D10", CultureInfo.InvariantCulture),
                        > 10 => codigoTrim.Substring(0, 4) + "0" + codigoTrim.Substring(5, 5),
                        _ => codigoTrim.PadLeft(10, '0').Substring(0, 10)
                    };

                    string nombre = (item.beneficiario ?? string.Empty).Trim();
                    nombre = nombre.Length > 30 ? nombre.Substring(0, 30) : nombre.PadRight(30, ' ');

                    string cuenta = (item.cta_ahorros ?? "0").Trim();
                    cuenta = cuenta.Length > 13 ? cuenta.Substring(0, 13) : cuenta.PadLeft(13, '0');

                    decimal monto = item.monto ?? 0m;
                    string strMonto = monto.ToString("000000000.00", CultureInfo.InvariantCulture).Replace(".", "");

                    string strFecha = vFecha.ToString(fechaFormat2, CultureInfo.InvariantCulture);

                    var line = new StringBuilder(120);
                    line.Append(codigo10);
                    line.Append(nombre);
                    line.Append(cuenta);
                    line.Append(' ');
                    line.Append(strMonto);
                    line.Append(strFecha);
                    line.Append('A');
                    line.Append("06");
                    line.Append('P');
                    line.Append(strFecha);
                    line.Append(strMonto);

                    sb.AppendLine(line.ToString());
                }

                return ArchivoResponse(BancoConsec, "txt", sb);
            }
            catch (Exception ex)
            {
                return Err(ex.Message);
            }
        }

        private ErrorDto<object> sbTeFormatoEstandar(int CodEmpresa, TesEmisionDocFiltros filtros)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            string pFormato = filtros.formatoTE ?? string.Empty;
            int BancoID = filtros.banco;

            try
            {
                var empresaData = connection.QueryFirstOrDefault("select REPLACE(cedula_juridica,'-','') as cedula_Juridica from SIF_EMPRESA");
                string vNumNegocio = empresaData?.cedula_Juridica?.ToString() ?? string.Empty;

                var formatoData = connection.QueryFirstOrDefault(
                    "select Procedimiento,Extension from vTes_Formatos where cod_formato = @formato",
                    new { formato = pFormato });

                string vExtension = formatoData?.Extension?.ToString() ?? "txt";
                string vProcedimiento = formatoData?.Procedimiento?.ToString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(vProcedimiento))
                    return Err("Formato no configurado en vTes_Formatos.");

                string BancoTDoc = filtros.tipoDoc;
                string BancoPlan = filtros.plan;
                long BancoConsec = mTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+", BancoPlan).Result;

                var (solInicio, solCorte, fechaInicio, fechaCorte) = GetRangos(filtros);

                var sb = new StringBuilder();

                for (int numLinea = 1; numLinea <= 3; numLinea++)
                {
                    // SP viene de configuración DB (no input usuario).
                    var queryLinea = new StringBuilder();
                    queryLinea.Append("EXEC ");
                    queryLinea.Append(vProcedimiento);
                    queryLinea.Append(" @numLinea, @bancoID, @bancoTDoc, @numNegocio, @bancoConsec, @cantidadSolicitudes, @mSolInicio, @mSolCorte, @mFechaInicio, @mFechaCorte");

                    if (!string.Equals(BancoPlan, "-sp-", StringComparison.OrdinalIgnoreCase))
                        queryLinea.Append(", @bancoPlan");

                    var parametros = new
                    {
                        numLinea,
                        bancoID = BancoID,
                        bancoTDoc = BancoTDoc,
                        numNegocio = vNumNegocio,
                        bancoConsec = BancoConsec,
                        cantidadSolicitudes = filtros.cantidad,
                        mSolInicio = solInicio,
                        mSolCorte = solCorte,
                        mFechaInicio = fechaInicio?.ToString(fechaFormat, CultureInfo.InvariantCulture),
                        mFechaCorte = fechaCorte?.ToString(fechaFormat, CultureInfo.InvariantCulture),
                        bancoPlan = BancoPlan
                    };

                    var lineas = connection.Query<string>(queryLinea.ToString(), parametros);
                    foreach (var linea in lineas)
                        AppendIfNotEmpty(sb, linea);
                }

                return ArchivoResponse(BancoConsec, vExtension, sb);
            }
            catch (Exception ex)
            {
                return Err(ex.Message);
            }
        }

        private ErrorDto<object> sbTeBCR_Planilla(int CodEmpresa, TesEmisionDocFiltros filtros, List<TesTransaccionDto> transaccionesList, long vTestKey, decimal vMontoTotal)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            DateTime vFecha = DateTime.Now;

            try
            {
                string vRazon = GetParametro(CodEmpresa, "BCRFormat3").PadRight(30, ' ');
                string vNumNegocio = GetParametro(CodEmpresa, "BCRFormat1");
                string vCedulaReg = GetParametro(CodEmpresa, "BCRFormat2");

                int BancoID = filtros.banco;
                string BancoTDoc = filtros.tipoDoc;

                int i = GetConsecutivoArchivoDelDia(connection, BancoID, vFecha);
                string vConArchivo = i.ToString("D3", CultureInfo.InvariantCulture);

                var vCuentaBancoStr = connection.QueryFirstOrDefault<string>(
                    "select Cta from Tes_Bancos where id_Banco = @banco",
                    new { banco = filtros.banco }) ?? "0";

                if (!int.TryParse(vCuentaBancoStr, out var cuentaN))
                    cuentaN = 0;

                string vCuentaBanco = "001" + cuentaN.ToString("D8", CultureInfo.InvariantCulture);

                const string qTest = @"select dbo.fxTESBCRTestkey(@cuentaBanco, @montoTotal) as TestKey";
                int xTestKey = connection.QueryFirstOrDefault<int>(qTest, new { cuentaBanco = vCuentaBanco, montoTotal = vMontoTotal });

                vTestKey = Math.Min(vTestKey + xTestKey, 2147483468);

                var vTesKeyCh = vTestKey.ToString(CultureInfo.InvariantCulture).Trim();
                if (vTesKeyCh.Length > 12)
                    vTestKey = long.Parse(vTesKeyCh[^12..], CultureInfo.InvariantCulture);

                long BancoConsec = mTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+").Result;

                var sb = new StringBuilder();

                var header = new StringBuilder(220);
                header.Append("000");
                header.Append(vNumNegocio);
                header.Append(vConArchivo);
                header.Append(zero6Append);
                header.Append(vCedulaReg);
                header.Append(Convert.ToInt64(vTestKey).ToString("D12", CultureInfo.InvariantCulture));
                header.Append(zero6Append);
                header.Append(vFecha.Day.ToString("D2", CultureInfo.InvariantCulture));
                header.Append(vFecha.Month.ToString("D2", CultureInfo.InvariantCulture));
                header.Append(vFecha.Year.ToString("D4", CultureInfo.InvariantCulture));
                header.Append(new string(' ', 21));
                header.Append('Y');
                sb.AppendLine(header.ToString());

                int lineaIndex = 1;

                var debito = new StringBuilder(220);
                debito.Append("000");
                debito.Append('1');
                debito.Append("00000");
                debito.Append(vCuentaBanco.Trim().PadRight(11).Substring(0, 11));
                debito.Append('1');
                debito.Append('4');
                debito.Append("0000");
                debito.Append(BancoConsec.ToString("D4", CultureInfo.InvariantCulture));
                debito.Append(lineaIndex.ToString("D4", CultureInfo.InvariantCulture));
                debito.Append(((long)(vMontoTotal * 100)).ToString("D12", CultureInfo.InvariantCulture));
                debito.Append(vFecha.ToString(fechaFormat2, CultureInfo.InvariantCulture));
                debito.Append('0');
                debito.Append(vRazon);
                sb.AppendLine(debito.ToString());

                foreach (var item in transaccionesList)
                {
                    lineaIndex++;

                    string cuenta = (item.cta_ahorros ?? string.Empty).PadRight(11).Substring(0, 11).Trim();
                    long montoCents = (long)Math.Round(((item.monto ?? 0m) * 100m), 0, MidpointRounding.AwayFromZero);

                    var credito = new StringBuilder(220);
                    credito.Append("000");
                    credito.Append('2');
                    credito.Append("00000");
                    credito.Append(cuenta);
                    credito.Append('1');
                    credito.Append('2');
                    credito.Append("0000");
                    credito.Append(BancoConsec.ToString("D4", CultureInfo.InvariantCulture));
                    credito.Append(lineaIndex.ToString("D4", CultureInfo.InvariantCulture));
                    credito.Append(montoCents.ToString("D12", CultureInfo.InvariantCulture));
                    credito.Append(vFecha.ToString(fechaFormat2, CultureInfo.InvariantCulture));
                    credito.Append('0');
                    credito.Append(vRazon);

                    sb.AppendLine(credito.ToString());
                }

                return ArchivoResponse(BancoConsec, "BCR", sb);
            }
            catch (Exception ex)
            {
                return Err(ex.Message);
            }
        }

        // Refactor anti-duplicidad (Empresarial y Comercial son casi idénticos)
        private ErrorDto<object> EmitirBcr2y3(
    int codEmpresa,
    TesEmisionDocFiltros filtros,
    string spName,
    Func<string, string, DateTime, string> buildControl,
    string extension)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, codEmpresa);

            try
            {
                var (solInicio, solCorte, fechaInicio, fechaCorte) = GetRangos(filtros);
                var (vNumNegocio, vCedulaReg) = GetEmpresaNumNegocioYReg(connection);

                int bancoId = filtros.banco;
                string bancoTDoc = filtros.tipoDoc;

                long bancoConsec = mTesoreria
                    .fxTesTipoDocConsec(codEmpresa, bancoId, bancoTDoc, "+")
                    .Result;

                DateTime fecha = DateTime.Now;

                int i = GetConsecutivoArchivoDelDia(connection, bancoId, fecha);
                string conArchivo = i.ToString("D3", CultureInfo.InvariantCulture);

                var sb = new StringBuilder();
                sb.AppendLine(buildControl(vCedulaReg, conArchivo, fecha));

                // Whitelist para evitar SQL dynamic/formatting (Sonar S2077)
                const string spEmpresarial = "spTES_BCR_Empresarial";
                const string spComercial = "spTES_BCR_Comercial";

                // Selección permitida (solo estos dos)
                var sp = spName switch
                {
                    spEmpresarial => spEmpresarial,
                    spComercial => spComercial,
                    _ => null
                };

                if (sp is null)
                    return Err($"Stored procedure no permitido: {spName}");

                // IMPORTANTE: no hay string.Format, no hay interpolación.
                // Se ejecuta por nombre de SP + CommandType.StoredProcedure.
                var parametros = new
                {
                    banco = bancoId,
                    bancoTDoc,
                    numNegocio = vNumNegocio,
                    bancoConsec,
                    cantidad = filtros.cantidad,
                    solInicio,
                    solCorte,
                    fechaInicio,
                    fechaCorte
                };

                // Línea 2 y 3 sin duplicar parámetros (solo cambia numLinea)
                AppendIfNotEmpty(sb, EjecutarLineaBcr(connection, sp, 2, parametros));
                AppendIfNotEmpty(sb, EjecutarLineaBcr(connection, sp, 3, parametros));

                return ArchivoResponse(bancoConsec, extension, sb);
            }
            catch (Exception ex)
            {
                return Err(ex.Message);
            }
        }

        private static string? EjecutarLineaBcr(SqlConnection connection, string sp, int numLinea, object parametrosBase)
        {
            // Un solo lugar donde se define el “shape” del SP.
            // Dapper: CommandType.StoredProcedure evita “EXEC ...” en texto.
            return connection.QueryFirstOrDefault<string>(
                sp,
                new { numLinea, parametrosBase },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        private ErrorDto<object> sbTeBCR_Empresarial(int CodEmpresa, TesEmisionDocFiltros filtros)
        {
            static string Control(string cedulaReg, string conArchivo, DateTime fecha)
            {
                var control = new StringBuilder(200);
                control.Append("000");
                control.Append((cedulaReg ?? string.Empty).Trim().PadLeft(12, '0'));
                control.Append(conArchivo);
                control.Append(fecha.ToString(fechaFormat2, CultureInfo.InvariantCulture));
                control.Append(zero12Append);
                control.Append(zero12Append);
                control.Append(zero6Append);
                control.Append(new string(' ', 6));
                control.Append("TLB");
                control.Append(new string(' ', 128));
                control.Append('D');
                return control.ToString();
            }

            return EmitirBcr2y3(CodEmpresa, filtros, "spTES_BCR_Empresarial", Control, "txt");
        }

        private ErrorDto<object> sbTeBCR_Comercial(int CodEmpresa, TesEmisionDocFiltros filtros)
        {
            static string Control(string cedulaReg, string conArchivo, DateTime fecha)
            {
                var control = new StringBuilder(200);
                control.Append("000");
                control.Append((cedulaReg ?? string.Empty).Trim().PadLeft(12, '0'));
                control.Append(conArchivo);
                control.Append(fecha.ToString(fechaFormat2, CultureInfo.InvariantCulture));
                control.Append(zero12Append);
                control.Append(zero12Append);
                control.Append(zero6Append);
                control.Append(new string('0', 138));
                return control.ToString();
            }

            return EmitirBcr2y3(CodEmpresa, filtros, "spTES_BCR_Comercial", Control, "txt");
        }

        private ErrorDto<object> sbTeBCT_Enlace(int CodEmpresa, TesEmisionDocFiltros filtros)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var (solInicio, solCorte, fechaInicio, fechaCorte) = GetRangos(filtros);

                int BancoID = filtros.banco;
                string BancoTDoc = filtros.tipoDoc;
                long BancoConsec = mTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+").Result;

                var sb = new StringBuilder();

                const string query = @"exec spTES_BCT_Enlace 
@banco, @bancoTDoc, @bancoConsec, @cantidad, @solInicio, @solCorte, @fechaInicio, @fechaCorte";

                var resultado = connection.QueryFirstOrDefault(query, new
                {
                    banco = BancoID,
                    bancoTDoc = BancoTDoc,
                    bancoConsec = BancoConsec,
                    cantidad = filtros.cantidad,
                    solInicio,
                    solCorte,
                    fechaInicio,
                    fechaCorte
                });

                string linea = resultado?.Linea?.ToString() ?? string.Empty;
                AppendIfNotEmpty(sb, linea);

                return ArchivoResponse(BancoConsec, "txt", sb);
            }
            catch (Exception ex)
            {
                return Err(ex.Message);
            }
        }

        private ErrorDto<object> sbTeBNCR_Sinpe(int CodEmpresa, TesEmisionDocFiltros filtros)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var (solInicio, solCorte, fechaInicio, fechaCorte) = GetRangos(filtros);

                int BancoID = filtros.banco;
                string BancoTDoc = filtros.tipoDoc;
                long BancoConsec = mTesoreria.fxTesTipoDocConsec(CodEmpresa, BancoID, BancoTDoc, "+").Result;

                var sb = new StringBuilder();

                const string sp = @"exec spTES_BNCR_SINPE 
@numLinea, @banco, @bancoTDoc, @bancoConsec, @cantidad, @solInicio, @solCorte, @fechaInicio, @fechaCorte";

                var parametrosBase = new
                {
                    banco = BancoID,
                    bancoTDoc = BancoTDoc,
                    bancoConsec = BancoConsec,
                    cantidad = filtros.cantidad,
                    solInicio,
                    solCorte,
                    fechaInicio,
                    fechaCorte
                };

                foreach (var linea in ExecSP3Lineas(connection, sp, parametrosBase))
                    AppendIfNotEmpty(sb, linea);

                return ArchivoResponse(BancoConsec, "tef", sb);
            }
            catch (Exception ex)
            {
                return Err(ex.Message);
            }
        }

        #endregion

        #region ===== SINPE General (ya “limpio” para Sonar) =====

        private ErrorDto<object> sbTeBancoSinpeGeneral(int codEmpresa, TesEmisionDocFiltros filtro, List<TesTransaccionDto> transaccionesList)
        {
            if (!string.Equals(filtro.tipoDoc, "TS", StringComparison.OrdinalIgnoreCase))
                return OkJson(new { results = Array.Empty<ErrorDto>() });

            if (transaccionesList == null || transaccionesList.Count == 0)
                return OkJson(new { results = Array.Empty<ErrorDto>() });

            if (string.IsNullOrWhiteSpace(filtro.usuario))
                return Err("Usuario requerido para procesar SINPE.");

            try
            {
                var servicio = _factory.CrearServicio(codEmpresa, filtro.usuario);
                var results = new List<ErrorDto>(capacity: transaccionesList.Count);

                foreach (var trx in transaccionesList)
                {
                    results.Add(EmitirSinpe(servicio, codEmpresa, filtro.usuario, trx));
                }

                return OkJson(new { results });
            }
            catch (Exception ex)
            {
                return Err(ex.Message);
            }
        }



        private ErrorDto EmitirSinpe(IWfcSinpe servicio, int codEmpresa, string usuario, TesTransaccionDto trx)
        {
            var now = DateTime.Now;

            if (codEmpresa == 61)
            {
                switch (trx.tipo_girosinpe)
                {
                    case "CD":
                       return servicio.fxTesEmisionSinpeCreditoDirecto(codEmpresa, trx.nsolicitud, now, usuario, 0, 0);
                    case "TR":
                        return servicio.fxTesEmisionSinpeTiempoReal(codEmpresa, trx.nsolicitud, now, usuario, 0, 0);
                    default:
                        break;
                }
            }
            else
            {
                switch (trx.tipo_girosinpe)
                {
                    case "CD":
                        return servicio.fxTesEmisionSinpeCreditoDirecto(codEmpresa, trx.nsolicitud, now, usuario, 0, 0);
                       
                    case "TR":
                        return servicio.fxTesEmisionSinpeCreditoDirecto(codEmpresa, trx.nsolicitud, now, usuario, 0, 0);
                    default:
                        break;
                }
            }

            return new ErrorDto
            {
                Code = -1,
                Description = "Emision No Valida."
            };
        }

        #endregion
    }
}
