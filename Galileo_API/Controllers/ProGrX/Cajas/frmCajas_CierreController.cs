using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cajas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCajasCierreController : ControllerBase
    {
        private readonly FrmCajasCierreBl _bl;

        public FrmCajasCierreController(IConfiguration config) => _bl = new FrmCajasCierreBl(config);

        [Authorize]
        [HttpGet("CajasCierre_Divisas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CajasCierre_Divisas_Obtener(int CodEmpresa, int Contabilidad)
        {
            return _bl.CajasCierre_Divisas_Obtener(CodEmpresa, Contabilidad);
        }

        [Authorize]
        [HttpGet("CajasCierre_Cuentas_Obtener")]
        public ErrorDto<List<CajasCierreCuentasData>> CajasCierre_Cuentas_Obtener(int CodEmpresa)
        {
            return _bl.CajasCierre_Cuentas_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CajasCierre_FormaPago_Obtener")]
        public ErrorDto<List<CajasCierreFormaPagoData>> CajasCierre_FormaPago_Obtener(int CodEmpresa, string Caja, int Apertura, string Divisa)
        {
            return _bl.CajasCierre_FormaPago_Obtener(CodEmpresa, Caja, Apertura, Divisa);
        }

        [Authorize]
        [HttpGet("CajasCierre_Denominacion_Obtener")]
        public ErrorDto<List<CajasCierreDenominacionData>> CajasCierre_Denominacion_Obtener(int CodEmpresa, string Caja, int Apertura, string Divisa, string Tipo)
        {
            return _bl.CajasCierre_Denominacion_Obtener(CodEmpresa, Caja, Apertura, Divisa, Tipo);
        }

        [Authorize]
        [HttpGet("CajasCierre_Depositos_Obtener")]
        public ErrorDto<List<CajasCierreDepositosData>> CajasCierre_Depositos_Obtener(int CodEmpresa, string Caja, int Apertura, string Divisa)
        {
            return _bl.CajasCierre_Depositos_Obtener(CodEmpresa, Caja, Apertura, Divisa);
        }

        [Authorize]
        [HttpGet("CajasCierre_TotalDepositar_Obtener")]
        public ErrorDto<decimal> CajasCierre_TotalDepositar_Obtener(int CodEmpresa, string Caja, int Apertura, string Divisa)
        {
            return _bl.CajasCierre_TotalDepositar_Obtener(CodEmpresa, Caja, Apertura, Divisa);
        }


    }
}