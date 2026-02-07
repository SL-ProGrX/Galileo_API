using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo.Models.Security;


namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSysMonitorAutoGestionDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _security_MainDB;
        private readonly int vModulo = 3;

        public FrmSysMonitorAutoGestionDB(IConfiguration config)
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
        public ErrorDto<MonitorAutoGestionLista> Sys_Monitor_AutoGestion_Lista_Obtener(int CodEmpresa, MonitorAutoGestionListaFiltroDto filtroDto)
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

                string? estado = string.IsNullOrWhiteSpace(filtroDto.estado) ? null : filtroDto.estado.Trim().Substring(0, 1);
                string? tramite = string.IsNullOrWhiteSpace(filtroDto.tramite_estado_id) ? null : filtroDto.tramite_estado_id.Trim().Substring(0, 1);

                bool todasFechas = string.Equals(filtroDto.fechaTipo ?? "", "Todas", StringComparison.OrdinalIgnoreCase);
                bool useResFecha = string.Equals(filtroDto.fechaTipo, "Resolución", StringComparison.OrdinalIgnoreCase);

                var ini = new DateTime(filtroDto.fechaInicio.Year, filtroDto.fechaInicio.Month, filtroDto.fechaInicio.Day, 0, 0, 0, filtroDto.fechaInicio.Kind);
                var fin = new DateTime(filtroDto.fechaFin.Year, filtroDto.fechaFin.Month, filtroDto.fechaFin.Day, 23, 59, 59, filtroDto.fechaFin.Kind);

                string? codigo = string.IsNullOrWhiteSpace(filtroDto.codigoLinea) ? null : filtroDto.codigoLinea.Trim();
                string? cedula = string.IsNullOrWhiteSpace(filtroDto.cedula) ? null : filtroDto.cedula.Trim();

                string rawQ = (filtroDto.filtros?.filtro ?? string.Empty).Trim();
                string? qLike = string.IsNullOrWhiteSpace(rawQ) ? null : $"%{rawQ}%";

                string sortField = (filtroDto.filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant();
                int sortOrder = filtroDto.filtros?.sortOrder ?? 1; // 0=DESC, 1=ASC

                int offset = Math.Max(0, filtroDto.filtros?.pagina ?? 0);
                int fetch = Math.Max(1, filtroDto.filtros?.paginacion ?? 30);

                p.Add("@ESTADO", estado);
                p.Add("@TRAMITE", tramite);
                p.Add("@TODAS_FECHAS", todasFechas ? 1 : 0, DbType.Int32);
                p.Add("@USE_RES", useResFecha ? 1 : 0, DbType.Int32);
                p.Add("@INI", ini, DbType.DateTime);
                p.Add("@FIN", fin, DbType.DateTime);
                p.Add("@CODIGO", codigo);
                p.Add("@CEDULA", cedula);
                p.Add("@Q", qLike);
                p.Add("@SORT_FIELD", sortField);
                p.Add("@SORT_ORDER", sortOrder);
                p.Add("@OFFSET", offset);
                p.Add("@FETCH", fetch);

                const string sqlCount = @"
                    SELECT COUNT(1)
                    FROM vCrd_Solicitudes_AutoGestion
                    WHERE 1=1
                      AND (@ESTADO IS NULL OR ESTADO = @ESTADO)
                      AND (@TRAMITE IS NULL OR TRAMITE_ESTADO_ID = @TRAMITE)
                      AND (
                            @TODAS_FECHAS = 1
                            OR (
                                (@USE_RES = 1 AND ISDATE(RES_FECHA) = 1 AND CONVERT(datetime, RES_FECHA, 121) BETWEEN @INI AND @FIN)
                                OR (@USE_RES = 0 AND ISDATE(REGISTRO_FECHA) = 1 AND CONVERT(datetime, REGISTRO_FECHA, 121) BETWEEN @INI AND @FIN)
                               )
                          )
                      AND (@CODIGO IS NULL OR CODIGO = @CODIGO)
                      AND (@CEDULA IS NULL OR CEDULA = @CEDULA)
                      AND (
                            @Q IS NULL
                            OR CEDULA LIKE @Q
                            OR NOMBRE LIKE @Q
                            OR LINEA_DESC LIKE @Q
                            OR ESTADO_DESC LIKE @Q
                          );";

                result.Result.total = cn.ExecuteScalar<int>(sqlCount, p, commandTimeout: 60);

                const string sql = @"
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
                    WHERE 1=1
                      AND (@ESTADO IS NULL OR ESTADO = @ESTADO)
                      AND (@TRAMITE IS NULL OR TRAMITE_ESTADO_ID = @TRAMITE)
                      AND (
                            @TODAS_FECHAS = 1
                            OR (
                                (@USE_RES = 1 AND ISDATE(RES_FECHA) = 1 AND CONVERT(datetime, RES_FECHA, 121) BETWEEN @INI AND @FIN)
                                OR (@USE_RES = 0 AND ISDATE(REGISTRO_FECHA) = 1 AND CONVERT(datetime, REGISTRO_FECHA, 121) BETWEEN @INI AND @FIN)
                               )
                          )
                      AND (@CODIGO IS NULL OR CODIGO = @CODIGO)
                      AND (@CEDULA IS NULL OR CEDULA = @CEDULA)
                      AND (
                            @Q IS NULL
                            OR CEDULA LIKE @Q
                            OR NOMBRE LIKE @Q
                            OR LINEA_DESC LIKE @Q
                            OR ESTADO_DESC LIKE @Q
                          )
                    ORDER BY
                        -- ASC
                        CASE WHEN @SORT_ORDER = 1 AND @SORT_FIELD = 'cod_solicitud' THEN COD_SOLICITUD END ASC,
                        CASE WHEN @SORT_ORDER = 1 AND @SORT_FIELD = 'estado_desc' THEN ESTADO_DESC END ASC,
                        CASE WHEN @SORT_ORDER = 1 AND @SORT_FIELD = 'cedula' THEN CEDULA END ASC,
                        CASE WHEN @SORT_ORDER = 1 AND @SORT_FIELD = 'nombre' THEN NOMBRE END ASC,
                        CASE WHEN @SORT_ORDER = 1 AND @SORT_FIELD = 'linea_desc' THEN LINEA_DESC END ASC,
                        CASE WHEN @SORT_ORDER = 1 AND @SORT_FIELD = 'monto' THEN MONTO END ASC,
                        CASE WHEN @SORT_ORDER = 1 AND @SORT_FIELD = 'plazo' THEN PLAZO END ASC,
                        CASE WHEN @SORT_ORDER = 1 AND @SORT_FIELD = 'tasa' THEN TASA END ASC,
                        CASE WHEN @SORT_ORDER = 1 AND @SORT_FIELD = 'cuota' THEN CUOTA END ASC,
                        CASE WHEN @SORT_ORDER = 1 AND @SORT_FIELD = 'garantia_desc' THEN GARANTIA_DESC END ASC,
                        CASE WHEN @SORT_ORDER = 1 AND @SORT_FIELD = 'registro_fecha' THEN REGISTRO_FECHA END ASC,
                        CASE WHEN @SORT_ORDER = 1 AND @SORT_FIELD = 'res_fecha' THEN RES_FECHA END ASC,
                        CASE WHEN @SORT_ORDER = 1 AND @SORT_FIELD = 'res_codigo' THEN RES_CODIGO END ASC,
                        CASE WHEN @SORT_ORDER = 1 AND @SORT_FIELD = 'tramite_estado_desc' THEN TRAMITE_ESTADO_DESC END ASC,

                        -- DESC
                        CASE WHEN @SORT_ORDER = 0 AND @SORT_FIELD = 'cod_solicitud' THEN COD_SOLICITUD END DESC,
                        CASE WHEN @SORT_ORDER = 0 AND @SORT_FIELD = 'estado_desc' THEN ESTADO_DESC END DESC,
                        CASE WHEN @SORT_ORDER = 0 AND @SORT_FIELD = 'cedula' THEN CEDULA END DESC,
                        CASE WHEN @SORT_ORDER = 0 AND @SORT_FIELD = 'nombre' THEN NOMBRE END DESC,
                        CASE WHEN @SORT_ORDER = 0 AND @SORT_FIELD = 'linea_desc' THEN LINEA_DESC END DESC,
                        CASE WHEN @SORT_ORDER = 0 AND @SORT_FIELD = 'monto' THEN MONTO END DESC,
                        CASE WHEN @SORT_ORDER = 0 AND @SORT_FIELD = 'plazo' THEN PLAZO END DESC,
                        CASE WHEN @SORT_ORDER = 0 AND @SORT_FIELD = 'tasa' THEN TASA END DESC,
                        CASE WHEN @SORT_ORDER = 0 AND @SORT_FIELD = 'cuota' THEN CUOTA END DESC,
                        CASE WHEN @SORT_ORDER = 0 AND @SORT_FIELD = 'garantia_desc' THEN GARANTIA_DESC END DESC,
                        CASE WHEN @SORT_ORDER = 0 AND @SORT_FIELD = 'registro_fecha' THEN REGISTRO_FECHA END DESC,
                        CASE WHEN @SORT_ORDER = 0 AND @SORT_FIELD = 'res_fecha' THEN RES_FECHA END DESC,
                        CASE WHEN @SORT_ORDER = 0 AND @SORT_FIELD = 'res_codigo' THEN RES_CODIGO END DESC,
                        CASE WHEN @SORT_ORDER = 0 AND @SORT_FIELD = 'tramite_estado_desc' THEN TRAMITE_ESTADO_DESC END DESC,

                        COD_SOLICITUD ASC
                    OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY;";

                result.Result.lista = cn.Query<MonitorAutoGestionListaData>(sql, p, commandTimeout: 60).ToList();
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
        public ErrorDto<List<MonitorAutoGestionListaData>> Sys_Monitor_AutoGestion_Obtener(int CodEmpresa, MonitorAutoGestionListaFiltroDto filtroDto)
        {
            var result = new ErrorDto<List<MonitorAutoGestionListaData>>
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

                string? estado = string.IsNullOrWhiteSpace(filtroDto.estado) ? null : filtroDto.estado.Trim().Substring(0, 1);
                string? tramite = string.IsNullOrWhiteSpace(filtroDto.tramite_estado_id) ? null : filtroDto.tramite_estado_id.Trim().Substring(0, 1);

                bool todasFechas = string.Equals(filtroDto.fechaTipo ?? "", "Todas", StringComparison.OrdinalIgnoreCase);
                bool useResFecha = string.Equals(filtroDto.fechaTipo, "Resolución", StringComparison.OrdinalIgnoreCase);

                var ini = new DateTime(filtroDto.fechaInicio.Year, filtroDto.fechaInicio.Month, filtroDto.fechaInicio.Day, 0, 0, 0, filtroDto.fechaInicio.Kind);
                var fin = new DateTime(filtroDto.fechaFin.Year, filtroDto.fechaFin.Month, filtroDto.fechaFin.Day, 23, 59, 59, filtroDto.fechaFin.Kind);

                string? codigo = string.IsNullOrWhiteSpace(filtroDto.codigoLinea) ? null : filtroDto.codigoLinea.Trim();
                string? cedula = string.IsNullOrWhiteSpace(filtroDto.cedula) ? null : filtroDto.cedula.Trim();

                string rawQ = (filtroDto.filtros?.filtro ?? string.Empty).Trim();
                string? qLike = string.IsNullOrWhiteSpace(rawQ) ? null : $"%{rawQ}%";

                p.Add("@ESTADO", estado);
                p.Add("@TRAMITE", tramite);
                p.Add("@TODAS_FECHAS", todasFechas ? 1 : 0, DbType.Int32);
                p.Add("@USE_RES", useResFecha ? 1 : 0, DbType.Int32);
                p.Add("@INI", ini, DbType.DateTime);
                p.Add("@FIN", fin, DbType.DateTime);
                p.Add("@CODIGO", codigo);
                p.Add("@CEDULA", cedula);
                p.Add("@Q", qLike);

                const string sql = @"
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
                    WHERE 1=1
                      AND (@ESTADO IS NULL OR ESTADO = @ESTADO)
                      AND (@TRAMITE IS NULL OR TRAMITE_ESTADO_ID = @TRAMITE)
                      AND (
                            @TODAS_FECHAS = 1
                            OR (
                                (@USE_RES = 1 AND ISDATE(RES_FECHA) = 1 AND CONVERT(datetime, RES_FECHA, 121) BETWEEN @INI AND @FIN)
                                OR (@USE_RES = 0 AND ISDATE(REGISTRO_FECHA) = 1 AND CONVERT(datetime, REGISTRO_FECHA, 121) BETWEEN @INI AND @FIN)
                               )
                          )
                      AND (@CODIGO IS NULL OR CODIGO = @CODIGO)
                      AND (@CEDULA IS NULL OR CEDULA = @CEDULA)
                      AND (
                            @Q IS NULL
                            OR CEDULA LIKE @Q
                            OR NOMBRE LIKE @Q
                            OR LINEA_DESC LIKE @Q
                            OR ESTADO_DESC LIKE @Q
                          )
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
                p.Add("@Inicio", new DateTime(fechaInicio.Year, fechaInicio.Month, fechaInicio.Day, 0, 0, 0, fechaInicio.Kind), DbType.DateTime);
                p.Add("@Corte", new DateTime(fechaFin.Year, fechaFin.Month, fechaFin.Day, 23, 59, 59, fechaFin.Kind), DbType.DateTime);

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
                using var cn = OpenEmpresaConnection(CodEmpresa);
                cn.Open();

                using var cmd = CreateAdjuntoSelectCommand(cn, archivo_id);
                using var rdr = cmd.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.SingleRow);

                if (!rdr.Read())
                    return NotFoundAdjunto(result);

                string nombre = ReadNombre(rdr);
                string tipoFinal = ResolveMimeTypeAndMaybeFixNombre(rdr, ref nombre);
                byte[] buffer = ReadBlobBytes(rdr, "ARCHIVO_BIT");

                result.Result = (buffer, nombre, tipoFinal);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = (Array.Empty<byte>(), "", "");
            }

            return result;
        }

        private SqlConnection OpenEmpresaConnection(int CodEmpresa)
        {
            string connStr = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            return new SqlConnection(connStr);
        }

        private static SqlCommand CreateAdjuntoSelectCommand(SqlConnection cn, long archivo_id)
        {
            const string sql = @"
                SELECT ARCHIVO_NOMBRE, ARCHIVO_TIPO, ARCHIVO_BIT
                FROM CRD_SOLICITUDES_ADJUNTOS
                WHERE ARCHIVO_ID = @ID;";

            var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = archivo_id;
            return cmd;
        }

        private static ErrorDto<(byte[], string, string)> NotFoundAdjunto(ErrorDto<(byte[], string, string)> result)
        {
            result.Code = 1;
            result.Description = "Adjunto no encontrado.";
            result.Result = (Array.Empty<byte>(), "", "");
            return result;
        }

        private static string ReadNombre(SqlDataReader rdr)
        {
            return (rdr["ARCHIVO_NOMBRE"] as string ?? "archivo").Trim();
        }

        private static string ResolveMimeTypeAndMaybeFixNombre(SqlDataReader rdr, ref string nombre)
        {
            string tipoDb = (rdr["ARCHIVO_TIPO"] as string ?? "").Trim();

            if (!string.IsNullOrWhiteSpace(tipoDb) && tipoDb.Contains('/'))
                return tipoDb;

            string ext = tipoDb.Trim('.').ToLowerInvariant();
            if (string.IsNullOrEmpty(ext))
                ext = TryGetExtensionFromNombre(nombre);

            string mime = ExtToMime(ext);

            if (!string.IsNullOrEmpty(ext) && !nombre.Contains('.'))
                nombre = $"{nombre}.{ext}";

            return mime;
        }

        private static string TryGetExtensionFromNombre(string nombre)
        {
            int dot = nombre.LastIndexOf('.');
            if (dot >= 0 && dot < nombre.Length - 1)
                return nombre[(dot + 1)..].ToLowerInvariant();

            return "";
        }

        private static string ExtToMime(string ext)
        {
            return ext switch
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
        }

        private static byte[] ReadBlobBytes(SqlDataReader rdr, string columnName)
        {
            const int CHUNK = 1024 * 64;

            int ord = rdr.GetOrdinal(columnName);
            long len = rdr.GetBytes(ord, 0, null, 0, 0);

            if (len <= 0)
                return Array.Empty<byte>();

            long bytesLeidos = 0;
            byte[] bufferTemp = new byte[CHUNK];

            using var ms = new MemoryStream(capacity: len > int.MaxValue ? CHUNK : (int)len);

            while (bytesLeidos < len)
            {
                int toRead = (int)Math.Min(CHUNK, len - bytesLeidos);
                int leidos = (int)rdr.GetBytes(ord, bytesLeidos, bufferTemp, 0, toRead);
                if (leidos <= 0)
                    break;

                ms.Write(bufferTemp, 0, leidos);
                bytesLeidos += leidos;
            }

            return ms.ToArray();
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