using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

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

        /// <summary>Lista de productos de beneficios.</summary>
        public ErrorDto<ProductoDataLista> ProductoLista_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
            => _db.ProductoLista_Obtener(CodCliente, pagina, paginacion, filtro);

        /// <summary>Exporta la lista completa de productos.</summary>
        public ErrorDto<List<ProductoData>> Producto_Exportar(int CodCliente)
            => _db.Producto_Exportar(CodCliente);

        /// <summary>Guarda un producto (inserta o actualiza).</summary>
        public ErrorDto Producto_Guardar(int CodCliente, ProductoData producto, string usuario)
            => _db.Producto_Guardar(CodCliente, producto, usuario);

        /// <summary>Elimina un producto.</summary>
        public ErrorDto Producto_Eliminar(int CodCliente, string cod_producto)
            => _db.Producto_Eliminar(CodCliente, cod_producto);
    }
}
