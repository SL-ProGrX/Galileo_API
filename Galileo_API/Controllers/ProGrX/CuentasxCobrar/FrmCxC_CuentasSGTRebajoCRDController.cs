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
    public class FrmCxCCuentasSgtRebajoCrdController : ControllerBase
    {
        private readonly FrmCxCCuentasSgtRebajoCrdBL BL;

        public FrmCxCCuentasSgtRebajoCrdController(IConfiguration config)
        {
            BL = new FrmCxCCuentasSgtRebajoCrdBL(config);
        }

        [HttpGet("CxC_Cuentas_SGT_Rebajo_CRD_Operacion_Obtener")]
        public ErrorDto<CxCCuentasSgtRebajoCrdPantallaDto> CxC_Cuentas_SGT_Rebajo_CRD_Operacion_Obtener(
    int codEmpresa,
    int operacion,
    int cta_Pendientes)
        {
            return BL.CxC_Cuentas_SGT_Rebajo_CRD_Operacion_Obtener(codEmpresa, operacion, cta_Pendientes);
        }

        [HttpGet("CxC_Cuentas_SGT_Rebajo_CRD_Terceros_Obtener")]
        public ErrorDto<List<CxCCuentaRebajoCrdDto>> CxC_Cuentas_SGT_Rebajo_CRD_Terceros_Obtener(
            int codEmpresa,
            string cedula,
            int cta_Pendientes)
        {
            return BL.CxC_Cuentas_SGT_Rebajo_CRD_Terceros_Obtener(codEmpresa, cedula, cta_Pendientes);
        }

        [HttpGet("CxC_Cuentas_SGT_Rebajo_CRD_Existe_Obtener")]
        public ErrorDto<bool> CxC_Cuentas_SGT_Rebajo_CRD_Existe_Obtener(
            int codEmpresa,
            int operacion,
            int id_Solicitud)
        {
            return BL.CxC_Cuentas_SGT_Rebajo_CRD_Existe_Obtener(codEmpresa, operacion, id_Solicitud);
        }

        [HttpPost("CxC_Cuentas_SGT_Rebajo_CRD_Guardar")]
        public ErrorDto CxC_Cuentas_SGT_Rebajo_CRD_Guardar(
            int codEmpresa,
            [FromBody] CxCCuentasSgtRebajoCrdGuardarDto req)
        {
            return BL.CxC_Cuentas_SGT_Rebajo_CRD_Guardar(codEmpresa, req);
        }

        [HttpPost("CxC_Cuentas_SGT_Rebajo_CRD_Eliminar")]
        public ErrorDto CxC_Cuentas_SGT_Rebajo_CRD_Eliminar(
            int codEmpresa,
            [FromBody] CxCCuentasSgtRebajoCrdEliminarDto req)
        {
            return BL.CxC_Cuentas_SGT_Rebajo_CRD_Eliminar(codEmpresa, req);
        }

        [HttpPost("CxC_Cuentas_SGT_Rebajo_CRD_Actualizar")]
        public ErrorDto CxC_Cuentas_SGT_Rebajo_CRD_Actualizar(
            int codEmpresa,
            [FromBody] CxCCuentasSgtRebajoCrdActualizarDto req)
        {
            return BL.CxC_Cuentas_SGT_Rebajo_CRD_Actualizar(codEmpresa, req);
        }
    }
}
