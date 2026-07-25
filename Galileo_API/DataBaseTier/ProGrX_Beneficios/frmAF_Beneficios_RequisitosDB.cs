using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del catálogo de Requisitos para Beneficios (frmAF_Beneficios_Requisitos).
    /// Consultas aquí; guardado en el parcial .Guardar.
    /// </summary>
    public partial class FrmAfBeneficiosRequisitosDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficiosRequisitosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene la lista de requisitos para beneficios con paginación y filtro.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">JSON con filtro de búsqueda y paginación.</param>
        /// <returns>Lista de requisitos y total.</returns>
        public ErrorDto<BeneRequisitosDataLista> AfBeneRequisitos_Obtener(int CodCliente, string filtros)
        {
            var filtro = JsonConvert.DeserializeObject<AfiRequerimientoFiltros>(filtros) ?? new AfiRequerimientoFiltros();

            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new BeneRequisitosDataLista();

                const string sqlCount = "SELECT COUNT(COD_REQUISITO) FROM AFI_BENE_REQUISITOS";
                response.total = connection.QueryFirstOrDefault<int>(sqlCount);

                var like = string.IsNullOrWhiteSpace(filtro.filtro) ? null : $"%{filtro.filtro}%";
                var offset = filtro.pagina ?? 0;
                var fetch = filtro.paginacion ?? 10;

                const string sql = @"SELECT COD_REQUISITO AS cod_requisito, descripcion, Activo AS activo, requerido
                                     FROM AFI_BENE_REQUISITOS
                                     WHERE (@like IS NULL OR COD_REQUISITO LIKE @like OR descripcion LIKE @like)
                                     ORDER BY COD_REQUISITO
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.lista = connection.Query<BeneRequisitosData>(sql, new { like, offset, fetch }).ToList();
                return response;
            });
        }
    }
}
