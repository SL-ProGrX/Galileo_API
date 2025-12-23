using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Activos_Fijos;
using Galileo.Models.ERROR;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmActivosParametrosEnlacesController : ControllerBase
    {
        private readonly FrmActivosParametrosEnlacesBL _bl;
        public FrmActivosParametrosEnlacesController(IConfiguration config)
        {
            _bl = new FrmActivosParametrosEnlacesBL(config);
        }
             
        [Authorize]
        [HttpPost("Activos_ParametrosEnlaces_Proveedores_Guardar")]
        public ErrorDto Activos_ParametrosEnlaces_Proveedores_Guardar(int CodEmpresa)
        {
            return _bl.Activos_ParametrosEnlaces_Proveedores_Guardar(CodEmpresa);
        }
    }
}
