using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndRastreoMovDocDb
    {
        private readonly IConfiguration _config;

        private const string SqlResumen = @"
                ;WITH Resumen AS (
                    SELECT 
                        ISNULL(C.descripcion, '--> No Existe!') AS descripcion,
                        A.fnd_cuenta        AS fnd_Cuenta,
                        A.fnd_debehaber     AS fnd_DebeHaber,
                        SUM(A.fnd_monto)    AS movimiento,
                        'Cola Doc.'         AS origen
                    FROM dbo.fnd_documentos D
                    INNER JOIN dbo.fnd_asientos A 
                        ON D.tipo          = A.tipo
                       AND D.id_documento  = A.id_documento
                       AND D.cod_operadora = A.cod_operadora
                    LEFT JOIN dbo.Cntx_Cuentas C 
                        ON A.fnd_cuenta       = C.cod_cuenta
                       AND C.cod_contabilidad = @CodContabilidad
                    WHERE D.fecha BETWEEN @FechaDesde AND @FechaHasta
                    GROUP BY A.fnd_cuenta, A.fnd_debehaber, C.descripcion

                    UNION ALL

                    SELECT 
                        ISNULL(C.descripcion, '--> No Existe!') AS descripcion,
                        A.fnd_cuenta        AS fnd_Cuenta,
                        A.fnd_debehaber     AS fnd_DebeHaber,
                        SUM(A.fnd_monto)    AS movimiento,
                        'Cola Asientos'     AS origen
                    FROM dbo.fnd_asientos_cola A
                    LEFT JOIN dbo.Cntx_Cuentas C 
                        ON A.fnd_cuenta       = C.cod_cuenta
                       AND C.cod_contabilidad = @CodContabilidad
                    WHERE A.fnd_fecha BETWEEN @FechaDesde AND @FechaHasta
                    GROUP BY A.fnd_cuenta, A.fnd_debehaber, C.descripcion
                )
                SELECT TOP (@Cantidad)
                    descripcion,
                    fnd_Cuenta,
                    fnd_DebeHaber,
                    movimiento,
                    origen,
                    COUNT(*) OVER() AS total
                FROM Resumen
                ORDER BY fnd_Cuenta, fnd_DebeHaber, descripcion, origen, movimiento;";

        private const string SqlDetalle = @"
                SELECT TOP (@Cantidad)
                    X.fecha,
                    X.tipo,
                    X.referencia,
                    X.fnd_Cuenta,
                    X.mDebe,
                    X.mHaber,
                    X.concepto,
                    X.cliente,
                    X.descripcion,
                    X.usuario,
                    X.extra,
                    X.origen
                FROM (
                    SELECT
                        D.fecha AS fecha,
                        A.tipo AS tipo,
                        CAST(D.id_documento AS varchar(50)) AS referencia,
                        A.fnd_cuenta AS fnd_Cuenta,
                        CASE WHEN A.fnd_debehaber = 'D' THEN A.fnd_monto ELSE 0 END AS mDebe,
                        CASE WHEN A.fnd_debehaber <> 'D' THEN A.fnd_monto ELSE 0 END AS mHaber,
                        D.concepto AS concepto,
                        D.cliente AS cliente,
                        O.descripcion AS descripcion,
                        D.usuario AS usuario,
                        '' AS extra,
                        'DOC_ASIENTO' AS origen
                    FROM dbo.fnd_documentos D
                    INNER JOIN dbo.fnd_asientos A 
                        ON D.tipo = A.tipo
                       AND D.id_documento = A.id_documento
                       AND D.cod_operadora = A.cod_operadora
                    INNER JOIN dbo.fnd_operadoras O 
                        ON O.cod_operadora = D.cod_operadora
                    WHERE D.fecha BETWEEN @FechaDesde AND @FechaHasta

                    UNION ALL

                    SELECT
                        P.fecha AS fecha,
                        'PRM' AS tipo,
                        CONVERT(varchar(10), P.fecha_proceso, 120) AS referencia,
                        ISNULL(CASE WHEN P.opex = 1 THEN C.CTAOAMORT ELSE C.CTANAMORT END, '') AS fnd_Cuenta,
                        0.0 AS mDebe,
                        P.amortiza AS mHaber,
                        'Ded.Pla : ' + CONVERT(varchar(10), P.fecha_proceso, 120) AS concepto,
                        RTRIM(P.cedula) + ' - ' + ISNULL(S.Nombre, '') AS cliente,
                        RTRIM(P.codigo) + ' Op.' + CAST(P.id_solicitud AS varchar(20)) + ' Ex.' + CAST(P.opex AS varchar(10)) AS descripcion,
                        'Ded.Pla' AS usuario,
                        CAST(ISNULL(Cnt.cod_plan, '') AS varchar(50)) AS extra,
                        'PRM' AS origen
                    FROM dbo.prm_creditos P
                    LEFT JOIN dbo.Socios S 
                        ON P.cedula = S.cedula
                    LEFT JOIN dbo.catalogo C 
                        ON P.codigo = C.codigo
                    INNER JOIN dbo.fnd_planes F 
                        ON P.codigo = F.codigo_ase
                    LEFT JOIN dbo.fnd_contratos Cnt 
                        ON P.id_solicitud = Cnt.Operacion
                    WHERE P.IND_PASO = 1
                      AND P.amortiza > 0
                      AND P.fecha BETWEEN @FechaDesde AND @FechaHasta

                    UNION ALL

                    SELECT
                        A.fnd_fecha AS fecha,
                        A.fnd_tipo AS tipo,
                        'Cola de Asientos' AS referencia,
                        A.fnd_cuenta AS fnd_Cuenta,
                        CASE WHEN A.fnd_debehaber = 'D' THEN A.fnd_monto ELSE 0 END AS mDebe,
                        CASE WHEN A.fnd_debehaber <> 'D' THEN A.fnd_monto ELSE 0 END AS mHaber,
                        UPPER(
                            CASE UPPER(A.fnd_tipo)
                                WHEN 'LI' THEN 'LIQUIDACION'
                                WHEN 'RT' THEN 'RETIROS'
                                WHEN 'RE' THEN 'RENDIMIENTOS'
                                WHEN 'CR' THEN 'RENDIMIENTOS'
                                WHEN 'PM' THEN 'PROCESO MENSUAL'
                                WHEN 'PR' THEN 'PROCESO PLANILLA'
                                WHEN 'RL' THEN 'REV.LIQUIDACION'
                                ELSE 'No Identificado!'
                            END
                        ) + ' - ' + CAST(A.fnd_caso AS varchar(50)) AS concepto,
                        RTRIM(Cnt.Cedula) + ' - ' + ISNULL(S.nombre, '--> No Existe!') AS cliente,
                        'Operadora : ' + CAST(A.cod_operadora AS varchar(20)) + ' Plan : ' + CAST(A.cod_plan AS varchar(20)) + ' - Contrato : ' + CAST(A.cod_contrato AS varchar(20)) AS descripcion,
                        'N/A' AS usuario,
                        CAST(A.cod_plan AS varchar(50)) AS extra,
                        'COLA_ASIENTO' AS origen
                    FROM dbo.fnd_asientos_cola A
                    LEFT JOIN dbo.CntX_Cuentas C 
                        ON A.fnd_cuenta = C.cod_cuenta
                       AND C.cod_Contabilidad = @CodContabilidad
                    INNER JOIN dbo.Fnd_Contratos Cnt 
                        ON A.cod_operadora = Cnt.Cod_Operadora
                       AND A.cod_Plan = Cnt.Cod_Plan
                       AND A.cod_Contrato = Cnt.Cod_Contrato
                    LEFT JOIN dbo.Socios S 
                        ON Cnt.Cedula = S.cedula
                    WHERE A.fnd_fecha BETWEEN @FechaDesde AND @FechaHasta
                      AND A.fnd_tipo NOT IN ('PRM')
                ) AS X
                ORDER BY X.fecha, X.tipo, X.referencia;";

        private const string SqlArchivo = @"
                SELECT TOP (@Cantidad)
                    X.Fecha,
                    X.Tipo,
                    X.Referencia,
                    X.Cuenta,
                    X.Debe,
                    X.Haber,
                    X.Concepto,
                    X.Cliente,
                    X.Descripcion,
                    X.Usuario,
                    X.Extra
                FROM (
                    SELECT
                        D.fecha AS Fecha,
                        A.tipo AS Tipo,
                        CAST(D.id_documento AS varchar(50)) AS Referencia,
                        A.fnd_cuenta AS Cuenta,
                        CASE WHEN A.fnd_debehaber = 'D' THEN A.fnd_monto ELSE 0 END AS Debe,
                        CASE WHEN A.fnd_debehaber <> 'D' THEN A.fnd_monto ELSE 0 END AS Haber,
                        D.concepto AS Concepto,
                        D.cliente AS Cliente,
                        O.descripcion AS Descripcion,
                        D.usuario AS Usuario,
                        '' AS Extra
                    FROM dbo.fnd_documentos D
                    INNER JOIN dbo.fnd_asientos A 
                        ON D.tipo = A.tipo
                       AND D.id_documento = A.id_documento
                       AND D.cod_operadora = A.cod_operadora
                    INNER JOIN dbo.fnd_operadoras O 
                        ON O.cod_operadora = D.cod_operadora
                    WHERE D.fecha BETWEEN @FechaDesde AND @FechaHasta

                    UNION ALL

                    SELECT
                        P.fecha AS Fecha,
                        'PRM' AS Tipo,
                        CONVERT(varchar(10), P.fecha_proceso, 120) AS Referencia,
                        ISNULL(CASE WHEN P.opex = 1 THEN C.CTAOAMORT ELSE C.CTANAMORT END, '') AS Cuenta,
                        0.0 AS Debe,
                        P.amortiza AS Haber,
                        'Ded.Pla : ' + CONVERT(varchar(10), P.fecha_proceso, 120) AS Concepto,
                        RTRIM(P.cedula) + ' - ' + ISNULL(S.Nombre, '') AS Cliente,
                        RTRIM(P.codigo) + ' Op.' + CAST(P.id_solicitud AS varchar(20)) + ' Ex.' + CAST(P.opex AS varchar(10)) AS Descripcion,
                        'Ded.Pla' AS Usuario,
                        CAST(ISNULL(Cnt.cod_plan, '') AS varchar(50)) AS Extra
                    FROM dbo.prm_creditos P
                    LEFT JOIN dbo.Socios S 
                        ON P.cedula = S.cedula
                    LEFT JOIN dbo.catalogo C 
                        ON P.codigo = C.codigo
                    INNER JOIN dbo.fnd_planes F 
                        ON P.codigo = F.codigo_ase
                    LEFT JOIN dbo.fnd_contratos Cnt 
                        ON P.id_solicitud = Cnt.Operacion
                    WHERE P.IND_PASO = 1
                      AND P.amortiza > 0
                      AND P.fecha BETWEEN @FechaDesde AND @FechaHasta

                    UNION ALL

                    SELECT
                        A.fnd_fecha AS Fecha,
                        A.fnd_tipo AS Tipo,
                        'Cola Asientos' AS Referencia,
                        A.fnd_cuenta AS Cuenta,
                        CASE WHEN A.fnd_debehaber = 'D' THEN A.fnd_monto ELSE 0 END AS Debe,
                        CASE WHEN A.fnd_debehaber <> 'D' THEN A.fnd_monto ELSE 0 END AS Haber,
                        UPPER(
                            CASE UPPER(A.fnd_tipo)
                                WHEN 'LIQ' THEN 'LIQUIDACION'
                                WHEN 'RET' THEN 'RETIROS'
                                WHEN 'REN' THEN 'RENDIMIENTOS'
                                WHEN 'PRM' THEN 'PROCESO MENSUAL'
                                ELSE 'No Identificado!'
                            END
                        ) + ' - ' + CAST(A.fnd_caso AS varchar(50)) AS Concepto,
                        RTRIM(Cnt.Cedula) + ' - ' + ISNULL(S.nombre, '--> No Existe!') AS Cliente,
                        'Operadora : ' + CAST(A.cod_operadora AS varchar(20)) + ' Plan : ' + CAST(A.cod_plan AS varchar(20)) + ' - Contrato : ' + CAST(A.cod_contrato AS varchar(20)) AS Descripcion,
                        'N/A' AS Usuario,
                        CAST(A.cod_plan AS varchar(50)) AS Extra
                    FROM dbo.fnd_asientos_cola A
                    INNER JOIN dbo.Fnd_Contratos Cnt
                        ON A.cod_operadora = Cnt.Cod_Operadora
                       AND A.cod_Plan = Cnt.Cod_Plan
                       AND A.cod_Contrato = Cnt.Cod_Contrato
                    LEFT JOIN dbo.Socios S
                        ON Cnt.Cedula = S.cedula
                    WHERE A.fnd_fecha BETWEEN @FechaDesde AND @FechaHasta
                      AND A.fnd_tipo <> 'PRM'
                ) AS X
                ORDER BY X.Fecha, X.Tipo, X.Referencia;";

        public FrmFndRastreoMovDocDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtener resumen rastreo de movimiento de documentos y asientos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<FndRastreoMovDocResumenData>> FND_RastreoMovDoc_Resumen_Obtener(int CodEmpresa, FndRastreoMovDocFiltros Filtros)
        {
            if (Filtros is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros de rastreo son requeridos.",
                    -2,
                    new List<FndRastreoMovDocResumenData>());
            }

            return DbHelper.ExecuteListQuery<FndRastreoMovDocResumenData>(
                new PortalDB(_config),
                CodEmpresa,
                SqlResumen,
                CrearParametros(Filtros, 100));
        }

        /// <summary>
        /// Obtener detalle rastreo de movimiento de documentos y asientos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<FndRastreoMovDocDetalleData>> FND_RastreoMovDoc_Detalle_Obtener(int CodEmpresa, FndRastreoMovDocFiltros Filtros)
        {
            if (Filtros is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros de rastreo son requeridos.",
                    -2,
                    new List<FndRastreoMovDocDetalleData>());
            }

            return DbHelper.ExecuteListQuery<FndRastreoMovDocDetalleData>(
                new PortalDB(_config),
                CodEmpresa,
                SqlDetalle,
                CrearParametros(Filtros, 100));
        }

        /// <summary>
        /// Obtener información para generacion de archivo de rastreo de movimiento de documentos y asientos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<FndRastreoMovDocArchivosData>> FND_RastreoMovDoc_Archivo_Obtener(int CodEmpresa, FndRastreoMovDocFiltros Filtros)
        {
            if (Filtros is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros de rastreo son requeridos.",
                    -2,
                    new List<FndRastreoMovDocArchivosData>());
            }

            return DbHelper.ExecuteListQuery<FndRastreoMovDocArchivosData>(
                new PortalDB(_config),
                CodEmpresa,
                SqlArchivo,
                CrearParametros(Filtros, 1000));
        }

        private static object CrearParametros(FndRastreoMovDocFiltros filtros, int cantidadDefault)
        {
            return new
            {
                FechaDesde = filtros.fecha_inicio.Date,
                FechaHasta = filtros.fecha_corte.Date.AddDays(1).AddTicks(-1),
                CodContabilidad = filtros.cod_contabilidad,
                Cantidad = Math.Clamp(filtros.lineas > 0 ? filtros.lineas : cantidadDefault, 1, 10000)
            };
        }
    }
}