using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;

namespace PgxAPI.DataBaseTier.ProGrX.Clientes
{
    public class frmAF_PadronEmpleadosDB
    {
        private readonly IConfiguration _config;

        public frmAF_PadronEmpleadosDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Metodo que obtiene la lista de instituciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_PadronEmpleadosInstituciones_Obtener(int CodEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>> 
            { 
                Code = 0,
                Description = "OK",
                Result = new() 
            };
            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);
                {
                    const string query = @"Select COD_INSTITUCION as 'item',  CONCAT('[',COD_DIVISA,']  ', DESCRIPCION) as 'descripcion'
                                             from INSTITUCIONES where ACTIVA = 1
                                             order by COD_INSTITUCION";

                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// Metodo que obtiene la lista de estados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_PadronEmpleadosEstados_Obtener(int CodEmpresa)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "OK",
                Result = new()
            };
            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);
                {
                    const string query = @"Select RTRIM(COD_ESTADO) as 'item',  RTRIM( DESCRIPCION ) as 'descripcion'
                                             from AFI_ESTADOS_PERSONA where ACTIVO = 1
                                             order by COD_ESTADO";

                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// Metodo que elimina un empleado del padrón
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto AF_PadronEmpleados_Eliminar(int CodEmpresa, string cedula)
        {
            var response = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };
            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);
                {
                    const string query = @"delete AFI_PADRON WHERE CEDULA = @cedula";
                    connection.Execute(query, new { cedula = cedula });
                } 
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Metodo que obtiene el padrón de empleados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> AF_PadronEmpleados_Obtener(int CodEmpresa, bool exporta ,AfPadronEmpleadosFiltro filtros, FiltrosLazyLoadData tblFiltros)
        {
            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Description = "OK",
                Result = new TablasListaGenericaModel()
                {
                    lista = new List<AfPadronEmpleadosDto>(),
                    total = 0
                }
            };
            try
            {
                string conn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(conn);
                {
                    var query = $@"Select P.* , isnull(Pe.Descripcion,'No Localizado') as 'EstadoPersona'
		                            from AFI_PADRON P left join Socios S on S.CEDULA in(P.ID_ALTERNO, P.CEDULA)
		                            left join AFI_ESTADOS_PERSONA Pe on S.EstadoActual = Pe.COD_ESTADO WHERE 1 = 1 ";

                    //FILTROS
                    var where = "";
                    if (tblFiltros.sortField == "" || tblFiltros.sortField == null)
                    {
                        tblFiltros.sortField = "P.CEDULA";
                    }

                    if(filtros.estado != null && filtros.estado.Count > 0)
                    {
                        var estados = string.Join("','", filtros.estado);
                        where += $" AND S.EstadoActual IN ('{estados}') ";
                    }

                    if (filtros.institucion != null && filtros.institucion.Count > 0)
                    {
                        var instituciones = string.Join(",", filtros.institucion);
                        where += $" AND P.INSTITUCION IN ({instituciones}) ";
                    }

                    if(tblFiltros.filters != null)
                    {
                        var dictFilters = ((JObject)tblFiltros.filters).ToObject<Dictionary<string, object>>();

                        foreach (var filter in dictFilters)
                        {
                            // Cada filter.Value es un JObject con { value, matchMode }
                            var filtroObj = filter.Value as JObject;
                            if (filtroObj != null)
                            {
                                var valor = filtroObj["value"]?.ToString();
                                if (!string.IsNullOrEmpty(valor))
                                {
                                    where += $" AND P.{filter.Key} LIKE '%{valor}%' ";
                                }
                            }
                        }
                    }

                    if (filtros.ing_chk_fecha)
                    {
                        where += $" AND P.FECHA_INGRESO BETWEEN '{filtros.ing_fecha_inicio:yyyy-MM-dd}' AND '{filtros.ing_fecha_corte:yyyy-MM-dd}' ";
                    }

                    if (filtros.reg_chk_fecha)
                    {
                        where += $" AND P.REGISTRO_FECHA BETWEEN '{filtros.reg_fecha_inicio:yyyy-MM-dd}' AND '{filtros.reg_fecha_corte:yyyy-MM-dd}' ";
                    }

              

                    var qryCount = $"Select count(*) from ({query}) as T";
                    
                    response.Result.total = connection.Query<int>(qryCount).FirstOrDefault();

                    if (!exporta)
                    {
                        query = $"{query} {where} ORDER BY {tblFiltros.sortField} {(tblFiltros.sortOrder == 1 ? "ASC" : "DESC")} OFFSET {tblFiltros.pagina} ROWS FETCH NEXT {tblFiltros.paginacion} ROWS ONLY";
                    }
                    else
                    {
                        query = $"{query} {where} ORDER BY {tblFiltros.sortField} {(tblFiltros.sortOrder == 1 ? "ASC" : "DESC")}";
                    }

                        response.Result.lista = connection.Query<AfPadronEmpleadosDto>(query).ToList();
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
