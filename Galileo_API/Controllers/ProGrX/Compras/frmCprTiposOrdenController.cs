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

        [HttpPost("Cpr_TiposOrden_Guardar")]
        public ErrorDto Cpr_TiposOrden_Guardar(
            int CodEmpresa,
            string usuario,
            TiposOrdenDto tipoOrden)
        {
            return _bl.Cpr_TiposOrden_Guardar(CodEmpresa, usuario, tipoOrden);
        }

        [HttpDelete("Cpr_TiposOrden_Eliminar")]
        public ErrorDto Cpr_TiposOrden_Eliminar(
            int CodEmpresa,
            string usuario,
            string tipoOrden)
        {
            return _bl.Cpr_TiposOrden_Eliminar(CodEmpresa, usuario, tipoOrden);
        }

        [HttpGet("rangosMontos_Obtener")]
        public ErrorDto<List<RangosMontos>> rangosMontos_Obtener(int CodEmpresa, string usuario)
        {
            return _bl.rangosMontos_Obtener(CodEmpresa, usuario);
        }
    }
}
