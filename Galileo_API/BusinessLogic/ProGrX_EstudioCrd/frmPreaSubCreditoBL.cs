using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmPreaSubCreditoBL
    {
        private readonly FrmPreaSubCreditoDB _db;

        public FrmPreaSubCreditoBL(IConfiguration config)
        {
            _db = new FrmPreaSubCreditoDB(config);
        }

        public ErrorDto<FrmPreaSubCreditoCargarResponse> Prea_frmPreaSubCredito_Cargar(
            int codEmpresa,
            string usuario,
            string cod_preanalisis)
        {
            if (string.IsNullOrWhiteSpace(cod_preanalisis))
            {
                return new ErrorDto<FrmPreaSubCreditoCargarResponse>
                {
                    Code = -1,
                    Description = "Debe indicar el expediente.",
                    Result = new FrmPreaSubCreditoCargarResponse()
                };
            }

            var request = new FrmPreaSubCreditoCargarRequest
            {
                usuario = usuario?.Trim() ?? string.Empty,
                cod_preanalisis = cod_preanalisis.Trim()
            };

            return _db.Prea_frmPreaSubCredito_Cargar(codEmpresa, request);
        }

        public ErrorDto<FrmPreaSubCreditoAplicarResponse> Prea_frmPreaSubCredito_Aplicar(
            int codEmpresa,
            FrmPreaSubCreditoAplicarRequest request)
        {
            if (request is null)
            {
                return new ErrorDto<FrmPreaSubCreditoAplicarResponse>
                {
                    Code = -1,
                    Description = "Debe indicar la información de la solicitud.",
                    Result = new FrmPreaSubCreditoAplicarResponse()
                };
            }

            if (string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaSubCreditoAplicarResponse>
                {
                    Code = -1,
                    Description = "Debe indicar el expediente.",
                    Result = new FrmPreaSubCreditoAplicarResponse()
                };
            }

            if (request.banco <= 0)
            {
                return new ErrorDto<FrmPreaSubCreditoAplicarResponse>
                {
                    Code = -1,
                    Description = "Debe seleccionar el banco.",
                    Result = new FrmPreaSubCreditoAplicarResponse()
                };
            }

            if (string.IsNullOrWhiteSpace(request.tipo_documento))
            {
                return new ErrorDto<FrmPreaSubCreditoAplicarResponse>
                {
                    Code = -1,
                    Description = "Debe seleccionar el tipo de documento.",
                    Result = new FrmPreaSubCreditoAplicarResponse()
                };
            }

            if (string.IsNullOrWhiteSpace(request.cuenta))
            {
                return new ErrorDto<FrmPreaSubCreditoAplicarResponse>
                {
                    Code = -1,
                    Description = "Debe seleccionar la cuenta.",
                    Result = new FrmPreaSubCreditoAplicarResponse()
                };
            }

            if (request.operacion <= 0)
            {
                return new ErrorDto<FrmPreaSubCreditoAplicarResponse>
                {
                    Code = -1,
                    Description = "Debe seleccionar la operación.",
                    Result = new FrmPreaSubCreditoAplicarResponse()
                };
            }

            request.usuario = request.usuario?.Trim() ?? string.Empty;
            request.cod_preanalisis = request.cod_preanalisis.Trim();
            request.tipo_documento = request.tipo_documento.Trim().ToUpperInvariant();
            request.cuenta = request.cuenta.Trim();

            return _db.Prea_frmPreaSubCredito_Aplicar(codEmpresa, request);
        }

        public ErrorDto<FrmPreaSubCreditoCuentasResponse> Prea_frmPreaSubCredito_Cuentas_Obtener(
            int codEmpresa,
            FrmPreaSubCreditoCuentasRequest request)
        {
            if (request is null)
            {
                return new ErrorDto<FrmPreaSubCreditoCuentasResponse>
                {
                    Code = -1,
                    Description = "Debe indicar la información de la consulta.",
                    Result = new FrmPreaSubCreditoCuentasResponse()
                };
            }

            if (string.IsNullOrWhiteSpace(request.cedula))
            {
                return new ErrorDto<FrmPreaSubCreditoCuentasResponse>
                {
                    Code = -1,
                    Description = "Debe indicar la cédula.",
                    Result = new FrmPreaSubCreditoCuentasResponse()
                };
            }

            if (request.banco <= 0)
            {
                return new ErrorDto<FrmPreaSubCreditoCuentasResponse>
                {
                    Code = -1,
                    Description = "Debe seleccionar el banco.",
                    Result = new FrmPreaSubCreditoCuentasResponse()
                };
            }

            request.usuario = request.usuario?.Trim() ?? string.Empty;
            request.cedula = request.cedula.Trim();

            return _db.Prea_frmPreaSubCredito_Cuentas_Obtener(codEmpresa, request);
        }

    }
}
