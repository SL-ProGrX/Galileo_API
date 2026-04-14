using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCOAplicaAbonosBalloonsController : ControllerBase
    {
        private readonly FrmCOAplicaAbonosBalloonsBL _bl;
        public FrmCOAplicaAbonosBalloonsController(IConfiguration config)
        {
            _bl = new FrmCOAplicaAbonosBalloonsBL(config);
        }
        [Authorize]
        [HttpGet("CO_Aplica_Abonos_Balloons_Lista_Obtener")]
        public ErrorDto<CoAplicaAbonosBalloonsListaResult> CO_Aplica_Abonos_Balloons_Lista_Obtener(int CodEmpresa,string parametros)
        {
            return _bl.CO_Aplica_Abonos_Balloons_Lista_Obtener(CodEmpresa, parametros);
        }
        [Authorize]
        [HttpGet("CO_Aplica_Abonos_Balloons_Lista_Export")]
        public ErrorDto<CoAplicaAbonosBalloonsListaResult> CO_Aplica_Abonos_Balloons_Lista_Export(int CodEmpresa,string parametros)
        {
            return _bl.CO_Aplica_Abonos_Balloons_Lista_Export(CodEmpresa, parametros);
        }
        [Authorize]
        [HttpPost("CO_Aplica_Abonos_Balloons_Aplicar")]
        public ErrorDto<CoAplicaAbonosBalloonsAplicarResult> CO_Aplica_Abonos_Balloons_Aplicar(int CodEmpresa,[FromBody] CoAplicaAbonosBalloonsAplicarRequest? req)
        {
            return _bl.CO_Aplica_Abonos_Balloons_Aplicar(CodEmpresa, req);
        }
    }
}