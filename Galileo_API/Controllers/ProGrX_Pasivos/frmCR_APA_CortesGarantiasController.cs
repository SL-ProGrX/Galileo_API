using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Pasivos;
using Galileo_API.Models.ProGrX_Pasivos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Pasivos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrApaCortesGarantiasController : ControllerBase
    {
        private readonly FrmCrApaCortesGarantiasBL _bl;

        public FrmCrApaCortesGarantiasController(IConfiguration config)
        {
            _bl = new FrmCrApaCortesGarantiasBL(config);
        }

        [HttpGet("CR_APA_CortesGarantias_Acreedores_Obtener")]
        public ErrorDto<List<FrmCrApaCortesGarantiasCatalogoDto>> CR_APA_CortesGarantias_Acreedores_Obtener(int codEmpresa)
        {
            return _bl.CR_APA_CortesGarantias_Acreedores_Obtener(codEmpresa);
        }

        [HttpGet("CR_APA_CortesGarantias_Operaciones_Obtener")]
        public ErrorDto<List<FrmCrApaCortesGarantiasCatalogoDto>> CR_APA_CortesGarantias_Operaciones_Obtener(int codEmpresa, string cod_acreedor)
        {
            return _bl.CR_APA_CortesGarantias_Operaciones_Obtener(codEmpresa, cod_acreedor);
        }

        [HttpGet("CR_APA_CortesGarantias_Encabezado_Obtener")]
        public ErrorDto<FrmCrApaCortesGarantiasEncabezadoDto?> CR_APA_CortesGarantias_Encabezado_Obtener(int codEmpresa, string operacion)
        {
            return _bl.CR_APA_CortesGarantias_Encabezado_Obtener(codEmpresa, operacion);
        }

        [HttpGet("CR_APA_CortesGarantias_Catalogo_Obtener")]
        public ErrorDto<List<FrmCrApaCortesGarantiasCatalogoDto>> CR_APA_CortesGarantias_Catalogo_Obtener(int codEmpresa, string tipo)
        {
            return _bl.CR_APA_CortesGarantias_Catalogo_Obtener(codEmpresa, tipo);
        }

        [HttpGet("CR_APA_CortesGarantias_Historico_Obtener")]
        public ErrorDto<List<FrmCrApaCortesGarantiasCorteDto>> CR_APA_CortesGarantias_Historico_Obtener(int codEmpresa, string cod_acreedor, string operacion)
        {
            return _bl.CR_APA_CortesGarantias_Historico_Obtener(codEmpresa, cod_acreedor, operacion);
        }

        [HttpGet("CR_APA_CortesGarantias_Corte_Obtener")]
        public ErrorDto<FrmCrApaCortesGarantiasCorteDatosDto?> CR_APA_CortesGarantias_Corte_Obtener(int codEmpresa, string request)
        {
            return _bl.CR_APA_CortesGarantias_Corte_Obtener(codEmpresa, request);
        }

        [HttpGet("CR_APA_CortesGarantias_Detalle_Obtener")]
        public ErrorDto<List<FrmCrApaCortesGarantiasDetalleDto>> CR_APA_CortesGarantias_Detalle_Obtener(int codEmpresa, string request)
        {
            return _bl.CR_APA_CortesGarantias_Detalle_Obtener(codEmpresa, request);
        }

        [HttpGet("CR_APA_CortesGarantias_Inclusiones_Obtener")]
        public ErrorDto<List<FrmCrApaCortesGarantiasDetalleDto>> CR_APA_CortesGarantias_Inclusiones_Obtener(int codEmpresa, string request)
        {
            return _bl.CR_APA_CortesGarantias_Inclusiones_Obtener(codEmpresa, request);
        }

        [HttpGet("CR_APA_CortesGarantias_Totales_Obtener")]
        public ErrorDto<FrmCrApaCortesGarantiasTotalesDto?> CR_APA_CortesGarantias_Totales_Obtener(int codEmpresa, string request)
        {
            return _bl.CR_APA_CortesGarantias_Totales_Obtener(codEmpresa, request);
        }

        [HttpPost("CR_APA_CortesGarantias_Guardar")]
        public ErrorDto CR_APA_CortesGarantias_Guardar(int codEmpresa, FrmCrApaCortesGarantiasGuardarRequest request)
        {
            return _bl.CR_APA_CortesGarantias_Guardar(codEmpresa, request);
        }

        [HttpPost("CR_APA_CortesGarantias_Cerrar")]
        public ErrorDto CR_APA_CortesGarantias_Cerrar(int codEmpresa, FrmCrApaCortesGarantiasClaveRequest request)
        {
            return _bl.CR_APA_CortesGarantias_Cerrar(codEmpresa, request);
        }

        [HttpPost("CR_APA_CortesGarantias_Actualizar")]
        public ErrorDto CR_APA_CortesGarantias_Actualizar(int codEmpresa, FrmCrApaCortesGarantiasClaveRequest request)
        {
            return _bl.CR_APA_CortesGarantias_Actualizar(codEmpresa, request);
        }

        [HttpDelete("CR_APA_CortesGarantias_Excluir")]
        public ErrorDto CR_APA_CortesGarantias_Excluir(int codEmpresa, [FromBody] FrmCrApaCortesGarantiasExcluirRequest request)
        {
            return _bl.CR_APA_CortesGarantias_Excluir(codEmpresa, request);
        }

        [HttpPost("CR_APA_CortesGarantias_Incluir")]
        public ErrorDto CR_APA_CortesGarantias_Incluir(int codEmpresa, FrmCrApaCortesGarantiasIncluirRequest request)
        {
            return _bl.CR_APA_CortesGarantias_Incluir(codEmpresa, request);
        }
    }
}
