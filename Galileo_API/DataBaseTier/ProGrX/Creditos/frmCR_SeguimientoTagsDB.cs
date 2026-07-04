using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCRSeguimientoTagsDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;

        private const int ModuloCreditos = 8;
        private const string EstadoTodos = "Todos";
        private const string EstadoRecibida = "Recibida";
        private const string EstadoPendiente = "Pendiente";
        private const string EstadoFormalizada = "Formalizada";
        private const string DocumentacionTodos = "Todos";
        private const string DocumentacionRecepcion = "Recepción";
        private const string DocumentacionDevolucion = "Devolución";
        private const string MensajeParametrosInvalidos = "Los parámetros de consulta son inválidos.";
        private const string MensajeUsuarioRequerido = "El usuario es requerido.";
        private const string MensajeOperacionRequerida = "La operación es requerida.";
        private const string MensajeEtiquetaRequerida = "La etiqueta es requerida.";
        private const string MensajeSinOperaciones = "Debe seleccionar al menos una operación.";
        private const string MovimientoBitacora = "REGISTRA-WEB";

        public FrmCRSeguimientoTagsDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Inserta en bitácora un movimiento del módulo de créditos.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDb.Bitacora(data);
        }

        /// <summary>
        /// Obtiene el usuario y nombre completo para la pantalla de aplicación de etiquetas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CrSeguimientoTagsUsuarioDto> CR_SeguimientoTags_Usuario_Obtener(
            int CodEmpresa,
            string usuario)
        {
            usuario = S(usuario).ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return ErrorUsuario(MensajeUsuarioRequerido);
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var result = conn.QueryFirstOrDefault<CrSeguimientoTagsUsuarioDto>(
                    SqlUsuarioObtener,
                    new { usuario });

                if (result == null)
                {
                    return ErrorUsuario("No se encontró información para el usuario indicado.");
                }

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return ErrorUsuario(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene las etiquetas activas disponibles para el usuario según sus grupos asignados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_SeguimientoTags_Etiquetas_Dropdown_Obtener(
            int CodEmpresa,
            string usuario)
        {
            usuario = S(usuario).ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    MensajeUsuarioRequerido,
                    -1,
                    new List<DropDownListaGenericaModel>());
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var result = conn.Query<DropDownListaGenericaModel>(
                    SqlEtiquetasDropdown,
                    new { usuario }).ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    ex.Message,
                    -1,
                    new List<DropDownListaGenericaModel>());
            }
        }

        /// <summary>
        /// Obtiene la información básica de una operación digitada para agregarla a la lista temporal.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<CrSeguimientoTagsOperacionDto> CR_SeguimientoTags_Operacion_Obtener(
            int CodEmpresa,
            long operacion)
        {
            if (operacion <= 0)
            {
                return ErrorOperacion(MensajeOperacionRequerida);
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var result = conn.QueryFirstOrDefault<CrSeguimientoTagsOperacionDto>(
                    SqlOperacionObtener,
                    new { operacion });

                if (result == null)
                {
                    return ErrorOperacion("No se encontró la operación indicada.");
                }

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return ErrorOperacion(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la lista paginada de solicitudes para aplicar etiquetas con filtros, búsqueda global, ordenamiento y paginación desde base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrSeguimientoTagsLista> CR_SeguimientoTags_Lista_Obtener(
            int CodEmpresa,
            string parametros)
        {
            var filtrosResult = ParseFiltros(parametros);
            if (filtrosResult.Code != 0 || filtrosResult.Result == null)
            {
                return ErrorLista(filtrosResult.Description ?? MensajeParametrosInvalidos);
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var query = CrearListaQuery(filtrosResult.Result);
                var total = conn.QuerySingle<int>(query.SqlCount, query.Parameters);
                var lista = conn.Query<CrSeguimientoTagsListaData>(
                    query.SqlPage,
                    query.Parameters).ToList();

                return DbHelper.CreateOkResponse(new CrSeguimientoTagsLista
                {
                    total = total,
                    lista = lista
                });
            }
            catch (SqlException ex)
            {
                return ErrorLista(ex.Message);
            }
        }

        /// <summary>
        /// Exporta la lista de solicitudes para aplicar etiquetas sin paginación, respetando filtros, búsqueda global y ordenamiento.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto<CrSeguimientoTagsLista> CR_SeguimientoTags_Lista_Export(
            int CodEmpresa,
            string parametros)
        {
            return CR_SeguimientoTags_Lista_Obtener(CodEmpresa, ForzarExport(parametros));
        }

        /// <summary>
        /// Aplica una etiqueta de seguimiento a una lista de operaciones seleccionadas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrSeguimientoTagsAplicarResult> CR_SeguimientoTags_Aplicar(
     int CodEmpresa,
     CrSeguimientoTagsAplicarRequest request)
        {
            var validacion = ValidarAplicar(request);
            if (validacion.Code != 0)
            {
                return validacion;
            }

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            using var tx = conn.BeginTransaction();

            try
            {
                var result = AplicarOperaciones(conn, tx, request);
                RegistrarBitacoraAplicar(CodEmpresa, request, result.total_procesadas);

                tx.Commit();

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                RollbackSeguro(tx);
                return ErrorAplicar(MensajeSqlAmigable(ex));
            }
            catch (InvalidOperationException ex)
            {
                RollbackSeguro(tx);
                return ErrorAplicar(ex.Message);
            }
        }

        private static ListaQuery CrearListaQuery(FiltrosLazyLoadData filtros)
        {
            var filtroPantalla = ParseFiltroPantalla(filtros.filtro);
            var parametros = CrearParametrosLista(filtroPantalla);
            var where = CrearWhereLista(filtroPantalla);
            var orderBy = CrearOrderByLista(filtros.sortField, filtros.sortOrder);
            var paginacion = CrearPaginacion(filtros, parametros);

            var sqlCount = $@"
                select count(1)
                from REG_CREDITOS R
                inner join SOCIOS S
                    on S.CEDULA = R.CEDULA
                where {where};";

            var sqlPage = $@"
                select
                    R.ID_SOLICITUD as id_solicitud,
                    R.FECHAFORP as fechaforp,
                    R.FECHASOL as fechasol,
                    rtrim(R.CEDULA) as cedula,
                    rtrim(S.NOMBRE) as nombre,
                    rtrim(R.CODIGO) as codigo,
                    isnull(R.MONTOAPR,0) as montoapr,
                    isnull(R.CUOTA,0) as cuota,
                    isnull(R.PLAZO,0) as plazo,
                    isnull(R.INT,0) as int_tasa,
                    rtrim(isnull(R.ESTADOSOL,'')) as estadosol,
                    case R.ESTADOSOL
                        when 'R' then 'Recibido'
                        when 'P' then 'Pendiente'
                        when 'F' then 'Formalizada'
                        else rtrim(isnull(R.ESTADOSOL,''))
                    end as estado_desc,
                    case isnull(R.ANALISTAS_RECEPCION,'P')
                        when 'R' then 'Recepción'
                        when 'D' then 'Devolución'
                        else ''
                    end as documentacion,
                    isnull(R.ANALISTAS_REVISION,0) as analistas_revision,
                    R.EN_ESPERA_FECHA as en_espera_fecha,
                    cast(0 as bit) as seleccionado
                from REG_CREDITOS R
                inner join SOCIOS S
                    on S.CEDULA = R.CEDULA
                where {where}
                {orderBy}
                {paginacion};";

            return new ListaQuery(sqlCount, sqlPage, parametros);
        }

        private static DynamicParameters CrearParametrosLista(
     CrSeguimientoTagsListaFiltroDto filtroPantalla)
        {
            var parametros = new DynamicParameters();

            var texto = S(filtroPantalla.texto);
            var like = $"%{texto}%";

            parametros.Add("@fechaInicio", filtroPantalla.fecha_inicio?.Date);
            parametros.Add("@fechaFin", filtroPantalla.fecha_fin?.Date.AddDays(1).AddTicks(-1));
            parametros.Add("@estado", NormalizarEstado(filtroPantalla.estado));
            parametros.Add("@documentacion", NormalizarDocumentacion(filtroPantalla.documentacion));
            parametros.Add("@soloRevisados", filtroPantalla.solo_revisados ? 1 : 0);
            parametros.Add("@soloEspera", filtroPantalla.solo_espera ? 1 : 0);
            parametros.Add("@texto", string.IsNullOrWhiteSpace(texto) ? null : texto);
            parametros.Add("@like", like);

            return parametros;
        }

        private static string CrearWhereLista(CrSeguimientoTagsListaFiltroDto filtro)
        {
            var where = new StringBuilder("1 = 1");

            if (filtro.fecha_inicio.HasValue)
            {
                where.AppendLine(" and R.FECHAFORP >= @fechaInicio");
            }

            if (filtro.fecha_fin.HasValue)
            {
                where.AppendLine(" and R.FECHAFORP <= @fechaFin");
            }

            where.AppendLine(@"
                and (
                       @estado = 'Todos'
                    or (@estado = 'Recibida' and R.ESTADOSOL = 'R')
                    or (@estado = 'Pendiente' and R.ESTADOSOL = 'P')
                    or (@estado = 'Formalizada' and R.ESTADOSOL = 'F')
                )
                and (
                       @estado <> 'Todos'
                    or R.ESTADOSOL in ('P','R','F')
                )
                and (
                       @documentacion = 'Todos'
                    or (@documentacion = 'Recepción' and isnull(R.ANALISTAS_RECEPCION,'P') = 'R')
                    or (@documentacion = 'Devolución' and isnull(R.ANALISTAS_RECEPCION,'P') = 'D')
                )
                and (
                       @soloRevisados = 0
                    or isnull(R.ANALISTAS_REVISION,0) = 1
                )
                and (
                       @soloEspera = 0
                    or R.EN_ESPERA_FECHA is not null
                )
                and (
                       @texto is null
                    or cast(R.ID_SOLICITUD as varchar(30)) like @like
                    or R.CEDULA like @like
                    or S.NOMBRE like @like
                    or R.CODIGO like @like
                )");

            return where.ToString();
        }

        private static string CrearOrderByLista(string? sortField, int sortOrder)
        {
            var field = S(sortField).ToLowerInvariant();
            var direction = sortOrder == 1 ? "asc" : "desc";

            var column = field switch
            {
                "id_solicitud" => "R.ID_SOLICITUD",
                "fechaforp" => "R.FECHAFORP",
                "fechasol" => "R.FECHASOL",
                "cedula" => "R.CEDULA",
                "nombre" => "S.NOMBRE",
                "codigo" => "R.CODIGO",
                "montoapr" => "R.MONTOAPR",
                "cuota" => "R.CUOTA",
                "plazo" => "R.PLAZO",
                "int_tasa" => "R.INT",
                "estado_desc" => "R.ESTADOSOL",
                "documentacion" => "R.ANALISTAS_RECEPCION",
                _ => "R.ID_SOLICITUD"
            };

            if (column == "R.ID_SOLICITUD")
            {
                return $" order by R.ID_SOLICITUD {direction}";
            }

            return $" order by {column} {direction}, R.ID_SOLICITUD desc";
        }

        private static string CrearPaginacion(FiltrosLazyLoadData filtros, DynamicParameters parametros)
        {
            var fetch = filtros.paginacion < 0 ? 0 : filtros.paginacion;
            if (fetch <= 0)
            {
                return string.Empty;
            }

            var pagina = filtros.pagina < 0 ? 0 : filtros.pagina;
            var offset = pagina * fetch;

            parametros.Add("@offset", offset);
            parametros.Add("@fetch", fetch);

            return " offset @offset rows fetch next @fetch rows only";
        }

        private static ErrorDto<FiltrosLazyLoadData> ParseFiltros(string parametros)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(parametros))
                {
                    return DbHelper.CreateOkResponse(new FiltrosLazyLoadData());
                }

                var filtros = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(parametros)
                              ?? new FiltrosLazyLoadData();

                return DbHelper.CreateOkResponse(filtros);
            }
            catch (JsonException ex)
            {
                return DbHelper.CreateErrorResponse<FiltrosLazyLoadData>(
                    ex.Message,
                    -1,
                    new FiltrosLazyLoadData());
            }
        }

        private static CrSeguimientoTagsListaFiltroDto ParseFiltroPantalla(string? filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return new CrSeguimientoTagsListaFiltroDto();
            }

            try
            {
                var parsed = JsonConvert.DeserializeObject<CrSeguimientoTagsListaFiltroDto>(filtro);
                return parsed ?? new CrSeguimientoTagsListaFiltroDto();
            }
            catch (JsonException)
            {
                return new CrSeguimientoTagsListaFiltroDto
                {
                    texto = filtro.Trim()
                };
            }
        }

        private static string ForzarExport(string parametros)
        {
            var filtros = ParseFiltros(parametros).Result ?? new FiltrosLazyLoadData();
            filtros.pagina = 0;
            filtros.paginacion = 0;
            return JsonConvert.SerializeObject(filtros);
        }

        private static ErrorDto<CrSeguimientoTagsAplicarResult> ValidarAplicar(
            CrSeguimientoTagsAplicarRequest request)
        {
            if (request == null)
            {
                return ErrorAplicar("La solicitud de aplicación de etiquetas es requerida.");
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return ErrorAplicar(MensajeUsuarioRequerido);
            }

            if (string.IsNullOrWhiteSpace(request.tag_codigo))
            {
                return ErrorAplicar(MensajeEtiquetaRequerida);
            }

            if (request.operaciones == null || request.operaciones.Count == 0)
            {
                return ErrorAplicar(MensajeSinOperaciones);
            }

            if (request.operaciones.All(x => x.id_solicitud <= 0))
            {
                return ErrorAplicar("No se recibieron operaciones válidas para procesar.");
            }

            return DbHelper.CreateOkResponse(new CrSeguimientoTagsAplicarResult());
        }

        private static CrSeguimientoTagsAplicarResult AplicarOperaciones(
     SqlConnection conn,
     SqlTransaction tx,
     CrSeguimientoTagsAplicarRequest request)
        {
            var result = new CrSeguimientoTagsAplicarResult();
            var tag = S(request.tag_codigo).ToUpperInvariant();
            var usuario = S(request.usuario).ToUpperInvariant();
            var notas = S(request.observacion);

            var operaciones = request.operaciones
                .Where(x => x.id_solicitud > 0)
                .GroupBy(x => x.id_solicitud)
                .Select(x => x.First())
                .ToList();

            foreach (var item in operaciones)
            {
                var codigo = S(item.codigo).ToUpperInvariant();

                if (ExisteEtiquetaOperacion(conn, tx, item.id_solicitud, codigo, tag))
                {
                    result.errores.Add(new CrSeguimientoTagsAplicarErrorDto
                    {
                        id_solicitud = item.id_solicitud,
                        error = "La operación ya tiene asignada esa etiqueta."
                    });
                    continue;
                }

                try
                {
                    InsertarOperacionTag(
                        conn,
                        tx,
                        new OperacionTagInsertDto
                        {
                            linea = ObtenerSiguienteLineaTag(conn, tx, item.id_solicitud, codigo),
                            codigo = codigo,
                            idSolicitud = item.id_solicitud,
                            tag = tag,
                            usuario = usuario,
                            notas = notas
                        });

                    result.total_procesadas++;
                }
                catch (SqlException ex)
                {
                    result.errores.Add(new CrSeguimientoTagsAplicarErrorDto
                    {
                        id_solicitud = item.id_solicitud,
                        error = MensajeSqlAmigable(ex)
                    });
                }
            }

            result.total_errores = result.errores.Count;
            return result;
        }

        private static void InsertarOperacionTag(SqlConnection conn,SqlTransaction tx,OperacionTagInsertDto data)
        {
            conn.Execute(
                SqlOperacionTagInsertar,
                data,
                tx);
        }

        private void RegistrarBitacoraAplicar(
            int CodEmpresa,
            CrSeguimientoTagsAplicarRequest request,
            int totalProcesadas)
        {
            if (totalProcesadas <= 0)
            {
                return;
            }

            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = S(request.usuario).ToUpperInvariant(),
                Modulo = ModuloCreditos,
                Movimiento = MovimientoBitacora,
                DetalleMovimiento =
                    $"Aplicación de etiqueta {S(request.tag_codigo)} a {totalProcesadas} operación(es)."
            });
        }

        private static string NormalizarEstado(string? estado)
        {
            var value = S(estado);

            return value switch
            {
                EstadoRecibida => EstadoRecibida,
                EstadoPendiente => EstadoPendiente,
                EstadoFormalizada => EstadoFormalizada,
                _ => EstadoTodos
            };
        }

        private static string NormalizarDocumentacion(string? documentacion)
        {
            var value = S(documentacion);

            return value switch
            {
                DocumentacionRecepcion => DocumentacionRecepcion,
                DocumentacionDevolucion => DocumentacionDevolucion,
                _ => DocumentacionTodos
            };
        }

        private static ErrorDto<CrSeguimientoTagsUsuarioDto> ErrorUsuario(string mensaje)
        {
            return DbHelper.CreateErrorResponse<CrSeguimientoTagsUsuarioDto>(
                mensaje,
                -1,
                new CrSeguimientoTagsUsuarioDto());
        }

        private static ErrorDto<CrSeguimientoTagsOperacionDto> ErrorOperacion(string mensaje)
        {
            return DbHelper.CreateErrorResponse<CrSeguimientoTagsOperacionDto>(
                mensaje,
                -1,
                new CrSeguimientoTagsOperacionDto());
        }

        private static ErrorDto<CrSeguimientoTagsLista> ErrorLista(string mensaje)
        {
            return DbHelper.CreateErrorResponse<CrSeguimientoTagsLista>(
                mensaje,
                -1,
                new CrSeguimientoTagsLista());
        }

        private static ErrorDto<CrSeguimientoTagsAplicarResult> ErrorAplicar(string mensaje)
        {
            return DbHelper.CreateErrorResponse<CrSeguimientoTagsAplicarResult>(
                mensaje,
                -1,
                new CrSeguimientoTagsAplicarResult());
        }

        private static string S(object? value)
        {
            return Convert.ToString(value)?.Trim() ?? string.Empty;
        }

        private sealed record ListaQuery(
            string SqlCount,
            string SqlPage,
            DynamicParameters Parameters);

        private const string SqlUsuarioObtener = @"
            select
                rtrim(USUARIO) as usuario,
                rtrim(isnull(DESCRIPCION,'')) as nombre
            from USUARIOS
            where USUARIO = @usuario;";

        private const string SqlEtiquetasDropdown = @"
            select distinct
                rtrim(T.TAG_CODIGO) as item,
                rtrim(T.TAG_CODIGO) + ' - ' + rtrim(T.DESCRIPCION) as descripcion
            from CRD_TAGS T
            inner join CRD_TAGS_GRUPOS TG
                on TG.TAG_CODIGO = T.TAG_CODIGO
            inner join CRD_GRPUSERS G
                on G.COD_GRUPO = TG.COD_GRUPO
            where G.USUARIO = @usuario
              and isnull(T.ACTIVO,0) = 1
            order by rtrim(T.TAG_CODIGO);";

        private const string SqlOperacionObtener = @"
            select
                R.ID_SOLICITUD as id_solicitud,
                rtrim(R.CODIGO) as codigo,
                rtrim(R.CEDULA) as cedula,
                rtrim(isnull(S.NOMBRE,'')) as nombre,
                rtrim(isnull(O.DESCRIPCION,'')) as oficina,
                cast(0 as bit) as seleccionado
            from REG_CREDITOS R
            inner join SOCIOS S
                on S.CEDULA = R.CEDULA
            left join SIF_OFICINAS O
                on O.COD_OFICINA = R.COD_OFICINA_R
            where R.ID_SOLICITUD = @operacion;";

        private const string SqlOperacionTagInsertar = @"
            insert into CRD_OPERACION_TAGS
            (
                LINEA,
                CODIGO,
                ID_SOLICITUD,
                TAG_CODIGO,
                ASIGNADO_A,
                REGISTRO_FECHA,
                REGISTRO_USUARIO,
                NOTAS
            )
            values
            (
                @linea,
                @codigo,
                @idSolicitud,
                @tag,
                null,
                dbo.MyGetdate(),
                @usuario,
                @notas
            );";
        private static bool ExisteEtiquetaOperacion(
    SqlConnection conn,
    SqlTransaction tx,
    long idSolicitud,
    string codigo,
    string tag)
        {
            const string sql = @"
        select count(1)
        from CRD_OPERACION_TAGS
        where ID_SOLICITUD = @idSolicitud
          and CODIGO = @codigo
          and TAG_CODIGO = @tag;";

            return conn.QuerySingle<int>(
                sql,
                new { idSolicitud, codigo, tag },
                tx) > 0;
        }

        private static int ObtenerSiguienteLineaTag(
            SqlConnection conn,
            SqlTransaction tx,
            long idSolicitud,
            string codigo)
        {
            const string sql = @"
        select isnull(max(LINEA),0) + 1
        from CRD_OPERACION_TAGS with (UPDLOCK, HOLDLOCK)
        where ID_SOLICITUD = @idSolicitud
          and CODIGO = @codigo;";

            return conn.QuerySingle<int>(
                sql,
                new { idSolicitud, codigo },
                tx);
        }

        private static string MensajeSqlAmigable(SqlException ex)
        {
            return ex.Number switch
            {
                2601 or 2627 => "La etiqueta ya existe para una de las operaciones seleccionadas.",
                515 => "No se pudo aplicar la etiqueta porque falta información requerida de la operación.",
                547 => "No se pudo aplicar la etiqueta porque la información relacionada no existe o no es válida.",
                _ => "No se pudo aplicar la etiqueta. Verifique las operaciones seleccionadas e intente nuevamente."
            };
        }

        private static void RollbackSeguro(SqlTransaction tx)
        {
            if (tx.Connection != null)
            {
                tx.Rollback();
            }
        }
        private sealed class OperacionTagInsertDto
        {
            public int linea { get; set; }
            public string codigo { get; set; } = string.Empty;
            public long idSolicitud { get; set; }
            public string tag { get; set; } = string.Empty;
            public string usuario { get; set; } = string.Empty;
            public string notas { get; set; } = string.Empty;
        }
    }
}