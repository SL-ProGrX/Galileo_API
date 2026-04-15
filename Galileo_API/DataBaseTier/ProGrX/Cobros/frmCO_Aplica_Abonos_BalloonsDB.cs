using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOAplicaAbonosBalloonsDB
    {
        private readonly PortalDB _portalDB;

        public FrmCOAplicaAbonosBalloonsDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la lista principal de casos para aplicación de abonos balloons.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CoAplicaAbonosBalloonsListaResult> CO_Aplica_Abonos_Balloons_Lista_Obtener(
            int CodEmpresa,
            string parametros)
        {
            FiltrosLazyLoadData filtros;

            try
            {
                filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros)
                    ?? new FiltrosLazyLoadData();
            }
            catch (JsonException jex)
            {
                return DbHelper.CreateErrorResponse(
                    jex.Message,
                    -1,
                    new CoAplicaAbonosBalloonsListaResult());
            }

            return EjecutarLista(CodEmpresa, filtros, true);
        }

        /// <summary>
        /// Exporta la lista principal sin paginación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CoAplicaAbonosBalloonsListaResult> CO_Aplica_Abonos_Balloons_Lista_Export(
            int CodEmpresa,
            string parametros)
        {
            FiltrosLazyLoadData filtros;

            try
            {
                filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros)
                    ?? new FiltrosLazyLoadData();
            }
            catch (JsonException jex)
            {
                return DbHelper.CreateErrorResponse(
                    jex.Message,
                    -1,
                    new CoAplicaAbonosBalloonsListaResult());
            }

            filtros.pagina = 0;
            filtros.paginacion = 0;

            return EjecutarLista(CodEmpresa, filtros, false);
        }

        /// <summary>
        /// Ejecuta la aplicación de fondos a 1 o muchos casos seleccionados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto<CoAplicaAbonosBalloonsAplicarResult> CO_Aplica_Abonos_Balloons_Aplicar(
            int CodEmpresa,
            CoAplicaAbonosBalloonsAplicarRequest? req)
        {
            var response = new ErrorDto<CoAplicaAbonosBalloonsAplicarResult>
            {
                Code = 0,
                Description = "Ok",
                Result = new CoAplicaAbonosBalloonsAplicarResult()
            };

            if (req == null)
            {
                return DbHelper.CreateErrorResponse(
                    "No se recibió la solicitud.",
                    -2,
                    response.Result);
            }

            string usuario = Clean(req.usuario);
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.CreateErrorResponse(
                    "El usuario es requerido.",
                    -2,
                    response.Result);
            }

            var casos = (req.casos ?? new List<CoAplicaAbonosBalloonsCasoAplicarDto>())
                .Where(x => x != null)
                .Select(x => new CoAplicaAbonosBalloonsCasoAplicarDto
                {
                    cedula = Clean(x.cedula),
                    operacion = x.operacion
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.cedula) && x.operacion > 0)
                .GroupBy(x => new { x.cedula, x.operacion })
                .Select(g => g.First())
                .ToList();

            if (casos.Count == 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Indique algún caso a procesar!",
                    -2,
                    response.Result);
            }

            try
            {
                using var conn = _portalDB.CreateConnection(CodEmpresa);

                const string sqlGuia = "exec spCBR_Creditos_Balloons_Guia @pUsuario;";

                var guia = conn.QueryFirstOrDefault<CoAplicaAbonosBalloonsGuiaDto>(
                    sqlGuia,
                    new { pUsuario = usuario },
                    commandTimeout: 0);

                int idAplicacion = guia?.Aplicacion ?? 0;
                if (idAplicacion <= 0)
                {
                    return DbHelper.CreateErrorResponse(
                        "No fue posible generar la guía de aplicación.",
                        -1,
                        response.Result);
                }

                response.Result.id_aplicacion = idAplicacion;

                const string sqlAplicar = @"
exec spCBR_Creditos_Balloons_Aplicacion
    @pNumeroOperacion,
    @vdAcuerdo,
    @pIdAplicacion,
    @vUsuario;";

                const string sqlValida = @"
select dbo.fxCBR_Creditos_Balloons_Aplicacion_Valida(
    @cedula,
    @idAplicacion
) as Resultado;";

                foreach (var caso in casos)
                {
                    var item = new CoAplicaAbonosBalloonsAplicarDetalleDto
                    {
                        cedula = caso.cedula,
                        operacion = caso.operacion,
                        ok = false,
                        mensaje = string.Empty
                    };

                    try
                    {
                        conn.Execute(
                            sqlAplicar,
                            new
                            {
                                pNumeroOperacion = caso.operacion,
                                vdAcuerdo = 0,
                                pIdAplicacion = idAplicacion,
                                vUsuario = usuario
                            },
                            commandTimeout: 0);

                        bool valido = conn.QueryFirstOrDefault<bool>(
                            sqlValida,
                            new
                            {
                                cedula = caso.cedula,
                                idAplicacion
                            },
                            commandTimeout: 0);

                        item.ok = valido;
                        item.mensaje = item.ok
                            ? "Procesado correctamente."
                            : $"El caso: {caso.cedula}, No se procesó!";

                        if (item.ok)
                        {
                            response.Result.procesados++;
                        }
                        else
                        {
                            response.Result.fallidos++;
                        }
                    }
                    catch (SqlException ex)
                    {
                        item.ok = false;
                        item.mensaje = ex.Message;
                        response.Result.fallidos++;
                    }

                    response.Result.detalle.Add(item);
                }

                response.Description = response.Result.fallidos == 0
                    ? "Proceso concluído Satisfactoriamente!"
                    : $"Proceso concluído con observaciones. Procesados: {response.Result.procesados}. Fallidos: {response.Result.fallidos}.";

                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    response.Result);
            }
        }

        /// <summary>
        /// Ejecuta la consulta base, aplica filtro global, ordenamiento y paginación en memoria.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="usarPaginacion"></param>
        /// <returns></returns>
        private ErrorDto<CoAplicaAbonosBalloonsListaResult> EjecutarLista(
            int CodEmpresa,
            FiltrosLazyLoadData filtros,
            bool usarPaginacion)
        {
            var result = new CoAplicaAbonosBalloonsListaResult
            {
                total = 0,
                lista = new List<CoAplicaAbonosBalloonsListaItemDto>()
            };

            try
            {
                using var conn = _portalDB.CreateConnection(CodEmpresa);

                var lista = conn.Query<CoAplicaAbonosBalloonsListaItemDto>(
                    sql: "exec spCBR_Creditos_Balloons_Consulta",
                    commandTimeout: 0).ToList();

                IEnumerable<CoAplicaAbonosBalloonsListaItemDto> query = lista;

                query = ApplyFiltro(query, filtros);
                query = ApplySort(query, filtros);

                result.total = query.Count();

                if (usarPaginacion)
                {
                    query = ApplyPaginacion(query, filtros);
                }

                result.lista = query.ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, result);
            }
        }

        /// <summary>
        /// Aplica filtro global en memoria sobre la lista cargada.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static IEnumerable<CoAplicaAbonosBalloonsListaItemDto> ApplyFiltro(
            IEnumerable<CoAplicaAbonosBalloonsListaItemDto> source,
            FiltrosLazyLoadData filtros)
        {
            string texto = Clean(filtros?.filtro);

            if (string.IsNullOrWhiteSpace(texto))
            {
                return source;
            }

            return source.Where(x =>
                   ContainsText(x.cedula, texto)
                || ContainsText(x.nombre, texto)
                || ContainsText(x.codigo, texto)
                || ContainsText(x.operacion.ToString(CultureInfo.InvariantCulture), texto)
                || ContainsText(x.cuota.ToString("0.##", CultureInfo.InvariantCulture), texto)
                || ContainsText(x.preanalisis, texto)
                || ContainsText(x.periodicidad, texto)
                || ContainsText(x.disponible_cuenta.ToString("0.##", CultureInfo.InvariantCulture), texto)
                || ContainsText(x.disponible_sobres.ToString("0.##", CultureInfo.InvariantCulture), texto)
                || ContainsText(x.disponible_fondos.ToString("0.##", CultureInfo.InvariantCulture), texto)
                || ContainsText(x.disponible_fondos_especial.ToString("0.##", CultureInfo.InvariantCulture), texto)
                || ContainsText(x.indicador ? "SI" : "NO", texto)
                || ContainsText(x.traslado_salario ? "SI" : "NO", texto));
        }

        /// <summary>
        /// Aplica ordenamiento en memoria según el sort recibido desde lazy load.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static IEnumerable<CoAplicaAbonosBalloonsListaItemDto> ApplySort(
            IEnumerable<CoAplicaAbonosBalloonsListaItemDto> source,
            FiltrosLazyLoadData filtros)
        {
            string sortField = Clean(filtros?.sortField).ToLowerInvariant();
            bool asc = filtros?.sortOrder != 0;

            return (sortField, asc) switch
            {
                ("cedula", true) => source.OrderBy(x => x.cedula),
                ("cedula", false) => source.OrderByDescending(x => x.cedula),

                ("nombre", true) => source.OrderBy(x => x.nombre),
                ("nombre", false) => source.OrderByDescending(x => x.nombre),

                ("codigo", true) => source.OrderBy(x => x.codigo),
                ("codigo", false) => source.OrderByDescending(x => x.codigo),

                ("operacion", true) => source.OrderBy(x => x.operacion),
                ("operacion", false) => source.OrderByDescending(x => x.operacion),

                ("cuota", true) => source.OrderBy(x => x.cuota),
                ("cuota", false) => source.OrderByDescending(x => x.cuota),

                ("preanalisis", true) => source.OrderBy(x => x.preanalisis),
                ("preanalisis", false) => source.OrderByDescending(x => x.preanalisis),

                ("periodicidad", true) => source.OrderBy(x => x.periodicidad),
                ("periodicidad", false) => source.OrderByDescending(x => x.periodicidad),

                ("disponible_cuenta", true) => source.OrderBy(x => x.disponible_cuenta),
                ("disponible_cuenta", false) => source.OrderByDescending(x => x.disponible_cuenta),

                ("disponible_sobres", true) => source.OrderBy(x => x.disponible_sobres),
                ("disponible_sobres", false) => source.OrderByDescending(x => x.disponible_sobres),

                ("disponible_fondos", true) => source.OrderBy(x => x.disponible_fondos),
                ("disponible_fondos", false) => source.OrderByDescending(x => x.disponible_fondos),

                ("disponible_fondos_especial", true) => source.OrderBy(x => x.disponible_fondos_especial),
                ("disponible_fondos_especial", false) => source.OrderByDescending(x => x.disponible_fondos_especial),

                ("indicador", true) => source.OrderBy(x => x.indicador),
                ("indicador", false) => source.OrderByDescending(x => x.indicador),

                ("traslado_salario", true) => source.OrderBy(x => x.traslado_salario),
                ("traslado_salario", false) => source.OrderByDescending(x => x.traslado_salario),

                (_, true) => source.OrderBy(x => x.cedula).ThenBy(x => x.operacion),
                _ => source.OrderByDescending(x => x.cedula).ThenByDescending(x => x.operacion)
            };
        }

        /// <summary>
        /// Aplica paginación en memoria sobre la lista ya filtrada y ordenada.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        private static IEnumerable<CoAplicaAbonosBalloonsListaItemDto> ApplyPaginacion(
            IEnumerable<CoAplicaAbonosBalloonsListaItemDto> source,
            FiltrosLazyLoadData filtros)
        {
            int offset = filtros?.pagina ?? 0;
            int fetch = filtros?.paginacion ?? 0;

            if (offset < 0)
            {
                offset = 0;
            }

            if (fetch <= 0)
            {
                return source;
            }

            return source.Skip(offset).Take(fetch);
        }

        /// <summary>
        /// Evalúa si el texto indicado está contenido en la fuente, ignorando mayúsculas/minúsculas.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        private static bool ContainsText(string? source, string filtro)
            => !string.IsNullOrWhiteSpace(source)
               && source.Contains(filtro, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Normaliza texto null a string vacío y trim.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string Clean(string? value)
            => (value ?? string.Empty).Trim();
    }
}