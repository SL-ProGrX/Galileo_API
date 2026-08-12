using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Guarda el preanálisis (nuevo o modificado) con toda la información del formulario.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2GuardarResponse> Prea_frmPreaEstudiov2_Guardar(
            int codEmpresa,
            FrmPreaEstudiov2GuardarRequest request)
        {
            var response = new ErrorDto<FrmPreaEstudiov2GuardarResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2GuardarResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                var parameters = new DynamicParameters();
                parameters.Add("@Expediente", request.cod_preanalisis?.Trim() ?? string.Empty, DbType.String);
                parameters.Add("@Usuario", request.usuario?.Trim() ?? string.Empty, DbType.String);
                parameters.Add("@Cedula", request.cedula?.Trim() ?? string.Empty, DbType.String);
                parameters.Add("@Nombre", request.nombre?.Trim() ?? string.Empty, DbType.String);
                parameters.Add("@Sexo", request.sexo?.Trim() ?? string.Empty, DbType.String);
                parameters.Add("@FechaNacimiento", request.fecha_nacimiento, DbType.Date);
                parameters.Add("@Linea", request.linea?.Trim() ?? string.Empty, DbType.String);
                parameters.Add("@Destino", request.destino?.Trim() ?? string.Empty, DbType.String);
                parameters.Add("@Garantia", request.garantia?.Trim() ?? string.Empty, DbType.String);
                parameters.Add("@Fiadores", request.fiadores, DbType.Int32);
                parameters.Add("@Contrato", request.contrato?.Trim() ?? string.Empty, DbType.String);
                parameters.Add("@NoOpCrm", request.no_op_crm?.Trim() ?? string.Empty, DbType.String);
                parameters.Add("@Monto", request.monto, DbType.Decimal);
                parameters.Add("@Tasa", request.tasa, DbType.Decimal);
                parameters.Add("@Plazo", request.plazo, DbType.Int32);
                parameters.Add("@Cuota", request.cuota, DbType.Decimal);
                parameters.Add("@MontoConstruccion", request.monto_construccion, DbType.Decimal);
                parameters.Add("@TipoSalario", request.tipo_salario?.Trim() ?? string.Empty, DbType.String);
                parameters.Add("@CorteColilla", request.corte_colilla, DbType.Date);
                parameters.Add("@SalarioDevengado", request.salario_devengado, DbType.Decimal);
                parameters.Add("@SalarioMensual", request.salario_mensual, DbType.Decimal);
                parameters.Add("@SalarioConstancia", request.salario_constancia, DbType.Decimal);
                parameters.Add("@SalarioOrdenPatronal", request.salario_orden_patronal, DbType.Decimal);
                parameters.Add("@IngresoPrivado", request.ingreso_privado, DbType.Decimal);
                parameters.Add("@IngresoPrivadoPorc", request.ingreso_privado_porc, DbType.Decimal);
                parameters.Add("@ComponenteAdicionalId", request.componente_adicional_id, DbType.Int32);
                parameters.Add("@ComponenteAdicionalPorc", request.componente_adicional_porc, DbType.Decimal);
                parameters.Add("@NotasCumplimiento", request.notas_cumplimiento?.Trim() ?? string.Empty, DbType.String);

                var codPreanalisis = connection.QueryFirstOrDefault<string>(
                    "spCrdPreaPreanalisisModifica",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                response.Result = new FrmPreaEstudiov2GuardarResponse
                {
                    cod_preanalisis = codPreanalisis ?? request.cod_preanalisis?.Trim() ?? string.Empty,
                    estado = "G",
                    estado_desc = "Guardado",
                    mensaje = "Preanálisis guardado correctamente."
                };

                return response;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new FrmPreaEstudiov2GuardarResponse();
                return response;
            }
        }
    }
}
