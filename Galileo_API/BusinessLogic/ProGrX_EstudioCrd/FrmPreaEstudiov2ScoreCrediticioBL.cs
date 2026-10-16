using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public sealed class FrmPreaEstudiov2ScoreCrediticioBL
    {
        private const int EmpresaHabilitada = 61;
        private const string MensajeEmpresaNoHabilitada = "Score Crediticio está disponible únicamente para la empresa 61.";
        private readonly FrmPreaEstudiov2ScoreCrediticioDB _db;
        private readonly FrmPreaEstudiov2CreditForceClient _creditForce;

        public FrmPreaEstudiov2ScoreCrediticioBL(IConfiguration config, IHostEnvironment? environment = null)
        {
            _db = new FrmPreaEstudiov2ScoreCrediticioDB(config);
            _creditForce = new FrmPreaEstudiov2CreditForceClient(config, environment: environment);
        }

        public ErrorDto<PreaScoreCrediticioHistorialDto> Consultar(
            int codEmpresa, string codPreanalisis, string cedula)
        {
            if (codEmpresa != EmpresaHabilitada)
            {
                return new ErrorDto<PreaScoreCrediticioHistorialDto>
                {
                    Code = -1,
                    Description = MensajeEmpresaNoHabilitada,
                    Result = new PreaScoreCrediticioHistorialDto()
                };
            }

            if (string.IsNullOrWhiteSpace(codPreanalisis)
                || codPreanalisis.Trim().Length > 20 || string.IsNullOrWhiteSpace(cedula))
            {
                return new ErrorDto<PreaScoreCrediticioHistorialDto>
                {
                    Code = -1,
                    Description = "Debe indicar la empresa, el expediente y la identificación válidos.",
                    Result = new PreaScoreCrediticioHistorialDto()
                };
            }

            return _db.Consultar(codEmpresa, codPreanalisis.Trim(), cedula.Trim());
        }

        public async Task<ErrorDto<PreaScoreCrediticioRegistroDto>> ActualizarAsync(
            int codEmpresa, PreaScoreCrediticioRequest request, string usuario, string ip,
            CancellationToken cancellationToken)
        {
            var respuesta = new ErrorDto<PreaScoreCrediticioRegistroDto>();
            if (codEmpresa != EmpresaHabilitada)
            {
                respuesta.Code = -1;
                respuesta.Description = MensajeEmpresaNoHabilitada;
                return respuesta;
            }
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis)
                || request.cod_preanalisis.Trim().Length > 20 || string.IsNullOrWhiteSpace(request.cedula)
                || string.IsNullOrWhiteSpace(usuario) || usuario.Length > 30)
            {
                respuesta.Code = -1;
                respuesta.Description = "Debe indicar empresa, expediente, identificación y usuario autenticado válidos.";
                return respuesta;
            }

            try
            {
                var expediente = request.cod_preanalisis.Trim();
                var cedula = request.cedula.Trim();
                if (!_db.PerteneceExpediente(codEmpresa, expediente, cedula))
                    throw new InvalidOperationException("La identificación no corresponde al expediente seleccionado.");

                var detalle = await _creditForce.ObtenerComportamientoAsync(cedula, usuario, ip, cancellationToken);
                if (detalle is null)
                {
                    respuesta.Code = 1;
                    respuesta.Description = "Sin historial o datos suficientes en CreditForce.";
                    return respuesta;
                }

                cancellationToken.ThrowIfCancellationRequested();
                respuesta.Result = _db.Guardar(codEmpresa, expediente, cedula, usuario, detalle);
                respuesta.Code = 0;
                respuesta.Description = "Score crediticio obtenido y registrado correctamente.";
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                respuesta.Code = -1;
                respuesta.Description = "CreditForce excedió el tiempo de espera. Intente nuevamente.";
            }
            catch (Exception ex)
            {
                respuesta.Code = -1;
                respuesta.Description = ex.Message;
            }

            return respuesta;
        }
    }
}
