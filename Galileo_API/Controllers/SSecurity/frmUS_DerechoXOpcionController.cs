using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmUsDerechoXOpcionController : ControllerBase
    {
        readonly FrmUsDerechoXOpcionBl DerechoXOpcionBL;

        public FrmUsDerechoXOpcionController(IConfiguration config)
        {
            DerechoXOpcionBL = new FrmUsDerechoXOpcionBl(config);
        }

        [HttpGet("ModulosObtener")]
        public List<ModuloResultDto> ModulosObtener()
        {
            return DerechoXOpcionBL.ModulosObtener();
        }

        [HttpGet("FormulariosObtener")]
        public List<FormularioResultDto> FormulariosObtener()
        {
            return DerechoXOpcionBL.FormulariosObtener();
        }

        [HttpGet("OpcionesObtener")]
        public List<OpcionResultDto> OpcionesObtener()
        {
            return DerechoXOpcionBL.OpcionesObtener();
        }

        [HttpGet("DatosObtener")]
        public List<DatosResultDto> DatosObtener(int opcion, char estado)
        {
            return DerechoXOpcionBL.DatosObtener(opcion, estado);
        }

        [HttpPost("RolPermisosActualizar")]
        public ErrorDto RolPermisosActualizar(OpcionRolRequestDto req)
        {
            return DerechoXOpcionBL.RolPermisosActualizar(req);
        }
    }
}
