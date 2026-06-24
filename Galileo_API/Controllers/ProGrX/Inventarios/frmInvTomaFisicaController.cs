using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmInvTomaFisicaController : ControllerBase
    {
        private readonly FrmInvTomaFisicaBL _bl;
        public FrmInvTomaFisicaController(IConfiguration config)
        {
            _bl = new FrmInvTomaFisicaBL(config);
        }

        [HttpGet("TomaFisica_Obtener")]
        public ErrorDto<List<TomaFisicaDto>> Facturas_Obtener(int CodEmpresa, int Cod_Proveedor, int? pagina, int? paginacion, string? filtro)
        {
            return _bl.TomaFisica_Obtener(CodEmpresa, Cod_Proveedor, pagina, paginacion, filtro);
        }

        [HttpGet("tomasFisicasDetalle_Obtener")]
        public ErrorDto<List<TomaFisicaDetalleDto>> tomasFisicasDetalle_Obtener(int CodEmpresa, int Consecutivo, int? pagina, int? paginacion, string? filtro)
        {
            return _bl.tomasFisicasDetalle_Obtener(CodEmpresa, Consecutivo, pagina, paginacion, filtro);
        }

        [HttpPost("tomaFisica_Insertar")]
        public ErrorDto tomaFisica_Insertar(int CodEmpresa, TomaFisicaDto data)
        {
            return _bl.tomaFisica_Insertar(CodEmpresa, data);
        }

        [HttpPost("tomaFisicaDetalle_Insertar")]
        public ErrorDto tomaFisicaDetalle_Insertar(int CodEmpresa, TomaFisicaDetalleDto data)
        {
            return _bl.tomaFisicaDetalle_Insertar(CodEmpresa, data);
        }

        [HttpGet("ConsultaAscDesc")]
        public ErrorDto<TomaFisicaDto> ConsultaAscDesc(int CodEmpresa, int consecutivo, string tipo)
        {
            return _bl.ConsultaAscDesc(CodEmpresa, consecutivo, tipo);
        }

        [HttpGet("tomaFisicaConsecutivo_Obtener")]
        public ErrorDto<TomaFisicaDto> ProveedorDetalle_Obtener(int CodEmpresa, int consecutivo)
        {
            return _bl.tomaFisicaConsecutivo_Obtener(CodEmpresa, consecutivo);
        }

        [HttpPost("actualizarTomaFisica")]
        public ErrorDto actualizarTomaFisica(int CodEmpresa, TomaFisicaDto request)
        {
            return _bl.actualizarTomaFisica(CodEmpresa, request);
        }

        [HttpPost("actualizarTomaFisicaDetalle")]
        public ErrorDto actualizarTomaFisicaDetalle(int CodEmpresa, TomaFisicaDetalleDto data)
        {
            return _bl.actualizarTomaFisicaDetalle(CodEmpresa, data);
        }

        [HttpDelete("EliminarDetalleTomaFisica")]
        public ErrorDto EliminarDetalleTomaFisica(int CodEmpresa, int consecutivo, string cod_producto)
        {
            return _bl.EliminarDetalleTomaFisica(CodEmpresa, consecutivo, cod_producto);
        }

        [HttpDelete("EliminarTomaFisica")]
        public ErrorDto EliminarTomaFisica(int CodEmpresa, int consecutivo)
        {
            return _bl.EliminarTomaFisica(CodEmpresa, consecutivo);
        }

        [HttpGet("TomaFisicaProdBarras_Obtener")]
        public ErrorDto<TomaFisicaDetalleDto> TomaFisicaProdBarras_Obtener(
            int CodEmpresa, string cod_bodega, string cod_barras, string tipo)
        {
            return _bl.TomaFisicaProdBarras_Obtener(CodEmpresa, cod_bodega, cod_barras, tipo);
        }

        [HttpPost("TomaFisicaBarras_Guardar")]
        public ErrorDto TomaFisicaBarras_Guardar(int CodEmpresa, TomaFisicaDetalleDto linea)
        { 
            return _bl.TomaFisicaBarras_Guardar(CodEmpresa, linea);
        }
    }
}