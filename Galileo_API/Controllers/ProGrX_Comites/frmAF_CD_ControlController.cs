using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_CxC;
using Galileo_API.Models.ProGrX_Comites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_CxC
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmAfCdControlController : ControllerBase
    {
        private readonly FrmAfCdControlBl _bl;

        public FrmAfCdControlController(IConfiguration config)
        {
            _bl = new FrmAfCdControlBl(config);
        }

        [HttpPost("Listar")]
        public ErrorDto<List<AFCDCuentaDto>> Listar(int codEmpresa, AFCDCuentaFiltroDto filtro)
        {
            return _bl.Listar(codEmpresa, filtro);
        }

        [HttpGet("Tipos")]
        public ErrorDto<List<DropDownListaGenericaModel>> Tipos(int codEmpresa)
        {
            return _bl.Tipos(codEmpresa);
        }

        [HttpGet("Procesos")]
        public ErrorDto<List<DropDownListaGenericaModel>> Procesos(int codEmpresa)
        {
            return _bl.Procesos(codEmpresa);
        }

        [HttpGet("Estados")]
        public ErrorDto<List<DropDownListaGenericaModel>> Estados(int codEmpresa)
        {
            return _bl.Estados(codEmpresa);
        }

        [HttpGet("Comites")]
        public ErrorDto<List<DropDownListaGenericaModel>> Comites(int codEmpresa)
        {
            return _bl.Comites(codEmpresa);
        }


    }
}