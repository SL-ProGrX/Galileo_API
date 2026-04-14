using Galileo.Models;
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
    public class FrmVivZonasController : ControllerBase
    {
        private readonly FrmVivZonasBl _bl;

        public FrmVivZonasController(IConfiguration config)
        {
            _bl = new FrmVivZonasBl(config);
        }

        [HttpGet("VivZonas_Lista_Obtener")]
        public ErrorDto<List<VivZonaData>> VivZonas_Lista_Obtener(int codEmpresa)
        {
            return _bl.VivZonas_Lista_Obtener(codEmpresa);
        }

        [HttpGet("Provincias_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Provincias_Obtener(int codEmpresa)
        {
            return _bl.Provincias_Obtener(codEmpresa);
        }

        [HttpGet("Cantones_Obtener")]
        public ErrorDto<List<VivZonaCantonData>> Cantones_Obtener(
            int codEmpresa, int idZona, string provincia, bool soloAsignadas)
        {
            return _bl.Cantones_Obtener(codEmpresa, idZona, provincia, soloAsignadas);
        }

        [HttpPost("VivZonas_Asignar")]
        public ErrorDto VivZonas_Asignar(
            int codEmpresa, int idZona, string provincia, string canton, string usuario, bool isChecked)
        {
            return _bl.VivZonas_Asignar(codEmpresa, idZona, provincia, canton, usuario, isChecked);
        }

        [HttpPost("VivZonas_TodosCantones_Asignar")]
        public ErrorDto VivZonas_TodosCantones_Asignar(
            int codEmpresa, int idZona, string provincia, string usuario)
        {
            return _bl.VivZonas_TodosCantones_Asignar(codEmpresa, idZona, provincia, usuario);
        }

        [HttpPost("VivZonas_Guardar")]
        public ErrorDto VivZonas_Guardar(int codEmpresa, string usuario, VivZonaData request)
        {
            return _bl.VivZonas_Guardar(codEmpresa, usuario, request);
        }

        [HttpDelete("VivZonas_Eliminar")]
        public ErrorDto VivZonas_Eliminar(int codEmpresa, int idZona, string usuario)
        {
            return _bl.VivZonas_Eliminar(codEmpresa, idZona, usuario);
        }
    }
}
