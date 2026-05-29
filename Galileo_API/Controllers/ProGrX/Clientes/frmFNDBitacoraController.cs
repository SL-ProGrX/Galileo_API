using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.ERROR;

namespace Galileo.Controllers.ProGrX.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndBitacoraController : ControllerBase
    {
        private readonly FrmFndBitacoraBl _bl;

        public FrmFndBitacoraController(IConfiguration config)
        {
            _bl = new FrmFndBitacoraBl(config);
        }

        [Authorize]
        [HttpGet("Fnd_Movimientos_Obtener")]
        public ErrorDto<List<UsMovimiento>> Fnd_Movimientos_Obtener(int CodEmpresa)
        {
            return _bl.Fnd_Movimientos_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("Fnd_Bitacora_Cambios_Obtener")]
        public ErrorDto<List<FndBitacoraCambiosResult>> Fnd_Bitacora_Cambios_Obtener(
            int CodEmpresa, [FromBody] FndBitacoraCambiosRequest request)
        {
            return _bl.Fnd_Bitacora_Cambios_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("Fnd_Bitacora_Cambio_Revisar")]
        public ErrorDto<bool> Fnd_Bitacora_Cambio_Revisar(
            int CodEmpresa, [FromBody] FndBitacoraCambioRevisarRequest request)
        {
            return _bl.Fnd_Bitacora_Cambio_Revisar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("Sif_RegistraTags")]
        public ErrorDto<bool> Sif_RegistraTags(
            int CodEmpresa, [FromBody] SifRegistraTagsRequest request)
        {
            return _bl.Sif_RegistraTags(CodEmpresa, request);
        }

    }
}