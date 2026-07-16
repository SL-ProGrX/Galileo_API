using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosIntegralOrPDB
    {
        /// <summary>
        /// Inserta una proyección de pago y deja traza en bitácora.
        /// </summary>
        public ErrorDto AfiBeneficioIntegralProyeccionPago_Insertar(int CodCliente, AfiBenePagoProyecta beneficio)
        {
            const string sqlInsert = @"
                INSERT AFI_BENE_PAGO_PROYECTA(cedula,consec,cod_beneficio,tipo,fecha_vence,monto,cod_banco,tipo_emision,
                                              cta_bancaria,estado,activa_usuario,activa_fecha,t_identificacion,t_beneficiario,
                                              t_email,registro_fecha,registro_usuario,cod_producto)
                VALUES(@cedula,@consec,@codBeneficio,@tipo,@fechaVence,@monto,@codBanco,@tipoEmision,
                       @ctaBancaria,'P',@activaUsuario,GETDATE(),@tIdentificacion,@tBeneficiario,
                       @tEmail,GETDATE(),@registroUsuario,@codProducto)";

            const string sqlIdPlan = @"SELECT TOP 1 PLAN_ID FROM AFI_BENE_PAGO_PROYECTA
                                        WHERE cedula = @cedula AND consec = @consec AND COD_BENEFICIO = @codBeneficio
                                        ORDER BY PLAN_ID DESC";

            try
            {
                var cedulaLimpia = beneficio.cedula.Trim().Replace("-", "");
                var tIdentLimpia = (beneficio.t_identificacion ?? string.Empty).Trim().Replace("-", "");

                var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                {
                    connection.Execute(sqlInsert, new
                    {
                        cedula = cedulaLimpia,
                        consec = beneficio.consec,
                        codBeneficio = beneficio.cod_beneficio,
                        tipo = beneficio.tipo,
                        fechaVence = beneficio.fecha_vence,
                        monto = beneficio.monto,
                        codBanco = beneficio.cod_banco ?? 0,
                        tipoEmision = beneficio.tipo_emision,
                        ctaBancaria = beneficio.cta_bancaria,
                        activaUsuario = beneficio.activa_usuario,
                        tIdentificacion = tIdentLimpia,
                        tBeneficiario = beneficio.t_beneficiario,
                        tEmail = beneficio.t_email,
                        registroUsuario = beneficio.registro_usuario,
                        codProducto = beneficio.cod_producto
                    });

                    var idPlan = connection.QueryFirstOrDefault<int>(sqlIdPlan, new
                    {
                        cedula = cedulaLimpia,
                        consec = beneficio.consec,
                        codBeneficio = beneficio.cod_beneficio
                    });

                    RegistrarBitacora(CodCliente, beneficio.cod_beneficio, beneficio.consec,
                        beneficio.registro_usuario, "Inserta", $"Inserta Proyección de Pago COD: [{idPlan}]");

                    return idPlan;
                });

                return new ErrorDto
                {
                    Code = result.Code,
                    Description = result.Code == 0 ? "Proyección de pago cargada exitosamente" : result.Description
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        /// <summary>
        /// Actualiza una proyección de pago pendiente (estado 'P') y deja traza en bitácora.
        /// </summary>
        public ErrorDto AfiBeneficioIntegralProyeccionPago_Actualizar(int CodCliente, AfiBenePagoProyecta beneficio)
        {
            const string sqlUpdate = @"
                UPDATE AFI_BENE_PAGO_PROYECTA
                   SET fecha_vence = @fechaVence, monto = @monto, cod_banco = @codBanco, tipo_emision = @tipoEmision,
                       cta_bancaria = @ctaBancaria, activa_usuario = @activaUsuario, activa_fecha = GETDATE(),
                       t_identificacion = @tIdentificacion, t_beneficiario = @tBeneficiario, t_email = @tEmail,
                       cod_producto = @codProducto, tipo = @tipo
                 WHERE cedula = @cedula AND cod_beneficio = @codBeneficio AND plan_id = @planId AND estado = 'P'";

            try
            {
                var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                {
                    connection.Execute(sqlUpdate, new
                    {
                        fechaVence = beneficio.fecha_vence,
                        monto = beneficio.monto,
                        codBanco = beneficio.cod_banco,
                        tipoEmision = beneficio.tipo_emision,
                        ctaBancaria = beneficio.cta_bancaria,
                        activaUsuario = beneficio.activa_usuario,
                        tIdentificacion = beneficio.t_identificacion,
                        tBeneficiario = beneficio.t_beneficiario,
                        tEmail = beneficio.t_email,
                        codProducto = beneficio.cod_producto,
                        tipo = beneficio.tipo,
                        cedula = beneficio.cedula.Trim(),
                        codBeneficio = beneficio.cod_beneficio,
                        planId = beneficio.plan_id
                    });

                    RegistrarBitacora(CodCliente, beneficio.cod_beneficio, beneficio.consec,
                        beneficio.registro_usuario, "Actualiza", $"Actualiza Proyección de Pago COD: [{beneficio.plan_id}]");

                    return 1;
                });

                return new ErrorDto
                {
                    Code = result.Code,
                    Description = result.Code == 0 ? "Proyección de pago actualizada exitosamente" : result.Description
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        /// <summary>
        /// Elimina una proyección de pago pendiente (estado 'P').
        /// </summary>
        public ErrorDto AfiBeneficioIntegralProyeccionPago_Eliminar(int CodCliente, int Plan_Id)
        {
            const string sql = "DELETE FROM AFI_BENE_PAGO_PROYECTA WHERE Plan_Id = @planId AND estado = 'P'";

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                connection.Execute(sql, new { planId = Plan_Id }));

            if (result.Code != 0)
            {
                return new ErrorDto { Code = -1, Description = result.Description };
            }

            return new ErrorDto
            {
                Code = 0,
                Description = result.Result > 0 ? "Proyección de pago eliminada exitosamente" : "No se encontraron resultados"
            };
        }
    }
}
