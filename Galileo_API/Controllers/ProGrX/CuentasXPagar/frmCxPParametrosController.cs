using Microsoft.AspNetCore.Mvc;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Galileo.BusinessLogic.ProGrX.CxP;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmCxPParametrosController : ControllerBase
    {
        private readonly FrmCxPParametrosBL _bl;

        public FrmCxPParametrosController(IConfiguration config)
        {
            _bl = new FrmCxPParametrosBL(config);
        }

        [HttpGet("ObtenerParametros")]
        public ErrorDto<List<ParametrosDto>> ObtenerParametros(int CodCliente)
        {
            return _bl.ObtenerParametros(CodCliente);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("ExecParametros")]
        public ErrorDto ExecParametros(int CodCliente)
        {
            return _bl.ExecParametros(CodCliente);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("ActualizarParametros")]
        public ErrorDto ActualizarParametros(int CodCliente, string Usuario, string Valor, string Parametro)
        {
            return _bl.ActualizarParametros(CodCliente, Usuario, Valor, Parametro);
        }
    }
}