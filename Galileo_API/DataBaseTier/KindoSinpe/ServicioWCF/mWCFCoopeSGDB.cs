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

        #region Validaciones Galileo -> Kindo

        /// <summary>
        /// Servicio para Validacion SINPE ASECCSS
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto fxValidacionSinpe(int CodEmpresa, string solicitud, string usuario)
        {
            try
            {
                var request = _cliente.fxValidacionSinpe(CodEmpresa, solicitud, usuario);
                return new ErrorDto
                {
                    Code = request.Code,
                    Description = request.Description
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = $"Error al validar Sinpe: {ex.Message}"
                };
            }
        }

        #endregion


        #region Validaciones Kindo -> Galilo

        #endregion

        /// <summary>
        /// Servicio para Validacion SINPE ASECCSS
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Nsolicitud"></param>
        /// <param name="vfecha"></param>
        /// <param name="vUsuario"></param>
        /// <param name="doc_base"></param>
        /// <param name="contador"></param>
        /// <returns></returns>
        public ErrorDto fxTesEmisionSinpeCreditoDirecto(int CodEmpresa, int Nsolicitud, DateTime vfecha, string vUsuario, int doc_base, int contador)
        {
            try
            {
                var request = _cliente.fxTesEmisionSinpeCreditoDirecto(CodEmpresa, Nsolicitud, vfecha, vUsuario, doc_base, contador);
                return new ErrorDto
                {
                    Code = request.Code,
                    Description = request.Description
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = $"Error al TesEmisionSinpeCreditoDirecto: {ex.Message}"
                };
            }
        }

    }
}
