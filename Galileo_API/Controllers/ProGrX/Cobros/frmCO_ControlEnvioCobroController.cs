using Galileo.BusinessLogicTier.ProGrX.Cobros;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class FrmCOControlEnvioCobroController : ControllerBase
    {
        private readonly FrmCOControlEnvioCobroBL _bl;

        public FrmCOControlEnvioCobroController(IConfiguration config)
        {
            _bl = new FrmCOControlEnvioCobroBL(config);
        }
        
        [HttpGet("Co_ControlEnvioCobro_Gestiones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
            Co_ControlEnvioCobro_Gestiones_Obtener(int codEmpresa)
        {
            return _bl.Co_ControlEnvioCobro_Gestiones_Obtener(codEmpresa);
        }

        [HttpGet("Co_ControlEnvioCobro_Pendientes_Obtener")]
        public ErrorDto<List<CoControlEnvioCobroPendienteData>>
            Co_ControlEnvioCobro_Pendientes_Obtener(
                int codEmpresa,
                bool todos,
                string? codGestion = null)
        {
            return _bl.Co_ControlEnvioCobro_Pendientes_Obtener(
                codEmpresa,
                todos,
                codGestion);
        }
    }
}