using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers.ProGrX.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfCartasNoCotizantesController : ControllerBase
    {
        private readonly FrmAfCartasNoCotizantesBL _bl;
        public FrmAfCartasNoCotizantesController(IConfiguration config)
        {
            _bl = new FrmAfCartasNoCotizantesBL(config);
        }

        [Authorize]
        [HttpGet("Af_CartasNoCotizantes_Obtener")]
        public ErrorDto<decimal> Af_CartasNoCotizantes_Obtener(int CodEmpresa, int contabilidad)
        {
            return _bl.Af_CartasNoCotizantes_Obtener(CodEmpresa, contabilidad);
        }

        [Authorize]
        [HttpGet("Af_CartasNoCotizantesDatos_Obtener")]
        public ErrorDto<List<AfCartasNoCotizantesData>> Af_CartasNoCotizantesDatos_Obtener(int CodEmpresa, string jFiltros)
        {
            return _bl.Af_CartasNoCotizantesDatos_Obtener(CodEmpresa, jFiltros);
        }

    }
}