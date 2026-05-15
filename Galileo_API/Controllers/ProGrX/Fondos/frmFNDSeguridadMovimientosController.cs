using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmFndSeguridadMovimientosController : ControllerBase
    {
        private readonly FrmFndSeguridadMovimientosBL _BL;

        public FrmFndSeguridadMovimientosController(IConfiguration? config)
        {
            _BL = new FrmFndSeguridadMovimientosBL(config);
        }

        [HttpGet("Seguridad_Planes_Obtener")]
        public ErrorDto<List<SeguridadMovimientoPlanDto>> Seguridad_Planes_Obtener(
            int CodEmpresa,
            string cod_grupo)
        {
            return _BL.Seguridad_Planes_Obtener(CodEmpresa, cod_grupo);
        }

        [HttpPost("Seguridad_Planes_Marcar")]
        public ErrorDto<bool> Seguridad_Planes_Marcar(
            int CodEmpresa,
            string cod_grupo,
            string cod_plan,
            int cod_operadora,
            bool marcado,
            string usuario)
        {
            return _BL.Seguridad_Planes_Marcar(CodEmpresa, cod_grupo, cod_plan, cod_operadora, marcado, usuario);
        }


        [HttpGet("Seguridad_Usuarios_Obtener")]
        public ErrorDto<List<SeguridadMovimientoUsuarioDto>> Seguridad_Usuarios_Obtener(
            int CodEmpresa,
            string cod_grupo)
        {
            return _BL.Seguridad_Usuarios_Obtener(CodEmpresa, cod_grupo);
        }


        [HttpPost("Seguridad_Usuarios_Marcar")]
        public ErrorDto<bool> Seguridad_Usuarios_Marcar(
            int CodEmpresa,
            string cod_grupo,
            string usuarioMarcado,
            bool marcado,
            string usuario)
        {
            return _BL.Seguridad_Usuarios_Marcar(CodEmpresa, cod_grupo, usuarioMarcado, marcado, usuario);
        }


        [HttpGet("Seguridad_Autorizadores_Obtener")]
        public ErrorDto<List<SeguridadMovimientoUsuarioDto>> Seguridad_Autorizadores_Obtener(
            int CodEmpresa,
            string cod_grupo)
        {
            return _BL.Seguridad_Autorizadores_Obtener(CodEmpresa, cod_grupo);
        }

        [HttpPost("Seguridad_Autorizadores_Marcar")]
        public ErrorDto<bool> Seguridad_Autorizadores_Marcar(
            int CodEmpresa,
            string cod_grupo,
            string usuarioMarcado,
            bool marcado,
            string usuario)
        {
            return _BL.Seguridad_Autorizadores_Marcar(CodEmpresa, cod_grupo, usuarioMarcado, marcado, usuario);
        }

        [HttpGet("Seguridad_Niveles_Obtener")]
        public ErrorDto<List<SeguridadMovimientoNivelDto>> Seguridad_Niveles_Obtener(int CodEmpresa)
        {
            return _BL.Seguridad_Niveles_Obtener(CodEmpresa);
        }

        [HttpPost("Seguridad_Niveles_Guardar")]
        public ErrorDto<bool> Seguridad_Niveles_Guardar(
            int CodEmpresa,
            SeguridadMovimientoNivelDto dto,
            string usuario)
        {
            return _BL.Seguridad_Niveles_Guardar(CodEmpresa, dto, usuario);
        }

        [HttpPost("Seguridad_Niveles_Eliminar")]
        public ErrorDto<bool> Seguridad_Niveles_Eliminar(
            int CodEmpresa,
            string cod_grupo,
            string usuario)
        {
            return _BL.Seguridad_Niveles_Eliminar(CodEmpresa, cod_grupo, usuario);
        }
    }
}