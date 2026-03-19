using Galileo.DataBaseTier;
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
    public class FrmCxcCuentasController : ControllerBase
    {
        private readonly FrmCxcCuentasBL _BL;
       

        public FrmCxcCuentasController(IConfiguration config)
        {
            _BL = new FrmCxcCuentasBL(config);
            
        }

        [HttpGet("fxFechaServidor")]
        public DateTime fxFechaServidor(int CodEmpresa)
        {
            return _BL.fxFechaServidor(CodEmpresa);
        }

        [HttpGet("fxCxC_Parametro")]
        public ErrorDto<string> fxCxC_Parametro(int codEmpresa, string codParametro)
        {
            return _BL.fxCxC_Parametro(codEmpresa, codParametro);
        }

        [HttpGet("CxCCuentasBusquedaOperacionLista_Obtener")]
        public ErrorDto<CxCCuentasBusquedaOperacionLista> CxCCuentasBusquedaOperacionLista_Obtener(
           int CodEmpresa,
           string filtros,
           bool esExportar = false)
        {
            return _BL.CxCCuentasBusquedaOperacionLista_Obtener(CodEmpresa, filtros, esExportar);
        }

        [HttpGet("CxCCuentasOperacion_Obtener")]
        public ErrorDto<CxCCuentasBusquedaOperacionItem> CxCCuentasOperacion_Obtener(int CodEmpresa, long operacion)
        {
            return _BL.CxCCuentasOperacion_Obtener(CodEmpresa, operacion);
        }

        [HttpGet("CxCCuentasOperacionScroll_Obtener")]
        public ErrorDto<CxCCuentasBusquedaOperacionItem> CxCCuentasOperacionScroll_Obtener(int CodEmpresa, long operacion, int tipo)
        {
            return _BL.CxCCuentasOperacionScroll_Obtener(CodEmpresa, operacion, tipo);
        }

        [HttpGet("CxCCuentas_Divisas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxCCuentas_Divisas_Obtener(int CodEmpresa, int codContabilidad)
        {
            return _BL.CxCCuentas_Divisas_Obtener(CodEmpresa, codContabilidad);
        }

        [HttpGet("CxCCuentas_Oficinas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxCCuentas_Oficinas_Obtener(int CodEmpresa)
        {
            return _BL.CxCCuentas_Oficinas_Obtener(CodEmpresa);
        }

        [HttpGet("CxCCuentas_Bancos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxCCuentas_Bancos_Obtener(int CodEmpresa)
        {
            return _BL.CxCCuentas_Bancos_Obtener(CodEmpresa);
        }

        [HttpGet("CxCCuentas_Consulta_Obtener")]
        public ErrorDto<CxCCuentasConsultaData> CxCCuentas_Consulta_Obtener(int CodEmpresa, long operacion)
        {
            return _BL.CxCCuentas_Consulta_Obtener(CodEmpresa, operacion);
        }
    }
}
