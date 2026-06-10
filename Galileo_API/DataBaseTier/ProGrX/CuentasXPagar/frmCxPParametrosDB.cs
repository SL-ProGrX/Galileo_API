using Galileo.Models.CxP;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public class FrmCxPParametrosDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmCxPParametrosDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmCxPParametrosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Ejecuta el procedimiento almacenado que inicializa o recalcula los parámetros de cuentas por pagar.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto ExecParametros(int CodCliente)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodCliente,
                "spCxPParametros");

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al ejecutar parámetros de CxP.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Obtiene el listado de parámetros de cuentas por pagar.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <returns>Listado de parámetros configurados.</returns>
        public ErrorDto<List<ParametrosDto>> ObtenerParametros(int CodCliente)
        {
            return DbHelper.ExecuteListQuery<ParametrosDto>(
                CreatePortalDb(),
                CodCliente,
                "select * from cxp_parametros order by cod_parametro");
        }

        /// <summary>
        /// Actualiza el valor de un parámetro y registra el usuario y fecha de modificación.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="Usuario">Usuario que realiza la modificación.</param>
        /// <param name="Valor">Nuevo valor del parámetro.</param>
        /// <param name="Parametro">Código del parámetro a actualizar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto ActualizarDatosParametro(int CodCliente, string Usuario, string Valor, string Parametro)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodCliente,
                "update cxp_parametros set modifica_usuario = @Usuario, modifica_Fecha = Getdate(), valor = @Valor where cod_parametro = @Parametro",
                new
                {
                    Usuario,
                    Valor,
                    Parametro
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Parámetro actualizado correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar parámetro.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);
    }
}