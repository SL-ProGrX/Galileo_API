using System.Text.Json;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;

namespace Galileo_API.BusinessLogic.ProGrX_ControlTramites
{
    public sealed class FrmCrSeguimientoRecepcionTagBl
    {
        private readonly FrmCrSeguimientoRecepcionTagDb _db;

        public FrmCrSeguimientoRecepcionTagBl(
            IConfiguration config)
        {
            _db =
                new FrmCrSeguimientoRecepcionTagDb(
                    config);
        }

        public ErrorDto<
            CrSeguimientoRecepcionTagInicializarResponse>
            CR_frmCR_SeguimientoRecepcionTag_Inicializar(
                int codEmpresa)
        {
            return _db
                .CR_frmCR_SeguimientoRecepcionTag_Inicializar(
                    codEmpresa);
        }

        public ErrorDto<
            CrSeguimientoRecepcionTagOperacionResponse?>
            CR_frmCR_SeguimientoRecepcionTag_Operacion_Obtener(
                int codEmpresa,
                long idSolicitud,
                string movimiento)
        {
            return _db
                .CR_frmCR_SeguimientoRecepcionTag_Operacion_Obtener(
                    codEmpresa,
                    idSolicitud,
                    movimiento);
        }

        public ErrorDto<List<
            CrSeguimientoRecepcionTagPendienteResponse>>
            CR_frmCR_SeguimientoRecepcionTag_Pendientes_Obtener(
                int codEmpresa,
                CrSeguimientoRecepcionTagPendientesRequest
                    request)
        {
            return _db
                .CR_frmCR_SeguimientoRecepcionTag_Pendientes_Obtener(
                    codEmpresa,
                    request);
        }

        public ErrorDto<
            CrSeguimientoRecepcionTagAplicarResponse>
            CR_frmCR_SeguimientoRecepcionTag_Movimiento_Aplicar(
                int codEmpresa,
                CrSeguimientoRecepcionTagAplicarRequest
                    request)
        {
            return _db
                .CR_frmCR_SeguimientoRecepcionTag_Movimiento_Aplicar(
                    codEmpresa,
                    request);
        }

        public ErrorDto<List<
            CrSeguimientoRecepcionTagHistorialResponse>>
            CR_frmCR_SeguimientoRecepcionTag_Historial_Obtener(
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
                            CrSeguimientoRecepcionTagHistorialResponse>());
                }

                var filtros =
                    JsonSerializer.Deserialize<
                        CrSeguimientoRecepcionTagHistorialRequest>(
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
                            CrSeguimientoRecepcionTagHistorialResponse>());
                }

                return _db
                    .CR_frmCR_SeguimientoRecepcionTag_Historial_Obtener(
                        codEmpresa,
                        filtros);
            }
            catch (JsonException)
            {
                return DbHelper.CreateErrorResponse(
                    "El formato de los filtros de consulta no es v&aacute;lido.",
                    -2,
                    new List<
                        CrSeguimientoRecepcionTagHistorialResponse>());
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new List<
                        CrSeguimientoRecepcionTagHistorialResponse>());
            }
        }
    }
}