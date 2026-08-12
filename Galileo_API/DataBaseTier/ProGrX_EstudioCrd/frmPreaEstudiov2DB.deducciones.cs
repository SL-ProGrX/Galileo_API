using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Consulta las deducciones del expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2DeduccionesResponse> Prea_frmPreaEstudiov2_Deducciones_Consultar(
            int codEmpresa,
            string cod_preanalisis)
        {
            var result = new ErrorDto<FrmPreaEstudiov2DeduccionesResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2DeduccionesResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var parameters = new DynamicParameters();
                parameters.Add("@Expediente", cod_preanalisis.Trim(), DbType.String);

                var deducciones = connection.Query<FrmPreaEstudiov2DeduccionesDetalleDto>(
                    "spCrdPreaConsultaDeducciones",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                result.Result = new FrmPreaEstudiov2DeduccionesResponse
                {
                    deducciones = deducciones,
                    total_deducciones = deducciones.Sum(d => d.monto)
                };
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2DeduccionesResponse();
            }

            return result;
        }

        /// <summary>
        /// Agrega una deducción al expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2DeduccionesResponse> Prea_frmPreaEstudiov2_Deducciones_Agregar(
            int codEmpresa,
            FrmPreaEstudiov2DeduccionesAgregarRequest request)
        {
            var response = new ErrorDto<FrmPreaEstudiov2DeduccionesResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2DeduccionesResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                var parameters = new DynamicParameters();
                parameters.Add("@Expediente", request.cod_preanalisis.Trim(), DbType.String);
                parameters.Add("@Param2", 0, DbType.Int32);
                parameters.Add("@IdDeduccion", 0, DbType.Int32);
                parameters.Add("@Descripcion", request.descripcion?.Trim() ?? string.Empty, DbType.String);
                parameters.Add("@Monto", request.monto, DbType.Decimal);
                parameters.Add("@MontoTotal", request.monto, DbType.Decimal);
                parameters.Add("@Usuario", request.usuario?.Trim() ?? string.Empty, DbType.String);

                connection.QueryFirstOrDefault<dynamic>(
                    "spCrdPrea_Deducciones_Add",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return Prea_frmPreaEstudiov2_Deducciones_Consultar(codEmpresa, request.cod_preanalisis.Trim());
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new FrmPreaEstudiov2DeduccionesResponse();
                return response;
            }
        }
    }
}
