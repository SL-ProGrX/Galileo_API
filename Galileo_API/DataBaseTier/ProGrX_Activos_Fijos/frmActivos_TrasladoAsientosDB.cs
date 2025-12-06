using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        private const string OkMessage               = "Ok";
        private const string MsgTrasladoOk           = "Traslado realizado satisfactoriamente.";
        private const string MsgSinAsientos          = "No se recibieron asientos para trasladar.";
        private const string MsgPeriodoCerradoParcial = "Algunos asientos no se trasladaron porque el período contable está cerrado.";

        private const string TableActivosAsientos        = "Activos_Asientos";
        private const string TableActivosAsientosDetalle = "Activos_Asientos_detalle";
        private const string TableCntAsientos            = "CntX_Asientos";
        private const string TableCntAsientosDetalle     = "CntX_Asientos_detalle";

        private const string ColNumAsiento      = "Num_Asiento";
        private const string ColTipoAsiento     = "Tipo_Asiento";
        private const string ColFechaAsiento    = "Fecha_Asiento";
        private const string ColDescripcion     = "Descripcion";
        private const string ColAnio            = "Anio";
        private const string ColMes             = "Mes";
        private const string ColCodContabilidad = "Cod_Contabilidad";
        private const string ColFechaTraslado   = "fecha_traslado";
        private const string ColUserTraslada    = "user_traslada";

        private const string ParamFiltro      = "@filtro";
        private const string ParamFechaInicio = "@fechaInicio";
        private const string ParamFechaCorte  = "@fechaCorte";

        private const string DefaultSortField = ColNumAsiento;

        public FrmActivosTrasladoAsientosDB(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _mCntLinkDB = new MCntLinkDB(config);
            _portalDB = new PortalDB(config);
        }

        // --------------------------------------------------------
        // Resolución segura del campo de ordenamiento (anti S2077)
        // --------------------------------------------------------
        private static string ResolveSortField(string? sortFieldRaw)
        {
            var key = (sortFieldRaw ?? string.Empty).Trim().ToLowerInvariant();

            return key switch
            {
                // posibles nombres desde el front / DTO
                "num_asiento"      => ColNumAsiento,
                "tipo_asiento"     => ColTipoAsiento,
                "fecha_asiento"    => ColFechaAsiento,
                "descripcion"      => ColDescripcion,
                "anio"             => ColAnio,
                "mes"              => ColMes,
                "cod_contabilidad" => ColCodContabilidad,

                // por defecto
                _ => DefaultSortField
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

                var (where, parameters) = BuildWhereClauseAndParameters(
                    todosActivos,
                    fechaInicio,
                    fechaCorte,
                    filtros.filtro ?? string.Empty
                );

                var qTotal = $@"
                    SELECT COUNT(1)
                    FROM {TableActivosAsientos}
                    {where}";

                resp.Result.total = connection.ExecuteScalar<int>(qTotal, parameters);

                // Orden usando switch seguro
                var sortField = ResolveSortField(filtros.sortField);
                var sortOrder = filtros.sortOrder == 0 ? "ASC" : "DESC";

                var pagina = filtros.pagina <= 0 ? 1 : filtros.pagina;
                var paginacion = filtros.paginacion <= 0 ? 10 : filtros.paginacion;
                var offset = pagina <= 1 ? 0 : (pagina - 1) * paginacion;

                parameters.Add("@offset", offset);
                parameters.Add("@fetch", paginacion);

                var qDatos = $@"
                    SELECT
                        {ColNumAsiento}      AS num_asiento,
                        {ColTipoAsiento}     AS tipo_asiento,
                        {ColFechaAsiento}    AS fecha_asiento,
                        {ColDescripcion}     AS descripcion,
                        {ColAnio}            AS anio,
                        {ColMes}             AS mes,
                        {ColCodContabilidad} AS cod_contabilidad
                    FROM {TableActivosAsientos}
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

        private static (string where, DynamicParameters parameters) BuildWhereClauseAndParameters(
            int todosActivos,
            DateTime? fechaInicio,
            DateTime? fechaCorte,
            string filtro)
        {
            var where = $" WHERE {ColFechaTraslado} IS NULL ";
            var p = new DynamicParameters();

            if (todosActivos == 0 && fechaInicio.HasValue && fechaCorte.HasValue)
            {
                where += $" AND {ColFechaAsiento} BETWEEN {ParamFechaInicio} AND {ParamFechaCorte} ";
                p.Add(ParamFechaInicio, fechaInicio.Value.Date);
                p.Add(ParamFechaCorte,  fechaCorte.Value.Date);
            }

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                where += $@"
                AND (
                       {ColNumAsiento}  LIKE {ParamFiltro}
                    OR {ColTipoAsiento} LIKE {ParamFiltro}
                    OR {ColDescripcion} LIKE {ParamFiltro}
                    OR CONVERT(varchar(10), {ColFechaAsiento}, 23) LIKE {ParamFiltro}
                    OR CONVERT(varchar(4), {ColAnio}) LIKE {ParamFiltro}
                    OR CONVERT(varchar(2), {ColMes})  LIKE {ParamFiltro}
                )";
                p.Add(ParamFiltro, "%" + filtro + "%");
            }

            return (where, p);
        }

        /// <summary>
        /// Traslada en bloque los asientos seleccionados (igual que el For del VB6).
        /// </summary>
        public ErrorDto<bool> Activos_TrasladoAsientos_Trasladar(int CodEmpresa, List<ActivosTrasladoAsientoRequest> lista)
        {
            var resp = new ErrorDto<bool>
            {
                Code = 0,
                Description = MsgTrasladoOk,
                Result = true
            };

            if (lista == null || !lista.Any())
            {
                resp.Code = -2;
                resp.Description = MsgSinAsientos;
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
                        var fecha = connection.ExecuteScalar<DateTime?>($@"
                            SELECT {ColFechaAsiento} 
                            FROM {TableActivosAsientos}
                            WHERE {ColCodContabilidad} = @cc
                              AND {ColNumAsiento}      = @na
                              AND {ColTipoAsiento}     = @ta",
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

                        var insertMaestro = $@"
                            INSERT INTO {TableCntAsientos}
                            (COD_CONTABILIDAD, tipo_asiento, num_asiento, anio, mes, fecha_asiento,
                             descripcion, balanceado, notas, referencia, modulo, user_crea)
                            SELECT 
                                {ColCodContabilidad}, {ColTipoAsiento}, {ColNumAsiento}, {ColAnio}, {ColMes}, {ColFechaAsiento},
                                {ColDescripcion}, 'S', ISNULL(Notas,''), ISNULL(Referencia,''), @modulo, user_crea
                            FROM {TableActivosAsientos}
                            WHERE {ColCodContabilidad} = @cc
                              AND {ColNumAsiento}      = @na
                              AND {ColTipoAsiento}     = @ta";

                        connection.Execute(insertMaestro, new
                        {
                            cc = item.cod_contabilidad,
                            na = item.num_asiento,
                            ta = item.tipo_asiento,
                            modulo = vModulo
                        }, tran);

                        var insertDetalle = $@"
                            INSERT INTO {TableCntAsientosDetalle}
                            (num_linea, COD_CONTABILIDAD, tipo_asiento, num_asiento, cod_cuenta,
                             documento, detalle, tipo_cambio, monto_debito, monto_credito,
                             cod_unidad, cod_divisa, cod_centro_costo)
                            SELECT 
                                num_linea, COD_CONTABILIDAD, tipo_asiento, num_asiento, cod_cuenta,
                                documento, detalle, 1, monto_debito, monto_credito,
                                cod_unidad, cod_divisa, cod_centro_costo
                            FROM {TableActivosAsientosDetalle}
                            WHERE COD_CONTABILIDAD = @cc
                              AND num_asiento      = @na
                              AND tipo_asiento     = @ta";

                        connection.Execute(insertDetalle, new
                        {
                            cc = item.cod_contabilidad,
                            na = item.num_asiento,
                            ta = item.tipo_asiento
                        }, tran);

                        var updateOrigen = $@"
                            UPDATE {TableActivosAsientos}
                            SET {ColFechaTraslado} = GETDATE(),
                                {ColUserTraslada}  = @usuario
                            WHERE {ColCodContabilidad} = @cc
                              AND {ColNumAsiento}      = @na
                              AND {ColTipoAsiento}     = @ta";

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
                    resp.Description = MsgPeriodoCerradoParcial;
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