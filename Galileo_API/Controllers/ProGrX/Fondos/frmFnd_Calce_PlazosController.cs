using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmFndCalcePlazosController : ControllerBase
    {
        private readonly FrmFndCalcePlazosBL _BL;

        public FrmFndCalcePlazosController(IConfiguration config)
        {
            _BL = new FrmFndCalcePlazosBL(config);
        }

        [HttpGet("Periodos_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Periodos_Lista(int CodEmpresa)
        {
            return _BL.Periodos_Lista(CodEmpresa);
        }

        [HttpPost("Proyeccion_Presupuesto")]
        public ErrorDto Proyeccion_Presupuesto(int CodEmpresa, int Anio, string Usuario, int Tipo)
        {
            return _BL.Proyeccion_Presupuesto(CodEmpresa, Anio, Usuario, Tipo);
        }

        [HttpGet("Proyeccion_Presupuesto_Export")]
        public ErrorDto<List<Dictionary<string, object?>>> Proyeccion_Presupuesto_Export(int CodEmpresa, int Anio)
        {
            return _BL.Proyeccion_Presupuesto_Export(CodEmpresa, Anio);
        }
    }
}
