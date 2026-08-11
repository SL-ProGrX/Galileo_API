using System.Data;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_ControlTramites;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_ControlTramites
{
    public sealed class FrmFNDRecepcionLiqFondosTagsDb
    {
        private const string Modulo = "FLQ";
        private const string MovimientoRecepcion = "RECEPCION";
        private const string MovimientoDevolucion = "DEVOLUCION";

        private readonly PortalDB _portalDb;

        public FrmFNDRecepcionLiqFondosTagsDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene los parametros de tags y los usuarios activos requeridos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<FndRecepcionLiqFondosTagsInicializarResponse>
            FND_frmFNDRecepcionLiqFondosTags_Inicializar(
                int codEmpresa)
        {
            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDb, codEmpresa);

                connection.Open();

                var tags = FND_frmFNDRecepcionLiqFondosTags_Tags_Obtener(
                    connection,
                    null);

                var usuarios = connection.Query<
                    DropDownListaGenericaModel>(
                    """
                    select
                        upper(rtrim(nombre)) as item,
                        upper(rtrim(nombre)) as descripcion
                    from Usuarios
                    where estado = 'A'
                    order by nombre;
                    """).ToList();

                var response =
                    new FndRecepcionLiqFondosTagsInicializarResponse
                    {
                        tag_recepcion = tags.tag_recepcion,
                        tag_devolucion = tags.tag_devolucion,
                        tag_recepcion_devolucion =
                            tags.tag_recepcion_devolucion,
                        usuarios = usuarios
                    };

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new FndRecepcionLiqFondosTagsInicializarResponse());
            }
        }

        /// <summary>
        /// Valida y obtiene la liquidacion asociada con una boleta.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="numeroBoleta"></param>
        /// <param name="movimiento"></param>
        /// <returns></returns>
        public ErrorDto<FndRecepcionLiqFondosTagsBoletaResponse?>
            FND_frmFNDRecepcionLiqFondosTags_Boleta_Obtener(
                int codEmpresa,
                long numeroBoleta,
                string? movimiento)
        {
            if (numeroBoleta <= 0)
            {
                return DbHelper.CreateErrorResponse<
                    FndRecepcionLiqFondosTagsBoletaResponse?>(
                    "El n&uacute;mero de boleta no es v&aacute;lido.",
                    -2,
                    null);
            }

            string movimientoNormalizado =
                FND_frmFNDRecepcionLiqFondosTags_Movimiento_Normalizar(
                    movimiento);

            if (string.IsNullOrEmpty(movimientoNormalizado))
            {
                return DbHelper.CreateErrorResponse<
                    FndRecepcionLiqFondosTagsBoletaResponse?>(
                    "El tipo de movimiento no es v&aacute;lido.",
                    -2,
                    null);
            }

            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDb, codEmpresa);

                connection.Open();

                var tags = FND_frmFNDRecepcionLiqFondosTags_Tags_Obtener(
                    connection,
                    null);

                string? validacion =
                    FND_frmFNDRecepcionLiqFondosTags_Movimiento_Validar(
                        connection,
                        null,
                        numeroBoleta,
                        movimientoNormalizado,
                        tags);

                if (!string.IsNullOrWhiteSpace(validacion))
                {
                    return DbHelper.CreateErrorResponse<
                        FndRecepcionLiqFondosTagsBoletaResponse?>(
                        validacion,
                        -2,
                        null);
                }

                var boleta = connection.QueryFirstOrDefault<
                    FndRecepcionLiqFondosTagsBoletaResponse>(
                    """
                    select top 1
                        L.consec,
                        rtrim(L.cod_plan) as cod_plan,
                        rtrim(L.cod_contrato) as cod_contrato,
                        rtrim(F.cedula) as cedula,
                        rtrim(S.nombre) as nombre,
                        isnull(rtrim(O.descripcion), '') as descripcion
                    from Fnd_Liquidacion L
                    inner join FND_Contratos F
                        on L.cod_plan = F.cod_plan
                        and L.cod_contrato = F.cod_contrato
                        and L.cod_operadora = F.cod_operadora
                    inner join Socios S
                        on F.cedula = S.cedula
                    left join SIF_Oficinas O
                        on L.cod_oficina = O.cod_oficina
                    where L.consec = @NumeroBoleta;
                    """,
                    new
                    {
                        NumeroBoleta = numeroBoleta
                    });

                if (boleta is null)
                {
                    return DbHelper.CreateErrorResponse<
                        FndRecepcionLiqFondosTagsBoletaResponse?>(
                        "No se encontr&oacute; la liquidaci&oacute;n indicada.",
                        -2,
                        null);
                }

                return DbHelper.CreateOkResponse<
                    FndRecepcionLiqFondosTagsBoletaResponse?>(
                    boleta);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<
                    FndRecepcionLiqFondosTagsBoletaResponse?>(
                    ex.Message,
                    -1,
                    null);
            }
        }

        /// <summary>
        /// Consulta las liquidaciones pendientes de recepción o devolución.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<
            FndRecepcionLiqFondosTagsPendienteResponse>>
            FND_frmFNDRecepcionLiqFondosTags_Pendientes_Obtener(
                int codEmpresa,
                FndRecepcionLiqFondosTagsPendientesRequest? request)
        {
            request ??=
                new FndRecepcionLiqFondosTagsPendientesRequest();

            string movimiento =
                FND_frmFNDRecepcionLiqFondosTags_Movimiento_Normalizar(
                    request.movimiento);

            if (string.IsNullOrEmpty(movimiento))
            {
                return DbHelper.CreateErrorResponse(
                    "El tipo de movimiento no es v&aacute;lido.",
                    -2,
                    new List<
                        FndRecepcionLiqFondosTagsPendienteResponse>());
            }

            string caso =
                FND_frmFNDRecepcionLiqFondosTags_Caso_Normalizar(
                    request.caso);

            if (string.IsNullOrEmpty(caso))
            {
                return DbHelper.CreateErrorResponse(
                    "El tipo de caso no es v&aacute;lido.",
                    -2,
                    new List<
                        FndRecepcionLiqFondosTagsPendienteResponse>());
            }

            const string sql = """
                select
                    L.consec,
                    rtrim(L.cod_plan) as cod_plan,
                    rtrim(L.cod_contrato) as cod_contrato,
                    rtrim(F.cedula) as cedula,
                    rtrim(S.nombre) as nombre,
                    isnull(rtrim(O.descripcion), '') as descripcion,
                    L.fecha,
                    rtrim(L.usuario) as usuario
                from Fnd_Liquidacion L
                inner join FND_Contratos F
                    on L.cod_plan = F.cod_plan
                    and L.cod_contrato = F.cod_contrato
                    and L.cod_operadora = F.cod_operadora
                inner join Socios S
                    on F.cedula = S.cedula
                left join SIF_Oficinas O
                    on L.cod_oficina = O.cod_oficina
                where isnull(L.analista_recepcion, 0) =
                    case
                        when @Movimiento = 'RECEPCION' then 0
                        else 2
                    end
                  and (
                      @Caso = 'TODOS'
                      or (
                          @Caso = 'DESEMBOLSO'
                          and L.retencion_codigo is null
                      )
                      or (
                          @Caso = 'RETENIDOS'
                          and L.retencion_codigo is not null
                      )
                  )
                  and (
                      @Usuario = ''
                      or L.usuario like '%' + @Usuario + '%'
                  )
                order by L.fecha desc, L.consec desc;
                """;

            return DbHelper.ExecuteListQuery<
                FndRecepcionLiqFondosTagsPendienteResponse>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        Movimiento = movimiento,
                        Caso = caso,
                        Usuario = request.usuario?.Trim()
                            ?? string.Empty
                    });
        }

        /// <summary>
        /// Registra las etiquetas de recepcion o devolucion para las
        /// liquidaciones seleccionadas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<FndRecepcionLiqFondosTagsAplicarResponse>
            FND_frmFNDRecepcionLiqFondosTags_Movimiento_Aplicar(
                int codEmpresa,
                FndRecepcionLiqFondosTagsAplicarRequest? request)
        {
            var validacionRequest =
                FND_frmFNDRecepcionLiqFondosTags_Aplicar_Validar(
                    request);

            if (validacionRequest is not null)
            {
                return validacionRequest;
            }

            string movimiento =
                FND_frmFNDRecepcionLiqFondosTags_Movimiento_Normalizar(
                    request!.movimiento);

            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDb, codEmpresa);

                connection.Open();

                using var transaction =
                    connection.BeginTransaction();

                try
                {
                    var tags =
                        FND_frmFNDRecepcionLiqFondosTags_Tags_Obtener(
                            connection,
                            transaction);

                    string tag =
                        movimiento == MovimientoRecepcion
                            ? tags.tag_recepcion
                            : tags.tag_devolucion;

                    string observacion =
                        movimiento == MovimientoRecepcion
                            ? "Recibida la documentaci&oacute;n de la Liquidaci&oacute;n"
                            : "Devoluci&oacute;n la documentaci&oacute;n de la Liquidaci&oacute;n";

                    int aplicados = 0;

                    foreach (long consecutivo in
                        request.consecutivos.Distinct())
                    {
                        var boleta = connection.QueryFirstOrDefault<
                            FndRecepcionLiqFondosTagsBoletaResponse>(
                            """
                            select top 1
                                L.consec,
                                rtrim(L.cod_plan) as cod_plan,
                                rtrim(L.cod_contrato) as cod_contrato,
                                rtrim(F.cedula) as cedula,
                                rtrim(S.nombre) as nombre,
                                isnull(
                                    rtrim(O.descripcion),
                                    ''
                                ) as descripcion
                            from Fnd_Liquidacion L
                            inner join FND_Contratos F
                                on L.cod_plan = F.cod_plan
                                and L.cod_contrato = F.cod_contrato
                                and L.cod_operadora =
                                    F.cod_operadora
                            inner join Socios S
                                on F.cedula = S.cedula
                            left join SIF_Oficinas O
                                on L.cod_oficina = O.cod_oficina
                            where L.consec = @Consecutivo;
                            """,
                            new
                            {
                                Consecutivo = consecutivo
                            },
                            transaction);

                        if (boleta is null)
                        {
                            throw new InvalidOperationException(
                                $"No se encontro la liquidacion {consecutivo}.");
                        }

                        string? validacion =
                            FND_frmFNDRecepcionLiqFondosTags_Movimiento_Validar(
                                connection,
                                transaction,
                                consecutivo,
                                movimiento,
                                tags);

                        if (!string.IsNullOrWhiteSpace(validacion))
                        {
                            throw new InvalidOperationException(
                                validacion);
                        }

                        connection.Execute(
                            "spSIFRegistraTags",
                            new
                            {
                                Codigo = boleta.cedula,
                                Tag = tag,
                                Usuario = request.usuario.Trim(),
                                Observacion = observacion,
                                Documento =
                                    consecutivo.ToString(),
                                Modulo,
                                Llave_01 =
                                    consecutivo.ToString(),
                                Llave_02 = string.Empty,
                                Llave_03 = string.Empty
                            },
                            transaction,
                            commandType:
                                CommandType.StoredProcedure);

                        aplicados++;
                    }

                    transaction.Commit();

                    return DbHelper.CreateOkResponse(
                        new FndRecepcionLiqFondosTagsAplicarResponse
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
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new FndRecepcionLiqFondosTagsAplicarResponse());
            }
        }

        /// <summary>
        /// Obtiene el historial de tags aplicando filtros opcionales.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<
            FndRecepcionLiqFondosTagsHistorialResponse>>
            FND_frmFNDRecepcionLiqFondosTags_Historial_Obtener(
                int codEmpresa,
                FndRecepcionLiqFondosTagsHistorialRequest? request)
        {
            request ??=
                new FndRecepcionLiqFondosTagsHistorialRequest();

            string usuario =
                request.usuario?.Trim()
                ?? string.Empty;

            if (
                request.numero_boleta.HasValue &&
                request.numero_boleta.Value <= 0
            )
            {
                return DbHelper.CreateErrorResponse(
                    "El n&uacute;mero de boleta no es v&aacute;lido.",
                    -2,
                    new List<
                        FndRecepcionLiqFondosTagsHistorialResponse>());
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
                        FndRecepcionLiqFondosTagsHistorialResponse>());
            }

            const string sql = """
            select
                rtrim(T.descripcion) as descripcion,
                isnull(rtrim(CT.notas), '') as notas,
                CT.registro_fecha,
                isnull(
                    rtrim(CT.registro_usuario),
                    ''
                ) as registro_usuario
            from SIF_Control_Tags CT
            inner join SIF_Tags T
                on CT.tag_codigo = T.tag_codigo
            where CT.cod_modulo = @Modulo
              and (
                  @NumeroBoleta is null
                  or CT.documento =
                      convert(varchar(30), @NumeroBoleta)
              )
              and (
                  @Usuario = ''
                  or CT.registro_usuario like
                      '%' + @Usuario + '%'
              )
              and (
                  @FechaInicio is null
                  or CT.registro_fecha >= @FechaInicio
              )
              and (
                  @FechaFin is null
                  or CT.registro_fecha <
                      dateadd(
                          day,
                          1,
                          convert(date, @FechaFin)
                      )
              )
            order by CT.registro_fecha desc;
            """;

            return DbHelper.ExecuteListQuery<
                FndRecepcionLiqFondosTagsHistorialResponse>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        NumeroBoleta =
                            request.numero_boleta,
                        Usuario = usuario,
                        FechaInicio =
                            request.fecha_inicio?.Date,
                        FechaFin =
                            request.fecha_fin?.Date,
                        Modulo
                    });
        }

        private static TagsConfiguracion
            FND_frmFNDRecepcionLiqFondosTags_Tags_Obtener(
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

            string ObtenerValor(string codigo)
            {
                return parametros
                    .FirstOrDefault(item =>
                        item.cod_parametro == codigo)
                    ?.valor
                    ?? string.Empty;
            }

            var tags = new TagsConfiguracion
            {
                tag_recepcion = ObtenerValor("10"),
                tag_devolucion = ObtenerValor("11"),
                tag_recepcion_devolucion =
                    ObtenerValor("12")
            };

            if (string.IsNullOrWhiteSpace(tags.tag_recepcion))
            {
                throw new InvalidOperationException(
                    "No est&aacute; definida la etiqueta de recepci&oacute;n.");
            }

            if (string.IsNullOrWhiteSpace(tags.tag_devolucion))
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

            int tagsExistentes = connection.ExecuteScalar<int>(
            """
            select
                case
                    when exists (
                        select 1
                        from SIF_Tags
                        where tag_codigo = @TagRecepcion
                    )
                    then 1
                    else 0
                end
                +
                case
                    when exists (
                        select 1
                        from SIF_Tags
                        where tag_codigo = @TagDevolucion
                    )
                    then 1
                    else 0
                end
                +
                case
                    when exists (
                        select 1
                        from SIF_Tags
                        where tag_codigo = @TagRecepcionDevolucion
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
            FND_frmFNDRecepcionLiqFondosTags_Movimiento_Validar(
                SqlConnection connection,
                SqlTransaction? transaction,
                long numeroBoleta,
                string movimiento,
                TagsConfiguracion tags)
        {
            int resultado = connection.ExecuteScalar<int>(
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
                    NumeroBoleta = numeroBoleta,
                    TagRecepcion = tags.tag_recepcion,
                    TagDevolucion = tags.tag_devolucion,
                    Modulo
                },
                transaction);

            if (movimiento == MovimientoRecepcion
                && resultado == 2)
            {
                return "No es posible registrar consecutivamente "
                    + $"dos recepciones en la boleta {numeroBoleta}.";
            }

            if (movimiento == MovimientoDevolucion
                && resultado == 3)
            {
                return "No es posible registrar consecutivamente "
                    + $"dos devoluciones en la boleta {numeroBoleta}.";
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
                        NumeroBoleta = numeroBoleta,
                        TagRecepcion = tags.tag_recepcion,
                        TagDevolucion = tags.tag_devolucion,
                        Modulo,
                        TagRecepcionDevolucion =
                            tags.tag_recepcion_devolucion
                    },
                    transaction);

            if (resultadoRecepcionDevolucion == 4)
            {
                return "No es posible registrar una recepci&oacute;n "
                    + "sin aplicar la devoluci&oacute;n en la boleta "
                    + $"{numeroBoleta}.";
            }

            return null;
        }

        private static ErrorDto<
            FndRecepcionLiqFondosTagsAplicarResponse>?
            FND_frmFNDRecepcionLiqFondosTags_Aplicar_Validar(
                FndRecepcionLiqFondosTagsAplicarRequest? request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los datos del proceso son requeridos.",
                    -2,
                    new FndRecepcionLiqFondosTagsAplicarResponse());
            }

            string movimiento =
                FND_frmFNDRecepcionLiqFondosTags_Movimiento_Normalizar(
                    request.movimiento);

            if (string.IsNullOrWhiteSpace(movimiento))
            {
                return DbHelper.CreateErrorResponse(
                    "El tipo de movimiento no es v&aacute;lido.",
                    -2,
                    new FndRecepcionLiqFondosTagsAplicarResponse());
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse(
                    "El usuario es requerido.",
                    -2,
                    new FndRecepcionLiqFondosTagsAplicarResponse());
            }

            if (
                request.consecutivos is null ||
                request.consecutivos.Count == 0
            )
            {
                return DbHelper.CreateErrorResponse(
                    "Debe seleccionar al menos una liquidaci&oacute;n.",
                    -2,
                    new FndRecepcionLiqFondosTagsAplicarResponse());
            }

            if (
                request.consecutivos.Any(
                    consecutivo => consecutivo <= 0)
            )
            {
                return DbHelper.CreateErrorResponse(
                    "La lista contiene n&uacute;meros de boleta no v&aacute;lidos.",
                    -2,
                    new FndRecepcionLiqFondosTagsAplicarResponse());
            }

            return null;
        }

        private static string
            FND_frmFNDRecepcionLiqFondosTags_Movimiento_Normalizar(
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

        private static string
            FND_frmFNDRecepcionLiqFondosTags_Caso_Normalizar(
                string? caso)
        {
            string valor =
                caso?.Trim().ToUpperInvariant()
                ?? string.Empty;

            return valor is "TODOS"
                or "DESEMBOLSO"
                or "RETENIDOS"
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