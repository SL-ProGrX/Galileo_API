using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Beneficios;

namespace Galileo_API.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints del catálogo de Productos de Beneficios (frmAF_BeneficioProd).
    /// </summary>
    [Route("api/frmAF_BeneficioProd")]
    [ApiController]
    public class FrmAfBeneficioProdController : ControllerBase
    {
        private readonly FrmAfBeneficioProdBL _bl;

        public FrmAfBeneficioProdController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneficioProdBL(config);
        }

        /// <summary>Lista de productos de beneficios.</summary>
        [Authorize]
        [HttpGet("ProductoLista_Obtener")]
        public ErrorDto<ProductoDataLista> ProductoLista_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
            => _bl.ProductoLista_Obtener(CodCliente, pagina, paginacion, filtro);

        /// <summary>Exporta la lista completa de productos.</summary>
        [Authorize]
        [HttpGet("Producto_Exportar")]
        public ErrorDto<List<ProductoData>> Producto_Exportar(int CodCliente)
            => _bl.Producto_Exportar(CodCliente);

        /// <summary>Guarda un producto (inserta o actualiza).</summary>
        [Authorize]
        [HttpPost("Producto_Guardar")]
        public ErrorDto Producto_Guardar(int CodCliente, string usuario, [FromBody] ProductoData producto)
            => _bl.Producto_Guardar(CodCliente, producto, usuario);

        /// <summary>Elimina un producto.</summary>
        [Authorize]
        [HttpDelete("Producto_Eliminar")]
        public ErrorDto Producto_Eliminar(int CodCliente, string cod_producto)
            => _bl.Producto_Eliminar(CodCliente, cod_producto);
    }
}
