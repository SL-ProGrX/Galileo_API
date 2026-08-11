using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Consulta las fianzas del expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2FianzasResponse> Prea_frmPreaEstudiov2_Fianzas_Consultar(
            int codEmpresa,
            string cod_preanalisis)
        {
            var result = new ErrorDto<FrmPreaEstudiov2FianzasResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2FianzasResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var parameters = new DynamicParameters();
                parameters.Add("@Expediente", cod_preanalisis.Trim(), DbType.String);

                var fianzas = connection.Query<FrmPreaEstudiov2FianzaDto>(
                    "spCrdPreaConsultaFianzas",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                result.Result = new FrmPreaEstudiov2FianzasResponse
                {
                    fianzas = fianzas,
                    total_saldos = fianzas.Sum(f => f.saldo),
                    total_cuotas = fianzas.Sum(f => f.cuota)
                };
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2FianzasResponse();
            }

            return result;
        }
    }
}
