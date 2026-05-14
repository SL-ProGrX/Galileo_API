using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndCalculadoraInversionesController : ControllerBase
    {
        private readonly FrmFndCalculadoraInversionesBL _BL;

        public FrmFndCalculadoraInversionesController(IConfiguration config)
        {
            _BL = new FrmFndCalculadoraInversionesBL(config);
        }

        [Authorize]
        [HttpGet("Fnd_Calculadora_Planes_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Calculadora_Planes_Obtener(int CodEmpresa, int TipoInv)
        {
            return _BL.Fnd_Calculadora_Planes_Obtener(CodEmpresa, TipoInv);
        }

        [Authorize]
        [HttpGet("Fnd_Calculadora_ConsultaPlan_Obtener")]
        public ErrorDto<FndCalculadoraPlanes> Fnd_Calculadora_ConsultaPlan_Obtener(int CodEmpresa, string CodPlan)
        {
            return _BL.Fnd_Calculadora_ConsultaPlan_Obtener(CodEmpresa, CodPlan);
        }

        [Authorize]
        [HttpGet("Fnd_Calculadora_PlazosInv_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Calculadora_PlazosInv_Obtener(int CodEmpresa, string CodPlan)
        {
            return _BL.Fnd_Calculadora_PlazosInv_Obtener(CodEmpresa, CodPlan);
        }

        [Authorize]
        [HttpGet("Fnd_Calculadora_Cupones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Calculadora_Cupones_Obtener(int CodEmpresa, int Plazo, string CodPlan)
        {
            return _BL.Fnd_Calculadora_Cupones_Obtener(CodEmpresa, Plazo, CodPlan);
        }

        [Authorize]
        [HttpGet("Fnd_Calculadora_PlazosDias_Obtener")]
        public ErrorDto<int> Fnd_Calculadora_PlazosDias_Obtener(int CodEmpresa, int Plazo, string CodPlan)
        {
            return _BL.Fnd_Calculadora_PlazosDias_Obtener(CodEmpresa, Plazo, CodPlan);
        }

        [Authorize]
        [HttpGet("Fnd_Calculadora_TasaRef_Obtener")]
        public ErrorDto<decimal> Fnd_Calculadora_TasaRef_Obtener(int CodEmpresa, int PlazoDias, string Tipo, string Plan, int Operadora, bool chkCupon, int rpTipo, int PlazoInv, int? CuponId)
        {
            return _BL.Fnd_Calculadora_TasaRef_Obtener(CodEmpresa, PlazoDias, Tipo, Plan, Operadora, chkCupon, rpTipo, PlazoInv, CuponId);
        }

        [Authorize]
        [HttpGet("Fnd_Calculadora_Inversiones_Calcular")]
        public ErrorDto<List<FndCalculadoraInversionesFlujoData>> Fnd_Calculadora_Inversiones_Calcular(int CodEmpresa, string FiltrosCalculadora)
        {
            return _BL.Fnd_Calculadora_Inversiones_Calcular(CodEmpresa, FiltrosCalculadora);
        }

        [Authorize]
        [HttpPost("Fnd_Calculadora_Inversiones_EmailEnviar")]
        public ErrorDto Fnd_Calculadora_Inversiones_EmailEnviar(int CodEmpresa, int CalculoId, string Usuario)
        {
            return _BL.Fnd_Calculadora_Inversiones_EmailEnviar(CodEmpresa, CalculoId, Usuario);
        }
    }
}