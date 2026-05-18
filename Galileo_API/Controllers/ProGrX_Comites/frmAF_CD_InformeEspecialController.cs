using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Comites;
using Galileo_API.Models.ProGrX_Comites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Comites
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmAfCdInformeEspecialController : ControllerBase
    {
        private readonly FrmAfCdInformeEspecialBl _bl;

        public FrmAfCdInformeEspecialController(IConfiguration config)
            => _bl = new FrmAfCdInformeEspecialBl(config);

        [HttpGet("AfCdInformeEspecial_Pantalla_Obtener")]
        public ErrorDto<AfCdInformeEspecialPantallaData> AfCdInformeEspecial_Pantalla_Obtener(int codEmpresa)
        {
            return _bl.AfCdInformeEspecial_Pantalla_Obtener(codEmpresa);
        }

        [HttpGet("AfCdInformeEspecial_Comites_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AfCdInformeEspecial_Comites_Obtener(int codEmpresa, string codZona)
        {
            return _bl.AfCdInformeEspecial_Comites_Obtener(codEmpresa, codZona);
        }

        [HttpGet("AfCdInformeEspecial_Unidades_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AfCdInformeEspecial_Unidades_Obtener(int codEmpresa, string codComite)
        {
            return _bl.AfCdInformeEspecial_Unidades_Obtener(codEmpresa, codComite);
        }
    }
}
