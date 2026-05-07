using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_EstudioCrd;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX_EstudioCrd.FrmPreaParametrosModels;

namespace Galileo_API.Controllers.ProGrX_EstudioCrd
{

    [Route("api/[controller]")]
    [ApiController]

    public class FrmPreaParametrosController : ControllerBase
    {
        private readonly FrmPreaParametrosBL _bl;

        public FrmPreaParametrosController(IConfiguration config)
        {
            _bl = new FrmPreaParametrosBL(config);
        }

        [Authorize]
        [HttpPost("PreaParametros_Inicializar")]
        public ErrorDto<List<PreaParametroModel>> PreaParametros_Inicializar(int codEmpresa)
               => _bl.PreaParametros_Inicializar(codEmpresa);

        [Authorize]
        [HttpGet("PreaParametros_Grid_Obtener")]
        public ErrorDto<List<PreaParametroModel>> PreaParametros_Grid_Obtener(int codEmpresa)
                 => _bl.PreaParametros_Grid_Obtener(codEmpresa);

        [Authorize]
        [HttpPost("PreaParametros_Parametro_Actualizar")]
        public ErrorDto PreaParametros_Parametro_Actualizar(int codEmpresa, [FromBody] PreaParametroActualizarRequest request)
            => _bl.PreaParametros_Parametro_Actualizar(codEmpresa, request);

        [Authorize]
        [HttpGet("PreaParametros_Historico_Obtener")]
        public ErrorDto<List<PreaParametroHistoricoModel>> PreaParametros_Historico_Obtener(int codEmpresa, string codParametro)
                 => _bl.PreaParametros_Historico_Obtener(codEmpresa, codParametro);

    }
}
