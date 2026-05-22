using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Personas;
using Galileo_API.BusinessLogic.ProGrX_Personas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Galileo_API.Controllers.ProGrX_Personas
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmAfNoCotizaRangosController : ControllerBase
    {
        private readonly FrmAfNoCotizaRangosBL _bl;

        public FrmAfNoCotizaRangosController(IConfiguration config)
        {
            _bl = new FrmAfNoCotizaRangosBL(config);
        }

        [Authorize]
        [HttpGet("AF_NoCotizaRangos_Obtener")]
        public ErrorDto<NoCotizaRangosLista> AF_NoCotizaRangos_Obtener(int codEmpresa, string filtros)
        {
            return _bl.AF_NoCotizaRangos_Obtener(codEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("AF_NoCotizaRangos_Guardar")]
        public ErrorDto AF_NoCotizaRangos_Guardar(int codEmpresa, string usuario, NoCotizaRangosData rango)
        {
            return _bl.AF_NoCotizaRangos_Guardar(codEmpresa, usuario, rango);
        }

        [Authorize]
        [HttpDelete("AF_NoCotizaRangos_Eliminar")]
        public ErrorDto AF_NoCotizaRangos_Eliminar(int codEmpresa, string usuario, int lineaId)
        {
            return _bl.AF_NoCotizaRangos_Eliminar(codEmpresa, usuario, lineaId);
        }

        [Authorize]
        [HttpGet("AF_NoCotizaRangos_Exportar")]
        public ErrorDto<NoCotizaRangosLista> AF_NoCotizaRangos_Exportar(int codEmpresa, string filtros)
        {
            return _bl.AF_NoCotizaRangos_Exportar(codEmpresa, filtros);
        }
    }
}
