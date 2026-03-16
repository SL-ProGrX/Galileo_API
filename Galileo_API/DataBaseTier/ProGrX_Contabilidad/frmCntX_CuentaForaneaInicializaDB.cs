using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXCuentaForaneaInicializaDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _mSecurityMainDb;
        private readonly MCntLinkDB _mCntLinkDb;
        private readonly int vModulo = 20;

        public FrmCntXCuentaForaneaInicializaDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config), new MCntLinkDB(config)) { }
        public FrmCntXCuentaForaneaInicializaDb(PortalDB portalDb, MSecurityMainDb mProGrxMain, MCntLinkDB mCntLinkDb)
        {
            _portalDb = portalDb;
            _mSecurityMainDb = mProGrxMain;
            _mCntLinkDb = mCntLinkDb;
        }

        /// <summary>
        /// Obtiene la divisa local funcional para la contabilidad seleccionada
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <returns></returns>
        public ErrorDto<string?> CntXDivisaLocal_Obtener(int codEmpresa, int codConta)
        {
            string query = @"select cod_divisa from CntX_Divisas 
                Where DIVISA_LOCAL = 1 and COD_CONTABILIDAD = @codConta";

            return DbHelper.ExecuteSingleQuery<string>(
                _portalDb, codEmpresa, query, null,
                new { codConta });
        }

        /// <summary>
        /// Obtiene las cuentas foraneas para la contabilidad y divisa seleccionadas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="codDivisa"></param>
        /// <returns></returns>
        public ErrorDto<List<CntXCuentaForaneaData>> CntXCuentaForaneas_Obtener(int codEmpresa, int codConta, string codDivisa)
        {
            var sql = @"select cod_cuenta_mask, descripcion, cod_divisa from CntX_Cuentas 
                where cod_contabilidad = @codConta 
                and cod_divisa <> @codDivisa and Acepta_Movimientos = 1";
            return DbHelper.ExecuteListQuery<CntXCuentaForaneaData>(
                _portalDb, codEmpresa, sql, 
                new { codConta, codDivisa });
        }

        /// <summary>
        /// Obtiene los saldos de la cuenta foranea seleccionada
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConta"></param>
        /// <param name="codCuenta"></param>
        /// <param name="anio"></param>
        /// <param name="mes"></param>
        /// <returns></returns>
        public ErrorDto<CntXCuentaMovSaldoData?> CntXCuentaMovSaldo_Obtener(int codEmpresa, int codConta, string codCuenta, int anio, int mes)
        {
            codCuenta = _mCntLinkDb.fxgCntCuentaFormato(codEmpresa, false, codCuenta, 0);

            var sql = @"select Saldo_Inicial, DF_Saldo_Inicial 
                From vCntX_Mov_Cuentas_General 
                Where cod_contabilidad = @codConta    
                and cod_cuenta = @codCuenta 
                and anio = @anio and mes = @mes";
            return DbHelper.ExecuteSingleQuery<CntXCuentaMovSaldoData>(
                _portalDb, codEmpresa, sql, null, 
                new { codConta, codCuenta, anio, mes });
        }

        /// <summary>
        /// Inicializa la cuenta foranea 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CntXCuentaForanea_Inicializar(int codEmpresa, CntXCuentaForaneaInicializaRequest request)
        {
            string query = @"exec spCntX_Cuenta_Foranea_Inicializa @CodConta, '', @CodCuenta, @SaldoInicial, @Anio, @Mes, @Usuario";

            var resp = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    CodConta = request.cod_contabilidad,
                    CodCuenta = request.cod_cuenta,
                    SaldoInicial = request.saldo_inicial,
                    Anio = request.anio,
                    Mes = request.mes,
                    Usuario = request.usuario
                });

            if (resp.Code < 0)
                return resp;

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                movimiento: "Inicializa - WEB",
                detalle: $"Saldo Divisa Extranjera, Cuenta: {request.cod_cuenta}, Saldo Inicial: {request.saldo_inicial}"
            );

            return resp;
        }

        /// <summary>
        /// Registrar en bitacora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="movimiento"></param>
        /// <param name="detalle"></param>
        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _mSecurityMainDb.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}
