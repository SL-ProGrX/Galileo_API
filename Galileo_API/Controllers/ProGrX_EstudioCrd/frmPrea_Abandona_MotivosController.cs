using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_EstudioCrd
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmPreaAbandonaMotivosController : ControllerBase
    {
        private readonly FrmPreaAbandonaMotivosBL _bl;

        public FrmPreaAbandonaMotivosController(IConfiguration config)
        {
            _bl = new FrmPreaAbandonaMotivosBL(config);
        }

        [HttpGet("Prea_frmPreaAbandonaMotivos_Lista_Obtener")]
        public ErrorDto<FrmPreaAbandonaMotivosListaResponse> Prea_frmPreaAbandonaMotivos_Lista_Obtener(
            int codEmpresa,
            string usuario,
             string cod_preanalisis)
        {
            return _bl.Prea_frmPreaAbandonaMotivos_Lista_Obtener(codEmpresa, usuario, cod_preanalisis);
        }

        [HttpPost("Prea_frmPreaAbandonaMotivos_Registrar")]
        public ErrorDto<FrmPreaAbandonaMotivosRegistrarResponse> Prea_frmPreaAbandonaMotivos_Registrar(
            int codEmpresa,
            [FromBody] FrmPreaAbandonaMotivosRegistrarRequest request)
        {
            return _bl.Prea_frmPreaAbandonaMotivos_Registrar(codEmpresa, request);
        }
    }
}
