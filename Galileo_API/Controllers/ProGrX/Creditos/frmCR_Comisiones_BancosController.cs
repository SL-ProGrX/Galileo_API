using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FrmCrComisionesBancosController : ControllerBase
    {
        private readonly FrmCrComisionesBancosBl _bl;

        public FrmCrComisionesBancosController(IConfiguration config)
        {
            _bl = new FrmCrComisionesBancosBl(config);
        }

        [HttpPost("CR_frmCR_Comisiones_Bancos_Inicializar")]
        public ErrorDto<List<CrComisionesBancosItem>>
            CR_frmCR_Comisiones_Bancos_Inicializar(
                int codEmpresa,
                CrComisionesBancosInicializarRequest request)
        {
            return _bl.CR_frmCR_Comisiones_Bancos_Inicializar(
                codEmpresa,
                request);
        }

        [HttpGet("CR_frmCR_Comisiones_Bancos_Obtener")]
        public ErrorDto<List<CrComisionesBancosItem>>
            CR_frmCR_Comisiones_Bancos_Obtener(int codEmpresa)
        {
            return _bl.CR_frmCR_Comisiones_Bancos_Obtener(codEmpresa);
        }

        [HttpPost("CR_frmCR_Comisiones_Bancos_Actualizar")]
        public ErrorDto CR_frmCR_Comisiones_Bancos_Actualizar(
            int codEmpresa,
            CrComisionesBancosActualizarRequest request)
        {
            return _bl.CR_frmCR_Comisiones_Bancos_Actualizar(
                codEmpresa,
                request);
        }
    }
}