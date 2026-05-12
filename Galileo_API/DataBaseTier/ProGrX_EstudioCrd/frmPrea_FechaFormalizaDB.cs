using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd.Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public class FrmPreaFechaFormalizaDB
    {
        private readonly PortalDB _portalDb;

        public FrmPreaFechaFormalizaDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Carga la información inicial de frmPrea_FechaFormaliza para que Angular
        /// solo pinte fechas y valores calculados base del expediente.
        /// </summary>
        public ErrorDto<FrmPreaFechaFormalizaCargarResponse> Prea_frmPreaFechaFormaliza_Cargar(
            int codEmpresa,
            FrmPreaFechaFormalizaCargarRequest request)
        {
            var response = new ErrorDto<FrmPreaFechaFormalizaCargarResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaFechaFormalizaCargarResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var baseData = connection.QueryFirstOrDefault<FrmPreaFechaFormalizaBaseData>(
                    "spCrdPrea_FechaCalIntereses",
                    new { Expediente = request.cod_preanalisis.Trim() },
                    commandType: CommandType.StoredProcedure
                ) ?? new FrmPreaFechaFormalizaBaseData();

                response.Result = new FrmPreaFechaFormalizaCargarResponse
                {
                    cod_preanalisis = request.cod_preanalisis.Trim(),
                    planilla_aplica = baseData.planilla_aplica,
                    planilla_envio = baseData.planilla_envio,
                    fecha_corte = baseData.fecha_corte,
                    formalizacion = baseData.formalizacion,
                    monto = baseData.monto,
                    tasa = baseData.tasa,
                    dias = 0,
                    monto_interes = 0
                };

                return response;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new FrmPreaFechaFormalizaCargarResponse();
                return response;
            }
        }

        /// <summary>
        /// Calcula intereses proyectando la fecha de formalización a partir del expediente.
        /// El API resuelve monto, tasa y fecha de corte para no depender del cliente.
        /// </summary>
        public ErrorDto<FrmPreaFechaFormalizaCalcularResponse> Prea_frmPreaFechaFormaliza_Calcular(
            int codEmpresa,
            FrmPreaFechaFormalizaCalcularRequest request)
        {
            var response = new ErrorDto<FrmPreaFechaFormalizaCalcularResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaFechaFormalizaCalcularResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                var baseData = connection.QueryFirstOrDefault<FrmPreaFechaFormalizaBaseData>(
                    "spCrdPrea_FechaCalIntereses",
                    new { Expediente = request.cod_preanalisis.Trim() },
                    commandType: CommandType.StoredProcedure
                ) ?? new FrmPreaFechaFormalizaBaseData();

                var calculo = connection.QueryFirstOrDefault<FrmPreaFechaFormalizaCalculoSpData>(
                    "spCrdPrea_InteresesFormaliza_Calculo",
                    new
                    {
                        Expediente = request.cod_preanalisis.Trim(),
                        Monto = baseData.monto,
                        Tasa = baseData.tasa,
                        Formalizacion = request.fecha_formaliza,
                        Corte = baseData.fecha_corte
                    },
                    commandType: CommandType.StoredProcedure
                ) ?? new FrmPreaFechaFormalizaCalculoSpData();

                response.Result = new FrmPreaFechaFormalizaCalcularResponse
                {
                    fecha_corte = baseData.fecha_corte,
                    fecha_formaliza = request.fecha_formaliza,
                    dias = calculo.dias,
                    monto_interes = calculo.monto_interes
                };

                return response;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new FrmPreaFechaFormalizaCalcularResponse();
                return response;
            }
        }

        /// <summary>
        /// Recalcula y guarda el monto de interés proyectado de frmPrea_FechaFormaliza.
        /// Angular solo envía expediente y fecha; el API valida, calcula y persiste.
        /// </summary>
        public ErrorDto<FrmPreaFechaFormalizaCambiarResponse> Prea_frmPreaFechaFormaliza_Cambiar(
            int codEmpresa,
            FrmPreaFechaFormalizaCambiarRequest request)
        {
            var response = new ErrorDto<FrmPreaFechaFormalizaCambiarResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaFechaFormalizaCambiarResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                connection.Execute(
                       "spCrdPreaGuardaMontoInteresProyectado",
                       new
                       {
                           COD_PREANALISIS = request.cod_preanalisis.Trim(),
                           MONTO_INTERES = request.monto_interes,
                           INDICADOR_CALC = 1
                       },
                       commandType: CommandType.StoredProcedure
                   );

                response.Result = new FrmPreaFechaFormalizaCambiarResponse
                {
                    fecha_formaliza = request.fecha_formaliza,
                    monto_interes = request.monto_interes,
                    mensaje = "Se ha realizado la acción correctamente."
                };

                return response;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new FrmPreaFechaFormalizaCambiarResponse();
                return response;
            }
        }
    }
}
