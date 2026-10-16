using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Convenios;
using Galileo_API.Models.ProGrX.Convenios;

namespace Galileo_API.BusinessLogic.ProGrX.Convenios
{
    public class FrmCrConveniosAhorrosBl
    {
        private const string MensajeConvenioRequerido =
            "El convenio es requerido.";

        private const string MensajeOrdenRequerida =
            "El número de orden es requerido.";

        private readonly FrmCrConveniosAhorrosDb _db;

        public FrmCrConveniosAhorrosBl(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _db = new FrmCrConveniosAhorrosDb(config);
        }

        public ErrorDto<CrConveniosAhorrosData?>
            CR_Convenios_Ahorros_Obtener(
                int codEmpresa,
                string? codConvenio,
                int codOrden)
        {
            if (string.IsNullOrWhiteSpace(codConvenio))
            {
                return DbHelper.CreateErrorResponse
                    <CrConveniosAhorrosData?>(
                        MensajeConvenioRequerido,
                        -2);
            }

            if (codOrden <= 0)
            {
                return DbHelper.CreateErrorResponse
                    <CrConveniosAhorrosData?>(
                        MensajeOrdenRequerida,
                        -2);
            }

            return _db.CR_Convenios_Ahorros_Obtener(
                codEmpresa,
                codConvenio,
                codOrden);
        }

        public ErrorDto<CrConveniosAhorrosData?>
            CR_Convenios_Ahorros_Monto_Actualizar(
                int codEmpresa,
                CrConveniosAhorrosActualizarRequest? request)
        {
            var mensajeValidacion =
                CR_Convenios_Ahorros_Solicitud_Validar(request);

            if (!string.IsNullOrEmpty(mensajeValidacion))
            {
                return DbHelper.CreateErrorResponse
                    <CrConveniosAhorrosData?>(
                        mensajeValidacion,
                        -2);
            }

            return _db.CR_Convenios_Ahorros_Monto_Actualizar(
                codEmpresa,
                request!);
        }

        private static string CR_Convenios_Ahorros_Solicitud_Validar(
            CrConveniosAhorrosActualizarRequest? request)
        {
            if (request is null)
            {
                return "La solicitud es requerida.";
            }

            if (string.IsNullOrWhiteSpace(request.cod_convenio))
            {
                return MensajeConvenioRequerido;
            }

            if (request.cod_orden <= 0)
            {
                return MensajeOrdenRequerida;
            }

            if (string.IsNullOrWhiteSpace(request.cod_plan))
            {
                return "El plan de ahorro es requerido.";
            }

            if (request.cod_contrato <= 0)
            {
                return "El contrato es requerido.";
            }

            return string.Empty;
        }
    }
}