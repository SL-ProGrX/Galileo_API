using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.SIF;

namespace PgxAPI.DataBaseTier
{
    public class frmSIF_JuzgadosDB
    {
        private readonly IConfiguration _config;

        public frmSIF_JuzgadosDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto Juzgado_Insertar(int CodCliente, JuzgadosDTO juzgado)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"INSERT INTO sif_juzgados (
                                                    cod_juzgado,
                                                    nombre,
                                                    telefono_01,
                                                    telefono_02,
                                                    tel_fax,
                                                    email_01,
                                                    email_02,
                                                    apto_postal,
                                                    direccion,
                                                    nombre_contacto,
                                                    sitio_web,
                                                    provincia,
                                                    canton, 
                                                    distrito,
                                                    activo,
                                                    registro_fecha,
                                                    registro_usuario)  
                                                      VALUES ( 
                                        '{juzgado.cod_juzgado}',
                                        '{juzgado.nombre}',
                                        '{juzgado.telefono_01}',
                                        '{juzgado.telefono_02}',
                                         '{juzgado.tel_fax}',
                                        '{juzgado.email_01}',
                                        '{juzgado.email_02}',
                                        '{juzgado.apto_postal}',
                                        '{juzgado.direccion}',
                                        '{juzgado.nombre_contacto}',
                                        '{juzgado.sitio_web}',
                                        {juzgado.provincia},
                                        '{juzgado.canton}',
                                        '{juzgado.distrito}',
                                            1,
                                         Getdate(), 
                                         'PEDRO'
                                         ) ";
                    connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                info.Code = 1;
                if (ex.Message.Contains("Cannot insert duplicate key"))
                {
                    info.Description = "El c�digo de juzgado ya existe";
                }
                else
                {
                    info.Description = ex.Message;
                }
            }
            return info;
        }


        public int ConsultaAscDesc(int CodEmpresa, int consecutivo, string tipo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

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
                            query = $@"select Top 1 COD_JUZGADO from sif_juzgados
                                    order by COD_JUZGADO desc";
                        }
                        else
                        {
                            query = $@"select Top 1 COD_JUZGADO from sif_juzgados
                                    where COD_JUZGADO < {consecutivo} order by COD_JUZGADO desc";
                        }

                    }
                    else
                    {
                        query = $@"select Top 1 COD_JUZGADO from sif_juzgados
                                    where COD_JUZGADO > {consecutivo} order by COD_JUZGADO asc";
                    }


                    result = connection.Query<int>(query).FirstOrDefault();

                    result = result == 0 || result == consecutivo ? consecutivo : result;



                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;
        }


        public JuzgadosDTO Juzgado_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            JuzgadosDTO info = new JuzgadosDTO();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT * FROM sif_juzgados WHERE COD_JUZGADO = '{Cod_Proveedor}'";

                    info = connection.Query<JuzgadosDTO>(query).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }



        public ErrorDto Juzgado_Actualizar(int CodEmpresa, JuzgadosDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"UPDATE sif_juzgados SET 

                                nombre = '{request.nombre}',
                                telefono_01  = '{request.telefono_01}',
                                 telefono_02  = '{request.telefono_02}',
                                 tel_fax = '{request.tel_fax}',
                                 email_01 = '{request.email_01}',
                                 email_02 = '{request.email_02}',
                                 apto_postal = '{request.apto_postal}',
                                 direccion = '{request.direccion}',
                                 nombre_contacto = '{request.nombre_contacto}',
                                 sitio_web = '{request.sitio_web}',
                                provincia = {request.provincia},
                                canton = '{request.canton}',
                                distrito ='{request.distrito}'

                                WHERE cod_juzgado = {request.cod_juzgado}";

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







    }
}