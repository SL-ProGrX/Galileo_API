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
    public class FrmCxPCargosAdicionalesController : ControllerBase
    {
        private readonly FrmCxPCargosAdicionalesBL _bl;

        public FrmCxPCargosAdicionalesController(IConfiguration config)
        {
            _bl = new FrmCxPCargosAdicionalesBL(config);
        }

        [HttpGet("ObtenerCargosAdicionales")]
        public ErrorDto<List<CargosAdicionalDto>> ObtenerCargosAdicionales(int CodEmpresa)
        {
            return _bl.ObtenerCargosAdicionales(CodEmpresa);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("ExisteCargoAdicional")]
        public ErrorDto ExisteCargoAdicional(int CodEmpresa, string CodCargo)
        {
            return _bl.ExisteCargoAdicional(CodEmpresa, CodCargo);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("EliminarCargoAdicional")]
        public ErrorDto EliminarCargoAdicional(int CodEmpresa, string CodCargo)
        {
            return _bl.EliminarCargoAdicional(CodEmpresa, CodCargo);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("InsertarCargoAdicional")]
        public ErrorDto InsertarCargoAdicional(int CodEmpresa, CargosAdicionalDto Info)
        {
            return _bl.InsertarCargoAdicional(CodEmpresa, Info);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("ActualizarCargoAdicional")]
        public ErrorDto ActualizarCargoAdicional(int CodEmpresa, CargosAdicionalDto Info)
        {
            return _bl.ActualizarCargoAdicional(CodEmpresa, Info);
        }
    }
}