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
    public class FrmCxCCuentasSgtRebajosInternosController : ControllerBase
    {
        private readonly FrmCxCCuentasSgtRebajosInternosBL BL;

        public FrmCxCCuentasSgtRebajosInternosController(IConfiguration config)
        {
            BL = new FrmCxCCuentasSgtRebajosInternosBL(config);
        }

        
        [HttpGet("CxC_Cuentas_SGT_Rebajos_Operacion_Obtener")]
        public ErrorDto<CxCCuentasSgtRebajosInternosPantallaDto> CxC_Cuentas_SGT_Rebajos_Operacion_Obtener(
            int codEmpresa,
            int operacion)
        {
            return BL.CxC_Cuentas_SGT_Rebajos_Operacion_Obtener(codEmpresa, operacion);
        }

        [HttpGet("CxC_Cuentas_SGT_Rebajos_Terceros_Obtener")]
        public ErrorDto<List<CxCCuentaRebajoInternoDto>> CxC_Cuentas_SGT_Rebajos_Terceros_Obtener(
           int CodEmpresa,
           string Cedula)
        {
            return BL.CxC_Cuentas_SGT_Rebajos_Terceros_Obtener(CodEmpresa, Cedula);
        }

        [HttpGet("CxC_Cuentas_SGT_Rebajos_CargoReposicion_Obtener")]
        public ErrorDto<decimal> CxC_Cuentas_SGT_Rebajos_CargoReposicion_Obtener(
    int codEmpresa,
    int operacion)
        {
            return BL.CxC_Cuentas_SGT_Rebajos_CargoReposicion_Obtener(codEmpresa, operacion);
        }

        [HttpGet("CxC_Cuentas_SGT_Rebajos_Existe_Obtener")]
        public ErrorDto<bool> CxC_Cuentas_SGT_Rebajos_Existe_Obtener(
            int CodEmpresa,
            int Operacion,
            int Operacion_Aplicada)
        {
            return BL.CxC_Cuentas_SGT_Rebajos_Existe_Obtener(CodEmpresa, Operacion, Operacion_Aplicada);
        }

        [HttpPost("CxC_Cuentas_SGT_Rebajos_Guardar")]
        public ErrorDto CxC_Cuentas_SGT_Rebajos_Guardar(
            int CodEmpresa,
            string Usuario,
            int Contabilidad,
            [FromBody] CxCCuentasSgtRebajosInternosGuardarDto req)
        {
            return BL.CxC_Cuentas_SGT_Rebajos_Guardar(CodEmpresa, Usuario, Contabilidad, req);
        }

        [HttpPost("CxC_Cuentas_SGT_Rebajos_Eliminar")]
        public ErrorDto CxC_Cuentas_SGT_Rebajos_Eliminar(
           int CodEmpresa,
           [FromBody] CxCCuentasSgtRebajosInternosEliminarDto req)
        {
            return BL.CxC_Cuentas_SGT_Rebajos_Eliminar(CodEmpresa, req);
        }
    }
}
