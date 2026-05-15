using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndPlanesCopiaController : ControllerBase
    {
        private readonly FrmFndPlanesCopiaBl _BL;

        public FrmFndPlanesCopiaController(IConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _BL = new FrmFndPlanesCopiaBl(config);
        }

        [Authorize]
        [HttpGet("FND_Planes_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_Planes_Obtener(int CodEmpresa)
        {
            return _BL.FND_Planes_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_Plan_Scroll_Obtener")]
        public ErrorDto<DropDownListaGenericaModel> AF_Plan_Scroll_Obtener(int CodEmpresa,string plan, int scrollCode)
        {
            return _BL.AF_Plan_Scroll_Obtener(CodEmpresa, plan, scrollCode);
        }


        [Authorize]
        [HttpPost("FND_Planes_Copiar")]
        public ErrorDto FND_Planes_Copiar(int CodEmpresa, string usuario, FndPlanesCopiaRequestDto dto)
        {
            return _BL.FND_Planes_Copiar(CodEmpresa, usuario, dto);
        }
    }
}