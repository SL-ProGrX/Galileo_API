using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmFndRastreoVerificaSaldosController : ControllerBase
    {
        private readonly FrmFndRastreoVerificaSaldosBL _BL;

        public FrmFndRastreoVerificaSaldosController(IConfiguration? config)
        {
            _BL = new FrmFndRastreoVerificaSaldosBL(config);
        }

        [HttpGet("Planes_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Planes_Lista(int CodEmpresa)
        {
            return _BL.Planes_Lista(CodEmpresa);
        }

        [HttpGet("Periodos_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Periodos_Lista(int CodEmpresa)
        {
            return _BL.Periodos_Lista(CodEmpresa);
        }

        [HttpGet("VerificacionSaldos_Buscar")]
        public ErrorDto<List<FndVerificacionSaldoDto>> VerificacionSaldos_Buscar(int CodEmpresa,string Plan,string PeriodoId,int Lineas,bool SoloDiferencias)
        {
            return _BL.VerificacionSaldos_Buscar(CodEmpresa, Plan, PeriodoId, Lineas, SoloDiferencias);
        }
    }
}