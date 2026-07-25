using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del catálogo de Tipos de Sanciones de Beneficios (frmAF_Bene_Sanciones_Tipos).
    /// Consultas aquí; guardado en el parcial .Guardar.
    /// </summary>
    public partial class FrmAfBeneSancionesTiposDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneSancionesTiposDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene la lista de tipos de sanciones con paginación y filtro.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">JSON con filtro de búsqueda y paginación.</param>
        /// <returns>Lista de tipos de sanciones y total.</returns>
        public ErrorDto<AfTipoSancionesDtoLista> afBeneTipoSancionObtener(int CodCliente, string filtros)
        {
            var filtro = JsonConvert.DeserializeObject<AfiTipoSancionfiltros>(filtros) ?? new AfiTipoSancionfiltros();

            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new AfTipoSancionesDtoLista();

                const string sqlCount = "SELECT COUNT(TIPO_SANCION) FROM AFI_BENE_SANCIONES_TIPOS";
                response.total = connection.QueryFirstOrDefault<int>(sqlCount);

                var like = string.IsNullOrWhiteSpace(filtro.filtro) ? null : $"%{filtro.filtro}%";
                var offset = filtro.pagina ?? 0;
                var fetch = filtro.paginacion ?? 10;

                const string sql = @"SELECT TIPO_SANCION AS tipo_sancion, DESCRIPCION AS descripcion,
                                            CODIGO_COBRO AS codigo_cobro, PLAZO_MAXIMO AS plazo_maximo, ACTIVO AS activo,
                                            REGISTRO_FECHA AS registro_fecha, REGISTRO_USUARIO AS registro_usuario,
                                            MODIFICA_FECHA AS modifica_fecha, MODIFICA_USUARIO AS modifica_usuario
                                     FROM AFI_BENE_SANCIONES_TIPOS
                                     WHERE (@like IS NULL OR TIPO_SANCION LIKE @like OR DESCRIPCION LIKE @like
                                            OR REGISTRO_USUARIO LIKE @like OR CODIGO_COBRO LIKE @like)
                                     ORDER BY TIPO_SANCION
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.lista = connection.Query<AfTipoSancionesDto>(sql, new { like, offset, fetch }).ToList();
                return response;
            });
        }

        /// <summary>
        /// Obtiene el catálogo de retenciones disponibles.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de retenciones.</returns>
        public ErrorDto<List<BeneListaRetencion>> BeneRetenciones_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = "SELECT * FROM [dbo].[vAFI_Bene_Retenciones_Catalogo]";
                return connection.Query<BeneListaRetencion>(sql).ToList();
            });
        }
    }
}
