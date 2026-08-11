using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Consulta la resolución del expediente (comité, autorizaciones, asignaciones).
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2ResolucionResponse> Prea_frmPreaEstudiov2_Resolucion_Consultar(
            int codEmpresa,
            string cod_preanalisis)
        {
            var result = new ErrorDto<FrmPreaEstudiov2ResolucionResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2ResolucionResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var parameters = new DynamicParameters();
                parameters.Add("@Expediente", cod_preanalisis.Trim(), DbType.String);
                parameters.Add("@Tipo", "RES", DbType.String);

                var historial = connection.Query<FrmPreaEstudiov2HistorialDto>(
                    "spCrd_Estudio_Resolucion_Detalle",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                result.Result = new FrmPreaEstudiov2ResolucionResponse
                {
                    historial = historial
                };
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2ResolucionResponse();
            }

            return result;
        }
    }
}
