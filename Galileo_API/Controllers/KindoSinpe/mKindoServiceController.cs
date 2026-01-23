using Galileo.Models.KindoSinpe;
using Microsoft.AspNetCore.Mvc;
using PgxAPI.BusinessLogic.KindoSinpe;

namespace Galileo_API.Controllers.KindoSinpe
{
    [Route("api/[controller]")]
    [ApiController]
    public class MKindoServiceController : ControllerBase
    {
        private readonly MKindoServiceBL _BL;
        public MKindoServiceController(IConfiguration config)
        {
            _BL = new MKindoServiceBL(config);
        }

        #region Métodos de integración de uso general

        [HttpGet("ServicioDisponible/{CodEmpresa}")]
        public bool ServicioDisponible(int CodEmpresa)
        {
            return _BL.ServicioDisponible(CodEmpresa);
        }

        [HttpPost("ObtenerCuentaIBAN/{CodEmpresa}")]
        public CoreInterno.CuentaIBAN_Response ObtenerCuentaIBAN(int CodEmpresa, CoreInterno.CuentaIBAN_Request DatosCuenta)
        {
           return _BL.ObtenerCuentaIBAN(CodEmpresa, DatosCuenta);
        }

        [HttpGet("ObtieneInfoCuenta/{CodEmpresa}")]
        public CoreInterno.CL_ObtieneInfoCuenta ObtieneInfoCuenta(int CodEmpresa, string? Identificacion, string? CuentaIBAN)
        {
            return _BL.ObtieneInfoCuenta(CodEmpresa, Identificacion, CuentaIBAN);
        }

        [HttpGet("ValidaCuenta/{CodEmpresa}")]
        public CoreInterno.CL_ValidaCuenta ValidaCuenta(int CodEmpresa, string? Identificacion, string? CuentaIBAN, int? CodigoMoneda = 1)
        {
            return _BL.ValidaCuenta(CodEmpresa, Identificacion, CuentaIBAN, CodigoMoneda);
        }

        [HttpPost("ObtenerTipoCambio/{CodEmpresa}")]
        public CoreInterno.CL_ResultadoTipoCambio ObtenerTipoCambio(int CodEmpresa, CoreInterno.SI_Rastro? Rastro, int? CodigoServicio, string? Cuentaorigen, string? CuentaDestino, decimal? Monto, int? Moneda)
        {
            return _BL.ObtenerTipoCambio(CodEmpresa, Rastro, CodigoServicio, Cuentaorigen, CuentaDestino, Monto, Moneda);
        }

        [HttpPost("ComisionRespectiva/{CodEmpresa}")]
        public CoreInterno.ComisionRespectivaResponse ComisionRespectiva(int CodEmpresa, CoreInterno.ComisionRespectivaRequest request)
        {
            return _BL.ComisionRespectiva(CodEmpresa, request);
        }

        [HttpPost("ValidaDebitos/{CodEmpresa}")]
        public CoreInterno.CL_ResultadoValidacion[] ValidaDebitos(int CodEmpresa, [FromBody] ValidaTransRequest request)
        {
            return _BL.ValidaDebitos(CodEmpresa, request);
        }

        [HttpPost("ValidaCreditos/{CodEmpresa}")]
        public CoreInterno.CL_ResultadoValidacion[] ValidaCreditos(int CodEmpresa, [FromBody] ValidaTransRequest request)
        {
            return _BL.ValidaCreditos(CodEmpresa, request);
        }

        [HttpPost("ValidarPerfilTransaccional/{CodEmpresa}")]
        public CoreInterno.ValidacionPerfilTrx_Response ValidarPerfilTransaccional(int CodEmpresa, CoreInterno.ValidacionPerfilTrx_Request transaccion)
        {
            return _BL.ValidarPerfilTransaccional(CodEmpresa, transaccion);
        }

        #endregion

        #region Métodos para la integración transaccional

        [HttpPost("AplicaDebitosCongelados/{CodEmpresa}")]
        public CoreInterno.CL_RespuestaTransaccion[] AplicaDebitosCongelados(int CodEmpresa, [FromBody] ValidaTransaccionRequest request)
        {
            return _BL.AplicaDebitosCongelados(CodEmpresa, request.Rastro!, request.Transacciones!);
        }

        [HttpPost("AplicaCreditosCongelados/{CodEmpresa}")]
        public CoreInterno.CL_RespuestaTransaccion[] AplicaCreditosCongelados(int CodEmpresa, [FromBody] ValidaTransaccionRequest request)
        {
            return _BL.AplicaCreditosCongelados(CodEmpresa, request.Rastro!, request.Transacciones!);
        }


        [HttpPost("ConfirmaDebitosCongelados/{CodEmpresa}")]
        public CoreInterno.CL_ResultadoActualizacion[] ConfirmaDebitosCongelados(int CodEmpresa, [FromBody] ValidaActualizaTransaccionRequest request)
        {
            return _BL.ConfirmaDebitosCongelados(CodEmpresa, request.Rastro!, request.Transacciones!);
        }

        [HttpPost("ConfirmaCreditosCongelados/{CodEmpresa}")]
        public CoreInterno.CL_ResultadoActualizacion[] ConfirmaCreditosCongelados(int CodEmpresa, [FromBody] ValidaActualizaTransaccionRequest request)
        {
            return _BL.ConfirmaCreditosCongelados(CodEmpresa, request.Rastro!, request.Transacciones!);
        }

        [HttpPost("ReversaCreditos/{CodEmpresa}")]
        public CoreInterno.CL_ResultadoActualizacion[] ReversaCreditos(int CodEmpresa, ValidaTransaccionRechazoRequest request)
        {
            return _BL.ReversaCreditos(CodEmpresa, request.Rastro!, request.Transacciones!);
        }

        [HttpPost("ReversaDebitos/{CodEmpresa}")]
        public CoreInterno.CL_ResultadoActualizacion[] ReversaDebitos(int CodEmpresa, ValidaTransaccionRechazoRequest request)
        {
            return _BL.ReversaDebitos(CodEmpresa, request.Rastro!, request.Transacciones!);
        }

        [HttpPost("ObtieneEstadoTransaccion/{CodEmpresa}")]
        public CoreInterno.ObtieneEstadoTransaccionResponse ObtieneEstadoTransaccion(int CodEmpresa, CoreInterno.ObtieneEstadoTransaccionRequest Request)
        {
            return _BL.ObtieneEstadoTransaccion(CodEmpresa, Request);
        }


        #endregion

        #region Métodos para la integración de la liquidación de la cámara

        [HttpPost("ActualizarFechaCiclo/{CodEmpresa}")]
        public bool ActualizarFechaCiclo(int CodEmpresa, CL_ActualizaFechaRequest request)
        {
            return _BL.ActualizarFechaCiclo(CodEmpresa,request);
        }

        [HttpPost("LiquidarCiclo/{CodEmpresa}")]
        public bool LiquidarCiclo(int CodEmpresa, CLCierraCiclo request)
        {
            return _BL.LiquidarCiclo(CodEmpresa, request);
        }

        #endregion

        #region Métodos para la integración del PortalCGP

        [HttpPost("SaldoDisponible/{CodEmpresa}")]
        public CoreInterno.SaldoDisponibleResponse SaldoDisponible(int CodEmpresa, CoreInterno.SaldoDisponibleRequest request)
        {
            return _BL.SaldoDisponible(CodEmpresa, request);
        }

        [HttpPost("ObtenerInformacionCliente/{CodEmpresa}")]
        public CoreInterno.ObtenerInformacionClienteResponse ObtenerInformacionCliente(int CodEmpresa, CoreInterno.ObtenerInformacionClienteRequest request)
        {
            return _BL.ObtenerInformacionCliente(CodEmpresa, request);
        }

        [HttpPost("ObtenerProductosPorCliente/{CodEmpresa}")]
        public CoreInterno.ObtenerProductosPorClienteResponse ObtenerProductosPorCliente(int CodEmpresa, CoreInterno.ObtenerProductosPorClienteRequest request)
        {
            return _BL.ObtenerProductosPorCliente(CodEmpresa, request);
        }

        #endregion
    }
}
