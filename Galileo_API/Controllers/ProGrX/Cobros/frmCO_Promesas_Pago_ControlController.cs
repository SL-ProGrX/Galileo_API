using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCOPromesasPagoControlController : ControllerBase
    {
        private readonly FrmCOPromesasPagoControlBL _bl;

        public FrmCOPromesasPagoControlController(IConfiguration config)
        {
            _bl = new FrmCOPromesasPagoControlBL(config);
        }

        [Authorize]
        [HttpGet("PromesasPago_Usuarios_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> PromesasPago_Usuarios_Obtener(int codEmpresa)
        {
            return _bl.PromesasPago_Usuarios_Obtener(codEmpresa);
        }

        [Authorize]
        [HttpPost("PromesasPago_Consulta")]
        public ErrorDto<List<PromesasPagoConsultaResult>> PromesasPago_Consulta([FromBody] PromesasPagoConsultaParams param)
        {
            return _bl.PromesasPago_Consulta(param);
        }
    }
}
