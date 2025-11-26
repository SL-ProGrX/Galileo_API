using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;

namespace Galileo.Controllers
{
    [Route("api/frmCC_AutorizaSolicitudes")]
    [ApiController]
    public class FrmCcAutorizaSolicitudesController : ControllerBase
    {
        readonly FrmCcAutorizaSolicitudesBl BL_CC_AutorizaSolicitudes;
        public FrmCcAutorizaSolicitudesController(IConfiguration config)
        {
            BL_CC_AutorizaSolicitudes = new FrmCcAutorizaSolicitudesBl(config);
        }

        [HttpGet("CC_Cuentas_Obtener")]
        public List<CCGenericList> CC_Cuentas_Obtener(int CodEmpresa)
        {
            return BL_CC_AutorizaSolicitudes.CC_Cuentas_Obtener(CodEmpresa);
        }

        [HttpGet("CC_ModuloCredito_Obtener")]
        public List<AutorizaSolicitudesCreditoData> CC_ModuloCredito_Obtener(int CodEmpresa, int? CodBanco, string FechaInicio, string FechaCorte)
        {
            return BL_CC_AutorizaSolicitudes.CC_ModuloCredito_Obtener(CodEmpresa, CodBanco, FechaInicio, FechaCorte);
        }

        [HttpGet("CC_ModuloFondos_Obtener")]
        public List<AutorizaSolicitudesFondosData> CC_ModuloFondos_Obtener(int CodEmpresa, int? CodBanco, string FechaInicio, string FechaCorte)
        {
            return BL_CC_AutorizaSolicitudes.CC_ModuloFondos_Obtener(CodEmpresa, CodBanco, FechaInicio, FechaCorte);
        }

        [HttpGet("CC_ModuloLiquidacion_Obtener")]
        public List<AutorizaSolicitudesLiquidacionData> CC_ModuloLiquidacion_Obtener(int CodEmpresa, int? CodBanco, string FechaInicio, string FechaCorte)
        {
            return BL_CC_AutorizaSolicitudes.CC_ModuloLiquidacion_Obtener(CodEmpresa, CodBanco, FechaInicio, FechaCorte);
        }

        [HttpGet("CC_ModuloBeneficios_Obtener")]
        public List<AutorizaSolicitudesBeneficiosData> CC_ModuloBeneficios_Obtener(int CodEmpresa, int? CodBanco, string FechaInicio, string FechaCorte)
        {
            return BL_CC_AutorizaSolicitudes.CC_ModuloBeneficios_Obtener(CodEmpresa, CodBanco, FechaInicio, FechaCorte);
        }

        [HttpGet("CC_ModuloHipotecario_Obtener")]
        public List<AutorizaSolicitudesHipotecarioData> CC_ModuloHipotecario_Obtener(int CodEmpresa, int? CodBanco, string FechaInicio, string FechaCorte)
        {
            return BL_CC_AutorizaSolicitudes.CC_ModuloHipotecario_Obtener(CodEmpresa, CodBanco, FechaInicio, FechaCorte);
        }

        [HttpPost("CC_ModuloCredito_Autorizar")]
        public ErrorDto CC_ModuloCredito_Autorizar(int CodEmpresa, string Usuario, int Id_Solicitud)
        {
            return BL_CC_AutorizaSolicitudes.CC_ModuloCredito_Autorizar(CodEmpresa, Usuario, Id_Solicitud);
        }

        [HttpPost("CC_ModuloFondos_Autorizar")]
        public ErrorDto CC_ModuloFondos_Autorizar(int CodEmpresa, string Usuario, int Consec)
        {
            return BL_CC_AutorizaSolicitudes.CC_ModuloFondos_Autorizar(CodEmpresa, Usuario, Consec);
        }

        [HttpPost("CC_ModuloLiquidacion_Autorizar")]
        public ErrorDto CC_ModuloLiquidacion_Autorizar(int CodEmpresa, string Usuario, int Consec)
        {
            return BL_CC_AutorizaSolicitudes.CC_ModuloLiquidacion_Autorizar(CodEmpresa, Usuario, Consec);
        }

        [HttpPost("CC_ModuloBeneficios_Autorizar")]
        public ErrorDto CC_ModuloBeneficios_Autorizar(int CodEmpresa, string Usuario, int Consec, string Cod_Beneficio)
        {
            return BL_CC_AutorizaSolicitudes.CC_ModuloBeneficios_Autorizar(CodEmpresa, Usuario, Consec, Cod_Beneficio);
        }

        [HttpPost("CC_ModuloHipotecario_Autorizar")]
        public ErrorDto CC_ModuloHipotecario_Autorizar(int CodEmpresa, string Usuario, int CodigoDesembolso)
        {
            return BL_CC_AutorizaSolicitudes.CC_ModuloHipotecario_Autorizar(CodEmpresa, Usuario, CodigoDesembolso);
        }
    }
}