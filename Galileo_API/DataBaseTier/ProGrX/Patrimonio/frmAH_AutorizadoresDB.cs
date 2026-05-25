using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public class FrmAhAutorizadoresDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _dbBitacora;
        private const int vModulo = 2;

        public FrmAhAutorizadoresDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _dbBitacora = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene un autorizador de patrimonio por usuario.
        /// </summary>
        public ErrorDto<AutorizadorePatrimonioDto> Patrimonio_frmAH_Autorizadores_Obtener(
            int codEmpresa,
            string usuario)
        {
            var usuarioNormalizado = Patrimonio_frmAH_Autorizadores_NormalizarUsuario(usuario);
            var response = new AutorizadorePatrimonioDto();

            if (string.IsNullOrWhiteSpace(usuarioNormalizado))
            {
                return DbHelper.CreateErrorResponse(
                    "El usuario es requerido.",
                    -2,
                    response);
            }

            const string sql = @"
select
    rtrim(isnull(USUARIO, '')) as usuario,
    rtrim(isnull(NOTAS, '')) as notas,
    rtrim(isnull(ESTADO, '')) as estado,
    case rtrim(isnull(ESTADO, ''))
        when 'A' then 'Activo'
        else 'Inactivo'
    end as estado_desc
from PAT_AUTORIZADORES
where USUARIO = @usuario;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var result = conn.QueryFirstOrDefault<AutorizadorePatrimonioDto>(
                    sql,
                    new { usuario = usuarioNormalizado });

                return result == null
                    ? DbHelper.CreateErrorResponse(
                        "No se encontró registro, verifique.",
                        -2,
                        response)
                    : DbHelper.CreateOkResponse(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
        }

        /// <summary>
        /// Obtiene el usuario anterior o siguiente según el orden solicitado.
        /// </summary>
        public ErrorDto<string> Patrimonio_frmAH_Autorizadores_ConsultaAscDesc(
            int codEmpresa,
            string usuario,
            string tipo)
        {
            var usuarioNormalizado = Patrimonio_frmAH_Autorizadores_NormalizarUsuario(usuario);
            var tipoNormalizado = Patrimonio_frmAH_Autorizadores_NormalizarTipo(tipo);

            const string sqlAscPrimero = @"
select top 1 rtrim(isnull(USUARIO, ''))
from PAT_AUTORIZADORES
order by USUARIO asc;";

            const string sqlAsc = @"
select top 1 rtrim(isnull(USUARIO, ''))
from PAT_AUTORIZADORES
where USUARIO > @usuario
order by USUARIO asc;";

            const string sqlDescPrimero = @"
select top 1 rtrim(isnull(USUARIO, ''))
from PAT_AUTORIZADORES
order by USUARIO desc;";

            const string sqlDesc = @"
select top 1 rtrim(isnull(USUARIO, ''))
from PAT_AUTORIZADORES
where USUARIO < @usuario
order by USUARIO desc;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                if (tipoNormalizado == "DESC")
                {
                    sql = string.IsNullOrWhiteSpace(usuarioNormalizado)
                        ? sqlDescPrimero
                        : sqlDesc;
                }
                else
                {
                    sql = string.IsNullOrWhiteSpace(usuarioNormalizado)
                        ? sqlAscPrimero
                        : sqlAsc;
                }

                var result = conn.QueryFirstOrDefault<string>(sql, new { usuario = usuarioNormalizado }) ?? string.Empty;

                return DbHelper.CreateOkResponse(string.IsNullOrWhiteSpace(result) ? usuarioNormalizado : result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, string.Empty);
            }
        }

        /// <summary>
        /// Obtiene la lista de autorizadores de patrimonio para búsquedas.
        /// </summary>
        public ErrorDto<List<AutorizadorePatrimonioDto>> Patrimonio_frmAH_Autorizadores_Lista(
            int codEmpresa,
            string? filtro)
        {
            var filtroNormalizado = (filtro ?? string.Empty).Trim();

            const string sql = @"
select
    rtrim(isnull(USUARIO, '')) as usuario,
    rtrim(isnull(NOTAS, '')) as notas,
    rtrim(isnull(ESTADO, '')) as estado,
    case rtrim(isnull(ESTADO, ''))
        when 'A' then 'Activo'
        else 'Inactivo'
    end as estado_desc
from PAT_AUTORIZADORES
where (@filtro = ''
    or USUARIO like @filtro_like
    or ESTADO like @filtro_like)
order by USUARIO asc;";

            return DbHelper.ExecuteListQuery<AutorizadorePatrimonioDto>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    filtro = filtroNormalizado,
                    filtro_like = $"%{filtroNormalizado}%"
                });
        }

        /// <summary>
        /// Inserta un autorizador de patrimonio.
        /// </summary>
        public ErrorDto<FrmAhAutorizadoresGuardarResponse> Patrimonio_frmAH_Autorizadores_Insertar(
            int codEmpresa,
            FrmAhAutorizadoresGuardarRequest request)
        {
            return Patrimonio_frmAH_Autorizadores_Guardar(codEmpresa, request, true);
        }

        /// <summary>
        /// Actualiza un autorizador de patrimonio.
        /// </summary>
        public ErrorDto<FrmAhAutorizadoresGuardarResponse> Patrimonio_frmAH_Autorizadores_Actualizar(
            int codEmpresa,
            FrmAhAutorizadoresGuardarRequest request)
        {
            return Patrimonio_frmAH_Autorizadores_Guardar(codEmpresa, request, false);
        }

        /// <summary>
        /// Elimina un autorizador de patrimonio por usuario.
        /// </summary>
        public ErrorDto<bool> Patrimonio_frmAH_Autorizadores_Eliminar(
            int codEmpresa,
            string usuario,
            string registroUsuario)
        {
            var usuarioNormalizado = Patrimonio_frmAH_Autorizadores_NormalizarUsuario(usuario);
            var registroUsuarioNormalizado = Patrimonio_frmAH_Autorizadores_NormalizarUsuario(registroUsuario);

            if (string.IsNullOrWhiteSpace(usuarioNormalizado))
            {
                return DbHelper.CreateErrorResponse("El usuario a eliminar es requerido.", -2, false);
            }

            if (string.IsNullOrWhiteSpace(registroUsuarioNormalizado))
            {
                return DbHelper.CreateErrorResponse("El usuario del sistema es requerido.", -2, false);
            }

            const string sqlDelete = @"
delete from PAT_AUTORIZADORES
where USUARIO = @usuario;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                if (!Patrimonio_frmAH_Autorizadores_Existe(conn, usuarioNormalizado))
                {
                    return DbHelper.CreateErrorResponse("El autorizador indicado no existe.", -2, false);
                }

                conn.Execute(sqlDelete, new { usuario = usuarioNormalizado });

                Patrimonio_frmAH_Autorizadores_RegistrarBitacora(
                    codEmpresa,
                    registroUsuarioNormalizado,
                    usuarioNormalizado,
                    "Elimina - WEB");

                return DbHelper.CreateOkResponse(true);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, false);
            }
        }

        private ErrorDto<FrmAhAutorizadoresGuardarResponse> Patrimonio_frmAH_Autorizadores_Guardar(
            int codEmpresa,
            FrmAhAutorizadoresGuardarRequest? request,
            bool esNuevo)
        {
            var response = new FrmAhAutorizadoresGuardarResponse();
            var validacion = Patrimonio_frmAH_Autorizadores_ValidarGuardarRequest(request, response);

            if (validacion != null)
            {
                return validacion;
            }

            var usuarioNormalizado = Patrimonio_frmAH_Autorizadores_NormalizarUsuario(request!.usuario);
            var notasNormalizadas = (request.notas ?? string.Empty).Trim();
            var estadoNormalizado = request.estado.Trim().ToUpperInvariant();
            var registroUsuarioNormalizado = Patrimonio_frmAH_Autorizadores_NormalizarUsuario(request.registro_usuario);

            const string sqlGuardar = @"
exec spPAT_Autorizador_Add
    @usuario,
    @estado,
    @notas,
    @registro_usuario;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var existe = Patrimonio_frmAH_Autorizadores_Existe(conn, usuarioNormalizado);

                if (esNuevo && existe)
                {
                    return DbHelper.CreateErrorResponse(
                        "El autorizador indicado ya existe.",
                        -2,
                        response);
                }

                if (!esNuevo && !existe)
                {
                    return DbHelper.CreateErrorResponse(
                        "El autorizador indicado no existe.",
                        -2,
                        response);
                }

                conn.Execute(
                    sqlGuardar,
                    new
                    {
                        usuario = usuarioNormalizado,
                        estado = estadoNormalizado,
                        notas = notasNormalizadas,
                        registro_usuario = registroUsuarioNormalizado
                    });

                var movimiento = esNuevo ? "Registra - WEB" : "Modifica - WEB";
                var accion = esNuevo ? "Registra" : "Modifica";

                Patrimonio_frmAH_Autorizadores_RegistrarBitacora(
                    codEmpresa,
                    registroUsuarioNormalizado,
                    usuarioNormalizado,
                    movimiento);

                response.usuario = usuarioNormalizado;
                response.accion = accion;
                response.mensaje = "Información guardada satisfactoriamente...";

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
        }

        private static ErrorDto<FrmAhAutorizadoresGuardarResponse>? Patrimonio_frmAH_Autorizadores_ValidarGuardarRequest(
            FrmAhAutorizadoresGuardarRequest? request,
            FrmAhAutorizadoresGuardarResponse response)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse(
                    "La solicitud es requerida.",
                    -2,
                    response);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse(
                    "El usuario es requerido.",
                    -2,
                    response);
            }

            if (string.IsNullOrWhiteSpace(request.registro_usuario))
            {
                return DbHelper.CreateErrorResponse(
                    "El usuario del sistema es requerido.",
                    -2,
                    response);
            }

            var estado = (request.estado ?? string.Empty).Trim().ToUpperInvariant();
            if (estado != "A" && estado != "I")
            {
                return DbHelper.CreateErrorResponse(
                    "El estado indicado no es válido.",
                    -2,
                    response);
            }

            return null;
        }

        private static bool Patrimonio_frmAH_Autorizadores_Existe(
            SqlConnection conn,
            string usuario)
        {
            const string sql = @"
select cast(count(1) as int)
from PAT_AUTORIZADORES
where USUARIO = @usuario;";

            return conn.QueryFirstOrDefault<int>(sql, new { usuario }) > 0;
        }

        private void Patrimonio_frmAH_Autorizadores_RegistrarBitacora(
            int codEmpresa,
            string usuarioSistema,
            string usuarioAutorizador,
            string movimiento)
        {
            _dbBitacora.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuarioSistema,
                Movimiento = movimiento,
                Modulo = vModulo,
                DetalleMovimiento = $"PAT: Usuario Autorizador: {usuarioAutorizador}"
            });
        }

        private static string Patrimonio_frmAH_Autorizadores_NormalizarUsuario(string? usuario)
        {
            return (usuario ?? string.Empty).Trim();
        }

        private static string Patrimonio_frmAH_Autorizadores_NormalizarTipo(string? tipo)
        {
            return (tipo ?? string.Empty).Trim().Equals("desc", StringComparison.OrdinalIgnoreCase)
                ? "DESC"
                : "ASC";
        }
    }
}
