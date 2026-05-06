using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.BusinessLogic.ProGrX_Hipotecario
{
    public class FrmVivCoberturasBl
    {
        private readonly FrmVivCoberturasDb _db;

        public FrmVivCoberturasBl(IConfiguration config)
        {
            _db = new FrmVivCoberturasDb(config);
        }

        public ErrorDto<FrmVivCoberturasCargaResponse> Viv_Coberturas_Cargar(
            int codEmpresa,
            long numero_operacion)
        {
            if (numero_operacion <= 0)
            {
                return new ErrorDto<FrmVivCoberturasCargaResponse>
                {
                    Code = -1,
                    Description = "Debe indicar una operación válida.",
                    Result = new FrmVivCoberturasCargaResponse()
                };
            }

            return _db.Viv_Coberturas_Cargar(codEmpresa, numero_operacion);
        }

        public ErrorDto<FrmVivCoberturasResumenResponse> Viv_CoberturasResumen_Obtener(
            int codEmpresa,
            FrmVivCoberturasResumenRequest request)
        {
            if (request.numero_operacion <= 0)
            {
                return new ErrorDto<FrmVivCoberturasResumenResponse>
                {
                    Code = -1,
                    Description = "Debe indicar una operación válida.",
                    Result = new FrmVivCoberturasResumenResponse()
                };
            }

            string modoCobertura = (request.modo_cobertura ?? string.Empty).Trim().ToUpperInvariant();
            if (modoCobertura != "GENERAL" && modoCobertura != "INDIVIDUAL")
            {
                return new ErrorDto<FrmVivCoberturasResumenResponse>
                {
                    Code = -1,
                    Description = "Debe indicar un modo de cobertura válido.",
                    Result = new FrmVivCoberturasResumenResponse()
                };
            }

            if (modoCobertura == "INDIVIDUAL" && string.IsNullOrWhiteSpace(request.numero_finca))
            {
                return new ErrorDto<FrmVivCoberturasResumenResponse>
                {
                    Code = -1,
                    Description = "Debe seleccionar una finca para la cobertura individual.",
                    Result = new FrmVivCoberturasResumenResponse()
                };
            }

            return _db.Viv_CoberturasResumen_Obtener(codEmpresa, request);
        }
    }
}
