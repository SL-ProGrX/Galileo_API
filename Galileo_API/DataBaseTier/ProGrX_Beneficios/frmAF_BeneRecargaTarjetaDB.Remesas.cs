using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneRecargaTarjetaDB
    {
        /// <summary>
        /// Inserta una remesa de tarjetas.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="remesa">Datos de la remesa.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfiTarjetasRemesa_Insertar(int CodCliente, AfiBeneTarjetasRemesasData remesa)
        {
            var fecha_inicio = MProGrXAuxiliarDB.validaFechaGlobal(remesa.fecha_inicio, FechaFormat);
            var fecha_corte = MProGrXAuxiliarDB.validaFechaGlobal(remesa.fecha_corte, FechaFormat);

            const string sql = @"INSERT INTO AFI_BENE_TARJETAS_REMESAS
                                    (registro_usuario, registro_fecha, estado, fecha_inicio, fecha_corte, notas)
                                 VALUES
                                    (@registro_usuario, GETDATE(), 'A', @fecha_inicio, @fecha_corte, @notas)";

            var result = DbHelper.ExecuteNonQueryWithResult(CreatePortalDb(), CodCliente, sql, new
            {
                remesa.registro_usuario,
                fecha_inicio,
                fecha_corte,
                remesa.notas
            });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description);
            }

            return result.Result > 0
                ? new ErrorDto { Code = 0 }
                : DbHelper.ErrorResponse("Error al actualizar el registro");
        }

        /// <summary>
        /// Actualiza una remesa de tarjetas.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="remesa">Datos de la remesa.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfiTarjetasRemesa_Actualizar(int CodCliente, AfiBeneTarjetasRemesasData remesa)
        {
            var fecha_inicio = MProGrXAuxiliarDB.validaFechaGlobal(remesa.fecha_inicio, FechaFormat);
            var fecha_corte = MProGrXAuxiliarDB.validaFechaGlobal(remesa.fecha_corte, FechaFormat);

            const string sql = @"UPDATE AFI_BENE_TARJETAS_REMESAS
                                 SET estado = @estado, fecha_inicio = @fecha_inicio, fecha_corte = @fecha_corte, notas = @notas
                                 WHERE cod_remesa_tr = @cod_remesa_tr";

            var result = DbHelper.ExecuteNonQueryWithResult(CreatePortalDb(), CodCliente, sql, new
            {
                remesa.estado,
                fecha_inicio,
                fecha_corte,
                remesa.notas,
                remesa.cod_remesa_tr
            });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description);
            }

            return result.Result > 0
                ? new ErrorDto { Code = 0 }
                : DbHelper.ErrorResponse("Error al actualizar el registro");
        }

        /// <summary>
        /// Elimina una remesa de tarjetas y libera los pagos asociados.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_remesa">Código de la remesa.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfiTarjetasRemesa_Eliminar(int CodCliente, long cod_remesa)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                const string sqlLiberar = @"UPDATE afi_bene_pago SET COD_REMESA = NULL
                                            WHERE TIPO = 'P'
                                              AND COD_PRODUCTO IN (SELECT cod_producto FROM AFI_BENE_TARJETAS_REGALO)
                                              AND COD_REMESA = @cod_remesa";
                connection.Execute(sqlLiberar, new { cod_remesa });

                const string sqlDelete = "DELETE FROM AFI_BENE_TARJETAS_REMESAS WHERE COD_REMESA_TR = @cod_remesa";
                connection.Execute(sqlDelete, new { cod_remesa });

                return new ErrorDto { Code = 0 };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
    }
}
