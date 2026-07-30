using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXDivisasDCcB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _mSecurityMainDb;
        private const int ModuloContabilidad = 20;

        public FrmCntXDivisasDCcB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _mSecurityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la lista de divisas disponibles para una empresa específica, excluyendo la divisa local.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa usado para resolver la conexión.</param>
        /// <param name="codContabilidad">Código de la contabilidad activa.</param>
        /// <returns>Lista de divisas foráneas.</returns>
        public ErrorDto<List<DivisaDto>> ObtenerDivisas(int codEmpresa, int codContabilidad)
        {
            const string sql = @"
                     SELECT
                         RTRIM(cod_divisa)   AS cod_divisa,
                         RTRIM(descripcion) AS descripcion
                     FROM CntX_Divisas
                     WHERE cod_contabilidad = @codContabilidad
                       AND divisa_local = 0
                ";

            return DbHelper.ExecuteListQuery<DivisaDto>(
                _portalDB,
                codEmpresa,
                sql,
                new { codContabilidad }
            );
        }

        /// <summary>
        /// Obtiene los tipos de cambio registrados para una divisa específica, empresa, año y mes. Devuelve un máximo de 50 registros ordenados por fecha de corte descendente.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa usado para resolver la conexión.</param>
        /// <param name="codContabilidad">Código de la contabilidad activa.</param>
        /// <param name="anio">Año del período contable.</param>
        /// <param name="mes">Mes del período contable.</param>
        /// <param name="codDivisa">Código de la divisa.</param>
        /// <returns>Hasta 50 tipos de cambio ordenados por corte descendente.</returns>
        public ErrorDto<List<TipoCambioDto>> ObtenerTiposCambio(
            int codEmpresa,
            int codContabilidad,
            int anio,
            int mes,
            string codDivisa)
        {
            const string sql = @"
                    SELECT TOP 50
                        ID_Cambio AS id_cambio,
                        TC_Compra AS tc_compra,
                        TC_Venta  AS tc_venta,
                        Inicio,
                        Corte
                    FROM CntX_Divisas_Tipo_Cambio
                    WHERE cod_divisa = @codDivisa
                      AND cod_contabilidad = @codContabilidad
                      AND DATEPART(MONTH, Corte) = @mes
                      AND DATEPART(YEAR, Corte) = @anio
                    ORDER BY Corte DESC
                ";

            return DbHelper.ExecuteListQuery<TipoCambioDto>(
                _portalDB,
                codEmpresa,
                sql,
                new { codDivisa, codContabilidad, mes, anio }
            );
        }

        /// <summary>
        /// Procesa el diferencial cambiario para una divisa foránea.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa usado para resolver la conexión.</param>
        /// <param name="codContabilidad">Código de la contabilidad activa.</param>
        /// <param name="anio">Año del período contable.</param>
        /// <param name="mes">Mes del período contable.</param>
        /// <param name="codDivisa">Código de la divisa.</param>
        /// <param name="tcCompra">Tipo de cambio de compra.</param>
        /// <param name="tcVenta">Tipo de cambio de venta.</param>
        /// <param name="usuario">Usuario que ejecuta el proceso.</param>
        /// <returns>Resultado del proceso.</returns>
        public ErrorDto Procesar(
            int codEmpresa,
            int codContabilidad,
            int anio,
            int mes,
            string? codDivisa,
            decimal? tcCompra,
            decimal? tcVenta,
            string usuario)
        {
            if (string.IsNullOrWhiteSpace(codDivisa) || tcCompra is null || tcVenta is null)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Especifique un Tipo de Cambio válido para la aplicación del Diferencial."
                };
            }

            const string sqlCuentas = @"
                SELECT
                    RTRIM(ISNULL(cod_cuenta, '')) AS CuentaIngresos,
                    RTRIM(ISNULL(cod_cuenta_gasto, '')) AS CuentaGastos
                FROM CntX_Divisas
                WHERE cod_contabilidad = @codContabilidad
                  AND cod_divisa = @codDivisa";

            var cuentas = DbHelper.ExecuteSingleQuery<DivisaCuentas>(
                _portalDB,
                codEmpresa,
                sqlCuentas,
                default,
                new { codContabilidad, codDivisa }
            );

            if (cuentas.Code < 0)
                return new ErrorDto { Code = cuentas.Code, Description = cuentas.Description };

            if (cuentas.Result is null)
                return new ErrorDto { Code = -1, Description = "La divisa indicada no existe." };

            var validacionIngreso = ValidarCuenta(
                codEmpresa,
                codContabilidad,
                cuentas.Result.CuentaIngresos
            );
            if (validacionIngreso.Code < 0)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "La cuenta de Ingreso por Diferencial Cambiario no es válida, revise la configuración de la divisa."
                };
            }

            var validacionGasto = ValidarCuenta(
                codEmpresa,
                codContabilidad,
                cuentas.Result.CuentaGastos
            );
            if (validacionGasto.Code < 0)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "La cuenta de Gasto por Diferencial Cambiario no es válida, revise la configuración de la divisa."
                };
            }

            const string sqlProcesar = @"
                EXEC spCntX_DiferencialCambiario
                    @Contabilidad = @Contabilidad,
                    @Anio = @Anio,
                    @Mes = @Mes,
                    @Divisa = @Divisa,
                    @TC_Compra = @TcCompra,
                    @TC_Venta = @TcVenta,
                    @Usuario = @Usuario";

            var response = DbHelper.ExecuteNonQuery(
                _portalDB,
                codEmpresa,
                sqlProcesar,
                new
                {
                    Contabilidad = codContabilidad,
                    Anio = anio,
                    Mes = mes,
                    Divisa = codDivisa,
                    TcCompra = tcCompra,
                    TcVenta = tcVenta,
                    Usuario = usuario
                }
            );

            if (response.Code < 0)
                return response;

            _mSecurityMainDb.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                Movimiento = "Aplica",
                Modulo = ModuloContabilidad,
                DetalleMovimiento =
                    $"Asientos-Diferencial Cambiario (Conta.:{codContabilidad} - Periodo.: {anio}-{mes} - Divisa.: {codDivisa})"
            });

            return response;
        }

        /// <summary>
        /// Valida que una cuenta exista y acepte movimientos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa usado para resolver la conexión.</param>
        /// <param name="codContabilidad">Código de la contabilidad activa.</param>
        /// <param name="cuenta">Código de la cuenta contable.</param>
        /// <returns>Resultado correcto cuando la cuenta es válida.</returns>
        private ErrorDto ValidarCuenta(int codEmpresa, int codContabilidad, string cuenta)
        {
            const string sql = @"
                SELECT ISNULL(COUNT(*), 0)
                FROM CntX_Cuentas
                WHERE cod_contabilidad = @codContabilidad
                  AND cod_cuenta = @cuenta
                  AND acepta_movimientos = 1";

            var response = DbHelper.ExecuteSingleQuery<int>(
                _portalDB,
                codEmpresa,
                sql,
                0,
                new { codContabilidad, cuenta }
            );

            return response.Code < 0 || response.Result == 0
                ? new ErrorDto { Code = -1, Description = response.Description }
                : DbHelper.CreateOkResponse();
        }

        private sealed class DivisaCuentas
        {
            public string CuentaIngresos { get; init; } = string.Empty;
            public string CuentaGastos { get; init; } = string.Empty;
        }
    }
}
