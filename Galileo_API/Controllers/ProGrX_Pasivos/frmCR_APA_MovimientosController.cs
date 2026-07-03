using Galileo.Models;
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
    public class FrmCrApaMovimientosController : ControllerBase
    {
        private readonly FrmCrApaMovimientosBL _bl;

        public FrmCrApaMovimientosController(IConfiguration config)
        {
            _bl = new FrmCrApaMovimientosBL(config);
        }

        [HttpGet("CR_APA_Movimientos_Acreedor_Obtener")]
        public ErrorDto<FrmCrApaMovimientosAcreedorDto?> CR_APA_Movimientos_Acreedor_Obtener(
            int codEmpresa,
            string cod_acreedor)
        {
            return _bl.CR_APA_Movimientos_Acreedor_Obtener(codEmpresa, cod_acreedor);
        }

        [HttpGet("CR_APA_Movimientos_Operacion_Obtener")]
        public ErrorDto<FrmCrApaMovimientosOperacionDto?> CR_APA_Movimientos_Operacion_Obtener(
            int codEmpresa,
            string cod_acreedor,
            string operacion)
        {
            return _bl.CR_APA_Movimientos_Operacion_Obtener(codEmpresa, cod_acreedor, operacion);
        }

        [HttpGet("CR_APA_Movimientos_Detalle_Obtener")]
        public ErrorDto<List<FrmCrApaMovimientosDetalleDto>> CR_APA_Movimientos_Detalle_Obtener(
            int codEmpresa,
            string cod_acreedor,
            string operacion)
        {
            return _bl.CR_APA_Movimientos_Detalle_Obtener(codEmpresa, cod_acreedor, operacion);
        }

        [HttpGet("CR_APA_Movimientos_Cuenta_Obtener")]
        public ErrorDto<FrmCrApaMovimientosCuentaDto?> CR_APA_Movimientos_Cuenta_Obtener(int codEmpresa, string usuario)
        {
            return _bl.CR_APA_Movimientos_Cuenta_Obtener(codEmpresa, usuario);
        }

        [HttpGet("CR_APA_Movimientos_Operacion_Navegar")]
        public ErrorDto<FrmCrApaMovimientosNavegarDto?> CR_APA_Movimientos_Operacion_Navegar(
            int codEmpresa,
            string request)
        {
            return _bl.CR_APA_Movimientos_Operacion_Navegar(codEmpresa, request);
        }

        [HttpPost("CR_APA_Movimientos_Aplicar")]
        public ErrorDto<FrmCrApaMovimientosAplicarResultadoDto?> CR_APA_Movimientos_Aplicar(
            int codEmpresa,
            FrmCrApaMovimientosAplicarRequest request)
        {
            return _bl.CR_APA_Movimientos_Aplicar(codEmpresa, request);
        }

        [HttpGet("CR_APA_Movimientos_Acreedores_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_APA_Movimientos_Acreedores_Obtener(int codEmpresa)
        {
            return _bl.CR_APA_Movimientos_Acreedores_Obtener(codEmpresa);
        }

        [HttpGet("CR_APA_Movimientos_Operaciones_Obtener")]
        public ErrorDto<List<FrmCrApaMovimientosOperacionBusquedaDto>> CR_APA_Movimientos_Operaciones_Obtener(
            int codEmpresa,
            string cod_acreedor)
        {
            return _bl.CR_APA_Movimientos_Operaciones_Obtener(codEmpresa, cod_acreedor);
        }
    }
}