using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Acceso a datos de los Parámetros de Beneficios Fosol (frmFSL_Parametros).
    /// </summary>
    public partial class FrmFslParametrosDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmFslParametrosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene la lista de parámetros Fosol con paginación y filtro.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">JSON con filtro de búsqueda y paginación.</param>
        /// <returns>Lista de parámetros y total.</returns>
        public ErrorDto<FdlParametrosListaDto> FslParametros_Obtener(int CodCliente, string filtros)
        {
            var filtro = JsonConvert.DeserializeObject<FdlParametrosFiltros>(filtros) ?? new FdlParametrosFiltros();

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new FdlParametrosListaDto();

                const string sqlCount = "SELECT COUNT(*) FROM FSL_PARAMETROS";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount);

                var like = string.IsNullOrWhiteSpace(filtro.filtro) ? null : $"%{filtro.filtro}%";
                var offset = filtro.pagina ?? 0;
                var fetch = filtro.paginacion ?? 10;

                const string sql = @"SELECT COD_PARAMETRO AS cod_parametro, DETALLE AS detalle, TIPO AS tipo,
                                            VALOR AS valor, NOTAS AS notas
                                     FROM FSL_PARAMETROS
                                     WHERE (@like IS NULL OR COD_PARAMETRO LIKE @like OR DETALLE LIKE @like)
                                     ORDER BY COD_PARAMETRO DESC
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.Comites = connection.Query<FdlParametrosDto>(sql, new { like, offset, fetch }).ToList();
                return response;
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<FdlParametrosListaDto>("FslParametros_Obtener - " + result.Description);
            }

            return result;
        }

        /// <summary>
        /// Actualiza el valor de un parámetro Fosol.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="parametro">Datos del parámetro.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FslParametros_Actualizar(int CodCliente, FdlParametrosDto parametro)
        {
            const string sql = @"UPDATE FSL_PARAMETROS
                                 SET REGISTRO_USUARIO = @registro_usuario, REGISTRO_FECHA = GETDATE(), valor = @valor
                                 WHERE cod_parametro = @cod_parametro";

            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new
            {
                parametro.registro_usuario,
                parametro.valor,
                parametro.cod_parametro
            });
        }
    }
}
