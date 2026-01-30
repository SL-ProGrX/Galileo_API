
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc; 

namespace Galileo_API.Controllers.ProGrX.CuentasxCobrar
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCxCConsultaController : ControllerBase
    {
        private readonly FrmCxCConsultaBL _bl;

        public FrmCxCConsultaController(IConfiguration config)
            => _bl = new FrmCxCConsultaBL(config);

        [Authorize]
        [HttpPost("CxCClientesClasifica_Guardar")]
        public ErrorDto CxCCargosTCxCClientesClasifica_Guardaripos_Guardar(int CodEmpresa, string usuario, [FromBody] CxCConsultaData datos)
        {
            return _bl.CxCClientesClasifica_Guardar(CodEmpresa, usuario, datos);
        }
    }
}