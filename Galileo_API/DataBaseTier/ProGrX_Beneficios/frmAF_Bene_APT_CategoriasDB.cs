using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del catálogo de Categorías Apremiantes de Beneficios (frmAF_Bene_APT_Categorias).
    /// </summary>
    public partial class FrmAfBeneAptCategoriasDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneAptCategoriasDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene la lista de categorías apremiantes con paginación y filtro.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="pagina">Offset de paginación.</param>
        /// <param name="paginacion">Cantidad de registros por página.</param>
        /// <param name="filtro">Filtro de búsqueda por descripción.</param>
        /// <returns>Lista de categorías y total.</returns>
        public ErrorDto<AptCategoriasDataLista> CategoriasApremiante_Obtener(int CodEmpresa, int? pagina, int? paginacion, string? filtro)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var response = new AptCategoriasDataLista();

                const string sqlCount = "SELECT COUNT(*) FROM AFI_BENE_APT_CATEGORIAS";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount);

                var like = string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro}%";
                var offset = pagina ?? 0;
                var fetch = paginacion ?? 10;

                const string sql = @"SELECT id_apt_categoria, descripcion, activo, registro_fecha,
                                            registro_usuario, modifica_fecha, modifica_usuario
                                     FROM AFI_BENE_APT_CATEGORIAS
                                     WHERE (@like IS NULL OR DESCRIPCION LIKE @like)
                                     ORDER BY ID_APT_CATEGORIA
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.Lista = connection.Query<AptCategorias>(sql, new { like, offset, fetch }).ToList();
                return response;
            });
        }

        /// <summary>
        /// Inserta una categoría apremiante.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Datos de la categoría.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto CategoriasApremiante_Agregar(int CodEmpresa, AptCategorias request)
        {
            const string sql = @"INSERT INTO AFI_BENE_APT_CATEGORIAS (descripcion, activo, registro_fecha, registro_usuario)
                                 VALUES (@descripcion, @activo, GETDATE(), @registro_usuario)";

            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodEmpresa, sql, new
            {
                request.descripcion,
                activo = request.activo ? 1 : 0,
                request.registro_usuario
            });
        }

        /// <summary>
        /// Actualiza una categoría apremiante existente.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Datos de la categoría.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto CategoriasApremiante_Actualizar(int CodEmpresa, AptCategorias request)
        {
            const string sql = @"UPDATE AFI_BENE_APT_CATEGORIAS
                                 SET descripcion = @descripcion, activo = @activo,
                                     modifica_fecha = GETDATE(), modifica_usuario = @modifica_usuario
                                 WHERE ID_APT_CATEGORIA = @id_apt_categoria";

            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodEmpresa, sql, new
            {
                request.descripcion,
                activo = request.activo ? 1 : 0,
                request.modifica_usuario,
                request.id_apt_categoria
            });
        }

        /// <summary>
        /// Elimina una categoría apremiante.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="id">Identificador de la categoría.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto CategoriasApremiante_Eliminar(int CodEmpresa, int id)
        {
            const string sql = "DELETE FROM AFI_BENE_APT_CATEGORIAS WHERE ID_APT_CATEGORIA = @id";

            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodEmpresa, sql, new { id });
        }
    }
}
