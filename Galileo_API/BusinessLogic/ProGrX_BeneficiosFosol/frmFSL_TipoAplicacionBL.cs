using Galileo.DataBaseTier;
using Galileo.DataBaseTier.ProGrX_BeneficiosFosol;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol
{
    public sealed class FrmFslTipoAplicacionBl
    {
        private const int CodigoValidacion = -2;

        private const string MensajeFiltrosInvalidos =
            "Los filtros enviados no son v&aacute;lidos.";

        private readonly FrmFslTipoAplicacionDB _db;

        public FrmFslTipoAplicacionBl(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _db = new FrmFslTipoAplicacionDB(
                config);
        }

        public ErrorDto<
            FslListaPaginadaDto<
                FslTipoAplicacionPlanDto>>
            FSL_TipoAplicacion_Planes_Lista_Obtener(
                int CodEmpresa,
                string? filtros)
        {
            if (
                !FSL_TipoAplicacion_Filtros_Deserializar(
                    filtros,
                    out FslTipoAplicacionFiltros request)
            )
            {
                return DbHelper.CreateErrorResponse(
                    MensajeFiltrosInvalidos,
                    CodigoValidacion,
                    new FslListaPaginadaDto<
                        FslTipoAplicacionPlanDto>());
            }

            return _db
                .FSL_TipoAplicacion_Planes_Lista_Obtener(
                    CodEmpresa,
                    request);
        }

        public ErrorDto<
            FslListaPaginadaDto<
                FslTipoAplicacionCausaDto>>
            FSL_TipoAplicacion_Causas_Lista_Obtener(
                int CodEmpresa,
                string? codPlan,
                string? filtros)
        {
            if (
                !FSL_TipoAplicacion_Filtros_Deserializar(
                    filtros,
                    out FslTipoAplicacionFiltros request)
            )
            {
                return DbHelper.CreateErrorResponse(
                    MensajeFiltrosInvalidos,
                    CodigoValidacion,
                    new FslListaPaginadaDto<
                        FslTipoAplicacionCausaDto>());
            }

            return _db
                .FSL_TipoAplicacion_Causas_Lista_Obtener(
                    CodEmpresa,
                    codPlan,
                    request);
        }

        public ErrorDto<
            List<DropDownListaGenericaModel>>
            FSL_TipoAplicacion_Planes_Selector_Obtener(
                int CodEmpresa)
        {
            return _db
                .FSL_TipoAplicacion_Planes_Selector_Obtener(
                    CodEmpresa);
        }

        public ErrorDto
            FSL_TipoAplicacion_Plan_Registrar(
                int CodEmpresa,
                FslTipoAplicacionPlanGuardarRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return _db
                .FSL_TipoAplicacion_Plan_Registrar(
                    CodEmpresa,
                    request);
        }

        public ErrorDto
            FSL_TipoAplicacion_Plan_Actualizar(
                int CodEmpresa,
                FslTipoAplicacionPlanGuardarRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return _db
                .FSL_TipoAplicacion_Plan_Actualizar(
                    CodEmpresa,
                    request);
        }

        public ErrorDto
            FSL_TipoAplicacion_Plan_Eliminar(
                int CodEmpresa,
                string? codPlan,
                string? usuario)
        {
            return _db
                .FSL_TipoAplicacion_Plan_Eliminar(
                    CodEmpresa,
                    codPlan,
                    usuario);
        }

        public ErrorDto
            FSL_TipoAplicacion_Causa_Registrar(
                int CodEmpresa,
                FslTipoAplicacionCausaGuardarRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return _db
                .FSL_TipoAplicacion_Causa_Registrar(
                    CodEmpresa,
                    request);
        }

        public ErrorDto
            FSL_TipoAplicacion_Causa_Actualizar(
                int CodEmpresa,
                FslTipoAplicacionCausaGuardarRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return _db
                .FSL_TipoAplicacion_Causa_Actualizar(
                    CodEmpresa,
                    request);
        }

        public ErrorDto
            FSL_TipoAplicacion_Causa_Eliminar(
                int CodEmpresa,
                string? codPlan,
                string? codCausa,
                string? usuario)
        {
            return _db
                .FSL_TipoAplicacion_Causa_Eliminar(
                    CodEmpresa,
                    codPlan,
                    codCausa,
                    usuario);
        }

        private static bool
            FSL_TipoAplicacion_Filtros_Deserializar(
                string? filtros,
                out FslTipoAplicacionFiltros request)
        {
            request =
                new FslTipoAplicacionFiltros();

            if (string.IsNullOrWhiteSpace(filtros))
            {
                return true;
            }

            try
            {
                request =
                    JsonConvert.DeserializeObject<
                        FslTipoAplicacionFiltros>(
                            filtros) ??
                    new FslTipoAplicacionFiltros();

                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}