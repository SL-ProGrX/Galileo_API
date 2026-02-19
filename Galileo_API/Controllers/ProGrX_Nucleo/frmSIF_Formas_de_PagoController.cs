using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Nucleo;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.Controllers.ProGrX_Nucleo
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class FrmSifFormasDePagoController : ControllerBase
    {
        private readonly FrmSifFormasDePagoBL _bl;

        public FrmSifFormasDePagoController(IConfiguration config)
        {
            _bl = new FrmSifFormasDePagoBL(config);
        }

        [HttpGet("SIF_Formas_Pago_Obtener")]
        public ActionResult<ErrorDto<SifFormasPago>> SIF_Formas_Pago_Obtener(int CodEmpresa, string codFormaPago)
        {
            return _bl.SIF_Formas_Pago_Obtener(CodEmpresa, codFormaPago);
        }

        [HttpGet("SIF_Formas_Pago_Obtener_SigAnt")]
        public ActionResult<ErrorDto<string>> SIF_Formas_Pago_Obtener_SigAnt(int CodEmpresa, string? codFormaPagoActual, string orden)
        {
            return _bl.SIF_Formas_Pago_Obtener_SigAnt(CodEmpresa, codFormaPagoActual, orden);            
        }

        [HttpPost("SIF_Formas_Pago_Guardar")]
        public ActionResult<ErrorDto> SIF_Formas_Pago_Guardar(int CodEmpresa, [FromBody] SifFormasPago formaPago)
        {
            return _bl.SIF_Formas_Pago_Guardar(CodEmpresa, formaPago);
        }

        [HttpGet("SIF_Formas_Pago_Obtener_Lista")]
        public ActionResult<ErrorDto<List<SifFormasPagoList>>> SIF_Formas_Pago_Obtener_Lista(int CodEmpresa, string? filtro)
        {
            return _bl.SIF_Formas_Pago_Obtener_Lista(CodEmpresa, filtro);            
        }

        [HttpGet("CuentasBancarias_Obtener_Lista")]
        public List<SysCuentasBancariasList> CuentasBancarias_Obtener_Lista(int codEmpresa, string codFormaPago)
        {
            return _bl.CuentasBancarias_Obtener_Lista(codEmpresa, codFormaPago);
        }

        [HttpPost("CuentasBancarias_Asignar")]
        public ErrorDto CuentasBancarias_Asignar(int codEmpresa, [FromBody] SifFormasPagoBancoAsgDto data)
        {
            return _bl.CuentasBancarias_Asignar(codEmpresa, data);
        }
    }
}