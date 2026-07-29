using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.CuentasxCobrar
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCxCFacturasMonitoreoController : ControllerBase
    {
        private readonly FrmCxCFacturasMonitoreoBL _bl;

        public FrmCxCFacturasMonitoreoController(IConfiguration config)
        {
            _bl = new FrmCxCFacturasMonitoreoBL(config);
        }

        [Authorize]
        [HttpGet("CxCFacturasMonitoreoPersonas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxCFacturasMonitoreoPersonas_Obtener(
            int codEmpresa,
            string ordenarPor,
            bool esPagador)
        {
            return _bl.CxCFacturasMonitoreoPersonas_Obtener(codEmpresa, ordenarPor, esPagador);
        }

        [Authorize]
        [HttpGet("CxCFacturasMonitoreoConceptos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxCFacturasMonitoreoConceptos_Obtener(int codEmpresa)
        {
            return _bl.CxCFacturasMonitoreoConceptos_Obtener(codEmpresa);
        }

        [Authorize]
        [HttpGet("CxCFacturasMonitoreoContratos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxCFacturasMonitoreoContratos_Obtener(int codEmpresa)
        {
            return _bl.CxCFacturasMonitoreoContratos_Obtener(codEmpresa);
        }

        [Authorize]
        [HttpGet("CxCFacturasMonitoreoEstados_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxCFacturasMonitoreoEstados_Obtener(int codEmpresa)
        {
            return _bl.CxCFacturasMonitoreoEstados_Obtener(codEmpresa);
        }

        [Authorize]
        [HttpGet("CxCFacturasMonitoreoEstadosPorProceso_Obtener")]
        public ErrorDto<List<CxCFacturasMonitoreoEstadoProcesoDto>> CxCFacturasMonitoreoEstadosPorProceso_Obtener(
            int codEmpresa,
            string proceso)
        {
            return _bl.CxCFacturasMonitoreoEstadosPorProceso_Obtener(codEmpresa, proceso);
        }

        [Authorize]
        [HttpPost("CxCFacturasMonitoreoFacturas_Obtener")]
        public ErrorDto<List<CxCFacturasMonitoreoItemDto>> CxCFacturasMonitoreoFacturas_Obtener(
            int codEmpresa,
            CxCFacturasMonitoreoFiltroDto filtro)
        {
            return _bl.CxCFacturasMonitoreoFacturas_Obtener(codEmpresa, filtro);
        }

        [Authorize]
        [HttpPost("CxCFacturasMonitoreoDetalle_Obtener")]
        public ErrorDto<TablasListaGenericaModel> CxCFacturasMonitoreoDetalle_Obtener(
            int codEmpresa,
            CxCFacturasMonitoreoDetalleRequestDto request)
        {
            return _bl.CxCFacturasMonitoreoDetalle_Obtener(codEmpresa, request);
        }

        [Authorize]
        [HttpPost("CxCFacturasMonitoreoEstado_Actualizar")]
        public ErrorDto<bool> CxCFacturasMonitoreoEstado_Actualizar(
            int codEmpresa,
            CxCFacturasMonitoreoEstadoRequestDto request)
        {
            return _bl.CxCFacturasMonitoreoEstado_Actualizar(codEmpresa, request);
        }
    }
}
