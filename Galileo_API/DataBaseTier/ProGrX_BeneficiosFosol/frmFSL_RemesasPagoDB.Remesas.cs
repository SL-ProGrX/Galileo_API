using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public partial class FrmFslRemesasPagoDB
    {
        /// <summary>
        /// Inserta una remesa de tesorería calculando su consecutivo.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="remesa">Datos de la remesa.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FslRemesa_Agregar(int CodEmpresa, FslRemesaInsertar remesa)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);
            try
            {
                var consecutivo = connection.QueryFirstOrDefault<int>(
                    "SELECT COALESCE(MAX(TESORERIA_REMESA), 0) + 1 AS Ultimo FROM FSL_REMESAS_TESORERIA");

                const string sql = @"INSERT FSL_REMESAS_TESORERIA
                                        (TESORERIA_REMESA, registro_usuario, registro_fecha, estado, fecha_inicio, fecha_corte, notas)
                                     VALUES
                                        (@consecutivo, @usuario, GETDATE(), 'A', @fecha_inicio, @fecha_corte, @notas)";
                connection.Execute(sql, new { consecutivo, remesa.usuario, remesa.fecha_inicio, remesa.fecha_corte, remesa.notas });

                return DbHelper.OkResponse("Remesa agregada correctamente");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza una remesa; si es nueva (cod_remesa = 0), delega en el alta.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="remesa">Datos de la remesa.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FslRemesa_Actualizar(int CodEmpresa, FslRemesaInsertar remesa)
        {
            if (remesa.cod_remesa == 0)
            {
                return FslRemesa_Agregar(CodEmpresa, remesa);
            }

            const string sql = @"UPDATE FSL_REMESAS_TESORERIA
                                 SET fecha_inicio = @fecha_inicio, fecha_corte = @fecha_corte, notas = @notas
                                 WHERE TESORERIA_REMESA = @cod_remesa";

            var result = DbHelper.ExecuteNonQuery(CreatePortalDb(), CodEmpresa, sql,
                new { remesa.fecha_inicio, remesa.fecha_corte, remesa.notas, remesa.cod_remesa });

            if (result.Code == 0)
            {
                result.Description = "Remesa actualizada correctamente";
            }

            return result;
        }

        /// <summary>
        /// Elimina una remesa de tesorería.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="cod_remesa">Código de la remesa.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FslRemesa_Eliminar(int CodEmpresa, int cod_remesa)
        {
            const string sql = "DELETE FROM FSL_REMESAS_TESORERIA WHERE TESORERIA_REMESA = @cod_remesa";

            var result = DbHelper.ExecuteNonQuery(CreatePortalDb(), CodEmpresa, sql, new { cod_remesa });

            if (result.Code == 0)
            {
                result.Description = "Remesa eliminada correctamente";
            }

            return result;
        }

        /// <summary>
        /// Cierra una remesa de tesorería (estado 'C'), validando que esté abierta.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="cod_remesa">Código de la remesa.</param>
        /// <param name="usuario">Usuario que cierra.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FslRemesa_Cerrar(int CodEmpresa, int cod_remesa, string usuario)
        {
            return FslRemesa_CerrarInterno(CodEmpresa, cod_remesa, usuario);
        }

        /// <summary>
        /// Ejecuta el cierre compartido de una remesa abierta y registra el movimiento.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="cod_remesa">Código de la remesa.</param>
        /// <param name="usuario">Usuario que cierra.</param>
        /// <returns>Resultado de la operación.</returns>
        private ErrorDto FslRemesa_CerrarInterno(int CodEmpresa, int cod_remesa, string usuario)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);
            try
            {
                var abierta = connection.QueryFirstOrDefault<int>(
                    "SELECT COUNT(*) AS Existe FROM FSL_REMESAS_TESORERIA WHERE TESORERIA_REMESA = @cod_remesa AND estado = 'A'",
                    new { cod_remesa });

                if (abierta == 0)
                {
                    return DbHelper.ErrorResponse("La Remesa actual; ya se encuentra cerrada...");
                }

                connection.Execute("UPDATE FSL_REMESAS_TESORERIA SET estado = 'C' WHERE TESORERIA_REMESA = @cod_remesa", new { cod_remesa });

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario.ToUpper(),
                    DetalleMovimiento = "Cierra Remesa Traslado a Tesoreria :" + cod_remesa,
                    Movimiento = "APLICA - WEB",
                    Modulo = 7
                });

                return DbHelper.OkResponse("Remesa cerrada correctamente");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
    }
}
