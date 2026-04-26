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
    public class FrmPreaTiposSalariosController : ControllerBase
    {
        private readonly FrmPreaTiposSalariosBl _bl;

        public FrmPreaTiposSalariosController(IConfiguration config) =>
            _bl = new FrmPreaTiposSalariosBl(config);

        [HttpGet("CrPreaTiposSalarios_Obtener")]
        public ErrorDto<List<CrdPreaTiposSalariosData>> CrPreaTiposSalarios_Obtener(int codEmpresa)
        {
            return _bl.CrPreaTiposSalarios_Obtener(codEmpresa);
        }

        [HttpPost("CrPreaTiposSalarios_Guardar")]
        public ErrorDto CrPreaTiposSalarios_Guardar(int codEmpresa, string usuario, CrdPreaTiposSalariosData request)
        {
            return _bl.CrPreaTiposSalarios_Guardar(codEmpresa, usuario, request);
        }

        [HttpDelete("CrPreaTiposSalarios_Eliminar")]
        public ErrorDto CrPreaTiposSalarios_Eliminar(int codEmpresa, string tipoSalario, string usuario)
        {
            return _bl.CrPreaTiposSalarios_Eliminar(codEmpresa, tipoSalario, usuario);
        }
    }
}