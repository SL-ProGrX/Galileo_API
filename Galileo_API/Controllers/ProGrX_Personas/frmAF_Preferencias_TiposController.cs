using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Personas;
using Galileo_API.Models.ProGrX_Personas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Personas
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmAfPreferenciasTiposController : ControllerBase
    {
        private readonly FrmAfPreferenciasTiposBL _bl;

        public FrmAfPreferenciasTiposController(IConfiguration config)
        {
            _bl = new FrmAfPreferenciasTiposBL(config);
        }

        [Authorize]
        [HttpGet("AF_Preferencias_Obtener")]
        public ErrorDto<PreferenciaTipoLista> AF_Preferencias_Obtener(int codEmpresa, string filtros)
        {
            return _bl.AF_Preferencias_Obtener(codEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("AF_Preferencias_Guardar")]
        public ErrorDto AF_Preferencias_Guardar(int codEmpresa, string usuario, PreferenciaTipoData preferenciaTipo)
        {
            return _bl.AF_Preferencias_Guardar(codEmpresa, usuario, preferenciaTipo);
        }

        [Authorize]
        [HttpDelete("AF_Preferencias_Eliminar")]
        public ErrorDto AF_Preferencias_Eliminar(int codEmpresa, string usuario, string codPreferencia)
        {
            return _bl.AF_Preferencias_Eliminar(codEmpresa, usuario, codPreferencia);
        }

        [Authorize]
        [HttpGet("AF_Preferencias_Valida")]
        public ErrorDto AF_Preferencias_Valida(int codEmpresa, string codPreferencia)
        {
            return _bl.AF_Preferencias_Valida(codEmpresa, codPreferencia);
        }

        [Authorize]
        [HttpGet("AF_Preferencias_Exportar")]
        public ErrorDto<PreferenciaTipoLista> AF_Preferencias_Exportar(int codEmpresa, string filtros)
        {
            return _bl.AF_Preferencias_Exportar(codEmpresa, filtros);
        }
    }
}
