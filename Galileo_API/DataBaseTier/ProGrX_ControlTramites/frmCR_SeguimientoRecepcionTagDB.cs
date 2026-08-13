using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_ControlTramites;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_ControlTramites
{
    public sealed class FrmCrSeguimientoRecepcionTagDb
    {
        private const string MovimientoRecepcion = "RECEPCION";

        private const string MovimientoDevolucion = "DEVOLUCION";

        private const string EstadoRecepcion = "N";

        private const string EstadoDevolucion = "D";

        private readonly PortalDB _portalDb;

        public FrmCrSeguimientoRecepcionTagDb(
            IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene los tags de recepcion y devolucion,
        /// junto con los usuarios activos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<
            CrSeguimientoRecepcionTagInicializarResponse>
            CR_frmCR_SeguimientoRecepcionTag_Inicializar(
                int codEmpresa)
        {
            try
            {
                using var connection =
                    DbHelper.OpenConnection(
                        _portalDb,
                        codEmpresa);

                connection.Open();

                var tags =
                    CR_frmCR_SeguimientoRecepcionTag_Tags_Obtener(
                        connection,
                        null);

                var usuarios =
                    connection.Query<
                        DropDownListaGenericaModel>(
                        """
                        select
                            upper(rtrim(nombre)) as item,
                            upper(rtrim(nombre)) as descripcion
                        from Usuarios
                        where estado = 'A'
                        order by nombre;
                        """)
                    .ToList();

                var response =
                    new CrSeguimientoRecepcionTagInicializarResponse
                    {
                        tag_recepcion =
                            tags.tag_recepcion,
                        tag_devolucion =
                            tags.tag_devolucion,
                        usuarios = usuarios
                    };

                return DbHelper.CreateOkResponse(
                    response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CrSeguimientoRecepcionTagInicializarResponse());
            }
        }

        /// <summary>
        /// Valida y obtiene una operacion de credito.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="idSolicitud"></param>
        /// <param name="movimiento"></param>
        /// <returns></returns>
        public ErrorDto<
            CrSeguimientoRecepcionTagOperacionResponse?>
            CR_frmCR_SeguimientoRecepcionTag_Operacion_Obtener(
                int codEmpresa,
                long idSolicitud,
                string? movimiento)
        {
            if (idSolicitud <= 0)
            {
                return DbHelper.CreateErrorResponse<
                    CrSeguimientoRecepcionTagOperacionResponse?>(
                        "El n&uacute;mero de operaci&oacute;n no es v&aacute;lido.",
                        -2,
                        null);
            }

            string movimientoNormalizado =
                CR_frmCR_SeguimientoRecepcionTag_Movimiento_Normalizar(
                    movimiento);

            if (string.IsNullOrWhiteSpace(
                    movimientoNormalizado))
            {
                return DbHelper.CreateErrorResponse<
                    CrSeguimientoRecepcionTagOperacionResponse?>(
                        "El tipo de movimiento no es v&aacute;lido.",
                        -2,
                        null);
            }

            try
            {
                using var connection =
                    DbHelper.OpenConnection(
                        _portalDb,
                        codEmpresa);

                connection.Open();

                var tags =
                    CR_frmCR_SeguimientoRecepcionTag_Tags_Obtener(
                        connection,
                        null);

                string? validacion =
                    CR_frmCR_SeguimientoRecepcionTag_Movimiento_Validar(
                        connection,
                        null,
                        idSolicitud,
                        movimientoNormalizado,
                        tags);

                if (!string.IsNullOrWhiteSpace(
                        validacion))
                {
                    return DbHelper.CreateErrorResponse<
                        CrSeguimientoRecepcionTagOperacionResponse?>(
                            validacion,
                            -2,
                            null);
                }

                var operacion =
                    CR_frmCR_SeguimientoRecepcionTag_Operacion_Consultar(
                        connection,
                        null,
                        idSolicitud);

                if (operacion is null)
                {
                    return DbHelper.CreateErrorResponse<
                        CrSeguimientoRecepcionTagOperacionResponse?>(
                            "No se encontr&oacute; la operaci&oacute;n indicada.",
                            -2,
                            null);
                }

                return DbHelper.CreateOkResponse<
                    CrSeguimientoRecepcionTagOperacionResponse?>(
                        operacion);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<
                    CrSeguimientoRecepcionTagOperacionResponse?>(
                        ex.Message,
                        -1,
                        null);
            }
        }

        /// <summary>
        /// Obtiene las operaciones pendientes de recepcion
        /// o devolucion de documentos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<
            CrSeguimientoRecepcionTagPendienteResponse>>
            CR_frmCR_SeguimientoRecepcionTag_Pendientes_Obtener(
                int codEmpresa,
                CrSeguimientoRecepcionTagPendientesRequest?
                    request)
        {
            request ??=
                new CrSeguimientoRecepcionTagPendientesRequest();

            string movimiento =
                CR_frmCR_SeguimientoRecepcionTag_Movimiento_Normalizar(
                    request.movimiento);

            if (string.IsNullOrWhiteSpace(movimiento))
            {
                return DbHelper.CreateErrorResponse(
                    "El tipo de movimiento no es v&aacute;lido.",
                    -2,
                    new List<
                        CrSeguimientoRecepcionTagPendienteResponse>());
            }

            string estado =
                movimiento == MovimientoRecepcion
                    ? EstadoRecepcion
                    : EstadoDevolucion;

            const string sql = """
                select
                    R.ID_SOLICITUD as id_solicitud,
                    R.FECHAFORP as fechaforp,
                    isnull(rtrim(R.CEDULA), '') as cedula,
                    isnull(rtrim(S.NOMBRE), '') as nombre,
                    isnull(rtrim(R.CODIGO), '') as codigo,
                    isnull(rtrim(O.DESCRIPCION), '') as descripcion,
                    isnull(R.MONTOSOL, 0) as montosol,
                    isnull(rtrim(R.USERFOR), '') as userfor,
                    isnull(
                        rtrim(T.REGISTRO_USUARIO),
                        ''
                    ) as usuario_revision,
                    isnull(RA.REMESA, 0) as remesa,
                    isnull(
                        rtrim(RE.USUARIO),
                        ''
                    ) as usuario_remesa
                from REG_CREDITOS R
                inner join SOCIOS S
                    on S.CEDULA = R.CEDULA
                left join SIF_OFICINAS O
                    on R.COD_OFICINA_R = O.COD_OFICINA
                left join CATALOGO C
                    on R.CODIGO = C.CODIGO
                left join CRD_OPERACION_TAGS T
                    on R.ID_SOLICITUD = T.ID_SOLICITUD
                    and T.TAG_CODIGO = 'S10'
                left join CRD_REMESA_ASG RA
                    on R.ID_SOLICITUD = RA.ID_SOLICITUD
                left join CRD_REMESAS RE
                    on RE.REMESA = RA.REMESA
                where R.ESTADOSOL = 'F'
                  and R.REFERENCIA is null
                  and isnull(
                      R.ANALISTAS_RECEPCION,
                      'N'
                  ) = @Estado
                  and R.CODIGO not in (
                      select COD_PLAN
                      from FND_PLANES
                  )
                  and C.RETENCION = 'N'
                order by R.ID_SOLICITUD;
                """;

            return DbHelper.ExecuteListQuery<
                CrSeguimientoRecepcionTagPendienteResponse>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        Estado = estado
                    });
        }

        /// <summary>
        /// Registra los tags de recepcion o devolucion
        /// para las operaciones seleccionadas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<
            CrSeguimientoRecepcionTagAplicarResponse>
            CR_frmCR_SeguimientoRecepcionTag_Movimiento_Aplicar(
                int codEmpresa,
                CrSeguimientoRecepcionTagAplicarRequest?
                    request)
        {
            var validacionRequest =
                CR_frmCR_SeguimientoRecepcionTag_Aplicar_Validar(
                    request);

            if (validacionRequest is not null)
            {
                return validacionRequest;
            }

            string movimiento =
                CR_frmCR_SeguimientoRecepcionTag_Movimiento_Normalizar(
                    request.movimiento);

            try
            {
                using var connection =
                    DbHelper.OpenConnection(
                        _portalDb,
                        codEmpresa);

                connection.Open();

                using var transaction =
                    connection.BeginTransaction();

                try
                {
                    var tags =
                        CR_frmCR_SeguimientoRecepcionTag_Tags_Obtener(
                            connection,
                            transaction);

                    string tag =
                        movimiento == MovimientoRecepcion
                            ? tags.tag_recepcion
                            : tags.tag_devolucion;

                    string observacion =
                        movimiento == MovimientoRecepcion
                            ? "Recibida la documentaci&oacute;n de la operaci&oacute;n"
                            : "Devoluci&oacute;n de la documentaci&oacute;n de la operaci&oacute;n";

                    int aplicados = 0;

                    foreach (
                        long idSolicitud in
                        request.operaciones.Distinct())
                    {
                        var operacion =
                            CR_frmCR_SeguimientoRecepcionTag_Operacion_Consultar(
                                connection,
                                transaction,
                                idSolicitud);

                        if (operacion is null)
                        {
                            throw new InvalidOperationException(
                                $"No se encontr&oacute; la operaci&oacute;n {idSolicitud}.");
                        }

                        string? validacion =
                            CR_frmCR_SeguimientoRecepcionTag_Movimiento_Validar(
                                connection,
                                transaction,
                                idSolicitud,
                                movimiento,
                                tags);

                        if (!string.IsNullOrWhiteSpace(
                                validacion))
                        {
                            throw new InvalidOperationException(
                                validacion);
                        }

                        MCredito.sbCrdOperacionTags(
                            connection,
                            transaction,
                            new MCredito
                                .CrOperacionTagRegistrarRequest
                            {
                                operacion =
                                    idSolicitud,
                                linea =
                                    operacion.codigo,
                                tag = tag,
                                usuario =
                                    request.usuario.Trim(),
                                asignado =
                                    string.Empty,
                                notas =
                                    observacion
                            });

                        aplicados++;
                    }

                    transaction.Commit();

                    return DbHelper.CreateOkResponse(
                        new CrSeguimientoRecepcionTagAplicarResponse
                        {
                            registros_aplicados =
                                aplicados
                        },
                        "Proceso concluido con éxito.");
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new CrSeguimientoRecepcionTagAplicarResponse());
            }
        }

        /// <summary>
        /// Obtiene el historial de recepcion y devolucion
        /// aplicando filtros opcionales.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<
            CrSeguimientoRecepcionTagHistorialResponse>>
            CR_frmCR_SeguimientoRecepcionTag_Historial_Obtener(
                int codEmpresa,
                CrSeguimientoRecepcionTagHistorialRequest?
                    request)
        {
            request ??=
                new CrSeguimientoRecepcionTagHistorialRequest();

            string usuario =
                request.usuario?.Trim()
                ?? string.Empty;

            if (
                request.id_solicitud.HasValue &&
                request.id_solicitud.Value <= 0
            )
            {
                return DbHelper.CreateErrorResponse(
                    "El n&uacute;mero de operaci&oacute;n no es v&aacute;lido.",
                    -2,
                    new List<
                        CrSeguimientoRecepcionTagHistorialResponse>());
            }

            if (
                request.fecha_inicio.HasValue &&
                request.fecha_fin.HasValue &&
                request.fecha_inicio.Value.Date >
                request.fecha_fin.Value.Date
            )
            {
                return DbHelper.CreateErrorResponse(
                    "La fecha inicial no puede ser mayor que la fecha final.",
                    -2,
                    new List<
                        CrSeguimientoRecepcionTagHistorialResponse>());
            }

            try
            {
                using var connection =
                    DbHelper.OpenConnection(
                        _portalDb,
                        codEmpresa);

                connection.Open();

                var tags =
                    CR_frmCR_SeguimientoRecepcionTag_Tags_Obtener(
                        connection,
                        null);

                const string sql = """
                    select
                        isnull(
                            rtrim(T.DESCRIPCION),
                            ''
                        ) as descripcion,
                        isnull(
                            rtrim(OT.NOTAS),
                            ''
                        ) as notas,
                        OT.REGISTRO_FECHA as registro_fecha,
                        isnull(
                            rtrim(OT.REGISTRO_USUARIO),
                            ''
                        ) as registro_usuario
                    from CRD_OPERACION_TAGS OT
                    inner join CRD_TAGS T
                        on OT.TAG_CODIGO =
                           T.TAG_CODIGO
                    where (
                        @IdSolicitud is null
                        or OT.ID_SOLICITUD =
                           @IdSolicitud
                    )
                      and (
                          OT.TAG_CODIGO =
                              @TagRecepcion
                          or OT.TAG_CODIGO =
                              @TagDevolucion
                      )
                      and (
                          @Usuario = ''
                          or OT.REGISTRO_USUARIO like
                              '%' + @Usuario + '%'
                      )
                      and (
                          @FechaInicio is null
                          or OT.REGISTRO_FECHA >=
                             @FechaInicio
                      )
                      and (
                          @FechaFin is null
                          or OT.REGISTRO_FECHA <
                              dateadd(
                                  day,
                                  1,
                                  convert(
                                      date,
                                      @FechaFin
                                  )
                              )
                      )
                    order by
                        OT.REGISTRO_FECHA desc,
                        OT.LINEA desc;
                    """;

                var historial =
                    connection.Query<
                        CrSeguimientoRecepcionTagHistorialResponse>(
                            sql,
                            new
                            {
                                IdSolicitud =
                                    request.id_solicitud,
                                TagRecepcion =
                                    tags.tag_recepcion,
                                TagDevolucion =
                                    tags.tag_devolucion,
                                Usuario = usuario,
                                FechaInicio =
                                    request.fecha_inicio?.Date,
                                FechaFin =
                                    request.fecha_fin?.Date
                            })
                        .ToList();

                return DbHelper.CreateOkResponse(
                    historial);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new List<
                        CrSeguimientoRecepcionTagHistorialResponse>());
            }
        }

        private static
            CrSeguimientoRecepcionTagOperacionResponse?
            CR_frmCR_SeguimientoRecepcionTag_Operacion_Consultar(
                SqlConnection connection,
                SqlTransaction? transaction,
                long idSolicitud)
        {
            const string sql = """
                select top 1
                    R.ID_SOLICITUD as id_solicitud,
                    isnull(
                        rtrim(R.CODIGO),
                        ''
                    ) as codigo,
                    isnull(
                        rtrim(R.CEDULA),
                        ''
                    ) as cedula,
                    R.FECHAFORF as fechaforf,
                    isnull(
                        rtrim(O.DESCRIPCION),
                        ''
                    ) as descripcion
                from REG_CREDITOS R
                left join SIF_OFICINAS O
                    on R.COD_OFICINA_R =
                       O.COD_OFICINA
                where R.ID_SOLICITUD =
                      @IdSolicitud;
                """;

            return connection.QueryFirstOrDefault<
                CrSeguimientoRecepcionTagOperacionResponse>(
                    sql,
                    new
                    {
                        IdSolicitud = idSolicitud
                    },
                    transaction);
        }

        private static TagsConfiguracion
            CR_frmCR_SeguimientoRecepcionTag_Tags_Obtener(
                SqlConnection connection,
                SqlTransaction? transaction)
        {
            var parametros =
                connection.Query<ParametroTag>(
                    """
                    select
                        rtrim(COD_PARAMETRO)
                            as cod_parametro,
                        isnull(
                            rtrim(VALOR),
                            ''
                        ) as valor
                    from CRD_PARAMETROS
                    where COD_PARAMETRO
                        in ('28', '29');
                    """,
                    transaction: transaction)
                .ToList();

            string ObtenerValor(string codigo)
            {
                return parametros
                    .FirstOrDefault(
                        item =>
                            item.cod_parametro ==
                            codigo)
                    ?.valor
                    ?? string.Empty;
            }

            var tags =
                new TagsConfiguracion
                {
                    tag_recepcion =
                        ObtenerValor("28"),
                    tag_devolucion =
                        ObtenerValor("29")
                };

            if (string.IsNullOrWhiteSpace(
                    tags.tag_recepcion))
            {
                throw new InvalidOperationException(
                    "No est&aacute; definida la etiqueta de recepci&oacute;n.");
            }

            if (string.IsNullOrWhiteSpace(
                    tags.tag_devolucion))
            {
                throw new InvalidOperationException(
                    "No est&aacute; definida la etiqueta de devoluci&oacute;n.");
            }

            int tagsExistentes =
                connection.ExecuteScalar<int>(
                    """
                    select
                        case
                            when exists (
                                select 1
                                from CRD_TAGS
                                where TAG_CODIGO =
                                    @TagRecepcion
                            )
                            then 1
                            else 0
                        end
                        +
                        case
                            when exists (
                                select 1
                                from CRD_TAGS
                                where TAG_CODIGO =
                                    @TagDevolucion
                            )
                            then 1
                            else 0
                        end;
                    """,
                    new
                    {
                        TagRecepcion =
                            tags.tag_recepcion,
                        TagDevolucion =
                            tags.tag_devolucion
                    },
                    transaction);

            if (tagsExistentes != 2)
            {
                throw new InvalidOperationException(
                    "Uno o mas tags configurados no existen.");
            }

            return tags;
        }

        private static string?
            CR_frmCR_SeguimientoRecepcionTag_Movimiento_Validar(
                SqlConnection connection,
                SqlTransaction? transaction,
                long idSolicitud,
                string movimiento,
                TagsConfiguracion tags)
        {
            string tagActual =
                movimiento == MovimientoRecepcion
                    ? tags.tag_recepcion
                    : tags.tag_devolucion;

            string tagAnterior =
                movimiento == MovimientoRecepcion
                    ? tags.tag_devolucion
                    : tags.tag_recepcion;

            int resultado =
                connection.ExecuteScalar<int>(
                    """
                    select dbo.fxCrdOperacionValidaTagRev(
                        @IdSolicitud,
                        @TagActual,
                        @TagAnterior
                    );
                    """,
                    new
                    {
                        IdSolicitud = idSolicitud,
                        TagActual = tagActual,
                        TagAnterior = tagAnterior
                    },
                    transaction);

            if (resultado != 1)
            {
                return null;
            }

            return movimiento == MovimientoRecepcion
                ? "No es posible registrar consecutivamente "
                    + "dos recepciones en la operacion "
                    + $"{idSolicitud}."
                : "No es posible registrar consecutivamente "
                    + "dos devoluciones en la operacion "
                    + $"{idSolicitud}.";
        }

        private static ErrorDto<
            CrSeguimientoRecepcionTagAplicarResponse>?
            CR_frmCR_SeguimientoRecepcionTag_Aplicar_Validar(
                CrSeguimientoRecepcionTagAplicarRequest?
                    request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los datos del proceso son requeridos.",
                    -2,
                    new CrSeguimientoRecepcionTagAplicarResponse());
            }

            string movimiento =
                CR_frmCR_SeguimientoRecepcionTag_Movimiento_Normalizar(
                    request.movimiento);

            if (string.IsNullOrWhiteSpace(movimiento))
            {
                return DbHelper.CreateErrorResponse(
                    "El tipo de movimiento no es v&aacute;lido.",
                    -2,
                    new CrSeguimientoRecepcionTagAplicarResponse());
            }

            if (string.IsNullOrWhiteSpace(
                    request.usuario))
            {
                return DbHelper.CreateErrorResponse(
                    "El usuario es requerido.",
                    -2,
                    new CrSeguimientoRecepcionTagAplicarResponse());
            }

            if (
                request.operaciones is null ||
                request.operaciones.Count == 0
            )
            {
                return DbHelper.CreateErrorResponse(
                    "Debe seleccionar al menos una operaci&oacute;n.",
                    -2,
                    new CrSeguimientoRecepcionTagAplicarResponse());
            }

            if (
                request.operaciones.Any(
                    idSolicitud =>
                        idSolicitud <= 0)
            )
            {
                return DbHelper.CreateErrorResponse(
                    "La lista contiene n&uacute;meros de operaci&oacute;n no v&aacute;lidos.",
                    -2,
                    new CrSeguimientoRecepcionTagAplicarResponse());
            }

            return null;
        }

        private static string
            CR_frmCR_SeguimientoRecepcionTag_Movimiento_Normalizar(
                string? movimiento)
        {
            string valor =
                movimiento?.Trim().ToUpperInvariant()
                ?? string.Empty;

            return valor is MovimientoRecepcion
                or MovimientoDevolucion
                    ? valor
                    : string.Empty;
        }

        private sealed class ParametroTag
        {
            public string cod_parametro { get; set; } =
                string.Empty;

            public string valor { get; set; } =
                string.Empty;
        }

        private sealed class TagsConfiguracion
        {
            public string tag_recepcion { get; set; } =
                string.Empty;

            public string tag_devolucion { get; set; } =
                string.Empty;
        }
    }
}