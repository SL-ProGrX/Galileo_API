using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Consulta los desembolsos del expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2DesembolsosResponse> Prea_frmPreaEstudiov2_Desembolsos_Consultar(
            int codEmpresa,
            string cod_preanalisis)
        {
            var result = new ErrorDto<FrmPreaEstudiov2DesembolsosResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2DesembolsosResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var parameters = new DynamicParameters();
                parameters.Add("@Expediente", cod_preanalisis.Trim(), DbType.String);

                var desembolsos = connection.Query<FrmPreaEstudiov2DesembolsoDto>(
                    "spCrdPreaGuardaDesembolsos",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();

                var bancos = connection.Query<FrmPreaEstudiov2DropdownDto>(
                    "exec spCrd_SGT_Bancos_Desembolso @Usuario = ''"
                ).ToList();

                result.Result = new FrmPreaEstudiov2DesembolsosResponse
                {
                    desembolsos = desembolsos,
                    bancos = bancos
                };
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2DesembolsosResponse();
            }

            return result;
        }

        /// <summary>
        /// Guarda un desembolso del expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2DesembolsosResponse> Prea_frmPreaEstudiov2_Desembolsos_Guardar(
            int codEmpresa,
            FrmPreaEstudiov2DesembolsoGuardarRequest request)
        {
            var response = new ErrorDto<FrmPreaEstudiov2DesembolsosResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2DesembolsosResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                var parameters = new DynamicParameters();
                parameters.Add("@Expediente", request.cod_preanalisis.Trim(), DbType.String);
                parameters.Add("@Tag", string.Empty, DbType.String);
                parameters.Add("@Ordinario", 0, DbType.Int32);
                parameters.Add("@Descripcion", request.tipo?.Trim() ?? string.Empty, DbType.String);
                parameters.Add("@Cuota", 0, DbType.Decimal);
                parameters.Add("@Monto", request.monto, DbType.Decimal);
                parameters.Add("@Tipo", request.tipo?.Trim() ?? string.Empty, DbType.String);
                parameters.Add("@Identificacion", string.Empty, DbType.String);
                parameters.Add("@TipoId", 0, DbType.Int32);
                parameters.Add("@Cuenta", request.cuenta?.Trim() ?? string.Empty, DbType.String);
                parameters.Add("@Divisa", "CRC", DbType.String);
                parameters.Add("@Param12", string.Empty, DbType.String);
                parameters.Add("@Correo", string.Empty, DbType.String);
                parameters.Add("@Detalle", request.concepto?.Trim() ?? string.Empty, DbType.String);
                parameters.Add("@Param15", string.Empty, DbType.String);
                parameters.Add("@Banco", 0, DbType.Int32);

                connection.Execute(
                    "spCrdPreaGuardaDesembolsos",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return Prea_frmPreaEstudiov2_Desembolsos_Consultar(codEmpresa, request.cod_preanalisis.Trim());
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new FrmPreaEstudiov2DesembolsosResponse();
                return response;
            }
        }

        /// <summary>
        /// Elimina un desembolso del expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2DesembolsosResponse> Prea_frmPreaEstudiov2_Desembolsos_Eliminar(
            int codEmpresa,
            string cod_preanalisis,
            int id_desembolso)
        {
            var response = new ErrorDto<FrmPreaEstudiov2DesembolsosResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2DesembolsosResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                var parameters = new DynamicParameters();
                parameters.Add("@Expediente", cod_preanalisis.Trim(), DbType.String);
                parameters.Add("@IdDesembolso", id_desembolso, DbType.Int32);

                connection.Execute(
                    "spCrdPreaEliminarDesembolsos",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return Prea_frmPreaEstudiov2_Desembolsos_Consultar(codEmpresa, cod_preanalisis.Trim());
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new FrmPreaEstudiov2DesembolsosResponse();
                return response;
            }
        }
    }
}
