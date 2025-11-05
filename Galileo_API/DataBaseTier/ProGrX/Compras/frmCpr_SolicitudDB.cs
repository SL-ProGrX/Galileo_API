using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.CPR;
using PgxAPI.Models.CxP;
using PgxAPI.Models.ERROR;
using System.Data;


namespace PgxAPI.DataBaseTier
{
    public class frmCpr_SolicitudDB
    {
        private readonly IConfiguration _config;
        private readonly mProGrX_AuxiliarDB _AuxiliarDB;
        private readonly mSecurityMainDb DBBitacora;
        private readonly EnvioCorreoDB _envioCorreoDB;
        public string sendEmail = "";
        public string TestMail = "";
        public string Notificaciones = "";
        public string EmailUsuario = "";
        public string EncargadoUsuario = "";

        public frmCpr_SolicitudDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new mSecurityMainDb(config);
            _AuxiliarDB = new mProGrX_AuxiliarDB(config);
            sendEmail = _config.GetSection("AppSettings").GetSection("EnviaEmail").Value.ToString();
            _envioCorreoDB = new EnvioCorreoDB(_config);
            Notificaciones = _config.GetSection("AppSettings").GetSection("Notificaciones").Value.ToString();

        }

        public ErrorDto Bitacora(BitacoraInsertarDTO data)
        {
            return DBBitacora.Bitacora(data);
        }

        /// <summary>
        /// Método para validar si el usuario tiene acceso a la solicitud
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<CprSolicitudLista> CprSolicitudLista_Obtener(int CodEmpresa, string filtros)
        {
            CprSolicitudFiltros filtro = JsonConvert.DeserializeObject<CprSolicitudFiltros>(filtros) ?? new CprSolicitudFiltros();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<CprSolicitudLista>();
            response.Result = new CprSolicitudLista
            {
                total = 0
            };
            try
            {
                var query = "";
                string where = " ", paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(stringConn);
                {
                    if (filtro.filtro != null)
                    {
                        where = "where ( S.CPR_ID LIKE '%" + filtro.filtro + "%' OR " +
                            "P.ADJUDICA_ORDEN LIKE '%" + filtro.filtro + "%' OR " +
                            "S.REGISTRO_USUARIO LIKE '%" + filtro.filtro + "%' OR " +
                            "(select DISTINCT DESCRIPCION from CORE_UENS WHERE COD_UNIDAD = S .COD_UNIDAD_SOLICITANTE) LIKE '%" + filtro.filtro + "%' ) ";
                    }

                    if (filtro.solicitante != null && filtro.solicitante.Count > 0)
                    {
                        string usuarios = "";
                        foreach (var item in filtro.solicitante)
                        {
                            if (item != "")
                            {
                                usuarios += "'"+ item + "',";
                            }
                        }
                        where += "and S.REGISTRO_USUARIO IN ("+ usuarios.TrimEnd(',') + ")";
                    }

                    if (filtro.encargado != null && filtro.encargado.Count > 0)
                    {
                        string encargado = "";
                        foreach (var item in filtro.encargado)
                        {
                            if (item != "")
                            {
                                encargado += "'" + item + "',";
                            }
                        }
                        where += "and S.ENCARGADO_USUARIO IN (" + encargado.TrimEnd(',') + ")";
                    }

                    if (filtro.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtro.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtro.paginacion + " ROWS ONLY ";
                    }

                    query = $@"select DISTINCT COUNT(*)
                                    from CPR_SOLICITUD S LEFT JOIN CPR_SOLICITUD_PROV P ON S.CPR_ID = P.CPR_ID 
                                    AND P.ADJUDICA_ORDEN is not null";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();

                    query = $@"select DISTINCT S.CPR_ID, P.ADJUDICA_ORDEN, S.DOCUMENTO, 
    (select DISTINCT DESCRIPCION from CORE_UENS WHERE COD_UNIDAD = S .COD_UNIDAD_SOLICITANTE) AS COD_UNIDAD_SOLICITANTE
    , S.ESTADO, S.REGISTRO_USUARIO, S.ENCARGADO_USUARIO 
    from CPR_SOLICITUD S LEFT JOIN CPR_SOLICITUD_PROV P ON S.CPR_ID = P.CPR_ID
    AND P.ADJUDICA_ORDEN is not null {where}
            order by S.CPR_ID desc {paginaActual} {paginacionActual}";
                    response.Result.solicitudes = connection.Query<CprSolicitudDTO>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.solicitudes = null;
            }

            return response;
        }
        /// <summary>
        /// Método para validar si el usuario tiene acceso a la solicitud
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cpr_id"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CprSolicitudDTO> CprSolicitud_Obtener(int CodEmpresa, int cpr_id, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<CprSolicitudDTO>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $"SELECT ISNULL(ESTADO, 'N') FROM CPR_SOLICITUD_PROV where CPR_ID = {cpr_id}";
                    var listaEstados = connection.Query<string>(query).ToList();
                    bool todosEstadoV = listaEstados.All(e => e.Equals("V"));
                    if (todosEstadoV && listaEstados.Count > 0)
                    {
                        query = $@"select estado from CPR_SOLICITUD WHERE CPR_ID = {cpr_id}";
                        string estadoSoli = connection.Query<string>(query).FirstOrDefault();
                        if (estadoSoli != "D" && estadoSoli != "F")
                        {
                            query = $@"update CPR_SOLICITUD set estado = 'V' where CPR_ID = {cpr_id}";
                            connection.Query(query);
                        }
                    }

                    query = $"select * from CPR_SOLICITUD where CPR_ID = {cpr_id}";
                    response.Result = connection.QueryFirstOrDefault<CprSolicitudDTO>(query);

                    if (response.Result.tipo_orden == CprSolicitud_TipoExcepcion(CodEmpresa).Description)
                    {
                        //busco proveedor
                        query = $@"SELECT PROVEEDOR_CODIGO as com_dir_cod_proveedor,
                                    cp.DESCRIPCION as com_dir_des_proveedor
                                    FROM CPR_SOLICITUD_PROV P
                                    left join CXP_PROVEEDORES cp ON cp.COD_PROVEEDOR = P.PROVEEDOR_CODIGO
                                    WHERE CPR_ID = {cpr_id} ";
                        var proveedor = connection.QueryFirstOrDefault<CprSolicitudDTO>(query);
                        if (proveedor != null)
                        {
                            response.Result.com_dir_cod_proveedor = proveedor.com_dir_cod_proveedor;
                            response.Result.com_dir_des_proveedor = proveedor.com_dir_des_proveedor;
                        }
                    }
                }

                if (ValidaUsuarioSolicitud(CodEmpresa, usuario, "C", response.Result.cod_unidad).Code == -1)
                {
                    response.Code = -1;
                    response.Description = "El usuario no tiene permisos para realizar esta acción";
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
        /// Método para validar si el usuario tiene acceso a la solicitud
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="scroll"></param>
        /// <param name="usuario"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<CprSolicitudDTO> CprSolicitud_Scroll(int CodEmpresa, int scroll, string usuario, string? codigo)
        {
            var clientConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<CprSolicitudDTO>();
            try
            {
                string where = " ", orderBy = " ";
                if (scroll == 1)
                {
                    where = $@" where CPR_ID > '{codigo}' ";
                    orderBy = " order by CPR_ID asc";
                }
                else
                {
                    where = $@" where CPR_ID < '{codigo}' ";
                    orderBy = " order by CPR_ID desc";
                }

                where = $@" {where} and COD_UNIDAD IN ( 
                            select R.COD_UNIDAD FROM CORE_UENS_USUARIOS_ROLES R
                            LEFT JOIN CORE_UENS U ON R.COD_UNIDAD = U.COD_UNIDAD
                            WHERE R.CORE_USUARIO = '{usuario}'
                            AND (R.ROL_CONSULTA = 1 OR R.ROL_ENCARGADO = 1)
                            ) ";

                using var connection = new SqlConnection(clientConnString);
                {

                    var query = $@"select Top 1 * from CPR_SOLICITUD {where} {orderBy}";
                    response.Result = connection.QueryFirstOrDefault<CprSolicitudDTO>(query);
                }



                if (response.Result != null)
                {
                    if (response.Result.tipo_orden == CprSolicitud_TipoExcepcion(CodEmpresa).Description)
                    {
                        //busco proveedor
                        var queryProv = $@"SELECT PROVEEDOR_CODIGO as com_dir_cod_proveedor,
                                    cp.DESCRIPCION as com_dir_des_proveedor
                                    FROM CPR_SOLICITUD_PROV P
                                    left join CXP_PROVEEDORES cp ON cp.COD_PROVEEDOR = P.PROVEEDOR_CODIGO
                                    WHERE CPR_ID = {response.Result.cpr_id} ";
                        var proveedor = connection.QueryFirstOrDefault<CprSolicitudDTO>(queryProv);
                        if (proveedor != null)
                        {
                            response.Result.com_dir_cod_proveedor = proveedor.com_dir_cod_proveedor;
                            response.Result.com_dir_des_proveedor = proveedor.com_dir_des_proveedor;
                        }
                    }

                    if (ValidaUsuarioSolicitud(CodEmpresa, usuario, "C", response.Result.cod_unidad).Code == -1)
                    {
                        response.Code = -1;
                        response.Description = "El usuario no tiene permisos para realizar esta acción";
                    }
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
        /// Método para validar si el usuario tiene acceso a la solicitud
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Edita"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        public ErrorDto CprSolicitud_Guardar(int CodEmpresa, bool Edita, CprSolicitudDTO solicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto error = new()
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    if (Edita)
                    {
                        error = CprSolicitud_Actualizar(CodEmpresa, solicitud);
                    }
                    else
                    {
                        error = CprSolicitud_Insertar(CodEmpresa, solicitud);

                    }
                }
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }

            return error;
        }
        /// <summary>
        /// Método para validar si el usuario tiene acceso a la solicitud
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        private ErrorDto CprSolicitud_Insertar(int CodEmpresa, CprSolicitudDTO solicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto error = new()
            {
                Code = 0
            };

            try
            {

                if (solicitud.monto < CprSolicitud_gastoMenorMonto(CodEmpresa).Result && 
                    (solicitud.tipo_orden != CprSolicitud_TipoExcepcionGM(CodEmpresa).Description 
                    && solicitud.tipo_orden != CprSolicitud_TipoExcepcion(CodEmpresa).Description))
                {
                    error.Code = -1;
                    error.Description = "El monto de la orden clasifica como gasto menor";
                    return error;
                }

                if (solicitud.monto > CprSolicitud_gastoMenorMonto(CodEmpresa).Result &&
                   (solicitud.tipo_orden == CprSolicitud_TipoExcepcionGM(CodEmpresa).Description
                   && solicitud.tipo_orden == CprSolicitud_TipoExcepcion(CodEmpresa).Description))
                {
                    error.Code = -1;
                    error.Description = "El monto de la orden es muy alto para esta clasificación";
                    return error;
                }

                using var connection = new SqlConnection(stringConn);
                {
                    //Busco el ultimo siguiente consecutivo si es null es el primero de la tabla como 0
                    var queryID = "SELECT ISNULL(MAX(CPR_ID),0) + 1 FROM CPR_SOLICITUD";
                    var secuencia = connection.Query<int>(queryID).FirstOrDefault();
                    solicitud.cpr_id = secuencia;

                    string xmlOutput = _AuxiliarDB.fxConvertModelToXml<CprSolicitudDTO>(solicitud);

                    var insert = $@"exec spCPR_Solicitud_Insertar '{xmlOutput}'";

                    var respuesta = connection.Query(insert);


                    error.Description = solicitud.cpr_id.ToString();

                    AsignaEncargado_Solicitud(CodEmpresa, solicitud.cod_unidad_solicitante, secuencia);

                    //if (solicitud.tipo_orden == CprSolicitud_TipoExcepcion(CodEmpresa).Description
                    //    && solicitud.monto < CprSolicitud_gastoMenorMonto(CodEmpresa).Result)
                    //{
                    //    CompraDirectaProv_Agregar(CodEmpresa, (int)solicitud.cpr_id, solicitud);
                    //}

                    var queryUsuario = @$"SELECT ENCARGADO_USUARIO FROM CPR_SOLICITUD WHERE CPR_ID = {secuencia}";
                    string usuario = connection.Query<string>(queryUsuario).FirstOrDefault();

                    CorreoNotificaSolicitud_Enviar(CodEmpresa, secuencia, usuario);


                }
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }

            return error;
        }
        /// <summary>
        /// Método para validar si el usuario tiene acceso a la solicitud
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        private ErrorDto CprSolicitud_Actualizar(int CodEmpresa, CprSolicitudDTO solicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto error = new()
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string xmlOutput = _AuxiliarDB.fxConvertModelToXml<CprSolicitudDTO>(solicitud);
                    var query = $@"exec spCPR_Solicitud_Actualizar '{xmlOutput}'";

                    var respuesta = connection.Query(query);
                    error.Description = solicitud.cpr_id.ToString();
                }
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }

            return error;
        }
        /// <summary>
        /// Método para validar si el usuario tiene acceso a la solicitud
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cpr_id"></param>
        /// <returns></returns>
        public ErrorDto CprSolicitud_Eliminar(int CodEmpresa, int cpr_id)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto error = new()
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"exec spCPR_Solicitud_Eliminar '{cpr_id}'";

                    connection.ExecuteAsync(query);
                    error.Description = cpr_id.ToString();
                }
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }

            return error;
        }

        /// <summary>
        /// Método para obtener la lista de productos de la solicitud
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cpr_id"></param>
        /// <param name="cod_unidad"></param>
        /// <returns></returns>
        public ErrorDto<List<CprSolicitudBsDTO>> CprSolicitudBs_Obtener(int CodEmpresa, int? cpr_id, string? cod_unidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CprSolicitudBsDTO>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $"exec spCPR_SolicitudDetalle_Consultar {cpr_id}, '{cod_unidad}' ";
                    response.Result = connection.Query<CprSolicitudBsDTO>(query).ToList();

                    foreach (CprSolicitudBsDTO item in response.Result)
                    {
                        {
                            var queryUnidad = $@"SELECT COD_UNIDAD FROM PV_PRODUCTOS WHERE COD_PRODUCTO = '{item.cod_producto}'";

                            var unidad = connection.QuerySingleOrDefault<string>(queryUnidad);

                            item.unidad = unidad;

                        }
                    }
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
        /// Método para guardar la solicitud de compra
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="editaBs"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        public ErrorDto CprSolicitudBs_Guardar(int CodEmpresa, bool editaBs, CprSolicitudBsDTO solicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto response = new()
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string xmlOutput = _AuxiliarDB.fxConvertModelToXml<CprSolicitudBsDTO>(solicitud);

                    var insert = $@"exec spCPR_SolicitudDetalle_Guardar '{xmlOutput}'";
                    var result = connection.Execute(insert);
                    response.Description = solicitud.cpr_id.ToString();

                    //valido si es compra directa
                    var query = $@"SELECT TIPO_ORDEN FROM CPR_SOLICITUD WHERE CPR_ID = {solicitud.cpr_id}";
                    string tipo_orden = connection.Query<string>(query).FirstOrDefault();

                    if (tipo_orden == CprSolicitud_TipoExcepcion(CodEmpresa).Description)
                    {
                        CompraDirectaProvBs_Guardar(CodEmpresa, solicitud);
                    }


                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }
        /// <summary>
        /// Método para eliminar la solicitud de compra
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cpr_id"></param>
        /// <param name="cod_producto"></param>
        /// <param name="cod_unidad"></param>
        /// <returns></returns>
        public ErrorDto CprSolicitudBs_Eliminar(int CodEmpresa, int cpr_id, string cod_producto, string cod_unidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto response = new()
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spCPR_SolicitudDetalle_Eliminar {cpr_id}, '{cod_producto}', '{cod_unidad}' ";

                    connection.ExecuteAsync(query);
                    response.Description = cpr_id.ToString();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }
        /// <summary>
        /// Método para validar si el usuario tiene acceso a la solicitud
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CprValoracionLista>> CprValoracionesLista_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CprValoracionLista>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $"select VAL_ID as item , descripcion from CPR_VALORA_ESQUEMA ";
                    response.Result = connection.Query<CprValoracionLista>(query).ToList();
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
        /// Método para obtener la lista de UENs por usuario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<CprUensLista>> CprUens_Obtener(int CodEmpresa, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CprUensLista>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select R.COD_UNIDAD item, U.DESCRIPCION, 
                    (select TOP 1 DESCRIPCION from CNTX_UNIDADES WHERE COD_UNIDAD = U.CNTX_UNIDAD) AS CNTX_UNIDAD,
                    (select TOP 1 DESCRIPCION from CNTX_CENTRO_COSTOS WHERE COD_CENTRO_COSTO = U.CNTX_CENTRO_COSTO) AS CNTX_CENTRO_COSTO
                    FROM CORE_UENS_USUARIOS_ROLES R LEFT JOIN CORE_UENS U 
                    ON R.COD_UNIDAD = U.COD_UNIDAD WHERE R.CORE_USUARIO = '{usuario}'";
                    response.Result = connection.Query<CprUensLista>(query).ToList();
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
        /// Método para obtener la lista de UENs
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CprValoracionLista>> CprSolicitudUens_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CprValoracionLista>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT [COD_UNIDAD] AS ITEM
                                      ,[DESCRIPCION] 
                                  FROM [dbo].[CORE_UENS]";
                    response.Result = connection.Query<CprValoracionLista>(query).ToList();
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
        /// Método para validar si el usuario tiene acceso a la solicitud
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_producto"></param>
        /// <returns></returns>
        public ErrorDto CprSolicitudBuscaProdPlan_Obtener(int CodEmpresa, string cod_producto)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto error = new()
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //valida si existe
                    var query = $@"exec spCPR_Solicitud_ProductoPlan_Consultar '{cod_producto}'";
                    var resp = connection.Query<int>(query).FirstOrDefault();
                    error.Description = resp.ToString();
                }
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }

            return error;
        }

        /// <summary>
        /// Método para obtener la cantidad de producto en el plan
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_producto"></param>
        /// <param name="cantidad"></param>
        /// <returns></returns>
        public ErrorDto CprSolicitudBuscaProdCantPlan_Obtener(int CodEmpresa, string cod_producto, float cantidad)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto error = new ErrorDto();
            error.Code = 0;

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    // Abrimos la conexión
                    connection.Open();

                    // Parámetros para la consulta
                    var parametros = new DynamicParameters();
                    parametros.Add("@cod_producto", cod_producto);
                    parametros.Add("@cantidad", cantidad);

                    // Parámetro de salida
                    parametros.Add("@ValorSalida", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);

                    // Ejecución del procedimiento almacenado
                    connection.Execute("spCPR_SolicitudProductoCantPlan_Consultar", parametros, commandType: System.Data.CommandType.StoredProcedure);

                    // Obtener el valor del parámetro de salida
                    string resultado = parametros.Get<string>("@ValorSalida");

                    error.Description = resultado;
                }
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }

            return error;
        }

        /// <summary>
        /// Método para obtener el seguimiento de la solicitud
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_solicitud"></param>
        /// <returns></returns>
        public ErrorDto<List<CprSolicitudSeguimientoDTO>> Segumiento_Obtener(int CodCliente, int cod_solicitud)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<CprSolicitudSeguimientoDTO>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"
                                    SELECT REGISTRO_FECHA, REGISTRO_USUARIO, AUTORIZA_FECHA,AUTORIZA_USUARIO,MODIFICA_FECHA, MODIFICA_USUARIO, PRESUPUESTO_USUARIO, PRESUPUESTO_FECHA,
                            ADJUDICA_USUARIO, ADJUDICA_FECHA, DETALLE_SEGUIMIENTO FROM CPR_SOLICITUD WHERE CPR_ID = {cod_solicitud}";

                    response.Result = connection.Query<CprSolicitudSeguimientoDTO>(query).ToList();
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
        /// Método para obtener la lista de cotizaciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cpr_id"></param>
        /// <param name="cod_unidad"></param>
        /// <param name="cod_cotizacion"></param>
        /// <returns></returns>
        public ErrorDto<CprSolicitudCotizacionPrvBsLista> CprSolicitudCotizacionBs_Obtener(int CodEmpresa, int? cpr_id, string? cod_unidad, string cod_cotizacion)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<CprSolicitudCotizacionPrvBsLista>();
            response.Result = new CprSolicitudCotizacionPrvBsLista();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    if (cod_cotizacion == null)
                    {

                        var query = $@"exec [spCPR_SolicitudCotizacion_Consultar] '{cpr_id}','{cod_unidad}'";
                        response.Result.cotizaciones = connection.Query<CprSolicitudCotizacionPrvBs>(query).ToList();

                    }
                    else
                    {
                        var query = $@"exec [spCPR_SolicitudCotizacion_Consultar] '{cpr_id}','{cod_unidad}','{cod_cotizacion}'";
                        response.Result.cotizaciones = connection.Query<CprSolicitudCotizacionPrvBs>(query).ToList();

                    }

                    foreach (CprSolicitudCotizacionPrvBs item in response.Result.cotizaciones)
                    {
                        {
                            var queryUnidad = $@"SELECT COD_UNIDAD FROM PV_PRODUCTOS WHERE COD_PRODUCTO = '{item.cod_producto}'";

                            var unidad = connection.QuerySingleOrDefault<string>(queryUnidad);

                            item.unidad = unidad;

                        }


                    }
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
        /// Método para autorizar la solicitud
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CPR_ID"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto AutorizaSolicitud(int CodEmpresa, int CPR_ID, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //valido el estado actual:
                    var queryEstado = $@"SELECT ESTADO FROM CPR_SOLICITUD WHERE CPR_ID = {CPR_ID}";
                    string estado = connection.Query<string>(queryEstado).FirstOrDefault();

                    //si el estado es A o F no se puede cambiar
                    if (estado == "A" || estado == "F")
                    {
                        info.Code = -1;
                        info.Description = "El estado de la solicitud no permite autorizarla";
                        return info;
                    }

                    var query = $@"UPDATE CPR_SOLICITUD
                                            SET estado = 'A', AUTORIZA_FECHA = GETDATE(), AUTORIZA_USUARIO = '{usuario}'
                                            WHERE CPR_ID = {CPR_ID}";
                    info.Code = connection.Execute(query);

                    //busco tipo de orden
                    var queryTipoOrden = $@"SELECT TIPO_ORDEN FROM CPR_SOLICITUD WHERE CPR_ID = {CPR_ID}";
                    string tipoOrden = connection.Query<string>(queryTipoOrden).FirstOrDefault();

                    //if (tipoOrden == CprSolicitud_TipoExcepcion(CodEmpresa).Description)
                    //{
                    //    info = CompraDirectaSolicitud_Autorizar(CodEmpresa, CPR_ID, usuario);
                    //}

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;
        }

        /// <summary>
        /// Método para denegar la solicitud
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CPR_ID"></param>
        /// <param name="usuario"></param>
        /// <param name="detalle_seguimiento"></param>
        /// <returns></returns>
        public ErrorDto DeniegaSolicitud(int CodEmpresa, int CPR_ID, string usuario, string detalle_seguimiento)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"UPDATE CPR_SOLICITUD
                                            SET estado = 'D', AUTORIZA_FECHA = GETDATE(), MODIFICA_USUARIO = '{usuario}',
                                            detalle_seguimiento = '{detalle_seguimiento}'
                                             
                                            WHERE CPR_ID = {CPR_ID}";
                    info.Code = connection.Execute(query);

                    CorreoNotificacionDevolucion_Enviar(CodEmpresa, CPR_ID);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;
        }

        /// <summary>
        /// Método para validar si el usuario tiene permisos para realizar la acción
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="permiso"></param>
        /// <param name="cod_unidad"></param>
        /// <returns></returns>
        public ErrorDto ValidaUsuarioSolicitud(int CodEmpresa, string usuario, string permiso, string? cod_unidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto();

            try
            {
                string valorPermiso = " ";

                if (cod_unidad != null)
                {
                    valorPermiso = $" AND R.COD_UNIDAD = '{cod_unidad}' ";
                }

                switch (permiso)
                {
                    case "C":
                        valorPermiso += " AND R.ROL_CONSULTA = 1 ";
                        break;
                    case "A":
                        valorPermiso += " AND R.ROL_AUTORIZA = 1 ";
                        break;
                    case "S":
                        valorPermiso += " AND R.ROL_SOLICITA = 1 ";
                        break;
                    case "E":
                        valorPermiso += " AND R.ROL_ENCARGADO = 1 ";
                        break;
                    case "L":
                        valorPermiso += " AND R.ROL_LIDER = 1 ";
                        break;
                    default:
                        valorPermiso += " ";
                        break;
                }

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select 'X' Existe FROM CORE_UENS_USUARIOS_ROLES R
                                    LEFT JOIN CORE_UENS U ON R.COD_UNIDAD = U.COD_UNIDAD
                                    WHERE R.CORE_USUARIO = '{usuario}'
                                    {valorPermiso}";

                    var existe = connection.Query<string>(query).FirstOrDefault();

                    if (existe == null)
                    {
                        response.Code = -1;
                        response.Description = "El usuario no tiene permisos para realizar esta acción";
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Método para obtener los artículos de la compra directa
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="pagina"></param>
        /// <param name="paginacion"></param>
        /// <param name="filtro"></param>
        /// <param name="cod_unidad"></param>
        /// <returns></returns>
        public ErrorDto<ArticuloDataLista> Articulos_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro, string? cod_unidad)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<ArticuloDataLista>();
            response.Result = new ArticuloDataLista();
            response.Result.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = $@"SELECT COUNT(*) FROM (
                        select DISTINCT D.COD_PRODUCTO  from CPR_PLAN_DT D
                        INNER JOIN CPR_PLAN_COMPRAS C ON D.ID_PC = C.ID_PC
                        INNER JOIN CPR_PLAN_DT_CORTES S ON D.ID_PLAN = S.ID_PLAN
                        INNER JOIN PV_PRODUCTOS P ON D.COD_PRODUCTO = P.COD_PRODUCTO
						WHERE S.CORTE >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)) T ";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {

                        filtro = " AND D.COD_PRODUCTO LIKE '%" + filtro + "%' " +
                             "OR P.DESCRIPCION LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"select DISTINCT D.COD_PRODUCTO, P.DESCRIPCION, P.COSTO_REGULAR, P.EXISTENCIA, P.COD_BARRAS, 
                                P.CABYS, P.PRECIO_REGULAR, P.IMPUESTO_VENTAS, P.COD_FABRICANTE, P.I_STOCK from CPR_PLAN_DT D
                                                        INNER JOIN CPR_PLAN_COMPRAS C ON D.ID_PC = C.ID_PC AND (C.COD_UNIDAD = '{cod_unidad}' OR C.COD_UNIDAD_DESTINO = '{cod_unidad}' )
                                                        INNER JOIN CPR_PLAN_DT_CORTES S ON D.ID_PLAN = S.ID_PLAN
                                                        INNER JOIN PV_PRODUCTOS P ON D.COD_PRODUCTO = P.COD_PRODUCTO
						                                WHERE S.CORTE >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)
                                                       {filtro} ORDER BY  D.COD_PRODUCTO  {paginaActual}
                                                      {paginacionActual} ";


                    response.Result.Articulos = connection.Query<ArticuloData>(query).ToList();

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
        /// Método para agregar el proveedor a la compra directa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cpr_id"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        public ErrorDto CompraDirectaProv_Agregar(int CodEmpresa, int cpr_id, CprSolicitudDTO solicitud)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //encabezado
                    var query = @$"UPDATE CPR_SOLICITUD_PROV SET
                                        PROVEEDOR_ESTADO = 'I', 
                                        ESTADO = 'V' 
                                    WHERE CPR_ID = {solicitud.cpr_id}  ";
                    response.Code = connection.Execute(query);

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;

        }

        /// <summary>
        /// Método para guardar la compra directa
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        private ErrorDto CompraDirectaProvBs_Guardar(int CodEmpresa, CprSolicitudBsDTO solicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    //reviso si existe proveedor 
                    var queryProv = $@"SELECT COUNT(*) FROM CPR_SOLICITUD_PROV WHERE CPR_ID = {solicitud.cpr_id}";
                    var existeProv = connection.Query<int>(queryProv).FirstOrDefault();

                    if (existeProv == 0)
                    {
                        CprSolicitudDTO solicitudEncabezado = new CprSolicitudDTO();
                        solicitudEncabezado.cpr_id = solicitud.cpr_id;
                        solicitudEncabezado.com_dir_cod_proveedor = solicitud.comp_dir_cod_proveedor;
                        solicitudEncabezado.registro_usuario = solicitud.registro_usuario;

                        CompraDirectaProv_Agregar(CodEmpresa, (int)solicitud.cpr_id, solicitudEncabezado);
                    }

                    //elimino lineas
                    var queryDelete = $@"DELETE FROM CPR_SOLICITUD_PROV_BS WHERE CPR_ID = {solicitud.cpr_id}";
                    connection.Execute(queryDelete);

                    //detalle
                    var queryDetalle = $@"INSERT INTO CPR_SOLICITUD_PROV_BS (CPR_ID, COD_PRODUCTO, PROVEEDOR_CODIGO, CODIGO, MONTO, CANTIDAD, TOTAL , 
                                        IVA_PORC,IVA_MONTO , DESC_PORC, DESC_MONTO, registro_fecha, registro_usuario, ESTADO, NO_COTIZACION)
                                        SELECT CPR_ID, COD_PRODUCTO , {solicitud.comp_dir_cod_proveedor} AS COD_PROVEEDOR , NULL AS CODIGO , MONTO, CANTIDAD, TOTAL,
                                        IVA_PORC AS IVA_PORC, IVA_MONTO AS IVA_MONTO, DESC_PORC as DESC_PORC, DESC_MONTO AS DESC_MONTO, getdate() as registro_fecha, 
                                        registro_usuario , 'V' as ESTADO, '{solicitud.comp_dir_documento}' AS NO_COTIZACION FROM CPR_SOLICITUD_BS csb WHERE CPR_ID = {solicitud.cpr_id}";

                    response.Code = connection.Execute(queryDetalle);

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;


        }

      

        /// <summary>
        /// Método para enviar correo de notificación de devolución
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="CPR_ID"></param>
        /// <returns></returns>
        public async Task<ErrorDto> CorreoNotificacionDevolucion_Enviar(int CodCliente, int CPR_ID)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            EnvioCorreoModels eConfig = new();
            string emailCobros = "";
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    eConfig = _envioCorreoDB.CorreoConfig(CodCliente, Notificaciones);


                    var query = $@"SELECT detalle_seguimiento FROM CPR_SOLICITUD WHERE cpr_id = {CPR_ID}";
                    string detalle = connection.QueryFirstOrDefault<string>(query);

                    var queryUsuario = $@"SELECT autoriza_usuario FROM CPR_SOLICITUD WHERE cpr_id = {CPR_ID}";
                    string usuario = connection.QueryFirstOrDefault<string>(query);

                    var queryCorreo = $@"SELECT email FROM USUARIOS WHERE cpr_id = '{usuario}'";
                    string Correousuario = connection.QueryFirstOrDefault<string>(query);


                    string body = @$"<html lang=""es"">
                            <head>
                                <meta charset=""UTF-8"">
                                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                <title>Solicitud de Cotización</title>
                                <style>
                                    body {{
                                        font-family: Arial, sans-serif;
                                    }}
                                    .container {{
                                        width: 600px;
                                        margin: 0 auto;
                                        border: 1px solid #eaeaea;
                                        padding: 20px;
                                    }}
                                    .header {{
                                        background-color: #e8f3ff;
                                        padding: 10px;
                                    }}
                                    .header img {{
                                        width: auto;
                                        height: 50px;
                                    }}
                                    .content {{
                                        margin-top: 20px;
                                    }}
                                    .content h2 {{
                                        font-size: 16px;
                                        color: #0072ce;
                                    }}
                                    .table {{
                                        width: 100%;
                                        margin-top: 20px;
                                        border-collapse: collapse;
                                    }}
                                    .table th, .table td {{
                                        padding: 10px;
                                        border: 1px solid #dcdcdc;
                                        text-align: left;
                                    }}
                                    .table th {{
                                        background-color: #0072ce;
                                        color: white;
                                    }}
                                    
                                </style>
                            </head>
                            <body>
                                <div class=""container"">
                                    <div class=""header"">
                                        <img src=""https://www.aseccssenlinea.com/Content/Login/ASECCSSLogo.png"" alt=""Logo"">
                                    </div>
                                    <div class=""content"">
                                        <h2><strong>Anulación Solicitud de Compra</strong> </h2>
                                        <p>No. Solicitud <strong>{CPR_ID}</strong> </p>
                                        <p>Mediante la presente se le comunica la anulación de la Solicitud de Compra #{CPR_ID}</p>
                                        <p>Justificación: {detalle}</p>";

                    List<IFormFile> Attachments = new List<IFormFile>();

                    //var file = ConvertByteArrayToIFormFileList(parametros.filecontent, parametros.filename);

                    //Attachments.AddRange(file);

                    if (sendEmail == "Y")
                    {
                        EmailRequest emailRequest = new EmailRequest();

                        emailRequest.To = Correousuario;
                        emailRequest.From = eConfig.User;
                        emailRequest.Subject = "Anulación de Solicitud";
                        emailRequest.Body = body;
                        //emailRequest.Attachments = Attachments;

                        if (eConfig != null)
                        {
                            await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, info);
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        /// <summary>
        /// Regresa el tipo de excepción para la compra directa
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto CprSolicitud_TipoExcepcion(int CodCliente)
        {
            ErrorDto info = new();
            try
            {
                info.Code = 0;
                info.Description = _config.GetSection("Crp_Compras").GetSection("CrpCompraDirecta").Value.ToString();
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }

            return info;
        }

        /// <summary>
        /// Regresa el tipo de excepción para el gasto menor
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto CprSolicitud_TipoExcepcionGM(int CodCliente)
        {
            ErrorDto info = new();
            try
            {
                info.Code = 0;
                info.Description = _config.GetSection("Crp_Compras").GetSection("CrpCompraGastoMenor").Value.ToString();
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }

            return info;
        }

        /// <summary>
        /// Regreso el mínimo de monto para gasto menor
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<float> CprSolicitud_gastoMenorMonto(int CodCliente)
        {
            ErrorDto<float> info = new ErrorDto<float>();
            try
            {
                info.Code = 0;
                info.Result = float.Parse(_config.GetSection("Crp_Compras").GetSection("CrpCompraGM_Monto").Value.ToString());
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
                info.Result = 0;
            }

            return info;
        }
      
      /// <summary>
/// Asigna encargado por Solicitud
/// </summary>
/// <param name="CodCliente"></param>
/// <param name="cod_unidad"></param>
/// <param name="cpr_id"></param>
/// <returns></returns>
public ErrorDto AsignaEncargado_Solicitud(int CodCliente, string cod_unidad, int cpr_id)
{
    ErrorDto info = new();
    try
    {

        var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
        ErrorDto resp = new ErrorDto();
        resp.Code = 0;
        try
        {

            using var connection = new SqlConnection(clienteConnString);
            {
                var query = @$"exec spCpr_AsignaEncargadoPorSolicitud {cpr_id},'{cod_unidad}'";
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
    catch (Exception ex)
    {
        info.Code = -1;
        info.Description = ex.Message;
    }

    return info;
}


    /// <summary>
    /// Notifica Asignacion Solicitud
    /// </summary>
    /// <param name="CodEmpresa"></param>
    /// <param name="cpr_id"></param>
    /// <param name="usuario"></param>
    /// <returns></returns>
    private async Task CorreoNotificaSolicitud_Enviar(int CodEmpresa, int cpr_id, string usuario)
    {

        ErrorDto resp = new ErrorDto();
        string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
        List<OrdenLineas> info = new List<OrdenLineas>();

        int CodCliente = CodEmpresa;

        string solicitudMascara = cpr_id.ToString("D10");

        try
        {

            using var connection = new SqlConnection(stringConn);
            {

                var queryEmail = @$"SELECT EMAIL FROM CORE_USUARIOS";
                string EmailUsuario = connection.Query<string>(queryEmail).FirstOrDefault();

            }


            EnvioCorreoModels eConfig = _envioCorreoDB.CorreoConfig(CodEmpresa, Notificaciones);
            string body = @$"<html lang=""es"">
                        <head>
                            <meta charset=""UTF-8"">
                            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                            <title>Solicitud de Cotización</title>
                            <style>
                                body {{
                                    font-family: Arial, sans-serif;
                                }}
                                .container {{
                                    width: 600px;
                                    margin: 0 auto;
                                    border: 1px solid #eaeaea;
                                    padding: 20px;
                                }}
                                .header {{
                                    background-color: #e8f3ff;
                                    padding: 10px;
                                }}
                                .header img {{
                                    width: auto;
                                    height: 50px;
                                }}
                                .content {{
                                    margin-top: 20px;
                                }}
                                .content h2 {{
                                    font-size: 16px;
                                    color: #0072ce;
                                }}
                                .table {{
                                    width: 100%;
                                    margin-top: 20px;
                                    border-collapse: collapse;
                                }}
                                .table th, .table td {{
                                    padding: 10px;
                                    border: 1px solid #dcdcdc;
                                    text-align: left;
                                }}
                                .table th {{
                                    background-color: #0072ce;
                                    color: white;
                                }}

                            </style>
                        </head>
                        <body>
                            <div class=""container"">
                                <div class=""header"">
                                    <img src=""https://www.aseccssenlinea.com/Content/Login/ASECCSSLogo.png"" alt=""Logo"">
                                </div>
                                <div class=""content"">
                                    <h2><strong>Notificación de asignación de Solicitud de Compra</strong></h2>
                                    <p>Estimado/a {usuario} se le comunica la asignación de la solicitud de compra #{solicitudMascara}</p>";



            List<IFormFile> Attachments = new List<IFormFile>();


            if (sendEmail == "Y")
            {
                EmailRequest emailRequest = new EmailRequest();

                emailRequest.To = EmailUsuario;
                emailRequest.From = eConfig.User;
                emailRequest.Subject = "Asignación de Solicitud de Compra";
                emailRequest.Body = body;
                emailRequest.Attachments = Attachments;

                if (eConfig != null)
                {
                    await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, resp);
                }
            }
        }
        catch (Exception ex)
        {
            resp.Code = -1;
            resp.Description = ex.Message;
        }
    }


        /// <summary>
        /// Obtiene Encargados de UEns
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_unidad"></param>
        /// <returns></returns>
        public ErrorDto<List<EncargadosDto>> Encargados_Obtener(int CodEmpresa, int cod_unidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<EncargadosDto>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"SELECT u.CORE_USUARIO,u.NOMBRE, u.EMAIL,ur.COD_UNIDAD
                                FROM CORE_UENS_USUARIOS_ROLES ur
                                INNER JOIN CORE_USUARIOS u ON ur.CORE_USUARIO = u.CORE_USUARIO WHERE ROL_ENCARGADO = 1 AND COD_UNIDAD = {cod_unidad}";
                    response.Result = connection.Query<EncargadosDto>(query).ToList();
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
/// Obtiene lista de los usuarios solicitantes
/// </summary>
/// <param name="CodEmpresa"></param>
/// <returns></returns>
public ErrorDto<List<string>> CprSolicitud_UsuariosSolicitantes_Obtener(int CodEmpresa)
{
 
    string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
 
    var response = new ErrorDto<List<string>>();
 
    try
    {
        using var connection = new SqlConnection(stringConn);
        {
            var query = @$"select REGISTRO_USUARIO 
            from CPR_SOLICITUD 
            GROUP BY REGISTRO_USUARIO";
            response.Result = connection.Query<string>(query).ToList();
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
/// Obtiene lista de los usuarios encargados
/// </summary>
/// <param name="CodEmpresa"></param>
/// <returns></returns>
public ErrorDto<List<string>> CprSolicitud_UsuariosEncargados_Obtener(int CodEmpresa)
{
 
    string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
 
    var response = new ErrorDto<List<string>>();
 
    try
    {
        using var connection = new SqlConnection(stringConn);
        {
            var query = @$"select ENCARGADO_USUARIO 
            from CPR_SOLICITUD 
            WHERE ENCARGADO_USUARIO IS NOT NULL
            GROUP BY ENCARGADO_USUARIO";
            response.Result = connection.Query<string>(query).ToList();
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
      
    }


}