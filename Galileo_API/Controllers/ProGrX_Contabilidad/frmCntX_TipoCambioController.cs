using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class frmCntXTipoCambioController : ControllerBase
    {
        private readonly FrmCntXTipoCambioBl _bl;

        public frmCntXTipoCambioController(IConfiguration config)
            => _bl = new FrmCntXTipoCambioBl(config);

        [HttpPost("CntX_TipoCambio_Inicializa")]
        public ErrorDto<CntXTipoCambioInicializaData> CntX_TipoCambio_Inicializa(
            int codEmpresa,
            int codConta,
            CntXTipoCambioInicializaRequest request)
        {
            return _bl.CntX_TipoCambio_Inicializa(codEmpresa, codConta, request);
        }
    }
}
