using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models.ERROR;
using Galileo.Models.GG_PE;

namespace Galileo.DataBaseTier
{
    public class FrmGgPeObjetivosDb
    {
        private readonly IConfiguration _config;

        public FrmGgPeObjetivosDb(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<PeObjetivosEstrategicosDatosLista> PeObjetivosEstrategicosLista_Obtener(int CodEmpresa, string Jfiltros)
        {
            PeObjetivosEstrategicosFiltros filtros = JsonConvert.DeserializeObject<PeObjetivosEstrategicosFiltros>(Jfiltros)
                ?? new PeObjetivosEstrategicosFiltros();

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<PeObjetivosEstrategicosDatosLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new PeObjetivosEstrategicosDatosLista
                {
                    total = 0,
                    data = new List<PeObjetivosEstrategicosDto>()
                }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                var p = new DynamicParameters();
                string where = BuildObjetivosWhere(filtros, p);

                int offset = filtros.pagina ?? 0;
                if (offset < 0) offset = 0;

                string sqlCount = $"SELECT COUNT(1) FROM PE_OBJETIVOS O {where};";
                response.Result.total = connection.ExecuteScalar<int>(sqlCount, p);

                var sb = new StringBuilder();
                sb.Append(@"SELECT  O.[OBJETIVO_ID]
                          ,O.[PERSPECTIVA_ID]
                          ,P.DESCRIPCION AS nombre_pespectiva
                          ,O.[NOMBRE]
                          ,O.[DESCRIPCION]
                          ,O.[INDICADOR_CLAVE]
                          ,O.[META]
                          ,O.[UNIDAD_MEDIDA]
                          ,O.[ACTIVO]
                          ,O.[REGISTRO_USUARIO]
                          ,O.[REGISTRO_FECHA]
                          ,O.[MODIFICA_FECHA]
                          ,O.[MODIFICA_USUARIO]
                    FROM PE_OBJETIVOS O
                    LEFT JOIN PE_PERSPECTIVAS P ON P.PERSPECTIVA_ID = O.PERSPECTIVA_ID ");
                sb.Append(where);
                sb.Append(" ORDER BY O.objetivo_id DESC ");

                // Paginación opcional (solo si viene pagina)
                if (filtros.pagina != null)
                {
                    int pageFetch = filtros.paginacion ?? 30;
                    if (pageFetch < 1) pageFetch = 30;
                    p.Add("@OFFSET", offset);
                    p.Add("@FETCH", pageFetch);
                    sb.Append(" OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY ");
                }

                response.Result.data = connection.Query<PeObjetivosEstrategicosDto>(sb.ToString(), p).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.data = null;
            }

            return response;
        }

        private static string BuildObjetivosWhere(PeObjetivosEstrategicosFiltros filtros, DynamicParameters p)
        {
            if (filtros?.filtro == null)
                return string.Empty;

            string q = filtros.filtro.Trim();
            if (string.IsNullOrWhiteSpace(q))
                return string.Empty;

            // Búsqueda "global" sobre varias columnas. OBJETIVO_ID se castea para permitir LIKE.
            p.Add("@Q", $"%{q}%");

            return @"WHERE (
                 CAST(O.objetivo_id AS varchar(50)) LIKE @Q
              OR O.DESCRIPCION LIKE @Q
              OR O.nombre LIKE @Q
              OR O.indicador_clave LIKE @Q
              OR O.meta LIKE @Q
              OR O.unidad_medida LIKE @Q
            ) ";
        }

        public ErrorDto ObjetivosEstrategicos_Guardar(int CodEmpresa, PeObjetivosEstrategicosDto objetivo)
        {
            ErrorDto error = new()
            {
                Code = 0
            };

            try
            {
                if (objetivo.objetivo_id == 0)
                {
                    error = ObjetivosEstrategicos_Insertar(CodEmpresa, objetivo);
                }
                else
                {
                    error = ObjetivosEstrategicos_Actualizar(CodEmpresa, objetivo);
                }
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }

            return error;
        }

        private ErrorDto ObjetivosEstrategicos_Insertar(int CodEmpresa, PeObjetivosEstrategicosDto objetivo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto error = new()
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var queryID = "SELECT ISNULL(MAX(OBJETIVO_ID),0) + 1 FROM PE_OBJETIVOS";
                var secuencia = connection.Query<int>(queryID).FirstOrDefault();
                objetivo.objetivo_id = secuencia;

                int activa = objetivo.activo ? 1 : 0;

                const string insert = @"INSERT INTO PE_OBJETIVOS
(OBJETIVO_ID, PERSPECTIVA_ID, NOMBRE, DESCRIPCION, INDICADOR_CLAVE, META, UNIDAD_MEDIDA, ACTIVO, REGISTRO_USUARIO, REGISTRO_FECHA)
VALUES
(@objetivo_id, @perspectiva_id, @nombre, @descripcion, @indicador_clave, @meta, @unidad_medida, @activa, @registro_usuario, GetDate());";

                var p = new
                {
                    objetivo_id = objetivo.objetivo_id,
                    perspectiva_id = objetivo.perspectiva_id,
                    nombre = objetivo.nombre,
                    descripcion = objetivo.descripcion,
                    indicador_clave = objetivo.indicador_clave,
                    meta = objetivo.meta,
                    unidad_medida = objetivo.unidad_medida,
                    activa = activa,
                    registro_usuario = objetivo.registro_usuario
                };

                error.Code = connection.Execute(insert, p);
                error.Description = secuencia.ToString();

            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }
            return error;
        }

        private ErrorDto ObjetivosEstrategicos_Actualizar(int CodEmpresa, PeObjetivosEstrategicosDto objetivo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto error = new()
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                int activa = objetivo.activo ? 1 : 0;

                const string update = @"UPDATE PE_OBJETIVOS
SET PERSPECTIVA_ID = @perspectiva_id,
    NOMBRE = @nombre,
    DESCRIPCION = @descripcion,
    INDICADOR_CLAVE = @indicador_clave,
    META = @meta,
    UNIDAD_MEDIDA = @unidad_medida,
    ACTIVO = @activa,
    MODIFICA_USUARIO = @modifica_usuario,
    MODIFICA_FECHA = GetDate()
WHERE OBJETIVO_ID = @objetivo_id;";

                var p = new
                {
                    objetivo_id = objetivo.objetivo_id,
                    perspectiva_id = objetivo.perspectiva_id,
                    nombre = objetivo.nombre,
                    descripcion = objetivo.descripcion,
                    indicador_clave = objetivo.indicador_clave,
                    meta = objetivo.meta,
                    unidad_medida = objetivo.unidad_medida,
                    activa = activa,
                    modifica_usuario = objetivo.modifica_usuario
                };

                error.Code = connection.Execute(update, p);
                error.Description = objetivo.objetivo_id.ToString();
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }
            return error;
        }

        public ErrorDto ObjetivosEstrategicos_Eliminar(int CodEmpresa, int objetivo_id)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto error = new()
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                //busco si se encuentra en proyectos_objetivos
                const string qProyectos = "SELECT COUNT(1) FROM PE_PROYECTOS_OBJETIVOS WHERE OBJETIVO_ID = @objetivo_id";
                var count = connection.ExecuteScalar<int>(qProyectos, new { objetivo_id });

                if (count > 0)
                {
                    error.Code = -2;
                    error.Description = "No se puede eliminar el objetivo, ya que se encuentra asociado a un proyecto";
                    return error;
                }

                //Busco si se encuentra en KPIS
                const string qKpis = "SELECT COUNT(1) FROM PE_KPIS WHERE OBJETIVO_ID = @objetivo_id";
                count = connection.ExecuteScalar<int>(qKpis, new { objetivo_id });

                if (count > 0)
                {
                    error.Code = -2;
                    error.Description = "No se puede eliminar el objetivo, ya que se encuentra asociado a un KPI";
                    return error;
                }

                const string delete = "DELETE FROM PE_OBJETIVOS WHERE OBJETIVO_ID = @objetivo_id";
                error.Code = connection.Execute(delete, new { objetivo_id });
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }
            return error;
        }

        public ErrorDto<List<PeObjetivosEstrategicosDto>> PePerspectivaLista_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<PeObjetivosEstrategicosDto>>();
            response.Result = new List<PeObjetivosEstrategicosDto>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = $@"SELECT PERSPECTIVA_ID, DESCRIPCION AS nombre_pespectiva 
                                    FROM PE_PERSPECTIVAS WHERE ACTIVA = 1 ";
                response.Result = connection.Query<PeObjetivosEstrategicosDto>(query).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;

        }

        public ErrorDto<List<PeObjetivosEstrategicosDto>> PeObservacionesExportar_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<PeObjetivosEstrategicosDto>>();
            response.Result = new List<PeObjetivosEstrategicosDto>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = @"select  O.[OBJETIVO_ID]
                  ,O.[PERSPECTIVA_ID]
                  ,P.DESCRIPCION AS nombre_pespectiva
                  ,O.[NOMBRE]
                  ,O.[DESCRIPCION]
                  ,O.[INDICADOR_CLAVE]
                  ,O.[META]
                  ,O.[UNIDAD_MEDIDA]
                  ,O.[ACTIVO]
                  ,O.[REGISTRO_USUARIO]
                  ,O.[REGISTRO_FECHA]
                  ,O.[MODIFICA_FECHA]
                  ,O.[MODIFICA_USUARIO]
            from PE_OBJETIVOS O
            left join PE_PERSPECTIVAS P ON P.PERSPECTIVA_ID = O.PERSPECTIVA_ID
            order by O.objetivo_id desc ";
                response.Result = connection.Query<PeObjetivosEstrategicosDto>(query).ToList();
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