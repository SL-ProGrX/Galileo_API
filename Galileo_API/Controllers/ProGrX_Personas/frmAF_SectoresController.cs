using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Personas;
using Galileo_API.BusinessLogic.ProGrX_Personas;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Personas
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmAfSectoresController : ControllerBase
    {
        private readonly FrmAfSectoresBL _bl;

        public FrmAfSectoresController(IConfiguration config)
        {
            _bl = new FrmAfSectoresBL(config);
        }        

        [HttpGet("AF_Sectores_Obtener")]
        public ErrorDto<SectoresLista> AF_Sectores_Obtener([FromQuery] int codEmpresa)
        {
            return _bl.AF_Sectores_Obtener(codEmpresa);
        }

        [HttpPost("AF_Sectores_Guardar")]
        public ErrorDto AF_Sectores_Guardar([FromQuery] int codEmpresa, [FromQuery] string usuario, [FromBody] SectoresData sector)
        {
            return _bl.AF_Sectores_Guardar(codEmpresa, usuario, sector);
        }

        [HttpDelete("AF_Sectores_Eliminar")]
        public ErrorDto AF_Sectores_Eliminar([FromQuery] int codEmpresa, [FromQuery] string usuario, [FromQuery] int codSector)
        {
            return _bl.AF_Sectores_Eliminar(codEmpresa, usuario, codSector);
        }
    }
}
