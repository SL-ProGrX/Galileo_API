using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.BusinessLogic.ProGrX_Nucleo;
using Galileo_API.Models.ProGrX_Nucleo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Nucleo
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSugefRomMonitorController : ControllerBase
    {
        private readonly FrmSugefRomMonitorBL _bl;

        public FrmSugefRomMonitorController(IConfiguration config)
        {
            _bl = new FrmSugefRomMonitorBL(config);
        }

        [Authorize]
        [HttpGet("SUGEF_TipoCambio_Obtener")]
        public ErrorDto<SugefTipoCambioResult?> SUGEF_TipoCambio_Obtener(int codEmpresa, [FromQuery] DateTime fecha)
        {
            return _bl.SUGEF_TipoCambio_Obtener(codEmpresa, fecha);
        }

        [Authorize]
        [HttpGet("SUGEF_ROM_Monitor_Consulta")]
        public ErrorDto<List<SugefRomMonitorConsultaResult>> SUGEF_ROM_Monitor_Consulta(int codEmpresa, [FromQuery] DateTime corte)
        {
            return _bl.SUGEF_ROM_Monitor_Consulta(codEmpresa, corte);
        }

        [Authorize]
        [HttpGet("SUGEF_ROM_Monitor_Detalle")]
        public ErrorDto<List<SugefRomMonitorDetalleResult>> SUGEF_ROM_Monitor_Detalle(int codEmpresa, [FromQuery] DateTime corte, [FromQuery] int rom)
        {
            return _bl.SUGEF_ROM_Monitor_Detalle(codEmpresa, corte, rom);
        }

        [Authorize]
        [HttpGet("SUGEF_ROM_Monitor_Forma_Pago")]
        public ErrorDto<List<SugefRomMonitorFormaPagoResult>> SUGEF_ROM_Monitor_Forma_Pago(
            int codEmpresa,
            [FromQuery] DateTime corte,
            [FromQuery] string tipoDoc,
            [FromQuery] string numDoc)
        {
            return _bl.SUGEF_ROM_Monitor_Forma_Pago(codEmpresa, corte, tipoDoc, numDoc);
        }

        [Authorize]
        [HttpGet("SUGEF_EntidadesPago_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> SUGEF_EntidadesPago_Lista(int codEmpresa)
        {
            return _bl.SUGEF_EntidadesPago_Lista(codEmpresa);
        }

        [Authorize]
        [HttpGet("SUGEF_OrigenRecursos_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> SUGEF_OrigenRecursos_Lista(int codEmpresa)
        {
            return _bl.SUGEF_OrigenRecursos_Lista(codEmpresa);
        }

        [Authorize]
        [HttpPost("SUGEF_ROM_Monitor")]
        public ErrorDto<bool> SUGEF_ROM_Monitor(int codEmpresa, [FromBody] SugefRomMonitorParams param)
        {
            return _bl.SUGEF_ROM_Monitor(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("SUGEF_ROM_Monitor_Forma_Pago_Actualiza")]
        public ErrorDto<bool> SUGEF_ROM_Monitor_Forma_Pago_Actualiza(
            int codEmpresa,
            [FromBody] SugefRomMonitorFormaPagoActualizaParams param)
        {
            return _bl.SUGEF_ROM_Monitor_Forma_Pago_Actualiza(codEmpresa, param);
        }
    }
}
