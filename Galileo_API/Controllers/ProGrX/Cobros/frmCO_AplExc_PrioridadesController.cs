using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCOAplExcPrioridadesController : ControllerBase
    {
        private readonly FrmCOAplExcPrioridadesBL _bl;

        public FrmCOAplExcPrioridadesController(IConfiguration config)
        {
            _bl = new FrmCOAplExcPrioridadesBL(config);
        }

        [Authorize]
        [HttpGet("Co_AplExc_Prioridades_Lista_Obtener")]
        public ErrorDto<COAplExcPrioridadesListaResult> Co_AplExc_Prioridades_Lista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Co_AplExc_Prioridades_Lista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Co_AplExc_Prioridades_Lista_Export")]
        public ErrorDto<COAplExcPrioridadesListaResult> Co_AplExc_Prioridades_Lista_Export(int CodEmpresa, string filtros)
        {
            return _bl.Co_AplExc_Prioridades_Lista_Export(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("Co_AplExc_Prioridades_Guardar")]
        public ErrorDto Co_AplExc_Prioridades_Guardar(int CodEmpresa, string usuario, [FromBody] COAplExcPrioridadData prioridad)
        {
            return _bl.Co_AplExc_Prioridades_Guardar(CodEmpresa, usuario, prioridad);
        }

        [Authorize]
        [HttpDelete("Co_AplExc_Prioridades_Eliminar")]
        public ErrorDto Co_AplExc_Prioridades_Eliminar(int CodEmpresa, string usuario, string codigo)
        {
            return _bl.Co_AplExc_Prioridades_Eliminar(CodEmpresa, usuario, codigo);
        }

        [Authorize]
        [HttpGet("Co_AplExc_Prioridades_GarantiasDisponibles_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Co_AplExc_Prioridades_GarantiasDisponibles_Obtener(int CodEmpresa)
        {
            return _bl.Co_AplExc_Prioridades_GarantiasDisponibles_Obtener(CodEmpresa);
        }
    }
}
