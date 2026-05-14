using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndCierreMensualFndDB
    {
        private readonly IConfiguration _config;

        private const string SqlMovimientosDocumentos = @"
                    SELECT
                        A.fnd_cuenta,
                        A.fnd_debehaber,
                        ISNULL(SUM(A.fnd_monto), 0) AS Movimiento
                    FROM dbo.fnd_documentos D
                    INNER JOIN dbo.fnd_asientos A
                        ON D.tipo = A.tipo
                        AND D.id_documento = A.id_documento
                        AND D.cod_operadora = A.cod_operadora
                    WHERE DATEPART(yyyy, D.fecha) = @anio
                      AND DATEPART(mm, D.fecha) = @mes
                    GROUP BY A.fnd_cuenta, A.fnd_debehaber;";

        private const string SqlMovimientosCola = @"
                    SELECT
                        A.fnd_cuenta,
                        A.fnd_debehaber,
                        ISNULL(SUM(A.fnd_monto), 0) AS Movimiento
                    FROM dbo.fnd_asientos_cola A
                    WHERE DATEPART(yyyy, A.fnd_fecha) = @anio
                      AND DATEPART(mm, A.fnd_fecha) = @mes
                    GROUP BY A.fnd_cuenta, A.fnd_debehaber;";

        public FrmFndCierreMensualFndDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// PASOS
        ///'1. Guardar El Estado Actual de los Creditos
        ///'   saldo_inicial,saldo,proceso,opex,id_solicitud,codigo
        ///'2. Actualizar Total_debitos y Total_creditos con los
        ///'   movimientos del mes
        ///'3. Establecer Nuevo Corte de Saldos.
        ///'4. Insertar en Historicos, el periodo procesado.
        ///'5. Crear Referencia Contable (Metodo Contable)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto Fnd_CierreMensual_Aplicar(int CodEmpresa)
        {
            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
            {
                var periodo = CrearPeriodoCierre(DateTime.Now);
                var validacion = ValidarPeriodoCierre(connection, periodo);
                if (validacion.Code != 0)
                {
                    return validacion;
                }

                PrepararCierre(connection, periodo);
                var movimientos = SbMovCuentas(connection, periodo.Anio, periodo.Mes);
                return movimientos.Code == 0
                    ? DbHelper.OkResponse("Cierre concluido satisfactoriamente.")
                    : movimientos;
            });

            return result.Code == 0
                ? result.Result ?? DbHelper.ErrorResponse("No se obtuvo resultado del cierre.")
                : DbHelper.ErrorResponse(result.Description ?? "Error al aplicar cierre mensual.", result.Code ?? -1);
        }

        /// <summary>
        /// Metodo para procesar los movimientos de las cuentas
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="anio"></param>
        /// <param name="mes"></param>
        /// <returns></returns>
        private ErrorDto SbMovCuentas(SqlConnection connection, int anio, int mes)
        {
            try
            {
                ProcesarMovimientos(connection, anio, mes, SqlMovimientosDocumentos);
                ProcesarMovimientos(connection, anio, mes, SqlMovimientosCola);
                ActualizarClasificacionCuentas(connection, anio, mes);
                ActualizarSaldosFinales(connection, anio, mes);
                CrearPeriodoSiguiente(connection, anio, mes);

                return DbHelper.OkResponse("Ok");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Metodo auxiliar para verificar si existe la cuenta en el periodo
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="mes"></param>
        /// <param name="anio"></param>
        /// <param name="cuenta"></param>
        /// <returns></returns>
        private ErrorDto<bool> FxExisteCuenta(SqlConnection connection, int mes, int anio, string cuenta)
        {
            try
            {
                const string sql = @"
                    SELECT ISNULL(COUNT(1), 0)
                    FROM dbo.fnd_per_cuentas
                    WHERE anio = @anio
                      AND mes = @mes
                      AND cod_cuenta = @cuenta;";

                var existe = connection.ExecuteScalar<int>(sql, new { anio, mes, cuenta = NormalizarTexto(cuenta) }) > 0;
                return DbHelper.CreateOkResponse(existe);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, false);
            }
        }


        // === Private Helper Methods for Cierre Mensual Logic ===

        private static CierrePeriodo CrearPeriodoCierre(DateTime fecha) => new(fecha.Year, fecha.Month);

        private static ErrorDto ValidarPeriodoCierre(SqlConnection connection, CierrePeriodo periodo)
        {
            const string sql = @"
                    SELECT ISNULL(COUNT(1), 0)
                    FROM dbo.fnd_per_historico
                    WHERE anio = @anio
                      AND mes = @mes;";

            var existe = connection.ExecuteScalar<int>(sql, new { anio = periodo.Anio, mes = periodo.Mes });
            return existe > 0
                ? DbHelper.ErrorResponse("El procedimiento de cierre ya fue ejecutado para este mes.")
                : DbHelper.OkResponse("Ok");
        }

        private static void PrepararCierre(SqlConnection connection, CierrePeriodo periodo)
        {
            connection.Execute(
                "DELETE dbo.FND_per_cerrados WHERE anio = @anio AND mes = @mes;",
                new { anio = periodo.Anio, mes = periodo.Mes });

            const string insertEstado = @"
                    INSERT INTO dbo.FND_per_cerrados
                    (
                        anio,
                        mes,
                        cod_operadora,
                        cod_plan,
                        cod_contrato,
                        aportes,
                        rendimientos,
                        estado
                    )
                    SELECT
                        @anio,
                        @mes,
                        cod_operadora,
                        cod_plan,
                        cod_contrato,
                        aportes,
                        rendimiento,
                        estado
                    FROM dbo.fnd_contratos;";

            connection.Execute(insertEstado, new { anio = periodo.Anio, mes = periodo.Mes });

            connection.Execute(
                "INSERT INTO dbo.fnd_per_historico(anio, mes) VALUES(@anio, @mes);",
                new { anio = periodo.Anio, mes = periodo.Mes });
        }


        private void ProcesarMovimientos(SqlConnection connection, int anio, int mes, string sqlMovimientos)
        {
            foreach (var mov in connection.Query(sqlMovimientos, new { anio, mes }))
            {
                var movimiento = CrearMovimientoCuenta(mov);
                AplicarMovimientoCuenta(connection, anio, mes, movimiento);
            }
        }

        private static MovimientoCuenta CrearMovimientoCuenta(dynamic mov)
        {
            return new MovimientoCuenta(
                Convert.ToString(mov.fnd_cuenta) ?? string.Empty,
                Convert.ToString(mov.fnd_debehaber) ?? string.Empty,
                Convert.ToDecimal(mov.Movimiento));
        }

        private void AplicarMovimientoCuenta(SqlConnection connection, int anio, int mes, MovimientoCuenta movimiento)
        {
            var existe = FxExisteCuenta(connection, mes, anio, movimiento.Cuenta);
            if (existe.Code != 0)
            {
                throw new InvalidOperationException(existe.Description);
            }

            if (existe.Result)
            {
                ActualizarMovimientoCuenta(connection, anio, mes, movimiento);
                return;
            }

            InsertarMovimientoCuenta(connection, anio, mes, movimiento);
        }

        private static void ActualizarMovimientoCuenta(SqlConnection connection, int anio, int mes, MovimientoCuenta movimiento)
        {
            if (EsDebito(movimiento.DebeHaber))
            {
                ActualizarDebitoCuenta(connection, anio, mes, movimiento);
                return;
            }

            ActualizarCreditoCuenta(connection, anio, mes, movimiento);
        }

        private static void ActualizarDebitoCuenta(SqlConnection connection, int anio, int mes, MovimientoCuenta movimiento)
        {
            const string sql = @"
                    UPDATE dbo.fnd_per_cuentas
                    SET total_debitos = total_debitos + @monto
                    WHERE anio = @anio
                      AND mes = @mes
                      AND cod_cuenta = @cuenta;";

            connection.Execute(sql, new { monto = movimiento.Monto, anio, mes, cuenta = movimiento.Cuenta });
        }

        private static void ActualizarCreditoCuenta(SqlConnection connection, int anio, int mes, MovimientoCuenta movimiento)
        {
            const string sql = @"
                    UPDATE dbo.fnd_per_cuentas
                    SET total_creditos = total_creditos + @monto
                    WHERE anio = @anio
                      AND mes = @mes
                      AND cod_cuenta = @cuenta;";

            connection.Execute(sql, new { monto = movimiento.Monto, anio, mes, cuenta = movimiento.Cuenta });
        }

        private static void InsertarMovimientoCuenta(SqlConnection connection, int anio, int mes, MovimientoCuenta movimiento)
        {
            const string sql = @"
                    INSERT INTO dbo.fnd_per_cuentas
                    (
                        anio,
                        mes,
                        cod_cuenta,
                        saldo_inicial,
                        total_debitos,
                        total_creditos,
                        saldo_final
                    )
                    VALUES
                    (
                        @anio,
                        @mes,
                        @cuenta,
                        0,
                        @debito,
                        @credito,
                        0
                    );";

            connection.Execute(sql, new
            {
                anio,
                mes,
                cuenta = movimiento.Cuenta,
                debito = EsDebito(movimiento.DebeHaber) ? movimiento.Monto : 0m,
                credito = EsDebito(movimiento.DebeHaber) ? 0m : movimiento.Monto
            });
        }

        private static void ActualizarClasificacionCuentas(SqlConnection connection, int anio, int mes)
        {
            const string sql = @"
                    UPDATE A
                    SET A.clasificacion = T.clasificacion
                    FROM dbo.fnd_per_cuentas A
                    INNER JOIN dbo.cuentas C
                        ON A.cod_cuenta = C.cod_cuenta
                    INNER JOIN dbo.Tipos_cuentas T
                        ON C.tipo_cuenta = T.tipo_cuenta
                    WHERE A.anio = @anio
                      AND A.mes = @mes
                      AND A.clasificacion IS NULL;";

            connection.Execute(sql, new { anio, mes });
        }

        private static void ActualizarSaldosFinales(SqlConnection connection, int anio, int mes)
        {
            const string sqlDebito = @"
                    UPDATE dbo.fnd_per_cuentas
                    SET saldo_final = saldo_inicial + total_debitos - total_creditos
                    WHERE anio = @anio
                      AND mes = @mes
                      AND clasificacion IN ('G', 'A', 'O', 'V');";

            const string sqlCredito = @"
                    UPDATE dbo.fnd_per_cuentas
                    SET saldo_final = saldo_inicial - total_debitos + total_creditos
                    WHERE anio = @anio
                      AND mes = @mes
                      AND clasificacion IN ('I', 'C', 'P');";

            connection.Execute(sqlDebito, new { anio, mes });
            connection.Execute(sqlCredito, new { anio, mes });
        }

        private static void CrearPeriodoSiguiente(SqlConnection connection, int anio, int mes)
        {
            var siguiente = ObtenerPeriodoSiguiente(anio, mes);
            const string sql = @"
                    INSERT INTO dbo.fnd_per_cuentas
                    (
                        cod_cuenta,
                        anio,
                        mes,
                        saldo_inicial,
                        total_creditos,
                        total_debitos,
                        saldo_final,
                        clasificacion
                    )
                    SELECT
                        cod_cuenta,
                        @anioN,
                        @mesN,
                        saldo_final,
                        0,
                        0,
                        0,
                        clasificacion
                    FROM dbo.fnd_per_cuentas
                    WHERE anio = @anio
                      AND mes = @mes;";

            connection.Execute(sql, new { anioN = siguiente.Anio, mesN = siguiente.Mes, anio, mes });
        }

        private static CierrePeriodo ObtenerPeriodoSiguiente(int anio, int mes) =>
            mes == 12 ? new CierrePeriodo(anio + 1, 1) : new CierrePeriodo(anio, mes + 1);

        private static bool EsDebito(string debeHaber) => string.Equals(NormalizarTexto(debeHaber), "D", StringComparison.OrdinalIgnoreCase);

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();

        private sealed record CierrePeriodo(int Anio, int Mes);

        private sealed record MovimientoCuenta(string Cuenta, string DebeHaber, decimal Monto);
    }
}