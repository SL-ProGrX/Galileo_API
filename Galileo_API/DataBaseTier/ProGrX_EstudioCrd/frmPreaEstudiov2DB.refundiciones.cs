using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Consulta las refundiciones del expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2RefundicionesResponse> Prea_frmPreaEstudiov2_Refundiciones_Consultar(
            int codEmpresa,
            string cod_preanalisis)
        {
            var result = new ErrorDto<FrmPreaEstudiov2RefundicionesResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2RefundicionesResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var parameters = new DynamicParameters();
                parameters.Add("@Expediente", cod_preanalisis.Trim(), DbType.String);

                var refundiciones = connection.Query<FrmPreaEstudiov2RefundicionDto>(
                    "spCrdPreaConsultaRefundicionesPreanalisis",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                result.Result = new FrmPreaEstudiov2RefundicionesResponse
                {
                    refundiciones = refundiciones,
                    total_cuotas = refundiciones.Sum(r => r.cuota),
                    total_refunde = refundiciones.Sum(r => r.refunde),
                    total_mora = refundiciones.Sum(r => r.mora)
                };
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2RefundicionesResponse();
            }

            return result;
        }
    }
}
