using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Acceso a datos de las Gestiones de Expediente Fosol (frmFSL_ExpedienteGestiones).
    /// </summary>
    public partial class FrmFslExpedienteGestionesDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmFslExpedienteGestionesDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene los tipos de gestión activos.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de tipos de gestión.</returns>
        public ErrorDto<List<FslGestionesListaDatos>> FslGestiones_Obtener(int CodCliente)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT cod_gestion AS item, RTRIM(cod_gestion) + ' - ' + DESCRIPCION AS descripcion
                                     FROM FSL_TIPOS_GESTIONES WHERE ACTIVA = 1";
                return connection.Query<FslGestionesListaDatos>(sql).ToList();
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<List<FslGestionesListaDatos>>("FslGestiones_Obtener - " + result.Description);
            }

            return result;
        }

        /// <summary>
        /// Registra una gestión de expediente mediante SP.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="gestion">Datos de la gestión.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FslGestion_Agregar(int CodCliente, FslGestionAgregar gestion)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                connection.Execute("[spFSL_GestionRegistra]", new
                {
                    Expediente = gestion.cod_expediente,
                    Tipo = gestion.cod_gestion,
                    Notas = gestion.notas,
                    Usuario = gestion.usuario
                }, commandType: CommandType.StoredProcedure);

                return new ErrorDto { Code = 0 };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
    }
}
