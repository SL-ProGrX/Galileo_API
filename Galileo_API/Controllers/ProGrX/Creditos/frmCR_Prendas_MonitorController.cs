using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrPrendasMonitorController : ControllerBase
    {
        private readonly FrmCrPrendasMonitorBl _bl;

        public FrmCrPrendasMonitorController(IConfiguration config)
        {
            _bl = new FrmCrPrendasMonitorBl(config);
        }

        [Authorize]
        [HttpGet("CrPrendasMonitor_TiposPrenda_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrPrendasMonitor_TiposPrenda_Obtener(int codEmpresa)
            => _bl.CrPrendasMonitor_TiposPrenda_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("CrPrendasMonitor_Catalogo_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrPrendasMonitor_Catalogo_Obtener(int codEmpresa, string tipo)
            => _bl.CrPrendasMonitor_Catalogo_Obtener(codEmpresa, tipo);

        [Authorize]
        [HttpGet("CrPrendasMonitor_EstadosPersona_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrPrendasMonitor_EstadosPersona_Obtener(int codEmpresa)
            => _bl.CrPrendasMonitor_EstadosPersona_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("CrPrendasMonitor_UnidadesCilindraje_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrPrendasMonitor_UnidadesCilindraje_Obtener(int codEmpresa, string tipo)
            => _bl.CrPrendasMonitor_UnidadesCilindraje_Obtener(codEmpresa, tipo);

        [Authorize]
        [HttpPost("CrPrendasMonitor_Consulta_Obtener")]
        public ErrorDto<List<CrPrendasMonitorConsultaData>> CrPrendasMonitor_Consulta_Obtener(
            int codEmpresa,
            [FromBody] CrPrendasMonitorConsultaRequest request)
            => _bl.CrPrendasMonitor_Consulta_Obtener(codEmpresa, request);
    }
}
