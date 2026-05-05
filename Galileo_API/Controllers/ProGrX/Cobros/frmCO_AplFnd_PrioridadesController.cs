using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoAplFndPrioridadesModels;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCoAplFndPrioridadesController : ControllerBase
    {
        private readonly FrmCoAplFndPrioridadesBl _bl;

        public FrmCoAplFndPrioridadesController(IConfiguration config)
        {
            _bl = new FrmCoAplFndPrioridadesBl(config);
        }

        [HttpGet("Co_AplFnd_Prioridades_Lista_Obtener")]
        public ErrorDto<COAplFndPrioridadesListaResult> Co_AplFnd_Prioridades_Lista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Co_AplFnd_Prioridades_Lista_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("Co_AplFnd_Prioridades_Lista_Export")]
        public ErrorDto<COAplFndPrioridadesListaResult> Co_AplFnd_Prioridades_Lista_Export(int CodEmpresa, string filtros)
        {
            return _bl.Co_AplFnd_Prioridades_Lista_Export(CodEmpresa, filtros);
        }

        [HttpPost("Co_AplFnd_Prioridades_Guardar")]
        public ErrorDto Co_AplFnd_Prioridades_Guardar(int CodEmpresa, string usuario, [FromBody] COAplFndPrioridadData prioridad)
        {
            return _bl.Co_AplFnd_Prioridades_Guardar(CodEmpresa, usuario, prioridad);
        }

        [HttpDelete("Co_AplFnd_Prioridades_Eliminar")]
        public ErrorDto Co_AplFnd_Prioridades_Eliminar(int CodEmpresa, string usuario, string codigo)
        {
            return _bl.Co_AplFnd_Prioridades_Eliminar(CodEmpresa, usuario, codigo);
        }

        [HttpGet("Co_AplFnd_Prioridades_GarantiasDisponibles_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Co_AplFnd_Prioridades_GarantiasDisponibles_Obtener(int CodEmpresa)
        {
            return _bl.Co_AplFnd_Prioridades_GarantiasDisponibles_Obtener(CodEmpresa);
        }

        [HttpGet("Co_AplFnd_PrioridadEjecucion_Obtener")]
        public ErrorDto<int> Co_AplFnd_PrioridadEjecucion_Obtener(int CodEmpresa)
        {
            return _bl.Co_AplFnd_PrioridadEjecucion_Obtener(CodEmpresa);
        }

        [HttpPost("Co_AplFnd_PrioridadEjecucion_Actualizar")]
        public ErrorDto Co_AplFnd_PrioridadEjecucion_Actualizar(int CodEmpresa, string usuario, [FromBody] int prioridad)
        {
            return _bl.Co_AplFnd_PrioridadEjecucion_Actualizar(CodEmpresa, usuario, prioridad);
        }
    }
}
