using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Galileo_API.Models.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesMonitorSinpeController : ControllerBase
    {
        private readonly FrmTesMonitorSinpeBL _bl;
        public FrmTesMonitorSinpeController(IConfiguration config)
        {
            _bl = new FrmTesMonitorSinpeBL(config);
        }

        [HttpGet("fxFnd_SobresConsultaTotal")]
        public ErrorDto<decimal> fxFnd_SobresConsultaTotal(int CodEmpresa, string? cedula, string? plan)
        {
            return _bl.fxFnd_SobresConsultaTotal(CodEmpresa, cedula, plan);
        }

        [HttpGet("Tes_MonitorSinpeContrato_Consultar")]
        public ErrorDto<decimal> Tes_MonitorSinpeContrato_Consultar(int CodEmpresa)
        {
            return _bl.Tes_MonitorSinpeContrato_Consultar(CodEmpresa);
        }

        [HttpGet("Tes_MonitorSinpeDebCred_Consultar")]
        public ErrorDto<List<TesMonitorSinpeDebCrdModels>> Tes_MonitorSinpeDebCred_Consultar(int CodEmpresa, DateTime fechaInicio, DateTime fechaFin)
        {
            return _bl.Tes_MonitorSinpeDebCred_Consultar(CodEmpresa, fechaInicio, fechaFin);
        }

        [HttpGet("Tes_MonitorSinpeTransitos_Consultar")]
        public ErrorDto<List<TesMonitorSinpeDebCrdModels>> Tes_MonitorSinpeTransitos_Consultar(int CodEmpresa, DateTime fechaInicio, DateTime fechaFin)
        {
            return _bl.Tes_MonitorSinpeTransitos_Consultar(CodEmpresa, fechaInicio, fechaFin);
        }
    }
}
