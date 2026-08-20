using System.Data;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_ControlTramites;

namespace Galileo_API.DataBaseTier.ProGrX_ControlTramites
{
    public sealed class FrmSifRecepcionNdNcDb
    {
        private const string Modulo = "DOC";
        private const string MovimientoRecepcion = "RECEPCION";
        private const string MovimientoDevolucion = "DEVOLUCION";
        private const string MensajeTipoDocumentoInvalido =
            "El tipo de documento no es v&aacute;lido.";

        private readonly PortalDB _portalDb;

        public FrmSifRecepcionNdNcDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene los parametros, tipos de documento, usuarios y fecha inicial.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<SifRecepcionNdNcInicializaData>
            SIF_RecepcionNdNc_Inicializar(int codEmpresa)
        {
            ErrorDto<TagsConfiguracion> tagsResultado =
                SIF_RecepcionNdNc_Tags_Obtener(codEmpresa);

            if (tagsResultado.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    tagsResultado.Description ??
                    "No fue posible obtener la configuraci&oacute;n de etiquetas.",
                    tagsResultado.Code.GetValueOrDefault(-1),
                    new SifRecepcionNdNcInicializaData());
            }

            const string sqlTiposDocumento = """
                select
                    rtrim(TIPO_DOCUMENTO) as item,
                    rtrim(DESCRIPCION) as descripcion
                from SIF_DOCUMENTOS
                where TIPO_DOCUMENTO in
                (
                    'NC',
                    'ND',
                    'FND',
                    'FNC',
                    'CA',
                    'CD.Liq',
                    'BEAC',
                    'CBJ',
                    'FSL',
                    'REA',
                    'RH',
                    'TCP',
                    'TRFA',
                    'THCJ',
                    'TRA',
                    'THAV'
                )
                order by DESCRIPCION;
                """;

            ErrorDto<List<DropDownListaGenericaModel>> tiposResultado =
                DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                    _portalDb,
                    codEmpresa,
                    sqlTiposDocumento);

            if (tiposResultado.Code == -1)
            {
                return DbHelper.CreateErrorResponse(
                    tiposResultado.Description ??
                    "No fue posible obtener los tipos de documento.",
                    -2,
                    new SifRecepcionNdNcInicializaData());
            }

            const string sqlUsuarios = """
                select
                    item,
                    descripcion
                from
                (
                    select
                        '' as item,
                        'TODOS' as descripcion,
                        0 as orden

                    union all

                    select
                        rtrim(NOMBRE) as item,
                        upper(rtrim(NOMBRE)) as descripcion,
                        1 as orden
                    from USUARIOS
                    where ESTADO = 'A'
                ) U
                order by
                    orden,
                    descripcion;
                """;

            ErrorDto<List<DropDownListaGenericaModel>> usuariosResultado =
                DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                    _portalDb,
                    codEmpresa,
                    sqlUsuarios);

            if (usuariosResultado.Code == -1)
            {
                return DbHelper.CreateErrorResponse(
                    usuariosResultado.Description ??
                    "No fue posible obtener los usuarios.",
                    -2,
                    new SifRecepcionNdNcInicializaData());
            }

            ErrorDto<DateTime?> fechaResultado =
                DbHelper.ExecuteSingleQuery<DateTime?>(
                    _portalDb,
                    codEmpresa,
                    "select Getdate();",
                    null);

            if (fechaResultado.Code == -1)
            {
                return DbHelper.CreateErrorResponse(
                    fechaResultado.Description ??
                    "No fue posible obtener la fecha del servidor.",
                    -2,
                    new SifRecepcionNdNcInicializaData());
            }

            TagsConfiguracion tags =
                tagsResultado.Result ?? new TagsConfiguracion();

            return DbHelper.CreateOkResponse(
                new SifRecepcionNdNcInicializaData
                {
                    tag_recepcion = tags.tag_recepcion,
                    tag_devolucion = tags.tag_devolucion,
                    tag_recepcion_devolucion =
                        tags.tag_recepcion_devolucion,
                    fecha_servidor = fechaResultado.Result,
                    tipos_documento = tiposResultado.Result ?? [],
                    usuarios = usuariosResultado.Result ?? []
                });
        }

        /// <summary>
        /// Obtiene los primeros 100 documentos para recepcion o devolucion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<SifRecepcionNdNcDocumentoData>>
            SIF_RecepcionNdNc_Documentos_Obtener(
                int codEmpresa,
                SifRecepcionNdNcDocumentosRequest? request)
        {
            if (request is null)
            {
                return CrearErrorLista(
                    "Los filtros de documentos son requeridos.");
            }

            string tipoDocumento =
                SIF_RecepcionNdNc_TipoDocumento_Normalizar(
                    request.tipo_documento);

            if (tipoDocumento == string.Empty)
            {
                return CrearErrorLista(
                    MensajeTipoDocumentoInvalido);
            }

            string movimiento =
                SIF_RecepcionNdNc_Movimiento_Normalizar(
                    request.movimiento);

            if (movimiento == string.Empty)
            {
                return CrearErrorLista(
                    "El tipo de movimiento no es v&aacute;lido.");
            }

            int estadoRecepcion =
                movimiento == MovimientoRecepcion ? 0 : 1;

            const string sql = """
                select top (100)
                    isnull(rtrim(T.COD_TRANSACCION), '')
                        as cod_transaccion,
                    isnull(rtrim(T.TIPO_DOCUMENTO), '')
                        as tipo_documento,
                    isnull(rtrim(T.CLIENTE_IDENTIFICACION), '')
                        as cliente_identificacion,
                    isnull(rtrim(T.CLIENTE_NOMBRE), '')
                        as cliente_nombre,
                    isnull(rtrim(T.REGISTRO_USUARIO), '')
                        as registro_usuario,
                    T.REGISTRO_FECHA as registro_fecha
                from SIF_TRANSACCIONES T
                where T.TIPO_DOCUMENTO = @TipoDocumento
                  and T.ANALISTA_REVISION is null
                  and isnull(T.ANALISTA_RECEPCION, 0) =
                      @EstadoRecepcion
                order by T.REGISTRO_FECHA desc;
                """;

            return DbHelper.ExecuteListQuery<SifRecepcionNdNcDocumentoData>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    TipoDocumento = tipoDocumento,
                    EstadoRecepcion = estadoRecepcion
                });
        }

        /// <summary>
        /// Obtiene los primeros 300 documentos pendientes de recepcion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<SifRecepcionNdNcDocumentoData>>
            SIF_RecepcionNdNc_Pendientes_Obtener(
                int codEmpresa,
                SifRecepcionNdNcPendientesRequest? request)
        {
            if (request is null)
            {
                return CrearErrorLista(
                    "Los filtros de pendientes son requeridos.");
            }

            string tipoDocumento =
                SIF_RecepcionNdNc_TipoDocumento_Normalizar(
                    request.tipo_documento);

            if (tipoDocumento == string.Empty)
            {
                return CrearErrorLista(
                    MensajeTipoDocumentoInvalido);
            }

            const string sql = """
                select top (300)
                    isnull(rtrim(T.COD_TRANSACCION), '')
                        as cod_transaccion,
                    isnull(rtrim(T.TIPO_DOCUMENTO), '')
                        as tipo_documento,
                    isnull(rtrim(T.CLIENTE_IDENTIFICACION), '')
                        as cliente_identificacion,
                    isnull(rtrim(T.CLIENTE_NOMBRE), '')
                        as cliente_nombre,
                    isnull(rtrim(T.REGISTRO_USUARIO), '')
                        as registro_usuario,
                    T.REGISTRO_FECHA as registro_fecha
                from SIF_TRANSACCIONES T
                where T.TIPO_DOCUMENTO = @TipoDocumento
                  and isnull(T.ANALISTA_RECEPCION, 0) = 0
                order by T.REGISTRO_FECHA desc;
                """;

            return DbHelper.ExecuteListQuery<SifRecepcionNdNcDocumentoData>(
                _portalDb,
                codEmpresa,
                sql,
                new { TipoDocumento = tipoDocumento });
        }

        /// <summary>
        /// Obtiene el historial de etiquetas de un documento.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<SifRecepcionNdNcConsultaData>>
            SIF_RecepcionNdNc_Consulta_Obtener(
                int codEmpresa,
                SifRecepcionNdNcConsultaRequest? request)
        {
            if (request is null)
            {
                return CrearErrorConsulta(
                    "Los filtros de consulta son requeridos.");
            }

            string tipoDocumento =
                SIF_RecepcionNdNc_TipoDocumento_Normalizar(
                    request.tipo_documento);

            if (tipoDocumento == string.Empty)
            {
                return CrearErrorConsulta(
                    MensajeTipoDocumentoInvalido);
            }

            string codTransaccion =
                NormalizarTexto(request.cod_transaccion);

            if (codTransaccion == string.Empty)
            {
                return CrearErrorConsulta(
                    "Debe indicar el n&uacute;mero de documento.");
            }

            if (
                request.fecha_inicio.HasValue &&
                request.fecha_fin.HasValue &&
                request.fecha_inicio.Value.Date >
                    request.fecha_fin.Value.Date
            )
            {
                return CrearErrorConsulta(
                    "La fecha inicial no puede ser mayor que la fecha final.");
            }

            const string sql = """
                select
                    isnull(rtrim(T.DESCRIPCION), '')
                        as descripcion,
                    isnull(rtrim(CT.NOTAS), '')
                        as notas,
                    CT.REGISTRO_FECHA as registro_fecha,
                    isnull(rtrim(CT.REGISTRO_USUARIO), '')
                        as registro_usuario
                from SIF_CONTROL_TAGS CT
                inner join SIF_TAGS T
                    on CT.TAG_CODIGO = T.TAG_CODIGO
                where CT.CODIGO = @TipoDocumento
                  and CT.COD_MODULO = @Modulo
                  and CT.DOCUMENTO = @CodTransaccion
                  and (
                      @Usuario = ''
                      or CT.REGISTRO_USUARIO = @Usuario
                  )
                  and (
                      @FechaInicio is null
                      or CT.REGISTRO_FECHA >= @FechaInicio
                  )
                  and (
                      @FechaFin is null
                      or CT.REGISTRO_FECHA <
                          dateadd(day, 1, @FechaFin)
                  )
                order by CT.REGISTRO_FECHA desc;
                """;

            return DbHelper.ExecuteListQuery<SifRecepcionNdNcConsultaData>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    TipoDocumento = tipoDocumento,
                    CodTransaccion = codTransaccion,
                    Usuario = NormalizarTexto(request.usuario),
                    FechaInicio = request.fecha_inicio?.Date,
                    FechaFin = request.fecha_fin?.Date,
                    Modulo
                });
        }

        /// <summary>
        /// Registra la recepcion o devolucion de los documentos seleccionados.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<int>
            SIF_RecepcionNdNc_Movimiento_Aplicar(
                int codEmpresa,
                SifRecepcionNdNcAplicarRequest? request)
        {
            if (request is null)
            {
                return CrearErrorAplicar(
                    "Los datos del proceso son requeridos.");
            }

            string? validacion =
                SIF_RecepcionNdNc_Aplicar_Validar(request);

            if (validacion is not null)
            {
                return CrearErrorAplicar(validacion);
            }

            ErrorDto<TagsConfiguracion> tagsResultado =
                SIF_RecepcionNdNc_Tags_Obtener(codEmpresa);

            if (tagsResultado.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    tagsResultado.Description ??
                    "No fue posible obtener la configuraci&oacute;n de etiquetas.",
                    tagsResultado.Code.GetValueOrDefault(-1),
                    0);
            }

            MovimientoAplicarContexto contexto =
                SIF_RecepcionNdNc_Movimiento_Contexto_Crear(
                    request,
                    tagsResultado.Result ?? new TagsConfiguracion());

            return SIF_RecepcionNdNc_Movimiento_Ejecutar(
                codEmpresa,
                contexto);
        }

        /// <summary>
        /// Construye la informacion normalizada necesaria para aplicar
        /// el movimiento.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        private static MovimientoAplicarContexto
            SIF_RecepcionNdNc_Movimiento_Contexto_Crear(
                SifRecepcionNdNcAplicarRequest request,
                TagsConfiguracion tags)
        {
            string movimiento =
                SIF_RecepcionNdNc_Movimiento_Normalizar(
                    request.movimiento);

            return new MovimientoAplicarContexto
            {
                TipoDocumento =
                    SIF_RecepcionNdNc_TipoDocumento_Normalizar(
                        request.tipo_documento),
                Movimiento = movimiento,
                Documentos = (request.documentos ?? [])
                    .Select(NormalizarTexto)
                    .Where(documento => documento != string.Empty)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                Usuario = NormalizarTexto(request.usuario),
                Tags = tags,
                Tag = movimiento == MovimientoRecepcion
                    ? tags.tag_recepcion
                    : tags.tag_devolucion,
                Observacion = movimiento == MovimientoRecepcion
                    ? "Recibida la documentacion de la liquidacion"
                    : "Devolucion la documentacion de la liquidacion"
            };
        }

        /// <summary>
        /// Ejecuta el movimiento dentro de una conexion controlada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="contexto"></param>
        /// <returns></returns>
        private ErrorDto<int>
            SIF_RecepcionNdNc_Movimiento_Ejecutar(
                int codEmpresa,
                MovimientoAplicarContexto contexto)
        {
            ErrorDto<ErrorDto<int>> ejecucion =
                DbHelper.WithConn(
                    _portalDb,
                    codEmpresa,
                    connection =>
                        SIF_RecepcionNdNc_Movimiento_Transaccion_Ejecutar(
                            connection,
                            contexto));

            if (ejecucion.Code == -1)
            {
                return DbHelper.CreateErrorResponse(
                    ejecucion.Description ??
                    "Ocurri&oacute; un error al aplicar el movimiento.",
                    -1,
                    0);
            }

            return ejecucion.Result ??
                DbHelper.CreateErrorResponse(
                    "No fue posible aplicar el movimiento.",
                    -1,
                    0);
        }

        /// <summary>
        /// Controla la transaccion utilizada para procesar los documentos.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="contexto"></param>
        /// <returns></returns>
        private static ErrorDto<int>
            SIF_RecepcionNdNc_Movimiento_Transaccion_Ejecutar(
                IDbConnection connection,
                MovimientoAplicarContexto contexto)
        {
            connection.Open();

            using IDbTransaction transaction =
                connection.BeginTransaction();

            try
            {
                ErrorDto<int> resultado =
                    SIF_RecepcionNdNc_Movimiento_Documentos_Procesar(
                        connection,
                        transaction,
                        contexto);

                if (resultado.Code != 0)
                {
                    transaction.Rollback();
                    return resultado;
                }

                transaction.Commit();
                return resultado;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Valida y registra cada documento incluido en el movimiento.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="contexto"></param>
        /// <returns></returns>
        private static ErrorDto<int>
            SIF_RecepcionNdNc_Movimiento_Documentos_Procesar(
                IDbConnection connection,
                IDbTransaction transaction,
                MovimientoAplicarContexto contexto)
        {
            int aplicados = 0;

            foreach (string codTransaccion in contexto.Documentos)
            {
                string? error =
                    SIF_RecepcionNdNc_Movimiento_Documento_Validar(
                        connection,
                        transaction,
                        contexto,
                        codTransaccion);

                if (error is not null)
                {
                    return CrearErrorAplicar(error);
                }

                SIF_RecepcionNdNc_Movimiento_Documento_Registrar(
                    connection,
                    transaction,
                    contexto,
                    codTransaccion);

                aplicados++;
            }

            return DbHelper.CreateOkResponse(
                aplicados,
                "Proceso concluido con exito.");
        }

        /// <summary>
        /// Valida la existencia y el estado de recepcion o devolucion
        /// de un documento.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="contexto"></param>
        /// <param name="codTransaccion"></param>
        /// <returns></returns>
        private static string?
            SIF_RecepcionNdNc_Movimiento_Documento_Validar(
                IDbConnection connection,
                IDbTransaction transaction,
                MovimientoAplicarContexto contexto,
                string codTransaccion)
        {
            bool existe =
                SIF_RecepcionNdNc_Movimiento_Documento_Existe(
                    connection,
                    transaction,
                    contexto.TipoDocumento,
                    codTransaccion);

            if (!existe)
            {
                return "No se encontr&oacute; el documento "
                    + $"{codTransaccion}.";
            }

            string? validacionMovimiento =
                SIF_RecepcionNdNc_Movimiento_Consecutivo_Validar(
                    connection,
                    transaction,
                    contexto,
                    codTransaccion);

            if (validacionMovimiento is not null)
            {
                return validacionMovimiento;
            }

            return SIF_RecepcionNdNc_Movimiento_Devolucion_Validar(
                connection,
                transaction,
                contexto,
                codTransaccion);
        }

        /// <summary>
        /// Verifica que el documento exista en SIF_TRANSACCIONES.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="tipoDocumento"></param>
        /// <param name="codTransaccion"></param>
        /// <returns></returns>
        private static bool
            SIF_RecepcionNdNc_Movimiento_Documento_Existe(
                IDbConnection connection,
                IDbTransaction transaction,
                string tipoDocumento,
                string codTransaccion)
        {
            const string sql = """
                select count(1)
                from SIF_TRANSACCIONES
                where TIPO_DOCUMENTO = @TipoDocumento
                  and COD_TRANSACCION = @CodTransaccion;
                """;

            int cantidad =
                connection.ExecuteScalar<int>(
                    sql,
                    new
                    {
                        TipoDocumento = tipoDocumento,
                        CodTransaccion = codTransaccion
                    },
                    transaction);

            return cantidad > 0;
        }

        /// <summary>
        /// Evita registrar dos recepciones o devoluciones consecutivas.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="contexto"></param>
        /// <param name="codTransaccion"></param>
        /// <returns></returns>
        private static string?
            SIF_RecepcionNdNc_Movimiento_Consecutivo_Validar(
                IDbConnection connection,
                IDbTransaction transaction,
                MovimientoAplicarContexto contexto,
                string codTransaccion)
        {
            string tagPrincipal =
                contexto.Movimiento == MovimientoRecepcion
                    ? contexto.Tags.tag_recepcion
                    : contexto.Tags.tag_devolucion;

            string tagAlterno =
                contexto.Movimiento == MovimientoRecepcion
                    ? contexto.Tags.tag_devolucion
                    : contexto.Tags.tag_recepcion;

            const string sql = """
                select dbo.fxSIFValidaTagRev
                (
                    @TipoDocumento,
                    @TagPrincipal,
                    @TagAlterno,
                    @Modulo,
                    @CodTransaccion,
                    null
                );
                """;

            int resultado =
                connection.ExecuteScalar<int>(
                    sql,
                    new
                    {
                        contexto.TipoDocumento,
                        TagPrincipal = tagPrincipal,
                        TagAlterno = tagAlterno,
                        Modulo,
                        CodTransaccion = codTransaccion
                    },
                    transaction);

            return (contexto.Movimiento, resultado) switch
            {
                (MovimientoRecepcion, 2) =>
                    "No es posible registrar en forma "
                    + "consecutiva dos recepciones del documento "
                    + $"{codTransaccion}.",

                (MovimientoDevolucion, 3) =>
                    "No es posible registrar en forma "
                    + "consecutiva dos devoluciones del documento "
                    + $"{codTransaccion}.",

                _ => null
            };
        }

        /// <summary>
        /// Valida que no exista una recepcion pendiente de devolucion.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="contexto"></param>
        /// <param name="codTransaccion"></param>
        /// <returns></returns>
        private static string?
            SIF_RecepcionNdNc_Movimiento_Devolucion_Validar(
                IDbConnection connection,
                IDbTransaction transaction,
                MovimientoAplicarContexto contexto,
                string codTransaccion)
        {
            const string sql = """
                select dbo.fxSIFValidaTagRev
                (
                    @TipoDocumento,
                    @TagDevolucion,
                    @TagRecepcion,
                    @Modulo,
                    @CodTransaccion,
                    @TagRecepcionDevolucion
                );
                """;

            int resultado =
                connection.ExecuteScalar<int>(
                    sql,
                    new
                    {
                        contexto.TipoDocumento,
                        TagDevolucion =
                            contexto.Tags.tag_devolucion,
                        TagRecepcion =
                            contexto.Tags.tag_recepcion,
                        Modulo,
                        CodTransaccion = codTransaccion,
                        TagRecepcionDevolucion =
                            contexto.Tags.tag_recepcion_devolucion
                    },
                    transaction);

            return resultado == 4
                ? "No es posible registrar una recepci&oacute;n "
                    + "sin aplicar la devoluci&oacute;n del documento "
                    + $"{codTransaccion}."
                : null;
        }

        /// <summary>
        /// Registra la etiqueta correspondiente al documento.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="contexto"></param>
        /// <param name="codTransaccion"></param>
        private static void
            SIF_RecepcionNdNc_Movimiento_Documento_Registrar(
                IDbConnection connection,
                IDbTransaction transaction,
                MovimientoAplicarContexto contexto,
                string codTransaccion)
        {
            connection.Execute(
                "spSIFRegistraTags",
                new
                {
                    Codigo = contexto.TipoDocumento,
                    contexto.Tag,
                    contexto.Usuario,
                    Notas = contexto.Observacion,
                    Documento = codTransaccion,
                    Modulo,
                    Llave_01 = contexto.TipoDocumento,
                    Llave_02 = codTransaccion,
                    Llave_03 = string.Empty
                },
                transaction,
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Obtiene y valida la configuracion de etiquetas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        private ErrorDto<TagsConfiguracion>
            SIF_RecepcionNdNc_Tags_Obtener(
                int codEmpresa)
        {
            const string sqlParametros = """
                select
                    rtrim(COD_PARAMETRO) as item,
                    isnull(rtrim(VALOR), '') as descripcion
                from SIF_PARAMETROS
                where COD_PARAMETRO in ('10', '11', '12');
                """;

            ErrorDto<List<DropDownListaGenericaModel<string>>>
                parametrosResultado =
                    DbHelper.ExecuteListQuery<
                        DropDownListaGenericaModel<string>>(
                            _portalDb,
                            codEmpresa,
                            sqlParametros);

            if (parametrosResultado.Code == -1)
            {
                return DbHelper.CreateErrorResponse(
                    parametrosResultado.Description ??
                    "No fue posible obtener los par&aacute;metros de etiquetas.",
                    -2,
                    new TagsConfiguracion());
            }

            List<DropDownListaGenericaModel<string>> parametros =
                parametrosResultado.Result ?? [];

            string ObtenerValor(string codigo)
            {
                return parametros
                    .FirstOrDefault(
                        parametro =>
                            string.Equals(
                                parametro.item,
                                codigo,
                                StringComparison.OrdinalIgnoreCase))
                    ?.descripcion
                    ?.Trim() ?? string.Empty;
            }

            var tags = new TagsConfiguracion
            {
                tag_recepcion = ObtenerValor("10"),
                tag_devolucion = ObtenerValor("11"),
                tag_recepcion_devolucion = ObtenerValor("12")
            };

            if (tags.tag_recepcion == string.Empty)
            {
                return CrearErrorTags(
                    "Falta agregar el par&aacute;metro 10 en la base de datos.");
            }

            if (tags.tag_devolucion == string.Empty)
            {
                return CrearErrorTags(
                    "Falta agregar el par&aacute;metro 11 en la base de datos.");
            }

            if (tags.tag_recepcion_devolucion == string.Empty)
            {
                return CrearErrorTags(
                    "Falta agregar el par&aacute;metro 12 en la base de datos.");
            }

            const string sqlValidar = """
                select
                    case
                        when exists
                        (
                            select 1
                            from SIF_TAGS
                            where TAG_CODIGO = @TagRecepcion
                        )
                        then 1
                        else 0
                    end as recepcion_existe,
                    case
                        when exists
                        (
                            select 1
                            from SIF_TAGS
                            where TAG_CODIGO = @TagDevolucion
                        )
                        then 1
                        else 0
                    end as devolucion_existe,
                    case
                        when exists
                        (
                            select 1
                            from SIF_TAGS
                            where TAG_CODIGO =
                                  @TagRecepcionDevolucion
                        )
                        then 1
                        else 0
                    end as recepcion_devolucion_existe;
                """;

            ErrorDto<TagsValidacion?> validacionResultado =
                DbHelper.ExecuteSingleQuery<TagsValidacion>(
                    _portalDb,
                    codEmpresa,
                    sqlValidar,
                    null,
                    new
                    {
                        TagRecepcion = tags.tag_recepcion,
                        TagDevolucion = tags.tag_devolucion,
                        TagRecepcionDevolucion =
                            tags.tag_recepcion_devolucion
                    });

            if (validacionResultado.Code == -1)
            {
                return DbHelper.CreateErrorResponse(
                    validacionResultado.Description ??
                    "No fue posible validar las etiquetas configuradas.",
                    -2,
                    new TagsConfiguracion());
            }

            TagsValidacion validacion =
                validacionResultado.Result ?? new TagsValidacion();

            if (validacion.recepcion_existe == 0)
            {
                return CrearErrorTags(
                    "El c&oacute;digo de tag definido en el "
                    + "par&aacute;metro 10 no existe.");
            }

            if (validacion.devolucion_existe == 0)
            {
                return CrearErrorTags(
                    "El c&oacute;digo de tag definido en el "
                    + "par&aacute;metro 11 no existe.");
            }

            if (validacion.recepcion_devolucion_existe == 0)
            {
                return CrearErrorTags(
                    "El c&oacute;digo de tag definido en el "
                    + "par&aacute;metro 12 no existe.");
            }

            return DbHelper.CreateOkResponse(tags);
        }

        /// <summary>
        /// Valida los datos requeridos para aplicar el movimiento.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string?
            SIF_RecepcionNdNc_Aplicar_Validar(
                SifRecepcionNdNcAplicarRequest request)
        {
            if (
                SIF_RecepcionNdNc_TipoDocumento_Normalizar(
                    request.tipo_documento) == string.Empty
            )
            {
                return MensajeTipoDocumentoInvalido;
            }

            if (
                SIF_RecepcionNdNc_Movimiento_Normalizar(
                    request.movimiento) == string.Empty
            )
            {
                return "El tipo de movimiento no es v&aacute;lido.";
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return "Debe indicar un usuario v&aacute;lido.";
            }

            if (
                request.documentos is null ||
                request.documentos.Count == 0
            )
            {
                return "Debe seleccionar al menos un documento.";
            }

            if (request.documentos.Any(string.IsNullOrWhiteSpace))
            {
                return "La selecci&oacute;n contiene documentos no v&aacute;lidos.";
            }

            return null;
        }

        private static string
            SIF_RecepcionNdNc_Movimiento_Normalizar(
                string? movimiento)
        {
            string valor =
                NormalizarTexto(movimiento).ToUpperInvariant();

            return valor is MovimientoRecepcion
                or MovimientoDevolucion
                    ? valor
                    : string.Empty;
        }

        private static string
            SIF_RecepcionNdNc_TipoDocumento_Normalizar(
                string? tipoDocumento)
        {
            string valor =
                NormalizarTexto(tipoDocumento).ToUpperInvariant();

            return valor switch
            {
                "NC" => "NC",
                "ND" => "ND",
                "FND" => "FND",
                "FNC" => "FNC",
                "CA" => "CA",
                "CD.LIQ" => "CD.Liq",
                "BEAC" => "BEAC",
                "CBJ" => "CBJ",
                "FSL" => "FSL",
                "REA" => "REA",
                "RH" => "RH",
                "TCP" => "TCP",
                "TRFA" => "TRFA",
                "THCJ" => "THCJ",
                "TRA" => "TRA",
                "THAV" => "THAV",
                _ => string.Empty
            };
        }

        private static ErrorDto<List<SifRecepcionNdNcDocumentoData>>
            CrearErrorLista(string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                -2,
                new List<SifRecepcionNdNcDocumentoData>());
        }

        private static ErrorDto<List<SifRecepcionNdNcConsultaData>>
            CrearErrorConsulta(string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                -2,
                new List<SifRecepcionNdNcConsultaData>());
        }

        private static ErrorDto<int> CrearErrorAplicar(
            string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                -2,
                0);
        }

        private static ErrorDto<TagsConfiguracion>
            CrearErrorTags(string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                -2,
                new TagsConfiguracion());
        }

        private static string NormalizarTexto(string? valor)
        {
            return valor?.Trim() ?? string.Empty;
        }

        private sealed class MovimientoAplicarContexto
        {
            public string TipoDocumento { get; set; } = string.Empty;
            public string Movimiento { get; set; } = string.Empty;
            public List<string> Documentos { get; set; } = [];
            public string Usuario { get; set; } = string.Empty;
            public TagsConfiguracion Tags { get; set; } = new();
            public string Tag { get; set; } = string.Empty;
            public string Observacion { get; set; } = string.Empty;
        }

        private sealed class TagsConfiguracion
        {
            public string tag_recepcion { get; set; } = string.Empty;
            public string tag_devolucion { get; set; } = string.Empty;
            public string tag_recepcion_devolucion { get; set; } =
                string.Empty;
        }

        private sealed class TagsValidacion
        {
            public int recepcion_existe { get; set; } = 0;
            public int devolucion_existe { get; set; } = 0;
            public int recepcion_devolucion_existe { get; set; } = 0;
        }
    }
}