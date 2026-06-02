using Galileo_API.BusinessLogic.ProGrX.Clientes;
using Galileo.Models.ERROR;
using Galileo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFReportesRenunciasController : ControllerBase
    {
        private readonly FrmAFReportesRenunciasBL _bl;

        public FrmAFReportesRenunciasController(IConfiguration config)
        {
            _bl = new FrmAFReportesRenunciasBL(config);
        }

        [Authorize]
        [HttpGet("AfReportesRenunciasOficinas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AfReportesRenunciasOficinas_Obtener(int codEmpresa)
        {
            return _bl.AfReportesRenunciasOficinas_Obtener(codEmpresa);
        }
    }
}