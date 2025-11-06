using System.Data;
using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.Security;
using static PgxAPI.Models.ProGrX_Nucleo.FrmSysMonitorAutoGestionModels;
namespace PgxAPI.DataBaseTier.ProGrX_Nucleo
{
    public class frmSYS_Monitor_AutoGestionDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _security_MainDB;
        private readonly int vModulo = 3;

        public frmSYS_Monitor_AutoGestionDB(IConfiguration config)
        {
            _config = config;
            _security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene lista (grid) con LazyLoad, filtros y ordenamiento.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="estado"></param>
        /// <param name="tramite_estado_id"></param>
        /// <param name="fechaTipo"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaFin"></param>
        /// <param name="codigoLinea"></param>
        /// <param name="cedula"></param>
        public ErrorDto<MonitorAutoGestionLista> Sys_Monitor_AutoGestion_Lista_Obtener(int CodEmpresa,FiltrosLazyLoadData filtros,string? estado,string? tramite_estado_id,string fechaTipo,DateTime fechaInicio,DateTime fechaFin,string? codigoLinea,string? cedula)
        {
            var result = new ErrorDto<MonitorAutoGestionLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new MonitorAutoGestionLista
                {
                    total = 0,
                    lista = new List<MonitorAutoGestionListaData>()
                }
            };

            try
            {
                string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var cn = new SqlConnection(connStr);

                var p = new DynamicParameters();
                var sbWhere = new StringBuilder(" WHERE 1=1 ");

                if (!string.IsNullOrWhiteSpace(estado))
                {
                    sbWhere.Append(" AND ESTADO = @ESTADO ");
                    p.Add("@ESTADO", estado.Trim().Substring(0, 1));
                }

                if (!string.IsNullOrWhiteSpace(tramite_estado_id))
                {
                    sbWhere.Append(" AND TRAMITE_ESTADO_ID = @TRAMITE ");
                    p.Add("@TRAMITE", tramite_estado_id.Trim().Substring(0, 1));
                }
                var ini = new DateTime(fechaInicio.Year, fechaInicio.Month, fechaInicio.Day, 0, 0, 0);
                var fin = new DateTime(fechaFin.Year, fechaFin.Month, fechaFin.Day, 23, 59, 59);

                if (!string.Equals(fechaTipo ?? "", "Todas", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(fechaTipo, "Resolución", StringComparison.OrdinalIgnoreCase))
                    {
                        sbWhere.Append(@"
                        AND ISDATE(RES_FECHA) = 1
                        AND CONVERT(datetime, RES_FECHA, 121) BETWEEN @INI AND @FIN ");
                    }
                    else
                    {
                        sbWhere.Append(@"
                        AND ISDATE(REGISTRO_FECHA) = 1
                        AND CONVERT(datetime, REGISTRO_FECHA, 121) BETWEEN @INI AND @FIN ");
                    }
                    p.Add("@INI", ini, DbType.DateTime);
                    p.Add("@FIN", fin, DbType.DateTime);
                }


                if (!string.IsNullOrWhiteSpace(codigoLinea))
                {
                    sbWhere.Append(" AND CODIGO = @CODIGO ");
                    p.Add("@CODIGO", codigoLinea.Trim());
                }

                if (!string.IsNullOrWhiteSpace(cedula))
                {
                    sbWhere.Append(" AND CEDULA = @CEDULA ");
                    p.Add("@CEDULA", cedula.Trim());
                }
                if (!string.IsNullOrWhiteSpace(filtros?.filtro))
                {
                    sbWhere.Append(@"
                        AND (
                             CEDULA      LIKE @Q
                          OR NOMBRE      LIKE @Q
                          OR LINEA_DESC  LIKE @Q
                          OR ESTADO_DESC LIKE @Q
                        )");
                    p.Add("@Q", "%" + filtros.filtro.Trim() + "%");
                }
                string orderBy = (filtros?.sortField ?? "").Trim().ToUpperInvariant() switch
                {
                    "COD_SOLICITUD" => "COD_SOLICITUD",
                    "ESTADO_DESC" => "ESTADO_DESC",
                    "CEDULA" => "CEDULA",
                    "NOMBRE" => "NOMBRE",
                    "LINEA_DESC" => "LINEA_DESC",
                    "MONTO" => "MONTO",
                    "PLAZO" => "PLAZO",
                    "TASA" => "TASA",
                    "CUOTA" => "CUOTA",
                    "GARANTIA_DESC" => "GARANTIA_DESC",
                    "REGISTRO_FECHA" => "REGISTRO_FECHA",
                    "RES_FECHA" => "RES_FECHA",
                    "RES_CODIGO" => "RES_CODIGO",
                    "TRAMITE_ESTADO_DESC" => "TRAMITE_ESTADO_DESC",
                    _ => "COD_SOLICITUD"
                };
                string orderDir = (filtros?.sortOrder ?? 1) == 0 ? "DESC" : "ASC";
                string sqlCount = $@"
                    SELECT COUNT(1)
                    FROM vCrd_Solicitudes_AutoGestion
                    {sbWhere};";

                result.Result.total = cn.ExecuteScalar<int>(sqlCount, p, commandTimeout: 60);

                int offset = Math.Max(0, filtros?.pagina ?? 0);
                int fetch = Math.Max(1, filtros?.paginacion ?? 30);

                string sql = $@"
                    SELECT 
                        COD_SOLICITUD           AS Cod_Solicitud,
                        ESTADO_DESC             AS Estado_Desc,
                        CEDULA                  AS Cedula,
                        NOMBRE                  AS Nombre,
                        LINEA_DESC              AS Linea_Desc,
                        MONTO                   AS Monto,
                        PLAZO                   AS Plazo,
                        TASA                    AS Tasa,
                        CUOTA                   AS Cuota,
                        GARANTIA_DESC           AS Garantia_Desc,
                        REGISTRO_FECHA          AS Registro_Fecha,
                        RES_FECHA               AS Res_Fecha,
                        RES_CODIGO              AS Res_Codigo,
                        TRAMITE_ESTADO_DESC     AS Tramite_Estado_Desc,
                        RES_TIPO                AS Res_Tipo 
                        FROM vCrd_Solicitudes_AutoGestion
                        {sbWhere}
                        ORDER BY {orderBy} {orderDir}
                        OFFSET {offset} ROWS
                        FETCH NEXT {fetch} ROWS ONLY;";

                result.Result.lista = cn.Query<MonitorAutoGestionListaData>(
                    sql, p, commandTimeout: 60).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = new List<MonitorAutoGestionListaData>();
            }
            return result;
        }

        /// <summary>
        /// Exporta la lista (sin paginar) con los mismos filtros del grid.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="estado"></param>
        /// <param name="tramite_estado_id"></param>
        /// <param name="fechaTipo"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaFin"></param>
        /// <param name="codigoLinea"></param>
        /// <param name="cedula"></param>
        public ErrorDto<List<MonitorAutoGestionListaData>> Sys_Monitor_AutoGestion_Obtener(int CodEmpresa,FiltrosLazyLoadData filtros,string? estado,string? tramite_estado_id,string fechaTipo,DateTime fechaInicio,DateTime fechaFin,string? codigoLinea,string? cedula)
        {
            var result = new ErrorDto<List< MonitorAutoGestionListaData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<MonitorAutoGestionListaData>()
            };

            try
            {
                string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var cn = new SqlConnection(connStr);

                var p = new DynamicParameters();
                var sbWhere = new StringBuilder(" WHERE 1=1 ");
                if (!string.IsNullOrWhiteSpace(estado))
                {
                    sbWhere.Append(" AND ESTADO = @ESTADO ");
                    p.Add("@ESTADO", estado.Trim().Substring(0, 1));
                }

                if (!string.IsNullOrWhiteSpace(tramite_estado_id))
                {
                    sbWhere.Append(" AND TRAMITE_ESTADO_ID = @TRAMITE ");
                    p.Add("@TRAMITE", tramite_estado_id.Trim().Substring(0, 1));
                }

                var ini = new DateTime(fechaInicio.Year, fechaInicio.Month, fechaInicio.Day, 0, 0, 0);
                var fin = new DateTime(fechaFin.Year, fechaFin.Month, fechaFin.Day, 23, 59, 59);

                if (!string.Equals(fechaTipo ?? "", "Todas", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(fechaTipo, "Resolución", StringComparison.OrdinalIgnoreCase))
                    {
                        sbWhere.Append(@"
                        AND ISDATE(RES_FECHA) = 1
                        AND CONVERT(datetime, RES_FECHA, 121) BETWEEN @INI AND @FIN ");
                    }
                    else
                    {
                        sbWhere.Append(@"
                        AND ISDATE(REGISTRO_FECHA) = 1
                        AND CONVERT(datetime, REGISTRO_FECHA, 121) BETWEEN @INI AND @FIN ");
                    }
                    p.Add("@INI", ini, DbType.DateTime);
                    p.Add("@FIN", fin, DbType.DateTime);
                }


                if (!string.IsNullOrWhiteSpace(codigoLinea))
                {
                    sbWhere.Append(" AND CODIGO = @CODIGO ");
                    p.Add("@CODIGO", codigoLinea.Trim());
                }

                if (!string.IsNullOrWhiteSpace(cedula))
                {
                    sbWhere.Append(" AND CEDULA = @CEDULA ");
                    p.Add("@CEDULA", cedula.Trim());
                }
                if (!string.IsNullOrWhiteSpace(filtros?.filtro))
                {
                    sbWhere.Append(@"
                AND (
                     CEDULA      LIKE @Q
                  OR NOMBRE      LIKE @Q
                  OR LINEA_DESC  LIKE @Q
                  OR ESTADO_DESC LIKE @Q
                )");
                    p.Add("@Q", "%" + filtros.filtro.Trim() + "%");
                }

                string sql = $@"
                SELECT 
                    COD_SOLICITUD           AS Cod_Solicitud,
                    ESTADO_DESC             AS Estado_Desc,
                    CEDULA                  AS Cedula,
                    NOMBRE                  AS Nombre,
                    LINEA_DESC              AS Linea_Desc,
                    MONTO                   AS Monto,
                    PLAZO                   AS Plazo,
                    TASA                    AS Tasa,
                    CUOTA                   AS Cuota,
                    GARANTIA_DESC           AS Garantia_Desc,
                    REGISTRO_FECHA          AS Registro_Fecha,
                    RES_FECHA               AS Res_Fecha,
                    RES_CODIGO              AS Res_Codigo,
                    TRAMITE_ESTADO_DESC     AS Tramite_Estado_Desc,
                    RES_TIPO                AS Res_Tipo 
                FROM vCrd_Solicitudes_AutoGestion
            {sbWhere}
            ORDER BY COD_SOLICITUD ASC;";

                result.Result = cn.Query<MonitorAutoGestionListaData>(sql, p, commandTimeout: 60).ToList();
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
        /// Obtiene el detalle de un caso por COD_SOLICITUD.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_solicitud"></param>
        public ErrorDto<MonitorAutoGestionCasoDetalle> Sys_Monitor_AutoGestion_Caso_Obtener(int CodEmpresa,long cod_solicitud)
        {
            var result = new ErrorDto<MonitorAutoGestionCasoDetalle> { Code = 0, Description = "Ok" };

            try
            {
                string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var cn = new SqlConnection(connStr);

                const string sql = @"
                    SELECT 
                        COD_SOLICITUD AS Cod_Solicitud,
                        ESTADO_DESC   AS Estado_Desc,
                        ESTADO        AS Estado,
                        GARANTIA_DESC AS Garantia_Desc,
                        CEDULA        AS Cedula,
                        NOMBRE        AS Nombre,
                        CODIGO        AS Codigo,
                        LINEA_DESC    AS Linea_Desc,
                        MONTO         AS Monto,
                        PLAZO         AS Plazo,
                        TASA          AS Tasa,
                        CUOTA         AS Cuota,
                        REGISTRO_FECHA  AS Registro_Fecha,
                        REGISTRO_USUARIO AS Registro_Usuario,
                        RES_FECHA       AS Res_Fecha,
                        RES_USUARIO     AS Res_Usuario,
                        RES_CODIGO      AS Res_Codigo,
                        NOTAS           AS Notas,
                        CASE WHEN ISNULL(REFUNDE_IND,0)=0 THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END AS Refunde_Ind
                    FROM vCrd_Solicitudes_AutoGestion
                    WHERE COD_SOLICITUD = @ID";

                result.Result = cn.QueryFirstOrDefault<MonitorAutoGestionCasoDetalle>(sql, new { ID = cod_solicitud }, commandTimeout: 60);
                if (result.Result == null)
                {
                    result.Code = 1;
                    result.Description = "Caso no encontrado.";
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
        /// Obtiene el resumen por estado en el rango de fechas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaFin"></param>
        public ErrorDto<MonitorAutoGestionResumenLista> Sys_Monitor_AutoGestion_Resumen_Obtener(int CodEmpresa,DateTime fechaInicio,DateTime fechaFin)
        {
            var result = new ErrorDto<MonitorAutoGestionResumenLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new MonitorAutoGestionResumenLista
                {
                    total = 0,
                    lista = new List<MonitorAutoGestionResumenData>()
                }
            };

            try
            {
                string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var cn = new SqlConnection(connStr);

                var p = new DynamicParameters();
                p.Add("@Inicio", new DateTime(fechaInicio.Year, fechaInicio.Month, fechaInicio.Day, 0, 0, 0), DbType.DateTime);
                p.Add("@Corte", new DateTime(fechaFin.Year, fechaFin.Month, fechaFin.Day, 23, 59, 59), DbType.DateTime);

                var data = cn.Query<MonitorAutoGestionResumenData>(
                    "spCrd_Solicitudes_AutoGestion_Rsm",
                    p, commandType: CommandType.StoredProcedure, commandTimeout: 60).ToList();

                result.Result.lista = data;
                result.Result.total = data.Count;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = new List<MonitorAutoGestionResumenData>();
            }
            return result;
        }

        /// <summary>
        /// Lista adjuntos del caso (solo metadatos).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_solicitud"></param>
        public ErrorDto<MonitorAutoGestionAdjuntosLista> Sys_Monitor_AutoGestion_Adjuntos_Obtener(int CodEmpresa,long cod_solicitud)
        {
            var result = new ErrorDto<MonitorAutoGestionAdjuntosLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new MonitorAutoGestionAdjuntosLista
                {
                    total = 0,
                    lista = new List<MonitorAutoGestionAdjuntoData>()
                }
            };

            try
            {
                string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var cn = new SqlConnection(connStr);

                const string sql = @"
                    SELECT 
                        A.ARCHIVO_ID   AS Archivo_Id,
                        T.DESCRIPCION  AS Tipo_Adjunto,
                        A.ARCHIVO_NOMBRE AS Archivo_Nombre,
                        A.ARCHIVO_TIPO   AS Archivo_Tipo
                    FROM CRD_SOLICITUDES_ADJUNTOS A
                    INNER JOIN CRD_ADJUNTOS_TIPOS T ON A.COD_ADJUNTO = T.COD_ADJUNTO
                    WHERE A.TRANSAC_TIPO = 'SOL' AND A.TRANSAC_CODIGO = @ID
                    ORDER BY A.ARCHIVO_ID ASC;";

                var lista = cn.Query<MonitorAutoGestionAdjuntoData>(sql, new { ID = cod_solicitud }, commandTimeout: 60).ToList();
                result.Result.lista = lista;
                result.Result.total = lista.Count;
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = new List<MonitorAutoGestionAdjuntoData>();
            }
            return result;
        }

        /// <summary>
        /// Descarga un adjunto (bytes, nombre y tipo).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="archivo_id"></param>
        public ErrorDto<(byte[] buffer, string nombre, string tipo)> Sys_Monitor_AutoGestion_Adjunto_Descargar(int CodEmpresa,long archivo_id)
        {
            var result = new ErrorDto<(byte[], string, string)>
            {
                Code = 0,
                Description = "Ok",
                Result = (Array.Empty<byte>(), "", "")
            };

            try
            {
                string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var cn = new SqlConnection(connStr);

                const string sql = @"
            SELECT ARCHIVO_NOMBRE, ARCHIVO_TIPO, ARCHIVO_BIT
            FROM CRD_SOLICITUDES_ADJUNTOS
            WHERE ARCHIVO_ID = @ID;";

                using var cmd = new SqlCommand(sql, cn);
                cmd.Parameters.Add("@ID", System.Data.SqlDbType.BigInt).Value = archivo_id;

                cn.Open();
                using var rdr = cmd.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.SingleRow);

                if (!rdr.Read())
                {
                    result.Code = 1;
                    result.Description = "Adjunto no encontrado.";
                    return result;
                }

                string nombre = (rdr["ARCHIVO_NOMBRE"] as string ?? "archivo").Trim();
                string tipoDb = (rdr["ARCHIVO_TIPO"] as string ?? "").Trim();
                string tipoFinal;
                if (!string.IsNullOrWhiteSpace(tipoDb) && tipoDb.Contains('/'))
                {
                    tipoFinal = tipoDb;
                }
                else
                {
                    string ext = tipoDb.Trim('.').ToLowerInvariant();
                    if (string.IsNullOrEmpty(ext))
                    {
                        int dot = nombre.LastIndexOf('.');
                        if (dot >= 0 && dot < nombre.Length - 1)
                            ext = nombre[(dot + 1)..].ToLowerInvariant();
                    }
                    var mime = ext switch
                    {
                        "png" => "image/png",
                        "jpg" => "image/jpeg",
                        "jpeg" => "image/jpeg",
                        "gif" => "image/gif",
                        "bmp" => "image/bmp",
                        "webp" => "image/webp",
                        "pdf" => "application/pdf",
                        "txt" => "text/plain",
                        "csv" => "text/csv",
                        "xml" => "application/xml",
                        "json" => "application/json",
                        "doc" => "application/msword",
                        "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                        "xls" => "application/vnd.ms-excel",
                        "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ppt" => "application/vnd.ms-powerpoint",
                        "pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                        "zip" => "application/zip",
                        "rar" => "application/vnd.rar",
                        "7z" => "application/x-7z-compressed",
                        "mp3" => "audio/mpeg",
                        "mp4" => "video/mp4",
                        _ => "application/octet-stream"
                    };

                    tipoFinal = mime;

                    if (!string.IsNullOrEmpty(ext) && !nombre.Contains('.'))
                        nombre = $"{nombre}.{ext}";
                }

                const int CHUNK = 1024 * 64;
                long bytesLeidos = 0;
                byte[] bufferTemp = new byte[CHUNK];
                using var ms = new MemoryStream();

                int ordBit = rdr.GetOrdinal("ARCHIVO_BIT");
                long len = rdr.GetBytes(ordBit, 0, null, 0, 0);
                while (bytesLeidos < len)
                {
                    int toRead = (int)Math.Min(CHUNK, len - bytesLeidos);
                    int leidos = (int)rdr.GetBytes(ordBit, bytesLeidos, bufferTemp, 0, toRead);
                    if (leidos <= 0) break;
                    ms.Write(bufferTemp, 0, leidos);
                    bytesLeidos += leidos;
                }

                result.Result = (ms.ToArray(), nombre, tipoFinal);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = (Array.Empty<byte>(), "", "");
            }
            return result;
        }


        /// <summary>
        /// Aplica resolución del caso llamando al SP y registra bitácora.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="dto"></param>
        public ErrorDto<MonitorAutoGestionResolucionResponse> Sys_Monitor_AutoGestion_Resolucion_Aplicar(int CodEmpresa,MonitorAutoGestionResolucionRequest dto)
        {
            var result = new ErrorDto<MonitorAutoGestionResolucionResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new MonitorAutoGestionResolucionResponse()
            };

            try
            {
                string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var cn = new SqlConnection(connStr);

                var p = new DynamicParameters();
                p.Add("@Solicitud", dto.cod_solicitud);
                p.Add("@Resolucion", (dto.resolucion ?? "P").Trim().Substring(0, 1));
                p.Add("@Notas", dto.notas ?? "");
                p.Add("@Usuario", dto.usuario ?? "");
                p.Add("@Gestion", (dto.gestion ?? "S").Trim().Substring(0, 1));
                cn.Execute("spCrd_Solicitudes_AutoGestion_Resolucion", p, commandType: CommandType.StoredProcedure, commandTimeout: 90);

                var post = Sys_Monitor_AutoGestion_Caso_Obtener(CodEmpresa, dto.cod_solicitud);
                if (post.Code == 0 && post.Result != null)
                {
                    result.Result.cod_solicitud = post.Result.Cod_Solicitud;
                    result.Result.estado = post.Result.Estado;
                    result.Result.estado_desc = post.Result.Estado_Desc;
                    result.Result.res_fecha = post.Result.Res_Fecha;
                    result.Result.res_usuario = post.Result.Res_Usuario;
                    result.Result.res_codigo = post.Result.Res_Codigo;
                    result.Result.notas = post.Result.Notas;
                }

                // Bitácora
                _security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = dto.usuario ?? "",
                    Modulo = vModulo,
                    Movimiento = "Resolución - WEB",
                    DetalleMovimiento = $"Caso {dto.cod_solicitud} → {dto.resolucion}/{dto.gestion}"
                });
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
        /// Ejecuta mantenimiento de adjuntos (fix opcional).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        public ErrorDto Sys_Monitor_AutoGestion_Adjuntos_Fix(int CodEmpresa)
        {
            var res = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var cn = new SqlConnection(connStr);
                cn.Execute("spCrd_Solicitudes_Adjuntos_Fix", commandType: CommandType.StoredProcedure, commandTimeout: 60);
            }
            catch (Exception ex)
            {
                res.Code = -1;
                res.Description = ex.Message;
            }
            return res;
        }
    }
}
