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
    public class FrmCajasAutorizacionSaldosEnTransitoController : ControllerBase
    {
        private readonly FrmCajasAutorizacionSaldosEnTransitoBL _bl;

        public FrmCajasAutorizacionSaldosEnTransitoController(IConfiguration config)
        {
            _bl = new FrmCajasAutorizacionSaldosEnTransitoBL(config);
        }
        
        [HttpGet("Cajas_SaldosFavor_TiposDoc_DropDown_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>>Cajas_SaldosFavor_TiposDoc_DropDown_Obtener(int CodEmpresa)
        {
            return _bl.Cajas_SaldosFavor_TiposDoc_DropDown_Obtener(CodEmpresa);
        }

        [HttpGet("Cajas_SaldosFavor_EntidadesPago_DropDown_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>>Cajas_SaldosFavor_EntidadesPago_DropDown_Obtener(int CodEmpresa)
        {
            return _bl.Cajas_SaldosFavor_EntidadesPago_DropDown_Obtener(CodEmpresa);
        }

        [HttpGet("Cajas_SaldosFavor_OrigenRecursos_DropDown_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>>Cajas_SaldosFavor_OrigenRecursos_DropDown_Obtener(int CodEmpresa)
        {
            return _bl.Cajas_SaldosFavor_OrigenRecursos_DropDown_Obtener(CodEmpresa);
        }
        
        [HttpGet("Cajas_SaldosFavor_Lista_Obtener")]
        [Authorize]
        public ErrorDto<CajasSaldosFavorLista>Cajas_SaldosFavor_Lista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Cajas_SaldosFavor_Lista_Obtener(CodEmpresa, filtros);
        }
        
        [HttpGet("Cajas_SaldosFavor_Lista_Export")]
        [Authorize]
        public ErrorDto<CajasSaldosFavorLista>Cajas_SaldosFavor_Lista_Export(int CodEmpresa, string filtros)
        {
            return _bl.Cajas_SaldosFavor_Lista_Export(CodEmpresa, filtros);
        }
        
        [HttpPost("Cajas_SaldosFavor_ValoresTransito_Autorizar")]
        [Authorize]
        public ErrorDto Cajas_SaldosFavor_ValoresTransito_Autorizar(int CodEmpresa,CajasSaldosFavorAutorizaRequest data)
        {
            return _bl.Cajas_SaldosFavor_ValoresTransito_Autorizar(CodEmpresa, data);
        }
        
        [HttpGet("Cajas_SaldosFavor_EmpresaInfo_Obtener")]
        [Authorize]
        public ErrorDto<CajasEmpresaInfoDto> Cajas_SaldosFavor_EmpresaInfo_Obtener(int CodEmpresa)
        {
            return _bl.Cajas_SaldosFavor_EmpresaInfo_Obtener(CodEmpresa);
            
        }
    }
}