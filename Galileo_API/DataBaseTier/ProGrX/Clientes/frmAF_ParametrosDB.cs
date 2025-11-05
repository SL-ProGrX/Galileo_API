using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Clientes;

namespace PgxAPI.DataBaseTier.ProGrX.Clientes
{
    public class frmAF_ParametrosDB
    {
        private readonly IConfiguration _config;
        private readonly mSecurityMainDb _mSecurity;

        public frmAF_ParametrosDB(IConfiguration config)
        {
            _config = config;
            _mSecurity = new mSecurityMainDb(_config);
        }

        public ErrorDTO Bitacora(BitacoraInsertarDTO data)
        {
            return _mSecurity.Bitacora(data);
        }

        /// <summary>
        /// Obtener lista de parámetros
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<AF_ParametrosLista> AF_Parametros_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<AF_ParametrosLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new AF_ParametrosLista()
                {
                    total = 0,
                    lista = new List<AF_ParametrosDTO>()
                }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var querySP = "exec spAFIParametros";
                    connection.Execute(querySP);

                    var queryT = "select count(cod_parametro) from afi_parametros";
                    response.Result.total = connection.Query<int>(queryT).FirstOrDefault();

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( cod_parametro LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR valor LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "cod_parametro";
                    }

                    var query = $@"select cod_parametro,descripcion,valor from afi_parametros 
                                        {filtros.filtro} 
                                     order by {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                         OFFSET {filtros.pagina} ROWS 
                                         FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                    response.Result.lista = connection.Query<AF_ParametrosDTO>(query).ToList();
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
        /// Actualizar parámetro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Codigo"></param>
        /// <param name="Valor"></param>
        /// <returns></returns>
        public ErrorDTO AF_Parametros_Actualizar(int CodEmpresa, string Usuario, string Codigo, string Valor)
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
                    var query = "update afi_parametros set valor = @Valor where cod_parametro = @Codigo";
                    connection.Execute(query, new { Codigo, Valor } );

                    Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = Usuario.ToUpper(),
                        DetalleMovimiento = "Parametro General de Afiliación : " + Codigo,
                        Movimiento = "Modifica - WEB",
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
