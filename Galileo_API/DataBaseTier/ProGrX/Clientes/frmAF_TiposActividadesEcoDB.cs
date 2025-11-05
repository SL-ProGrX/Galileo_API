using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;

namespace PgxAPI.DataBaseTier.ProGrX.Clientes
{
    public class frmAF_TiposActividadesEcoDB
    {
        private readonly IConfiguration _config;

        public frmAF_TiposActividadesEcoDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtener la lista de tipos de actividades económicas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<AF_TiposActividadesEcoLista> AF_TiposActividadesEco_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<AF_TiposActividadesEcoLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new AF_TiposActividadesEcoLista()
                {
                    total = 0,
                    lista = new List<AF_TiposActividadesEcoDTO>()
                }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var queryT = "select count(cod_actividad) from AFI_ACTIVIDADES_ECO";
                    response.Result.total = connection.Query<int>(queryT).FirstOrDefault();

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( cod_actividad LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "cod_actividad";
                    }

                    var query = $@"select cod_actividad,descripcion, activa from AFI_ACTIVIDADES_ECO
                                        {filtros.filtro} 
                                     order by {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")}";
                    if (filtros.paginacion != 0 || filtros.paginacion == null)
                    {
                        query += $" OFFSET {filtros.pagina} ROWS FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                    }
                    response.Result.lista = connection.Query<AF_TiposActividadesEcoDTO>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.total = 0;
                response.Result.lista = null;
            }

            return response;
        }

        /// <summary>
        /// Guardar un tipo de actividad económica (inserta o actualiza)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Info"></param>
        /// <returns></returns>
        public ErrorDto AF_TiposActividadesEco_Guardar(int CodEmpresa, string Usuario, AF_TiposActividadesEcoDTO Info)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select isnull(count(*),0) as Existe from AFI_ACTIVIDADES_ECO where cod_actividad = @CodActividad";
                    int existe = connection.QueryFirstOrDefault<int>(query,
                        new
                        {
                            CodActividad = Info.cod_actividad
                        }
                    );

                    if (existe == 0)
                    {
                        query = @"insert AFI_ACTIVIDADES_ECO(cod_actividad,descripcion,activa)
                            values( @CodActividad, @Descripcion, @Activa)";

                        connection.Execute(query,
                            new
                            {
                                CodActividad = Info.cod_actividad,
                                Descripcion = Info.descripcion,
                                Activa = Info.activa ? 1 : 0
                            }
                        );
                    }
                    else
                    {
                        query = @"update AFI_ACTIVIDADES_ECO set descripcion = @Descripcion, activa = @Activa
                            where cod_actividad = @CodActividad";

                        connection.Execute(query,
                            new
                            {
                                CodActividad = Info.cod_actividad,
                                Descripcion = Info.descripcion,
                                Activa = Info.activa ? 1 : 0
                            }
                        );
                    }
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
        /// Eliminar un tipo de actividad económica
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="CodActividad"></param>
        /// <returns></returns>
        public ErrorDto AF_TiposActividadesEco_Eliminar(int CodEmpresa, string Usuario, string CodActividad)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "delete AFI_ACTIVIDADES_ECO where cod_actividad = @CodActividad";
                    connection.Execute(query, new { CodActividad });
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
        /// Obtener la lista de sub actividades económicas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodActividad"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<AF_TiposActividadesEcoLista> AF_TiposActividadesEco_SubActividad_Obtener(int CodEmpresa, string CodActividad, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<AF_TiposActividadesEcoLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new AF_TiposActividadesEcoLista()
                {
                    total = 0,
                    lista = new List<AF_TiposActividadesEcoDTO>()
                }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var queryT = "select count(COD_SUB_ACT) from AFI_ACTIVIDADES_ECO_SUB where cod_actividad = @CodActividad";
                    response.Result.total = connection.Query<int>(queryT, new { CodActividad }).FirstOrDefault();

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " AND ( COD_SUB_ACT LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "COD_SUB_ACT";
                    }

                    
                    var query = $@"select COD_SUB_ACT, descripcion, activa, cod_actividad from AFI_ACTIVIDADES_ECO_SUB 
                             where cod_actividad = @CodActividad
                                        {filtros.filtro} 
                                     order by {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")} ";
                    if (filtros.paginacion != 0 || filtros.paginacion == null)
                    {
                        query += $" OFFSET {filtros.pagina} ROWS FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                    }
                    response.Result.lista = connection.Query<AF_TiposActividadesEcoDTO>(query, new { CodActividad }).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.total = 0;
                response.Result.lista = null;
            }

            return response;
        }

        /// <summary>
        /// Guardar una sub actividad económica (inserta o actualiza)
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Info"></param>
        /// <returns></returns>
        public ErrorDto AF_TiposActividadesEco_SubActividad_Guardar(int CodEmpresa, string Usuario, AF_TiposActividadesEcoDTO Info)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"select isnull(count(*),0) as Existe from AFI_ACTIVIDADES_ECO_SUB 
                        where cod_actividad = @CodActividad  and COD_SUB_ACT = @CodSubAct";
                    int existe = connection.QueryFirstOrDefault<int>(query,
                        new
                        {
                            CodActividad = Info.cod_actividad,
                            CodSubAct = Info.cod_sub_act
                        }
                    );

                    if (existe == 0)
                    {
                        query = @"insert AFI_ACTIVIDADES_ECO_SUB(cod_actividad,COD_SUB_ACT,descripcion,activa)
                            values( @CodActividad, @CodSubAct, @Descripcion, @Activa)";

                        connection.Execute(query,
                            new
                            {
                                CodActividad = Info.cod_actividad,
                                CodSubAct = Info.cod_sub_act,
                                Descripcion = Info.descripcion,
                                Activa = Info.activa ? 1 : 0
                            }
                        );
                    }
                    else
                    {
                        query = @"update AFI_ACTIVIDADES_ECO_SUB set descripcion = @Descripcion, activa = @Activa
                            where cod_actividad = @CodActividad and COD_SUB_ACT = @CodSubAct";

                        connection.Execute(query,
                            new
                            {
                                CodActividad = Info.cod_actividad,
                                CodSubAct = Info.cod_sub_act,
                                Descripcion = Info.descripcion,
                                Activa = Info.activa ? 1 : 0
                            }
                        );
                    }
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
        /// Eliminar una sub actividad económica
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="CodActividad"></param>
        /// <param name="CodSubAct"></param>
        /// <returns></returns>
        public ErrorDto AF_TiposActividadesEco_SubActividad_Eliminar(int CodEmpresa, string Usuario, string CodActividad, string CodSubAct)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "delete AFI_ACTIVIDADES_ECO_SUB where cod_actividad = @CodActividad and COD_SUB_ACT = @CodSubAct";
                    connection.Execute(query, new { CodActividad, CodSubAct });
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
