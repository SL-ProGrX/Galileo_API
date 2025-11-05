using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;

namespace PgxAPI.DataBaseTier.ProGrX.Clientes
{
    public class frmAF_DepartamentosDB
    {
        private readonly IConfiguration _config;

        public frmAF_DepartamentosDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtener lista de instituciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_DepartamentosInstituciones_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"Select cod_institucion as 'item',descripcion from instituciones";
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
        /// Obtener lista de departamentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="institucion"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<AF_DepartamentosLista> AF_DepartamentosLista_Obtener(int CodEmpresa, int institucion ,FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<AF_DepartamentosLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new AF_DepartamentosLista()
                {
                    total = 0,
                    lista = new List<AF_DepartamentosDTO>()
                }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var queryT = $@"select COUNT(cod_departamento) from AFDepartamentos
                                    where cod_institucion = @institucion ";
                    response.Result.total = connection.QueryFirstOrDefault<int>(queryT, new { institucion });

                    var query = $@"select cod_departamento,descripcion, cod_institucion from AFDepartamentos 
                        where cod_institucion = @institucion";

                    if (filtros.filtro != null && filtros.filtro != "")
                    {
                        filtros.filtro = $@" AND (
                                    descripcion LIKE '%{filtros.filtro}%'
                                    OR cod_departamento LIKE '%{filtros.filtro}%')";
                        query += filtros.filtro;
                    } 
                    query += " order by cod_departamento ";

                    if (filtros.pagina != null)
                    {
                        query += $@" OFFSET {filtros.pagina} ROWS
                                FETCH NEXT {filtros.paginacion} ROWS ONLY";
                    }
                    response.Result.lista = connection.Query<AF_DepartamentosDTO>(query, new { institucion, filtro = filtros.filtro }).ToList();
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
        /// Obtener lista de secciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="institucion"></param>
        /// <param name="departamento"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<AF_SeccionesLista> AF_DepartamentosSecciones_Obtener(int CodEmpresa, int institucion, string departamento, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<AF_SeccionesLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new AF_SeccionesLista()
                {
                    total = 0,
                    lista = new List<AF_SeccionesDTO>()
                }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var queryT = $@"select COUNT(cod_seccion) from AfSecciones
                        where cod_institucion = @institucion and cod_departamento = @departamento";
                    response.Result.total = connection.QueryFirstOrDefault<int>(queryT, new { institucion, departamento });

                    var query = $@"select cod_seccion,descripcion, cod_institucion, cod_departamento from AfSecciones
                        where cod_institucion = @institucion and cod_departamento = @departamento";

                    if (filtros.filtro != null && filtros.filtro != "")
                    {
                        filtros.filtro = $@" AND (
                                    descripcion LIKE '%{filtros.filtro}%'
                                    OR cod_seccion LIKE '%{filtros.filtro}%')";
                        query += filtros.filtro;
                    }
                    query += " order by cod_seccion ";

                    if (filtros.pagina != null)
                    {
                        query += $@" OFFSET {filtros.pagina} ROWS
                                FETCH NEXT {filtros.paginacion} ROWS ONLY";
                    }

                    response.Result.lista = connection.Query<AF_SeccionesDTO>(query, new { institucion, departamento, filtro = filtros.filtro }).ToList();
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
        /// Actualizar o agregar un departamento según corresponde, valida si este existe o no
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Info"></param>
        /// <returns></returns>
        public ErrorDto AF_Departamentos_Guardar(int CodEmpresa, AF_DepartamentosDTO Info)
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
                    var query = $@"select isnull(count(*),0) as Existe from AfDepartamentos 
                        where cod_institucion = @institucion and cod_departamento = @departamento";
                    int existe = connection.QueryFirstOrDefault<int>(query,
                        new 
                        {
                            institucion = Info.cod_institucion,
                            departamento = Info.cod_departamento 
                        }
                    );

                    if (existe == 0)
                    {
                        query = $@"insert AfDepartamentos(cod_institucion,cod_departamento,descripcion) 
                            values ( @institucion, @departamento, @descripcion)";

                        connection.Execute(query, 
                            new 
                            {
                                institucion = Info.cod_institucion,
                                departamento = Info.cod_departamento,
                                descripcion = Info.descripcion
                            }
                        );

                        var query2 = $@"insert AfSecciones(cod_institucion,cod_departamento,cod_seccion,descripcion) 
                            values( @institucion, @departamento , '', 'Sin Descripción')";

                        connection.Execute(query2, 
                            new
                            {
                                institucion = Info.cod_institucion,
                                departamento = Info.cod_departamento
                            }
                        );
                    } 
                    else 
                    {
                        query = $@"update AfDepartamentos set descripcion = @descripcion 
                            where cod_institucion = @institucion and cod_departamento = @departamento";

                        connection.Execute(query, 
                            new
                            {
                                institucion = Info.cod_institucion,
                                departamento = Info.cod_departamento,
                                descripcion = Info.descripcion
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
        /// Actualizar o agregar una sección según corresponde, valida si este existe o no
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Info"></param>
        /// <returns></returns>
        public ErrorDto AF_DepartamentosSecciones_Guardar(int CodEmpresa, AF_SeccionesDTO Info)
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
                    var query = $@"select isnull(count(*),0) as Existe from afSecciones 
                        where cod_institucion = @institucion and cod_departamento = @departamento and cod_seccion = @seccion";
                    int existe = connection.QueryFirstOrDefault<int>(query,
                        new
                        {
                            institucion = Info.cod_institucion,
                            departamento = Info.cod_departamento,
                            seccion = Info.cod_seccion
                        }
                    );

                    if (existe == 0)
                    {
                        query = $@"Insert AfSecciones(cod_institucion, cod_departamento, cod_seccion, descripcion) 
                            values( @institucion, @departamento, @seccion, @descripcion)";

                        connection.Execute(query, 
                            new
                            {
                                institucion = Info.cod_institucion,
                                departamento = Info.cod_departamento,
                                seccion = Info.cod_seccion,
                                descripcion = Info.descripcion
                            }
                        );
                    }
                    else
                    {
                        query = $@"update AfSecciones set descripcion = @descripcion 
                            where cod_institucion = @institucion and cod_departamento = @departamento and cod_seccion = @seccion";

                        connection.Execute(query, 
                            new
                            {
                                institucion = Info.cod_institucion,
                                departamento = Info.cod_departamento,
                                seccion = Info.cod_seccion,
                                descripcion = Info.descripcion
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
        /// Borrar un departamento
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Institucion"></param>
        /// <param name="Departamento"></param>
        /// <returns></returns>
        public ErrorDto AF_Departamentos_Borrar(int CodEmpresa, int Institucion, string Departamento)
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
                    var query = $@"delete AfSecciones 
                        where cod_institucion = @Institucion and cod_departamento = @departamento";
                    connection.Execute(query,
                        new
                        {
                            Institucion,
                            Departamento
                        }
                    );

                    var query2 = $@"delete AfDepartamentos 
                        where cod_institucion = @institucion and cod_departamento = @departamento";
                    connection.Execute(query2,
                        new
                        {
                            Institucion,
                            Departamento
                        }
                    );
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
        /// Borrar una sección
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Institucion"></param>
        /// <param name="Departamento"></param>
        /// <param name="Seccion"></param>
        /// <returns></returns>
        public ErrorDto AF_DepartamentosSecciones_Borrar(int CodEmpresa, int Institucion, string Departamento, string Seccion)
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
                    var query = $@"delete AfSecciones 
                        where cod_institucion = @institucion and cod_departamento = @departamento and cod_seccion = @seccion";
                    connection.Execute(query,
                        new
                        {
                            Institucion,
                            Departamento,
                            Seccion
                        }
                    );
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
