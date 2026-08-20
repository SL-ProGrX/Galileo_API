using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_ControlTramites;
using Galileo_API.Models.ProGrX_ControlTramites;
using System.Text.Json;

namespace Galileo_API.BusinessLogic.ProGrX_ControlTramites
{
    public sealed class FrmFndRecepcionLiqFondosTagsBl
    {
        private readonly FrmFndRecepcionLiqFondosTagsDb _db;

        public FrmFndRecepcionLiqFondosTagsBl(
            IConfiguration config)
        {
            _db = new FrmFndRecepcionLiqFondosTagsDb(config);
        }

        public ErrorDto<FndRecepcionLiqFondosTagsInicializarResponse>
            FND_frmFNDRecepcionLiqFondosTags_Inicializar(
                int codEmpresa)
        {
            return _db
                .FND_frmFNDRecepcionLiqFondosTags_Inicializar(
                    codEmpresa);
        }

        public ErrorDto<FndRecepcionLiqFondosTagsBoletaResponse?>
            FND_frmFNDRecepcionLiqFondosTags_Boleta_Obtener(
                int codEmpresa,
                long numeroBoleta,
                string? movimiento)
        {
            return _db
                .FND_frmFNDRecepcionLiqFondosTags_Boleta_Obtener(
                    codEmpresa,
                    numeroBoleta,
                    movimiento);
        }

        public ErrorDto<List<
            FndRecepcionLiqFondosTagsPendienteResponse>>
            FND_frmFNDRecepcionLiqFondosTags_Pendientes_Obtener(
                int codEmpresa,
                FndRecepcionLiqFondosTagsPendientesRequest request)
        {
            return _db
                .FND_frmFNDRecepcionLiqFondosTags_Pendientes_Obtener(
                    codEmpresa,
                    request);
        }

        public ErrorDto<FndRecepcionLiqFondosTagsAplicarResponse>
            FND_frmFNDRecepcionLiqFondosTags_Movimiento_Aplicar(
                int codEmpresa,
                FndRecepcionLiqFondosTagsAplicarRequest request)
        {
            return _db
                .FND_frmFNDRecepcionLiqFondosTags_Movimiento_Aplicar(
                    codEmpresa,
                    request);
        }

        public ErrorDto<List<
            FndRecepcionLiqFondosTagsHistorialResponse>>
            FND_frmFNDRecepcionLiqFondosTags_Historial_Obtener(
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
                            FndRecepcionLiqFondosTagsHistorialResponse>());
                }

                var filtros =
                    JsonSerializer.Deserialize<
                        FndRecepcionLiqFondosTagsHistorialRequest>(
                            request,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                if (filtros is null)
                {
                    return DbHelper.CreateErrorResponse(
                        "Los filtros de consulta no son v&aacute;lidos.",
                        -2,
                        new List<
                            FndRecepcionLiqFondosTagsHistorialResponse>());
                }

                return _db
                    .FND_frmFNDRecepcionLiqFondosTags_Historial_Obtener(
                        codEmpresa,
                        filtros);
            }
            catch (JsonException)
            {
                return DbHelper.CreateErrorResponse(
                    "El formato de los filtros de consulta no es v&aacute;lido.",
                    -2,
                    new List<
                        FndRecepcionLiqFondosTagsHistorialResponse>());
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new List<
                        FndRecepcionLiqFondosTagsHistorialResponse>());
            }
        }
    }
}