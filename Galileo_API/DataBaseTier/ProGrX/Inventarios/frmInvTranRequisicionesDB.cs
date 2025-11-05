using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.BusinessLogic;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmInvTranRequisicionesDB
    {
        private readonly IConfiguration _config;

        public frmInvTranRequisicionesDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtener requsicion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodRequisicion"></param>
        /// <returns></returns>
        public ErrorDto<TranRequisicionData> InvTranRequisicion_Obtener(int CodEmpresa, int CodRequisicion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TranRequisicionData>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select X.*,(rtrim(C.cod_entsal) + ' - ' + C.descripcion) as Causa 
                        from pv_requisiciones X inner join pv_entrada_salida C on X.cod_entsal = C.cod_entsal
                        where X.cod_requisicion = {CodRequisicion}";
                    response.Result = connection.Query<TranRequisicionData>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            if (response.Result != null)
            {
                switch (response.Result.Estado)
                {
                    case "S":
                        response.Result.Estado = "Solicitada";
                        break;
                    case "P":
                        response.Result.Estado = "Procesada";
                        break;
                    case "A":
                        response.Result.Estado = "Autorizada";
                        break;
                    case "R":
                        response.Result.Estado = "Rechazada";
                        break;
                    case "N":
                        response.Result.Estado = "Procesada - Pendiente";
                        break;
                    default:
                        break;
                }
            }
            return response;
        }

        /// <summary>
        /// Obtener requiscion producto obtener
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodRequisicion"></param>
        /// <returns></returns>
        public ErrorDto<List<InvReqProduc>> InvRequesicionProduc_Obtener(int CodEmpresa, int CodRequisicion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<InvReqProduc>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"select D.linea, D.Cod_Producto,P.Descripcion,D.Cantidad,D.Costo,
                        (D.cantidad * D.Costo) as Total,isnull(D.despacho,0) as Despacho, D.Cod_Bodega, B.descripcion as Bodega, D.solicitado
                        from pv_requi_detalle D inner join pv_productos P on D.cod_producto = P.cod_producto
                        inner join PV_Bodegas B on D.cod_bodega = B.cod_bodega
                        where D.cod_requisicion = {CodRequisicion}
                        order by D.Linea";
                    response.Result = connection.Query<InvReqProduc>(query).ToList();
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
        /// Obtener requisicion scroll
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="scrollValue"></param>
        /// <param name="CodRequisicion"></param>
        /// <returns></returns>
        public ErrorDto<TranRequisicionData> InvTranRequisicion_scroll(int CodEmpresa, int scrollValue, int? CodRequisicion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TranRequisicionData>
            {
                Code = 0
            };
            try
            {
                string filtro;

                if (scrollValue == 1)
                {
                    filtro = $"where cod_requisicion > {CodRequisicion} order by cod_requisicion asc";
                }
                else
                {
                    filtro = $"where cod_requisicion < {CodRequisicion} order by cod_requisicion desc";
                }

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Top 1 cod_requisicion from pv_requisiciones {filtro}";
                    response.Result = connection.Query<TranRequisicionData>(query).FirstOrDefault();
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
       /// Insertar requiscion
       /// </summary>
       /// <param name="CodEmpresa"></param>
       /// <param name="request"></param>
       /// <returns></returns>
        public ErrorDto InvTranRequisicion_Insertar(int CodEmpresa, TranRequisicionData request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new()
            {
                Code = 0
            };
            string ultimaBoleta = string.Empty;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var queryC = $@"select isnull(max(cod_requisicion),0)+1 as Ultimo from pv_requisiciones";

                    var consecutivo = connection.Query<string>(queryC).FirstOrDefault();
                    consecutivo = consecutivo.PadLeft(10, '0');
                    ultimaBoleta = consecutivo.ToString();

                    var query = "insert pv_requisiciones(cod_requisicion,cod_entsal,genera_fecha,documento," +
                        "notas,genera_user,estado,plantilla,cod_unidad,RECIBE_USER,RESPONSABLE_ACTIVO )" +
                        "values(@CodReq,@Cod_Entsal,getdate(),@Documento,@Notas," +
                        "@Genera_User,'S',@Plantilla,@Cod_Unidad, @Recibe_user, @Responsable_Activo )";

                    var parameters = new DynamicParameters();
                    parameters.Add("CodReq", ultimaBoleta, DbType.String);
                    parameters.Add("Cod_Entsal", request.Cod_Entsal, DbType.String);
                    parameters.Add("Documento", request.Documento, DbType.String);
                    parameters.Add("Notas", request.Notas, DbType.String);
                    parameters.Add("Genera_User", request.Genera_User, DbType.String);
                    parameters.Add("Plantilla", request.Plantilla, DbType.Boolean);
                    parameters.Add("Cod_Unidad", request.cod_unidad, DbType.String);

                    parameters.Add("Recibe_user", request.recibe_user, DbType.String);
                    parameters.Add("Responsable_Activo", request.responsable_activo, DbType.String);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = ultimaBoleta;
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
        /// Actualizar requisicion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto InvTranRequisicion_Actualizar(int CodEmpresa, TranRequisicionData request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = "Update pv_requisiciones SET cod_Entsal = @Cod_Entsal, genera_fecha = getdate(), " +
                        "documento = @Documento, notas = @Notas, plantilla = @Plantilla, cod_unidad =  @Cod_Unidad " +
                        "WHERE cod_requisicion = @CodReq";

                    var parameters = new DynamicParameters();
                    parameters.Add("CodReq", request.Cod_Requisicion, DbType.String);
                    parameters.Add("Cod_Entsal", request.Cod_Entsal, DbType.String);
                    parameters.Add("Documento", request.Documento, DbType.String);
                    parameters.Add("Notas", request.Notas, DbType.String);
                    parameters.Add("Genera_User", request.Genera_User, DbType.String);
                    parameters.Add("Plantilla", request.Plantilla, DbType.Boolean);
                    parameters.Add("Cod_Unidad", request.cod_unidad, DbType.String);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Requisici�n actualizada correctamente";
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
        /// Eliminar requisicion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodRequisicion"></param>
        /// <returns></returns>
        public ErrorDto InvTranRequesicion_Eliminar(int CodEmpresa, int CodRequisicion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new()
            {
                Code = 0
            };
            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    var query1 = $@"delete pv_requi_detalle where cod_requisicion = {CodRequisicion}";
                    connection.Execute(query1);
                    var query2 = $@"delete pv_requisiciones where cod_requisicion = '{CodRequisicion}'";
                    connection.Execute(query2);

                    resp.Description = "Requisición eliminada correctamente";
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
        /// Insertar producto requisición
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodRequisicion"></param>
        /// <param name="producLineas"></param>
        /// <returns></returns>
        public ErrorDto InvRequesicionProduc_Insertar(int CodEmpresa, int CodRequisicion, List<InvReqProduc> producLineas)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto errorDTO = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Obtengo id_control anteriores
                    var query = $@"select id_control from pv_control_activos where cod_requisicion = '{CodRequisicion}'";
                    var idControl = connection.Query<int>(query).ToList();

                    //actualiza Activo Control
                    foreach (var item in idControl)
                    {
                        query = $@"update pv_control_activos set cod_requisicion = null, entrega_usuario = null, id_responsable = null where id_control = {item} ";
                        connection.Execute(query);
                    }


                    query = $@"delete pv_requi_detalle where cod_requisicion = {CodRequisicion}";
                    var resp = connection.Execute(query);

                    if (resp >= 0)
                    {

                        int contador = 0;
                        foreach (InvReqProduc item in producLineas)
                        {
                            contador++;

                            var Cantidad = item.solicitado + item.Despacho;

                            query = $@"insert pv_requi_detalle(linea,cod_requisicion,cod_producto,cantidad,despacho,cod_bodega, costo, solicitado)
                            values( {contador}, '{CodRequisicion}', '{item.Cod_Producto}', {item.Cantidad}, 0, '{item.Cod_Bodega}', {item.Costo},{Cantidad} )";

                            errorDTO.Code = connection.Execute(query);

                            //actualiza Activo Control
                            //consulto encabezado de requesicion
                            var queryEncabezado = $@"select * from pv_requisiciones where cod_requisicion = {CodRequisicion}";
                            var encabezado = connection.Query<TranRequisicionData>(queryEncabezado).FirstOrDefault();

                            query = $@"update pv_control_activos set 
                                            cod_requisicion = '{CodRequisicion}',
                                            entrega_usuario = '{encabezado.recibe_user}',
                                            id_responsable = '{encabezado.responsable_activo}'
                                        where id_control = '{item.id_control}' ";
                            connection.Execute(query);
                        }

                        errorDTO.Description = "Informaci�n guardada satisfactoriamente...";
                    }
                }
            }
            catch (Exception ex)
            {
                errorDTO.Code = -1;
                errorDTO.Description = ex.Message;
            }
            return errorDTO;
        }

        /// <summary>
        /// Obtener transaccion plantilla obtener
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodRequisicion"></param>
        /// <param name="GeneraUser"></param>
        /// <param name="GeneraFecha"></param>
        /// <returns></returns>
        public ErrorDto<List<TranRequisicionData>> InvTranPlantilla_Obtener(int CodEmpresa, int? CodRequisicion, string? GeneraUser, string? GeneraFecha)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<TranRequisicionData>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string filtro = $"where plantilla = 1 ";

                    if (CodRequisicion != 0)
                    {
                        filtro += $"and cod_requisicion = {CodRequisicion} ";
                    }
                    if (!string.IsNullOrEmpty(GeneraUser))
                    {
                        filtro += $"and genera_user like '%{GeneraUser}%' ";
                    }
                    if (!string.IsNullOrEmpty(GeneraFecha))
                    {
                        filtro += $"and genera_fecha between '{GeneraFecha} 00:00:00' and '{GeneraFecha} 23:59:59' ";
                    }

                    var query = $@"select cod_requisicion,genera_user,genera_fecha,documento,notas from pv_requisiciones {filtro}";
                    response.Result = connection.Query<TranRequisicionData>(query).ToList();
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
        /// Obtener lista transaccion requisicion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<TranRequisicionData>> InvTranRequisiciones_Lista(int CodEmpresa, string usuario, string columna, string estado)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<TranRequisicionData>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string query = "SELECT * FROM pv_requisiciones";

                    if (usuario == "T" && columna == "N" && estado == "T")
                    {
                        query = "SELECT * FROM pv_requisiciones ORDER BY COD_REQUISICION ASC";
                    }
                    else
                    {

                        if (!string.IsNullOrEmpty(estado) && estado != "T")
                        {
                            query += " WHERE estado = @estado";
                        }

                        if (usuario != "T" && columna == "G")
                        {

                            if (!string.IsNullOrEmpty(usuario) && usuario != "T" && columna == "G" && estado != "T")
                            {
                                query += " AND genera_user = @usuario";
                            }
                            else
                            {
                                query += " WHERE genera_user = @usuario";
                            }
                        }

                        if(usuario != "T" && columna == "A")
                        {

                        if (!string.IsNullOrEmpty(usuario) && usuario != "T" && columna == "A" && estado != "T")
                        {
                            query += " AND autoriza_user = @usuario";
                        }
                        else
                        {
                            query += " WHERE autoriza_user = @usuario";
                        }
                        }
                    }

                    response.Result = connection.Query<TranRequisicionData>(query, new { usuario, estado }).ToList();
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
        /// Elimina requisicion producto
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodRequisicion"></param>
        /// <param name="Linea"></param>
        /// <returns></returns>
        public ErrorDto InvRequisicionProduc_Eliminar(int CodEmpresa, int CodRequisicion, int Linea)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"delete pv_requi_detalle where COD_REQUISICION = {CodRequisicion} and linea = {Linea}";
                    resp.Code = connection.Execute(query);

                    resp.Description = "Requisición eliminada correctamente";
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
        /// Obtiene UENs
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CatalogosLista>> UENS_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CatalogosLista>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select COD_UNIDAD as item, DESCRIPCION FROM CORE_UENS";
                    response.Result = connection.Query<CatalogosLista>(query).ToList();
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
        /// Obtiene lista usuario que reciben
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_unidad"></param>
        /// <returns></returns>
        public ErrorDto<List<InvRequsUsuarioRecibe>> UsuarioRecibeLista_Obtener(int CodEmpresa, string cod_unidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<InvRequsUsuarioRecibe>>
            {
                Code = 0
            };  
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT R.CORE_USUARIO AS usuario, U.DESCRIPCION AS nombre, '' AS identificacion FROM 
                                      CORE_UENS_USUARIOS_ROLES R LEFT JOIN 
                                      USUARIOS U ON U.NOMBRE = R.CORE_USUARIO
                                      WHERE R.COD_UNIDAD = '{cod_unidad}' ";

                    response.Result = connection.Query<InvRequsUsuarioRecibe>(query).ToList();
                }

                //Busco cedulas en protal
                using (var portalConn = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {

                    foreach (var item in response.Result)
                    {
                        var strSQL = $@"SELECT [USUARIO] AS usuario
                                          ,[NOMBRE] AS nombre
                                          ,[IDENTIFICACION] AS identificacion
                                      FROM [PGX_Portal].[dbo].[US_USUARIOS] 
                                      WHERE USUARIO = '{item.usuario}' ";

                        InvRequsUsuarioRecibe usuario = portalConn.Query<InvRequsUsuarioRecibe>(strSQL).FirstOrDefault();

                        if(usuario != null)
                        {
                            item.identificacion = usuario.identificacion;
                            item.nombre = usuario.nombre;
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
        /// llama identificaciones para requisiciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>

        public ErrorDto<List<InvRequsUsuarioRecibe>> UsuariosActivoLista_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<InvRequsUsuarioRecibe>>
            {
                Code = 0
            };

            var itemsToRemove = new List<InvRequsUsuarioRecibe>();
            try
            {
                //Busco cedulas en protal
                using (var portalConn = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var strSQL = $@"SELECT [USUARIO] AS usuario,[NOMBRE] AS nombre
                                          ,[IDENTIFICACION] AS identificacion
                                      FROM [PGX_Portal].[dbo].[US_USUARIOS]";

                    response.Result = portalConn.Query<InvRequsUsuarioRecibe>(strSQL).ToList();
                   
                }

                using var connection = new SqlConnection(stringConn);
                {
                    foreach (var item in response.Result)
                    {
                        var query = $@"SELECT DISTINCT IDENTIFICACION FROM ACTIVOS_RESPONSABLES
                                         WHERE IDENTIFICACION = '{item.identificacion}' ";

                        var usuario = connection.Query<InvRequsUsuarioRecibe>(query).FirstOrDefault();

                        if (usuario == null)
                        {
                            itemsToRemove.Add(item);
                        }
                    }
                }

                // Eliminar los elementos despu�s del recorrido.
                foreach (var item in itemsToRemove)
                {
                    response.Result.Remove(item);
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
        /// Obtiene productos activo de requisiciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="invReqFiltros"></param>
        /// <returns></returns>
        public ErrorDto<InvRequesicionesActivosLista> ProductosRequesicionesActivo_Obtener(int CodEmpresa, string invReqFiltros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            InvReqFiltros filtros = JsonConvert.DeserializeObject<InvReqFiltros>(invReqFiltros);
            var response = new ErrorDto<InvRequesicionesActivosLista>
            {
                Code = 0
            };

            response.Result = new InvRequesicionesActivosLista();
            response.Result.total = 0;
            string paginaActual = " ", paginacionActual = " ";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var qTotal = $@"SELECT count(A.COD_PRODUCTO)
                                  FROM PV_CONTROL_ACTIVOS A LEFT JOIN PV_PRODUCTOS P ON
                                  P.COD_PRODUCTO = A.COD_PRODUCTO WHERE A.ENTREGA_USUARIO = '' 
                                    AND A.COD_UEN = '{filtros.cod_unidad}' AND REGISTRO_USUARIO = '{filtros.usuario}' ";

                    response.Result.total = connection.Query<int>(qTotal).FirstOrDefault();


                    if (filtros.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                    }

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " AND (P.DESCRIPCION LIKE '%" + filtros.filtro + "%' " +
                            "OR A.COD_PRODUCTO LIKE '%" + filtros.filtro + "%' " +
                            "OR P.CABYS LIKE '%" + filtros.filtro + "%' " +
                            "OR P.COD_BARRAS LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    var query = $@"SELECT 
                                        A.ID_CONTROL,
                                        P.COD_PRODUCTO,
                                        P.DESCRIPCION,
                                        CASE WHEN A.COD_PRODUCTO IS NOT NULL THEN 1 ELSE P.EXISTENCIA END AS CANTIDAD,
                                        CASE WHEN A.COSTO_UNITARIO IS NOT NULL THEN A.COSTO_UNITARIO ELSE P.COSTO_REGULAR END AS COSTO,
                                        A.COSTO_UNITARIO AS COSTO,
                                        A.COD_BODEGA,
                                        P.CABYS,
                                        P.COD_BARRAS,
                                        A.NUMERO_PLACA
                                    FROM 
                                        PV_PRODUCTOS P
                                    LEFT JOIN 
                                        PV_CONTROL_ACTIVOS A ON A.COD_PRODUCTO = P.COD_PRODUCTO 
                                        -- AND A.COD_UEN = '{filtros.cod_unidad}' 
                                        AND A.ENTREGA_USUARIO = '' 
                                        AND A.REGISTRO_USUARIO = '{filtros.usuario}'
                                    WHERE  P.COD_PRODCLAS NOT IN (118)
                                      {filtros.filtro} ORDER BY A.COD_PRODUCTO
                                        {paginaActual}
                                        {paginacionActual} ";
                    
                    response.Result.lista = connection.Query<InvRequesicionesActivosData>(query).ToList();
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
        /// Autoriza requisicion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodRequisicion"></param>
        /// <param name="Usuario"></param>
        /// <param name="Estado"></param>
        /// <returns></returns>
        public ErrorDto InvRequisicion_Autorizar(int CodEmpresa, int CodRequisicion, string Usuario, string Estado)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "update PV_REQUISICIONES set estado = @Estado, Autoriza_user = @Autoriza_User, autoriza_fecha = getdate() " +
                        "where cod_requisicion = @Requisicion";

                    var parameters = new DynamicParameters();
                    parameters.Add("Requisicion", CodRequisicion, DbType.Int32);
                    parameters.Add("Autoriza_User", Usuario, DbType.String);
                    parameters.Add("Estado", Estado, DbType.String);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Requisicion ejecutada correctamente";
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
        /// Procesa requisicion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodRequisicion"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto InvRequisicion_Procesar(int CodEmpresa, int CodRequisicion, string Usuario, string Estado)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //var procedure = "InvRequisicion_Procesar";
                    var procedure = "[spINV_W_Requisicion_Procesa]";
                    var values = new
                    {
                        Requisicion = CodRequisicion,
                        Procesa_Usuario = Usuario,
                        Estado = Estado,
                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Requisici�n procesada correctamente";
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
        /// Valida permisos de requisiciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>Articulos_Obtener
        /// <param name="cod_unidad"></param>
        /// <returns></returns>

        public ErrorDto ValidaAutorizacion(int CodEmpresa, string usuario, string cod_unidad, string cod_proceso)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto info = new ErrorDto { Code = 0 };

            try
            {
                using var connection = new SqlConnection(stringConn);
                connection.Open();

                // 1. Verificar si hay una transacci�n mancomunada
                const string queryMancomunado = @"
                                            SELECT TOP 1 1 
                                            FROM pv_entrada_salida 
                                            WHERE MANCOMUNADO = 1
                                            AND COD_ENTSAL = 'R'";
             

                bool esMancomunado = connection.ExecuteScalar<int?>(queryMancomunado) == 1;

                // 2. Si es mancomunado, verificar si el usuario est� en pv_requisiciones
                if (esMancomunado)
                {
                    const string queryUsuario = @"
                                            SELECT TOP 1 1 
                                            FROM pv_requisiciones 
                                            WHERE GENERA_USER = @usuario";

                    bool usuarioCoincide = connection.ExecuteScalar<int?>(queryUsuario, new { usuario }) == 1;

                    if (usuarioCoincide)
                    {
                        info.Code = 2; // Usuario no autorizado si es mancomunado y si coincide
                        return info;
                    }
                }

                // 3. Verificar si el usuario es l�der en la unidad
                string campoRol = cod_proceso == "P" ? "ROL_ENCARGADO" : "ROL_AUTORIZA";

                string queryAutoriza = $@"
                                        SELECT 1
                                        WHERE EXISTS (
                                            SELECT 1
                                            FROM CORE_UENS_USUARIOS_ROLES r
                                            WHERE r.CORE_USUARIO = @usuario
                                              AND r.{campoRol} = 1
                                              AND r.COD_UNIDAD = @cod_unidad
                                        );";

                bool esAutoriza = connection.ExecuteScalar<int?>(queryAutoriza, new { usuario, cod_unidad }) == 1;

                info.Code = esAutoriza ? 1 : 0;
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }

            return info;
        }


        /// <summary>
        /// Obtiene los usuarios 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<string>> ObtenerUsuario(int CodEmpresa)
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

