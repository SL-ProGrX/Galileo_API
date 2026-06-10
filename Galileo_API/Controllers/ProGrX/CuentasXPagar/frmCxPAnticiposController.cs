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
    public class FrmCxPAnticiposController : ControllerBase
    {

       private readonly FrmCxPAnticiposBL _bl;
        public FrmCxPAnticiposController(IConfiguration config)
        {
            _bl = new FrmCxPAnticiposBL(config);
        }

        [HttpPost("ExeAnticipos")]
        public ErrorDto ExeAnticipos(int CodCliente, string filtros)
        {
            return _bl.ExeAnticipos(CodCliente, filtros);
        }

        [HttpGet("ObtenerCargos")]
        public ErrorDto<List<CargoDto>> ObtenerCargos(int CodCliente)
        {
            return _bl.ObtenerCargos(CodCliente);
        }

        [HttpGet("ObtenerAdelantosRegistrados")]
        public ErrorDto<List<AdelantoRegistradoDto>> ObtenerAdelantosRegistrados(int CodCliente, int Proveedor)
        {
            return _bl.ObtenerAdelantosRegistrados(CodCliente, Proveedor);
        }

        [HttpPost("ObtenerHistorialDePagos")]
        public ErrorDto<List<HistorialPagoDto>> ObtenerHistorialDePagos(int CodCliente, int Proveedor, string Anticipos)
        {
            return _bl.ObtenerHistorialDePagos(CodCliente, Proveedor, Anticipos);
        }

        [HttpGet("ObtenerProveedores")]
        public ErrorDto<List<Proveedor>> ObtenerProveedores(int CodEmpresa)
        {
            return _bl.ObtenerProveedores(CodEmpresa);
        }

        [HttpGet("ConsecutivoAdelanto")]
        public ErrorDto ConsecutivoAdelanto(int CodEmpresa, int Proveedor)
        {
            return _bl.ConsecutivoAdelanto(CodEmpresa, Proveedor);
        }
    }//end class
}//end namespace