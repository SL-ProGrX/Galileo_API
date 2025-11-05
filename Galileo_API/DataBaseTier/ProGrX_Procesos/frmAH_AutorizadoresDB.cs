using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.AH;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmAH_AutorizadoresDB
    {
        private readonly IConfiguration _config;

        public frmAH_AutorizadoresDB(IConfiguration config)
        {
            _config = config;
        }


        public ErrorDto Autorizador_Insertar(int CodCliente, AutorizadorePatrimonioDTO autorizador)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"insert into PAT_AUTORIZADORES (
                                        usuario,
                                        notas,
                                        estado,
                                    registro_fecha,
                                    registro_usuario) VALUES  ( 
                                        '{autorizador.usuario}',
                                        '{autorizador.notas}',
                                        '{autorizador.estado}',
                                        Getdate(), 
                                         'Pedro'
                                         ) ";
                    connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                info.Code = 1;
                if (ex.Message.Contains("Cannot insert duplicate key"))
                {
                    info.Description = "El c�digo de beneficio ya existe";
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
                            query = $@"select Top 1 ID_USUARIO from PAT_AUTORIZADORES
                                    order by ID_USUARIO desc";
                        }
                        else
                        {
                            query = $@"select Top 1 ID_USUARIO from PAT_AUTORIZADORES
                                    where ID_USUARIO < {consecutivo} order by USUARIO desc";
                        }

                    }
                    else
                    {
                        query = $@"select Top 1 ID_USUARIO from PAT_AUTORIZADORES
                                    where ID_USUARIO > {consecutivo} order by USUARIO asc";
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

        public AutorizadorePatrimonioDTO Autorizador_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            AutorizadorePatrimonioDTO info = new AutorizadorePatrimonioDTO();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT * FROM PAT_AUTORIZADORES WHERE ID_USUARIO = {Cod_Proveedor}";

                    info = connection.Query<AutorizadorePatrimonioDTO>(query).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }


        public ErrorDto Autorizador_Actualizar(int CodEmpresa, AutorizadorePatrimonioDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"UPDATE PAT_AUTORIZADORES SET 
                                usuario = '{request.usuario}'
                                ,notas =  '{request.notas}'
                                ,estado = '{request.estado}'
                                WHERE id_usuario = {request.id_usuario}";

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