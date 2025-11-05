using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmInvBodegasDB
    {
        private readonly IConfiguration _config;

        public frmInvBodegasDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Metodo para obtener los permisos de las bodegas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodBodega"></param>
        /// <returns></returns>
        public ErrorDto<List<PermisosBodegasDTO>> Autorizador_ObtenerTodos(int CodEmpresa, string CodBodega)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<PermisosBodegasDTO>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query =
                    $@"	SELECT 
	                            U.Nombre,
	                            U.DESCRIPCION,
	                            C.COD_BODEGA,
	                            ISNULL(C.E_PROCESA, 0) AS E_Procesa,
                                ISNULL(C.E_MODIFICA, 0) AS E_Modifica,
                                ISNULL(C.E_AUTORIZA, 0) AS E_Autoriza
                            FROM usuarios U 
                            LEFT JOIN PV_BODEGAS_PERMISOS C 
                            ON U.nombre = C.usuario AND C.cod_bodega = {CodBodega}
                            WHERE U.estado = 'A' 
                            ORDER BY U.nombre ASC;
";
                    response.Result = connection.Query<PermisosBodegasDTO>(query).ToList();
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
        /// Metodo para obtener las bodegas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<BodegasDTO>> Bodegas_Obtener(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<BodegasDTO>>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "select * from  PV_BODEGAS";

                    response.Result = connection.Query<BodegasDTO>(query).ToList();
                    foreach (BodegasDTO dt in response.Result)
                    {
                        dt.Cod_Bodega = dt.Cod_Bodega;

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
        /// Metodo para obtener la bodega por scroll button
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="consecutivo"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<BodegasDTO> ConsultaAscDesc(int CodEmpresa, int consecutivo, string tipo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<BodegasDTO>();
            int result = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "";

                    if (tipo == "desc")
                    {
                        if (consecutivo == 0)
                        {
                            query = $@"select Top 1 COD_BODEGA from PV_BODEGAS
                                    order by COD_BODEGA desc";
                        }
                        else
                        {
                            query = $@"select Top 1 COD_BODEGA from PV_BODEGAS
                                    where COD_BODEGA < {consecutivo} order by COD_BODEGA desc";
                        }

                    }
                    else
                    {
                        query = $@"select Top 1 COD_BODEGA from PV_BODEGAS
                                    where COD_BODEGA > {consecutivo} order by COD_BODEGA asc";
                    }


                    result = connection.Query<int>(query).FirstOrDefault();

                    response.Result = connection.Query<BodegasDTO>(query).FirstOrDefault();



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
        /// Metodo para obtener el consecutivo de la bodega
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="consecutivo"></param>
        /// <returns></returns>
        public ErrorDto<BodegasDTO> bodegaConsecutivo_Obtener(int CodEmpresa, string consecutivo)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<BodegasDTO>();
            response.Result = new BodegasDTO();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT * FROM PV_BODEGAS
                                WHERE COD_BODEGA = {consecutivo}";

                    response.Result = connection.Query<BodegasDTO>(query).FirstOrDefault();

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
        /// Metodo para insertar una bodega
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto bodega_Insertar(int CodEmpresa, BodegasDTO data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query1 = $@"SELECT COUNT(*) FROM PV_BODEGAS WHERE cod_bodega = '{data.Cod_Bodega}'";
                    resp.Code = connection.Query<int>(query1).FirstOrDefault();

                    if (resp.Code >= 1)
                    {
                        resp.Code = -1;
                        resp.Description = "Ya existe el numero de Bodega";

                    }
                    else
                    {

                        var query = $@"INSERT INTO pv_bodegas (
                                        cod_bodega,
                                        descripcion,
                                        observacion,
                                        estado,
                                        fecha_inclusion,
                                        permite_entradas,
                                        permite_salidas,
                                        cod_cuenta,
                                        cod_cta_ingresosTF,
                                        cod_cta_gastosTF,
                                        UTILIZA_PERMISOS)  
                                    values({data.Cod_Bodega},'{data.Descripcion}','{data.Observacion}','{data.Estado}','{DateTime.Now}',{data.Permite_Entradas},{data.Permite_Salidas}
                                    ,'{data.Cod_Cuenta}',' {data.Cod_Cta_Ingresostf}','{data.Cod_Cta_Gastostf}',{data.Utiliza_Permisos})";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
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
        /// Metodo para actualizar una bodega
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto bodega_Actualizar(int CodEmpresa, BodegasDTO data)
         {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"UPDATE pv_bodegas SET 

                                observacion = '{data.Observacion}',
                                cod_cuenta  = {data.Cod_Cuenta},
                                 cod_cta_gastoSTF  = {data.Cod_Cta_Gastostf},
                                 cod_cta_ingresostf = {data.Cod_Cta_Ingresostf},
                                 permite_entradas = {data.Permite_Entradas},
                                 permite_salidas = {data.Permite_Salidas},
                                 utiliza_permisos = {data.Utiliza_Permisos},
                                 estado = '{data.Estado}',
                                DESCRIPCION = '{data.Descripcion}'
                                WHERE cod_bodega = {data.Cod_Bodega}";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Ok";


                    //if (resp.Code == 0)
                    //{
                    //    Bitacora(new BitacoraInsertarDTO
                    //    {
                    //        EmpresaId = CodEmpresa,
                    //        Usuario = data.Creacion_User,
                    //        DetalleMovimiento = "CxP Factura: " + data.Cod_Factura + " Prov: " + data.Cod_Proveedor,
                    //        Movimiento = "REGISTRA - WEB",
                    //        Modulo = 30
                    //    });
                    //}

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
        /// Metodo para eliminar una bodega
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_bodega"></param>
        /// <returns></returns>
        public ErrorDto bodega_Eliminar(int CodEmpresa, string cod_bodega)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);

                {
                    var queryPermisos = $@"DELETE FROM PV_BODEGAS_PERMISOS WHERE COD_BODEGA = '{cod_bodega}' ";
                    resp.Code = connection.Query<int>(queryPermisos).FirstOrDefault();
                    resp.Description = "Ok";


                    var query = $@"DELETE FROM PV_BODEGAS WHERE COD_BODEGA = '{cod_bodega}' ";
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
        /// Metodo para actualizar los permisos de las bodegas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="cod_bodega"></param>
        /// <returns></returns>
        public ErrorDto permisosBodega_Actualizar(int CodEmpresa, PermisosBodegasDTO request, string cod_bodega)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();


            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var procedure = "[spINV_W_PermisosBodegas_Actualizar]";

                    var values = new
                    {

                        Modifica = request.E_Modifica ? 1 : 0,
                        Autoriza = request.E_Autoriza ? 1 : 0,
                        Procesa = request.E_Procesa ? 1 : 0,
                        Autorizador = request.Nombre,
                        cod_bodega = cod_bodega
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

    }
}