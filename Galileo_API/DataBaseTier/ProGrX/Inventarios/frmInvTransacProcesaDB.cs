using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmInvTransacProcesaDB
    {
        private readonly IConfiguration _config;

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvTransacProcesaDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvTransacProcesaDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        #endregion

        #region Mantenimiento

        /// <summary>
        /// Ejecuta el proceso de inventarios utilizando el procedimiento almacenado <c>spINVTranProcesa</c>.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos requeridos para procesar la transacción.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto InvTransacProcesa_SP(int CodEmpresa, InvTransacProcesa request)
        {
            var result = DbHelper.WithConn<int>(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(
                    "spINVTranProcesa",
                    new
                    {
                        Tipo = request.Tipo,
                        Boleta = request.Boleta,
                        Usuario = request.Usuario
                    },
                    commandType: CommandType.StoredProcedure);

                return 0;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Procesamiento Finalizado Satisfactoriamente...")
                : DbHelper.ErrorResponse(result.Description ?? "Error al procesar la transacción de inventario.", result.Code.GetValueOrDefault(-1));
        }

        #endregion
    }
}