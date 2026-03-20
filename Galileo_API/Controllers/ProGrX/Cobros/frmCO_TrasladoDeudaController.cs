using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo_API.BusinessTier.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCOTrasladoDeudaController : ControllerBase
    {
        private readonly FrmCOTrasladoDeudaBL BL;

        public FrmCOTrasladoDeudaController(IConfiguration config)
        {
            BL = new FrmCOTrasladoDeudaBL(config);
        }

        [Authorize]
        [HttpGet("CO_TrasladoDeuda_Obtener")]
        public ErrorDto<CoTrasladoDeudaObtenerDto> CO_TrasladoDeuda_Obtener(int CodEmpresa, long id_solicitud)
        {
            return BL.CO_TrasladoDeuda_Obtener(CodEmpresa, id_solicitud);
        }
        [Authorize]
        [HttpPost("CO_TrasladoDeuda_Calcular")]
        public ErrorDto<CoTrasladoDeudaCalcularResponse> CO_TrasladoDeuda_Calcular(int CodEmpresa, CoTrasladoDeudaCalcularRequest data)
        {
            return BL.CO_TrasladoDeuda_Calcular(CodEmpresa, data);
        }
        [Authorize]
        [HttpPost("CO_TrasladoDeuda_Aplicar")]
        public ErrorDto<CoTrasladoDeudaAplicarResponse> CO_TrasladoDeuda_Aplicar(int CodEmpresa, CoTrasladoDeudaAplicarRequest data)
        {
            return BL.CO_TrasladoDeuda_Aplicar(CodEmpresa, data);
        }
        [Authorize]
        [HttpPost("CO_TrasladoDeuda_Export")]
        public ErrorDto<CoTrasladoDeudaExportResponse> CO_TrasladoDeuda_Export(int CodEmpresa, CoTrasladoDeudaExportRequest data)
        {
            return BL.CO_TrasladoDeuda_Export(CodEmpresa, data);
        }
    }
}