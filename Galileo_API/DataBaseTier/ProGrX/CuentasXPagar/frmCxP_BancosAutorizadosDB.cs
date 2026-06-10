using Galileo.Models.CxP;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public class FrmCxPBancosAutorizadosDB
    {
        private readonly IConfiguration _config;

        public FrmCxPBancosAutorizadosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene el listado de bancos autorizados para cuentas por pagar.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <returns>Lista de bancos autorizados.</returns>
        public ErrorDto<List<BancosAutorizadosDto>> ObtenerBancosAutorizados(int CodCliente)
        {
            return DbHelper.ExecuteListQuery<BancosAutorizadosDto>(
                CreatePortalDb(),
                CodCliente,
                "spObtenerTodosBancosAutorizados");
        }

        /// <summary>
        /// Inserta en la tabla de bancos autorizados los bancos de tesorería que aún no han sido registrados.
        /// </summary>
        /// <param name="Usuario">Usuario que registra la operación.</param>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto IngresarTesBancosNuevos(string Usuario, int CodCliente)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodCliente,
                @"insert into CxP_Bancos_Autorizados(id_banco, cheques, transferencias, registro_fecha, registro_usuario)
                  select id_banco, 0, 0, dbo.MyGetdate(), @Usuario
                  from Tes_Bancos
                  where id_banco not in (select id_banco from CxP_Bancos_Autorizados)",
                new { Usuario });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al ingresar bancos nuevos de tesorería.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Actualiza la autorización de transferencias para un banco.
        /// </summary>
        /// <param name="BancoId">Identificador del banco.</param>
        /// <param name="Valor">Valor de autorización a aplicar.</param>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto ActualizarTransferencia(int BancoId, bool Valor, int CodCliente)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodCliente,
                "spActualizarBancoAutorizacionTransferencia",
                new
                {
                    Valor,
                    BancoId
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar autorización de transferencia.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Actualiza la autorización de cheques para un banco.
        /// </summary>
        /// <param name="BancoId">Identificador del banco.</param>
        /// <param name="Valor">Valor de autorización a aplicar.</param>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto ActualizarCheque(int BancoId, bool Valor, int CodCliente)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodCliente,
                "spActualizarBancoAutorizacionCheques",
                new
                {
                    Valor,
                    BancoId
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar autorización de cheques.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);
    }
}