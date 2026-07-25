using Dapper;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos de Categorías de Beneficios (frmAF_Beneficios_Categorias).
    /// Consultas aquí; guardado en .Guardar, permisos por rol en .Permisos.
    /// </summary>
    public partial class FrmAfBeneficiosCategoriasDB
    {
        private readonly IConfiguration _config;
        private readonly MBeneficiosDB _mBeneficiosDB;

        /// <summary>
        /// Inicializa el acceso a datos y la bitácora de beneficios con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficiosCategoriasDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mBeneficiosDB = new MBeneficiosDB(_config);
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene la lista de categorías de beneficios con paginación y filtro.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="pagina">Offset de paginación.</param>
        /// <param name="paginacion">Cantidad de registros por página.</param>
        /// <param name="filtro">Filtro por código o descripción.</param>
        /// <returns>Lista de categorías y total.</returns>
        public ErrorDto<BEeneCategoriaDataLista> BeneficiosCategorias_Obtener(int CodEmpresa, int? pagina, int? paginacion, string? filtro)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var response = new BEeneCategoriaDataLista();

                const string sqlCount = "SELECT COUNT(*) FROM AFI_BENE_CATEGORIAS";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount);

                var like = string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro}%";
                var offset = pagina ?? 0;
                var fetch = paginacion ?? 10;

                const string sql = @"SELECT * FROM AFI_BENE_CATEGORIAS
                                     WHERE (@like IS NULL OR COD_CATEGORIA LIKE @like OR DESCRIPCION LIKE @like)
                                     ORDER BY COD_CATEGORIA
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.Lista = connection.Query<BeneCategoria>(sql, new { like, offset, fetch }).ToList();
                return response;
            });
        }

        /// <summary>
        /// Obtiene los permisos por usuario de una categoría mediante SP.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_categoria">Código de la categoría.</param>
        /// <param name="filtro">Filtro de búsqueda de usuario.</param>
        /// <returns>Lista de permisos.</returns>
        public ErrorDto<List<BeneCategoriaPermisos>> BeneficiosCategorias_ObtenerPermisos(int CodCliente, string cod_categoria, string? filtro)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Query<BeneCategoriaPermisos>("EXEC spAFI_Bene_CategoriaPermisos @cod_categoria, @filtro",
                    new { cod_categoria, filtro }).ToList());
        }

        /// <summary>
        /// Obtiene el catálogo de validaciones de beneficios activas.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de validaciones.</returns>
        public ErrorDto<List<BeneValidaLista>> BeneValidacionesLista_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = "SELECT COD_VAL AS item, DESCRIPCION AS descripcion FROM AFI_BENE_VALIDACIONES WHERE ESTADO = 1";
                return connection.Query<BeneValidaLista>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene las validaciones asignadas a una categoría.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_categoria">Código de la categoría.</param>
        /// <returns>Lista de validaciones por categoría.</returns>
        public ErrorDto<List<BeneCategoriaValidaLista>> BeneCategoriaValida_Obtener(int CodCliente, string cod_categoria)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = "SELECT * FROM AFI_BENE_VALIDA_CATEGORIA WHERE cod_categoria = @cod_categoria";
                return connection.Query<BeneCategoriaValidaLista>(sql, new { cod_categoria }).ToList();
            });
        }

        /// <summary>
        /// Registra un movimiento en la bitácora de beneficios.
        /// </summary>
        private void RegistrarBitacora(int CodCliente, string movimiento, string detalle, string codBeneficio, string registraUser)
        {
            _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDto
            {
                EmpresaId = CodCliente,
                cod_beneficio = codBeneficio,
                consec = -2,
                movimiento = movimiento,
                detalle = detalle,
                registro_usuario = registraUser
            });
        }
    }
}
