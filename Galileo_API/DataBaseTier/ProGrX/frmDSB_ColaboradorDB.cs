using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX
{
    public class FrmDsbColaboradorDB
    {
        private const string SpRhPortalVinculado = "spRH_Portal_Vinculado";
        private const string OpcionVacaciones = "vacaciones";
        private const string OpcionIncapacidades = "incapacidades";
        private const string OpcionPermisos = "permisos";
        private const string OpcionBoletasPago = "boletasPago";
        private const string OpcionTraslados = "traslados";
        private const string AppCodProGrX = "ProGrX";
        private const string AppNameWeb = "ProGrX_WEB";
        private const string SqlIdentificacionColaborador = "SELECT IDENTIFICACION FROM RH_PERSONAS WHERE EMPLEADO_ID = @EmpleadoId";
        private const string AccionRecibir = "recibir";
        private static readonly byte[] FirmaJpeg = [0xFF, 0xD8, 0xFF];
        private static readonly byte[] FirmaJpegFin = [0xFF, 0xD9];
        private static readonly byte[] FirmaPng = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        private static readonly byte[] FirmaBmp = [0x42, 0x4D];

        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmDsbColaboradorDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        public ErrorDto<ColaboradorVinculoData> Colaborador_Vinculado_Obtener(
            int CodEmpresa,
            string usuario)
        {
            var result = new ColaboradorVinculoData();

            if (ValidarUsuario(usuario, result, "El usuario autenticado es requerido.")
                is { } errorUsuario)
            {
                return errorUsuario;
            }

            try
            {
                var vinculacion = ObtenerVinculacion(CodEmpresa, usuario);
                var empleadoId = vinculacion?.Empleado_ID;

                return DbHelper.CreateOkResponse(new ColaboradorVinculoData
                {
                    Vinculado = !string.IsNullOrWhiteSpace(empleadoId),
                    EmpleadoId = string.IsNullOrWhiteSpace(empleadoId)
                        ? null
                        : empleadoId.Trim()
                });
            }
            catch (Exception ex)
            {
                return CrearError(ex, result);
            }
        }

        public ErrorDto<ColaboradorPerfilData> Colaborador_Perfil_Obtener(
            int CodEmpresa,
            string usuario,
            string AppVersion)
        {
            var result = new ColaboradorPerfilData();

            if (ValidarUsuario(usuario, result, "El usuario autenticado es requerido.")
                is { } errorUsuario)
            {
                return errorUsuario;
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
                var vinculacion = ObtenerVinculacion(connection, usuario);
                var empleadoId = vinculacion?.Empleado_ID?.Trim();

                if (string.IsNullOrWhiteSpace(empleadoId))
                {
                    return DbHelper.CreateErrorResponse(
                        "El usuario autenticado no tiene un colaborador vinculado.",
                        -2,
                        result);
                }

                var perfil = CargarPerfil(connection, empleadoId, usuario, AppVersion);

                if (perfil is null)
                {
                    return DbHelper.CreateErrorResponse(
                        "No se encontraron los datos del colaborador vinculado.",
                        -3,
                        result);
                }

                return DbHelper.CreateOkResponse(perfil);
            }
            catch (Exception ex)
            {
                return CrearError(ex, result);
            }
        }

        public ErrorDto<bool> Colaborador_Foto_Cambia(
            int codEmpresa,
            string usuario,
            ColaboradorFotoCambiaRequest request)
        {
            if (string.IsNullOrWhiteSpace(usuario)
                || request is null
                || string.IsNullOrWhiteSpace(request.EmpleadoId)
                || request.EmpleadoId.Length > 20
                || request.FotoBase64 is null
                || (!request.Quitar && string.IsNullOrWhiteSpace(request.FotoBase64))
                || (request.Quitar && !string.IsNullOrEmpty(request.FotoBase64))
                || request.FotoBase64.Length > 1_400_000)
            {
                return DbHelper.CreateErrorResponse("La foto seleccionada no es válida.", -2, false);
            }

            byte[]? foto = null;
            if (!request.Quitar)
            {
                try
                {
                    foto = Convert.FromBase64String(request.FotoBase64);
                }
                catch (FormatException)
                {
                    return DbHelper.CreateErrorResponse("El archivo de imagen no es válido.", -2, false);
                }

                if (foto.Length < 4
                    || foto.Length > 1_000_000
                    || !foto.AsSpan().StartsWith(FirmaJpeg)
                    || !foto.AsSpan().EndsWith(FirmaJpegFin))
                {
                    return DbHelper.CreateErrorResponse(
                        "La foto debe ser una imagen JPEG menor de 1 MB.",
                        -2,
                        false);
                }
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var empleadoId = request.EmpleadoId.Trim();
                var errorAcceso = ValidarAccesoColaborador(
                    connection,
                    usuario,
                    empleadoId,
                    request.Clave);

                if (errorAcceso is not null)
                {
                    return DbHelper.CreateErrorResponse(errorAcceso, -3, false);
                }

                var filasActualizadas = request.Quitar
                    ? connection.Execute(
                        "UPDATE dbo.RH_PERSONAS SET FOTO = NULL WHERE EMPLEADO_ID = @EmpleadoId",
                        new { EmpleadoId = empleadoId })
                    : connection.Execute(
                        "UPDATE dbo.RH_PERSONAS SET FOTO = @Foto WHERE EMPLEADO_ID = @EmpleadoId",
                        new { Foto = foto, EmpleadoId = empleadoId });

                if (filasActualizadas != 1)
                {
                    return DbHelper.CreateErrorResponse(
                        "No fue posible actualizar la foto del colaborador.",
                        -4,
                        false);
                }

                _securityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario.Trim(),
                    Movimiento = "Actualiza",
                    DetalleMovimiento = (request.Quitar
                        ? "Quita foto oficial del colaborador: "
                        : "Actualiza foto oficial del colaborador: ") + empleadoId,
                    Modulo = 23
                });

                return DbHelper.CreateOkResponse(true);
            }
            catch (Exception ex)
            {
                return CrearError(ex, false);
            }
        }

        public ErrorDto<List<ColaboradorEmpleadoOpcionData>> Colaborador_Consulta_Id(
            int CodEmpresa,
            string Identificacion,
            string? EmpleadoId)
        {
            var result = new List<ColaboradorEmpleadoOpcionData>();

            if (string.IsNullOrWhiteSpace(Identificacion) || Identificacion.Length > 20)
            {
                return DbHelper.CreateErrorResponse(
                    "La identificación es requerida y debe tener máximo 20 caracteres.",
                    -1,
                    result);
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
                var empleados = connection.Query<ColaboradorConsultaIdRow>(
                    "spRH_Portal_Consulta_Id",
                    new
                    {
                        Identificacion,
                        EmpleadoId = string.IsNullOrWhiteSpace(EmpleadoId)
                            ? string.Empty
                            : EmpleadoId.Trim()
                    },
                    commandType: CommandType.StoredProcedure);

                return DbHelper.CreateOkResponse(empleados
                    .Where(empleado => !string.IsNullOrWhiteSpace(empleado.EMPLEADO_ID))
                    .Select(empleado =>
                    {
                        var empleadoId = empleado.EMPLEADO_ID!.Trim();
                        var nombre = empleado.NOMBRE_COMPLETO?.Trim() ?? string.Empty;

                        return new ColaboradorEmpleadoOpcionData
                        {
                            Id = empleadoId,
                            Descripcion = string.IsNullOrWhiteSpace(nombre)
                                ? empleadoId
                                : $"{empleadoId} - {nombre}",
                            EmpleadoId = empleadoId,
                            NombreCompleto = nombre,
                            Identificacion = empleado.IDENTIFICACION?.Trim() ?? string.Empty,
                            EstadoPersona = empleado.ESTADO_PERSONA?.Trim()
                        };
                    })
                    .ToList());
            }
            catch (Exception ex)
            {
                return CrearError(ex, result);
            }
        }

        public ErrorDto<ColaboradorAccesoData> Colaborador_Acceso(
            int CodEmpresa,
            string usuario,
            ColaboradorAccesoRequest request)
        {
            var result = new ColaboradorAccesoData();

            if (ValidarSolicitudUsuario(
                usuario,
                result,
                () => request is null
                    || string.IsNullOrWhiteSpace(request.EmpleadoId)
                    || request.EmpleadoId.Length > 20
                    || string.IsNullOrEmpty(request.Clave)
                    || request.Clave.Length > 100,
                "Seleccione un empleado e ingrese una clave válida.") is { } errorValidacion)
            {
                return errorValidacion;
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);

                if (request.Vincular)
                {
                    var vinculacionActual = ObtenerVinculacion(connection, usuario);

                    if (!string.IsNullOrWhiteSpace(vinculacionActual?.Empleado_ID))
                    {
                        return DbHelper.CreateErrorResponse(
                            "El usuario del sistema ya está vinculado a un colaborador.",
                            -2,
                            result);
                    }
                }

                var validacion = ConsultarClave(
                    connection,
                    request.EmpleadoId.Trim(),
                    request.Clave);

                if (validacion?.Existe != 1)
                {
                    return DbHelper.CreateOkResponse(new ColaboradorAccesoData
                    {
                        ClaveValida = false
                    });
                }

                var empleadoId = request.EmpleadoId.Trim();
                var perfil = CargarPerfil(connection, empleadoId, usuario, request.AppVersion);

                if (perfil is null)
                {
                    return DbHelper.CreateErrorResponse(
                        "No se encontraron los datos del colaborador seleccionado.",
                        -3,
                        result);
                }

                string? errorVinculacion = null;

                if (request.Vincular)
                {
                    try
                    {
                        connection.Execute(
                            "spRH_Portal_Empleado_Vincula",
                            CrearParametrosAuditoria(empleadoId, usuario, request.AppVersion),
                            commandType: CommandType.StoredProcedure);
                    }
                    catch (Exception ex)
                    {
                        errorVinculacion = ex.Message;
                    }
                }

                return DbHelper.CreateOkResponse(new ColaboradorAccesoData
                {
                    ClaveValida = true,
                    Vinculado = request.Vincular && errorVinculacion is null,
                    Perfil = perfil,
                    ErrorVinculacion = errorVinculacion
                });
            }
            catch (Exception ex)
            {
                return CrearError(ex, result);
            }
        }

        public ErrorDto<ColaboradorClaveReestableceData> Colaborador_Clave_Reestablece(
            int CodEmpresa,
            string usuario,
            ColaboradorClaveReestableceRequest request)
        {
            var result = new ColaboradorClaveReestableceData();

            if (ValidarSolicitudUsuario(
                usuario,
                result,
                () => request is null
                    || string.IsNullOrWhiteSpace(request.EmpleadoId)
                    || request.EmpleadoId.Length > 20
                    || string.IsNullOrWhiteSpace(request.Email)
                    || request.Email.Length > 100,
                "Seleccione un empleado e ingrese el correo registrado.") is { } errorValidacion)
            {
                return errorValidacion;
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
                var cambio = connection.QueryFirstOrDefault<ColaboradorClaveReestableceRow>(
                    "spRH_Portal_Clave_Reestablece",
                    new
                    {
                        EmpleadoId = request.EmpleadoId.Trim(),
                        Email = request.Email,
                        Usuario = usuario.Trim(),
                        AppName = AppNameWeb,
                        AppVersion = NormalizarAppVersion(request.AppVersion),
                        Equipo = "WEB"
                    },
                    commandType: CommandType.StoredProcedure);

                return DbHelper.CreateOkResponse(new ColaboradorClaveReestableceData
                {
                    Cambio = cambio?.CAMBIO == 1
                });
            }
            catch (Exception ex)
            {
                return CrearError(ex, result);
            }
        }

        public ErrorDto<bool> Colaborador_Clave_Cambia(
            int CodEmpresa,
            string usuario,
            ColaboradorClaveCambiaRequest request)
        {
            if (ValidarSolicitudUsuario(
                usuario,
                false,
                () => request is null
                    || string.IsNullOrWhiteSpace(request.EmpleadoId)
                    || request.EmpleadoId.Length > 20
                    || request.ClaveActual?.Length > 100
                    || string.IsNullOrEmpty(request.ClaveNueva)
                    || request.ClaveNueva.Length is < 3 or > 100,
                "Seleccione un colaborador e ingrese una clave de 3 a 100 caracteres.")
                is { } errorValidacion)
            {
                return errorValidacion;
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
                var empleadoId = request.EmpleadoId.Trim();
                var empleadoVinculado = ObtenerVinculacion(connection, usuario)?.Empleado_ID?.Trim();

                if (!string.IsNullOrWhiteSpace(empleadoVinculado))
                {
                    if (!string.Equals(empleadoVinculado, empleadoId, StringComparison.OrdinalIgnoreCase))
                    {
                        return DbHelper.CreateErrorResponse(
                            "El colaborador solicitado no corresponde al usuario vinculado.",
                            -2,
                            false);
                    }
                }
                else if (string.IsNullOrEmpty(request.ClaveActual)
                    || ConsultarClave(connection, empleadoId, request.ClaveActual)?.Existe != 1)
                {
                    return DbHelper.CreateErrorResponse(
                        "La clave actual del portal no es válida.", -2, false);
                }

                var existeEmpleado = connection.ExecuteScalar<int>(
                    "SELECT COUNT(1) FROM RH_PERSONAS WHERE EMPLEADO_ID = @EmpleadoId",
                    new { EmpleadoId = empleadoId });

                if (existeEmpleado != 1)
                {
                    return DbHelper.CreateErrorResponse(
                        "No se encontró el colaborador solicitado.", -3, false);
                }

                connection.Execute(
                    "spRH_Portal_Clave_Cambia",
                    new
                    {
                        EmpleadoId = empleadoId,
                        Clave = request.ClaveNueva,
                        Usuario = usuario.Trim(),
                        AppName = AppNameWeb,
                        AppVersion = NormalizarAppVersion(request.AppVersion),
                        Equipo = "WEB"
                    },
                    commandType: CommandType.StoredProcedure);

                return DbHelper.CreateOkResponse(true);
            }
            catch (Exception ex)
            {
                return CrearError(ex, false);
            }
        }

        public ErrorDto<ColaboradorSolicitudConfiguracionData> Colaborador_Solicitud_Configuracion_Obtener(
            int CodEmpresa,
            string usuario,
            ColaboradorSolicitudConfiguracionRequest request)
        {
            var result = new ColaboradorSolicitudConfiguracionData();

            if (ValidarSolicitudUsuario(
                usuario,
                result,
                () => request is null
                    || string.IsNullOrWhiteSpace(request.EmpleadoId)
                    || request.EmpleadoId.Length > 20
                    || request.Clave?.Length > 100
                    || !OpcionSolicitudValida(request.Opcion)
                    || request.Tipo?.Length > 10,
                "Los datos de la solicitud no son válidos.") is { } errorValidacion)
            {
                return errorValidacion;
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
                var empleadoId = request.EmpleadoId.Trim();

                if (ValidarAccesoColaborador(connection, usuario, empleadoId, request.Clave)
                    is { } errorAcceso)
                {
                    return DbHelper.CreateErrorResponse(errorAcceso, -2, result);
                }

                var fechaActual = connection.QueryFirst<DateTime>(
                    "SELECT dbo.MyGetdate()");
                var tipos = ObtenerTiposSolicitud(connection, request.Opcion);
                var codigoSeleccionado = request.Tipo?.Trim();
                ColaboradorSolicitudTipoDetalleRow? detalleTipo = null;

                if (!string.IsNullOrWhiteSpace(codigoSeleccionado))
                {
                    if (!tipos.Any(tipo => string.Equals(
                        tipo.Codigo,
                        codigoSeleccionado,
                        StringComparison.OrdinalIgnoreCase)))
                    {
                        return DbHelper.CreateErrorResponse(
                            "El tipo de solicitud seleccionado no está disponible.",
                            -3,
                            result);
                    }

                    detalleTipo = ObtenerDetalleTipoSolicitud(
                        connection,
                        request.Opcion,
                        codigoSeleccionado);
                }

                DateTime? fechaMinima = null;
                decimal diasDisponibles = 0;

                if (request.Opcion == OpcionVacaciones)
                {
                    var vacaciones = connection.QueryFirstOrDefault<ColaboradorVacacionesInfoRow>(
                        "SELECT Dias_Disponibles, Fecha_Inicio " +
                        "FROM vRH_Vacaciones_Info WHERE Empleado_Id = @EmpleadoId",
                        new { EmpleadoId = empleadoId });

                    fechaMinima = vacaciones?.Fecha_Inicio;
                    diasDisponibles = vacaciones?.Dias_Disponibles ?? 0;
                }
                else if (request.Opcion == OpcionIncapacidades)
                {
                    fechaMinima = connection.QueryFirstOrDefault<DateTime?>(
                        "SELECT dbo.fxRH_Nomina_Inicial_Actual(COD_NOMINA) " +
                        "FROM RH_PERSONAS WHERE EMPLEADO_ID = @EmpleadoId",
                        new { EmpleadoId = empleadoId });
                }

                return DbHelper.CreateOkResponse(new ColaboradorSolicitudConfiguracionData
                {
                    Tipos = tipos,
                    FechaActual = fechaActual,
                    FechaMinima = fechaMinima,
                    DiasDisponibles = diasDisponibles,
                    RequiereAutorizacion = detalleTipo is null
                        || detalleTipo.REQUIERE_AUTORIZACION != 0,
                    HorasMaximas = detalleTipo?.PERMISO_HRS_MAX,
                    PermiteLiquidacion = detalleTipo?.PERMITE_LIQUIDACION == 1,
                    PorcentajePatrono = detalleTipo?.PORC_PATRONO
                });
            }
            catch (Exception ex)
            {
                return CrearError(ex, result);
            }
        }

        public ErrorDto<int> Colaborador_Solicitud_Dias_Obtener(
            int CodEmpresa,
            string usuario,
            ColaboradorSolicitudDiasRequest request)
        {
            if (ValidarSolicitudUsuario(
                usuario,
                0,
                () => request is null
                    || string.IsNullOrWhiteSpace(request.EmpleadoId)
                    || request.EmpleadoId.Length > 20
                    || request.Clave?.Length > 100
                    || request.Inicio.GetValueOrDefault() == default
                    || request.Corte.GetValueOrDefault() == default
                    || request.Inicio.GetValueOrDefault().Date > request.Corte.GetValueOrDefault().Date,
                "Seleccione un rango de fechas válido.") is { } errorValidacion)
            {
                return errorValidacion;
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
                var empleadoId = request.EmpleadoId.Trim();

                if (ValidarAccesoColaborador(connection, usuario, empleadoId, request.Clave)
                    is { } errorAcceso)
                {
                    return DbHelper.CreateErrorResponse(errorAcceso, -2, 0);
                }

                var dias = connection.QueryFirst<int>(
                    "SELECT dbo.fxRH_Dias_Laborales(@EmpleadoId, @Inicio, @Corte)",
                    new
                    {
                        EmpleadoId = empleadoId,
                        Inicio = request.Inicio.GetValueOrDefault().Date,
                        Corte = request.Corte.GetValueOrDefault().Date
                    });

                return DbHelper.CreateOkResponse(dias);
            }
            catch (Exception ex)
            {
                return CrearError(ex, 0);
            }
        }

        public ErrorDto<ColaboradorSolicitudRegistroData> Colaborador_Solicitud_Registrar(
            int CodEmpresa,
            string usuario,
            ColaboradorSolicitudRegistrarRequest request)
        {
            var result = new ColaboradorSolicitudRegistroData();

            if (ValidarSolicitudUsuario(
                usuario,
                result,
                () => request is null
                    || string.IsNullOrWhiteSpace(request.EmpleadoId)
                    || request.EmpleadoId.Length > 20
                    || request.Clave?.Length > 100
                    || !OpcionSolicitudValida(request.Opcion)
                    || string.IsNullOrWhiteSpace(request.Tipo)
                    || request.Tipo.Length > 10
                    || request.Notas?.Length > 1000
                    || request.Inicio.GetValueOrDefault() == default
                    || request.Corte.GetValueOrDefault() == default
                    || !EstadoSolicitudValido(request.Estado),
                "Los datos de la solicitud no son válidos.") is { } errorValidacion)
            {
                return errorValidacion;
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
                var empleadoId = request.EmpleadoId.Trim();
                var tipo = request.Tipo.Trim();

                if (ValidarAccesoColaborador(connection, usuario, empleadoId, request.Clave)
                    is { } errorAcceso)
                {
                    return DbHelper.CreateErrorResponse(errorAcceso, -2, result);
                }

                var tipos = ObtenerTiposSolicitud(connection, request.Opcion);

                if (!tipos.Any(tipoDisponible => string.Equals(
                    tipoDisponible.Codigo,
                    tipo,
                    StringComparison.OrdinalIgnoreCase)))
                {
                    return DbHelper.CreateErrorResponse(
                        "El tipo de solicitud seleccionado no está disponible.",
                        -3,
                        result);
                }

                var detalleTipo = ObtenerDetalleTipoSolicitud(
                    connection,
                    request.Opcion,
                    tipo);

                if (detalleTipo is null)
                {
                    return DbHelper.CreateErrorResponse(
                        "No se encontró la configuración del tipo de solicitud.",
                        -4,
                        result);
                }

                if (request.Estado == "A" && detalleTipo.REQUIERE_AUTORIZACION != 0)
                {
                    return DbHelper.CreateErrorResponse(
                        "Este tipo de solicitud requiere autorización.",
                        -5,
                        result);
                }

                if (request.Opcion == OpcionPermisos)
                {
                    return RegistrarPermisoSolicitud(connection, usuario, empleadoId, tipo, request, detalleTipo, result);
                }

                if (request.Inicio.GetValueOrDefault().Date > request.Corte.GetValueOrDefault().Date)
                {
                    return DbHelper.CreateErrorResponse(
                        "Error en rango de fechas.",
                        -6,
                        result);
                }

                var inicio = request.Inicio.GetValueOrDefault().Date;
                var corte = request.Corte.GetValueOrDefault().Date.AddDays(1).AddSeconds(-1);

                if (request.Opcion == OpcionVacaciones)
                {
                    return RegistrarVacacionesSolicitud(connection, usuario, empleadoId, tipo, request, (inicio, corte), result);
                }

                return RegistrarIncapacidadSolicitud(connection, usuario, empleadoId, tipo, request, (inicio, corte), result);
            }
            catch (Exception ex)
            {
                return CrearError(ex, result);
            }
        }

        public ErrorDto<List<Dictionary<string, object?>>> Colaborador_Menu_Obtener(
            int CodEmpresa,
            string usuario,
            ColaboradorMenuRequest request)
        {
            var result = new List<Dictionary<string, object?>>();

            if (ValidarSolicitudUsuario(
                usuario,
                result,
                () => request is null
                    || string.IsNullOrWhiteSpace(request.EmpleadoId)
                    || request.EmpleadoId.Length > 20
                    || request.Clave?.Length > 100,
                "Seleccione un colaborador válido.") is { } errorValidacion)
            {
                return errorValidacion;
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
                var empleadoId = request.EmpleadoId.Trim();

                var vinculacion = ObtenerVinculacion(connection, usuario);
                var empleadoVinculado = vinculacion?.Empleado_ID?.Trim();

                if (!string.IsNullOrWhiteSpace(empleadoVinculado))
                {
                    if (!string.Equals(empleadoVinculado, empleadoId, StringComparison.OrdinalIgnoreCase))
                    {
                        return DbHelper.CreateErrorResponse(
                            "El colaborador solicitado no corresponde al usuario vinculado.",
                            -2,
                            result);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(request.Clave))
                    {
                        return DbHelper.CreateErrorResponse(
                            "La clave del portal es requerida para consultar estas secciones.",
                            -2,
                            result);
                    }

                    var validacion = ConsultarClave(connection, empleadoId, request.Clave);

                    if (validacion?.Existe != 1)
                    {
                        return DbHelper.CreateErrorResponse(
                            "La clave registrada no es válida.",
                            -2,
                            result);
                    }
                }

                var identificacion = connection.QueryFirstOrDefault<string>(
                    SqlIdentificacionColaborador,
                    new { EmpleadoId = empleadoId });

                if (string.IsNullOrWhiteSpace(identificacion))
                {
                    return DbHelper.CreateErrorResponse(
                        "No se encontró la identificación del colaborador.",
                        -3,
                        result);
                }

                var (sql, parametros) = CrearConsultaMenu(
                    request,
                    CodEmpresa,
                    empleadoId,
                    identificacion.Trim());

                if (string.IsNullOrWhiteSpace(sql))
                {
                    return DbHelper.CreateErrorResponse(
                        "La opción del portal no es válida.",
                        -4,
                        result);
                }

                var rows = connection.Query(sql, parametros).ToList();
                return DbHelper.CreateOkResponse(rows
                    .Select(row => ((IDictionary<string, object?>)row)
                        .ToDictionary(
                            column => char.ToLowerInvariant(column.Key[0]) + column.Key[1..],
                            column => column.Value))
                    .ToList());
            }
            catch (Exception ex)
            {
                return CrearError(ex, result);
            }
        }

        public ErrorDto<bool> Colaborador_Autorizacion_Registrar(
            int CodEmpresa,
            string usuario,
            ColaboradorAutorizacionRequest request)
        {
            if (ValidarSolicitudUsuario(
                usuario,
                false,
                () => request is null
                    || string.IsNullOrWhiteSpace(request.EmpleadoId)
                    || request.EmpleadoId.Length > 20
                    || request.Clave?.Length > 100
                    || string.IsNullOrWhiteSpace(request.BoletaId)
                    || request.BoletaId.Length > 30
                    || request.Tipo is not ("P" or "V" or "I")
                    || request.EstadoActual is not ("S" or "A" or "D")
                    || request.EstadoNuevo is not ("A" or "D")
                    || request.EstadoActual == request.EstadoNuevo,
                "Los datos de autorización no son válidos.") is { } errorValidacion)
            {
                return errorValidacion;
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
                var empleadoId = request.EmpleadoId.Trim();
                if (ValidarAccesoAutorizador(connection, usuario, empleadoId, request)
                    is { } errorAcceso)
                {
                    return errorAcceso;
                }

                string? ConsultarEstado()
                {
                    var parametros = new
                    {
                        BoletaId = request.BoletaId.Trim(),
                        AutorizadorId = empleadoId
                    };

                    return request.Tipo switch
                    {
                        "P" => connection.QueryFirstOrDefault<string>(
                            """
                            SELECT Estado
                            FROM vRH_Boleta_Permisos
                            WHERE Boleta_Id = @BoletaId
                              AND dbo.fxRH_Autorizador_Valida(Empleado_ID, @AutorizadorId) = 1
                            """,
                            parametros),
                        "V" => connection.QueryFirstOrDefault<string>(
                            """
                            SELECT Estado
                            FROM vRH_Boleta_Vacaciones
                            WHERE Boleta_Id = @BoletaId
                              AND dbo.fxRH_Autorizador_Valida(Empleado_ID, @AutorizadorId) = 1
                            """,
                            parametros),
                        _ => connection.QueryFirstOrDefault<string>(
                            """
                            SELECT Estado
                            FROM vRH_Boleta_Incapacidades
                            WHERE Boleta_Id = @BoletaId
                              AND dbo.fxRH_Autorizador_Valida(Empleado_ID, @AutorizadorId) = 1
                            """,
                            parametros)
                    };
                }

                var estado = ConsultarEstado();

                if (estado != request.EstadoActual)
                {
                    return DbHelper.CreateErrorResponse(
                        "La boleta ya no está disponible en el estado consultado. Actualice la lista.",
                        -3,
                        false);
                }

                connection.Execute(
                    "spRH_Autorizaciones_Registro",
                    new
                    {
                        AutorizadorId = empleadoId,
                        Tipo = request.Tipo,
                        BoletaId = request.BoletaId.Trim(),
                        Usuario = usuario.Trim(),
                        Estado = request.EstadoNuevo,
                        AppCod = AppCodProGrX
                    },
                    commandType: CommandType.StoredProcedure);

                var estadoRegistrado = ConsultarEstado();

                if (estadoRegistrado != request.EstadoNuevo)
                {
                    return DbHelper.CreateErrorResponse(
                        "La boleta no cambió de estado. Revise la nómina y actualice la lista.",
                        -4,
                        false);
                }

                _securityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario.Trim(),
                    Movimiento = "Aplica",
                    DetalleMovimiento =
                        $"{(request.EstadoNuevo == "A" ? "Autoriza" : "Deniega")} de Boleta Id:" +
                        request.BoletaId.Trim() + "..Autorizador Id: " + empleadoId,
                    Modulo = 23
                });

                return DbHelper.CreateOkResponse(true);
            }
            catch (Exception ex)
            {
                return CrearError(ex, false);
            }
        }

        public ErrorDto<bool> Colaborador_Traslado_Gestionar(
            int CodEmpresa,
            string usuario,
            ColaboradorTrasladoGestionRequest request)
        {
            if (ValidarSolicitudUsuario(
                usuario,
                false,
                () => request is null
                    || string.IsNullOrWhiteSpace(request.EmpleadoId)
                    || request.EmpleadoId.Length > 20
                    || request.Clave?.Length > 100
                    || string.IsNullOrWhiteSpace(request.BoletaId)
                    || request.BoletaId.Length > 20
                    || request.Accion is not (AccionRecibir or "descartar"),
                "Los datos del traslado no son válidos.") is { } errorValidacion)
            {
                return errorValidacion;
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
                var empleadoId = request.EmpleadoId.Trim();
                var errorAcceso = ValidarAccesoColaborador(
                    connection,
                    usuario,
                    empleadoId,
                    request.Clave);

                if (errorAcceso is not null)
                {
                    return DbHelper.CreateErrorResponse(errorAcceso, -2, false);
                }

                var identificacion = connection.QueryFirstOrDefault<string>(
                    SqlIdentificacionColaborador,
                    new { EmpleadoId = empleadoId });

                if (string.IsNullOrWhiteSpace(identificacion))
                {
                    return DbHelper.CreateErrorResponse(
                        "No se encontró la identificación del colaborador.",
                        -3,
                        false);
                }

                var boletaId = request.BoletaId.Trim();
                using var transaction = connection.BeginTransaction();
                var traslado = connection.QueryFirstOrDefault<ColaboradorTrasladoEstadoRow>(
                    "SELECT IDENTIFICACION AS IdentificacionOrigen, " +
                    "IDENTIFICACION_DESTINO AS IdentificacionDestino, ESTADO AS Estado " +
                    "FROM ACTIVOS_TRASLADOS WITH (UPDLOCK, HOLDLOCK) " +
                    "WHERE COD_TRASLADO = @BoletaId",
                    new { BoletaId = boletaId },
                    transaction);

                var identificacionBoleta = request.Accion == AccionRecibir
                    ? traslado?.IdentificacionDestino
                    : traslado?.IdentificacionOrigen;

                if (traslado?.Estado != "S"
                    || !string.Equals(
                        identificacionBoleta?.Trim(),
                        identificacion.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    return DbHelper.CreateErrorResponse(
                        "La boleta solicitada no está disponible para este colaborador. Actualice la lista.",
                        -4,
                        false);
                }

                var procedimiento = request.Accion == AccionRecibir
                    ? "spActivos_Responsable_Cambio_Procesa"
                    : "spActivos_Responsable_Cambio_Descarta";
                var parametros = request.Accion == AccionRecibir
                    ? new { Boleta = boletaId, Usuario = usuario.Trim() }
                    : (object)new { BoletaId = boletaId, Usuario = usuario.Trim() };
                var resultado = connection.QueryFirstOrDefault<ColaboradorTrasladoResultadoRow>(
                    procedimiento,
                    parametros,
                    transaction,
                    commandType: CommandType.StoredProcedure);

                if (resultado?.Pass != 1)
                {
                    return DbHelper.CreateErrorResponse(
                        resultado?.Mensaje ?? "La boleta no pudo procesarse.",
                        -5,
                        false);
                }

                var estadoFinal = connection.QueryFirstOrDefault<string>(
                    "SELECT ESTADO FROM ACTIVOS_TRASLADOS WHERE COD_TRASLADO = @BoletaId",
                    new { BoletaId = boletaId },
                    transaction);
                var estadoEsperado = request.Accion == AccionRecibir ? "P" : "D";

                if (estadoFinal != estadoEsperado)
                {
                    return DbHelper.CreateErrorResponse(
                        "La boleta no cambió de estado. Actualice la lista.",
                        -6,
                        false);
                }

                transaction.Commit();
                _securityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario.Trim(),
                    Movimiento = request.Accion == AccionRecibir ? "Procesa" : "Descarta",
                    DetalleMovimiento = "Boleta de Cambio Responsable: " + boletaId,
                    Modulo = 36
                });

                return DbHelper.CreateOkResponse(true);
            }
            catch (Exception ex)
            {
                return CrearError(ex, false);
            }
        }

        public ErrorDto<ColaboradorTrasladoConfiguracionData> Colaborador_Traslado_Configuracion_Obtener(
            int CodEmpresa,
            string usuario,
            ColaboradorTrasladoAccesoRequest request)
        {
            var result = new ColaboradorTrasladoConfiguracionData();

            if (ValidarSolicitudUsuario(
                usuario,
                result,
                () => request is null
                    || string.IsNullOrWhiteSpace(request.EmpleadoId)
                    || request.EmpleadoId.Length > 20
                    || request.Clave?.Length > 100
                    || request.FiltroDestino?.Length > 100,
                "Los datos de consulta del traslado no son válidos.") is { } errorValidacion)
            {
                return errorValidacion;
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
                var empleadoId = request.EmpleadoId.Trim();
                var errorAcceso = ValidarAccesoColaborador(
                    connection,
                    usuario,
                    empleadoId,
                    request.Clave);

                if (errorAcceso is not null)
                {
                    return DbHelper.CreateErrorResponse(errorAcceso, -2, result);
                }

                var identificacion = connection.QueryFirstOrDefault<string>(
                    SqlIdentificacionColaborador,
                    new { EmpleadoId = empleadoId });

                if (string.IsNullOrWhiteSpace(identificacion))
                {
                    return DbHelper.CreateErrorResponse(
                        "No se encontró la identificación del colaborador.",
                        -3,
                        result);
                }

                var filtro = request.FiltroDestino?.Trim() ?? string.Empty;
                result.Motivos = connection.Query<ColaboradorTrasladoOpcionData>(
                    "SELECT RTRIM(COD_MOTIVO) AS Codigo, RTRIM(Descripcion) AS Descripcion " +
                    "FROM ACTIVOS_TRASLADOS_MOTIVOS WHERE ACTIVO = 1 ORDER BY COD_MOTIVO")
                    .ToList();
                result.Destinatarios = connection.Query<ColaboradorTrasladoOpcionData>(
                    "SELECT TOP (50) IDENTIFICACION AS Codigo, Nombre AS Descripcion " +
                    "FROM ACTIVOS_PERSONAS " +
                    "WHERE IDENTIFICACION <> @Identificacion " +
                    "AND (@Filtro = '' OR IDENTIFICACION LIKE @Busqueda OR Nombre LIKE @Busqueda) " +
                    "ORDER BY Nombre",
                    new
                    {
                        Identificacion = identificacion.Trim(),
                        Filtro = filtro,
                        Busqueda = $"%{filtro}%"
                    }).ToList();
                result.Placas = connection.Query<ColaboradorTrasladoPlacaRow>(
                    "spActivos_Responsable_Cambio_Consulta_Placas",
                    new
                    {
                        BoletaId = string.Empty,
                        Identificacion = identificacion.Trim(),
                        Usuario = usuario.Trim(),
                        ModoRecepcion = 0
                    },
                    commandType: CommandType.StoredProcedure)
                    .Select(placa => new ColaboradorTrasladoPlacaData
                    {
                        NumPlaca = placa.NUM_PLACA?.Trim() ?? string.Empty,
                        Descripcion = placa.Descripcion?.Trim() ?? string.Empty,
                        DepreciacionAc = placa.DEPRECIACION_AC,
                        DepreciacionMes = placa.DEPRECIACION_MES,
                        ValorLibros = placa.VALOR_LIBROS,
                        Asignado = placa.asignado == 1
                    }).ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return CrearError(ex, result);
            }
        }

        public ErrorDto<ColaboradorTrasladoRegistroData> Colaborador_Traslado_Registrar(
            int CodEmpresa,
            string usuario,
            ColaboradorTrasladoRegistrarRequest request)
        {
            var result = new ColaboradorTrasladoRegistroData();

            if (ValidarSolicitudUsuario(
                usuario,
                result,
                () => request is null
                    || string.IsNullOrWhiteSpace(request.EmpleadoId)
                    || request.EmpleadoId.Length > 20
                    || request.Clave?.Length > 100
                    || string.IsNullOrWhiteSpace(request.MotivoId)
                    || request.MotivoId.Length > 10
                    || string.IsNullOrWhiteSpace(request.DestinoId)
                    || request.DestinoId.Length > 20
                    || string.IsNullOrWhiteSpace(request.Notas)
                    || request.Notas.Trim().Length < 10
                    || request.Notas.Length > 1000
                    || request.Placas is null
                    || request.Placas.Count == 0
                    || request.Placas.Any(placa => string.IsNullOrWhiteSpace(placa)
                        || placa.Length > 30),
                "Los datos del traslado no son válidos.") is { } errorValidacion)
            {
                return errorValidacion;
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
                var empleadoId = request.EmpleadoId.Trim();
                var errorAcceso = ValidarAccesoColaborador(
                    connection,
                    usuario,
                    empleadoId,
                    request.Clave);

                if (errorAcceso is not null)
                {
                    return DbHelper.CreateErrorResponse(errorAcceso, -2, result);
                }

                var identificacion = connection.QueryFirstOrDefault<string>(
                    SqlIdentificacionColaborador,
                    new { EmpleadoId = empleadoId });
                var destinoId = request.DestinoId.Trim();

                if (string.IsNullOrWhiteSpace(identificacion)
                    || string.Equals(identificacion.Trim(), destinoId, StringComparison.OrdinalIgnoreCase))
                {
                    return DbHelper.CreateErrorResponse(
                        "Seleccione un responsable destino distinto al actual.",
                        -3,
                        result);
                }

                var motivoDisponible = connection.QueryFirstOrDefault<int>(
                    "SELECT COUNT(1) FROM ACTIVOS_TRASLADOS_MOTIVOS " +
                    "WHERE COD_MOTIVO = @MotivoId AND ACTIVO = 1",
                    new { MotivoId = request.MotivoId.Trim() }) == 1;
                var destinoDisponible = connection.QueryFirstOrDefault<int>(
                    "SELECT COUNT(1) FROM vActivos_Personas WHERE IDENTIFICACION = @DestinoId",
                    new { DestinoId = destinoId }) == 1;

                if (!motivoDisponible || !destinoDisponible)
                {
                    return DbHelper.CreateErrorResponse(
                        "El motivo o el responsable destino ya no está disponible.",
                        -4,
                        result);
                }

                var placas = request.Placas
                    .Select(placa => placa.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                var placasDisponibles = connection.Query<ColaboradorTrasladoPlacaRow>(
                    "spActivos_Responsable_Cambio_Consulta_Placas",
                    new
                    {
                        BoletaId = string.Empty,
                        Identificacion = identificacion.Trim(),
                        Usuario = usuario.Trim(),
                        ModoRecepcion = 0
                    },
                    commandType: CommandType.StoredProcedure)
                    .Select(placa => placa.NUM_PLACA?.Trim() ?? string.Empty)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (placas.Count != request.Placas.Count
                    || placas.Any(placa => !placasDisponibles.Contains(placa)))
                {
                    return DbHelper.CreateErrorResponse(
                        "La selección de placas ya no está disponible. Actualice la lista.",
                        -5,
                        result);
                }

                return GuardarBoletaTraslado(
                    CodEmpresa, connection, usuario, identificacion, request, placas, result);
            }
            catch (Exception ex)
            {
                return CrearError(ex, result);
            }
        }

        private ErrorDto<ColaboradorTrasladoRegistroData> GuardarBoletaTraslado(
            int CodEmpresa,
            IDbConnection connection,
            string usuario,
            string identificacion,
            ColaboradorTrasladoRegistrarRequest request,
            List<string> placas,
            ColaboradorTrasladoRegistroData result)
        {
            var destinoId = request.DestinoId.Trim();
            using var transaction = connection.BeginTransaction();
            var placasDelResponsable = connection.QueryFirst<int>(
                "SELECT COUNT(1) FROM ACTIVOS_PRINCIPAL WITH (UPDLOCK, HOLDLOCK) " +
                "WHERE NUM_PLACA IN @Placas AND IDENTIFICACION = @Identificacion",
                new
                {
                    Placas = placas,
                    Identificacion = identificacion.Trim()
                },
                transaction);

            if (placasDelResponsable != placas.Count)
            {
                return DbHelper.CreateErrorResponse(
                    "Una o más placas ya no pertenecen al colaborador. Actualice la lista.",
                    -5,
                    result);
            }

            var fechaAplicacion = connection.QueryFirst<DateTime>(
                "SELECT CAST(GETDATE() AS date)",
                transaction: transaction);
            var boleta = connection.QueryFirstOrDefault<ColaboradorTrasladoResultadoRow>(
                "spActivos_Responsable_Cambio_Boleta_Add",
                new
                {
                    BoletaId = string.Empty,
                    MotivoId = request.MotivoId.Trim(),
                    Notas = request.Notas.Trim(),
                    A_Id = identificacion.Trim(),
                    N_Id = destinoId,
                    Usuario = usuario.Trim(),
                    FechaAplicacion = fechaAplicacion
                },
                transaction,
                commandType: CommandType.StoredProcedure);

            if (boleta?.Pass != 1 || string.IsNullOrWhiteSpace(boleta.Boleta))
            {
                return DbHelper.CreateErrorResponse(
                    boleta?.Mensaje ?? "No fue posible registrar la boleta.",
                    -6,
                    result);
            }

            var boletaId = boleta.Boleta.Trim();

            for (var index = 0; index < placas.Count; index++)
            {
                var detalle = connection.QueryFirstOrDefault<ColaboradorTrasladoResultadoRow>(
                    "spActivos_Responsable_Cambio_Boleta_Placas",
                    new
                    {
                        BoletaId = boletaId,
                        Placa = placas[index],
                        Usuario = usuario.Trim(),
                        Inicial = index == 0 ? 1 : 0
                    },
                    transaction,
                    commandType: CommandType.StoredProcedure);

                if (detalle?.Pass != 1)
                {
                    return DbHelper.CreateErrorResponse(
                        detalle?.Mensaje ?? "No fue posible agregar una placa a la boleta.",
                        -7,
                        result);
                }
            }

            var placasRegistradas = connection.QueryFirst<int>(
                "SELECT COUNT(1) FROM ACTIVOS_TRASLADO_RESPONSABLES " +
                "WHERE COD_TRASLADO = @BoletaId",
                new { BoletaId = boletaId },
                transaction);

            if (placasRegistradas != placas.Count)
            {
                return DbHelper.CreateErrorResponse(
                    "No se registraron todas las placas. La boleta no se guardó.",
                    -8,
                    result);
            }

            transaction.Commit();
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = usuario.Trim(),
                Movimiento = "Registra",
                DetalleMovimiento = "Boleta de Cambio Responsable: " + boletaId,
                Modulo = 36
            });

            return DbHelper.CreateOkResponse(new ColaboradorTrasladoRegistroData
            {
                BoletaId = boletaId
            });
        }

        public ErrorDto<bool> Colaborador_Reporte_Validar(
            int codEmpresa,
            string usuario,
            ColaboradorReporteRequest request)
        {
            if (SolicitudReporteInvalida(usuario, request))
            {
                return DbHelper.CreateErrorResponse("La boleta solicitada no es válida.", -2, false);
            }

            var sql = request.Opcion switch
            {
                OpcionBoletasPago => "SELECT TOP (1) 1 FROM dbo.vRH_Nomina_Boleta_Encabezado " +
                    "WHERE EMPLEADO_ID = @EmpleadoId AND COD_NOMINA = @CodNomina AND NOMINA_NUM = @NominaNum",
                "vacaciones" => "SELECT TOP (1) 1 FROM dbo.vRH_Boleta_Vacaciones " +
                    "WHERE EMPLEADO_ID = @EmpleadoId AND BOLETA_ID = @BoletaId",
                "incapacidades" => "SELECT TOP (1) 1 FROM dbo.vRH_Boleta_Incapacidades " +
                    "WHERE EMPLEADO_ID = @EmpleadoId AND BOLETA_ID = @BoletaId",
                "permisos" => "SELECT TOP (1) 1 FROM dbo.vRH_Boleta_Permisos " +
                    "WHERE EMPLEADO_ID = @EmpleadoId AND BOLETA_ID = @BoletaId",
                "accionesPersonal" => "SELECT TOP (1) 1 FROM dbo.vRH_Accion_Personal " +
                    "WHERE EMPLEADO_ID = @EmpleadoId AND COD_ACCION = @BoletaId",
                OpcionTraslados => "SELECT TOP (1) 1 FROM ACTIVOS_TRASLADOS " +
                    "WHERE COD_TRASLADO = @BoletaId AND " +
                    "(IDENTIFICACION = @Identificacion OR IDENTIFICACION_DESTINO = @Identificacion)",
                _ => null
            };

            if (sql is null
                || (request.Opcion == OpcionBoletasPago
                    && (string.IsNullOrWhiteSpace(request.CodNomina) || request.NominaNum is null or <= 0))
                || (request.Opcion != OpcionBoletasPago && string.IsNullOrWhiteSpace(request.BoletaId)))
            {
                return DbHelper.CreateErrorResponse("La boleta solicitada no es válida.", -2, false);
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var empleadoId = request.EmpleadoId.Trim();
                var errorAcceso = ValidarAccesoColaborador(connection, usuario, empleadoId, request.Clave);
                if (errorAcceso is not null)
                {
                    return DbHelper.CreateErrorResponse(errorAcceso, -2, false);
                }

                var identificacion = request.Opcion == OpcionTraslados
                    ? connection.QueryFirstOrDefault<string>(
                        SqlIdentificacionColaborador,
                        new { EmpleadoId = empleadoId })?.Trim()
                    : null;

                if (request.Opcion == OpcionTraslados && string.IsNullOrWhiteSpace(identificacion))
                {
                    return DbHelper.CreateErrorResponse("No se encontró el colaborador.", -3, false);
                }

                var existe = connection.QueryFirstOrDefault<int?>(sql, new
                {
                    EmpleadoId = empleadoId,
                    BoletaId = request.BoletaId.Trim(),
                    CodNomina = request.CodNomina.Trim(),
                    request.NominaNum,
                    Identificacion = identificacion
                });

                return existe == 1
                    ? DbHelper.CreateOkResponse(true)
                    : DbHelper.CreateErrorResponse(
                        "La boleta no está disponible para este colaborador.", -3, false);
            }
            catch (Exception ex)
            {
                return CrearError(ex, false);
            }
        }

        private static bool SolicitudReporteInvalida(
            string usuario,
            ColaboradorReporteRequest? request)
        {
            return string.IsNullOrWhiteSpace(usuario) || request is null
                || string.IsNullOrWhiteSpace(request.EmpleadoId)
                || request.EmpleadoId.Length > 20
                || request.Clave?.Length > 100
                || request.BoletaId is null or { Length: > 30 }
                || request.CodNomina is null or { Length: > 20 };
        }

        public ErrorDto<List<ColaboradorTrasladoPlacaData>> Colaborador_Traslado_Detalle_Obtener(
            int codEmpresa,
            string usuario,
            ColaboradorTrasladoDetalleRequest request)
        {
            var result = new List<ColaboradorTrasladoPlacaData>();
            var validacion = Colaborador_Reporte_Validar(codEmpresa, usuario,
                new ColaboradorReporteRequest
                {
                    EmpleadoId = request.EmpleadoId,
                    Clave = request.Clave,
                    Opcion = OpcionTraslados,
                    BoletaId = request.BoletaId
                });

            if (validacion.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    validacion.Description ?? "No fue posible validar la boleta.",
                    validacion.Code ?? -1,
                    result);
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var identificacion = connection.QueryFirst<string>(
                    SqlIdentificacionColaborador,
                    new { EmpleadoId = request.EmpleadoId.Trim() });
                var placas = connection.Query<ColaboradorTrasladoPlacaRow>(
                    "spActivos_Responsable_Cambio_Consulta_Placas",
                    new
                    {
                        BoletaId = request.BoletaId.Trim(),
                        Identificacion = identificacion.Trim(),
                        Usuario = usuario.Trim(),
                        ModoRecepcion = 1
                    },
                    commandType: CommandType.StoredProcedure);

                result = placas.Select(placa => new ColaboradorTrasladoPlacaData
                {
                    NumPlaca = placa.NUM_PLACA?.Trim() ?? string.Empty,
                    Descripcion = placa.Descripcion?.Trim() ?? string.Empty,
                    DepreciacionAc = placa.DEPRECIACION_AC,
                    DepreciacionMes = placa.DEPRECIACION_MES,
                    ValorLibros = placa.VALOR_LIBROS
                }).ToList();

                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return CrearError(ex, result);
            }
        }

        public ErrorDto<bool> Colaborador_Reporte_Bitacora(
            int codEmpresa,
            string usuario,
            ColaboradorReporteRequest request)
        {
            var accion = request.Opcion switch
            {
                OpcionBoletasPago => "10",
                "vacaciones" => "11",
                "permisos" => "12",
                "incapacidades" => "13",
                "accionesPersonal" => "14",
                _ => null
            };

            if (accion is null)
            {
                return DbHelper.CreateErrorResponse("El tipo de boleta no es válido.", -2, false);
            }

            try
            {
                using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
                connection.Execute(
                    "EXEC spRH_Portal_Bitacora @Accion, @EmpleadoId, @Usuario, " +
                    "@Detalle, @AppName, @AppVersion, @Equipo",
                    new
                    {
                        Accion = accion,
                        EmpleadoId = request.EmpleadoId.Trim(),
                        Usuario = usuario.Trim(),
                        Detalle = request.Opcion == OpcionBoletasPago
                            ? $"Nomina Id: {request.NominaNum}"
                            : $"Boleta Id: {request.BoletaId.Trim()}",
                        AppName = AppNameWeb,
                        AppVersion = NormalizarAppVersion(request.AppVersion),
                        Equipo = "WEB"
                    });
                return DbHelper.CreateOkResponse(true);
            }
            catch (Exception ex)
            {
                return CrearError(ex, false);
            }
        }

        private static ErrorDto<bool>? ValidarAccesoAutorizador(
            IDbConnection connection,
            string usuario,
            string empleadoId,
            ColaboradorAutorizacionRequest request)
        {
            var mensaje = ValidarAccesoColaborador(
                connection,
                usuario,
                empleadoId,
                request.Clave);

            return mensaje is null
                ? null
                : DbHelper.CreateErrorResponse(mensaje, -2, false);
        }

        private static string? ValidarAccesoColaborador(
            IDbConnection connection,
            string usuario,
            string empleadoId,
            string? clave)
        {
            var empleadoVinculado = ObtenerVinculacion(connection, usuario)?.Empleado_ID?.Trim();

            if (!string.IsNullOrWhiteSpace(empleadoVinculado)
                && !string.Equals(empleadoVinculado, empleadoId, StringComparison.OrdinalIgnoreCase))
            {
                return "El colaborador solicitado no corresponde al usuario vinculado.";
            }

            if (string.IsNullOrWhiteSpace(empleadoVinculado)
                && (string.IsNullOrEmpty(clave)
                    || ConsultarClave(connection, empleadoId, clave)?.Existe != 1))
            {
                return "La clave registrada no es válida.";
            }

            return null;
        }

        private static ErrorDto<ColaboradorSolicitudRegistroData> RegistrarPermisoSolicitud(
            IDbConnection connection,
            string usuario,
            string empleadoId,
            string tipo,
            ColaboradorSolicitudRegistrarRequest request,
            ColaboradorSolicitudTipoDetalleRow detalleTipo,
            ColaboradorSolicitudRegistroData result)
        {
            if (request.Inicio.GetValueOrDefault() > request.Corte.GetValueOrDefault())
            {
                return DbHelper.CreateErrorResponse(
                    "Error en rango de horas.",
                    -6,
                    result);
            }

                if (request.Horas.GetValueOrDefault() < 0
                    || detalleTipo.PERMISO_HRS_MAX is null
                    || request.Horas.GetValueOrDefault() > detalleTipo.PERMISO_HRS_MAX)
                {
                    return DbHelper.CreateErrorResponse(
                        "Las horas de permiso exceden el total permitido.",
                        -7,
                        result);
                }

                var registroPermiso = connection.QueryFirstOrDefault<ColaboradorSolicitudRegistroRow>(
                    "spRH_Permisos_Registro",
                    new
                    {
                        EmpleadoId = empleadoId,
                        Tipo = tipo,
                        Notas = request.Notas ?? string.Empty,
                        Usuario = usuario.Trim(),
                        Inicio = request.Inicio.GetValueOrDefault(),
                        Corte = request.Corte.GetValueOrDefault(),
                        Horas = request.Horas.GetValueOrDefault(),
                        PermisoFecha = request.Inicio.GetValueOrDefault().Date,
                        Estado = request.Estado,
                        AutorizaId = (string?)null,
                        AppCod = AppCodProGrX
                    },
                    commandType: CommandType.StoredProcedure);

                return ObtenerRespuestaRegistro(registroPermiso, result);
        }

        private static ErrorDto<ColaboradorSolicitudRegistroData> RegistrarVacacionesSolicitud(
            IDbConnection connection,
            string usuario,
            string empleadoId,
            string tipo,
            ColaboradorSolicitudRegistrarRequest request,
            (DateTime Inicio, DateTime Corte) periodo,
            ColaboradorSolicitudRegistroData result)
        {
            var (inicio, corte) = periodo;

            if (request.Dias.GetValueOrDefault() < 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Días de vacaciones inválidos.",
                    -7,
                    result);
            }

                var nomina = connection.QueryFirstOrDefault<string>(
                    "SELECT COD_NOMINA FROM RH_PERSONAS WHERE EMPLEADO_ID = @EmpleadoId",
                    new { EmpleadoId = empleadoId });
                var fechasValidas = connection.QueryFirstOrDefault<int?>(
                    "SELECT dbo.fxRH_Vacaciones_Valida(@Nomina, @EmpleadoId, @Inicio, @Corte)",
                    new
                    {
                        Nomina = nomina,
                        EmpleadoId = empleadoId,
                        Inicio = inicio,
                        Corte = corte
                    });

                if (fechasValidas != 1)
                {
                    return DbHelper.CreateErrorResponse(
                        "Existe conflicto de fechas de disfrute con alguna otra boleta procesada o con una Nómina ya ejecutada!",
                        -8,
                        result);
                }

                var vacaciones = connection.QueryFirstOrDefault<ColaboradorVacacionesInfoRow>(
                    "SELECT Dias_Disponibles FROM vRH_Vacaciones_Info " +
                    "WHERE Empleado_Id = @EmpleadoId",
                    new { EmpleadoId = empleadoId });
                var registroVacaciones = connection.QueryFirstOrDefault<ColaboradorSolicitudRegistroRow>(
                    "spRH_Vacaciones_Registro",
                    new
                    {
                        EmpleadoId = empleadoId,
                        Tipo = tipo,
                        Notas = request.Notas ?? string.Empty,
                        Usuario = usuario.Trim(),
                        Inicio = inicio,
                        Corte = corte,
                        D_Disfrutados = request.Dias.GetValueOrDefault(),
                        D_Disponibles = vacaciones?.Dias_Disponibles ?? 0,
                        LiquidaID = request.LiquidaId.GetValueOrDefault(),
                        Estado = request.Estado,
                        AutorizaId = (string?)null,
                        AppCod = AppCodProGrX
                    },
                    commandType: CommandType.StoredProcedure);

                return ObtenerRespuestaRegistro(registroVacaciones, result);
        }

        private static ErrorDto<ColaboradorSolicitudRegistroData> RegistrarIncapacidadSolicitud(
            IDbConnection connection,
            string usuario,
            string empleadoId,
            string tipo,
            ColaboradorSolicitudRegistrarRequest request,
            (DateTime Inicio, DateTime Corte) periodo,
            ColaboradorSolicitudRegistroData result)
        {
            var (inicio, corte) = periodo;

            if (request.Dias.GetValueOrDefault() < 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Días de incapacidad inválidos.",
                    -7,
                    result);
            }

                var fechaMinima = connection.QueryFirstOrDefault<DateTime?>(
                    "SELECT dbo.fxRH_Nomina_Inicial_Actual(COD_NOMINA) " +
                    "FROM RH_PERSONAS WHERE EMPLEADO_ID = @EmpleadoId",
                    new { EmpleadoId = empleadoId });

                if (fechaMinima.HasValue && inicio < fechaMinima.Value.Date)
                {
                    return DbHelper.CreateErrorResponse(
                        "La fecha inicial no puede ser anterior al inicio de la nómina.",
                        -8,
                        result);
                }

                var registroIncapacidad = connection.QueryFirstOrDefault<ColaboradorSolicitudRegistroRow>(
                    "spRH_Incapacidades_Registro",
                    new
                    {
                        EmpleadoId = empleadoId,
                        Tipo = tipo,
                        Notas = request.Notas ?? string.Empty,
                        Usuario = usuario.Trim(),
                        Inicio = inicio,
                        Corte = corte,
                        Dias = request.Dias.GetValueOrDefault(),
                        Porcentaje = request.PorcentajePatrono.GetValueOrDefault(),
                        Estado = request.Estado,
                        AutorizaId = (string?)null,
                        AppCod = AppCodProGrX
                    },
                    commandType: CommandType.StoredProcedure);

                return ObtenerRespuestaRegistro(registroIncapacidad, result);
        }

        private static bool OpcionSolicitudValida(string? opcion)
        {
            return opcion is OpcionVacaciones or OpcionPermisos or OpcionIncapacidades;
        }

        private static bool EstadoSolicitudValido(string? estado)
        {
            return estado is "S" or "A";
        }

        private static List<ColaboradorSolicitudTipoData> ObtenerTiposSolicitud(
            IDbConnection connection,
            string opcion)
        {
            var procedimiento = opcion switch
            {
                OpcionVacaciones => "spRH_Portal_Vacaciones_Tipos",
                OpcionPermisos => "spRH_Portal_Permisos_Tipos",
                _ => "spRH_Portal_Incapacidades_Tipos"
            };

            return connection.Query<ColaboradorSolicitudTipoRow>(
                    procedimiento,
                    commandType: CommandType.StoredProcedure)
                .Where(tipo => tipo.IdX is not null
                    && !string.IsNullOrWhiteSpace(tipo.ItmX))
                .Select(tipo => new ColaboradorSolicitudTipoData
                {
                    Codigo = tipo.IdX!.ToString()?.Trim() ?? string.Empty,
                    Descripcion = tipo.ItmX!.Trim()
                })
                .ToList();
        }

        private static ColaboradorSolicitudTipoDetalleRow? ObtenerDetalleTipoSolicitud(
            IDbConnection connection,
            string opcion,
            string tipo)
        {
            return opcion switch
            {
                OpcionVacaciones => connection.QueryFirstOrDefault<ColaboradorSolicitudTipoDetalleRow>(
                    "EXEC spRH_Portal_Vacaciones_Tipos @Tipo",
                    new { Tipo = tipo }),
                OpcionPermisos => connection.QueryFirstOrDefault<ColaboradorSolicitudTipoDetalleRow>(
                    "SELECT REQUIERE_AUTORIZACION, PERMISO_HRS_MAX " +
                    "FROM RH_PERMISOS_TIPOS WHERE PERMISO_TIPO = @Tipo",
                    new { Tipo = tipo }),
                OpcionIncapacidades => connection.QueryFirstOrDefault<ColaboradorSolicitudTipoDetalleRow>(
                    "EXEC spRH_Portal_Incapacidades_Tipos @Tipo",
                    new { Tipo = tipo }),
                _ => null
            };
        }

        private static ErrorDto<ColaboradorSolicitudRegistroData> ObtenerRespuestaRegistro(
            ColaboradorSolicitudRegistroRow? registro,
            ColaboradorSolicitudRegistroData result)
        {
            if (string.IsNullOrWhiteSpace(registro?.BoletaId))
            {
                return DbHelper.CreateErrorResponse(
                    "El procedimiento no devolvió el número de boleta.",
                    -9,
                    result);
            }

            return DbHelper.CreateOkResponse(new ColaboradorSolicitudRegistroData
            {
                BoletaId = registro.BoletaId.Trim()
            });
        }

        private static (string Sql, object Parametros) CrearConsultaMenu(
            ColaboradorMenuRequest request,
            int codEmpresa,
            string empleadoId,
            string identificacion)
        {
            var parametros = new
            {
                EmpleadoId = empleadoId,
                Identificacion = identificacion,
                Estado = NormalizarEstadoAutorizacion(request.Estado),
                FechaInicio = request.FechaInicio?.Date ?? DateTime.Today,
                FechaCorte = request.FechaCorte?.Date ?? DateTime.Today,
                ClienteCod = codEmpresa
            };

            return request.Opcion switch
            {
                "familiares" => (
                    "SELECT Identificacion AS identificacion, Nombre AS nombre, " +
                    "Parentesco_Desc AS parentesco " +
                    "FROM vRH_Personas_Familiares WHERE Empleado_Id = @EmpleadoId " +
                    "ORDER BY Nombre",
                    parametros),
                "cuentas" => (
                    "SELECT C.CUENTA_INTERNA AS cuenta, RTRIM(B.Descripcion) AS banco, " +
                    "CASE WHEN C.Tipo = 'A' THEN 'Ahorros' ELSE 'Corriente' END AS tipo, " +
                    "C.Cod_Divisa AS divisa, C.CUENTA_INTERBANCA AS interbanca, " +
                    "C.Destino AS destino, C.Activa AS activa, C.REGISTRO_FECHA AS fecha, " +
                    "C.REGISTRO_USUARIO AS usuario " +
                    "FROM SYS_CUENTAS_BANCARIAS C " +
                    "INNER JOIN TES_BANCOS_GRUPOS B ON C.Cod_Banco = B.Cod_Grupo " +
                    "WHERE C.Identificacion = @Identificacion ORDER BY C.Registro_Fecha DESC",
                    parametros),
                "tarjetas" => (
                    "EXEC spAFI_PersonaTarjetas_Consulta @ClienteCod, @Cedula, @Token",
                    new { parametros.ClienteCod, Cedula = identificacion, Token = string.Empty }),
                OpcionBoletasPago => (
                    "SELECT TOP 50 Nomina_Num AS nominaNum, NPago_Mes AS nPagoMes, " +
                    "COD_NOMINA AS codNomina, Fecha_Inicio AS fechaInicio, " +
                    "Fecha_Corte AS fechaCorte, SALARIO_ORDINARIO AS salarioOrdinario, " +
                    "Ingresos AS ingresos, Egresos AS egresos, Salario_Neto AS salarioNeto, " +
                    "Nomina_Desc AS nominaDesc, COD_NOMINA AS codNominaDetalle " +
                    "FROM vRH_Boleta_Pago_List WHERE Empleado_Id = @EmpleadoId " +
                    "ORDER BY Fecha_Corte DESC",
                    parametros),
                OpcionVacaciones or OpcionIncapacidades or OpcionPermisos =>
                    CrearConsultaBoleta(request.Opcion, parametros),
                "accionesPersonal" => (
                    "SELECT Cod_Accion AS codAccion, Fecha_Accion AS fechaAccion, " +
                    "TipoAccionDesc AS tipoAccion, Salario_Actual AS salarioActual, " +
                    "ANT_Salario AS salarioAnterior, PuestoDesc AS puesto, " +
                    "A_PuestoDesc AS puestoAnterior, CentroDesc AS centro, " +
                    "A_CentroDesc AS centroAnterior, DepartamentoDesc AS departamento, " +
                    "A_DepartamentoDesc AS departamentoAnterior, SeccionDesc AS seccion, " +
                    "A_SeccionDesc AS seccionAnterior, NominaDesc AS nomina, " +
                    "EstadoPersonaDesc AS estado, A_EstadoPersonaDesc AS estadoAnterior, " +
                    "Notas AS notas, Registro_Fecha AS registroFecha, " +
                    "Registro_Usuario AS registroUsuario " +
                    "FROM vRH_Accion_Personal WHERE Empleado_Id = @EmpleadoId " +
                    "ORDER BY Cod_Accion DESC",
                    parametros),
                "autorizaciones" => CrearConsultaAutorizaciones(request, parametros),
                "activos" => (
                    "SELECT NUM_PLACA AS numPlaca, Placa_Alterna AS placaAlterna, " +
                    "Nombre AS nombre, Fecha_Adquisicion AS fechaAdquisicion, " +
                    "Fecha_Instalacion AS fechaInstalacion, Tipo_Activo_Desc AS tipoActivo, " +
                    "Vida_Util AS vidaUtil, VIDA_UTIL_EN AS vidaUtilEn, " +
                    "Valor_Historico AS valorHistorico, Valor_Desecho AS valorDesecho, " +
                    "Estado_Desc AS estado, Identificacion AS identificacion, " +
                    "Responsable AS responsable, Departamento AS departamento, " +
                    "Seccion AS seccion, Localizacion AS localizacion, Proveedor AS proveedor, " +
                    "Modelo AS modelo, Marca AS marca, Num_Serie AS numSerie, " +
                    "Otras_Senas AS otrasSenas FROM vActivos_General " +
                    "WHERE Estado = 'A' AND Identificacion = @Identificacion " +
                    "ORDER BY Tipo_Activo, Num_Placa",
                    parametros),
                OpcionTraslados => CrearConsultaTraslados(request, parametros),
                _ => (string.Empty, parametros)
            };
        }

        private static (string Sql, object Parametros) CrearConsultaBoleta(
            string opcion,
            object parametros)
        {
            var configuracion = opcion switch
            {
                OpcionVacaciones => (
                    Vista: "vRH_Boleta_Vacaciones",
                    Fechas: "Fecha_Salida AS fechaSalida, Fecha_Entrada AS fechaEntrada",
                    Cantidad: "Dias_Disfrutados AS dias",
                    Orden: "Boleta_VAC"),
                OpcionIncapacidades => (
                    Vista: "vRH_Boleta_Incapacidades",
                    Fechas: "Fecha_Salida AS fechaSalida, Fecha_Entrada AS fechaEntrada",
                    Cantidad: "Dias AS dias",
                    Orden: "Boleta_ID"),
                _ => (
                    Vista: "vRH_Boleta_Permisos",
                    Fechas: "Hora_Inicio AS horaInicio, Hora_Corte AS horaCorte",
                    Cantidad: "Hrs_Total AS horas",
                    Orden: "Boleta_ID")
            };

            return (
                "SELECT Boleta_Id AS boletaId, Motivo AS motivo, " +
                configuracion.Fechas + ", " + configuracion.Cantidad +
                ", Estado_Transaccion AS estado, " +
                "Registro_Usuario AS usuario, Registro_Fecha AS fecha " +
                $"FROM {configuracion.Vista} WHERE Empleado_Id = @EmpleadoId " +
                $"ORDER BY {configuracion.Orden} DESC",
                parametros);
        }

        private static (string Sql, object Parametros) CrearConsultaAutorizaciones(
            ColaboradorMenuRequest request,
            object parametros)
        {
            var consulta = request.Tipo switch
            {
                "V" => (
                    "vRH_Boleta_Vacaciones",
                    "Dias_Disfrutados AS cantidad, Fecha_Salida AS fechaInicio, " +
                    "Fecha_Entrada AS fechaCorte"),
                "I" => (
                    "vRH_Boleta_Incapacidades",
                    "Dias AS cantidad, Fecha_Salida AS fechaInicio, " +
                    "Fecha_Entrada AS fechaCorte"),
                "P" => (
                    "vRH_Boleta_Permisos",
                    "Hrs_Total AS cantidad, Hora_Inicio AS fechaInicio, " +
                    "Hora_Corte AS fechaCorte"),
                _ => (string.Empty, string.Empty)
            };

            if (string.IsNullOrWhiteSpace(consulta.Item1))
            {
                return (string.Empty, parametros);
            }

            return (
                $"SELECT Boleta_Id AS boletaId, Empleado_ID AS empleadoId, " +
                "Identificacion AS identificacion, NOMBRE_COMPLETO AS nombre, " +
                "TipoDesc AS concepto, Motivo AS notas, Registro_Usuario AS usuario, " +
                "Registro_Fecha AS fecha, " + consulta.Item2 + ", Estado AS estado " +
                $"FROM {consulta.Item1} WHERE Estado = @Estado " +
                "AND Registro_Fecha >= @FechaInicio " +
                "AND Registro_Fecha < DATEADD(day, 1, @FechaCorte) " +
                "AND dbo.fxRH_Autorizador_Valida(Empleado_ID, @EmpleadoId) = 1 " +
                "ORDER BY Registro_Fecha DESC",
                parametros);
        }

        private static (string Sql, object Parametros) CrearConsultaTraslados(
            ColaboradorMenuRequest request,
            object parametros)
        {
            var filtro = request.Vista == "recepcion"
                ? "Identificacion_Destino = @Identificacion"
                : "Identificacion = @Identificacion";

            return (
                "SELECT V.Cod_Traslado AS codTraslado, Estado_Desc AS estado, " +
                "(SELECT T.ESTADO FROM ACTIVOS_TRASLADOS T " +
                "WHERE T.COD_TRASLADO = V.Cod_Traslado) AS estadoCodigo, " +
                "Registro_Fecha AS fecha, Registro_Usuario AS usuario, " +
                "Identificacion AS identificacionOrigen, Persona AS personaOrigen, " +
                "Departamento AS departamentoOrigen, Seccion AS seccionOrigen, " +
                "Identificacion_Destino AS identificacionDestino, " +
                "Persona_Destino AS personaDestino, " +
                "Departamento_Destino AS departamentoDestino, " +
                "Seccion_Destino AS seccionDestino, Motivo AS motivo, " +
                "PROCESADO_FECHA AS procesadoFecha, " +
                "PROCESADO_USUARIO AS procesadoUsuario " +
                $"FROM vActivos_Traslados_Boletas V WHERE {filtro} " +
                "ORDER BY Registro_Fecha DESC",
                parametros);
        }

        private static string NormalizarEstadoAutorizacion(string? estado)
        {
            return estado switch
            {
                "A" => "A",
                "D" => "D",
                _ => "S"
            };
        }

        private static ErrorDto<T>? ValidarUsuario<T>(
            string? usuario,
            T result,
            string mensaje,
            int? longitudMaxima = null)
        {
            if (string.IsNullOrWhiteSpace(usuario)
                || (longitudMaxima.HasValue && usuario.Length > longitudMaxima.Value))
            {
                return DbHelper.CreateErrorResponse(mensaje, -1, result);
            }

            return null;
        }

        private static ErrorDto<T>? ValidarSolicitudUsuario<T>(
            string? usuario,
            T result,
            Func<bool> solicitudInvalida,
            string mensajeSolicitud)
        {
            if (ValidarUsuario(
                usuario,
                result,
                "El usuario autenticado no es válido.",
                30) is { } errorUsuario)
            {
                return errorUsuario;
            }

            return solicitudInvalida()
                ? DbHelper.CreateErrorResponse(mensajeSolicitud, -1, result)
                : null;
        }

        private ColaboradorVinculoRow? ObtenerVinculacion(
            int codEmpresa,
            string usuario)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
            return ObtenerVinculacion(connection, usuario);
        }

        private static ColaboradorVinculoRow? ObtenerVinculacion(
            IDbConnection connection,
            string usuario)
        {
            return connection.QueryFirstOrDefault<ColaboradorVinculoRow>(
                SpRhPortalVinculado,
                new { Usuario = usuario.Trim() },
                commandType: CommandType.StoredProcedure);
        }

        private static ColaboradorClaveValidaRow? ConsultarClave(
            IDbConnection connection,
            string empleadoId,
            string clave)
        {
            return connection.QueryFirstOrDefault<ColaboradorClaveValidaRow>(
                "spRH_Portal_Clave_Valida",
                new { EmpleadoId = empleadoId, Clave = clave },
                commandType: CommandType.StoredProcedure);
        }

        private static ErrorDto<T> CrearError<T>(Exception ex, T result)
        {
            return DbHelper.CreateErrorResponse(ex.Message, -1, result);
        }

        private static ColaboradorPerfilData? CargarPerfil(
            System.Data.IDbConnection connection,
            string empleadoId,
            string usuario,
            string? appVersion)
        {
            var perfil = connection.QueryFirstOrDefault<ColaboradorPerfilRow>(
                "spRH_Portal_Empleado_Load",
                CrearParametrosAuditoria(empleadoId, usuario, appVersion),
                commandType: CommandType.StoredProcedure);

            if (perfil is null)
            {
                return null;
            }

            var foto = connection.QueryFirstOrDefault<byte[]>(
                "SELECT FOTO FROM RH_PERSONAS WHERE EMPLEADO_ID = @EmpleadoId",
                new { EmpleadoId = empleadoId });

            return new ColaboradorPerfilData
            {
                EmpleadoId = perfil.EMPLEADO_ID?.Trim() ?? empleadoId,
                Identificacion = perfil.IDENTIFICACION?.Trim() ?? string.Empty,
                NombreCompleto = perfil.NOMBRE_COMPLETO?.Trim() ?? string.Empty,
                EstadoPersona = perfil.ESTADO_PERSONA?.Trim(),
                CodNomina = perfil.COD_NOMINA?.Trim(),
                FechaIngreso = perfil.FECHA_INGRESO,
                CentroTrabajo = perfil.CENTRO_TRABAJO?.Trim(),
                Departamento = perfil.DEPARTAMENTO?.Trim(),
                Seccion = perfil.SECCION?.Trim(),
                FotoBase64 = foto is null ? null : Convert.ToBase64String(foto),
                FotoContentType = foto is null ? null : DetectarFotoContentType(foto)
            };
        }

        private static string DetectarFotoContentType(byte[] foto)
        {
            if (foto.AsSpan().StartsWith(FirmaJpeg))
            {
                return "image/jpeg";
            }

            if (foto.AsSpan().StartsWith(FirmaPng))
            {
                return "image/png";
            }

            if (foto.Length >= 6
                && foto.AsSpan().StartsWith("GIF8"u8)
                && foto[5] == 0x61)
            {
                return "image/gif";
            }

            if (foto.AsSpan().StartsWith(FirmaBmp))
            {
                return "image/bmp";
            }

            return "image/jpeg";
        }

        private static object CrearParametrosAuditoria(
            string empleadoId,
            string usuario,
            string? appVersion)
        {
            return new
            {
                EmpleadoId = empleadoId,
                Usuario = usuario.Trim(),
                AppName = AppNameWeb,
                AppVersion = NormalizarAppVersion(appVersion),
                Equipo = "WEB"
            };
        }

        private static string NormalizarAppVersion(string? appVersion)
        {
            var version = appVersion?.Trim() ?? string.Empty;
            return version.Length > 20 ? version[..20] : version;
        }
    }

    internal sealed class ColaboradorVinculoRow
    {
        public string? Empleado_ID { get; set; }
    }

    internal sealed class ColaboradorPerfilRow
    {
        public string? EMPLEADO_ID { get; set; }
        public string? NOMBRE_COMPLETO { get; set; }
        public string? IDENTIFICACION { get; set; }
        public string? ESTADO_PERSONA { get; set; }
        public string? COD_NOMINA { get; set; }
        public DateTime? FECHA_INGRESO { get; set; }
        public string? CENTRO_TRABAJO { get; set; }
        public string? DEPARTAMENTO { get; set; }
        public string? SECCION { get; set; }
    }

    internal sealed class ColaboradorConsultaIdRow
    {
        public string? EMPLEADO_ID { get; set; }
        public string? NOMBRE_COMPLETO { get; set; }
        public string? IDENTIFICACION { get; set; }
        public string? ESTADO_PERSONA { get; set; }
    }

    internal sealed class ColaboradorClaveValidaRow
    {
        public int Existe { get; set; }
    }

    internal sealed class ColaboradorClaveReestableceRow
    {
        public int CAMBIO { get; set; }
    }

    internal sealed class ColaboradorSolicitudTipoRow
    {
        public object? IdX { get; set; }
        public string? ItmX { get; set; }
    }

    internal sealed class ColaboradorSolicitudTipoDetalleRow
    {
        public int REQUIERE_AUTORIZACION { get; set; }
        public decimal? PERMISO_HRS_MAX { get; set; }
        public int PERMITE_LIQUIDACION { get; set; }
        public decimal? PORC_PATRONO { get; set; }
    }

    internal sealed class ColaboradorVacacionesInfoRow
    {
        public decimal? Dias_Disponibles { get; set; }
        public DateTime? Fecha_Inicio { get; set; }
    }

    internal sealed class ColaboradorSolicitudRegistroRow
    {
        public string? BoletaId { get; set; }
    }

    internal sealed class ColaboradorTrasladoEstadoRow
    {
        public string? IdentificacionOrigen { get; set; }
        public string? IdentificacionDestino { get; set; }
        public string? Estado { get; set; }
    }

    internal sealed class ColaboradorTrasladoResultadoRow
    {
        public short Pass { get; set; }
        public string? Mensaje { get; set; }
        public string? Boleta { get; set; }
    }

    internal sealed class ColaboradorTrasladoPlacaRow
    {
        public short asignado { get; set; }
        public string? NUM_PLACA { get; set; }
        public string? Descripcion { get; set; }
        public decimal DEPRECIACION_AC { get; set; }
        public decimal DEPRECIACION_MES { get; set; }
        public decimal VALOR_LIBROS { get; set; }
    }
}
