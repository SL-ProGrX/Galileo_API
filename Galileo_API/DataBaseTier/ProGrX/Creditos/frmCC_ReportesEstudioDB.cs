using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public sealed class FrmCcReportesEstudioDB
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrxMain;

        /// <summary>Inicializa el acceso a datos del formulario y a los parámetros globales.</summary>
        /// <param name="config">Configuración usada para resolver las conexiones del API.</param>
        public FrmCcReportesEstudioDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mProGrxMain = new MProGrxMain(config);
        }

        /// <summary>Obtiene los catalogos requeridos por los filtros del formulario.</summary>
        /// <param name="codEmpresa">Codigo de empresa usado para resolver la base de datos cliente.</param>
        /// <returns>Instituciones, estados de persona y clasificaciones de cartera.</returns>
        public ErrorDto<CcReportesEstudioCatalogosResponseDto> CC_ReportesEstudio_Catalogos_Obtener(int codEmpresa)
        {
            try
            {
                var catalogos = new CcReportesEstudioCatalogosResponseDto();
                using var connection = _portalDb.CreateConnection(codEmpresa);
                const string sql = """
                    SELECT CONVERT(varchar(20), cod_institucion) AS item, RTRIM(descripcion) AS descripcion
                    FROM instituciones ORDER BY descripcion;
                    SELECT RTRIM(cod_estado) AS item, RTRIM(descripcion) AS descripcion
                    FROM afi_estados_persona ORDER BY descripcion;
                    SELECT RTRIM(cod_clasificacion) AS item, RTRIM(descripcion) AS descripcion
                    FROM cbr_clasificacion_cartera ORDER BY cod_clasificacion;
                    """;
                using var multi = connection.QueryMultiple(sql);
                catalogos.instituciones = multi.Read<CcReportesEstudioCatalogoDto>().ToList();
                catalogos.estados = multi.Read<CcReportesEstudioCatalogoDto>().ToList();
                catalogos.carteras = multi.Read<CcReportesEstudioCatalogoDto>().ToList();
                return DbHelper.CreateOkResponse(catalogos);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CcReportesEstudioCatalogosResponseDto>(ex.Message);
            }
        }

        /// <summary>Consulta las lineas de credito disponibles para las proyecciones.</summary>
        /// <param name="codEmpresa">Codigo de empresa usado para resolver la base de datos cliente.</param>
        /// <param name="request">Filtros de retencion, cartera, lineas internas y saldo.</param>
        /// <returns>Lineas con descripcion, saldo y cantidad de operaciones.</returns>
        public ErrorDto<List<Dictionary<string, object?>>> CC_ReportesEstudio_Lineas_Obtener(
            int codEmpresa,
            CcReportesEstudioLineasRequestDto request)
        {
            const string sql = """
                SELECT RTRIM(cat.codigo) AS codigo,
                       RTRIM(cat.descripcion) AS descripcion,
                       SUM(reg.saldo) AS saldo,
                       COUNT_BIG(1) AS operaciones
                FROM catalogo cat
                INNER JOIN reg_creditos reg ON
                    (@Retencion = 1 AND
                        ((cat.codigo = reg.codigo AND cat.retencion = 'S') OR cat.poliza = 'S'))
                    OR
                    (@Retencion = 0 AND cat.codigo = reg.codigo
                        AND cat.retencion = 'N' AND cat.poliza = 'N')
                WHERE reg.estado = 'A'
                  AND (@LineasInternas = 0 OR cat.linea_interna = 1)
                  AND (NULLIF(@CodCartera, '') IS NULL OR EXISTS (
                      SELECT 1 FROM cbr_clasificacion_detalle d
                      WHERE d.codigo = cat.codigo AND d.cod_clasificacion = @CodCartera))
                GROUP BY cat.codigo, cat.descripcion
                HAVING (@SoloConSaldo = 0 OR SUM(reg.saldo) > 1)
                ORDER BY cat.codigo;
                """;

            return EjecutarConsulta(codEmpresa, sql, new
            {
                Retencion = request.retencion,
                LineasInternas = request.lineas_internas,
                SoloConSaldo = request.solo_con_saldo,
                CodCartera = request.cod_cartera?.Trim()
            });
        }

        /// <summary>Genera el conjunto tabular correspondiente a la opcion seleccionada.</summary>
        /// <param name="codEmpresa">Codigo de empresa usado para resolver la base de datos cliente.</param>
        /// <param name="usuario">Usuario actual, requerido para recuperar los parametros globales.</param>
        /// <param name="request">Codigo del informe y filtros de consulta.</param>
        /// <returns>Filas con las columnas originales del informe.</returns>
        public ErrorDto<List<Dictionary<string, object?>>> CC_ReportesEstudio_Resultado_Obtener(
            int codEmpresa,
            string usuario,
            CcReportesEstudioGenerarRequestDto request)
        {
            var codigo = request.codigo_reporte.Trim();
            if (codigo.Equals("x07", StringComparison.OrdinalIgnoreCase))
            {
                return DbHelper.CreateErrorResponse<List<Dictionary<string, object?>>>(
                    "El reporte Creditos Activos vrs Ahorros no fue implementado en el formulario original.");
            }

            if (request.rango_proyeccion is < 2 or > 60)
            {
                return DbHelper.CreateErrorResponse<List<Dictionary<string, object?>>>(
                    "El rango de proyeccion debe estar entre 2 y 60.");
            }

            if ((codigo.Equals("H00", StringComparison.OrdinalIgnoreCase)
                    || codigo.Equals("H01", StringComparison.OrdinalIgnoreCase))
                && request.fecha_inicio.Date > request.fecha_corte.Date)
            {
                return DbHelper.CreateErrorResponse<List<Dictionary<string, object?>>>(
                    "La fecha de inicio no puede ser mayor que la fecha de corte.");
            }

            if (codigo.Equals("x15", StringComparison.OrdinalIgnoreCase) && request.frecuencia < 1)
            {
                return DbHelper.CreateErrorResponse<List<Dictionary<string, object?>>>(
                    "La frecuencia de creditos debe ser mayor o igual a 1.");
            }

            var esAseVersion = false;
            decimal fechaCredito = 0;
            if (codigo is "x02" or "x13" or "x13.2")
            {
                var globalesResponse = _mProGrxMain.sbSifParametrosInicializa(codEmpresa, usuario);
                if (globalesResponse == null || globalesResponse.Code < 0 || globalesResponse.Result == null)
                {
                    return DbHelper.CreateErrorResponse<List<Dictionary<string, object?>>>(
                        globalesResponse?.Description ?? "No fue posible obtener los parametros globales.",
                        globalesResponse?.Code ?? -1);
                }

                esAseVersion = globalesResponse.Result.SysASEVersion;
                fechaCredito = globalesResponse.Result.GlngFechaCR;
            }

            var consulta = ResolverConsulta(codigo, request, esAseVersion, fechaCredito);
            if (consulta is null)
            {
                return DbHelper.CreateErrorResponse<List<Dictionary<string, object?>>>("El reporte solicitado no es valido.");
            }

            return EjecutarConsulta(codEmpresa, consulta.Value.Sql, consulta.Value.Parametros);
        }

        /// <summary>Resuelve la consulta y los parámetros correspondientes al reporte solicitado.</summary>
        /// <param name="codigo">Código original asignado al reporte.</param>
        /// <param name="request">Filtros enviados por el formulario.</param>
        /// <param name="esAseVersion">Indica si deben utilizarse unidades de la variante ASE.</param>
        /// <param name="fechaCredito">Fecha numérica requerida para calcular plazos de crédito.</param>
        /// <returns>Consulta parametrizada o <see langword="null"/> cuando el código no existe.</returns>
        private static (string Sql, object Parametros)? ResolverConsulta(
            string codigo,
            CcReportesEstudioGenerarRequestDto request,
            bool esAseVersion,
            decimal fechaCredito)
        {
            var filtros = new
            {
                FechaInicio = request.fecha_inicio.Date,
                FechaCorte = request.fecha_corte.Date.AddDays(1).AddTicks(-1),
                request.rango_proyeccion,
                request.cod_institucion,
                CodEstado = request.cod_estado?.Trim(),
                request.frecuencia,
                Lineas = request.lineas?.Trim(),
                FechaCredito = fechaCredito
            };

            return codigo switch
            {
                "H00" => ("EXEC dbo.spAfi_History_Afiliacion @FechaInicio, @FechaCorte;", filtros),
                "H01" => ("EXEC dbo.spAfi_History_Liquidacion @FechaInicio, @FechaCorte;", filtros),
                "x00" => (SqlProyeccion(false, request.rango_proyeccion), filtros),
                "x01" => ("EXEC dbo.spCrdProyectaCartera @FechaInicio, @rango_proyeccion, 'A';", filtros),
                "x02" => (SqlTasasPlazos(), filtros),
                "x03" => (SqlProyeccion(true, request.rango_proyeccion), filtros),
                "x04" => (SqlEndeudamiento(), filtros),
                "x05" => (SqlAntiguedadAhorros(), filtros),
                "x06" => (SqlAntiguedadSaldos(), filtros),
                "x08" => ("EXEC dbo.spSIFEstudioEndeuda;", filtros),
                "x09" => ("EXEC dbo.spSIFEstudioPersonasSaldos;", filtros),
                "x10" => ("EXEC dbo.spSIFEstudioPatrimonioMemb;", filtros),
                "x11" => ("EXEC dbo.spSIFEstudioPatrimonioInst;", filtros),
                "x12" => ("EXEC dbo.spSIFEstudioEdadesAsociados;", filtros),
                "x13" => (SqlPersonasDeuda(false, esAseVersion), filtros),
                "x13.2" => (SqlPersonasDeuda(true, esAseVersion), filtros),
                "x13.3" => ("EXEC dbo.spCrd_Listado_Fiadores 'A';", filtros),
                "x14" => ("EXEC dbo.spSIFEstudioCarteraCategoria;", filtros),
                "x15" => ("EXEC dbo.spSIFEstudioFrecuenciaCreditos 6, @frecuencia;", filtros),
                "x16.1" => ("SELECT * FROM dbo.vCrd_Disponible_List_sAhorros;", filtros),
                "x16.2" => ("SELECT * FROM dbo.vCrd_Disponible_List_sExcedentes;", filtros),
                "x16.3" => ("SELECT * FROM dbo.vCrd_Disponible_List_sFondos;", filtros),
                _ => null
            };
        }

        /// <summary>Construye la proyección mensual dinámica de cartera o retenciones.</summary>
        /// <param name="retencion">Indica si se proyectan líneas de retención y pólizas.</param>
        /// <param name="periodos">Cantidad de meses que se incluirán como columnas.</param>
        /// <returns>Consulta SQL con las columnas dinámicas de la proyección.</returns>
        private static string SqlProyeccion(bool retencion, int periodos)
        {
            var columnasPeriodos = string.Join(",\n       ", Enumerable.Range(1, periodos).Select(periodo =>
                $"SUM(CASE WHEN periodo = {periodo} THEN saldo ELSE 0 END) AS SALDO{periodo}, " +
                $"SUM(CASE WHEN periodo = {periodo} THEN interes ELSE 0 END) AS INT{periodo}, " +
                $"SUM(CASE WHEN periodo = {periodo} THEN amortiza ELSE 0 END) AS AMORTIZA{periodo}"));

            return $$"""
                WITH Creditos AS (
                    SELECT RTRIM(r.codigo) AS linea, RTRIM(c.descripcion) AS descripcion,
                           CONVERT(decimal(19,4), {{(retencion ? "(r.cuota * r.plazo) - r.amortiza" : "r.saldo")}}) AS saldo,
                           CONVERT(decimal(19,4), r.cuota) AS cuota,
                           CONVERT(decimal(19,8), r.interesv / 1200.0) AS tasa
                    FROM reg_creditos r
                    INNER JOIN catalogo c ON r.codigo = c.codigo
                    INNER JOIN socios s ON r.cedula = s.cedula
                    WHERE r.saldo > 0 AND r.estado = 'A'
                      AND {{(retencion ? "(c.retencion = 'S' OR c.poliza = 'S')" : "c.retencion = 'N' AND c.poliza = 'N'")}}
                      AND (@cod_institucion IS NULL OR s.cod_institucion = @cod_institucion)
                      AND (NULLIF(@Lineas, '') IS NULL
                        OR CHARINDEX(',' + LTRIM(RTRIM(r.codigo)) + ',', ',' + @Lineas + ',') > 0)
                ), Proyeccion AS (
                    SELECT linea, descripcion, 1 AS periodo, saldo,
                           CONVERT(decimal(19,4), saldo * tasa) AS interes,
                           CONVERT(decimal(19,4), CASE WHEN saldo >= cuota - (saldo * tasa)
                               THEN cuota - (saldo * tasa) ELSE saldo END) AS amortiza,
                           CONVERT(decimal(19,4), CASE WHEN saldo >= cuota - (saldo * tasa)
                               THEN saldo - (cuota - (saldo * tasa)) ELSE 0 END) AS saldo_final,
                           cuota, tasa
                    FROM Creditos
                    UNION ALL
                    SELECT linea, descripcion, periodo + 1, saldo_final,
                           CONVERT(decimal(19,4), saldo_final * tasa),
                           CONVERT(decimal(19,4), CASE WHEN saldo_final >= cuota - (saldo_final * tasa)
                               THEN cuota - (saldo_final * tasa) ELSE saldo_final END),
                           CONVERT(decimal(19,4), CASE WHEN saldo_final >= cuota - (saldo_final * tasa)
                               THEN saldo_final - (cuota - (saldo_final * tasa)) ELSE 0 END),
                           cuota, tasa
                    FROM Proyeccion
                    WHERE periodo < @rango_proyeccion AND saldo_final > 0
                )
                SELECT ISNULL(@cod_institucion, 0) AS COD_INSTITUCION,
                       linea AS LINEA,
                       MAX(descripcion) AS DESCRIPCION,
                       CAST('0' AS varchar(100)) AS OFICINA,
                       CAST('0' AS varchar(100)) AS GARANTIA,
                       CAST('0' AS varchar(100)) AS UNIDAD,
                       CAST('0' AS varchar(100)) AS CENTRO_COSTO,
                       CAST('0' AS varchar(100)) AS CUENTA,
                       SUM(CASE WHEN periodo = 1 THEN saldo ELSE 0 END) AS SALDO_INICIAL,
                       0 AS PLAZO,
                       {{columnasPeriodos}},
                       SUM(interes) AS TOTAL_INT,
                       SUM(amortiza) AS TOTAL_AMORTIZA
                FROM Proyeccion
                GROUP BY linea
                ORDER BY linea
                OPTION (MAXRECURSION 60);
                """;
        }

        /// <summary>Obtiene la consulta de tasas y plazos ponderados por línea.</summary>
        /// <returns>Consulta SQL parametrizada.</returns>
        private static string SqlTasasPlazos() => """
            WITH Base AS (
                SELECT r.codigo, MAX(c.descripcion) AS descripcion, SUM(r.saldo) AS saldo, COUNT_BIG(1) AS casos
                FROM reg_creditos r
                INNER JOIN catalogo c ON r.codigo = c.codigo
                INNER JOIN socios s ON r.cedula = s.cedula
                WHERE r.saldo > 0 AND r.estado = 'A' AND r.proceso <> 'J'
                  AND c.retencion = 'N' AND c.poliza = 'N'
                  AND (@cod_institucion IS NULL OR s.cod_institucion = @cod_institucion)
                  AND (NULLIF(@Lineas, '') IS NULL
                    OR CHARINDEX(',' + LTRIM(RTRIM(r.codigo)) + ',', ',' + @Lineas + ',') > 0)
                GROUP BY r.codigo
            )
            SELECT @cod_institucion AS cod_institucion, RTRIM(r.codigo) AS linea, MAX(b.descripcion) AS descripcion,
                   MAX(b.saldo) AS saldo, MAX(b.casos) AS casos,
                   SUM((r.saldo / NULLIF(b.saldo, 0)) * dbo.fxCrdPlazoRestante(r.plazo, r.prideduc, @FechaCredito)) AS plazo,
                   SUM((r.saldo / NULLIF(b.saldo, 0)) * r.interesv) AS tasa
            FROM reg_creditos r INNER JOIN Base b ON r.codigo = b.codigo
            INNER JOIN socios s ON r.cedula = s.cedula
            WHERE r.saldo > 0 AND r.estado = 'A' AND r.proceso <> 'J'
              AND (@cod_institucion IS NULL OR s.cod_institucion = @cod_institucion)
            GROUP BY r.codigo ORDER BY r.codigo;
            """;

        /// <summary>Obtiene el detalle general de endeudamiento por persona.</summary>
        /// <returns>Consulta SQL parametrizada.</returns>
        private static string SqlEndeudamiento() => """
            SELECT s.cedula AS identificacion, s.nombre, s.fechaingreso AS ingreso,
                   est.descripcion AS estado, i.descripcion AS institucion, a.fecahorro AS ultimo_aporte,
                   a.ahorromes AS aporte_mes, a.ahorro AS obrero, a.capitaliza AS capitalizacion,
                   a.aporte AS patronal, ISNULL(SUM(r.saldo), 0) AS saldos, dbo.fxCRDFianzas(s.cedula) AS fianzas
            FROM socios s INNER JOIN instituciones i ON s.cod_institucion = i.cod_institucion
            INNER JOIN afi_estados_persona est ON s.estadoactual = est.cod_estado
            INNER JOIN ahorro_consolidado a ON s.cedula = a.cedula
            LEFT JOIN reg_creditos r ON s.cedula = r.cedula AND r.estado = 'A'
            WHERE (@CodEstado IS NULL OR @CodEstado = '' OR s.estadoactual = @CodEstado)
              AND (@cod_institucion IS NULL OR s.cod_institucion = @cod_institucion)
            GROUP BY s.cedula, s.nombre, s.fechaingreso, est.descripcion, i.descripcion,
                     a.fecahorro, a.ahorromes, a.ahorro, a.capitaliza, a.aporte;
            """;

        /// <summary>Obtiene los rangos de antigüedad de ahorro definidos por el formulario original.</summary>
        /// <returns>Consulta SQL con los rangos 6, 12, 24, 36, 48 y 60+.</returns>
        private static string SqlAntiguedadAhorros() => """
            WITH Rangos AS (
                SELECT 1 AS orden, '6' AS antiguedad
                UNION ALL SELECT 2, '12'
                UNION ALL SELECT 3, '24'
                UNION ALL SELECT 4, '36'
                UNION ALL SELECT 5, '48'
                UNION ALL SELECT 6, '60+'
            ), Base AS (
                SELECT a.cedula, a.ahorro,
                       DATEDIFF(month, s.fechaingreso, dbo.MyGetdate()) AS meses
                FROM ahorro_consolidado a
                INNER JOIN socios s ON a.cedula = s.cedula
                WHERE s.estadoactual = 'S'
            ), Datos AS (
                SELECT cedula, ahorro,
                       CASE WHEN meses <= 6 THEN '6'
                            WHEN meses <= 12 THEN '12'
                            WHEN meses <= 24 THEN '24'
                            WHEN meses <= 36 THEN '36'
                            WHEN meses <= 48 THEN '48'
                            WHEN meses > 60 THEN '60+'
                       END AS antiguedad
                FROM Base
                WHERE meses <= 48 OR meses > 60
            )
            SELECT r.antiguedad, COUNT_BIG(d.cedula) AS casos,
                   ISNULL(SUM(d.ahorro), 0) AS ahorros
            FROM Rangos r
            LEFT JOIN Datos d ON d.antiguedad = r.antiguedad
            GROUP BY r.orden, r.antiguedad
            ORDER BY r.orden;
            """;

        /// <summary>Obtiene los saldos agrupados con el algoritmo original de rangos de ahorro.</summary>
        /// <returns>Consulta SQL con los diez rangos originales.</returns>
        private static string SqlAntiguedadSaldos() => """
            WITH Rangos AS (
                SELECT 1 AS orden, '1000000' AS rango
                UNION ALL SELECT 2, '2000000'
                UNION ALL SELECT 3, '3000000'
                UNION ALL SELECT 4, '4000000'
                UNION ALL SELECT 5, '5000000'
                UNION ALL SELECT 6, '6000000'
                UNION ALL SELECT 7, '7000000'
                UNION ALL SELECT 8, '8000000'
                UNION ALL SELECT 9, '9000000'
                UNION ALL SELECT 10, '10000000+'
            ), Datos AS (
                SELECT a.cedula, a.ahorro, SUM(r.saldo) AS saldo,
                       CASE WHEN a.ahorro >= 1000000 THEN 10
                            ELSE CONVERT(int, LEFT(CONVERT(varchar(30),
                                CONVERT(bigint, FLOOR(a.ahorro))), 1))
                       END AS orden
                FROM ahorro_consolidado a
                INNER JOIN reg_creditos r ON a.cedula = r.cedula
                WHERE a.ahorro > 0 AND r.estado = 'A' AND r.saldo > 0
                GROUP BY a.cedula, a.ahorro
            )
            SELECT r.rango, ISNULL(SUM(d.saldo), 0) AS saldos,
                   COUNT_BIG(d.cedula) AS casos
            FROM Rangos r
            LEFT JOIN Datos d ON d.orden = r.orden
            GROUP BY r.orden, r.rango
            ORDER BY r.orden;
            """;

        /// <summary>Construye el listado de personas con o sin operaciones de crédito activas.</summary>
        /// <param name="conDeuda">Indica si se incluyen personas con deuda o sin deuda.</param>
        /// <param name="esAseVersion">Indica qué estructura organizativa debe proyectarse.</param>
        /// <returns>Consulta SQL parametrizada.</returns>
        private static string SqlPersonasDeuda(bool conDeuda, bool esAseVersion)
        {
            var unidades = esAseVersion
                ? ", u.descripcion AS unidad_programatica, tra.ut_descripcion AS unidad_trabajo"
                : ", dept.descripcion AS departamento, sec.descripcion AS seccion";
            var joins = esAseVersion
                ? " LEFT JOIN uprogramatica u ON s.up = u.codigo LEFT JOIN utrabajo tra ON s.ut = tra.ut_codigo"
                : " LEFT JOIN afdepartamentos dept ON s.cod_institucion = dept.cod_institucion AND s.cod_departamento = dept.cod_departamento LEFT JOIN afsecciones sec ON s.cod_institucion = sec.cod_institucion AND s.cod_departamento = sec.cod_departamento AND s.cod_seccion = sec.cod_seccion";
            var saldo = conDeuda ? ", dbo.fxCrdSaldo(s.cedula) AS saldos" : string.Empty;
            var existencia = conDeuda ? "EXISTS" : "NOT EXISTS";
            return $"""
                SELECT s.cedula AS identificacion, s.nombre, s.fechaingreso AS ingreso, est.descripcion AS estado,
                       a.ahorro, a.capitaliza, a.aporte,
                       CASE WHEN i.porc_ahorro = 0 THEN 0 ELSE ISNULL(a.ahorromes, 0) / (i.porc_ahorro / 100.0) END AS salario,
                       DATEDIFF(year, s.fecha_nac, dbo.MyGetdate()) AS edad{saldo},
                       dbo.fxCRDClasificacion(s.cedula, dbo.MyGetdate()) AS categoria,
                       i.descripcion AS institucion{unidades}, dbo.fxAFITelefono(s.cedula, 1) AS tel_hab,
                       dbo.fxAFITelefono(s.cedula, 2) AS tel_trab, dbo.fxAFITelefono(s.cedula, 3) AS tel_cel
                FROM socios s INNER JOIN instituciones i ON s.cod_institucion = i.cod_institucion
                INNER JOIN afi_estados_persona est ON s.estadoactual = est.cod_estado
                LEFT JOIN ahorro_consolidado a ON s.cedula = a.cedula {joins}
                WHERE (@CodEstado IS NULL OR @CodEstado = '' OR s.estadoactual = @CodEstado)
                  AND (@cod_institucion IS NULL OR s.cod_institucion = @cod_institucion)
                  AND {existencia} (SELECT 1 FROM reg_creditos r INNER JOIN catalogo c ON r.codigo = c.codigo
                      WHERE r.cedula = s.cedula AND r.estado = 'A' AND c.retencion = 'N' AND c.poliza = 'N');
                """;
        }

        /// <summary>Ejecuta una consulta dinámica y conserva los nombres originales de sus columnas.</summary>
        /// <param name="codEmpresa">Código de empresa usado para resolver la conexión.</param>
        /// <param name="sql">Consulta SQL que se ejecutará.</param>
        /// <param name="parametros">Parámetros asociados a la consulta.</param>
        /// <returns>Filas dinámicas dentro de la respuesta estándar del API.</returns>
        private ErrorDto<List<Dictionary<string, object?>>> EjecutarConsulta(int codEmpresa, string sql, object parametros)
        {
            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                var rows = connection.Query(sql, parametros, commandTimeout: 180);
                var resultado = rows.Select(row => ((IDictionary<string, object?>)row)
                    .ToDictionary(column => column.Key, column => column.Value)).ToList();
                return DbHelper.CreateOkResponse(resultado);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<Dictionary<string, object?>>>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<List<Dictionary<string, object?>>>(ex.Message);
            }
        }
    }
}
