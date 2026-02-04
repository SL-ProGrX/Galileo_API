using Galileo.Models.GG_PE;
using Dapper;
using Galileo.Models.ERROR;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class FrmGgPePerspectivasDB
    {
        private readonly IConfiguration _config;

        public FrmGgPePerspectivasDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<PePerspectivasDto> PePerspectiva_Obtener(int CodEmpresa, int perspectiva)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<PePerspectivasDto>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                const string query = @"select [PERSPECTIVA_ID]
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
                                          ,[MODIFICA_USUARIO]
                                   from PE_PERSPECTIVAS
                                   WHERE PERSPECTIVA_ID = @perspectiva;";
                response.Result = connection.Query<PePerspectivasDto>(query, new { perspectiva }).FirstOrDefault();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        public ErrorDto<PePerspectivasDto> PePerspectiva_Scroll(int CodEmpresa, int scroll, int? perspectiva)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<PePerspectivasDto>();
            try
            {
                using var connection = new SqlConnection(stringConn);

                const string qNext = @"select top 1 *
from PE_PERSPECTIVAS
where PERSPECTIVA_ID > @perspectiva
order by PERSPECTIVA_ID asc;";

                const string qPrev = @"select top 1 *
from PE_PERSPECTIVAS
where PERSPECTIVA_ID < @perspectiva
order by PERSPECTIVA_ID desc;";

                var param = new { perspectiva = perspectiva ?? 0 };
                var query = (scroll == 1) ? qNext : qPrev;
                response.Result = connection.Query<PePerspectivasDto>(query, param).FirstOrDefault();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        public ErrorDto PePerspectiva_Guardar(int CodEmpresa, PePerspectivasDto perspectiva)
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
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        private ErrorDto Perspectiva_Insertar(int CodEmpresa, PePerspectivasDto perspectiva)
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
                const string query = @"INSERT INTO [dbo].[PE_PERSPECTIVAS]
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
                                       (@perspectiva_id
                                       ,@descripcion
                                       ,@pe_id
                                       ,@objetivo_a_1
                                       ,@objetivo_a_2
                                       ,@objetivo_a_3
                                       ,@responsable
                                       ,@activa
                                       ,@registro_usuario
                                       ,GetDate());";

                var p = new
                {
                    perspectiva_id = perspectiva.perspectiva_id,
                    descripcion = perspectiva.descripcion,
                    pe_id = perspectiva.pe_id,
                    objetivo_a_1 = perspectiva.objetivo_a_1,
                    objetivo_a_2 = perspectiva.objetivo_a_2,
                    objetivo_a_3 = perspectiva.objetivo_a_3,
                    responsable = perspectiva.responsable,
                    activa = activa,
                    registro_usuario = perspectiva.registro_usuario
                };

                connection.Execute(query, p);
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        private ErrorDto Perspectiva_Actualizar(int CodEmpresa, PePerspectivasDto perspectiva)
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
                const string query = @"UPDATE [dbo].[PE_PERSPECTIVAS]
                                       SET [DESCRIPCION] = @descripcion
                                          ,[PE_ID] = @pe_id
                                          ,[OBJETIVO_A_1] = @objetivo_a_1
                                          ,[OBJETIVO_A_2] = @objetivo_a_2
                                          ,[OBJETIVO_A_3] = @objetivo_a_3
                                          ,[RESPONSABLE] = @responsable
                                          ,[ACTIVA] = @activa
                                          ,[MODIFICA_FECHA] = GetDate()
                                          ,[MODIFICA_USUARIO] = @modifica_usuario
                                     WHERE PERSPECTIVA_ID = @perspectiva_id;";

                var p = new
                {
                    perspectiva_id = perspectiva.perspectiva_id,
                    descripcion = perspectiva.descripcion,
                    pe_id = perspectiva.pe_id,
                    objetivo_a_1 = perspectiva.objetivo_a_1,
                    objetivo_a_2 = perspectiva.objetivo_a_2,
                    objetivo_a_3 = perspectiva.objetivo_a_3,
                    responsable = perspectiva.responsable,
                    activa = (perspectiva.activa ? 1 : 0),
                    modifica_usuario = perspectiva.modifica_usuario
                };

                connection.Execute(query, p);
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
                //Valida si existe en la tabla de objetivos
                const string qCount = @"SELECT COUNT(1) FROM [dbo].[PE_OBJETIVOS] WHERE PERSPECTIVA_ID = @perspectiva;";
                var count = connection.ExecuteScalar<int>(qCount, new { perspectiva });
                if (count > 0)
                {
                    resp.Code = -1;
                    resp.Description = "No se puede eliminar la perspectiva, ya que tiene objetivos asociados.";
                    return resp;
                }

                const string qDelete = @"DELETE FROM [dbo].[PE_PERSPECTIVAS] WHERE PERSPECTIVA_ID = @perspectiva;";
                connection.Execute(qDelete, new { perspectiva });
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto<List<PePerspectivasDto>> PePlanesLista_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<PePerspectivasDto>>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                const string query = @"select [PE_ID]
                                      ,[DESCRIPCION] from PE_PLANES Where ESTADO = 'A' 
                                       AND FINALIZACION > getDate() ";
                response.Result = connection.Query<PePerspectivasDto>(query).ToList();
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
                data = new List<PePerspectivasDto>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                var p = new DynamicParameters();
                bool hasFilter = TryAddPerspectivasFiltro(filtros, p);

                int offset = filtros.pagina ?? 0;
                if (offset < 0) offset = 0;

                const string countNoFilter = "select COUNT(1) from PE_PERSPECTIVAS;";
                const string countWithFilter = @"select COUNT(1)
from PE_PERSPECTIVAS
where CAST(perspectiva_id AS varchar(50)) LIKE @Q
   OR DESCRIPCION LIKE @Q;";

                response.Result.total = connection.ExecuteScalar<int>(hasFilter ? countWithFilter : countNoFilter, p);

                var sb = new StringBuilder();
                sb.Append(@"select [PERSPECTIVA_ID]
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
                              ,[MODIFICA_USUARIO]
                       from PE_PERSPECTIVAS ");

                if (hasFilter)
                {
                    sb.Append(@" where CAST(perspectiva_id AS varchar(50)) LIKE @Q
                                 OR DESCRIPCION LIKE @Q ");
                }

                sb.Append(" order by PERSPECTIVA_ID desc ");

                if (filtros.pagina != null)
                {
                    int pageFetch = filtros.paginacion ?? 30;
                    if (pageFetch < 1) pageFetch = 30;
                    p.Add("@OFFSET", offset);
                    p.Add("@FETCH", pageFetch);
                    sb.Append(" OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY ");
                }

                response.Result.data = connection.Query<PePerspectivasDto>(sb.ToString(), p).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.data = null;
            }
            return response;
        }

        private static bool TryAddPerspectivasFiltro(PePerspectivasFiltros filtros, DynamicParameters p)
        {
            string q = (filtros?.filtro ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(q))
                return false;

            p.Add("@Q", $"%{q}%");
            return true;
        }

    }
}