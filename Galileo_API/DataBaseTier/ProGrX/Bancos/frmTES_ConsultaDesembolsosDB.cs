using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using System.Text;

namespace Galileo_API.DataBaseTier.TES
{
    public class FrmTesConsultaDesembolsosDB
    {
        private readonly PortalDB _portalDB;
        private readonly string vFechaEmision = "fecha_emision";

        public FrmTesConsultaDesembolsosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Verifica si el usuario tiene autorización para realizar una operación en la empresa especificada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ErrorDto VerificarAutorizacion(int codEmpresa, string usuario)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
            try
            {
                const string query = @"SELECT COUNT(*) 
                               FROM TES_AUTORIZACIONES 
                               WHERE ESTADO = 'A' AND NOMBRE = @Usuario";
                int count = conn.ExecuteScalar<int>(query, new { Usuario = usuario });

                if (count == 0)
                {
                    return DbHelper.ErrorResponse("No tiene autorización para realizar esta operación.", 2);
                }

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error al verificar la autorización: {ex.Message}", -1);
            }
        }


        /// <summary>
        /// Obtiene los grupos de bancos activos de la base de datos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de grupos de bancos.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_Grupos_Obtener(int codEmpresa)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string query = @"
                                    SELECT 
                                        COD_GRUPO as item, 
                                        DESCRIPCION
                                    FROM TES_BANCOS_GRUPOS
                                    WHERE ACTIVO = 1
                                    ORDER BY DESCRIPCION";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }


        /// <summary>
        /// Obtiene las cuentas de la base de datos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="codGrupo">Código del grupo de bancos (opcional).</param>
        /// <returns>Lista de cuentas.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_Cuentas_Obtener(int codEmpresa, string usuario, string? codGrupo = null)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            if (codGrupo == "null") codGrupo = null;

            try
            {
                
                var sql = @"
                            SELECT 
                                id_Banco AS item,
                                RTRIM(Descripcion) AS descripcion
                            FROM Tes_Bancos
                            WHERE estado = 'A'";

                if (!string.IsNullOrWhiteSpace(codGrupo) && codGrupo.ToUpper() != "TODOS")
                {
                    sql += " AND Cod_Grupo = @CodGrupo";
                }

                sql += $@" AND id_Banco 
                                    in(select id_banco from tes_documentos_ASG Where nombre = @usuario and Solicita = 1 
                                    group by id_banco)";


                var result = conn.Query<DropDownListaGenericaModel>(sql, new
                {
                    CodGrupo = codGrupo,
                    usuario = usuario
                }).ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>($"Error al obtener las cuentas: {ex.Message}");
            }
        }


        /// <summary>
        /// Obtiene los conceptos de la base de datos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de conceptos.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Conceptos_Obtener(int codEmpresa)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string query = @"
                            SELECT 
                                COD_CONCEPTO as item,
                                RTRIM(Descripcion) AS descripcion
                            FROM TES_CONCEPTOS
                            WHERE estado = 'A'";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }


        /// <summary>
        /// Obtiene los tipos de documentos de la base de datos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Lista de tipos de documentos.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Tipos_Documentos_Obtener(int codEmpresa)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string query = @"
                            SELECT 
                                TIPO as item,
                                RTRIM(DESCRIPCION) AS descripcion
                            FROM TES_TIPOS_DOC";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }


        /// <summary>
        /// Busca desembolsos en la base de datos según los filtros proporcionados.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="CodConta"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ErrorDto<DesembolsosLista> Desembolsos_Buscar(int codEmpresa, int CodConta, FiltrosBusqueda filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            try
            {
                filtros ??= new FiltrosBusqueda();

                var p = CreateBaseParameters(CodConta, filtros);

                var where = BuildWhere(filtros);
                var orderBy = BuildOrderBy(filtros);
                var paging = BuildPaging(filtros);

                var sql = BuildSqlList(where, orderBy, paging);
                var sqlTotal = BuildSqlTotal(where);

                var lista = conn.Query<Desembolsos>(sql, p).ToList();
                var totales = conn.QueryFirstOrDefault<DesembolsoTotales>(sqlTotal, p) ?? new DesembolsoTotales();

                return DbHelper.CreateOkResponse(new DesembolsosLista { lista = lista, totales = totales });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<DesembolsosLista>($"Error al buscar desembolsos: {ex.Message}");
            }
        }

        private static DynamicParameters CreateBaseParameters(int codConta, FiltrosBusqueda filtros)
        {
            var p = new DynamicParameters();
            p.Add("@CodContabilidad", codConta);

            // filtro general
            var texto = filtros.filtro?.Trim();
            var hasFiltro = !string.IsNullOrWhiteSpace(texto);
            p.Add("@like", hasFiltro ? $"%{texto}%" : null);

            // fechas (si no se usan, no pasa nada)
            string vFechaIni = MProGrXAuxiliarDB.validaFechaGlobal(Convert.ToDateTime(filtros.FechaInicio), "yyyy-MM-dd" + " 00:00:00") ?? "";
            string vFechaCorte = MProGrXAuxiliarDB.validaFechaGlobal(Convert.ToDateTime(filtros.FechaCorte), "yyyy-MM-dd" + " 23:59:59") ?? "";

            p.Add("@FechaInicio", vFechaIni);
            p.Add("@FechaCorte", vFechaCorte);

            // paging
            var offset = filtros.pagina < 0 ? 0 : filtros.pagina;
            var fetch = filtros.paginacion <= 0 ? 0 : filtros.paginacion;
            p.Add("@offset", offset);
            p.Add("@fetch", fetch);

            // listas
            if (filtros.Cuentas?.Count > 0)
                p.Add("@Cuentas", filtros.Cuentas.Select(x => x.item).ToList());

            if (filtros.TiposDocumento?.Count > 0)
                p.Add("@TiposDocumento", filtros.TiposDocumento.Select(x => x.item).ToList());

            if (filtros.Conceptos?.Count > 0)
                p.Add("@Conceptos", filtros.Conceptos.Select(x => x.item).ToList());

            // likes (si quedan null, ok)
            AddLikeParam(p, "@Usuario", filtros.Usuario);
            AddLikeParam(p, "@Codigo", filtros.Codigo);
            AddLikeParam(p, "@Beneficiario", filtros.Beneficiario);
            AddLikeParam(p, "@Detalle", filtros.Detalle?.Trim());
            AddLikeParam(p, "@NoDocumento", filtros.NoDocumento);
            AddLikeParam(p, "@IdAplicacion", filtros.IdAplicacion);
            AddLikeParam(p, "@Ref01", filtros.Ref01);
            AddLikeParam(p, "@Ref02", filtros.Ref02);
            AddLikeParam(p, "@Ref03", filtros.Ref03);
            AddLikeParam(p, "@Transferencia", filtros.Transferencia);

            return p;
        }

        private static void AddLikeParam(DynamicParameters p, string name, string? value)
        {
            p.Add(name, string.IsNullOrWhiteSpace(value) ? null : $"%{value}%");
        }

        private static string BuildWhere(FiltrosBusqueda filtros)
        {
            var sb = new StringBuilder();

                            AppendEstado(sb, filtros);
                            AppendProtegido(sb, filtros);
                            AppendUsuarioTipo(sb, filtros);
                            AppendLikeFilters(sb, filtros);
                            AppendInFilters(sb, filtros);
                            AppendFechaFiltro(sb, filtros);

                            // filtro general (si existe)
                            var hasFiltro = !string.IsNullOrWhiteSpace(filtros.filtro);
                            if (hasFiltro)
                            {
                                sb.Append(@"
                 AND (
                        CAST(C.nsolicitud AS NVARCHAR(50)) LIKE @like
                     OR CAST(C.ndocumento AS NVARCHAR(50)) LIKE @like
                     OR C.beneficiario LIKE @like
                 )");
            }

            return sb.ToString();
        }
        private static void AppendEstado(StringBuilder sb, FiltrosBusqueda filtros)
        {
            if (string.IsNullOrEmpty(filtros.Estado)) return;

            sb.Append(filtros.Estado switch
            {
                "E" or "T" => " AND C.estado IN ('E','T') ",
                "A" => " AND C.estado = 'A' ",
                "S" => " AND C.estado = 'P' ",
                "I" => " AND C.estado = 'I' ",
                _ => string.Empty
            });
        }

        private static void AppendProtegido(StringBuilder sb, FiltrosBusqueda filtros)
        {
            if (!filtros.ChkProtegido)
                sb.Append(" AND ISNULL(C.MODO_PROTEGIDO,0) = 0 ");
        }

        private static void AppendUsuarioTipo(StringBuilder sb, FiltrosBusqueda filtros)
        {
            if (string.IsNullOrWhiteSpace(filtros.Usuario) || string.IsNullOrWhiteSpace(filtros.TipoUsuario))
                return;

            var col = filtros.TipoUsuario switch
            {
                "S" => "C.user_solicita",
                "A" => "C.user_autoriza",
                "E" => "C.user_genera",
                "N" => "C.user_anula",
                _ => null
            };

            if (col != null)
                sb.Append($" AND {col} LIKE @Usuario ");
        }

        private static void AppendLikeFilters(StringBuilder sb, FiltrosBusqueda filtros)
        {
            var items = new (string Sql, string? Value)[]
            {
        (" AND C.codigo LIKE @Codigo ", filtros.Codigo),
        (" AND C.beneficiario LIKE @Beneficiario ", filtros.Beneficiario),
        (" AND (ISNULL(C.Detalle1,'') + ISNULL(C.Detalle2,'') + ISNULL(C.Detalle3,'') + ISNULL(C.Detalle4,'') + ISNULL(C.Detalle5,'')) LIKE @Detalle ", filtros.Detalle),
        (" AND CAST(C.ndocumento AS NVARCHAR(50)) LIKE @NoDocumento ", filtros.NoDocumento),
        (" AND ISNULL(C.Cod_App,'') LIKE @IdAplicacion ", filtros.IdAplicacion),
        (" AND ISNULL(C.Ref_01,'') LIKE @Ref01 ", filtros.Ref01),
        (" AND ISNULL(C.Ref_02,'') LIKE @Ref02 ", filtros.Ref02),
        (" AND ISNULL(C.Ref_03,'') LIKE @Ref03 ", filtros.Ref03),
        (" AND C.Documento_Base LIKE @Transferencia ", filtros.Transferencia),
            };

            foreach (var (sql, value) in items)
                if (!string.IsNullOrWhiteSpace(value))
                    sb.Append(sql);
        }

        private static void AppendInFilters(StringBuilder sb, FiltrosBusqueda filtros)
        {
            if (filtros.Cuentas?.Count > 0) sb.Append(" AND C.id_banco IN @Cuentas ");
            if (filtros.TiposDocumento?.Count > 0) sb.Append(" AND C.tipo IN @TiposDocumento ");
            if (filtros.Conceptos?.Count > 0) sb.Append(" AND C.cod_concepto IN @Conceptos ");
        }

        private static void AppendFechaFiltro(StringBuilder sb, FiltrosBusqueda filtros)
        {
            var campo = filtros.TipoFecha switch
            {
                "E" => "C.fecha_emision",
                "A" => "C.fecha_anula",
                "S" => "C.fecha_solicitud",
                "I" => "C.fecha_autorizacion",
                _ => null
            };

            if (campo != null)
                sb.Append($" AND {campo} BETWEEN @FechaInicio AND @FechaCorte ");
        }

        private string BuildOrderBy(FiltrosBusqueda filtros)
        {
            var sortField = (filtros.sortField ?? string.Empty).Trim();

            var field = sortField switch
            {
                "fecha_emision" => vFechaEmision,
                "fecha_anula" => "fecha_anula",
                "nsolicitud" => "nsolicitud",
                "doc_interno" => "doc_interno",
                "beneficiario" => "beneficiario",
                "monto" => "monto",
                "estado" => "estado",
                "banco" => "banco",
                "codigo" => "codigo",
                "unidad" => "unidad",
                "concepto" => "concepto",
                _ => vFechaEmision
            };

            var dir = filtros.sortOrder == 0 ? "DESC" : "ASC";
            return $"ORDER BY {field} {dir}";
        }

        private static string BuildPaging(FiltrosBusqueda filtros)
        {
            //@offset/@fetch; aquí solo decides si aplica
            var fetch = filtros.paginacion;
            return fetch > 0
                ? "\nOFFSET @offset ROWS\nFETCH NEXT @fetch ROWS ONLY"
                : string.Empty;
        }

        private static string BuildSqlList(string where, string orderBy, string paging)
        {
              return $@"
                    SELECT
                        Id,
                        nsolicitud,
                        doc_interno,
                        doc_banco,
                        tipo,
                        monto,
                        estado,
                        fecha_emision,
                        fecha_anula,
                        beneficiario,
                        cta_ahorros,
                        banco,
                        codigo,
                        detalle,
                        ref_banco,
                        unidad,
                        concepto,
                        tipo_cliente,
                        User_Solicita,
                        User_Genera,
                        User_Anula,
                        cod_divisa,
                        Tipo_Cambio,
                        grupo_bancario,
                        Periodo,
                        REF_01,
                        REF_02,
                        REF_03,
                        id_desembolso,
                        REFERENCIA_SINPE,
                        NOMBRE_ORIGEN,
                        USER_AUTORIZA,
                        fecha_autoriza
                    FROM
                    (
                        SELECT
                            0 AS Id,
                            C.nsolicitud,
                            ISNULL(C.ndocumento, 0) AS doc_interno,
                            ISNULL(C.DOCUMENTO_BANCO, '') AS doc_banco,
                            C.tipo,
                            C.monto,
                            CASE
                                WHEN C.estado = 'A' THEN 'Anulado'
                                WHEN C.estado = 'P'
                                     AND C.fecha_autorizacion IS NOT NULL
                                     AND C.fecha_emision IS NULL THEN 'Autorizado'
                                WHEN (C.estado = 'T' OR C.estado = 'I')
                                     AND C.fecha_emision IS NOT NULL THEN 'Emitido'
                                WHEN C.estado = 'P' THEN 'Pendiente'
                            END AS estado,
                            ISNULL(C.fecha_emision, '') AS fecha_emision,
                            ISNULL(C.fecha_anula, '') AS fecha_anula,
                            C.beneficiario,
                            C.cta_ahorros,
                            B.descripcion AS banco,
                            C.codigo,
                            (
                                ISNULL(C.Detalle1, '') + ' ' +
                                ISNULL(C.Detalle2, '') + ' ' +
                                ISNULL(C.Detalle3, '') + ' ' +
                                ISNULL(C.Detalle4, '') + ' ' +
                                ISNULL(C.Detalle5, '')
                            ) AS detalle,
                            ISNULL(C.REFERENCIA_BANCARIA, '') AS ref_banco,
                            U.descripcion AS unidad,
                            Con.descripcion AS concepto,
                            CASE C.Tipo_Beneficiario
                                WHEN 1 THEN 'Personas'
                                WHEN 2 THEN 'Bancos'
                                WHEN 3 THEN 'Proveedores'
                                WHEN 4 THEN 'Acreedores'
                            END AS tipo_cliente,
                            C.User_Solicita,
                            C.User_Genera,
                            C.User_Anula,
                            C.cod_divisa,
                            C.Tipo_Cambio,
                            Grp.Descripcion AS grupo_bancario,
                            CASE MONTH(C.fecha_emision)
                                WHEN 1 THEN CONVERT(varchar(4), YEAR(C.fecha_emision)) + ' - 01 Enero'
                                WHEN 2 THEN CONVERT(varchar(4), YEAR(C.fecha_emision)) + ' - 02 Febrero'
                                WHEN 3 THEN CONVERT(varchar(4), YEAR(C.fecha_emision)) + ' - 03 Marzo'
                                WHEN 4 THEN CONVERT(varchar(4), YEAR(C.fecha_emision)) + ' - 04 Abril'
                                WHEN 5 THEN CONVERT(varchar(4), YEAR(C.fecha_emision)) + ' - 05 Mayo'
                                WHEN 6 THEN CONVERT(varchar(4), YEAR(C.fecha_emision)) + ' - 06 Junio'
                                WHEN 7 THEN CONVERT(varchar(4), YEAR(C.fecha_emision)) + ' - 07 Julio'
                                WHEN 8 THEN CONVERT(varchar(4), YEAR(C.fecha_emision)) + ' - 08 Agosto'
                                WHEN 9 THEN CONVERT(varchar(4), YEAR(C.fecha_emision)) + ' - 09 Septiembre'
                                WHEN 10 THEN CONVERT(varchar(4), YEAR(C.fecha_emision)) + ' - 10 Octubre'
                                WHEN 11 THEN CONVERT(varchar(4), YEAR(C.fecha_emision)) + ' - 11 Noviembre'
                                WHEN 12 THEN CONVERT(varchar(4), YEAR(C.fecha_emision)) + ' - 12 Diciembre'
                                ELSE ''
                            END AS Periodo,
                            C.REF_01,
                            C.REF_02,
                            C.REF_03,
                            C.ID_DESEMBOLSO AS id_desembolso,
                            C.REFERENCIA_SINPE,
                            C.NOMBRE_ORIGEN,
                            C.USER_AUTORIZA,
                            ISNULL(C.FECHA_AUTORIZACION, '') AS fecha_autoriza
                        FROM Tes_Transacciones C
                        INNER JOIN Tes_Bancos B
                            ON C.id_banco = B.id_banco
                        LEFT JOIN tes_bancos_grupos Grp
                            ON B.cod_grupo = Grp.cod_grupo
                        LEFT JOIN CntX_Unidades U
                            ON C.cod_unidad = U.cod_unidad
                           AND U.cod_contabilidad = @CodContabilidad
                        LEFT JOIN Tes_Conceptos Con
                            ON C.cod_concepto = Con.cod_concepto
                        WHERE 1 = 1
                        {where}
                    ) T
                    {orderBy}
                    {paging};";
        }

        private static string BuildSqlTotal(string where)
        {
            return $@"
                SELECT
                    COUNT(C.nsolicitud) AS total,
                    SUM(C.monto) AS montototal
                FROM Tes_Transacciones C
                INNER JOIN Tes_Bancos B
                    ON C.id_banco = B.id_banco
                LEFT JOIN tes_bancos_grupos Grp
                    ON B.cod_grupo = Grp.cod_grupo
                LEFT JOIN CntX_Unidades U
                    ON C.cod_unidad = U.cod_unidad
                   AND U.cod_contabilidad = @CodContabilidad
                LEFT JOIN Tes_Conceptos Con
                    ON C.cod_concepto = Con.cod_concepto
                WHERE 1 = 1
                {where};";
        }

        public ErrorDto<List<Desembolsos>> Desembolsos_Exportar(int codEmpresa, int CodConta, FiltrosBusqueda filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            try
            {
                filtros ??= new FiltrosBusqueda();

                // Reutiliza parámetros base (incluye @CodContabilidad, fechas, likes, IN, etc.)
                var p = CreateBaseParameters(CodConta, filtros);

                // Reutiliza el WHERE armado y seguro (sin concatenar texto del usuario)
                var where = BuildWhere(filtros);

                // Reutiliza ORDER BY con whitelist (evita S2077)
                var orderBy = BuildOrderBy(filtros);

                // Exportación normalmente NO pagina
                var paging = string.Empty;

                // Reutiliza el SQL completo del listado
                var sql = BuildSqlList(where, orderBy, paging);

                var resultado = conn.Query<Desembolsos>(sql, p, commandTimeout: 900).ToList();
                return DbHelper.CreateOkResponse(resultado);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<Desembolsos>>($"Error al exportar desembolsos: {ex.Message}");
            }
        }

        public static string? validaFechaGlobal(DateTime? fecha)
        {
            return MProGrXAuxiliarDB.validaFechaGlobal(fecha, "yyyy-MM-dd HH:mm:ss") ?? string.Empty;
        }
    }
}
