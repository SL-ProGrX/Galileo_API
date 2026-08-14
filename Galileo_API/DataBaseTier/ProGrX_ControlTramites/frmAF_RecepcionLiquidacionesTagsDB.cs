using System.Data;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_ControlTramites;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_ControlTramites
{
    public sealed class FrmAfRecepcionLiquidacionesTagsDb
    {
        private const string Modulo = "LIQ";
        private const string MovimientoRecepcion = "RECEPCION";
        private const string MovimientoDevolucion = "DEVOLUCION";

        private readonly PortalDB _portalDb;

        public FrmAfRecepcionLiquidacionesTagsDb(
            IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene los tags configurados y los usuarios activos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<
            AfRecepcionLiquidacionesTagInicializarResponse>
            AF_frmAF_RecepcionLiquidacionesTag_Inicializar(
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
                    AF_frmAF_RecepcionLiquidacionesTag_Tags_Obtener(
                        connection,
                        null);

                var usuarios =
                    connection.Query<
                        DropDownListaGenericaModel>(
                        """
                        select
                            upper(rtrim(NOMBRE)) as item,
                            upper(rtrim(NOMBRE)) as descripcion
                        from USUARIOS
                        where ESTADO = 'A'
                        order by NOMBRE;
                        """)
                    .ToList();

                var response =
                    new AfRecepcionLiquidacionesTagInicializarResponse
                    {
                        tag_recepcion =
                            tags.tag_recepcion,
                        tag_devolucion =
                            tags.tag_devolucion,
                        tag_recepcion_devolucion =
                            tags.tag_recepcion_devolucion,
                        usuarios = usuarios
                    };

                return DbHelper.CreateOkResponse(
                    response);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -2,
                    new AfRecepcionLiquidacionesTagInicializarResponse());
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new AfRecepcionLiquidacionesTagInicializarResponse());
            }
        }

        /// <summary>
        /// Valida y obtiene la liquidacion asociada con una boleta.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="numeroBoleta"></param>
        /// <param name="movimiento"></param>
        /// <returns></returns>
        public ErrorDto<
            AfRecepcionLiquidacionesTagLiquidacionResponse?>
            AF_frmAF_RecepcionLiquidacionesTag_Liquidacion_Obtener(
                int codEmpresa,
                long numeroBoleta,
                string? movimiento)
        {
            if (numeroBoleta <= 0)
            {
                return DbHelper.CreateErrorResponse<
                    AfRecepcionLiquidacionesTagLiquidacionResponse?>(
                        "El n&uacute;mero de boleta no es v&aacute;lido.",
                        -2,
                        null);
            }

            string movimientoNormalizado =
                AF_frmAF_RecepcionLiquidacionesTag_Movimiento_Normalizar(
                    movimiento);

            if (string.IsNullOrWhiteSpace(
                    movimientoNormalizado))
            {
                return DbHelper.CreateErrorResponse<
                    AfRecepcionLiquidacionesTagLiquidacionResponse?>(
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
                    AF_frmAF_RecepcionLiquidacionesTag_Tags_Obtener(
                        connection,
                        null);

                string? validacion =
                    AF_frmAF_RecepcionLiquidacionesTag_Movimiento_Validar(
                        connection,
                        null,
                        numeroBoleta,
                        movimientoNormalizado,
                        tags);

                if (!string.IsNullOrWhiteSpace(validacion))
                {
                    return DbHelper.CreateErrorResponse<
                        AfRecepcionLiquidacionesTagLiquidacionResponse?>(
                            validacion,
                            -2,
                            null);
                }

                var liquidacion =
                    AF_frmAF_RecepcionLiquidacionesTag_Liquidacion_Consultar(
                        connection,
                        null,
                        numeroBoleta);

                if (liquidacion is null)
                {
                    return DbHelper.CreateErrorResponse<
                        AfRecepcionLiquidacionesTagLiquidacionResponse?>(
                            "No se encontr&oacute; la liquidaci&oacute;n indicada.",
                            -2,
                            null);
                }

                return DbHelper.CreateOkResponse<
                    AfRecepcionLiquidacionesTagLiquidacionResponse?>(
                        liquidacion);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<
                    AfRecepcionLiquidacionesTagLiquidacionResponse?>(
                        ex.Message,
                        -2,
                        null);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<
                    AfRecepcionLiquidacionesTagLiquidacionResponse?>(
                        ex.Message,
                        -1,
                        null);
            }
        }

        /// <summary>
        /// Obtiene las liquidaciones pendientes de recepcion
        /// o devolucion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<
            AfRecepcionLiquidacionesTagPendienteResponse>>
            AF_frmAF_RecepcionLiquidacionesTag_Pendientes_Obtener(
                int codEmpresa,
                AfRecepcionLiquidacionesTagPendientesRequest?
                    request)
        {
            request ??=
                new AfRecepcionLiquidacionesTagPendientesRequest();

            string movimiento =
                AF_frmAF_RecepcionLiquidacionesTag_Movimiento_Normalizar(
                    request.movimiento);

            if (string.IsNullOrWhiteSpace(movimiento))
            {
                return DbHelper.CreateErrorResponse(
                    "El tipo de movimiento no es v&aacute;lido.",
                    -2,
                    new List<
                        AfRecepcionLiquidacionesTagPendienteResponse>());
            }

            int estado =
                movimiento == MovimientoRecepcion
                    ? 0
                    : 2;

            const string sql = """
                select
                    L.CONSEC as consec,
                    isnull(
                        rtrim(L.CEDULA),
                        ''
                    ) as cedula,
                    isnull(
                        rtrim(S.NOMBRE),
                        ''
                    ) as nombre,
                    isnull(
                        rtrim(O.DESCRIPCION),
                        ''
                    ) as descripcion,
                    L.FECLIQ as fecliq,
                    isnull(
                        rtrim(L.USUARIO),
                        ''
                    ) as usuario
                from LIQUIDACION L
                inner join SOCIOS S
                    on L.CEDULA = S.CEDULA
                left join SIF_OFICINAS O
                    on L.COD_OFICINA =
                       O.COD_OFICINA
                where isnull(
                    L.ANALISTA_RECEPCION,
                    0
                ) = @Estado
                order by
                    L.FECLIQ desc,
                    L.CONSEC desc;
                """;

            return DbHelper.ExecuteListQuery<
                AfRecepcionLiquidacionesTagPendienteResponse>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        Estado = estado
                    });
        }

        /// <summary>
        /// Registra los tags de recepcion o devolucion para
        /// las liquidaciones seleccionadas.
        /// </summary>
        public ErrorDto<
            AfRecepcionLiquidacionesTagAplicarResponse>
            AF_frmAF_RecepcionLiquidacionesTag_Movimiento_Aplicar(
                int codEmpresa,
                AfRecepcionLiquidacionesTagAplicarRequest?
                    request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los datos del proceso son requeridos.",
                    -2,
                    new AfRecepcionLiquidacionesTagAplicarResponse());
            }

            string movimiento =
                AF_frmAF_RecepcionLiquidacionesTag_Movimiento_Normalizar(
                    request.movimiento);

            if (string.IsNullOrWhiteSpace(movimiento))
            {
                return DbHelper.CreateErrorResponse(
                    "El tipo de movimiento no es v&aacute;lido.",
                    -2,
                    new AfRecepcionLiquidacionesTagAplicarResponse());
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse(
                    "El usuario es requerido.",
                    -2,
                    new AfRecepcionLiquidacionesTagAplicarResponse());
            }

            if (
                request.boletas is null ||
                request.boletas.Count == 0
            )
            {
                return DbHelper.CreateErrorResponse(
                    "Debe seleccionar al menos una boleta.",
                    -2,
                    new AfRecepcionLiquidacionesTagAplicarResponse());
            }

            if (
                request.boletas.Any(
                    numeroBoleta =>
                        numeroBoleta <= 0)
            )
            {
                return DbHelper.CreateErrorResponse(
                    "La lista contiene n&uacute;meros de boleta no v&aacute;lidos.",
                    -2,
                    new AfRecepcionLiquidacionesTagAplicarResponse());
            }

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
                        AF_frmAF_RecepcionLiquidacionesTag_Tags_Obtener(
                            connection,
                            transaction);

                    string tag =
                        movimiento == MovimientoRecepcion
                            ? tags.tag_recepcion
                            : tags.tag_devolucion;

                    string observacion =
                        movimiento == MovimientoRecepcion
                            ? "Recibida la documentacion de la liquidacion"
                            : "Devolucion la documentacion de la liquidacion";

                    int aplicados = 0;

                    foreach (
                        long numeroBoleta in
                        request.boletas.Distinct())
                    {
                        var liquidacion =
                            AF_frmAF_RecepcionLiquidacionesTag_Liquidacion_Consultar(
                                connection,
                                transaction,
                                numeroBoleta);

                        if (liquidacion is null)
                        {
                            transaction.Rollback();

                            return DbHelper.CreateErrorResponse(
                                $"No se encontr&oacute; la liquidaci&oacute;n {numeroBoleta}.",
                                -2,
                                new AfRecepcionLiquidacionesTagAplicarResponse());
                        }

                        string? validacion =
                            AF_frmAF_RecepcionLiquidacionesTag_Movimiento_Validar(
                                connection,
                                transaction,
                                numeroBoleta,
                                movimiento,
                                tags);

                        if (!string.IsNullOrWhiteSpace(
                                validacion))
                        {
                            transaction.Rollback();

                            return DbHelper.CreateErrorResponse(
                                validacion,
                                -2,
                                new AfRecepcionLiquidacionesTagAplicarResponse());
                        }

                        connection.Execute(
                            "spSIFRegistraTags",
                            new
                            {
                                Codigo =
                                    liquidacion.cedula,
                                Tag = tag,
                                Usuario =
                                    request.usuario.Trim(),
                                Notas =
                                    observacion,
                                Documento =
                                    numeroBoleta.ToString(),
                                Modulo,
                                Llave_01 =
                                    numeroBoleta.ToString(),
                                Llave_02 =
                                    string.Empty,
                                Llave_03 =
                                    string.Empty
                            },
                            transaction,
                            commandType:
                                CommandType.StoredProcedure);

                        aplicados++;
                    }

                    transaction.Commit();

                    return DbHelper.CreateOkResponse(
                        new AfRecepcionLiquidacionesTagAplicarResponse
                        {
                            registros_aplicados =
                                aplicados
                        },
                        "Proceso concluido con exito.");
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -2,
                    new AfRecepcionLiquidacionesTagAplicarResponse());
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new AfRecepcionLiquidacionesTagAplicarResponse());
            }
        }

        /// <summary>
        /// Obtiene el historial de tags de una boleta.
        /// </summary>
        public ErrorDto<List<
            AfRecepcionLiquidacionesTagHistorialResponse>>
            AF_frmAF_RecepcionLiquidacionesTag_Historial_Obtener(
                int codEmpresa,
                AfRecepcionLiquidacionesTagHistorialRequest?
                    request)
        {
            if (
                request is null ||
                (
                    request.documento is null &&
                    string.IsNullOrWhiteSpace(
                        request.usuario) &&
                    request.fecha_inicio is null &&
                    request.fecha_fin is null
                )
            )
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar al menos un criterio de consulta.",
                    -2,
                    new List<
                        AfRecepcionLiquidacionesTagHistorialResponse>());
            }

            if (
                request.documento.HasValue &&
                request.documento.Value <= 0
            )
            {
                return DbHelper.CreateErrorResponse(
                    "El n&uacute;mero de boleta no es v&aacute;lido.",
                    -2,
                    new List<
                        AfRecepcionLiquidacionesTagHistorialResponse>());
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
                        AfRecepcionLiquidacionesTagHistorialResponse>());
            }

            const string sql = """
                select
                    isnull(
                        rtrim(T.DESCRIPCION),
                        ''
                    ) as descripcion,
                    CT.REGISTRO_FECHA
                        as registro_fecha,
                    isnull(
                        rtrim(CT.REGISTRO_USUARIO),
                        ''
                    ) as registro_usuario
                from SIF_CONTROL_TAGS CT
                inner join SIF_TAGS T
                    on CT.TAG_CODIGO =
                       T.TAG_CODIGO
                where CT.COD_MODULO =
                      @Modulo
                  and (
                      @Documento is null
                      or CT.DOCUMENTO =
                         convert(varchar(30), @Documento)
                  )
                  and (
                      @Usuario = ''
                      or CT.REGISTRO_USUARIO =
                         @Usuario
                  )
                  and (
                      @FechaInicio is null
                      or CT.REGISTRO_FECHA >=
                         @FechaInicio
                  )
                  and (
                      @FechaFin is null
                      or CT.REGISTRO_FECHA <
                         dateadd(day, 1, @FechaFin)
                  )
                order by
                    CT.REGISTRO_FECHA desc;
                """;

            return DbHelper.ExecuteListQuery<
                AfRecepcionLiquidacionesTagHistorialResponse>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        Documento =
                            request.documento,
                        Usuario =
                            request.usuario.Trim(),
                        FechaInicio =
                            request.fecha_inicio?.Date,
                        FechaFin =
                            request.fecha_fin?.Date,
                        Modulo
                    });
        }

        private static
            AfRecepcionLiquidacionesTagLiquidacionResponse?
            AF_frmAF_RecepcionLiquidacionesTag_Liquidacion_Consultar(
                SqlConnection connection,
                SqlTransaction? transaction,
                long numeroBoleta)
        {
            const string sql = """
                select top 1
                    isnull(
                        rtrim(L.CEDULA),
                        ''
                    ) as cedula,
                    isnull(
                        rtrim(S.NOMBRE),
                        ''
                    ) as nombre,
                    isnull(
                        rtrim(O.DESCRIPCION),
                        ''
                    ) as descripcion,
                    L.CONSEC as consec
                from LIQUIDACION L
                inner join SOCIOS S
                    on L.CEDULA = S.CEDULA
                left join SIF_OFICINAS O
                    on L.COD_OFICINA =
                       O.COD_OFICINA
                where L.CONSEC =
                      @NumeroBoleta;
                """;

            return connection.QueryFirstOrDefault<
                AfRecepcionLiquidacionesTagLiquidacionResponse>(
                    sql,
                    new
                    {
                        NumeroBoleta = numeroBoleta
                    },
                    transaction);
        }

        private static TagsConfiguracion
            AF_frmAF_RecepcionLiquidacionesTag_Tags_Obtener(
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
                    from SIF_PARAMETROS
                    where COD_PARAMETRO
                        in ('10', '11', '12');
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
                        ObtenerValor("10"),
                    tag_devolucion =
                        ObtenerValor("11"),
                    tag_recepcion_devolucion =
                        ObtenerValor("12")
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

            if (string.IsNullOrWhiteSpace(
                    tags.tag_recepcion_devolucion))
            {
                throw new InvalidOperationException(
                    "No est&aacute; definida la etiqueta de recepci&oacute;n de devoluci&oacute;n.");
            }

            int tagsExistentes =
                connection.ExecuteScalar<int>(
                    """
                    select
                        case
                            when exists (
                                select 1
                                from SIF_TAGS
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
                                from SIF_TAGS
                                where TAG_CODIGO =
                                      @TagDevolucion
                            )
                            then 1
                            else 0
                        end
                        +
                        case
                            when exists (
                                select 1
                                from SIF_TAGS
                                where TAG_CODIGO =
                                      @TagRecepcionDevolucion
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
                            tags.tag_devolucion,
                        TagRecepcionDevolucion =
                            tags.tag_recepcion_devolucion
                    },
                    transaction);

            if (tagsExistentes != 3)
            {
                throw new InvalidOperationException(
                    "Uno o m&aacute;s tags configurados no existen.");
            }

            return tags;
        }

        private static string?
            AF_frmAF_RecepcionLiquidacionesTag_Movimiento_Validar(
                SqlConnection connection,
                SqlTransaction? transaction,
                long numeroBoleta,
                string movimiento,
                TagsConfiguracion tags)
        {
            int resultado =
                connection.ExecuteScalar<int>(
                    """
                    select dbo.fxSIFValidaTagRev(
                        @NumeroBoleta,
                        @TagRecepcion,
                        @TagDevolucion,
                        @Modulo,
                        null,
                        null
                    );
                    """,
                    new
                    {
                        NumeroBoleta =
                            numeroBoleta,
                        TagRecepcion =
                            tags.tag_recepcion,
                        TagDevolucion =
                            tags.tag_devolucion,
                        Modulo
                    },
                    transaction);

            if (
                movimiento == MovimientoRecepcion &&
                resultado == 2
            )
            {
                return "No es posible registrar consecutivamente "
                    + "dos recepciones en la boleta "
                    + $"{numeroBoleta}.";
            }

            if (
                movimiento == MovimientoDevolucion &&
                resultado == 3
            )
            {
                return "No es posible registrar consecutivamente "
                    + "dos devoluciones en la boleta "
                    + $"{numeroBoleta}.";
            }

            int resultadoRecepcionDevolucion =
                connection.ExecuteScalar<int>(
                    """
                    select dbo.fxSIFValidaTagRev(
                        @NumeroBoleta,
                        @TagRecepcion,
                        @TagDevolucion,
                        @Modulo,
                        null,
                        @TagRecepcionDevolucion
                    );
                    """,
                    new
                    {
                        NumeroBoleta =
                            numeroBoleta,
                        TagRecepcion =
                            tags.tag_recepcion,
                        TagDevolucion =
                            tags.tag_devolucion,
                        Modulo,
                        TagRecepcionDevolucion =
                            tags.tag_recepcion_devolucion
                    },
                    transaction);

            if (resultadoRecepcionDevolucion == 4)
            {
                return "No es posible registrar una recepcion "
                    + "sin aplicar la devolucion en la boleta "
                    + $"{numeroBoleta}.";
            }

            return null;
        }

        private static string
            AF_frmAF_RecepcionLiquidacionesTag_Movimiento_Normalizar(
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
            public string cod_parametro { get; set; } = string.Empty;

            public string valor { get; set; } = string.Empty;
        }

        private sealed class TagsConfiguracion
        {
            public string tag_recepcion { get; set; } = string.Empty;

            public string tag_devolucion { get; set; } = string.Empty;

            public string tag_recepcion_devolucion
            {
                get;
                set;
            } = string.Empty;
        }
    }
}