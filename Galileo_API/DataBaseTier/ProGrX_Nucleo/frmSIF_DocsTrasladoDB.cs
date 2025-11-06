using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using static PgxAPI.Models.ProGrX_Nucleo.FrmSifDocsTrasladoModels;
using PgxAPI.Models.Security;

namespace PgxAPI.DataBaseTier.ProGrX_Nucleo
{
    public class frmSIF_DocsTrasladoDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _security_MainDB;
        private readonly int vModulo = 10;

        public frmSIF_DocsTrasladoDB(IConfiguration config)
        {
            _config = config;
            _security_MainDB = new MSecurityMainDb(_config);
        }
        /// <summary>
        /// Obtiene la lista de documentos del control de traslado (pendientes/bloqueados) con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaFin"></param>
        /// <param name="soloBalanceados"></param>
        public ErrorDto<SifDocsTrasladoDocumentosLista> Sif_DocsTraslado_Lista_Obtener(int CodEmpresa,FiltrosLazyLoadData filtros,DateTime fechaInicio,DateTime fechaFin,bool soloBalanceados)
        {
            var result = new ErrorDto<SifDocsTrasladoDocumentosLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new SifDocsTrasladoDocumentosLista { total = 0, lista = new List<SifDocsTrasladoDocumentosData>() }
            };

            try
            {
                string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var cn = new SqlConnection(connStr);
                var p = new DynamicParameters();
                p.Add("@Inicio", new DateTime(fechaInicio.Year, fechaInicio.Month, fechaInicio.Day, 0, 0, 0), DbType.DateTime);
                p.Add("@Corte", new DateTime(fechaFin.Year, fechaFin.Month, fechaFin.Day, 23, 59, 59), DbType.DateTime);
                p.Add("@Balance", soloBalanceados ? (short)1 : (short)2, DbType.Int16);

                var lista = new List<SifDocsTrasladoDocumentosData>();
                using (var dr = cn.ExecuteReader("spSys_Asientos_CtrlDoc_Busca", p, commandType: CommandType.StoredProcedure, commandTimeout: 60))
                {
                    while (dr.Read())
                    {
                        var row = new SifDocsTrasladoDocumentosData
                        {
                            Tipo_Documento = dr["Tipo_Documento"] as string ?? "",
                            descripcion = dr["Descripcion"] as string ?? "",
                            pendientes = dr["Pendientes"] == DBNull.Value ? 0 : Convert.ToInt32(dr["Pendientes"]),
                            bloqueados = dr["Bloqueados"] == DBNull.Value ? 0 : Convert.ToInt32(dr["Bloqueados"]),
                            codContabilidad = dr["COD_CONTABILIDAD"] == DBNull.Value ? 0 : Convert.ToInt32(dr["COD_CONTABILIDAD"]),
                            asientoTransaccion = null
                        };
                        lista.Add(row);
                    }
                }

                string q = filtros?.filtro?.Trim() ?? "";
                if (q.Length > 0)
                {
                    var upper = q.ToUpperInvariant();
                    lista = lista.Where(x =>
                        (x.Tipo_Documento ?? "").ToUpperInvariant().Contains(upper) ||
                        (x.descripcion ?? "").ToUpperInvariant().Contains(upper)
                    ).ToList();
                }
                string sortField = (filtros?.sortField ?? "").Trim().ToLowerInvariant();
                int sortOrder = filtros?.sortOrder ?? 1;
                lista.Sort((a, b) =>
                {
                    int mul = (sortOrder == 1) ? 1 : -1;
                    switch (sortField)
                    {
                        case "tipodocumento":
                        case "tipo_documento":
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

                int pagina = filtros?.pagina ?? 0;
                int paginacion = filtros?.paginacion ?? 30;
                if (pagina < 0) pagina = 0;
                if (paginacion <= 0) paginacion = 30;

                int total = lista.Count;
                int from = Math.Min(pagina, total);
                int take = Math.Min(paginacion, Math.Max(0, total - from));

                result.Result.total = total;
                result.Result.lista = lista.Skip(from).Take(take).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = new List<SifDocsTrasladoDocumentosData>();
            }

            return result;
        }

        /// <summary>
        /// Obtiene la lista de transacciones desbalanceadas (solo lectura) con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaFin"></param>
        public ErrorDto<SifDocsTrasladoDesbalanceadosLista> Sif_DocsTraslado_Desbalanceados_Obtener(int CodEmpresa,FiltrosLazyLoadData filtros,DateTime fechaInicio,DateTime fechaFin)
        {
            var result = new ErrorDto<SifDocsTrasladoDesbalanceadosLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new SifDocsTrasladoDesbalanceadosLista { total = 0, lista = new List<SifDocsTrasladoDesbalanceadoData>() }
            };

            try
            {
                string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var cn = new SqlConnection(connStr);
                var p = new DynamicParameters();
                p.Add("@Inicio", new DateTime(fechaInicio.Year, fechaInicio.Month, fechaInicio.Day, 0, 0, 0), DbType.DateTime);
                p.Add("@Corte", new DateTime(fechaFin.Year, fechaFin.Month, fechaFin.Day, 23, 59, 59), DbType.DateTime);

                var lista = cn.Query<SifDocsTrasladoDesbalanceadoData>(
                    "spSys_Asientos_CtrlDoc_Desbalanceados",
                    p, commandType: CommandType.StoredProcedure, commandTimeout: 60
                ).AsList();
                string q = filtros?.filtro?.Trim() ?? "";
                if (q.Length > 0)
                {
                    var u = q.ToUpperInvariant();
                    var filtrada = new List<SifDocsTrasladoDesbalanceadoData>();
                    for (int i = 0; i < lista.Count; i++)
                    {
                        var x = lista[i];
                        if (((x.Tipo_Documento ?? "").ToUpperInvariant().IndexOf(u, StringComparison.Ordinal) >= 0) ||
                            ((x.cod_transaccion ?? "").ToUpperInvariant().IndexOf(u, StringComparison.Ordinal) >= 0) ||
                            ((x.Registro_Usuario ?? "").ToUpperInvariant().IndexOf(u, StringComparison.Ordinal) >= 0) ||
                            ((x.Referencia ?? "").ToUpperInvariant().IndexOf(u, StringComparison.Ordinal) >= 0) ||
                            ((x.Notas ?? "").ToUpperInvariant().IndexOf(u, StringComparison.Ordinal) >= 0))
                        {
                            filtrada.Add(x);
                        }
                    }
                    lista = filtrada;
                }
                string sf = (filtros?.sortField ?? "").Trim().ToLowerInvariant();
                int so = filtros?.sortOrder ?? 1;
                lista.Sort((a, b) =>
                {
                    int m = (so == 1) ? 1 : -1;
                    switch (sf)
                    {
                        case "tipo_documento":
                            return m * string.Compare(a.Tipo_Documento, b.Tipo_Documento, StringComparison.OrdinalIgnoreCase);
                        case "cod_transaccion":
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
                int pagina = filtros?.pagina ?? 0;
                int paginacion = filtros?.paginacion ?? 30;
                if (pagina < 0) pagina = 0;
                if (paginacion <= 0) paginacion = 30;

                int total = lista.Count;
                int from = Math.Min(pagina, total);
                int take = Math.Min(paginacion, Math.Max(0, total - from));

                var page = new List<SifDocsTrasladoDesbalanceadoData>(take);
                for (int i = 0; i < take; i++) page.Add(lista[from + i]);

                result.Result.total = total;
                result.Result.lista = page;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = new List<SifDocsTrasladoDesbalanceadoData>();
            }

            return result;
        }
        /// <summary>
        /// Obtiene la configuración del documento en SIF_DOCUMENTOS.
        /// <param name="CodEmpresa"></param>
        /// <param name="tipoDocumento"></param>
        /// </summary>
        public ErrorDto<SifDocsTrasladoDocumentoConfig> Sif_DocsTraslado_Documento_Config_Obtener(int CodEmpresa,string tipoDocumento)
        {
            var result = new ErrorDto<SifDocsTrasladoDocumentoConfig> { Code = 0, Description = "Ok" };

            try
            {
                string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var cn = new SqlConnection(connStr);

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

                result.Result = cn.QueryFirstOrDefault<SifDocsTrasladoDocumentoConfig>(
                    sql, new { doc = tipoDocumento }, commandTimeout: 60);

                if (result.Result == null)
                {
                    result.Code = 1;
                    result.Description = "No existe configuración para el documento.";
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }

            return result;
        }


        /// <summary>
        /// Ejecuta la revisión/“reactivación” de documentos de traslado en el rango de fechas.
        /// <param name="CodEmpresa"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaFin"></param>
        /// </summary>
        public ErrorDto<string> Sif_DocsTraslado_Reactivar(int CodEmpresa,DateTime fechaInicio,DateTime fechaFin)
        {
            var result = new ErrorDto<string> { Code = 0, Description = "Ok", Result = "Revisión realizada" };

            try
            {
                string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var cn = new SqlConnection(connStr);

                var ini = new DateTime(fechaInicio.Year, fechaInicio.Month, fechaInicio.Day, 0, 0, 0);
                var fin = new DateTime(fechaFin.Year, fechaFin.Month, fechaFin.Day, 23, 59, 59);

                var p = new DynamicParameters();
                p.Add("@Inicio", ini, DbType.DateTime);
                p.Add("@Corte", fin, DbType.DateTime);

                cn.Execute("spSys_Asiento_Revisa_Traslado", p, commandType: CommandType.StoredProcedure, commandTimeout: 120);

                result.Description = "Revisión de documentos realizada satisfactoriamente.";
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }

            return result;
        }

        /// <summary>
        /// Aplica el traslado de asientos para un documento y rango de fechas (modo diario o individual).
        /// <param name="CodEmpresa"></param>
        /// <param name="dto"></param>
        /// </summary>
        public ErrorDto<string> Sif_DocsTraslado_Aplica(int CodEmpresa,SifDocsTrasladoEjecutarRequest dto)
        {
            var result = new ErrorDto<string> { Code = 0, Description = "Ok", Result = "Traslado realizado" };

            try
            {
                string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var cn = new SqlConnection(connStr);

                var p = new DynamicParameters();
                p.Add("@TipoDoc", dto.tipoDocumento, DbType.String);
                p.Add("@FechaInicio", new DateTime(dto.fechaInicio.Year, dto.fechaInicio.Month, dto.fechaInicio.Day, 0, 0, 0), DbType.DateTime);
                p.Add("@FechaCorte", new DateTime(dto.fechaFin.Year, dto.fechaFin.Month, dto.fechaFin.Day, 23, 59, 59), DbType.DateTime);
                p.Add("@pUsuario", dto.usuario ?? "", DbType.String);
                p.Add("@Balance", dto.soloBalanceados ? (short)1 : (short)2, DbType.Int16);
                string sp = (dto.modo ?? "").Trim().ToLowerInvariant() == "individual"
                    ? "spSys_Asientos_CtrlDoc_Traslado_Individual"
                    : "spSys_Asientos_CtrlDoc_Traslado_Bloque_Diario";

                cn.Execute(sp, p, commandType: CommandType.StoredProcedure, commandTimeout: 0);

                _security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = dto.usuario ?? "",
                    Modulo = vModulo,
                    Movimiento = "Aplica - WEB",
                    DetalleMovimiento = "Asientos del Control de Documentos"
                });

                result.Description = "Se realizó el Traslado de Asientos a Contabilidad.";
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }

            return result;
        }

        /// <summary>
        /// Exporta la lista de documentos del control de traslado (sin paginación).
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaFin"></param>
        /// <param name="soloBalanceados"></param>
        /// </summary>
        public ErrorDto<List<SifDocsTrasladoDocumentosData>> Sif_DocsTraslado_Lista_Export(int CodEmpresa,FiltrosLazyLoadData filtros,DateTime fechaInicio,DateTime fechaFin,bool soloBalanceados)
        {
            var result = new ErrorDto<List<SifDocsTrasladoDocumentosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SifDocsTrasladoDocumentosData>()
            };

            try
            {
                string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var cn = new SqlConnection(connStr);

                var p = new DynamicParameters();
                p.Add("@Inicio", new DateTime(fechaInicio.Year, fechaInicio.Month, fechaInicio.Day, 0, 0, 0), DbType.DateTime);
                p.Add("@Corte", new DateTime(fechaFin.Year, fechaFin.Month, fechaFin.Day, 23, 59, 59), DbType.DateTime);
                // 1 = balanceados, 2 = todos
                p.Add("@Balance", soloBalanceados ? (short)1 : (short)2, DbType.Int16);

                var lista = new List<SifDocsTrasladoDocumentosData>();
                using (var dr = cn.ExecuteReader("spSys_Asientos_CtrlDoc_Busca", p, commandType: CommandType.StoredProcedure, commandTimeout: 60))
                {
                    while (dr.Read())
                    {
                        lista.Add(new SifDocsTrasladoDocumentosData
                        {
                            Tipo_Documento = dr["Tipo_Documento"] as string ?? "",
                            descripcion = dr["Descripcion"] as string ?? "",
                            pendientes = dr["Pendientes"] == DBNull.Value ? 0 : Convert.ToInt32(dr["Pendientes"]),
                            bloqueados = dr["Bloqueados"] == DBNull.Value ? 0 : Convert.ToInt32(dr["Bloqueados"]),
                            codContabilidad = dr["COD_CONTABILIDAD"] == DBNull.Value ? 0 : Convert.ToInt32(dr["COD_CONTABILIDAD"]),
                            asientoTransaccion = null
                        });
                    }
                }

                // Filtro
                string q = filtros?.filtro?.Trim() ?? "";
                if (q.Length > 0)
                {
                    var up = q.ToUpperInvariant();
                    lista = lista.Where(x =>
                        (x.Tipo_Documento ?? "").ToUpperInvariant().Contains(up) ||
                        (x.descripcion ?? "").ToUpperInvariant().Contains(up)
                    ).ToList();
                }

                // Orden
                string sortField = (filtros?.sortField ?? "").Trim().ToLowerInvariant();
                int sortOrder = filtros?.sortOrder ?? 1; // 1 asc, 0 desc
                lista.Sort((a, b) =>
                {
                    int mul = (sortOrder == 1) ? 1 : -1;
                    switch (sortField)
                    {
                        case "tipodocumento":
                        case "tipo_documento":
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

                result.Result = lista; // sin paginar
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }

            return result;
        }

        /// <summary>
        /// Exporta la lista de transacciones desbalanceadas (sin paginación).
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaFin"></param>
        /// </summary>
        public ErrorDto<List<SifDocsTrasladoDesbalanceadoData>> Sif_DocsTraslado_Desbalanceados_Export(int CodEmpresa,FiltrosLazyLoadData filtros,DateTime fechaInicio,DateTime fechaFin)
        {
            var result = new ErrorDto<List<SifDocsTrasladoDesbalanceadoData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SifDocsTrasladoDesbalanceadoData>()
            };

            try
            {
                string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var cn = new SqlConnection(connStr);

                var p = new DynamicParameters();
                p.Add("@Inicio", new DateTime(fechaInicio.Year, fechaInicio.Month, fechaInicio.Day, 0, 0, 0), DbType.DateTime);
                p.Add("@Corte", new DateTime(fechaFin.Year, fechaFin.Month, fechaFin.Day, 23, 59, 59), DbType.DateTime);

                var lista = cn.Query<SifDocsTrasladoDesbalanceadoData>(
                    "spSys_Asientos_CtrlDoc_Desbalanceados",
                    p, commandType: CommandType.StoredProcedure, commandTimeout: 60
                ).AsList();

                // Filtro (según TU modelo)
                string q = filtros?.filtro?.Trim() ?? "";
                if (q.Length > 0)
                {
                    var up = q.ToUpperInvariant();
                    var filtrada = new List<SifDocsTrasladoDesbalanceadoData>();
                    for (int i = 0; i < lista.Count; i++)
                    {
                        var x = lista[i];
                        if ((x.Tipo_Documento ?? "").ToUpperInvariant().IndexOf(up, StringComparison.Ordinal) >= 0 ||
                            (x.cod_transaccion ?? "").ToUpperInvariant().IndexOf(up, StringComparison.Ordinal) >= 0 ||
                            (x.Registro_Usuario ?? "").ToUpperInvariant().IndexOf(up, StringComparison.Ordinal) >= 0 ||
                            (x.Referencia ?? "").ToUpperInvariant().IndexOf(up, StringComparison.Ordinal) >= 0 ||
                            (x.Notas ?? "").ToUpperInvariant().IndexOf(up, StringComparison.Ordinal) >= 0)
                        {
                            filtrada.Add(x);
                        }
                    }
                    lista = filtrada;
                }

                // Orden (según TU modelo)
                string sortField = (filtros?.sortField ?? "").Trim().ToLowerInvariant();
                int sortOrder = filtros?.sortOrder ?? 1; // 1 asc, 0 desc
                lista.Sort((a, b) =>
                {
                    int mul = (sortOrder == 1) ? 1 : -1;
                    switch (sortField)
                    {
                        case "registro_fecha":
                        case "fecha":
                            return mul * a.Registro_Fecha.CompareTo(b.Registro_Fecha);
                        case "tipo_documento":
                            return mul * string.Compare(a.Tipo_Documento, b.Tipo_Documento, StringComparison.OrdinalIgnoreCase);
                        case "cod_transaccion":
                        case "transaccion":
                            return mul * string.Compare(a.cod_transaccion, b.cod_transaccion, StringComparison.OrdinalIgnoreCase);
                        case "registro_usuario":
                            return mul * string.Compare(a.Registro_Usuario, b.Registro_Usuario, StringComparison.OrdinalIgnoreCase);
                        case "monto":
                            return mul * a.Monto.CompareTo(b.Monto);
                        case "referencia":
                            return mul * string.Compare(a.Referencia, b.Referencia, StringComparison.OrdinalIgnoreCase);
                        case "notas":
                            return mul * string.Compare(a.Notas, b.Notas, StringComparison.OrdinalIgnoreCase);
                        default:
                            return mul * a.Registro_Fecha.CompareTo(b.Registro_Fecha);
                    }
                });

                result.Result = lista; // sin paginar
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }

            return result;
        }



    }
}
