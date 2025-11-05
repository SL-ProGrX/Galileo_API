using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;
using System.Data;


namespace PgxAPI.DataBaseTier
{
    public class frmInvPaquetesDB
    {
        private readonly IConfiguration _config;
        private mProGrX_AuxiliarDB mAuxiliarDB;

        public frmInvPaquetesDB(IConfiguration config)
        {
            _config = config;
            mAuxiliarDB = new mProGrX_AuxiliarDB(_config);
        }


        #region Paquetes


        /// <summary>
        /// Obtiene la lista lazy de paquetes 
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="pagina"></param>
        /// <param name="paginacion"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDTO<PaqueteDataLista> Paquetes_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<PaqueteDataLista>();
            response.Result = new PaqueteDataLista();
            response.Result.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = "SELECT COUNT(*) FROM pv_paquetes";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " WHERE cod_paquete LIKE '%" + filtro + "%' OR DESCRIPCION LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"SELECT *
                                       FROM pv_paquetes
                                         {filtro} 
                                        ORDER BY cod_paquete
                                        {paginaActual}
                                        {paginacionActual} ";


                    response.Result.Lista = connection.Query<PaqueteDTO>(query).ToList();

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



        public ErrorDTO<List<PaqueteDTO>> Paquetes_ObtenerTodos(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<PaqueteDTO>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "SELECT * FROM pv_paquetes";
                    response.Result = connection.Query<PaqueteDTO>(query).ToList();
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

        public ErrorDTO<PaqueteDTO> Paquete_Obtener(int CodEmpresa, int Cod_Paquete)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<PaqueteDTO>();
            response.Result = new PaqueteDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT * FROM pv_paquetes WHERE cod_paquete = {Cod_Paquete}";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Paquete", Cod_Paquete, DbType.Int32);

                    response.Result = connection.Query<PaqueteDTO>(query, parameters).FirstOrDefault();
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

        public ErrorDTO<List<PaqueteDetalleDTO>> Paquete_ObtenerDetalles(int CodEmpresa, int Cod_Paquete)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<PaqueteDetalleDTO>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "SELECT D.linea, D.cod_producto,P.descripcion,D.cantidad,D.porc_utilidad,D.precio,D.imp_ventas, " +
                        " (D.cantidad * (D.precio + D.precio * D.porc_utilidad / 100)) + ((D.cantidad * (D.precio + D.precio * D.porc_utilidad / 100)) * (D.imp_ventas / 100)) AS Total " +
                        " FROM pv_paquetes_detalle D INNER JOIN pv_productos P ON D.cod_producto = P.cod_producto WHERE D.cod_paquete = @Cod_Paquete ORDER BY D.Linea";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Paquete", Cod_Paquete, DbType.Int32);

                    response.Result = connection.Query<PaqueteDetalleDTO>(query, parameters).ToList();

                    foreach(PaqueteDetalleDTO item in response.Result)
                    {
                        {
                            var queryUnidad = $@"SELECT COD_UNIDAD FROM PV_PRODUCTOS WHERE COD_PRODUCTO = '{item.Cod_Producto}'";

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

        public ErrorDTO Paquete_Actualizar(int CodEmpresa, PaqueteDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = "UPDATE pv_paquetes SET descripcion = @Descripcion,notas = @Notas, user_modifica = @User_Modifica ,fecha_modifica = @Fecha_Modifica" +
                        ",fecha_inicio = @Fecha_Inicio,fecha_corte = @Fecha_Corte,frecuencia_horai = @Frecuencia_Horai,frecuencia_horac = @Frecuencia_Horac" +
                        ",frecuencia_lunes = @Frecuencia_Lunes,frecuencia_martes = @Frecuencia_Martes, frecuencia_miercoles = @Frecuencia_Miercoles" +
                        ", frecuencia_jueves = @Frecuencia_Jueves, frecuencia_viernes = @Frecuencia_Viernes, frecuencia_sabado = @Frecuencia_Sabado" +
                        ",frecuencia_domingo = @Frecuencia_Domingo WHERE cod_paquete = @Cod_Paquete";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Paquete", request.Cod_Paquete, DbType.Int32);
                    parameters.Add("Descripcion", request.Descripcion, DbType.String);
                    parameters.Add("Notas", request.Notas, DbType.String);
                    parameters.Add("User_Modifica", request.User_Modifica, DbType.String);

                    parameters.Add("Fecha_Modifica", DateTime.Now, DbType.DateTime);
                    parameters.Add("Fecha_Inicio", request.Fecha_Inicio.ToLocalTime(), DbType.DateTime);
                    parameters.Add("Fecha_Corte", request.Fecha_Corte.ToLocalTime(), DbType.DateTime);
                    parameters.Add("Frecuencia_Horai", request.Frecuencia_Horai.ToLocalTime(), DbType.DateTime);
                    parameters.Add("Frecuencia_Horac", request.Frecuencia_Horac.ToLocalTime(), DbType.DateTime);

                    parameters.Add("Frecuencia_Lunes", request.Frecuencia_Lunes, DbType.Boolean);
                    parameters.Add("Frecuencia_Martes", request.Frecuencia_Martes, DbType.Boolean);
                    parameters.Add("Frecuencia_Miercoles", request.Frecuencia_Miercoles, DbType.Boolean);
                    parameters.Add("Frecuencia_Jueves", request.Frecuencia_Jueves, DbType.Boolean);
                    parameters.Add("Frecuencia_Viernes", request.Frecuencia_Viernes, DbType.Boolean);
                    parameters.Add("Frecuencia_Sabado", request.Frecuencia_Sabado, DbType.Boolean);
                    parameters.Add("Frecuencia_Domingo", request.Frecuencia_Domingo, DbType.Boolean);


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

        public ErrorDTO Paquete_Insertar2(int CodEmpresa, PaqueteDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string Fecha_Inicio = mAuxiliarDB.validaFechaGlobal(request.Fecha_Inicio);
                    string Fecha_Corte = mAuxiliarDB.validaFechaGlobal(request.Fecha_Corte);  
                    string Frecuencia_Horai = mAuxiliarDB.validaFechaGlobal(request.Frecuencia_Horai);
                    string Frecuencia_Horac = mAuxiliarDB.validaFechaGlobal(request.Frecuencia_Horac);

                    var procedure = "[spINV_W_Paquete_Agregar]";
                    var values = new
                    {
                        Descripcion = request.Descripcion,
                        Notas = request.Notas,
                        User_Crea = request.User_Crea,

                        Fecha_Inicio = Fecha_Inicio,
                        Fecha_Corte = Fecha_Corte,
                        Frecuencia_Horai = Frecuencia_Horai,
                        Frecuencia_Horac = Frecuencia_Horac,


                        Frecuencia_Lunes = request.Frecuencia_Lunes,
                        Frecuencia_Martes = request.Frecuencia_Martes,
                        Frecuencia_Miercoles = request.Frecuencia_Miercoles,
                        Frecuencia_Jueves = request.Frecuencia_Jueves,
                        Frecuencia_Viernes = request.Frecuencia_Viernes,
                        Frecuencia_Sabado = request.Frecuencia_Sabado,
                        Frecuencia_Domingo = request.Frecuencia_Domingo
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

        public ErrorDTO Paquete_Insertar(int CodEmpresa, PaqueteDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new ErrorDTO();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string Fecha_Inicio = mAuxiliarDB.validaFechaGlobal(request.Fecha_Inicio);
                    string Fecha_Corte = mAuxiliarDB.validaFechaGlobal(request.Fecha_Corte);
                    string Frecuencia_Horai = mAuxiliarDB.validaFechaGlobal(request.Frecuencia_Horai);
                    string Frecuencia_Horac = mAuxiliarDB.validaFechaGlobal(request.Frecuencia_Horac);

                    var procedure = "[spINV_W_Paquete_Agregar]";

                    // Define the parameters including the output parameter
                    var parameters = new DynamicParameters();
                    parameters.Add("Descripcion", request.Descripcion);
                    parameters.Add("Notas", request.Notas);
                    parameters.Add("User_Crea", request.User_Crea);
                    parameters.Add("Fecha_Inicio", Fecha_Inicio);
                    parameters.Add("Fecha_Corte", Fecha_Corte);
                    parameters.Add("Frecuencia_Horai", Frecuencia_Horai);
                    parameters.Add("Frecuencia_Horac", Frecuencia_Horac);
                    parameters.Add("Frecuencia_Lunes", request.Frecuencia_Lunes);
                    parameters.Add("Frecuencia_Martes", request.Frecuencia_Martes);
                    parameters.Add("Frecuencia_Miercoles", request.Frecuencia_Miercoles);
                    parameters.Add("Frecuencia_Jueves", request.Frecuencia_Jueves);
                    parameters.Add("Frecuencia_Viernes", request.Frecuencia_Viernes);
                    parameters.Add("Frecuencia_Sabado", request.Frecuencia_Sabado);
                    parameters.Add("Frecuencia_Domingo", request.Frecuencia_Domingo);

                    // Add the output parameter
                    parameters.Add("NewID", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    // Execute the stored procedure
                    connection.Execute(procedure, parameters, commandType: CommandType.StoredProcedure);

                    // Retrieve the output parameter value
                    int newID = parameters.Get<int>("NewID");

                    resp.Code = newID;
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


        public ErrorDTO PaqueteDetalle_Insertar(int CodEmpresa, PaqueteDetalleDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var procedure = "[spINV_W_PaqueteDetalle_Agregar]";
                    var values = new
                    {
                        Cod_Producto = request.Cod_Producto,
                        Cod_Paquete = request.Cod_Paquete,
                        Cantidad = request.Cantidad,

                        Porc_Utilidad = request.Porc_Utilidad,
                        Precio = request.Precio,
                        Imp_Ventas = request.Imp_Ventas,
                        Imp_Consumo = request.Imp_Consumo,

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

        public ErrorDTO PaqueteDetalle_Actualizar(int CodEmpresa, PaqueteDetalleDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var procedure = "[spINV_W_PaqueteDetalle_Actualizar]";
                    var values = new
                    {
                        Cod_Producto = request.Cod_Producto,
                        Cod_Paquete = request.Cod_Paquete,
                        Cantidad = request.Cantidad,
                        Porc_Utilidad = request.Porc_Utilidad,
                        Precio = request.Precio,
                        Imp_Ventas = request.Imp_Ventas,
                        Imp_Consumo = request.Imp_Consumo,

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

        public ErrorDTO PaqueteDetalle_Eliminar(int CodEmpresa, PaqueteDetalleDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "DELETE pv_paquetes_detalle WHERE linea = @Linea";

                    var parameters = new DynamicParameters();
                    parameters.Add("Linea", request.Linea, DbType.Int32);

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


        /*Por si habilitan el borrado por ahora no aplica*/
        public ErrorDTO Paquete_Eliminar(int CodEmpresa, PaqueteDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = "UPDATE pv_paquetes SET descripcion = @Descripcion,notas = @Notas, user_modifica = @User_Modifica ,fecha_modifica = @Fecha_Modifica" +
                        ",fecha_inicio = @Fecha_Inicio,fecha_corte = @Fecha_Corte,frecuencia_horai = @Frecuencia_Horai,frecuencia_horac = @Frecuencia_Horac" +
                        ",frecuencia_lunes = @Frecuencia_Lunes,frecuencia_martes = @Frecuencia_Martes, frecuencia_miercoles = @Frecuencia_Miercoles" +
                        ", frecuencia_jueves = @Frecuencia_Jueves, frecuencia_viernes = @Frecuencia_Viernes, frecuencia_sabado = @Frecuencia_Sabado" +
                        ",frecuencia_domingo = @Frecuencia_Domingo WHERE cod_paquete = @Cod_Paquete";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Paquete", request.Cod_Paquete, DbType.Int32);
                    parameters.Add("Descripcion", request.Descripcion, DbType.String);
                    parameters.Add("Notas", request.Notas, DbType.String);
                    parameters.Add("User_Modifica", request.User_Modifica, DbType.String);

                    parameters.Add("Fecha_Modifica", DateTime.Now, DbType.DateTime);
                    parameters.Add("Fecha_Inicio", request.Fecha_Inicio.ToLocalTime(), DbType.DateTime);
                    parameters.Add("Fecha_Corte", request.Fecha_Corte.ToLocalTime(), DbType.DateTime);
                    parameters.Add("Frecuencia_Horai", request.Frecuencia_Horai.ToLocalTime(), DbType.DateTime);
                    parameters.Add("Frecuencia_Horac", request.Frecuencia_Horac.ToLocalTime(), DbType.DateTime);

                    parameters.Add("Frecuencia_Lunes", request.Frecuencia_Lunes, DbType.Boolean);
                    parameters.Add("Frecuencia_Martes", request.Frecuencia_Martes, DbType.Boolean);
                    parameters.Add("Frecuencia_Miercoles", request.Frecuencia_Miercoles, DbType.Boolean);
                    parameters.Add("Frecuencia_Jueves", request.Frecuencia_Jueves, DbType.Boolean);
                    parameters.Add("Frecuencia_Viernes", request.Frecuencia_Viernes, DbType.Boolean);
                    parameters.Add("Frecuencia_Sabado", request.Frecuencia_Sabado, DbType.Boolean);
                    parameters.Add("Frecuencia_Domingo", request.Frecuencia_Domingo, DbType.Boolean);


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

        public ErrorDTO Paquete_EliminarDetalles(int CodEmpresa, PaqueteDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "DELETE pv_paquetes_detalle WHERE cod_paquete = @Cod_Paquete";
                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Paquete", request.Cod_Paquete, DbType.Int32);

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
