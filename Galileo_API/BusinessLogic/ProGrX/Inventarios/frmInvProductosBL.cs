using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvProductosBL
    {
        private readonly FrmInvProductosDB _db;

        public FrmInvProductosBL(IConfiguration config)
        {
            _db = new FrmInvProductosDB(config);
        }

        public ErrorDto<Producto> ConsultaAscDesc(int CodEmpresa, string Cod_Producto, string tipo)
        {
            return _db.ConsultaAscDesc(CodEmpresa, Cod_Producto, tipo);
        }

        public ErrorDto<ProvProductoDataLista> ProveedoresProducto_Obtener(int CodCliente, string producto, int? pagina, int? paginacion, string? filtro)
        {
            return _db.ProveedoresProducto_Obtener(CodCliente, producto, pagina, paginacion, filtro);
        }

        public ErrorDto<List<ProductoDto>> Producto_ObtenerTodos(int CodEmpresa)
        {
            return _db.Producto_ObtenerTodos(CodEmpresa);
        }

        public ErrorDto<ProductoDto> Producto_ObtenerDetalle(int CodEmpresa, string Cod_Producto)
        {
            return _db.Producto_ObtenerDetalle(CodEmpresa, Cod_Producto);
        }

        public ErrorDto<List<PrecioProducto>> PreciosProducto_ObtenerTodos(int CodEmpresa, string Cod_Producto)
        {
            return _db.PreciosProducto_ObtenerTodos(CodEmpresa, Cod_Producto);
        }

        public ErrorDto<List<MovimientoProducto>> MovimientosProducto_ObtenerTodos(int CodEmpresa, string Cod_Producto)
        {
            return _db.MovimientosProducto_ObtenerTodos(CodEmpresa, Cod_Producto);
        }


        public ErrorDto<List<DescuentoProducto>> DescuentoProducto_ObtenerTodos(int CodEmpresa, string Cod_Producto)
        {
            return _db.DescuentoProducto_ObtenerTodos(CodEmpresa, Cod_Producto);
        }

        public ErrorDto<List<BonificacionProducto>> BonificacionProducto_ObtenerTodos(int CodEmpresa, string Cod_Producto)
        {
            return _db.BonificacionProducto_ObtenerTodos(CodEmpresa, Cod_Producto);
        }

        public ErrorDto<List<SimilarProducto>> SimilaresProducto_ObtenerTodos(int CodEmpresa, string Cod_Producto)
        {
            return _db.SimilaresProducto_ObtenerTodos(CodEmpresa, Cod_Producto);
        }

        public ErrorDto Producto_Actualizar(int CodEmpresa, ProductoDto request)
        {
            return _db.Producto_Actualizar(CodEmpresa, request);
        }

        public ErrorDto Producto_Insertar(int CodEmpresa, ProductoDto request)
        {
            return _db.Producto_Insertar(CodEmpresa, request);
        }

        public ErrorDto PrecioProducto_AgregarActualizar(int CodEmpresa, PrecioProducto request)
        {
            return _db.PrecioProducto_AgregarActualizar(CodEmpresa, request);
        }

        public ErrorDto DescuentoProducto_Agregar(int CodEmpresa, DescuentoProducto request)
        {
            return _db.DescuentoProducto_Agregar(CodEmpresa, request);
        }

        public ErrorDto DescuentoProducto_Actualizar(int CodEmpresa, DescuentoProducto request)
        {
            return _db.DescuentoProducto_Actualizar(CodEmpresa, request);
        }

        public ErrorDto DescuentoProducto_Eliminar(int CodEmpresa, DescuentoProducto request)
        {
            return _db.DescuentoProducto_Eliminar(CodEmpresa, request);
        }

        public ErrorDto BonificacionProducto_Agregar(int CodEmpresa, BonificacionProducto request)
        {
            return _db.BonificacionProducto_Agregar(CodEmpresa, request);
        }

        public ErrorDto BonificacionProducto_Actualizar(int CodEmpresa, BonificacionProducto request)
        {
            return _db.BonificacionProducto_Actualizar(CodEmpresa, request);
        }

        public ErrorDto BonificacionProducto_Eliminar(int CodEmpresa, BonificacionProducto request)
        {
            return _db.BonificacionProducto_Eliminar(CodEmpresa, request);
        }

        public ErrorDto SimilaresProducto_Actualizar(int CodEmpresa, SimilarProducto request)
        {
            return _db.SimilaresProducto_Actualizar(CodEmpresa, request);
        }

        public ErrorDto SimilaresProducto_Eliminar(int CodEmpresa, SimilarProducto request)
        {
            return _db.SimilaresProducto_Eliminar(CodEmpresa, request);
        }

        public ErrorDto<List<ProveedorProducto>> ProveedorProducto_ObtenerTodos(int CodEmpresa, string Cod_Producto)
        {
            return _db.ProveedorProducto_ObtenerTodos(CodEmpresa, Cod_Producto);
        }

        public ErrorDto ProveedorProducto_Insertar(int CodEmpresa, ProveedorProducto request)
        {
            return _db.ProveedorProducto_Insertar(CodEmpresa, request);
        }

        public ErrorDto ProveedorProducto_Eliminar(int CodEmpresa, ProveedorProducto request)
        {
            return _db.ProveedorProducto_Eliminar(CodEmpresa, request);
        }

        public ErrorDto BarrasProducto_Actualizar(int CodEmpresa, string codigoBarras, string Cod_Producto)
        {
            return _db.BarrasProducto_Actualizar(CodEmpresa, codigoBarras, Cod_Producto);
        }

        public ErrorDto<CabysHereda> Producto_ObtenerCabys(int CodEmpresa, int Cod_Prodclas, string Cod_Linea_Sub)
        {
            return _db.Producto_ObtenerCabys(CodEmpresa, Cod_Prodclas, Cod_Linea_Sub);
        }

        public ErrorDto<List<BodegaExistenciaProducto>> BodegaExistenciaProducto_Obtener(int CodEmpresa, BodegaExistenciaProducto request)
        {
            return _db.BodegaExistenciaProducto_Obtener(CodEmpresa, request);
        }

        public ErrorDto<List<UensProductos>> UensProducto_Obtener(int CodEmpresa, string Cod_Producto)
        {
            return _db.UensProducto_Obtener(CodEmpresa, Cod_Producto);
        }

        public ErrorDto UensProducto_Actualizar(int CodEmpresa, UensProductos request)
        {
            return _db.UensProducto_Actualizar(CodEmpresa, request);
        }

        public ErrorDto<List<TipoActivoList>> TipoActivoLista_Obtener(int CodEmpresa)
        {
            return _db.TipoActivoLista_Obtener(CodEmpresa);
        }

        public ErrorDto producto_Eliminar(int CodEmpresa, string cod_producto)
        {
            return _db.producto_Eliminar(CodEmpresa, cod_producto);
        }

        public ErrorDto<List<BitacoraProductosDto>> BitacoraProducto_Obtener(int CodCliente, string cod_producto)
        {
            return _db.BitacoraProducto_Obtener(CodCliente, cod_producto);
        }
    }
}