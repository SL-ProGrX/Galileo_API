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
    public class FrmGgPePlanesController : ControllerBase
    {
        private readonly FrmGgPePlanesBL BL_GG_PE_Planes;
        public FrmGgPePlanesController(IConfiguration config)
        {
            BL_GG_PE_Planes = new FrmGgPePlanesBL(config);
        }

        [HttpGet("PePlanesLista_Obtener")]
        public ErrorDto<PePlanesDatosLista> PePlanesLista_Obtener(int CodEmpresa, string filtros)
        {
            return BL_GG_PE_Planes.PePlanesLista_Obtener(CodEmpresa, filtros);
        }

        [HttpPost("PePlanes_Guardar")]
        public ErrorDto PePlanes_Guardar(int CodEmpresa, PePlanesDto plan)
        {
            return BL_GG_PE_Planes.PePlanes_Guardar(CodEmpresa, plan);
        }

        [HttpGet("PePlanes_Eliminar")]
        public ErrorDto PePlanes_Eliminar(int CodEmpresa, int pe_id)
        {
            return BL_GG_PE_Planes.PePlanes_Eliminar(CodEmpresa, pe_id);
        }

        [HttpGet("PePlanes_Exportar")]
        public ErrorDto<List<PePlanesDto>> PePlanes_Exportar(int CodEmpresa)
        {
            return BL_GG_PE_Planes.PePlanes_Exportar(CodEmpresa);
        }
    }
}