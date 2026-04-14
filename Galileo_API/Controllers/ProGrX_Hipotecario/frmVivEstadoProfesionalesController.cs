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
    public class FrmVivEstadoProfesionalesController : ControllerBase
    {
        private readonly FrmVivEstadoProfesionalesBl _bl;

        public FrmVivEstadoProfesionalesController(IConfiguration config)
        {
            _bl = new FrmVivEstadoProfesionalesBl(config);
        }

        [HttpGet("ViviendaContactos_Lista_Obtener")]
        public ErrorDto<List<ViviendaContactosData>> ViviendaContactos_Lista_Obtener(int codEmpresa)
        {
            return _bl.ViviendaContactos_Lista_Obtener(codEmpresa);
        }

        [HttpGet("VivEstadoProfesionales_Obtener")]
        public ErrorDto<ViviendaContactosData?> VivEstadoProfesionales_Obtener(int codEmpresa, int idContacto)
        {
            return _bl.VivEstadoProfesionales_Obtener(codEmpresa, idContacto);
        }

        [HttpGet("VivEstadoProfesionales_ConsultaExterna_Obtener")]
        public ErrorDto<ViviendaContactosData?> VivEstadoProfesionales_ConsultaExterna_Obtener(int codEmpresa, string cedula)
        {
            return _bl.VivEstadoProfesionales_ConsultaExterna_Obtener(codEmpresa, cedula);
        }

        [HttpPost("VivEstadoProfesionales_Suspender")]
        public ErrorDto VivEstadoProfesionales_Suspender(int codEmpresa, string usuario, ViviendaContactosData request)
        {
            return _bl.VivEstadoProfesionales_Suspender(codEmpresa, usuario, request);
        }
    }
}
