using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Cajas;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.Controllers.ProGrX.Cajas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCajasConsultaMovimientosFormaPagoController : ControllerBase
    {
        private readonly FrmCajasConsultaMovimientosFormaPagoBL _bl;

        public FrmCajasConsultaMovimientosFormaPagoController(IConfiguration config)
        {
            _bl = new FrmCajasConsultaMovimientosFormaPagoBL(config);
        }

        [HttpGet("Cajas_FormasPago_DropDown_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_FormasPago_DropDown_Obtener(int CodEmpresa)
        {
            return _bl.Cajas_FormasPago_DropDown_Obtener(CodEmpresa);
        }

        [HttpGet("Cajas_Cajas_DropDown_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_Cajas_DropDown_Obtener(int CodEmpresa)
        {
            return _bl.Cajas_Cajas_DropDown_Obtener(CodEmpresa);
        }

        [HttpGet("Cajas_EntidadesPago_DropDown_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_EntidadesPago_DropDown_Obtener(int CodEmpresa)
        {
            return _bl.Cajas_EntidadesPago_DropDown_Obtener(CodEmpresa);
        }

        [HttpGet("Cajas_OrigenRecursos_DropDown_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_OrigenRecursos_DropDown_Obtener(int CodEmpresa)
        {
            return _bl.Cajas_OrigenRecursos_DropDown_Obtener(CodEmpresa);
        }

        [HttpGet("Cajas_UltimaApertura_Obtener")]
        [Authorize]
        public ErrorDto<long> Cajas_UltimaApertura_Obtener(int CodEmpresa, string CodCaja)
        {
            return _bl.Cajas_UltimaApertura_Obtener(CodEmpresa, CodCaja);
        }

        [HttpGet("Cajas_ConsultaMovimientos_FormaPago_Lista_Obtener")]
        [Authorize]
        public ErrorDto<CajasMovimientosFormaPagoLista> Cajas_ConsultaMovimientos_FormaPago_Lista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Cajas_ConsultaMovimientos_FormaPago_Lista_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("Cajas_ConsultaMovimientos_FormaPago_Lista_Export")]
        [Authorize]
        public ErrorDto<CajasMovimientosFormaPagoLista> Cajas_ConsultaMovimientos_FormaPago_Lista_Export(int CodEmpresa, string filtros)
        {
            return _bl.Cajas_ConsultaMovimientos_FormaPago_Lista_Export(CodEmpresa, filtros);
        }
    }
}
