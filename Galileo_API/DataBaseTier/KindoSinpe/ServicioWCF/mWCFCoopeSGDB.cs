using Galileo.Models.ERROR;
using Galileo_API.Controllers.WFCSinpe;

namespace Galileo_API.DataBaseTier
{
    public class MSrvWfcCoopeSg : IWfcSinpe
    {
        private readonly CoopeSanGabrielValidator _cliente;

        public MSrvWfcCoopeSg(IConfiguration config)
        {
            _cliente = new CoopeSanGabrielValidator(config);
        }

        #region Métodos comunes

        private ErrorDto EjecutarOperacion(Func<dynamic> operacion, string mensajeError)
        {
            try
            {
                var result = operacion();
                return new ErrorDto
                {
                    Code = result.Code,
                    Description = result.Description
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = $"{mensajeError}: {ex.Message}"
                };
            }
        }

        #endregion

        #region Validaciones Galileo -> Kindo

        /// <summary>
        /// Servicio para Validación SINPE CSG.
        /// </summary>
        public ErrorDto fxValidacionSinpe(int CodEmpresa, string solicitud, string usuario)
        {
            return EjecutarOperacion(
                () => _cliente.fxValidacionSinpe(CodEmpresa, solicitud, usuario),
                "Error al validar Sinpe"
            );
        }

        /// <summary>
        /// Servicio para emisión de crédito directo SINPE CSG.
        /// </summary>
        public ErrorDto fxTesEmisionSinpeCreditoDirecto(
            int CodEmpresa, int Nsolicitud, DateTime vfecha, string vUsuario, int doc_base, int contador)
        {
            return EjecutarOperacion(
                () => _cliente.fxTesEmisionSinpeCreditoDirecto(CodEmpresa, Nsolicitud, vfecha, vUsuario, doc_base, contador),
                "Error al TesEmisionSinpeCreditoDirecto"
            );
        }

        #endregion
    }
}