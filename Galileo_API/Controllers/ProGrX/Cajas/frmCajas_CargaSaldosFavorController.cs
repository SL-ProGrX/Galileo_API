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
    public class FrmCajasCargaSaldosFavorController : ControllerBase
    {
        private readonly FrmCajasCargaSaldosFavorBL _bl;

        public FrmCajasCargaSaldosFavorController(IConfiguration config)
        {
            _bl = new FrmCajasCargaSaldosFavorBL(config);
        }

        [Authorize]
        [HttpGet("CargaSaldosFavor_Tipos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CargaSaldosFavor_Tipos_Obtener(int codEmpresa)
        {
            return _bl.CargaSaldosFavor_Tipos_Obtener(codEmpresa);
        }

        [Authorize]
        [HttpGet("CargaSaldosFavor_EntidadesPagadoras_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CargaSaldosFavor_EntidadesPagadoras_Obtener(int codEmpresa, bool ordenPorDescripcion = false)
        {
            return _bl.CargaSaldosFavor_EntidadesPagadoras_Obtener(codEmpresa, ordenPorDescripcion);
        }

        [Authorize]
        [HttpGet("CargaSaldosFavor_OrigenRecursos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CargaSaldosFavor_OrigenRecursos_Obtener(int codEmpresa, bool ordenPorDescripcion = false)
        {
            return _bl.CargaSaldosFavor_OrigenRecursos_Obtener(codEmpresa, ordenPorDescripcion);
        }

        [Authorize]
        [HttpPost("CargaSaldosFavor_TipoLiquidacion_Obtener")]
        public ErrorDto<List<CajasSaldoFavorTipoLiquidacionResult>> CargaSaldosFavor_TipoLiquidacion_Obtener(int codEmpresa, [FromBody] CajasSaldoFavorTipoLiquidacionParams param)
        {
            return _bl.CargaSaldosFavor_TipoLiquidacion_Obtener(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("CargaSaldosFavor_Consulta")]
        public ErrorDto<List<CajasSaldosFavorConsultaResult>> CargaSaldosFavor_Consulta([FromBody] CajasSaldosFavorConsultaParams param)
        {
            return _bl.CargaSaldosFavor_Consulta(param);
        }

        [Authorize]
        [HttpPost("CargaSaldosFavor_CuentasBancariasAut_Obtener")]
        public ErrorDto<List<CajasDepositosCuentaBancariaAutResult>> CargaSaldosFavor_CuentasBancariasAut_Obtener(int codEmpresa, [FromBody] CajasDepositosCuentaBancariaAutParams param)
        {
            return _bl.CargaSaldosFavor_CuentasBancariasAut_Obtener(codEmpresa, param);
        }

        [Authorize]
        [HttpPost("Cajas_DepositosTramiteIdentifica_Consulta")]
        public ErrorDto<List<CajasDepositosTramiteIdentificaResult>> Cajas_DepositosTramiteIdentifica_Consulta([FromBody] CajasDepositosTramiteIdentificaParams param)
        {
            return _bl.Cajas_DepositosTramiteIdentifica_Consulta(param);
        }

        [Authorize]
        [HttpGet("CargaSaldosFavor_FormasPago_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CargaSaldosFavor_FormasPago_Obtener(int codEmpresa)
        {
            return _bl.CargaSaldosFavor_FormasPago_Obtener(codEmpresa);
        }

        [Authorize]
        [HttpGet("CargaSaldosFavor_FormaPagoTipo_Obtener")]
        public ErrorDto<CajasFormasPagoTipoResult?> CargaSaldosFavor_FormaPagoTipo_Obtener(int codEmpresa, string codFormaPago)
        {
            return _bl.CargaSaldosFavor_FormaPagoTipo_Obtener(codEmpresa, codFormaPago);
        }

        [Authorize]
        [HttpPost("Cajas_DepositosCargado_Existe")]
        public ErrorDto<CajasDepositosCargadoResult?> Cajas_DepositosCargado_Existe([FromBody] CajasDepositosCargadoParams param)
        {
            return _bl.Cajas_DepositosCargado_Existe(param);
        }

        [Authorize]
        [HttpPost("Cajas_DepositosTramite_Insertar")]
        public ErrorDto<bool> Cajas_DepositosTramite_Insertar([FromBody] CajasDepositosTramiteInsertParams param)
        {
            return _bl.Cajas_DepositosTramite_Insertar(param);
        }

        [Authorize]
        [HttpPost("Cajas_IdentificaTesDepositos")]
        public ErrorDto<bool> Cajas_IdentificaTesDepositos([FromBody] CajasIdentificaTesDepositosParams param)
        {
            return _bl.Cajas_IdentificaTesDepositos(param);
        }

        [Authorize]
        [HttpPost("Cajas_DepositosTramiteInconsistencia_Insertar")]
        public ErrorDto<bool> Cajas_DepositosTramiteInconsistencia_Insertar([FromBody] CajasDepositosTramiteInconsistenciaInsertParams param)
        {
            return _bl.Cajas_DepositosTramiteInconsistencia_Insertar(param);
        }

        [Authorize]
        [HttpPost("Cajas_SaldoFavorCarga")]
        public ErrorDto<bool> Cajas_SaldoFavorCarga([FromBody] CajasSaldoFavorCargaParams param)
        {
            return _bl.Cajas_SaldoFavorCarga(param);
        }

        [Authorize]
        [HttpPost("Cajas_IdentificaTesDepositos_Full")]
        public ErrorDto<bool> Cajas_IdentificaTesDepositos_Full([FromBody] CajasIdentificaTesDepositosFullParams param)
        {
            return _bl.Cajas_IdentificaTesDepositos_Full(param);
        }

        [Authorize]
        [HttpPost("Cajas_NotificaDepositos")]
        public ErrorDto<bool> Cajas_NotificaDepositos([FromBody] CajasNotificaDepositosParams param)
        {
            return _bl.Cajas_NotificaDepositos(param);
        }

        [Authorize]
        [HttpPost("Cajas_SaldoFavorLiquidacion")]
        public ErrorDto<bool> Cajas_SaldoFavorLiquidacion([FromBody] CajasSaldoFavorLiquidacionParams param)
        {
            return _bl.Cajas_SaldoFavorLiquidacion(param);
        }
    }
}