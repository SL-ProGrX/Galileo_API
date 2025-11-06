using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.Security;
using PgxAPI.Models.TES;

namespace PgxAPI.DataBaseTier
{
    public class frmTES_ParametrosDB
    {
        private readonly IConfiguration? _config;
        MSecurityMainDb DBBitacora;

        public frmTES_ParametrosDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new MSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return DBBitacora.Bitacora(data);
        }

        /// <summary>
        /// Obtener lista de parametros
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> TES_Parametros_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TablasListaGenericaModel>
            {
                Code = 0,
                Result = new TablasListaGenericaModel()
            };
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(stringConn);
                {
                    query = $@"select COUNT(cod_parametro) from tes_parametros";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (!string.IsNullOrEmpty(filtros.filtro))
                    {
                        filtros.filtro = " WHERE ( cod_parametro LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR valor LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (filtros.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                    }

                    query = $@"select cod_parametro, descripcion, valor from tes_parametros 
                        {filtros.filtro} 
                        order by cod_parametro
                         {paginaActual}
                         {paginacionActual} ";

                    response.Result.lista = connection.Query<TesParametrosDto>(query).ToList();
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
        /// Actualizar valor de un parametro
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public ErrorDto TES_Parametros_Guardar(int CodEmpresa, string Usuario, TesParametrosDto param)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto()
            {
                Code = 0,
                Description = ""
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"update tes_parametros set valor = @valor
                    where cod_parametro = @parametro";
                    connection.Execute(query, new {
                        parametro = param.cod_parametro,
                        valor = param.valor
                    });

                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = Usuario.ToUpper(),
                        DetalleMovimiento = "Parametro de Tesorería : " + param.cod_parametro,
                        Movimiento = "MODIFICA - WEB",
                        Modulo = 9
                    });

                    response.Description = "Registro actualizado correctamente";
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