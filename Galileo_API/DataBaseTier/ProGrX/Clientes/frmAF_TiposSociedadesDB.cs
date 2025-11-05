using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;

namespace PgxAPI.DataBaseTier.ProGrX.Clientes
{
    public class frmAF_TiposSociedadesDB
    {
        private readonly IConfiguration _config;
        private readonly mSecurityMainDb _mSecurity;

        public frmAF_TiposSociedadesDB(IConfiguration config)
        {
            _config = config;
            _mSecurity = new mSecurityMainDb(_config);
        }

        public ErrorDTO Bitacora(BitacoraInsertarDTO data)
        {
            return _mSecurity.Bitacora(data);
        }

        /// <summary>
        /// Obtener tipos de sociedades
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<AF_TiposSociedadesLista> AF_TiposSociedades_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<AF_TiposSociedadesLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new AF_TiposSociedadesLista()
                {
                    total = 0,
                    lista = new List<AF_TiposSociedadesDTO>()
                }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var queryT = "select count(Cod_Sociedad) from AFI_Sociedades_Tipos";
                    response.Result.total = connection.Query<int>(queryT).FirstOrDefault();

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( Cod_Sociedad LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "Cod_Sociedad";
                    }

                    var query = $@"select Cod_Sociedad,descripcion,Activa from AFI_Sociedades_Tipos
                                        {filtros.filtro} 
                                     order by {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                         OFFSET {filtros.pagina} ROWS 
                                         FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                    response.Result.lista = connection.Query<AF_TiposSociedadesDTO>(query).ToList();
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
        /// Guardar tipos de sociedades
        /// Insertar o Modificar según si existe o no el código
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Info"></param>
        /// <returns></returns>
        public ErrorDTO AF_TiposSociedades_Guardar(int CodEmpresa, string Usuario, AF_TiposSociedadesDTO Info)
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
                    var query = "select isnull(count(*),0) as Existe from AFI_Sociedades_Tipos where Cod_Sociedad = @CodSociedad";
                    int existe = connection.QueryFirstOrDefault<int>(query,
                        new
                        {
                            CodSociedad = Info.cod_Sociedad
                        }
                    );

                    if (existe == 0)
                    {
                        query = @"insert into AFI_Sociedades_Tipos(Cod_Sociedad,descripcion,Activa) 
                            values( @CodSociedad, @Descripcion, @Activa)";

                        connection.Execute(query,
                            new
                            {
                                CodSociedad = Info.cod_Sociedad,
                                Descripcion = Info.descripcion,
                                Activa = Info.activa ? 1 : 0    
                            }
                        );

                        Bitacora(new BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = Usuario.ToUpper(),
                            DetalleMovimiento = "Sociedades : " + Info.cod_Sociedad,
                            Movimiento = "Registra - WEB",
                            Modulo = 9
                        });
                    }
                    else
                    {
                        query = @"update AFI_Sociedades_Tipos set descripcion = @Descripcion, Activa = @Activa
                            where Cod_Sociedad = @CodSociedad";

                        connection.Execute(query,
                            new
                            {
                                CodSociedad = Info.cod_Sociedad,
                                Descripcion = Info.descripcion,
                                Activa = Info.activa ? 1 : 0
                            }
                        );

                        Bitacora(new BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = Usuario.ToUpper(),
                            DetalleMovimiento = "Sociedades : " + Info.cod_Sociedad,
                            Movimiento = "Modifica - WEB",
                            Modulo = 9
                        });
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
        /// Eliminar tipo de sociedad
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="CodSociedad"></param>
        /// <returns></returns>
        public ErrorDTO AF_TiposSociedades_Eliminar(int CodEmpresa, string Usuario, string CodSociedad)
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
                    var query = "delete AFI_Sociedades_Tipos where Cod_Sociedad = @CodSociedad";
                    connection.Execute(query, new { CodSociedad });

                    Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = Usuario.ToUpper(),
                        DetalleMovimiento = "Sociedades : " + CodSociedad,
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
