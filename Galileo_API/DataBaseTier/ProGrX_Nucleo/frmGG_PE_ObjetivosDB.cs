using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.GG_PE;

namespace PgxAPI.DataBaseTier
{
    public class frmGG_PE_ObjetivosDB
    {
        private readonly IConfiguration _config;

        public frmGG_PE_ObjetivosDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<PeObjetivosEstrategicosDatosLista> PeObjetivosEstrategicosLista_Obtener(int CodEmpresa, string Jfiltros)
        {
            PeObjetivosEstrategicosFiltros filtros = JsonConvert.DeserializeObject<PeObjetivosEstrategicosFiltros>(Jfiltros) ?? new PeObjetivosEstrategicosFiltros();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<PeObjetivosEstrategicosDatosLista>();
            response.Result = new PeObjetivosEstrategicosDatosLista
            {
                total = 0,
                data = new List<PeObjetivosEstrategicosDTO>()
            };

            try
            {
                var query = "";
                string where = " ", paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(stringConn);
                {
                    if (filtros.filtro != null)
                    {
                        where = "where O.objetivo_id LIKE '%" + filtros.filtro + "%' OR " +
                            "O.DESCRIPCION LIKE '%" + filtros.filtro + "%' OR " +
                            "O.nombre LIKE '%" + filtros.filtro + "%' OR " +
                            "O.indicador_clave LIKE '%" + filtros.filtro + "%' OR " +
                            "O.meta LIKE '%" + filtros.filtro + "%' OR " +
                            "O.unidad_medida LIKE '%" + filtros.filtro + "%'  ";
                    }

                    if (filtros.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                    }

                    query = $"select COUNT(*) from PE_OBJETIVOS O {where}";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();

                    query = $@"select  O.[OBJETIVO_ID]
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
                                      ,O.[MODIFICA_USUARIO] from PE_OBJETIVOS O
                            left join PE_PERSPECTIVAS P ON P.PERSPECTIVA_ID = O.PERSPECTIVA_ID {where} order 
                            by O.objetivo_id desc {paginaActual} {paginacionActual}";
                    response.Result.data = connection.Query<PeObjetivosEstrategicosDTO>(query).ToList();
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

        public ErrorDto ObjetivosEstrategicos_Guardar(int CodEmpresa, PeObjetivosEstrategicosDTO objetivo)
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

        private ErrorDto ObjetivosEstrategicos_Insertar(int CodEmpresa, PeObjetivosEstrategicosDTO objetivo)
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
                    var queryID = "SELECT ISNULL(MAX(OBJETIVO_ID),0) + 1 FROM PE_OBJETIVOS";
                    var secuencia = connection.Query<int>(queryID).FirstOrDefault();
                    objetivo.objetivo_id = secuencia;

                    int activa = objetivo.activo ? 1 : 0;

                    var insert = $@"INSERT INTO PE_OBJETIVOS
                                        (OBJETIVO_ID
                                        ,PERSPECTIVA_ID
                                        ,NOMBRE
                                        ,DESCRIPCION
                                        ,INDICADOR_CLAVE
                                        ,META
                                        ,UNIDAD_MEDIDA
                                        ,ACTIVO
                                        ,REGISTRO_USUARIO
                                        ,REGISTRO_FECHA)
                                        VALUES
                                        ({objetivo.objetivo_id}
                                        ,{objetivo.perspectiva_id}
                                        ,'{objetivo.nombre}'
                                        ,'{objetivo.descripcion}'
                                        ,'{objetivo.indicador_clave}'
                                        ,'{objetivo.meta}'
                                        ,'{objetivo.unidad_medida}'
                                        ,{activa}
                                        ,'{objetivo.registro_usuario}'
                                        ,GetDate())";

                    error.Code = connection.Execute(insert, objetivo);
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

        private ErrorDto ObjetivosEstrategicos_Actualizar(int CodEmpresa, PeObjetivosEstrategicosDTO objetivo)
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
                    int activa = objetivo.activo ? 1 : 0;

                    var update = $@"UPDATE PE_OBJETIVOS
                                    SET PERSPECTIVA_ID = {objetivo.perspectiva_id}
                                    ,NOMBRE = '{objetivo.nombre}'
                                    ,DESCRIPCION = '{objetivo.descripcion}'
                                    ,INDICADOR_CLAVE = '{objetivo.indicador_clave}'
                                    ,META = '{objetivo.meta}'
                                    ,UNIDAD_MEDIDA = '{objetivo.unidad_medida}'
                                    ,ACTIVO = {activa}
                                    ,MODIFICA_USUARIO = '{objetivo.modifica_usuario}'
                                    ,MODIFICA_FECHA = GetDate()
                                    WHERE OBJETIVO_ID = {objetivo.objetivo_id}";

                    error.Code = connection.Execute(update, objetivo);

                    error.Description = objetivo.objetivo_id.ToString();
                }
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
                {
                    //busco si se encuentra en proyectos_objetivos
                    var query = $@"SELECT COUNT(*) FROM PE_PROYECTOS_OBJETIVOS WHERE OBJETIVO_ID = {objetivo_id}";
                    var count = connection.Query<int>(query).FirstOrDefault();

                    if (count > 0)
                    {
                        error.Code = -2;
                        error.Description = "No se puede eliminar el objetivo, ya que se encuentra asociado a un proyecto";
                        return error;
                    }

                    //Busco si se encuentra en KPIS
                    query = $@"SELECT COUNT(*) FROM PE_KPIS WHERE OBJETIVO_ID = {objetivo_id}";
                    count = connection.Query<int>(query).FirstOrDefault();

                    if (count > 0)
                    {
                        error.Code = -2;
                        error.Description = "No se puede eliminar el objetivo, ya que se encuentra asociado a un KPI";
                        return error;
                    }

                    var delete = $@"DELETE FROM PE_OBJETIVOS WHERE OBJETIVO_ID = {objetivo_id}";
                    error.Code = connection.Execute(delete);
                }
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }
            return error;
        }

        public ErrorDto<List<PeObjetivosEstrategicosDTO>> PePerspectivaLista_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<PeObjetivosEstrategicosDTO>>();
            response.Result = new List<PeObjetivosEstrategicosDTO>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT PERSPECTIVA_ID, DESCRIPCION AS nombre_pespectiva 
                                    FROM PE_PERSPECTIVAS WHERE ACTIVA = 1 ";
                    response.Result = connection.Query<PeObjetivosEstrategicosDTO>(query).ToList();
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

        public ErrorDto<List<PeObjetivosEstrategicosDTO>> PeObservacionesExportar_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<PeObjetivosEstrategicosDTO>>();
            response.Result = new List<PeObjetivosEstrategicosDTO>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select  O.[OBJETIVO_ID]
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
                                      ,O.[MODIFICA_USUARIO] from PE_OBJETIVOS O
                            left join PE_PERSPECTIVAS P ON P.PERSPECTIVA_ID = O.PERSPECTIVA_IDorder 
                            by O.objetivo_id desc ";
                    response.Result = connection.Query<PeObjetivosEstrategicosDTO>(query).ToList();
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