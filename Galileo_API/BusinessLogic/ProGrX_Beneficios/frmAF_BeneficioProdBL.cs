using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio del catálogo de Productos de Beneficios (frmAF_BeneficioProd).
    /// </summary>
    public class FrmAfBeneficioProdBL
    {
        private readonly FrmAfBeneficioProdDB _db;

        public FrmAfBeneficioProdBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneficioProdDB(config);
        }

        /// <summary>Lista de productos de beneficios con paginación, filtro y ordenamiento.</summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa serializados en JSON.</param>
        public ErrorDto<ProductoDataLista> AfiBeneficioProd_ProductoLista_Obtener(int CodCliente, string? filtros)
            => _db.AfiBeneficioProd_ProductoLista_Obtener(CodCliente, DeserializarFiltros(filtros));

        /// <summary>Exporta la lista de productos aplicando el filtro vigente, sin paginar.</summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">Filtros de carga perezosa serializados en JSON.</param>
        public ErrorDto<List<ProductoData>> AfiBeneficioProd_Producto_Exportar(int CodCliente, string? filtros)
            => _db.AfiBeneficioProd_Producto_Exportar(CodCliente, DeserializarFiltros(filtros));

        /// <summary>
        /// Convierte el JSON de filtros recibido desde Angular en el modelo de carga perezosa.
        /// </summary>
        /// <param name="filtros">Filtros serializados en JSON.</param>
        /// <returns>Filtros deserializados; instancia vacía si el JSON viene nulo o inválido.</returns>
        private static FiltrosLazyLoadData DeserializarFiltros(string? filtros)
        {
            if (string.IsNullOrWhiteSpace(filtros))
            {
                return new FiltrosLazyLoadData();
            }

            return JsonConvert.DeserializeObject<FiltrosLazyLoadData>(filtros) ?? new FiltrosLazyLoadData();
        }

        /// <summary>Guarda un producto (inserta o actualiza).</summary>
        public ErrorDto AfiBeneficioProd_Producto_Guardar(int CodCliente, ProductoData producto, string usuario)
            => _db.AfiBeneficioProd_Producto_Guardar(CodCliente, producto, usuario);

        /// <summary>Elimina un producto.</summary>
        public ErrorDto AfiBeneficioProd_Producto_Eliminar(int CodCliente, string cod_producto)
            => _db.AfiBeneficioProd_Producto_Eliminar(CodCliente, cod_producto);
    }
}
