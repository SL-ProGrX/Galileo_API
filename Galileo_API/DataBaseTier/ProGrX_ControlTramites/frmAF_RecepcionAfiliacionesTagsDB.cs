using System.Data;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_ControlTramites;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_ControlTramites
{
    public sealed class FrmAfRecepcionAfiliacionesTagsDb
    {
        private const string Modulo = "AFI";
        private const string MovimientoRecepcion = "RECEPCION";
        private const string MovimientoDevolucion = "DEVOLUCION";
        private const string MensajeMovimientoInvalido =
            "El tipo de movimiento no es valido.";
        private readonly PortalDB _portalDb;

        /// <summary>
        /// Inicializa el acceso a datos del formulario.
        /// </summary>
        /// <param name="config">Configuracion de conexiones.</param>
        public FrmAfRecepcionAfiliacionesTagsDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene las etiquetas configuradas y los usuarios activos.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <returns>Datos requeridos para inicializar el formulario.</returns>
        public ErrorDto<AfRecepcionAfiliacionesTagsInicializarResponse>
            AF_frmAF_RecepcionAfiliacionesTags_Inicializar(int codEmpresa)
        {
            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                var tags =
                    AF_frmAF_RecepcionAfiliacionesTags_Tags_Obtener(
                        connection,
                        null);
                var usuarios = connection.Query<DropDownListaGenericaModel>(
                    """
                    select
                        upper(rtrim(nombre)) as item,
                        upper(rtrim(nombre)) as descripcion
                    from Usuarios
                    where estado = 'A'
                    order by nombre;
                    """).ToList();

                return DbHelper.CreateOkResponse(
                    new AfRecepcionAfiliacionesTagsInicializarResponse
                    {
                        tag_recepcion = tags.tag_recepcion,
                        tag_devolucion = tags.tag_devolucion,
                        tag_recepcion_devolucion =
                            tags.tag_recepcion_devolucion,
                        usuarios = usuarios
                    });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new AfRecepcionAfiliacionesTagsInicializarResponse());
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new AfRecepcionAfiliacionesTagsInicializarResponse());
            }
        }

        /// <summary>
        /// Elimina afiliaciones pendientes antiguas como lo hace el Form_Load.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <returns>Confirmacion de ejecucion del mantenimiento.</returns>
        public ErrorDto<AfRecepcionAfiliacionesTagsMantenimientoResponse>
            AF_frmAF_RecepcionAfiliacionesTags_Pendientes_Antiguos_Eliminar(
                int codEmpresa)
        {
            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Execute(
                    "spAFI_Afiliaciones_Duplicados_Elimina",
                    commandType: CommandType.StoredProcedure);

                return DbHelper.CreateOkResponse(
                    new AfRecepcionAfiliacionesTagsMantenimientoResponse
                    {
                        proceso_ejecutado = true
                    });
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new AfRecepcionAfiliacionesTagsMantenimientoResponse());
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new AfRecepcionAfiliacionesTagsMantenimientoResponse());
            }
        }

        /// <summary>
        /// Obtiene las boletas pendientes de una cedula segun el movimiento.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="cedula">Cedula de la afiliacion.</param>
        /// <param name="movimiento">Tipo de movimiento.</param>
        /// <returns>Boletas disponibles.</returns>
        public ErrorDto<List<AfRecepcionAfiliacionesTagsBoletaResponse>>
            AF_frmAF_RecepcionAfiliacionesTags_Boletas_Obtener(
                int codEmpresa,
                string? cedula,
                string? movimiento)
        {
            string identificacion = cedula?.Trim() ?? string.Empty;
            string tipo =
                AF_frmAF_RecepcionAfiliacionesTags_Movimiento_Normalizar(
                    movimiento);

            if (string.IsNullOrWhiteSpace(identificacion))
            {
                return DbHelper.CreateErrorResponse(
                    "La cedula es requerida.",
                    -2,
                    new List<AfRecepcionAfiliacionesTagsBoletaResponse>());
            }

            if (string.IsNullOrWhiteSpace(tipo))
            {
                return DbHelper.CreateErrorResponse(
                    MensajeMovimientoInvalido,
                    -2,
                    new List<AfRecepcionAfiliacionesTagsBoletaResponse>());
            }

            const string sql = """
                select
                    I.consec,
                    isnull(rtrim(I.usuario), '') as usuario,
                    I.fecha_ingreso,
                    concat(
                        convert(varchar(30), I.consec),
                        ' - ',
                        isnull(rtrim(I.usuario), ''),
                        ' (',
                        convert(varchar(10), I.fecha_ingreso, 103),
                        ')'
                    ) as descripcion
                from AFI_Ingresos I
                where I.cedula = @Cedula
                  and isnull(I.analista_recepcion, 0) =
                    case when @Movimiento = 'RECEPCION' then 0 else 1 end
                order by I.fecha_ingreso desc, I.consec desc;
                """;

            return DbHelper.ExecuteListQuery<
                AfRecepcionAfiliacionesTagsBoletaResponse>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        Cedula = identificacion,
                        Movimiento = tipo
                    });
        }

        /// <summary>
        /// Valida y obtiene una afiliacion por cedula y boleta.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="cedula">Cedula de la afiliacion.</param>
        /// <param name="numeroBoleta">Consecutivo de la boleta.</param>
        /// <param name="movimiento">Tipo de movimiento.</param>
        /// <returns>Afiliacion compatible con el movimiento.</returns>
        public ErrorDto<AfRecepcionAfiliacionesTagsAfiliacionResponse?>
            AF_frmAF_RecepcionAfiliacionesTags_Afiliacion_Obtener(
                int codEmpresa,
                string? cedula,
                long numeroBoleta,
                string? movimiento)
        {
            string identificacion = cedula?.Trim() ?? string.Empty;
            string tipo =
                AF_frmAF_RecepcionAfiliacionesTags_Movimiento_Normalizar(
                    movimiento);

            var error =
                AF_frmAF_RecepcionAfiliacionesTags_Afiliacion_Validar(
                    identificacion,
                    numeroBoleta,
                    tipo);
            if (error is not null)
            {
                return error;
            }

            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();
                var tags =
                    AF_frmAF_RecepcionAfiliacionesTags_Tags_Obtener(
                        connection,
                        null);

                string? validacion =
                    AF_frmAF_RecepcionAfiliacionesTags_Tag_Validar(
                        connection,
                        null,
                        identificacion,
                        numeroBoleta,
                        tipo,
                        tags);
                if (validacion is not null)
                {
                    return DbHelper.CreateErrorResponse<
                        AfRecepcionAfiliacionesTagsAfiliacionResponse?>(
                            validacion,
                            -2,
                            null);
                }

                var afiliacion =
                    AF_frmAF_RecepcionAfiliacionesTags_Afiliacion_Consultar(
                        connection,
                        null,
                        identificacion,
                        numeroBoleta,
                        tipo);

                if (afiliacion is null)
                {
                    return DbHelper.CreateErrorResponse<
                        AfRecepcionAfiliacionesTagsAfiliacionResponse?>(
                            "No se encontro la afiliacion con el estado requerido.",
                            -2,
                            null);
                }

                return DbHelper.CreateOkResponse<
                    AfRecepcionAfiliacionesTagsAfiliacionResponse?>(
                        afiliacion);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<
                    AfRecepcionAfiliacionesTagsAfiliacionResponse?>(
                        ex.Message,
                        -1,
                        null);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<
                    AfRecepcionAfiliacionesTagsAfiliacionResponse?>(
                        ex.Message,
                        -1,
                        null);
            }
        }

        /// <summary>
        /// Obtiene las afiliaciones pendientes del movimiento indicado.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="movimiento">Tipo de movimiento.</param>
        /// <returns>Afiliaciones pendientes.</returns>
        public ErrorDto<List<AfRecepcionAfiliacionesTagsPendienteResponse>>
            AF_frmAF_RecepcionAfiliacionesTags_Pendientes_Obtener(
                int codEmpresa,
                string? movimiento)
        {
            string tipo =
                AF_frmAF_RecepcionAfiliacionesTags_Movimiento_Normalizar(
                    movimiento);
            if (string.IsNullOrWhiteSpace(tipo))
            {
                return DbHelper.CreateErrorResponse(
                    MensajeMovimientoInvalido,
                    -2,
                    new List<AfRecepcionAfiliacionesTagsPendienteResponse>());
            }

            const string sql = """
                select
                    I.consec,
                    rtrim(I.cedula) as cedula,
                    rtrim(S.nombre) as nombre,
                    isnull(rtrim(O.descripcion), '') as descripcion,
                    I.fecha as fecha,
                    isnull(rtrim(I.usuario), '') as usuario
                from AFI_Ingresos I
                inner join Socios S on I.cedula = S.cedula
                left join SIF_Oficinas O
                    on I.cod_oficina = O.cod_oficina
                where isnull(I.analista_recepcion, 0) =
                    case when @Movimiento = 'RECEPCION' then 0 else 2 end
                  and S.estadoactual in ('S', 'A', 'P')
                order by I.fecha desc, I.consec desc;
                """;

            return DbHelper.ExecuteListQuery<
                AfRecepcionAfiliacionesTagsPendienteResponse>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new { Movimiento = tipo });
        }

        /// <summary>
        /// Registra las etiquetas para las afiliaciones seleccionadas.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="request">Movimiento y afiliaciones seleccionadas.</param>
        /// <returns>Cantidad de registros aplicados.</returns>
        public ErrorDto<AfRecepcionAfiliacionesTagsAplicarResponse>
            AF_frmAF_RecepcionAfiliacionesTags_Movimiento_Aplicar(
                int codEmpresa,
                AfRecepcionAfiliacionesTagsAplicarRequest? request)
        {
            var error =
                AF_frmAF_RecepcionAfiliacionesTags_Aplicar_Validar(request);
            if (error is not null)
            {
                return error;
            }

            string movimiento =
                AF_frmAF_RecepcionAfiliacionesTags_Movimiento_Normalizar(
                    request!.movimiento);

            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();
                using var transaction = connection.BeginTransaction();

                try
                {
                    var tags =
                        AF_frmAF_RecepcionAfiliacionesTags_Tags_Obtener(
                            connection,
                            transaction);
                    string tag = movimiento == MovimientoRecepcion
                        ? tags.tag_recepcion
                        : tags.tag_devolucion;
                    string observacion = movimiento == MovimientoRecepcion
                        ? "Recibida la documentacion de la afiliacion"
                        : "Devolucion de la documentacion de la afiliacion";
                    int aplicados = 0;

                    foreach (var item in request.afiliaciones
                        .GroupBy(x => new
                        {
                            Cedula = x.cedula.Trim(),
                            x.consec
                        })
                        .Select(group => group.First()))
                    {
                        string identificacion = item.cedula.Trim();
                        string? validacion =
                            AF_frmAF_RecepcionAfiliacionesTags_Tag_Validar(
                                connection,
                                transaction,
                                identificacion,
                                item.consec,
                                movimiento,
                                tags);
                        if (validacion is not null)
                        {
                            throw new InvalidOperationException(validacion);
                        }

                        var afiliacion =
                            AF_frmAF_RecepcionAfiliacionesTags_Afiliacion_Consultar(
                                connection,
                                transaction,
                                identificacion,
                                item.consec,
                                movimiento);
                        if (afiliacion is null)
                        {
                            throw new InvalidOperationException(
                                $"La cedula {identificacion} y boleta {item.consec} ya no cumplen el estado requerido.");
                        }

                        connection.Execute(
                            "spSIFRegistraTags",
                            new
                            {
                                Codigo = afiliacion.cedula,
                                Tag = tag,
                                Usuario = request.usuario.Trim(),
                                Observacion = observacion,
                                Documento = afiliacion.consec.ToString(),
                                Modulo,
                                Llave_01 = afiliacion.cedula,
                                Llave_02 = afiliacion.consec.ToString(),
                                Llave_03 = string.Empty
                            },
                            transaction,
                            commandType: CommandType.StoredProcedure);
                        aplicados++;
                    }

                    transaction.Commit();
                    return DbHelper.CreateOkResponse(
                        new AfRecepcionAfiliacionesTagsAplicarResponse
                        {
                            registros_aplicados = aplicados
                        },
                        "Proceso concluido con exito.");
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new AfRecepcionAfiliacionesTagsAplicarResponse());
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new AfRecepcionAfiliacionesTagsAplicarResponse());
            }
        }

        /// <summary>
        /// Obtiene el historial de etiquetas de afiliaciones.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="request">Filtros del historial.</param>
        /// <returns>Movimientos de etiquetas encontrados.</returns>
        public ErrorDto<List<AfRecepcionAfiliacionesTagsHistorialResponse>>
            AF_frmAF_RecepcionAfiliacionesTags_Historial_Obtener(
                int codEmpresa,
                AfRecepcionAfiliacionesTagsHistorialRequest? request)
        {
            request ??= new AfRecepcionAfiliacionesTagsHistorialRequest();
            string cedula = request.cedula?.Trim() ?? string.Empty;
            string usuario = request.usuario?.Trim() ?? string.Empty;

            if (request.documento.HasValue && request.documento <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "La boleta no es valida.",
                    -2,
                    new List<AfRecepcionAfiliacionesTagsHistorialResponse>());
            }

            if (request.fecha_inicio?.Date > request.fecha_fin?.Date)
            {
                return DbHelper.CreateErrorResponse(
                    "La fecha inicial no puede ser mayor que la fecha final.",
                    -2,
                    new List<AfRecepcionAfiliacionesTagsHistorialResponse>());
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
                  and (@Cedula = '' or CT.codigo like '%' + @Cedula + '%')
                  and (
                      @Documento is null
                      or CT.documento like
                          '%' + convert(varchar(30), @Documento) + '%'
                  )
                  and (
                      @Usuario = ''
                      or CT.registro_usuario like '%' + @Usuario + '%'
                  )
                  and (
                      @FechaInicio is null
                      or CT.registro_fecha >= @FechaInicio
                  )
                  and (
                      @FechaFin is null
                      or CT.registro_fecha < dateadd(day, 1, @FechaFin)
                  )
                order by CT.registro_fecha desc;
                """;

            return DbHelper.ExecuteListQuery<
                AfRecepcionAfiliacionesTagsHistorialResponse>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        Modulo,
                        Cedula = cedula,
                        request.documento,
                        Usuario = usuario,
                        FechaInicio = request.fecha_inicio?.Date,
                        FechaFin = request.fecha_fin?.Date
                    });
        }

        /// <summary>
        /// Consulta una afiliacion que cumple el estado del movimiento.
        /// </summary>
        /// <param name="connection">Conexion SQL.</param>
        /// <param name="transaction">Transaccion SQL opcional.</param>
        /// <param name="cedula">Cedula de la afiliacion.</param>
        /// <param name="numeroBoleta">Consecutivo de la boleta.</param>
        /// <param name="movimiento">Tipo de movimiento.</param>
        /// <returns>Afiliacion encontrada.</returns>
        private static AfRecepcionAfiliacionesTagsAfiliacionResponse?
            AF_frmAF_RecepcionAfiliacionesTags_Afiliacion_Consultar(
                SqlConnection connection,
                SqlTransaction? transaction,
                string cedula,
                long numeroBoleta,
                string movimiento)
        {
            const string sql = """
                select top 1
                    rtrim(I.cedula) as cedula,
                    rtrim(S.nombre) as nombre,
                    isnull(rtrim(O.descripcion), '') as descripcion,
                    I.consec
                from AFI_Ingresos I
                inner join Socios S on I.cedula = S.cedula
                left join SIF_Oficinas O
                    on I.cod_oficina = O.cod_oficina
                where I.cedula = @Cedula
                  and I.consec = @NumeroBoleta
                  and S.estadoactual = 'S'
                  and isnull(I.analista_recepcion, 0) =
                    case when @Movimiento = 'RECEPCION' then 0 else 1 end;
                """;

            return connection.QueryFirstOrDefault<
                AfRecepcionAfiliacionesTagsAfiliacionResponse>(
                    sql,
                    new
                    {
                        Cedula = cedula,
                        NumeroBoleta = numeroBoleta,
                        Movimiento = movimiento
                    },
                    transaction);
        }

        /// <summary>
        /// Valida la secuencia de etiquetas usada por el formulario VB6.
        /// </summary>
        /// <param name="connection">Conexion SQL.</param>
        /// <param name="transaction">Transaccion SQL opcional.</param>
        /// <param name="cedula">Cedula de la afiliacion.</param>
        /// <param name="numeroBoleta">Consecutivo de la boleta.</param>
        /// <param name="movimiento">Tipo de movimiento.</param>
        /// <param name="tags">Configuracion de etiquetas.</param>
        /// <returns>Mensaje de validacion o null.</returns>
        private static string?
            AF_frmAF_RecepcionAfiliacionesTags_Tag_Validar(
                SqlConnection connection,
                SqlTransaction? transaction,
                string cedula,
                long numeroBoleta,
                string movimiento,
                TagsConfiguracion tags)
        {
            int resultado = connection.ExecuteScalar<int>(
                """
                select dbo.fxSIFValidaTagRev(
                    @Cedula,
                    @TagRecepcion,
                    @TagDevolucion,
                    @Modulo,
                    @Documento,
                    null
                );
                """,
                new
                {
                    Cedula = cedula,
                    TagRecepcion = tags.tag_recepcion,
                    TagDevolucion = tags.tag_devolucion,
                    Modulo,
                    Documento = numeroBoleta.ToString()
                },
                transaction);

            if (movimiento == MovimientoRecepcion && resultado == 2)
            {
                return $"No es posible registrar dos recepciones consecutivas en la cedula {cedula}.";
            }

            if (movimiento == MovimientoDevolucion && resultado == 3)
            {
                return $"No es posible registrar dos devoluciones consecutivas en la cedula {cedula}.";
            }

            int resultadoRecepcionDevolucion = connection.ExecuteScalar<int>(
                """
                select dbo.fxSIFValidaTagRev(
                    @Cedula,
                    @TagRecepcion,
                    @TagDevolucion,
                    @Modulo,
                    @Documento,
                    @TagRecepcionDevolucion
                );
                """,
                new
                {
                    Cedula = cedula,
                    TagRecepcion = tags.tag_recepcion,
                    TagDevolucion = tags.tag_devolucion,
                    Modulo,
                    Documento = numeroBoleta.ToString(),
                    TagRecepcionDevolucion =
                        tags.tag_recepcion_devolucion
                },
                transaction);

            return resultadoRecepcionDevolucion == 4
                ? $"No es posible registrar una recepcion sin aplicar la devolucion en la cedula {cedula}."
                : null;
        }

        /// <summary>
        /// Obtiene y valida los parametros 10, 11 y 12 de etiquetas.
        /// </summary>
        /// <param name="connection">Conexion SQL.</param>
        /// <param name="transaction">Transaccion SQL opcional.</param>
        /// <returns>Configuracion de etiquetas.</returns>
        private static TagsConfiguracion
            AF_frmAF_RecepcionAfiliacionesTags_Tags_Obtener(
                SqlConnection connection,
                SqlTransaction? transaction)
        {
            var parametros = connection.Query<ParametroTag>(
                """
                select
                    rtrim(cod_parametro) as cod_parametro,
                    isnull(rtrim(valor), '') as valor
                from SIF_Parametros
                where cod_parametro in ('10', '11', '12');
                """,
                transaction: transaction).ToList();

            string Obtener(string codigo) => parametros
                .FirstOrDefault(x => x.cod_parametro == codigo)?.valor
                ?? string.Empty;

            var tags = new TagsConfiguracion
            {
                tag_recepcion = Obtener("10"),
                tag_devolucion = Obtener("11"),
                tag_recepcion_devolucion = Obtener("12")
            };

            if (string.IsNullOrWhiteSpace(tags.tag_recepcion) ||
                string.IsNullOrWhiteSpace(tags.tag_devolucion) ||
                string.IsNullOrWhiteSpace(tags.tag_recepcion_devolucion))
            {
                throw new InvalidOperationException(
                    "Falta configurar uno de los parametros de etiquetas 10, 11 o 12.");
            }

            int existentes = connection.ExecuteScalar<int>(
                """
                select
                    case when exists (
                        select 1 from SIF_Tags where tag_codigo = @Recepcion
                    ) then 1 else 0 end
                    + case when exists (
                        select 1 from SIF_Tags where tag_codigo = @Devolucion
                    ) then 1 else 0 end
                    + case when exists (
                        select 1 from SIF_Tags
                        where tag_codigo = @RecepcionDevolucion
                    ) then 1 else 0 end;
                """,
                new
                {
                    Recepcion = tags.tag_recepcion,
                    Devolucion = tags.tag_devolucion,
                    RecepcionDevolucion = tags.tag_recepcion_devolucion
                },
                transaction);

            if (existentes != 3)
            {
                throw new InvalidOperationException(
                    "Uno o mas codigos de etiqueta configurados no existen.");
            }

            return tags;
        }

        /// <summary>
        /// Valida los datos para obtener una afiliacion.
        /// </summary>
        /// <param name="cedula">Cedula de la afiliacion.</param>
        /// <param name="numeroBoleta">Consecutivo de la boleta.</param>
        /// <param name="movimiento">Movimiento normalizado.</param>
        /// <returns>Error de validacion o null.</returns>
        private static ErrorDto<
            AfRecepcionAfiliacionesTagsAfiliacionResponse?>?
            AF_frmAF_RecepcionAfiliacionesTags_Afiliacion_Validar(
                string cedula,
                long numeroBoleta,
                string movimiento)
        {
            if (string.IsNullOrWhiteSpace(cedula) || numeroBoleta <= 0)
            {
                return DbHelper.CreateErrorResponse<
                    AfRecepcionAfiliacionesTagsAfiliacionResponse?>(
                        "Debe indicar una cedula y boleta validas.",
                        -2,
                        null);
            }

            if (string.IsNullOrWhiteSpace(movimiento))
            {
                return DbHelper.CreateErrorResponse<
                    AfRecepcionAfiliacionesTagsAfiliacionResponse?>(
                        MensajeMovimientoInvalido,
                        -2,
                        null);
            }

            return null;
        }

        /// <summary>
        /// Valida el request del proceso de aplicacion.
        /// </summary>
        /// <param name="request">Request a validar.</param>
        /// <returns>Error de validacion o null.</returns>
        private static ErrorDto<
            AfRecepcionAfiliacionesTagsAplicarResponse>?
            AF_frmAF_RecepcionAfiliacionesTags_Aplicar_Validar(
                AfRecepcionAfiliacionesTagsAplicarRequest? request)
        {
            if (request is null ||
                string.IsNullOrWhiteSpace(request.usuario) ||
                request.afiliaciones is null ||
                request.afiliaciones.Count == 0)
            {
                return DbHelper.CreateErrorResponse(
                    "El usuario y las afiliaciones seleccionadas son requeridos.",
                    -2,
                    new AfRecepcionAfiliacionesTagsAplicarResponse());
            }

            if (string.IsNullOrWhiteSpace(
                    AF_frmAF_RecepcionAfiliacionesTags_Movimiento_Normalizar(
                        request.movimiento)))
            {
                return DbHelper.CreateErrorResponse(
                    MensajeMovimientoInvalido,
                    -2,
                    new AfRecepcionAfiliacionesTagsAplicarResponse());
            }

            if (request.afiliaciones.Any(
                    item => string.IsNullOrWhiteSpace(item.cedula) ||
                        item.consec <= 0))
            {
                return DbHelper.CreateErrorResponse(
                    "La lista contiene cedulas o boletas no validas.",
                    -2,
                    new AfRecepcionAfiliacionesTagsAplicarResponse());
            }

            return null;
        }

        /// <summary>
        /// Normaliza un tipo de movimiento permitido.
        /// </summary>
        /// <param name="movimiento">Movimiento recibido.</param>
        /// <returns>Movimiento normalizado o cadena vacia.</returns>
        private static string
            AF_frmAF_RecepcionAfiliacionesTags_Movimiento_Normalizar(
                string? movimiento)
        {
            string valor = movimiento?.Trim().ToUpperInvariant()
                ?? string.Empty;
            return valor is MovimientoRecepcion or MovimientoDevolucion
                ? valor
                : string.Empty;
        }

        private sealed class ParametroTag
        {
            public string cod_parametro { get; set; } = string.Empty;
            public string valor { get; set; } = string.Empty;
        }

        private sealed class TagsConfiguracion
        {
            public string tag_recepcion { get; set; } = string.Empty;
            public string tag_devolucion { get; set; } = string.Empty;
            public string tag_recepcion_devolucion { get; set; } = string.Empty;
        }
    }
}
