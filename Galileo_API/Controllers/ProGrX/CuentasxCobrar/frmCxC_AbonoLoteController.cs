using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.CuentasxCobrar
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class FrmCxCAbonoLoteController : ControllerBase
    {
        private readonly FrmCxCAbonoLoteBl _bl;

        public FrmCxCAbonoLoteController(IConfiguration config)
        {
            _bl = new FrmCxCAbonoLoteBl(config);
        }

        [HttpGet("CxC_AbonoLote_Clientes_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
            CxC_AbonoLote_Clientes_Obtener(
                int codEmpresa)
        {
            return _bl.CxC_AbonoLote_Clientes_Obtener(codEmpresa);
        }

        [HttpGet("CxC_AbonoLote_Divisas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
            CxC_AbonoLote_Divisas_Obtener(
                int codEmpresa,
                string cedula)
        {
            return _bl.CxC_AbonoLote_Divisas_Obtener(
                codEmpresa,
                cedula);
        }

        [HttpGet("CxC_AbonoLote_Facturas_Obtener")]
        public ErrorDto<List<CxCFactPendienteCancelacionData>>
            CxC_AbonoLote_Facturas_Obtener(
                int codEmpresa,
                [FromQuery] CxCAbonoLoteFacturasRequest request)
        {
            return _bl.CxC_AbonoLote_Facturas_Obtener(
                codEmpresa,
                request);
        }

        [HttpGet("CxC_AbonoLote_TiposDocumento_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
            CxC_AbonoLote_TiposDocumento_Obtener(
                int codEmpresa,
                string caja)
        {
            return _bl.CxC_AbonoLote_TiposDocumento_Obtener(
                codEmpresa,
                caja);
        }

        [HttpPost("CxC_AbonoLote_Aplicar")]
        public ErrorDto<CxCAbonoLoteAplicarData>
            CxC_AbonoLote_Aplicar(
                int codEmpresa,
                CxCAbonoLoteAplicarRequest request)
        {
            return _bl.CxC_AbonoLote_Aplicar(
                codEmpresa,
                request);
        }
    }
}