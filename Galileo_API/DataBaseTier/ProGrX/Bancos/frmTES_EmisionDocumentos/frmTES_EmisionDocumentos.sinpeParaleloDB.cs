using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Galileo_API.Controllers.WFCSinpe;
using System.Collections.Concurrent;
using System.Globalization;
using System.Xml.Linq;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos.frmTES_EmisionDocumentos
{
    /// <summary>
    /// Resultado interno del procesamiento paralelo de solicitudes TS.
    /// </summary>
    public sealed class TesEmisionDocumentosSinpeParallelResult
    {
        public TesEmisionGenerarLoteResult Resultado { get; init; } = new();
        public IReadOnlyList<int> SolicitudesExitosas { get; init; } = [];
    }

    /// <summary>
    /// Procesa solicitudes TS con instancias SINPE independientes y un límite
    /// compartido por empresa.
    /// </summary>
    public sealed class TesEmisionDocumentosSinpeParallelProcessor
    {
        private const int ValorPredeterminado = 20;
        private static readonly ConcurrentDictionary<int, SemaphoreSlim>
            LimitesPorEmpresa = new();

        private readonly int _maximoParalelo;
        private readonly Func<int, string, IWfcSinpe> _servicioFactory;

        public TesEmisionDocumentosSinpeParallelProcessor(
            int maximoParalelo,
            Func<int, string, IWfcSinpe> servicioFactory)
        {
            ArgumentNullException.ThrowIfNull(servicioFactory);
            _maximoParalelo = NormalizarMaximoParalelo(maximoParalelo);
            _servicioFactory = servicioFactory;
        }

        public static int NormalizarMaximoParalelo(int? configurado) =>
            configurado is >= 1 and <= 20
                ? configurado.Value
                : ValorPredeterminado;

        /// <summary>
        /// Ejecuta un lote TS conservando el orden de entrada y aislando los
        /// errores por solicitud.
        /// </summary>
        public async Task<TesEmisionDocumentosSinpeParallelResult> ProcesarAsync(
            int codEmpresa,
            string usuario,
            IReadOnlyList<TesTransaccionDto> transacciones)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(usuario);
            ArgumentNullException.ThrowIfNull(transacciones);

            var unicas = transacciones
                .GroupBy(item => item.nsolicitud)
                .Select(grupo => grupo.First())
                .ToArray();
            if (unicas.Length == 0)
                return new TesEmisionDocumentosSinpeParallelResult();

            var respuestas = new ErrorDto?[unicas.Length];
            var limiteEmpresa = LimitesPorEmpresa.GetOrAdd(
                codEmpresa,
                _ => new SemaphoreSlim(
                    _maximoParalelo,
                    _maximoParalelo));
            var siguiente = new[] { -1 };
            var cantidadTrabajadores = Math.Min(
                _maximoParalelo,
                unicas.Length);
            var tareas = Enumerable
                .Range(0, cantidadTrabajadores)
                .Select(_ => Task.Factory.StartNew(
                    () => TES_EmisionDocumentos_Sinpe_ProcesarTrabajador(
                            codEmpresa,
                            usuario,
                            unicas,
                            respuestas,
                            limiteEmpresa,
                            siguiente),
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default))
                .ToArray();

            await Task.WhenAll(tareas);

            return TES_EmisionDocumentos_Sinpe_CrearResultado(
                unicas,
                respuestas);
        }

        private void TES_EmisionDocumentos_Sinpe_ProcesarTrabajador(
            int codEmpresa,
            string usuario,
            IReadOnlyList<TesTransaccionDto> transacciones,
            ErrorDto?[] respuestas,
            SemaphoreSlim limiteEmpresa,
            int[] siguiente)
        {
            var servicio = _servicioFactory(codEmpresa, usuario);

            while (true)
            {
                var indice = Interlocked.Increment(ref siguiente[0]);
                if (indice >= transacciones.Count)
                    return;

                limiteEmpresa.Wait();
                try
                {
                    respuestas[indice] =
                        TES_EmisionDocumentos_Sinpe_Emitir(
                            servicio,
                            codEmpresa,
                            usuario,
                            transacciones[indice]);
                }
                catch (Exception ex)
                {
                    respuestas[indice] = new ErrorDto
                    {
                        Code = -1,
                        Description = ex.Message
                    };
                }
                finally
                {
                    limiteEmpresa.Release();
                }
            }
        }

        private static ErrorDto TES_EmisionDocumentos_Sinpe_Emitir(
            IWfcSinpe servicio,
            int codEmpresa,
            string usuario,
            TesTransaccionDto transaccion)
        {
            var fecha = DateTime.Now;
            return transaccion.tipo_girosinpe switch
            {
                "CD" => servicio.fxTesEmisionSinpeCreditoDirecto(
                    codEmpresa,
                    transaccion.nsolicitud,
                    fecha,
                    usuario,
                    0,
                    0),
                "TR" => servicio.fxTesEmisionSinpeTiempoReal(
                    codEmpresa,
                    transaccion.nsolicitud,
                    fecha,
                    usuario,
                    0,
                    0),
                _ => new ErrorDto
                {
                    Code = -1,
                    Description = "Emisión no válida."
                }
            };
        }

        private static TesEmisionDocumentosSinpeParallelResult
            TES_EmisionDocumentos_Sinpe_CrearResultado(
                IReadOnlyList<TesTransaccionDto> transacciones,
                IReadOnlyList<ErrorDto?> respuestas)
        {
            var exitosas = new List<int>();
            var errores = new List<TesEmisionProcesoError>();

            for (var indice = 0; indice < transacciones.Count; indice++)
            {
                var respuesta = respuestas[indice] ?? new ErrorDto
                {
                    Code = -1,
                    Description =
                        "No se obtuvo respuesta del servicio SINPE."
                };
                if (respuesta.Code == 0)
                {
                    exitosas.Add(transacciones[indice].nsolicitud);
                    continue;
                }

                errores.Add(new TesEmisionProcesoError
                {
                    NSolicitud = transacciones[indice].nsolicitud,
                    Codigo = respuesta.Code ?? -1,
                    Descripcion = respuesta.Description
                        ?? "Emisión no válida."
                });
            }

            return new TesEmisionDocumentosSinpeParallelResult
            {
                SolicitudesExitosas = exitosas,
                Resultado = new TesEmisionGenerarLoteResult
                {
                    Procesados = exitosas.Count,
                    ConErrores = errores.Count,
                    Errores = errores
                }
            };
        }
    }

    public partial class FrmTesEmisionDocumentosDb
    {
        private const string SqlTesEmisionDocumentosSinpeSeleccionar = @"
DECLARE @SolicitudesXmlData XML = CAST(@SolicitudesXml AS XML);

WITH Solicitudes AS
(
    SELECT Nodo.value('@Id', 'INT') AS NSolicitud
    FROM @SolicitudesXmlData.nodes('/Solicitudes/Solicitud') AS X(Nodo)
)
SELECT T.*
FROM Tes_Transacciones AS T
INNER JOIN Solicitudes AS S
    ON S.NSolicitud = T.NSolicitud
WHERE T.Estado = 'P'
  AND T.Tipo = @TipoDoc
  AND T.ID_Banco = @Banco
  AND T.Autoriza = 'S'
  AND T.fecha_hold IS NULL
ORDER BY T.NSolicitud;";

        private const string SqlTesEmisionDocumentosSinpeMarcarExitosas = @"
DECLARE @SolicitudesXmlData XML = CAST(@SolicitudesXml AS XML);

WITH Solicitudes AS
(
    SELECT Nodo.value('@Id', 'INT') AS NSolicitud
    FROM @SolicitudesXmlData.nodes('/Solicitudes/Solicitud') AS X(Nodo)
)
UPDATE T
SET T.Documento_Base = @BancoConsec
FROM Tes_Transacciones AS T
INNER JOIN Solicitudes AS S
    ON S.NSolicitud = T.NSolicitud
WHERE T.Estado = 'I'
  AND T.Tipo = @TipoDoc
  AND T.ID_Banco = @Banco;";

        private readonly TesEmisionDocumentosSinpeParallelProcessor
            _tesEmisionDocumentosSinpeProcessor;

        /// <summary>
        /// Emite las solicitudes TS exactas del lote con concurrencia limitada
        /// por empresa y marca únicamente las solicitudes exitosas.
        /// </summary>
        public async Task<ErrorDto<TesEmisionGenerarLoteResult>>
            TES_EmisionDocumentos_Sinpe_GenerarLoteAsync(
                TesEmisionGenerarLoteRequest request)
        {
            var error =
                TES_EmisionDocumentos_Sinpe_ValidarLote(request);
            if (error is not null)
            {
                return DbHelper.CreateErrorResponse(
                    error,
                    -2,
                    new TesEmisionGenerarLoteResult());
            }

            var filtro = ParseFiltros(request.Filtros);
            filtro.usuario = request.Usuario;
            filtro.generarPor = nSolicitudes;
            filtro.minimo = request.Minimo;
            filtro.maximo = request.Maximo;
            if (!string.Equals(
                    filtro.tipoDoc,
                    "TS",
                    StringComparison.OrdinalIgnoreCase))
            {
                return DbHelper.CreateErrorResponse(
                    "GenerarLote solo aplica a documentos SINPE (TS).",
                    -2,
                    new TesEmisionGenerarLoteResult());
            }

            try
            {
                var queryContexto = BuildQueries(filtro);
                var solicitudesXml =
                    TES_EmisionDocumentos_Sinpe_CrearSolicitudesXml(
                        request.NSolicitudes);
                using var connection =
                    DbHelper.OpenConnection(_portalDB, request.CodEmpresa);
                await connection.OpenAsync();

                var transacciones =
                    (await connection.QueryAsync<TesTransaccionDto>(
                        SqlTesEmisionDocumentosSinpeSeleccionar,
                        new
                        {
                            SolicitudesXml = solicitudesXml,
                            TipoDoc = filtro.tipoDoc,
                            Banco = filtro.banco
                        })).ToList();
                var procesado =
                    await _tesEmisionDocumentosSinpeProcessor.ProcesarAsync(
                        request.CodEmpresa,
                        request.Usuario,
                        transacciones);
                TES_EmisionDocumentos_Sinpe_AgregarNoEncontradas(
                    request.NSolicitudes,
                    transacciones,
                    procesado.Resultado);

                await TES_EmisionDocumentos_Sinpe_MarcarExitosasAsync(
                    connection,
                    request,
                    filtro,
                    procesado.SolicitudesExitosas);

                procesado.Resultado.BancoConsec =
                    request.BancoConsec.ToString(
                        CultureInfo.InvariantCulture);
                procesado.Resultado.StrQuery = new TesEmisionLoteQuery
                {
                    QueryTransac = queryContexto.QueryTransac,
                    BaseQuery = queryContexto.BaseQuery
                };

                return DbHelper.CreateOkResponse(procesado.Resultado);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new TesEmisionGenerarLoteResult());
            }
        }

        private static string?
            TES_EmisionDocumentos_Sinpe_ValidarLote(
                TesEmisionGenerarLoteRequest request)
        {
            if (request is null)
                return "La solicitud es requerida.";
            if (request.CodEmpresa <= 0)
                return "La empresa es requerida.";
            if (string.IsNullOrWhiteSpace(request.Usuario))
                return "El usuario es requerido.";
            if (string.IsNullOrWhiteSpace(request.Filtros))
                return "Los filtros son requeridos.";
            if (request.NSolicitudes is null
                || request.NSolicitudes.Count == 0)
                return "Debe seleccionar al menos una solicitud.";
            if (request.NSolicitudes.Count > 200)
                return "El lote no puede superar 200 registros.";
            if (request.NSolicitudes.Any(item => item <= 0))
                return "Las solicitudes deben ser mayores que cero.";
            if (request.BancoConsec < 0)
                return "El consecutivo bancario no puede ser negativo.";

            return null;
        }

        private static void
            TES_EmisionDocumentos_Sinpe_AgregarNoEncontradas(
                IEnumerable<int> solicitudesSolicitadas,
                IEnumerable<TesTransaccionDto> transacciones,
                TesEmisionGenerarLoteResult resultado)
        {
            var encontradas = transacciones
                .Select(item => item.nsolicitud)
                .ToHashSet();
            var noEncontradas = solicitudesSolicitadas
                .Distinct()
                .Where(item => !encontradas.Contains(item))
                .Select(item => new TesEmisionProcesoError
                {
                    NSolicitud = item,
                    Codigo = -2,
                    Descripcion =
                        "La solicitud no cumple las condiciones para emisión TS."
                });

            resultado.Errores.AddRange(noEncontradas);
            resultado.ConErrores = resultado.Errores.Count;
        }

        private static string
            TES_EmisionDocumentos_Sinpe_CrearSolicitudesXml(
                IEnumerable<int> solicitudes)
        {
            return new XElement(
                "Solicitudes",
                solicitudes
                    .Where(item => item > 0)
                    .Distinct()
                    .Select(item => new XElement(
                        "Solicitud",
                        new XAttribute("Id", item))))
                .ToString(SaveOptions.DisableFormatting);
        }

        private static async Task
            TES_EmisionDocumentos_Sinpe_MarcarExitosasAsync(
                Microsoft.Data.SqlClient.SqlConnection connection,
                TesEmisionGenerarLoteRequest request,
                TesEmisionDocFiltros filtro,
                IReadOnlyList<int> solicitudesExitosas)
        {
            if (solicitudesExitosas.Count == 0)
                return;
            if (request.BancoConsec == 0)
                return;

            var exitosasXml =
                TES_EmisionDocumentos_Sinpe_CrearSolicitudesXml(
                    solicitudesExitosas);
            await connection.ExecuteAsync(
                SqlTesEmisionDocumentosSinpeMarcarExitosas,
                new
                {
                    SolicitudesXml = exitosasXml,
                    BancoConsec = request.BancoConsec,
                    TipoDoc = filtro.tipoDoc,
                    Banco = filtro.banco
                });
        }
    }
}
