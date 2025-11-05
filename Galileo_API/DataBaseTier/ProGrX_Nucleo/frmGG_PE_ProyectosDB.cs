using PgxAPI.Models.GG_PE;
using Dapper;
using PgxAPI.Models.ERROR;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;

namespace PgxAPI.DataBaseTier
{
    public class frmGG_PE_ProyectosDB
    {
        private readonly IConfiguration _config;

        public frmGG_PE_ProyectosDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<PeProyectosLista> PeProyectoLista_Obtener(int CodEmpresa, string Jfiltros)
        {
            PeProyectosFiltros filtros = JsonConvert.DeserializeObject<PeProyectosFiltros>(Jfiltros) ?? new PeProyectosFiltros();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<PeProyectosLista>();
            response.Result = new PeProyectosLista
            {
                total = 0,
                proyectos = new List<PeProyectosDTO>()
            };

            try
            {
                var query = "";
                string where = " ", paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(stringConn);
                {
                    if (filtros.filtro != null)
                    {
                        where = "where   proyecto_id LIKE '%" + filtros.filtro + "%' OR " +
                                        "tipo LIKE '%" + filtros.filtro + "%' OR " +
                                        "nombre LIKE '%" + filtros.filtro + "%' OR " +
                                        "descripcion LIKE '%" + filtros.filtro + "%' OR " +
                                        "responsable LIKE '%" + filtros.filtro + "%'  ";
                    }

                    if (filtros.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                    }

                    query = $"select COUNT(*) from PE_PROYECTOS {where}";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();

                    query = $@"select [PROYECTO_ID]
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
                                      ,[MODIFICA_USUARIO] from PE_PROYECTOS {where} order 
                            by PROYECTO_ID desc {paginaActual} {paginacionActual}";
                    response.Result.proyectos = connection.Query<PeProyectosDTO>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.proyectos = null;
            }

            return response;
        }

        public ErrorDto PeProyecto_Guardar(int CodEmpresa, PeProyectosDTO proyectos)
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

        private ErrorDto PeProyecto_Insertar(int CodEmpresa, PeProyectosDTO proyectos)
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
                    var queryID = "SELECT ISNULL(MAX(PROYECTO_ID),0) + 1 FROM PE_PROYECTOS";
                    var secuencia = connection.Query<int>(queryID).FirstOrDefault();
                    proyectos.proyecto_id = secuencia;

                    int activa = proyectos.activo ? 1 : 0;

                    proyectos.programa_id = proyectos.programa_id == 0 ? proyectos.proyecto_id : proyectos.programa_id;

                    var query = $@"INSERT INTO PE_PROYECTOS
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
                                ({proyectos.proyecto_id}
                                ,{proyectos.programa_id}
                                ,'{proyectos.tipo}'
                                ,'{proyectos.nombre}'
                                ,'{proyectos.descripcion}'
                                ,'{proyectos.responsable}'
                                ,{proyectos.presupuesto}
                                ,'{proyectos.fecha_inicio}'
                                ,'{proyectos.fecha_finaliza}'
                                ,{activa}
                                ,'{proyectos.registro_usuario}'
                                ,GetDate())";
                    error.Code = connection.Execute(query, proyectos);
                    error.Description = secuencia.ToString();
                }
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }

            return error;
        }

        private ErrorDto PeProyecto_Actualizar(int CodEmpresa, PeProyectosDTO proyectos)
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
                    int activa = proyectos.activo ? 1 : 0;

                    var query = $@"UPDATE PE_PROYECTOS
                                SET [PROGRAMA_ID] = {proyectos.programa_id}
                                ,[TIPO] = '{proyectos.tipo}'
                                ,[NOMBRE] = '{proyectos.nombre}'
                                ,[DESCRIPCION] = '{proyectos.descripcion}'
                                ,[RESPONSABLE] = '{proyectos.responsable}'
                                ,[PRESUPUESTO] = {proyectos.presupuesto}
                                ,[FECHA_INICIO] = '{proyectos.fecha_inicio}'
                                ,[FECHA_FINALIZA] = '{proyectos.fecha_finaliza}'
                                ,[ACTIVO] = {activa}
                                ,[MODIFICA_USUARIO] = '{proyectos.modifica_usuario}'
                                ,[MODIFICA_FECHA] = GetDate()
                                WHERE PROYECTO_ID = {proyectos.proyecto_id}";
                    error.Code = connection.Execute(query, proyectos);
                    error.Description = proyectos.proyecto_id.ToString();
                }
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
                {
                    //Buscar si proyectos tiene dependencias con objetivos
                    var query = $@"SELECT COUNT(*) FROM PE_PROYECTOS_OBJETIVOS WHERE PROYECTO_ID = {proyecto_id}";
                    var dependencias = connection.Query<int>(query).FirstOrDefault();

                    if (dependencias > 0)
                    {
                        error.Code = -1;
                        error.Description = "No se puede eliminar el proyecto, tiene dependencias con objetivos";
                        return error;
                    }

                    query = $@"DELETE FROM PE_PROYECTOS WHERE PROYECTO_ID = {proyecto_id}";
                    error.Code = connection.Execute(query);
                }
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
                {
                    var query = $@"SELECT 
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
                                    LEFT JOIN PE_PROYECTOS_OBJETIVOS PO ON PO.OBJETIVO_ID = O.OBJETIVO_ID AND PO.PROYECTO_ID = '{proyecto_id}'
                                  order by PO.REGISTRO_USUARIO desc ";
                    response.Result = connection.Query<PeProyectoObjetivosLista>(query).ToList();
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
                {
                    var query = $@"INSERT INTO PE_PROYECTOS_OBJETIVOS
                                ([PROYECTO_ID]
                                ,[OBJETIVO_ID]
                                ,[REGISTRO_USUARIO]
                                ,[REGISTRO_FECHA])
                                VALUES
                                ({proyecto_id}
                                ,{objetivo_id}
                                ,'{usuario}'
                                ,GetDate())";
                    error.Code = connection.Execute(query);
                }
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
                {
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