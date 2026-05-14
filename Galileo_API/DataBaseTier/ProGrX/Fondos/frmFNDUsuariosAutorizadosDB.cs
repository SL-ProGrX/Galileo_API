using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndUsuariosAutorizadosDb
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly int vModulo = 18; // Módulo de Colaboradores CC

        private const string SqlColaboradores = @"
                    SELECT
                        USUARIO,
                        activo,
                        registro_fecha,
                        registro_usuario,
                        actualiza_fecha,
                        actualiza_usuario
                    FROM dbo.FND_COLABORADORES_CC
                    ORDER BY USUARIO;";

        private const string SqlExisteColaborador = @"
                    SELECT ISNULL(COUNT(1), 0)
                    FROM dbo.FND_COLABORADORES_CC
                    WHERE USUARIO = @Usuario;";

        private const string SqlInsertColaborador = @"
                    INSERT INTO dbo.FND_COLABORADORES_CC
                    (
                        USUARIO,
                        activo,
                        registro_fecha,
                        registro_usuario
                    )
                    VALUES
                    (
                        @Usuario,
                        @Activo,
                        dbo.MyGetdate(),
                        @UsuarioLogueado
                    );";

        private const string SqlUpdateColaborador = @"
                    UPDATE dbo.FND_COLABORADORES_CC
                    SET activo = @Activo,
                        actualiza_fecha = dbo.MyGetdate(),
                        actualiza_usuario = @UsuarioLogueado
                    WHERE USUARIO = @Usuario;";

        private const string SqlDeleteColaborador = @"
                    DELETE FROM dbo.FND_COLABORADORES_CC
                    WHERE USUARIO = @Usuario;";

        public FrmFndUsuariosAutorizadosDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de colaboradores CC.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>ErrorDto con la lista de colaboradores.</returns>
        public ErrorDto<List<FndColaboradoresCcData>> FndColaboradoresCc_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<FndColaboradoresCcData>(
                new PortalDB(_config),
                CodEmpresa,
                SqlColaboradores);
        }

        /// <summary>
        /// Valida si un usuario existe en FND_COLABORADORES_CC.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="usuario">Usuario a validar.</param>
        /// <returns>ErrorDto con el resultado de la validación.</returns>
        public ErrorDto FndColaboradoresCc_Valida(int CodEmpresa, string usuario)
        {
            var usuarioNormalizado = NormalizarUsuario(usuario);
            var existe = ExisteColaborador(CodEmpresa, usuarioNormalizado);

            if (existe.Code != 0)
            {
                return DbHelper.ErrorResponse(existe.Description ?? "Error al validar usuario.", existe.Code.GetValueOrDefault(-1));
            }

            return existe.Result > 0
                ? DbHelper.ErrorResponse("El usuario ya existe.", -1)
                : DbHelper.OkResponse("El usuario es válido.");
        }

        /// <summary>
        /// Inserta o actualiza un colaborador CC y registra en bitácora.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="usuarioLogueado">Usuario que realiza la operación.</param>
        /// <param name="colaborador">Datos del colaborador.</param>
        /// <returns>ErrorDto con el resultado de la operación.</returns>
        public ErrorDto FndColaboradoresCc_Guardar(int CodEmpresa, string usuarioLogueado, FndColaboradoresCcData colaborador)
        {
            if (colaborador is null)
            {
                return DbHelper.ErrorResponse("Los datos del colaborador son requeridos.", -2);
            }

            var usuarioNormalizado = NormalizarUsuario(colaborador.Usuario);
            var existe = ExisteColaborador(CodEmpresa, usuarioNormalizado);

            if (existe.Code != 0)
            {
                return DbHelper.ErrorResponse(existe.Description ?? "Error al validar usuario.", existe.Code.GetValueOrDefault(-1));
            }

            if (colaborador.isNew && existe.Result > 0)
            {
                return DbHelper.ErrorResponse($"El usuario {usuarioNormalizado} ya existe.", -2);
            }

            if (!colaborador.isNew && existe.Result == 0)
            {
                return DbHelper.ErrorResponse($"El usuario {usuarioNormalizado} no existe.", -2);
            }

            var result = DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                colaborador.isNew ? SqlInsertColaborador : SqlUpdateColaborador,
                new
                {
                    Usuario = usuarioNormalizado,
                    colaborador.Activo,
                    UsuarioLogueado = NormalizarUsuario(usuarioLogueado)
                });

            if (result.Code == 0)
            {
                RegistrarBitacora(
                    CodEmpresa,
                    usuarioLogueado,
                    usuarioNormalizado,
                    colaborador.isNew ? "Registra - WEB" : "Modifica - WEB");
            }

            return result;
        }

        /// <summary>
        /// Elimina un colaborador CC por usuario y registra en bitácora.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="usuario">Usuario a eliminar.</param>
        /// <returns>ErrorDto con el resultado de la operación.</returns>
        public ErrorDto FndColaboradoresCc_Eliminar(int CodEmpresa, string usuario)
        {
            var usuarioNormalizado = NormalizarUsuario(usuario);
            var result = DbHelper.ExecuteNonQuery(
                new PortalDB(_config),
                CodEmpresa,
                SqlDeleteColaborador,
                new { Usuario = usuarioNormalizado });

            if (result.Code == 0)
            {
                RegistrarBitacora(CodEmpresa, usuario, usuarioNormalizado, "Elimina - WEB");
            }

            return result;
        }

        private ErrorDto<int> ExisteColaborador(int codEmpresa, string usuario)
        {
            return DbHelper.ExecuteSingleQuery(
                new PortalDB(_config),
                codEmpresa,
                SqlExisteColaborador,
                0,
                new { Usuario = usuario });
        }

        private void RegistrarBitacora(int codEmpresa, string usuarioLogueado, string usuarioMovimiento, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarUsuario(usuarioLogueado),
                DetalleMovimiento = $"Usuario Aut. Colaboradores: {usuarioMovimiento}",
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        private static string NormalizarUsuario(string? usuario) => (usuario ?? string.Empty).Trim().ToUpperInvariant();
    }
}