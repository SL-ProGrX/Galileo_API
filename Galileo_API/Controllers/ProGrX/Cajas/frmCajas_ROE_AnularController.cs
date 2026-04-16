using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic; 
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.Controllers.ProGrX.Cajas
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmCajasRoeAnularController : ControllerBase
    {
        private readonly FrmCajasRoeAnularBL _bl;

        public FrmCajasRoeAnularController(IConfiguration config)
        {
            _bl = new FrmCajasRoeAnularBL(config);
        }

        [Authorize]
        [HttpGet("CajasRoeAnular_Obtener")]       

        public ErrorDto<CajasRoeAnularLista> CajasRoeAnular_Obtener(int CodEmpresa, [FromQuery] FiltrosCajasRoeAnularData filtros)
        {
            return _bl.CajasRoeAnular_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("CajasRoeAnular_Anular")]
        public ErrorDto CajasRoeAnular_Anular(int CodEmpresa, string usuario, string roe, string notas)
        {
            return _bl.CajasRoeAnular_Anular(CodEmpresa, usuario, roe, notas);
        }
    }
}