using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cajas;

namespace Galileo_API.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasCierreDb
    {
        private readonly PortalDB _portalDB;

        public FrmCajasCierreDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmCajasCierreDb(PortalDB portalDB)
        {
            _portalDB = portalDB;
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CajasCierre_Divisas_Obtener(int CodEmpresa, int Contabilidad)
        {
            const string sql = @"Select rtrim(cod_divisa) as 'item', rtrim(Descripcion) as 'descripcion' 
                from CNTX_DIVISAS where COD_CONTABILIDAD = @Contabilidad Order by DIVISA_LOCAL desc,COD_DIVISA";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDB, CodEmpresa, sql, new { Contabilidad });
        }

        public ErrorDto<List<CajasCierreCuentasData>> CajasCierre_Cuentas_Obtener(int CodEmpresa)
        {
            const string sql = @"exec spCajas_DepositosCuentasBancarias";

            return DbHelper.ExecuteListQuery<CajasCierreCuentasData>(
                _portalDB, CodEmpresa, sql);
        }

        public ErrorDto<List<CajasCierreFormaPagoData>> CajasCierre_FormaPago_Obtener(int CodEmpresa, string Caja, int Apertura, string Divisa)
        {
            const string sql = @"exec spCajas_CierreFPTotal @Caja, @Apertura, @Divisa";

            return DbHelper.ExecuteListQuery<CajasCierreFormaPagoData>(
                _portalDB, CodEmpresa, sql, new {Caja, Apertura, Divisa});
        }

        public ErrorDto<List<CajasCierreDenominacionData>> CajasCierre_Denominacion_Obtener(int CodEmpresa, string Caja, int Apertura, string Divisa, string Tipo)
        {
            const string sql = @"exec spCajas_CierreEFDetalle @Caja, @Apertura, @Divisa, @Tipo";

            return DbHelper.ExecuteListQuery<CajasCierreDenominacionData>(
                _portalDB, CodEmpresa, sql, new { Caja, Apertura, Divisa, Tipo });
        }

        public ErrorDto<List<CajasCierreDepositosData>> CajasCierre_Depositos_Obtener(int CodEmpresa, string Caja, int Apertura, string Divisa)
        {
            const string sql = @"exec spCajas_CierreDepositoDivisa @Caja, @Apertura, @Divisa";

            return DbHelper.ExecuteListQuery<CajasCierreDepositosData>(
                _portalDB, CodEmpresa, sql, new { Caja, Apertura, Divisa });
        }

        public ErrorDto<decimal> CajasCierre_TotalDepositar_Obtener(int CodEmpresa, string Caja, int Apertura, string Divisa)
        {
            const string sql = @"select SI_EFECTIVO From CAJAS_APERTURAS_CIERRES 
                where COD_CAJA = @Caja and COD_APERTURA = @Apertura and COD_DIVISA = @Divisa";

            return DbHelper.ExecuteSingleQuery<decimal>(
                _portalDB, CodEmpresa, sql, 0, new { Caja, Apertura, Divisa });
        }
    }
}
