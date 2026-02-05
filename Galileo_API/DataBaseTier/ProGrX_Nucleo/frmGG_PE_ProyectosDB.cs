using Galileo.Models.GG_PE;
using Dapper;
using Galileo.Models.ERROR;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class FrmGgPeProyectosDB
    {
        private readonly IConfiguration _config;

        public FrmGgPeProyectosDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<PeProyectosLista> PeProyectoLista_Obtener(int CodEmpresa, string Jfiltros)
        {
            PeProyectosFiltros filtros = JsonConvert.DeserializeObject<PeProyectosFiltros>(Jfiltros) ?? new PeProyectosFiltros();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<PeProyectosLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new PeProyectosLista
                {
                    total = 0,
                    proyectos = new List<PeProyectosDto>()
                }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                var p = new DynamicParameters();
                bool hasFilter = TryAddProyectosFiltro(filtros, p);

                int offset = filtros.pagina ?? 0;
                if (offset < 0) offset = 0;

                const string countNoFilter = "select COUNT(1) from PE_PROYECTOS;";
                const string countWithFilter = @"select COUNT(1)
from PE_PROYECTOS
where CAST(PROYECTO_ID AS varchar(50)) LIKE @Q
   OR TIPO LIKE @Q
   OR NOMBRE LIKE @Q
   OR DESCRIPCION LIKE @Q
   OR RESPONSABLE LIKE @Q;";

                response.Result.total = connection.ExecuteScalar<int>(hasFilter ? countWithFilter : countNoFilter, p);

                var sb = new StringBuilder();
                sb.Append(@"select [PROYECTO_ID]
                          ,[PROGRAMA_ID]
                          ,[TIPO]
                          ,[NOMBRE]
                          ,[DESCRIPCION]
                          ,[RESPONSABLE]
                          ,[PRESUPUESTO]
                          ,[FECHA_INICIO]
                          ,[FECHA_FINALIZA]
                          ,[ACTIVO]
                          ,[REGISTRO_USUARIO]
                          ,[REGISTRO_FECHA]
                          ,[MODIFICA_FECHA]
                          ,[MODIFICA_USUARIO]
                   from PE_PROYECTOS ");

                if (hasFilter)
                {
                    sb.Append(@" where CAST(PROYECTO_ID AS varchar(50)) LIKE @Q
                         OR TIPO LIKE @Q
                         OR NOMBRE LIKE @Q
                         OR DESCRIPCION LIKE @Q
                         OR RESPONSABLE LIKE @Q ");
                }

                sb.Append(" order by PROYECTO_ID desc ");

                if (filtros.pagina != null)
                {
                    int pageFetch = filtros.paginacion ?? 30;
                    if (pageFetch < 1) pageFetch = 30;
                    p.Add("@OFFSET", offset);
                    p.Add("@FETCH", pageFetch);
                    sb.Append(" OFFSET @OFFSET ROWS FETCH NEXT @FETCH ROWS ONLY ");
                }

                response.Result.proyectos = connection.Query<PeProyectosDto>(sb.ToString(), p).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.proyectos = null;
            }

            return response;
        }

        private static bool TryAddProyectosFiltro(PeProyectosFiltros filtros, DynamicParameters p)
        {
            string q = (filtros?.filtro ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(q))
                return false;

            p.Add("@Q", $"%{q}%");
            return true;
        }

        public ErrorDto PeProyecto_Guardar(int CodEmpresa, PeProyectosDto proyectos)
        {
            ErrorDto error = new()
            {
                Code = 0
            };

            try
            {
                if (proyectos.proyecto_id == 0)
                {
                    error = PeProyecto_Insertar(CodEmpresa, proyectos);
                }
                else
                {
                    error = PeProyecto_Actualizar(CodEmpresa, proyectos);
                }
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }

            return error;
        }

        private ErrorDto PeProyecto_Insertar(int CodEmpresa, PeProyectosDto proyectos)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto error = new()
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var queryID = "SELECT ISNULL(MAX(PROYECTO_ID),0) + 1 FROM PE_PROYECTOS";
                var secuencia = connection.Query<int>(queryID).FirstOrDefault();
                proyectos.proyecto_id = secuencia;

                int activa = proyectos.activo ? 1 : 0;
                proyectos.programa_id = proyectos.programa_id == 0 ? proyectos.proyecto_id : proyectos.programa_id;

                var query = @"INSERT INTO PE_PROYECTOS
                            ([PROYECTO_ID]
                            ,[PROGRAMA_ID]
                            ,[TIPO]
                            ,[NOMBRE]
                            ,[DESCRIPCION]
                            ,[RESPONSABLE]
                            ,[PRESUPUESTO]
                            ,[FECHA_INICIO]
                            ,[FECHA_FINALIZA]
                            ,[ACTIVO]
                            ,[REGISTRO_USUARIO]
                            ,[REGISTRO_FECHA])
                            VALUES
                            (@proyecto_id
                            ,@programa_id
                            ,@tipo
                            ,@nombre
                            ,@descripcion
                            ,@responsable
                            ,@presupuesto
                            ,@fecha_inicio
                            ,@fecha_finaliza
                            ,@activo
                            ,@registro_usuario
                            ,GetDate())";
                var parameters = new
                {
                    proyecto_id = proyectos.proyecto_id,
                    programa_id = proyectos.programa_id,
                    tipo = proyectos.tipo,
                    nombre = proyectos.nombre,
                    descripcion = proyectos.descripcion,
                    responsable = proyectos.responsable,
                    presupuesto = proyectos.presupuesto,
                    fecha_inicio = proyectos.fecha_inicio,
                    fecha_finaliza = proyectos.fecha_finaliza,
                    activo = activa,
                    registro_usuario = proyectos.registro_usuario
                };
                error.Code = connection.Execute(query, parameters);
                error.Description = secuencia.ToString();
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }

            return error;
        }

        private ErrorDto PeProyecto_Actualizar(int CodEmpresa, PeProyectosDto proyectos)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto error = new()
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                int activa = proyectos.activo ? 1 : 0;

                var query = @"UPDATE PE_PROYECTOS
                            SET [PROGRAMA_ID] = @programa_id
                            ,[TIPO] = @tipo
                            ,[NOMBRE] = @nombre
                            ,[DESCRIPCION] = @descripcion
                            ,[RESPONSABLE] = @responsable
                            ,[PRESUPUESTO] = @presupuesto
                            ,[FECHA_INICIO] = @fecha_inicio
                            ,[FECHA_FINALIZA] = @fecha_finaliza
                            ,[ACTIVO] = @activo
                            ,[MODIFICA_USUARIO] = @modifica_usuario
                            ,[MODIFICA_FECHA] = GetDate()
                            WHERE PROYECTO_ID = @proyecto_id";
                var parameters = new
                {
                    programa_id = proyectos.programa_id,
                    tipo = proyectos.tipo,
                    nombre = proyectos.nombre,
                    descripcion = proyectos.descripcion,
                    responsable = proyectos.responsable,
                    presupuesto = proyectos.presupuesto,
                    fecha_inicio = proyectos.fecha_inicio,
                    fecha_finaliza = proyectos.fecha_finaliza,
                    activo = activa,
                    modifica_usuario = proyectos.modifica_usuario,
                    proyecto_id = proyectos.proyecto_id
                };
                error.Code = connection.Execute(query, parameters);
                error.Description = proyectos.proyecto_id.ToString();
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }

            return error;
        }

        public ErrorDto PeProyecto_Eliminar(int CodEmpresa, int proyecto_id)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto error = new()
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                //Buscar si proyectos tiene dependencias con objetivos
                var query = @"SELECT COUNT(*) FROM PE_PROYECTOS_OBJETIVOS WHERE PROYECTO_ID = @proyecto_id";
                var dependencias = connection.Query<int>(query, new { proyecto_id }).FirstOrDefault();

                if (dependencias > 0)
                {
                    error.Code = -1;
                    error.Description = "No se puede eliminar el proyecto, tiene dependencias con objetivos";
                    return error;
                }

                query = @"DELETE FROM PE_PROYECTOS WHERE PROYECTO_ID = @proyecto_id";
                error.Code = connection.Execute(query, new { proyecto_id });
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }

            return error;
        }

        public ErrorDto<List<PeProyectoObjetivosLista>> PeObservacionesProyectos_Obtener(int CodEmpresa, int proyecto_id)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto<List<PeProyectoObjetivosLista>> response = new ErrorDto<List<PeProyectoObjetivosLista>>();
            response.Result = new List<PeProyectoObjetivosLista>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = @"SELECT 
                                O.OBJETIVO_ID, 
                                O.NOMBRE AS OBJETIVO, 
                                PR.DESCRIPCION AS PERSPECTIVA, 
                                PL.DESCRIPCION AS 'PLAN',
                                PO.REGISTRO_USUARIO,
                                CASE 
                                        WHEN PO.REGISTRO_USUARIO IS NULL THEN 0 
                                        ELSE 1 
                                    END AS asignado
                                FROM PE_OBJETIVOS O
                                LEFT JOIN PE_PERSPECTIVAS PR ON PR.PERSPECTIVA_ID = O.PERSPECTIVA_ID
                                LEFT JOIN PE_PLANES PL ON PL.PE_ID = PR.PE_ID
                                LEFT JOIN PE_PROYECTOS_OBJETIVOS PO ON PO.OBJETIVO_ID = O.OBJETIVO_ID AND PO.PROYECTO_ID = @proyecto_id
                              order by PO.REGISTRO_USUARIO desc ";
                response.Result = connection.Query<PeProyectoObjetivosLista>(query, new { proyecto_id }).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        public ErrorDto PeObjetivoProyecto_Asociar(int CodEmpresa, int proyecto_id, int objetivo_id, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto error = new()
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = @"INSERT INTO PE_PROYECTOS_OBJETIVOS
                            ([PROYECTO_ID]
                            ,[OBJETIVO_ID]
                            ,[REGISTRO_USUARIO]
                            ,[REGISTRO_FECHA])
                            VALUES
                            (@proyecto_id
                            ,@objetivo_id
                            ,@usuario
                            ,GetDate())";
                error.Code = connection.Execute(query, new { proyecto_id, objetivo_id, usuario });
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }
            return error;

        }

        public ErrorDto<List<PeProyectoObjetivosExportar>> PeProyectoObj_Exportar(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto<List<PeProyectoObjetivosExportar>> response = new ErrorDto<List<PeProyectoObjetivosExportar>>();
            response.Result = new List<PeProyectoObjetivosExportar>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                    var query = $@"SELECT 
	                                    T.Proyecto_ID,
	                                    T.Proyecto,
	                                    T.Programa_ID,
	                                    T.Programa,
	                                    T.Tipo,
	                                    T.DESCRIPCION,
	                                    (T.PRESUPUESTO / (SELECT CASE 
                                            WHEN COUNT(*) = 0 THEN 1 
                                            ELSE COUNT(*) 
                                        END AS resultado FROM PE_PROYECTOS_OBJETIVOS PO WHERE PO.PROYECTO_ID = T.Proyecto_ID))AS PRESUPUESTO,
	                                    T.FECHA_INICIO,
	                                    T.FECHA_FINALIZA,
	                                    T.ACTIVO,
	                                    T.Objetivo,
	                                    T.[Descripcion_Objetivo]
                                    FROM (
                                    SELECT 
                                    A.PROYECTO_ID AS 'Proyecto_ID', 
                                    A.NOMBRE AS 'Proyecto', 
                                    A.PROGRAMA_ID AS 'Programa_ID', 
                                    (SELECT B.NOMBRE FROM PE_PROYECTOS B WHERE B.PROYECTO_ID = A.PROGRAMA_ID) AS 'Programa',
                                    A.TIPO, 
                                    A.DESCRIPCION,
                                    A.PRESUPUESTO, 
                                    A.FECHA_INICIO, 
                                    A.FECHA_FINALIZA, 
                                    A.ACTIVO
                                    ,O.NOMBRE AS 'Objetivo' 
                                    ,O.DESCRIPCION AS 'Descripcion_Objetivo'
                                    FROM PE_PROYECTOS A
                                    LEFT JOIN PE_PROYECTOS_OBJETIVOS PO ON PO.PROYECTO_ID = A.PROYECTO_ID AND PO.PROYECTO_ID = A.PROYECTO_ID
                                    LEFT JOIN PE_OBJETIVOS O ON O.OBJETIVO_ID = PO.OBJETIVO_ID 
                                    )T";
                    response.Result = connection.Query<PeProyectoObjetivosExportar>(query).ToList();
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