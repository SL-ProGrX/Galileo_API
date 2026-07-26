using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo.Models.Security;


namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSifDocsTrasladoDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _security_MainDB;
        private readonly int vModulo = 10;

        private const string PendientesField = "Pendientes";
        private const string BloqueadosField = "Bloqueados";
        private const string CodContabilidadField = "COD_CONTABILIDAD";
        private const string TipoDocumentoField = "Tipo_Documento";
        private const string DescripcionField = "Descripcion";
        private const string TipoDocumentoSortField = "tipo_documento";
        private const string InicioParam = "@Inicio";
        private const string CorteParam = "@Corte";
        private const string BalanceParam = "@Balance";

        public FrmSifDocsTrasladoDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _security_MainDB = new MSecurityMainDb(config);
        }

        private ErrorDto<T> WithEmpresaConn<T>(int codEmpresa, Func<SqlConnection, T> action)
            => DbHelper.WithConn(_portalDB, codEmpresa, action);

        private static DateTime StartOfDay(DateTime dt)
            => new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, DateTimeKind.Local);

        private static DateTime EndOfDay(DateTime dt)
            => new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59, DateTimeKind.Local);

        private static DynamicParameters BuildRangoParams(DateTime ini, DateTime fin)
        {
            var p = new DynamicParameters();
            p.Add(InicioParam, StartOfDay(ini), DbType.DateTime);
            p.Add(CorteParam, EndOfDay(fin), DbType.DateTime);
            return p;
        }

        private static DynamicParameters BuildRangoParamsConBalance(DateTime ini, DateTime fin, bool soloBalanceados)
        {
            var p = BuildRangoParams(ini, fin);
            // 1 = balanceados, 2 = todos
            p.Add(BalanceParam, soloBalanceados ? (short)1 : (short)2, DbType.Int16);
            return p;
        }

        private static string GetTrasladoSp(string? modo)
            => (modo ?? string.Empty).Trim().ToLowerInvariant() == "individual"
                ? "spSys_Asientos_CtrlDoc_Traslado_Individual"
                : "spSys_Asientos_CtrlDoc_Traslado_Bloque_Diario";

        private ErrorDto TryBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            try
            {
                _security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = (usuario ?? string.Empty).ToUpperInvariant(),
                    Modulo = vModulo,
                    Movimiento = movimiento,
                    DetalleMovimiento = detalle
                });

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message ?? "Error inesperado");
            }
        }

        private static (int from, int take) ComputePage(FiltrosLazyLoadData? filtros, int total)
        {
            int from = filtros?.pagina ?? 0;
            int take = filtros?.paginacion ?? 30;

            if (from < 0) from = 0;
            if (take <= 0) take = 30;

            from = Math.Min(from, total);
            take = Math.Min(take, Math.Max(0, total - from));

            return (from, take);
        }

        private static List<SifDocsTrasladoDocumentosData> LoadCtrlDoc(SqlConnection cn, DateTime ini, DateTime fin, bool soloBalanceados)
        {
            var p = BuildRangoParamsConBalance(ini, fin, soloBalanceados);

            var lista = new List<SifDocsTrasladoDocumentosData>();
            using var dr = cn.ExecuteReader("spSys_Asientos_CtrlDoc_Busca", p, commandType: CommandType.StoredProcedure, commandTimeout: 60);

            while (dr.Read())
            {
                lista.Add(new SifDocsTrasladoDocumentosData
                {
                    Tipo_Documento = dr[TipoDocumentoField] as string ?? "",
                    descripcion = dr[DescripcionField] as string ?? "",
                    pendientes = dr[PendientesField] == DBNull.Value ? 0 : Convert.ToInt32(dr[PendientesField]),
                    bloqueados = dr[BloqueadosField] == DBNull.Value ? 0 : Convert.ToInt32(dr[BloqueadosField]),
                    codContabilidad = dr[CodContabilidadField] == DBNull.Value ? 0 : Convert.ToInt32(dr[CodContabilidadField]),
                    asientoTransaccion = null
                });
            }

            return lista;
        }

        private static List<SifDocsTrasladoDesbalanceadoData> LoadDesbalanceados(SqlConnection cn, DateTime ini, DateTime fin)
        {
            var p = BuildRangoParams(ini, fin);

            return cn.Query<SifDocsTrasladoDesbalanceadoData>(
                "spSys_Asientos_CtrlDoc_Desbalanceados",
                p,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 60
            ).AsList();
        }

        private static List<SifDocsTrasladoDesbalanceadoData> FiltrarDesbalanceados(List<SifDocsTrasladoDesbalanceadoData> lista, string filtro)
        {
            var q = (filtro ?? string.Empty).Trim();
            if (q.Length == 0)
                return lista;

            var u = q.ToUpperInvariant();
            var filtrada = new List<SifDocsTrasladoDesbalanceadoData>();

            for (int i = 0; i < lista.Count; i++)
            {
                var x = lista[i];
                if ((x.Tipo_Documento ?? string.Empty).ToUpperInvariant().IndexOf(u, StringComparison.Ordinal) >= 0 ||
                    (x.cod_transaccion ?? string.Empty).ToUpperInvariant().IndexOf(u, StringComparison.Ordinal) >= 0 ||
                    (x.Registro_Usuario ?? string.Empty).ToUpperInvariant().IndexOf(u, StringComparison.Ordinal) >= 0 ||
                    (x.Referencia ?? string.Empty).ToUpperInvariant().IndexOf(u, StringComparison.Ordinal) >= 0 ||
                    (x.Notas ?? string.Empty).ToUpperInvariant().IndexOf(u, StringComparison.Ordinal) >= 0)
                {
                    filtrada.Add(x);
                }
            }

            return filtrada;
        }

        private static void OrdenarDesbalanceados(List<SifDocsTrasladoDesbalanceadoData> lista, string sortField, int? sortOrder)
        {
            var sf = (sortField ?? string.Empty).Trim().ToLowerInvariant();
            int so = sortOrder ?? 1;

            lista.Sort((a, b) =>
            {
                int m = (so == 1) ? 1 : -1;
                switch (sf)
                {
                    case TipoDocumentoSortField:
                        return m * string.Compare(a.Tipo_Documento, b.Tipo_Documento, StringComparison.OrdinalIgnoreCase);
                    case "cod_transaccion":
                    case "transaccion":
                        return m * string.Compare(a.cod_transaccion, b.cod_transaccion, StringComparison.OrdinalIgnoreCase);
                    case "registro_fecha":
                    case "fecha":
                        return m * a.Registro_Fecha.CompareTo(b.Registro_Fecha);
                    case "registro_usuario":
                        return m * string.Compare(a.Registro_Usuario, b.Registro_Usuario, StringComparison.OrdinalIgnoreCase);
                    case "monto":
                        return m * a.Monto.CompareTo(b.Monto);
                    case "referencia":
                        return m * string.Compare(a.Referencia, b.Referencia, StringComparison.OrdinalIgnoreCase);
                    case "notas":
                        return m * string.Compare(a.Notas, b.Notas, StringComparison.OrdinalIgnoreCase);
                    default:
                        return m * a.Registro_Fecha.CompareTo(b.Registro_Fecha);
                }
            });
        }

        private static List<T> PageOf<T>(List<T> lista, int from, int take)
            => take <= 0 ? new List<T>() : lista.Skip(from).Take(take).ToList();
        
        /// <summary>
        /// Obtiene la lista de documentos del control de traslado (pendientes/bloqueados) con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaFin"></param>
        /// <param name="soloBalanceados"></param>
        public ErrorDto<SifDocsTrasladoDocumentosLista> Sif_DocsTraslado_Lista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros, DateTime fechaInicio, DateTime fechaFin, bool soloBalanceados)
        {
            return WithEmpresaConn(CodEmpresa, cn =>
            {
                var lista = LoadCtrlDoc(cn, fechaInicio, fechaFin, soloBalanceados);

                lista = FiltrarDocumentos(lista, filtros?.filtro ?? string.Empty);
                OrdenarDocumentos(lista, filtros?.sortField ?? string.Empty, filtros?.sortOrder);

                int total = lista.Count;
                var (from, take) = ComputePage(filtros, total);

                return new SifDocsTrasladoDocumentosLista
                {
                    total = total,
                    lista = PageOf(lista, from, take)
                };
            });
        }

        private static List<SifDocsTrasladoDocumentosData> FiltrarDocumentos(List<SifDocsTrasladoDocumentosData> lista, string filtro)
        {
            string q = filtro?.Trim() ?? "";
            if (q.Length == 0)
                return lista;

            var upper = q.ToUpperInvariant();
            return lista.Where(x =>
                (x.Tipo_Documento ?? "").ToUpperInvariant().Contains(upper) ||
                (x.descripcion ?? "").ToUpperInvariant().Contains(upper)
            ).ToList();
        }

        private static void OrdenarDocumentos(List<SifDocsTrasladoDocumentosData> lista, string sortField, int? sortOrder)
        {
            string field = (sortField ?? "").Trim().ToLowerInvariant();
            int order = sortOrder ?? 1;
            lista.Sort((a, b) =>
            {
                int mul = (order == 1) ? 1 : -1;
                switch (field)
                {
                    case "tipodocumento":
                    case TipoDocumentoSortField:
                        return mul * string.Compare(a.Tipo_Documento, b.Tipo_Documento, StringComparison.OrdinalIgnoreCase);
                    case "descripcion":
                        return mul * string.Compare(a.descripcion, b.descripcion, StringComparison.OrdinalIgnoreCase);
                    case "pendientes":
                        return mul * a.pendientes.CompareTo(b.pendientes);
                    case "bloqueados":
                        return mul * a.bloqueados.CompareTo(b.bloqueados);
                    case "codcontabilidad":
                    case "cod_contabilidad":
                        return mul * a.codContabilidad.CompareTo(b.codContabilidad);
                    default:
                        return mul * string.Compare(a.Tipo_Documento, b.Tipo_Documento, StringComparison.OrdinalIgnoreCase);
                }
            });
        }

        /// <summary>
        /// Obtiene la lista de transacciones desbalanceadas (solo lectura) con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaFin"></param>
        public ErrorDto<SifDocsTrasladoDesbalanceadosLista> Sif_DocsTraslado_Desbalanceados_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros, DateTime fechaInicio, DateTime fechaFin)
        {
            return WithEmpresaConn(CodEmpresa, cn =>
            {
                var lista = LoadDesbalanceados(cn, fechaInicio, fechaFin);

                lista = FiltrarDesbalanceados(lista, filtros?.filtro ?? string.Empty);
                OrdenarDesbalanceados(lista, filtros?.sortField ?? string.Empty, filtros?.sortOrder);

                int total = lista.Count;
                var (from, take) = ComputePage(filtros, total);

                return new SifDocsTrasladoDesbalanceadosLista
                {
                    total = total,
                    lista = PageOf(lista, from, take)
                };
            });
        }
        /// <summary>
        /// Obtiene la configuración del documento en SIF_DOCUMENTOS.
        /// <param name="CodEmpresa"></param>
        /// <param name="tipoDocumento"></param>
        /// </summary>
        public ErrorDto<SifDocsTrasladoDocumentoConfig> Sif_DocsTraslado_Documento_Config_Obtener(int CodEmpresa, string tipoDocumento)
        {
            const string sql = @"
                    SELECT TOP 1
                        Tipo_Documento       AS tipoDocumento,
                        Tipo_Asiento         AS tipoAsiento,
                        Asiento_Mascara      AS asientoMascara,
                        Asiento_Transaccion  AS asientoTransaccion,
                        Asiento_Modulo       AS asientoModulo,
                        Descripcion          AS descripcion
                    FROM SIF_DOCUMENTOS
                    WHERE Tipo_Documento = @doc;";

            var r = WithEmpresaConn(CodEmpresa, cn =>
                cn.QueryFirstOrDefault<SifDocsTrasladoDocumentoConfig>(sql, new { doc = tipoDocumento }, commandTimeout: 60));

            if ((r.Code ?? -1) != 0)
                return new ErrorDto<SifDocsTrasladoDocumentoConfig> { Code = r.Code, Description = r.Description, Result = null };

            if (r.Result == null)
                return new ErrorDto<SifDocsTrasladoDocumentoConfig> { Code = 1, Description = "No existe configuración para el documento.", Result = null };

            return new ErrorDto<SifDocsTrasladoDocumentoConfig> { Code = 0, Description = "Ok", Result = r.Result };
        }


        /// <summary>
        /// Ejecuta la revisión/“reactivación” de documentos de traslado en el rango de fechas.
        /// <param name="CodEmpresa"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaFin"></param>
        /// </summary>
        public ErrorDto<string> Sif_DocsTraslado_Reactivar(int CodEmpresa, DateTime fechaInicio, DateTime fechaFin)
        {
            return WithEmpresaConn(CodEmpresa, cn =>
            {
                var p = BuildRangoParams(fechaInicio, fechaFin);
                cn.Execute("spSys_Asiento_Revisa_Traslado", p, commandType: CommandType.StoredProcedure, commandTimeout: 120);
                return "Revisión realizada";
            });
        }

        /// <summary>
        /// Aplica el traslado de asientos para un documento y rango de fechas (modo diario o individual).
        /// <param name="CodEmpresa"></param>
        /// <param name="dto"></param>
        /// </summary>
        public ErrorDto<string> Sif_DocsTraslado_Aplica(int CodEmpresa, SifDocsTrasladoEjecutarRequest dto)
        {
            if (dto == null)
            {
                return DbHelper.CreateErrorResponse<string>("Request inválido");
            }

            if (string.IsNullOrWhiteSpace(dto.tipoDocumento))
            {
                return DbHelper.CreateErrorResponse<string>("tipoDocumento es requerido");
            }

            var tipoDocumento = dto.tipoDocumento.Trim();
            var usuario = dto.usuario ?? string.Empty;
            var fechaInicio = dto.fechaInicio;
            var fechaFin = dto.fechaFin;
            var soloBalanceados = dto.soloBalanceados;
            var modo = dto.modo;

            var sp = GetTrasladoSp(modo);

            var exec = WithEmpresaConn(CodEmpresa, cn =>
            {
                var p = new DynamicParameters();

                p.Add("@TipoDoc", tipoDocumento, DbType.String);
                p.Add("@FechaInicio", StartOfDay(fechaInicio), DbType.DateTime);
                p.Add("@FechaCorte", EndOfDay(fechaFin), DbType.DateTime);
                p.Add("@pUsuario", usuario, DbType.String);
                p.Add(
                    BalanceParam,
                    soloBalanceados ? (short)1 : (short)2,
                    DbType.Int16);

                cn.Execute(
                    sp,
                    p,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 0);

                return "Traslado realizado";
            });

            if ((exec.Code ?? -1) != 0)
            {
                return exec;
            }

            var bit = TryBitacora(
                CodEmpresa,
                usuario,
                "Aplica - WEB",
                "Asientos del Control de Documentos");

            if ((bit.Code ?? -1) != 0)
            {
                return new ErrorDto<string>
                {
                    Code = bit.Code,
                    Description = bit.Description,
                    Result = null
                };
            }

            return new ErrorDto<string>
            {
                Code = 0,
                Description = "Se realizó el Traslado de Asientos a Contabilidad.",
                Result = exec.Result
            };
        }

        /// <summary>
        /// Exporta la lista de documentos del control de traslado (sin paginación).
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaFin"></param>
        /// <param name="soloBalanceados"></param>
        /// </summary>
        public ErrorDto<List<SifDocsTrasladoDocumentosData>> Sif_DocsTraslado_Lista_Export(int CodEmpresa, FiltrosLazyLoadData filtros, DateTime fechaInicio, DateTime fechaFin, bool soloBalanceados)
        {
            return WithEmpresaConn(CodEmpresa, cn =>
            {
                var lista = LoadCtrlDoc(cn, fechaInicio, fechaFin, soloBalanceados);
                lista = FiltrarDocumentos(lista, filtros?.filtro ?? string.Empty);
                OrdenarDocumentos(lista, filtros?.sortField ?? string.Empty, filtros?.sortOrder);
                return lista;
            });
        }

        /// <summary>
        /// Exporta la lista de transacciones desbalanceadas (sin paginación).
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaFin"></param>
        /// </summary>
        public ErrorDto<List<SifDocsTrasladoDesbalanceadoData>> Sif_DocsTraslado_Desbalanceados_Export(int CodEmpresa, FiltrosLazyLoadData filtros, DateTime fechaInicio, DateTime fechaFin)
        {
            return WithEmpresaConn(CodEmpresa, cn =>
            {
                var lista = LoadDesbalanceados(cn, fechaInicio, fechaFin);
                lista = FiltrarDesbalanceados(lista, filtros?.filtro ?? string.Empty);
                OrdenarDesbalanceados(lista, filtros?.sortField ?? string.Empty, filtros?.sortOrder);
                return lista;
            });
        }

        /// <summary>
        /// Aplica el traslado de asientos para varios documentos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto<SifDocsTrasladoResultadoLote> Sif_DocsTraslado_Aplica_Lote(int CodEmpresa, SifDocsTrasladoEjecutarLoteRequest dto)
        {
            if (dto == null)
                return DbHelper.CreateErrorResponse<SifDocsTrasladoResultadoLote>("Request inválido");

            var usuario = dto.usuario ?? string.Empty;
            var tipos = dto.tiposDocumento ?? new List<string>();
            var fechaInicio = dto.fechaInicio;
            var fechaFin = dto.fechaFin;
            var soloBalanceados = dto.soloBalanceados;
            var modo = dto.modo;

            var spName = GetTrasladoSp(modo);

            var r = WithEmpresaConn(CodEmpresa, cn =>
            {
                cn.Open();

                var res = new SifDocsTrasladoResultadoLote();
                var lista = tipos;

                res.total = lista.Count;

                foreach (var tipo in lista)
                {
                    var item = new SifDocsTrasladoResultadoItem { tipoDocumento = tipo };

                    try
                    {
                        var p = new DynamicParameters();
                        p.Add("@TipoDoc", tipo, DbType.String);
                        p.Add("@FechaInicio", StartOfDay(fechaInicio), DbType.DateTime);
                        p.Add("@FechaCorte", EndOfDay(fechaFin), DbType.DateTime);
                        p.Add("@pUsuario", usuario, DbType.String);
                        p.Add(BalanceParam, soloBalanceados ? (short)1 : (short)2, DbType.Int16);

                        cn.Execute(spName, p, commandType: CommandType.StoredProcedure, commandTimeout: 0);

                        item.code = 0;
                        item.description = "Traslado aplicado";
                        res.ok++;
                    }
                    catch (Exception exDoc)
                    {
                        item.code = -1;
                        item.description = exDoc.Message;
                        res.fail++;
                    }

                    res.detalle.Add(item);
                }

                return res;
            });

            if ((r.Code ?? -1) != 0)
                return new ErrorDto<SifDocsTrasladoResultadoLote> { Code = r.Code, Description = r.Description, Result = null };

            if (r.Result == null)
                return new ErrorDto<SifDocsTrasladoResultadoLote> { Code = -1, Description = "No se pudo generar el resultado del lote", Result = null };

            var detalle = $"Asientos del Control de Documentos | Total: {r.Result.total} | OK: {r.Result.ok} | Fail: {r.Result.fail}";
            var bit = TryBitacora(CodEmpresa, usuario, "Aplica Lote - WEB", detalle);

            if ((bit.Code ?? -1) != 0)
                return new ErrorDto<SifDocsTrasladoResultadoLote> { Code = bit.Code, Description = bit.Description, Result = r.Result };

            return new ErrorDto<SifDocsTrasladoResultadoLote>
            {
                Code = 0,
                Description = $"Procesados: {r.Result.total}. OK: {r.Result.ok}. Error: {r.Result.fail}.",
                Result = r.Result
            };
        }

    }
}
