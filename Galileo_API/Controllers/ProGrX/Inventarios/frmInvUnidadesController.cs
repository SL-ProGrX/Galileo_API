using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmInvUnidadesController : ControllerBase
    {
        private readonly FrmInvUnidadesBL _bl;

        public FrmInvUnidadesController(IConfiguration config)
        {
            _bl = new FrmInvUnidadesBL(config);
        }

        [HttpGet("UnidadMedicion_Obtener")]
        public ErrorDto<UnidadesDataLista> UnidadMedicion_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            return _bl.UnidadMedicion_Obtener(CodCliente, pagina, paginacion, filtro);
        }

        [HttpGet("UnidadMedicion_ObtenerTodosDetalle")]
        public ErrorDto<List<UnidadMedicionDto>> UnidadMedicion_ObtenerTodosDetalle(int CodEmpresa)
        {
            return _bl.UnidadMedicion_ObtenerTodosDetalle(CodEmpresa);
        }

        [HttpGet("UnidadMedicion_ObtenerTodos")]
        public ErrorDto<List<UnidadMedicion>> UnidadMedicion_ObtenerTodos(int CodEmpresa)
        {
            return _bl.UnidadMedicion_ObtenerTodos(CodEmpresa);
        }

        [HttpPost("UnidadMedicion_Insertar")]
        public ErrorDto UnidadMedicion_Insertar(int CodEmpresa, UnidadMedicionDto request)
        {
            return _bl.UnidadMedicion_Insertar(CodEmpresa, request);
        }

        [HttpPost("UnidadMedicion_Actualizar")]
        public ErrorDto UnidadMedicion_Actualizar(int CodEmpresa, UnidadMedicionDto request)
        {
            return _bl.UnidadMedicion_Actualizar(CodEmpresa, request);
        }

        [HttpPost("UnidadMedicion_Eliminar")]
        public ErrorDto UnidadMedicion_Eliminar(int CodEmpresa, string unidad)
        {
            return _bl.UnidadMedicion_Eliminar(CodEmpresa, unidad);
        }
    }
}