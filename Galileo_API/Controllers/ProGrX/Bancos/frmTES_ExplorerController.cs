using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.BusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmTesExplorerController : ControllerBase
    {
        private readonly FrmTesExplorerBL _bl;

        public FrmTesExplorerController(IConfiguration config)
        {
            _bl = new FrmTesExplorerBL(config);
        }

        [Authorize]
        [HttpGet("Tes_Bancos_Obtener")]
        public ErrorDto<List<TesDropDownListaBancosExplorer>> Tes_Bancos_Obtener(int CodEmpresa)
        {
            return _bl.Tes_Bancos_Obtener(CodEmpresa);
        }
        [Authorize]
        [HttpGet("Tes_explorer_Obtener")]
        public ErrorDto<TablasListaGenericaModel> TES_explorer_Obtener(int CodEmpresa, string filtrosExplorer, string filtros)
        {
            return _bl.TES_explorer_Obtener(CodEmpresa, filtrosExplorer, filtros);
        }


    }
}
