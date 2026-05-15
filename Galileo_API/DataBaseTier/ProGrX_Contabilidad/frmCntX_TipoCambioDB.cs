using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXTipoCambioDb
    {
        private readonly PortalDB _portalDb;
        private readonly MCntXModuloDb _mCntXModuloDb;

        public FrmCntXTipoCambioDb(IConfiguration config)
            : this(new PortalDB(config), new MCntXModuloDb(config))
        {
        }

        public FrmCntXTipoCambioDb(PortalDB portalDb, MCntXModuloDb mCntXModuloDb)
        {
            _portalDb = portalDb;
            _mCntXModuloDb = mCntXModuloDb;
        }

        /// <summary>
        /// Inicializa la referencia requerida por el dialogo de tipo de cambio.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CntXTipoCambioInicializaData> CntX_TipoCambio_Inicializa(
            int codEmpresa,
            int codConta,
            CntXTipoCambioInicializaRequest request)
        {
            string moneda = (request.moneda ?? string.Empty).Trim().ToUpperInvariant();
            string cuenta = NormalizarCuenta(request.cuenta);
            DateTime fecha = request.fecha == DateTime.MinValue ? DateTime.Today : request.fecha.Date;

            if (string.IsNullOrWhiteSpace(moneda))
            {
                throw new InvalidOperationException("Debe indicar la divisa para consultar el tipo de cambio.");
            }

            if (string.IsNullOrWhiteSpace(cuenta))
            {
                throw new InvalidOperationException("Debe indicar la cuenta para determinar el tipo de cambio permitido.");
            }

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                const string sqlVariacion = @"
                    select top 1 isnull(variacion, 0)
                    from CntX_Divisas_Tipo_Cambio
                    where cod_contabilidad = @CodConta
                      and cod_divisa = @CodDivisa
                      and @Fecha between inicio and corte";

                decimal variacion = conn.ExecuteScalar<decimal?>(
                    sqlVariacion,
                    new
                    {
                        CodConta = codConta,
                        CodDivisa = moneda,
                        Fecha = fecha
                    }) ?? 0m;

                ErrorDto<string> descResp = _mCntXModuloDb.fxCntX_Divisas(codEmpresa, codConta, "D", moneda);
                if (descResp.Code < 0)
                {
                    throw new InvalidOperationException(descResp.Description);
                }

                ErrorDto<decimal> tcResp = _mCntXModuloDb.fxCntX_TipoCambio(codEmpresa, codConta, moneda, cuenta, fecha);
                if (tcResp.Code < 0)
                {
                    throw new InvalidOperationException(tcResp.Description);
                }

                decimal factorActual = (decimal)MProGrxMain.fxSys_Tipo_Cambio_Apl(request.tc_actual);
                decimal montoDivisa = factorActual == 0m ? 0m : request.monto_actual / factorActual;
                decimal tcInicial = request.tc_actual == 0m ? tcResp.Result : request.tc_actual;

                return new CntXTipoCambioInicializaData
                {
                    tc_actual = request.tc_actual,
                    tc_inicial = tcInicial,
                    monto_actual = request.monto_actual,
                    monto_divisa = montoDivisa,
                    monto_funcional = request.monto_actual,
                    tc_permitido = tcResp.Result,
                    variacion = variacion,
                    moneda = moneda,
                    cuenta = cuenta,
                    divisa_descripcion = descResp.Result ?? string.Empty,
                    fecha = fecha
                };
            });
        }

        private static string NormalizarCuenta(string cuenta)
        {
            return (cuenta ?? string.Empty).Trim().Replace("-", string.Empty);
        }
    }
}
