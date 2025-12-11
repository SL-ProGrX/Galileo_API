using Galileo.Models.ERROR;

namespace Galileo_API.Controllers.WFCSinpe
{
    public interface IWfcSinpe
    {
        #region KINDO
        /// <summary>
        /// Servicio Kindo
        /// </summary>
        /// <param name="vUsuario"></param>
        /// <returns></returns>
        ErrorDto<bool> ServicioDisponible(string vUsuario)
        {
            // Implementación por defecto
            return new ErrorDto<bool>
            {
                Code = 0,
                Description = "No implementado",
                Result = false
            };
        }

        #endregion

        #region ASECCSS

        /// <summary>
        /// Servicio ASECCSS
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        ErrorDto fxValidacionSinpe(int CodEmpresa, string solicitud, string usuario)
        {
            // Implementación por defecto
            return new ErrorDto
            {
                Code = 0,
                Description = "No implementado",
            };
        }

        /// <summary>
        /// Servicio ASECCSS
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Nsolicitud"></param>
        /// <param name="vfecha"></param>
        /// <param name="vUsuario"></param>
        /// <param name="doc_base"></param>
        /// <param name="contador"></param>
        /// <returns></returns>
        ErrorDto fxTesEmisionSinpeCreditoDirecto(int CodEmpresa,
            int Nsolicitud, DateTime vfecha, string vUsuario, int doc_base, int contador)
        {
            // Implementación por defecto
            return new ErrorDto
            {
                Code = 0,
                Description = "No implementado",
            };
        }

        /// <summary>
        ///  Servicio ASECCSS
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Nsolicitud"></param>
        /// <param name="vfecha"></param>
        /// <param name="vUsuario"></param>
        /// <param name="doc_base"></param>
        /// <param name="contador"></param>
        /// <returns></returns>
        ErrorDto fxTesEmisionSinpeTiempoReal(int CodEmpresa, int Nsolicitud, DateTime vfecha, string vUsuario, int doc_base, int contador)
        {
            // Implementación por defecto
            return new ErrorDto
            {
                Code = 0,
                Description = "No implementado",
            };
        }

        /// <summary>
        /// Servicio ASECCSS
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pCedula"></param>
        /// <param name="pNumeroDocumento"></param>
        /// <param name="pTipoDoc"></param>
        /// <param name="pTipoDocEletronico"></param>
        /// <param name="pNotas"></param>
        /// <param name="pTipoTramite"></param>
        /// <returns></returns>
        ErrorDto<bool> GenerarFacturacionElectronica(int CodEmpresa,
           string pCedula,
           string pNumeroDocumento, string pTipoDoc,
           byte pTipoDocEletronico, string pNotas, string pTipoTramite)
        {
            // Implementación por defecto
            return new ErrorDto<bool>
            {
                Code = 0,
                Description = "No implementado",
                Result = false
            };
        }

        #endregion
    }

}
