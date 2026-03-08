using Galileo.Models;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PgxAPI.BusinessLogic.ProGrX.Fondos;
using PgxAPI.Models.ProGrX.Fondos;


namespace Galileo_API.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndLiquidacionPlanController : ControllerBase
    {
        private readonly FrmFndLiquidacionPlanBL _bl;
        public FrmFndLiquidacionPlanController(IConfiguration config)
        {
            _bl = new FrmFndLiquidacionPlanBL(config);
        }

        [Authorize]
        [HttpGet("FND_LiquidacionPlan_Listas")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_LiquidacionPlan_Listas(int CodEmpresa, string usuario, string catalogo)
        {
            return _bl.FND_LiquidacionPlan_Listas(CodEmpresa, usuario, catalogo);
        }

        [Authorize]
        [HttpGet("FND_LiquidacionPlan_buscar")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_LiquidacionPlan_buscar(int CodEmpresa, int codOperadora)
        {
            return _bl.FND_LiquidacionPlan_buscar(CodEmpresa, codOperadora);
        }

        [Authorize]
        [HttpGet("FND_LiquidacionPlan_Operadora_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_LiquidacionPlan_Operadora_Obtener(int CodEmpresa)
        {
            return _bl.FND_LiquidacionPlan_Operadora_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("FND_LiquidacionPlanContartos_Buscar")]
        public ErrorDto<List<FndConsultaPlanRowDto>> FND_LiquidacionPlanContartos_Buscar(
          int CodEmpresa,
          FndLiquidacionPlanFiltrosData filtro)
        {
            return _bl.FND_LiquidacionPlanContartos_Buscar(CodEmpresa, filtro);
        }


        //[Authorize]
        //[HttpGet("FND_LiquidacionPlan_ObtenerDetalle")]
        //public ErrorDto<List<FndPlanDetalleData>> FND_LiquidacionPlan_ObtenerDetalle(int CodEmpresa, int codOperadora, string codPlan)
        //{
        //    return _bl.FND_LiquidacionPlan_ObtenerDetalle(CodEmpresa, codOperadora, codPlan);
        //}

        //[Authorize]
        //[HttpGet("FND_LiquidacionPlan_ObtenerUno")]
        //public ErrorDto<DropDownListaGenericaModel?> FND_LiquidacionPlan_ObtenerUno(int CodEmpresa, int codOperadora)
        //{
        //    return _bl.FND_LiquidacionPlan_ObtenerUno(CodEmpresa, codOperadora);
        //}

        //[Authorize]
        //[HttpGet("FND_LiquidacionPlanCatalogo_Obtener")]
        //public ErrorDto<List<DropDownListaGenericaModel>> FND_LiquidacionPlanCatalogo_Obtener(int CodEmpresa)
        //{
        //    return _bl.FND_LiquidacionPlanCatalogo_Obtener(CodEmpresa);
        //}

        //[Authorize]
        //[HttpPost("SbDocumentoMaestro_Guardar")]
        //public ErrorDto SbDocumentoMaestro_Guardar(int CodEmpresa, string usuario, FndLiquidacionPlanData data)
        //{
        //    return _bl.SbDocumentoMaestro_Guardar(CodEmpresa, usuario, data);
        //}

        //[Authorize]
        //[HttpPost("FND_ConsultaPlan")]
        //public ErrorDto<List<FndConsultaPlanRow>> FND_ConsultaPlan(int CodEmpresa, FndConsultaPlanFiltro f)
        //{
        //    return _bl.FND_ConsultaPlan(CodEmpresa, f);
        //}

        //[Authorize]
        //[HttpGet("FND_GrupoBancario_Obtener")]
        //public ErrorDto<string> FND_GrupoBancario_Obtener(int CodEmpresa, int bancoId)
        //{
        //    return _bl.FND_GrupoBancario_Obtener(CodEmpresa, bancoId);
        //}

        //[Authorize]
        //[HttpGet("FND_CuentaPlan_Obtener")]
        //public ErrorDto<string> FND_CuentaPlan_Obtener(int CodEmpresa, string tipo, int codOperadora, string codPlan)
        //{
        //    return _bl.FND_CuentaPlan_Obtener(CodEmpresa, tipo, codOperadora, codPlan);
        //}

        //[Authorize]
        //[HttpGet("FND_CuentaRetencion_Obtener")]
        //public ErrorDto<string> FND_CuentaRetencion_Obtener(int CodEmpresa, string retencionCodigo)
        //{
        //    return _bl.FND_CuentaRetencion_Obtener(CodEmpresa, retencionCodigo);
        //}

        //[Authorize]
        //[HttpGet("fxCuentaRetiros")]
        //public ErrorDto<string> fxCuentaRetiros(int CodEmpresa, int codOperadora)
        //{
        //    return _bl.fxCuentaRetiros(CodEmpresa, codOperadora);
        //}

        //[Authorize]
        //[HttpGet("sbDocumento")]
        //public ErrorDto<bool> sbDocumento(int CodEmpresa, string vTipoDoc, string vDocRef, DateTime vFecha, string vConcepto, int codOperadora, string codPlan, string descripcionPlan)
        //{
        //    return _bl.sbDocumento(CodEmpresa, vTipoDoc, vDocRef, vFecha, vConcepto, codOperadora, codPlan, descripcionPlan);
        //}

    }
}
