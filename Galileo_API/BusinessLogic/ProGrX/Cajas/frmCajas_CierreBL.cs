using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;

namespace Galileo_API.BusinessLogic.ProGrX.Cajas
{
    public class FrmCajasCierreBl
    {
        private readonly FrmCajasCierreDb _db;

        public FrmCajasCierreBl(IConfiguration config) => _db = new FrmCajasCierreDb(config);

        public ErrorDto<CajasCierreData> CajasCierre_AperturaCarga_Obtener(int CodEmpresa, string Caja, int Apertura)
        {
            return _db.CajasCierre_AperturaCarga_Obtener(CodEmpresa, Caja, Apertura);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CajasCierre_Divisas_Obtener(int CodEmpresa, int Contabilidad)
        {
            return _db.CajasCierre_Divisas_Obtener(CodEmpresa, Contabilidad);
        }

        public ErrorDto<List<CajasCierreCuentasData>> CajasCierre_Cuentas_Obtener(int CodEmpresa)
        {
            return _db.CajasCierre_Cuentas_Obtener(CodEmpresa); 
        }

        public ErrorDto<List<CajasCierreFormaPagoData>> CajasCierre_FormaPago_Obtener(int CodEmpresa, string Caja, int Apertura, string Divisa)
        {
            return _db.CajasCierre_FormaPago_Obtener(CodEmpresa, Caja, Apertura, Divisa);
        }

        public ErrorDto<List<CajasCierreDenominacionData>> CajasCierre_Denominacion_Obtener(int CodEmpresa, string Caja, int Apertura, string Divisa, string Tipo)
        {
            return _db.CajasCierre_Denominacion_Obtener(CodEmpresa, Caja, Apertura, Divisa, Tipo);
        }

        public ErrorDto<List<CajasCierreDepositosData>> CajasCierre_Depositos_Obtener(int CodEmpresa, string Caja, int Apertura, string Divisa)
        {
            return _db.CajasCierre_Depositos_Obtener(CodEmpresa, Caja, Apertura, Divisa);
        }

        public ErrorDto<decimal> CajasCierre_TotalDepositar_Obtener(int CodEmpresa, string Caja, int Apertura, string Divisa)
        {
            return _db.CajasCierre_TotalDepositar_Obtener(CodEmpresa, Caja, Apertura, Divisa);
        }

        public ErrorDto<List<CajasCierreFPDetalleData>> CajasCierre_FPDetalle_Obtener(int CodEmpresa, string Caja, int Apertura, string Divisa, string CodFP)
        {
            return _db.CajasCierre_FPDetalle_Obtener(CodEmpresa, Caja, Apertura, Divisa, CodFP);
        }

        public ErrorDto CajasCierre_Deposito_Guardar(int CodEmpresa, CajasCierreDepositoRequest Request)
        {
            return _db.CajasCierre_Deposito_Guardar(CodEmpresa, Request);
        }

        public ErrorDto CajasCierre_Preliminar_Aplicar(int CodEmpresa, string Caja, int Apertura, string Usuario)
        {
            return _db.CajasCierre_Preliminar_Aplicar(CodEmpresa, Caja, Apertura, Usuario);
        }

        public ErrorDto CajasCierre_Aplicar(int CodEmpresa, string Caja, int Apertura, string Usuario)
        {
            return _db.CajasCierre_Aplicar(CodEmpresa, Caja, Apertura, Usuario);
        }

        public ErrorDto CajasCierre_Denominacion_Registrar(int CodEmpresa, CajasCierreDenominacionRequest Request)
        {
            return _db.CajasCierre_Denominacion_Registrar(CodEmpresa, Request);
        }
    }
}
