using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers.ProGrX.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndMonitorVencimientoController : ControllerBase
    {
        private readonly FrmFndMonitorVencimientoBl _bl;

        public FrmFndMonitorVencimientoController(IConfiguration config)
        {
            _bl = new FrmFndMonitorVencimientoBl(config);
        }

        [Authorize]
        [HttpGet("Fnd_Planes_TipoPlan_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Planes_TipoPlan_Obtener(int CodEmpresa)
        {
            return _bl.Fnd_Planes_TipoPlan_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("Fnd_Planes_Obtener")]
        public ErrorDto<List<FndPlanesItem>> Fnd_Planes_Obtener(int CodEmpresa, [FromBody] FndPlanesObtenerRequest request)
        {
            return _bl.Fnd_Planes_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("Fnd_Vencimientos_Consulta")]
        public ErrorDto<List<FndVencimientosConsultaResult>> Fnd_Vencimientos_Consulta(
            int CodEmpresa, [FromBody] FndVencimientosConsultaRequest request)
        {
            return _bl.Fnd_Vencimientos_Consulta(CodEmpresa, request);
        }
    }
}