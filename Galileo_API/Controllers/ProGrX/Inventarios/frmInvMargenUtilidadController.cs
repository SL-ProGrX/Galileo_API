using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;
using Galileo.Models.INV;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmInvMargenUtilidadController : ControllerBase
    {
        private readonly FrmInvMargenUtilidadBL _bl;
        public FrmInvMargenUtilidadController(IConfiguration config)
        {
            _bl = new FrmInvMargenUtilidadBL(config);
        }

        [HttpGet("Linea_Obtener")]
        public ErrorDto<List<LineaDto>> Linea_Obtener(int CodEmpresa)
        {
            return _bl.Linea_Obtener(CodEmpresa);
        }

        [HttpGet("SubLinea_Obtener")]
        public ErrorDto<List<SubLineaDto>> SubLinea_Obtener(int CodEmpresa)
        {
            return _bl.SubLinea_Obtener(CodEmpresa);
        }

        [HttpGet("ListadoPrecios_Obtener")]
        public ErrorDto<List<PrecioDto>> ListadoPrecios_Obtener(int CodEmpresa)
        {
            return _bl.ListadoPrecios_Obtener(CodEmpresa);
        }

        [HttpPost("cambio_margen")]
        public ErrorDto cambio_margen(int CodEmpresa, int monto, int cod_linea, int cod_sublinea, string cambio_margen)
        {
            return _bl.cambio_margen(CodEmpresa, monto, cod_linea, cod_sublinea, cambio_margen);
        }
    }
}