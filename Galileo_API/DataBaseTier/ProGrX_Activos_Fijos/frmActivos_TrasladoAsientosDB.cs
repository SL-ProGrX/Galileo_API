using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using System.Globalization;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosTrasladoAsientosDB
    {
        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly MCntLinkDB _mCntLinkDB;
        private readonly PortalDB _portalDB;
        private const string OkMessage = "Ok";
        private const string MsgTrasladoOk = "Traslado realizado satisfactoriamente.";
        private const string MsgSinAsientos = "No se recibieron asientos para trasladar.";
        private const string MsgPeriodoCerradoParcial = "Algunos asientos no se trasladaron porque el período contable no existe o está cerrado.";
        private const string MsgPeriodoCerradoTotal = "No se trasladaron asientos porque el período contable no existe o está cerrado.";
        private const string SortAsc = "ASC";
        private const string SortDesc = "DESC";
        private const string LikeWildcard = "%";
        private const string WhereActivosKey = @"
                            WHERE Cod_Contabilidad = @cc
                              AND Num_Asiento      = @na
                              AND Tipo_Asiento     = @ta";
        private const string WhereActivosDetalleKey = @"
                            WHERE COD_CONTABILIDAD = @cc
                              AND num_asiento      = @na
                              AND tipo_asiento     = @ta";
        private const string WhereActivosPendientes = @"
                WHERE fecha_traslado IS NULL
                  AND (
                        @todosActivos = 1
                        OR (
                            @todosActivos = 0
                            AND @fechaInicio IS NOT NULL
                            AND @fechaCorte  IS NOT NULL
                            AND Fecha_Asiento BETWEEN @fechaInicio AND @fechaCorte
                        )
                      )
                  AND (
                        @filtro IS NULL
                        OR Num_Asiento LIKE @filtro
                        OR Tipo_Asiento LIKE @filtro
                        OR Descripcion LIKE @filtro
                        OR CONVERT(varchar(10), Fecha_Asiento, 23) LIKE @filtro
                        OR CONVERT(varchar(4), Anio) LIKE @filtro
                        OR CONVERT(varchar(2), Mes)  LIKE @filtro
                      )";
        private const string SqlActivosCount = @"
                SELECT COUNT(1)
                FROM Activos_Asientos
                " + WhereActivosPendientes + ";";

        private const string SqlActivosSelect = @"
            SELECT
                Num_Asiento      AS num_asiento,
                Tipo_Asiento     AS tipo_asiento,
                Fecha_Asiento    AS fecha_asiento,
                Descripcion      AS descripcion,
                Anio             AS anio,
                Mes              AS mes,
                Cod_Contabilidad AS cod_contabilidad
            FROM Activos_Asientos
            " + WhereActivosPendientes + @"
            ORDER BY
                CASE WHEN @sortField = 'Num_Asiento'      AND @sortOrder = 'ASC'  THEN Num_Asiento END ASC,
                CASE WHEN @sortField = 'Num_Asiento'      AND @sortOrder = 'DESC' THEN Num_Asiento END DESC,

                CASE WHEN @sortField = 'Tipo_Asiento'     AND @sortOrder = 'ASC'  THEN Tipo_Asiento END ASC,
                CASE WHEN @sortField = 'Tipo_Asiento'     AND @sortOrder = 'DESC' THEN Tipo_Asiento END DESC,

                CASE WHEN @sortField = 'Fecha_Asiento'    AND @sortOrder = 'ASC'  THEN Fecha_Asiento END ASC,
                CASE WHEN @sortField = 'Fecha_Asiento'    AND @sortOrder = 'DESC' THEN Fecha_Asiento END DESC,

                CASE WHEN @sortField = 'Descripcion'      AND @sortOrder = 'ASC'  THEN Descripcion END ASC,
                CASE WHEN @sortField = 'Descripcion'      AND @sortOrder = 'DESC' THEN Descripcion END DESC,

                CASE WHEN @sortField = 'Anio'             AND @sortOrder = 'ASC'  THEN Anio END ASC,
                CASE WHEN @sortField = 'Anio'             AND @sortOrder = 'DESC' THEN Anio END DESC,

                CASE WHEN @sortField = 'Mes'              AND @sortOrder = 'ASC'  THEN Mes END ASC,
                CASE WHEN @sortField = 'Mes'              AND @sortOrder = 'DESC' THEN Mes END DESC,

                CASE WHEN @sortField = 'Cod_Contabilidad' AND @sortOrder = 'ASC'  THEN Cod_Contabilidad END ASC,
                CASE WHEN @sortField = 'Cod_Contabilidad' AND @sortOrder = 'DESC' THEN Cod_Contabilidad END DESC,

                Num_Asiento ASC
            OFFSET @offset ROWS
            FETCH NEXT @fetch ROWS ONLY;";

        private const string SqlSelectFechaAsiento = @"
            SELECT Fecha_Asiento 
            FROM Activos_Asientos
            " + WhereActivosKey + ";";

        private const string SqlInsertMaestro = @"
            INSERT INTO CntX_Asientos
            (COD_CONTABILIDAD, tipo_asiento, num_asiento, anio, mes, fecha_asiento,
             descripcion, balanceado, notas, referencia, modulo, user_crea)
            SELECT 
                Cod_Contabilidad, Tipo_Asiento, Num_Asiento, Anio, Mes, Fecha_Asiento,
                Descripcion, 'S', ISNULL(Notas,''), ISNULL(Referencia,''), @modulo, user_crea
            FROM Activos_Asientos
            " + WhereActivosKey + ";";

        private const string SqlInsertDetalle = @"
            INSERT INTO CntX_Asientos_detalle
            (num_linea, COD_CONTABILIDAD, tipo_asiento, num_asiento, cod_cuenta,
             documento, detalle, tipo_cambio, monto_debito, monto_credito,
             cod_unidad, cod_divisa, cod_centro_costo)
            SELECT 
                num_linea, COD_CONTABILIDAD, tipo_asiento, num_asiento, cod_cuenta,
                documento, detalle, 1, monto_debito, monto_credito,
                cod_unidad, cod_divisa, cod_centro_costo
            FROM Activos_Asientos_detalle
            " + WhereActivosDetalleKey + ";";

        private const string SqlUpdateOrigen = @"
            UPDATE Activos_Asientos
            SET fecha_traslado = GETDATE(),
                user_traslada  = @usuario
            " + WhereActivosKey + ";";
        public FrmActivosTrasladoAsientosDB(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _mCntLinkDB = new MCntLinkDB(config);
            _portalDB = new PortalDB(config);
        }
        private static string ResolveSortField(string? sortFieldRaw)
        {
            var key = (sortFieldRaw ?? string.Empty).Trim().ToLowerInvariant();

            return key switch
            {
                "num_asiento" => "Num_Asiento",
                "tipo_asiento" => "Tipo_Asiento",
                "fecha_asiento" => "Fecha_Asiento",
                "descripcion" => "Descripcion",
                "anio" => "Anio",
                "mes" => "Mes",
                "cod_contabilidad" => "Cod_Contabilidad",
                _ => "Num_Asiento"
            };
        }
        /// <summary>
        /// Lista paginada de asientos de activos pendientes de traslado a contabilidad
        /// </summary>
        public ErrorDto<TablasListaGenericaModel> Activos_TrasladoAsientos_Lista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var resp = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Description = OkMessage,
                Result = new TablasListaGenericaModel
                {
                    total = 0,
                    lista = new List<ActivosTrasladoAsientosDto>()
                }
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var (todosActivos, fechaInicio, fechaCorte) =
                    ParseParametros(filtros.parametros ?? new object());

                var filtroTexto = filtros.filtro ?? string.Empty;
                var sortField = ResolveSortField(filtros.sortField);
                var sortOrder = filtros.sortOrder == 0 ? SortAsc : SortDesc;

                var pagina = filtros.pagina <= 0 ? 1 : filtros.pagina;
                var paginacion = filtros.paginacion <= 0 ? 10 : filtros.paginacion;
                var offset = pagina <= 1 ? 0 : (pagina - 1) * paginacion;

                var parameters = new DynamicParameters();
                parameters.Add("@todosActivos", todosActivos);
                parameters.Add("@fechaInicio", fechaInicio);
                parameters.Add("@fechaCorte", fechaCorte);
                parameters.Add("@filtro", string.IsNullOrWhiteSpace(filtroTexto)
                    ? null
                    : LikeWildcard + filtroTexto + LikeWildcard);
                parameters.Add("@sortField", sortField);
                parameters.Add("@sortOrder", sortOrder);
                parameters.Add("@offset", offset);
                parameters.Add("@fetch", paginacion);
                resp.Result.total = connection.ExecuteScalar<int>(SqlActivosCount, parameters);

                resp.Result.lista = connection
                    .Query<ActivosTrasladoAsientosDto>(SqlActivosSelect, parameters)
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result.total = 0;
                resp.Result.lista = null;
            }

            return resp;
        }
        private static (int todosActivos, DateTime? fechaInicio, DateTime? fechaCorte) ParseParametros(object parametros)
        {
            int todosActivos = 0;
            DateTime? fechaInicio = null;
            DateTime? fechaCorte = null;

            var dict = TryDeserializeParametros(parametros);

            if (dict != null)
            {
                todosActivos = ParseTodosActivos(dict);
                fechaInicio = ParseFecha(dict, "fechaInicio");
                fechaCorte = ParseFecha(dict, "fechaCorte");
            }

            return (todosActivos, fechaInicio, fechaCorte);
        }
        private static Dictionary<string, object>? TryDeserializeParametros(object parametros)
        {
            if (parametros == null) return null;

            var parametrosStr = parametros.ToString();
            if (string.IsNullOrWhiteSpace(parametrosStr)) return null;

            return Newtonsoft.Json.JsonConvert
                .DeserializeObject<Dictionary<string, object>>(parametrosStr);
        }
        private static int ParseTodosActivos(Dictionary<string, object> dict)
        {
            if (dict.ContainsKey("todosActivos"))
                return Convert.ToInt32(dict["todosActivos"]);
            return 0;
        }
        private static DateTime? ParseFecha(Dictionary<string, object> dict, string key)
        {
            if (dict.ContainsKey(key) && dict[key] != null)
            {
                var fechaStr = dict[key]?.ToString();
                if (!string.IsNullOrWhiteSpace(fechaStr))
                    return DateTime.Parse(fechaStr, CultureInfo.InvariantCulture);
            }
            return null;
        }
        /// <summary>
        /// Traslada en bloque los asientos seleccionados.
        /// </summary>
        public ErrorDto<bool> Activos_TrasladoAsientos_Trasladar(
            int CodEmpresa,
            List<ActivosTrasladoAsientoRequest> lista)
        {
            if (lista == null || !lista.Any())
                return CrearErrorTraslado(MsgSinAsientos, -2);

            using var connection = _portalDB.CreateConnection(CodEmpresa);

            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            var resumen = new TrasladoAsientosResumen();

            foreach (var item in lista)
            {
                var respError = ProcesarResultadoTraslado(CodEmpresa, connection, item, resumen);

                if (respError != null)
                    return respError;
            }

            return CrearRespuestaTraslado(resumen);
        }
        private sealed class TrasladoAsientosResumen
        {
            public int Trasladados { get; set; }
            public int PeriodoCerrado { get; set; }
        }

        private ErrorDto<bool>? ProcesarResultadoTraslado(
            int codEmpresa,
            SqlConnection connection,
            ActivosTrasladoAsientoRequest item,
            TrasladoAsientosResumen resumen)
        {
            try
            {
                var resultado = TrasladarAsientoItem(
                    codEmpresa,
                    connection,
                    item);

                ActualizarResumenTraslado(resultado, resumen);

                return null;
            }
            catch (SqlException ex)
            {
                return CrearErrorTraslado(ex.Message, -1);
            }
            catch (InvalidOperationException ex)
            {
                return CrearErrorTraslado(ex.Message, -1);
            }
        }

        private static void ActualizarResumenTraslado(
            TrasladoAsientoResultado resultado,
            TrasladoAsientosResumen resumen)
        {
            if (resultado == TrasladoAsientoResultado.Trasladado)
                resumen.Trasladados++;

            if (resultado == TrasladoAsientoResultado.PeriodoCerrado)
                resumen.PeriodoCerrado++;
        }

        private static ErrorDto<bool> CrearRespuestaTraslado(TrasladoAsientosResumen resumen)
        {
            var descripcion = MsgTrasladoOk;

            if (resumen.PeriodoCerrado > 0 && resumen.Trasladados > 0)
                descripcion = MsgPeriodoCerradoParcial;

            if (resumen.PeriodoCerrado > 0 && resumen.Trasladados == 0)
                descripcion = MsgPeriodoCerradoTotal;

            return new ErrorDto<bool>
            {
                Code = 0,
                Description = descripcion,
                Result = true
            };
        }
        private enum TrasladoAsientoResultado
        {
            Omitido,
            Trasladado,
            PeriodoCerrado
        }
        private static ErrorDto<bool> CrearErrorTraslado(string mensaje, int code)
        {
            return new ErrorDto<bool>
            {
                Code = code,
                Description = mensaje,
                Result = false
            };
        }
        private TrasladoAsientoResultado TrasladarAsientoItem(int codEmpresa,SqlConnection connection,ActivosTrasladoAsientoRequest item)
        {
            if (item == null ||
                string.IsNullOrWhiteSpace(item.num_asiento) ||
                string.IsNullOrWhiteSpace(item.tipo_asiento) ||
                item.cod_contabilidad == 0)
            {
                return TrasladoAsientoResultado.Omitido;
            }

            var fecha = connection.ExecuteScalar<DateTime?>(
                SqlSelectFechaAsiento,
                new
                {
                    cc = item.cod_contabilidad,
                    na = item.num_asiento,
                    ta = item.tipo_asiento
                });

            if (!fecha.HasValue)
                return TrasladoAsientoResultado.Omitido;

            var periodoAbierto = _mCntLinkDB.fxgCntPeriodoValida(
                codEmpresa,
                fecha.Value);

            if (!periodoAbierto)
                return TrasladoAsientoResultado.PeriodoCerrado;

            using (var tran = connection.BeginTransaction())
            {
                try
                {
                    connection.Execute(
                        SqlInsertMaestro,
                        new
                        {
                            cc = item.cod_contabilidad,
                            na = item.num_asiento,
                            ta = item.tipo_asiento,
                            modulo = vModulo
                        },
                        tran);

                    connection.Execute(
                        SqlInsertDetalle,
                        new
                        {
                            cc = item.cod_contabilidad,
                            na = item.num_asiento,
                            ta = item.tipo_asiento
                        },
                        tran);

                    connection.Execute(
                        SqlUpdateOrigen,
                        new
                        {
                            usuario = item.usuario,
                            cc = item.cod_contabilidad,
                            na = item.num_asiento,
                            ta = item.tipo_asiento
                        },
                        tran);

                    tran.Commit();
                }
                catch (SqlException ex)
                {
                    if (tran.Connection != null)
                        tran.Rollback();

                    throw new InvalidOperationException(
                        $"No fue posible trasladar el asiento {item.num_asiento}. " +
                        "Verifique que el asiento tenga todas sus cuentas contables configuradas.",
                        ex);
                }
            }

            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = item.usuario,
                DetalleMovimiento =
                    $"Traslado a contabilidad. COD_CONTABILIDAD={item.cod_contabilidad}, " +
                    $"TIPO_ASIENTO={item.tipo_asiento}, NUM_ASIENTO={item.num_asiento}",
                Movimiento = "Trasladar - WEB",
                Modulo = vModulo
            });

            return TrasladoAsientoResultado.Trasladado;
        }
    }
}