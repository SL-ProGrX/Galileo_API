using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAfPromotoresReportesDB
    {
        private readonly IConfiguration _config;

        private const string SqlPromotoresReportesTotal = @"
                    SELECT COUNT(*)
                    FROM dbo.Promotores
                    WHERE (@hasFilter = 0 OR ID_Promotor LIKE @filtro OR Nombre LIKE @filtro)
                      AND (@Estado IS NULL OR Estado = @Estado)
                      AND (@Tipo IS NULL OR Tipo = @Tipo);";

        private const string SqlPromotoresReportesLista = @"
                    SELECT ID_Promotor AS item,
                           Nombre AS descripcion
                    FROM dbo.Promotores
                    WHERE (@hasFilter = 0 OR ID_Promotor LIKE @filtro OR Nombre LIKE @filtro)
                      AND (@Estado IS NULL OR Estado = @Estado)
                      AND (@Tipo IS NULL OR Tipo = @Tipo)
                    ORDER BY
                        CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN ID_Promotor END ASC,
                        CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN ID_Promotor END DESC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN Nombre END ASC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN Nombre END DESC,
                        ID_Promotor ASC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        private static readonly IReadOnlyDictionary<string, int> PromotoresReportesSortMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["ID_Promotor"] = 1,
            ["ID_PROMOTOR"] = 1,
            ["Nombre"] = 2,
            ["NOMBRE"] = 2
        };

        public FrmAfPromotoresReportesDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene la lista paginada de promotores para reportes.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtro">Filtros de búsqueda, parámetros, ordenamiento y paginación.</param>
        /// <returns>Lista paginada de promotores para reportes.</returns>
        public ErrorDto<TablasListaGenericaModel> AF_PromotoresReportes_Obtener(int CodEmpresa, FiltrosLazyLoadData filtro)
        {
            filtro ??= new FiltrosLazyLoadData();
            var parametros = CrearParametrosReporte(filtro);

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection => new TablasListaGenericaModel
            {
                total = connection.QueryFirstOrDefault<int>(SqlPromotoresReportesTotal, parametros),
                lista = connection.Query<DropDownListaGenericaModel>(SqlPromotoresReportesLista, parametros).ToList()
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearListaVacia())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener promotores para reportes.",
                    result.Code.GetValueOrDefault(-1),
                    CrearListaVacia());
        }

        /// <summary>
        /// Crea parámetros seguros para consultar promotores de reportes.
        /// </summary>
        /// <param name="filtro">Filtros de búsqueda y paginación.</param>
        /// <returns>Parámetros para Dapper.</returns>
        private static object CrearParametrosReporte(FiltrosLazyLoadData filtro)
        {
            var sortField = string.IsNullOrWhiteSpace(filtro.sortField)
                ? "ID_Promotor"
                : filtro.sortField;

            if (!PromotoresReportesSortMap.TryGetValue(sortField, out var sortCode))
            {
                sortCode = 1;
            }

            var textoFiltro = NormalizarTexto(filtro.filtro);
            var pageSize = Math.Max(1, filtro.paginacion);

            return new
            {
                hasFilter = string.IsNullOrWhiteSpace(textoFiltro) ? 0 : 1,
                filtro = string.IsNullOrWhiteSpace(textoFiltro) ? null : $"%{textoFiltro}%",
                Estado = ObtenerParametroTexto(filtro.parametros, "Estado"),
                Tipo = ObtenerParametroTexto(filtro.parametros, "Tipo"),
                sortCode,
                isAsc = filtro.sortOrder == -1 ? 0 : 1,
                offset = Math.Max(0, filtro.pagina),
                fetch = pageSize
            };
        }

        /// <summary>
        /// Obtiene un parámetro de texto permitido desde el objeto de parámetros.
        /// </summary>
        /// <param name="parametros">Objeto de parámetros recibido desde pantalla.</param>
        /// <param name="nombre">Nombre del parámetro.</param>
        /// <returns>Valor normalizado o null.</returns>
        private static string? ObtenerParametroTexto(object? parametros, string nombre)
        {
            if (parametros is null)
            {
                return null;
            }

            var json = parametros.ToString();
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            var jObject = Newtonsoft.Json.Linq.JObject.Parse(json);
            var valor = NormalizarTexto(jObject[nombre]?.ToString());

            return string.IsNullOrWhiteSpace(valor) ? null : valor;
        }

        /// <summary>
        /// Crea una lista vacía para resultados paginados.
        /// </summary>
        /// <returns>Modelo de lista vacío.</returns>
        private static TablasListaGenericaModel CrearListaVacia()
        {
            return new TablasListaGenericaModel
            {
                total = 0,
                lista = new List<DropDownListaGenericaModel>()
            };
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        /// <returns>Instancia de PortalDB configurada.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        /// <param name="valor">Valor original.</param>
        /// <returns>Texto sin espacios externos o cadena vacía.</returns>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}