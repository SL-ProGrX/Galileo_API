using Dapper;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosIntegralGenDB
    {
        /// <summary>
        /// Guarda el registro de mora del beneficio (inserta o actualiza) y deja traza en bitácora.
        /// </summary>
        public ErrorDto BeneRegistroMora_Guardar(int CodCliente, BeneRegistroMoraGuardar cobroMora)
        {
            const string sqlExiste = @"SELECT COUNT(*) FROM AFI_BENE_REGISTRO_MORA
                                        WHERE COD_BENEFICIO = @codBeneficio AND CONSEC = @consec AND CEDULA = @cedula";

            const string sqlUpdate = @"
                UPDATE AFI_BENE_REGISTRO_MORA
                   SET ACUERDO = @acuerdo, ACUERDO_FECHA = @acuerdoFecha,
                       CANCELACION_MORA = @cancelacionMora, MES_CANCELACION = @mesCancelacion,
                       ADELANTO_CUOTA = @adelantoCuota, MES_ADELANTO = @mesAdelanto,
                       CANCELACION_TOTAL_OPERACION = @cancelacionTotal, NUMERO_OPERACION = @numeroOperacion
                 WHERE COD_BENEFICIO = @codBeneficio AND CONSEC = @consec AND CEDULA = @cedula";

            const string sqlInsert = @"
                INSERT INTO AFI_BENE_REGISTRO_MORA
                    (COD_BENEFICIO, CONSEC, CEDULA, ACUERDO, ACUERDO_FECHA, REGISTRO_FECHA, REGISTRO_USUARIO,
                     CANCELACION_MORA, MES_CANCELACION, ADELANTO_CUOTA, MES_ADELANTO, CANCELACION_TOTAL_OPERACION, NUMERO_OPERACION)
                VALUES
                    (@codBeneficio, @consec, @cedula, @acuerdo, @acuerdoFecha, GETDATE(), @registroUsuario,
                     @cancelacionMora, @mesCancelacion, @adelantoCuota, @mesAdelanto, @cancelacionTotal, @numeroOperacion)";

            var parametros = new
            {
                codBeneficio = cobroMora.cod_beneficio,
                consec = cobroMora.consec,
                cedula = cobroMora.cedula,
                acuerdo = cobroMora.acuerdo,
                acuerdoFecha = cobroMora.acuerdo_fecha,
                cancelacionMora = cobroMora.cancelacion_mora,
                mesCancelacion = string.IsNullOrEmpty(cobroMora.mes_cancelacion) ? null : cobroMora.mes_cancelacion,
                adelantoCuota = cobroMora.adelanto_cuota,
                mesAdelanto = string.IsNullOrEmpty(cobroMora.mes_adelanto) ? null : cobroMora.mes_adelanto,
                cancelacionTotal = cobroMora.cancelacion_total_operacion,
                numeroOperacion = string.IsNullOrEmpty(cobroMora.numero_operacion) ? null : cobroMora.numero_operacion,
                registroUsuario = cobroMora.registro_usuario
            };

            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var existe = connection.QueryFirstOrDefault<int>(sqlExiste, parametros);

                // Nota: se conserva la etiqueta de bitácora original del sistema anterior
                // (existente -> "Inserta", nuevo -> "Actualiza").
                if (existe > 0)
                {
                    connection.Execute(sqlUpdate, parametros);
                    RegistrarBitacoraMora(CodCliente, cobroMora, "Inserta");
                }
                else
                {
                    connection.Execute(sqlInsert, parametros);
                    RegistrarBitacoraMora(CodCliente, cobroMora, "Actualiza");
                }

                return true;
            });

            return new ErrorDto
            {
                Code = result.Code,
                Description = result.Code == 0 ? "Registro para cobro de mora guardado correctamente" : result.Description
            };
        }

        /// <summary>
        /// Registra el movimiento de cobro de mora en la bitácora de beneficios.
        /// </summary>
        private void RegistrarBitacoraMora(int CodCliente, BeneRegistroMoraGuardar cobroMora, string movimiento)
        {
            _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDto
            {
                EmpresaId = CodCliente,
                cod_beneficio = cobroMora.cod_beneficio,
                consec = cobroMora.consec,
                movimiento = movimiento,
                detalle = $"{(movimiento == "Inserta" ? "Inserta" : "Actualiza")} Cobro de Mora del asociado {cobroMora.cedula}",
                registro_usuario = cobroMora.registro_usuario
            });
        }
    }
}
