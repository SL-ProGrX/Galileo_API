using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosTrasladoDB
    {
        /// <summary>
        /// Inserta una nueva remesa de traslado y deja traza en bitácora.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="remesa">Datos de la remesa.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfiRemesa_Insertar(int CodCliente, AfiBeneficiosRemesasDto remesa)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                var ultimo = connection.QueryFirstOrDefault<int>("SELECT ISNULL(MAX(cod_remesa), 0) + 1 FROM AFI_BENEFICIOS_REMESAS");

                var fecha_inicio = MProGrXAuxiliarDB.validaFechaGlobal(remesa.fecha_inicio, FechaFormat);
                var fecha_corte = MProGrXAuxiliarDB.validaFechaGlobal(remesa.fecha_corte, FechaFormat);

                const string sql = @"INSERT INTO AFI_BENEFICIOS_REMESAS (cod_remesa, usuario, fecha, estado, fecha_inicio, fecha_corte, notas)
                                     VALUES (@ultimo, @usuario, GETDATE(), 'A', @fecha_inicio, @fecha_corte, @notas)";
                var resp = connection.Execute(sql, new { ultimo, remesa.usuario, fecha_inicio, fecha_corte, remesa.notas });

                if (resp <= 0)
                {
                    return DbHelper.ErrorResponse("Error al actualizar el registro");
                }

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodCliente,
                    Usuario = remesa.usuario.ToUpper(),
                    DetalleMovimiento = $"Registra, Remesa de Beneficios Traslado a Tesoreria: {remesa.cod_remesa} ",
                    Movimiento = "REGISTRA - WEB",
                    Modulo = 7
                });

                return new ErrorDto { Code = 0 };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza una remesa de traslado y deja traza en bitácora.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="remesa">Datos de la remesa.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfiRemesa_Actualizar(int CodCliente, AfiBeneficiosRemesasDto remesa)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                var fecha_inicio = MProGrXAuxiliarDB.validaFechaGlobal(remesa.fecha_inicio, FechaFormat);
                var fecha_corte = MProGrXAuxiliarDB.validaFechaGlobal(remesa.fecha_corte, FechaFormat);

                const string sql = @"UPDATE AFI_BENEFICIOS_REMESAS
                                     SET usuario = @usuario, fecha_inicio = @fecha_inicio, fecha_corte = @fecha_corte, notas = @notas
                                     WHERE cod_remesa = @cod_remesa";
                var resp = connection.Execute(sql, new { remesa.usuario, fecha_inicio, fecha_corte, remesa.notas, remesa.cod_remesa });

                if (resp <= 0)
                {
                    return DbHelper.ErrorResponse("Error al actualizar el registro");
                }

                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodCliente,
                    Usuario = remesa.usuario.ToUpper(),
                    DetalleMovimiento = $"Modifica, Remesa de Beneficios Traslado a Tesoreria: {remesa.cod_remesa} ",
                    Movimiento = "Modifica - WEB",
                    Modulo = 7
                });

                return new ErrorDto { Code = 0 };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Elimina una remesa de traslado y libera los otorgamientos asociados.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_remesa">Código de la remesa.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfiRemesa_Eliminar(int CodCliente, long cod_remesa)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                connection.Execute("UPDATE afi_bene_otorga SET COD_REMESA = NULL WHERE Cod_Remesa = @cod_remesa", new { cod_remesa });
                connection.Execute("DELETE FROM AFI_BENEFICIOS_REMESAS WHERE Cod_Remesa = @cod_remesa", new { cod_remesa });
                return new ErrorDto { Code = 0 };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
    }
}
