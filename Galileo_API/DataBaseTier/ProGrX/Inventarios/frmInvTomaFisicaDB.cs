using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;

namespace PgxAPI.DataBaseTier
{
    public class frmInvTomaFisicaDB
    {
        private readonly IConfiguration _config;

        public frmInvTomaFisicaDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Toma fisica obtener
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_Proveedor"></param>
        /// <param name="pagina"></param>
        /// <param name="paginacion"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDTO<List<Toma_FisicaDTO>> TomaFisica_Obtener(int CodEmpresa, int Cod_Proveedor, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<Toma_FisicaDTO>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "Select * From pv_InvTomaFisica";

                    response.Result = connection.Query<Toma_FisicaDTO>(query).ToList();
                    foreach (Toma_FisicaDTO dt in response.Result)
                    {
                        dt.consecutivo = dt.consecutivo;

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
        /// Toma fisica detalle obtener
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="consecutivo"></param>
        /// <param name="pagina"></param>
        /// <param name="paginacion"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDTO<List<Toma_FisicaDetalleDTO>> tomasFisicasDetalle_Obtener(int CodEmpresa, int consecutivo, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDTO<List<Toma_FisicaDetalleDTO>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"SELECT 
                                    d.consecutivo,
                                    d.Cod_Bodega,
                                    d.Cod_Producto,
                                    d.Existencia_Logica,
                                    d.Existencia_Fisica,
                                    d.Ubicacion,  
                                    p.Descripcion,
                                    p.tipo_producto as tipo,
                                    b.descripcion as bodega 
                                FROM 
                                    pv_invTF_Detalle d
                                INNER JOIN 
                                    PV_PRODUCTOS p ON d.COD_PRODUCTO = p.COD_PRODUCTO
                                INNER JOIN 
                                    PV_BODEGAS b ON d.COD_BODEGA = b.COD_BODEGA 
                                where consecutivo = '{consecutivo}'";

                    response.Result = connection.Query<Toma_FisicaDetalleDTO>(query).ToList();
                    foreach (Toma_FisicaDetalleDTO dt in response.Result)
                    {
                        dt.consecutivo = dt.consecutivo;

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
        /// Toma Fisica Insertar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDTO tomaFisica_Insertar(int CodEmpresa, Toma_FisicaDTO data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var queryC = @"SELECT ISNULL(MAX(consecutivo), 0) + 1 AS Ultimo FROM PV_INVTOMAFISICA";

                    // Obtener el consecutivo como int directamente
                    int ultimaBoleta = connection.Query<int>(queryC).FirstOrDefault();


                    var query1 = $@"SELECT COUNT(*) FROM pv_invTF_Detalle WHERE consecutivo = '{data.consecutivo}'";
                    resp.Code = connection.Query<int>(query1).FirstOrDefault();

                    if (resp.Code >= 1)
                    {
                        resp.Code = -1;
                        resp.Description = "Ya existe el No. Boleta, por favor verifique";

                    }
                    else
                    {

                        var query = $@"INSERT PV_INVTOMAFISICA(CONSECUTIVO,COD_BODEGA,FECHA_INICIO,FECHA_CORTE,ESTADO
                                ,FECHA_CREA,USER_CREA,NOTAS)
                                values({ultimaBoleta},'{data.Cod_Bodega}','{data.Fecha_Inicio}','{data.Fecha_Corte}'
                                ,'S','{DateTime.Now}','{data.User_Crea}','{data.notas}')";

                        resp.Code = connection.Query<int>(query).FirstOrDefault();
                        resp.Description = ultimaBoleta.ToString();
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
        /// Toma Fisica Insertar Detalle
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDTO tomaFisicaDetalle_Insertar(int CodEmpresa, Toma_FisicaDetalleDTO data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"INSERT INTO pv_invTF_Detalle(CONSECUTIVO,COD_BODEGA,COD_PRODUCTO,EXISTENCIA_LOGICA,EXISTENCIA_FISICA,UBICACION) 
                                values({data.consecutivo},'{data.Cod_Bodega}','{data.Cod_Producto}',{data.Existencia_Logica}
                                ,{data.Existencia_Fisica},'{data.Ubicacion}')";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
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
        /// Consulta AsDesc Toma Fisica
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="consecutivo"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDTO<Toma_FisicaDTO> ConsultaAscDesc(int CodEmpresa, int consecutivo, string tipo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            int result = 0;

            var response = new ErrorDTO<Toma_FisicaDTO>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "";

                    if (tipo == "desc")
                    {
                        if (consecutivo == 0)
                        {
                            query = $@"select Top 1 consecutivo from PV_INVTOMAFISICA
                                    order by consecutivo desc";
                        }
                        else
                        {
                            query = $@"select Top 1 consecutivo from PV_INVTOMAFISICA
                                    where consecutivo < {consecutivo} order by consecutivo desc";
                        }

                    }
                    else
                    {
                        query = $@"select Top 1 consecutivo from PV_INVTOMAFISICA
                                    where consecutivo > {consecutivo} order by consecutivo asc";
                    }


                    result = connection.Query<int>(query).FirstOrDefault();
                    response.Result = connection.Query<Toma_FisicaDTO>(query).FirstOrDefault();

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
        /// Obtener toma fisica consecutivo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="consecutivo"></param>
        /// <returns></returns>
        public ErrorDTO<Toma_FisicaDTO> tomaFisicaConsecutivo_Obtener(int CodEmpresa, int consecutivo)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<Toma_FisicaDTO>();
            response.Result = new Toma_FisicaDTO();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT * FROM PV_INVTOMAFISICA
                                WHERE CONSECUTIVO = {consecutivo}";

                    response.Result = connection.Query<Toma_FisicaDTO>(query).FirstOrDefault();

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
        /// Actualizar Toma Fisica
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDTO actualizarTomaFisica(int CodEmpresa, Toma_FisicaDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"UPDATE PV_INVTOMAFISICA SET 
                                COD_BODEGA = '{request.Cod_Bodega}'
                                ,FECHA_CREA = '{request.Fecha_Inicio}'
                                ,FECHA_APLICA = '{request.Fecha_Corte}'
                                ,NOTAS =  '{request.notas}'
                                WHERE consecutivo = {request.consecutivo}";



                    resp.Code = connection.Query<int>(query).FirstOrDefault();
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
        /// Actualizar Toma Fisica Detalle
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDTO actualizarTomaFisicaDetalle(int CodEmpresa, Toma_FisicaDetalleDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query1 = $@"SELECT COUNT(*) FROM pv_invTF_Detalle WHERE cod_producto = '{request.Cod_Producto}' and CONSECUTIVO = {request.consecutivo}";
                    resp.Code = connection.Query<int>(query1).FirstOrDefault();

                    if (resp.Code >= 1)
                    {
                        var query = $@"UPDATE pv_invTF_Detalle SET 
                                COD_BODEGA = '{request.Cod_Bodega}'
                                ,EXISTENCIA_LOGICA =  {request.Existencia_Logica}
                                ,EXISTENCIA_FISICA = {request.Existencia_Fisica}
                                ,UBICACION = {request.Ubicacion}
                                WHERE CONSECUTIVO = {request.consecutivo} and COD_PRODUCTO = '{request.Cod_Producto}'";

                        resp.Code = connection.Query<int>(query).FirstOrDefault();
                        resp.Description = "Ok";
                    }
                    else
                    {

                        var query2 = $@"INSERT INTO pv_invTF_Detalle(CONSECUTIVO,COD_BODEGA,COD_PRODUCTO,EXISTENCIA_LOGICA,EXISTENCIA_FISICA,UBICACION) 
                                values({request.consecutivo},'{request.Cod_Bodega}','{request.Cod_Producto}',{request.Existencia_Logica}
                                ,{request.Existencia_Fisica},'{request.Ubicacion}')";

                        resp.Code = connection.Query<int>(query2).FirstOrDefault();
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
        /// Eliminar detalle Toma Fisica
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="consecutivo"></param>
        /// <param name="cod_producto"></param>
        /// <returns></returns>
        public ErrorDTO EliminarDetalleTomaFisica(int CodEmpresa, int consecutivo, string cod_producto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"DELETE FROM pv_invTF_Detalle WHERE CONSECUTIVO = {consecutivo} and cod_producto = '{cod_producto}' ";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
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
        /// Eliminar Toma Fisica
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="consecutivo"></param>
        /// <returns></returns>
        public ErrorDTO EliminarTomaFisica(int CodEmpresa, int consecutivo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"DELETE FROM pv_invTF_Detalle WHERE CONSECUTIVO = {consecutivo}";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Ok";

                    var query1 = $@"DELETE FROM PV_INVTOMAFISICA WHERE CONSECUTIVO = {consecutivo}";

                    resp.Code = connection.Query<int>(query1).FirstOrDefault();
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

        public ErrorDTO<Toma_FisicaDetalleDTO> TomaFisicaProdBarras_Obtener(
            int CodEmpresa, string cod_bodega, string cod_barras, string tipo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var resp = new ErrorDTO<Toma_FisicaDetalleDTO>();
            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {

                    if(tipo == "CB")
                    {
                        query = $@"SELECT 
                                    pp.COD_PRODUCTO,
                                    pp.DESCRIPCION,
                                    pp.TIPO_PRODUCTO as TIPO,
                                    SUM(CASE WHEN im.TIPO = 'E' THEN im.CANTIDAD ELSE 0 END) - 
                                      SUM(CASE WHEN im.TIPO = 'S' THEN im.CANTIDAD ELSE 0 END) AS existencia_Logica
                                    FROM PV_PRODUCTOS pp LEFT JOIN PV_INVENTARIO_MOV im ON 
                                    pp.COD_PRODUCTO = im.COD_PRODUCTO AND im.COD_BODEGA = '{cod_bodega}'
                                    WHERE pp.COD_BARRAS = '{cod_barras}'
                                    GROUP BY pp.COD_PRODUCTO,
                                    pp.DESCRIPCION,
                                    pp.TIPO_PRODUCTO";
                    }
                    else
                    {
                        query = $@"SELECT 
                                    pp.COD_PRODUCTO,
                                    pp.DESCRIPCION,
                                    pp.TIPO_PRODUCTO as TIPO,
                                    SUM(CASE WHEN im.TIPO = 'E' THEN im.CANTIDAD ELSE 0 END) - 
                                      SUM(CASE WHEN im.TIPO = 'S' THEN im.CANTIDAD ELSE 0 END) AS existencia_Logica
                                    FROM PV_PRODUCTOS pp LEFT JOIN PV_INVENTARIO_MOV im ON 
                                    pp.COD_PRODUCTO = im.COD_PRODUCTO AND im.COD_BODEGA = '{cod_bodega}'
                                    WHERE pp.COD_PRODUCTO = '{cod_barras}'
                                    GROUP BY pp.COD_PRODUCTO,
                                    pp.DESCRIPCION,
                                    pp.TIPO_PRODUCTO";
                    }
                    
                    resp.Result = connection.Query<Toma_FisicaDetalleDTO>(query).FirstOrDefault();

                    if(resp.Result == null)
                    {
                        resp.Code = -1;
                        resp.Description = "No existe Producto con este codigo";
                        resp.Result = new Toma_FisicaDetalleDTO();
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


        public ErrorDTO TomaFisicaBarras_Guardar(int CodEmpresa, Toma_FisicaDetalleDTO linea )
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query1 = $@"SELECT COUNT(*) FROM pv_invTF_Detalle WHERE cod_producto = '{linea.Cod_Producto}' 
                                  and CONSECUTIVO = {linea.consecutivo} and UBICACION = '{linea.Ubicacion}'";
                    var existe = connection.Query<int>(query1).FirstOrDefault();

                    if(existe == 0)
                    {
                        var query = $@"INSERT INTO pv_invTF_Detalle(CONSECUTIVO,COD_BODEGA,COD_PRODUCTO,EXISTENCIA_LOGICA,EXISTENCIA_FISICA,UBICACION) 
                                values({linea.consecutivo},'{linea.Cod_Bodega}','{linea.Cod_Producto}',{linea.Existencia_Logica}
                                ,{linea.Existencia_Fisica},'{linea.Ubicacion}')";

                        resp.Code = connection.Execute(query);
                    }
                    else
                    {
                        resp.Code = -1;
                        resp.Description = "Producto ya se encuentra en la boleta registrado";
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

    }

}