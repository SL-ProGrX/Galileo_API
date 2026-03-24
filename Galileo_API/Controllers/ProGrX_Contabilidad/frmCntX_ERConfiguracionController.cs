using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
 
    public class FrmCntXErConfiguracionController : ControllerBase
    {
        private readonly FrmCntXErConfiguracionBl _bl;

        public FrmCntXErConfiguracionController(IConfiguration config) => _bl = new FrmCntXErConfiguracionBl(config);

        [Authorize]
        [HttpGet("CargarTiposCuenta")]
        public ErrorDto<List<CntxTipoCuentaERDto>> CargarTiposCuenta(int codEmpresa,int codContabilidad,string tipo)
        {
            return _bl.CargarTiposCuenta(codEmpresa, codContabilidad, tipo);
        }

        [HttpPost("GuardarTiposCuenta")]
        public ErrorDto<bool> GuardarTiposCuenta(int codEmpresa,int codContabilidad,string usuario,string tipo, List<CntxTipoCuentaERDto> data)
        {
            return _bl.GuardarTiposCuenta(codEmpresa, codContabilidad, usuario, tipo, data);
        }


    }
}