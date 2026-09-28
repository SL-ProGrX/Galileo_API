using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public sealed class FrmInvTomaFisicaController : ControllerBase
    {
        private readonly FrmInvTomaFisicaBL _bl;

        public FrmInvTomaFisicaController(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _bl = new FrmInvTomaFisicaBL(config);
        }

        [HttpGet("INV_TomaFisica_Lista_Obtener")]
        public ErrorDto<List<TomaFisicaDto>> INV_TomaFisica_Lista_Obtener(
            int CodEmpresa,
            string filtros)
        {
            return _bl.INV_TomaFisica_Lista_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("INV_TomaFisica_Detalle_Obtener")]
        public ErrorDto<List<TomaFisicaDetalleDto>> INV_TomaFisica_Detalle_Obtener(
            int CodEmpresa,
            string filtros)
        {
            return _bl.INV_TomaFisica_Detalle_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("INV_TomaFisica_Consecutivo_Obtener")]
        public ErrorDto<TomaFisicaDto> INV_TomaFisica_Consecutivo_Obtener(
            int CodEmpresa,
            int consecutivo)
        {
            return _bl.INV_TomaFisica_Consecutivo_Obtener(
                CodEmpresa,
                consecutivo);
        }

        [HttpGet("INV_TomaFisica_Consecutivo_Navegar")]
        public ErrorDto<TomaFisicaDto> INV_TomaFisica_Consecutivo_Navegar(
            int CodEmpresa,
            int consecutivo,
            string? tipo)
        {
            return _bl.INV_TomaFisica_Consecutivo_Navegar(
                CodEmpresa,
                consecutivo,
                tipo);
        }

        [HttpGet("INV_TomaFisica_Producto_Obtener")]
        public ErrorDto<TomaFisicaDetalleDto> INV_TomaFisica_Producto_Obtener(
            int CodEmpresa,
            string filtros)
        {
            return _bl.INV_TomaFisica_Producto_Obtener(CodEmpresa, filtros);
        }

        [HttpPost("INV_TomaFisica_Guardar")]
        public ErrorDto<int> INV_TomaFisica_Guardar(
            int CodEmpresa,
            TomaFisicaGuardarRequest request)
        {
            return _bl.INV_TomaFisica_Guardar(CodEmpresa, request);
        }

        [HttpDelete("INV_TomaFisica_Eliminar")]
        public ErrorDto INV_TomaFisica_Eliminar(
            int CodEmpresa,
            int consecutivo,
            string? usuario)
        {
            return _bl.INV_TomaFisica_Eliminar(
                CodEmpresa,
                consecutivo,
                usuario);
        }

        [HttpPost("INV_TomaFisica_Barras_Guardar")]
        public ErrorDto INV_TomaFisica_Barras_Guardar(
            int CodEmpresa,
            TomaFisicaDetalleDto linea)
        {
            return _bl.INV_TomaFisica_Barras_Guardar(CodEmpresa, linea);
        }

        [HttpPost("INV_TomaFisica_InventarioLogico_Obtener")]
        public ErrorDto<List<TomaFisicaDetalleDto>>
            INV_TomaFisica_InventarioLogico_Obtener(
                int CodEmpresa,
                TomaFisicaInventarioRequest request)
        {
            return _bl.INV_TomaFisica_InventarioLogico_Obtener(
                CodEmpresa,
                request);
        }

        [HttpPost("INV_TomaFisica_Comparar")]
        public ErrorDto<List<TomaFisicaDetalleDto>> INV_TomaFisica_Comparar(
            int CodEmpresa,
            TomaFisicaGuardarRequest request)
        {
            return _bl.INV_TomaFisica_Comparar(CodEmpresa, request);
        }
    }
}
