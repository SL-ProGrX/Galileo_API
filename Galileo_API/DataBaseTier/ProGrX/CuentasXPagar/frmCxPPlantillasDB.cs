using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.CxP;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmCxPPlantillasDB
    {

        private readonly IConfiguration _config;

        public frmCxPPlantillasDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDTO<List<PlantillaDTO>> Plantillas_Obtener(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDTO<List<PlantillaDTO>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT * FROM CxP_Plantillas";

                    response.Result = connection.Query<PlantillaDTO>(query).ToList();

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

        public ErrorDTO<List<Unidad>> Unidades_Obtener(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDTO<List<Unidad>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT cod_unidad,descripcion FROM CntX_unidades WHERE Activa = 1 and cod_contabilidad = 1";

                    response.Result = connection.Query<Unidad>(query).ToList();

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

        public ErrorDTO<List<Centro_Costo>> CentrosCosto_Obtener(int CodEmpresa, string Cod_Unidad)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDTO<List<Centro_Costo>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT C.COD_CENTRO_COSTO,C.descripcion
                                FROM CNTX_CENTRO_COSTOS C 
                                INNER JOIN CNTX_UNIDADES_CC A ON C.COD_CENTRO_COSTO = A.COD_CENTRO_COSTO
                                AND C.cod_contabilidad = A.cod_Contabilidad
                                AND A.cod_unidad = '{Cod_Unidad}'
                                AND C.cod_contabilidad = 1";

                    response.Result = connection.Query<Centro_Costo>(query).ToList();

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


        public ErrorDTO<PlantillaDTO> PlantillaDetalle_Obtener(int CodEmpresa, string Cod_Plantilla)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDTO<PlantillaDTO>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT * FROM CxP_Plantillas WHERE cod_plantilla = '{Cod_Plantilla}'";

                    response.Result = connection.Query<PlantillaDTO>(query).FirstOrDefault();

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

        public ErrorDTO<PlantillaDTO> PlantillaDetalle_Scroll(int CodEmpresa, int scroll, string Cod_Plantilla)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDTO<PlantillaDTO>
            {
                Code = 0
            };

            try
            {
                string where = " ", orderBy = " ";
                if (scroll == 1)
                {
                    where = $@" where cod_plantilla > '{Cod_Plantilla}' ";
                    orderBy = " order by cod_plantilla asc";
                }
                else
                {
                    where = $@" where cod_plantilla < '{Cod_Plantilla}' ";
                    orderBy = " order by cod_plantilla desc";
                }
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT top 1 * FROM CxP_Plantillas {where} {orderBy}";

                    response.Result = connection.Query<PlantillaDTO>(query).FirstOrDefault();

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

        public ErrorDTO<List<Plantilla_AsientoDTO>> PlantillaAsientos_Obtener(int CodEmpresa, string Cod_Plantilla)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDTO<List<Plantilla_AsientoDTO>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT A.linea, A.cod_cuenta,B.descripcion AS Desc_Cuenta, B.cod_cuenta_mask, A.cod_unidad,A.cod_Centro_Costo,A.porcentaje, A.cod_plantilla, A.cod_divisa
                                FROM CxP_Plantillas_Asiento A INNER JOIN CntX_cuentas B ON A.cod_cuenta = B.cod_cuenta
                                AND A.cod_contabilidad = B.cod_contabilidad
                                WHERE B.cod_contabilidad = 1
                                AND A.cod_plantilla = '{Cod_Plantilla}'
                                ORDER BY Linea";

                    response.Result = connection.Query<Plantilla_AsientoDTO>(query).ToList();

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


        public ErrorDTO Plantilla_Actualizar(int CodEmpresa, PlantillaDTO data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new()
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"UPDATE CxP_Plantillas SET 
                                descripcion = '{data.Descripcion}'
                                ,notas =  '{data.Notas}'
                                ,activo = '{Convert.ToInt32(data.Activo)}'
                                WHERE cod_plantilla = {data.Cod_Plantilla}";

                    var query2 = $@"DELETE CxP_Plantillas_Asiento WHERE cod_plantilla = {data.Cod_Plantilla}";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Plantilla actualizar correctamente";

                    if (resp.Code == 0)
                    {
                        resp.Code = connection.Query<int>(query2).FirstOrDefault();
                        resp.Description = "Plantilla actualizar correctamente";
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

        public ErrorDTO Plantilla_Insertar(int CodEmpresa, PlantillaDTO data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"INSERT INTO CxP_Plantillas(cod_plantilla, descripcion, notas
                                ,Registro_Usuario, registro_fecha, activo) 
                                values('{data.Cod_Plantilla}','{data.Descripcion}','{data.Notas}'
                                ,'{data.Registro_Usuario}','{DateTime.Now}',{Convert.ToInt32(data.Activo)})";

                    var query2 = $@"DELETE CxP_Plantillas_Asiento WHERE cod_plantilla = {data.Cod_Plantilla}";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Plantilla agregada correctamente";

                    if (resp.Code == 0)
                    {
                        resp.Code = connection.Query<int>(query2).FirstOrDefault();
                        resp.Description = "Plantilla agregada correctamente";
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

        public ErrorDTO Plantilla_Borrar(int CodEmpresa, string Cod_Plantilla)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"DELETE CxP_Plantillas_Asiento WHERE cod_plantilla = {Cod_Plantilla}";

                    var query2 = $@"DELETE CxP_Plantillas WHERE cod_plantilla = {Cod_Plantilla}";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Plantilla eliminada correctamente";

                    if (resp.Code == 0)
                    {
                        resp.Code = connection.Query<int>(query2).FirstOrDefault();
                        resp.Description = "Plantilla eliminada correctamente";
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


        public ErrorDTO PlantillaAsiento_Insertar(int CodEmpresa, Plantilla_AsientoDTO data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"INSERT INTO CxP_Plantillas_Asiento(Linea,cod_plantilla,cod_cuenta,cod_contabilidad,cod_divisa,cod_unidad,cod_centro_costo,porcentaje) 
                                values({data.Linea},'{data.Cod_Plantilla}','{data.Cod_Cuenta}',{data.Cod_Contabilidad}
                                ,'{data.Cod_Divisa}','{data.Cod_Unidad}','{data.Cod_Centro_Costo}',{data.Porcentaje})";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Plantilla asiento agregada correctamente";

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDTO PlantillaAsiento_Actualizar(int CodEmpresa, Plantilla_AsientoDTO data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"UPDATE CxP_Plantillas_Asiento SET 
                                cod_cuenta = '{data.Cod_Cuenta}'
                                ,cod_divisa =  '{data.Cod_Divisa}'
                                ,cod_unidad = '{data.Cod_Unidad}'
                                ,cod_centro_costo =  '{data.Cod_Centro_Costo}'
                                ,porcentaje =  {data.Porcentaje}
                                WHERE linea = {data.Linea} AND cod_plantilla = '{data.Cod_Plantilla}'";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Plantilla asiento actualizada correctamente";

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDTO PlantillaAsiento_Borrar(int CodEmpresa, Plantilla_AsientoDTO data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"DELETE CxP_Plantillas_Asiento WHERE cod_plantilla = {data.Cod_Plantilla} AND linea = {data.Linea}";


                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Plantilla asiento eliminada correctamente";

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
