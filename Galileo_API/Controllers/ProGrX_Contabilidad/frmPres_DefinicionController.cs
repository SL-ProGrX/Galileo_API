using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;

namespace Galileo.Controllers
{
    [Route("api/frmPres_Definicion")]
    [Route("api/FrmPresDefinicion")]
    [ApiController]
    public class FrmPresDefinicionController : ControllerBase
    {
        readonly FrmPresDefinicionBl _bl;
        public FrmPresDefinicionController(IConfiguration config)
        {
            _bl = new FrmPresDefinicionBl(config);
        }

        [HttpGet("Pres_Modelos_Obtener")]
        [Authorize]
        public ErrorDto<List<ModeloGenericList>> Pres_Modelos_Obtener(int CodEmpresa, string usuario, int codContab)
        {
            return _bl.Pres_Modelos_Obtener(CodEmpresa, usuario, codContab);
        }

        [HttpGet("Pres_Modelo_Unidades_Obtener")]
        [Authorize]
        public ErrorDto<List<ModeloGenericList>> Pres_Modelo_Unidades_Obtener(int CodEmpresa, string codModelo, int codContab, string usuario)
        {
            return _bl.Pres_Modelo_Unidades_Obtener(CodEmpresa, codModelo, codContab, usuario);
        }

        [HttpGet("Pres_Modelo_Unidades_CC_Obtener")]
        [Authorize]
        public ErrorDto<List<ModeloGenericList>> Pres_Modelo_Unidades_CC_Obtener(int CodEmpresa, string codModelo, int codContab, string codUnidad)
        {
            return _bl.Pres_Modelo_Unidades_CC_Obtener(CodEmpresa, codModelo, codContab, codUnidad);
        }

        [HttpGet("Pres_Definicion_scroll")]
        [Authorize]
        public ErrorDto<CntxCuentasData> Pres_Definicion_scroll(int CodEmpresa, int scrollValue, string? CodCtaMask, int CodContab)
        {
            return _bl.Pres_Definicion_scroll(CodEmpresa, scrollValue, CodCtaMask, CodContab);
        }

        [HttpPost("Pres_VistaPresupuesto_Cuenta_SP")]
        [Authorize]
        public ErrorDto<List<VistaPresCuentaData>> Pres_VistaPresupuesto_Cuenta_SP(int CodEmpresa, PresCuenta request)
        {
            return _bl.Pres_VistaPresupuesto_Cuenta_SP(CodEmpresa, request);
        }

        [HttpGet("Pres_Cuentas_Obtener")]
        [Authorize]
        public ErrorDto<CuentasLista> Pres_Cuentas_Obtener(int CodEmpresa, string cod_contabilidad, int? pagina, int? paginacion, string? filtro)
        {
            return _bl.Pres_Cuentas_Obtener(CodEmpresa, cod_contabilidad, pagina, paginacion, filtro);
        }
    }
}