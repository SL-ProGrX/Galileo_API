using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmCprTiposOrdenController : ControllerBase
    {
        private readonly FrmCprTiposOrdenBL _bl;

        public FrmCprTiposOrdenController(IConfiguration config)
        {
            _bl = new FrmCprTiposOrdenBL(config);
        }

        [HttpGet("TiposOrden_Obtener")]
        public ErrorDto<TiposOrdenLista> TiposOrden_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.ObtenerTiposOrdenes(CodEmpresa, filtros);
        }

        [HttpPut("TipoOrden_Actualizar")]
        public ErrorDto TipoOrden_Actualizar(int CodEmpresa, TiposOrdenDto tiposOrden)
        {
            return _bl.TipoOrden_Actualizar(CodEmpresa, tiposOrden);
        }

        [HttpDelete("TipoOrden_Eliminar")]
        public ErrorDto TipoOrden_Eliminar(int CodEmpresa, string tiposOrden)
        {
            return _bl.TipoOrden_Eliminar(CodEmpresa, tiposOrden);
        }
        [HttpPost("TipoOrden_Insertar")]
        public ErrorDto TipoOrden_Insertar(int CodEmpresa, TiposOrdenDto tiposOrden)
        {
            return _bl.TipoOrden_Insertar(CodEmpresa, tiposOrden);
        }

        [HttpGet("rangosMontos_Obtener")]
        public ErrorDto<List<RangosMontos>> rangosMontos_Obtener(int CodEmpresa, string usuario)
        {
            return _bl.rangosMontos_Obtener(CodEmpresa, usuario);
        }
    }
}
