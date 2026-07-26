using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public partial class FrmCxcCuentasDB
    {
        #region Anular

        /// <summary>
        /// Verifica si una operación de CxC puede anularse.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="request">Datos mínimos requeridos para validar la anulación.</param>
        /// <returns>Resultado de validación de anulación.</returns>
        public ErrorDto<CxCCuentasAnulacionVerificaResult> CxCCuentasAnulacion_Verifica(
            int codEmpresa,
            CxCCuentasAnulacionRequest request)
        {

            if (request is null)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasAnulacionVerificaResult>(CxCCuentasConstantes.solicitudRequerida);
            }

            if (request.operacion <= 0)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasAnulacionVerificaResult>(CxCCuentasConstantes.operacionRequerida);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                const string sql = @"
                    SELECT TOP 1
                        ISNULL(Estado, '') AS estado
                    FROM CxC_Cuentas
                    WHERE Operacion = @operacion;";

                var estado = conn.QueryFirstOrDefault<string>(sql, new
                {
                    operacion = request.operacion
                });

                if (string.IsNullOrWhiteSpace(estado))
                {
                    return DbHelper.CreateErrorResponse<CxCCuentasAnulacionVerificaResult>("No se encontró la operación indicada.");
                }

                var pass = estado.Trim().ToUpperInvariant() == "A";

                return DbHelper.CreateOkResponse<CxCCuentasAnulacionVerificaResult>(
                    new CxCCuentasAnulacionVerificaResult
                    {
                        pass = pass,
                        mensaje = pass ? string.Empty : "Solo pueden anularse operaciones activas."
                    }
                    ); 
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasAnulacionVerificaResult>(
                    $"Error inesperado al verificar la anulación. {ex.Message}");
            }
        }

        /// <summary>
        /// Anula una operación de CxC.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="request">Datos requeridos para anular la operación.</param>
        /// <returns>Resultado del proceso de anulación.</returns>
        public ErrorDto<bool> CxCCuentasAnulacion_Anular(
            int codEmpresa,
            CxCCuentasAnulacionRequest request)
        {
            var response = DbHelper.CreateOkResponse<bool>();

            if (request is null)
            {
                return DbHelper.CreateErrorResponse<bool>(CxCCuentasConstantes.solicitudRequerida);
            }

            if (request.operacion <= 0)
            {
                return DbHelper.CreateErrorResponse<bool>(CxCCuentasConstantes.operacionRequerida);
            }

            var usuario = NormalizarTexto(request.usuario);
            var notas = string.IsNullOrWhiteSpace(request.notas) ? string.Empty : request.notas.Trim();

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.CreateErrorResponse<bool>(CxCCuentasConstantes.usuarioRequerido);
            }

            var verifica = CxCCuentasAnulacion_Verifica(codEmpresa, request);
            if (verifica.Code == -1 || verifica.Result is null || !verifica.Result.pass)
            {
                return DbHelper.CreateErrorResponse<bool>(verifica.Result?.mensaje ?? verifica.Description);
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                const string sql = @"
                    exec spCxC_Cuenta_Anulacion @operacion, @usuario, @notas;";

                conn.Execute(sql, new
                {
                    operacion = request.operacion,
                    usuario,
                    notas
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<bool>($"Error inesperado al anular la operación. {ex.Message}");
            }

            return response;
        }

        #endregion
    }
}
