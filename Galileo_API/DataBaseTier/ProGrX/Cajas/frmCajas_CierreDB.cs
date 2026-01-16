using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cajas;

namespace Galileo_API.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasCierreDb
    {
        private readonly PortalDB _portalDb;

        public FrmCajasCierreDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmCajasCierreDb(PortalDB portalDB) => _portalDb = portalDB;

        public ErrorDto<CajasCierreData> CajasCierre_AperturaCarga_Obtener(int CodEmpresa, string Caja, int Apertura)
        {
            const string sqlCierreTipo = @"select CIERRE_TIPO
                from CAJAS_DEFINICION where COD_CAJA = @Caja;";

            var cierreTipoResult = DbHelper.ExecuteSingleQuery<string>(
                _portalDb, CodEmpresa, sqlCierreTipo, "", new { Caja });

            var cierreTipo = cierreTipoResult.Result;
            var vCierreCiego = string.Equals(cierreTipo, "C", StringComparison.OrdinalIgnoreCase);

            const string sqlApertura = @"
                select *, Case when Estado = 'A' then 'Abierta' else 'Cerrada' end as 'Estado'
                from CAJAS_APERTURAS_MAIN
                where COD_CAJA = @Caja and COD_APERTURA = @Apertura;";

            var aperturaResult = DbHelper.ExecuteSingleQuery<CajasCierreData>(
                _portalDb, 
                CodEmpresa,
                sqlApertura, 
                defaultValue: new CajasCierreData(), 
                parameters: new { Caja, Apertura });

            var resultData = aperturaResult.Result ?? new CajasCierreData();
            resultData.cierre_ciego = vCierreCiego;

            return DbHelper.CreateOkResponse(resultData);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CajasCierre_Divisas_Obtener(int CodEmpresa, int Contabilidad)
        {
            const string sql = @"Select rtrim(cod_divisa) as 'item', rtrim(Descripcion) as 'descripcion' 
                from CNTX_DIVISAS where COD_CONTABILIDAD = @Contabilidad Order by DIVISA_LOCAL desc,COD_DIVISA";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb, CodEmpresa, sql, new { Contabilidad });
        }

        public ErrorDto<List<CajasCierreCuentasData>> CajasCierre_Cuentas_Obtener(int CodEmpresa)
        {
            const string sql = @"exec spCajas_DepositosCuentasBancarias";

            return DbHelper.ExecuteListQuery<CajasCierreCuentasData>(
                _portalDb, CodEmpresa, sql);
        }

        public ErrorDto<List<CajasCierreFormaPagoData>> CajasCierre_FormaPago_Obtener(int CodEmpresa, string Caja, int Apertura, string Divisa)
        {
            const string sql = @"exec spCajas_CierreFPTotal @Caja, @Apertura, @Divisa";

            return DbHelper.ExecuteListQuery<CajasCierreFormaPagoData>(
                _portalDb, CodEmpresa, sql, new {Caja, Apertura, Divisa});
        }

        public ErrorDto<List<CajasCierreDenominacionData>> CajasCierre_Denominacion_Obtener(int CodEmpresa, string Caja, int Apertura, string Divisa, string Tipo)
        {
            const string sql = @"exec spCajas_CierreEFDetalle @Caja, @Apertura, @Divisa, @Tipo";

            return DbHelper.ExecuteListQuery<CajasCierreDenominacionData>(
                _portalDb, CodEmpresa, sql, new { Caja, Apertura, Divisa, Tipo });
        }

        public ErrorDto<List<CajasCierreDepositosData>> CajasCierre_Depositos_Obtener(int CodEmpresa, string Caja, int Apertura, string Divisa)
        {
            const string sql = @"exec spCajas_CierreDepositoDivisa @Caja, @Apertura, @Divisa";

            return DbHelper.ExecuteListQuery<CajasCierreDepositosData>(
                _portalDb, CodEmpresa, sql, new { Caja, Apertura, Divisa });
        }

        public ErrorDto<decimal> CajasCierre_TotalDepositar_Obtener(int CodEmpresa, string Caja, int Apertura, string Divisa)
        {
            const string sql = @"select SI_EFECTIVO From CAJAS_APERTURAS_CIERRES 
                where COD_CAJA = @Caja and COD_APERTURA = @Apertura and COD_DIVISA = @Divisa";

            return DbHelper.ExecuteSingleQuery<decimal>(
                _portalDb, CodEmpresa, sql, 0, new { Caja, Apertura, Divisa });
        }

        public ErrorDto<List<CajasCierreFPDetalleData>> CajasCierre_FPDetalle_Obtener(int CodEmpresa, string Caja, int Apertura, string Divisa, string CodFP)
        {
            const string sql = @"exec spCajas_CierreFPDetalle @Caja, @Apertura, @Divisa, @CodFP";

            return DbHelper.ExecuteListQuery<CajasCierreFPDetalleData>(
                _portalDb, CodEmpresa, sql, new { Caja, Apertura, Divisa, CodFP });
        }

        public ErrorDto CajasCierre_Deposito_Guardar(int CodEmpresa, CajasCierreDepositoRequest request)
        {
            const string sql = @"exec spCajas_CierreRegistraDeposito @Caja, @Apertura, @Divisa, 
                @Monto, @DP_Numero, @DP_Cuenta, @Usuario, @DP_Banco, @Estado";

            return DbHelper.ExecuteNonQuery(
                _portalDb, 
                CodEmpresa, 
                sql, 
                new { 
                    Caja = request.caja, 
                    Apertura = request.apertura, 
                    Divisa = request.divisa,
                    Monto = request.monto,
                    DP_Numero = request.dp_numero,
                    DP_Cuenta = request.dp_cuenta,
                    Usuario = request.usuario,
                    DP_Banco = request.dp_banco,
                    Estado = request.estado
                }
            );
        }

        public ErrorDto CajasCierre_Preliminar_Aplicar(int CodEmpresa, string Caja, int Apertura, string Usuario)
        {
            const string sql = @"exec spCajas_CierreCajaMain @Caja, @Apertura, @Usuario";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                CodEmpresa,
                sql,
                new { Caja, Apertura, Usuario }
            );
        }

        public ErrorDto CajasCierre_Aplicar(int CodEmpresa, string Caja, int Apertura, string Usuario)
        {
            const string sqlValida = @"exec spCajas_Cierre_Validacion @Caja, @Usuario, @Apertura";

            var fxValidaCierreCaja = DbHelper.ExecuteSingleQuery<string>(
                _portalDb,
                CodEmpresa,
                sqlValida,
                "",
                new { Caja, Apertura, Usuario }
            ).Result;

            if (!string.IsNullOrEmpty(fxValidaCierreCaja) && fxValidaCierreCaja.Length > 0)
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = fxValidaCierreCaja
                };
            }

            const string sql = @"exec spCajas_CierreCajaMain @Caja, @Apertura, @Usuario, 0";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                CodEmpresa,
                sql,
                new { Caja, Apertura, Usuario }
            );
        }

        public ErrorDto CajasCierre_Denominacion_Registrar(int CodEmpresa, CajasCierreDenominacionRequest request)
        {
            const string sql = @"exec spCajas_CierreRegistraEFDenominacion @Caja, @Apertura, @Divisa, @Denominacion, @Cantidad, @Tipo";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                CodEmpresa,
                sql,
                new { 
                    Caja = request.caja, 
                    Apertura = request.apertura, 
                    Divisa = request.divisa, 
                    Denominacion = request.denominacion,
                    Cantidad = request.cantidad,
                    Tipo = request.tipo
                }
            );
        }
    }
}
