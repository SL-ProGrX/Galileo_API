using PgxAPI.DataBaseTier;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;

namespace PgxAPI.BusinessLogic
{
    public class DataBL
    {
        private readonly IConfiguration _config;
        DataDB DbData;

        public DataBL(IConfiguration config)
        {
            _config = config;
            DbData = new DataDB(_config);
        }

        public ErrorDto<ProveedoresDataLista> Proveedores_Obtener(int CodCliente, ProveedorDataFiltros filtro)
        {
            return DbData.Proveedores_Obtener(CodCliente, filtro);
        }

        public CargoDataLista Cargos_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return DbData.Cargos_Obtener(CodCliente, pagina, paginacion, filtro);
        }

        public BodegaDataLista Bodegas_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return DbData.Bodegas_Obtener(CodCliente, pagina, paginacion, filtro);
        }

        public ErrorDto<ArticuloDataLista> Articulos_Obtener(int CodCliente, ArticuloDataFiltros filtro)
        {
            return DbData.Articulos_Obtener(CodCliente, filtro);
        }

        public OrdenesDataLista Ordenes_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro, string? proveedor, string? familia)
        {
            return DbData.Ordenes_Obtener(CodCliente, pagina, paginacion, filtro, proveedor, familia);
        }

        public OrdenesDataLista OrdenesFiltro_Obtener(int CodCliente, int? pagina, int? paginacion,
            string? filtro, string? proveedor, string? familia, string? subfamilia)
        {
            return DbData.OrdenesFiltro_Obtener(CodCliente, pagina, paginacion, filtro, proveedor, familia, subfamilia);
        }

        public FacturasDataLista Facturas_Obtener(int CodCliente, int CodProveedor, int? pagina, int? paginacion, string? filtro)
        {
            return DbData.ObtenerListaFacturas(CodCliente, CodProveedor, pagina, paginacion, filtro);
        }

        public UsuarioDataLista Usuarios_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return DbData.Usuarios_Obtener(CodCliente, pagina, paginacion, filtro);
        }

        public FacturasProveedorLista FacturaProveedor_Obtener(int CodCliente, string filtros)
        {
            return DbData.FacturaProveedor_Obtener(CodCliente, filtros);
        }

        public CompraDevLista Devoluciones_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return DbData.Devoluciones_Obtener(CodCliente, pagina, paginacion, filtro);
        }

        public BeneficioDataLista Beneficios_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return DbData.Beneficios_Obtener(CodCliente, pagina, paginacion, filtro);
        }

        public ErrorDto<SociosDataLista> Socios_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return DbData.Socios_Obtener(CodCliente, pagina, paginacion, filtro);
        }

        public BeneficioProductoLista BeneficioProducto_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return DbData.BeneficioProducto_Obtener(CodCliente, pagina, paginacion, filtro);
        }

        public DepartamentoDataLista Departamentos_Obtener(int CodCliente, string Institucion, int? pagina, int? paginacion, string? filtro)
        {
            return DbData.Departamentos_Obtener(CodCliente, Institucion, pagina, paginacion, filtro);
        }

        public List<CatalogosLista> Catalogo_Obtener(int CodCliente, int tipo, int modulo)
        {
            return DbData.Catalogo_Obtener(CodCliente, tipo, modulo);
        }

        public ErrorDto<List<CatalogosLista>> UENS_Obtener(int CodEmpresa)
        {
            return DbData.UENS_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CompraOrdenProveedoresLista_Obtener(int CodEmpresa)
        {
            return DbData.CompraOrdenProveedoresLista_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CompraOrdenFamiliaLista_Obtener(int CodEmpresa)
        {
            return DbData.CompraOrdenFamiliaLista_Obtener(CodEmpresa);
        }

        public ErrorDto<List<TipoProductoSubGradaData>> TipoProductoSub_ObtenerTodos(int CodEmpresa, string Cod_Prodclas)
        {
            return DbData.TipoProductoSub_ObtenerTodos(CodEmpresa, Cod_Prodclas);
        }

        public ErrorDto<TablasListaGenericaModel> Personas_Obtener(int CodEmpresa, string jfiltro)
        {
            return DbData.Personas_Obtener(CodEmpresa, jfiltro);
        }

        public ErrorDto<TablasListaGenericaModel> Socios_Obtener(int CodEmpresa, string jfiltro)
        {
            return DbData.Socios_Obtener(CodEmpresa, jfiltro);
        }
    }
}
