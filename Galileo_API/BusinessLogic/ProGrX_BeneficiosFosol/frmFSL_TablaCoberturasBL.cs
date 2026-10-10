using Galileo.DataBaseTier;
using Galileo.DataBaseTier.ProGrX_BeneficiosFosol;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol
{
    public sealed class FrmFslTablaCoberturasBl
    {
        private const int CodigoValidacion = -2;

        private readonly FrmFslTablaCoberturasDb _db;

        public FrmFslTablaCoberturasBl(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _db = new FrmFslTablaCoberturasDb(config);
        }

        public ErrorDto<
            FslListaPaginadaDto<FslTablaCoberturaDto>>
            FSL_TablaCoberturas_Lista_Obtener(
                int CodEmpresa,
                string filtros)
        {
            if (
                !FSL_TablaCoberturas_Filtros_Deserializar(
                    filtros,
                    out FslTablaCoberturasFiltros request)
            )
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros enviados no son v&aacute;lidos.",
                    CodigoValidacion,
                    new FslListaPaginadaDto<
                        FslTablaCoberturaDto>());
            }

            return _db
                .FSL_TablaCoberturas_Lista_Obtener(
                    CodEmpresa,
                    request);
        }

        public ErrorDto
            FSL_TablaCoberturas_Cobertura_Registrar(
                int CodEmpresa,
                FslTablaCoberturaGuardarRequest request)
        {
            return _db
                .FSL_TablaCoberturas_Cobertura_Registrar(
                    CodEmpresa,
                    request);
        }

        public ErrorDto
            FSL_TablaCoberturas_Cobertura_Actualizar(
                int CodEmpresa,
                FslTablaCoberturaGuardarRequest request)
        {
            return _db
                .FSL_TablaCoberturas_Cobertura_Actualizar(
                    CodEmpresa,
                    request);
        }

        public ErrorDto
            FSL_TablaCoberturas_Cobertura_Eliminar(
                int CodEmpresa,
                string tipo,
                int linea,
                string usuario)
        {
            return _db
                .FSL_TablaCoberturas_Cobertura_Eliminar(
                    CodEmpresa,
                    tipo,
                    linea,
                    usuario);
        }

        private static bool
            FSL_TablaCoberturas_Filtros_Deserializar(
                string filtros,
                out FslTablaCoberturasFiltros request)
        {
            request =
                new FslTablaCoberturasFiltros();

            if (string.IsNullOrWhiteSpace(filtros))
            {
                return true;
            }

            try
            {
                request =
                    JsonConvert.DeserializeObject<
                        FslTablaCoberturasFiltros>(
                            filtros) ??
                    new FslTablaCoberturasFiltros();

                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}