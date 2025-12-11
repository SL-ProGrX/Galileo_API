using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Activos_Fijos;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;


namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmActivosCierrePeriodoController : ControllerBase
    {
        private readonly FrmActivosCierrePeriodoBL _bl;
        public FrmActivosCierrePeriodoController(IConfiguration config)
        {
            _bl = new FrmActivosCierrePeriodoBL(config);
        }

        [Authorize]
        [HttpGet("Activos_PeriodoEstado_Obtener")]
        public ErrorDto<string?> Activos_PeriodoEstado_Obtener(int CodEmpresa, DateTime periodo)
        {
            return _bl.Activos_PeriodoEstado_Obtener(CodEmpresa, periodo);
        }

        [Authorize]
        [HttpPost("Activos_Periodo_Cerrar")]
        public ErrorDto Activos_Periodo_Cerrar(int CodEmpresa, string usuario, DateTime periodo)
        {
            return _bl.Activos_Periodo_Cerrar(CodEmpresa, usuario, periodo);
        }

        [Authorize]
        [HttpGet("Activos_Periodo_Consultar")]
        public ErrorDto<DateTime> Activos_Periodo_Consultar(int CodEmpresa, int contabilidad)
        {
            return _bl.Activos_Periodo_Consultar(CodEmpresa, contabilidad);
        }

    }
}
