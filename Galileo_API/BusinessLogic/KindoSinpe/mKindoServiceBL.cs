using Galileo.Models.KindoSinpe;
using Galileo_API.DataBaseTier;

namespace Galileo_API.BusinessLogic.KindoSinpe
{
    public class MKindoServiceBL
    {
        private readonly MKindoServiceDb _DB;

        public MKindoServiceBL(IConfiguration config)
        {
            _DB = new MKindoServiceDb(config);
        }

        #region Métodos de integración de uso general

        public bool ServicioDisponible(int CodEmpresa)
        {
            return _DB.ServicioDisponible(CodEmpresa);
        }

        public CoreInterno.CuentaIBAN_Response ObtenerCuentaIBAN(int CodEmpresa, CoreInterno.CuentaIBAN_Request DatosCuenta)
        {
            return _DB.ObtenerCuentaIBAN(CodEmpresa, DatosCuenta);
        }

        public CoreInterno.CL_ObtieneInfoCuenta ObtieneInfoCuenta(int CodEmpresa, string? Identificacion, string? CuentaIBAN)
        {
            return _DB.ObtieneInfoCuenta(CodEmpresa, Identificacion, CuentaIBAN);
        }

        public CoreInterno.CL_ValidaCuenta ValidaCuenta(int CodEmpresa, string? Identificacion, string? CuentaIBAN, int? CodigoMoneda)
        {
            return _DB.ValidaCuenta(CodEmpresa, Identificacion, CuentaIBAN, CodigoMoneda);
        }

        public CoreInterno.CL_ResultadoTipoCambio ObtenerTipoCambio(int CodEmpresa, CoreInterno.SI_Rastro? Rastro, int? CodigoServicio, string? Cuentaorigen, string? CuentaDestino, decimal? Monto, int? Moneda)
        {
            return _DB.ObtenerTipoCambio(CodEmpresa, Rastro, CodigoServicio, Cuentaorigen, CuentaDestino, Monto, Moneda);
        }

        public CoreInterno.ComisionRespectivaResponse ComisionRespectiva(int CodEmpresa, CoreInterno.ComisionRespectivaRequest request)
        {
            return _DB.ComisionRespectiva(CodEmpresa,request);
        }

        public CoreInterno.CL_ResultadoValidacion[] ValidaDebitos(int CodEmpresa, ValidaTransRequest request)
        {
            return _DB.ValidaDebitos(CodEmpresa, request);
        }

        public CoreInterno.CL_ResultadoValidacion[] ValidaCreditos(int CodEmpresa, ValidaTransRequest request)
        {
            return _DB.ValidaCreditos(CodEmpresa, request);
        }

        public CoreInterno.ValidacionPerfilTrx_Response ValidarPerfilTransaccional(int CodEmpresa, CoreInterno.ValidacionPerfilTrx_Request transaccion)
        {
            return _DB.ValidarPerfilTransaccional(CodEmpresa, transaccion);
        }

        #endregion

        #region Métodos para la integración transaccional

        public CoreInterno.CL_RespuestaTransaccion[] AplicaDebitosCongelados(int CodEmpresa, CoreInterno.SI_Rastro Rastro, CoreInterno.CL_Transaccion[] Debitos)
        {
            return _DB.AplicaDebitosCongelados(CodEmpresa, Rastro, Debitos);
        }

        public CoreInterno.CL_RespuestaTransaccion[] AplicaCreditosCongelados(int CodEmpresa, CoreInterno.SI_Rastro Rastro, CoreInterno.CL_Transaccion[] Creditos)
        {
            return _DB.AplicaCreditosCongelados(CodEmpresa, Rastro, Creditos);
        }

        public CoreInterno.CL_ResultadoActualizacion[] ConfirmaCreditosCongelados(int CodEmpresa, CoreInterno.SI_Rastro Rastro, CoreInterno.CL_ActualizaTransaccion[] Transacciones)
        {
            return _DB.ConfirmaCreditosCongelados(CodEmpresa, Rastro, Transacciones);
        }

        public CoreInterno.CL_ResultadoActualizacion[] ConfirmaDebitosCongelados(int CodEmpresa, CoreInterno.SI_Rastro Rastro, CoreInterno.CL_ActualizaTransaccion[] Transacciones)
        {
            return _DB.ConfirmaDebitosCongelados(CodEmpresa, Rastro, Transacciones);
        }

        public CoreInterno.CL_ResultadoActualizacion[] ReversaCreditos(int CodEmpresa, CoreInterno.SI_Rastro Rastro, CoreInterno.TransaccionRechazada[] Transacciones)
        {
            return _DB.ReversaCreditos(CodEmpresa, Rastro, Transacciones);
        }

        public CoreInterno.CL_ResultadoActualizacion[] ReversaDebitos(int CodEmpresa, CoreInterno.SI_Rastro Rastro, CoreInterno.TransaccionRechazada[] Transacciones)
        {
            return _DB.ReversaDebitos(CodEmpresa, Rastro, Transacciones);
        }

        public CoreInterno.ObtieneEstadoTransaccionResponse ObtieneEstadoTransaccion(int CodEmpresa, CoreInterno.ObtieneEstadoTransaccionRequest Request)
        {
            return _DB.ObtieneEstadoTransaccion(CodEmpresa, Request);
        }

        #endregion

        #region Métodos para la integración de la liquidación de la cámara

        public static bool ActualizarFechaCiclo(int CodEmpresa, CL_ActualizaFechaRequest request)
        {
            return MKindoServiceDb.ActualizarFechaCiclo(CodEmpresa, request);
        }

        public static bool LiquidarCiclo(int CodEmpresa, CLCierraCiclo request)
        {
            return MKindoServiceDb.LiquidarCiclo(CodEmpresa, request);
        }

        #endregion

        #region Métodos para la integración del PortalCGP

        public CoreInterno.SaldoDisponibleResponse SaldoDisponible(int CodEmpresa, CoreInterno.SaldoDisponibleRequest Request)
        {
            return _DB.SaldoDisponible(CodEmpresa, Request);
        }

        public CoreInterno.ObtenerInformacionClienteResponse ObtenerInformacionCliente(int CodEmpresa, CoreInterno.ObtenerInformacionClienteRequest request)
        {
            return _DB.ObtenerInformacionCliente(CodEmpresa, request);
        }

        public CoreInterno.ObtenerProductosPorClienteResponse ObtenerProductosPorCliente(int CodEmpresa, CoreInterno.ObtenerProductosPorClienteRequest request)
        {
            return _DB.ObtenerProductosPorCliente(CodEmpresa, request);
        }

        #endregion
    }
}
