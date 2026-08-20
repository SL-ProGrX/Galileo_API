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

        /// <summary>
        /// Agrega una etiqueta de seguimiento con nota al expediente. Fiel a VB6
        /// btnEtiqueta_Click (frmPreaEstudiov2.frm línea ~13247): exec spCrdPreaAgregaEtiqueta
        /// '&lt;expediente&gt;', '&lt;etiqueta&gt;', '&lt;nota&gt;', '&lt;usuario&gt;', luego recarga
        /// el historial (sbHistorial_Load("E")).
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2HistorialResponse> Prea_frmPreaEstudiov2_Etiqueta_Agregar(
            int codEmpresa,
            FrmPreaEstudiov2EtiquetaAgregarRequest request)
        {
            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                const string sql = "EXEC spCrdPreaAgregaEtiqueta @Expediente, @Etiqueta, @Nota, @Usuario";
                connection.Execute(sql, new
                {
                    Expediente = request.cod_preanalisis.Trim(),
                    Etiqueta = (request.cod_etiqueta ?? string.Empty).Trim(),
                    Nota = (request.nota ?? string.Empty).Trim(),
                    Usuario = (request.usuario ?? string.Empty).Trim()
                });
            }
            catch (Exception ex)
            {
                return new ErrorDto<FrmPreaEstudiov2HistorialResponse>
                {
                    Code = -1,
                    Description = ex.Message,
                    Result = new FrmPreaEstudiov2HistorialResponse()
                };
            }

            return Prea_frmPreaEstudiov2_Historial_Consultar(codEmpresa, request.cod_preanalisis);
        }
    }
}
