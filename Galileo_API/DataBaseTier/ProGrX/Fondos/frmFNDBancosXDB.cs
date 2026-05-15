using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndBancosXDb
    {
        private readonly IConfiguration _config;

        public FrmFndBancosXDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene la lista de bancos con sus flags de cheque y transferencia.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>ErrorDto con la lista de bancos.</returns>
        public ErrorDto<List<FndBancosXModel>> BancosX_Obtener(int codEmpresa)
        {
            const string query = @"
                    SELECT
                        X.id_banco,
                        B.descripcion,
                        X.cheque,
                        X.transferencia
                    FROM dbo.Fnd_Bancos_X X
                    INNER JOIN dbo.Tes_Bancos B
                        ON X.id_banco = B.id_Banco
                    ORDER BY B.id_banco;";

            return DbHelper.ExecuteListQuery<FndBancosXModel>(new PortalDB(_config), codEmpresa, query);
        }

        /// <summary>
        /// Inserta los bancos que no existen en Fnd_Bancos_X.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>ErrorDto con el resultado de la operación.</returns>
        public ErrorDto BancosX_Insertar(int codEmpresa)
        {
            const string query = @"
                    INSERT INTO dbo.Fnd_Bancos_X
                    (
                        id_banco,
                        cheque,
                        transferencia
                    )
                    SELECT
                        id_banco,
                        0,
                        0
                    FROM dbo.Tes_Bancos
                    WHERE id_Banco NOT IN
                    (
                        SELECT id_Banco
                        FROM dbo.Fnd_Bancos_X
                    );";

            return DbHelper.ExecuteNonQuery(new PortalDB(_config), codEmpresa, query);
        }

        /// <summary>
        /// Actualiza el valor de cheque o transferencia para un banco específico.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de actualización.</param>
        /// <returns>ErrorDto con el resultado de la operación.</returns>
        public ErrorDto BancosX_Actualizar(int codEmpresa, FndBancosXUpdateParam param)
        {
            if (param is null)
            {
                return DbHelper.ErrorResponse("Los parámetros de actualización son requeridos.", -2);
            }

            return NormalizarCampo(param.Campo) switch
            {
                "cheque" => ActualizarCheque(codEmpresa, param),
                "transferencia" => ActualizarTransferencia(codEmpresa, param),
                _ => DbHelper.ErrorResponse("Campo de actualización inválido.", -2)
            };
        }

        /// <summary>
        /// Actualiza únicamente el flag de cheque.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de actualización.</param>
        /// <returns>ErrorDto con el resultado de la operación.</returns>
        private ErrorDto ActualizarCheque(int codEmpresa, FndBancosXUpdateParam param)
        {
            const string query = @"
                    UPDATE dbo.Fnd_Bancos_X
                    SET cheque = @Valor
                    WHERE id_Banco = @IdBanco;";

            return EjecutarActualizacionBanco(codEmpresa, query, param);
        }

        /// <summary>
        /// Actualiza únicamente el flag de transferencia.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de actualización.</param>
        /// <returns>ErrorDto con el resultado de la operación.</returns>
        private ErrorDto ActualizarTransferencia(int codEmpresa, FndBancosXUpdateParam param)
        {
            const string query = @"
                    UPDATE dbo.Fnd_Bancos_X
                    SET transferencia = @Valor
                    WHERE id_Banco = @IdBanco;";

            return EjecutarActualizacionBanco(codEmpresa, query, param);
        }

        /// <summary>
        /// Ejecuta una actualización parametrizada sobre Fnd_Bancos_X.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="query">Consulta SQL fija permitida.</param>
        /// <param name="param">Parámetros de actualización.</param>
        /// <returns>ErrorDto con el resultado de la operación.</returns>
        private ErrorDto EjecutarActualizacionBanco(int codEmpresa, string query, FndBancosXUpdateParam param)
        {
            return DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                codEmpresa,
                query,
                new { param.Valor, param.IdBanco });
        }

        /// <summary>
        /// Normaliza el nombre del campo solicitado.
        /// </summary>
        /// <param name="campo">Nombre del campo recibido.</param>
        /// <returns>Campo normalizado.</returns>
        private static string NormalizarCampo(string? campo) => (campo ?? string.Empty).Trim().ToLowerInvariant();
    }
}
