using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCntXRastreoMovimientosController : ControllerBase
    {
        private readonly FrmCntXRastreoMovimientosBl _bl;

        public FrmCntXRastreoMovimientosController(IConfiguration config)
        {
            _bl = new FrmCntXRastreoMovimientosBl(config);
        }

        [Authorize]
        [HttpPost("Buscar")]
        public ErrorDto<List<RastreoMovimientosTablaDto>> Buscar(int codEmpresa,[FromBody] RastreoMovimientosFiltroDto filtros)
        {
            return _bl.Buscar(codEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Contabilidades_Buscar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Contabilidades_Buscar(int codEmpresa,string tipo)
        {
            return _bl.Contabilidades_Buscar(codEmpresa, tipo);
        }

        [Authorize]
        [HttpGet("Cuentas_Buscar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cuentas_Buscar(
            int codEmpresa,
            string tipo,
            int codigo)
        {
            return _bl.Cuentas_Buscar(codEmpresa, tipo, codigo);
        }
    }
}
