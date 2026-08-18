using System.Data;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_ControlTramites;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_ControlTramites
{
    public sealed class FrmFndRecepcionFondosTagsDb
    {
        private const string Modulo = "FND";
        private const string MovimientoRecepcion = "RECEPCION";
        private const string MovimientoDevolucion = "DEVOLUCION";
        private readonly PortalDB _portalDb;

        /// <summary>
        /// Inicializa el acceso a datos del formulario.
        /// </summary>
        /// <param name="config">Configuracion de conexiones.</param>
        public FrmFndRecepcionFondosTagsDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene las etiquetas configuradas y los usuarios activos.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <returns>Datos requeridos para inicializar el formulario.</returns>
        public ErrorDto<FndRecepcionFondosTagsInicializarResponse>
            FND_frmFNDRecepcionFondosTags_Inicializar(int codEmpresa)
        {
            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();

                var tags = FND_frmFNDRecepcionFondosTags_Tags_Obtener(
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
                    new FndRecepcionFondosTagsInicializarResponse
                    {
                        tag_recepcion = tags.tag_recepcion,
                        tag_devolucion = tags.tag_devolucion,
                        tag_recepcion_devolucion =
                            tags.tag_recepcion_devolucion,
                        usuarios = usuarios
                    });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new FndRecepcionFondosTagsInicializarResponse());
            }
        }

        /// <summary>
        /// Obtiene los planes de la operadora principal para la busqueda F4.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <returns>Planes disponibles.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            FND_frmFNDRecepcionFondosTags_Planes_Obtener(int codEmpresa)
        {
            const string sql = """
                select
                    rtrim(cod_plan) as item,
                    rtrim(descripcion) as descripcion
                from FND_Planes
                where cod_operadora = 1
                order by cod_plan;
                """;

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql);
        }

        /// <summary>
        /// Obtiene los contratos activos de un plan para la busqueda F4.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="codPlan">Codigo del plan.</param>
        /// <param name="cedula">Cedula opcional para limitar los contratos.</param>
        /// <returns>Contratos activos que cumplen los filtros.</returns>
        public ErrorDto<List<FndRecepcionFondosTagsContratoBusquedaResponse>>
            FND_frmFNDRecepcionFondosTags_Contratos_Obtener(
                int codEmpresa,
                string? codPlan,
                string? cedula)
        {
            string plan = codPlan?.Trim() ?? string.Empty;
            string identificacion = cedula?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(plan))
            {
                return DbHelper.CreateErrorResponse(
                    "El plan es requerido.",
                    -2,
                    new List<FndRecepcionFondosTagsContratoBusquedaResponse>());
            }

            const string sql = """
                select
                    F.cod_contrato,
                    F.cod_operadora,
                    rtrim(F.cedula) as cedula,
                    rtrim(S.nombre) as nombre
                from FND_Contratos F
                inner join Socios S on F.cedula = S.cedula
                where F.cod_plan = @CodPlan
                  and (@Cedula = '' or F.cedula = @Cedula)
                  and F.estado = 'A'
                order by F.cod_contrato;
                """;

            return DbHelper.ExecuteListQuery<
                FndRecepcionFondosTagsContratoBusquedaResponse>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new { CodPlan = plan, Cedula = identificacion });
        }

        /// <summary>
        /// Valida y obtiene un contrato para recepcion o devolucion.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="codPlan">Codigo del plan.</param>
        /// <param name="codContrato">Codigo del contrato.</param>
        /// <param name="movimiento">Tipo de movimiento.</param>
        /// <returns>Contrato que cumple el estado requerido.</returns>
        public ErrorDto<FndRecepcionFondosTagsContratoResponse?>
            FND_frmFNDRecepcionFondosTags_Contrato_Obtener(
                int codEmpresa,
                string? codPlan,
                long codContrato,
                string? movimiento)
        {
            string plan = codPlan?.Trim() ?? string.Empty;
            string tipo =
                FND_frmFNDRecepcionFondosTags_Movimiento_Normalizar(
                    movimiento);

            if (string.IsNullOrWhiteSpace(plan) || codContrato <= 0)
            {
                return DbHelper.CreateErrorResponse<
                    FndRecepcionFondosTagsContratoResponse?>(
                        "Debe indicar un plan y contrato validos.",
                        -2,
                        null);
            }

            if (string.IsNullOrWhiteSpace(tipo))
            {
                return DbHelper.CreateErrorResponse<
                    FndRecepcionFondosTagsContratoResponse?>(
                        "El tipo de movimiento no es valido.",
                        -2,
                        null);
            }

            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDb, codEmpresa);
                var contrato =
                    FND_frmFNDRecepcionFondosTags_Contrato_Consultar(
                        connection,
                        null,
                        plan,
                        codContrato,
                        tipo,
                        true);

                if (contrato is null)
                {
                    return DbHelper.CreateErrorResponse<
                        FndRecepcionFondosTagsContratoResponse?>(
                            "No se encontro un contrato con el estado requerido.",
                            -2,
                            null);
                }

                return DbHelper.CreateOkResponse<
                    FndRecepcionFondosTagsContratoResponse?>(contrato);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<
                    FndRecepcionFondosTagsContratoResponse?>(
                        ex.Message,
                        -1,
                        null);
            }
        }

        /// <summary>
        /// Obtiene los contratos pendientes de recepcion o devolucion.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="movimiento">Tipo de movimiento.</param>
        /// <returns>Contratos pendientes.</returns>
        public ErrorDto<List<FndRecepcionFondosTagsPendienteResponse>>
            FND_frmFNDRecepcionFondosTags_Pendientes_Obtener(
                int codEmpresa,
                string? movimiento)
        {
            string tipo =
                FND_frmFNDRecepcionFondosTags_Movimiento_Normalizar(
                    movimiento);
            if (string.IsNullOrWhiteSpace(tipo))
            {
                return DbHelper.CreateErrorResponse(
                    "El tipo de movimiento no es valido.",
                    -2,
                    new List<FndRecepcionFondosTagsPendienteResponse>());
            }

            const string sql = """
                select
                    rtrim(F.cedula) as cedula,
                    rtrim(S.nombre) as nombre,
                    isnull(rtrim(O.descripcion), '') as descripcion,
                    F.fecha_inicio,
                    isnull(rtrim(F.usuario), '') as usuario,
                    F.cod_operadora,
                    rtrim(F.cod_plan) as cod_plan,
                    F.cod_contrato
                from FND_Contratos F
                inner join Socios S on F.cedula = S.cedula
                left join SIF_Oficinas O
                    on F.cod_oficina = O.cod_oficina
                where isnull(F.analista_recepcion, 0) =
                    case when @Movimiento = 'RECEPCION' then 0 else 2 end
                order by F.fecha_inicio desc, F.cod_plan, F.cod_contrato;
                """;

            return DbHelper.ExecuteListQuery<
                FndRecepcionFondosTagsPendienteResponse>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new { Movimiento = tipo });
        }

        /// <summary>
        /// Registra las etiquetas de los contratos seleccionados.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="request">Movimiento y contratos seleccionados.</param>
        /// <returns>Cantidad de registros aplicados.</returns>
        public ErrorDto<FndRecepcionFondosTagsAplicarResponse>
            FND_frmFNDRecepcionFondosTags_Movimiento_Aplicar(
                int codEmpresa,
                FndRecepcionFondosTagsAplicarRequest? request)
        {
            var error = FND_frmFNDRecepcionFondosTags_Aplicar_Validar(request);
            if (error is not null)
            {
                return error;
            }

            string movimiento =
                FND_frmFNDRecepcionFondosTags_Movimiento_Normalizar(
                    request!.movimiento);

            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Open();
                using var transaction = connection.BeginTransaction();

                try
                {
                    var tags = FND_frmFNDRecepcionFondosTags_Tags_Obtener(
                        connection,
                        transaction);
                    string tag = movimiento == MovimientoRecepcion
                        ? tags.tag_recepcion
                        : tags.tag_devolucion;
                    string observacion = movimiento == MovimientoRecepcion
                        ? "Recibida la documentacion del contrato"
                        : "Devolucion de la documentacion del contrato";
                    int aplicados = 0;

                    foreach (var item in request.contratos
                        .GroupBy(x => new
                        {
                            Plan = x.cod_plan.Trim(),
                            x.cod_contrato
                        })
                        .Select(group => group.First()))
                    {
                        string plan = item.cod_plan.Trim();
                        var contrato =
                            FND_frmFNDRecepcionFondosTags_Contrato_Consultar(
                                connection,
                                transaction,
                                plan,
                                item.cod_contrato,
                                movimiento,
                                true);

                        if (contrato is null)
                        {
                            throw new InvalidOperationException(
                                $"El contrato {plan}-{item.cod_contrato} ya no cumple el estado requerido.");
                        }

                        connection.Execute(
                            "spSIFRegistraTags",
                            new
                            {
                                Codigo = contrato.cod_plan,
                                Tag = tag,
                                Usuario = request.usuario.Trim(),
                                Observacion = observacion,
                                Documento = contrato.cod_contrato.ToString(),
                                Modulo,
                                Llave_01 = contrato.cod_plan,
                                Llave_02 = contrato.cod_contrato.ToString(),
                                Llave_03 = string.Empty
                            },
                            transaction,
                            commandType: CommandType.StoredProcedure);
                        aplicados++;
                    }

                    transaction.Commit();
                    return DbHelper.CreateOkResponse(
                        new FndRecepcionFondosTagsAplicarResponse
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
                    new FndRecepcionFondosTagsAplicarResponse());
            }
        }

        /// <summary>
        /// Obtiene el historial de etiquetas de fondos.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <param name="request">Filtros del historial.</param>
        /// <returns>Movimientos de etiquetas encontrados.</returns>
        public ErrorDto<List<FndRecepcionFondosTagsHistorialResponse>>
            FND_frmFNDRecepcionFondosTags_Historial_Obtener(
                int codEmpresa,
                FndRecepcionFondosTagsHistorialRequest? request)
        {
            request ??= new FndRecepcionFondosTagsHistorialRequest();
            string plan = request.cod_plan?.Trim() ?? string.Empty;
            string usuario = request.usuario?.Trim() ?? string.Empty;

            if (request.cod_contrato.HasValue && request.cod_contrato <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "El contrato no es valido.",
                    -2,
                    new List<FndRecepcionFondosTagsHistorialResponse>());
            }

            if (request.fecha_inicio?.Date > request.fecha_fin?.Date)
            {
                return DbHelper.CreateErrorResponse(
                    "La fecha inicial no puede ser mayor que la fecha final.",
                    -2,
                    new List<FndRecepcionFondosTagsHistorialResponse>());
            }

            const string sql = """
                select
                    rtrim(T.descripcion) as descripcion,
                    isnull(rtrim(CT.notas), '') as notas,
                    CT.registro_fecha,
                    isnull(rtrim(CT.registro_usuario), '')
                        as registro_usuario
                from SIF_Control_Tags CT
                inner join SIF_Tags T
                    on CT.tag_codigo = T.tag_codigo
                where CT.cod_modulo = @Modulo
                  and (@CodPlan = '' or CT.codigo = @CodPlan)
                  and (
                      @CodContrato is null
                      or CT.documento = convert(varchar(30), @CodContrato)
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
                FndRecepcionFondosTagsHistorialResponse>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        Modulo,
                        CodPlan = plan,
                        CodContrato = request.cod_contrato,
                        Usuario = usuario,
                        FechaInicio = request.fecha_inicio?.Date,
                        FechaFin = request.fecha_fin?.Date
                    });
        }

        /// <summary>
        /// Consulta un contrato y opcionalmente exige el estado del movimiento.
        /// </summary>
        /// <param name="connection">Conexion SQL.</param>
        /// <param name="transaction">Transaccion SQL opcional.</param>
        /// <param name="codPlan">Codigo del plan.</param>
        /// <param name="codContrato">Codigo del contrato.</param>
        /// <param name="movimiento">Tipo de movimiento.</param>
        /// <param name="validarEstado">Indica si valida analista_recepcion.</param>
        /// <returns>Contrato encontrado.</returns>
        private static FndRecepcionFondosTagsContratoResponse?
            FND_frmFNDRecepcionFondosTags_Contrato_Consultar(
                SqlConnection connection,
                SqlTransaction? transaction,
                string codPlan,
                long codContrato,
                string movimiento,
                bool validarEstado)
        {
            const string sql = """
                select top 1
                    rtrim(F.cedula) as cedula,
                    rtrim(S.nombre) as nombre,
                    rtrim(F.cod_plan) as cod_plan,
                    F.cod_operadora,
                    F.cod_contrato,
                    isnull(rtrim(O.descripcion), '') as descripcion,
                    case F.estado
                        when 'A' then 'Activa'
                        when 'L' then 'Liquidada'
                        else isnull(rtrim(F.estado), '')
                    end as estado
                from FND_Contratos F
                inner join Socios S on F.cedula = S.cedula
                left join SIF_Oficinas O
                    on F.cod_oficina = O.cod_oficina
                where F.cod_plan = @CodPlan
                  and F.cod_contrato = @CodContrato
                  and (
                      @ValidarEstado = 0
                      or (
                          @Movimiento = 'RECEPCION'
                          and isnull(F.analista_recepcion, 0) = 0
                      )
                      or (
                          @Movimiento = 'DEVOLUCION'
                          and F.analista_recepcion = 1
                      )
                  );
                """;

            return connection.QueryFirstOrDefault<
                FndRecepcionFondosTagsContratoResponse>(
                    sql,
                    new
                    {
                        CodPlan = codPlan,
                        CodContrato = codContrato,
                        Movimiento = movimiento,
                        ValidarEstado = validarEstado
                    },
                    transaction);
        }

        /// <summary>
        /// Obtiene y valida los parametros 10, 11 y 12 de etiquetas.
        /// </summary>
        /// <param name="connection">Conexion SQL.</param>
        /// <param name="transaction">Transaccion SQL opcional.</param>
        /// <returns>Configuracion de etiquetas.</returns>
        private static TagsConfiguracion
            FND_frmFNDRecepcionFondosTags_Tags_Obtener(
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
        /// Valida el request del proceso de aplicacion.
        /// </summary>
        /// <param name="request">Request a validar.</param>
        /// <returns>Error de validacion o null.</returns>
        private static ErrorDto<FndRecepcionFondosTagsAplicarResponse>?
            FND_frmFNDRecepcionFondosTags_Aplicar_Validar(
                FndRecepcionFondosTagsAplicarRequest? request)
        {
            if (request is null ||
                string.IsNullOrWhiteSpace(request.usuario) ||
                request.contratos is null ||
                request.contratos.Count == 0)
            {
                return DbHelper.CreateErrorResponse(
                    "El usuario y los contratos seleccionados son requeridos.",
                    -2,
                    new FndRecepcionFondosTagsAplicarResponse());
            }

            if (string.IsNullOrWhiteSpace(
                    FND_frmFNDRecepcionFondosTags_Movimiento_Normalizar(
                        request.movimiento)))
            {
                return DbHelper.CreateErrorResponse(
                    "El tipo de movimiento no es valido.",
                    -2,
                    new FndRecepcionFondosTagsAplicarResponse());
            }

            if (request.contratos.Any(
                    x => string.IsNullOrWhiteSpace(x.cod_plan) ||
                        x.cod_contrato <= 0))
            {
                return DbHelper.CreateErrorResponse(
                    "La lista contiene planes o contratos no validos.",
                    -2,
                    new FndRecepcionFondosTagsAplicarResponse());
            }

            return null;
        }

        /// <summary>
        /// Normaliza un tipo de movimiento permitido.
        /// </summary>
        /// <param name="movimiento">Movimiento recibido.</param>
        /// <returns>Movimiento normalizado o cadena vacia.</returns>
        private static string
            FND_frmFNDRecepcionFondosTags_Movimiento_Normalizar(
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
