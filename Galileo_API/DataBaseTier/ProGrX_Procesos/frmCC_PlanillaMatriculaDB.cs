using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Procesos;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX.Procesos
{
    public class FrmCCPlanillaMatriculaDB
    {
        private const int ModuloProcesos = 10;
        private const int BatchSqlLength = 20000;

        private static readonly Dictionary<string, string> SortColumns = new()
        {
            ["id_referencia"] = "ID_REFERENCIA",
            ["tipo"] = "TIPO",
            ["fecha_proceso"] = "FECHA_PROCESO",
            ["cod_deduccion"] = "COD_DEDUCCION",
            ["operacion"] = "OPERACION",
            ["formalizacion"] = "FORMALIZACION",
            ["monto"] = "MONTO",
            ["cuota"] = "CUOTA",
            ["plazo"] = "PLAZO",
            ["tasa"] = "TASA",
            ["nreferencia_ext"] = "NREFENCIA_EXT",
            ["cedula"] = "CEDULA",
            ["nombre"] = "NOMBRE",
            ["b_indica"] = "B_INDICA"
        };
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        public FrmCCPlanillaMatriculaDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }
        /// <summary>
        /// Inserta en bitácora un movimiento del módulo.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDb.Bitacora(data);
        }
        /// <summary>
        /// Obtiene las instituciones activas para el selector principal.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CC_PlanillaMatricula_Instituciones_Dropdown_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string sql = @"
                    select
                        cast(COD_INSTITUCION as varchar(20)) as item,
                        rtrim(DESCRIPCION) as descripcion
                    from INSTITUCIONES
                    where ACTIVA = 1
                    order by DESCRIPCION;";
                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }
        /// <summary>
        /// Obtiene la lista principal de matrículas según filtros de pantalla.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CcPlanillaMatriculaListaResultDto> CC_PlanillaMatricula_Lista_Obtener(int CodEmpresa, string parametros)
        {
            var response = new CcPlanillaMatriculaListaResultDto();
            try
            {
                var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros) ?? new FiltrosLazyLoadData();
                var filtroPantalla = JsonConvert.DeserializeObject<CcPlanillaMatriculaFiltroDto>(filtros.filtro ?? string.Empty)
                                     ?? new CcPlanillaMatriculaFiltroDto();
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var sqlInfo = BuildListaSql(filtroPantalla, filtros);
                response.total = conn.QueryFirstOrDefault<int>(
                    sqlInfo.SqlCount,
                    sqlInfo.Parameters);

                response.lista = conn.Query<CcPlanillaMatriculaListaDto>(
                    sqlInfo.SqlLista,
                    sqlInfo.Parameters).ToList();

                return DbHelper.CreateOkResponse(response);
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
        }
        /// <summary>
        /// Exporta la lista principal de matrículas según filtros de pantalla.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CcPlanillaMatriculaListaResultDto> CC_PlanillaMatricula_Lista_Export(int CodEmpresa, string parametros)
        {
            try
            {
                var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros) ?? new FiltrosLazyLoadData();
                filtros.pagina = 0;
                filtros.paginacion = 0;

                return CC_PlanillaMatricula_Lista_Obtener(CodEmpresa, JsonConvert.SerializeObject(filtros));
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CcPlanillaMatriculaListaResultDto());
            }
        }
        /// <summary>
        /// Bloquea una matrícula individual por referencia.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CC_PlanillaMatricula_Bloquear(int CodEmpresa, string usuario, CcPlanillaMatriculaBloquearRequest request)
        {
            try
            {
                if (request == null || request.id_referencia <= 0)
                {
                    return DbHelper.ErrorResponse("La referencia es requerida.");
                }
                var usuarioNorm = (usuario ?? string.Empty).Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(usuarioNorm))
                {
                    return DbHelper.ErrorResponse("El usuario es requerido.");
                }
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                const string sp = @"
                    exec spPrm_Matricula_Bloqueo
                        @idReferencia,
                        @Usuario;";
                conn.Execute(sp, new
                {
                    idReferencia = request.id_referencia,
                    Usuario = usuarioNorm
                }, commandTimeout: 0);
                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuarioNorm,
                    DetalleMovimiento = $"Planilla Matrícula - Bloqueo referencia [{request.id_referencia}]",
                    Movimiento = "MODIFICA-WEB",
                    Modulo = ModuloProcesos
                });
                return DbHelper.OkResponse("Caso bloqueado satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
        /// <summary>
        /// Ejecuta bloqueo masivo de matrículas desde datos cargados por archivo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CcPlanillaMatriculaBloqueoMasivoResultDto> CC_PlanillaMatricula_BloqueoMasivo(int CodEmpresa,string usuario,CcPlanillaMatriculaBloqueoMasivoRequest request)
        {
            var response = new CcPlanillaMatriculaBloqueoMasivoResultDto();
            try
            {
                if (request == null)
                {
                    return DbHelper.CreateErrorResponse("La solicitud es requerida.", -1, response);
                }
                if (request.cod_institucion <= 0)
                {
                    return DbHelper.CreateErrorResponse("La institución es requerida.", -1, response);
                }
                var usuarioNorm = (usuario ?? string.Empty).Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(usuarioNorm))
                {
                    return DbHelper.CreateErrorResponse("El usuario es requerido.", -1, response);
                }
                var items = NormalizarBloqueoMasivo(request.items);
                if (items.Count == 0)
                {
                    return DbHelper.CreateErrorResponse("No existen registros válidos para bloquear.", -1, response);
                }
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                using var tx = conn.BeginTransaction();
                EjecutarBloqueoMasivoBatch(conn, tx, request.cod_institucion, usuarioNorm, items);
                tx.Commit();
                response.casos_bloqueados = items.Count;
                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuarioNorm,
                    DetalleMovimiento = $"Planilla Matrícula - Bloqueo masivo institución [{request.cod_institucion}] Registros={response.casos_bloqueados}",
                    Movimiento = "MODIFICA-WEB",
                    Modulo = ModuloProcesos
                });
                return DbHelper.CreateOkResponse(response);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
        }
        /// <summary>
        /// Genera el archivo CSV de matrícula total para la institución indicada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CcPlanillaMatriculaArchivoTotalDto> CC_PlanillaMatricula_ArchivoTotal_Generar(int CodEmpresa,CcPlanillaMatriculaArchivoTotalRequest request)
        {
            var response = new CcPlanillaMatriculaArchivoTotalDto();
            try
            {
                if (request == null || request.cod_institucion <= 0)
                {
                    return DbHelper.CreateErrorResponse("La institución es requerida.", -1, response);
                }
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
                var institucion = ObtenerDatosInstitucion(conn, request.cod_institucion);
                if (institucion == null)
                {
                    return DbHelper.CreateErrorResponse("No se encontró la institución indicada.", -1, response);
                }
                const string sp = @"
                    exec spPrm_Formato_Integra_New_Matricula_Total
                        @Institucion;";
                var lineas = conn.Query<CcPlanillaMatriculaCadenaDto>(sp, new
                {
                    Institucion = request.cod_institucion
                }, commandTimeout: 0).ToList();
                var contenido = string.Join(Environment.NewLine, lineas.Select(x => x.cadena ?? string.Empty));
                var bytes = Encoding.UTF8.GetBytes(contenido);
                response.nombre_archivo = BuildNombreArchivoTotal(institucion.codigo_inst_deduc);
                response.contenido_base64 = Convert.ToBase64String(bytes);
                response.content_type = "text/csv";
                return DbHelper.CreateOkResponse(response);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
        }
        private static CcPlanillaMatriculaSqlResult BuildListaSql(CcPlanillaMatriculaFiltroDto filtro,FiltrosLazyLoadData lazy)
        {
            var parameters = BuildListaParameters(filtro, lazy, out var usarPaginacion);
            var sqlCount = new StringBuilder();
            sqlCount.AppendLine("select count(1)");
            sqlCount.AppendLine("from vPrm_Matricula");
            AppendWhereLista(sqlCount, filtro);
            var sqlLista = new StringBuilder();
            sqlLista.AppendLine(@"
                select
                    ID_REFERENCIA as id_referencia,
                    rtrim(TIPO) as tipo,
                    FECHA_PROCESO as fecha_proceso,
                    rtrim(COD_DEDUCCION) as cod_deduccion,
                    ID_SOLICITUD as id_solicitud,
                    rtrim(OPERACION) as operacion,
                    FORMALIZACION as formalizacion,
                    isnull(MONTO, 0) as monto,
                    isnull(CUOTA, 0) as cuota,
                    isnull(PLAZO, 0) as plazo,
                    isnull(TASA, 0) as tasa,
                    rtrim(isnull(NREFENCIA_EXT, '')) as nreferencia_ext,
                    rtrim(CEDULA) as cedula,
                    rtrim(NOMBRE) as nombre,
                    isnull(B_INDICA, 0) as b_indica
                from vPrm_Matricula");
            AppendWhereLista(sqlLista, filtro);
            AppendOrderBy(sqlLista, lazy.sortField, lazy.sortOrder);
            if (usarPaginacion)
            {
                sqlLista.AppendLine("offset @offset rows fetch next @fetch rows only;");
            }
            return new CcPlanillaMatriculaSqlResult
            {
                SqlCount = sqlCount.ToString(),
                SqlLista = sqlLista.ToString(),
                Parameters = parameters
            };
        }
        private static DynamicParameters BuildListaParameters(CcPlanillaMatriculaFiltroDto filtro,FiltrosLazyLoadData lazy,out bool usarPaginacion)
        {
            var parameters = new DynamicParameters();
            parameters.Add("cod_institucion", filtro.cod_institucion);
            parameters.Add("b_indica", filtro.casos_activos ? 0 : 1);
            AddLikeParameter(parameters, "cedula", filtro.cedula);
            AddLikeParameter(parameters, "nombre", filtro.nombre);
            AddLikeParameter(parameters, "codigo", filtro.codigo);
            AddLikeParameter(parameters, "operacion", filtro.operacion);
            AddLikeParameter(parameters, "doc_referencia", filtro.doc_referencia);

            if (filtro.proceso.HasValue && filtro.proceso.Value > 0)
            {
                parameters.Add("proceso", filtro.proceso.Value);
            }
            var pagina = lazy.pagina < 0 ? 0 : lazy.pagina;
            var paginacion = lazy.paginacion < 0 ? 0 : lazy.paginacion;
            usarPaginacion = paginacion > 0;
            if (usarPaginacion)
            {
                parameters.Add("offset", pagina * paginacion);
                parameters.Add("fetch", paginacion);
            }
            return parameters;
        }
        private static void AppendWhereLista(StringBuilder sql, CcPlanillaMatriculaFiltroDto filtro)
        {
            sql.AppendLine("where COD_INSTITUCION = @cod_institucion");
            sql.AppendLine("  and isnull(B_INDICA, 0) = @b_indica");
            AppendLikeCondition(sql, "CEDULA", "cedula", filtro.cedula);
            AppendLikeCondition(sql, "NOMBRE", "nombre", filtro.nombre);
            AppendLikeCondition(sql, "COD_DEDUCCION", "codigo", filtro.codigo);
            AppendLikeCondition(sql, "OPERACION", "operacion", filtro.operacion);
            AppendLikeCondition(sql, "NREFENCIA_EXT", "doc_referencia", filtro.doc_referencia);
            if (filtro.proceso.HasValue && filtro.proceso.Value > 0)
            {
                sql.AppendLine("  and FECHA_PROCESO = @proceso");
            }
        }
        private static void AppendLikeCondition(StringBuilder sql,string column,string parameterName,string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }
            sql.Append("  and ");
            sql.Append(column);
            sql.Append(" like @");
            sql.AppendLine(parameterName);
        }
        private static void AddLikeParameter(DynamicParameters parameters,string parameterName,string? value)
        {
            var text = (value ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }
            parameters.Add(parameterName, $"%{text}%");
        }
        private static void AppendOrderBy(StringBuilder sql, string? sortField, int sortOrder)
        {
            var field = (sortField ?? string.Empty).Trim().ToLowerInvariant();
            if (!SortColumns.TryGetValue(field, out var column))
            {
                column = "ID_REFERENCIA";
            }
            var direction = sortOrder == 1 ? "asc" : "desc";
            sql.Append("order by ");
            sql.Append(column);
            sql.Append(' ');
            sql.AppendLine(direction);
        }
        private static List<CcPlanillaMatriculaBloqueoMasivoItem> NormalizarBloqueoMasivo(List<CcPlanillaMatriculaBloqueoMasivoItem> items)
        {
            return (items ?? new List<CcPlanillaMatriculaBloqueoMasivoItem>())
                .Select(x => new CcPlanillaMatriculaBloqueoMasivoItem
                {
                    cedula = (x.cedula ?? string.Empty).Trim(),
                    numerooperacion = (x.numerooperacion ?? string.Empty).Trim(),
                    codigodeduccion = (x.codigodeduccion ?? string.Empty).Trim()
                })
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.cedula) &&
                    !string.IsNullOrWhiteSpace(x.numerooperacion) &&
                    !string.IsNullOrWhiteSpace(x.codigodeduccion))
                .ToList();
        }
        private static void EjecutarBloqueoMasivoBatch(SqlConnection conn,SqlTransaction tx,int codInstitucion,string usuario,List<CcPlanillaMatriculaBloqueoMasivoItem> items)
        {
            var sql = new StringBuilder();
            var parameters = new DynamicParameters();
            var index = 0;
            foreach (var item in items)
            {
                AppendBloqueoMasivoExec(sql, index);
                parameters.Add($"Institucion{index}", codInstitucion);
                parameters.Add($"Cedula{index}", item.cedula);
                parameters.Add($"Codigo{index}", item.codigodeduccion);
                parameters.Add($"Operacion{index}", item.numerooperacion);
                parameters.Add($"Usuario{index}", usuario);
                index++;
                if (sql.Length > BatchSqlLength)
                {
                    conn.Execute(sql.ToString(), parameters, tx, commandTimeout: 0);
                    sql.Clear();
                    parameters = new DynamicParameters();
                }
            }
            if (sql.Length > 0)
            {
                conn.Execute(sql.ToString(), parameters, tx, commandTimeout: 0);
            }
        }
        private static void AppendBloqueoMasivoExec(StringBuilder sql, int index)
        {
            sql.AppendLine("exec spPrm_Matricula_Bloqueo_Masivo");
            sql.Append("    @Institucion");
            sql.Append(index);
            sql.AppendLine(",");
            sql.Append("    @Cedula");
            sql.Append(index);
            sql.AppendLine(",");
            sql.Append("    @Codigo");
            sql.Append(index);
            sql.AppendLine(",");
            sql.Append("    @Operacion");
            sql.Append(index);
            sql.AppendLine(",");
            sql.Append("    @Usuario");
            sql.Append(index);
            sql.AppendLine(";");
        }
        private static CcPlanillaMatriculaInstitucionArchivoDto? ObtenerDatosInstitucion(SqlConnection conn,int codInstitucion)
        {
            const string sql = @"
                select
                    cast(cod_institucion as int) as cod_institucion,
                    rtrim(isnull(codigo_inst_deduc, '')) as codigo_inst_deduc
                from instituciones
                where cod_institucion = @codInstitucion;";

            return conn.QueryFirstOrDefault<CcPlanillaMatriculaInstitucionArchivoDto>(
                sql,
                new { codInstitucion });
        }
        private static string BuildNombreArchivoTotal(string codigoInstitucionDeduc)
        {
            var codigo = (codigoInstitucionDeduc ?? string.Empty).Trim();
            var fecha = DateTime.Now;
            return $"MD-{codigo}-{fecha:yyyyMMdd}-01_TOTAL.csv";
        }
        private sealed class CcPlanillaMatriculaSqlResult
        {
            public string SqlCount { get; set; } = string.Empty;

            public string SqlLista { get; set; } = string.Empty;

            public DynamicParameters Parameters { get; set; } = new DynamicParameters();
        }
        private sealed class CcPlanillaMatriculaInstitucionArchivoDto
        {
            public int cod_institucion { get; set; } = 0;
            public string codigo_inst_deduc { get; set; } = string.Empty;
        }
    }
}