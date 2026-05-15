using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmPreaBallonPaymentBL
    {
        private readonly FrmPreaBallonPaymentDB _db;
        private const string validaExpediente = "Debe indicar el expediente.";
        public FrmPreaBallonPaymentBL(IConfiguration config)
        {
            _db = new FrmPreaBallonPaymentDB(config);
        }

        public ErrorDto<FrmPreaBallonPaymentCargarResponse> Prea_frmPrea_BallonPayment_Cargar(
            int codEmpresa,
            string usuario,
            string cod_preanalisis)
        {
            if (string.IsNullOrWhiteSpace(cod_preanalisis))
            {
                return new ErrorDto<FrmPreaBallonPaymentCargarResponse>
                {
                    Code = -1,
                    Description = validaExpediente,
                    Result = new FrmPreaBallonPaymentCargarResponse()
                };
            }

            return _db.Prea_frmPrea_BallonPayment_Cargar(codEmpresa, usuario, cod_preanalisis);
        }

        public ErrorDto<FrmPreaBallonPaymentTablaPagosResponse> Prea_frmPrea_BallonPayment_TablaPagos_Obtener(
            int codEmpresa,
            string usuario,
            string cod_preanalisis)
        {
            if (string.IsNullOrWhiteSpace(cod_preanalisis))
            {
                return new ErrorDto<FrmPreaBallonPaymentTablaPagosResponse>
                {
                    Code = -1,
                    Description = validaExpediente,
                    Result = new FrmPreaBallonPaymentTablaPagosResponse()
                };
            }

            return _db.Prea_frmPrea_BallonPayment_TablaPagos_Obtener(codEmpresa, usuario, cod_preanalisis);
        }

        public ErrorDto<FrmPreaBallonPaymentCalcularResponse> Prea_frmPrea_BallonPayment_Calcular(
            int codEmpresa,
            FrmPreaBallonPaymentCalcularRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaBallonPaymentCalcularResponse>
                {
                    Code = -1,
                    Description = validaExpediente,
                    Result = new FrmPreaBallonPaymentCalcularResponse()
                };
            }

            if (request.monto <= 0)
            {
                return new ErrorDto<FrmPreaBallonPaymentCalcularResponse>
                {
                    Code = -1,
                    Description = "El monto debe ser mayor a cero.",
                    Result = new FrmPreaBallonPaymentCalcularResponse()
                };
            }

            if (request.tasa <= 0)
            {
                return new ErrorDto<FrmPreaBallonPaymentCalcularResponse>
                {
                    Code = -1,
                    Description = "La tasa debe ser mayor a cero.",
                    Result = new FrmPreaBallonPaymentCalcularResponse()
                };
            }

            if (request.plazo <= 0)
            {
                return new ErrorDto<FrmPreaBallonPaymentCalcularResponse>
                {
                    Code = -1,
                    Description = "El plazo debe ser mayor a cero.",
                    Result = new FrmPreaBallonPaymentCalcularResponse()
                };
            }

            if (request.periodicidad <= 0)
            {
                return new ErrorDto<FrmPreaBallonPaymentCalcularResponse>
                {
                    Code = -1,
                    Description = "Debe indicar el tipo de pago.",
                    Result = new FrmPreaBallonPaymentCalcularResponse()
                };
            }

            if (request.cuota_balloon < 0)
            {
                return new ErrorDto<FrmPreaBallonPaymentCalcularResponse>
                {
                    Code = -1,
                    Description = "La cuota balloon no es válida.",
                    Result = new FrmPreaBallonPaymentCalcularResponse()
                };
            }

            return _db.Prea_frmPrea_BallonPayment_Calcular(codEmpresa, request);
        }

        public ErrorDto<FrmPreaBallonPaymentCondicionesGuardarResponse> Prea_frmPrea_BallonPayment_Condiciones_Guardar(
            int codEmpresa,
            FrmPreaBallonPaymentCondicionesGuardarRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaBallonPaymentCondicionesGuardarResponse>
                {
                    Code = -1,
                    Description = validaExpediente,
                    Result = new FrmPreaBallonPaymentCondicionesGuardarResponse()
                };
            }

            return _db.Prea_frmPrea_BallonPayment_Condiciones_Guardar(codEmpresa, request);
        }

        public ErrorDto<FrmPreaBallonPaymentGuardarResponse> Prea_frmPrea_BallonPayment_Guardar(
            int codEmpresa,
            FrmPreaBallonPaymentGuardarRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaBallonPaymentGuardarResponse>
                {
                    Code = -1,
                    Description = validaExpediente,
                    Result = new FrmPreaBallonPaymentGuardarResponse()
                };
            }

            if (request.periodicidad <= 0)
            {
                return new ErrorDto<FrmPreaBallonPaymentGuardarResponse>
                {
                    Code = -1,
                    Description = "Debe indicar el tipo de pago.",
                    Result = new FrmPreaBallonPaymentGuardarResponse()
                };
            }

            if (request.tasa <= 0 || request.plazo <= 0 || request.monto <= 0)
            {
                return new ErrorDto<FrmPreaBallonPaymentGuardarResponse>
                {
                    Code = -1,
                    Description = "Los datos del Balloon Payment no son válidos.",
                    Result = new FrmPreaBallonPaymentGuardarResponse()
                };
            }

            return _db.Prea_frmPrea_BallonPayment_Guardar(codEmpresa, request);
        }
    }
}
