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
    public class FrmPreaSeguimientoCausasController : ControllerBase
    {
        private readonly FrmPreaSeguimientoCausasBL _bl;

        public FrmPreaSeguimientoCausasController(IConfiguration config)
        {
            _bl = new FrmPreaSeguimientoCausasBL(config);
        }

        [HttpGet("Prea_frmPreaSeguimientoCausas_Lista_Obtener")]
        public ErrorDto<FrmPreaSeguimientoCausasListaResponse> Prea_frmPreaSeguimientoCausas_Lista_Obtener(
            int codEmpresa,
            string usuario,
            string cod_preanalisis,
            string tipo,
            string? codigo)
        {
            return _bl.Prea_frmPreaSeguimientoCausas_Lista_Obtener(
                codEmpresa,
                usuario,
                cod_preanalisis,
                tipo,
                codigo ?? string.Empty);
        }

        [HttpPost("Prea_frmPreaSeguimientoCausas_Registrar")]
        public ErrorDto<FrmPreaSeguimientoCausasRegistrarResponse> Prea_frmPreaSeguimientoCausas_Registrar(
            int codEmpresa,
            [FromBody] FrmPreaSeguimientoCausasRegistrarRequest request)
        {
            return _bl.Prea_frmPreaSeguimientoCausas_Registrar(codEmpresa, request);
        }
    }
}
