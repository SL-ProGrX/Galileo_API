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
    public class FrmCrApaOperacionesController : ControllerBase
    {
        private readonly FrmCrApaOperacionesBL _bl;

        public FrmCrApaOperacionesController(IConfiguration config)
        {
            _bl = new FrmCrApaOperacionesBL(config);
        }

        [HttpGet("CR_APA_Operaciones_Acreedores_Obtener")]
        public ErrorDto<List<FrmCrApaOperacionAcreedorDto>> CR_APA_Operaciones_Acreedores_Obtener(int codEmpresa)
        {
            return _bl.CR_APA_Operaciones_Acreedores_Obtener(codEmpresa);
        }

        [HttpGet("CR_APA_Operaciones_Contactos_Obtener")]
        public ErrorDto<List<FrmCrApaOperacionContactoDto>> CR_APA_Operaciones_Contactos_Obtener(
            int codEmpresa,
            string cod_acreedor)
        {
            return _bl.CR_APA_Operaciones_Contactos_Obtener(codEmpresa, cod_acreedor);
        }

        [HttpGet("CR_APA_Operaciones_Obtener")]
        public ErrorDto<FrmCrApaOperacionListaDto> CR_APA_Operaciones_Obtener(
            int codEmpresa,
            string cod_acreedor,
            string? operacion,
            string? estado,
            string? filtro)
        {
            return _bl.CR_APA_Operaciones_Obtener(
                codEmpresa,
                cod_acreedor,
                operacion ?? string.Empty,
                estado ?? "T",
                filtro ?? "{}");
        }

        [HttpGet("CR_APA_Operacion_Obtener")]
        public ErrorDto<FrmCrApaOperacionDatosDto> CR_APA_Operacion_Obtener(
            int codEmpresa,
            string cod_acreedor,
            string operacion)
        {
            return _bl.CR_APA_Operacion_Obtener(codEmpresa, cod_acreedor, operacion);
        }

        [HttpPost("CR_APA_Operacion_Insertar")]
        public ErrorDto<int> CR_APA_Operacion_Insertar(
            int codEmpresa,
            FrmCrApaOperacionGuardarRequest request)
        {
            return _bl.CR_APA_Operacion_Insertar(codEmpresa, request);
        }

        [HttpPut("CR_APA_Operacion_Actualizar")]
        public ErrorDto<int> CR_APA_Operacion_Actualizar(
            int codEmpresa,
            FrmCrApaOperacionGuardarRequest request)
        {
            return _bl.CR_APA_Operacion_Actualizar(codEmpresa, request);
        }

        [HttpPut("CR_APA_Operacion_Cerrar")]
        public ErrorDto<int> CR_APA_Operacion_Cerrar(
            int codEmpresa,
            string cod_acreedor,
            string operacion)
        {
            return _bl.CR_APA_Operacion_Cerrar(codEmpresa, cod_acreedor, operacion);
        }

        [HttpGet("CR_APA_Operacion_PagosCantidad")]
        public ErrorDto<int> CR_APA_Operacion_PagosCantidad(
            int codEmpresa,
            string cod_acreedor,
            string operacion)
        {
            return _bl.CR_APA_Operacion_PagosCantidad(codEmpresa, cod_acreedor, operacion);
        }

        [HttpGet("CR_APA_Operaciones_Lineas_Obtener")]
        public ErrorDto<List<FrmCrApaOperacionCatalogoDto>> CR_APA_Operaciones_Lineas_Obtener(
            int codEmpresa,
            string cod_acreedor)
        {
            return _bl.CR_APA_Operaciones_Lineas_Obtener(codEmpresa, cod_acreedor);
        }

        [HttpGet("CR_APA_Operaciones_Oficinas_Obtener")]
        public ErrorDto<List<FrmCrApaOperacionCatalogoDto>> CR_APA_Operaciones_Oficinas_Obtener(int codEmpresa)
        {
            return _bl.CR_APA_Operaciones_Oficinas_Obtener(codEmpresa);
        }

        [HttpGet("CR_APA_Operaciones_Divisas_Obtener")]
        public ErrorDto<List<FrmCrApaOperacionCatalogoDto>> CR_APA_Operaciones_Divisas_Obtener(int codEmpresa)
        {
            return _bl.CR_APA_Operaciones_Divisas_Obtener(codEmpresa);
        }

        [HttpGet("CR_APA_Operaciones_Pagos_Obtener")]
        public ErrorDto<FrmCrApaOperacionPagoListaDto> CR_APA_Operaciones_Pagos_Obtener(
            int codEmpresa,
            string cod_acreedor,
            string operacion,
            string? estado,
            DateTime? fecha_desde,
            DateTime? fecha_hasta,
            string? filtro)
        {
            return _bl.CR_APA_Operaciones_Pagos_Obtener(
                codEmpresa,
                cod_acreedor,
                operacion,
                estado ?? "T",
                fecha_desde,
                fecha_hasta,
                filtro ?? "{}");
        }

        [HttpGet("CR_APA_Operaciones_Pago_Obtener")]
        public ErrorDto<FrmCrApaOperacionPagoDatosDto> CR_APA_Operaciones_Pago_Obtener(
            int codEmpresa,
            string cod_acreedor,
            string operacion,
            int npago)
        {
            return _bl.CR_APA_Operaciones_Pago_Obtener(codEmpresa, cod_acreedor, operacion, npago);
        }

        [HttpGet("CR_APA_Operaciones_UltimoPago_Obtener")]
        public ErrorDto<FrmCrApaOperacionUltimoPagoDto> CR_APA_Operaciones_UltimoPago_Obtener(
            int codEmpresa,
            string cod_acreedor,
            string operacion,
            int? npago)
        {
            return _bl.CR_APA_Operaciones_UltimoPago_Obtener(codEmpresa, cod_acreedor, operacion, npago);
        }

        [HttpPost("CR_APA_Operaciones_Pago_Insertar")]
        public ErrorDto<int> CR_APA_Operaciones_Pago_Insertar(
            int codEmpresa,
            FrmCrApaOperacionPagoGuardarRequest request)
        {
            return _bl.CR_APA_Operaciones_Pago_Insertar(codEmpresa, request);
        }

        [HttpGet("CR_APA_Operaciones_Autorizados_Obtener")]
        public ErrorDto<List<FrmCrApaOperacionAutorizadoDto>> CR_APA_Operaciones_Autorizados_Obtener(
            int codEmpresa,
            string cod_acreedor)
        {
            return _bl.CR_APA_Operaciones_Autorizados_Obtener(codEmpresa, cod_acreedor);
        }

        [HttpPut("CR_APA_Operaciones_Pago_Autorizado_Actualizar")]
        public ErrorDto<int> CR_APA_Operaciones_Pago_Autorizado_Actualizar(
            int codEmpresa,
            FrmCrApaOperacionAsignarAutorizadoRequest request)
        {
            return _bl.CR_APA_Operaciones_Pago_Autorizado_Actualizar(codEmpresa, request);
        }
    }
}
