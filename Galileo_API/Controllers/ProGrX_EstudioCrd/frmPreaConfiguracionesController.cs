using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_EstudioCrd
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmPreaConfiguracionesController : ControllerBase
    {
        private readonly FrmPreaConfiguracionesBL _bl;

        public FrmPreaConfiguracionesController(IConfiguration config)
        {
            _bl = new FrmPreaConfiguracionesBL(config);
        }

        [Authorize]
        [HttpGet("CR_Prea_Configuraciones_ComiteMax_Lista_Obtener")]
        public ErrorDto<CrPreaConfiguracionesComiteMaxListaResult> CR_Prea_Configuraciones_ComiteMax_Lista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.CR_Prea_Configuraciones_ComiteMax_Lista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("CR_Prea_Configuraciones_ComiteMax_Lista_Export")]
        public ErrorDto<CrPreaConfiguracionesComiteMaxListaResult> CR_Prea_Configuraciones_ComiteMax_Lista_Export(int CodEmpresa, string filtros)
        {
            return _bl.CR_Prea_Configuraciones_ComiteMax_Lista_Export(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("CR_Prea_Configuraciones_ComiteMax_Guardar")]
        public ErrorDto CR_Prea_Configuraciones_ComiteMax_Guardar(int CodEmpresa, string usuario, [FromBody] CrPreaConfiguracionesComiteMaxGuardarRequest request)
        {
            return _bl.CR_Prea_Configuraciones_ComiteMax_Guardar(CodEmpresa, usuario, request);
        }

        [Authorize]
        [HttpGet("CR_Prea_Configuraciones_ComiteLineas_Lista_Obtener")]
        public ErrorDto<CrPreaConfiguracionesComiteLineasListaResult> CR_Prea_Configuraciones_ComiteLineas_Lista_Obtener(int CodEmpresa, string? codigoLinea, string filtros)
        {
            return _bl.CR_Prea_Configuraciones_ComiteLineas_Lista_Obtener(CodEmpresa, codigoLinea, filtros);
        }

        [Authorize]
        [HttpGet("CR_Prea_Configuraciones_ComiteLineas_Lista_Export")]
        public ErrorDto<CrPreaConfiguracionesComiteLineasListaResult> CR_Prea_Configuraciones_ComiteLineas_Lista_Export(int CodEmpresa, string? codigoLinea, string filtros)
        {
            return _bl.CR_Prea_Configuraciones_ComiteLineas_Lista_Export(CodEmpresa, codigoLinea, filtros);
        }

        [Authorize]
        [HttpPost("CR_Prea_Configuraciones_ComiteLineas_Guardar")]
        public ErrorDto CR_Prea_Configuraciones_ComiteLineas_Guardar(int CodEmpresa, string usuario, [FromBody] CrPreaConfiguracionesComiteLineasGuardarRequest request)
        {
            return _bl.CR_Prea_Configuraciones_ComiteLineas_Guardar(CodEmpresa, usuario, request);
        }

        [Authorize]
        [HttpGet("CR_Prea_Configuraciones_ComiteLineas_Dropdown_Obtener")]
        public ErrorDto<List<CrPreaConfiguracionesLineaDropdownDto>> CR_Prea_Configuraciones_ComiteLineas_Dropdown_Obtener(int CodEmpresa, string? filtro)
        {
            return _bl.CR_Prea_Configuraciones_ComiteLineas_Dropdown_Obtener(CodEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("CR_Prea_Configuraciones_ComiteAdjuntos_Lista_Obtener")]
        public ErrorDto<CrPreaConfiguracionesComiteAdjuntosListaResult> CR_Prea_Configuraciones_ComiteAdjuntos_Lista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.CR_Prea_Configuraciones_ComiteAdjuntos_Lista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("CR_Prea_Configuraciones_ComiteAdjuntos_Lista_Export")]
        public ErrorDto<CrPreaConfiguracionesComiteAdjuntosListaResult> CR_Prea_Configuraciones_ComiteAdjuntos_Lista_Export(int CodEmpresa, string filtros)
        {
            return _bl.CR_Prea_Configuraciones_ComiteAdjuntos_Lista_Export(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("CR_Prea_Configuraciones_ComiteAdjuntos_Guardar")]
        public ErrorDto CR_Prea_Configuraciones_ComiteAdjuntos_Guardar(int CodEmpresa, string usuario, [FromBody] CrPreaConfiguracionesComiteAdjuntosGuardarRequest request)
        {
            return _bl.CR_Prea_Configuraciones_ComiteAdjuntos_Guardar(CodEmpresa, usuario, request);
        }

        [Authorize]
        [HttpGet("CR_Prea_Configuraciones_GarantiaLiquidez_Lista_Obtener")]
        public ErrorDto<CrPreaConfiguracionesGarantiaLiquidezListaResult> CR_Prea_Configuraciones_GarantiaLiquidez_Lista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.CR_Prea_Configuraciones_GarantiaLiquidez_Lista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("CR_Prea_Configuraciones_GarantiaLiquidez_Lista_Export")]
        public ErrorDto<CrPreaConfiguracionesGarantiaLiquidezListaResult> CR_Prea_Configuraciones_GarantiaLiquidez_Lista_Export(int CodEmpresa, string filtros)
        {
            return _bl.CR_Prea_Configuraciones_GarantiaLiquidez_Lista_Export(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("CR_Prea_Configuraciones_GarantiaLiquidez_Guardar")]
        public ErrorDto CR_Prea_Configuraciones_GarantiaLiquidez_Guardar(int CodEmpresa, string usuario, [FromBody] CrPreaConfiguracionesGarantiaLiquidezGuardarRequest request)
        {
            return _bl.CR_Prea_Configuraciones_GarantiaLiquidez_Guardar(CodEmpresa, usuario, request);
        }

        [Authorize]
        [HttpGet("CR_Prea_Configuraciones_GarantiaRefunde_Lista_Obtener")]
        public ErrorDto<CrPreaConfiguracionesGarantiaRefundeListaResult> CR_Prea_Configuraciones_GarantiaRefunde_Lista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.CR_Prea_Configuraciones_GarantiaRefunde_Lista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("CR_Prea_Configuraciones_GarantiaRefunde_Lista_Export")]
        public ErrorDto<CrPreaConfiguracionesGarantiaRefundeListaResult> CR_Prea_Configuraciones_GarantiaRefunde_Lista_Export(int CodEmpresa, string filtros)
        {
            return _bl.CR_Prea_Configuraciones_GarantiaRefunde_Lista_Export(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("CR_Prea_Configuraciones_GarantiaRefunde_Guardar")]
        public ErrorDto CR_Prea_Configuraciones_GarantiaRefunde_Guardar(int CodEmpresa, string usuario, [FromBody] CrPreaConfiguracionesGarantiaRefundeGuardarRequest request)
        {
            return _bl.CR_Prea_Configuraciones_GarantiaRefunde_Guardar(CodEmpresa, usuario, request);
        }

        [Authorize]
        [HttpGet("CR_Prea_Configuraciones_CambioEstado_Lista_Obtener")]
        public ErrorDto<CrPreaConfiguracionesCambioEstadoListaResult> CR_Prea_Configuraciones_CambioEstado_Lista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.CR_Prea_Configuraciones_CambioEstado_Lista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("CR_Prea_Configuraciones_CambioEstado_Lista_Export")]
        public ErrorDto<CrPreaConfiguracionesCambioEstadoListaResult> CR_Prea_Configuraciones_CambioEstado_Lista_Export(int CodEmpresa, string filtros)
        {
            return _bl.CR_Prea_Configuraciones_CambioEstado_Lista_Export(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("CR_Prea_Configuraciones_CambioEstado_Guardar")]
        public ErrorDto CR_Prea_Configuraciones_CambioEstado_Guardar(int CodEmpresa, string usuario, [FromBody] CrPreaConfiguracionesCambioEstadoGuardarRequest request)
        {
            return _bl.CR_Prea_Configuraciones_CambioEstado_Guardar(CodEmpresa, usuario, request);
        }

        [Authorize]
        [HttpGet("CR_Prea_Configuraciones_EdadPension_Lista_Obtener")]
        public ErrorDto<CrPreaConfiguracionesEdadPensionListaResult> CR_Prea_Configuraciones_EdadPension_Lista_Obtener(int CodEmpresa, string? codigoLinea, string filtros)
        {
            return _bl.CR_Prea_Configuraciones_EdadPension_Lista_Obtener(CodEmpresa, codigoLinea, filtros);
        }

        [Authorize]
        [HttpGet("CR_Prea_Configuraciones_EdadPension_Lista_Export")]
        public ErrorDto<CrPreaConfiguracionesEdadPensionListaResult> CR_Prea_Configuraciones_EdadPension_Lista_Export(int CodEmpresa, string? codigoLinea, string filtros)
        {
            return _bl.CR_Prea_Configuraciones_EdadPension_Lista_Export(CodEmpresa, codigoLinea, filtros);
        }

        [Authorize]
        [HttpPost("CR_Prea_Configuraciones_EdadPension_Guardar")]
        public ErrorDto CR_Prea_Configuraciones_EdadPension_Guardar(int CodEmpresa, string usuario, [FromBody] CrPreaConfiguracionesEdadPensionGuardarRequest request)
        {
            return _bl.CR_Prea_Configuraciones_EdadPension_Guardar(CodEmpresa, usuario, request);
        }
    }
}