using Galileo.Models.TES;
using Galileo_API.DataBaseTier.ProGrX.Bancos.frmTES_EmisionDocumentos;
using Newtonsoft.Json;

namespace Galileo_API.Services.ProGrX.Bancos
{
    public sealed class TesEmisionDocumentosProcesoWorker : BackgroundService
    {
        private const int TamanoGrupo = 20;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TesEmisionDocumentosProcesoWorker> _logger;
        private readonly TesEmisionDocumentosProcesoQueue _queue;

        public TesEmisionDocumentosProcesoWorker(
            IConfiguration configuration,
            ILogger<TesEmisionDocumentosProcesoWorker> logger,
            TesEmisionDocumentosProcesoQueue queue)
        {
            _configuration = configuration;
            _logger = logger;
            _queue = queue;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var trabajo = await _queue.LeerAsync(stoppingToken);
                    await TES_EmisionDocumentos_Proceso_ProcesarAsync(
                        trabajo,
                        stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        private async Task TES_EmisionDocumentos_Proceso_ProcesarAsync(
            TesEmisionDocumentosProcesoTrabajo trabajo,
            CancellationToken stoppingToken)
        {
            var db = new FrmTesEmisionDocumentosDb(_configuration);
            try
            {
                var contexto =
                    db.TES_EmisionDocumentos_Sinpe_Proceso_Trabajo_Obtener(
                        trabajo.CodEmpresa,
                        trabajo.ProcesoId);
                if (contexto == null)
                    return;

                db.TES_EmisionDocumentos_Sinpe_Proceso_Recuperar(
                    trabajo.CodEmpresa,
                    trabajo.ProcesoId);
                TES_EmisionDocumentos_Proceso_Confirmaciones_Recuperar(
                    db,
                    trabajo);

                TesEmisionGenerarLoteResult? ultimoResultado = null;
                while (!stoppingToken.IsCancellationRequested)
                {
                    var solicitudes =
                        db.TES_EmisionDocumentos_Sinpe_Proceso_Detalle_Tomar(
                            trabajo.CodEmpresa,
                            trabajo.ProcesoId,
                            TamanoGrupo);
                    if (solicitudes.Count == 0)
                        break;

                    ultimoResultado =
                        await TES_EmisionDocumentos_Proceso_Grupo_ProcesarAsync(
                            db,
                            contexto,
                            solicitudes);
                }

                db.TES_EmisionDocumentos_Sinpe_Proceso_Finalizar(
                    trabajo.CodEmpresa,
                    trabajo.ProcesoId,
                    JsonConvert.SerializeObject(new
                    {
                        strQuery = ultimoResultado?.StrQuery
                    }));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Falló la emisión TS {ProcesoId} de la empresa {Empresa}.",
                    trabajo.ProcesoId,
                    trabajo.CodEmpresa);
                db.TES_EmisionDocumentos_Sinpe_Proceso_Error_Finalizar(
                    trabajo.CodEmpresa,
                    trabajo.ProcesoId,
                    "La emisión TS no pudo completarse.");
            }
            finally
            {
                _queue.Liberar(trabajo.ProcesoId);
            }
        }

        private static void
            TES_EmisionDocumentos_Proceso_Confirmaciones_Recuperar(
                FrmTesEmisionDocumentosDb db,
                TesEmisionDocumentosProcesoTrabajo trabajo)
        {
            var confirmaciones =
                db.TES_EmisionDocumentos_Sinpe_Proceso_Confirmaciones_Obtener(
                    trabajo.CodEmpresa,
                    trabajo.ProcesoId);
            foreach (var solicitud in confirmaciones)
            {
                db.TES_EmisionDocumentos_Sinpe_Proceso_Resultado_Registrar(
                    trabajo.CodEmpresa,
                    trabajo.ProcesoId,
                    solicitud,
                    0,
                    "Resultado SINPE recuperado.");
            }
        }

        private static async Task<TesEmisionGenerarLoteResult>
            TES_EmisionDocumentos_Proceso_Grupo_ProcesarAsync(
                FrmTesEmisionDocumentosDb db,
                TesEmisionDocumentosProcesoTrabajoContexto contexto,
                IReadOnlyList<int> solicitudes)
        {
            var filtro = new TesEmisionDocFiltros
            {
                cantidad = solicitudes.Count,
                banco = contexto.Banco,
                plan = contexto.Cod_Plan,
                generarPor = "solicitudes",
                tipoDoc = "TS",
                minimo = solicitudes.Min(),
                maximo = solicitudes.Max(),
                verificacion = solicitudes.Count,
                usuario = contexto.Usuario
            };
            var response =
                await db.TES_EmisionDocumentos_Sinpe_GenerarLoteAsync(
                    new TesEmisionGenerarLoteRequest
                    {
                        CodEmpresa = contexto.CodEmpresa,
                        Usuario = contexto.Usuario,
                        Filtros = JsonConvert.SerializeObject(filtro),
                        Minimo = filtro.minimo,
                        Maximo = filtro.maximo,
                        NSolicitudes = solicitudes.ToList(),
                        BancoConsec = 0
                    });

            var errores = response.Result?.Errores
                .ToDictionary(item => item.NSolicitud)
                ?? new Dictionary<int, TesEmisionProcesoError>();
            foreach (var solicitud in solicitudes)
            {
                var error = errores.GetValueOrDefault(solicitud);
                var codigo = response.Code == 0
                    ? error?.Codigo ?? 0
                    : response.Code ?? -1;
                var descripcion = response.Code == 0
                    ? error?.Descripcion ?? "Ok"
                    : response.Description ?? "Error al procesar el lote.";
                db.TES_EmisionDocumentos_Sinpe_Proceso_Resultado_Registrar(
                    contexto.CodEmpresa,
                    contexto.ProcesoId,
                    solicitud,
                    codigo,
                    descripcion);
            }

            return response.Result ?? new TesEmisionGenerarLoteResult();
        }
    }
}
