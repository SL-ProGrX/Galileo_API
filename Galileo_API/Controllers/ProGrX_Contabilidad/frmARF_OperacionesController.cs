using Galileo.Models;
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
    public class FrmArfOperacionesController : ControllerBase
    {
        private readonly FrmArfOperacionesBl _bl;

        public FrmArfOperacionesController(IConfiguration config)
        {
            _bl = new FrmArfOperacionesBl(config);
        }

        [HttpGet("Divisas_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Divisas_Listar(int codEmpresa)
        {
            return _bl.Divisas_Listar(codEmpresa);
        }

        [HttpGet("Operaciones_Listar")]
        public ErrorDto<List<ArfOperacionBusquedaDto>> Operaciones_Listar(int codEmpresa)
        {
            return _bl.Operaciones_Listar(codEmpresa);
        }

        [HttpGet("Consultar")]
        public ErrorDto<ArfOperacionRegistroDto?> Consultar(int codEmpresa, int operacion)
        {
            return _bl.Consultar(codEmpresa, operacion);
        }

        [HttpGet("Scroll")]
        public ErrorDto<ArfOperacionRegistroDto?> Scroll(int codEmpresa, int operacion, int direccion)
        {
            return _bl.Scroll(codEmpresa, operacion, direccion);
        }

        [HttpGet("Arrendadores_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Arrendadores_Listar(int codEmpresa)
        {
            return _bl.Arrendadores_Listar(codEmpresa);
        }

        [HttpGet("Unidades_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Unidades_Listar(int codEmpresa)
        {
            return _bl.Unidades_Listar(codEmpresa);
        }

        [HttpPost("Guardar")]
        public ErrorDto<ArfOperacionGuardarResponseDto> Guardar(int codEmpresa, ArfOperacionGuardarRequestDto request)
        {
            return _bl.Guardar(codEmpresa, request);
        }

        [HttpPost("Activar")]
        public ErrorDto Activar(int codEmpresa, ArfOperacionActivarRequestDto request)
        {
            return _bl.Activar(codEmpresa, request);
        }

        [HttpGet("Plan_Listar")]
        public ErrorDto<List<ArfOperacionPlanDto>> Plan_Listar(int codEmpresa, int operacion)
        {
            return _bl.Plan_Listar(codEmpresa, operacion);
        }

        [HttpGet("Cierres_Listar")]
        public ErrorDto<List<ArfOperacionCierreDto>> Cierres_Listar(int codEmpresa, int operacion)
        {
            return _bl.Cierres_Listar(codEmpresa, operacion);
        }

        [HttpGet("AsientosMain_Listar")]
        public ErrorDto<List<ArfOperacionAsientoMainDto>> AsientosMain_Listar(
            int codEmpresa,
            int operacion,
            DateTime fechaInicio,
            DateTime fechaCorte)
        {
            return _bl.AsientosMain_Listar(codEmpresa, operacion, fechaInicio, fechaCorte);
        }

        [HttpGet("AsientoDetalle_Listar")]
        public ErrorDto<List<ArfOperacionAsientoDetalleDto>> AsientoDetalle_Listar(
            int codEmpresa,
            int codContabilidad,
            string tipoAsiento,
            string numAsiento)
        {
            return _bl.AsientoDetalle_Listar(codEmpresa, codContabilidad, tipoAsiento, numAsiento);
        }

        [HttpGet("Cambios_Listar")]
        public ErrorDto<List<ArfOperacionCambioDto>> Cambios_Listar(int codEmpresa, int operacion)
        {
            return _bl.Cambios_Listar(codEmpresa, operacion);
        }

        [HttpGet("CierreActual_Obtener")]
        public ErrorDto<ArfOperacionFiniquitoPreviewDto?> CierreActual_Obtener(int codEmpresa, int operacion)
        {
            return _bl.CierreActual_Obtener(codEmpresa, operacion);
        }

        [HttpPost("Cambio_Aplicar")]
        public ErrorDto Cambio_Aplicar(int codEmpresa, ArfOperacionCambioRequestDto request)
        {
            return _bl.Cambio_Aplicar(codEmpresa, request);
        }

        [HttpPost("Finiquito_Aplicar")]
        public ErrorDto Finiquito_Aplicar(int codEmpresa, ArfOperacionFiniquitoRequestDto request)
        {
            return _bl.Finiquito_Aplicar(codEmpresa, request);
        }
    }

}

