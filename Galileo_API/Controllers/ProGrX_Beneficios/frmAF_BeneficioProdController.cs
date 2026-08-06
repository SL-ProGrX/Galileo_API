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
    [Authorize]
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

        /// <summary>Lista de productos de beneficios con paginación, filtro y ordenamiento.</summary>
        [HttpGet("AfiBeneficioProd_ProductoLista_Obtener")]
        public ErrorDto<ProductoDataLista> AfiBeneficioProd_ProductoLista_Obtener(int CodCliente, string? filtros)
            => _bl.AfiBeneficioProd_ProductoLista_Obtener(CodCliente, filtros);

        /// <summary>Exporta la lista de productos aplicando el filtro vigente, sin paginar.</summary>
        [HttpGet("AfiBeneficioProd_Producto_Exportar")]
        public ErrorDto<List<ProductoData>> AfiBeneficioProd_Producto_Exportar(int CodCliente, string? filtros)
            => _bl.AfiBeneficioProd_Producto_Exportar(CodCliente, filtros);

        /// <summary>Guarda un producto (inserta o actualiza).</summary>
        [HttpPost("AfiBeneficioProd_Producto_Guardar")]
        public ErrorDto AfiBeneficioProd_Producto_Guardar(int CodCliente, string usuario, [FromBody] ProductoData producto)
            => _bl.AfiBeneficioProd_Producto_Guardar(CodCliente, producto, usuario);

        /// <summary>Elimina un producto.</summary>
        [HttpDelete("AfiBeneficioProd_Producto_Eliminar")]
        public ErrorDto AfiBeneficioProd_Producto_Eliminar(int CodCliente, string cod_producto)
            => _bl.AfiBeneficioProd_Producto_Eliminar(CodCliente, cod_producto);
    }
}
