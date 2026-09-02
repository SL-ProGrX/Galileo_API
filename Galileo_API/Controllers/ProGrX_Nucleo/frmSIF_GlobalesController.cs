using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo_API.BusinessLogic.ProGrX_Nucleo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Nucleo
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmSifGlobalesController(IConfiguration config) : ControllerBase
    {
        private readonly FrmSifGlobalesBL _bl = new(config);

        [HttpGet("Sif_Globales_Obtener")]
        public ErrorDto<List<SifVariableGlobalDto>> Obtener() => _bl.Obtener();

        [HttpPut("Sif_Globales_Guardar")]
        public ErrorDto Guardar(int CodEmpresa, string usuario, SifVariableGlobalDto dato) =>
            _bl.Guardar(CodEmpresa, usuario, dato);
    }
}
