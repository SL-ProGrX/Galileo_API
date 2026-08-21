using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Conciliacion;
using Galileo_API.Models.ProGrX_Conciliacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Conciliacion
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class FrmRastreoMovOpController : ControllerBase
    {
        private readonly FrmRastreoMovOpBL _bl;

        public FrmRastreoMovOpController(IConfiguration config)
        {
            _bl = new FrmRastreoMovOpBL(config);
        }

        [HttpGet("RastreoMovOp_Periodos_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> RastreoMovOp_Periodos_Obtener(
            int codEmpresa)
        {
            return _bl.RastreoMovOp_Periodos_Obtener(codEmpresa);
        }

        [HttpGet("RastreoMovOp_Saldos_Obtener")]
        [Authorize]
        public ErrorDto<List<RastreoMovOpSaldosData>> RastreoMovOp_Saldos_Obtener(
            int codEmpresa,
            int idPerHistorico,
            int lineas = 1000,
            bool diferencias = false)
        {
            return _bl.RastreoMovOp_Saldos_Obtener(
                codEmpresa,
                new RastreoMovOpSaldosRequest
                {
                    Id_Per_Historico = idPerHistorico,
                    Lineas = lineas,
                    Diferencias = diferencias,
                });
        }
    }
}
