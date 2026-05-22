using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.Models.ProGrX_Personas;
using Galileo_API.BusinessLogic.ProGrX_Personas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Galileo_API.Controllers.ProGrX_Personas
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmAfEscolaridadController : ControllerBase
    {
        private readonly FrmAfEscolaridadBL _bl;

        public FrmAfEscolaridadController(IConfiguration config)
        {
            _bl = new FrmAfEscolaridadBL(config);
        }

        [Authorize]
        [HttpGet("AF_EscolaridadTipos_Obtener")]
        public ErrorDto<NivelEscolaridadLista> AF_EscolaridadTipos_Obtener(int codEmpresa, string filtros)
        {
            return _bl.AF_EscolaridadTipos_Obtener(codEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("AF_EscolaridadTipos_Guardar")]
        public ErrorDto AF_EscolaridadTipos_Guardar(int codEmpresa, string usuario, NivelEscolaridadData escolaridad)
        {
            return _bl.AF_EscolaridadTipos_Guardar(codEmpresa, usuario, escolaridad);
        }

        [Authorize]
        [HttpDelete("AF_EscolaridadTipos_Eliminar")]
        public ErrorDto AF_EscolaridadTipos_Eliminar(int codEmpresa, string usuario, string escolaridadTipo)
        {
            return _bl.AF_EscolaridadTipos_Eliminar(codEmpresa, usuario, escolaridadTipo);
        }

        [Authorize]
        [HttpGet("AF_EscolaridadTipos_Valida")]
        public ErrorDto AF_EscolaridadTipos_Valida(int codEmpresa, string escolaridadTipo)
        {
            return _bl.AF_EscolaridadTipos_Valida(codEmpresa, escolaridadTipo);
        }

        [Authorize]
        [HttpGet("AF_EscolaridadTipos_Exportar")]
        public ErrorDto<NivelEscolaridadLista> AF_EscolaridadTipos_Exportar(int codEmpresa, string filtros)
        {
            return _bl.AF_EscolaridadTipos_Exportar(codEmpresa, filtros);
        }
    }
}
