using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Cobros;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;

namespace Galileo.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCONotificaEmailController : Controller
    {
        private readonly IConfiguration? _config;
        private readonly FrmCONotificaEmailBL _bl;

        public FrmCONotificaEmailController(IConfiguration config)
        {
            _config = config;
            _bl = new FrmCONotificaEmailBL(_config);
        }

        [Authorize]
        [HttpGet("Co_NotificaEmail_Lista_Obtener")]
        public ErrorDto<FrmCONotificaEmailListaResult> Co_NotificaEmail_Lista_Obtener(int CodEmpresa,string filtros, [FromQuery] FrmCONotificaEmailConsultaDto dto)
        {
            return _bl.Co_NotificaEmail_Lista_Obtener(CodEmpresa, filtros, dto);
        }

        [Authorize]
        [HttpGet("Co_NotificaEmail_Export")]
        public ErrorDto<FrmCONotificaEmailListaResult> Co_NotificaEmail_Export(int CodEmpresa,string filtros, [FromQuery] FrmCONotificaEmailConsultaDto dto)
        {
            return _bl.Co_NotificaEmail_Export(CodEmpresa, filtros, dto);
        }

        [Authorize]
        [HttpPost("Co_NotificaEmail_Notificar_Bulk")]
        public ErrorDto Co_NotificaEmail_Notificar_Bulk(int CodEmpresa, string usuario, FrmCONotificaEmailNotificarBulkDto dto)
        {
            return _bl.Co_NotificaEmail_Notificar_Bulk(CodEmpresa, usuario, dto);
        }

        [Authorize]
        [HttpGet("Co_EstadosPersona_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Co_EstadosPersona_Dropdown_Obtener(int CodEmpresa)
        {
            return _bl.Co_EstadosPersona_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Co_Instituciones_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Co_Instituciones_Dropdown_Obtener(int CodEmpresa)
        {
            return _bl.Co_Instituciones_Dropdown_Obtener(CodEmpresa);
        }
    }
}
