using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier;
using Galileo_API.DataBaseTier.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;

namespace Galileo_API.BusinessLogic.ProGrX_Hipotecario
{
    public class FrmVivCorregirMontoCreditoBL
    {
        private readonly FrmVivCorregirMontoCreditoDB _db;
        private readonly ClsConsultarBD _clsConsultar;

        public FrmVivCorregirMontoCreditoBL(IConfiguration config)
        {
            _db = new FrmVivCorregirMontoCreditoDB(config);
            _clsConsultar = new ClsConsultarBD(config);
        }

        public ErrorDto<FrmVivCorregirMontoCreditoResponse> Viv_CorregirMontoCredito_Obtener(
            int codEmpresa,
            long numero_operacion)
        {
            if (numero_operacion <= 0)
            {
                return Error(
                    "Debe indicar una operación válida.",
                    new FrmVivCorregirMontoCreditoResponse());
            }

            return _db.Viv_CorregirMontoCredito_Obtener(codEmpresa, numero_operacion);
        }

        public ErrorDto<FrmVivCorregirMontoCreditoGuardarResponse> Viv_CorregirMontoCredito_Guardar(
            int codEmpresa,
            FrmVivCorregirMontoCreditoGuardarRequest request)
        {
            if (request.numero_operacion <= 0)
            {
                return Error(
                    "Debe indicar una operación válida.",
                    new FrmVivCorregirMontoCreditoGuardarResponse());
            }

            if (request.monto_credito <= 0)
            {
                return Error(
                    "El monto de la operación es inválido.",
                    new FrmVivCorregirMontoCreditoGuardarResponse());
            }

            if (request.monto_no_gravable < 0)
            {
                return Error(
                    "El monto no gravable es inválido.",
                    new FrmVivCorregirMontoCreditoGuardarResponse());
            }

            var estadoOperacion = _clsConsultar.fxEstadoOperacion(codEmpresa, request.numero_operacion);
            if (estadoOperacion.Code < 0)
            {
                return Error(
                    estadoOperacion.Description ?? "No fue posible validar el estado de la operación.",
                    new FrmVivCorregirMontoCreditoGuardarResponse());
            }

            string estado = (estadoOperacion.Result ?? string.Empty).Trim().ToUpperInvariant();
            if (estado != "R" && estado != "P")
            {
                return Error(
                    "Para modificar el monto del crédito la operación debe estar en estado Recibida o Pendiente.",
                    new FrmVivCorregirMontoCreditoGuardarResponse());
            }

            return _db.Viv_CorregirMontoCredito_Guardar(codEmpresa, request);
        }

        private static ErrorDto<T> Error<T>(string description, T result)
        {
            return new ErrorDto<T>
            {
                Code = -1,
                Description = description,
                Result = result
            };
        }
    }
}
