using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.SYS;

namespace PgxAPI.DataBaseTier
{
    public class frmSYS_RA_AutorizadoresDB
    {
        private readonly IConfiguration _config;

        public frmSYS_RA_AutorizadoresDB(IConfiguration config)
        {
            _config = config;
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
                            query = $@"select Top 1 AUTORIZADOR_ID from SYS_EXP_AUTORIZADORES
                                    order by AUTORIZADOR_ID desc";
                        }
                        else
                        {
                            query = $@"select Top 1 AUTORIZADOR_ID from SYS_EXP_AUTORIZADORES
                                    where AUTORIZADOR_ID < {consecutivo} order by AUTORIZADOR_ID desc";
                        }

                    }
                    else
                    {
                        query = $@"select Top 1 AUTORIZADOR_ID from SYS_EXP_AUTORIZADORES
                                    where AUTORIZADOR_ID > {consecutivo} order by AUTORIZADOR_ID asc";
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

        public AutorizadoresExpDto Autorizador_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            AutorizadoresExpDto info = new AutorizadoresExpDto();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT * FROM sif_juzgados WHERE COD_JUZGADO = '{Cod_Proveedor}'";

                    info = connection.Query<AutorizadoresExpDto>(query).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }


        public ErrorDto Autorizador_Insertar(int CodCliente, AutorizadoresExpDto autorizador)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"insert into SYS_EXP_AUTORIZADORES (
                                        autorizador_id,
                                        aut_usuario,
                                        aut_clave,
                                        notas,
                                        estado,
                                    registro_fecha,
                                    registro_usuario) VALUES  ( 
                                        {autorizador.autorizador_id},
                                        '{autorizador.aut_usuario}',
                                        '{autorizador.aut_clave}',
                                        '{autorizador.notas}',
                                        '{autorizador.estado}',
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
                    info.Description = "El c�digo de autorizador ya existe";
                }
                else
                {
                    info.Description = ex.Message;
                }
            }
            return info;
        }

        public ErrorDto Autorizador_Actualizar(int CodEmpresa, AutorizadoresExpDto request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"UPDATE SYS_EXP_AUTORIZADORES SET 
                                autorizador_id = {request.autorizador_id}
                                ,aut_usuario =  '{request.aut_usuario}'
                                 ,aut_clave =  '{request.aut_clave}'
                                ,notas =  '{request.notas}'
                                ,estado = '{request.estado}'
                                WHERE autorizador_id = {request.autorizador_id}";

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