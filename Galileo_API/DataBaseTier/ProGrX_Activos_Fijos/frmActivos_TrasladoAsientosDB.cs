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
        private const string _numAsiento = "Num_Asiento";

        // Lista blanca de columnas para ORDER BY
        private static readonly Dictionary<string, string> SortFieldMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                // Nombres de columnas en la tabla
                { _numAsiento, _numAsiento },
                { "Tipo_Asiento", "Tipo_Asiento" },
                { "Fecha_Asiento", "Fecha_Asiento" },
                { "Descripcion", "Descripcion" },
                { "Anio", "Anio" },
                { "Mes", "Mes" },
                { "Cod_Contabilidad", "Cod_Contabilidad" },

                // Posibles nombres desde el front / DTO
                { "num_asiento", _numAsiento },
                { "tipo_asiento", "Tipo_Asiento" },
                { "fecha_asiento", "Fecha_Asiento" },
                { "descripcion", "Descripcion" },
                { "anio", "Anio" },
                { "mes", "Mes" },
                { "cod_contabilidad", "Cod_Contabilidad" }
            };

        public FrmActivosTrasladoAsientosDB(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _mCntLinkDB = new MCntLinkDB(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Lista paginada de asientos de activos pendientes de traslado a contabilidad
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> Activos_TrasladoAsientos_Lista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var resp = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new TablasListaGenericaModel
                {
                    total = 0,
                    lista = new List<ActivosTrasladoAsientosDto>()
                }
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var (todosActivos, fechaInicio, fechaCorte) = ParseParametros(filtros.parametros ?? new object());
                var (where, parameters) = BuildWhereClauseAndParameters(
                    todosActivos,
                    fechaInicio,
                    fechaCorte,
                    filtros.filtro ?? string.Empty
                );

                var qTotal = $@"SELECT COUNT(1)
                        FROM Activos_Asientos
                        {where}";

                resp.Result.total = connection.ExecuteScalar<int>(qTotal, parameters);

                // Orden usando lista blanca
                var sortFieldKey = string.IsNullOrWhiteSpace(filtros.sortField)
                    ? _numAsiento
                    : filtros.sortField!;
                if (!SortFieldMap.TryGetValue(sortFieldKey, out var sortField))
                    sortField = _numAsiento;

                var sortOrder = filtros.sortOrder == 0 ? "ASC" : "DESC";

                var pagina = filtros.pagina <= 0 ? 1 : filtros.pagina;
                var paginacion = filtros.paginacion <= 0 ? 10 : filtros.paginacion;
                var offset = (pagina <= 1 ? 0 : (pagina - 1) * paginacion);

                parameters.Add("@offset", offset);
                parameters.Add("@fetch", paginacion);

                var qDatos = $@"
            SELECT
                Num_Asiento       AS num_asiento,
                Tipo_Asiento      AS tipo_asiento,
                Fecha_Asiento     AS fecha_asiento,
                Descripcion       AS descripcion,
                Anio              AS anio,
                Mes               AS mes,
                Cod_Contabilidad  AS cod_contabilidad
            FROM Activos_Asientos
            {where}
            ORDER BY {sortField} {sortOrder}
            OFFSET @offset ROWS
            FETCH NEXT @fetch ROWS ONLY;";

                resp.Result.lista = connection
                    .Query<ActivosTrasladoAsientosDto>(qDatos, parameters)
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
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(parametrosStr);
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
                    return DateTime.Parse(fechaStr, System.Globalization.CultureInfo.InvariantCulture);
            }
            return null;
        }

        private static (string where, DynamicParameters parameters) BuildWhereClauseAndParameters(
            int todosActivos,
            DateTime? fechaInicio,
            DateTime? fechaCorte,
            string filtro)
        {
            var where = " WHERE fecha_traslado IS NULL ";
            var p = new DynamicParameters();

            if (todosActivos == 0 && fechaInicio.HasValue && fechaCorte.HasValue)
            {
                where += " AND fecha_asiento BETWEEN @fechaInicio AND @fechaCorte ";
                p.Add("@fechaInicio", fechaInicio.Value.Date);
                p.Add("@fechaCorte", fechaCorte.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                where += @"
                AND (
                       Num_Asiento  LIKE @filtro
                    OR Tipo_Asiento LIKE @filtro
                    OR Descripcion  LIKE @filtro
                    OR CONVERT(varchar(10), Fecha_Asiento, 23) LIKE @filtro
                    OR CONVERT(varchar(4), Anio) LIKE @filtro
                    OR CONVERT(varchar(2), Mes) LIKE @filtro
                )";
                p.Add("@filtro", "%" + filtro + "%");
            }

            return (where, p);
        }

        /// <summary>
        /// Traslada en bloque los asientos seleccionados (igual que el For del VB6).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="lista"></param>
        /// <returns></returns>
        public ErrorDto<bool> Activos_TrasladoAsientos_Trasladar(int CodEmpresa, List<ActivosTrasladoAsientoRequest> lista)
        {
            var resp = new ErrorDto<bool>
            {
                Code = 0,
                Description = "Traslado realizado satisfactoriamente.",
                Result = true
            };

            if (lista == null || !lista.Any())
            {
                resp.Code = -2;
                resp.Description = "No se recibieron asientos para trasladar.";
                resp.Result = false;
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
                        var fecha = connection.ExecuteScalar<DateTime?>(@"
                    SELECT fecha_asiento 
                    FROM Activos_Asientos
                    WHERE COD_CONTABILIDAD = @cc
                      AND num_asiento      = @na
                      AND tipo_asiento     = @ta",
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

                        var insertMaestro = @"
                    INSERT INTO CntX_Asientos
                    (COD_CONTABILIDAD, tipo_asiento, num_asiento, anio, mes, fecha_asiento,
                     descripcion, balanceado, notas, referencia, modulo, user_crea)
                    SELECT 
                        COD_CONTABILIDAD, tipo_asiento, num_asiento, anio, mes, fecha_asiento,
                        descripcion, 'S', ISNULL(Notas,''), ISNULL(Referencia,''), @modulo, user_crea
                    FROM Activos_Asientos
                    WHERE COD_CONTABILIDAD = @cc
                      AND num_asiento      = @na
                      AND tipo_asiento     = @ta";

                        connection.Execute(insertMaestro, new
                        {
                            cc = item.cod_contabilidad,
                            na = item.num_asiento,
                            ta = item.tipo_asiento,
                            modulo = vModulo
                        }, tran);

                        var insertDetalle = @"
                    INSERT INTO CntX_Asientos_detalle
                    (num_linea, COD_CONTABILIDAD, tipo_asiento, num_asiento, cod_cuenta,
                     documento, detalle, tipo_cambio, monto_debito, monto_credito,
                     cod_unidad, cod_divisa, cod_centro_costo)
                    SELECT 
                        num_linea, COD_CONTABILIDAD, tipo_asiento, num_asiento, cod_cuenta,
                        documento, detalle, 1, monto_debito, monto_credito,
                        cod_unidad, cod_divisa, cod_centro_costo
                    FROM Activos_Asientos_detalle
                    WHERE COD_CONTABILIDAD = @cc
                      AND num_asiento      = @na
                      AND tipo_asiento     = @ta";

                        connection.Execute(insertDetalle, new
                        {
                            cc = item.cod_contabilidad,
                            na = item.num_asiento,
                            ta = item.tipo_asiento
                        }, tran);

                        var updateOrigen = @"
                    UPDATE Activos_Asientos
                    SET fecha_traslado = GETDATE(),
                        user_traslada  = @usuario
                    WHERE COD_CONTABILIDAD = @cc
                      AND num_asiento      = @na
                      AND tipo_asiento     = @ta";

                        connection.Execute(updateOrigen, new
                        {
                            usuario = item.usuario,
                            cc = item.cod_contabilidad,
                            na = item.num_asiento,
                            ta = item.tipo_asiento
                        }, tran);

                        tran.Commit();

                        _Security_MainDB.Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = item.usuario,
                            DetalleMovimiento =
                                $"Traslado a contabilidad. COD_CONTABILIDAD={item.cod_contabilidad}, " +
                                $"TIPO_ASIENTO={item.tipo_asiento}, NUM_ASIENTO={item.num_asiento}",
                            Movimiento = "Trasladar - WEB",
                            Modulo = vModulo
                        });
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        resp.Code = -1;
                        resp.Description = ex.Message;
                        resp.Result = false;
                    }
                }

                if (huboPeriodoCerrado && resp.Code == 0)
                {
                    resp.Description = "Algunos asientos no se trasladaron porque el período contable está cerrado.";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = false;
            }

            return resp;
        }
    }
}