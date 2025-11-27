using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class frmPres_Modelo_CuentasController : ControllerBase
    {
        readonly FrmPresModeloCuentasBl _bl;
        public frmPres_Modelo_CuentasController(IConfiguration config)
        {
            _bl = new FrmPresModeloCuentasBl(config);
        }

        [HttpGet("spPres_CuentasCatalogo_Obtener")]
        [Authorize]
        public ErrorDto<List<CuentasCatalogoData>> spPres_CuentasCatalogo_Obtener(int CodEmpresa, int CodContab, string CodModelo, string CodUnidad, string CodCentroCosto)
        {
            return _bl.spPres_CuentasCatalogo_Obtener(CodEmpresa, CodContab, CodModelo, CodUnidad, CodCentroCosto);
        }

        [HttpGet("Pres_Modelos_Obtener")]
        [Authorize]
        public ErrorDto<List<ModeloGenericList>> Pres_Modelos_Obtener(int CodEmpresa, int CodContab, string Usuario)
        {
            return _bl.Pres_Modelos_Obtener(CodEmpresa, CodContab, Usuario);
        }

        [HttpGet("Pres_Unidades_Obtener")]
        [Authorize]
        public ErrorDto<List<ModeloGenericList>> Pres_Unidades_Obtener(int CodEmpresa, int CodContab, string Usuario)
        {
            return _bl.Pres_Unidades_Obtener(CodEmpresa, CodContab, Usuario);
        }

        [HttpGet("Pres_CentroCosto_Obtener")]
        [Authorize]
        public ErrorDto<List<ModeloGenericList>> Pres_CentroCosto_Obtener(int CodEmpresa, int CodContab, string CodUnidad)
        {
            return _bl.Pres_CentroCosto_Obtener(CodEmpresa, CodContab, CodUnidad);
        }

        [HttpPost("spPres_Modelo_Cuentas_CargaDatos")]
        [Authorize]
        public ErrorDto spPres_Modelo_Cuentas_CargaDatos(int CodEmpresa, List<PresModeloCuentasImportData> request)
        {
            return _bl.spPres_Modelo_Cuentas_CargaDatos(CodEmpresa, request);
        }

        [HttpGet("spPres_Modelo_Cuentas_RevisaImport")]
        [Authorize]
        public ErrorDto<List<PresModeloCuentasImportData>> spPres_Modelo_Cuentas_RevisaImport(int CodEmpresa, int CodContab, string CodModelo, string Usuario)
        {
            return _bl.spPres_Modelo_Cuentas_RevisaImport(CodEmpresa, CodContab, CodModelo, Usuario);
        }

        [HttpPost("spPres_Modelo_Cuentas_Import")]
        [Authorize]
        public ErrorDto spPres_Modelo_Cuentas_Import(int CodEmpresa, int CodContab, string CodModelo, string Usuario)
        {
            return _bl.spPres_Modelo_Cuentas_Import(CodEmpresa, CodContab, CodModelo, Usuario);
        }

        [HttpPost("spCntX_Periodo_Fiscal_Meses")]
        [Authorize]
        public ErrorDto<List<PresModeloCuentasImportData>> spCntX_Periodo_Fiscal_Meses(int CodEmpresa, int CodContab, string CodModelo, string Usuario, List<PresModeloCuentasHorizontal> request)
        {
            return _bl.spCntX_Periodo_Fiscal_Meses(CodEmpresa, CodContab, CodModelo, Usuario, request);
        }
    }
}