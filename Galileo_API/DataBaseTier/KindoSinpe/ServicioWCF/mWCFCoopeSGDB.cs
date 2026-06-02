using Galileo.Models.ERROR;
using Galileo_API.Controllers.WFCSinpe;
using Galileo_API.Models.ProGrX.Bancos;

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
        public ErrorDto fxValidacionSinpe(int CodEmpresa, string solicitud, string usuario, string? tipo = "PIN")
        {
            return EjecutarOperacion(
                () => _cliente.fxValidacionSinpe(CodEmpresa, solicitud, usuario,  tipo),
                "Error al validar Sinpe"
            );
        }

        public ErrorDto fxValidacionSinpeTransaccion(int CodEmpresa, string cedula, string cuenta, string usuario)
        {
            return EjecutarOperacion(
                () => _cliente.fxValidacionSinpeTransaccion(CodEmpresa, cedula, cuenta, usuario),
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

        public ErrorDto fxTesEmisionSinpeTiempoReal(
            int CodEmpresa, int Nsolicitud, DateTime vfecha, string vUsuario, int doc_base, int contador)
        {
            return EjecutarOperacion(
                () => _cliente.fxTesEmisionSinpeTiempoReal(CodEmpresa, Nsolicitud, vfecha, vUsuario, doc_base, contador),
                "Error al fxTesEmisionSinpeTiempoReal"
            );
        }

        public ErrorDto ConsultaCuentaSinpe(int CodEmpresa, TesConsultaCuentaSinpeModels cuenta)
        {
           return EjecutarOperacion(
                () => _cliente.ConsultaCuentaSinpe(CodEmpresa, cuenta),
                "Error al consultar cuenta Sinpe"
            );
        }

        #endregion
    }
}