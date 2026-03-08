using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo_API.BusinessLogic.ProGrX.Fondos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndConsultaDetalleController : ControllerBase
    {
        private readonly FrmFndConsultaDetalleBL _BL;

        public FrmFndConsultaDetalleController(IConfiguration? config)
        {
            _BL = new FrmFndConsultaDetalleBL(config);
        }
        [Authorize]
        [HttpGet("FndConsultaDetalle_Obtener")]
        public ErrorDto<FndConsultaDetalleData> FndConsultaDetalle_Obtener(int CodEmpresa, string vCedula, string cod_plan, int cod_contrato)
        {
            return _BL.FndConsultaDetalle_Obtener(CodEmpresa, vCedula, cod_plan, cod_contrato);
        }

        [Authorize]
        [HttpGet("FndConsultaContratos_Obtener")]
        public ErrorDto<List<FndConsultaContratoDetallesData>> FndConsultaContratos_Obtener(int CodEmpresa, string vCedula, string cod_plan, int cod_contrato)
        {
            return _BL.FndConsultaContratos_Obtener(CodEmpresa, vCedula, cod_plan, cod_contrato);
        }

        [Authorize]
        [HttpGet("FndConsultaSubCuentas_Obtener")]
        public ErrorDto<List<FndConsultaSubCuentasData>> FndConsultaSubCuentas_Obtener(int CodEmpresa, string vCedula, string cod_plan, int cod_contrato, string subCuenta)
        {
            return _BL.FndConsultaSubCuentas_Obtener(CodEmpresa, vCedula, cod_plan, cod_contrato, subCuenta);
        }

        [Authorize]
        [HttpGet("FndConsultaSubCuentasDetalle_Obtener")]
        public ErrorDto<List<FndConsultaSubCuentasDetalleData>> FndConsultaSubCuentasDetalle_Obtener(int CodEmpresa, string vCedula, string cod_plan, int cod_contrato, string subCuenta)
        {
            return _BL.FndConsultaSubCuentasDetalle_Obtener(CodEmpresa, vCedula, cod_plan, cod_contrato, subCuenta);
        }

        [Authorize]
        [HttpGet("FndConsultaContratosBeneficiario_Obtener")]
        public ErrorDto<List<FndConsultaBeneficiarioDetalle>> FndConsultaContratosBeneficiario_Obtener(int CodEmpresa, string vCedula, string cod_plan, int cod_contrato)
        {
            return _BL.FndConsultaContratosBeneficiario_Obtener(CodEmpresa, vCedula, cod_plan, cod_contrato);
        }

        [Authorize]
        [HttpGet("FndConsultaMovTransito_Obtener")]
        public ErrorDto<List<FndConsultaMovTransitoData>> FndConsultaMovTransito_Obtener(int CodEmpresa, string cuenta)
        {
            return _BL.FndConsultaMovTransito_Obtener(CodEmpresa, cuenta);
        }
    }
}
