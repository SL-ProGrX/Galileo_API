using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad.Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public partial class FrmCntXExploradorContableDb
    {
        /// <summary>
        /// Obtiene cuentas 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cntx_Cuentas_Obtener(int codEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>();

            try
            {
                using var cn = new SqlConnection(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"SELECT cod_cuenta AS Item,
                                   descripcion AS Descripcion
                            FROM CntX_Cuentas
                            WHERE cod_contabilidad = @codEmpresa
                            ORDER BY cod_cuenta";

                response.Result = cn.Query<DropDownListaGenericaModel>(sql,
                    new { codEmpresa }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtiene tipo de asientos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxTipoAsientoDto>> Cntx_TiposAsiento_Obtener(int codEmpresa, int cod_contabilidad)
        {
            var response = new ErrorDto<List<CntxTipoAsientoDto>>();

            try
            {
                using var cn = new SqlConnection(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"SELECT
                        tipo_asiento,
                        descripcion,
                        consecutivo
                            FROM CntX_Tipos_Asientos
                            WHERE cod_contabilidad = @cod_contabilidad
                            ORDER BY tipo_asiento";

                response.Result = cn.Query<CntxTipoAsientoDto>(sql,
                    new { cod_contabilidad }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtiene periodos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_contabilidad"></param>
        /// <param name="estado"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxPeriodoDto>> Cntx_Periodos_Obtener(int codEmpresa, int cod_contabilidad, string estado)
        {
            var response = new ErrorDto<List<CntxPeriodoDto>>();

            try
            {
                using var cn = new SqlConnection(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"SELECT 
                                ANIO AS anio,
                                MES AS mes,
                                PERIODO_CORTE AS fecha_corte,

                                CASE 
                                    WHEN ESTADO = 'C' THEN 'CERRADO'
                                    ELSE 'ABIERTO'
                                END AS cerrado

                            FROM CNTX_PERIODOS
                            WHERE COD_CONTABILIDAD = @cod_contabilidad
                            ORDER BY ANIO, MES";

                response.Result = cn.Query<CntxPeriodoDto>(sql,
                    new { cod_contabilidad, estado }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

  
        /// <summary>
        /// Asientos listar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_contabilidad"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxAsientoRsmDto>> Cntx_Asientos_Listar(int codEmpresa, int cod_contabilidad, CntxExploradorFiltrosDto filtros)
        {
            var response = new ErrorDto<List<CntxAsientoRsmDto>>();

            try
            {
                using var cn = new SqlConnection(_portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sb = new StringBuilder();

                sb.Append(@"
                    SELECT 
                        A.num_asiento,
                        A.tipo_asiento,
                        A.fecha_asiento,
                        A.anio,
                        A.mes,
                        A.ts,
                        A.descripcion,
                        ISNULL(SUM(D.monto_debito),0) AS Debe,
                        ISNULL(SUM(D.monto_credito),0) AS Haber,
                        CASE WHEN A.fecha_aplicado IS NULL THEN 'NO' ELSE 'SI' END AS Aplicado
                    FROM CntX_Asientos A
                    LEFT JOIN CntX_Asientos_Detalle D 
                        ON A.cod_contabilidad = D.cod_contabilidad
                        AND A.tipo_asiento = D.tipo_asiento
                        AND A.num_asiento = D.num_asiento
                    WHERE A.cod_contabilidad = @cod_contabilidad
                ");

                // ==== FILTROS DINÁMICOS ====

                if (!string.IsNullOrWhiteSpace(filtros.cod_tipo_asiento))
                    sb.Append(" AND A.tipo_asiento = @tipo ");

                if (!string.IsNullOrWhiteSpace(filtros.num_asiento))
                    sb.Append(" AND A.num_asiento LIKE '%' + @num + '%' ");

                if (filtros.todas != true && filtros.fecha_desde != null && filtros.fecha_hasta != null)
                {
                    sb.Append(" AND A.fecha_asiento >= @desde AND A.fecha_asiento < DATEADD(day, 1, CAST(@hasta AS date)) ");
                }

                switch (filtros.estado_asiento?.ToUpperInvariant())
                {
                    case "APLICADOS":
                        sb.Append(" AND A.fecha_aplicado IS NOT NULL ");
                        break;
                    case "PENDIENTES":
                        sb.Append(" AND A.fecha_aplicado IS NULL ");
                        break;
                    case "DESBALANCEADOS":
                        sb.Append(" AND A.balanceado = 'N' ");
                        break;
                }

                sb.Append(@"
                    GROUP BY A.num_asiento, A.tipo_asiento, A.fecha_asiento,
                             A.anio, A.mes, A.ts, A.descripcion, A.fecha_aplicado
                    ORDER BY A.fecha_asiento DESC
                ");

                response.Result = cn.Query<CntxAsientoRsmDto>(
                    sb.ToString(),
                    new
                    {
                        codEmpresa,
                        cod_contabilidad,
                        tipo = filtros.cod_tipo_asiento,
                        num = filtros.num_asiento,
                        desde = filtros.fecha_desde,
                        hasta = filtros.fecha_hasta
                    }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Asientos detalle Listar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxAsientoDetDto>> AsientoDetalle_Listar(int codEmpresa, CntxExploradorFiltrosDto filtros)
        {
            var response = new ErrorDto<List<CntxAsientoDetDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
            SELECT 
                D.COD_CUENTA            AS cod_cuenta,
                C.DESCRIPCION           AS cuenta_descripcion,
                D.MONTO_DEBITO          AS monto_debito,
                D.MONTO_CREDITO         AS monto_credito,
                D.DOCUMENTO             AS documento,
                D.DETALLE               AS detalle,
                D.COD_UNIDAD            AS cod_unidad,
                D.COD_CENTRO_COSTO      AS cod_centro_costo,
                D.COD_DIVISA            AS cod_divisa
            FROM CntX_Asientos_Detalle D
            INNER JOIN CntX_Cuentas C
                ON D.COD_CONTABILIDAD = C.COD_CONTABILIDAD
                AND D.COD_CUENTA = C.COD_CUENTA
            WHERE D.COD_CONTABILIDAD = @codContabilidad
              AND D.TIPO_ASIENTO = @tipo
              AND D.NUM_ASIENTO = @numero
            ORDER BY D.NUM_LINEA";

                response.Result = cn.Query<CntxAsientoDetDto>(sql, new
                {
                    codContabilidad = filtros.cod_contabilidad,
                    tipo = filtros.cod_tipo_asiento,
                    numero = filtros.num_asiento
                }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Ejecuta la consulta analítica del explorador con filtros por línea.
        /// </summary>
        public ErrorDto<List<CntxConsultaAnaliticaDto>> ConsultaAnalitica(
            int codEmpresa,
            int codContabilidad,
            CntxExploradorFiltrosDto filtros)
        {
            var response = new ErrorDto<List<CntxConsultaAnaliticaDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = new StringBuilder(@"
                    SELECT TOP (@lineas)
                        A.tipo_asiento,
                        A.num_asiento,
                        A.fecha_asiento,
                        A.descripcion,
                        D.monto_debito,
                        D.monto_credito,
                        CASE WHEN A.fecha_aplicado IS NULL THEN 'NO' ELSE 'SI' END AS aplicado,
                        A.balanceado,
                        C.cod_cuenta_mask,
                        C.descripcion AS cuenta_descripcion,
                        D.cod_unidad,
                        D.cod_centro_costo,
                        D.cod_divisa,
                        D.tipo_cambio,
                        (D.monto_credito + D.monto_debito) /
                            dbo.fxSys_Tipo_Cambio_Apl(D.tipo_cambio) AS importe,
                        D.documento,
                        D.detalle,
                        A.referencia,
                        A.user_crea,
                        A.user_modifica,
                        A.user_autoriza,
                        A.user_aplica
                    FROM CntX_Cuentas C
                    INNER JOIN CntX_Asientos_Detalle D
                        ON C.cod_contabilidad = D.cod_contabilidad
                        AND C.cod_cuenta = D.cod_cuenta
                    INNER JOIN CntX_Asientos A
                        ON D.cod_contabilidad = A.cod_contabilidad
                        AND D.tipo_asiento = A.tipo_asiento
                        AND D.num_asiento = A.num_asiento
                    WHERE C.cod_contabilidad = @codContabilidad");

                if (!string.IsNullOrWhiteSpace(filtros.referencia))
                    sql.Append(" AND A.referencia LIKE '%' + @referencia + '%' ");
                if (!string.IsNullOrWhiteSpace(filtros.tipo))
                    sql.Append(" AND D.tipo_asiento = @tipo ");
                if (!string.IsNullOrWhiteSpace(filtros.num_asiento))
                    sql.Append(" AND D.num_asiento LIKE '%' + @numAsiento + '%' ");
                if (!string.IsNullOrWhiteSpace(filtros.unidad))
                    sql.Append(" AND D.cod_unidad = @unidad ");
                if (!string.IsNullOrWhiteSpace(filtros.cc))
                    sql.Append(" AND D.cod_centro_costo = @centroCosto ");
                if (!string.IsNullOrWhiteSpace(filtros.divisa))
                    sql.Append(" AND D.cod_divisa = @divisa ");
                if (!string.IsNullOrWhiteSpace(filtros.num_documento))
                    sql.Append(" AND D.documento LIKE '%' + @documento + '%' ");
                if (!string.IsNullOrWhiteSpace(filtros.detalle))
                    sql.Append(" AND D.detalle LIKE '%' + @detalle + '%' ");
                if (filtros.todas != true && filtros.fecha_desde.HasValue && filtros.fecha_hasta.HasValue)
                    sql.Append(" AND A.fecha_asiento >= @fechaDesde AND A.fecha_asiento < DATEADD(day, 1, CAST(@fechaHasta AS date)) ");

                var cuentaInicio = NormalizarCuenta(filtros.cuenta_inicio);
                var cuentaCorte = NormalizarCuenta(filtros.cuenta_corte);
                if (cuentaInicio != null && cuentaCorte != null)
                    sql.Append(" AND D.cod_cuenta BETWEEN @cuentaInicio AND @cuentaCorte ");
                else if (cuentaInicio != null)
                    sql.Append(" AND D.cod_cuenta = @cuentaInicio ");
                else if (cuentaCorte != null)
                    sql.Append(" AND D.cod_cuenta = @cuentaCorte ");

                switch (filtros.mov_tipo?.ToUpperInvariant())
                {
                    case "DB":
                        sql.Append(" AND D.monto_debito BETWEEN @movDesde AND @movHasta ");
                        break;
                    case "CR":
                        sql.Append(" AND D.monto_credito BETWEEN @movDesde AND @movHasta ");
                        break;
                    case "TD":
                        sql.Append(" AND (D.monto_debito + D.monto_credito) BETWEEN @movDesde AND @movHasta ");
                        break;
                }

                switch (filtros.estado_asiento?.ToUpperInvariant())
                {
                    case "APLICADOS":
                        sql.Append(" AND A.fecha_aplicado IS NOT NULL ");
                        break;
                    case "PENDIENTES":
                        sql.Append(" AND A.fecha_aplicado IS NULL ");
                        break;
                    case "DESBALANCEADOS":
                        sql.Append(" AND A.balanceado = 'N' ");
                        break;
                }

                sql.Append(" ORDER BY A.fecha_asiento DESC, A.tipo_asiento, A.num_asiento, D.num_linea ");

                response.Result = cn.Query<CntxConsultaAnaliticaDto>(
                    sql.ToString(),
                    new
                    {
                        lineas = Math.Clamp(filtros.lineas ?? 1000, 1, 10000),
                        codContabilidad,
                        filtros.referencia,
                        filtros.tipo,
                        numAsiento = filtros.num_asiento,
                        filtros.unidad,
                        centroCosto = filtros.cc,
                        filtros.divisa,
                        documento = filtros.num_documento,
                        filtros.detalle,
                        fechaDesde = filtros.fecha_desde,
                        fechaHasta = filtros.fecha_hasta,
                        cuentaInicio,
                        cuentaCorte,
                        movDesde = filtros.mov_desde,
                        movHasta = filtros.mov_hasta
                    }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtiene la vista de movimientos correspondiente al nodo activo.
        /// </summary>
        public ErrorDto<CntxMovimientoNodoDto> MovimientosNodo(CntxMovimientoNodoRequest request)
        {
            var response = new ErrorDto<CntxMovimientoNodoDto>
            {
                Result = new CntxMovimientoNodoDto()
            };

            if (!request.cod_empresa.HasValue || request.cod_contabilidad <= 0 ||
                string.IsNullOrWhiteSpace(request.tipo_nodo) || string.IsNullOrWhiteSpace(request.codigo))
            {
                response.Code = -1;
                response.Description = "Los datos del nodo son requeridos";
                return response;
            }

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(request.cod_empresa.Value));

                var tipoNodo = request.tipo_nodo.ToUpperInvariant();
                if (tipoNodo == "CUENTA")
                {
                    request.codigo = NormalizarCuenta(request.codigo);
                    var aceptaMovimientos = cn.QuerySingleOrDefault<bool>(
                        @"SELECT CAST(ISNULL(acepta_movimientos, 0) AS bit)
                          FROM CntX_Cuentas
                          WHERE cod_contabilidad = @cod_contabilidad
                            AND cod_cuenta = @codigo",
                        request);

                    if (aceptaMovimientos)
                    {
                        response.Result!.tipo_vista = "DETALLE_ASIENTOS";
                        response.Result.detalles = cn.Query<CntxMovimientoDetalleDto>(
                            @"SELECT
                                num_asiento,
                                tipo_asiento,
                                detalle,
                                monto_debito,
                                monto_credito,
                                cod_unidad,
                                cod_centro_costo,
                                cod_divisa,
                                tipo_cambio,
                                (monto_credito + monto_debito) /
                                    dbo.fxSys_Tipo_Cambio_Apl(tipo_cambio) AS importe
                              FROM CntX_Asientos_Detalle
                              WHERE cod_contabilidad = @cod_contabilidad
                                AND cod_cuenta = @codigo
                                AND fecha_asiento >= DATEFROMPARTS(@anio, @mes, 1)
                                AND fecha_asiento < DATEADD(month, 1, DATEFROMPARTS(@anio, @mes, 1))
                              ORDER BY fecha_asiento, tipo_asiento, num_asiento, num_linea",
                            request).ToList();
                        return response;
                    }
                }

                var joinMovimientos = tipoNodo == "CUENTA" ? "LEFT JOIN" : "INNER JOIN";
                var sql = new StringBuilder($@"
                    SELECT
                        A.cod_cuenta,
                        A.cod_cuenta_mask AS cuenta,
                        A.descripcion,
                        ISNULL(B.saldo_inicial, 0) AS saldo_inicial,
                        ISNULL(B.total_debitos, 0) AS total_debitos,
                        ISNULL(B.total_creditos, 0) AS total_creditos,
                        ISNULL(B.total_debitos, 0) + ISNULL(B.total_creditos, 0) AS movimiento_mes,
                        ISNULL(B.saldo_inicial, 0) + ISNULL(B.total_debitos, 0) + ISNULL(B.total_creditos, 0) AS saldo_actual,
                        CAST(ISNULL(A.acepta_movimientos, 0) AS bit) AS acepta_movimientos
                    FROM CntX_Cuentas A
                    {joinMovimientos} vCntX_Mov_Cuentas_General B
                        ON A.cod_contabilidad = B.cod_contabilidad
                        AND A.cod_cuenta = B.cod_cuenta
                        AND B.anio = @anio
                        AND B.mes = @mes ");

                if (tipoNodo == "AREA")
                {
                    sql.Append(@"
                        INNER JOIN CntX_Area_Cuentas X
                            ON A.cod_contabilidad = X.cod_contabilidad
                            AND A.cod_cuenta = X.cod_cuenta
                        WHERE X.cod_contabilidad = @cod_contabilidad
                          AND X.cod_area = TRY_CONVERT(int, @codigo) ");
                }
                else if (tipoNodo == "TIPO")
                {
                    sql.Append(@"
                        WHERE A.cod_contabilidad = @cod_contabilidad
                          AND A.tipo_cuenta = @codigo
                          AND ISNULL(A.cuenta_madre, '') = '' ");
                }
                else
                {
                    sql.Append(@"
                        WHERE A.cod_contabilidad = @cod_contabilidad
                          AND A.cuenta_madre = @codigo ");
                }

                sql.Append(" ORDER BY A.cod_cuenta ");
                response.Result!.cuentas = cn.Query<CntxMovimientoCuentaDto>(
                    sql.ToString(), request).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Obtiene la fecha servidor
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>

        public ErrorDto<string> FechaServidor_Obtener(int codEmpresa)
        {
            var response = new ErrorDto<string>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDb.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = "SELECT dbo.MyGetdate()";

                var fechaServidor = cn.QuerySingle<DateTime>(sql);

                response.Description =
                    fechaServidor.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }
    }
}
