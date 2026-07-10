using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.ERROR;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrConsultaFianzasController : ControllerBase
    {
        private readonly FrmCrConsultaFianzasBl _bl;

        public FrmCrConsultaFianzasController(IConfiguration config)
        {
            _bl = new FrmCrConsultaFianzasBl(config);
        }

        [HttpPost("CrConsultaFianzas_Consulta_Obtener")]
        public ErrorDto<CrConsultaFianzasConsultaData> CrConsultaFianzas_Consulta_Obtener(
            int codEmpresa,
            [FromBody] CrConsultaFianzasConsultaRequest request)
            => _bl.CrConsultaFianzas_Consulta_Obtener(codEmpresa, request);

        [HttpPost("CrConsultaFianzas_Detalle_Obtener")]
        public ErrorDto<CrConsultaFianzasDetalleData> CrConsultaFianzas_Detalle_Obtener(
            int codEmpresa,
            [FromBody] CrConsultaFianzasDetalleRequest request)
            => _bl.CrConsultaFianzas_Detalle_Obtener(codEmpresa, request);
    }
}
