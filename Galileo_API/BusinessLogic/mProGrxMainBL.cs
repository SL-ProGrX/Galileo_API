using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class MProGrxMainBL
    {
        private readonly IConfiguration _config;

        public MProGrxMainBL(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<ConfiguracionEmpresaSesionDto> ConfiguracionEmpresaSesion_Obtener(
            int codEmpresa,
            string usuario)
        {
            var globalesResponse = new MProGrxMain(_config)
                .sbSifParametrosInicializa(codEmpresa, usuario);

            if (globalesResponse?.Code != 0 || globalesResponse.Result is null)
            {
                return DbHelper.CreateErrorResponse<ConfiguracionEmpresaSesionDto>(
                    globalesResponse?.Description
                        ?? "No fue posible obtener la configuración de la empresa.",
                    globalesResponse?.Code ?? -1);
            }

            var globales = globalesResponse.Result;
            return DbHelper.CreateOkResponse(new ConfiguracionEmpresaSesionDto
            {
                CodEmpresaEnlace = globales.GEnlace,
                Nombre = globales.GstrNombreEmpresa,
                SysCrdPlanPago = globales.SysPlanPagos,
                SysDocVersion = globales.SysDocVersion,
                SysTesVersion = globales.SysTesVersion,
                SysPreaVersion = globales.SysPreaVersion,
                SysCcssInd = globales.SysASEVersion ? 1 : 0
            });
        }
    }
}
