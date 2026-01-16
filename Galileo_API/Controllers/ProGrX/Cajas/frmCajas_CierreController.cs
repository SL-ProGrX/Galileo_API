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
        [HttpGet("CajasCierre_AperturaCarga_Obtener")]
        public ErrorDto<CajasCierreData> CajasCierre_AperturaCarga_Obtener(int CodEmpresa, string Caja, int Apertura)
        {
            return _bl.CajasCierre_AperturaCarga_Obtener(CodEmpresa, Caja, Apertura);
        }

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

        [Authorize]
        [HttpGet("CajasCierre_FPDetalle_Obtener")]
        public ErrorDto<List<CajasCierreFPDetalleData>> CajasCierre_FPDetalle_Obtener(int CodEmpresa, string Caja, int Apertura, string Divisa, string CodFP)
        {
            return _bl.CajasCierre_FPDetalle_Obtener(CodEmpresa, Caja, Apertura, Divisa, CodFP);
        }

        [Authorize]
        [HttpPost("CajasCierre_Deposito_Guardar")]
        public ErrorDto CajasCierre_Deposito_Guardar(int CodEmpresa, CajasCierreDepositoRequest Request)
        {
            return _bl.CajasCierre_Deposito_Guardar(CodEmpresa, Request);
        }

        [Authorize]
        [HttpPost("CajasCierre_Preliminar_Aplicar")]
        public ErrorDto CajasCierre_Preliminar_Aplicar(int CodEmpresa, string Caja, int Apertura, string Usuario)
        {
            return _bl.CajasCierre_Preliminar_Aplicar(CodEmpresa, Caja, Apertura, Usuario);
        }

        [Authorize]
        [HttpPost("CajasCierre_Aplicar")]
        public ErrorDto CajasCierre_Aplicar(int CodEmpresa, string Caja, int Apertura, string Usuario)
        {
            return _bl.CajasCierre_Aplicar(CodEmpresa, Caja, Apertura, Usuario);
        }

        [Authorize]
        [HttpPost("CajasCierre_Denominacion_Registrar")]
        public ErrorDto CajasCierre_Denominacion_Registrar(int CodEmpresa, CajasCierreDenominacionRequest denominacionRequest)
        {
            return _bl.CajasCierre_Denominacion_Registrar(CodEmpresa, denominacionRequest);
        }
    }
}