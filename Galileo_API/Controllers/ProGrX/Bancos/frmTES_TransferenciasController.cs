using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo_API.BusinessLogic.ProGrX.Bancos;

namespace Galileo_API.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesTransferenciasController : ControllerBase
    {
        private readonly FrmTesTransferenciasBL _transferenciasBL;

        public FrmTesTransferenciasController(IConfiguration config)
        {
            _transferenciasBL = new FrmTesTransferenciasBL(config);
        }

        
        [HttpPost("TES_Transferencia_Aceptar")]
        public ErrorDto TES_Transferencia_Aceptar([FromQuery] int CodEmpresa,
            [FromBody] TesTransferenciasInfo transferencia)
        {
            return _transferenciasBL.TES_Transferencia_Aceptar(CodEmpresa, transferencia);
        }

        [HttpPost("TES_Transferencia_Reversar")]
        public ErrorDto TES_Transferencia_Reversar([FromQuery] int CodEmpresa,
            [FromBody] TesTransferenciasInfo transferencia)
        {
            return _transferenciasBL.TES_Transferencia_Reversar(CodEmpresa, transferencia);
        }
    }
}
