using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.SIF;

namespace PgxAPI.DataBaseTier
{
    public class frmSIF_ComunicadosDB
    {
        private readonly IConfiguration _config;

        public frmSIF_ComunicadosDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Inserta un comunicado en la base de datos del cliente
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="comunicado"></param>
        /// <returns></returns>
        public ErrorDTO Comunicados_Insertar(int CodCliente, SifComunicadoDTO comunicado)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"insert into sif_comunicados (
                                    fecha,
                                    inicio,
                                    corte,
                                    usuario,
                                    nota,
                                    ffuente,
                                    fcolor,
                                    fcursiva,
                                    fnegrita) VALUES  ( 
                                        '{DateTime.Now}',
                                        '{comunicado.inicio}',
                                        '{comunicado.corte}',
                                        '{comunicado.usuario}',
                                        '{comunicado.nota}',
                                         '{comunicado.ffuente}',
                                         '{comunicado.fcolor}',
                                         '{comunicado.fcursiva}',
                                         '{comunicado.fnegrita}')";

                    connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                if (ex.Message.Contains("Cannot insert duplicate key"))
                {
                    info.Description = "El código de comunicado ya existe";
                }
                else
                {
                    info.Description = ex.Message;
                }
            }
            return info;
        }


        /// <summary>
        /// Consulta el consecutivo del comunicado anterior o siguiente
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="consecutivo"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDTO<int> ConsultaAscDesc(int CodEmpresa, int consecutivo, string tipo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO<int> response = new ErrorDTO<int>();
            response.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "";

                    if (tipo == "desc")
                    {
                        if (consecutivo == 0)
                        {
                            query = $@"select Top 1 COD_COMUNICADO from sif_comunicados
                                    order by COD_COMUNICADO desc";
                        }
                        else
                        {
                            query = $@"select Top 1 COD_COMUNICADO from sif_comunicados
                                    where COD_COMUNICADO < {consecutivo} order by COD_COMUNICADO desc";
                        }

                    }
                    else
                    {
                        query = $@"select Top 1 COD_COMUNICADO from sif_comunicados
                                    where COD_COMUNICADO > {consecutivo} order by COD_COMUNICADO asc";
                    }

                    response.Result = connection.Query<int>(query).FirstOrDefault();

                    if (response.Result == 0)
                    {
                        response.Code = -2;
                        response.Description = "No se encontraron más resultados.";
                    }

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = 0;
            }
            return response;
        }


        /// <summary>
        /// Obtiene un comunicado por su código
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Cod_Comunicado"></param>
        /// <returns></returns>
        public ErrorDTO<SifComunicadoDTO> Comunicado_Obtener(int CodEmpresa, int Cod_Comunicado)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO<SifComunicadoDTO> response = new ErrorDTO<SifComunicadoDTO>();
            response.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT * FROM sif_comunicados WHERE COD_COMUNICADO = {Cod_Comunicado}";

                    response.Result = connection.Query<SifComunicadoDTO>(query).FirstOrDefault();
                    if (response.Result == null)
                    {
                        response.Code = -2;
                        response.Description = "No se encontraron resultados.";
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
        /// Obtiene lista de comunicados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<SifComunicadoDTO>> ComunicadosLista_Obtener(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO<List<SifComunicadoDTO>> response = new ErrorDTO<List<SifComunicadoDTO>>();
            response.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT * FROM sif_comunicados";

                    response.Result = connection.Query<SifComunicadoDTO>(query).ToList();
                    if (response.Result == null || !response.Result.Any())
                    {
                        response.Code = -2;
                        response.Description = "No se encontraron resultados.";
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
        /// Actualiza un comunicado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDTO Comunicado_Actualizar(int CodEmpresa, SifComunicadoDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"UPDATE sif_comunicados SET 
                                INICIO = '{request.inicio}'
                                ,CORTE =  '{request.corte}'
                                ,NOTA = '{request.nota}'
                                ,FFUENTE =  '{request.ffuente}'
                                ,FCOLOR =  '{request.fcolor}'
                                ,FCURSIVA =  '{request.fcursiva}'
                                ,FNEGRITA =  '{request.fnegrita}'

                                WHERE COD_COMUNICADO = {request.cod_comunicado}";

                    info.Code = connection.Query<int>(query).FirstOrDefault();
                    info.Description = "Ok";

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

    }
}