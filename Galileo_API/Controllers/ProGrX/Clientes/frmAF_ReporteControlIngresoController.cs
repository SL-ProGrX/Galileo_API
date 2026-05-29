using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.Controllers.ProGrX.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfReporteControlIngresoController : ControllerBase
    {
       private readonly FrmAfReporteControlIngresoBL _bl;

       public FrmAfReporteControlIngresoController(IConfiguration config)
       {
           _bl = new FrmAfReporteControlIngresoBL(config);
       }

       [Authorize]
       [HttpGet("AF_ReporteControlIngresoEstado_Obtener")]
       public ErrorDto<List<DropDownListaGenericaModel>> AF_ReporteControlIngresoEstado_Obtener(int CodEmpresa)
       {
           return _bl.AF_ReporteControlIngresoEstado_Obtener(CodEmpresa);
       }

       [Authorize]
       [HttpGet("AF_ReporteControlIngresoInstitucion_Obtener")]
       public ErrorDto<List<DropDownListaGenericaModel>> AF_ReporteControlIngresoInstitucion_Obtener(int CodEmpresa)
       {
           return _bl.AF_ReporteControlIngresoInstitucion_Obtener(CodEmpresa);
       }
    }
}