using System.Globalization;
using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosTrasladoAsientosDB
    {
        private readonly int vModulo = 36; // Módulo de Activos 
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly MCntLinkDB _mCntLinkDB;
        private readonly PortalDB _portalDB;

        private const string OkMessage                = "Ok";
        private const string MsgTrasladoOk            = "Traslado realizado satisfactoriamente.";
        private const string MsgSinAsientos           = "No se recibieron asientos para trasladar.";
        private const string MsgPeriodoCerradoParcial = "Algunos asientos no se trasladaron porque el período contable está cerrado.";

        private const string SortAsc  = "ASC";
        private const string SortDesc = "DESC";

        private const string LikeWildcard = "%";

        // Bloques WHERE reutilizables (solo constantes → no dispara S2077)
        private const string WhereActivosKey = @"
                            WHERE Cod_Contabilidad = @cc
                              AND Num_Asiento      = @na
                              AND Tipo_Asiento     = @ta";

        private const string WhereActivosDetalleKey = @"
                            WHERE COD_CONTABILIDAD = @cc
                              AND num_asiento      = @na
                              AND tipo_asiento     = @ta";

        // WHERE común para la lista de pendientes (solo constantes)
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

        // Query COUNT (constante)
        private const string SqlActivosCount = @"
SELECT COUNT(1)
FROM Activos_Asientos
" + WhereActivosPendientes + ";";

        // Query SELECT paginada con ORDER BY dinámico pero usando CASE + parámetros (query constante)
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
    -- Orden ascendente
    CASE 
        WHEN @sortOrder = 'ASC' AND @sortField = 'Num_Asiento'      THEN Num_Asiento
        WHEN @sortOrder = 'ASC' AND @sortField = 'Tipo_Asiento'     THEN Tipo_Asiento
        WHEN @sortOrder = 'ASC' AND @sortField = 'Fecha_Asiento'    THEN Fecha_Asiento
        WHEN @sortOrder = 'ASC' AND @sortField = 'Descripcion'      THEN Descripcion
        WHEN @sortOrder = 'ASC' AND @sortField = 'Anio'             THEN Anio
        WHEN @sortOrder = 'ASC' AND @sortField = 'Mes'              THEN Mes
        WHEN @sortOrder = 'ASC' AND @sortField = 'Cod_Contabilidad' THEN Cod_Contabilidad
    END ASC,
    -- Orden descendente
    CASE 
        WHEN @sortOrder = 'DESC' AND @sortField = 'Num_Asiento'      THEN Num_Asiento
        WHEN @sortOrder = 'DESC' AND @sortField = 'Tipo_Asiento'     THEN Tipo_Asiento
        WHEN @sortOrder = 'DESC' AND @sortField = 'Fecha_Asiento'    THEN Fecha_Asiento
        WHEN @sortOrder = 'DESC' AND @sortField = 'Descripcion'      THEN Descripcion
        WHEN @sortOrder = 'DESC' AND @sortField = 'Anio'             THEN Anio
        WHEN @sortOrder = 'DESC' AND @sortField = 'Mes'              THEN Mes
        WHEN @sortOrder = 'DESC' AND @sortField = 'Cod_Contabilidad' THEN Cod_Contabilidad
    END DESC
OFFSET @offset ROWS
FETCH NEXT @fetch ROWS ONLY;";

        // SQL usados en el traslado (todo constante)
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
            _mCntLinkDB      = new MCntLinkDB(config);
            _portalDB        = new PortalDB(config);
        }

        // --------------------------------------------------------
        // Resolución segura del campo de ordenamiento
        // (devuelve tokens que usamos en @sortField del ORDER BY)
        // --------------------------------------------------------
        private static string ResolveSortField(string? sortFieldRaw)
        {
            var key = (sortFieldRaw ?? string.Empty).Trim().ToLowerInvariant();

            return key switch
            {
                "num_asiento"      => "Num_Asiento",
                "tipo_asiento"     => "Tipo_Asiento",
                "fecha_asiento"    => "Fecha_Asiento",
                "descripcion"      => "Descripcion",
                "anio"             => "Anio",
                "mes"              => "Mes",
                "cod_contabilidad" => "Cod_Contabilidad",
                _                  => "Num_Asiento"
            };
        }

        /// <summary>
        /// Lista paginada de asientos de activos pendientes de traslado a contabilidad
        /// </summary>
        public ErrorDto<TablasListaGenericaModel> Activos_TrasladoAsientos_Lista_Obtener(
            int CodEmpresa,
            FiltrosLazyLoadData filtros)
        {
            var resp = new ErrorDto<TablasListaGenericaModel>
            {
                Code        = 0,
                Description = OkMessage,
                Result      = new TablasListaGenericaModel
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

                // Orden usando campo resuelto de forma segura
                var sortField = ResolveSortField(filtros.sortField);
                var sortOrder = filtros.sortOrder == 0 ? SortAsc : SortDesc;

                var pagina     = filtros.pagina     <= 0 ? 1  : filtros.pagina;
                var paginacion = filtros.paginacion <= 0 ? 10 : filtros.paginacion;
                var offset     = pagina <= 1 ? 0 : (pagina - 1) * paginacion;

                var parameters = new DynamicParameters();
                parameters.Add("@todosActivos", todosActivos);
                parameters.Add("@fechaInicio", fechaInicio);
                parameters.Add("@fechaCorte",  fechaCorte);
                parameters.Add("@filtro", string.IsNullOrWhiteSpace(filtroTexto)
                    ? null
                    : LikeWildcard + filtroTexto + LikeWildcard);
                parameters.Add("@sortField", sortField);
                parameters.Add("@sortOrder", sortOrder);
                parameters.Add("@offset",    offset);
                parameters.Add("@fetch",     paginacion);

                // Queries totalmente constantes → Sonar ya no ve SQL dinámico
                resp.Result.total = connection.ExecuteScalar<int>(SqlActivosCount, parameters);

                resp.Result.lista = connection
                    .Query<ActivosTrasladoAsientosDto>(SqlActivosSelect, parameters)
                    .ToList();
            }
            catch (Exception ex)
            {
                resp.Code          = -1;
                resp.Description   = ex.Message;
                resp.Result.total  = 0;
                resp.Result.lista  = null;
            }

            return resp;
        }

        private static (int todosActivos, DateTime? fechaInicio, DateTime? fechaCorte) ParseParametros(object parametros)
        {
            int       todosActivos = 0;
            DateTime? fechaInicio  = null;
            DateTime? fechaCorte   = null;

            var dict = TryDeserializeParametros(parametros);

            if (dict != null)
            {
                todosActivos = ParseTodosActivos(dict);
                fechaInicio  = ParseFecha(dict, "fechaInicio");
                fechaCorte   = ParseFecha(dict, "fechaCorte");
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
        /// Traslada en bloque los asientos seleccionados (igual que el For del VB6).
        /// </summary>
        public ErrorDto<bool> Activos_TrasladoAsientos_Trasladar(
            int CodEmpresa,
            List<ActivosTrasladoAsientoRequest> lista)
        {
            var resp = new ErrorDto<bool>
            {
                Code        = 0,
                Description = MsgTrasladoOk,
                Result      = true
            };

            if (lista == null || !lista.Any())
            {
                resp.Code        = -2;
                resp.Description = MsgSinAsientos;
                resp.Result      = false;
                return resp;
            }

            using var connection = _portalDB.CreateConnection(CodEmpresa);
            var huboPeriodoCerrado = false;

            try
            {
                foreach (var item in lista)
                {
                    if (item == null ||
                        string.IsNullOrWhiteSpace(item.num_asiento) ||
                        string.IsNullOrWhiteSpace(item.tipo_asiento) ||
                        item.cod_contabilidad == 0)
                    {
                        continue;
                    }

                    using var tran = connection.BeginTransaction();

                    try
                    {
                        var fecha = connection.ExecuteScalar<DateTime?>(
                            SqlSelectFechaAsiento,
                            new
                            {
                                cc = item.cod_contabilidad,
                                na = item.num_asiento,
                                ta = item.tipo_asiento
                            },
                            tran);

                        if (fecha == null)
                        {
                            tran.Rollback();
                            continue;
                        }

                        var periodoAbierto = _mCntLinkDB.fxgCntPeriodoValida(CodEmpresa, fecha.Value);
                        if (!periodoAbierto)
                        {
                            tran.Rollback();
                            huboPeriodoCerrado = true;
                            continue;
                        }

                        connection.Execute(
                            SqlInsertMaestro,
                            new
                            {
                                cc     = item.cod_contabilidad,
                                na     = item.num_asiento,
                                ta     = item.tipo_asiento,
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
                                cc      = item.cod_contabilidad,
                                na      = item.num_asiento,
                                ta      = item.tipo_asiento
                            },
                            tran);

                        tran.Commit();

                        _Security_MainDB.Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId         = CodEmpresa,
                            Usuario           = item.usuario,
                            DetalleMovimiento =
                                $"Traslado a contabilidad. COD_CONTABILIDAD={item.cod_contabilidad}, " +
                                $"TIPO_ASIENTO={item.tipo_asiento}, NUM_ASIENTO={item.num_asiento}",
                            Movimiento        = "Trasladar - WEB",
                            Modulo            = vModulo
                        });
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        resp.Code        = -1;
                        resp.Description = ex.Message;
                        resp.Result      = false;
                    }
                }

                if (huboPeriodoCerrado && resp.Code == 0)
                {
                    resp.Description = MsgPeriodoCerradoParcial;
                }
            }
            catch (Exception ex)
            {
                resp.Code        = -1;
                resp.Description = ex.Message;
                resp.Result      = false;
            }

            return resp;
        }
    }
}