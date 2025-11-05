using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.PRE;

namespace PgxAPI.DataBaseTier
{
    public class frmPres_ParametrosDB
    {
        private readonly IConfiguration _config;

        public frmPres_ParametrosDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Metodo para insertar o actualizar parametros
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDTO PresParametros_Guardar(int CodEmpresa, PresParametrosDTO parametros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                   //valido si existe un parametro
                   var query = $@"select * from Pres_Parametros where COD_PARAMETRO = '{parametros.cod_parametro}'";
                    var existe = connection.Query(query).FirstOrDefault();

                    if (existe == null)
                    {
                        query = $@"INSERT INTO [dbo].[PRES_PARAMETROS]
                                           ([COD_PARAMETRO]
                                           ,[DESCRIPCION]
                                           ,[NOTAS]
                                           ,[VALOR]
                                           ,[REGISTRO_USUARIO]
                                           ,[REGISTRO_FECHA])
                                     VALUES
                                           ('{parametros.cod_parametro}'
                                           ,'{parametros.descripcion}'
                                           ,'{parametros.notas}'
                                           ,'{parametros.valor}'
                                           ,'{parametros.registro_usuario}'
                                           ,GetDate() )";
                        
                    }
                    else
                    {
                        query = $@"UPDATE [dbo].[PRES_PARAMETROS]
                                       SET [DESCRIPCION] = '{parametros.descripcion}'
                                          ,[NOTAS] = '{parametros.notas}'
                                          ,[VALOR] = '{parametros.valor}'
                                          ,[MODIFICA_USUARIO] = '{parametros.modifica_usuario}'
                                          ,[MODIFICA_FECHA] = GetDate()
                                     WHERE COD_PARAMETRO = '{parametros.cod_parametro}'";
                    }

                    response.Code = connection.Execute(query);
                }
            }catch(Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Metodo para obtener la lista de parametros
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<PresParametrosDTO>> PresParametrosLista_Obtener(int CodEmpresa)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<PresParametrosDTO>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select * from Pres_Parametros";
                    response.Result = connection.Query<PresParametrosDTO>(query).ToList();
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