using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXRepEspecialesDb
    {
        private readonly PortalDB _portalDb;

        /// <summary>
        /// Inicializa el acceso a datos de informes especiales desde la configuración.
        /// </summary>
        /// <param name="config">Configuración de conexiones del API.</param>
        public FrmCntXRepEspecialesDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        /// <summary>
        /// Inicializa el acceso a datos con una instancia de PortalDB.
        /// </summary>
        /// <param name="portalDb">Proveedor de conexiones por empresa.</param>
        public FrmCntXRepEspecialesDb(PortalDB portalDb)
        {
            _portalDb = portalDb;
        }

        /// <summary>
        /// Lista los cierres contables y coloca primero el período activo.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa conectada.</param>
        /// <param name="codContabilidad">Código de la contabilidad activa.</param>
        /// <returns>Períodos disponibles para el informe.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_Periodos_Listar(int codEmpresa, int codContabilidad)
        {
            const string sql = @"
                SELECT
                    id_cierre AS item,
                    RTRIM(descripcion) AS descripcion
                FROM CntX_Cierres
                WHERE cod_contabilidad = @cod_contabilidad
                ORDER BY activo DESC, id_cierre DESC;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { cod_contabilidad = codContabilidad });
        }

        /// <summary>
        /// Lista las unidades de negocio de la contabilidad.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa conectada.</param>
        /// <param name="codContabilidad">Código de la contabilidad activa.</param>
        /// <returns>Unidades disponibles para el informe.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_Unidades_Listar(int codEmpresa, int codContabilidad)
        {
            const string sql = @"
                SELECT
                    RTRIM(cod_unidad) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM CntX_Unidades
                WHERE cod_contabilidad = @cod_contabilidad
                ORDER BY descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { cod_contabilidad = codContabilidad });
        }

        /// <summary>
        /// Lista los centros de costo de la unidad o todos para el consolidado.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa conectada.</param>
        /// <param name="codContabilidad">Código de la contabilidad activa.</param>
        /// <param name="unidad">Código de unidad o C para el consolidado.</param>
        /// <returns>Centros de costo disponibles para el filtro.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_CentroCostos_Listar(
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
                  )
                ORDER BY CC.descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    cod_contabilidad = codContabilidad,
                    unidad
                });
        }

        /// <summary>
        /// Prepara en una transacción la información trimestral que consumen los reportes.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa conectada.</param>
        /// <param name="codContabilidad">Código de la contabilidad activa.</param>
        /// <param name="f">Filtros seleccionados y período contable vigente.</param>
        /// <returns>Resultado de la preparación de los datos.</returns>
        public ErrorDto<bool> GenerarReporte(
            int codEmpresa,
            int codContabilidad,
            CntxRepEspecialFiltroDto f)
        {
            var validacion = ValidarFiltros(f);
            if (validacion is not null)
            {
                return DbHelper.CreateErrorResponse<bool>(validacion, result: false);
            }

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection => EjecutarPreparacion(connection, codContabilidad, f));
        }

        /// <summary>
        /// Valida los datos indispensables antes de abrir una conexión.
        /// </summary>
        /// <param name="f">Filtros recibidos desde la pantalla.</param>
        /// <returns>Mensaje de error o null cuando los filtros son válidos.</returns>
        private static string? ValidarFiltros(CntxRepEspecialFiltroDto f)
        {
            if (f.periodo is null or <= 0)
            {
                return "El período es requerido.";
            }

            if (f.periodoAnio is null or <= 0 || f.periodoMes is null or < 1 or > 12)
            {
                return "El año y mes del período contable son requeridos.";
            }

            if (string.IsNullOrWhiteSpace(f.usuario))
            {
                return "El usuario es requerido.";
            }

            if (f.reporte is not ("1" or "2" or "2.1" or "2.2" or "3"))
            {
                return "El tipo de reporte no es válido.";
            }

            return null;
        }

        /// <summary>
        /// Coordina la preparación transaccional del reporte seleccionado.
        /// </summary>
        /// <param name="connection">Conexión de la empresa.</param>
        /// <param name="codContabilidad">Código de la contabilidad activa.</param>
        /// <param name="f">Filtros del reporte.</param>
        /// <returns>True cuando toda la preparación termina correctamente.</returns>
        private static bool EjecutarPreparacion(
            SqlConnection connection,
            int codContabilidad,
            CntxRepEspecialFiltroDto f)
        {
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                if (f.reporte is "2.1" or "2.2")
                {
                    EjecutarRentabilidadEspecial(connection, transaction, codContabilidad, f);
                }
                else
                {
                    EjecutarMovimientoCatalogo(connection, transaction, codContabilidad, f);
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
        /// Replica la preparación de activos, pasivos, rentabilidad y balance del formulario VB6.
        /// </summary>
        /// <param name="connection">Conexión abierta de la empresa.</param>
        /// <param name="transaction">Transacción que agrupa toda la preparación.</param>
        /// <param name="codContabilidad">Código de la contabilidad activa.</param>
        /// <param name="f">Filtros del reporte.</param>
        private static void EjecutarMovimientoCatalogo(
            SqlConnection connection,
            SqlTransaction transaction,
            int codContabilidad,
            CntxRepEspecialFiltroDto f)
        {
            var unidad = NormalizarOpcion(f.unidad, "C");
            var centroCosto = NormalizarOpcion(f.centroCosto, "T");
            var fuente = ObtenerFuenteMovimientos(unidad, centroCosto);
            var filtrosFuente = CrearFiltrosFuente(unidad, centroCosto);

            var sql = $@"
                DELETE CntX_Rep_Periodos_mov
                WHERE usuario = @usuario;

                DECLARE @cuenta_utilidad varchar(100) = (
                    SELECT TOP (1) RTRIM(Cuenta_GanPer)
                    FROM CNTX_CIERRES
                    WHERE ID_CIERRE = @periodo
                      AND cod_contabilidad = @cod_contabilidad
                );

                IF NULLIF(@cuenta_utilidad, '') IS NULL
                    THROW 50001, 'El cierre seleccionado no tiene cuenta de ganancias y pérdidas.', 1;

                INSERT INTO CntX_Rep_Periodos_mov
                    (cod_cuenta, usuario, cod_contabilidad,
                     movimiento_10, movimiento_11, movimiento_12,
                     movimiento_01, movimiento_02, movimiento_03,
                     movimiento_04, movimiento_05, movimiento_06,
                     movimiento_07, movimiento_08, movimiento_09)
                VALUES
                    (@cuenta_utilidad, @usuario, @cod_contabilidad,
                     0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

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
                  AND C.cod_cuenta <> @cuenta_utilidad
                  AND (
                        (@reporte = '1' AND T.clasificacion IN ('A', 'P', 'C'))
                        OR (@reporte = '2' AND T.clasificacion IN ('I', 'V', 'G'))
                        OR (@reporte = '3' AND T.clasificacion IN ('I', 'V', 'C', 'G', 'P', 'A'))
                  );

                DECLARE @fecha_inicial date = DATEADD(month, -2, DATEFROMPARTS(@periodo_anio, @periodo_mes, 1));
                {CrearSqlMovimientosTrimestre(fuente, filtrosFuente)}

                DELETE CntX_Rep_Periodos_mov
                WHERE usuario = @usuario
                  AND movimiento_01 + movimiento_02 + movimiento_03
                    + movimiento_04 + movimiento_05 + movimiento_06 = 0;";

            connection.Execute(
                sql,
                CrearParametros(codContabilidad, f, unidad, centroCosto),
                transaction,
                commandTimeout: 0);
        }

        /// <summary>
        /// Crea las actualizaciones del trimestre para el origen de movimientos seleccionado.
        /// </summary>
        /// <param name="fuente">Vista o tabla determinada por unidad y centro de costo.</param>
        /// <param name="filtrosFuente">Predicados compatibles con las columnas de la fuente.</param>
        /// <returns>SQL de los tres meses y sus acumulados.</returns>
        private static string CrearSqlMovimientosTrimestre(string fuente, string filtrosFuente)
        {
            var instrucciones = new List<string>();

            for (var indice = 1; indice <= 3; indice++)
            {
                var desplazamiento = indice - 1;
                var columnaMovimiento = $"movimiento_{indice:00}";
                var columnaAcumulado = $"movimiento_{indice + 3:00}";

                instrucciones.Add($@"
                    ;WITH Movimientos AS
                    (
                        SELECT M.cod_cuenta,
                               SUM(M.Total_Debitos + M.Total_Creditos) AS movimiento,
                               SUM(M.Saldo_Inicial + M.Total_Debitos + M.Total_Creditos) AS acumulado
                        FROM {fuente} M
                        WHERE M.anio = YEAR(DATEADD(month, {desplazamiento}, @fecha_inicial))
                          AND M.mes = MONTH(DATEADD(month, {desplazamiento}, @fecha_inicial))
                          AND M.cod_contabilidad = @cod_contabilidad
                          {filtrosFuente}
                        GROUP BY M.cod_cuenta
                    )
                    UPDATE R
                    SET {columnaMovimiento} = M.movimiento,
                        {columnaAcumulado} = M.acumulado
                    FROM CntX_Rep_Periodos_mov R
                    INNER JOIN Movimientos M ON M.cod_cuenta = R.cod_cuenta
                    WHERE R.usuario = @usuario
                      AND R.cod_contabilidad = @cod_contabilidad;

                    UPDATE CntX_Rep_Periodos_mov
                    SET {columnaMovimiento} = {columnaMovimiento}
                            + dbo.fxCntX_UtilidadMes(
                                YEAR(DATEADD(month, {desplazamiento}, @fecha_inicial)),
                                MONTH(DATEADD(month, {desplazamiento}, @fecha_inicial)),
                                @cod_contabilidad, @unidad, @centro_costo),
                        {columnaAcumulado} = {columnaAcumulado}
                            + dbo.fxCntX_Utilidad(
                                YEAR(DATEADD(month, {desplazamiento}, @fecha_inicial)),
                                MONTH(DATEADD(month, {desplazamiento}, @fecha_inicial)),
                                @cod_contabilidad, @unidad, @centro_costo)
                    WHERE usuario = @usuario
                      AND cod_contabilidad = @cod_contabilidad
                      AND cod_cuenta IN (
                          SELECT cuenta
                          FROM dbo.fxCntX_CuentasCascada(@cod_contabilidad, @cuenta_utilidad)
                      );");
            }

            return string.Join(Environment.NewLine, instrucciones);
        }

        /// <summary>
        /// Replica la preparación de rentabilidad agrupada por centro de costo o unidad.
        /// </summary>
        /// <param name="connection">Conexión abierta de la empresa.</param>
        /// <param name="transaction">Transacción que agrupa toda la preparación.</param>
        /// <param name="codContabilidad">Código de la contabilidad activa.</param>
        /// <param name="f">Filtros del reporte.</param>
        private static void EjecutarRentabilidadEspecial(
            SqlConnection connection,
            SqlTransaction transaction,
            int codContabilidad,
            CntxRepEspecialFiltroDto f)
        {
            var unidad = NormalizarOpcion(f.unidad, "C");
            var centroCosto = NormalizarOpcion(f.centroCosto, "T");
            var sqlCarga = f.reporte == "2.1"
                ? CrearSqlCargaCentroCosto()
                : CrearSqlCargaUnidad();

            var sql = $@"
                DELETE CNTX_REP_PERIODOS_MOV_UNIDAD
                WHERE usuario = @usuario;

                {sqlCarga}

                DECLARE @fecha_inicial date = DATEADD(month, -2, DATEFROMPARTS(@periodo_anio, @periodo_mes, 1));
                {CrearSqlRentabilidadTrimestre()}

                UPDATE R
                SET movimiento_04 = dbo.fxCntX_UtilidadDetallada(
                        @periodo_anio,
                        @periodo_mes,
                        R.cod_contabilidad,
                        R.cod_unidad,
                        R.cod_centro_costo,
                        'A')
                FROM CNTX_REP_PERIODOS_MOV_UNIDAD R
                WHERE R.usuario = @usuario
                  AND R.cod_contabilidad = @cod_contabilidad;";

            connection.Execute(
                sql,
                CrearParametros(codContabilidad, f, unidad, centroCosto),
                transaction,
                commandTimeout: 0);
        }

        /// <summary>
        /// Crea la carga base del reporte de rentabilidad por centro de costo.
        /// </summary>
        /// <returns>SQL parametrizado de la carga inicial.</returns>
        private static string CrearSqlCargaCentroCosto()
        {
            return @"
                INSERT INTO CNTX_REP_PERIODOS_MOV_UNIDAD
                    (cod_unidad, cod_centro_costo, usuario, cod_contabilidad,
                     movimiento_10, movimiento_11, movimiento_12,
                     movimiento_01, movimiento_02, movimiento_03,
                     movimiento_04, movimiento_05, movimiento_06,
                     movimiento_07, movimiento_08, movimiento_09)
                SELECT UCC.cod_unidad, UCC.cod_centro_costo, @usuario, UCC.cod_contabilidad,
                       0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
                FROM CNTX_UNIDADES_CC UCC
                WHERE UCC.cod_contabilidad = @cod_contabilidad
                  AND @centro_costo = ''
                  AND (@unidad = '' OR UCC.cod_unidad = @unidad)
                UNION ALL
                SELECT @unidad, @centro_costo, @usuario, @cod_contabilidad,
                       0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
                WHERE @centro_costo <> '';";
        }

        /// <summary>
        /// Crea la carga base del reporte de rentabilidad por unidad.
        /// </summary>
        /// <returns>SQL parametrizado de la carga inicial.</returns>
        private static string CrearSqlCargaUnidad()
        {
            return @"
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
                  AND @centro_costo = ''
                  AND (@unidad = '' OR U.cod_unidad = @unidad)
                UNION ALL
                SELECT @unidad, @centro_costo, @usuario, @cod_contabilidad,
                       0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
                WHERE @centro_costo <> '';";
        }

        /// <summary>
        /// Crea las actualizaciones mensuales para la rentabilidad detallada.
        /// </summary>
        /// <returns>SQL de los tres meses del trimestre.</returns>
        private static string CrearSqlRentabilidadTrimestre()
        {
            var instrucciones = new List<string>();

            for (var indice = 1; indice <= 3; indice++)
            {
                var desplazamiento = indice - 1;
                instrucciones.Add($@"
                    UPDATE R
                    SET movimiento_{indice:00} = dbo.fxCntX_UtilidadDetallada(
                            YEAR(DATEADD(month, {desplazamiento}, @fecha_inicial)),
                            MONTH(DATEADD(month, {desplazamiento}, @fecha_inicial)),
                            R.cod_contabilidad,
                            R.cod_unidad,
                            R.cod_centro_costo,
                            'N')
                    FROM CNTX_REP_PERIODOS_MOV_UNIDAD R
                    WHERE R.usuario = @usuario
                      AND R.cod_contabilidad = @cod_contabilidad;");
            }

            return string.Join(Environment.NewLine, instrucciones);
        }

        /// <summary>
        /// Determina el origen equivalente al usado por el VB6 para cada combinación de filtros.
        /// </summary>
        /// <param name="unidad">Unidad normalizada; vacía representa consolidado.</param>
        /// <param name="centroCosto">Centro normalizado; vacío representa todos.</param>
        /// <returns>Nombre seguro de la vista o tabla de movimientos.</returns>
        private static string ObtenerFuenteMovimientos(string unidad, string centroCosto)
        {
            if (string.IsNullOrEmpty(unidad) && string.IsNullOrEmpty(centroCosto))
            {
                return "vCntX_Mov_Cuentas_General";
            }

            if (string.IsNullOrEmpty(unidad))
            {
                return "vCntX_Mov_Cuentas_CentroCosto";
            }

            if (string.IsNullOrEmpty(centroCosto))
            {
                return "vCntX_Mov_Cuentas_Unidad";
            }

            return "CntX_Mov_Cuentas_Detallado";
        }

        /// <summary>
        /// Genera solamente los filtros cuyas columnas existen en la fuente seleccionada.
        /// </summary>
        /// <param name="unidad">Unidad normalizada; vacía representa consolidado.</param>
        /// <param name="centroCosto">Centro normalizado; vacío representa todos.</param>
        /// <returns>Predicados SQL seguros para la fuente.</returns>
        private static string CrearFiltrosFuente(string unidad, string centroCosto)
        {
            if (string.IsNullOrEmpty(unidad) && string.IsNullOrEmpty(centroCosto))
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(unidad))
            {
                return "AND M.cod_centro_costo = @centro_costo";
            }

            if (string.IsNullOrEmpty(centroCosto))
            {
                return "AND M.cod_unidad = @unidad";
            }

            return "AND M.cod_unidad = @unidad AND M.cod_centro_costo = @centro_costo";
        }

        /// <summary>
        /// Convierte las opciones sintéticas de la interfaz en filtros vacíos como en el VB6.
        /// </summary>
        /// <param name="valor">Valor recibido desde la pantalla.</param>
        /// <param name="opcionTodos">Código que representa consolidado o todos.</param>
        /// <returns>Valor recortado o cadena vacía.</returns>
        private static string NormalizarOpcion(string? valor, string opcionTodos)
        {
            var normalizado = valor?.Trim() ?? string.Empty;
            return normalizado.Equals(opcionTodos, StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : normalizado;
        }

        /// <summary>
        /// Construye los parámetros comunes de las consultas de preparación.
        /// </summary>
        /// <param name="codContabilidad">Código de la contabilidad activa.</param>
        /// <param name="f">Filtros del reporte.</param>
        /// <param name="unidad">Unidad normalizada.</param>
        /// <param name="centroCosto">Centro de costo normalizado.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametros(
            int codContabilidad,
            CntxRepEspecialFiltroDto f,
            string unidad,
            string centroCosto)
        {
            return new
            {
                cod_contabilidad = codContabilidad,
                periodo = f.periodo!.Value,
                periodo_anio = f.periodoAnio!.Value,
                periodo_mes = f.periodoMes!.Value,
                reporte = f.reporte,
                usuario = f.usuario!.Trim(),
                unidad,
                centro_costo = centroCosto
            };
        }
    }
}
