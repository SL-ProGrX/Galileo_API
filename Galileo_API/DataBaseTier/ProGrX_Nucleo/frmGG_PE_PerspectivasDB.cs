using PgxAPI.Models.GG_PE;
using Dapper;
using PgxAPI.Models.ERROR;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;

namespace PgxAPI.DataBaseTier
{
    public class frmGG_PE_PerspectivasDB
    {
        private readonly IConfiguration _config;

        public frmGG_PE_PerspectivasDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<PePerspectivasDTO> PePerspectiva_Obtener(int CodEmpresa, int perspectiva)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<PePerspectivasDTO>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select [PERSPECTIVA_ID]
                                          ,[DESCRIPCION]
                                          ,[PE_ID]
                                          ,[OBJETIVO_A_1]
                                          ,[OBJETIVO_A_2]
                                          ,[OBJETIVO_A_3]
                                          ,[RESPONSABLE]
                                          ,[ACTIVA]
                                          ,[REGISTRO_USUARIO]
                                          ,[REGISTRO_FECHA]
                                          ,[MODIFICA_FECHA]
                                          ,[MODIFICA_USUARIO] from PE_PERSPECTIVAS WHERE PERSPECTIVA_ID = '{perspectiva}' ";
                    response.Result = connection.Query<PePerspectivasDTO>(query).FirstOrDefault();
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

        public ErrorDto<PePerspectivasDTO> PePerspectiva_Scroll(int CodEmpresa, int scroll, int? perspectiva)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<PePerspectivasDTO>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string where = " ", orderBy = " ";
                    if (scroll == 1)
                    {
                        where = $@" where PERSPECTIVA_ID > '{perspectiva}' ";
                        orderBy = " order by PERSPECTIVA_ID asc";
                    }
                    else
                    {
                        where = $@" where PERSPECTIVA_ID < '{perspectiva}' ";
                        orderBy = " order by PERSPECTIVA_ID desc";
                    }

                    var query = $@"select top 1 * from PE_PERSPECTIVAS {where} {orderBy}";
                    response.Result = connection.Query<PePerspectivasDTO>(query).FirstOrDefault();
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
    
        public ErrorDto PePerspectiva_Guardar(int CodEmpresa, PePerspectivasDTO perspectiva)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new()
            {
                Code = 0,
                Description = ""
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    if (perspectiva.perspectiva_id == 0)
                    {
                        //Obtengo el siguiente ID
                        var query = $@"SELECT ISNULL(MAX(PERSPECTIVA_ID),0) + 1 FROM PE_PERSPECTIVAS";
                        perspectiva.perspectiva_id = connection.QueryFirstOrDefault<int>(query);

                        resp = Perspectiva_Insertar(CodEmpresa, perspectiva);

                    }
                    else
                    {
                        resp = Perspectiva_Actualizar(CodEmpresa, perspectiva);
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

        private ErrorDto Perspectiva_Insertar(int CodEmpresa, PePerspectivasDTO perspectiva)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new()
            {
                Code = 0,
                Description = ""
            };
            try
            {

                int activa = perspectiva.activa ? 1 : 0;

                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"INSERT INTO [dbo].[PE_PERSPECTIVAS]
                                       ([PERSPECTIVA_ID]
                                       ,[DESCRIPCION]
                                       ,[PE_ID]
                                       ,[OBJETIVO_A_1]
                                       ,[OBJETIVO_A_2]
                                       ,[OBJETIVO_A_3]
                                       ,[RESPONSABLE]
                                       ,[ACTIVA]
                                       ,[REGISTRO_USUARIO]
                                       ,[REGISTRO_FECHA])
                                 VALUES
                                       ({perspectiva.perspectiva_id}
                                       ,'{perspectiva.descripcion}'
                                       ,{perspectiva.pe_id}
                                       ,'{perspectiva.objetivo_a_1}'
                                       ,'{perspectiva.objetivo_a_2}'
                                       ,'{perspectiva.objetivo_a_3}'
                                       ,'{perspectiva.responsable}'
                                       , {activa}
                                       ,'{perspectiva.registro_usuario}'
                                       ,GetDate() )";
                    connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        private ErrorDto Perspectiva_Actualizar(int CodEmpresa, PePerspectivasDTO perspectiva)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new()
            {
                Code = 0,
                Description = ""
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"UPDATE [dbo].[PE_PERSPECTIVAS]
                                   SET [DESCRIPCION] = '{perspectiva.descripcion}'
                                      ,[PE_ID] = {perspectiva.pe_id}
                                      ,[OBJETIVO_A_1] = '{perspectiva.objetivo_a_1}'
                                      ,[OBJETIVO_A_2] = '{perspectiva.objetivo_a_2}'
                                      ,[OBJETIVO_A_3] = '{perspectiva.objetivo_a_3}'
                                      ,[RESPONSABLE] = '{perspectiva.responsable}'
                                      ,[ACTIVA] = {(perspectiva.activa ? 1 : 0)}
                                      ,[MODIFICA_FECHA] = GetDate()
                                      ,[MODIFICA_USUARIO] = '{perspectiva.modifica_usuario}'
                                 WHERE PERSPECTIVA_ID = {perspectiva.perspectiva_id}";
                    connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto PePerspectiva_Eliminar(int CodEmpresa, int perspectiva)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new()
            {
                Code = 0,
                Description = ""
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //Valida si existe en la tabla de objetivos
                    var query = $@"SELECT COUNT(*) FROM [dbo].[PE_OBJETIVOS] WHERE PERSPECTIVA_ID = {perspectiva}";
                    var count = connection.QueryFirstOrDefault<int>(query);
                    if (count > 0)
                    {
                        resp.Code = -1;
                        resp.Description = "No se puede eliminar la perspectiva, ya que tiene objetivos asociados.";
                        return resp;
                    }

                    query = $@"DELETE FROM [dbo].[PE_PERSPECTIVAS] WHERE PERSPECTIVA_ID = {perspectiva}";
                    connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto<List<PePerspectivasDTO>> PePlanesLista_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<PePerspectivasDTO>>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select [PE_ID]
                                      ,[DESCRIPCION] from PE_PLANES Where ESTADO = 'A' 
                                       AND FINALIZACION > getDate() ";
                    response.Result = connection.Query<PePerspectivasDTO>(query).ToList();
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

        public ErrorDto<PePerspectivasDatosLista> PePerpectivasLista_Obtener(int CodEmpresa, string Jfiltros)
        {
            PePerspectivasFiltros filtros = JsonConvert.DeserializeObject<PePerspectivasFiltros>(Jfiltros) ?? new PePerspectivasFiltros();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<PePerspectivasDatosLista>();
            response.Result = new PePerspectivasDatosLista
            {
                total = 0,
                data = new List<PePerspectivasDTO>()
            };

            try
            {
                var query = "";
                string where = " ", paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(stringConn);
                {
                    if (filtros.filtro != null)
                    {
                        where = "where perspectiva_id LIKE '%" + filtros.filtro + "%' OR " +
                            "DESCRIPCION LIKE '%" + filtros.filtro + "%'";
                    }

                    if (filtros.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                    }

                    query = $"select COUNT(*) from PE_PERSPECTIVAS {where}";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();

                    query = $@"select [PERSPECTIVA_ID]
                                      ,[DESCRIPCION]
                                      ,[PE_ID]
                                      ,[OBJETIVO_A_1]
                                      ,[OBJETIVO_A_2]
                                      ,[OBJETIVO_A_3]
                                      ,[RESPONSABLE]
                                      ,[ACTIVA]
                                      ,[REGISTRO_USUARIO]
                                      ,[REGISTRO_FECHA]
                                      ,[MODIFICA_FECHA]
                                      ,[MODIFICA_USUARIO] from PE_PERSPECTIVAS {where} order by PERSPECTIVA_ID desc {paginaActual} {paginacionActual}";
                    response.Result.data = connection.Query<PePerspectivasDTO>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.data = null;
            }
            return response;
        }

    }
}