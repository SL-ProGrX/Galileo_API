using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.BusinessLogic
{
    public class FrmInvTiposProductosBL
    {
        private readonly FrmInvTiposProductosDB _db;
        public FrmInvTiposProductosBL(IConfiguration config)
        {
            _db = new FrmInvTiposProductosDB(config);

        }

        public ErrorDto<TipoProductoDataLista> TipoProducto_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro, int cod_contabilidad)
        {
            return _db.TipoProducto_Obtener(CodCliente, pagina, paginacion, filtro, cod_contabilidad);
        }

        public List<TipoProductoDto> TipoProducto_ObtenerTodos(int CodEmpresa, int cod_contabilidad)
        {
            return _db.TipoProducto_ObtenerTodos(CodEmpresa, cod_contabilidad);
        }

        public ErrorDto TipoProducto_Insertar(int CodEmpresa, TipoProductoDto request)
        {
            return _db.TipoProducto_Insertar(CodEmpresa, request);
        }

        public ErrorDto TipoProducto_Actualizar(int CodEmpresa, TipoProductoDto request)
        {
            return _db.TipoProducto_Actualizar(CodEmpresa, request);
        }

        public ErrorDto TipoProducto_Eliminar(int CodEmpresa, string producto)
        {
            return _db.TipoProducto_Eliminar(CodEmpresa, producto);
        }

        public ErrorDto<TipoProductoSubDataLista> TipoProductoSub_Obtener(int CodCliente, int ProdClas, int? pagina, int? paginacion, string? filtro)
        {
            return _db.TipoProductoSub_Obtener(CodCliente, ProdClas, pagina, paginacion, filtro);
        }

        public ErrorDto<List<TipoProductoSubGradaData>> TipoProductoSub_ObtenerTodos(int CodEmpresa, string Cod_Prodclas)
        {
            return _db.TipoProductoSub_ObtenerTodos(CodEmpresa, Cod_Prodclas);
        }

        public ErrorDto TipoProductoSub_Insertar(int CodEmpresa, TipoProductoSubDto request)
        {
            return _db.TipoProductoSub_Insertar(CodEmpresa, request);
        }

        public ErrorDto TipoProductoSub_Actualizar(int CodEmpresa, TipoProductoSubDto request)
        {
            return _db.TipoProductoSub_Actualizar(CodEmpresa, request);
        }

        public ErrorDto<List<InvCabys>> Cabys_ObtenerTodos(int CodEmpresa)
        {
            return _db.Cabys_ObtenerTodos(CodEmpresa);
        }

        public ErrorDto<List<InvCabys>> Cabys_Obtener(int CodEmpresa, string filtro)
        {
            return _db.Cabys_Obtener(CodEmpresa, filtro);
        }
    }
}