using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX
{
    public class FrmDsbColaboradorDB
    {
        private const string SpRhPortalVinculado = "spRH_Portal_Vinculado";

        private readonly PortalDB _portalDb;

        public FrmDsbColaboradorDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
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

            if (ValidarUsuario(
                usuario,
                result,
                "El usuario autenticado no es válido.",
                30) is { } errorUsuario)
            {
                return errorUsuario;
            }

            if (request is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Seleccione un empleado e ingrese una clave válida.",
                    -1,
                    result);
            }

            if (ValidarEmpleadoId(
                request.EmpleadoId,
                result,
                "Seleccione un empleado e ingrese una clave válida.") is { } errorEmpleadoId)
            {
                return errorEmpleadoId;
            }

            if (string.IsNullOrEmpty(request.Clave) || request.Clave.Length > 100)
            {
                return DbHelper.CreateErrorResponse(
                    "Seleccione un empleado e ingrese una clave válida.",
                    -1,
                    result);
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

                var validacion = connection.QueryFirstOrDefault<ColaboradorClaveValidaRow>(
                    "spRH_Portal_Clave_Valida",
                    new
                    {
                        EmpleadoId = request.EmpleadoId.Trim(),
                        Clave = request.Clave
                    },
                    commandType: CommandType.StoredProcedure);

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

            if (ValidarUsuario(
                usuario,
                result,
                "El usuario autenticado no es válido.",
                30) is { } errorUsuario)
            {
                return errorUsuario;
            }

            if (request is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Seleccione un empleado e ingrese el correo registrado.",
                    -1,
                    result);
            }

            if (ValidarEmpleadoId(
                request.EmpleadoId,
                result,
                "Seleccione un empleado e ingrese el correo registrado.") is { } errorEmpleadoId)
            {
                return errorEmpleadoId;
            }

            if (string.IsNullOrWhiteSpace(request.Email) || request.Email.Length > 100)
            {
                return DbHelper.CreateErrorResponse(
                    "Seleccione un empleado e ingrese el correo registrado.",
                    -1,
                    result);
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
                        AppName = "ProGrX_WEB",
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

        public ErrorDto<List<Dictionary<string, object?>>> Colaborador_Menu_Obtener(
            int CodEmpresa,
            string usuario,
            ColaboradorMenuRequest request)
        {
            var result = new List<Dictionary<string, object?>>();

            if (ValidarUsuario(
                usuario,
                result,
                "El usuario autenticado no es válido.",
                30) is { } errorUsuario)
            {
                return errorUsuario;
            }

            if (request is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Seleccione un colaborador válido.",
                    -1,
                    result);
            }

            if (ValidarEmpleadoId(
                request.EmpleadoId,
                result,
                "Seleccione un colaborador válido.") is { } errorEmpleadoId)
            {
                return errorEmpleadoId;
            }

            if (request.Clave?.Length > 100)
            {
                return DbHelper.CreateErrorResponse(
                    "Seleccione un colaborador válido.",
                    -1,
                    result);
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

                    var validacion = connection.QueryFirstOrDefault<ColaboradorClaveValidaRow>(
                        "spRH_Portal_Clave_Valida",
                        new { EmpleadoId = empleadoId, Clave = request.Clave },
                        commandType: CommandType.StoredProcedure);

                    if (validacion?.Existe != 1)
                    {
                        return DbHelper.CreateErrorResponse(
                            "La clave registrada no es válida.",
                            -2,
                            result);
                    }
                }

                var identificacion = connection.QueryFirstOrDefault<string>(
                    "SELECT IDENTIFICACION FROM RH_PERSONAS WHERE EMPLEADO_ID = @EmpleadoId",
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
                "boletasPago" => (
                    "SELECT TOP 50 Nomina_Num AS nominaNum, NPago_Mes AS nPagoMes, " +
                    "COD_NOMINA AS codNomina, Fecha_Inicio AS fechaInicio, " +
                    "Fecha_Corte AS fechaCorte, SALARIO_ORDINARIO AS salarioOrdinario, " +
                    "Ingresos AS ingresos, Egresos AS egresos, Salario_Neto AS salarioNeto, " +
                    "Nomina_Desc AS nominaDesc, COD_NOMINA AS codNominaDetalle " +
                    "FROM vRH_Boleta_Pago_List WHERE Empleado_Id = @EmpleadoId " +
                    "ORDER BY Fecha_Corte DESC",
                    parametros),
                "vacaciones" => (
                    "SELECT Boleta_Id AS boletaId, Motivo AS motivo, " +
                    "Fecha_Salida AS fechaSalida, Fecha_Entrada AS fechaEntrada, " +
                    "Dias_Disfrutados AS dias, Estado_Transaccion AS estado, " +
                    "Registro_Usuario AS usuario, Registro_Fecha AS fecha " +
                    "FROM vRH_Boleta_Vacaciones WHERE Empleado_Id = @EmpleadoId " +
                    "ORDER BY Boleta_VAC DESC",
                    parametros),
                "incapacidades" => (
                    "SELECT Boleta_Id AS boletaId, Motivo AS motivo, " +
                    "Fecha_Salida AS fechaSalida, Fecha_Entrada AS fechaEntrada, " +
                    "Dias AS dias, Estado_Transaccion AS estado, " +
                    "Registro_Usuario AS usuario, Registro_Fecha AS fecha " +
                    "FROM vRH_Boleta_Incapacidades WHERE Empleado_Id = @EmpleadoId " +
                    "ORDER BY Boleta_ID DESC",
                    parametros),
                "permisos" => (
                    "SELECT Boleta_Id AS boletaId, Motivo AS motivo, " +
                    "Hora_Inicio AS horaInicio, Hora_Corte AS horaCorte, " +
                    "Hrs_Total AS horas, Estado_Transaccion AS estado, " +
                    "Registro_Usuario AS usuario, Registro_Fecha AS fecha " +
                    "FROM vRH_Boleta_Permisos WHERE Empleado_Id = @EmpleadoId " +
                    "ORDER BY Boleta_ID DESC",
                    parametros),
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
                "traslados" => CrearConsultaTraslados(request, parametros),
                _ => (string.Empty, parametros)
            };
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
                "SELECT Cod_Traslado AS codTraslado, Estado_Desc AS estado, " +
                "Registro_Fecha AS fecha, Registro_Usuario AS usuario, " +
                "Identificacion AS identificacionOrigen, Persona AS personaOrigen, " +
                "Departamento AS departamentoOrigen, Seccion AS seccionOrigen, " +
                "Identificacion_Destino AS identificacionDestino, " +
                "Persona_Destino AS personaDestino, " +
                "Departamento_Destino AS departamentoDestino, " +
                "Seccion_Destino AS seccionDestino, Motivo AS motivo, " +
                "PROCESADO_FECHA AS procesadoFecha, " +
                "PROCESADO_USUARIO AS procesadoUsuario " +
                $"FROM vActivos_Traslados_Boletas WHERE {filtro} " +
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

        private ColaboradorVinculoRow? ObtenerVinculacion(
            int codEmpresa,
            string usuario)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
            return ObtenerVinculacion(connection, usuario);
        }

        private static ErrorDto<T>? ValidarEmpleadoId<T>(
            string? empleadoId,
            T result,
            string mensaje)
        {
            if (string.IsNullOrWhiteSpace(empleadoId) || empleadoId.Length > 20)
            {
                return DbHelper.CreateErrorResponse(mensaje, -1, result);
            }

            return null;
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
            if (foto.Length >= 3
                && foto[0] == 0xFF
                && foto[1] == 0xD8
                && foto[2] == 0xFF)
            {
                return "image/jpeg";
            }

            if (foto.Length >= 8
                && foto[0] == 0x89
                && foto[1] == 0x50
                && foto[2] == 0x4E
                && foto[3] == 0x47
                && foto[4] == 0x0D
                && foto[5] == 0x0A
                && foto[6] == 0x1A
                && foto[7] == 0x0A)
            {
                return "image/png";
            }

            if (foto.Length >= 6
                && foto[0] == 0x47
                && foto[1] == 0x49
                && foto[2] == 0x46
                && foto[3] == 0x38
                && foto[5] == 0x61)
            {
                return "image/gif";
            }

            if (foto.Length >= 2 && foto[0] == 0x42 && foto[1] == 0x4D)
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
                AppName = "ProGrX_WEB",
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
}
