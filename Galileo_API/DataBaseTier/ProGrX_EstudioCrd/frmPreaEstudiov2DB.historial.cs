using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Consulta el historial del expediente (ejecutivo y general).
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2HistorialResponse> Prea_frmPreaEstudiov2_Historial_Consultar(
            int codEmpresa,
            string cod_preanalisis)
        {
            var result = new ErrorDto<FrmPreaEstudiov2HistorialResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2HistorialResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var parameters = new DynamicParameters();
                parameters.Add("@Expediente", cod_preanalisis.Trim(), DbType.String);

                var historialEjecutivo = connection.Query<FrmPreaEstudiov2HistorialDto>(
                    "spCrdPreaGetHistorial",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                var historialGeneral = connection.Query<FrmPreaEstudiov2HistorialDto>(
                    "spCrdPreaGetHistorialGeneral",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                result.Result = new FrmPreaEstudiov2HistorialResponse
                {
                    ejecutivos = historialEjecutivo,
                    general = historialGeneral
                };
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2HistorialResponse();
            }

            return result;
        }
    }
}
