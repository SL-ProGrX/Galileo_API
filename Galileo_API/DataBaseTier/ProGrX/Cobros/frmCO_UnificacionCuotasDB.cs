using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCOUnificacionCuotasDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly MAfilicacionDB _mAfiliacion;
        private readonly int vModulo = 4;
        private const string CODIGO = "codigo";
        private const string CEDULA = "cedula";
        private const string CUOTA = "cuota";

        public FrmCOUnificacionCuotasDB(IConfiguration config)
            : this(
                new PortalDB(config),
                new MSecurityMainDb(config),
                new MAfilicacionDB(config))
        {
        }

        public FrmCOUnificacionCuotasDB(
            PortalDB portalDB,
            MSecurityMainDb securityMainDb,
            MAfilicacionDB mAfiliacion)
        {
            _portalDB = portalDB;
            _securityMainDb = securityMainDb;
            _mAfiliacion = mAfiliacion;
        }
        /// <summary>
        /// Obtiene lista de códigos (catálogo) para buscador (F4).
        /// <param name="CodEmpresa"></param>
        /// <param name="texto"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CO_UnificacionCuotas_Codigos_Obtener(int CodEmpresa, string? texto)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                texto = (texto ?? string.Empty).Trim();
                var like = texto.Length > 0 ? $"%{texto}%" : null;

                const string sql = @"
                    select
                        rtrim(CODIGO)      as item,
                        rtrim(DESCRIPCION) as descripcion
                    from dbo.CATALOGO
                    where (@texto = '' or CODIGO like @like or DESCRIPCION like @like)
                    order by DESCRIPCION;";

                return conn.Query<DropDownListaGenericaModel>(sql, new { texto, like }).ToList();
            });
        }
        /// <summary>
        /// Lista de cuotas para Unificación de Cuotas.
        /// <param name="CodEmpresa"></param>
        /// <param name="jfiltros"></param>
        /// <returns></returns>
        public ErrorDto<CoUnificacionCuotasListaResult> Co_UnificacionCuotas_Lista_Obtener(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros;
            try
            {
                filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse<CoUnificacionCuotasListaResult>(ex.Message);
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            var response = new ErrorDto<CoUnificacionCuotasListaResult>
            {
                Code = 0,
                Description = "Ok",
                Result = new CoUnificacionCuotasListaResult
                {
                    total = 0,
                    lista = new List<CoUnificacionCuotasData>()
                }
            };

            try
            {
                var (codigo, cuota, cedula, operadorCuota) = ExtraerParametrosBusqueda(filtros.parametros);

                const string q = @"
            SELECT
                a.ID_SOLICITUD AS id_solicitud,
                RTRIM(ISNULL(a.CODIGO,'')) AS codigo,
                RTRIM(ISNULL(b.CEDULA,'')) AS cedula,

                a.CUOTA AS cuota,
                a.INTC AS intc,
                a.INTM AS intm,
                a.AMORTIZA AS amortiza,
                a.CARGOS AS cargos,
                a.IVA AS iva,

                b.SALDO AS saldo,

                RTRIM(ISNULL(b.ESTADO,'')) AS estado,
                CAST('' AS varchar(10)) AS fecap,
                b.FECULT AS fecult,
                CAST('' AS varchar(10)) AS fecha_corte

            FROM dbo.Vista_Morosidad a
            INNER JOIN dbo.REG_CREDITOS b
                ON a.ID_SOLICITUD = b.ID_SOLICITUD
            WHERE (@codigo = '' OR a.CODIGO = @codigo)
                AND (
                      @cuota <= 0
                      OR (@operadorCuota = '='  AND a.CUOTA = @cuota)
                      OR (@operadorCuota = '>=' AND a.CUOTA >= @cuota)
                      OR (@operadorCuota = '<=' AND a.CUOTA <= @cuota)
                )
              AND (@cedula = '' OR b.CEDULA = @cedula)
              AND ISNULL(b.PROCESO,'') <> 'J';";

                var raw = conn.Query<dynamic>(q, new
                {
                    codigo,
                    cuota,
                    operadorCuota,
                    cedula
                }).AsList();

                var lista = MapRaw(raw, CodEmpresa);

                lista = AplicarFiltroGlobal(lista, filtros.filtro);
                lista = AplicarSort(lista, filtros.sortField, filtros.sortOrder);

                response.Result.total = lista.Count;

                bool exportAll = filtros.pagina == 0 || filtros.paginacion == 0;
                response.Result.lista = exportAll
                    ? lista
                    : AplicarPaginacion(lista, filtros.pagina, filtros.paginacion);

                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CoUnificacionCuotasListaResult>(ex.Message);
            }
        }
        /// <summary>
        /// Exporta la lista de cuotas para Unificación de Cuotas.
        /// <param name="CodEmpresa"></param>
        /// <param name="jfiltros"></param>
        /// <returns></returns>
        public ErrorDto<CoUnificacionCuotasListaResult> Co_UnificacionCuotas_Lista_Export(int CodEmpresa, string jfiltros)
        {
            FiltrosLazyLoadData filtros;
            try
            {
                filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse<CoUnificacionCuotasListaResult>(ex.Message);
            }

            filtros.pagina = 0;
            filtros.paginacion = 0;

            return Co_UnificacionCuotas_Lista_Obtener(CodEmpresa, JsonConvert.SerializeObject(filtros));
        }
        /// <summary>
        /// Ejecuta el proceso de unificación de cuotas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto<CoUnificacionCuotasUnificarResponse> Co_UnificacionCuotas_Unificar(int CodEmpresa, CoUnificacionCuotasUnificarRequest req)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            if (conn.State != ConnectionState.Open)
                conn.Open();

            try
            {
                var codigo = (req.codigo ?? "").Trim().ToUpperInvariant();
                var usuario = (req.usuario_sesion ?? "").Trim();

                if (string.IsNullOrWhiteSpace(codigo))
                    return DbHelper.CreateErrorResponse<CoUnificacionCuotasUnificarResponse>("codigo es requerido.");

                if (req.ids_solicitud == null || req.ids_solicitud.Count == 0)
                    return DbHelper.CreateErrorResponse<CoUnificacionCuotasUnificarResponse>("ids_solicitud es requerido.");

                var vFecha = ObtenerFechaServidor(conn);
                var fechaCR = int.Parse(vFecha.ToString("yyMMdd", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
                var fecult = vFecha;

                using var tx = conn.BeginTransaction();

                try
                {
                    int total = 0;

                    const string sqlUpdate = @"
                update dbo.MOROSIDAD
                set
                    ESTADO = 'N',
                    ESTADOI = 'R',
                    FECULT = @fecult
                where ID_SOLICITUD = @id_solicitud
                  and ESTADO = 'A';";

                    const string sqlInsert = @"
                insert into dbo.MOROSIDAD
                (
                    ID_SOLICITUD,
                    FECHAP,
                    CUOTA_MOROSA,
                    INTC,
                    INTM,
                    AMORTIZA,
                    ESTADO,
                    FECAP,
                    ESTADOI,
                    FECULT,
                    CODIGO
                )
                values
                (
                    @id_solicitud,
                    @fechap,
                    @cuota_morosa,
                    @intc,
                    @intm,
                    @amortiza,
                    'A',
                    @fecap,
                    'U',
                    @fecult,
                    @codigo
                );";

                    foreach (var id in req.ids_solicitud.Distinct())
                    {
                        const string sqlGet = @"
                    select top 1
                        cast(isnull(a.INTC,0) as decimal(18,2)) as intc,
                        cast(isnull(a.INTM,0) as decimal(18,2)) as intm,
                        cast(isnull(a.AMORTIZA,0) as decimal(18,2)) as amortiza
                    from dbo.Vista_Morosidad a
                    where a.ID_SOLICITUD = @id_solicitud
                      and a.CODIGO = @codigo;";

                        var row = conn.QueryFirstOrDefault<dynamic>(sqlGet, new { id_solicitud = id, codigo }, tx);
                        if (row == null)
                            continue;

                        var d = (IDictionary<string, object?>)row;

                        decimal intc = GetDec(d, "intc");
                        decimal intm = GetDec(d, "intm");
                        decimal amortiza = GetDec(d, "amortiza");
                        decimal cuotaMorosa = intc + intm + amortiza;

                        conn.Execute(sqlUpdate, new
                        {
                            id_solicitud = id,
                            fecult
                        }, tx);

                        conn.Execute(sqlInsert, new
                        {
                            id_solicitud = id,
                            fechap = fechaCR,
                            fecap = fechaCR,
                            cuota_morosa = cuotaMorosa,
                            intc,
                            intm,
                            amortiza,
                            fecult,
                            codigo
                        }, tx);

                        total++;
                    }

                    _securityMainDb.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        Movimiento = "Modifica - WEB",
                        Modulo = vModulo,
                        DetalleMovimiento = $"Unificación de cuotas. CODIGO={codigo}. Total={total}. IDS={string.Join(",", req.ids_solicitud)}"
                    });

                    tx.Commit();

                    return new ErrorDto<CoUnificacionCuotasUnificarResponse>
                    {
                        Code = 0,
                        Description = "Ok",
                        Result = new CoUnificacionCuotasUnificarResponse { total_procesadas = total }
                    };
                }
                catch (SqlException ex)
                {
                    tx.Rollback();
                    return DbHelper.CreateErrorResponse<CoUnificacionCuotasUnificarResponse>(ex.Message);
                }
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CoUnificacionCuotasUnificarResponse>(ex.Message);
            }
        }
        private static DateTime ObtenerFechaServidor(SqlConnection conn)
        {
            const string sql = "select getdate();";
            return conn.QuerySingle<DateTime>(sql);
        }
        private static (string codigo, int cuota, string cedula, string operadorCuota) ExtraerParametrosBusqueda(object? parametros)
        {
            string codigo = "";
            int cuota = 0;
            string cedula = "";
            string operadorCuota = "=";

            if (parametros == null)
                return (codigo, cuota, cedula, operadorCuota);

            if (parametros is Newtonsoft.Json.Linq.JObject jo)
            {
                codigo = (jo[CODIGO]?.ToString() ?? "").Trim();
                cedula = (jo[CEDULA]?.ToString() ?? "").Trim();

                var cuotaStr = (jo[CUOTA]?.ToString() ?? "").Trim();
                if (int.TryParse(cuotaStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var c))
                    cuota = c;

                operadorCuota = NormalizarOperadorCuota(jo["operador_cuota"]?.ToString());

                return (codigo, cuota, cedula, operadorCuota);
            }

            if (parametros is IDictionary<string, object?> dict)
            {
                codigo = (dict.TryGetValue(CODIGO, out var v1) ? Convert.ToString(v1) : "")?.Trim() ?? "";
                cedula = (dict.TryGetValue(CEDULA, out var v2) ? Convert.ToString(v2) : "")?.Trim() ?? "";

                var cuotaStr = (dict.TryGetValue(CUOTA, out var v3) ? Convert.ToString(v3) : "")?.Trim() ?? "";
                if (int.TryParse(cuotaStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var c))
                    cuota = c;

                operadorCuota = NormalizarOperadorCuota(
                    dict.TryGetValue("operador_cuota", out var v4) ? Convert.ToString(v4) : null);

                return (codigo, cuota, cedula, operadorCuota);
            }

            return (codigo, cuota, cedula, operadorCuota);
        }
        private static string NormalizarOperadorCuota(string? operadorCuota)
        {
            var op = (operadorCuota ?? "").Trim();

            return op switch
            {
                "<=" => "<=",
                ">=" => ">=",
                "=" => "=",
                _ => "="
            };
        }
        private List<CoUnificacionCuotasData> MapRaw(List<dynamic> raw, int CodEmpresa)
        {
            var rows = raw.Cast<IDictionary<string, object?>>().ToList();
            var lista = new List<CoUnificacionCuotasData>(rows.Count);

            foreach (var d in rows)
            {
                var cedula = GetStr(d, CEDULA);
                var estado = GetStr(d, "estado");

                var item = new CoUnificacionCuotasData
                {
                    id_solicitud = GetInt(d, "id_solicitud"),
                    codigo = GetStr(d, CODIGO),
                    cedula = cedula,
                    nombre = string.IsNullOrWhiteSpace(cedula) ? "" : (_mAfiliacion.fxNombre(CodEmpresa, cedula) ?? ""),
                    cuota = GetInt(d, CUOTA),
                    intc = GetDec(d, "intc"),
                    intm = GetDec(d, "intm"),
                    amortiza = GetDec(d, "amortiza"),
                    cargos = GetDec(d, "cargos"),
                    iva = GetDec(d, "iva"),
                    saldo = GetDec(d, "saldo"),
                    estado = estado,
                    estado_desc = string.IsNullOrWhiteSpace(estado) ? "" : MCobroDb.fxDescribeEstado(estado),
                    fecap = GetDateStr(d, "fecap"),
                    fecult = NormalizarFecultMes(GetStr(d, "fecult")),
                    fecha_corte = GetDateStr(d, "fecha_corte"),
                };

                lista.Add(item);
            }

            return lista;
        }
        private static string NormalizarFecultMes(string fecultRaw)
        {
            var s = (fecultRaw ?? "").Trim();
            if (s.Length >= 6 && s.All(char.IsDigit))
                return s.Substring(0, 4) + "-" + s.Substring(4, 2);

            return s;
        }
        private static List<CoUnificacionCuotasData> AplicarFiltroGlobal(List<CoUnificacionCuotasData> lista, string? filtro)
        {
            var f = (filtro ?? "").Trim();
            if (string.IsNullOrWhiteSpace(f)) return lista;

            f = f.ToUpperInvariant();

            return lista.Where(x =>
                    (x.codigo ?? "").ToUpperInvariant().Contains(f) ||
                    (x.cedula ?? "").ToUpperInvariant().Contains(f) ||
                    (x.nombre ?? "").ToUpperInvariant().Contains(f) ||
                    x.id_solicitud.ToString(CultureInfo.InvariantCulture).Contains(f) ||
                    x.cuota.ToString(CultureInfo.InvariantCulture).Contains(f))
                .ToList();
        }
        private static List<CoUnificacionCuotasData> AplicarSort(List<CoUnificacionCuotasData> lista, string? sortField, int sortOrder)
        {
            var f = (sortField ?? "").Trim().ToLowerInvariant();
            if (!EsSortValido(sortOrder))
                return lista;

            return f switch
            {
                "id_solicitud" => Ordenar(lista, x => x.id_solicitud, sortOrder),
                CODIGO => Ordenar(lista, x => x.codigo, sortOrder),
                CEDULA => Ordenar(lista, x => x.cedula, sortOrder),
                "nombre" => Ordenar(lista, x => x.nombre, sortOrder),
                CUOTA => Ordenar(lista, x => x.cuota, sortOrder),
                "saldo" => Ordenar(lista, x => x.saldo, sortOrder),
                "estado" => Ordenar(lista, x => x.estado, sortOrder),
                _ => lista
            };
        }
        private static bool EsSortValido(int sortOrder)
        {
            return sortOrder == 1 || sortOrder == 2;
        }
        private static List<CoUnificacionCuotasData> Ordenar<TKey>(List<CoUnificacionCuotasData> lista,Func<CoUnificacionCuotasData, TKey> keySelector,int sortOrder)
        {
            return sortOrder == 1
                ? lista.OrderBy(keySelector).ToList()
                : lista.OrderByDescending(keySelector).ToList();
        }
        private static List<CoUnificacionCuotasData> AplicarPaginacion(List<CoUnificacionCuotasData> lista, int pagina, int paginacion)
        {
            if (pagina <= 0) pagina = 1;
            if (paginacion <= 0) paginacion = 30;

            return lista.Skip((pagina - 1) * paginacion).Take(paginacion).ToList();
        }
        private static string GetStr(IDictionary<string, object?> d, string key)
        {
            if (!d.TryGetValue(key, out var v) || v == null) return "";
            return Convert.ToString(v)?.Trim() ?? "";
        }
        private static int GetInt(IDictionary<string, object?> d, string key)
        {
            if (!d.TryGetValue(key, out var v) || v == null) return 0;
            if (v is int i) return i;
            if (int.TryParse(Convert.ToString(v), NumberStyles.Integer, CultureInfo.InvariantCulture, out var r)) return r;
            return 0;
        }
        private static decimal GetDec(IDictionary<string, object?> d, string key)
        {
            if (!d.TryGetValue(key, out var v) || v == null) return 0m;
            if (v is decimal m) return m;
            if (decimal.TryParse(Convert.ToString(v), NumberStyles.Any, CultureInfo.InvariantCulture, out var r)) return r;
            return 0m;
        }
        private static string GetDateStr(IDictionary<string, object?> d, string key)
        {
            if (!d.TryGetValue(key, out var v) || v == null) return "";
            if (v is DateTime dt) return dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var s = Convert.ToString(v)?.Trim() ?? "";
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt2))
                return dt2.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            return s;
        }
    }
}