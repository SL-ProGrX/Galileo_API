using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmInvTransacAutorizaDB
    {
        private readonly IConfiguration _config;

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvTransacAutorizaDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvTransacAutorizaDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Crea una respuesta estándar para operaciones no query.
        /// </summary>
        /// <param name="result">Resultado devuelto por <see cref="DbHelper"/>.</param>
        /// <param name="successMessage">Mensaje de éxito.</param>
        /// <param name="errorMessage">Mensaje de error.</param>
        /// <returns>Respuesta estándar para operaciones no query.</returns>
        private static ErrorDto CrearRespuestaNonQuery(ErrorDto result, string successMessage, string errorMessage)
        {
            return result.Code == 0
                ? DbHelper.OkResponse(successMessage)
                : DbHelper.ErrorResponse(result.Description ?? errorMessage, result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Valida si la transacción es mancomunada para el tipo indicado.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="tipo">Tipo de transacción.</param>
        /// <returns><c>true</c> si es mancomunada; en caso contrario, <c>false</c>.</returns>
        private static bool EsTransaccionMancomunada(IDbConnection connection, string tipo)
        {
            return connection.ExecuteScalar<int?>(
                @"SELECT TOP 1 1 
                  FROM pv_entrada_salida 
                  WHERE MANCOMUNADO = 1
                    AND COD_ENTSAL = @tipo",
                new { tipo }) == 1;
        }

        /// <summary>
        /// Valida si el usuario coincide con el usuario generador de requisiciones.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="autorizaUser">Usuario que autoriza.</param>
        /// <returns><c>true</c> si coincide; en caso contrario, <c>false</c>.</returns>
        private static bool UsuarioCoincideEnRequisiciones(IDbConnection connection, string autorizaUser)
        {
            return connection.ExecuteScalar<int?>(
                @"SELECT TOP 1 1 
                  FROM pv_requisiciones 
                  WHERE GENERA_USER = @Autoriza_User",
                new { Autoriza_User = autorizaUser }) == 1;
        }

        #endregion

        #region Mantenimiento

        /// <summary>
        /// Autoriza o actualiza el estado de una transacción de inventario.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos de autorización.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto InvTransacAutoriza_Actualizar(int CodEmpresa, InvTransacAutoriza request)
        {
            var result = DbHelper.WithConn<ErrorDto>(CreatePortalDb(), CodEmpresa, connection =>
            {
                bool esMancomunado = EsTransaccionMancomunada(connection, request.Tipo);
                if (esMancomunado)
                {
                    bool usuarioCoincide = UsuarioCoincideEnRequisiciones(connection, request.Autoriza_User);
                    if (usuarioCoincide)
                    {
                        return new ErrorDto
                        {
                            Code = 2,
                            Description = "El usuario no puede autorizar una transacción mancomunada generada por sí mismo."
                        };
                    }
                }

                connection.Execute(
                    @"update pv_InvTranSac
                      set estado = @Estado,
                          Autoriza_user = @Autoriza_User,
                          autoriza_fecha = getdate()
                      where tipo = @Tipo
                        and Boleta = @Boleta",
                    new
                    {
                        request.Boleta,
                        request.Tipo,
                        request.Autoriza_User,
                        request.Estado
                    });

                return new ErrorDto
                {
                    Code = 0,
                    Description = "Resolución ejecutada satisfactoriamente..."
                };
            });

            return result.Code == 0
                ? result.Result ?? DbHelper.OkResponse("Resolución ejecutada satisfactoriamente...")
                : CrearRespuestaNonQuery(
                    DbHelper.ErrorResponse(result.Description ?? "Error al actualizar la autorización de la transacción.", result.Code.GetValueOrDefault(-1)),
                    "Resolución ejecutada satisfactoriamente...",
                    "Error al actualizar la autorización de la transacción.");
        }

        #endregion
    }
}