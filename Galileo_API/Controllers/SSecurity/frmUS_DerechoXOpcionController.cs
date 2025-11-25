using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.Controllers
{
    [Route("api/FrmUsDerechoXOpcion")]
    [Route("api/frmUS_DerechoXOpcion")]
    [ApiController]
    public class FrmUsDerechoXOpcionController : ControllerBase
    {
        readonly FrmUsDerechoXOpcionBl DerechoXOpcionBL;

        public FrmUsDerechoXOpcionController(IConfiguration config)
        {
            DerechoXOpcionBL = new FrmUsDerechoXOpcionBl(config);
        }

        [HttpGet("ModulosObtener")]
        //[Authorize]
        public List<ModuloResultDto> ModulosObtener()
        {
            return DerechoXOpcionBL.ModulosObtener();
        }

        [HttpGet("FormulariosObtener")]
        //[Authorize]
        public List<FormularioResultDto> FormulariosObtener()
        {
            return DerechoXOpcionBL.FormulariosObtener();
        }

        [HttpGet("OpcionesObtener")]
        //[Authorize]
        public List<OpcionResultDto> OpcionesObtener()
        {
            return DerechoXOpcionBL.OpcionesObtener();
        }

        [HttpGet("DatosObtener")]
        //[Authorize]
        public List<DatosResultDto> DatosObtener(int opcion, char estado)
        {
            return DerechoXOpcionBL.DatosObtener(opcion, estado);
        }

        [HttpPost("RolPermisosActualizar")]
        // [Authorize]
        public ErrorDto RolPermisosActualizar(OpcionRolRequestDto req)
        {
            return DerechoXOpcionBL.RolPermisosActualizar(req);
        }
    }
}
