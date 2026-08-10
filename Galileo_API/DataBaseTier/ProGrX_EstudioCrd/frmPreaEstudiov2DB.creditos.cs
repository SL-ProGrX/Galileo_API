using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        private const string DescripcionCreditoCancelado = "CANCELADO";

        /// <summary>
        /// Consulta los créditos en tránsito del expediente (cancelados y por cobrar).
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2CreditosResponse> Prea_frmPreaEstudiov2_Creditos_Consultar(
            int codEmpresa,
            string cod_preanalisis)
        {
            var result = new ErrorDto<FrmPreaEstudiov2CreditosResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2CreditosResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var parameters = new DynamicParameters();
                parameters.Add("@Expediente", cod_preanalisis.Trim(), DbType.String);
                parameters.Add("@Accion", "I", DbType.String);
                parameters.Add("@Proceso", 0, DbType.Int32);

                var creditos = connection.Query<FrmPreaEstudiov2CreditoTransitoDto>(
                    "spCRDPreaCreditosTransito",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                result.Result = new FrmPreaEstudiov2CreditosResponse
                {
                    cancelados = creditos.Where(c => c.descripcion.Contains(DescripcionCreditoCancelado)).ToList(),
                    por_cobrar = creditos.Where(c => !c.descripcion.Contains(DescripcionCreditoCancelado)).ToList(),
                    total_cancelados = creditos.Where(c => c.descripcion.Contains(DescripcionCreditoCancelado)).Sum(c => c.saldo),
                    total_por_cobrar = creditos.Where(c => !c.descripcion.Contains(DescripcionCreditoCancelado)).Sum(c => c.saldo)
                };
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2CreditosResponse();
            }

            return result;
        }
    }
}
