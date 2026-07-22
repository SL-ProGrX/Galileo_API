using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;
using Galileo.Models.ERROR;
using Galileo.Models.SIF;

namespace Galileo.DataBaseTier
{
    public class FrmSysCoreUsuariosDB
    {
        private readonly IConfiguration _config;

        public FrmSysCoreUsuariosDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene la lista de usuarios con paginación y filtros aplicados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<CoreUsuariosLista> CoreUsuariosLista_Obtener(int CodEmpresa, string filtros)
        {
            var vfiltro = JsonConvert.DeserializeObject<CoreUsuarioFiltros>(filtros);
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<CoreUsuariosLista>();
            response.Result = new CoreUsuariosLista();
            response.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);

                var search = vfiltro?.filtro?.Trim();
                string? searchLike = string.IsNullOrWhiteSpace(search) ? null : $"%{search}%";

                var offset = vfiltro?.pagina ?? 0;
                var fetch = vfiltro?.paginacion ?? 0;
                if (fetch <= 0)
                {
                    fetch = int.MaxValue;
                }

                // Total (conteo con filtro)
                const string totalQuery = @"SELECT COUNT(*)
                                           FROM CORE_USUARIOS
                                           WHERE (@search IS NULL
                                                  OR CORE_USUARIO LIKE @search
                                                  OR Nombre LIKE @search)";

                response.Result.total = connection.Query<int>(totalQuery, new { search = searchLike }).FirstOrDefault();

                // Datos paginados
                const string query = @"SELECT CORE_USUARIO, Nombre
                                       FROM CORE_USUARIOS
                                       WHERE (@search IS NULL
                                              OR CORE_USUARIO LIKE @search
                                              OR Nombre LIKE @search)
                                       ORDER BY CORE_USUARIO
                                       OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

                response.Result.lista = connection.Query<CoreUsuariosData>(query, new
                {
                    search = searchLike,
                    offset,
                    fetch
                }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.total = 0;
                response.Result.lista = new List<CoreUsuariosData>();
            }

            return response;
        }


        /// <summary>
        /// Verifica si un usuario ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CoreUsuariosExiste_Obtener(int CodEmpresa, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto();
            resp.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);

                var query = "SELECT COUNT(*) FROM CORE_USUARIOS WHERE CORE_USUARIO = @usuario";
                int result = connection.QueryFirstOrDefault<int>(query, new { usuario });

                (resp.Code, resp.Description) =
                    (result == 0) ? (0, "USUARIO: Libre!") : (-2, "USUARIO: Ocupado!");
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Guarda (inserta o actualiza) un usuario en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuariosData"></param>
        /// <returns></returns>
        public ErrorDto CoreUsuarios_Guardar(int CodEmpresa, CoreUsuariosData usuariosData)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto { Code = 0, Description = string.Empty };

            try
            {
                // 1) Validaciones
                var errores = new List<string>();

                if (usuariosData == null)
                {
                    return new ErrorDto
                    {
                        Code = -1,
                        Description = "Datos de usuario no proporcionados."
                    };
                }

                if (string.IsNullOrWhiteSpace(usuariosData.core_usuario))
                    errores.Add("No ha indicado el nombre de CORE_USUARIO.");

                if (string.IsNullOrWhiteSpace(usuariosData.nombre))
                    errores.Add("No ha indicado el nombre del usuario.");

                if (string.IsNullOrWhiteSpace(usuariosData.email))
                    errores.Add("Debe indicar un email válido.");
                else
                {
                    try
                    {
                        // Validación simple de email
                        var _ = new System.Net.Mail.MailAddress(usuariosData.email);
                    }
                    catch
                    {
                        errores.Add("El email indicado no tiene un formato válido.");
                    }
                }

                if (string.IsNullOrWhiteSpace(usuariosData.tel_movil))
                    errores.Add("No ha indicado un número de teléfono móvil.");

                if (errores.Count > 0)
                {
                    resp.Code = -1;
                    resp.Description = string.Join(" | ", errores);
                    return resp;
                }

                // 2) DB
                using var connection = new SqlConnection(clienteConnString);

                // Una sola consulta para existencia (parametrizada)
                const string queryExiste = @"
                                        SELECT COUNT(1)
                                        FROM CORE_USUARIOS
                                        WHERE CORE_USUARIO = @Usuario;";

                int existe = connection.QueryFirstOrDefault<int>(queryExiste, new { Usuario = usuariosData.core_usuario });

                // 3) Upsert: si existe → actualizar; si no → insertar
                resp = (existe == 0)
                    ? CoreUsuarios_Insertar(CodEmpresa, usuariosData)
                    : CoreUsuarios_Actualizar(CodEmpresa, usuariosData);
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Obtiene los detalles de un usuario específico.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CoreUsuariosData> CoreUsuarios_Obtener(int CodEmpresa, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto<CoreUsuariosData> resp = new ErrorDto<CoreUsuariosData>();
            resp.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                const string query = @"SELECT * FROM CORE_USUARIOS WHERE CORE_USUARIO = @usuario";
                resp.Result = connection.QueryFirstOrDefault<CoreUsuariosData>(query, new { usuario });
                if (resp.Result == null)
                {
                    resp.Code = -2;
                    resp.Description = "Usuario no encontrado.";
                }
            }
            catch (Exception)
            {
                resp.Code = -1;
                resp.Description = "Error al obtener el usuario.";
                resp.Result = null;
            }

            return resp;

        }

        /// <summary>
        /// Importa/Sincroniza los usuarios del sistema.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto CoreUsuarios_Importar(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                const string sp = "spCORE_Usuarios_Importar";
                connection.Execute(sp, commandType: CommandType.StoredProcedure);
                resp.Description = "Usuarios del Sistema Sincronizados/Importados Satisfactoriamente!";
            }
            catch (Exception ex)
            {
                resp.Code = 1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Navegación (scroll) entre usuarios.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="scroll"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CoreUsuariosData> CoreUsuario_Scroll(int CodEmpresa, int scroll, string? usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto<CoreUsuariosData> resp = new ErrorDto<CoreUsuariosData>();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);

                if (scroll == 1)
                {
                    const string query = @"SELECT TOP 1 *
                                           FROM CORE_USUARIOS
                                           WHERE CORE_USUARIO > @usuario
                                           ORDER BY CORE_USUARIO ASC";
                    resp.Result = connection.QueryFirstOrDefault<CoreUsuariosData>(query, new { usuario });
                }
                else
                {
                    const string query = @"SELECT TOP 1 *
                                           FROM CORE_USUARIOS
                                           WHERE CORE_USUARIO < @usuario
                                           ORDER BY CORE_USUARIO DESC";
                    resp.Result = connection.QueryFirstOrDefault<CoreUsuariosData>(query, new { usuario });
                }

                if (resp.Result == null)
                {
                    resp.Code = -2;
                    resp.Description = "No se encontraron más resultados.";
                }
            }
            catch (Exception)
            {
                resp.Code = -1;
                resp.Description = "Error al obtener el usuario.";
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Inserta un nuevo usuario en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuariosData"></param>
        /// <returns></returns>
        private ErrorDto CoreUsuarios_Insertar(int CodEmpresa, CoreUsuariosData usuariosData)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);

                const string query = @"INSERT INTO CORE_USUARIOS
                                       (CORE_USUARIO, Usuario_Ref, Nombre, Registro_Fecha, Registro_Usuario, Activo, Notas, Email, Tel_Movil)
                                       VALUES
                                       (@core_usuario, @usuario_ref, @nombre, GETDATE(), @registro_usuario, 1, @notas, @email, @tel_movil);";

                connection.Execute(query, new
                {
                    core_usuario = usuariosData.core_usuario,
                    usuario_ref = usuariosData.usuario_ref,
                    nombre = usuariosData.nombre,
                    registro_usuario = usuariosData.registro_usuario,
                    notas = usuariosData.notas,
                    email = usuariosData.email,
                    tel_movil = usuariosData.tel_movil
                });

                resp.Description = "Usuario Ingresado Satisfactoriamente!";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Actualiza los detalles de un usuario existente en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuariosData"></param>
        /// <returns></returns>
        private ErrorDto CoreUsuarios_Actualizar(int CodEmpresa, CoreUsuariosData usuariosData)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                int activo = usuariosData.activo ? 1 : 0;
                using var connection = new SqlConnection(clienteConnString);

                const string query = @"UPDATE CORE_USUARIOS
                                       SET Nombre = @nombre,
                                           Activo = @activo,
                                           Usuario_Ref = @usuario_ref,
                                           Notas = @notas,
                                           Email = @email,
                                           Tel_Movil = @tel_movil,
                                           Modifica_Fecha = GETDATE(),
                                           Modifica_Usuario = @modificacion_usuario
                                       WHERE CORE_USUARIO = @core_usuario;";

                connection.Execute(query, new
                {
                    nombre = usuariosData.nombre,
                    activo,
                    usuario_ref = usuariosData.usuario_ref,
                    notas = usuariosData.notas,
                    email = usuariosData.email,
                    tel_movil = usuariosData.tel_movil,
                    modificacion_usuario = usuariosData.modificacion_usuario,
                    core_usuario = usuariosData.core_usuario
                });

                resp.Description = "Usuario Actualizado Satisfactoriamente!";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }


        /// <summary>
        /// Elimina un usuario del sistema (no se permite de momento).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CoreUsuarios_Eliminar(int CodEmpresa, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                //Incluir bitacora
                resp.Description = "No se puede eliminar un Usuario del Sistema!";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// / Obtiene las UENs (Unidades Estratégicas de Negocio) asignadas a un usuario.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<CoreMiembrosData>> CoreUsuariosMiembros_Obtener(int CodEmpresa, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto<List<CoreMiembrosData>> resp = new ErrorDto<List<CoreMiembrosData>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                const string sp = "spSys_CORE_Users_UENs_Miembros_Consultas";
                resp.Result = connection.Query<CoreMiembrosData>(sp, new { usuario }, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null;
            }

            return resp;
        }

        /// <summary>
        /// Obtiene las UENs (Unidades Estratégicas de Negocio) y los roles asignados a un usuario.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<CoreMiembrosRolData>> CoreUsuariosUENs_Roles_Obtener(int CodEmpresa, string usuario)
        {
            const string query = @"SELECT U.COD_UNIDAD,
                                          U.DESCRIPCION,
                                          U.ACTIVA,
                                          U.REGISTRO_FECHA,
                                          U.REGISTRO_USUARIO,
                                          UR.ROL_SOLICITA,
                                          UR.ROL_CONSULTA,
                                          UR.ROL_AUTORIZA,
                                          UR.ROL_ENCARGADO,
                                          UR.ROL_LIDER
                                   FROM CORE_UENS U
                                   INNER JOIN CORE_UENS_USUARIOS_ROLES UR
                                       ON U.COD_UNIDAD = UR.COD_UNIDAD
                                      AND UR.CORE_USUARIO = @usuario
                                   WHERE U.ACTIVA = 1";

            return DbHelper.ExecuteListQuery<CoreMiembrosRolData>(
                new PortalDB(_config),
                CodEmpresa,
                query,
                new { usuario });
        }

        /// <summary>
        /// Actualiza (inserta o elimina) un miembro (UEN) para un usuario.
        /// </summary>
        /// <param name="miembro"></param>
        /// <returns></returns>
        public ErrorDto CoreUsuariosMiembro_Actualiza(string miembro)
        {
            CoreMiembro? core = JsonConvert.DeserializeObject<CoreMiembro>(miembro);
            if (core == null)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Datos de miembro no válidos o no proporcionados."
                };
            }
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(core.codEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;

            try
            {
                string movItem = core.mov ? "A" : "E";
                using var connection = new SqlConnection(clienteConnString);
                const string sp = "spSys_UENS_Miembros_Registro";
                connection.Execute(sp, new
                {
                    uen = core.uen,
                    CoreUser = core.core_usuario,
                    RegUser = core.usuario,
                    Mov = movItem
                }, commandType: CommandType.StoredProcedure);
                resp.Description = "UENs actualizados satisfactoriamente!";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Actualiza (inserta o elimina) los roles asignados a un usuario para una UEN específica.
        /// </summary>
        /// <param name="miembroRol"></param>
        /// <returns></returns>
        public ErrorDto CoreUsuariosMiembroRol_Actualiza(string miembroRol)
        {
            CoreMiembroRol? core = JsonConvert.DeserializeObject<CoreMiembroRol>(miembroRol);
            if (core == null)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "Datos de miembro de rol no válidos o no proporcionados."
                };
            }
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(core.codEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;

            try
            {
                int rol_solicita = core.rol_solicita ? 1 : 0;
                int rol_consulta = core.rol_consulta ? 1 : 0;
                int rol_autoriza = core.rol_autoriza ? 1 : 0;
                int rol_encargado = core.rol_encargado ? 1 : 0;
                int rol_lider = core.rol_lider ? 1 : 0;
                using var connection = new SqlConnection(clienteConnString);
                const string sp = "spSys_UENS_Roles_Registro";
                connection.Execute(sp, new
                {
                    UEN = core.uen,
                    CoreUser = core.core_usuario,
                    R_Solicita = rol_solicita,
                    R_Consulta = rol_consulta,
                    R_Autoriza = rol_autoriza,
                    R_Encargado = rol_encargado,
                    R_Lider = rol_lider,
                    Usuario = core.usuario
                }, commandType: CommandType.StoredProcedure);
                resp.Description = "Rol Actualizado Satisfactoriamente!";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

    }
}
