using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Hipotecario
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmVivMantenimientoController : ControllerBase
    {
        private readonly FrmVivMantenimientoBl _bl;

        public FrmVivMantenimientoController(IConfiguration config)
        {
            _bl = new FrmVivMantenimientoBl(config);
        }

        [HttpGet("VivMantenimiento_ArbolInicial_Obtener")]
        public ErrorDto<List<VivMantenimientoNodoData>> VivMantenimiento_ArbolInicial_Obtener()
        {
            return FrmVivMantenimientoBl.VivMantenimiento_ArbolInicial_Obtener();
        }

        [HttpGet("VivMantenimiento_NodosHijos_Obtener")]
        public ErrorDto<List<VivMantenimientoNodoData>> VivMantenimiento_NodosHijos_Obtener(int codEmpresa, string tag, string key)
        {
            return _bl.VivMantenimiento_NodosHijos_Obtener(codEmpresa, tag, key);
        }

        [HttpGet("VivMantenimiento_Lista_Obtener")]
        public ErrorDto<List<VivMantenimientoListaData>> VivMantenimiento_Lista_Obtener(int codEmpresa, string tag, string key)
        {
            return _bl.VivMantenimiento_Lista_Obtener(codEmpresa, tag, key);
        }

        [HttpPost("VivMantenimiento_ZonaContacto_Asignar")]
        public ErrorDto VivMantenimiento_ZonaContacto_Asignar(int codEmpresa, VivMantenimientoZonaContactoAsignarRequest request)
        {
            return _bl.VivMantenimiento_ZonaContacto_Asignar(codEmpresa, request);
        }
    }
}
