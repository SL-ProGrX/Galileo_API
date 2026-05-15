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
    public class FrmPreaHipotecaMontoController : ControllerBase
    {
        private readonly FrmPreaHipotecaMontoBL _bl;

        public FrmPreaHipotecaMontoController(IConfiguration config)
        {
            _bl = new FrmPreaHipotecaMontoBL(config);
        }

        [HttpGet("Prea_frmPreaHipotecaMonto_Lista_Obtener")]
        public ErrorDto<FrmPreaHipotecaMontoListaResponse> Prea_frmPreaHipotecaMonto_Lista_Obtener(
            int codEmpresa,
            string cod_preanalisis,
            string tipo)
        {
            return _bl.Prea_frmPreaHipotecaMonto_Lista_Obtener(codEmpresa, cod_preanalisis, tipo);
        }

        [HttpPost("Prea_frmPreaHipotecaMonto_Seleccion_Guardar")]
        public ErrorDto<FrmPreaHipotecaMontoGuardarResponse> Prea_frmPreaHipotecaMonto_Seleccion_Guardar(
            int codEmpresa,
            [FromBody] FrmPreaHipotecaMontoGuardarRequest request)
        {
            return _bl.Prea_frmPreaHipotecaMonto_Seleccion_Guardar(codEmpresa, request);
        }
    }
}
