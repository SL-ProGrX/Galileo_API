using Dapper;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del catálogo de Estados de Beneficios (frmAF_Beneficios_Estados).
    /// </summary>
    public partial class FrmAfBeneficiosEstadosDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficiosEstadosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        // ==========================
        // Cuerpos SQL constantes
        // ==========================

        private const string SqlEstadosSelect = @"
SELECT cod_estado, descripcion, activo, orden, p_inicia, p_finaliza,
       registro_fecha, registro_usuario, modifica_fecha, modifica_usuario, proceso
FROM AFI_BENE_ESTADOS
";

        private const string SqlEstadosWhere = @"
WHERE (@filtro IS NULL)
   OR (COD_ESTADO LIKE @like)
   OR (DESCRIPCION LIKE @like)
";

        private const string SqlEstadosCount = @"
SELECT COUNT(1)
FROM AFI_BENE_ESTADOS
" + SqlEstadosWhere;

        /// <summary>
        /// Obtiene la lista de estados de beneficios con paginación, filtro y ordenamiento.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa (página, paginación, filtro y orden).</param>
        /// <returns>Lista de estados y total de registros.</returns>
        public ErrorDto<BeneEstadoDataLista> BeneficiosEstados_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var lista = QueryEstados(connection, filtros, true, out var total);
                return new BeneEstadoDataLista { total = total, lista = lista };
            });
        }

        /// <summary>
        /// Exporta la lista de estados aplicando el filtro vigente, sin paginar.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa; se ignora la paginación.</param>
        /// <returns>Lista de estados sin paginar.</returns>
        public ErrorDto<List<BeneEstado>> BeneficiosEstados_Exportar(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                QueryEstados(connection, filtros, false, out _));
        }

        // ==========================
        // Helpers privados de consulta
        // ==========================

        /// <summary>
        /// Consulta los estados aplicando filtro, orden y, opcionalmente, paginación.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="filtros">Filtros de carga perezosa.</param>
        /// <param name="usarPaginacion">Indica si se aplica OFFSET/FETCH.</param>
        /// <param name="total">Total de registros que cumplen el filtro.</param>
        /// <returns>Lista de estados.</returns>
        private static List<BeneEstado> QueryEstados(
            SqlConnection connection,
            FiltrosLazyLoadData filtros,
            bool usarPaginacion,
            out int total)
        {
            var (filtro, like) = BuildFiltroLike(filtros);
            var (sortField, sortOrder) = ResolveSort(filtros);

            total = connection.QuerySingle<int>(SqlEstadosCount, new { filtro, like });

            var sqlList = SqlEstadosSelect + SqlEstadosWhere + $"\nORDER BY {sortField} {sortOrder}";

            var offset = filtros?.pagina ?? 0;
            var fetch = filtros?.paginacion ?? 0;

            if (usarPaginacion && fetch > 0)
            {
                sqlList += "\nOFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";
            }
            else
            {
                sqlList += ";";
            }

            return connection.Query<BeneEstado>(sqlList, new { filtro, like, offset, fetch }).ToList();
        }

        /// <summary>
        /// Construye el texto de filtro y su patrón LIKE. Devuelve nulos cuando no hay filtro.
        /// </summary>
        /// <param name="filtros">Filtros de carga perezosa.</param>
        /// <returns>Tupla con el filtro normalizado y su patrón LIKE.</returns>
        private static (string? filtro, string? like) BuildFiltroLike(FiltrosLazyLoadData filtros)
        {
            var texto = filtros?.filtro?.Trim();
            if (string.IsNullOrWhiteSpace(texto))
            {
                return (null, null);
            }

            return (texto, $"%{texto}%");
        }

        /// <summary>
        /// Resuelve el campo y la dirección de ordenamiento usando una lista blanca de columnas.
        /// </summary>
        /// <param name="filtros">Filtros de carga perezosa.</param>
        /// <returns>Tupla con el campo y la dirección de ordenamiento.</returns>
        private static (string sortField, string sortOrder) ResolveSort(FiltrosLazyLoadData filtros)
        {
            // ORDER BY seguro (whitelist), nunca se concatena texto recibido del usuario.
            var sortField = (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "descripcion" => "DESCRIPCION",
                "orden" => "ORDEN",
                "proceso" => "PROCESO",
                "activo" => "ACTIVO",
                _ => "COD_ESTADO"
            };

            // Convención de PrimeNG: -1 descendente, 1 ascendente (ASC por defecto).
            var sortOrder = filtros?.sortOrder == -1 ? "DESC" : "ASC";
            return (sortField, sortOrder);
        }

        /// <summary>
        /// Inserta un estado de beneficio, validando que el código no exista.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Datos del estado.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto BeneficiosEstados_Agregar(int CodEmpresa, BeneEstado request)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);
            try
            {
                const string sqlExiste = "SELECT COUNT(*) FROM AFI_BENE_ESTADOS WHERE COD_ESTADO = @cod_estado";
                var existe = connection.QueryFirstOrDefault<int>(sqlExiste, new { request.cod_estado });

                if (existe > 0)
                {
                    return DbHelper.ErrorResponse("Ya existe un estado con el codigo: " + request.cod_estado + ", por favor verifique");
                }

                const string sql = @"INSERT INTO AFI_BENE_ESTADOS
                                        (cod_estado, descripcion, activo, orden, p_inicia, p_finaliza,
                                         registro_fecha, registro_usuario, proceso)
                                     VALUES
                                        (@cod_estado, @descripcion, @activo, @orden, @p_inicia, @p_finaliza,
                                         GETDATE(), @registro_usuario, @proceso)";

                connection.Execute(sql, new
                {
                    request.cod_estado,
                    request.descripcion,
                    activo = request.activo ? 1 : 0,
                    request.orden,
                    p_inicia = request.p_inicia ? 1 : 0,
                    p_finaliza = request.p_finaliza ? 1 : 0,
                    request.registro_usuario,
                    request.proceso
                });

                return DbHelper.OkResponse("Estado agregado correctamente");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza un estado de beneficio.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Datos del estado.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto BeneficiosEstados_Actualizar(int CodEmpresa, BeneEstado request)
        {
            const string sql = @"UPDATE AFI_BENE_ESTADOS
                                 SET descripcion = @descripcion, activo = @activo, orden = @orden,
                                     p_inicia = @p_inicia, p_finaliza = @p_finaliza,
                                     modifica_fecha = GETDATE(), modifica_usuario = @modifica_usuario, proceso = @proceso
                                 WHERE cod_estado = @cod_estado";

            var result = DbHelper.ExecuteNonQuery(CreatePortalDb(), CodEmpresa, sql, new
            {
                request.descripcion,
                activo = request.activo ? 1 : 0,
                request.orden,
                p_inicia = request.p_inicia ? 1 : 0,
                p_finaliza = request.p_finaliza ? 1 : 0,
                request.modifica_usuario,
                request.proceso,
                request.cod_estado
            });

            if (result.Code == 0)
            {
                result.Description = "Estado actualizado correctamente";
            }

            return result;
        }

        /// <summary>
        /// Elimina un estado de beneficio.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="id">Código del estado a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto BeneficiosEstados_Eliminar(int CodEmpresa, string id)
        {
            const string sql = "DELETE FROM AFI_BENE_ESTADOS WHERE COD_ESTADO = @id";

            var result = DbHelper.ExecuteNonQuery(CreatePortalDb(), CodEmpresa, sql, new { id });

            if (result.Code == 0)
            {
                result.Description = "Estado eliminado correctamente";
            }

            return result;
        }
    }
}
