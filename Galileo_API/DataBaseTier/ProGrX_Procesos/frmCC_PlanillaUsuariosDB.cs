using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo.Models.ProGrX_Procesos;

namespace Galileo.DataBaseTier.ProGrX_Procesos
{
    public class FrmCCPlanillaUsuariosDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 5;
        private readonly MSecurityMainDb _Security_MainDB;
        private const string MensajeModoInvalido = "Modo inválido.";
        private const string ModoInstituciones = "INSTITUCIONES";
        private const string ModoUsuarios = "USUARIOS";
        private const string MensajeCodInstitucionInvalido = "cod_institucion inválido.";

        public FrmCCPlanillaUsuariosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Lista izquierda (Instituciones / Usuarios).
        /// <param name="CodEmpresa"></param>
        /// <param name="modo"></param>
        /// </summary>
        public ErrorDto<List<CCPlanillaListaData>> CC_Planilla_Lista_Obtener(int CodEmpresa, string modo)
        {
            var modoValidado = ValidarModo(modo);
            if (modoValidado.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    modoValidado.Description ?? MensajeModoInvalido,
                    modoValidado.Code.GetValueOrDefault(-1),
                    new List<CCPlanillaListaData>());
            }

            var query = modoValidado.Result == ModoInstituciones
                ? @"
                    SELECT 
                          CAST(i.cod_institucion AS VARCHAR(50)) AS idx
                        , RTRIM(i.descripcion)                  AS itmx
                    FROM instituciones i
                    WHERE i.Activa = 1
                    ORDER BY i.descripcion;"
                : @"
                    SELECT
                          RTRIM(u.Nombre) AS idx
                        , RTRIM(u.Nombre) AS itmx
                    FROM vPrmUsuariosAutorizados u
                    ORDER BY u.Nombre;";

            return DbHelper.ExecuteListQuery<CCPlanillaListaData>(
                CreatePortalDb(),
                CodEmpresa,
                query);
        }

        /// <summary>
        /// Lista de usuarios o instituciones.
        /// <param name="CodEmpresa"></param>
        /// <param name="modo"></param>
        /// <param name="dato"></param>
        /// </summary>
        public ErrorDto<List<CCPlanillaDetalleData>> CC_Planilla_Detalle_Obtener(int CodEmpresa, string modo, string dato)
        {
            var modoValidado = ValidarModo(modo);
            if (modoValidado.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    modoValidado.Description ?? MensajeModoInvalido,
                    modoValidado.Code.GetValueOrDefault(-1),
                    new List<CCPlanillaDetalleData>());
            }

            if (string.IsNullOrWhiteSpace(dato))
            {
                return DbHelper.CreateErrorResponse("Debe indicar el dato seleccionado.", -2, new List<CCPlanillaDetalleData>());
            }

            if (modoValidado.Result == ModoInstituciones)
            {
                if (!int.TryParse(dato.Trim(), out var codInstitucion))
                {
                    return DbHelper.CreateErrorResponse(MensajeCodInstitucionInvalido, -2, new List<CCPlanillaDetalleData>());
                }

                const string queryInstituciones = @"
                    SELECT
                          RTRIM(u.Nombre) AS idx
                        , RTRIM(u.Nombre) AS itmx
                        , CASE WHEN a.cod_institucion IS NULL THEN 0 ELSE 1 END AS marcado
                    FROM vPrmUsuariosAutorizados u
                    LEFT JOIN PRM_USUARIOS a
                           ON a.usuario = u.Nombre
                          AND a.cod_institucion = @cod_institucion
                    ORDER BY u.Nombre;";

                return DbHelper.ExecuteListQuery<CCPlanillaDetalleData>(
                    CreatePortalDb(),
                    CodEmpresa,
                    queryInstituciones,
                    new { cod_institucion = codInstitucion });
            }

            const string queryUsuarios = @"
                    SELECT
                          CAST(i.cod_institucion AS VARCHAR(50)) AS idx
                        , RTRIM(i.descripcion)                   AS itmx
                        , CASE WHEN a.cod_institucion IS NULL THEN 0 ELSE 1 END AS marcado
                    FROM instituciones i
                    LEFT JOIN PRM_USUARIOS a
                           ON a.cod_institucion = i.cod_institucion
                          AND a.usuario = @usuario
                    WHERE i.Activa = 1
                    ORDER BY i.descripcion;";

            return DbHelper.ExecuteListQuery<CCPlanillaDetalleData>(
                CreatePortalDb(),
                CodEmpresa,
                queryUsuarios,
                new { usuario = dato.Trim() });
        }

        /// <summary>
        /// Aplica check individual (insert/delete) en PRM_USUARIOS.
        /// <param name="CodEmpresa"></param>
        /// <param name="usuarioSesion"></param>
        /// <param name="req"></param>
        /// </summary>
        public ErrorDto CC_Planilla_Aplica(int CodEmpresa, string usuarioSesion, CCPlanillaAplicaRequest req)
        {
            if (req is null)
            {
                return DbHelper.ErrorResponse("Request inválido.", -2);
            }

            var modoValidado = ValidarModo(req.modo);
            if (modoValidado.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    modoValidado.Description ?? MensajeModoInvalido,
                    modoValidado.Code.GetValueOrDefault(-1));
            }

            if (modoValidado.Result is not string modo)
            {
                return DbHelper.ErrorResponse(MensajeModoInvalido, -2);
            }

            if (string.IsNullOrWhiteSpace(req.dato) || string.IsNullOrWhiteSpace(req.item))
            {
                return DbHelper.ErrorResponse("Debe indicar dato e item.", -2);
            }

            var parseo = ObtenerRelacionPlanilla(modo, req.dato, req.item);

            if (parseo.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    parseo.Description ?? "Datos inválidos.",
                    parseo.Code.GetValueOrDefault(-1));
            }

            if (parseo.Result is not PlanillaRelacionData relacion)
            {
                return DbHelper.ErrorResponse("Datos inválidos.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                if (req.marcado)
                {
                    const string qInsert = @"
                IF NOT EXISTS (
                    SELECT 1
                    FROM PRM_USUARIOS
                    WHERE cod_institucion = @cod_institucion
                      AND usuario = @usuario
                )
                BEGIN
                    INSERT PRM_USUARIOS(cod_institucion, usuario, registro_fecha, registro_usuario)
                    VALUES(@cod_institucion, @usuario, dbo.MyGetdate(), @registro_usuario);
                END;";

                    connection.Execute(qInsert, new
                    {
                        cod_institucion = relacion.CodInstitucion,
                        usuario = relacion.Usuario,
                        registro_usuario = usuarioSesion ?? string.Empty
                    });

                    RegistrarBitacora(
                        CodEmpresa,
                        usuarioSesion,
                        "Registra - WEB",
                        $"PRM_USUARIOS (Asigna). Institución: {relacion.CodInstitucion}, Usuario: {relacion.Usuario}");
                }
                else
                {
                    const string qDelete = @"
                DELETE FROM PRM_USUARIOS
                WHERE cod_institucion = @cod_institucion
                  AND usuario = @usuario;";

                    connection.Execute(qDelete, new
                    {
                        cod_institucion = relacion.CodInstitucion,
                        usuario = relacion.Usuario
                    });

                    RegistrarBitacora(
                        CodEmpresa,
                        usuarioSesion,
                        "Elimina - WEB",
                        $"PRM_USUARIOS (Desasigna). Institución: {relacion.CodInstitucion}, Usuario: {relacion.Usuario}");
                }

                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(
                    result.Description ?? "Error al aplicar cambio de planilla usuarios.",
                    result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Aplica checkbox "Todos" (operación en bloque).
        /// <param name="CodEmpresa"></param>
        /// <param name="usuarioSesion"></param>
        /// <param name="req"></param>
        /// </summary>
        public ErrorDto CC_Planilla_Todos_Aplica(int CodEmpresa, string usuarioSesion, CCPlanillaTodosRequest req)
        {
            if (req is null)
            {
                return DbHelper.ErrorResponse("Request inválido.", -2);
            }

            var modoValidado = ValidarModo(req.modo);
            if (modoValidado.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    modoValidado.Description ?? MensajeModoInvalido,
                    modoValidado.Code.GetValueOrDefault(-1));
            }

            if (modoValidado.Result is not string modo)
            {
                return DbHelper.ErrorResponse(MensajeModoInvalido, -2);
            }

            if (string.IsNullOrWhiteSpace(req.dato))
            {
                return DbHelper.ErrorResponse("Debe indicar el dato seleccionado.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                if (!req.todos)
                {
                    AplicarTodosOff(connection, CodEmpresa, usuarioSesion, modo, req.dato);
                    return true;
                }

                AplicarTodosOn(connection, CodEmpresa, usuarioSesion, modo, req.dato);
                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(
                    result.Description ?? "Error al aplicar operación masiva de planilla usuarios.",
                    result.Code.GetValueOrDefault(-1));
        }

        private static ErrorDto<string> ValidarModo(string? modo)
        {
            if (string.IsNullOrWhiteSpace(modo))
            {
                return DbHelper.CreateErrorResponse("Debe indicar el modo.", -2, string.Empty);
            }

            var modoNormalizado = modo.Trim().ToUpperInvariant();
            return modoNormalizado != ModoInstituciones && modoNormalizado != ModoUsuarios
                ? DbHelper.CreateErrorResponse("Modo inválido. Valores permitidos: INSTITUCIONES | USUARIOS.", -2, string.Empty)
                : DbHelper.CreateOkResponse(modoNormalizado);
        }

        private ErrorDto<PlanillaRelacionData> ObtenerRelacionPlanilla(string modo, string dato, string item)
        {
            if (modo == ModoInstituciones)
            {
                if (!int.TryParse(dato.Trim(), out var codInstitucion))
                {
                    return DbHelper.CreateErrorResponse(MensajeCodInstitucionInvalido, -2, new PlanillaRelacionData());
                }

                return DbHelper.CreateOkResponse(new PlanillaRelacionData
                {
                    CodInstitucion = codInstitucion,
                    Usuario = item.Trim()
                });
            }

            if (!int.TryParse(item.Trim(), out var codInstitucionUsuario))
            {
                return DbHelper.CreateErrorResponse(MensajeCodInstitucionInvalido, -2, new PlanillaRelacionData());
            }

            return DbHelper.CreateOkResponse(new PlanillaRelacionData
            {
                CodInstitucion = codInstitucionUsuario,
                Usuario = dato.Trim()
            });
        }

        private void AplicarTodosOff(SqlConnection connection, int codEmpresa, string usuarioSesion, string modo, string dato)
        {
            if (modo == ModoInstituciones)
            {
                if (!int.TryParse(dato.Trim(), out var codInstitucion))
                {
                    throw new ArgumentException(MensajeCodInstitucionInvalido);
                }

                const string qInstitucion = @"DELETE PRM_USUARIOS WHERE cod_institucion = @cod_institucion;";
                connection.Execute(qInstitucion, new { cod_institucion = codInstitucion });

                RegistrarBitacora(
                    codEmpresa,
                    usuarioSesion,
                    "Elimina - WEB",
                    $"PRM_USUARIOS (Todos OFF). Institución: {codInstitucion}");
                return;
            }

            const string qUsuario = @"DELETE PRM_USUARIOS WHERE usuario = @usuario;";
            connection.Execute(qUsuario, new { usuario = dato.Trim() });

            RegistrarBitacora(
                codEmpresa,
                usuarioSesion,
                "Elimina - WEB",
                $"PRM_USUARIOS (Todos OFF). Usuario: {dato.Trim()}");
        }

        private void AplicarTodosOn(SqlConnection connection, int codEmpresa, string usuarioSesion, string modo, string dato)
        {
            if (modo == ModoInstituciones)
            {
                if (!int.TryParse(dato.Trim(), out var codInstitucion))
                {
                    throw new ArgumentException(MensajeCodInstitucionInvalido);
                }

                const string qInstitucion = @"
                    INSERT PRM_USUARIOS(cod_institucion, usuario, registro_fecha, registro_usuario)
                    SELECT
                          @cod_institucion
                        , u.Nombre
                        , dbo.MyGetdate()
                        , @registro_usuario
                    FROM vPrmUsuariosAutorizados u
                    WHERE NOT EXISTS (
                        SELECT 1
                        FROM PRM_USUARIOS a
                        WHERE a.cod_institucion = @cod_institucion
                          AND a.usuario = u.Nombre
                    );";

                connection.Execute(qInstitucion, new
                {
                    cod_institucion = codInstitucion,
                    registro_usuario = usuarioSesion ?? string.Empty
                });

                RegistrarBitacora(
                    codEmpresa,
                    usuarioSesion,
                    "Registra - WEB",
                    $"PRM_USUARIOS (Todos ON). Institución: {codInstitucion}");
                return;
            }

            const string qUsuario = @"
                    INSERT PRM_USUARIOS(cod_institucion, usuario, registro_fecha, registro_usuario)
                    SELECT
                          i.cod_institucion
                        , @usuario
                        , dbo.MyGetdate()
                        , @registro_usuario
                    FROM instituciones i
                    WHERE i.Activa = 1
                      AND NOT EXISTS (
                        SELECT 1
                        FROM PRM_USUARIOS a
                        WHERE a.cod_institucion = i.cod_institucion
                          AND a.usuario = @usuario
                    );";

            connection.Execute(qUsuario, new
            {
                usuario = dato.Trim(),
                registro_usuario = usuarioSesion ?? string.Empty
            });

            RegistrarBitacora(
                codEmpresa,
                usuarioSesion,
                "Registra - WEB",
                $"PRM_USUARIOS (Todos ON). Usuario: {dato.Trim()}");
        }

        private void RegistrarBitacora(int codEmpresa, string? usuarioSesion, string movimiento, string detalle)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuarioSesion ?? string.Empty,
                Modulo = vModulo,
                Movimiento = movimiento,
                DetalleMovimiento = detalle
            });
        }

        private PortalDB CreatePortalDb() => new(_config);

        private sealed class PlanillaRelacionData
        {
            public int CodInstitucion { get; init; }
            public string Usuario { get; init; } = string.Empty;
        }
    }
}
