using Galileo.DataBaseTier;
using Galileo.DataBaseTier.ProGrX_BeneficiosFosol;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol
{
    public sealed class FrmFslTablasTiposBl
    {
        private const int CodigoValidacion = -2;

        private const string MensajeFiltrosInvalidos =
            "Los filtros enviados no son v&aacute;lidos.";

        private readonly FrmFslTablasTiposDb _db;

        public FrmFslTablasTiposBl(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _db = new FrmFslTablasTiposDb(config);
        }

        public ErrorDto<
            FslListaPaginadaDto<FslTablaTipoDto>>
            FSL_TablasTipos_Lista_Obtener(
                int CodEmpresa,
                string? filtros)
        {
            if (
                !FSL_TablasTipos_Filtros_Deserializar(
                    filtros,
                    out FslTablasTiposFiltros request)
            )
            {
                return DbHelper.CreateErrorResponse(
                    MensajeFiltrosInvalidos,
                    CodigoValidacion,
                    new FslListaPaginadaDto<
                        FslTablaTipoDto>());
            }

            return _db
                .FSL_TablasTipos_Lista_Obtener(
                    CodEmpresa,
                    request);
        }

        public ErrorDto
            FSL_TablasTipos_Tipo_Registrar(
                int CodEmpresa,
                FslTablaTipoGuardarRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return _db
                .FSL_TablasTipos_Tipo_Registrar(
                    CodEmpresa,
                    request);
        }

        public ErrorDto
            FSL_TablasTipos_Tipo_Actualizar(
                int CodEmpresa,
                FslTablaTipoGuardarRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return _db
                .FSL_TablasTipos_Tipo_Actualizar(
                    CodEmpresa,
                    request);
        }

        public ErrorDto
            FSL_TablasTipos_Tipo_Eliminar(
                int CodEmpresa,
                string? tipo,
                string? codigo,
                string? usuario)
        {
            return _db
                .FSL_TablasTipos_Tipo_Eliminar(
                    CodEmpresa,
                    tipo,
                    codigo,
                    usuario);
        }

        private static bool
            FSL_TablasTipos_Filtros_Deserializar(
                string? filtros,
                out FslTablasTiposFiltros request)
        {
            request = new FslTablasTiposFiltros();

            if (string.IsNullOrWhiteSpace(filtros))
            {
                return true;
            }

            try
            {
                request =
                    JsonConvert.DeserializeObject<
                        FslTablasTiposFiltros>(
                            filtros) ??
                    new FslTablasTiposFiltros();

                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}