using ClosedXML.Excel;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;
using static Galileo_API.Models.ProGrX_Contabilidad.FrmCntxConInformeEspecialModels;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXRepEspecialesDb
    {
        private const string ExcelContentType =
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

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
        /// Genera el Excel de balance por centro de costo o por unidad (equivale a sbInformeCentroCostos / sbInformeUENS del VB6).
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa conectada.</param>
        /// <param name="codContabilidad">Código de la contabilidad activa.</param>
        /// <param name="f">Filtros del reporte, incluido el período contable vigente.</param>
        /// <returns>Archivo Excel generado con el primer result set del procedimiento.</returns>
        public ErrorDto<ArchivoGeneradoModel> GenerarExcel(
            int codEmpresa,
            int codContabilidad,
            CntxRepEspecialFiltroDto f)
        {
            var validacion = ValidarFiltrosExcel(f);
            if (validacion is not null)
            {
                return DbHelper.CreateErrorResponse(validacion, -1, new ArchivoGeneradoModel());
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                return DbHelper.CreateOkResponse(
                    GenerarArchivoBalanceExcel(connection, codContabilidad, f));
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse(
                    "Error al generar el informe en Excel.",
                    -1,
                    new ArchivoGeneradoModel());
            }
        }

        /// <summary>
        /// Valida los filtros que requieren los informes Excel de balance.
        /// </summary>
        /// <param name="f">Filtros recibidos desde la pantalla.</param>
        /// <returns>Mensaje de error o null cuando los filtros son válidos.</returns>
        private static string? ValidarFiltrosExcel(CntxRepEspecialFiltroDto f)
        {
            if (f.periodoAnio is null or <= 0 || f.periodoMes is null or < 1 or > 12)
            {
                return "El año y mes del período contable son requeridos.";
            }

            if (string.IsNullOrWhiteSpace(f.usuario))
            {
                return "El usuario es requerido.";
            }

            if (f.reporte is not ("4" or "4.1" or "5" or "5.1"))
            {
                return "El tipo de reporte Excel no es válido.";
            }

            if (f.reporte is "5" or "5.1" && !int.TryParse(f.nivel, out _))
            {
                return "El nivel es requerido para el balance por unidad.";
            }

            return null;
        }

        /// <summary>
        /// Ejecuta el procedimiento del informe seleccionado y lo convierte en un archivo Excel.
        /// </summary>
        /// <param name="connection">Conexión abierta de la empresa.</param>
        /// <param name="codContabilidad">Código de la contabilidad activa.</param>
        /// <param name="f">Filtros del reporte.</param>
        /// <returns>Archivo Excel listo para descargar.</returns>
        private static ArchivoGeneradoModel GenerarArchivoBalanceExcel(
            SqlConnection connection,
            int codContabilidad,
            CntxRepEspecialFiltroDto f)
        {
            var (sql, parametros, nombre) = PrepararProcedimientoExcel(codContabilidad, f);

            using var reader = connection.ExecuteReader(sql, parametros, commandTimeout: 0);

            var encabezados = new List<string>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                encabezados.Add(reader.GetName(i));
            }

            var filas = new List<object?[]>();
            while (reader.Read())
            {
                var fila = new object?[reader.FieldCount];
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    fila[i] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }

                filas.Add(fila);
            }

            using var libro = new XLWorkbook();
            var hoja = libro.Worksheets.Add("Balance");

            for (var c = 0; c < encabezados.Count; c++)
            {
                AsignarValorCelda(hoja.Cell(1, c + 1), encabezados[c]);
            }

            hoja.Row(1).Style.Font.Bold = true;

            for (var r = 0; r < filas.Count; r++)
            {
                for (var c = 0; c < encabezados.Count; c++)
                {
                    AsignarValorCelda(hoja.Cell(r + 2, c + 1), filas[r][c]);
                }
            }

            hoja.SheetView.FreezeRows(1);
            hoja.Columns().AdjustToContents();

            using var memoria = new MemoryStream();
            libro.SaveAs(memoria);

            return new ArchivoGeneradoModel
            {
                NombreArchivo = $"{nombre}_{f.periodoAnio:0000}{f.periodoMes:00}.xlsx",
                ContentType = ExcelContentType,
                ArchivoBytes = memoria.ToArray(),
            };
        }

        /// <summary>
        /// Define el procedimiento, sus parámetros posicionales y el nombre del archivo según el informe (VB6: exec spCntX_Balance_*).
        /// </summary>
        /// <param name="codContabilidad">Código de la contabilidad activa.</param>
        /// <param name="f">Filtros del reporte.</param>
        /// <returns>SQL de ejecución, parámetros y nombre base del archivo.</returns>
        private static (string Sql, object Parametros, string Nombre) PrepararProcedimientoExcel(
            int codContabilidad,
            CntxRepEspecialFiltroDto f)
        {
            var anio = f.periodoAnio ?? 0;
            var mes = f.periodoMes ?? 0;
            var usuario = f.usuario?.Trim() ?? string.Empty;

            return f.reporte switch
            {
                "4" => (
                    "EXEC spCntX_Balance_Centro_Costos_Vertical @cod_contabilidad, @periodo_anio, @periodo_mes, @usuario",
                    new { cod_contabilidad = codContabilidad, periodo_anio = anio, periodo_mes = mes, usuario },
                    "Balance_CentroCosto_Vertical"),
                "4.1" => (
                    "EXEC spCntX_Balance_Centro_Costos_Horizontal @cod_contabilidad, @periodo_anio, @periodo_mes, @usuario",
                    new { cod_contabilidad = codContabilidad, periodo_anio = anio, periodo_mes = mes, usuario },
                    "Balance_CentroCosto_Horizontal"),
                "5" => (
                    "EXEC spCntX_Balance_UENS_Vertical @cod_contabilidad, @periodo_anio, @periodo_mes, @nivel, @usuario",
                    new { cod_contabilidad = codContabilidad, periodo_anio = anio, periodo_mes = mes, nivel = int.Parse(f.nivel ?? "0"), usuario },
                    "Balance_Unidad_Vertical"),
                _ => (
                    "EXEC spCntX_Balance_UENS_Horizontal @cod_contabilidad, @periodo_anio, @periodo_mes, @nivel, @usuario",
                    new { cod_contabilidad = codContabilidad, periodo_anio = anio, periodo_mes = mes, nivel = int.Parse(f.nivel ?? "0"), usuario },
                    "Balance_Unidad_Horizontal"),
            };
        }

        /// <summary>
        /// Asigna un valor de lectura de base de datos a una celda, conservando el tipo para el formato de Excel.
        /// </summary>
        /// <param name="celda">Celda destino.</param>
        /// <param name="valor">Valor leído.</param>
        private static void AsignarValorCelda(IXLCell celda, object? valor)
        {
            celda.Value = valor switch
            {
                null => (XLCellValue)string.Empty,
                decimal numeroDecimal => (XLCellValue)numeroDecimal,
                double numeroDouble => (XLCellValue)numeroDouble,
                int numeroEntero => (XLCellValue)numeroEntero,
                short numeroCorto => (XLCellValue)numeroCorto,
                long numeroLargo => (XLCellValue)numeroLargo,
                bool valorBooleano => (XLCellValue)valorBooleano,
                DateTime fecha => (XLCellValue)fecha,
                _ => (XLCellValue)(valor.ToString() ?? string.Empty),
            };
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

            const string sql = @"
            DELETE CntX_Rep_Periodos_mov
            WHERE usuario = @usuario;

            DECLARE @cuenta_utilidad varchar(100) = (
                SELECT TOP (1) RTRIM(Cuenta_GanPer)
                FROM CNTX_CIERRES
                WHERE ID_CIERRE = @periodo
                  AND cod_contabilidad = @cod_contabilidad
            );

            IF NULLIF(@cuenta_utilidad, '') IS NULL
                THROW 50001,
                    'El cierre seleccionado no tiene cuenta de ganancias y pérdidas.',
                    1;

            INSERT INTO CntX_Rep_Periodos_mov
                (
                    cod_cuenta, usuario, cod_contabilidad,
                    movimiento_10, movimiento_11, movimiento_12,
                    movimiento_01, movimiento_02, movimiento_03,
                    movimiento_04, movimiento_05, movimiento_06,
                    movimiento_07, movimiento_08, movimiento_09
                )
            VALUES
                (
                    @cuenta_utilidad, @usuario, @cod_contabilidad,
                    0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
                );

            INSERT INTO CntX_Rep_Periodos_mov
                (
                    cod_cuenta, usuario, cod_contabilidad,
                    movimiento_10, movimiento_11, movimiento_12,
                    movimiento_01, movimiento_02, movimiento_03,
                    movimiento_04, movimiento_05, movimiento_06,
                    movimiento_07, movimiento_08, movimiento_09
                )
            SELECT
                C.cod_cuenta, @usuario, @cod_contabilidad,
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
                    OR (@reporte = '3'
                        AND T.clasificacion IN ('I', 'V', 'C', 'G', 'P', 'A'))
              );

            DECLARE @fecha_inicial date =
                DATEADD(
                    month,
                    -2,
                    DATEFROMPARTS(@periodo_anio, @periodo_mes, 1)
                );

            ;WITH MovimientosBase AS
            (
                SELECT
                    M.cod_cuenta,
                    M.Saldo_Inicial,
                    M.Total_Debitos,
                    M.Total_Creditos,
                    M.anio,
                    M.mes
                FROM vCntX_Mov_Cuentas_General M
                WHERE @unidad = ''
                  AND @centro_costo = ''
                  AND M.cod_contabilidad = @cod_contabilidad

                UNION ALL

                SELECT
                    M.cod_cuenta,
                    M.Saldo_Inicial,
                    M.Total_Debitos,
                    M.Total_Creditos,
                    M.anio,
                    M.mes
                FROM vCntX_Mov_Cuentas_CentroCosto M
                WHERE @unidad = ''
                  AND @centro_costo <> ''
                  AND M.cod_contabilidad = @cod_contabilidad
                  AND M.cod_centro_costo = @centro_costo

                UNION ALL

                SELECT
                    M.cod_cuenta,
                    M.Saldo_Inicial,
                    M.Total_Debitos,
                    M.Total_Creditos,
                    M.anio,
                    M.mes
                FROM vCntX_Mov_Cuentas_Unidad M
                WHERE @unidad <> ''
                  AND @centro_costo = ''
                  AND M.cod_contabilidad = @cod_contabilidad
                  AND M.cod_unidad = @unidad

                UNION ALL

                SELECT
                    M.cod_cuenta,
                    M.Saldo_Inicial,
                    M.Total_Debitos,
                    M.Total_Creditos,
                    M.anio,
                    M.mes
                FROM CntX_Mov_Cuentas_Detallado M
                WHERE @unidad <> ''
                  AND @centro_costo <> ''
                  AND M.cod_contabilidad = @cod_contabilidad
                  AND M.cod_unidad = @unidad
                  AND M.cod_centro_costo = @centro_costo
            ),
            Movimientos AS
            (
                SELECT
                    M.cod_cuenta,
                    SUM(
                        CASE
                            WHEN M.anio = YEAR(@fecha_inicial)
                             AND M.mes = MONTH(@fecha_inicial)
                            THEN M.Total_Debitos + M.Total_Creditos
                            ELSE 0
                        END
                    ) AS movimiento_01,
                    SUM(
                        CASE
                            WHEN M.anio = YEAR(DATEADD(month, 1, @fecha_inicial))
                             AND M.mes = MONTH(DATEADD(month, 1, @fecha_inicial))
                            THEN M.Total_Debitos + M.Total_Creditos
                            ELSE 0
                        END
                    ) AS movimiento_02,
                    SUM(
                        CASE
                            WHEN M.anio = YEAR(DATEADD(month, 2, @fecha_inicial))
                             AND M.mes = MONTH(DATEADD(month, 2, @fecha_inicial))
                            THEN M.Total_Debitos + M.Total_Creditos
                            ELSE 0
                        END
                    ) AS movimiento_03,
                    SUM(
                        CASE
                            WHEN M.anio = YEAR(@fecha_inicial)
                             AND M.mes = MONTH(@fecha_inicial)
                            THEN M.Saldo_Inicial
                                 + M.Total_Debitos
                                 + M.Total_Creditos
                            ELSE 0
                        END
                    ) AS movimiento_04,
                    SUM(
                        CASE
                            WHEN M.anio = YEAR(DATEADD(month, 1, @fecha_inicial))
                             AND M.mes = MONTH(DATEADD(month, 1, @fecha_inicial))
                            THEN M.Saldo_Inicial
                                 + M.Total_Debitos
                                 + M.Total_Creditos
                            ELSE 0
                        END
                    ) AS movimiento_05,
                    SUM(
                        CASE
                            WHEN M.anio = YEAR(DATEADD(month, 2, @fecha_inicial))
                             AND M.mes = MONTH(DATEADD(month, 2, @fecha_inicial))
                            THEN M.Saldo_Inicial
                                 + M.Total_Debitos
                                 + M.Total_Creditos
                            ELSE 0
                        END
                    ) AS movimiento_06
                FROM MovimientosBase M
                WHERE
                    (
                        M.anio = YEAR(@fecha_inicial)
                        AND M.mes = MONTH(@fecha_inicial)
                    )
                    OR
                    (
                        M.anio = YEAR(DATEADD(month, 1, @fecha_inicial))
                        AND M.mes = MONTH(DATEADD(month, 1, @fecha_inicial))
                    )
                    OR
                    (
                        M.anio = YEAR(DATEADD(month, 2, @fecha_inicial))
                        AND M.mes = MONTH(DATEADD(month, 2, @fecha_inicial))
                    )
                GROUP BY M.cod_cuenta
            )
            UPDATE R
            SET movimiento_01 = M.movimiento_01,
                movimiento_02 = M.movimiento_02,
                movimiento_03 = M.movimiento_03,
                movimiento_04 = M.movimiento_04,
                movimiento_05 = M.movimiento_05,
                movimiento_06 = M.movimiento_06
            FROM CntX_Rep_Periodos_mov R
            INNER JOIN Movimientos M
                ON M.cod_cuenta = R.cod_cuenta
            WHERE R.usuario = @usuario
              AND R.cod_contabilidad = @cod_contabilidad;

            UPDATE CntX_Rep_Periodos_mov
            SET movimiento_01 = movimiento_01
                    + dbo.fxCntX_UtilidadMes(
                        YEAR(@fecha_inicial),
                        MONTH(@fecha_inicial),
                        @cod_contabilidad,
                        @unidad,
                        @centro_costo),
                movimiento_02 = movimiento_02
                    + dbo.fxCntX_UtilidadMes(
                        YEAR(DATEADD(month, 1, @fecha_inicial)),
                        MONTH(DATEADD(month, 1, @fecha_inicial)),
                        @cod_contabilidad,
                        @unidad,
                        @centro_costo),
                movimiento_03 = movimiento_03
                    + dbo.fxCntX_UtilidadMes(
                        YEAR(DATEADD(month, 2, @fecha_inicial)),
                        MONTH(DATEADD(month, 2, @fecha_inicial)),
                        @cod_contabilidad,
                        @unidad,
                        @centro_costo),
                movimiento_04 = movimiento_04
                    + dbo.fxCntX_Utilidad(
                        YEAR(@fecha_inicial),
                        MONTH(@fecha_inicial),
                        @cod_contabilidad,
                        @unidad,
                        @centro_costo),
                movimiento_05 = movimiento_05
                    + dbo.fxCntX_Utilidad(
                        YEAR(DATEADD(month, 1, @fecha_inicial)),
                        MONTH(DATEADD(month, 1, @fecha_inicial)),
                        @cod_contabilidad,
                        @unidad,
                        @centro_costo),
                movimiento_06 = movimiento_06
                    + dbo.fxCntX_Utilidad(
                        YEAR(DATEADD(month, 2, @fecha_inicial)),
                        MONTH(DATEADD(month, 2, @fecha_inicial)),
                        @cod_contabilidad,
                        @unidad,
                        @centro_costo)
            WHERE usuario = @usuario
              AND cod_contabilidad = @cod_contabilidad
              AND cod_cuenta IN (
                  SELECT cuenta
                  FROM dbo.fxCntX_CuentasCascada(
                      @cod_contabilidad,
                      @cuenta_utilidad)
              );

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
            const string sqlCentroCosto = @"
                DELETE CNTX_REP_PERIODOS_MOV_UNIDAD
                WHERE usuario = @usuario;

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
                WHERE @centro_costo <> '';

                DECLARE @fecha_inicial date = DATEADD(month, -2, DATEFROMPARTS(@periodo_anio, @periodo_mes, 1));

                UPDATE R
                SET movimiento_01 = dbo.fxCntX_UtilidadDetallada(
                        YEAR(@fecha_inicial), MONTH(@fecha_inicial),
                        R.cod_contabilidad, R.cod_unidad, R.cod_centro_costo, 'N'),
                    movimiento_02 = dbo.fxCntX_UtilidadDetallada(
                        YEAR(DATEADD(month, 1, @fecha_inicial)),
                        MONTH(DATEADD(month, 1, @fecha_inicial)),
                        R.cod_contabilidad, R.cod_unidad, R.cod_centro_costo, 'N'),
                    movimiento_03 = dbo.fxCntX_UtilidadDetallada(
                        YEAR(DATEADD(month, 2, @fecha_inicial)),
                        MONTH(DATEADD(month, 2, @fecha_inicial)),
                        R.cod_contabilidad, R.cod_unidad, R.cod_centro_costo, 'N'),
                    movimiento_04 = dbo.fxCntX_UtilidadDetallada(
                        @periodo_anio, @periodo_mes,
                        R.cod_contabilidad, R.cod_unidad, R.cod_centro_costo, 'A')
                FROM CNTX_REP_PERIODOS_MOV_UNIDAD R
                WHERE R.usuario = @usuario
                  AND R.cod_contabilidad = @cod_contabilidad;";

            const string sqlUnidad = @"
                DELETE CNTX_REP_PERIODOS_MOV_UNIDAD
                WHERE usuario = @usuario;

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
                WHERE @centro_costo <> '';

                DECLARE @fecha_inicial date = DATEADD(month, -2, DATEFROMPARTS(@periodo_anio, @periodo_mes, 1));

                UPDATE R
                SET movimiento_01 = dbo.fxCntX_UtilidadDetallada(
                        YEAR(@fecha_inicial), MONTH(@fecha_inicial),
                        R.cod_contabilidad, R.cod_unidad, R.cod_centro_costo, 'N'),
                    movimiento_02 = dbo.fxCntX_UtilidadDetallada(
                        YEAR(DATEADD(month, 1, @fecha_inicial)),
                        MONTH(DATEADD(month, 1, @fecha_inicial)),
                        R.cod_contabilidad, R.cod_unidad, R.cod_centro_costo, 'N'),
                    movimiento_03 = dbo.fxCntX_UtilidadDetallada(
                        YEAR(DATEADD(month, 2, @fecha_inicial)),
                        MONTH(DATEADD(month, 2, @fecha_inicial)),
                        R.cod_contabilidad, R.cod_unidad, R.cod_centro_costo, 'N'),
                    movimiento_04 = dbo.fxCntX_UtilidadDetallada(
                        @periodo_anio, @periodo_mes,
                        R.cod_contabilidad, R.cod_unidad, R.cod_centro_costo, 'A')
                FROM CNTX_REP_PERIODOS_MOV_UNIDAD R
                WHERE R.usuario = @usuario
                  AND R.cod_contabilidad = @cod_contabilidad;";

            var parametros = CrearParametros(codContabilidad, f, unidad, centroCosto);
            if (f.reporte == "2.1")
            {
                connection.Execute(sqlCentroCosto, parametros, transaction, commandTimeout: 0);
                return;
            }

            connection.Execute(sqlUnidad, parametros, transaction, commandTimeout: 0);
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
            ArgumentNullException.ThrowIfNull(f);

            var periodo = f.periodo
                ?? throw new ArgumentException("El período es requerido.", nameof(f));
            var periodoAnio = f.periodoAnio
                ?? throw new ArgumentException("El año del período es requerido.", nameof(f));
            var periodoMes = f.periodoMes
                ?? throw new ArgumentException("El mes del período es requerido.", nameof(f));

            if (string.IsNullOrWhiteSpace(f.usuario))
            {
                throw new ArgumentException("El usuario es requerido.", nameof(f));
            }

            var usuario = f.usuario.Trim();

            return new
            {
                cod_contabilidad = codContabilidad,
                periodo,
                periodo_anio = periodoAnio,
                periodo_mes = periodoMes,
                reporte = f.reporte,
                usuario,
                unidad,
                centro_costo = centroCosto
            };
        }
    }
}
