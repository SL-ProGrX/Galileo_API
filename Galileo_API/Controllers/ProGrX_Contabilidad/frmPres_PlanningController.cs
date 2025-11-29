using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.PRES;

namespace Galileo.Controllers
{
    [Route("api/frmPres_Planning")]
    [Route("api/FrmPresPlanning")]
    [ApiController]
    public class FrmPresPlanningController : ControllerBase
    {

        readonly FrmPresPlanningBl BL_frmPres_Planning;
        public FrmPresPlanningController(IConfiguration config)
        {
            BL_frmPres_Planning = new FrmPresPlanningBl(config);
        }

        [Authorize]
        [HttpGet("PresPlanning_Obtener")]
        public ErrorDto<List<PresVistaPresupuestoData>> PresPlanning_Obtener(int CodCliente, string datos)
        {
            return BL_frmPres_Planning.PresPlanning_Obtener(CodCliente, datos);
        }

        [Authorize]
        [HttpGet("PresPlanningCuenta_Obtener")]
        public ErrorDto<List<PreVistaPresupuestoCuentaData>> PresPlanningCuenta_Obtener(int CodCliente, string datos)
        {
            return BL_frmPres_Planning.PresPlanningCuenta_Obtener(CodCliente, datos);
        }

        [Authorize]
        [HttpGet("PresPlanningCuentaReal_Obtener")]
        public ErrorDto<List<PresVistaPresCuentaRealHistoricoData>> PresPlanningCuentaReal_Obtener(int CodCliente, string datos)
        {
            return BL_frmPres_Planning.PresPlanningCuentaReal_Obtener(CodCliente, datos);
        }

        [Authorize]
        [HttpPost("PresAjustes_Guardar")]
        public ErrorDto PresAjustes_Guardar(int CodCliente, PresAjustesGuarda request)
        {
            return BL_frmPres_Planning.PresAjustes_Guardar(CodCliente, request);
        }

        [Authorize]
        [HttpGet("Pres_Cierre_Obtener")]
        public ErrorDto<CntxCierres> Pres_Cierre_Obtener(int CodEmpresa, string codModelo, int codContab, string usuario)
        {
            return BL_frmPres_Planning.Pres_Cierre_Obtener(CodEmpresa, codModelo, codContab, usuario);
        }

        [Authorize]
        [HttpGet("Pres_Ajustes_Obtener")]
        public ErrorDto<List<PreVistaPresupuestoCuentaData>> Pres_Ajustes_Obtener(int CodCliente, int consulta, string datos)
        {
            return BL_frmPres_Planning.Pres_Ajustes_Obtener(CodCliente, consulta, datos);
        }

        [Authorize]
        [HttpGet("Pres_Modelos_Obtener")]
        public ErrorDto<List<PresModelisLista>> Pres_Modelos_Obtener(int CodEmpresa, int CodContab, string Usuario)
        {
            return BL_frmPres_Planning.Pres_Modelos_Obtener(CodEmpresa, CodContab, Usuario);
        }

        [Authorize]
        [HttpGet("Pres_Ajustes_Permitidos_Obtener")]
        public ErrorDto<List<ModeloGenericList>> Pres_Ajustes_Permitidos_Obtener(int CodEmpresa, int codContab, string codModelo, string Usuario)
        {
            return BL_frmPres_Planning.Pres_Ajustes_Permitidos_Obtener(CodEmpresa, codContab, codModelo, Usuario);
        }

        [Authorize]
        [HttpPost("Pres_AjusteMasivo_Guardar")]
        public ErrorDto Pres_AjusteMasivo_Guardar(int CodEmpresa, int codContab, string codModelo, string usuario, DateTime periodo, List<PresCargaMasivaModel> datos)
        {
            return BL_frmPres_Planning.Pres_AjusteMasivo_Guardar(CodEmpresa, codContab, codModelo, usuario, periodo, datos);
        }
    }
}
