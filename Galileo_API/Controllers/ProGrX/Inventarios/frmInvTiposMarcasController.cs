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
    public class FrmInvTiposMarcasController : ControllerBase
    {
        private readonly FrmInvTiposMarcasBL _bl;

        public FrmInvTiposMarcasController(IConfiguration config)
        {
            _bl = new FrmInvTiposMarcasBL(config);
        }

        [HttpGet("Marcas_Obtener")]
        public ErrorDto<MarcasDataLista> Marcas_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return _bl.Marcas_Obtener(CodCliente, pagina, paginacion, filtro);
        }

        [HttpGet("Marcas_ObtenerTodos")]
        public ErrorDto<List<MarcasDto>> Marcas_ObtenerTodos(int CodEmpresa)
        {
            return _bl.Marcas_ObtenerTodos(CodEmpresa);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("Marcas_Insertar")]
        public ErrorDto Marcas_Insertar(int CodEmpresa, MarcasDto request)
        {
            return _bl.Marcas_Insertar(CodEmpresa, request);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("Marcas_Actualizar")]
        public ErrorDto Marcas_Actualizar(int CodEmpresa, MarcasDto request)
        {
            return _bl.Marcas_Actualizar(CodEmpresa, request);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("Marcas_Eliminar")]
        public ErrorDto Marcas_Eliminar(int CodEmpresa, string marca)
        {
            return _bl.Marcas_Eliminar(CodEmpresa, marca);
        }

    }
}