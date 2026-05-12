using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public class FrmPreaEstudiov2DB
    {
        private readonly PortalDB _portalDb;

        public FrmPreaEstudiov2DB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la información inicial del tab Hipotecario del Estudio de Crédito v2.
        /// El API resuelve montos y permisos de botones para que Angular solo pinte el estado.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="request">Datos base del estudio.</param>
        /// <returns>Información hipotecaria inicial.</returns>
        public ErrorDto<FrmPreaEstudiov2HipotecarioResponse> Prea_frmPreaEstudiov2_Hipotecario_Obtener(
            int codEmpresa,
            FrmPreaEstudiov2HipotecarioRequest request)
        {
            var result = new ErrorDto<FrmPreaEstudiov2HipotecarioResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2HipotecarioResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var parameters = new DynamicParameters();
                parameters.Add("@cod_preanalisis", request.cod_preanalisis?.Trim() ?? string.Empty, DbType.String);
                parameters.Add("@id_solicitud", request.id_solicitud, DbType.Int64);
                parameters.Add("@usuario", request.usuario?.Trim() ?? string.Empty, DbType.String);

                result.Result = connection.QueryFirstOrDefault<FrmPreaEstudiov2HipotecarioResponse>(
                    "spPrea_frmPreaEstudiov2_Hipotecario_Obtener",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ) ?? new FrmPreaEstudiov2HipotecarioResponse();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2HipotecarioResponse();
            }

            return result;
        }

    }
}
