using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.CPR;
using PgxAPI.Models.ERROR;


namespace PgxAPI.DataBaseTier
{
    public class frmCprOrdNivelAutoDB
    {
        private readonly IConfiguration _config;

        public frmCprOrdNivelAutoDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtener usuarios autorizadores
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="jFiltros"></param>
        /// <returns></returns>
        public ErrorDTO<UsuariosAuthorizaLista> UsuariosAutorizadores_Obtener(int CodEmpresa, string jFiltros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            FiltroLazy filtros = JsonConvert.DeserializeObject<FiltroLazy>(jFiltros);
            string paginaActual = " ", paginacionActual = " ";
            var response = new ErrorDTO<UsuariosAuthorizaLista>();
            response.Result = new UsuariosAuthorizaLista();


            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var Qtotal = $@"select count(U.nombre) 
                                    from usuarios U left join cpr_orden_autorizadores A  on U.nombre = A.usuario";

                    response.Result.total = connection.Query<int>(Qtotal).FirstOrDefault();

                    string filtro = "";
                    if (filtros.filtro != null)
                    {
                        filtro = " Where U.nombre LIKE '%" + filtros.filtro + "%' OR U.descripcion LIKE '%" + filtros.filtro + "%' ";
                    }

                    if (filtros.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                    }

                    var query = $@"select U.nombre,U.descripcion,A.fecha, CASE 
                                                WHEN A.fecha IS NOT NULL THEN 1
                                                ELSE 0
                                            END AS isCheck
                                    from usuarios U left join cpr_orden_autorizadores A  on U.nombre = A.usuario
                                      {filtro}
                                    order by A.fecha desc 
                                        {paginaActual} {paginacionActual}";

                    response.Result.lista = connection.Query<UsuariosAutorizaData>(query).ToList();
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
        /// OrdenAutousers Insertar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="usuario_asignado"></param>
        /// <returns></returns>
        public ErrorDTO OrdenAutousers_Insertar(int CodEmpresa, string usuario, string usuario_asignado)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"insert cpr_orden_autousers(usuario,usuario_asignado,fecha_Asignacion) 
                                      values ( '{usuario}','{usuario_asignado}' , Getdate() )";
                    var insert = connection.Execute(query);
                    if (insert == 0)
                    {
                        resp.Code = 1;
                        resp.Description = "Error en guardar la orden de autorizaci�n";
                    }
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
        /// Orden AutoUsers eliminar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="usuario_asignado"></param>
        /// <returns></returns>
        public ErrorDTO OrdenAutousers_Eliminar(int CodEmpresa, string usuario, string usuario_asignado)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"delete cpr_orden_autousers where usuario = '{usuario}'  and usuario_asignado = '{usuario_asignado}'";
                    var insert = connection.Execute(query);
                    if (insert == 0)
                    {
                        resp.Code = 1;
                        resp.Description = "Error en borrar la orden de autorizaci�n";
                    }
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
        /// 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO OrdenAutorizadores_Insertar(int CodEmpresa, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"insert cpr_orden_autorizadores(usuario,fecha,estado) values 
                                      ( '{usuario}', Getdate(), 'A' )";
                    var insert = connection.Execute(query);
                    if (insert == 0)
                    {
                        resp.Code = 1;
                        resp.Description = "Error en guardar usuario Autorizadores";
                    }
                }
                resp.Description = $"Usuario Autorizador de Ordenes de Compra: {usuario}";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Orden Autorizadores eliminar    
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO OrdenAutorizadores_Eliminar(int CodEmpresa, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $"delete cpr_orden_autousers where usuario = '{usuario}'";
                    var deleteOA = connection.Execute(query);

                    query = $@"delete cpr_orden_autorizadores where usuario = '{usuario}'";
                    var delete = connection.Execute(query);

                    if (delete == 0)
                    {
                        resp.Code = 1;
                        resp.Description = "Error en borrar la orden de autorizaci�n";
                    }
                }
                resp.Description = $"Usuario Autorizador de Ordenes de Compra: {usuario}";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Cambio de fecha autorizadores
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="jFiltros"></param>
        /// <returns></returns>
        public ErrorDTO<UsuariosAuthorizaLista> FechaCamnbioAutorizadores_Obtener(int CodEmpresa, string jFiltros)
        {
            FiltroLazy filtros = JsonConvert.DeserializeObject<FiltroLazy>(jFiltros);
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<UsuariosAuthorizaLista>();
            response.Result = new UsuariosAuthorizaLista();
            string paginaActual = " ", paginacionActual = " ";

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var Qtotal = $@"select count(U.nombre) 
                                    from usuarios U left join cpr_INVUSRFECHAS A on U.nombre = A.usuario";

                    response.Result.total = connection.Query<int>(Qtotal).FirstOrDefault();

                    string filtro = "";
                    if (filtros.filtro != null)
                    {
                        filtro = " Where U.nombre LIKE '%" + filtros.filtro + "%' OR U.descripcion LIKE '%" + filtros.filtro + "%' ";
                    }

                    if (filtros.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                    }

                    var query = $@" select U.nombre,U.descripcion,A.usuario,
                                                 CASE 
                                                WHEN A.usuario IS NOT NULL THEN 1
                                                ELSE 0
                                            END AS isCheck
                                        from usuarios U left join cpr_INVUSRFECHAS A on U.nombre = A.usuario
                                           {filtro}
                                        order by A.usuario desc {paginaActual} {paginacionActual}";
                    response.Result.lista = connection.Query<UsuariosAutorizaData>(query).ToList();
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
        /// Lista de autorizadores
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDTO<List<UsuariosAutorizaData>> ListaAutorizador_Obtener(int CodEmpresa, string filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

  
            var response = new ErrorDTO<List<UsuariosAutorizaData>>();

            try
            {
                if (filtro == "0")
                {
                    filtro = "";
                }
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select U.nombre,U.nombre + ' - ' + U.descripcion as descripcion
                                        from usuarios U inner join cpr_orden_autorizadores A on U.nombre = A.usuario
                                        Where (U.nombre like '%{filtro}%'
                                        OR U.descripcion like '%{filtro}%')
                                        order by U.nombre";
                    response.Result = connection.Query<UsuariosAutorizaData>(query).ToList();
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
        /// Lista de usuarios Autorizadores
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="jFiltros"></param>
        /// <returns></returns>
        public ErrorDTO<UsuariosAuthorizaLista> ListaAutousers_Obtener(int CodEmpresa, string usuario, string jFiltros)
        {
            FiltroLazy filtros = JsonConvert.DeserializeObject<FiltroLazy>(jFiltros);
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<UsuariosAuthorizaLista>();
            response.Result = new UsuariosAuthorizaLista();
            string paginaActual = " ", paginacionActual = " ";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var Qtotal = $@"select count(U.nombre) 
                                    from usuarios U left join cpr_orden_autousers C on U.nombre = C.usuario_asignado and C.usuario = '{usuario}'";

                    response.Result.total = connection.Query<int>(Qtotal).FirstOrDefault();

                    string filtro = "";
                    if (filtros.filtro != null)
                    {
                        filtro = " Where U.nombre LIKE '%" + filtros.filtro + "%' OR U.descripcion LIKE '%" + filtros.filtro + "%' ";
                    }

                    if (filtros.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                    }

                    var query = $@"select U.nombre,U.descripcion,C.fecha_asignacion, CASE 
                                                WHEN C.fecha_asignacion IS NOT NULL THEN 1
                                                ELSE 0
                                            END AS isCheck
                                     from usuarios U left join cpr_orden_autousers C on U.nombre = C.usuario_asignado
                                        and C.usuario = '{usuario}'
                                      {filtro}
                                    order by C.fecha_asignacion desc 
                                        {paginaActual} {paginacionActual}";

                    response.Result.lista = connection.Query<UsuariosAutorizaData>(query).ToList();

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
        /// Fecha Cambio Autorizadores
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="registro_usuario"></param>
        /// <returns></returns>
        public ErrorDTO FechaCambioAutorizadores_Insertar(int CodEmpresa, string usuario, string registro_usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"insert CPR_INVUSRFECHAS(usuario,registro_fecha,registro_usuario)
                                      values ( '{usuario}', Getdate(), '{registro_usuario}' )";
                    var insert = connection.Execute(query);
                    if (insert == 0)
                    {
                        resp.Code = 1;
                        resp.Description = "Error en guardar la orden de autorizaci�n";
                    }
                }
                resp.Description = $"Usuario Autorizado Cambio Fecha Compras: {usuario}";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Fecha Cambio Autorizadores eliminar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO FechaCambioAutorizadores_Eliminar(int CodEmpresa, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"delete CPR_INVUSRFECHAS where usuario = '{usuario}' ";
                    var insert = connection.Execute(query);
                    if (insert == 0)
                    {
                        resp.Code = 1;
                        resp.Description = "Error en guardar la orden de autorizaci�n";
                    }
                }
                resp.Description = $"Usuario Autorizado Cambio Fecha Compras: {usuario}";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Obtener lista de rangos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<RangosDto>> ObtenerListaRangos(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);


            var response = new ErrorDTO<List<RangosDto>>();

            try
            {
               
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select * from cpr_orden_rangos
";
                    response.Result = connection.Query<RangosDto>(query).ToList();
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
        /// Obtener rangos de usuarios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_rango"></param>
        /// <param name="cod_uen"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDTO<List<RangosUsuariosDto>> obtenerRangoUsuarios(int CodCliente, string cod_rango, string cod_uen, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<RangosUsuariosDto>>();
            response.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = @$"exec spCPR_RANGOS_USUARIOS '{cod_rango}', '{cod_uen}','{filtro}'";
                    response.Result = connection.Query<RangosUsuariosDto>(query).ToList();
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
        /// Registro de rangos de usuarios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="Cod_Categoria"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDTO registroRangosUsuarios(int CodCliente, string Cod_Categoria, RangosUsuariosDto request)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = @$"exec spCpr_RegistroRangosUsuarios '{request.nombre}',{Convert.ToInt32(request.activo)},'{request.cod_rango}',
                    '{request.registro_usuario}',{request.cod_rango_usuario},'{request.uen}'";
                    connection.Execute(query);
                    resp.Description = "Registro actualizado satisfactoriamente";
        

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
        /// Actualizar rangos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDTO Rangos_Actualizar(int CodEmpresa, RangosDto request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {


                    var query = $@"UPDATE cpr_orden_rangos 
                         SET descripcion = '{request.descripcion}', monto_minimo = {request.monto_minimo}, monto_maximo = {request.monto_maximo}, 
                         modifica_fecha = GETDATE(), modifica_usuario = '{request.modifica_usuario}'
                         WHERE cod_rango = '{request.cod_rango}'";

                    resp.Code = connection.ExecuteAsync(query).Result;
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
        /// Rangos Agregar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDTO Rangos_Agregar(int CodEmpresa, RangosDto request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $"select COUNT(*) FROM cpr_orden_rangos WHERE cod_rango = '{request.cod_rango}'";
                    int existe = connection.Query<int>(query).FirstOrDefault();

                    if (existe > 0)
                    {
                        resp.Code = -1;
                        resp.Description = "Ya existe un rango con el c�digo: " + request.cod_rango + ", por favor verifique";
                    }
                    else
                    {

                        query = $@"INSERT INTO cpr_orden_rangos(cod_rango,descripcion,monto_minimo, monto_maximo,registro_fecha, registro_usuario)
                         values('{request.cod_rango}','{request.descripcion}', {request.monto_minimo}, {request.monto_maximo}, getdate(), '{request.registro_usuario}')";

                        resp.Code = connection.ExecuteAsync(query).Result;
                        resp.Description = "Ok";
                    }
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
        /// Eliminar rangos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ErrorDTO Rangos_Eliminar(int CodEmpresa, string id)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"DELETE cpr_orden_rangos where cod_rango = '{id}'";

                    resp.Code = connection.ExecuteAsync(query).Result;
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
    }
}