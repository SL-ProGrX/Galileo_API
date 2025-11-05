using PgxAPI.Models.GG_PE;
using Dapper;
using PgxAPI.Models.ERROR;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;


namespace PgxAPI.DataBaseTier
{
    public class frmGG_PE_PlanesDB
    {
        private readonly IConfiguration _config;

        public frmGG_PE_PlanesDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<PePlanesDatosLista> PePlanesLista_Obtener(int CodEmpresa, string Jfiltros)
        {
            PePlanesFiltros filtros = JsonConvert.DeserializeObject<PePlanesFiltros>(Jfiltros) ?? new PePlanesFiltros();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<PePlanesDatosLista>();
            response.Result = new PePlanesDatosLista
            {
                total = 0,
                data = new List<PePlanesDTO>()
            };

            try
            {
                var query = "";
                string where = " ", paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(stringConn);
                {
                    if (filtros.filtro != null)
                    {
                        where = "where PE_ID LIKE '%" + filtros.filtro + "%' OR " +
                            "DESCRIPCION LIKE '%" + filtros.filtro + "%' OR " +
                            "MISION LIKE '%" + filtros.filtro + "%' OR " +
                            "VISION LIKE '%" + filtros.filtro + "%' OR " +
                            "FINALIZACION LIKE '%" + filtros.filtro + "%' OR " +
                            "INICIO LIKE '%" + filtros.filtro + "%'  ";
                    }

                    if (filtros.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                    }

                    query = $"select COUNT(*) from PE_PLANES {where}";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();

                    query = $@"select [PE_ID]
                                      ,[DESCRIPCION]
                                      ,[INICIO]
                                      ,[FINALIZACION]
                                      ,[ESTADO]
                                      ,[MISION]
                                      ,[VISION]
                                      ,[MODIFICA_FECHA]
                                      ,[MODIFICA_USUARIO]
                                      ,[REGISTRO_USUARIO]
                                      ,[REGISTRO_FECHA] from PE_PLANES {where} order by PE_ID desc {paginaActual} {paginacionActual}";
                    response.Result.data = connection.Query<PePlanesDTO>(query).ToList();
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

        public ErrorDto PePlanes_Guardar(int CodEmpresa, PePlanesDTO plan)
        {
            ErrorDto error = new()
            {
                Code = 0
            };

            try
            {
                if (plan.pe_id == 0)
                {
                    error = PePlanes_Insertar(CodEmpresa, plan);
                }
                else
                {
                    error = PePlanes_Actualizar(CodEmpresa, plan);
                }
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }

            return error;
        }

        private ErrorDto PePlanes_Insertar(int CodEmpresa, PePlanesDTO plan)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto error = new()
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Busco el ultimo siguiente consecutico si es null es el primero de la tabla como 0
                    var queryID = "SELECT ISNULL(MAX(PE_ID),0) + 1 FROM PE_PLANES";
                    var secuencia = connection.Query<int>(queryID).FirstOrDefault();
                    plan.pe_id = secuencia;


                    var insert = $@"INSERT INTO [dbo].[PE_PLANES]
                                               ([PE_ID]
                                               ,[DESCRIPCION]
                                               ,[INICIO]
                                               ,[FINALIZACION]
                                               ,[ESTADO]
                                               ,[MISION]
                                               ,[VISION]
                                               ,[REGISTRO_USUARIO]
                                               ,[REGISTRO_FECHA])
                                         VALUES
                                               ({plan.pe_id}
                                               ,'{plan.descripcion}'
                                               ,'{plan.inicio}'
                                               ,'{plan.finalizacion}'
                                               ,'A'
                                               ,'{plan.mision}'
                                               ,'{plan.vision}'
                                               ,'{plan.registro_usuario}'
                                               ,getDate())";

                    error.Code = connection.Execute(insert);

                    error.Description = plan.pe_id.ToString();

                }
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }

            return error;
        }

        private ErrorDto PePlanes_Actualizar(int CodEmpresa, PePlanesDTO plan)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto error = new()
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var update = $@"UPDATE [dbo].[PE_PLANES]
                                       SET 
                                           [DESCRIPCION] = '{plan.descripcion}'
                                          ,[INICIO] = '{plan.inicio}'
                                          ,[FINALIZACION] = '{plan.finalizacion}'
                                          ,[ESTADO] = '{plan.estado}'
                                          ,[MISION] = '{plan.mision}'
                                          ,[VISION] = '{plan.vision}'
                                          ,[MODIFICA_FECHA] = GetDate()
                                          ,[MODIFICA_USUARIO] = '{plan.modifica_usuario}'
                                     WHERE [PE_ID] = {plan.pe_id} ";

                    error.Code = connection.Execute(update);

                    error.Description = plan.pe_id.ToString();

                }
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }

            return error;
        }

        public ErrorDto PePlanes_Eliminar(int CodEmpresa, int pe_id)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto error = new()
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Busco si el plan tiene registros en la tabla de perpectivas
                    var query = $"SELECT COUNT(*) FROM PE_PERSPECTIVAS WHERE PE_ID = {pe_id}";
                    var respuesta = connection.Query<int>(query).FirstOrDefault();

                    if (respuesta > 0)
                    {
                        error.Code = -1;
                        error.Description = "No se puede eliminar el plan, tiene perspectivas asociadas";
                    }
                    else
                    {
                        var delete = $@"DELETE FROM [dbo].[PE_PLANES] WHERE [PE_ID] = {pe_id}";
                        error.Code = connection.Execute(delete);
                    }
                }
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }
            return error;
        }

        public ErrorDto<List<PePlanesDTO>> PePlanes_Exportar(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<PePlanesDTO>>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select [PE_ID]
                                      ,[DESCRIPCION]
                                      ,[INICIO]
                                      ,[FINALIZACION]
                                      ,[ESTADO]
                                      ,[MISION]
                                      ,[VISION]
                                      ,[MODIFICA_FECHA]
                                      ,[MODIFICA_USUARIO]
                                      ,[REGISTRO_USUARIO]
                                      ,[REGISTRO_FECHA] from PE_PLANES order by PE_ID desc ";
                    response.Result = connection.Query<PePlanesDTO>(query).ToList();
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