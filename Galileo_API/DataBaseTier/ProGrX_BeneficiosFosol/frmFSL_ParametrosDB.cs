using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.FSL;

namespace PgxAPI.DataBaseTier
{
    public class frmFSL_ParametrosDB
    {
        private readonly IConfiguration _config;

        public frmFSL_ParametrosDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<FdlParametrosListaDto> FslParametros_Obtener(int CodCliente, string filtros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<FdlParametrosListaDto>();

            response.Result = new FdlParametrosListaDto();

            response.Result.Total = 0;
            try
            {

                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                string vFiltro = "";
                FdlParametrosFiltros filtro = JsonConvert.DeserializeObject<FdlParametrosFiltros>(filtros);


                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = "select count(*) " +
                        " from FSL_PARAMETROS";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtros != null)
                    {
                        vFiltro = " where COD_PARAMETRO LIKE '%" + filtro.filtro + "%' OR DETALLE LIKE '%" + filtro.filtro + "%' ";
                    }

                    if (filtro.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtro.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtro.paginacion + " ROWS ONLY ";
                    }

                    query = $@"select COD_PARAMETRO,DETALLE,TIPO,VALOR,NOTAS
                                         from FSL_PARAMETROS 
                                         {vFiltro} 
                                        order by COD_PARAMETRO DESC
                                        {paginaActual}
                                        {paginacionActual}; ";


                    response.Result.Comites = connection.Query<FdlParametrosDto>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "FslParametros_Obtener - " + ex.Message;
                response.Result = null;

            }
            return response;
        }

        public ErrorDto FslParametros_Actualizar(int CodCliente, FdlParametrosDto parametro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"update FSL_PARAMETROS set REGISTRO_USUARIO = '{parametro.registro_usuario}', REGISTRO_FECHA = getdate()
                                        ,valor = '{parametro.valor}' where cod_parametro = '{parametro.cod_parametro}' ";

                    var result = connection.Execute(query);

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;

        }


    }
}