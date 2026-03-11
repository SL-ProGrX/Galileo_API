using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCntXContabilidadesUsuariosController : ControllerBase
    {
        private readonly FrmCntXContabilidadesUsuariosBl _bl;

        public FrmCntXContabilidadesUsuariosController(IConfiguration config) => _bl = new FrmCntXContabilidadesUsuariosBl(config);
        
        [HttpGet("CntXContaUser_ObtenerCatalogo")]
        public ErrorDto<List<CntXContabilidadUsuarioData>> CntXContaUser_ObtenerCatalogo(
            int codEmpresa, bool obtenerUsuarios)
        {
            return _bl.CntXContaUser_ObtenerCatalogo(codEmpresa, obtenerUsuarios);
        }

        [HttpGet("CntXContaUser_ObtenerRelaciones")]
        public ErrorDto<List<CntXContabilidadUsuarioData>> CntXContaUser_ObtenerRelaciones(
            int codEmpresa, bool porContabilidad, string valor)
        {
            return _bl.CntXContaUser_ObtenerRelaciones(codEmpresa, porContabilidad, valor);
        }

        [HttpPost("CntXContaUser_GuardarRelacion")]
        public ErrorDto CntXContaUser_GuardarRelacion(
            int codEmpresa, int codContabilidad, string usuario, string usuarioRegistro)
        {
            return _bl.CntXContaUser_GuardarRelacion(codEmpresa, codContabilidad, usuario, usuarioRegistro);
        }

        [HttpDelete("CntXContaUser_EliminarRelacion")]
        public ErrorDto CntXContaUser_EliminarRelacion(int codEmpresa, int codContabilidad, string usuario)
        {
            return _bl.CntXContaUser_EliminarRelacion(codEmpresa, codContabilidad, usuario);
        }
    }
}