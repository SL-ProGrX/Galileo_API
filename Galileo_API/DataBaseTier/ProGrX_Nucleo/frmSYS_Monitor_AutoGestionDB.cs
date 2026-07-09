using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using static Galileo.Models.ProGrX_Nucleo.FrmSysMonitorAutoGestionModels;


namespace Galileo_API.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSysMonitorAutoGestionDb
    {
        private readonly MSecurityMainDb _security_MainDB;
        private readonly int vModulo = 3;
        private readonly PortalDB _portalDB;

        public FrmSysMonitorAutoGestionDb(IConfiguration config)
        {
            _security_MainDB = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Personas para Monitor AutoGestión (Cédula / Nombre).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Sys_Monitor_AutoGestion_Personas_DropDown_Obtener(int CodEmpresa, string? filtro)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                string f = (filtro ?? "").Trim();

                if (string.IsNullOrWhiteSpace(f))
                {
                    const string q = @"
                    SELECT
                        RTRIM(CEDULA) AS item,
                        RTRIM(NOMBRE) AS descripcion
                    FROM SOCIOS
                    WHERE LTRIM(RTRIM(ISNULL(CEDULA,''))) <> ''
                    GROUP BY CEDULA, NOMBRE
                    ORDER BY NOMBRE;";

                    return conn.Query<DropDownListaGenericaModel>(q).ToList();
                }
                else
                {
                    const string q = @"
                    SELECT
                        RTRIM(CEDULA) AS item,
                        RTRIM(NOMBRE) AS descripcion
                    FROM SOCIOS
                    WHERE LTRIM(RTRIM(ISNULL(CEDULA,''))) <> ''
                      AND (
                            CEDULA LIKE @Q
                         OR CEDULAR LIKE @Q
                         OR NOMBRE LIKE @Q
                      )
                    GROUP BY CEDULA, NOMBRE
                    ORDER BY NOMBRE;";

                    return conn.Query<DropDownListaGenericaModel>(q, new { Q = "%" + f + "%" }).ToList();
                }
            });
        }

        /// <summary>
        /// Lista Creditos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Sys_Monitor_AutoGestion_Creditos_DropDown_Obtener(int CodEmpresa, string? filtro)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                string f = (filtro ?? "").Trim();

                if (string.IsNullOrWhiteSpace(f))
                {
                    const string q = @"
                    SELECT
                        RTRIM(CODIGO) AS item,
                        RTRIM(DESCRIPCION) AS descripcion
                    FROM CATALOGO
                    WHERE LINEA_INTERNA = 1
                      AND RETENCION = 'N'
                      AND POLIZA = 'N'
                      AND WEBSITE = 1
                    ORDER BY DESCRIPCION;";

                    return conn.Query<DropDownListaGenericaModel>(q).ToList();
                }
                else
                {
                    const string q = @"
                    SELECT
                        RTRIM(CODIGO) AS item,
                        RTRIM(DESCRIPCION) AS descripcion
                    FROM CATALOGO
                    WHERE LINEA_INTERNA = 1
                      AND RETENCION = 'N'
                      AND POLIZA = 'N'
                      AND WEBSITE = 1
                      AND (
                            CODIGO LIKE @Q
                         OR DESCRIPCION LIKE @Q
                      )
                    ORDER BY DESCRIPCION;";

                    return conn.Query<DropDownListaGenericaModel>(q, new { Q = "%" + f + "%" }).ToList();
                }
            });
        }

        /// <summary>
        /// Obtiene lista (grid) con LazyLoad, filtros y ordenamiento.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="jfiltros"></param>
        /// <param name="req"></param>
        public ErrorDto<MonitorAutoGestionLista> Sys_Monitor_AutoGestion_Lista_Obtener(int CodEmpresa, string jfiltros, MonitorAutoGestionBuscarRequest req)
        {
            var filtrosResult = TryParseFiltros(jfiltros);
            if (filtrosResult.error != null)
                return filtrosResult.error;

            var filtros = filtrosResult.filtros!;
            var response = CreateEmptyOkResponse();

            try
            {
                using var cn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                var p = new DynamicParameters();
                FillParams(p, filtros, req);

                response.Result!.total = ExecuteTotal(cn, p);

                var sort = ResolveSort(filtros);
                bool exportAll = IsExportAll(filtros);

                AddPagingParamsIfNeeded(p, filtros, exportAll);

                string sql = BuildSelectSql(sort, exportAll);
                response.Result.lista = ExecuteLista(cn, sql, p);


                return response;
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<MonitorAutoGestionLista>(ex.Message);
            }
        }

        /// <summary>
        /// Exporta la lista (sin paginar) con los mismos filtros del grid.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="jfiltros"></param>
        /// <param name="req"></param>
        public ErrorDto<MonitorAutoGestionLista> Sys_Monitor_AutoGestion_Lista_Export(int CodEmpresa, string jfiltros, MonitorAutoGestionBuscarRequest req)
        {
            var filtrosResult = TryParseFiltros(jfiltros);
            if (filtrosResult.error != null)
                return filtrosResult.error;

            var filtros = filtrosResult.filtros!;
            filtros.pagina = 0;
            filtros.paginacion = 0;

            return Sys_Monitor_AutoGestion_Lista_Obtener(CodEmpresa, JsonConvert.SerializeObject(filtros), req);
        }
        /// <summary>
        /// Obtiene el detalle de un caso por COD_SOLICITUD.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_solicitud"></param>
        public ErrorDto<MonitorAutoGestionCasoDetalle> Sys_Monitor_AutoGestion_Caso_Obtener(int CodEmpresa, long cod_solicitud)
        {
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
            REGISTRO_FECHA     AS Registro_Fecha,
            REGISTRO_USUARIO   AS Registro_Usuario,
            RES_FECHA          AS Res_Fecha,
            RES_USUARIO        AS Res_Usuario,
            RES_CODIGO         AS Res_Codigo,
            NOTAS              AS Notas,
            CASE 
                WHEN ISNULL(REFUNDE_IND,0)=0 THEN CAST(0 AS BIT) 
                ELSE CAST(1 AS BIT) 
            END AS Refunde_Ind
        FROM vCrd_Solicitudes_AutoGestion
        WHERE COD_SOLICITUD = @ID;";

            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);

                var row = cn.QueryFirstOrDefault<MonitorAutoGestionCasoDetalle>(
                    sql,
                    new { ID = cod_solicitud },
                    commandTimeout: 60
                );

                if (row == null)
                {
                    return new ErrorDto<MonitorAutoGestionCasoDetalle>
                    {
                        Code = 1,
                        Description = "Caso no encontrado.",
                        Result = null
                    };
                }

                return DbHelper.CreateOkResponse(row);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<MonitorAutoGestionCasoDetalle>(ex.Message);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<MonitorAutoGestionCasoDetalle>(ex.Message);
            }
        }
        /// <summary>
        /// Obtiene el resumen por estado en el rango de fechas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaFin"></param>
        public ErrorDto<MonitorAutoGestionResumenLista> Sys_Monitor_AutoGestion_Resumen_Obtener(int CodEmpresa, string fechaInicio, string fechaFin)
        {
            if (!TryParseFecha(fechaInicio, out var fIni) || !TryParseFecha(fechaFin, out var fFin))
                return DbHelper.CreateErrorResponse<MonitorAutoGestionResumenLista>("Fechas inválidas.");

            var ini = new DateTime(fIni.Year, fIni.Month, fIni.Day, 0, 0, 0, DateTimeKind.Unspecified);
            var fin = new DateTime(fFin.Year, fFin.Month, fFin.Day, 23, 59, 59, DateTimeKind.Unspecified);

            var res = DbHelper.WithConn(_portalDB, CodEmpresa, cn =>
            {
                var p = new DynamicParameters();
                p.Add("@Inicio", ini, DbType.DateTime);
                p.Add("@Corte", fin, DbType.DateTime);

                var lista = cn.Query<MonitorAutoGestionResumenData>(
                  "spCrd_Solicitudes_AutoGestion_Rsm",
                  p,
                  commandType: CommandType.StoredProcedure,
                  commandTimeout: 60
                ).ToList();

                return new MonitorAutoGestionResumenLista { total = lista.Count, lista = lista };
            });

            if (res.Code != 0 || res.Result == null)
                return new ErrorDto<MonitorAutoGestionResumenLista>
                {
                    Code = res.Code == 0 ? -1 : res.Code,
                    Description = string.IsNullOrWhiteSpace(res.Description) ? "Error" : res.Description,
                    Result = new MonitorAutoGestionResumenLista { total = 0, lista = new List<MonitorAutoGestionResumenData>() }
                };

            return res;
        }

        /// <summary>
        /// Lista adjuntos del caso (solo metadatos).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_solicitud"></param>
        public ErrorDto<MonitorAutoGestionAdjuntosLista> Sys_Monitor_AutoGestion_Adjuntos_Obtener(int CodEmpresa, long cod_solicitud)
        {
            const string sql = @"
                SELECT 
                    A.ARCHIVO_ID     AS Archivo_Id,
                    T.DESCRIPCION    AS Tipo_Adjunto,
                    A.ARCHIVO_NOMBRE AS Archivo_Nombre,
                    A.ARCHIVO_TIPO   AS Archivo_Tipo
                FROM CRD_SOLICITUDES_ADJUNTOS A
                INNER JOIN CRD_ADJUNTOS_TIPOS T ON A.COD_ADJUNTO = T.COD_ADJUNTO
                WHERE A.TRANSAC_TIPO = 'SOL'
                  AND A.TRANSAC_CODIGO = @ID
                ORDER BY A.ARCHIVO_ID ASC;";

            var res = DbHelper.WithConn(_portalDB, CodEmpresa, cn =>
            {
                var lista = cn.Query<MonitorAutoGestionAdjuntoData>(
                    sql,
                    new { ID = cod_solicitud },
                    commandTimeout: 60
                ).ToList();

                return new MonitorAutoGestionAdjuntosLista
                {
                    total = lista.Count,
                    lista = lista
                };
            });

            if (res.Code != 0 || res.Result == null)
            {
                return new ErrorDto<MonitorAutoGestionAdjuntosLista>
                {
                    Code = res.Code == 0 ? -1 : res.Code,
                    Description = string.IsNullOrWhiteSpace(res.Description) ? "Error" : res.Description,
                    Result = new MonitorAutoGestionAdjuntosLista
                    {
                        total = 0,
                        lista = new List<MonitorAutoGestionAdjuntoData>()
                    }
                };
            }

            return res;
        }
        /// <summary>
        /// Descarga un adjunto (bytes, nombre y tipo).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="archivo_id"></param>
        public ErrorDto<(byte[] buffer, string nombre, string tipo)> Sys_Monitor_AutoGestion_Adjunto_Descargar(int CodEmpresa, long archivo_id)
        {
            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);
                cn.Open();

                using var rdr = ExecuteAdjuntoReader(cn, archivo_id);

                if (!rdr.Read())
                    return DbHelper.CreateErrorResponse<(byte[] buffer, string nombre, string tipo)>("Adjunto no encontrado.", code: 1, result: (Array.Empty<byte>(), "", ""));

                var nombre = ReadNombre(rdr);
                var tipoFinal = ReadTipoFinal(rdr, ref nombre);
                var bytes = ReadArchivoBytes(rdr, "ARCHIVO_BIT");

                return DbHelper.CreateOkResponse((bytes, nombre, tipoFinal));
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<(byte[] buffer, string nombre, string tipo)>(ex.Message, result: (Array.Empty<byte>(), "", ""));
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<(byte[] buffer, string nombre, string tipo)>(ex.Message, result: (Array.Empty<byte>(), "", ""));
            }
        }
        private static SqlDataReader ExecuteAdjuntoReader(SqlConnection cn, long archivoId)
        {
            const string sql = @"
            SELECT ARCHIVO_NOMBRE, ARCHIVO_TIPO, ARCHIVO_BIT
            FROM CRD_SOLICITUDES_ADJUNTOS
            WHERE ARCHIVO_ID = @ID;";

            var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = archivoId;
            return cmd.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.SingleRow);
        }

        private static string ReadNombre(IDataRecord rdr)
        {
            return (rdr["ARCHIVO_NOMBRE"] as string ?? "archivo").Trim();
        }

        private static string ReadTipoFinal(IDataRecord rdr, ref string nombre)
        {
            var tipoDb = (rdr["ARCHIVO_TIPO"] as string ?? "").Trim();
            return ResolveMimeAndFixFileName(ref nombre, tipoDb);
        }

        private static byte[] ReadArchivoBytes(SqlDataReader rdr, string columnName)
        {
            const int ChunkSize = 1024 * 64;

            var ord = rdr.GetOrdinal(columnName);
            var len = rdr.GetBytes(ord, 0, null, 0, 0);

            if (len <= 0)
                return Array.Empty<byte>();

            using var ms = new MemoryStream(capacity: len > int.MaxValue ? 0 : (int)len);

            var buffer = new byte[ChunkSize];
            long offset = 0;

            while (offset < len)
            {
                var toRead = (int)Math.Min(ChunkSize, len - offset);
                var read = (int)rdr.GetBytes(ord, offset, buffer, 0, toRead);
                if (read <= 0) break;

                ms.Write(buffer, 0, read);
                offset += read;
            }

            return ms.ToArray();
        }
        private static string ResolveMimeAndFixFileName(ref string nombre, string? tipoDb)
        {
            var tipo = (tipoDb ?? string.Empty).Trim();

            if (tipo.Length > 0 && tipo.Contains('/'))
                return tipo;

            string ext = tipo.Trim('.').ToLowerInvariant();

            if (string.IsNullOrEmpty(ext))
            {
                int dot = nombre.LastIndexOf('.');
                if (dot >= 0 && dot < nombre.Length - 1)
                    ext = nombre[(dot + 1)..].ToLowerInvariant();
            }

            string mime = ext switch
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

            if (!string.IsNullOrEmpty(ext) && !nombre.Contains('.'))
                nombre = $"{nombre}.{ext}";

            return mime;
        }


        /// <summary>
        /// Aplica resolución del caso llamando al SP y registra bitácora.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="dto"></param>
        public ErrorDto<MonitorAutoGestionResolucionResponse> Sys_Monitor_AutoGestion_Resolucion_Aplicar(int CodEmpresa,MonitorAutoGestionResolucionRequest dto)
        {
            if (!dto.cod_solicitud.HasValue)
                return DbHelper.CreateErrorResponse<MonitorAutoGestionResolucionResponse>(
                    "Código de solicitud inválido.");

            long codSolicitud = dto.cod_solicitud.Value;

            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);

                EjecutarResolucionSP(cn, dto);

                var response = ConstruirResolucionResponse(
                    CodEmpresa,
                    codSolicitud);

                RegistrarBitacoraResolucion(CodEmpresa, dto);

                return DbHelper.CreateOkResponse(response);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<MonitorAutoGestionResolucionResponse>(ex.Message);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<MonitorAutoGestionResolucionResponse>(ex.Message);
            }
        }

        /// <summary>
        /// Ejecuta mantenimiento de adjuntos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        public ErrorDto Sys_Monitor_AutoGestion_Adjuntos_Fix(int CodEmpresa)
        {
            try
            {
                using var cn = _portalDB.CreateConnection(CodEmpresa);

                cn.Execute(
                    "spCrd_Solicitudes_Adjuntos_Fix",
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 60
                );

                return DbHelper.CreateOkResponse();
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static (FiltrosLazyLoadData? filtros, ErrorDto<MonitorAutoGestionLista>? error) TryParseFiltros(string jfiltros)
        {
            try
            {
                var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(jfiltros) ?? new FiltrosLazyLoadData();
                return (filtros, null);
            }
            catch (JsonException ex)
            {
                return (null, DbHelper.CreateErrorResponse<MonitorAutoGestionLista>(ex.Message));
            }
        }
        private ErrorDto<MonitorAutoGestionLista> CreateEmptyOkResponse()
        {
            var response = DbHelper.CreateOkResponse(new MonitorAutoGestionLista
            {
                total = 0,
                lista = new List<MonitorAutoGestionListaData>()
            });

            response.Result ??= new MonitorAutoGestionLista
            {
                total = 0,
                lista = new List<MonitorAutoGestionListaData>()
            };

            return response;
        }
        private static bool IsExportAll(FiltrosLazyLoadData filtros)
        {
            return filtros.paginacion == 0;
        }
        private static (string orderBy, string orderDir) ResolveSort(FiltrosLazyLoadData filtros)
        {
            string orderBy = ResolveOrderBy(filtros.sortField);
            string orderDir = ResolveOrderDir(filtros.sortOrder);
            return (orderBy, orderDir);
        }
        private static string ResolveOrderDir(int sortOrder)
        {
            return sortOrder < 0 ? "DESC" : "ASC";
        }
        private static string ResolveOrderBy(string? sortField)
        {
            var sf = (sortField ?? "").Trim().ToLowerInvariant();

            return sf switch
            {
                "cod_solicitud" => "COD_SOLICITUD",
                "estado_desc" => "ESTADO_DESC",
                "cedula" => "CEDULA",
                "nombre" => "NOMBRE",
                "linea_desc" => "LINEA_DESC",
                "monto" => "MONTO",
                "plazo" => "PLAZO",
                "tasa" => "TASA",
                "cuota" => "CUOTA",
                "garantia_desc" => "GARANTIA_DESC",
                "registro_fecha" => "REGISTRO_FECHA",
                "res_fecha" => "RES_FECHA",
                "res_codigo" => "RES_CODIGO",
                "tramite_estado_desc" => "TRAMITE_ESTADO_DESC",
                "res_tipo" => "RES_TIPO",
                _ => "COD_SOLICITUD"
            };
        }
        private static int ExecuteTotal(SqlConnection cn, DynamicParameters p)
        {
            const string sqlCount = @"
                SELECT COUNT(1)
                FROM dbo.vCrd_Solicitudes_AutoGestion
                WHERE 1=1
                  AND @FECHA_INVALIDA = 0
                  AND (@ESTADO  IS NULL OR ESTADO = @ESTADO)
                  AND (@TRAMITE IS NULL OR TRAMITE_ESTADO_ID = @TRAMITE)
                  AND (@CODIGO  IS NULL OR CODIGO = @CODIGO)
                  AND (@CEDULA  IS NULL OR CEDULA = @CEDULA)
                  AND (
                        @Q IS NULL OR (
                             CEDULA      LIKE @Q
                          OR NOMBRE      LIKE @Q
                          OR LINEA_DESC  LIKE @Q
                          OR ESTADO_DESC LIKE @Q
                        )
                      )
                  AND (
                        @FECHA_TIPO IS NULL OR @FECHA_TIPO = 'Todas'
                     OR (@FECHA_TIPO = 'Registro' AND REGISTRO_FECHA BETWEEN @INI AND @FIN)
                     OR ((@FECHA_TIPO = 'Resolución' OR @FECHA_TIPO = 'Resolucion') AND RES_FECHA BETWEEN @INI AND @FIN)
                      );
                ";
            return cn.ExecuteScalar<int>(sqlCount, p, commandTimeout: 60);
        }
        private static void AddPagingParamsIfNeeded(DynamicParameters p, FiltrosLazyLoadData filtros, bool exportAll)
        {
            if (exportAll) return;

            int offset = GetOffset(filtros);
            int fetch = Math.Max(1, filtros.paginacion);

            p.Add("@OFFSET", offset, DbType.Int32);
            p.Add("@FETCH", fetch, DbType.Int32);
        }
        private static int GetOffset(FiltrosLazyLoadData filtros)
        {
            return Math.Max(0, filtros.pagina);
        }
        private static string BuildSelectSql((string orderBy, string orderDir) sort, bool exportAll)
        {
            string baseSelect = @"
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
            FROM dbo.vCrd_Solicitudes_AutoGestion
            WHERE 1=1
              AND @FECHA_INVALIDA = 0
              AND (@ESTADO  IS NULL OR ESTADO = @ESTADO)
              AND (@TRAMITE IS NULL OR TRAMITE_ESTADO_ID = @TRAMITE)
              AND (@CODIGO  IS NULL OR CODIGO = @CODIGO)
              AND (@CEDULA  IS NULL OR CEDULA = @CEDULA)
              AND (
                    @Q IS NULL OR (
                         CEDULA      LIKE @Q
                      OR NOMBRE      LIKE @Q
                      OR LINEA_DESC  LIKE @Q
                      OR ESTADO_DESC LIKE @Q
                    )
                  )
              AND (
                    @FECHA_TIPO IS NULL OR @FECHA_TIPO = 'Todas'
                 OR (@FECHA_TIPO = 'Registro' AND REGISTRO_FECHA BETWEEN @INI AND @FIN)
                 OR ((@FECHA_TIPO = 'Resolución' OR @FECHA_TIPO = 'Resolucion') AND RES_FECHA BETWEEN @INI AND @FIN)
                  )
            ";

            string orderClause = BuildOrderClause(sort.orderBy, sort.orderDir);

            if (exportAll)
                return baseSelect + orderClause + ";";

            return baseSelect + orderClause + " OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY;";
        }
        private static string BuildOrderClause(string orderBy, string orderDir)
        {
            if (orderBy.Equals("COD_SOLICITUD", StringComparison.OrdinalIgnoreCase))
                return " ORDER BY COD_SOLICITUD " + orderDir;
            return " ORDER BY " + orderBy + " " + orderDir + ", COD_SOLICITUD " + orderDir;
        }
        private static List<MonitorAutoGestionListaData> ExecuteLista(SqlConnection cn, string sql, DynamicParameters p)
        {
            return cn.Query<MonitorAutoGestionListaData>(sql, p, commandTimeout: 0).ToList();
        }
        private static void FillParams(DynamicParameters p, FiltrosLazyLoadData filtros, MonitorAutoGestionBuscarRequest req)
        {
            string estado1 = Normalizar1Char(req.estado);
            p.Add("@ESTADO", string.IsNullOrWhiteSpace(estado1) ? null : estado1);

            string tramite1 = Normalizar1Char(req.tramite_estado_id);
            p.Add("@TRAMITE", string.IsNullOrWhiteSpace(tramite1) ? null : tramite1);

            p.Add("@CODIGO", string.IsNullOrWhiteSpace(req.codigoLinea) ? null : req.codigoLinea.Trim());
            p.Add("@CEDULA", string.IsNullOrWhiteSpace(req.cedula) ? null : req.cedula.Trim());

            string q = (filtros?.filtro ?? "").Trim();
            p.Add("@Q", string.IsNullOrWhiteSpace(q) ? null : "%" + q + "%");

            string ft = (req.fechaTipo ?? "").Trim();
            p.Add("@FECHA_TIPO", string.IsNullOrWhiteSpace(ft) ? null : ft);

            p.Add("@FECHA_INVALIDA", 0);

            var sqlMin = new DateTime(1753, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

            bool esRegistro = ft.Equals("Registro", StringComparison.OrdinalIgnoreCase);
            bool esResolucion = ft.Equals("Resolución", StringComparison.OrdinalIgnoreCase) || ft.Equals("Resolucion", StringComparison.OrdinalIgnoreCase);
            bool esTodas = ft.Equals("Todas", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(ft);

            if (esRegistro || esResolucion)
            {
                if (!TryParseFecha(req.fechaInicio, out var fIni) || !TryParseFecha(req.fechaFin, out var fFin))
                {
                    p.Add("@FECHA_INVALIDA", 1);
                    p.Add("@INI", sqlMin, DbType.DateTime);
                    p.Add("@FIN", sqlMin, DbType.DateTime);
                    return;
                }

                var ini = new DateTime(fIni.Year, fIni.Month, fIni.Day, 0, 0, 0, DateTimeKind.Unspecified);
                var fin = new DateTime(fFin.Year, fFin.Month, fFin.Day, 23, 59, 59, DateTimeKind.Unspecified);

                p.Add("@INI", ini, DbType.DateTime);
                p.Add("@FIN", fin, DbType.DateTime);
                return;
            }

            if (esTodas)
            {
                p.Add("@INI", sqlMin, DbType.DateTime);
                p.Add("@FIN", sqlMin, DbType.DateTime);
                return;
            }

            p.Add("@INI", sqlMin, DbType.DateTime);
            p.Add("@FIN", sqlMin, DbType.DateTime);
        }
        private static bool TryParseFecha(string? value, out DateTime fecha)
        {
            fecha = default;
            var s = (value ?? "").Trim();
            if (s.Length == 0) return false;

            string[] formats = { "dd-MM-yyyy", "dd/MM/yyyy", "yyyy-MM-dd", "yyyy/MM/dd" };
            return DateTime.TryParseExact(
                s,
                formats,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out fecha
            );
        }
        private static string Normalizar1Char(string? value)
        {
            var s = (value ?? "").Trim();
            if (s.Length == 0) return "";
            return s.Substring(0, 1).ToUpperInvariant();
        }
        private static void EjecutarResolucionSP(SqlConnection cn, MonitorAutoGestionResolucionRequest dto)
        {
            var p = new DynamicParameters();
            p.Add("@Solicitud", dto.cod_solicitud);
            p.Add("@Resolucion", Normalizar1Char(dto.resolucion ?? "P"));
            p.Add("@Notas", dto.notas ?? "");
            p.Add("@Usuario", dto.usuario ?? "");
            p.Add("@Gestion", Normalizar1Char(dto.gestion ?? "S"));

            cn.Execute(
                "spCrd_Solicitudes_AutoGestion_Resolucion",
                p,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 90
            );
        }
        private MonitorAutoGestionResolucionResponse ConstruirResolucionResponse(int codEmpresa, long codSolicitud)
        {
            var response = new MonitorAutoGestionResolucionResponse
            {
                cod_solicitud = codSolicitud
            };

            var post = Sys_Monitor_AutoGestion_Caso_Obtener(codEmpresa, codSolicitud);
            if (post.Code != 0 || post.Result == null)
                return response;

            response.estado = post.Result.Estado;
            response.estado_desc = post.Result.Estado_Desc;
            response.res_fecha = post.Result.Res_Fecha;
            response.res_usuario = post.Result.Res_Usuario;
            response.res_codigo = post.Result.Res_Codigo;
            response.notas = post.Result.Notas;

            return response;
        }
        private void RegistrarBitacoraResolucion(int codEmpresa, MonitorAutoGestionResolucionRequest dto)
        {
            string usuario = dto.usuario ?? "";
            string resolucion = Normalizar1Char(dto.resolucion ?? "P");
            string gestion = Normalizar1Char(dto.gestion ?? "S");

            _security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                Modulo = vModulo,
                Movimiento = "Resolución - WEB",
                DetalleMovimiento = $"Caso {dto.cod_solicitud} → {resolucion}/{gestion}"
            });
        }
    }
}
