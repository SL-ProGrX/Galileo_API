using System.Text.Json;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;

namespace Galileo_API.BusinessLogic.ProGrX_ControlTramites
{
    public sealed class FrmAfRecepcionLiquidacionesTagsBl
    {
        private readonly
            FrmAfRecepcionLiquidacionesTagsDb _db;

        public FrmAfRecepcionLiquidacionesTagsBl(
            IConfiguration config)
        {
            _db =
                new FrmAfRecepcionLiquidacionesTagsDb(
                    config);
        }

        public ErrorDto<
            AfRecepcionLiquidacionesTagInicializarResponse>
            AF_frmAF_RecepcionLiquidacionesTag_Inicializar(
                int codEmpresa)
        {
            return _db
                .AF_frmAF_RecepcionLiquidacionesTag_Inicializar(
                    codEmpresa);
        }

        public ErrorDto<
            AfRecepcionLiquidacionesTagLiquidacionResponse?>
            AF_frmAF_RecepcionLiquidacionesTag_Liquidacion_Obtener(
                int codEmpresa,
                long numeroBoleta,
                string movimiento)
        {
            return _db
                .AF_frmAF_RecepcionLiquidacionesTag_Liquidacion_Obtener(
                    codEmpresa,
                    numeroBoleta,
                    movimiento);
        }

        public ErrorDto<
            AfRecepcionLiquidacionesTagAplicarResponse>
            AF_frmAF_RecepcionLiquidacionesTag_Movimiento_Aplicar(
                int codEmpresa,
                AfRecepcionLiquidacionesTagAplicarRequest?
                    request)
        {
            return _db
                .AF_frmAF_RecepcionLiquidacionesTag_Movimiento_Aplicar(
                    codEmpresa,
                    request);
        }

        public ErrorDto<List<
            AfRecepcionLiquidacionesTagPendienteResponse>>
            AF_frmAF_RecepcionLiquidacionesTag_Pendientes_Obtener(
                int codEmpresa,
                string? request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request))
                {
                    return DbHelper.CreateErrorResponse(
                        "Los filtros de pendientes son requeridos.",
                        -2,
                        new List<
                            AfRecepcionLiquidacionesTagPendienteResponse>());
                }

                var filtros =
                    JsonSerializer.Deserialize<
                        AfRecepcionLiquidacionesTagPendientesRequest>(
                            request,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive =
                                    true
                            });

                if (filtros is null)
                {
                    return DbHelper.CreateErrorResponse(
                        "Los filtros de pendientes no son v&aacute;lidos.",
                        -2,
                        new List<
                            AfRecepcionLiquidacionesTagPendienteResponse>());
                }

                return _db
                    .AF_frmAF_RecepcionLiquidacionesTag_Pendientes_Obtener(
                        codEmpresa,
                        filtros);
            }
            catch (JsonException)
            {
                return DbHelper.CreateErrorResponse(
                    "El formato de los filtros de pendientes no es v&aacute;lido.",
                    -2,
                    new List<
                        AfRecepcionLiquidacionesTagPendienteResponse>());
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new List<
                        AfRecepcionLiquidacionesTagPendienteResponse>());
            }
        }

        public ErrorDto<List<
            AfRecepcionLiquidacionesTagHistorialResponse>>
            AF_frmAF_RecepcionLiquidacionesTag_Historial_Obtener(
                int codEmpresa,
                string? request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request))
                {
                    return DbHelper.CreateErrorResponse(
                        "Los filtros de consulta son requeridos.",
                        -2,
                        new List<
                            AfRecepcionLiquidacionesTagHistorialResponse>());
                }

                var filtros =
                    JsonSerializer.Deserialize<
                        AfRecepcionLiquidacionesTagHistorialRequest>(
                            request,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive =
                                    true
                            });

                if (filtros is null)
                {
                    return DbHelper.CreateErrorResponse(
                        "Los filtros de consulta no son v&aacute;lidos.",
                        -2,
                        new List<
                            AfRecepcionLiquidacionesTagHistorialResponse>());
                }

                return _db
                    .AF_frmAF_RecepcionLiquidacionesTag_Historial_Obtener(
                        codEmpresa,
                        filtros);
            }
            catch (JsonException)
            {
                return DbHelper.CreateErrorResponse(
                    "El formato de los filtros de consulta no es v&aacute;lido.",
                    -2,
                    new List<
                        AfRecepcionLiquidacionesTagHistorialResponse>());
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new List<
                        AfRecepcionLiquidacionesTagHistorialResponse>());
            }
        }
    }
}