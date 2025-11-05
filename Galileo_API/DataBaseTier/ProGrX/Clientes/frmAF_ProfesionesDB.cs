using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier.ProGrX.Clientes
{
    public class frmAF_ProfesionesDB
    {
        private readonly IConfiguration _config;
        private readonly mSecurityMainDb _mSecurity;

        public frmAF_ProfesionesDB(IConfiguration config)
        {
            _config = config;
            _mSecurity = new mSecurityMainDb(_config);
        }

        public ErrorDTO Bitacora(BitacoraInsertarDTO data)
        {
            return _mSecurity.Bitacora(data);
        }

        /// <summary>
        /// Obtener lista de profesiones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<TablasListaGenericaModel> AF_Profesiones_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<TablasListaGenericaModel>
            {
                Code = 0,
                Description = "Ok",
                Result = new TablasListaGenericaModel()
                {
                    total = 0,
                    lista = new List<DropDownListaGenericaModel>()
                }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var queryT = "select count(cod_profesion) from afi_profesiones";
                    response.Result.total = connection.Query<int>(queryT).FirstOrDefault();

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( cod_profesion LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "cod_profesion";
                    }

                    var query = $@"select cod_profesion as item,descripcion from afi_profesiones 
                                        {filtros.filtro} 
                                     order by {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                         OFFSET {filtros.pagina} ROWS 
                                         FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                    response.Result.lista = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// Guardar profesión, guarda la información de la linea
        /// si es Insert devuelve el codigo, sino devuelve 0
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Info"></param>
        /// <returns></returns>
        public ErrorDTO AF_Profesiones_Guardar(int CodEmpresa, string Usuario, string Codigo, string Descripcion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "Ok"
            };
            var query = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    // Reemplaza la línea de validación original por la siguiente:
                    if (Codigo == "Nuevo")
                    {
                        query = @"insert into afi_profesiones(descripcion) values(@Descripcion)";

                        connection.Execute(query,
                            new
                            {
                                Descripcion
                            }
                        );

                        Bitacora(new BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = Usuario.ToUpper(),
                            DetalleMovimiento = "Profesion : " + Descripcion,
                            Movimiento = "Registra - WEB",
                            Modulo = 9
                        });

                        var queryU = "select max(cod_profesion) as ultimo from afi_profesiones where descripcion = @Descripcion";
                        int ultimo = connection.QueryFirstOrDefault<int>(queryU,
                            new
                            {
                                Descripcion
                            }
                        );

                        response.Code = ultimo;
                    }
                    else
                    {
                        query = @"update afi_profesiones set descripcion = @Descripcion 
                            where cod_profesion = @Codigo";

                        connection.Execute(query,
                            new
                            {
                                Codigo,
                                Descripcion
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
        /// Eliminar profesión
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="CodSociedad"></param>
        /// <returns></returns>
        public ErrorDTO AF_Profesiones_Eliminar(int CodEmpresa, string Usuario, int Codigo, string Descripcion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "delete afi_profesiones where cod_profesion = @Codigo";
                    connection.Execute(query, new { Codigo });

                    Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = Usuario.ToUpper(),
                        DetalleMovimiento = "Profesion : " + Descripcion,
                        Movimiento = "Elimina - WEB",
                        Modulo = 9
                    });
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
