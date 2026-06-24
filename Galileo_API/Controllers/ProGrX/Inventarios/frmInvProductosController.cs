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
    public class FrmInvProductosController : ControllerBase
    {
        private readonly FrmInvProductosBL _bl;
        public FrmInvProductosController(IConfiguration config)
        {
            _bl = new FrmInvProductosBL(config);
        }

        [HttpGet("ConsultaAscDesc")]
        public ErrorDto<Producto> ConsultaAscDesc(int CodEmpresa, string Cod_Producto, string tipo)
        {
            return _bl.ConsultaAscDesc(CodEmpresa, Cod_Producto, tipo);
        }

        [HttpGet("ProveedoresProducto_Obtener")]
        public ErrorDto<ProvProductoDataLista> ProveedoresProducto_Obtener(int CodCliente, string producto, int? pagina, int? paginacion, string? filtro)
        {
            return _bl.ProveedoresProducto_Obtener(CodCliente, producto, pagina, paginacion, filtro);
        }

        [HttpGet("Producto_ObtenerTodos")]
        public ErrorDto<List<ProductoDto>> Producto_ObtenerTodos(int CodEmpresa)
        {
            return _bl.Producto_ObtenerTodos(CodEmpresa);
        }

        [HttpGet("Producto_ObtenerDetalle")]
        public ErrorDto<ProductoDto> Producto_ObtenerDetalle(int CodEmpresa, string Cod_Producto)
        {
            return _bl.Producto_ObtenerDetalle(CodEmpresa, Cod_Producto);
        }

        [HttpPost("Producto_Actualizar")]
        public ErrorDto Producto_Actualizar(int CodEmpresa, ProductoDto request)
        {
            return _bl.Producto_Actualizar(CodEmpresa, request);
        }

        [HttpPost("Producto_Insertar")]
        public ErrorDto Producto_Insertar(int CodEmpresa, ProductoDto request)
        {
            return _bl.Producto_Insertar(CodEmpresa, request);
        }

        [HttpGet("PreciosProducto_ObtenerTodos")]
        public ErrorDto<List<PrecioProducto>> PreciosProducto_ObtenerTodos(int CodEmpresa, string Cod_Producto)
        {
            return _bl.PreciosProducto_ObtenerTodos(CodEmpresa, Cod_Producto);
        }

        [HttpGet("MovimientosProducto_ObtenerTodos")]
        public ErrorDto<List<MovimientoProducto>> MovimientosProducto_ObtenerTodos(int CodEmpresa, string Cod_Producto)
        {
            return _bl.MovimientosProducto_ObtenerTodos(CodEmpresa, Cod_Producto);
        }

        [HttpGet("DescuentoProducto_ObtenerTodos")]
        public ErrorDto<List<DescuentoProducto>> DescuentoProducto_ObtenerTodos(int CodEmpresa, string Cod_Producto)
        {
            return _bl.DescuentoProducto_ObtenerTodos(CodEmpresa, Cod_Producto);
        }

        [HttpPost("DescuentoProducto_Agregar")]
        public ErrorDto DescuentoProducto_Agregar(int CodEmpresa, DescuentoProducto request)
        {
            return _bl.DescuentoProducto_Agregar(CodEmpresa, request);
        }

        [HttpPost("DescuentoProducto_Actualizar")]
        public ErrorDto DescuentoProducto_Actualizar(int CodEmpresa, DescuentoProducto request)
        {
            return _bl.DescuentoProducto_Actualizar(CodEmpresa, request);
        }

        [HttpPost("DescuentoProducto_Eliminar")]
        public ErrorDto DescuentoProducto_Eliminar(int CodEmpresa, DescuentoProducto request)
        {
            return _bl.DescuentoProducto_Eliminar(CodEmpresa, request);
        }

        [HttpGet("BonificacionProducto_ObtenerTodos")]        public ErrorDto<List<BonificacionProducto>> BonificacionProducto_ObtenerTodos(int CodEmpresa, string Cod_Producto)
        {
            return _bl.BonificacionProducto_ObtenerTodos(CodEmpresa, Cod_Producto);
        }

        [HttpPost("BonificacionProducto_Agregar")]
        public ErrorDto BonificacionProducto_Agregar(int CodEmpresa, BonificacionProducto request)
        {
            return _bl.BonificacionProducto_Agregar(CodEmpresa, request);
        }

        [HttpPost("BonificacionProducto_Actualizar")]
        public ErrorDto BonificacionProducto_Actualizar(int CodEmpresa, BonificacionProducto request)
        {
            return _bl.BonificacionProducto_Actualizar(CodEmpresa, request);
        }

        [HttpPost("BonificacionProducto_Eliminar")]
        public ErrorDto BonificacionProducto_Eliminar(int CodEmpresa, BonificacionProducto request)
        {
            return _bl.BonificacionProducto_Eliminar(CodEmpresa, request);
        }

        [HttpGet("ProveedorProducto_ObtenerTodos")]
        public ErrorDto<List<ProveedorProducto>> ProveedorProducto_ObtenerTodos(int CodEmpresa, string Cod_Producto)
        {
            return _bl.ProveedorProducto_ObtenerTodos(CodEmpresa, Cod_Producto);
        }

        [HttpPost("ProveedorProducto_Eliminar")]
        public ErrorDto ProveedorProducto_Eliminar(int CodEmpresa, ProveedorProducto request)
        {
            return _bl.ProveedorProducto_Eliminar(CodEmpresa, request);
        }

        [HttpPost("ProveedorProducto_Insertar")]
        public ErrorDto ProveedorProducto_Insertar(int CodEmpresa, ProveedorProducto request)
        {
            return _bl.ProveedorProducto_Insertar(CodEmpresa, request);
        }

        [HttpPost("BarrasProducto_Actualizar")]
        public ErrorDto BarrasProducto_Actualizar(int CodEmpresa, string codigoBarras, string Cod_Producto)
        {
            return _bl.BarrasProducto_Actualizar(CodEmpresa, codigoBarras, Cod_Producto);
        }

        [HttpPost("BodegaExistenciaProducto_Obtener")]
        public ErrorDto<List<BodegaExistenciaProducto>> BodegaExistenciaProducto_Obtener(int CodEmpresa, BodegaExistenciaProducto request)
        {
            return _bl.BodegaExistenciaProducto_Obtener(CodEmpresa, request);
        }

        [HttpGet("SimilaresProducto_ObtenerTodos")]
        public ErrorDto<List<SimilarProducto>> SimilaresProducto_ObtenerTodos(int CodEmpresa, string Cod_Producto)
        {
            return _bl.SimilaresProducto_ObtenerTodos(CodEmpresa, Cod_Producto);
        }

        [HttpPost("PrecioProducto_AgregarActualizar")]
        public ErrorDto PrecioProducto_AgregarActualizar(int CodEmpresa, PrecioProducto request)
        {
            return _bl.PrecioProducto_AgregarActualizar(CodEmpresa, request);
        }

        [HttpPost("SimilaresProducto_Actualizar")]
        public ErrorDto SimilaresProducto_Actualizar(int CodEmpresa, SimilarProducto request)
        {
            return _bl.SimilaresProducto_Actualizar(CodEmpresa, request);
        }

        [HttpPost("SimilaresProducto_Eliminar")]
        public ErrorDto SimilaresProducto_Eliminar(int CodEmpresa, SimilarProducto request)
        {
            return _bl.SimilaresProducto_Eliminar(CodEmpresa, request);
        }

        [HttpGet("Producto_ObtenerCabys")]
        public ErrorDto<CabysHereda> Producto_ObtenerCabys(int CodEmpresa, int Cod_Prodclas, string Cod_Linea_Sub)
        {
            return _bl.Producto_ObtenerCabys(CodEmpresa, Cod_Prodclas, Cod_Linea_Sub);
        }

        [HttpGet("UensProducto_Obtener")]
        public ErrorDto<List<UensProductos>> UensProducto_Obtener(int CodEmpresa, string Cod_Producto)
        {
            return _bl.UensProducto_Obtener(CodEmpresa, Cod_Producto);
        }

        [HttpPost("UensProducto_Actualizar")]
        public ErrorDto UensProducto_Actualizar(int CodEmpresa, UensProductos request)
        {
            return _bl.UensProducto_Actualizar(CodEmpresa, request);
        }

        [HttpGet("TipoActivoLista_Obtener")]
        public ErrorDto<List<TipoActivoList>> TipoActivoLista_Obtener(int CodEmpresa)
        {
            return _bl.TipoActivoLista_Obtener(CodEmpresa);
        }

        [HttpDelete("producto_Eliminar")]
        public ErrorDto producto_Eliminar(int CodEmpresa, string cod_producto)
        {
            return _bl.producto_Eliminar(CodEmpresa, cod_producto);
        }

        [HttpGet("BitacoraProducto_Obtener")]
        public ErrorDto<List<BitacoraProductosDto>> BitacoraProducto_Obtener(int CodCliente, string cod_producto)
        {
            return _bl.BitacoraProducto_Obtener(CodCliente, cod_producto);
        }
    }
}