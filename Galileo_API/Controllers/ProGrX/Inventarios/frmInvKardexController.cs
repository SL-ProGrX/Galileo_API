using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmInvKardexController :
        ControllerBase
    {
        private readonly FrmInvKardexBl _bl;

        public FrmInvKardexController(
            IConfiguration config)
        {
            _bl = new FrmInvKardexBl(config);
        }

        [HttpGet("INV_Kardex_Bodegas_Obtener")]
        public ErrorDto<List<InvKardexBodegaDto>>
            INV_Kardex_Bodegas_Obtener(
                int CodEmpresa)
        {
            return _bl
                .INV_Kardex_Bodegas_Obtener(
                    CodEmpresa);
        }

        [HttpGet("INV_Kardex_Movimientos_Obtener")]
        public ErrorDto<InvKardexMovimientosListaDto>
            INV_Kardex_Movimientos_Obtener(
                int CodEmpresa,
                string filtros)
        {
            return _bl
                .INV_Kardex_Movimientos_Obtener(
                    CodEmpresa,
                    filtros);
        }
    }
}