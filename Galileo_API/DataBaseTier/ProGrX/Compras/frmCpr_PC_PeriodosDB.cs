using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.CPR;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmCpr_PC_PeriodosDB
    {
        private readonly IConfiguration _config;

        public frmCpr_PC_PeriodosDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDTO<List<CatalogosLista>> CprPeriodosContabilidades_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<CatalogosLista>>();
            response.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select cod_contabilidad as 'item', Nombre as 'descripcion' from CNTX_Contabilidades
                               order by cod_contabilidad";
                    response.Result = connection.Query<CatalogosLista>(query).ToList();
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

        public ErrorDTO<List<CatalogosLista>> CprPeriodosModelos_Obtener(int CodEmpresa, string usuario, int cod_contabilidad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<CatalogosLista>>();
            response.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select P.cod_modelo as 'item' , P.DESCRIPCION as 'descripcion'
                                From PRES_MODELOS P INNER JOIN PRES_MODELOS_USUARIOS Pmu on P.cod_Contabilidad = Pmu.cod_contabilidad
                                 and P.cod_Modelo = Pmu.cod_Modelo and Pmu.Usuario = '{usuario}'
                                inner join CNTX_CIERRES Cc on P.cod_Contabilidad = Cc.cod_Contabilidad and P.ID_CIERRE = Cc.ID_CIERRE 
                                Where P.COD_CONTABILIDAD = '{cod_contabilidad}'
                                order by Cc.Inicio_Anio desc";
                    response.Result = connection.Query<CatalogosLista>(query).ToList();
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

        public ErrorDTO<cprPlanPeriodosDTO> CprPeriodosPlan_Obtener(int CodEmpresa, int id_periodo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<cprPlanPeriodosDTO>();
            response.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT * FROM CPR_PLAN_PERIODOS WHERE ID_PERIODO = {id_periodo} ";
                    response.Result = connection.QueryFirstOrDefault<cprPlanPeriodosDTO>(query);
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

        public ErrorDTO<cprPeriodosPlanLista> CprPeriodosPlanLista_Obtener(int CodEmpresa, string filtros)
        {
            cprPeriodosPlanFiltros filtro = JsonConvert.DeserializeObject<cprPeriodosPlanFiltros>(filtros);
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<cprPeriodosPlanLista>();
            response.Result = new cprPeriodosPlanLista();
            response.Code = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";

                using var connection = new SqlConnection(stringConn);
                {
                    //Busco Total
                    query = "SELECT COUNT(*) FROM CPR_PLAN_PERIODOS";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();
                    if (filtro.filtro != null)
                    {
                        filtro.filtro = " WHERE (ID_PERIODO LIKE '%" + filtro.filtro + "%' " +
                             "OR COD_CONTABILIDAD LIKE '%" + filtro.filtro + "%' " +
                             "OR INICIO LIKE '%" + filtro.filtro + "%' " +
                             "OR CORTE LIKE '%" + filtro.filtro + "%') ";
                    }

                    if (filtro.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtro.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtro.paginacion + " ROWS ONLY ";
                    }

                    query = $@"SELECT ID_PERIODO, COD_CONTABILIDAD, INICIO, CORTE  FROM CPR_PLAN_PERIODOS
                                         {filtro.filtro} 
                                        ORDER BY ID_PERIODO
                                        {paginaActual}
                                        {paginacionActual} ";


                    response.Result.lista = connection.Query<cprPlanPeriodosDTO>(query).ToList();

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

        public ErrorDTO<cprPlanPeriodosDTO> CprPeriodoPlan_Scroll(int CodEmpresa, int scroll, int? id_periodo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<cprPlanPeriodosDTO>();
            try
            {
                string where = " ", orderBy = " ";
                if (scroll == 1)
                {
                    where = $@" where ID_PERIODO > '{id_periodo}' ";
                    orderBy = " order by ID_PERIODO asc";
                }
                else
                {
                    where = $@" where ID_PERIODO < '{id_periodo}' ";
                    orderBy = " order by ID_PERIODO desc";
                }

                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"Select top 1 * from CPR_PLAN_PERIODOS {where} {orderBy}";
                    response.Result = connection.QueryFirstOrDefault<cprPlanPeriodosDTO>(query);
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

        public ErrorDTO CprPeriodoPlan_Guardar(int CodEmpresa, cprPlanPeriodosDTO periodo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new()
            {
                Code = 0,
                Description = ""
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    if (periodo.id_periodo == 0)
                    {
                        //Obtengo el siguiente ID
                        var query = $@"SELECT ISNULL(MAX(ID_PERIODO),0) + 1 FROM CPR_PLAN_PERIODOS";
                        periodo.id_periodo = connection.QueryFirstOrDefault<int>(query);

                        resp = CprPeriodoPlan_Insertar(CodEmpresa, periodo);

                    }
                    else
                    {
                        resp = CprPeriodoPlan_Actualizar(CodEmpresa, periodo);
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

        private ErrorDTO CprPeriodoPlan_Insertar(int CodEmpresa, cprPlanPeriodosDTO periodo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"INSERT INTO [dbo].[CPR_PLAN_PERIODOS]
                                       ([ID_PERIODO]
                                       ,[COD_CONTABILIDAD]
                                       ,[INICIO]
                                       ,[CORTE]
                                       ,[ESTADO]
                                       ,[NOTAS]
                                       ,[REGISTRO_FECHA]
                                       ,[REGISTRO_USUARIO]
                                      )
                                 VALUES
                                       ({periodo.id_periodo}
                                       ,{periodo.cod_contabilidad}
                                       ,'{periodo.inicio}'
                                       ,'{periodo.corte}'
                                       ,'P'
                                       ,'{periodo.notas}'
                                       ,getdate()
                                       ,'{periodo.registro_usuario}'
                                      )";
                    connection.Execute(query);
                    query = $@"SELECT TOP 1 ID_PERIODO from CPR_PLAN_PERIODOS ORDER BY ID_PERIODO DESC";
                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Periodo agregado satisfactoriamente!";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        private ErrorDTO CprPeriodoPlan_Actualizar(int CodEmpresa, cprPlanPeriodosDTO periodo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"UPDATE [dbo].[CPR_PLAN_PERIODOS]
                                   SET 
                                      [COD_CONTABILIDAD] = {periodo.cod_contabilidad}
                                      ,[INICIO] = '{periodo.inicio}'
                                      ,[CORTE] = '{periodo.corte}'
                                      ,[ESTADO] = '{periodo.estado}'
                                      ,[NOTAS] = '{periodo.notas}'
                                      ,[MODIFICA_FECHA] = getdate()
                                      ,[MODIFICA_USUARIO] = '{periodo.modifica_usuario}'
                                 WHERE [ID_PERIODO] = {periodo.id_periodo}";
                    connection.Execute(query);
                    resp.Code = periodo.id_periodo;
                    resp.Description = "Periodo actualizado satisfactoriamente!";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDTO CprPeriodoPlan_Eliminar(int CodEmpresa, int id_periodo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //var query = $@"delete CPR_PLAN_PERIODOS where id_Periodo = '{id_periodo}'";
                    //connection.Execute(query);

                    //Incluir bitacora
                    resp.Description = "No se puede eliminar un Periodo del Sistema!";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDTO CprPeriodoPlan_Aprobar(int CodEmpresa, int id_periodo, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"UPDATE [dbo].[CPR_PLAN_PERIODOS]
                                   SET 
                                      ESTADO = 'A'
                                      ,[ACTUALIZA_FECHA] = getdate()
                                      ,[ACTUALIZA_USUARIO] = '{usuario}'
                                 WHERE [ID_PERIODO] = {id_periodo}";
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

        public ErrorDTO<cprModeloDateDatos> CprPeriodoPlanMeses_Obtener(string modelo)
        {
            cprModeloFiltro cpr = JsonConvert.DeserializeObject<cprModeloFiltro>(modelo);
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(cpr.codEmpresa);
            var response = new ErrorDTO<cprModeloDateDatos>();
            response.Result = new cprModeloDateDatos();
            cprModeloDatos datos = new cprModeloDatos();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select cc.INICIO_MES, cc.CORTE_MES  From PRES_MODELOS P INNER JOIN PRES_MODELOS_USUARIOS Pmu on P.cod_Contabilidad = Pmu.cod_contabilidad
                                 and P.cod_Modelo = Pmu.cod_Modelo and Pmu.Usuario = '{cpr.usuario}'
                                inner join CNTX_CIERRES Cc on P.cod_Contabilidad = Cc.cod_Contabilidad and P.ID_CIERRE = Cc.ID_CIERRE 
                                Where P.COD_CONTABILIDAD = '{cpr.cod_Contabilidad}' AND P.COD_MODELO = '{cpr.cod_modelo}'
                                order by Cc.Inicio_Anio desc";
                    datos = connection.QueryFirstOrDefault<cprModeloDatos>(query);
                    int annoActual = DateTime.Now.Year;

                    // Fecha de inicio: primer d�a del mes de inicio
                    DateTime fechaInicio = new DateTime(annoActual, datos.inicio_mes, 1);

                    // Fecha de fin: �ltimo d�a del mes de fin
                    int ultimoDiaMesFin = DateTime.DaysInMonth(annoActual, datos.corte_mes);

                    DateTime fechaFin = new DateTime(annoActual, datos.corte_mes, ultimoDiaMesFin);

                    response.Result.inicio_mes = fechaInicio;
                    response.Result.corte_mes = fechaFin;
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;

        }

    }
}