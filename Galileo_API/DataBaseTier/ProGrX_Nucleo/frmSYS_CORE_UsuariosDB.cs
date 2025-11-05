using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.SIF;

namespace PgxAPI.DataBaseTier
{
    public class frmSYS_CORE_UsuariosDB
    {
        private readonly IConfiguration _config;

        public frmSYS_CORE_UsuariosDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene la lista de usuarios con paginación y filtros aplicados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<coreUsuariosLista> CoreUsuariosLista_Obtener(int CodEmpresa, string filtros)
        {
            var vfiltro = JsonConvert.DeserializeObject<coreUsuarioFiltros>(filtros);
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<coreUsuariosLista>();
            response.Result = new coreUsuariosLista();
            response.Code = 0;

            try
            {
                var query = "";
                string where = "", paginaActual = "", paginacionActual = "";

                using var connection = new SqlConnection(clienteConnString);
                {
                    // Construcción del filtro WHERE
                    if (vfiltro != null)
                    {
                        if (!string.IsNullOrEmpty(vfiltro.filtro))
                        {
                            where = "WHERE CORE_USUARIO LIKE '%" + vfiltro.filtro + "%' OR Nombre LIKE '%" + vfiltro.filtro + "%' ";
                        }

                        if (vfiltro.pagina != null)
                        {
                            paginaActual = " OFFSET " + vfiltro.pagina + " ROWS ";
                            paginacionActual = " FETCH NEXT " + vfiltro.paginacion + " ROWS ONLY ";
                        }
                    }

                    // Consulta para el total de registros
                    query = $"SELECT COUNT(*) FROM CORE_USUARIOS {where}";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();

                    // Consulta para obtener los datos paginados
                    query = $@"
                            SELECT CORE_USUARIO, Nombre 
                            FROM CORE_USUARIOS 
                            {where}
                            ORDER BY CORE_USUARIO
                            {paginaActual} {paginacionActual}";

                    response.Result.lista = connection.Query<coreUsuariosData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.total = 0;
                response.Result.lista = null;
            }

            return response;
        }

        /// <summary>
        /// Verifica si un usuario ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO CoreUsuariosExiste_Obtener(int CodEmpresa, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDTO();
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
        public ErrorDTO CoreUsuarios_Guardar(int CodEmpresa, coreUsuariosData usuariosData)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDTO { Code = 0, Description = string.Empty };

            try
            {
                // 1) Validaciones
                var errores = new List<string>();

                if (usuariosData == null)
                {
                    return new ErrorDTO
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
        public ErrorDTO<coreUsuariosData> CoreUsuarios_Obtener(int CodEmpresa, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO<coreUsuariosData> resp = new ErrorDTO<coreUsuariosData>();
            resp.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"Select * from CORE_USUARIOS where CORE_USUARIO = '{usuario}'";
                    resp.Result = connection.QueryFirstOrDefault<coreUsuariosData>(query);
                    if (resp.Result == null)
                    {
                        resp.Code = -2;
                        resp.Description = "Usuario no encontrado.";
                    }
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
        public ErrorDTO CoreUsuarios_Importar(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"exec spCORE_Usuarios_Importar";
                    var sp = connection.Query(query);
                }

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
        public ErrorDTO<coreUsuariosData> CoreUsuario_Scroll(int CodEmpresa, int scroll, string? usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO<coreUsuariosData> resp = new ErrorDTO<coreUsuariosData>();
            resp.Code = 0;

            try
            {
                string where = " ", orderBy = " ";
                if (scroll == 1)
                {
                    where = $@" where CORE_USUARIO > '{usuario}' ";
                    orderBy = " order by CORE_USUARIO asc";
                }
                else
                {
                    where = $@" where CORE_USUARIO < '{usuario}' ";
                    orderBy = " order by CORE_USUARIO desc";
                }

                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"Select top 1 * from CORE_USUARIOS {where} {orderBy}";
                    resp.Result = connection.QueryFirstOrDefault<coreUsuariosData>(query);
                    if (resp.Result == null)
                    {
                        resp.Code = -2;
                        resp.Description = "No se encontraron más resultados.";
                    }
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
        private ErrorDTO CoreUsuarios_Insertar(int CodEmpresa, coreUsuariosData usuariosData)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"Insert CORE_USUARIOS(CORE_USUARIO, Usuario_Ref ,Nombre, Registro_Fecha, Registro_Usuario
                                     , Activo, Notas, Email, Tel_Movil )
                                        values('{usuariosData.core_usuario}','{usuariosData.usuario_ref}','{usuariosData.nombre}', 
                                        getdate(),'{usuariosData.registro_usuario}', 1
                                     , '{usuariosData.notas}', '{usuariosData.email}', '{usuariosData.tel_movil}')";
                    connection.Execute(query);
                    resp.Description = "Usuario Ingresado Satisfactoriamente!";
                }
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
        private ErrorDTO CoreUsuarios_Actualizar(int CodEmpresa, coreUsuariosData usuariosData)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            try
            {
                int activo = usuariosData.activo == true ? 1 : 0;
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"Update CORE_USUARIOS Set Nombre = '{usuariosData.nombre}', Activo = {activo} 
                                  , Usuario_Ref = '{usuariosData.usuario_ref}', Notas = '{usuariosData.notas}', 
                                    Email = '{usuariosData.email}',Tel_Movil = '{usuariosData.tel_movil}', 
		                            Modifica_Fecha = Getdate(), Modifica_Usuario = '{usuariosData.modificacion_usuario}' 
                                 Where CORE_USUARIO = '{usuariosData.core_usuario}'";

                    connection.Execute(query);
                    resp.Description = "Usuario Actualizado Satisfactoriamente!";
                }
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
        public ErrorDTO CoreUsuarios_Eliminar(int CodEmpresa, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //var query = $@"delete Miembros where Nombre = '{usuario}'";
                    //connection.Execute(query);

                    //query = $@"delete permisos where Tipo = 'U' and Nombre = '{usuario}'";
                    //connection.Execute(query);

                    //query = $@"delete CORE_USUARIOs where UserID = '{usuario}'";
                    //connection.Execute(query);

                    //Incluir bitacora
                    resp.Description = "No se puede eliminar un Usuario del Sistema!";
                }
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
        public ErrorDTO<List<coreMiembrosData>> CoreUsuariosMiembros_Obtener(int CodEmpresa, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO<List<coreMiembrosData>> resp = new ErrorDTO<List<coreMiembrosData>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"exec spSys_CORE_Users_UENs_Miembros_Consultas {usuario}";
                    resp.Result = connection.Query<coreMiembrosData>(query).ToList();
                }
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
        public ErrorDTO<List<coreMiembrosRolData>> CoreUsuariosUENs_Roles_Obtener(int CodEmpresa, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO<List<coreMiembrosRolData>> resp = new ErrorDTO<List<coreMiembrosRolData>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"exec spSys_CORE_Users_UENs_Roles_Consultas '{usuario}'";
                    resp.Result = connection.Query<coreMiembrosRolData>(query).ToList();
                }
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
        /// Actualiza (inserta o elimina) un miembro (UEN) para un usuario.
        /// </summary>
        /// <param name="miembro"></param>
        /// <returns></returns>
        public ErrorDTO CoreUsuariosMiembro_Actualiza(string miembro)
        {
            CoreMiembro core = JsonConvert.DeserializeObject<CoreMiembro>(miembro);
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(core.codEmpresa);
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;

            try
            {
                string movItem = core.mov == true ? "A" : "E";
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"exec spSys_UENS_Miembros_Registro '{core.uen}', '{core.core_usuario}', '{core.usuario}', '{movItem}' ";
                    connection.Execute(query);
                    resp.Description = "UENs actualizados satisfactoriamente!";
                }

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
        public ErrorDTO CoreUsuariosMiembroRol_Actualiza(string miembroRol)
        {
            CoreMiembroRol core = JsonConvert.DeserializeObject<CoreMiembroRol>(miembroRol);
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(core.codEmpresa);
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;

            try
            {
                int rol_solicita = core.rol_solicita == true ? 1 : 0;
                int rol_consulta = core.rol_consulta == true ? 1 : 0;
                int rol_autoriza = core.rol_autoriza == true ? 1 : 0;
                int rol_encargado = core.rol_encargado == true ? 1 : 0;
                int rol_lider = core.rol_lider == true ? 1 : 0;

                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"exec spSys_UENS_Roles_Registro  '{core.uen}', '{core.core_usuario}', 
                                      {rol_solicita}, {rol_consulta}, {rol_autoriza}, {rol_encargado}, {rol_lider}, '{core.usuario}' ";
                    connection.Execute(query);
                    resp.Description = "Rol Actualizado Satisfactoriamente!";
                }

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