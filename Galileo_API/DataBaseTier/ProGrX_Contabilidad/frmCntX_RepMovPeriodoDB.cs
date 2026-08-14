using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXRepMovPeriodoDb
    {
        private readonly PortalDB _portalDb;

        /// <summary>
        /// Inicializa el acceso a datos del reporte de movimientos del periodo.
        /// </summary>
        /// <param name="config">Configuración usada para resolver las conexiones por empresa.</param>
        public FrmCntXRepMovPeriodoDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene los periodos fiscales disponibles para la contabilidad.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <returns>Lista de periodos ordenada del más reciente al más antiguo.</returns>
        public ErrorDto<List<CntxRepMovPeriodoPeriodoDto>> CntX_PeriodosRepMov_Listar(
            int codEmpresa,
            int codContabilidad)
        {
            const string sql = @"
                SELECT
                    id_cierre AS item,
                    RTRIM(descripcion) AS descripcion,
                    CAST(activo AS bit) AS activo
                FROM CntX_Cierres
                WHERE cod_contabilidad = @cod_contabilidad
                ORDER BY activo DESC, id_cierre DESC;";

            return DbHelper.ExecuteListQuery<CntxRepMovPeriodoPeriodoDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { cod_contabilidad = codContabilidad });
        }

        /// <summary>
        /// Obtiene las unidades de negocio disponibles para la contabilidad.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <returns>Lista de unidades de negocio.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_UnidadesRepMov_Listar(
            int codEmpresa,
            int codContabilidad)
        {
            const string sql = @"
                SELECT
                    RTRIM(cod_unidad) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM CntX_Unidades
                WHERE cod_contabilidad = @cod_contabilidad;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { cod_contabilidad = codContabilidad });
        }

        /// <summary>
        /// Obtiene los centros de costo de la contabilidad y, cuando aplica, de una unidad.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <param name="unidad">Código de unidad o C para consolidado.</param>
        /// <returns>Lista de centros de costo disponibles.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_CentroCostosRepMov_Listar(
            int codEmpresa,
            int codContabilidad,
            string unidad)
        {
            const string sql = @"
                SELECT
                    RTRIM(CC.cod_centro_costo) AS item,
                    RTRIM(CC.descripcion) AS descripcion
                FROM CntX_Centro_Costos CC
                WHERE CC.cod_contabilidad = @cod_contabilidad
                  AND (
                        @unidad = 'C'
                        OR EXISTS (
                            SELECT 1
                            FROM CntX_Unidades_CC UCC
                            WHERE UCC.cod_contabilidad = CC.cod_contabilidad
                              AND UCC.cod_centro_costo = CC.cod_centro_costo
                              AND UCC.cod_unidad = @unidad
                        )
                  );";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    cod_contabilidad = codContabilidad,
                    unidad = string.IsNullOrWhiteSpace(unidad) ? "C" : unidad.Trim()
                });
        }

        /// <summary>
        /// Obtiene las áreas de trabajo disponibles para la contabilidad.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <returns>Lista de áreas de trabajo.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_Areas_Listar(
            int codEmpresa,
            int codContabilidad)
        {
            const string sql = @"
                SELECT
                    cod_area AS item,
                    RTRIM(descripcion) AS descripcion
                FROM CntX_Area_Definicion
                WHERE cod_contabilidad = @cod_contabilidad;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { cod_contabilidad = codContabilidad });
        }

        /// <summary>
        /// Prepara, de forma transaccional, los datos consumidos por el informe solicitado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <param name="f">Filtros seleccionados en la pantalla.</param>
        /// <returns>Resultado de la preparación de datos.</returns>
        public ErrorDto<bool> GenerarReporte(
            int codEmpresa,
            int codContabilidad,
            CntxRepMovPeriodoFiltroDto f)
        {
            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection => PrepararReporte(connection, codContabilidad, f));
        }

        /// <summary>
        /// Valida los filtros y coordina la rama de catálogo o resultados dentro de una transacción.
        /// </summary>
        /// <param name="connection">Conexión de la empresa.</param>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <param name="f">Filtros seleccionados en la pantalla.</param>
        /// <returns>True cuando toda la preparación finaliza correctamente.</returns>
        private static bool PrepararReporte(
            SqlConnection connection,
            int codContabilidad,
            CntxRepMovPeriodoFiltroDto f)
        {
            ValidarFiltros(f);

            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var (mes, anio) = ObtenerPeriodo(
                    connection,
                    transaction,
                    f.periodo.Value,
                    codContabilidad);

                if (f.reporte == "03")
                {
                    PrepararResultados(connection, transaction, codContabilidad, f, mes, anio);
                }
                else
                {
                    PrepararCatalogo(connection, transaction, codContabilidad, f, mes, anio);
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Valida los valores mínimos y los códigos permitidos antes de modificar tablas de preparación.
        /// </summary>
        /// <param name="f">Filtros seleccionados en la pantalla.</param>
        private static void ValidarFiltros(CntxRepMovPeriodoFiltroDto f)
        {
            ArgumentNullException.ThrowIfNull(f);

            ValidarCamposRequeridos(f);
            ValidarCodigos(f);
            ValidarNivel(f);
        }

        /// <summary>
        /// Valida los campos obligatorios del formulario.
        /// </summary>
        /// <param name="f">Filtros seleccionados en la pantalla.</param>
        private static void ValidarCamposRequeridos(CntxRepMovPeriodoFiltroDto f)
        {
            if (string.IsNullOrWhiteSpace(f.usuario))
                throw new ArgumentException("Usuario es requerido.");

            if (!f.periodo.HasValue)
                throw new ArgumentException("Periodo es requerido.");

            if (f.tipo == "02" && string.IsNullOrWhiteSpace(f.area))
                throw new ArgumentException("Área de trabajo es requerida.");
        }

        /// <summary>
        /// Valida los códigos cerrados enviados por la interfaz.
        /// </summary>
        /// <param name="f">Filtros seleccionados en la pantalla.</param>
        private static void ValidarCodigos(CntxRepMovPeriodoFiltroDto f)
        {
            if (f.tipo is not ("01" or "02"))
                throw new ArgumentException("Tipo de consulta inválido.");

            if (f.reporte is not ("01" or "02" or "03" or "04" or "05" or "06" or "07" or "08" or "09"))
                throw new ArgumentException("Tipo de reporte inválido.");

            if (f.mostrar is not ("A" or "N"))
                throw new ArgumentException("Forma de cálculo inválida.");
        }

        /// <summary>
        /// Valida el nivel según la rama de resultados o catálogo.
        /// </summary>
        /// <param name="f">Filtros seleccionados en la pantalla.</param>
        private static void ValidarNivel(CntxRepMovPeriodoFiltroDto f)
        {
            if (f.reporte == "03")
            {
                if (f.nivel is not ("Unidad" or "Centro"))
                    throw new ArgumentException("Nivel de resultados inválido.");

                return;
            }

            if (!int.TryParse(f.nivel, out var nivel) || nivel is < 1 or > 8)
                throw new ArgumentException("Nivel contable inválido.");
        }

        /// <summary>
        /// Obtiene el mes y año inicial del periodo fiscal seleccionado.
        /// </summary>
        /// <param name="connection">Conexión abierta de la empresa.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="periodo">Identificador del cierre fiscal.</param>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <returns>Mes y año iniciales del periodo.</returns>
        private static (int mes, int anio) ObtenerPeriodo(
            SqlConnection connection,
            SqlTransaction transaction,
            int periodo,
            int codContabilidad)
        {
            const string sql = @"
                SELECT inicio_mes, inicio_anio
                FROM CntX_Cierres
                WHERE id_cierre = @periodo
                  AND cod_contabilidad = @cod_contabilidad;";

            var result = connection.QueryFirstOrDefault<PeriodoInicioRow>(
                sql,
                new { periodo, cod_contabilidad = codContabilidad },
                transaction);

            if (result == null)
                throw new ArgumentException("No se encontró el periodo seleccionado.");

            return (result.inicio_mes, result.inicio_anio);
        }

        /// <summary>
        /// Replica la rama VB6 de ingresos, gastos, activos, pasivos y balances por cuenta.
        /// </summary>
        /// <param name="connection">Conexión abierta de la empresa.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <param name="f">Filtros seleccionados en la pantalla.</param>
        /// <param name="mesInicial">Mes inicial del periodo fiscal.</param>
        /// <param name="anioInicial">Año inicial del periodo fiscal.</param>
        private static void PrepararCatalogo(
            SqlConnection connection,
            SqlTransaction transaction,
            int codContabilidad,
            CntxRepMovPeriodoFiltroDto f,
            int mesInicial,
            int anioInicial)
        {
            const string sqlInicial = @"
                DELETE CntX_Rep_Periodos_mov
                WHERE usuario = @usuario
                  AND cod_contabilidad = @cod_contabilidad;

                INSERT INTO CntX_Rep_Periodos_mov
                    (cod_cuenta, usuario, cod_contabilidad,
                     movimiento_10, movimiento_11, movimiento_12,
                     movimiento_01, movimiento_02, movimiento_03,
                     movimiento_04, movimiento_05, movimiento_06,
                     movimiento_07, movimiento_08, movimiento_09)
                SELECT C.cod_cuenta, @usuario, @cod_contabilidad,
                       0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
                FROM CntX_Cuentas C
                INNER JOIN CntX_Tipos_Cuentas T
                    ON T.cod_contabilidad = C.cod_contabilidad
                   AND T.tipo_cuenta = C.tipo_cuenta
                WHERE C.cod_contabilidad = @cod_contabilidad
                  AND (
                        (@reporte = '01' AND T.clasificacion IN ('I', 'V'))
                        OR (@reporte = '02' AND T.clasificacion = 'G')
                        OR (@reporte = '04' AND T.clasificacion = 'A')
                        OR (@reporte = '05' AND T.clasificacion = 'P')
                        OR (@reporte = '06' AND T.clasificacion = 'C')
                        OR (@reporte = '07' AND T.clasificacion IN ('I', 'V', 'C', 'G', 'P', 'A'))
                        OR (@reporte = '08' AND T.clasificacion IN ('A', 'P', 'C'))
                        OR (@reporte = '09' AND T.clasificacion IN ('I', 'V', 'G'))
                  )
                  AND (
                        @tipo = '01'
                        OR EXISTS (
                            SELECT 1
                            FROM CntX_Area_Cuentas A
                            WHERE A.cod_contabilidad = C.cod_contabilidad
                              AND A.cod_cuenta = C.cod_cuenta
                              AND A.cod_area = @area
                        )
                  );";

            connection.Execute(
                sqlInicial,
                CrearParametros(codContabilidad, f),
                transaction,
                commandTimeout: 0);

            ActualizarMovimientosCatalogo(
                connection,
                transaction,
                codContabilidad,
                f,
                mesInicial,
                anioInicial);
        }

        /// <summary>
        /// Actualiza los doce meses usando las mismas fuentes elegidas por el VB6 para unidad y centro.
        /// </summary>
        /// <param name="connection">Conexión abierta de la empresa.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <param name="f">Filtros seleccionados en la pantalla.</param>
        /// <param name="mesInicial">Mes inicial del periodo fiscal.</param>
        /// <param name="anioInicial">Año inicial del periodo fiscal.</param>
        private static void ActualizarMovimientosCatalogo(
            SqlConnection connection,
            SqlTransaction transaction,
            int codContabilidad,
            CntxRepMovPeriodoFiltroDto f,
            int mesInicial,
            int anioInicial)
        {
            var unidad = NormalizarOpcion(f.unidad, "C");
            var centroCosto = NormalizarOpcion(f.centroCosto, "T");

            const string sql = @"
                DECLARE @fecha_inicial date = DATEFROMPARTS(@anio_inicial, @mes_inicial, 1);

                ;WITH MovimientosBase AS
                (
                    SELECT M.cod_cuenta, M.saldo_inicial, M.total_debitos, M.total_creditos,
                           DATEFROMPARTS(M.anio, M.mes, 1) AS fecha
                    FROM vCntX_Mov_Cuentas_General M
                    WHERE @unidad = ''
                      AND @centro_costo = ''
                      AND M.cod_contabilidad = @cod_contabilidad

                    UNION ALL

                    SELECT M.cod_cuenta, M.saldo_inicial, M.total_debitos, M.total_creditos,
                           DATEFROMPARTS(M.anio, M.mes, 1) AS fecha
                    FROM vCntX_Mov_Cuentas_CentroCosto M
                    WHERE @unidad = ''
                      AND @centro_costo <> ''
                      AND M.cod_contabilidad = @cod_contabilidad
                      AND M.cod_centro_costo = @centro_costo

                    UNION ALL

                    SELECT M.cod_cuenta, M.saldo_inicial, M.total_debitos, M.total_creditos,
                           DATEFROMPARTS(M.anio, M.mes, 1) AS fecha
                    FROM vCntX_Mov_Cuentas_Unidad M
                    WHERE @unidad <> ''
                      AND @centro_costo = ''
                      AND M.cod_contabilidad = @cod_contabilidad
                      AND M.cod_unidad = @unidad

                    UNION ALL

                    SELECT M.cod_cuenta, M.saldo_inicial, M.total_debitos, M.total_creditos,
                           DATEFROMPARTS(M.anio, M.mes, 1) AS fecha
                    FROM CntX_Mov_Cuentas_Detallado M
                    WHERE @unidad <> ''
                      AND @centro_costo <> ''
                      AND M.cod_contabilidad = @cod_contabilidad
                      AND M.cod_unidad = @unidad
                      AND M.cod_centro_costo = @centro_costo
                ),
                Movimientos AS
                (
                    SELECT M.cod_cuenta,
                           SUM(CASE WHEN M.fecha = DATEADD(month, 0, @fecha_inicial)
                               THEN CASE WHEN @mostrar = 'A'
                                   THEN M.saldo_inicial + M.total_debitos + M.total_creditos
                                   ELSE M.total_debitos + M.total_creditos END ELSE 0 END) AS movimiento_01,
                           SUM(CASE WHEN M.fecha = DATEADD(month, 1, @fecha_inicial)
                               THEN CASE WHEN @mostrar = 'A'
                                   THEN M.saldo_inicial + M.total_debitos + M.total_creditos
                                   ELSE M.total_debitos + M.total_creditos END ELSE 0 END) AS movimiento_02,
                           SUM(CASE WHEN M.fecha = DATEADD(month, 2, @fecha_inicial)
                               THEN CASE WHEN @mostrar = 'A'
                                   THEN M.saldo_inicial + M.total_debitos + M.total_creditos
                                   ELSE M.total_debitos + M.total_creditos END ELSE 0 END) AS movimiento_03,
                           SUM(CASE WHEN M.fecha = DATEADD(month, 3, @fecha_inicial)
                               THEN CASE WHEN @mostrar = 'A'
                                   THEN M.saldo_inicial + M.total_debitos + M.total_creditos
                                   ELSE M.total_debitos + M.total_creditos END ELSE 0 END) AS movimiento_04,
                           SUM(CASE WHEN M.fecha = DATEADD(month, 4, @fecha_inicial)
                               THEN CASE WHEN @mostrar = 'A'
                                   THEN M.saldo_inicial + M.total_debitos + M.total_creditos
                                   ELSE M.total_debitos + M.total_creditos END ELSE 0 END) AS movimiento_05,
                           SUM(CASE WHEN M.fecha = DATEADD(month, 5, @fecha_inicial)
                               THEN CASE WHEN @mostrar = 'A'
                                   THEN M.saldo_inicial + M.total_debitos + M.total_creditos
                                   ELSE M.total_debitos + M.total_creditos END ELSE 0 END) AS movimiento_06,
                           SUM(CASE WHEN M.fecha = DATEADD(month, 6, @fecha_inicial)
                               THEN CASE WHEN @mostrar = 'A'
                                   THEN M.saldo_inicial + M.total_debitos + M.total_creditos
                                   ELSE M.total_debitos + M.total_creditos END ELSE 0 END) AS movimiento_07,
                           SUM(CASE WHEN M.fecha = DATEADD(month, 7, @fecha_inicial)
                               THEN CASE WHEN @mostrar = 'A'
                                   THEN M.saldo_inicial + M.total_debitos + M.total_creditos
                                   ELSE M.total_debitos + M.total_creditos END ELSE 0 END) AS movimiento_08,
                           SUM(CASE WHEN M.fecha = DATEADD(month, 8, @fecha_inicial)
                               THEN CASE WHEN @mostrar = 'A'
                                   THEN M.saldo_inicial + M.total_debitos + M.total_creditos
                                   ELSE M.total_debitos + M.total_creditos END ELSE 0 END) AS movimiento_09,
                           SUM(CASE WHEN M.fecha = DATEADD(month, 9, @fecha_inicial)
                               THEN CASE WHEN @mostrar = 'A'
                                   THEN M.saldo_inicial + M.total_debitos + M.total_creditos
                                   ELSE M.total_debitos + M.total_creditos END ELSE 0 END) AS movimiento_10,
                           SUM(CASE WHEN M.fecha = DATEADD(month, 10, @fecha_inicial)
                               THEN CASE WHEN @mostrar = 'A'
                                   THEN M.saldo_inicial + M.total_debitos + M.total_creditos
                                   ELSE M.total_debitos + M.total_creditos END ELSE 0 END) AS movimiento_11,
                           SUM(CASE WHEN M.fecha = DATEADD(month, 11, @fecha_inicial)
                               THEN CASE WHEN @mostrar = 'A'
                                   THEN M.saldo_inicial + M.total_debitos + M.total_creditos
                                   ELSE M.total_debitos + M.total_creditos END ELSE 0 END) AS movimiento_12
                    FROM MovimientosBase M
                    WHERE M.fecha BETWEEN @fecha_inicial AND DATEADD(month, 11, @fecha_inicial)
                    GROUP BY M.cod_cuenta
                )
                UPDATE R
                SET R.movimiento_01 = X.movimiento_01,
                    R.movimiento_02 = X.movimiento_02,
                    R.movimiento_03 = X.movimiento_03,
                    R.movimiento_04 = X.movimiento_04,
                    R.movimiento_05 = X.movimiento_05,
                    R.movimiento_06 = X.movimiento_06,
                    R.movimiento_07 = X.movimiento_07,
                    R.movimiento_08 = X.movimiento_08,
                    R.movimiento_09 = X.movimiento_09,
                    R.movimiento_10 = X.movimiento_10,
                    R.movimiento_11 = X.movimiento_11,
                    R.movimiento_12 = X.movimiento_12
                FROM CntX_Rep_Periodos_mov R
                INNER JOIN Movimientos X ON X.cod_cuenta = R.cod_cuenta
                WHERE R.usuario = @usuario
                  AND R.cod_contabilidad = @cod_contabilidad;";

            connection.Execute(
                sql,
                new
                {
                    anio_inicial = anioInicial,
                    mes_inicial = mesInicial,
                    mostrar = f.mostrar,
                    cod_contabilidad = codContabilidad,
                    unidad,
                    centro_costo = centroCosto,
                    usuario = f.usuario.Trim()
                },
                transaction,
                commandTimeout: 0);
        }

        /// <summary>
        /// Replica la rama VB6 de resultados agrupados por unidad o centro de costo.
        /// </summary>
        /// <param name="connection">Conexión abierta de la empresa.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <param name="f">Filtros seleccionados en la pantalla.</param>
        /// <param name="mesInicial">Mes inicial del periodo fiscal.</param>
        /// <param name="anioInicial">Año inicial del periodo fiscal.</param>
        private static void PrepararResultados(
            SqlConnection connection,
            SqlTransaction transaction,
            int codContabilidad,
            CntxRepMovPeriodoFiltroDto f,
            int mesInicial,
            int anioInicial)
        {
            var unidad = NormalizarOpcion(f.unidad, "C");
            var centroCosto = NormalizarOpcion(f.centroCosto, "T");

            const string sqlInicial = @"
                DELETE CNTX_REP_PERIODOS_MOV_UNIDAD
                WHERE usuario = @usuario
                  AND cod_contabilidad = @cod_contabilidad;

                IF @tipo = '02'
                BEGIN
                    INSERT INTO CNTX_REP_PERIODOS_MOV_UNIDAD
                        (cod_unidad, cod_centro_costo, usuario, cod_contabilidad,
                         movimiento_10, movimiento_11, movimiento_12,
                         movimiento_01, movimiento_02, movimiento_03,
                         movimiento_04, movimiento_05, movimiento_06,
                         movimiento_07, movimiento_08, movimiento_09)
                    SELECT AU.cod_unidad, AU.cod_centro_costo, @usuario, @cod_contabilidad,
                           0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
                    FROM CntX_Area_Unidades AU
                    WHERE AU.cod_contabilidad = @cod_contabilidad
                      AND AU.cod_area = @area;
                END
                ELSE IF @nivel = 'Unidad'
                BEGIN
                    IF @centro_costo = ''
                    BEGIN
                        INSERT INTO CNTX_REP_PERIODOS_MOV_UNIDAD
                            (cod_unidad, cod_centro_costo, usuario, cod_contabilidad,
                             movimiento_10, movimiento_11, movimiento_12,
                             movimiento_01, movimiento_02, movimiento_03,
                             movimiento_04, movimiento_05, movimiento_06,
                             movimiento_07, movimiento_08, movimiento_09)
                        SELECT U.cod_unidad, '', @usuario, U.cod_contabilidad,
                               0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
                        FROM CntX_Unidades U
                        WHERE U.cod_contabilidad = @cod_contabilidad
                          AND (@unidad = '' OR U.cod_unidad = @unidad);
                    END
                    ELSE
                    BEGIN
                        INSERT INTO CNTX_REP_PERIODOS_MOV_UNIDAD
                            (cod_unidad, cod_centro_costo, usuario, cod_contabilidad,
                             movimiento_10, movimiento_11, movimiento_12,
                             movimiento_01, movimiento_02, movimiento_03,
                             movimiento_04, movimiento_05, movimiento_06,
                             movimiento_07, movimiento_08, movimiento_09)
                        VALUES
                            (@unidad, @centro_costo, @usuario, @cod_contabilidad,
                             0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                    END
                END
                ELSE
                BEGIN
                    IF @centro_costo = ''
                    BEGIN
                        INSERT INTO CNTX_REP_PERIODOS_MOV_UNIDAD
                            (cod_unidad, cod_centro_costo, usuario, cod_contabilidad,
                             movimiento_10, movimiento_11, movimiento_12,
                             movimiento_01, movimiento_02, movimiento_03,
                             movimiento_04, movimiento_05, movimiento_06,
                             movimiento_07, movimiento_08, movimiento_09)
                        SELECT UCC.cod_unidad, UCC.cod_centro_costo, @usuario, UCC.cod_contabilidad,
                               0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
                        FROM CntX_Unidades_CC UCC
                        WHERE UCC.cod_contabilidad = @cod_contabilidad
                          AND (@unidad = '' OR UCC.cod_unidad = @unidad);
                    END
                    ELSE
                    BEGIN
                        INSERT INTO CNTX_REP_PERIODOS_MOV_UNIDAD
                            (cod_unidad, cod_centro_costo, usuario, cod_contabilidad,
                             movimiento_10, movimiento_11, movimiento_12,
                             movimiento_01, movimiento_02, movimiento_03,
                             movimiento_04, movimiento_05, movimiento_06,
                             movimiento_07, movimiento_08, movimiento_09)
                        VALUES
                            (@unidad, @centro_costo, @usuario, @cod_contabilidad,
                             0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                    END
                END;";

            var parametros = new
            {
                usuario = f.usuario.Trim(),
                cod_contabilidad = codContabilidad,
                tipo = f.tipo,
                nivel = f.nivel,
                area = f.area,
                unidad,
                centro_costo = centroCosto
            };

            connection.Execute(sqlInicial, parametros, transaction, commandTimeout: 0);

            ActualizarMovimientosResultados(
                connection,
                transaction,
                codContabilidad,
                f,
                mesInicial,
                anioInicial);
        }

        /// <summary>
        /// Calcula los doce meses mediante fxCntX_UtilidadDetallada para cada agrupación preparada.
        /// </summary>
        /// <param name="connection">Conexión abierta de la empresa.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <param name="f">Filtros seleccionados en la pantalla.</param>
        /// <param name="mesInicial">Mes inicial del periodo fiscal.</param>
        /// <param name="anioInicial">Año inicial del periodo fiscal.</param>
        private static void ActualizarMovimientosResultados(
            SqlConnection connection,
            SqlTransaction transaction,
            int codContabilidad,
            CntxRepMovPeriodoFiltroDto f,
            int mesInicial,
            int anioInicial)
        {
            const string sql = @"
                DECLARE @fecha_inicial date = DATEFROMPARTS(@anio_inicial, @mes_inicial, 1);

                UPDATE CNTX_REP_PERIODOS_MOV_UNIDAD
                SET movimiento_01 = dbo.fxCntX_UtilidadDetallada(
                        YEAR(DATEADD(month, 0, @fecha_inicial)), MONTH(DATEADD(month, 0, @fecha_inicial)),
                        cod_contabilidad, cod_unidad, cod_centro_costo, @mostrar),
                    movimiento_02 = dbo.fxCntX_UtilidadDetallada(
                        YEAR(DATEADD(month, 1, @fecha_inicial)), MONTH(DATEADD(month, 1, @fecha_inicial)),
                        cod_contabilidad, cod_unidad, cod_centro_costo, @mostrar),
                    movimiento_03 = dbo.fxCntX_UtilidadDetallada(
                        YEAR(DATEADD(month, 2, @fecha_inicial)), MONTH(DATEADD(month, 2, @fecha_inicial)),
                        cod_contabilidad, cod_unidad, cod_centro_costo, @mostrar),
                    movimiento_04 = dbo.fxCntX_UtilidadDetallada(
                        YEAR(DATEADD(month, 3, @fecha_inicial)), MONTH(DATEADD(month, 3, @fecha_inicial)),
                        cod_contabilidad, cod_unidad, cod_centro_costo, @mostrar),
                    movimiento_05 = dbo.fxCntX_UtilidadDetallada(
                        YEAR(DATEADD(month, 4, @fecha_inicial)), MONTH(DATEADD(month, 4, @fecha_inicial)),
                        cod_contabilidad, cod_unidad, cod_centro_costo, @mostrar),
                    movimiento_06 = dbo.fxCntX_UtilidadDetallada(
                        YEAR(DATEADD(month, 5, @fecha_inicial)), MONTH(DATEADD(month, 5, @fecha_inicial)),
                        cod_contabilidad, cod_unidad, cod_centro_costo, @mostrar),
                    movimiento_07 = dbo.fxCntX_UtilidadDetallada(
                        YEAR(DATEADD(month, 6, @fecha_inicial)), MONTH(DATEADD(month, 6, @fecha_inicial)),
                        cod_contabilidad, cod_unidad, cod_centro_costo, @mostrar),
                    movimiento_08 = dbo.fxCntX_UtilidadDetallada(
                        YEAR(DATEADD(month, 7, @fecha_inicial)), MONTH(DATEADD(month, 7, @fecha_inicial)),
                        cod_contabilidad, cod_unidad, cod_centro_costo, @mostrar),
                    movimiento_09 = dbo.fxCntX_UtilidadDetallada(
                        YEAR(DATEADD(month, 8, @fecha_inicial)), MONTH(DATEADD(month, 8, @fecha_inicial)),
                        cod_contabilidad, cod_unidad, cod_centro_costo, @mostrar),
                    movimiento_10 = dbo.fxCntX_UtilidadDetallada(
                        YEAR(DATEADD(month, 9, @fecha_inicial)), MONTH(DATEADD(month, 9, @fecha_inicial)),
                        cod_contabilidad, cod_unidad, cod_centro_costo, @mostrar),
                    movimiento_11 = dbo.fxCntX_UtilidadDetallada(
                        YEAR(DATEADD(month, 10, @fecha_inicial)), MONTH(DATEADD(month, 10, @fecha_inicial)),
                        cod_contabilidad, cod_unidad, cod_centro_costo, @mostrar),
                    movimiento_12 = dbo.fxCntX_UtilidadDetallada(
                        YEAR(DATEADD(month, 11, @fecha_inicial)), MONTH(DATEADD(month, 11, @fecha_inicial)),
                        cod_contabilidad, cod_unidad, cod_centro_costo, @mostrar)
                WHERE usuario = @usuario
                  AND cod_contabilidad = @cod_contabilidad;";

            connection.Execute(
                sql,
                new
                {
                    anio_inicial = anioInicial,
                    mes_inicial = mesInicial,
                    mostrar = f.mostrar,
                    usuario = f.usuario.Trim(),
                    cod_contabilidad = codContabilidad
                },
                transaction,
                commandTimeout: 0);
        }

        /// <summary>
        /// Construye los parámetros comunes de la rama por cuenta.
        /// </summary>
        /// <param name="codContabilidad">Código de la contabilidad.</param>
        /// <param name="f">Filtros seleccionados en la pantalla.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametros(int codContabilidad, CntxRepMovPeriodoFiltroDto f)
        {
            return new
            {
                usuario = f.usuario.Trim(),
                cod_contabilidad = codContabilidad,
                reporte = f.reporte,
                tipo = f.tipo,
                area = f.area
            };
        }

        /// <summary>
        /// Convierte los códigos sintéticos de consolidado y todos en filtros vacíos.
        /// </summary>
        /// <param name="valor">Valor recibido desde la pantalla.</param>
        /// <param name="opcionTodos">Código que representa la opción global.</param>
        /// <returns>Valor normalizado para las consultas.</returns>
        private static string NormalizarOpcion(string? valor, string opcionTodos)
        {
            var normalizado = valor?.Trim() ?? string.Empty;
            return normalizado == opcionTodos ? string.Empty : normalizado;
        }

    }
}
