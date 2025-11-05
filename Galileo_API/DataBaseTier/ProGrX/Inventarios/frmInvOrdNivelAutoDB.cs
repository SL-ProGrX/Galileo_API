using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;
using System.Data;


namespace PgxAPI.DataBaseTier
{
    public class frmInvOrdNivelAutoDB
    {
        private readonly IConfiguration _config;

        public frmInvOrdNivelAutoDB(IConfiguration config)
        {
            _config = config;
        }

        #region Autorizaciones


        /// <summary>
        /// Obtiene la lista lazy de autorizadores 
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="pagina"></param>
        /// <param name="paginacion"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<AutorizadorDataLista> Autorizadores_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var response = new ErrorDto<AutorizadorDataLista>
            {
                Result = new AutorizadorDataLista()
            };
            response.Result.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = "SELECT count(*) FROM usuarios U LEFT JOIN pv_orden_autorizadores A ON U.nombre = A.usuario WHERE U.Estado = 'A'";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " AND U.nombre LIKE '%" + filtro + "%' OR U.DESCRIPCION LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"SELECT U.nombre as Usuario,U.descripcion,A.fecha FROM usuarios U LEFT JOIN pv_orden_autorizadores A  on U.nombre = A.usuario WHERE U.Estado = 'A'
                                         {filtro} 
                                        ORDER BY A.fecha ASC
                                        {paginaActual}
                                        {paginacionActual} ";


                    response.Result.Autorizadores = connection.Query<AutorizadorDTO>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Lista los usuarios totales incluídos autorizadores de la empresa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<AutorizadorDTO>> Autorizador_ObtenerTodos(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<AutorizadorDTO>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "SELECT U.nombre as Usuario,U.descripcion,A.fecha FROM usuarios U LEFT JOIN pv_orden_autorizadores A  on U.nombre = A.usuario WHERE U.Estado = 'A' ORDER BY A.fecha DESC";
                    response.Result = connection.Query<AutorizadorDTO>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }


        /// <summary>
        /// Lista los autorizadores de la empresa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<AutorizadorDTO>> Autorizador_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<AutorizadorDTO>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT U.nombre as Usuario,U.descripcion 
                                    FROM usuarios U INNER JOIN pv_orden_autorizadores A 
                                        ON U.nombre = A.usuario WHERE U.Estado = 'A' 
                                   ORDER BY U.nombre";
                    response.Result = connection.Query<AutorizadorDTO>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }


        /// <summary>
        /// Inserta un nuevo autorizador
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Autorizador_Insertar(int CodEmpresa, AutorizadorDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "INSERT pv_orden_autorizadores(USUARIO,FECHA,ESTADO) VALUES (@usuario,@fecha,@estado)";

                    var parameters = new DynamicParameters();
                    parameters.Add("usuario", request.Usuario, DbType.String);
                    parameters.Add("fecha", DateTime.Now, DbType.DateTime);
                    parameters.Add("estado", 'A', DbType.String);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Ok";
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
        /// Elimina un autorizador
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Autorizador_Eliminar(int CodEmpresa, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var procedure = "[spINV_W_Autorizador_Eliminar]";
                    var values = new
                    {
                        Usuario = usuario,
                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        #endregion


        #region Usuarios a Cargo


        /// <summary>
        /// Obtiene la lista lazy de usuarios a cargo de autorizador
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="usuario"></param>
        /// <param name="pagina"></param>
        /// <param name="paginacion"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<UsuariosACargoDataLista> UsuariosACargoAut_Obtener(int CodCliente, string usuario, int? pagina, int? paginacion, string? filtro)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<UsuariosACargoDataLista>();
            response.Result = new UsuariosACargoDataLista();
            response.Result.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = $@"SELECT count(*) FROM usuarios U LEFT JOIN pv_orden_autousers C on U.nombre = C.usuario_asignado AND C.usuario = '{usuario}' WHERE U.Estado = 'A' ";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " AND U.nombre LIKE '%" + filtro + "%' OR U.DESCRIPCION LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"SELECT U.nombre as Usuario,U.descripcion,C.Usuario AS Autorizador, isnull(C.Entradas,0) AS Entradas,isnull(C.Salidas,0) AS Salidas,isnull(C.requisiciones,0) AS Requisiciones,isnull(C.Traslados,0) AS Traslados
                                FROM usuarios U LEFT JOIN pv_orden_autousers C on U.nombre = C.usuario_asignado AND C.usuario = '{usuario}' WHERE U.Estado = 'A' 
                                         {filtro} 
                                        ORDER BY C.fecha_asignacion DESC
                                        {paginaActual}
                                        {paginacionActual} ";


                    response.Result.Usuarios = connection.Query<UsuarioaCargoDTO>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }


        /// <summary>
        /// Obtiene lista de usuarios a cargo de autorizador
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public List<UsuarioaCargoDTO> UsuariosACargo_Obtener(int CodEmpresa, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<UsuarioaCargoDTO> info = new List<UsuarioaCargoDTO>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "SELECT U.nombre as Usuario,U.descripcion,C.Usuario AS Autorizador, isnull(C.Entradas,0) AS Entradas,isnull(C.Salidas,0) AS Salidas,isnull(C.requisiciones,0) AS Requisiciones,isnull(C.Traslados,0) AS Traslados " +
                        "FROM usuarios U LEFT JOIN pv_orden_autousers C on U.nombre = C.usuario_asignado AND C.usuario = @Usuario WHERE U.Estado = 'A' ORDER BY C.fecha_asignacion DESC";

                    var parameters = new DynamicParameters();
                    parameters.Add("Usuario", usuario, DbType.String);


                    info = connection.Query<UsuarioaCargoDTO>(query, parameters).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }


        /// <summary>
        /// Actualiza los usuarios a cargo de autorizador
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto UsuarioACargo_Actualizar(int CodEmpresa, UsuarioaCargoDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var procedure = "[spINV_W_UsuarioACargo_Actualizar]";

                    var values = new
                    {
                        Entradas = request.Entradas,
                        Salidas = request.Salidas,
                        Requisiciones = request.Requisiciones,
                        Traslados = request.Traslados,
                        Autorizador = request.Autorizador,
                        Usuario = request.Usuario,
                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }


        #endregion


        #region Cambio Fecha

        /// <summary>
        /// Obtiene la lista lazy de usuarios que pueden cambiar fecha
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="tipo"></param>
        /// <param name="pagina"></param>
        /// <param name="paginacion"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public UsuariosCambioFchDataLista UsuariosCambioFch_Obtener(int CodCliente, string tipo, int? pagina, int? paginacion, string? filtro)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            UsuariosCambioFchDataLista info = new UsuariosCambioFchDataLista();
            info.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = $@"SELECT count(*) FROM usuarios U LEFT JOIN PV_INVUSRFECHAS A ON U.nombre = A.usuario AND A.tipo= '{tipo}' WHERE U.EStado = 'A' ";
                    info.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " AND U.nombre LIKE '%" + filtro + "%' OR U.DESCRIPCION LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"SELECT U.nombre as Usuario,U.descripcion,A.tipo FROM usuarios U LEFT JOIN PV_INVUSRFECHAS A ON U.nombre = A.usuario AND A.tipo= '{tipo}' WHERE U.EStado = 'A' 
                                         {filtro} 
                                       ORDER BY A.tipo ASC
                                        {paginaActual}
                                        {paginacionActual} ";


                    info.Usuarios = connection.Query<UsuarioaCambioFechaDTO>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }


        /// <summary>
        /// Obtiene la lista de usuarios que pueden cambiar fecha
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public List<UsuarioaCambioFechaDTO> UsuariosCambioFecha_Obtener(int CodEmpresa, string tipo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<UsuarioaCambioFechaDTO> info = new List<UsuarioaCambioFechaDTO>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "SELECT U.nombre as Usuario,U.descripcion,A.tipo FROM usuarios U LEFT JOIN PV_INVUSRFECHAS A ON U.nombre = A.usuario AND A.tipo=@Tipo WHERE U.EStado = 'A' ORDER BY A.tipo DESC";

                    var parameters = new DynamicParameters();
                    parameters.Add("Tipo", tipo, DbType.String);


                    info = connection.Query<UsuarioaCambioFechaDTO>(query, parameters).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }


        /// <summary>
        /// Inserta un nuevo usuario que puede cambiar fecha
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CambioFechas_Insertar(int CodEmpresa, UsuarioaCambioFechaDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "INSERT pv_invusrfechas(USUARIO,TIPO) VALUES (@usuario,@tipo)";

                    var parameters = new DynamicParameters();
                    parameters.Add("usuario", request.Usuario, DbType.String);
                    parameters.Add("tipo", request.Tipo, DbType.String);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Ok";
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
        /// Elimina un usuario que puede cambiar fecha
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CambioFechas_Eliminar(int CodEmpresa, UsuarioaCambioFechaDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "DELETE pv_invusrfechas WHERE USUARIO = @usuario AND TIPO = @tipo";

                    var parameters = new DynamicParameters();
                    parameters.Add("usuario", request.Usuario, DbType.String);
                    parameters.Add("tipo", request.Tipo, DbType.String);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }


        #endregion

    }
}
