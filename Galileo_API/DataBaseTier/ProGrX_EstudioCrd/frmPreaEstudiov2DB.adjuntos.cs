using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Consulta los adjuntos del expediente.
        /// </summary>
        public ErrorDto<List<FrmPreaEstudiov2AdjuntoDto>> Prea_frmPreaEstudiov2_Adjuntos_Consultar(
            int codEmpresa,
            string cod_preanalisis)
        {
            var result = new ErrorDto<List<FrmPreaEstudiov2AdjuntoDto>>
            {
                Code = 0,
                Description = "Ok",
                Result = []
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                const string sql = @"SELECT ID_ADJUNTO AS id_adjunto, NOM_ADJUNTO AS nombre_archivo, 
                                     FECHA_REG AS fecha, USUARIO_REG AS usuario 
                                     FROM CRD_PREA_V2_ADJUNTOS 
                                     WHERE ID_EXPEDIENTE = @Expediente 
                                     ORDER BY FECHA_REG DESC";

                result.Result = connection.Query<FrmPreaEstudiov2AdjuntoDto>(
                    sql,
                    new { Expediente = cod_preanalisis.Trim() },
                    commandType: CommandType.Text
                ).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = [];
            }

            return result;
        }
    }
}
