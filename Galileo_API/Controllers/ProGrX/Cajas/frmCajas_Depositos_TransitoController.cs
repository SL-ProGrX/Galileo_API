using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;

namespace Galileo.Controllers.ProGrX.Cajas
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmCajasDepositosTransitoController : ControllerBase
    {
        private readonly FrmCajasDepositosTransitoBL _bl;
        public FrmCajasDepositosTransitoController(IConfiguration config)
        {
            _bl = new FrmCajasDepositosTransitoBL(config);
        }

        [Authorize]
        [HttpGet("Cajas_DepositosTransito_Cuentas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_DepositosTransito_Cuentas_Obtener(int CodEmpresa)
        {
            return _bl.Cajas_DepositosTransito_Cuentas_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Cajas_Depositos_Transito_Consultar")]
        public ErrorDto<List<CajasDepositosTransitoData>> Cajas_Depositos_Transito_Consultar(int CodEmpresa, [FromQuery] FiltrosData filtros)
        {
            return _bl.Cajas_Depositos_Transito_Consultar(CodEmpresa, filtros);
        }
    }
}