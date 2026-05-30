using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmAFTiposActividadesEcoDB
    {
        private readonly IConfiguration _config;

        private const string SqlActividadesTotal = @"
                    SELECT COUNT(cod_actividad)
                    FROM dbo.AFI_ACTIVIDADES_ECO
                    WHERE @hasFilter = 0 OR
                          cod_actividad LIKE @filtro OR
                          descripcion LIKE @filtro;";

        private const string SqlActividadesLista = @"
                    SELECT cod_actividad,
                           descripcion,
                           activa
                    FROM dbo.AFI_ACTIVIDADES_ECO
                    WHERE @hasFilter = 0 OR
                          cod_actividad LIKE @filtro OR
                          descripcion LIKE @filtro
                    ORDER BY
                        CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN cod_actividad END ASC,
                        CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN cod_actividad END DESC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN descripcion END ASC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN descripcion END DESC,
                        cod_actividad ASC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        private const string SqlActividadExiste = @"
                    SELECT ISNULL(COUNT(*), 0) AS Existe
                    FROM dbo.AFI_ACTIVIDADES_ECO
                    WHERE cod_actividad = @CodActividad;";

        private const string SqlActividadInsert = @"
                    INSERT INTO dbo.AFI_ACTIVIDADES_ECO
                    (
                        cod_actividad,
                        descripcion,
                        activa
                    )
                    VALUES
                    (
                        @CodActividad,
                        @Descripcion,
                        @Activa
                    );";

        private const string SqlActividadUpdate = @"
                    UPDATE dbo.AFI_ACTIVIDADES_ECO
                    SET descripcion = @Descripcion,
                        activa = @Activa
                    WHERE cod_actividad = @CodActividad;";

        private const string SqlActividadDelete = @"
                    DELETE FROM dbo.AFI_ACTIVIDADES_ECO
                    WHERE cod_actividad = @CodActividad;";

        private const string SqlSubActividadesTotal = @"
                    SELECT COUNT(COD_SUB_ACT)
                    FROM dbo.AFI_ACTIVIDADES_ECO_SUB
                    WHERE cod_actividad = @CodActividad
                      AND (@hasFilter = 0 OR
                           COD_SUB_ACT LIKE @filtro OR
                           descripcion LIKE @filtro);";

        private const string SqlSubActividadesLista = @"
                    SELECT COD_SUB_ACT,
                           descripcion,
                           activa,
                           cod_actividad
                    FROM dbo.AFI_ACTIVIDADES_ECO_SUB
                    WHERE cod_actividad = @CodActividad
                      AND (@hasFilter = 0 OR
                           COD_SUB_ACT LIKE @filtro OR
                           descripcion LIKE @filtro)
                    ORDER BY
                        CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN COD_SUB_ACT END ASC,
                        CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN COD_SUB_ACT END DESC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN descripcion END ASC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN descripcion END DESC,
                        COD_SUB_ACT ASC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        private const string SqlSubActividadExiste = @"
                    SELECT ISNULL(COUNT(*), 0) AS Existe
                    FROM dbo.AFI_ACTIVIDADES_ECO_SUB
                    WHERE cod_actividad = @CodActividad
                      AND COD_SUB_ACT = @CodSubAct;";

        private const string SqlSubActividadInsert = @"
                    INSERT INTO dbo.AFI_ACTIVIDADES_ECO_SUB
                    (
                        cod_actividad,
                        COD_SUB_ACT,
                        descripcion,
                        activa
                    )
                    VALUES
                    (
                        @CodActividad,
                        @CodSubAct,
                        @Descripcion,
                        @Activa
                    );";

        private const string SqlSubActividadUpdate = @"
                    UPDATE dbo.AFI_ACTIVIDADES_ECO_SUB
                    SET descripcion = @Descripcion,
                        activa = @Activa
                    WHERE cod_actividad = @CodActividad
                      AND COD_SUB_ACT = @CodSubAct;";

        private const string SqlSubActividadDelete = @"
                    DELETE FROM dbo.AFI_ACTIVIDADES_ECO_SUB
                    WHERE cod_actividad = @CodActividad
                      AND COD_SUB_ACT = @CodSubAct;";

        private static readonly IReadOnlyDictionary<string, int> ActividadesSortMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["cod_actividad"] = 1,
            ["descripcion"] = 2
        };

        private static readonly IReadOnlyDictionary<string, int> SubActividadesSortMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["COD_SUB_ACT"] = 1,
            ["cod_sub_act"] = 1,
            ["descripcion"] = 2
        };

        public FrmAFTiposActividadesEcoDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtener la lista de tipos de actividades económicas.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros de búsqueda, ordenamiento y paginación.</param>
        /// <returns>Lista paginada de actividades económicas.</returns>
        public ErrorDto<AfTiposActividadesEcoLista> AF_TiposActividadesEco_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var spec = LazyLoadHelper.Build(filtros, ActividadesSortMap, "cod_actividad");

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection => new AfTiposActividadesEcoLista
            {
                total = connection.QueryFirstOrDefault<int>(SqlActividadesTotal, spec.Params),
                lista = connection.Query<AfTiposActividadesEcoDto>(SqlActividadesLista, spec.Params).ToList()
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearListaVacia())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener actividades económicas.",
                    result.Code.GetValueOrDefault(-1),
                    CrearListaVacia());
        }

        /// <summary>
        /// Guardar un tipo de actividad económica.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Usuario">Usuario que realiza la operación.</param>
        /// <param name="Info">Datos de actividad económica.</param>
        /// <returns>Resultado del guardado.</returns>
        public ErrorDto AF_TiposActividadesEco_Guardar(int CodEmpresa, string Usuario, AfTiposActividadesEcoDto Info)
        {
            if (Info is null)
            {
                return DbHelper.ErrorResponse("Los datos de la actividad económica son requeridos.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var parametros = CrearParametrosActividad(Info);
                var existe = connection.QueryFirstOrDefault<int>(SqlActividadExiste, parametros);
                connection.Execute(existe == 0 ? SqlActividadInsert : SqlActividadUpdate, parametros);
                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar actividad económica.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Eliminar un tipo de actividad económica.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Usuario">Usuario que realiza la operación.</param>
        /// <param name="CodActividad">Código de actividad económica.</param>
        /// <returns>Resultado de la eliminación.</returns>
        public ErrorDto AF_TiposActividadesEco_Eliminar(int CodEmpresa, string Usuario, string CodActividad)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlActividadDelete,
                new { CodActividad = NormalizarTexto(CodActividad) });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar actividad económica.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Obtener la lista de sub actividades económicas.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="CodActividad">Código de actividad económica.</param>
        /// <param name="filtros">Filtros de búsqueda, ordenamiento y paginación.</param>
        /// <returns>Lista paginada de sub actividades económicas.</returns>
        public ErrorDto<AfTiposActividadesEcoLista> AF_TiposActividadesEco_SubActividad_Obtener(int CodEmpresa, string CodActividad, FiltrosLazyLoadData filtros)
        {
            var spec = LazyLoadHelper.Build(filtros, SubActividadesSortMap, "COD_SUB_ACT");
            spec.Params.Add("@CodActividad", NormalizarTexto(CodActividad));

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection => new AfTiposActividadesEcoLista
            {
                total = connection.QueryFirstOrDefault<int>(SqlSubActividadesTotal, spec.Params),
                lista = connection.Query<AfTiposActividadesEcoDto>(SqlSubActividadesLista, spec.Params).ToList()
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearListaVacia())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener sub actividades económicas.",
                    result.Code.GetValueOrDefault(-1),
                    CrearListaVacia());
        }

        /// <summary>
        /// Guardar una sub actividad económica.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Usuario">Usuario que realiza la operación.</param>
        /// <param name="Info">Datos de sub actividad económica.</param>
        /// <returns>Resultado del guardado.</returns>
        public ErrorDto AF_TiposActividadesEco_SubActividad_Guardar(int CodEmpresa, string Usuario, AfTiposActividadesEcoDto Info)
        {
            if (Info is null)
            {
                return DbHelper.ErrorResponse("Los datos de la sub actividad económica son requeridos.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var parametros = CrearParametrosSubActividad(Info);
                var existe = connection.QueryFirstOrDefault<int>(SqlSubActividadExiste, parametros);
                connection.Execute(existe == 0 ? SqlSubActividadInsert : SqlSubActividadUpdate, parametros);
                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar sub actividad económica.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Eliminar una sub actividad económica.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Usuario">Usuario que realiza la operación.</param>
        /// <param name="CodActividad">Código de actividad económica.</param>
        /// <param name="CodSubAct">Código de sub actividad económica.</param>
        /// <returns>Resultado de la eliminación.</returns>
        public ErrorDto AF_TiposActividadesEco_SubActividad_Eliminar(int CodEmpresa, string Usuario, string CodActividad, string CodSubAct)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                SqlSubActividadDelete,
                new
                {
                    CodActividad = NormalizarTexto(CodActividad),
                    CodSubAct = NormalizarTexto(CodSubAct)
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar sub actividad económica.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Crea parámetros seguros para actividad económica.
        /// </summary>
        private static object CrearParametrosActividad(AfTiposActividadesEcoDto info)
        {
            return new
            {
                CodActividad = NormalizarTexto(info.cod_actividad),
                Descripcion = NormalizarTexto(info.descripcion),
                Activa = info.activa ? 1 : 0
            };
        }

        /// <summary>
        /// Crea parámetros seguros para sub actividad económica.
        /// </summary>
        private static object CrearParametrosSubActividad(AfTiposActividadesEcoDto info)
        {
            return new
            {
                CodActividad = NormalizarTexto(info.cod_actividad),
                CodSubAct = NormalizarTexto(info.cod_sub_act),
                Descripcion = NormalizarTexto(info.descripcion),
                Activa = info.activa ? 1 : 0
            };
        }

        /// <summary>
        /// Crea una lista vacía para actividades económicas.
        /// </summary>
        private static AfTiposActividadesEcoLista CrearListaVacia()
        {
            return new AfTiposActividadesEcoLista
            {
                total = 0,
                lista = new List<AfTiposActividadesEcoDto>()
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