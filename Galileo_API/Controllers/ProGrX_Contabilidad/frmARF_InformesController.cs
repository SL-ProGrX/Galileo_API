using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_ARF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_ARF
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmArfInformesController : ControllerBase
    {
        private readonly FrmArfInformesBl _bl;

        public FrmArfInformesController(IConfiguration config)
        {
            _bl = new FrmArfInformesBl(config);
        }

        [Authorize]
        [HttpGet]
        [Route("ARF_Unidades_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Unidades(int codEmpresa)
        {
            return _bl.ARF_Unidades_Listar(codEmpresa);
        }

        [Authorize]
        [HttpGet]
        [Route("ARF_Arrendadores_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Arrendadores(int codEmpresa)
        {
            return _bl.ARF_Arrendadores_Listar(codEmpresa);
        }
    }
}