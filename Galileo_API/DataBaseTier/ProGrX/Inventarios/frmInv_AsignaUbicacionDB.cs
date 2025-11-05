using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;
using System.Data;


namespace PgxAPI.DataBaseTier
{
    public class frmInv_AsignaUbicacionDB
    {
        private readonly IConfiguration? _config;

        public frmInv_AsignaUbicacionDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene las ubicaciones del producto en inventario.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodAsignaUbicacion"></param>
        /// <returns></returns>
        public ErrorDto<AsignaUbicacionDTO> InvUbicaciones_Obtener(int CodEmpresa, int CodAsignaUbicacion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<AsignaUbicacionDTO>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT COD_ASIGNAUBICACION,ESTADO,COD_BODEGA,RESPONSABLE,FECHA,NOTAS FROM INV_UBICACIONES
                                WHERE COD_ASIGNAUBICACION = {CodAsignaUbicacion}";
                    response.Result = connection.Query<AsignaUbicacionDTO>(query).FirstOrDefault();
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
        /// Obtiene la linea del producto en inventario para su ubicacion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodAsignaUbicacion"></param>
        /// <returns></returns>
        public ErrorDto<List<AsignaUbicacionDetalleDTO>> InvUbicacionProduc_Obtener(int CodEmpresa, int CodAsignaUbicacion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<AsignaUbicacionDetalleDTO>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"select D.COD_ASIGNAUBICACION,D.linea, D.cod_producto AS cod_producto,P.Descripcion,D.Cantidad as Existencia, D.UBICACION
                                    from INV_UBICACIONES_DETALLE D inner join pv_productos P on D.cod_producto = P.cod_producto
                                         where D.COD_ASIGNAUBICACION = {CodAsignaUbicacion}
                                    order by D.Linea";
                    response.Result = connection.Query<AsignaUbicacionDetalleDTO>(query).ToList();
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
        /// Obtener asignacion ubicacion scroll
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="scrollValue"></param>
        /// <param name="CodAsignaUbicacion"></param>
        /// <returns></returns>
        public ErrorDto<AsignaUbicacionDTO> InvUbicacion_scroll(int CodEmpresa, int scrollValue, int? CodAsignaUbicacion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<AsignaUbicacionDTO>
            {
                Code = 0
            };
            try
            {
                string filtro;

                if (scrollValue == 1)
                {
                    filtro = $"where COD_ASIGNAUBICACION > {CodAsignaUbicacion} order by COD_ASIGNAUBICACION asc";
                }
                else
                {
                    filtro = $"where COD_ASIGNAUBICACION < {CodAsignaUbicacion} order by COD_ASIGNAUBICACION desc";
                }

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Top 1 COD_ASIGNAUBICACION from INV_UBICACIONES {filtro}";
                    response.Result = connection.Query<AsignaUbicacionDTO>(query).FirstOrDefault();
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
        /// Inserta una nueva ubicacion de producto en inventario.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto InvAsignaUbicacion_Insertar(int CodEmpresa, AsignaUbicacionDTO request)
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
                    var queryC = $@"select isnull(max(COD_ASIGNAUBICACION),0)+1 as Ultimo from INV_UBICACIONES";

                    var consecutivo = connection.Query<string>(queryC).FirstOrDefault();
                    consecutivo = consecutivo.PadLeft(10, '0');
                    ultimaBoleta = consecutivo.ToString();

                    var query = "INSERT INTO INV_UBICACIONES(" +
                     "cod_asignaubicacion, cod_bodega, notas, estado, responsable, cod_unidad, fecha, genera_user, fecha_user) " +
                     "VALUES(@Cod_AsignaUbicacion, @Cod_Bodega, @Notas, @Estado, @Responsable, @Cod_Unidad, getdate(), @Genera_User, getdate())";

                    var parameters = new DynamicParameters();
                    parameters.Add("COD_ASIGNAUBICACION", ultimaBoleta, DbType.String);
                    parameters.Add("COD_BODEGA", request.cod_bodega, DbType.String);
                    parameters.Add("NOTAS", request.notas, DbType.String);
                    parameters.Add("ESTADO", request.estado, DbType.String);
                    parameters.Add("RESPONSABLE", request.responsable, DbType.String);
                    parameters.Add("COD_UNIDAD", request.cod_unidad, DbType.String);
                    parameters.Add("GENERA_USER", request.genera_user, DbType.String);


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
        /// Actualiza una ubicacion de producto en inventario.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto InvAsignaUbicacion_Actualizar(int CodEmpresa, AsignaUbicacionDTO request)
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

                    var query = "UPDATE INV_UBICACIONES SET cod_bodega = @cod_bodega, fecha_user = GETDATE(), " +
                                "documento = @Documento, notas = @Notas, cod_unidad = @Cod_Unidad, responsable = @responsable " +
                                    "WHERE COD_ASIGNAUBICACION = @CodAsignaUbicacion";

                    var parameters = new DynamicParameters();
                    parameters.Add("CodAsignaUbicacion", request.cod_asignaubicacion, DbType.String);
                    parameters.Add("COD_BODEGA", request.cod_bodega, DbType.String);
                    parameters.Add("DOCUMENTO", request.documento, DbType.String);
                    parameters.Add("NOTAS", request.notas, DbType.String);
                    parameters.Add("GENERA_USER", request.genera_user, DbType.String);
                    parameters.Add("Cod_Unidad", request.cod_unidad, DbType.String);
                    parameters.Add("responsable", request.responsable, DbType.String);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Registro actualizada correctamente";
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
        /// Elimina una ubicacion de producto en inventario.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodAsignaUbicacion"></param>
        /// <returns></returns>
        public ErrorDto InvAsignaUbicacion_Eliminar(int CodEmpresa, int CodAsignaUbicacion)
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
                    var query1 = $@"delete INV_UBICACIONES_DETALLE where COD_ASIGNAUBICACION = {CodAsignaUbicacion}";
                    connection.Execute(query1);
                    var query2 = $@"delete INV_UBICACIONES where COD_ASIGNAUBICACION = '{CodAsignaUbicacion}'";
                    connection.Execute(query2);

                    resp.Description = "Registro eliminado correctamente";
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
        /// Insertar una nueva ubicacion de producto en inventario.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodAsignaUbicacion"></param>
        /// <param name="producLineas"></param>
        /// <returns></returns>
        public ErrorDto InvAsignaUbicacionProduc_Insertar(int CodEmpresa, int CodAsignaUbicacion, List<AsignaUbicacionDetalleDTO> producLineas)
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

                    var query = $@"delete INV_UBICACIONES_DETALLE where COD_ASIGNAUBICACION = {CodAsignaUbicacion}";
                    var resp = connection.Execute(query);

                    if (resp >= 0)
                    {

                        int contador = 0;
                        foreach (AsignaUbicacionDetalleDTO item in producLineas)
                        {
                            contador++;

                            query = $@"insert INV_UBICACIONES_DETALLE(linea,COD_ASIGNAUBICACION,COD_PRODUCTO,CANTIDAD,UBICACION)
                                values( {contador}, '{CodAsignaUbicacion}', '{item.cod_producto}', {item.existencia}, '{item.ubicacion}' )";

                            errorDTO.Code = connection.Execute(query);

                        }

                        errorDTO.Description = "Informacion guardada satisfactoriamente...";
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
        /// Obtiene la lista de de tareas de ubicaciones de inventario.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<AsignaUbicacionDTO>> InvAsignaUbicacion_Lista(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<AsignaUbicacionDTO>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string query = @"
                                SELECT 
                                    U.*, 
                                    B.DESCRIPCION AS descripcion_bodega
                                FROM 
                                    INV_UBICACIONES U
                                LEFT JOIN 
                                    PV_BODEGAS B ON U.COD_BODEGA = B.COD_BODEGA";


                    response.Result = connection.Query<AsignaUbicacionDTO>(query).ToList();
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
        /// Cambia de estados la tarea
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigoAsignaUbicacion"></param>
        /// <param name="Usuario"></param>
        /// <param name="Estado"></param>
        /// <returns></returns>
        public ErrorDto InvAsignacionUbicacion_CerrarOrden_Finalizar(int CodEmpresa, int codigoAsignaUbicacion, string Usuario, string Estado)
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
                    var query = "update INV_UBICACIONES set estado = @Estado, Autoriza_user = @Autoriza_User, autoriza_fecha = getdate() " +
                        "where COD_ASIGNAUBICACION = @COD_ASIGNAUBICACION";

                    var parameters = new DynamicParameters();
                    parameters.Add("COD_ASIGNAUBICACION", codigoAsignaUbicacion, DbType.Int32);
                    parameters.Add("Autoriza_User", Usuario, DbType.String);
                    parameters.Add("Estado", Estado, DbType.String);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Registro actualizado correctamente";
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
        /// Elimina producto de ubicacion 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodAsignaUbicacion"></param>
        /// <param name="Linea"></param>
        /// <returns></returns>
        public ErrorDto InvAsignaUbicacionProduc_Eliminar(int CodEmpresa, int CodAsignaUbicacion, int Linea)
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
                    var query = $@"delete INV_UBICACIONES_DETALLE where COD_ASIGNAUBICACION = {CodAsignaUbicacion} and linea = {Linea}";
                    resp.Code = connection.Execute(query);

                    resp.Description = "Registro eliminado correctamente";
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