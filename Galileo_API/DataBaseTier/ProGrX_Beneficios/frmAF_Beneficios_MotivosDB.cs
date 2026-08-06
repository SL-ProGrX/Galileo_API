using Dapper;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del catálogo de Motivos de Beneficios (frmAF_Beneficios_Motivos).
    /// </summary>
    public partial class FrmAfBeneficiosMotivosDB : BeneficiosCatalogoDbBase
    {
        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficiosMotivosDB(IConfiguration config) : base(config)
        {
        }

        // ==========================
        // Cuerpos SQL constantes
        // ==========================

        private const string SqlMotivosSelect = @"
SELECT cod_motivo, descripcion, activo, registro_fecha,
       registro_usuario, modifica_fecha, modifica_usuario
FROM AFI_BENE_MOTIVOS
";

        private const string SqlMotivosWhere = @"
WHERE (@filtro IS NULL)
   OR (COD_MOTIVO LIKE @like)
   OR (DESCRIPCION LIKE @like)
";

        private const string SqlMotivosCount = @"
SELECT COUNT(1)
FROM AFI_BENE_MOTIVOS
" + SqlMotivosWhere;

        /// <summary>
        /// Obtiene la lista de motivos de beneficios con paginación, filtro y ordenamiento.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa (página, paginación, filtro y orden).</param>
        /// <returns>Lista de motivos y total de registros.</returns>
        public ErrorDto<BeneMotivosDataLista> BeneficiosMotivos_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var lista = QueryMotivos(connection, filtros, true, out var total);
                return new BeneMotivosDataLista { total = total, lista = lista };
            });
        }

        /// <summary>
        /// Exporta la lista de motivos aplicando el filtro vigente, sin paginar.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa; se ignora la paginación.</param>
        /// <returns>Lista de motivos sin paginar.</returns>
        public ErrorDto<List<BeneMotivos>> BeneficiosMotivos_Exportar(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                QueryMotivos(connection, filtros, false, out _));
        }

        // ==========================
        // Helpers privados de consulta
        // ==========================

        /// <summary>
        /// Consulta los motivos aplicando filtro, orden y, opcionalmente, paginación.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="filtros">Filtros de carga perezosa.</param>
        /// <param name="usarPaginacion">Indica si se aplica OFFSET/FETCH.</param>
        /// <param name="total">Total de registros que cumplen el filtro.</param>
        /// <returns>Lista de motivos.</returns>
        private static List<BeneMotivos> QueryMotivos(
            SqlConnection connection,
            FiltrosLazyLoadData filtros,
            bool usarPaginacion,
            out int total)
        {
            var (filtro, like) = BuildFiltroLike(filtros);
            var (sortField, sortOrder) = ResolveSort(filtros);

            total = connection.QuerySingle<int>(SqlMotivosCount, new { filtro, like });

            var sqlList = SqlMotivosSelect + SqlMotivosWhere + $"\nORDER BY {sortField} {sortOrder}";

            var offset = filtros?.pagina ?? 0;
            var fetch = filtros?.paginacion ?? 0;

            sqlList = AplicarPaginacion(sqlList, usarPaginacion, fetch);

            return connection.Query<BeneMotivos>(sqlList, new { filtro, like, offset, fetch }).ToList();
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
                "activo" => "ACTIVO",
                _ => "COD_MOTIVO"
            };

            // Convención de PrimeNG: -1 descendente, 1 ascendente (ASC por defecto).
            var sortOrder = filtros?.sortOrder == -1 ? "DESC" : "ASC";
            return (sortField, sortOrder);
        }

        /// <summary>
        /// Inserta un motivo de beneficio, validando que el código no exista.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Datos del motivo.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto BeneficiosMotivos_Agregar(int CodEmpresa, BeneMotivos request)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);
            try
            {
                const string sqlExiste = "SELECT COUNT(*) FROM AFI_BENE_MOTIVOS WHERE COD_MOTIVO = @cod_motivo";
                var existe = connection.QueryFirstOrDefault<int>(sqlExiste, new { request.cod_motivo });

                if (existe > 0)
                {
                    return DbHelper.ErrorResponse("Ya existe un motivo con el codigo: " + request.cod_motivo + ", por favor verifique");
                }

                const string sql = @"INSERT INTO AFI_BENE_MOTIVOS (cod_motivo, descripcion, activo, registro_fecha, registro_usuario)
                                     VALUES (@cod_motivo, @descripcion, @activo, GETDATE(), @registro_usuario)";

                connection.Execute(sql, new
                {
                    request.cod_motivo,
                    request.descripcion,
                    activo = request.activo ? 1 : 0,
                    request.registro_usuario
                });

                return DbHelper.OkResponse("Motivo agregado correctamente");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza el detalle de un motivo de beneficio.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Datos del motivo.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto BeneficiosMotivos_Actualizar(int CodEmpresa, BeneMotivos request)
        {
            const string sql = @"UPDATE AFI_BENE_MOTIVOS
                                 SET descripcion = @descripcion, activo = @activo,
                                     modifica_fecha = GETDATE(), modifica_usuario = @modifica_usuario
                                 WHERE cod_motivo = @cod_motivo";

            var result = DbHelper.ExecuteNonQuery(CreatePortalDb(), CodEmpresa, sql, new
            {
                request.descripcion,
                activo = request.activo ? 1 : 0,
                request.modifica_usuario,
                request.cod_motivo
            });

            if (result.Code == 0)
            {
                result.Description = "Motivo actualizado correctamente";
            }

            return result;
        }

        /// <summary>
        /// Elimina un motivo de beneficio.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="id">Código del motivo a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto BeneficiosMotivos_Eliminar(int CodEmpresa, string id)
        {
            const string sql = "DELETE FROM AFI_BENE_MOTIVOS WHERE COD_MOTIVO = @id";

            var result = DbHelper.ExecuteNonQuery(CreatePortalDb(), CodEmpresa, sql, new { id });

            if (result.Code == 0)
            {
                result.Description = "Motivo eliminado correctamente";
            }

            return result;
        }
    }
}
