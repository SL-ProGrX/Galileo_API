using Galileo.Models.GG_PE;
using Dapper;
using Galileo.Models.ERROR;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using System.Text;


namespace Galileo.DataBaseTier
{
    public class FrmGgPePlanesDB
    {
        private readonly IConfiguration _config;

        public FrmGgPePlanesDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<PePlanesDatosLista> PePlanesLista_Obtener(int CodEmpresa, string Jfiltros)
        {
            PePlanesFiltros filtros = JsonConvert.DeserializeObject<PePlanesFiltros>(Jfiltros) ?? new PePlanesFiltros();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<PePlanesDatosLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new PePlanesDatosLista
                {
                    total = 0,
                    data = new List<PePlanesDto>()
                }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                var p = new DynamicParameters();
                bool hasFilter = TryAddPlanesFiltro(filtros, p);

                int offset = filtros.pagina ?? 0;
                if (offset < 0) offset = 0;

                const string countNoFilter = "select COUNT(1) from PE_PLANES;";
                const string countWithFilter = @"select COUNT(1)
from PE_PLANES
where CAST(PE_ID AS varchar(50)) LIKE @Q
   OR DESCRIPCION LIKE @Q
   OR MISION LIKE @Q
   OR VISION LIKE @Q
   OR CAST(FINALIZACION AS varchar(50)) LIKE @Q
   OR CAST(INICIO AS varchar(50)) LIKE @Q;";

                response.Result.total = connection.ExecuteScalar<int>(hasFilter ? countWithFilter : countNoFilter, p);

                var sb = new StringBuilder();
                sb.Append(@"select [PE_ID]
                          ,[DESCRIPCION]
                          ,[INICIO]
                          ,[FINALIZACION]
                          ,[ESTADO]
                          ,[MISION]
                          ,[VISION]
                          ,[MODIFICA_FECHA]
                          ,[MODIFICA_USUARIO]
                          ,[REGISTRO_USUARIO]
                          ,[REGISTRO_FECHA]
                   from PE_PLANES ");

                if (hasFilter)
                {
                    sb.Append(@" where CAST(PE_ID AS varchar(50)) LIKE @Q
                         OR DESCRIPCION LIKE @Q
                         OR MISION LIKE @Q
                         OR VISION LIKE @Q
                         OR CAST(FINALIZACION AS varchar(50)) LIKE @Q
                         OR CAST(INICIO AS varchar(50)) LIKE @Q ");
                }

                sb.Append(" order by PE_ID desc ");

                if (filtros.pagina != null)
                {
                    int pageFetch = filtros.paginacion ?? 30;
                    if (pageFetch < 1) pageFetch = 30;
                    p.Add("@OFFSET", offset);
                    p.Add("@FETCH", pageFetch);
                    sb.Append(" OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY ");
                }

                response.Result.data = connection.Query<PePlanesDto>(sb.ToString(), p).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.data = null;
            }

            return response;
        }

        private static bool TryAddPlanesFiltro(PePlanesFiltros filtros, DynamicParameters p)
        {
            string q = (filtros?.filtro ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(q))
                return false;

            p.Add("@Q", $"%{q}%");
            return true;
        }

        public ErrorDto PePlanes_Guardar(int CodEmpresa, PePlanesDto plan)
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

        private ErrorDto PePlanes_Insertar(int CodEmpresa, PePlanesDto plan)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto error = new()
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                //Busco el ultimo siguiente consecutico si es null es el primero de la tabla como 0
                var queryID = "SELECT ISNULL(MAX(PE_ID),0) + 1 FROM PE_PLANES";
                var secuencia = connection.Query<int>(queryID).FirstOrDefault();
                plan.pe_id = secuencia;

                const string insert = @"INSERT INTO [dbo].[PE_PLANES]
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
       (@pe_id
       ,@descripcion
       ,@inicio
       ,@finalizacion
       ,'A'
       ,@mision
       ,@vision
       ,@registro_usuario
       ,getDate());";

                var p = new
                {
                    pe_id = plan.pe_id,
                    descripcion = plan.descripcion,
                    inicio = plan.inicio,
                    finalizacion = plan.finalizacion,
                    mision = plan.mision,
                    vision = plan.vision,
                    registro_usuario = plan.registro_usuario
                };

                error.Code = connection.Execute(insert, p);

                error.Description = plan.pe_id.ToString();
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }

            return error;
        }

        private ErrorDto PePlanes_Actualizar(int CodEmpresa, PePlanesDto plan)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto error = new()
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                const string update = @"UPDATE [dbo].[PE_PLANES]
   SET [DESCRIPCION] = @descripcion
      ,[INICIO] = @inicio
      ,[FINALIZACION] = @finalizacion
      ,[ESTADO] = @estado
      ,[MISION] = @mision
      ,[VISION] = @vision
      ,[MODIFICA_FECHA] = GetDate()
      ,[MODIFICA_USUARIO] = @modifica_usuario
 WHERE [PE_ID] = @pe_id;";

                var p = new
                {
                    pe_id = plan.pe_id,
                    descripcion = plan.descripcion,
                    inicio = plan.inicio,
                    finalizacion = plan.finalizacion,
                    estado = plan.estado,
                    mision = plan.mision,
                    vision = plan.vision,
                    modifica_usuario = plan.modifica_usuario
                };

                error.Code = connection.Execute(update, p);

                error.Description = plan.pe_id.ToString();
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
                //Busco si el plan tiene registros en la tabla de perpectivas
                const string qCount = "SELECT COUNT(1) FROM PE_PERSPECTIVAS WHERE PE_ID = @pe_id";
                var respuesta = connection.ExecuteScalar<int>(qCount, new { pe_id });

                if (respuesta > 0)
                {
                    error.Code = -1;
                    error.Description = "No se puede eliminar el plan, tiene perspectivas asociadas";
                }
                else
                {
                    const string qDelete = "DELETE FROM [dbo].[PE_PLANES] WHERE [PE_ID] = @pe_id";
                    error.Code = connection.Execute(qDelete, new { pe_id });
                }
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }
            return error;
        }

        public ErrorDto<List<PePlanesDto>> PePlanes_Exportar(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<PePlanesDto>>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                const string query = @"select [PE_ID]
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
                response.Result = connection.Query<PePlanesDto>(query).ToList();
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