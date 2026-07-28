using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del catálogo de Profesionales Apremiantes de Beneficios (frmAF_Beneficios_APT_Profesionales).
    /// Consultas aquí; guardado en el parcial .Guardar.
    /// </summary>
    public partial class FrmAfBeneficiosAptProfesionalesDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficiosAptProfesionalesDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene la lista de profesionales apremiantes con paginación y filtro.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">JSON con filtro de búsqueda y paginación.</param>
        /// <returns>Lista de profesionales y total.</returns>
        public ErrorDto<BeneAptProfesionalesDataLista> AfBeneAptPro_Obtener(int CodCliente, string filtros)
        {
            var filtro = JsonConvert.DeserializeObject<AfiAptProFiltros>(filtros) ?? new AfiAptProFiltros();

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new BeneAptProfesionalesDataLista();

                const string sqlCount = "SELECT COUNT(ID_PROFESIONAL) FROM AFI_BENE_APT_PROFESIONALES";
                response.total = connection.QueryFirstOrDefault<int>(sqlCount);

                var like = string.IsNullOrWhiteSpace(filtro.filtro) ? null : $"%{filtro.filtro}%";
                var offset = filtro.pagina ?? 0;
                var fetch = filtro.paginacion ?? 10;

                const string sql = @"SELECT ID_PROFESIONAL AS id_profesional, IDENTIFICACION AS identificacion,
                                            NOMBRE AS nombre, USUARIO AS usuario, ACTIVO AS activo,
                                            REGISTRO_FECHA AS registro_fecha, REGISTRO_USUARIO AS registro_usuario,
                                            MODIFICA_FECHA AS modifica_fecha, MODIFICA_USUARIO AS modifica_usuario
                                     FROM AFI_BENE_APT_PROFESIONALES
                                     WHERE (@like IS NULL OR ID_PROFESIONAL LIKE @like OR IDENTIFICACION LIKE @like
                                            OR USUARIO LIKE @like OR NOMBRE LIKE @like)
                                     ORDER BY ID_PROFESIONAL
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.lista = connection.Query<BeneAptProfesionalesData>(sql, new { like, offset, fetch }).ToList();
                return response;
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<BeneAptProfesionalesDataLista>("AfBeneAptPro_Obtener - " + result.Description);
            }

            return result;
        }
    }
}
