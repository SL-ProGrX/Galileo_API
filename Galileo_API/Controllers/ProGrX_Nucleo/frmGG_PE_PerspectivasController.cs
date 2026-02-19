using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.GG_PE;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmGgPePerspectivasController : ControllerBase
    {
        private readonly FrmGgPePerspectivasBL BL_GG_PE_Perspectivas;
        public FrmGgPePerspectivasController(IConfiguration config)
        {
            BL_GG_PE_Perspectivas = new FrmGgPePerspectivasBL(config);
        }

        [HttpGet("PePerspectiva_Obtener")]
        public ErrorDto<PePerspectivasDto> PePerspectiva_Obtener(int CodEmpresa, int perspectiva)
        {
            return BL_GG_PE_Perspectivas.PePerspectiva_Obtener(CodEmpresa, perspectiva);
        }

        [HttpGet("PePerspectiva_Scroll")]
        public ErrorDto<PePerspectivasDto> PePerspectiva_Scroll(int CodEmpresa, int scroll, int? perspectiva)
        {
            return BL_GG_PE_Perspectivas.PePerspectiva_Scroll(CodEmpresa, scroll, perspectiva);
        }

        [HttpPost("PePerspectiva_Guardar")]
        public ErrorDto PePerspectiva_Guardar(int CodEmpresa, PePerspectivasDto perspectiva)
        {
            return BL_GG_PE_Perspectivas.PePerspectiva_Guardar(CodEmpresa, perspectiva);
        }

        [HttpDelete("PePerspectiva_Eliminar")]
        public ErrorDto PePerspectiva_Eliminar(int CodEmpresa, int perspectiva)
        {
            return BL_GG_PE_Perspectivas.PePerspectiva_Eliminar(CodEmpresa, perspectiva);
        }

        [HttpGet("PePlanesLista_Obtener")]
        public ErrorDto<List<PePerspectivasDto>> PePlanesLista_Obtener(int CodEmpresa)
        {
            return BL_GG_PE_Perspectivas.PePlanesLista_Obtener(CodEmpresa);
        }

        [HttpGet("PePerpectivasLista_Obtener")]
        public ErrorDto<PePerspectivasDatosLista> PePerpectivasLista_Obtener(int CodEmpresa, string filtros)
        {
            return BL_GG_PE_Perspectivas.PePerpectivasLista_Obtener(CodEmpresa, filtros);
        }
    }
}