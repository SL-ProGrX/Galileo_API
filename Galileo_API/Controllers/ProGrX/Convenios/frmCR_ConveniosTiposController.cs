using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Convenios;
using Galileo_API.Models.ProGrX.Convenios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Convenios
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrConveniosTiposController : ControllerBase
    {
        private readonly FrmCrConveniosTiposBl _bl;

        public FrmCrConveniosTiposController(IConfiguration config)
        {
            _bl = new FrmCrConveniosTiposBl(config);
        }

        [HttpGet("CrConveniosTipos_Obtener")]
        public ErrorDto<List<CrConvenioTipoData>> CrConveniosTipos_Obtener(int codEmpresa)
            => _bl.CrConveniosTipos_Obtener(codEmpresa);

        [ValidateAntiForgeryToken]
        [HttpPost("CrConveniosTipos_Guardar")]
        public ErrorDto CrConveniosTipos_Guardar(int codEmpresa, CrConvenioTipoGuardarRequest request)
            => _bl.CrConveniosTipos_Guardar(codEmpresa, request);

        [ValidateAntiForgeryToken]
        [HttpDelete("CrConveniosTipos_Eliminar")]
        public ErrorDto CrConveniosTipos_Eliminar(int codEmpresa, CrConvenioTipoEliminarRequest request)
            => _bl.CrConveniosTipos_Eliminar(codEmpresa, request);
    }
}
