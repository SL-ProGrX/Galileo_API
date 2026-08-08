using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.Controllers
{
    [Route("api/frmCC_EstadoCuenta")]
    [ApiController]
    public class FrmCcEstadoCuentaController :
        ControllerBase
    {
        private readonly FrmCcEstadoCuentaBl _bl;

        public FrmCcEstadoCuentaController(
            IConfiguration config)
        {
            _bl = new FrmCcEstadoCuentaBl(config);
        }

        [HttpGet(
            "CC_FrmCCEstadoCuenta_Inicial_Obtener")]
        public ErrorDto<CcEstadoCuentaInicialData>
            CC_FrmCCEstadoCuenta_Inicial_Obtener(
                int codEmpresa)
        {
            return _bl
                .CC_FrmCCEstadoCuenta_Inicial_Obtener(
                    codEmpresa);
        }

        [HttpGet(
            "CC_FrmCCEstadoCuenta_Persona_Obtener")]
        public ErrorDto<CcEstadoCuentaPersonaData>
            CC_FrmCCEstadoCuenta_Persona_Obtener(
                int codEmpresa,
                string request)
        {
            return _bl
                .CC_FrmCCEstadoCuenta_Persona_Obtener(
                    codEmpresa,
                    request);
        }

        [HttpGet(
            "CC_FrmCCEstadoCuenta_Departamentos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
            CC_FrmCCEstadoCuenta_Departamentos_Obtener(
                int codEmpresa,
                string request)
        {
            return _bl
                .CC_FrmCCEstadoCuenta_Departamentos_Obtener(
                    codEmpresa,
                    request);
        }

        [HttpGet(
            "CC_FrmCCEstadoCuenta_Secciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
            CC_FrmCCEstadoCuenta_Secciones_Obtener(
                int codEmpresa,
                string request)
        {
            return _bl
                .CC_FrmCCEstadoCuenta_Secciones_Obtener(
                    codEmpresa,
                    request);
        }

        [HttpPost(
            "CC_FrmCCEstadoCuenta_Email_Enviar")]
        public ErrorDto
            CC_FrmCCEstadoCuenta_Email_Enviar(
                int codEmpresa,
                CcEstadoCuentaEmailRequest request)
        {
            return _bl
                .CC_FrmCCEstadoCuenta_Email_Enviar(
                    codEmpresa,
                    request);
        }

        [HttpPost(
            "CC_FrmCCEstadoCuenta_EmailMasivo_Enviar")]
        public ErrorDto
            CC_FrmCCEstadoCuenta_EmailMasivo_Enviar(
                int codEmpresa,
                CcEstadoCuentaEmailMasivoRequest request)
        {
            return _bl
                .CC_FrmCCEstadoCuenta_EmailMasivo_Enviar(
                    codEmpresa,
                    request);
        }

        [HttpPost("CC_FrmCCEstadoCuenta_Reporte_Bitacora_Registrar")]
        public ErrorDto CC_FrmCCEstadoCuenta_Reporte_Bitacora_Registrar(
            int codEmpresa,
        CcEstadoCuentaReporteBitacoraRequest request)
        {
            return _bl
                .CC_FrmCCEstadoCuenta_Reporte_Bitacora_Registrar(
                    codEmpresa,
                    request);
        }
    }
}