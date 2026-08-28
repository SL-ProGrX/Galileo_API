using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.Controllers
{
    [Route("api/frmCC_AutorizaSolicitudes")]
    [ApiController]
    public class FrmCcAutorizaSolicitudesController :
        ControllerBase
    {
        private readonly FrmCcAutorizaSolicitudesBl _bl;

        public FrmCcAutorizaSolicitudesController(
            IConfiguration config)
        {
            _bl = new FrmCcAutorizaSolicitudesBl(config);
        }

        [HttpGet("CC_Cuentas_Obtener")]
        public ErrorDto<List<CCGenericList>>
            CC_Cuentas_Obtener(
                int CodEmpresa)
        {
            return _bl
                .CC_Cuentas_Obtener(
                    CodEmpresa);
        }

        [HttpGet("CC_ModuloCredito_Obtener")]
        public ErrorDto<List<AutorizaSolicitudesCreditoData>>
            CC_ModuloCredito_Obtener(
                int CodEmpresa,
                int? CodBanco,
                string FechaInicio,
                string FechaCorte)
        {
            return _bl
                .CC_ModuloCredito_Obtener(
                    CodEmpresa,
                    CodBanco,
                    FechaInicio,
                    FechaCorte);
        }

        [HttpGet("CC_ModuloFondos_Obtener")]
        public ErrorDto<List<AutorizaSolicitudesFondosData>>
            CC_ModuloFondos_Obtener(
                int CodEmpresa,
                int? CodBanco,
                string FechaInicio,
                string FechaCorte)
        {
            return _bl
                .CC_ModuloFondos_Obtener(
                    CodEmpresa,
                    CodBanco,
                    FechaInicio,
                    FechaCorte);
        }

        [HttpGet("CC_ModuloLiquidacion_Obtener")]
        public ErrorDto<List<AutorizaSolicitudesLiquidacionData>>
            CC_ModuloLiquidacion_Obtener(
                int CodEmpresa,
                int? CodBanco,
                string FechaInicio,
                string FechaCorte)
        {
            return _bl
                .CC_ModuloLiquidacion_Obtener(
                    CodEmpresa,
                    CodBanco,
                    FechaInicio,
                    FechaCorte);
        }

        [HttpGet("CC_ModuloBeneficios_Obtener")]
        public ErrorDto<List<AutorizaSolicitudesBeneficiosData>>
            CC_ModuloBeneficios_Obtener(
                int CodEmpresa,
                int? CodBanco,
                string FechaInicio,
                string FechaCorte)
        {
            return _bl
                .CC_ModuloBeneficios_Obtener(
                    CodEmpresa,
                    CodBanco,
                    FechaInicio,
                    FechaCorte);
        }

        [HttpGet("CC_ModuloHipotecario_Obtener")]
        public ErrorDto<List<AutorizaSolicitudesHipotecarioData>>
            CC_ModuloHipotecario_Obtener(
                int CodEmpresa,
                int? CodBanco,
                string FechaInicio,
                string FechaCorte)
        {
            return _bl
                .CC_ModuloHipotecario_Obtener(
                    CodEmpresa,
                    CodBanco,
                    FechaInicio,
                    FechaCorte);
        }

        [HttpPut("CC_ModuloCredito_Autorizar")]
        public ErrorDto
            CC_ModuloCredito_Autorizar(
                int CodEmpresa,
                string Usuario,
                int Id_Solicitud)
        {
            return _bl
                .CC_ModuloCredito_Autorizar(
                    CodEmpresa,
                    Usuario,
                    Id_Solicitud);
        }

        [HttpPut("CC_ModuloFondos_Autorizar")]
        public ErrorDto
            CC_ModuloFondos_Autorizar(
                int CodEmpresa,
                string Usuario,
                int Consec)
        {
            return _bl
                .CC_ModuloFondos_Autorizar(
                    CodEmpresa,
                    Usuario,
                    Consec);
        }

        [HttpPut("CC_ModuloLiquidacion_Autorizar")]
        public ErrorDto
            CC_ModuloLiquidacion_Autorizar(
                int CodEmpresa,
                string Usuario,
                int Consec)
        {
            return _bl
                .CC_ModuloLiquidacion_Autorizar(
                    CodEmpresa,
                    Usuario,
                    Consec);
        }

        [HttpPut("CC_ModuloBeneficios_Autorizar")]
        public ErrorDto
            CC_ModuloBeneficios_Autorizar(
                int CodEmpresa,
                string Usuario,
                int Consec,
                string Cod_Beneficio)
        {
            return _bl
                .CC_ModuloBeneficios_Autorizar(
                    CodEmpresa,
                    Usuario,
                    Consec,
                    Cod_Beneficio);
        }

        [HttpPut("CC_ModuloHipotecario_Autorizar")]
        public ErrorDto
            CC_ModuloHipotecario_Autorizar(
                int CodEmpresa,
                string Usuario,
                int CodigoDesembolso)
        {
            return _bl
                .CC_ModuloHipotecario_Autorizar(
                    CodEmpresa,
                    Usuario,
                    CodigoDesembolso);
        }
    }
}