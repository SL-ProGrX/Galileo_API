using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrRevoTiposCausasController : ControllerBase
    {
        private readonly FrmCrRevoTiposCausasBl _bl;

        public FrmCrRevoTiposCausasController(IConfiguration config)
        {
            _bl = new FrmCrRevoTiposCausasBl(config);
        }

        [HttpGet("CR_Revo_TiposCausas_Obtener")]
        public ErrorDto<List<CrRevoTiposCausasData>> CR_Revo_TiposCausas_Obtener(int codEmpresa)
        {
            return _bl.CR_Revo_TiposCausas_Obtener(codEmpresa);
        }

        [HttpPost("CR_Revo_TiposCausas_Guardar")]
        public ErrorDto CR_Revo_TiposCausas_Guardar(
            int codEmpresa,
            string usuario,
            CrRevoTiposCausasData request)
        {
            return _bl.CR_Revo_TiposCausas_Guardar(codEmpresa, usuario, request);
        }

        [HttpDelete("CR_Revo_TiposCausas_Eliminar")]
        public ErrorDto CR_Revo_TiposCausas_Eliminar(
            int codEmpresa,
            string usuario,
            string codCausa)
        {
            return _bl.CR_Revo_TiposCausas_Eliminar(codEmpresa, usuario, codCausa);
        }
    }
}
