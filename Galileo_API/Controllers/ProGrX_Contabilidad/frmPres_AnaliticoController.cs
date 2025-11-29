using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;

namespace Galileo.Controllers
{
    [Route("api/frmPres_Analitico")]
    [Route("api/FrmPresAnalitico")]
    [ApiController]
    public class FrmPresAnaliticoController : ControllerBase
    {
        readonly FrmPresAnaliticoBl _bl;
        public FrmPresAnaliticoController(IConfiguration config)
        {
            _bl = new FrmPresAnaliticoBl(config);
        }

        [Authorize]
        [HttpGet("PresAnaliticoDesc_Obtener")]
        public ErrorDto<List<PresAnaliticoDescData>> PresAnaliticoDesc_Obtener(int CodCliente, string datos)
        {
            return _bl.PresAnaliticoDesc_Obtener(CodCliente, datos);
        }

        [Authorize]
        [HttpGet("PresAnalitico_Obtener")]
        public ErrorDto<List<PresAnaliticoData>> PresAnalitico_Obtener(int CodCliente, string datos)
        {
            return _bl.PresAnalitico_Obtener(CodCliente, datos);
        }
    }
}