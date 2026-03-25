using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Credito;
using Galileo.Models.ProGrX.Fondos;
using Galileo_API.BusinessLogic.ProGrX.Fondos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndConsultaContratosController : ControllerBase
    {
        private readonly FrmFndConsultaContratosBL _BL;

        public FrmFndConsultaContratosController(IConfiguration? config)
        {
            _BL = new FrmFndConsultaContratosBL(config);
        }

        [Authorize]
        [HttpGet("FND_ConsultaContratosSocios_Obtener")]
        public ErrorDto<List<CrConsultaCrdSociosData>> FND_ConsultaContratosSocios_Obtener(int CodEmpresa)
        {
            return _BL.FND_ConsultaContratosSocios_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("FND_ConsultaContratos_Contratos_Obtener")]
        public ErrorDto<List<FndConsultaContratosData>> FND_ConsultaContratos_Contratos_Obtener(int CodEmpresa, string vCedula, string vUsuario, string opcion)
        {
            return _BL.FND_ConsultaContratos_Contratos_Obtener(CodEmpresa, vCedula, vUsuario, opcion);
        }

        [Authorize]
        [HttpGet("FND_ConsultaContratos_SubCuentas_Obtener")]
        public ErrorDto<List<FndConsultaSubContratosData>> FND_ConsultaContratos_SubCuentas_Obtener(int CodEmpresa, string vCedula, string cod_plan, string cod_contrato)
        {
            return _BL.FND_ConsultaContratos_SubCuentas_Obtener(CodEmpresa, vCedula, cod_plan, cod_contrato);
        }

        [Authorize]
        [HttpGet("FND_ConsultaContratos_Liquidaciones_Obtener")]
        public ErrorDto<List<FndConsultaLiquidacionesData>> FND_ConsultaContratos_Liquidaciones_Obtener(int CodEmpresa, string vCedula)
        {
            return _BL.FND_ConsultaContratos_Liquidaciones_Obtener(CodEmpresa, vCedula);
        }

        [Authorize]
        [HttpGet("FND_ConsultaContratos_Movimiento_Obtener")]
        public ErrorDto<List<FndConsultaMovimientosData>> FND_ConsultaContratos_Movimiento_Obtener(
            int CodEmpresa,
            string vCedula,
            string filtros)
        {
            return _BL.FND_ConsultaContratos_Movimiento_Obtener(CodEmpresa, vCedula, filtros);
        }

        [Authorize]
        [HttpGet("FND_ConsultaContratos_Planes_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_ConsultaContratos_Planes_Obtener(int CodEmpresa, string vCedula)
        {
            return _BL.FND_ConsultaContratos_Planes_Obtener(CodEmpresa, vCedula);
        }

        [Authorize]
        [HttpPost("FND_ConsultaContratos_Reversar")]
        public ErrorDto FND_ConsultaContratos_Reversar(int CodEmpresa, string usuario, string boleta)
        {
            return _BL.FND_ConsultaContratos_Reversar(CodEmpresa, usuario, boleta);
        }

    }
}
