using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Acceso a datos de los Comités Fosol (frmFSL_Comite).
    /// Consultas aquí; guardado de comités y miembros en el parcial .Guardar.
    /// </summary>
    public partial class FrmFslComiteDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmFslComiteDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene la lista de comités con paginación y filtro.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">JSON con filtro de búsqueda y paginación.</param>
        /// <returns>Lista de comités y total.</returns>
        public ErrorDto<FslComitesDataLista> FslComites_Obtener(int CodCliente, string filtros)
        {
            var filtro = JsonConvert.DeserializeObject<FslComitefiltros>(filtros) ?? new FslComitefiltros();

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new FslComitesDataLista();

                const string sqlCount = "SELECT COUNT(*) FROM FSL_COMITES";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount);

                var like = string.IsNullOrWhiteSpace(filtro.filtro) ? null : $"%{filtro.filtro}%";
                var offset = filtro.pagina ?? 0;
                var fetch = filtro.paginacion ?? 10;

                const string sql = @"SELECT COD_COMITE AS cod_comite, DESCRIPCION AS descripcion,
                                            NUMERO_RESOLUTORES AS numero_resolutores, ACTIVO AS activo
                                     FROM FSL_COMITES
                                     WHERE (@like IS NULL OR COD_COMITE LIKE @like OR descripcion LIKE @like)
                                     ORDER BY COD_COMITE
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.Comites = connection.Query<FslComitesDto>(sql, new { like, offset, fetch }).ToList();
                return response;
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<FslComitesDataLista>("FslComites_Obtener - " + result.Description);
            }

            return result;
        }

        /// <summary>
        /// Obtiene los comités activos.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de comités activos.</returns>
        public ErrorDto<List<FslComitesActivosData>> FslComitesActivos_Obtener(int CodCliente)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT COD_COMITE AS item, RTRIM(COD_COMITE) + ' - ' + DESCRIPCION AS descripcion
                                     FROM FSL_COMITES WHERE ACTIVO = 1";
                return connection.Query<FslComitesActivosData>(sql).ToList();
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<List<FslComitesActivosData>>("FslComitesActivos_Obtener - " + result.Description);
            }

            return result;
        }

        /// <summary>
        /// Obtiene los miembros de un comité con paginación y filtro.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">JSON con comité seleccionado, filtro y paginación.</param>
        /// <returns>Lista de miembros y total.</returns>
        public ErrorDto<FslMiembrosComitesDataLista> FslMiembrosComite_Obtener(int CodCliente, string filtros)
        {
            var filtro = JsonConvert.DeserializeObject<FslComitefiltros>(filtros) ?? new FslComitefiltros();

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new FslMiembrosComitesDataLista();

                const string sqlCount = "SELECT COUNT(*) FROM FSL_COMITES_MIEMBROS";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount);

                var like = string.IsNullOrWhiteSpace(filtro.filtro) ? null : $"%{filtro.filtro}%";
                var offset = filtro.pagina ?? 0;
                var fetch = filtro.paginacion ?? 10;

                const string sql = @"SELECT CEDULA AS cedula, COD_COMITE AS cod_comite, NOMBRE AS nombre,
                                            USUARIO_VINCULADO AS usuario_Vinculado, REGISTRO_FECHA AS registro_Fecha,
                                            REGISTRO_USUARIO AS registro_Usuario, SALIDA_FECHA AS salida_Fecha, ACTIVO AS activo
                                     FROM FSL_COMITES_MIEMBROS
                                     WHERE COD_COMITE = @comiteSeleccionado
                                       AND (@like IS NULL OR COD_COMITE LIKE @like OR NOMBRE LIKE @like)
                                     ORDER BY COD_COMITE
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.Miembros = connection.Query<FslMiembrosComitesDto>(sql,
                    new { comiteSeleccionado = filtro.comiteSeleccionado, like, offset, fetch }).ToList();
                return response;
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<FslMiembrosComitesDataLista>("FslMiembrosComite_Obtener - " + result.Description);
            }

            return result;
        }
    }
}
