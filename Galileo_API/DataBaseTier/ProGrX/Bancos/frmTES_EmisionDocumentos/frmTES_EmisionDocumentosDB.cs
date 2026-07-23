using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo.Models.Security;
using Galileo.Models.TES;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.ReportingServices.Diagnostics.Internal;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos.frmTES_EmisionDocumentos
{
    public partial class FrmTesEmisionDocumentosDb
    {
        private readonly MTesoreria mTesoreria;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly MReportingServicesDB mReporting;
        private readonly PortalDB _portalDB;
        private readonly MTesFuncionesDb mTesFunciones;

        private const string nSolicitudes = "solicitudes";
        private const string nFechas = "fechas";

        // Sonar: regex sin timeout es hotspot
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(100);

        public FrmTesEmisionDocumentosDb(IConfiguration config)
        {
            mTesoreria = new MTesoreria(config);
            _Security_MainDB = new MSecurityMainDb(config);
            mReporting = new MReportingServicesDB(config);
            _portalDB = new PortalDB(config);
            mTesFunciones = new MTesFuncionesDb(config);
            var maximoParalelo = config.GetValue<int?>(
                "TES_EmisionDocumentos:SinpeMaximoParalelo");
            _tesEmisionDocumentosSinpeProcessor =
                new TesEmisionDocumentosSinpeParallelProcessor(
                    TesEmisionDocumentosSinpeParallelProcessor
                        .NormalizarMaximoParalelo(maximoParalelo),
                    (codEmpresa, usuario) =>
                        new VerificadorCoreFactory(config)
                            .CrearServicio(codEmpresa, usuario));
        }

        #region ===== Helpers comunes (reducción de duplicación / Sonar) =====

        private static string LimpiarReporte(string nombre)
            => Regex.Replace(nombre ?? string.Empty, @"\.(rdl|rdlc)$", "", RegexOptions.IgnoreCase, RegexTimeout);

        private static (int? solInicio, int? solCorte, DateTime? fechaInicio, DateTime? fechaCorte) GetRangos(TesEmisionDocFiltros f)
        {
            int? solInicio = string.Equals(f.generarPor, nSolicitudes, StringComparison.OrdinalIgnoreCase) ? f.minimo : 0;
            int? solCorte = string.Equals(f.generarPor, nSolicitudes, StringComparison.OrdinalIgnoreCase) ? f.maximo : 999999999;

            DateTime? fechaInicio = string.Equals(f.generarPor, nFechas, StringComparison.OrdinalIgnoreCase) ? f.fecha_inicio?.Date : null;
            DateTime? fechaCorte = string.Equals(f.generarPor, nFechas, StringComparison.OrdinalIgnoreCase)
                ? f.fecha_corte?.Date.AddDays(1).AddTicks(-1)
                : null;

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

        /// <summary>
        /// Avanza (+1) el documento inicial de la emisión SINPE una sola vez por emisión
        /// (modelo v6) y devuelve el consecutivo asignado.
        /// </summary>
        public ErrorDto<long> TES_EmisionDocumento_ConsecutivoIniciar(
            int CodEmpresa, int banco, string tipoDoc, string plan)
        {
            return mTesoreria.fxTesTipoDocConsec(CodEmpresa, banco, tipoDoc, "+", plan);
        }

        /// <summary>
        /// Revierte (-1) el documento inicial cuando la emisión falla (modelo v6, rollback).
        /// </summary>
        public ErrorDto<long> TES_EmisionDocumento_ConsecutivoRevertir(
            int CodEmpresa, int banco, string tipoDoc, string plan)
        {
            return mTesoreria.fxTesTipoDocConsec(CodEmpresa, banco, tipoDoc, "-", plan);
        }

        public ErrorDto<List<TesSolicitudesGenData>> TES_EmisionDocumento_Solicitudes_Obtener(int CodEmpresa, string filtros)
        {
            var filtro = ParseFiltros(filtros);
            NormalizarFiltroFechas(filtro);
            var (solInicio, solCorte, fechaInicio, fechaCorte) = GetRangos(filtro);

            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                var consecInt = mTesoreria
                    .fxTesTipoDocConsecInterno(CodEmpresa, filtro.banco, filtro.tipoDoc, "/", filtro.plan)
                    .Result;

                var usuario = filtro.usuario.ToUpperInvariant();
                var esUsuarioEspecial = TES_EmisionDocumento_UsuarioEsEspecial(conn, usuario);

                var query = TES_EmisionDocumento_Solicitudes_BuildQuery(filtro, esUsuarioEspecial);

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
                        fechaCorte,
                        usuario
                    }).ToList();

                return TES_EmisionDocumento_Solicitudes_Formatear(
                    new TesSolicitudesFormatoRequest
                    {
                        CodEmpresa = CodEmpresa,
                        Filtro = filtro,
                        Solicitudes = result,
                        ConsecutivoInterno = consecInt
                    });
            });
        }

        /// <summary>
        /// Valida si el usuario tiene solicitudes autorizadas de forma especial pendientes.
        /// </summary>
        private static bool TES_EmisionDocumento_UsuarioEsEspecial(SqlConnection conn, string usuario)
        {
            const string query = @"
select count(t.USUARIO_AUTORIZA_ESPECIAL)
from Tes_Transacciones t
where upper(t.USUARIO_AUTORIZA_ESPECIAL) = @usuario
  and t.Estado = 'P'
  and t.Autoriza = 'S'
  and t.fecha_hold is null";

            var especial = conn.QueryFirstOrDefault<int>(query, new { usuario });
            return especial > 0;
        }

        /// <summary>
        /// Formatea los documentos visibles y marca la información complementaria de las solicitudes generadas.
        /// </summary>
        private List<TesSolicitudesGenData> TES_EmisionDocumento_Solicitudes_Formatear(
            TesSolicitudesFormatoRequest request)
        {
            var now = DateTime.Now;
            var consecutivoVisible = request.Filtro.docInicial;
            var consecutivoInterno = request.ConsecutivoInterno;
            var tipoGestionCache = new FrmTesEmisionDocumentosTipoGestionCache(
                (banco, tipo) => ObtenerTipoGestionDocumento(request.CodEmpresa, banco, tipo));
            var linea = 1;
            var consecutivoInternoTS = consecutivoVisible;

            foreach (var item in request.Solicitudes)
            {
                var bancoItem = item.id_banco ?? request.Filtro.banco;
                var tipoItem = string.IsNullOrWhiteSpace(item.tipo)
                    ? request.Filtro.tipoDoc
                    : item.tipo;


                if (string.Equals(item.tipo, "TE", StringComparison.OrdinalIgnoreCase))
                {
                    item.documento =
                        $"{consecutivoVisible.ToString(CultureInfo.InvariantCulture)}-" +
                        $"{consecutivoInterno.ToString("000", CultureInfo.InvariantCulture)}";
                    consecutivoInterno++;
                }
                else if (string.Equals(item.tipo, "TS", StringComparison.OrdinalIgnoreCase))
                {
                    item.documento =
                        $"{consecutivoInternoTS.ToString(CultureInfo.InvariantCulture)}-" +
                        $"{linea.ToString("000", CultureInfo.InvariantCulture)}";
                    linea++;
                }
                else
                {
                    item.documento = consecutivoVisible.ToString(CultureInfo.InvariantCulture);
                    consecutivoVisible++;
                }

                item.fecha = now;
                item.firmas = item.firmas_autoriza_fecha == null ? "No" : "Sí";
            }

            return request.Solicitudes;
        }

        /// <summary>
        /// Construye la consulta de solicitudes a generar según el tipo de usuario y el rango seleccionado.
        /// </summary>
        private static string TES_EmisionDocumento_Solicitudes_BuildQuery(
            TesEmisionDocFiltros filtro,
            bool esUsuarioEspecial)
        {
            var query = new StringBuilder(@"
SELECT q.*,
       CAST(q.id_rechazo AS varchar(10)) + ' - ' + sm.descripcion AS estadoSinpe,
       dbo.fxTes_Cuentas_Bancarias_Pass(q.Id_Banco, q.Cta_Ahorros) AS Pass
FROM
(
    SELECT TOP (@top) t.*
    FROM Tes_Transacciones AS t
    WHERE t.Estado = 'P'
      AND t.Autoriza = 'S'
      AND t.fecha_hold IS NULL");

            query.AppendLine(esUsuarioEspecial
                ? "      AND t.USUARIO_AUTORIZA_ESPECIAL = @usuario"
                : @"      AND t.Tipo = @tipoDoc
      AND t.Id_Banco = @banco
      AND t.USUARIO_AUTORIZA_ESPECIAL IS NULL");

            if (string.Equals(filtro.generarPor, nSolicitudes, StringComparison.OrdinalIgnoreCase))
            {
                query.AppendLine("      AND t.NSolicitud BETWEEN @minimo AND @maximo");
            }
            else if (string.Equals(filtro.generarPor, nFechas, StringComparison.OrdinalIgnoreCase))
            {
                query.AppendLine("      AND t.Fecha_Solicitud BETWEEN @fechaInicio AND @fechaCorte");
            }

            query.Append(@"    ORDER BY t.NSolicitud
) AS q
LEFT JOIN SINPE_MOTIVOS AS sm
    ON sm.cod_motivo = q.id_rechazo
ORDER BY q.NSolicitud");

            return query.ToString();
        }

        public ErrorDto TES_EmisionDocumento_ValidaNumDocumento(
            int CodEmpresa,
            int banco,
            string tipoDoc,
            int docInicial,
            int cantidadList)
        {
            try
            {
                if (string.Equals(tipoDoc, "TE", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(tipoDoc, "TS", StringComparison.OrdinalIgnoreCase))
                {
                    return DbHelper.CreateOkResponse();
                }
                // CK => valida rango
                // TE => no valida nada
                var tipoGestion = ObtenerTipoGestionDocumento(CodEmpresa, banco, tipoDoc);

                if (string.Equals(tipoGestion, "TE", StringComparison.OrdinalIgnoreCase))
                {
                    return DbHelper.CreateOkResponse();
                }

                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var docFinal = docInicial + (cantidadList - 1);

                const string query = @"
                    SELECT ndocumento
                    FROM Tes_Transacciones
                    WHERE id_Banco = @banco
                      AND ndocumento BETWEEN @docInicial AND @docFinal
                      AND Tipo = @tipoDoc";

                var lista = conn.Query<int>(
                    query,
                    new { banco, docInicial, docFinal, tipoDoc }
                ).ToList();

                var docExistente = lista.FirstOrDefault(
                    nDoc => nDoc >= docInicial && nDoc <= docFinal
                );

                if (docExistente != 0)
                {
                    return DbHelper.ErrorResponse(
                        $"\nYa existe un Documento asignado [{docExistente}] dentro del rango suministrado",
                        -2
                    );
                }

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private string ObtenerTipoGestionDocumento(int codEmpresa, int banco, string tipoDoc)
        {
            var comprobante = mTesoreria
                .fxTesTipoDocExtraeDato(codEmpresa, banco, tipoDoc, "Comprobante")
                .Result?
                .Trim();

            if (string.Equals(comprobante, "4", StringComparison.OrdinalIgnoreCase))
            {
                return "TE";
            }

            return "CK";
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

        public ErrorDto<object> TES_EmisionDocumento_Generar(
            int codEmpresa,
            string filtros,
            Action<int, int>? avance = null)
        {
            try
            {
                var filtro = ParseFiltros(filtros);

                if (filtro.especial)
                {
                    var responses = new List<object>();
                    var solicitudes = TES_EmisionDocumento_Solicitudes_Obtener(codEmpresa, filtros).Result;

                    var procesadas = 0;
                    var totalSolicitudes = solicitudes?.Count ?? 0;

                    foreach (var item in solicitudes!)
                    {
                        var filtroItem = new TesEmisionDocFiltros
                        {
                            especial = filtro.especial,
                            usuario = filtro.usuario,
                            generarPor = nSolicitudes,
                            minimo = item.nsolicitud,
                            maximo = item.nsolicitud,
                            banco = item.id_banco ?? filtro.banco,
                            tipoDoc = item.tipo,
                            plan = "-sp-",
                            cantidad = filtro.cantidad,
                            fecha_inicio = null,
                            fecha_corte = null,
                            verificacion = filtro.verificacion
                        };

                        var documento = TES_EmisionDocumento_Buscar(
                            codEmpresa,
                            filtroItem.tipoDoc,
                            filtroItem.banco,
                            filtroItem.plan).Result;

                        filtroItem.cantidad = documento!.total;
                        filtroItem.docBloqueo = documento.docBloqueo;
                        filtroItem.docInicial = (int)documento.docInicial;

                        var formato = TES_EmisionDocumento_Formato_Obtener(
                            codEmpresa,
                            filtroItem.banco).Result;

                        filtroItem.formatoTE = item.tipo == "TS"
                            ? "SG"
                            : (string)formato![0].item!;

                        var proceso = ProcesoDocumentos(codEmpresa, filtroItem);
                        if (proceso.Code != 0)
                        {
                            return DbHelper.CreateErrorResponse<object>(
                                proceso.Description ?? $"Error al procesar la solicitud {item.nsolicitud}.");
                        }

                        responses.Add(new
                        {
                            result = proceso.Result
                        });
                        procesadas++;
                        avance?.Invoke(procesadas, totalSolicitudes);
                    }

                    return DbHelper.CreateOkResponse<object>(
                        JsonConvert.SerializeObject(responses, Formatting.Indented));
                }

                var resultado = ProcesoDocumentos(codEmpresa, filtro);
                if (resultado.Code == 0)
                {
                    avance?.Invoke(filtro.cantidad, filtro.cantidad);
                }
                return resultado;
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }

        private ErrorDto<object> ProcesoDocumentos(int codEmpresa, TesEmisionDocFiltros filtro)
        {
            try
            {
                using var conn = _portalDB.CreateConnection(codEmpresa);

                var bancoDocs = LoadBancoDocs(conn, filtro);
                if (bancoDocs == null)
                    return DbHelper.CreateErrorResponse<object>("No existe configuración en tes_banco_docs para el banco/tipoDoc indicado.");

                var bancoData = LoadBancoData(conn, filtro);
                if (bancoData == null)
                    return DbHelper.CreateErrorResponse<object>("No existe configuración en Tes_Bancos para el banco indicado.");

                var usaFirmas = LoadFirmasAut(conn, filtro);
                var q = BuildQueries(filtro);

                var BancoDesc = LoadBancoDesc(conn, filtro.banco);

                TesArchivosEspecialesData? chequesReport = null;
                if (bancoDocs.comprobante is "01" or "02" or "03")
                    chequesReport = mTesoreria.sbCargaArchivosEspeciales(codEmpresa, filtro.banco).Result;

                //agrego a filtros bancoDescripcion
                filtro.bancoDescripcion = BancoDesc;

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

                var result = bancoDocs.comprobante switch
                {
                    "01" or "02" or "03" => ProcesarChequesYBoletas(ctx),
                    "04" => ProcesarTransferencias(ctx),
                    _ => DbHelper.CreateErrorResponse<object>($"Comprobante '{bancoDocs.comprobante}' no soportado.")
                };

                if (result.Code != 0)
                {
                    return DbHelper.CreateErrorResponse<object>(
                        result.Description ?? "Ocurrió un error al generar la emisión."
                    );
                }

                var concatenado = new
                {
                    archivo = result,
                    strQuery = q,
                    parametros = filtro,
                    comprobante = bancoDocs.comprobante
                };

                return DbHelper.CreateOkResponse<object>(
                    JsonConvert.SerializeObject(concatenado, Formatting.Indented)
                );
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }

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

        private ErrorDto<object> ProcesarTransferencias(EmisionContext ctx)
        {
            List<TesTransaccionDto> Trans()
                => ctx.Conn.Query<TesTransaccionDto>(ctx.Q.QueryTransac, ctx.Q.Parametros).ToList();

            var solInicio = ctx.Filtro.generarPor == nSolicitudes
                ? ctx.Filtro.minimo
                : (int?)null;

            var solCorte = ctx.Filtro.generarPor == nSolicitudes
                ? ctx.Filtro.maximo
                : (int?)null;

            var fechaInicio = ctx.Filtro.generarPor == "fechas"
                ? ctx.Filtro.fecha_inicio?.Date
                : (DateTime?)null;

            var fechaCorte = ctx.Filtro.generarPor == "fechas"
                ? ctx.Filtro.fecha_corte?.Date.AddDays(1).AddTicks(-1)
                : (DateTime?)null;

            _ = ResolverBancoConsecTransferencia(ctx);

            return ctx.Filtro.formatoTE switch
            {
                "A" => ProcesarTE_BNCR_InternetBanking(
                    ctx.CodEmpresa,
                    ctx.Filtro,
                    ctx.Conn,
                    ctx.Q),

                "B" => MTesFuncionesDb.SbTeBancoPopularCore(
                    codEmpresa: ctx.CodEmpresa,
                    bancoId: ctx.Filtro.banco,
                    tipoDoc: ctx.Filtro.tipoDoc,
                    transaccionesList: Trans(),
                    resolveConsecutivo: ctx.Filtro.docInicial),

                "C" => ProcesarTE_BCR_Planilla(
                    ctx.CodEmpresa,
                    ctx.Filtro,
                    ctx.Conn,
                    ctx.Q
                    ),

                "D" => MTesFuncionesDb.SbTeBcrEmpresarialCore(
                    DbHelper.OpenConnection(_portalDB, ctx.CodEmpresa),
                    ctx.CodEmpresa,
                    new SbTeBcrParametros
                    {
                        vBanco = ctx.Filtro.banco,
                        vTipoDoc = ctx.Filtro.tipoDoc,
                        cantidadSolicitudes = ctx.Filtro.cantidad,
                        solInicio = solInicio,
                        solCorte = solCorte,
                        fechaInicio = fechaInicio,
                        fechaCorte = fechaCorte
                    },
                    ctx.Filtro.docInicial),

                "E" => sbTeBCT_Enlace(
                    ctx.CodEmpresa,
                    ctx.Filtro),

                "F" => mTesFunciones.SbTeBcrComercial(
                    DbHelper.OpenConnection(_portalDB, ctx.CodEmpresa),
                            ctx.CodEmpresa,
                            new SbTeBcrParametros
                            {
                                vBanco = ctx.Filtro.banco,
                                vTipoDoc = ctx.Filtro.tipoDoc,
                                cantidadSolicitudes = ctx.Filtro.cantidad,
                                solInicio = solInicio,
                                solCorte = solCorte,
                                fechaInicio = fechaInicio,
                                fechaCorte = fechaCorte
                            },
                            ctx.Filtro.docInicial),

                "G" => sbTeBNCR_Sinpe(
                    ctx.CodEmpresa,
                    ctx.Filtro),

                "DV1" or "DV2" => sbTeFormatoEstandar(
                    ctx.CodEmpresa,
                    ctx.Filtro),

                "S" => sbTEFormato_Interno(
                       ctx.CodEmpresa,
                       ctx.Filtro
                      ),

                "SG" => mTesFunciones.SbTesBancoSinpeGeneralCore(
                        ctx.CodEmpresa,
                        ctx.Filtro,
                        Trans(),
                       ctx.Filtro.docInicial),
                    _ => sbTeFormatoEstandar(
                       ctx.CodEmpresa,
                       ctx.Filtro
                       )
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

        private long ResolverBancoConsecTransferencia(EmisionContext ctx)
        {
            return mTesoreria
                .fxTesTipoDocConsec(
                    ctx.CodEmpresa,
                    ctx.Filtro.banco,
                    ctx.Filtro.tipoDoc,
                    "+",
                    ctx.Filtro.plan ?? string.Empty)
                .Result;
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

        private ErrorDto<object> ClasificarYGenerarBoletaSiAplica(
            EmisionContext ctx,
            ClasificacionState state,
            FrmReporteGlobal reporteData,
            TesTransaccionDto item)
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
        {
            if (pdfsBoleta == null || pdfsBoleta.Count == 0)
                return DbHelper.CreateOkResponse<object>(string.Empty);

            if (fileResultBoleta == null || string.IsNullOrWhiteSpace(fileResultBoleta.ContentType))
                return DbHelper.CreateErrorResponse<object>("No se pudo generar boleta: FileContentResult es nulo o inválido.");

            try
            {
                var combinado = MProGrXAuxiliarDB.CombinarBytesPdfSharp(pdfsBoleta.ToArray());
                if (combinado == null || combinado.Length == 0)
                    return DbHelper.CreateErrorResponse<object>("No se pudo generar boleta: el PDF combinado quedó vacío.");

                var resultFile = new FileContentResult(combinado, fileResultBoleta.ContentType)
                {
                    FileDownloadName = fileResultBoleta.FileDownloadName
                };

                return DbHelper.CreateOkResponse<object>(JsonConvert.SerializeObject(resultFile, Formatting.Indented));
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
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

        #region ===== Carga de datos / queries base =====

        private static TesEmisionDocFiltros ParseFiltros(string filtros)
            => JsonConvert.DeserializeObject<TesEmisionDocFiltros>(filtros) ?? new TesEmisionDocFiltros();

        private static void NormalizarFiltroFechas(TesEmisionDocFiltros filtro)
        {
            if (!string.Equals(filtro.usuario, nFechas, StringComparison.OrdinalIgnoreCase))
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

        private static string? LoadBancoDesc(SqlConnection connection, int bancoId)
        {
            const string sql = @"
select DESCRIPCION from tes_Bancos where ID_BANCO = @banco";

            return connection.QueryFirstOrDefault<string>(sql, new { banco = bancoId });
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

        private ErrorDto<object> ProcesarTE_BNCR_InternetBanking(
            int codEmpresa,
            TesEmisionDocFiltros filtro,
            SqlConnection connection,
            QueryBuildResult q)
        {
            var queryA = "select sum(monto) as PLx from Tes_Transacciones where nsolicitud in ";
            queryA += q.BaseQuery;
            var montoPL = connection.QueryFirstOrDefault<int>(queryA, q.Parametros);

            var transacciones = connection.Query<TesTransaccionDto>(q.QueryTransac, q.Parametros).ToList();
            return mTesFunciones.SbTeBancoNacionalCore(
                   conn: connection,
                   codEmpresa: codEmpresa,
                   bancoId: filtro.banco,
                   tipoDoc: filtro.tipoDoc,
                   transaccionesList: transacciones,
                   curPlanilla: montoPL,
                   resolveConsecutivo: filtro.docInicial
               );
        }

        private ErrorDto<object> ProcesarTE_BCR_Planilla(
                int codEmpresa,
                TesEmisionDocFiltros filtro,
                SqlConnection connection,
                QueryBuildResult q)
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

            FormatoBcrRequest request = new()
            {
                conn = connection,
                codEmpresa = codEmpresa,
                bancoId = filtro.banco,
                tipoDoc = filtro.tipoDoc,
                transaccionesList = transacciones,
                vTestKey = (int)xTestKey,
                vMontoTotal = totalMonto,
                resolveConsecutivoArchivoDelDia = (c, b, f) => MTesFuncionesDb.GetConsecutivoArchivoDelDia(connection, b, f),
                resolveBancoConsec = filtro.docInicial
            };

            return mTesFunciones.SbTeBcrCore(request);
        }

        #endregion

        #region ===== Implementaciones existentes (menos duplicación) =====

        private ErrorDto<object> sbTeFormatoEstandar(
           int CodEmpresa,
           TesEmisionDocFiltros filtros
           )
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            string pFormato = filtros.formatoTE ?? string.Empty;
            int BancoID = filtros.banco;

            try
            {
                var (vNumNegocio, _) = MTesFuncionesDb.GetEmpresaNumNegocioYReg(connection);

                var formatoData = mTesFunciones.vTesFormatos(connection, pFormato);
                if (formatoData.Code == -1)
                {
                    return DbHelper.CreateErrorResponse<object>(
                        "Error al obtener configuración del formato");
                }

                string vExtension = formatoData.Result?.Extension?.ToString() ?? "txt";
                string vProcedimiento = formatoData.Result?.Procedimiento?.ToString() ?? string.Empty;

                string BancoTDoc = filtros.tipoDoc;
                string BancoPlan = filtros.plan ?? "-sp-";

                long BancoConsec = filtros.docInicial;

                var (solInicio, solCorte, fechaInicio, fechaCorte) = GetRangos(filtros);

                var sb = new StringBuilder();

                if (!string.IsNullOrWhiteSpace(vProcedimiento) &&
                    !string.Equals(vProcedimiento.Trim(), "N/A", StringComparison.OrdinalIgnoreCase))
                {
                    for (int numLinea = 1; numLinea <= 3; numLinea++)
                    {
                        var queryLinea = new StringBuilder();
                        queryLinea.Append("EXEC ");
                        queryLinea.Append(vProcedimiento);
                        queryLinea.Append(" @numLinea, @bancoID, @bancoTDoc, @numNegocio, @bancoConsec, @cantidadSolicitudes, @mSolInicio, @mSolCorte, @mFechaInicio, @mFechaCorte");

                        if (!string.Equals(BancoPlan, "-sp-", StringComparison.OrdinalIgnoreCase))
                        {
                            queryLinea.Append(", @bancoPlan");
                        }

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
                            mFechaInicio = fechaInicio?.ToString(MTesFuncionesDb.fechaFormat, CultureInfo.InvariantCulture),
                            mFechaCorte = fechaCorte?.ToString(MTesFuncionesDb.fechaFormat, CultureInfo.InvariantCulture),
                            bancoPlan = BancoPlan,
                        };

                        var lineas = connection.Query<string>(queryLinea.ToString(), parametros);
                        foreach (var linea in lineas)
                        {
                            MTesFuncionesDb.AppendIfNotEmpty(sb, linea);
                        }
                    }
                }

                return MTesFuncionesDb.ArchivoResponse(BancoConsec -1 , vExtension, sb);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }

        private ErrorDto<object> sbTeBCT_Enlace(
            int CodEmpresa,
            TesEmisionDocFiltros filtros)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var (solInicio, solCorte, fechaInicio, fechaCorte) = GetRangos(filtros);

                int BancoID = filtros.banco;
                string BancoTDoc = filtros.tipoDoc;
                long BancoConsec = filtros.docInicial;

                var sb = new StringBuilder();

                const string query = @"exec spTES_BCT_Enlace 
@banco, @bancoTDoc, @bancoConsec, @cantidad, @solInicio, @solCorte, @fechaInicio, @fechaCorte";

                var lineas = connection.Query<dynamic>(query, new
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

                foreach (var item in lineas)
                {
                    string linea = item?.Linea?.ToString() ?? string.Empty;
                    MTesFuncionesDb.AppendIfNotEmpty(sb, linea);
                }

                return MTesFuncionesDb.ArchivoResponse(BancoConsec, "txt", sb);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }

        private ErrorDto<object> sbTeBNCR_Sinpe(
    int CodEmpresa,
    TesEmisionDocFiltros filtros)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var resp = new ErrorDto<object>
            {
                Code = 0,
                Description = ""
            };

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
                int BancoID = filtros.banco;
                string BancoTDoc = filtros.tipoDoc;
                long BancoConsec = filtros.docInicial;
                var parametros = new
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

                var sb = new StringBuilder();

                AppendLineasBncrSinpe(
                    connection,
                    sb,
                    @"exec spTES_BNCR_SINPE 1, @banco, @bancoTDoc,
                @bancoConsec, @cantidad,
                @solInicio, @solCorte, @fechaInicio, @fechaCorte",
                    parametros,
                    "Linea1");

                AppendLineasBncrSinpe(
                    connection,
                    sb,
                    @"exec spTES_BNCR_SINPE 2, @banco, @bancoTDoc,
                @bancoConsec, @cantidad,
                @solInicio, @solCorte, @fechaInicio, @fechaCorte",
                    parametros,
                    "Linea2");

                AppendLineasBncrSinpe(
                    connection,
                    sb,
                    @"exec spTES_BNCR_SINPE 3, @banco, @bancoTDoc,
                @bancoConsec, @cantidad,
                @solInicio, @solCorte, @fechaInicio, @fechaCorte",
                    parametros,
                    "Linea3");

                var archivo = new
                {
                    bancoConsec = BancoConsec.ToString(),
                    extension = "tef",
                    contenido = sb.ToString()
                };

                resp.Result = JsonConvert.SerializeObject(archivo, Formatting.Indented);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }

            return resp;
        }

        private static void AppendLineasBncrSinpe(
    SqlConnection connection,
    StringBuilder sb,
    string query,
    object parametros,
    string columna)
        {
            var lineas = connection.Query(query, parametros);

            foreach (var item in lineas)
            {
                var fila = item as IDictionary<string, object>;
                if (fila == null || !fila.TryGetValue(columna, out var valor))
                    continue;

                var linea = valor?.ToString() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(linea))
                {
                    sb.AppendLine(linea);
                }
            }
        }

        public ErrorDto<int> ValidaUsuarioEspecial(int CodEmpresa, string usuario)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            try
            {
                string query = $@"select COUNT(*) FROM Tes_Transacciones where Autoriza='S' 
                             AND Fecha_Autorizacion is not null AND USUARIO_AUTORIZA_ESPECIAL = @usuario";

                var parametros = new { usuario = usuario };

                int especial = connection.Query<int>(query, parametros).FirstOrDefault();

                return new ErrorDto<int>
                {
                    Code = 0,
                    Description = "OK",
                    Result = especial
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto<int>
                {
                    Code = -1,
                    Description = ex.Message,
                    Result = 0
                };
            }
            
        }

        public ErrorDto<object> sbTEFormato_Interno(int CodEmpresa,
           TesEmisionDocFiltros filtros)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            string pFormato = filtros.formatoTE ?? string.Empty;
            int BancoID = filtros.banco;

            try
            {
                
                var formatoData = mTesFunciones.vTesFormatos(connection, pFormato);
                if (formatoData.Code == -1)
                {
                    return DbHelper.CreateErrorResponse<object>(
                        "Error al obtener configuración del formato");
                }

                string BancoTDoc = filtros.tipoDoc;

                long BancoConsec = filtros.docInicial;

                var (solInicio, solCorte, fechaInicio, fechaCorte) = GetRangos(filtros);

                string sql = filtros.generarPor == nSolicitudes
                        ? @"
                    Select TOP (@top) *
                    From Tes_Transacciones
                    Where Estado = 'P'
                      And Tipo = @tipoDoc
                      And ID_Banco = @banco
                      And Autoriza = 'S'
                      And fecha_hold is null
                      And NSolicitud Between @minimo And @maximo
                    Order by Nsolicitud"
                        : @"
                    Select TOP (@top) *
                    From Tes_Transacciones
                    Where Estado = 'P'
                      And Tipo = @tipoDoc
                      And ID_Banco = @banco
                      And Autoriza = 'S'
                      And fecha_hold is null
                      And Fecha_Solicitud Between @fechaInicio And @fechaCorte
                    Order by Nsolicitud";

                                    List<TesTransaccionDto> transacciones =
                                    [
                                        .. connection.Query<TesTransaccionDto>(sql, new
                        {
                            top = filtros.cantidad,
                            banco = BancoID,
                            tipoDoc = BancoTDoc,
                            minimo = solInicio,
                            maximo = solCorte,
                            fechaInicio,
                            fechaCorte
                        })
                                    ];

                return SbSinpeInterno(transacciones, BancoConsec);

            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<object>(ex.Message);
            }
        }

        public static ErrorDto<object> SbSinpeInterno(List<TesTransaccionDto> transaccionesList, long bancoConsec)
        {
            var sb = new StringBuilder();

            string bancoId = transaccionesList.FirstOrDefault()?.id_banco.ToString() ?? "0000";
            string numCliente = "0000000000";
            string fecha = DateTime.Now.ToString("ddMMyyyy");
            decimal montoPlanilla = transaccionesList.Sum(t => t.monto ?? 0);

            string strMontoPlanilla = ((long)Math.Round(montoPlanilla * 100, 0))
                .ToString("D15", CultureInfo.InvariantCulture);

            // Header
            var header = new StringBuilder(120);
            header.Append('1');
            header.Append(numCliente);
            header.Append(fecha.Substring(0, 2));
            header.Append(fecha.Substring(2, 2));
            header.Append(fecha.Substring(4, 4));
            header.Append(bancoId.PadLeft(12, '0'));
            header.Append("10000");
            header.Append(strMontoPlanilla);
            header.Append("000000000000000000000000");

            sb.AppendLine(header.ToString());

            // Detalles
            foreach (var item in transaccionesList)
            {
                var detalle = new StringBuilder(200);

                decimal montoItem = item.monto ?? 0;

                detalle.Append('2');
                detalle.Append(item.nsolicitud.ToString().PadLeft(10, '0'));
                detalle.Append(bancoId.PadLeft(12, '0'));
                detalle.Append(((long)Math.Round(montoItem * 100, 0)).ToString("D15", CultureInfo.InvariantCulture));
                detalle.Append(item.beneficiario!.PadRight(50));
                detalle.Append(item.estado);
                detalle.Append(item.cta_ahorros!.PadLeft(20, '0'));
                detalle.Append(item.ndocumento!.PadLeft(15, '0'));

                sb.AppendLine(detalle.ToString());
            }

            return MTesFuncionesDb.ArchivoResponse(bancoConsec, "txt", sb);
        }

        #endregion


    }
}
