using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.Controllers.ProGrX_Nucleo
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSysPortalController : ControllerBase
    {
        private readonly FrmSysPortalBL _bl;
        public FrmSysPortalController(IConfiguration config)
        {
            _bl = new FrmSysPortalBL(config);
        }

        [Authorize]
        [HttpGet("Sys_MensajesPortal_Lista_Obtener")]
        public ErrorDto<SysMensajesPortalLista> Sys_MensajesPortal_Lista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Sys_MensajesPortal_Lista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Sys_MensajesPortal_Obtener")]
        public ErrorDto<List<SysMensajesPortalListaItem>> Sys_MensajesPortal_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Sys_MensajesPortal_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Sys_MensajesPortal_Detalle_Obtener")]
        public ErrorDto<SysMensajesPortalDetalleModel> Sys_MensajesPortal_Detalle_Obtener(int CodEmpresa, string codigo)
        {
            return _bl.Sys_MensajesPortal_Detalle_Obtener(CodEmpresa, codigo);
        }

        [Authorize]
        [HttpPost("Sys_MensajesPortal_Mensaje_Guardar")]
        public ErrorDto Sys_MensajesPortal_Mensaje_Guardar(int CodEmpresa,[FromBody] SysMensajesPortalDetalleModel dto,string usuario)
        {
            return _bl.Sys_MensajesPortal_Mensaje_Guardar(CodEmpresa, dto, usuario);
        }

        [Authorize]
        [HttpDelete("Sys_MensajesPortal_Mensaje_Eliminar")]
        public ErrorDto Sys_MensajesPortal_Mensaje_Eliminar(int CodEmpresa, string codigo, string usuario)
        {
            return _bl.Sys_MensajesPortal_Mensaje_Eliminar(CodEmpresa, codigo, usuario);
        }

        [Authorize]
        [HttpGet("Sys_MensajesPortal_Smtps_Obtener")]
        public ErrorDto<List<SysMensajesPortalSmtpDto>> Sys_MensajesPortal_Smtps_Obtener(int CodEmpresa)
        {
            return _bl.Sys_MensajesPortal_Smtps_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Sys_MensajesPortal_Formatos_Obtener")]
        public ErrorDto<List<SysMensajesPortalFormatoDto>> Sys_MensajesPortal_Formatos_Obtener(int CodEmpresa)
        {
            return _bl.Sys_MensajesPortal_Formatos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Sys_MensajesPortal_Activaciones_Obtener")]
        public ErrorDto<List<SysMensajesPortalActivacionDto>> Sys_MensajesPortal_Activaciones_Obtener()
        {
            return _bl.Sys_MensajesPortal_Activaciones_Obtener();
        }

        [Authorize]
        [HttpGet("Sys_MensajesPortal_Eventos_Obtener")]
        public ErrorDto<List<SysMensajesPortalEventoDto>> Sys_MensajesPortal_Eventos_Obtener(int CodEmpresa)
        {
            return _bl.Sys_MensajesPortal_Eventos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Sys_MensajesPortal_Portal_Obtener")]
        public ErrorDto<SysMensajesPortalPreferenciasModel> Sys_MensajesPortal_Portal_Obtener(int CodEmpresa)
        {
            return _bl.Sys_MensajesPortal_Portal_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("Sys_MensajesPortal_Portal_Guardar")]
        public ErrorDto Sys_MensajesPortal_Portal_Guardar(
            int CodEmpresa,
            [FromBody] SysMensajesPortalPreferenciasModel dto,
            string usuario)
        {
            return _bl.Sys_MensajesPortal_Portal_Guardar(CodEmpresa, dto, usuario);
        }

    }
}