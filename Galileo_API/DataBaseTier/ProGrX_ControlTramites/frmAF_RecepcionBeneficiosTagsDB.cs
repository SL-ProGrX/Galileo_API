using System.Data;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_ControlTramites;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_ControlTramites
{
    public sealed class FrmAfRecepcionBeneficiosTagsDb
    {
        private const string Modulo = "BEN";
        private const string MovimientoRecepcion = "RECEPCION";
        private const string MovimientoDevolucion = "DEVOLUCION";
        private const string UsuariosActivosSql = """
            select
                upper(rtrim(nombre)) as item,
                upper(rtrim(nombre)) as descripcion
            from Usuarios
            where estado = 'A'
            order by nombre;
            """;
        private readonly PortalDB _portalDb;

        /// <summary>
        /// Inicializa el acceso a datos del formulario.
        /// </summary>
        /// <param name="config">Configuracion de conexiones.</param>
        public FrmAfRecepcionBeneficiosTagsDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene etiquetas, beneficios activos y usuarios activos.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <returns>Datos requeridos para inicializar el formulario.</returns>
        public ErrorDto<AfRecepcionBeneficiosTagsInicializarResponse>
            AF_frmAF_RecepcionBeneficiosTags_Inicializar(int codEmpresa)
        {
            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                var tags = AF_frmAF_RecepcionBeneficiosTags_Tags_Obtener(
                    connection,
                    null);
                var beneficios = connection.Query<DropDownListaGenericaModel>(
                    """
                    select
                        rtrim(cod_beneficio) as item,
                        concat(
                            rtrim(cod_beneficio),
                            ' - ',
                            rtrim(descripcion)
                        ) as descripcion
                    from AFI_Beneficios
                    where estado = 'A'
                    order by cod_beneficio;
                    """).ToList();
                var usuarios = connection
                    .Query<DropDownListaGenericaModel>(UsuariosActivosSql)
                    .AsList();

                return DbHelper.CreateOkResponse(
                    new AfRecepcionBeneficiosTagsInicializarResponse
                    {
                        tag_recepcion = tags.TagRecepcion,
                        tag_devolucion = tags.TagDevolucion,
                        tag_recepcion_devolucion =
                            tags.TagRecepcionDevolucion,
                        beneficios = beneficios,
                        usuarios = usuarios
                    });
            }
            catch (Exception ex)
                when (ex is SqlException or InvalidOperationException)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new AfRecepcionBeneficiosTagsInicializarResponse());
            }
        }

        /// <summary>
        /// Valida y obtiene el beneficio indicado por codigo y consecutivo.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="codBeneficio">Codigo del beneficio.</param>
        /// <param name="consec">Consecutivo del otorgamiento.</param>
        /// <param name="movimiento">Recepcion o devolucion.</param>
        /// <returns>Beneficio encontrado.</returns>
        public ErrorDto<AfRecepcionBeneficiosTagsBeneficioResponse?>
            AF_frmAF_RecepcionBeneficiosTags_Beneficio_Obtener(
                int codEmpresa,
                string? codBeneficio,
                long consec,
                string? movimiento)
        {
            string beneficio = codBeneficio?.Trim() ?? string.Empty;
            string tipo =
                AF_frmAF_RecepcionBeneficiosTags_Movimiento_Normalizar(
                    movimiento);
            if (AF_frmAF_RecepcionBeneficiosTags_Beneficio_Validar(
                    beneficio,
                    consec,
                    tipo) is { } error)
            {
                return error;
            }

            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();
                var tags = AF_frmAF_RecepcionBeneficiosTags_Tags_Obtener(
                    connection,
                    null);
                string? validacion =
                    AF_frmAF_RecepcionBeneficiosTags_Tag_Validar(
                        connection,
                        null,
                        beneficio,
                        consec,
                        tipo,
                        tags);
                if (validacion is not null)
                {
                    return DbHelper.CreateErrorResponse<
                        AfRecepcionBeneficiosTagsBeneficioResponse?>(
                            validacion,
                            -2,
                            null);
                }

                var item =
                    AF_frmAF_RecepcionBeneficiosTags_Beneficio_Consultar(
                        connection,
                        null,
                        beneficio,
                        consec);
                if (item is null)
                {
                    return DbHelper.CreateErrorResponse<
                        AfRecepcionBeneficiosTagsBeneficioResponse?>(
                            "No se encontro el beneficio indicado.",
                            -2,
                            null);
                }

                return DbHelper.CreateOkResponse<
                    AfRecepcionBeneficiosTagsBeneficioResponse?>(item);
            }
            catch (Exception ex)
                when (ex is SqlException or InvalidOperationException)
            {
                return DbHelper.CreateErrorResponse<
                    AfRecepcionBeneficiosTagsBeneficioResponse?>(
                        ex.Message,
                        -1,
                        null);
            }
        }

        /// <summary>
        /// Obtiene los beneficios pendientes del movimiento indicado.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="movimiento">Recepcion o devolucion.</param>
        /// <returns>Beneficios pendientes.</returns>
        public ErrorDto<List<AfRecepcionBeneficiosTagsPendienteResponse>>
            AF_frmAF_RecepcionBeneficiosTags_Pendientes_Obtener(
                int codEmpresa,
                string? movimiento)
        {
            string tipo =
                AF_frmAF_RecepcionBeneficiosTags_Movimiento_Normalizar(
                    movimiento);
            if (string.IsNullOrWhiteSpace(tipo))
            {
                return DbHelper.CreateErrorResponse(
                    "El tipo de movimiento no es valido.",
                    -2,
                    new List<AfRecepcionBeneficiosTagsPendienteResponse>());
            }

            const string sql = """
                select
                    B.consec,
                    rtrim(B.cod_beneficio) as cod_beneficio,
                    rtrim(B.cedula) as cedula,
                    rtrim(S.nombre) as nombre,
                    isnull(rtrim(O.descripcion), '') as descripcion,
                    B.registra_fecha,
                    isnull(rtrim(B.registra_user), '') as registra_user
                from AFI_Bene_Otorga B
                inner join Socios S on B.cedula = S.cedula
                left join SIF_Oficinas O
                    on B.cod_oficina = O.cod_oficina
                where isnull(B.analista_recepcion, 0) =
                    case when @Movimiento = 'RECEPCION' then 0 else 2 end
                order by B.registra_fecha desc, B.consec desc;
                """;

            return DbHelper.ExecuteListQuery<
                AfRecepcionBeneficiosTagsPendienteResponse>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new { Movimiento = tipo });
        }

        /// <summary>
        /// Registra las etiquetas de los beneficios seleccionados.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="request">Movimiento y beneficios seleccionados.</param>
        /// <returns>Cantidad de registros aplicados.</returns>
        public ErrorDto<AfRecepcionBeneficiosTagsAplicarResponse>
            AF_frmAF_RecepcionBeneficiosTags_Movimiento_Aplicar(
                int codEmpresa,
                AfRecepcionBeneficiosTagsAplicarRequest? request)
        {
            if (AF_frmAF_RecepcionBeneficiosTags_Aplicar_Validar(request)
                is { } error)
            {
                return error;
            }

            string movimiento =
                AF_frmAF_RecepcionBeneficiosTags_Movimiento_Normalizar(
                    request.movimiento);

            try
            {
                using SqlConnection connection =
                    DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();
                using SqlTransaction transaction =
                    connection.BeginTransaction();

                try
                {
                    var tags =
                        AF_frmAF_RecepcionBeneficiosTags_Tags_Obtener(
                            connection,
                            transaction);
                    string tag = movimiento == MovimientoRecepcion
                        ? tags.TagRecepcion
                        : tags.TagDevolucion;
                    string observacion = movimiento == MovimientoRecepcion
                        ? "Recibida la documentacion del Beneficio"
                        : "Devolucion de la documentacion del Beneficio";
                    int aplicados = 0;

                    foreach (var item in request.beneficios
                        .GroupBy(x => new
                        {
                            Codigo = x.cod_beneficio.Trim(),
                            x.consec
                        })
                        .Select(group => group.First()))
                    {
                        string beneficio = item.cod_beneficio.Trim();
                        string? validacion =
                            AF_frmAF_RecepcionBeneficiosTags_Tag_Validar(
                                connection,
                                transaction,
                                beneficio,
                                item.consec,
                                movimiento,
                                tags);
                        if (validacion is not null)
                        {
                            throw new InvalidOperationException(validacion);
                        }

                        var registro =
                            AF_frmAF_RecepcionBeneficiosTags_Beneficio_Consultar(
                                connection,
                                transaction,
                                beneficio,
                                item.consec);
                        if (registro is null)
                        {
                            throw new InvalidOperationException(
                                $"El beneficio {beneficio}-{item.consec} ya no existe.");
                        }

                        connection.Execute(
                            "spSIFRegistraTags",
                            new
                            {
                                Codigo = registro.cod_beneficio,
                                Tag = tag,
                                Usuario = request.usuario.Trim(),
                                Observacion = observacion,
                                Documento = registro.consec.ToString(),
                                Modulo,
                                Llave_01 = registro.cod_beneficio,
                                Llave_02 = registro.consec.ToString(),
                                Llave_03 = registro.cedula
                            },
                            transaction,
                            commandType: CommandType.StoredProcedure);
                        aplicados++;
                    }

                    var resultado = new AfRecepcionBeneficiosTagsAplicarResponse
                    {
                        registros_aplicados = aplicados
                    };
                    transaction.Commit();
                    return DbHelper.CreateOkResponse(
                        resultado,
                        "Proceso concluido con exito.");
                }
                catch
                {
                    if (transaction.Connection is not null)
                    {
                        transaction.Rollback();
                    }
                    throw;
                }
            }
            catch (Exception ex)
                when (ex is SqlException or InvalidOperationException)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new AfRecepcionBeneficiosTagsAplicarResponse());
            }
        }

        /// <summary>
        /// Obtiene el historial de etiquetas de beneficios.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="request">Filtros del historial.</param>
        /// <returns>Movimientos de etiquetas encontrados.</returns>
        public ErrorDto<List<AfRecepcionBeneficiosTagsHistorialResponse>>
            AF_frmAF_RecepcionBeneficiosTags_Historial_Obtener(
                int codEmpresa,
                AfRecepcionBeneficiosTagsHistorialRequest? request)
        {
            request ??= new AfRecepcionBeneficiosTagsHistorialRequest();
            string beneficio = request.cod_beneficio?.Trim() ?? string.Empty;
            string usuario = request.usuario?.Trim() ?? string.Empty;

            if (request.consec.HasValue && request.consec <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "El codigo no es valido.",
                    -2,
                    new List<AfRecepcionBeneficiosTagsHistorialResponse>());
            }

            if (request.fecha_inicio?.Date > request.fecha_fin?.Date)
            {
                return DbHelper.CreateErrorResponse(
                    "La fecha inicial no puede ser mayor que la fecha final.",
                    -2,
                    new List<AfRecepcionBeneficiosTagsHistorialResponse>());
            }

            const string sql = """
                select
                    rtrim(T.descripcion) as descripcion,
                    isnull(rtrim(CT.notas), '') as notas,
                    CT.registro_fecha,
                    isnull(rtrim(CT.registro_usuario), '')
                        as registro_usuario,
                    isnull(rtrim(CT.documento), '') as documento
                from SIF_Control_Tags CT
                inner join SIF_Tags T
                    on CT.tag_codigo = T.tag_codigo
                where CT.cod_modulo = @Modulo
                  and (nullif(@CodBeneficio, '') is null
                       or CT.codigo = @CodBeneficio)
                  and (@Consec is null
                       or CT.documento = convert(varchar(30), @Consec))
                  and (nullif(@Usuario, '') is null
                       or CT.registro_usuario like '%' + @Usuario + '%')
                  and (@FechaInicio is null
                       or CT.registro_fecha >= @FechaInicio)
                  and (@FechaFin is null
                       or CT.registro_fecha < dateadd(day, 1, @FechaFin))
                order by CT.registro_fecha desc;
                """;

            return DbHelper.ExecuteListQuery<
                AfRecepcionBeneficiosTagsHistorialResponse>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        Modulo,
                        CodBeneficio = beneficio,
                        request.consec,
                        Usuario = usuario,
                        FechaInicio = request.fecha_inicio?.Date,
                        FechaFin = request.fecha_fin?.Date
                    });
        }

        /// <summary>
        /// Consulta un beneficio por codigo y consecutivo.
        /// </summary>
        /// <param name="connection">Conexion SQL.</param>
        /// <param name="transaction">Transaccion SQL opcional.</param>
        /// <param name="codBeneficio">Codigo del beneficio.</param>
        /// <param name="consec">Consecutivo del otorgamiento.</param>
        /// <returns>Beneficio encontrado.</returns>
        private static AfRecepcionBeneficiosTagsBeneficioResponse?
            AF_frmAF_RecepcionBeneficiosTags_Beneficio_Consultar(
                SqlConnection connection,
                SqlTransaction? transaction,
                string codBeneficio,
                long consec)
        {
            const string sql = """
                select top 1
                    rtrim(B.cedula) as cedula,
                    rtrim(S.nombre) as nombre,
                    isnull(rtrim(O.descripcion), '') as descripcion,
                    B.consec,
                    rtrim(B.cod_beneficio) as cod_beneficio
                from AFI_Bene_Otorga B
                inner join Socios S on B.cedula = S.cedula
                left join SIF_Oficinas O
                    on B.cod_oficina = O.cod_oficina
                where B.consec = @Consec
                  and B.cod_beneficio = @CodBeneficio;
                """;

            return connection.QueryFirstOrDefault<
                AfRecepcionBeneficiosTagsBeneficioResponse>(
                    sql,
                    new { Consec = consec, CodBeneficio = codBeneficio },
                    transaction);
        }

        /// <summary>
        /// Valida la secuencia de etiquetas usada por el formulario VB6.
        /// </summary>
        /// <param name="connection">Conexion SQL.</param>
        /// <param name="transaction">Transaccion SQL opcional.</param>
        /// <param name="codBeneficio">Codigo del beneficio.</param>
        /// <param name="consec">Consecutivo del otorgamiento.</param>
        /// <param name="movimiento">Tipo de movimiento.</param>
        /// <param name="tags">Configuracion de etiquetas.</param>
        /// <returns>Mensaje de validacion o null.</returns>
        private static string?
            AF_frmAF_RecepcionBeneficiosTags_Tag_Validar(
                SqlConnection connection,
                SqlTransaction? transaction,
                string codBeneficio,
                long consec,
                string movimiento,
                TagsConfiguracion tags)
        {
            int resultado = connection.ExecuteScalar<int>(
                """
                select dbo.fxSIFValidaTagRev(
                    @CodBeneficio,
                    @TagRecepcion,
                    @TagDevolucion,
                    @Modulo,
                    @Documento,
                    null
                );
                """,
                new
                {
                    CodBeneficio = codBeneficio,
                    TagRecepcion = tags.TagRecepcion,
                    TagDevolucion = tags.TagDevolucion,
                    Modulo,
                    Documento = consec.ToString()
                },
                transaction);

            if (movimiento == MovimientoRecepcion && resultado == 2)
            {
                return $"No es posible registrar dos recepciones consecutivas para el beneficio {codBeneficio}-{consec}.";
            }

            if (movimiento == MovimientoDevolucion && resultado == 3)
            {
                return $"No es posible registrar dos devoluciones consecutivas para el beneficio {codBeneficio}-{consec}.";
            }

            int resultadoRecepcionDevolucion = connection.ExecuteScalar<int>(
                """
                select dbo.fxSIFValidaTagRev(
                    @CodBeneficio,
                    @TagRecepcion,
                    @TagDevolucion,
                    @Modulo,
                    @Documento,
                    @TagRecepcionDevolucion
                );
                """,
                new
                {
                    CodBeneficio = codBeneficio,
                    TagRecepcion = tags.TagRecepcion,
                    TagDevolucion = tags.TagDevolucion,
                    Modulo,
                    Documento = consec.ToString(),
                    TagRecepcionDevolucion =
                        tags.TagRecepcionDevolucion
                },
                transaction);

            return resultadoRecepcionDevolucion == 4
                ? $"No es posible registrar una recepcion sin aplicar la devolucion para el beneficio {codBeneficio}-{consec}."
                : null;
        }

        /// <summary>
        /// Obtiene y valida los parametros 10, 11 y 12 de etiquetas.
        /// </summary>
        /// <param name="connection">Conexion SQL.</param>
        /// <param name="transaction">Transaccion SQL opcional.</param>
        /// <returns>Configuracion de etiquetas.</returns>
        private static TagsConfiguracion
            AF_frmAF_RecepcionBeneficiosTags_Tags_Obtener(
                SqlConnection connection,
                SqlTransaction? transaction)
        {
            var tags = connection.QuerySingle<TagsConfiguracion>(
                """
                with Configuracion as (
                    select
                        isnull(max(case
                            when cod_parametro = '10' then rtrim(valor)
                        end), '') as TagRecepcion,
                        isnull(max(case
                            when cod_parametro = '11' then rtrim(valor)
                        end), '') as TagDevolucion,
                        isnull(max(case
                            when cod_parametro = '12' then rtrim(valor)
                        end), '') as TagRecepcionDevolucion
                    from SIF_Parametros
                    where cod_parametro in ('10', '11', '12')
                )
                select
                    C.TagRecepcion,
                    C.TagDevolucion,
                    C.TagRecepcionDevolucion,
                    case when exists (
                        select 1 from SIF_Tags
                        where tag_codigo = C.TagRecepcion
                    ) then 1 else 0 end
                    + case when exists (
                        select 1 from SIF_Tags
                        where tag_codigo = C.TagDevolucion
                    ) then 1 else 0 end
                    + case when exists (
                        select 1 from SIF_Tags
                        where tag_codigo = C.TagRecepcionDevolucion
                    ) then 1 else 0 end as EtiquetasExistentes
                from Configuracion C;
                """,
                transaction: transaction);

            string[] codigosConfigurados =
            [
                tags.TagRecepcion,
                tags.TagDevolucion,
                tags.TagRecepcionDevolucion
            ];
            if (codigosConfigurados.Any(string.IsNullOrWhiteSpace))
            {
                throw new InvalidOperationException(
                    "Falta configurar uno de los parametros de etiquetas 10, 11 o 12.");
            }

            if (tags.EtiquetasExistentes != 3)
            {
                throw new InvalidOperationException(
                    "Uno o mas codigos de etiqueta configurados no existen.");
            }

            return tags;
        }

        /// <summary>
        /// Valida los datos requeridos para consultar un beneficio.
        /// </summary>
        /// <param name="codBeneficio">Codigo del beneficio.</param>
        /// <param name="consec">Consecutivo del otorgamiento.</param>
        /// <param name="movimiento">Tipo de movimiento normalizado.</param>
        /// <returns>Respuesta de error o null cuando los datos son validos.</returns>
        private static ErrorDto<
            AfRecepcionBeneficiosTagsBeneficioResponse?>?
            AF_frmAF_RecepcionBeneficiosTags_Beneficio_Validar(
                string codBeneficio,
                long consec,
                string movimiento)
        {
            if (string.IsNullOrWhiteSpace(codBeneficio) || consec <= 0)
            {
                return DbHelper.CreateErrorResponse<
                    AfRecepcionBeneficiosTagsBeneficioResponse?>(
                        "Debe indicar un beneficio y codigo validos.",
                        -2,
                        null);
            }

            if (string.IsNullOrWhiteSpace(movimiento))
            {
                return DbHelper.CreateErrorResponse<
                    AfRecepcionBeneficiosTagsBeneficioResponse?>(
                        "El tipo de movimiento no es valido.",
                        -2,
                        null);
            }

            return null;
        }

        /// <summary>
        /// Valida la solicitud para aplicar etiquetas a beneficios.
        /// </summary>
        /// <param name="request">Movimiento, usuario y beneficios seleccionados.</param>
        /// <returns>Respuesta de error o null cuando la solicitud es valida.</returns>
        private static ErrorDto<AfRecepcionBeneficiosTagsAplicarResponse>?
            AF_frmAF_RecepcionBeneficiosTags_Aplicar_Validar(
                AfRecepcionBeneficiosTagsAplicarRequest? request)
        {
            if (request is null ||
                string.IsNullOrWhiteSpace(request.usuario) ||
                request.beneficios is null ||
                request.beneficios.Count == 0)
            {
                return DbHelper.CreateErrorResponse(
                    "El usuario y los beneficios seleccionados son requeridos.",
                    -2,
                    new AfRecepcionBeneficiosTagsAplicarResponse());
            }

            if (string.IsNullOrWhiteSpace(
                    AF_frmAF_RecepcionBeneficiosTags_Movimiento_Normalizar(
                        request.movimiento)))
            {
                return DbHelper.CreateErrorResponse(
                    "El tipo de movimiento no es valido.",
                    -2,
                    new AfRecepcionBeneficiosTagsAplicarResponse());
            }

            if (request.beneficios.Any(
                    item => string.IsNullOrWhiteSpace(item.cod_beneficio) ||
                        item.consec <= 0))
            {
                return DbHelper.CreateErrorResponse(
                    "La lista contiene beneficios o codigos no validos.",
                    -2,
                    new AfRecepcionBeneficiosTagsAplicarResponse());
            }

            return null;
        }

        /// <summary>
        /// Normaliza y valida el tipo de movimiento recibido.
        /// </summary>
        /// <param name="movimiento">Movimiento solicitado.</param>
        /// <returns>Movimiento normalizado o una cadena vacia si no es valido.</returns>
        private static string
            AF_frmAF_RecepcionBeneficiosTags_Movimiento_Normalizar(
                string? movimiento)
        {
            string valor = movimiento?.Trim().ToUpperInvariant()
                ?? string.Empty;
            if (valor == MovimientoRecepcion ||
                valor == MovimientoDevolucion)
            {
                return valor;
            }

            return string.Empty;
        }

        private sealed class TagsConfiguracion
        {
            public TagsConfiguracion(
                string tagRecepcion,
                string tagDevolucion,
                string tagRecepcionDevolucion,
                int etiquetasExistentes)
            {
                TagRecepcion = tagRecepcion;
                TagDevolucion = tagDevolucion;
                TagRecepcionDevolucion = tagRecepcionDevolucion;
                EtiquetasExistentes = etiquetasExistentes;
            }

            public string TagRecepcion { get; }
            public string TagDevolucion { get; }
            public string TagRecepcionDevolucion { get; }
            public int EtiquetasExistentes { get; }
        }
    }
}
