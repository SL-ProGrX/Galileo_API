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
    public class FrmFndRetirosyLiquidacionesController : ControllerBase
    {
        private readonly FrmFndRetirosyLiquidacionesBL _BL;

        public FrmFndRetirosyLiquidacionesController(IConfiguration config)
        {
            _BL = new FrmFndRetirosyLiquidacionesBL(config);
        }

        [Authorize]
        [HttpGet("FND_RetLiq_SeguridadRango_Obtener")]
        public ErrorDto<FndSeguridadRango> FND_RetLiq_SeguridadRango_Obtener(int CodEmpresa, int Operadora, string Plan, string Usuario)
        {
            return _BL.FND_RetLiq_SeguridadRango_Obtener(CodEmpresa, Operadora, Plan, Usuario);
        }

        [Authorize]
        [HttpGet("FND_RetLiq_Bancos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_RetLiq_Bancos_Obtener(int CodEmpresa, string Usuario)
        {
            return _BL.FND_RetLiq_Bancos_Obtener(CodEmpresa, Usuario);
        }

        [Authorize]
        [HttpGet("FND_RetLiq_CuentasBancarias_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_RetLiq_CuentasBancarias_Obtener(int CodEmpresa, string Cedula, int Banco)
        {
            return _BL.FND_RetLiq_CuentasBancarias_Obtener(CodEmpresa, Cedula, Banco);
        }

        [Authorize]
        [HttpGet("FND_RetLiq_RetencionConceptos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_RetLiq_RetencionConceptos_Obtener(int CodEmpresa, string Usuario)
        {
            return _BL.FND_RetLiq_RetencionConceptos_Obtener(CodEmpresa, Usuario);
        }

        [Authorize]
        [HttpGet("FND_RetLiq_PlanesDestino_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_RetLiq_PlanesDestino_Obtener(int CodEmpresa, int Operadora, string Plan, int Contrato)
        {
            return _BL.FND_RetLiq_PlanesDestino_Obtener(CodEmpresa, Operadora, Plan, Contrato);
        }

        [Authorize]
        [HttpGet("FND_RetLiq_Rebajos_Obtener")]
        public ErrorDto<List<FndRetLiqRebajosData>> FND_RetLiq_Rebajos_Obtener(int CodEmpresa, string Usuario)
        {
            return _BL.FND_RetLiq_Rebajos_Obtener(CodEmpresa, Usuario);
        }

        [Authorize]
        [HttpGet("FND_RetLiq_Consulta_Obtener")]
        public ErrorDto<FndRetLiqConsultaData> FND_RetLiq_Consulta_Obtener(int CodEmpresa, int Operadora, string Plan, int Contrato)
        {
            return _BL.FND_RetLiq_Consulta_Obtener(CodEmpresa, Operadora, Plan, Contrato);
        }

        [Authorize]
        [HttpGet("FND_RetLiq_PagoTerceros_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_RetLiq_PagoTerceros_Obtener(int CodEmpresa, int Operadora, string Plan, int Contrato, string Cedula)
        {
            return _BL.FND_RetLiq_PagoTerceros_Obtener(CodEmpresa, Operadora, Plan, Contrato, Cedula);
        }

        [Authorize]
        [HttpGet("FND_RetLiq_Multa_Obtener")]
        public ErrorDto<decimal> FND_RetLiq_Multa_Obtener(int CodEmpresa, int Operadora, string Plan, int Contrato, decimal Monto)
        {
            return _BL.FND_RetLiq_Multa_Obtener(CodEmpresa, Operadora, Plan, Contrato, Monto);
        }

        [Authorize]
        [HttpGet("FND_RetLiq_RentaGlobal_Obtener")]
        public ErrorDto<FndRetLiqRentaGlobalData> FND_RetLiq_RentaGlobal_Obtener(int CodEmpresa, string Cedula, decimal RndRetiro, string Plan)
        {
            return _BL.FND_RetLiq_RentaGlobal_Obtener(CodEmpresa, Cedula, RndRetiro, Plan);
        }

        [Authorize]
        [HttpGet("FND_RetLiq_Aplicar")]
        public ErrorDto<FndRetLiqProcesoData> FND_RetLiq_Aplicar(int CodEmpresa, string Filtro)
        {
            return _BL.FND_RetLiq_Aplicar(CodEmpresa, Filtro);
        }
    }
}