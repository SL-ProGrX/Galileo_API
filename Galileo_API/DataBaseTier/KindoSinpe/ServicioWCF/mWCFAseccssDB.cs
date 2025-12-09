using Galileo.Models.ERROR;
using Galileo_API.Controllers.WFCSinpe;


namespace Galileo_API.DataBaseTier
{
    public class mWFCAseccss : IWFCSinpe
    {
        private readonly IConfiguration _config;
        private readonly AseccssSinpeValidator _cliente;

        public mWFCAseccss(IConfiguration config)
        {
            _config = config;
            _cliente = new AseccssSinpeValidator(_config);
        }

        #region Validaciones Galilo -> Kindo

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Nsolicitud"></param>
        /// <param name="vfecha"></param>
        /// <param name="vUsuario"></param>
        /// <param name="doc_base"></param>
        /// <param name="contador"></param>
        /// <returns></returns>
        public ErrorDto fxTesEmisionSinpeTiempoReal(int CodEmpresa, int Nsolicitud, DateTime vfecha, string vUsuario, int doc_base, int contador)
        {
            try
            {
                var request = _cliente.fxTesEmisionSinpeTiempoReal(CodEmpresa, Nsolicitud, vfecha, vUsuario, doc_base, contador);
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
                    Description = $"Error al fxTesEmisionSinpeTiempoReal: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Servicio para Validacion SINPE ASECCSS
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pCedula"></param>
        /// <param name="pNumeroDocumento"></param>
        /// <param name="pTipoDoc"></param>
        /// <param name="pTipoDocEletronico"></param>
        /// <param name="pNotas"></param>
        /// <param name="pTipoTramite"></param>
        /// <returns></returns>
        public ErrorDto<bool> GenerarFacturacionElectronica(
            int CodEmpresa,
            string pCedula,
            string pNumeroDocumento,
            string pTipoDoc,
            byte pTipoDocEletronico,
            string pNotas,
            string pTipoTramite)
        {
            try
            {
                var request = _cliente.GenerarFacturacionElectronica(
                    CodEmpresa,
                    pCedula,
                    pNumeroDocumento,
                    pTipoDoc,
                    pTipoDocEletronico,
                    pNotas,
                    pTipoTramite);
                return new ErrorDto<bool>
                {
                    Result = request.Result,
                    Code = request.Code,
                    Description = request.Description
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto<bool>
                {
                    Result = false,
                    Code = -1,
                    Description = $"Error al fxTesEmisionSinpeTiempoReal: {ex.Message}"
                };
            }
        }

        #endregion



    }
}
