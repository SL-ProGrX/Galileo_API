using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        readonly DataBL Databl;

        public DataController(IConfiguration config)
        {
            Databl = new DataBL(config);
        }

        [HttpGet("Proveedores_Obtener")]
        public ErrorDto<ProveedoresDataLista> Proveedores_Obtener(int CodCliente, string filtro)
        {
            var jFiltro = JsonConvert.DeserializeObject<ProveedorDataFiltros>(filtro) ?? new ProveedorDataFiltros();
            return Databl.Proveedores_Obtener(CodCliente, jFiltro);
        }

        [HttpGet("Cargos_Obtener")]
        public CargoDataLista Cargos_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return Databl.Cargos_Obtener(CodCliente, pagina, paginacion, filtro);
        }

        [HttpGet("Bodegas_Obtener")]
        public BodegaDataLista Bodegas_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return Databl.Bodegas_Obtener(CodCliente, pagina, paginacion, filtro);
        }

        [HttpGet("Articulos_Obtener")]
        public ErrorDto<ArticuloDataLista> Articulos_Obtener(int CodCliente, string filtro)
        {
            var jFiltro = JsonConvert.DeserializeObject<ArticuloDataFiltros>(filtro) ?? new ArticuloDataFiltros();
            return Databl.Articulos_Obtener(CodCliente, jFiltro);
        }

        [HttpGet("Ordenes_Obtener")]
        public OrdenesDataLista Ordenes_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro, string? proveedor, string? familia)
        {
            return Databl.Ordenes_Obtener(CodCliente, pagina, paginacion, filtro, proveedor, familia);
        }

        [HttpGet("OrdenesFiltro_Obtener")]
        public OrdenesDataLista OrdenesFiltro_Obtener(int CodCliente, int? pagina, int? paginacion,
            string? filtro, string? proveedor, string? familia, string? subfamilia)
        {
            return Databl.OrdenesFiltro_Obtener(CodCliente, pagina, paginacion, filtro, proveedor, familia, subfamilia);
        }

        [HttpGet("Facturas_Obtener")]
        public FacturasDataLista Facturas_Obtener(int CodCliente, int CodProveedor, int? pagina, int? paginacion, string? filtro)
        {
            return Databl.Facturas_Obtener(CodCliente, CodProveedor, pagina, paginacion, filtro);
        }

        [HttpGet("Usuarios_Obtener")]
        public UsuarioDataLista Usuarios_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return Databl.Usuarios_Obtener(CodCliente, pagina, paginacion, filtro);
        }

        [HttpGet("FacturaProveedor_Obtener")]
        public FacturasProveedorLista FacturaProveedor_Obtener(int CodCliente, string filtros)
        {
            return Databl.FacturaProveedor_Obtener(CodCliente, filtros);
        }

        [HttpGet("Devoluciones_Obtener")]
        public CompraDevLista Devoluciones_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return Databl.Devoluciones_Obtener(CodCliente, pagina, paginacion, filtro);
        }

        [HttpGet("Beneficios_Obtener")]
        public BeneficioDataLista Beneficios_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return Databl.Beneficios_Obtener(CodCliente, pagina, paginacion, filtro);
        }

        [HttpGet("Socios_Obtener")]
        public ErrorDto<SociosDataLista> Socios_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return Databl.Socios_Obtener(CodCliente, pagina, paginacion, filtro);
        }

        [Authorize]
        [HttpGet("Socios_Obtenerv2")]
        public ErrorDto<TablasListaGenericaModel> Socios_Obtener(int CodEmpresa, string filtro)
        {
            return Databl.Socios_Obtener(CodEmpresa, filtro);
        }

        [HttpGet("BeneficioProducto_Obtener")]
        public BeneficioProductoLista BeneficioProducto_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return Databl.BeneficioProducto_Obtener(CodCliente, pagina, paginacion, filtro);
        }

        [HttpGet("Departamentos_Obtener")]
        public DepartamentoDataLista Departamentos_Obtener(int CodCliente, string Institucion, int? pagina, int? paginacion, string? filtro)
        {
            return Databl.Departamentos_Obtener(CodCliente, Institucion, pagina, paginacion, filtro);
        }

        [HttpGet("Catalogo_Obtener")]
        public List<CatalogosLista> Catalogo_Obtener(int CodCliente, int tipo, int modulo)
        {
            return Databl.Catalogo_Obtener(CodCliente, tipo, modulo);
        }

        [HttpGet("UENS_Obtener")]
        public ErrorDto<List<CatalogosLista>> UENS_Obtener(int CodEmpresa)
        {
            return Databl.UENS_Obtener(CodEmpresa);
        }

        [HttpGet("CompraOrdenProveedoresLista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CompraOrdenProveedoresLista_Obtener(int CodEmpresa)
        {
            return Databl.CompraOrdenProveedoresLista_Obtener(CodEmpresa);
        }

        [HttpGet("CompraOrdenFamiliaLista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CompraOrdenFamiliaLista_Obtener(int CodEmpresa)
        {
            return Databl.CompraOrdenFamiliaLista_Obtener(CodEmpresa);
        }

        [HttpGet("TipoProductoSub_ObtenerTodos")]
        public ErrorDto<List<TipoProductoSubGradaData>> TipoProductoSub_ObtenerTodos(int CodEmpresa, string Cod_Prodclas)
        {
            return Databl.TipoProductoSub_ObtenerTodos(CodEmpresa, Cod_Prodclas);

        }

        [Authorize]
        [HttpGet("Personas_Obtener")]
        public ErrorDto<TablasListaGenericaModel> Personas_Obtener(int CodEmpresa, string filtro)
        {
            return Databl.Personas_Obtener(CodEmpresa, filtro);
        }


    }

}

