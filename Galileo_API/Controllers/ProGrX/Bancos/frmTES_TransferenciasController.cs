using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PgxAPI.BusinessLogic.ProGrX.Bancos;

namespace PgxAPI.Controllers.ProGrX.Bancos
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
        public ErrorDto TES_Transferencia_Aceptar(int CodEmpresa,TesTransferenciasInfo transferencia)
        {
            return _transferenciasBL.TES_Transferencia_Aceptar(CodEmpresa, transferencia);
        }

        [HttpPost("TES_Transferencia_Reversar")]
        public ErrorDto TES_Transferencia_Reversar(int CodEmpresa,TesTransferenciasInfo transferencia)
        {
            return _transferenciasBL.TES_Transferencia_Reversar(CodEmpresa, transferencia);
        }
    }
}
