using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Cajas;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.Controllers.ProGrX.Cajas
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmCajasUsuariosController : ControllerBase
    {
        private readonly FrmCajasUsuariosBL _bl;
        public FrmCajasUsuariosController(IConfiguration config)
        {
            _bl = new FrmCajasUsuariosBL(config);
        }
        
        [Authorize]
        [HttpGet("Cajas_Usuarios_Lista_Obtener")]
        public ErrorDto<List<CajasUsuariosListadoUsuarioData>> Cajas_Usuarios_Lista_Obtener(int CodEmpresa, string filtros, bool soloAsignados)
        {
            return _bl.Cajas_Usuarios_Lista_Obtener(CodEmpresa, filtros, soloAsignados);
            
        }

        [Authorize]
        [HttpPost("Cajas_Usuarios_Guardar")]
        public ErrorDto Cajas_Usuarios_Guardar(int CodEmpresa, string usuario, CajasUsuariosData usuarioCaja)
        {
            return _bl.Cajas_Usuarios_Guardar(CodEmpresa, usuario, usuarioCaja);
            
        }

        [Authorize]
        [HttpPost("Cajas_Usuarios_Eliminar")]
        public ErrorDto Cajas_Usuarios_Eliminar(int CodEmpresa, string usuario, string cod_caja, string usuarioCaja)
        {
            return _bl.Cajas_Usuarios_Eliminar(CodEmpresa, usuario, cod_caja, usuarioCaja);
            
        }

        [Authorize]
        [HttpGet("Cajas_Usuarios_Historico_Obtener")]
        public ErrorDto<List<CajasUsuariosHistData>> Cajas_Usuarios_Historico_Obtener(int CodEmpresa, string cod_caja, string usuarioCaja)
        {
            return _bl.Cajas_Usuarios_Historico_Obtener(CodEmpresa, cod_caja, usuarioCaja);
            
        }

        [Authorize]
        [HttpGet("Cajas_Usuarios_Cajas_Lista_Obtener")]
        public ErrorDto<List<CajasUsuariosCajaListaData>> Cajas_Usuarios_Cajas_Lista_Obtener(int CodEmpresa,string usuarioCaja)
        {
            return _bl.Cajas_Usuarios_Cajas_Lista_Obtener(CodEmpresa, usuarioCaja);
        }
    }
}