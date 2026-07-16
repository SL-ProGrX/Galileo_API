using System.Text;
using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosIntegralOrPDB
    {
        /// <summary>
        /// Agrega una orden de pago tras aplicar las validaciones de socio, beneficio y justificación.
        /// </summary>
        public ErrorDto AfiBeneficioIntegralOrdenPago_Agregar(int CodCliente, AfiBeneIntegralOrP beneficio)
        {
            var estadoSocio = _mBeneficiosDB.ValidaEstadoSocio(CodCliente, beneficio.cedula.Trim());
            if (estadoSocio.Code == -1)
            {
                return new ErrorDto { Code = estadoSocio.Code, Description = estadoSocio.Description };
            }

            var beneficioValida = MapBeneficioValida(beneficio);
            beneficioValida.id_beneficio = beneficio.id_beneficio ?? 0;

            var validaPago = _mBeneficiosDB.ValidarBeneficioPagoDato(CodCliente, beneficioValida);
            if (validaPago.Code == -1)
            {
                return validaPago;
            }

            try
            {
                var justifica = ObtenerRequiereJustificacion(CodCliente, beneficio.id_beneficio ?? 0);

                var validaJustifica = _mBeneficiosDB.ValidarBeneficioPagoJustificaDato(CodCliente, beneficioValida, justifica);
                if (validaJustifica.Code == -1)
                {
                    return validaJustifica;
                }

                return InsertarOrdenPago(CodCliente, beneficio);
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        /// <summary>
        /// Actualiza una orden de pago existente si aún no ha sido procesada (estado 'S').
        /// </summary>
        public ErrorDto AfiBeneficioIntegralOrdenPago_Actualizar(int CodCliente, AfiBeneIntegralOrP beneficio)
        {
            var estadoSocio = _mBeneficiosDB.ValidaEstadoSocio(CodCliente, beneficio.cedula.Trim());
            if (estadoSocio.Code == -1)
            {
                return new ErrorDto { Code = estadoSocio.Code, Description = estadoSocio.Description };
            }

            var beneficioValida = MapBeneficioValida(beneficio);

            var validaBene = _mBeneficiosDB.ValidarBeneficioDato(CodCliente, beneficioValida);
            if (validaBene.Code == -1)
            {
                return validaBene;
            }

            try
            {
                return ActualizarOrdenPago(CodCliente, beneficio);
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        /// <summary>
        /// Consulta si el beneficio requiere justificación.
        /// </summary>
        private bool ObtenerRequiereJustificacion(int CodCliente, long idBeneficio)
        {
            const string sql = "SELECT requiere_justificacion FROM afi_bene_otorga WHERE id_beneficio = @idBeneficio";
            var result = DbHelper.ExecuteSingleQuery<bool>(CreatePortalDb(), CodCliente, sql, false, new { idBeneficio });
            return result.Result;
        }

        /// <summary>
        /// Inserta el registro en afi_bene_pago y deja traza en bitácora.
        /// </summary>
        private ErrorDto InsertarOrdenPago(int CodCliente, AfiBeneIntegralOrP beneficio)
        {
            const string sqlInsert = @"
                INSERT afi_bene_pago(cedula,consec,cod_beneficio,tipo,monto,cod_banco,tipo_emision,cta_bancaria,estado,
                                     t_identificacion,t_beneficiario,t_email,registro_fecha,registro_usuario,cod_producto)
                VALUES(@cedula,@consec,@codBeneficio,@tipo,@monto,@codBanco,@tipoEmision,@ctaBancaria,'S',
                       @tIdentificacion,@tBeneficiario,@tEmail,GETDATE(),@registroUsuario,@codProducto)";

            const string sqlIdPago = @"SELECT TOP 1 ID_PAGO FROM afi_bene_pago
                                        WHERE cedula = @cedula AND consec = @consec AND COD_BENEFICIO = @codBeneficio
                                        ORDER BY ID_PAGO DESC";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var filas = connection.Execute(sqlInsert, new
                {
                    cedula = beneficio.cedula.Trim(),
                    consec = beneficio.consec,
                    codBeneficio = beneficio.cod_beneficio,
                    tipo = beneficio.tipo,
                    monto = beneficio.monto,
                    codBanco = beneficio.cod_banco ?? 0,
                    tipoEmision = beneficio.tipo_emision,
                    ctaBancaria = beneficio.cta_bancaria,
                    tIdentificacion = beneficio.t_identificacion,
                    tBeneficiario = beneficio.t_beneficiario,
                    tEmail = beneficio.t_email,
                    registroUsuario = beneficio.registro_usuario,
                    codProducto = beneficio.cod_producto
                });

                if (filas <= 0)
                {
                    return 0;
                }

                var idPago = connection.QueryFirstOrDefault<int>(sqlIdPago, new
                {
                    cedula = beneficio.cedula,
                    consec = beneficio.consec,
                    codBeneficio = beneficio.cod_beneficio
                });

                RegistrarBitacora(CodCliente, beneficio.cod_beneficio, beneficio.consec,
                    beneficio.registro_usuario, "Inserta", $"Ingresa Orden de Pago COD: [{idPago}]");

                return filas;
            });

            if (result.Code != 0)
            {
                return new ErrorDto { Code = -1, Description = result.Description };
            }

            return result.Result > 0
                ? new ErrorDto { Code = 0, Description = "Orden de pago cargada exitosamente" }
                : new ErrorDto { Code = 0, Description = string.Empty };
        }

        /// <summary>
        /// Actualiza el registro en afi_bene_pago (solo si estado = 'S') y deja traza en bitácora.
        /// </summary>
        private ErrorDto ActualizarOrdenPago(int CodCliente, AfiBeneIntegralOrP beneficio)
        {
            var extra = new StringBuilder();
            if (!string.IsNullOrEmpty(beneficio.cod_producto)) extra.Append(", cod_producto = @codProducto");
            if (!string.IsNullOrEmpty(beneficio.cta_bancaria)) extra.Append(", cta_bancaria = @ctaBancaria");
            if (beneficio.cod_banco != null) extra.Append(", cod_banco = @codBanco");

            const string sqlEstado = @"SELECT estado FROM afi_bene_pago
                                        WHERE cedula = @cedula AND consec = @consec AND cod_beneficio = @codBeneficio";

            var sqlUpdate = $@"
                UPDATE afi_bene_pago
                   SET tipo = @tipo, monto = @monto, tipo_emision = @tipoEmision,
                       t_identificacion = @tIdentificacion, t_beneficiario = @tBeneficiario, t_email = @tEmail {extra}
                 WHERE cedula = @cedula AND consec = @consec AND cod_beneficio = @codBeneficio";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var parametros = new
                {
                    cedula = beneficio.cedula,
                    consec = beneficio.consec,
                    codBeneficio = beneficio.cod_beneficio,
                    tipo = beneficio.tipo,
                    monto = beneficio.monto,
                    tipoEmision = beneficio.tipo_emision,
                    tIdentificacion = beneficio.t_identificacion,
                    tBeneficiario = beneficio.t_beneficiario,
                    tEmail = beneficio.t_email,
                    codProducto = beneficio.cod_producto,
                    ctaBancaria = beneficio.cta_bancaria,
                    codBanco = beneficio.cod_banco
                };

                var estadoValido = connection.QueryFirstOrDefault<string>(sqlEstado, parametros);
                if (estadoValido == null || estadoValido.Trim() != "S")
                {
                    return -1;
                }

                var filas = connection.Execute(sqlUpdate, parametros);
                if (filas > 0)
                {
                    RegistrarBitacora(CodCliente, beneficio.cod_beneficio, beneficio.consec,
                        beneficio.registro_usuario, "Actualiza", $"Actualiza Orden de Pago COD: [{beneficio.id_pago}]");
                }

                return filas;
            });

            if (result.Code != 0)
            {
                return new ErrorDto { Code = -1, Description = result.Description };
            }

            if (result.Result == -1)
            {
                return new ErrorDto { Code = -1, Description = "No se permite modificar la orden de pago porque ya se encuentra procesada" };
            }

            return new ErrorDto
            {
                Code = 0,
                Description = result.Result > 0 ? "Orden de pago actualizada exitosamente" : string.Empty
            };
        }
    }
}
