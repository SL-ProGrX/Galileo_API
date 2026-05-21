using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Personas;
using Galileo_API.BusinessLogic.ProGrX_Personas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Galileo_API.Controllers.ProGrX_Personas
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmAfEstadoLaboralController : ControllerBase
    {
        private readonly FrmAfEstadoLaboralBL _bl;

        public FrmAfEstadoLaboralController(IConfiguration config)
        {
            _bl = new FrmAfEstadoLaboralBL(config);
        }

        [Authorize]
        [HttpGet("AF_EstadoLaboral_Obtener")]
        public ErrorDto<EstadoLaboralLista> AF_EstadoLaboral_Obtener(int codEmpresa, string filtros)
        {
            return _bl.AF_EstadoLaboral_Obtener(codEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("AF_EstadoLaboral_Guardar")]
        public ErrorDto AF_EstadoLaboral_Guardar(int codEmpresa, string usuario, EstadoLaboralData estado)
        {
            return _bl.AF_EstadoLaboral_Guardar(codEmpresa, usuario, estado);
        }

        [Authorize]
        [HttpDelete("AF_EstadoLaboral_Eliminar")]
        public ErrorDto AF_EstadoLaboral_Eliminar(int codEmpresa, string usuario, string estadoLaboral)
        {
            return _bl.AF_EstadoLaboral_Eliminar(codEmpresa, usuario, estadoLaboral);
        }

        [Authorize]
        [HttpGet("AF_EstadoLaboral_Valida")]
        public ErrorDto AF_EstadoLaboral_Valida(int codEmpresa, string estadoLaboral)
        {
            return _bl.AF_EstadoLaboral_Valida(codEmpresa, estadoLaboral);
        }

        [Authorize]
        [HttpGet("AF_EstadoLaboral_Exportar")]
        public ErrorDto<EstadoLaboralLista> AF_EstadoLaboral_Exportar(int codEmpresa, string filtros)
        {
            return _bl.AF_EstadoLaboral_Exportar(codEmpresa, filtros);
        }
    }
}
