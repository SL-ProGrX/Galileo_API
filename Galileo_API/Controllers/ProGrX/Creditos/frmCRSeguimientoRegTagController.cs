using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCRSeguimientoRegTagController : ControllerBase
    {
        private readonly FrmCRSeguimientoRegTagBL _bl;

        public FrmCRSeguimientoRegTagController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmCRSeguimientoRegTagBL(config);
        }

        [HttpGet("CR_SeguimientoRegTag_Etiquetas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_SeguimientoRegTag_Etiquetas_Obtener(
            int codEmpresa, string usuario)
        {
            return _bl.CR_SeguimientoRegTag_Etiquetas_Obtener(codEmpresa, usuario);
        }

        [HttpGet("CR_SeguimientoRegTag_Operaciones_Obtener")]
        public ErrorDto<List<CrSeguimientoRegTagOperacionDto>> CR_SeguimientoRegTag_Operaciones_Obtener(
            int codEmpresa, [FromQuery] CrSeguimientoRegTagConsultaRequest request)
        {
            return _bl.CR_SeguimientoRegTag_Operaciones_Obtener(codEmpresa, request);
        }

        [HttpPost("CR_SeguimientoRegTag_Aplicar")]
        public ErrorDto CR_SeguimientoRegTag_Aplicar(
            int codEmpresa, [FromBody] CrSeguimientoRegTagAplicarRequest request)
        {
            return _bl.CR_SeguimientoRegTag_Aplicar(codEmpresa, request);
        }
    }
}
