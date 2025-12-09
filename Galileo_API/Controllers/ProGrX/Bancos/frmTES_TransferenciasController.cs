using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmTesTransferenciasController : ControllerBase
    {
        private readonly IConfiguration? _config;
        private readonly FrmTesTransferenciasBl _bl;

        public FrmTesTransferenciasController(IConfiguration config)
        {
            _config = config;
            _bl = new FrmTesTransferenciasBl(_config);
        }

        [Authorize]
        [HttpPost("TES_Transferencia_Aceptar")]
        public ErrorDto TES_Transferencia_Aceptar(int CodEmpresa, TesTransferenciasInfo transferencia)
        {
            return _bl.TES_Transferencia_Aceptar(CodEmpresa, transferencia);
        }

        [Authorize]
        [HttpPost("TES_Transferencia_Reversar")]
        public ErrorDto TES_Transferencia_Reversar(int CodEmpresa, TesTransferenciasInfo transferencia)
        {
            return _bl.TES_Transferencia_Reversar(CodEmpresa, transferencia);
        }
    }
}
