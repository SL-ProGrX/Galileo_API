using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Personas;
using Galileo_API.BusinessLogic.ProGrX_Personas;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Personas
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmAfPerfilTransaccionalController : ControllerBase
    {
        private readonly FrmAfPerfilTransaccionalBL _bl;

        public FrmAfPerfilTransaccionalController(IConfiguration config)
        {
            _bl = new FrmAfPerfilTransaccionalBL(config);
        }

        [HttpGet("AF_PerfilTransaccional_Obtener")]
        public ErrorDto<PerfilTransaccionalLista> AF_PerfilTransaccional_Obtener(int codEmpresa, string filtros)
        {
            return _bl.AF_PerfilTransaccional_Obtener(codEmpresa, filtros);
        }

        [HttpPost("AF_PerfilTransaccional_Guardar")]
        public ErrorDto AF_PerfilTransaccional_Guardar(int codEmpresa, string usuario, PerfilTransaccionalData perfil)
        {
            return _bl.AF_PerfilTransaccional_Guardar(codEmpresa, usuario, perfil);
        }

        [HttpDelete("AF_PerfilTransaccional_Eliminar")]
        public ErrorDto AF_PerfilTransaccional_Eliminar(int codEmpresa, string usuario, int ptId)
        {
            return _bl.AF_PerfilTransaccional_Eliminar(codEmpresa, usuario, ptId);
        }

        [HttpGet("AF_PerfilTransaccional_Exportar")]
        public ErrorDto<PerfilTransaccionalLista> AF_PerfilTransaccional_Exportar(int codEmpresa, string filtros)
        {
            return _bl.AF_PerfilTransaccional_Exportar(codEmpresa, filtros);
        }
    }
}
