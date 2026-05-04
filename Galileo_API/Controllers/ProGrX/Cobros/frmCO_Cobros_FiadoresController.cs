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
    public class FrmCOCobroFiadoresController : Controller
    {
        private readonly IConfiguration? _config;
        private readonly FrmCOCobroFiadoresBL _bl;

        public FrmCOCobroFiadoresController(IConfiguration config)
        {
            _config = config;
            _bl = new FrmCOCobroFiadoresBL(_config);
        }
        [Authorize]
        [HttpGet("Co_Instituciones_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Co_Instituciones_Dropdown_Obtener(int CodEmpresa)
        {
            return _bl.Co_Instituciones_Dropdown_Obtener(CodEmpresa);
        }
        [Authorize]
        [HttpGet("Co_EstadosPersona_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Co_EstadosPersona_Dropdown_Obtener(int CodEmpresa)
        {
            return _bl.Co_EstadosPersona_Dropdown_Obtener(CodEmpresa);
        }
        [Authorize]
        [HttpGet("Co_CobroFiadores_Pendientes_Lista_Obtener")]
        public ErrorDto<FrmCOCobroFiadoresPendientesListaResult> Co_CobroFiadores_Pendientes_Lista_Obtener(int CodEmpresa, string filtros, string dto)
        {
            return _bl.Co_CobroFiadores_Pendientes_Lista_Obtener(CodEmpresa, filtros, dto);
        }
        [Authorize]
        [HttpGet("Co_CobroFiadores_Pendientes_Lista_Export")]
        public ErrorDto<FrmCOCobroFiadoresPendientesListaResult> Co_CobroFiadores_Pendientes_Lista_Export(int CodEmpresa, string filtros, string dto)
        {
            return _bl.Co_CobroFiadores_Pendientes_Lista_Export(CodEmpresa, filtros, dto);
        }
        [Authorize]
        [HttpGet("Co_CobroFiadores_Activos_Lista_Obtener")]
        public ErrorDto<FrmCOCobroFiadoresActivosListaResult> Co_CobroFiadores_Activos_Lista_Obtener(int CodEmpresa, string filtros, string dto)
        {
            return _bl.Co_CobroFiadores_Activos_Lista_Obtener(CodEmpresa, filtros, dto);
        }
        [Authorize]
        [HttpGet("Co_CobroFiadores_Activos_Lista_Export")]
        public ErrorDto<FrmCOCobroFiadoresActivosListaResult> Co_CobroFiadores_Activos_Lista_Export(int CodEmpresa, string filtros, string dto)
        {
            return _bl.Co_CobroFiadores_Activos_Lista_Export(CodEmpresa, filtros, dto);
        }
        [Authorize]
        [HttpGet("Co_CobroFiadores_Consultas_Lista_Obtener")]
        public ErrorDto<FrmCOCobroFiadoresConsultasListaResult> Co_CobroFiadores_Consultas_Lista_Obtener(int CodEmpresa, string filtros, string dto)
        {
            return _bl.Co_CobroFiadores_Consultas_Lista_Obtener(CodEmpresa, filtros, dto);
        }
        [Authorize]
        [HttpGet("Co_CobroFiadores_Consultas_Lista_Export")]
        public ErrorDto<FrmCOCobroFiadoresConsultasListaResult> Co_CobroFiadores_Consultas_Lista_Export(int CodEmpresa, string filtros, string dto)
        {
            return _bl.Co_CobroFiadores_Consultas_Lista_Export(CodEmpresa, filtros, dto);
        }
        [Authorize]
        [HttpPost("Co_CobroFiadores_NotificaAdvertencia_Bulk")]
        public ErrorDto Co_CobroFiadores_NotificaAdvertencia_Bulk(int CodEmpresa, string usuario, FrmCOCobroFiadoresAccionBulkDto dto)
        {
            return _bl.Co_CobroFiadores_NotificaAdvertencia_Bulk(CodEmpresa, usuario, dto);
        }
        [Authorize]
        [HttpPost("Co_CobroFiadores_ProcesaCobros_Bulk")]
        public ErrorDto Co_CobroFiadores_ProcesaCobros_Bulk(int CodEmpresa, string usuario, FrmCOCobroFiadoresAccionBulkDto dto)
        {
            return _bl.Co_CobroFiadores_ProcesaCobros_Bulk(CodEmpresa, usuario, dto);
        }
        [Authorize]
        [HttpPost("Co_CobroFiadores_CancelaCobro_Bulk")]
        public ErrorDto Co_CobroFiadores_CancelaCobro_Bulk(int CodEmpresa, string usuario, FrmCOCobroFiadoresAccionBulkDto dto)
        {
            return _bl.Co_CobroFiadores_CancelaCobro_Bulk(CodEmpresa, usuario, dto);
        }
    }
}
