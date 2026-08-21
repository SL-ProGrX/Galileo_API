using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using Galileo_API.Models.ProGrX_ControlTramites;

namespace Galileo_API.DataBaseTier.ProGrX.ControlTramites
{
    public class FrmSifTagsGruposDB
    {
        private const int ModuloControlTramites = 8;
        private const int CodigoValidacion = -2;

        private const string MensajeGrupoRequerido =
            "Debe indicar el código del grupo.";

        private const string MensajeUsuarioRequerido =
            "Debe indicar el usuario.";

        private const string MensajeTagRequerido =
            "Debe indicar el código del tag.";

        private const string MensajeUsuarioOtroGrupo =
            "El Usuario ya ha sido asignado a otro grupo, proceda a excluirlo primero del otro grupo antes de agregarlo a este.";

        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmSifTagsGruposDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        #region Grupos

        /// <summary>
        /// Obtiene la lista completa de grupos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<SifGruposData>> SIF_Grupos_Lista_Obtener(
            int CodEmpresa)
        {
            return EjecutarConsulta(
                CodEmpresa,
                ObtenerGrupos,
                new List<SifGruposData>());
        }

        /// <summary>
        /// Obtiene los grupos disponibles para los controles dropdown.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            SIF_Grupos_Dropdown_Obtener(int CodEmpresa)
        {
            return EjecutarConsulta(
                CodEmpresa,
                ObtenerGruposDropdown,
                new List<DropDownListaGenericaModel>());
        }

        /// <summary>
        /// Inserta o actualiza un grupo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public ErrorDto SIF_Grupos_Guardar(
            int CodEmpresa,
            string Usuario,
            SifGruposGuardarRequest param)
        {
            string? validacion = ValidarGrupo(param);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            return EjecutarAccion(
                CodEmpresa,
                connection => GuardarGrupo(
                    connection,
                    CodEmpresa,
                    Usuario,
                    param));
        }

        #endregion

        #region Miembros

        /// <summary>
        /// Obtiene los usuarios y su asignación al grupo indicado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codGrupo"></param>
        /// <returns></returns>
        public ErrorDto<List<SifGruposMiembroData>>
            SIF_Grupos_Miembros_Lista_Obtener(
                int CodEmpresa,
                string codGrupo)
        {
            string codigo = NormalizarCodigo(codGrupo);

            if (string.IsNullOrEmpty(codigo))
            {
                return DbHelper.CreateErrorResponse(
                    MensajeGrupoRequerido,
                    CodigoValidacion,
                    new List<SifGruposMiembroData>());
            }

            return EjecutarConsulta(
                CodEmpresa,
                connection => ObtenerMiembros(
                    connection,
                    codigo),
                new List<SifGruposMiembroData>());
        }

        /// <summary>
        /// Asigna o desasigna un usuario del grupo indicado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public ErrorDto SIF_Grupos_Miembro_Asignar(
            int CodEmpresa,
            SifGruposMiembroAsignarRequest param)
        {
            string? validacion = ValidarMiembro(param);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            return EjecutarAccion(
                CodEmpresa,
                connection => ActualizarAsignacionMiembro(
                    connection,
                    param));
        }

        #endregion

        #region Tags

        /// <summary>
        /// Obtiene los tags y su asignación al grupo indicado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codGrupo"></param>
        /// <returns></returns>
        public ErrorDto<List<SifGruposTagData>>
            SIF_Grupos_Tags_Lista_Obtener(
                int CodEmpresa,
                string codGrupo)
        {
            string codigo = NormalizarCodigo(codGrupo);

            if (string.IsNullOrEmpty(codigo))
            {
                return DbHelper.CreateErrorResponse(
                    MensajeGrupoRequerido,
                    CodigoValidacion,
                    new List<SifGruposTagData>());
            }

            return EjecutarConsulta(
                CodEmpresa,
                connection => ObtenerTags(
                    connection,
                    codigo),
                new List<SifGruposTagData>());
        }

        /// <summary>
        /// Asigna o desasigna un tag del grupo indicado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public ErrorDto SIF_Grupos_Tag_Asignar(
            int CodEmpresa,
            SifGruposTagAsignarRequest param)
        {
            string? validacion = ValidarTag(param);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            return EjecutarAccion(
                CodEmpresa,
                connection => ActualizarAsignacionTag(
                    connection,
                    param));
        }

        #endregion

        #region Consultas privadas

        private static List<SifGruposData> ObtenerGrupos(
            SqlConnection connection)
        {
            const string query = """
                SELECT
                    RTRIM(COD_GRUPO) AS cod_grupo,
                    RTRIM(DESCRIPCION) AS descripcion
                FROM dbo.SIF_GRUPOS
                ORDER BY COD_GRUPO;
                """;

            return connection
                .Query<SifGruposData>(query)
                .ToList();
        }

        private static List<DropDownListaGenericaModel>
            ObtenerGruposDropdown(SqlConnection connection)
        {
            const string query = """
                SELECT
                    RTRIM(COD_GRUPO) AS item,
                    RTRIM(DESCRIPCION) AS descripcion
                FROM dbo.SIF_GRUPOS
                ORDER BY DESCRIPCION;
                """;

            return connection
                .Query<DropDownListaGenericaModel>(query)
                .ToList();
        }

        private static List<SifGruposMiembroData> ObtenerMiembros(
            SqlConnection connection,
            string codigo)
        {
            const string query = """
                SELECT
                    RTRIM(U.NOMBRE) AS usuario,
                    RTRIM(U.DESCRIPCION) AS descripcion,
                    CAST(
                        CASE
                            WHEN G.USUARIO IS NULL THEN 0
                            ELSE 1
                        END
                        AS bit
                    ) AS asignado
                FROM dbo.USUARIOS U
                LEFT JOIN dbo.SIF_GRPUSERS G
                    ON G.USUARIO = U.NOMBRE
                   AND G.COD_GRUPO = @codigo
                WHERE U.ESTADO = 'A'
                ORDER BY U.NOMBRE;
                """;

            return connection
                .Query<SifGruposMiembroData>(
                    query,
                    new { codigo })
                .ToList();
        }

        private static List<SifGruposTagData> ObtenerTags(
            SqlConnection connection,
            string codigo)
        {
            const string query = """
                SELECT
                    RTRIM(T.TAG_CODIGO) AS tag_codigo,
                    RTRIM(T.DESCRIPCION) AS descripcion,
                    CAST(
                        CASE
                            WHEN TG.TAG_CODIGO IS NULL THEN 0
                            ELSE 1
                        END
                        AS bit
                    ) AS asignado
                FROM dbo.SIF_TAGS T
                LEFT JOIN dbo.SIF_TAGS_GRUPOS TG
                    ON TG.TAG_CODIGO = T.TAG_CODIGO
                   AND TG.COD_GRUPO = @codigo
                ORDER BY T.DESCRIPCION;
                """;

            return connection
                .Query<SifGruposTagData>(
                    query,
                    new { codigo })
                .ToList();
        }

        #endregion

        #region Acciones privadas

        private ErrorDto GuardarGrupo(
            SqlConnection connection,
            int codEmpresa,
            string usuario,
            SifGruposGuardarRequest param)
        {
            string codigo = NormalizarCodigo(param.cod_grupo);
            string descripcion = NormalizarTexto(param.descripcion);

            bool existe = GrupoExiste(
                connection,
                codigo);

            int afectados = existe
                ? ActualizarGrupo(
                    connection,
                    codigo,
                    descripcion)
                : InsertarGrupo(
                    connection,
                    codigo,
                    descripcion);

            if (afectados == 0)
            {
                return CrearErrorGuardarGrupo(existe);
            }

            RegistrarBitacora(
                codEmpresa,
                usuario,
                codigo,
                existe);

            string mensaje = existe
                ? "Información actualizada satisfactoriamente."
                : "Información guardada satisfactoriamente.";

            return DbHelper.OkResponse(mensaje);
        }

        private static bool GrupoExiste(
            SqlConnection connection,
            string codigo)
        {
            const string query = """
                SELECT COUNT(1)
                FROM dbo.SIF_GRUPOS
                WHERE COD_GRUPO = @codigo;
                """;

            return connection.QuerySingle<int>(
                query,
                new { codigo }) > 0;
        }

        private static int InsertarGrupo(
            SqlConnection connection,
            string codigo,
            string descripcion)
        {
            const string query = """
                INSERT INTO dbo.SIF_GRUPOS
                (
                    COD_GRUPO,
                    DESCRIPCION
                )
                VALUES
                (
                    @codigo,
                    @descripcion
                );
                """;

            return connection.Execute(
                query,
                new
                {
                    codigo,
                    descripcion
                });
        }

        private static int ActualizarGrupo(
            SqlConnection connection,
            string codigo,
            string descripcion)
        {
            const string query = """
                UPDATE dbo.SIF_GRUPOS
                SET DESCRIPCION = @descripcion
                WHERE COD_GRUPO = @codigo;
                """;

            return connection.Execute(
                query,
                new
                {
                    codigo,
                    descripcion
                });
        }

        private static ErrorDto ActualizarAsignacionMiembro(
            SqlConnection connection,
            SifGruposMiembroAsignarRequest param)
        {
            string codigo = NormalizarCodigo(param.cod_grupo);
            string usuario = NormalizarUsuario(param.usuario);
            bool asignado = param.asignado.GetValueOrDefault();

            if (!asignado)
            {
                return DesasignarMiembro(
                    connection,
                    codigo,
                    usuario);
            }

            if (UsuarioAsignadoOtroGrupo(
                connection,
                codigo,
                usuario))
            {
                return DbHelper.ErrorResponse(
                    MensajeUsuarioOtroGrupo,
                    CodigoValidacion);
            }

            return AsignarMiembro(
                connection,
                codigo,
                usuario);
        }

        private static bool UsuarioAsignadoOtroGrupo(
            SqlConnection connection,
            string codigo,
            string usuario)
        {
            const string query = """
                SELECT COUNT(1)
                FROM dbo.SIF_GRPUSERS
                WHERE COD_GRUPO <> @codigo
                  AND USUARIO = @usuario;
                """;

            return connection.QuerySingle<int>(
                query,
                new
                {
                    codigo,
                    usuario
                }) > 0;
        }

        private static ErrorDto AsignarMiembro(
            SqlConnection connection,
            string codigo,
            string usuario)
        {
            const string query = """
                INSERT INTO dbo.SIF_GRPUSERS
                (
                    COD_GRUPO,
                    USUARIO
                )
                VALUES
                (
                    @codigo,
                    @usuario
                );
                """;

            connection.Execute(
                query,
                new
                {
                    codigo,
                    usuario
                });

            return DbHelper.OkResponse(
                "El usuario ha sido asignado satisfactoriamente.");
        }

        private static ErrorDto DesasignarMiembro(
            SqlConnection connection,
            string codigo,
            string usuario)
        {
            const string query = """
                DELETE FROM dbo.SIF_GRPUSERS
                WHERE COD_GRUPO = @codigo
                  AND USUARIO = @usuario;
                """;

            connection.Execute(
                query,
                new
                {
                    codigo,
                    usuario
                });

            return DbHelper.OkResponse(
                "El usuario ha sido desasignado satisfactoriamente.");
        }

        private static ErrorDto ActualizarAsignacionTag(
            SqlConnection connection,
            SifGruposTagAsignarRequest param)
        {
            string codigo = NormalizarCodigo(param.cod_grupo);
            string tagCodigo = NormalizarCodigo(param.tag_codigo);
            bool asignado = param.asignado.GetValueOrDefault();

            return asignado
                ? AsignarTag(
                    connection,
                    codigo,
                    tagCodigo)
                : DesasignarTag(
                    connection,
                    codigo,
                    tagCodigo);
        }

        private static ErrorDto AsignarTag(
            SqlConnection connection,
            string codigo,
            string tagCodigo)
        {
            const string query = """
                INSERT INTO dbo.SIF_TAGS_GRUPOS
                (
                    TAG_CODIGO,
                    COD_GRUPO
                )
                VALUES
                (
                    @tagCodigo,
                    @codigo
                );
                """;

            connection.Execute(
                query,
                new
                {
                    tagCodigo,
                    codigo
                });

            return DbHelper.OkResponse(
                "El tag ha sido asignado satisfactoriamente.");
        }

        private static ErrorDto DesasignarTag(
            SqlConnection connection,
            string codigo,
            string tagCodigo)
        {
            const string query = """
                DELETE FROM dbo.SIF_TAGS_GRUPOS
                WHERE TAG_CODIGO = @tagCodigo
                  AND COD_GRUPO = @codigo;
                """;

            connection.Execute(
                query,
                new
                {
                    tagCodigo,
                    codigo
                });

            return DbHelper.OkResponse(
                "El tag ha sido desasignado satisfactoriamente.");
        }

        private void RegistrarBitacora(
            int codEmpresa,
            string usuario,
            string codigo,
            bool existe)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = (usuario ?? string.Empty).ToUpperInvariant(),
                DetalleMovimiento = $"Grupo de Usuarios: {codigo}",
                Movimiento = existe
                    ? "MODIFICA-WEB"
                    : "REGISTRA-WEB",
                Modulo = ModuloControlTramites
            });
        }

        #endregion

        #region Validaciones

        private static string? ValidarGrupo(
            SifGruposGuardarRequest? param)
        {
            if (param == null)
            {
                return "Debe indicar la información del grupo.";
            }

            string codigo = NormalizarCodigo(param.cod_grupo);

            return string.IsNullOrEmpty(codigo)
                ? MensajeGrupoRequerido
                : null;
        }

        private static string? ValidarMiembro(
            SifGruposMiembroAsignarRequest? param)
        {
            if (param == null)
            {
                return "Debe indicar la información del miembro.";
            }

            if (string.IsNullOrEmpty(
                NormalizarCodigo(param.cod_grupo)))
            {
                return MensajeGrupoRequerido;
            }

            if (string.IsNullOrEmpty(
                NormalizarUsuario(param.usuario)))
            {
                return MensajeUsuarioRequerido;
            }

            if (!param.asignado.HasValue)
            {
                return "Debe indicar si el usuario será asignado o desasignado.";
            }

            return null;
        }

        private static string? ValidarTag(
            SifGruposTagAsignarRequest? param)
        {
            if (param == null)
            {
                return "Debe indicar la información del tag.";
            }

            if (string.IsNullOrEmpty(
                NormalizarCodigo(param.cod_grupo)))
            {
                return MensajeGrupoRequerido;
            }

            if (string.IsNullOrEmpty(
                NormalizarCodigo(param.tag_codigo)))
            {
                return MensajeTagRequerido;
            }

            if (!param.asignado.HasValue)
            {
                return "Debe indicar si el tag será asignado o desasignado.";
            }

            return null;
        }

        private static ErrorDto CrearErrorGuardarGrupo(
            bool existe)
        {
            string mensaje = existe
                ? "El grupo indicado no pudo ser actualizado."
                : "El grupo indicado no pudo ser registrado.";

            return DbHelper.ErrorResponse(
                mensaje,
                CodigoValidacion);
        }

        #endregion

        #region Ejecución y normalización

        private ErrorDto<T> EjecutarConsulta<T>(
            int codEmpresa,
            Func<SqlConnection, T> action,
            T errorResult)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(
                    _portalDB,
                    codEmpresa);

                return DbHelper.CreateOkResponse(
                    action(connection));
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    errorResult);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    errorResult);
            }
        }

        private ErrorDto EjecutarAccion(
            int codEmpresa,
            Func<SqlConnection, ErrorDto> action)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(
                    _portalDB,
                    codEmpresa);

                return action(connection);
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static string NormalizarCodigo(
            string? codigo)
        {
            return (codigo ?? string.Empty)
                .Trim()
                .ToUpperInvariant();
        }

        private static string NormalizarUsuario(
            string? usuario)
        {
            return (usuario ?? string.Empty)
                .Trim()
                .ToUpperInvariant();
        }

        private static string NormalizarTexto(
            string? texto)
        {
            return (texto ?? string.Empty)
                .Trim()
                .ToUpperInvariant();
        }

        #endregion
    }
}