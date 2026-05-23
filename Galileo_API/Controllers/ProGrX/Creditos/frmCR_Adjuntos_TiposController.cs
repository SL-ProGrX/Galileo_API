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
    public class FrmCrAdjuntosTiposController : ControllerBase
    {
        private readonly FrmCrAdjuntosTiposBl _bl;

        public FrmCrAdjuntosTiposController(IConfiguration config)
        {
            _bl = new FrmCrAdjuntosTiposBl(config);
        }

        [HttpGet("CrAdjuntosTipos_Obtener")]
        public ErrorDto<List<CrAdjuntoTipoData>> CrAdjuntosTipos_Obtener(int codEmpresa)
            => _bl.CrAdjuntosTipos_Obtener(codEmpresa);

        [HttpPost("CrAdjuntosTipos_Guardar")]
        public ErrorDto CrAdjuntosTipos_Guardar(int codEmpresa, CrAdjuntoTipoGuardarRequest request)
            => _bl.CrAdjuntosTipos_Guardar(codEmpresa, request);

        [HttpDelete("CrAdjuntosTipos_Eliminar")]
        public ErrorDto CrAdjuntosTipos_Eliminar(int codEmpresa, CrAdjuntoTipoEliminarRequest request)
            => _bl.CrAdjuntosTipos_Eliminar(codEmpresa, request);
    }
}