using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class FrmCcEstadoCuentaDb
    {
        private const int ModuloGeneral = 10;
        private const int CodigoValidacion = -2;

        private const string TipoEstadoCuenta =
            "ESTADO_CUENTA";

        private const string TipoExcedentes =
            "EXCEDENTES";

        private const string SqlFechaServidor =
            "select GetDate()";

        private const string SolicitudRequerida =
            "La solicitud es requerida.";

        private const string IdentificacionRequerida =
            "Especifique la identificaci&oacute;n de la persona.";

        private const string UsuarioRequerido =
            "No se pudo determinar el usuario.";

        private const string FechaCorteRequerida =
            "Seleccione una fecha de corte v&aacute;lida.";

        private const string ConfiguracionEmpresaNoEncontrada =
            "No se encontr&oacute; la configuraci&oacute;n de la empresa.";

        private static readonly Regex SegmentoRegex = new(
            @"^[A-Za-z0-9_\-]+$",
            RegexOptions.CultureInvariant,
            TimeSpan.FromMilliseconds(100));

        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrxMain;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmCcEstadoCuentaDb(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);
            _mProGrxMain = new MProGrxMain(config);
            _securityMainDb =
                new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene los catalogos, la fecha del servidor y la configuracion
        /// empresarial necesaria para el formulario.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<CcEstadoCuentaInicialData>
            CC_FrmCCEstadoCuenta_Inicial_Obtener(
                int codEmpresa)
        {
            return EjecutarConConexion(
                codEmpresa,
                connection =>
                {
                    var configuracion =
                        ObtenerConfiguracion(
                            connection);

                    if (configuracion is null)
                    {
                        return CrearErrorInicial(
                            ConfiguracionEmpresaNoEncontrada);
                    }

                    var resultado =
                        new CcEstadoCuentaInicialData
                        {
                            fecha_servidor =
                                ObtenerFechaServidor(
                                    connection),

                            configuracion =
                                configuracion,

                            periodos_cierre =
                                ObtenerPeriodosCierre(
                                    connection),

                            periodos_excedentes =
                                ObtenerPeriodosExcedentes(
                                    connection),

                            instituciones =
                                ObtenerInstituciones(
                                    connection),

                            estados_persona =
                                ObtenerEstadosPersona(
                                    connection)
                        };

                    return DbHelper.CreateOkResponse(
                        resultado);
                },
                new CcEstadoCuentaInicialData(),
                "No fue posible inicializar el formulario.");
        }

        /// <summary>
        /// Obtiene la informacion basica de una persona y valida el acceso
        /// restringido a su expediente.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CcEstadoCuentaPersonaData>
            CC_FrmCCEstadoCuenta_Persona_Obtener(
                int codEmpresa,
                CcEstadoCuentaPersonaRequest? request)
        {
            if (request is null)
            {
                return CrearErrorPersona(
                    SolicitudRequerida);
            }

            string? validacion =
                ValidarPersona(request);

            if (validacion is not null)
            {
                return CrearErrorPersona(
                    validacion);
            }

            string cedula =
                request.cedula.Trim();

            string usuario =
                request.usuario.Trim();

            return EjecutarConConexion(
                codEmpresa,
                connection =>
                {
                    if (!TieneAccesoPersona(
                            connection,
                            cedula,
                            usuario))
                    {
                        return CrearErrorPersona(
                            "Esta persona tiene el expediente restringido. " +
                            "Requiere autorizaci&oacute;n para consultar.");
                    }

                    const string sql = """
                        select
                            rtrim(cedula) as cedula,
                            rtrim(isnull(nombre, '')) as nombre,
                            rtrim(isnull(af_email, '')) as email
                        from socios
                        where cedula = @cedula
                        """;

                    var persona =
                        connection.QueryFirstOrDefault
                            <CcEstadoCuentaPersonaData>(
                                sql,
                                new { cedula });

                    return persona is null
                        ? CrearErrorPersona(
                            "No se encontr&oacute; la persona indicada.")
                        : DbHelper.CreateOkResponse(
                            persona);
                },
                new CcEstadoCuentaPersonaData(),
                "No fue posible consultar la persona.");
        }

        /// <summary>
        /// Obtiene los departamentos correspondientes a una institucion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codInstitucion"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            CC_FrmCCEstadoCuenta_Departamentos_Obtener(
                int codEmpresa,
                int codInstitucion)
        {
            if (codInstitucion <= 0)
            {
                return CrearListaVacia();
            }

            const string sql = """
                select
                    rtrim(cod_departamento) as item,
                    rtrim(descripcion) as descripcion
                from afdepartamentos
                where cod_institucion = @codInstitucion
                order by descripcion
                """;

            return DbHelper.ExecuteListQuery
                <DropDownListaGenericaModel>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new { codInstitucion });
        }

        /// <summary>
        /// Obtiene las secciones correspondientes a una institucion y
        /// departamento.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            CC_FrmCCEstadoCuenta_Secciones_Obtener(
                int codEmpresa,
                CcEstadoCuentaSeccionRequest? request)
        {
            if (request is null ||
                request.cod_institucion <= 0 ||
                string.IsNullOrWhiteSpace(
                    request.cod_departamento))
            {
                return CrearListaVacia();
            }

            string departamento =
                request.cod_departamento.Trim();

            if (!EsSegmentoValido(
                    departamento))
            {
                return CrearErrorLista(
                    "El departamento seleccionado no es v&aacute;lido.");
            }

            const string sql = """
                select
                    rtrim(cod_seccion) as item,
                    rtrim(descripcion) as descripcion
                from afsecciones
                where cod_institucion = @codInstitucion
                  and cod_departamento = @codDepartamento
                order by descripcion
                """;

            return DbHelper.ExecuteListQuery
                <DropDownListaGenericaModel>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        codInstitucion =
                            request.cod_institucion,

                        codDepartamento =
                            departamento
                    });
        }

        /// <summary>
        /// Envia un estado de cuenta individual por correo electronico.
        /// MProGrxMain registra internamente la bitacora del envio.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto
            CC_FrmCCEstadoCuenta_Email_Enviar(
                int codEmpresa,
                CcEstadoCuentaEmailRequest? request)
        {
            if (request is null)
            {
                return CrearError(
                    SolicitudRequerida);
            }

            string? validacion =
                ValidarEmailIndividual(
                    request);

            if (validacion is not null)
            {
                return CrearError(
                    validacion);
            }

            return _mProGrxMain
                .sbEstadoCuenta_Email_Corte(
                    codEmpresa,
                    request.usuario.Trim(),
                    request.cedula.Trim(),
                    request.email.Trim(),
                    request.fecha_corte
                        .GetValueOrDefault());
        }

        /// <summary>
        /// Ejecuta el envio masivo de estados de cuenta por correo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto
            CC_FrmCCEstadoCuenta_EmailMasivo_Enviar(
                int codEmpresa,
                CcEstadoCuentaEmailMasivoRequest? request)
        {
            if (request is null)
            {
                return CrearError(
                    SolicitudRequerida);
            }

            string? validacion =
                ValidarEmailMasivo(
                    request);

            if (validacion is not null)
            {
                return CrearError(
                    validacion);
            }

            DateTime fechaCorte =
                request.fecha_corte
                    .GetValueOrDefault()
                    .Date
                    .AddHours(23)
                    .AddMinutes(59);

            var resultado = DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                {
                    connection.Execute(
                        "spSys_Estados_Cuenta_Email",
                        new
                        {
                            Institucion =
                                request.cod_institucion > 0
                                    ? request.cod_institucion
                                    : (int?)null,

                            Departamento =
                                ValorOpcional(
                                    request.cod_departamento),

                            Seccion =
                                ValorOpcional(
                                    request.cod_seccion),

                            EstadoPersona =
                                ValorOpcional(
                                    request.cod_estado),

                            Usuario =
                                request.usuario.Trim(),

                            Corte =
                                fechaCorte
                        },
                        commandType:
                            CommandType.StoredProcedure,
                        commandTimeout: 0);

                    return ObtenerDescripcionEstado(
                        connection,
                        request.cod_estado);
                });

            if (resultado.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    resultado.Description ??
                    "No fue posible enviar los estados de cuenta.",
                    resultado.Code ?? -1);
            }

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Aplica",
                ConstruirDetalleEmailMasivo(
                    request,
                    resultado.Result ??
                    "TODOS"));

            return DbHelper.OkResponse(
                "Estados de cuenta notificados v&iacute;a email " +
                "satisfactoriamente.");
        }

        /// <summary>
        /// Registra la bitacora correspondiente a la generacion local
        /// de un estado de cuenta o reporte de excedentes.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto
            CC_FrmCCEstadoCuenta_Reporte_Bitacora_Registrar(
                int codEmpresa,
                CcEstadoCuentaReporteBitacoraRequest? request)
        {
            if (request is null)
            {
                return CrearError(
                    SolicitudRequerida);
            }

            if (string.IsNullOrWhiteSpace(
                    request.usuario))
            {
                return CrearError(
                    UsuarioRequerido);
            }

            string tipoReporte =
                request.tipo_reporte
                    .Trim()
                    .ToUpperInvariant();

            if (tipoReporte is not (
                    TipoEstadoCuenta or
                    TipoExcedentes))
            {
                return CrearError(
                    "El tipo de reporte indicado no es v&aacute;lido.");
            }

            if (request.por_segmentos)
            {
                return DbHelper.OkResponse(
                    "El reporte por segmentos no requiere bit&aacute;cora.");
            }

            if (string.IsNullOrWhiteSpace(
                    request.cedula))
            {
                return CrearError(
                    IdentificacionRequerida);
            }

            string cedula =
                request.cedula.Trim();

            if (tipoReporte ==
                TipoEstadoCuenta)
            {
                RegistrarBitacora(
                    codEmpresa,
                    request.usuario,
                    "Aplica",
                    $"Estado de Cuenta: {cedula}");

                return DbHelper.OkResponse(
                    "Movimiento registrado satisfactoriamente.");
            }

            if (request.id_periodo <= 0)
            {
                return CrearError(
                    "Seleccione un per&iacute;odo de excedentes v&aacute;lido.");
            }

            var periodo = ObtenerPeriodoExcedentes(
                codEmpresa,
                request.id_periodo);

            if (periodo.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    periodo.Description ??
                    "No fue posible consultar el per&iacute;odo.",
                    periodo.Code ?? -1);
            }

            string descripcionPeriodo =
                string.IsNullOrWhiteSpace(
                    periodo.Result)
                    ? request.id_periodo.ToString(
                        CultureInfo.InvariantCulture)
                    : periodo.Result.Trim();

            RegistrarBitacora(
                codEmpresa,
                request.usuario,
                "Imprime",
                $"Estado Excedentes Ced.{cedula} " +
                $"Periodo: {descripcionPeriodo}");

            return DbHelper.OkResponse(
                "Movimiento registrado satisfactoriamente.");
        }

        private ErrorDto<string>
            ObtenerPeriodoExcedentes(
                int codEmpresa,
                int idPeriodo)
        {
            const string sql = """
                select rtrim(isnull(itmx, ''))
                from vExc_Periodos
                where idx = @idPeriodo
                """;

            var resultado = DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                    connection
                        .QueryFirstOrDefault<string>(
                            sql,
                            new { idPeriodo }) ??
                    string.Empty);

            if (resultado.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    resultado.Description ??
                    "No fue posible consultar el per&iacute;odo.",
                    resultado.Code ?? -1,
                    string.Empty);
            }

            return DbHelper.CreateOkResponse(
                resultado.Result ??
                string.Empty);
        }

        private ErrorDto<T>
            EjecutarConConexion<T>(
                int codEmpresa,
                Func<IDbConnection, ErrorDto<T>> accion,
                T resultadoVacio,
                string mensajeError)
        {
            var resultado = DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                    accion(connection));

            if (resultado.Code != 0 ||
                resultado.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    resultado.Description ??
                    mensajeError,
                    resultado.Code ?? -1,
                    resultadoVacio);
            }

            return resultado.Result;
        }

        private static DateTime
            ObtenerFechaServidor(
                IDbConnection connection)
        {
            return connection
                .QueryFirst<DateTime>(
                    SqlFechaServidor);
        }

        private static CcEstadoCuentaConfiguracionData?
            ObtenerConfiguracion(
                IDbConnection connection)
        {
            const string sql = """
                select top 1
                    rtrim(isnull(nombre, ''))
                        as nombre_empresa,
                    isnull(sys_ccss_ind, 0)
                        as sys_ccss_ind,
                    isnull(ec_visible_patrimonio, 0)
                        as ec_visible_patrimonio,
                    isnull(ec_visible_fondos, 0)
                        as ec_visible_fondos,
                    isnull(ec_visible_creditos, 0)
                        as ec_visible_creditos,
                    isnull(ec_visible_fianzas, 0)
                        as ec_visible_fianzas,
                    rtrim(isnull(estadoCuenta, ''))
                        as estado_cuenta,
                    rtrim(isnull(
                        constancia_crd_encabezado,
                        ''
                    )) as constancia_crd_encabezado
                from sif_empresa
                """;

            return connection
                .QueryFirstOrDefault
                    <CcEstadoCuentaConfiguracionData>(
                        sql);
        }

        private static bool
            TieneAccesoPersona(
                IDbConnection connection,
                string cedula,
                string usuario)
        {
            var acceso =
                connection.QueryFirstOrDefault
                    <CcEstadoCuentaAccesoData>(
                        "spSYS_RA_Consulta_Status",
                        new
                        {
                            cedula,
                            usuario
                        },
                        commandType:
                            CommandType.StoredProcedure);

            return acceso is null ||
                   acceso.persona_id <= 0 ||
                   acceso.autorizacion_id != 0;
        }

        private static List<DropDownListaGenericaModel>
            ObtenerPeriodosCierre(
                IDbConnection connection)
        {
            return connection
                .Query<CcEstadoCuentaPeriodoData>(
                    "spSys_Periodos_Cierre_Consulta",
                    commandType:
                        CommandType.StoredProcedure)
                .Select(periodo =>
                    new DropDownListaGenericaModel
                    {
                        item =
                            NormalizarPeriodoCierre(
                                periodo.idx),

                        descripcion =
                            periodo.itmx.Trim()
                    })
                .ToList();
        }

        private static List<DropDownListaGenericaModel>
            ObtenerPeriodosExcedentes(
                IDbConnection connection)
        {
            const string sql = """
                select
                    idx as item,
                    rtrim(itmx) as descripcion
                from vExc_Periodos
                where estado = 'C'
                order by idx desc
                """;

            return connection
                .Query<DropDownListaGenericaModel>(
                    sql)
                .ToList();
        }

        private static List<DropDownListaGenericaModel>
            ObtenerInstituciones(
                IDbConnection connection)
        {
            const string sql = """
                select
                    cod_institucion as item,
                    rtrim(descripcion) as descripcion
                from instituciones
                where activa = 1
                order by descripcion
                """;

            return connection
                .Query<DropDownListaGenericaModel>(
                    sql)
                .ToList();
        }

        private static List<DropDownListaGenericaModel>
            ObtenerEstadosPersona(
                IDbConnection connection)
        {
            const string sql = """
                select
                    rtrim(cod_estado) as item,
                    rtrim(descripcion) as descripcion
                from afi_estados_persona
                where activo = 1
                order by descripcion
                """;

            return connection
                .Query<DropDownListaGenericaModel>(
                    sql)
                .ToList();
        }

        private static string
            ObtenerDescripcionEstado(
                IDbConnection connection,
                string? codEstado)
        {
            if (string.IsNullOrWhiteSpace(
                    codEstado))
            {
                return "TODOS";
            }

            const string sql = """
                select rtrim(isnull(descripcion, ''))
                from afi_estados_persona
                where cod_estado = @codEstado
                """;

            string codigo =
                codEstado.Trim();

            return connection
                       .QueryFirstOrDefault<string>(
                           sql,
                           new
                           {
                               codEstado =
                                   codigo
                           }) ??
                   codigo;
        }

        private static string?
            ValidarPersona(
                CcEstadoCuentaPersonaRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                    request.cedula))
            {
                return IdentificacionRequerida;
            }

            return string.IsNullOrWhiteSpace(
                request.usuario)
                    ? UsuarioRequerido
                    : null;
        }

        private static string?
            ValidarEmailIndividual(
                CcEstadoCuentaEmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                    request.cedula))
            {
                return IdentificacionRequerida;
            }

            if (string.IsNullOrWhiteSpace(
                    request.email))
            {
                return
                    "La persona no cuenta con un correo registrado, " +
                    "verifique.";
            }

            if (string.IsNullOrWhiteSpace(
                    request.usuario))
            {
                return UsuarioRequerido;
            }

            return request.fecha_corte.HasValue
                ? null
                : FechaCorteRequerida;
        }

        private static string?
            ValidarEmailMasivo(
                CcEstadoCuentaEmailMasivoRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                    request.usuario))
            {
                return UsuarioRequerido;
            }

            if (!request.fecha_corte.HasValue)
            {
                return FechaCorteRequerida;
            }

            return SegmentosValidos(request)
                ? null
                : "Los filtros de segmentaci&oacute;n no son v&aacute;lidos.";
        }

        private static bool
            SegmentosValidos(
                CcEstadoCuentaEmailMasivoRequest request)
        {
            return EsSegmentoValido(
                       request.cod_departamento) &&
                   EsSegmentoValido(
                       request.cod_seccion) &&
                   EsSegmentoValido(
                       request.cod_estado);
        }

        private static bool
            EsSegmentoValido(
                string? valor)
        {
            return string.IsNullOrWhiteSpace(
                       valor) ||
                   SegmentoRegex.IsMatch(
                       valor.Trim());
        }

        private static object
            NormalizarPeriodoCierre(
                object? valor)
        {
            string texto =
                Convert.ToString(
                    valor,
                    CultureInfo.InvariantCulture) ??
                string.Empty;

            texto = texto.Trim();

            if (string.Equals(
                    texto,
                    "HOY",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "HOY";
            }

            return DateTime.TryParse(
                texto,
                CultureInfo.CurrentCulture,
                DateTimeStyles.None,
                out DateTime fecha)
                    ? fecha.ToString(
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture)
                    : texto;
        }

        private static string?
            ValorOpcional(
                string? valor)
        {
            return string.IsNullOrWhiteSpace(
                valor)
                    ? null
                    : valor.Trim();
        }

        private static string
            ValorBitacora(
                string? valor)
        {
            return string.IsNullOrWhiteSpace(
                valor)
                    ? "T"
                    : valor.Trim();
        }

        private static string
            ValorBitacora(
                int valor)
        {
            return valor > 0
                ? valor.ToString(
                    CultureInfo.InvariantCulture)
                : "T";
        }

        private static string
            ConstruirDetalleEmailMasivo(
                CcEstadoCuentaEmailMasivoRequest request,
                string estadoDescripcion)
        {
            return
                "EC Masivo: " +
                $"[I: {ValorBitacora(request.cod_institucion)}, " +
                $"D: {ValorBitacora(request.cod_departamento)}, " +
                $"S: {ValorBitacora(request.cod_seccion)}] " +
                $"Estado: {estadoDescripcion}";
        }

        private void RegistrarBitacora(
            int codEmpresa,
            string usuario,
            string movimiento,
            string detalle)
        {
            _securityMainDb.Bitacora(
                new BitacoraInsertarDto
                {
                    EmpresaId =
                        codEmpresa,

                    Usuario =
                        usuario.Trim(),

                    DetalleMovimiento =
                        detalle,

                    Movimiento =
                        movimiento,

                    Modulo =
                        ModuloGeneral
                });
        }

        private static ErrorDto
            CrearError(
                string mensaje)
        {
            return DbHelper.ErrorResponse(
                mensaje,
                CodigoValidacion);
        }

        private static
            ErrorDto<CcEstadoCuentaInicialData>
            CrearErrorInicial(
                string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                CodigoValidacion,
                new CcEstadoCuentaInicialData());
        }

        private static
            ErrorDto<CcEstadoCuentaPersonaData>
            CrearErrorPersona(
                string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                CodigoValidacion,
                new CcEstadoCuentaPersonaData());
        }

        private static ErrorDto<
            List<DropDownListaGenericaModel>>
            CrearErrorLista(
                string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                CodigoValidacion,
                new List<DropDownListaGenericaModel>());
        }

        private static ErrorDto<
            List<DropDownListaGenericaModel>>
            CrearListaVacia()
        {
            return DbHelper.CreateOkResponse(
                new List<DropDownListaGenericaModel>());
        }
    }
}