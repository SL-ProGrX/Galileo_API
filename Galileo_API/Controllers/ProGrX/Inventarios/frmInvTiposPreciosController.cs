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
    public class FrmInvTiposPreciosController : ControllerBase
    {
        private readonly FrmInvTiposPreciosBL _bl;
        public FrmInvTiposPreciosController(IConfiguration config)
        {
            _bl = new FrmInvTiposPreciosBL(config);
        }

        [HttpGet("Precios_Obtener")]
        public ErrorDto<PreciosDataLista> Precios_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return _bl.Precios_Obtener(CodCliente, pagina, paginacion, filtro);
        }

        [HttpGet("Precios_ObtenerTodos")]
        public ErrorDto<List<Precio>> Precios_ObtenerTodos(int CodEmpresa)
        {
            return _bl.Precios_ObtenerTodos(CodEmpresa);
        }

        [HttpPost("Precios_Insertar")]
        public ErrorDto Precios_Insertar(int CodEmpresa, Precio request)
        {
            return _bl.Precios_Insertar(CodEmpresa, request);
        }

        [HttpPost("Precios_Actualizar")]
        public ErrorDto Precios_Actualizar(int CodEmpresa, Precio request)
        {
            return _bl.Precios_Actualizar(CodEmpresa, request);
        }

        [HttpPost("Precios_Eliminar")]
        public ErrorDto Precios_Eliminar(int CodEmpresa, string precio)
        {
            return _bl.Precios_Eliminar(CodEmpresa, precio);
        }
    }
}