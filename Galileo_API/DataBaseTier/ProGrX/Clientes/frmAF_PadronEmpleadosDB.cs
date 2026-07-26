using Dapper;
using Newtonsoft.Json.Linq;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAfPadronEmpleadosDB
    {
        private readonly IConfiguration _config;

        private const string SqlInstituciones = @"
                    SELECT COD_INSTITUCION AS item,
                           CONCAT('[', COD_DIVISA, ']  ', DESCRIPCION) AS descripcion
                    FROM dbo.INSTITUCIONES
                    WHERE ACTIVA = 1
                    ORDER BY COD_INSTITUCION;";

        private const string SqlEstados = @"
                    SELECT RTRIM(COD_ESTADO) AS item,
                           RTRIM(DESCRIPCION) AS descripcion
                    FROM dbo.AFI_ESTADOS_PERSONA
                    WHERE ACTIVO = 1
                    ORDER BY COD_ESTADO;";

        private const string SqlEliminarEmpleado = @"
                    DELETE FROM dbo.AFI_PADRON
                    WHERE CEDULA = @Cedula;";

        private const string SqlPadronTotal = @"
                    SELECT COUNT(*)
                    FROM dbo.AFI_PADRON P
                    LEFT JOIN dbo.Socios S
                        ON S.CEDULA IN (P.ID_ALTERNO, P.CEDULA)
                    LEFT JOIN dbo.AFI_ESTADOS_PERSONA Pe
                        ON S.EstadoActual = Pe.COD_ESTADO
                    WHERE
                        (@EstadosCount = 0 OR S.EstadoActual IN @Estados)
                        AND (@InstitucionesCount = 0 OR P.INSTITUCION IN @Instituciones)
                        AND (@Cedula IS NULL OR P.CEDULA LIKE @Cedula)
                        AND (@Nombre IS NULL OR P.NOMBRE LIKE @Nombre)
                        AND (@EstadoPersona IS NULL OR Pe.Descripcion LIKE @EstadoPersona)
                        AND (@IngChkFecha = 0 OR P.FECHA_INGRESO BETWEEN @IngFechaInicio AND @IngFechaCorte)
                        AND (@RegChkFecha = 0 OR P.REGISTRO_FECHA BETWEEN @RegFechaInicio AND @RegFechaCorte);";

        private const string SqlPadronLista = @"
                    SELECT P.*, ISNULL(Pe.Descripcion, 'No Localizado') AS EstadoPersona
                    FROM dbo.AFI_PADRON P
                    LEFT JOIN dbo.Socios S
                        ON S.CEDULA IN (P.ID_ALTERNO, P.CEDULA)
                    LEFT JOIN dbo.AFI_ESTADOS_PERSONA Pe
                        ON S.EstadoActual = Pe.COD_ESTADO
                    WHERE
                        (@EstadosCount = 0 OR S.EstadoActual IN @Estados)
                        AND (@InstitucionesCount = 0 OR P.INSTITUCION IN @Instituciones)
                        AND (@Cedula IS NULL OR P.CEDULA LIKE @Cedula)
                        AND (@Nombre IS NULL OR P.NOMBRE LIKE @Nombre)
                        AND (@EstadoPersona IS NULL OR Pe.Descripcion LIKE @EstadoPersona)
                        AND (@IngChkFecha = 0 OR P.FECHA_INGRESO BETWEEN @IngFechaInicio AND @IngFechaCorte)
                        AND (@RegChkFecha = 0 OR P.REGISTRO_FECHA BETWEEN @RegFechaInicio AND @RegFechaCorte)
                    ORDER BY
                        CASE WHEN @SortCode = 1 AND @IsAsc = 1 THEN P.CEDULA END ASC,
                        CASE WHEN @SortCode = 1 AND @IsAsc = 0 THEN P.CEDULA END DESC,
                        CASE WHEN @SortCode = 2 AND @IsAsc = 1 THEN P.NOMBRE END ASC,
                        CASE WHEN @SortCode = 2 AND @IsAsc = 0 THEN P.NOMBRE END DESC,
                        CASE WHEN @SortCode = 3 AND @IsAsc = 1 THEN P.INSTITUCION END ASC,
                        CASE WHEN @SortCode = 3 AND @IsAsc = 0 THEN P.INSTITUCION END DESC,
                        P.CEDULA ASC
                    OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

        private static readonly IReadOnlyDictionary<string, int> SortMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["P.CEDULA"] = 1,
            ["CEDULA"] = 1,
            ["P.NOMBRE"] = 2,
            ["NOMBRE"] = 2,
            ["P.INSTITUCION"] = 3,
            ["INSTITUCION"] = 3
        };

        public FrmAfPadronEmpleadosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene la lista de instituciones activas para el padrón de empleados.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado de instituciones.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_PadronEmpleadosInstituciones_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlInstituciones);
        }


        /// <summary>
        /// Obtiene la lista de estados activos para el padrón de empleados.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado de estados.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_PadronEmpleadosEstados_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlEstados);
        }


        /// <summary>
        /// Elimina un empleado del padrón.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="cedula">Cédula del empleado.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AF_PadronEmpleados_Eliminar(int CodEmpresa, string cedula)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlEliminarEmpleado,
                new
                {
                    Cedula = NormalizarTexto(cedula)
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar empleado del padrón.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Obtiene el padrón de empleados con filtros y paginación.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="exporta">Indica si se exportará el resultado completo.</param>
        /// <param name="filtros">Filtros del padrón.</param>
        /// <param name="tblFiltros">Configuración de paginación y ordenamiento.</param>
        /// <returns>Listado paginado del padrón.</returns>
        public ErrorDto<TablasListaGenericaModel> AF_PadronEmpleados_Obtener(int CodEmpresa, bool exporta, AfPadronEmpleadosFiltro filtros, FiltrosLazyLoadData tblFiltros)
        {
            filtros ??= new AfPadronEmpleadosFiltro();
            tblFiltros ??= new FiltrosLazyLoadData();

            var parametros = CrearParametrosPadron(filtros, tblFiltros, exporta);

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var total = connection.QueryFirstOrDefault<int>(SqlPadronTotal, parametros);
                var lista = connection.Query<AfPadronEmpleadosDto>(SqlPadronLista, parametros).ToList();

                return new TablasListaGenericaModel
                {
                    total = total,
                    lista = lista
                };
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearListaVacia())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener padrón de empleados.",
                    result.Code.GetValueOrDefault(-1),
                    CrearListaVacia());
        }

        /// <summary>
        /// Crea parámetros seguros para consultar el padrón.
        /// </summary>
        private static object CrearParametrosPadron(AfPadronEmpleadosFiltro filtros, FiltrosLazyLoadData tblFiltros, bool exporta)
        {
            var sortField = string.IsNullOrWhiteSpace(tblFiltros.sortField)
                ? "P.CEDULA"
                : tblFiltros.sortField;

            if (!SortMap.TryGetValue(sortField, out int sortCode))
            {
                sortCode = 1;
            }

            var isAsc = tblFiltros.sortOrder == 1;
            var fetch = exporta ? 1000000 : Math.Max(1, tblFiltros.paginacion);

            var filtrosTabla = ObtenerFiltrosTabla(tblFiltros.filters);

            return new
            {
                Estados = filtros.estado ?? new List<object>(),
                EstadosCount = filtros.estado?.Count ?? 0,
                Instituciones = filtros.institucion ?? new List<object>(),
                InstitucionesCount = filtros.institucion?.Count ?? 0,
                Cedula = CrearLikeValue(filtrosTabla, "CEDULA"),
                Nombre = CrearLikeValue(filtrosTabla, "NOMBRE"),
                EstadoPersona = CrearLikeValue(filtrosTabla, "EstadoPersona"),
                IngChkFecha = filtros.ing_chk_fecha ? 1 : 0,
                filtros.ing_fecha_inicio,
                filtros.ing_fecha_corte,
                IngFechaInicio = filtros.ing_fecha_inicio,
                IngFechaCorte = filtros.ing_fecha_corte,
                RegChkFecha = filtros.reg_chk_fecha ? 1 : 0,
                filtros.reg_fecha_inicio,
                filtros.reg_fecha_corte,
                RegFechaInicio = filtros.reg_fecha_inicio,
                RegFechaCorte = filtros.reg_fecha_corte,
                SortCode = sortCode,
                IsAsc = isAsc ? 1 : 0,
                Offset = Math.Max(0, tblFiltros.pagina),
                Fetch = fetch
            };
        }

        /// <summary>
        /// Obtiene filtros dinámicos de tabla.
        /// </summary>
        private static Dictionary<string, string> ObtenerFiltrosTabla(object? filtros)
        {
            if (filtros is not JObject jObject)
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            var dict = jObject.ToObject<Dictionary<string, object>>();
            if (dict is null)
            {
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            return dict
                .Select(x => new
                {
                    x.Key,
                    Valor = (x.Value as JObject)?["value"]?.ToString()
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Valor))
                .ToDictionary(x => x.Key, x => x.Valor, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Construye valores LIKE seguros.
        /// </summary>
        private static string? CrearLikeValue(Dictionary<string, string> filtros, string key)
        {
            return filtros.TryGetValue(key, out var valor)
                ? $"%{NormalizarTexto(valor)}%"
                : null;
        }

        /// <summary>
        /// Crea una lista vacía para resultados paginados.
        /// </summary>
        private static TablasListaGenericaModel CrearListaVacia()
        {
            return new TablasListaGenericaModel
            {
                lista = new List<AfPadronEmpleadosDto>(),
                total = 0
            };
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();

    }
}