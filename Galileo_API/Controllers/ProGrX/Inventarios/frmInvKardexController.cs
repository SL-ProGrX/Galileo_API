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
    public class FrmInvKardexController : ControllerBase
    {
        private readonly FrmInvKardexBL _bl;
        public FrmInvKardexController(IConfiguration config)
        {
            _bl = new FrmInvKardexBL(config);
        }

        [HttpGet("Obtener_Bodegas")]
        public ErrorDto<List<ConsultaMovimientoBodegaCDdto>> Obtener_Bodegas(int CodEmpresa)
        {
            return _bl.Obtener_Bodegas(CodEmpresa);
        }

        [HttpGet("consultarMovimientos_Obtener")]
        public ErrorDto<MovimientosDtoList> consultarMovimientos_Obtener(int CodCliente, string filtros)
        {
            return _bl.consultarMovimientos_Obtener(CodCliente, filtros);
        }
    }
}