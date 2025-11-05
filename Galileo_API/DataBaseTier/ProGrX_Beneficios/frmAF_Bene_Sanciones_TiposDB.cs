using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_Bene_Sanciones_TiposDB
    {
        private readonly IConfiguration _config;

        public frmAF_Bene_Sanciones_TiposDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Metodo para obtener los tipos de sanciones
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<AfTipoSancionesDTOLista> afBeneTipoSancionObtener(int CodCliente, string filtros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<AfTipoSancionesDTOLista>();
            response.Result = new AfTipoSancionesDTOLista();

            AfiTipoSancionfiltros filtro = JsonConvert.DeserializeObject<AfiTipoSancionfiltros>(filtros);

            response.Result.total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = "SELECT COUNT(TIPO_SANCION) from AFI_BENE_SANCIONES_TIPOS ";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();

                    string vFiltro = "";
                    if (filtro.filtro != null)
                    {
                        vFiltro = " where TIPO_SANCION LIKE '%" + filtro.filtro + "%'" +
                            " OR DESCRIPCION LIKE '%" + filtro.filtro + "%' " +
                             " OR REGISTRO_USUARIO LIKE '%" + filtro.filtro + "%' " +
                            " OR CODIGO_COBRO LIKE '%" + filtro.filtro + "%' ";
                    }

                    if (filtro.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtro.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtro.paginacion + " ROWS ONLY ";
                    }

                    query = $@"SELECT  [TIPO_SANCION]
                                          ,[DESCRIPCION]
                                          ,[CODIGO_COBRO]
                                          ,[PLAZO_MAXIMO]
                                          ,[ACTIVO]
                                          ,[REGISTRO_FECHA]
                                          ,[REGISTRO_USUARIO]
                                          ,[MODIFICA_FECHA]
                                          ,[MODIFICA_USUARIO]
                                      FROM AFI_BENE_SANCIONES_TIPOS 
                                         {vFiltro} 
                                        order by TIPO_SANCION
                                        {paginaActual}
                                        {paginacionActual} ";


                    response.Result.lista = connection.Query<AfTipoSancionesDTO>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.total = 0;
            }
            return response;
        }

        /// <summary>
        /// Metodo para insertar o actualizar un tipo de sancion
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="tipo_sancion"></param>
        /// <returns></returns>
        public ErrorDTO AfBeneTipoSancion_Insertar(int CodCliente, AfTipoSancionesDTO tipo_sancion)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //Valido si existe
                    var query = $@"select isnull(count(*),0) as Existe from AFI_BENE_SANCIONES_TIPOS 
                          where TIPO_SANCION = '{tipo_sancion.tipo_sancion}' ";
                    var existe = connection.Query<int>(query).FirstOrDefault();

                    if (existe == 0)
                    {
                        int activo = tipo_sancion.activo ? 1 : 0;
                        query = $@"INSERT INTO AFI_BENE_SANCIONES_TIPOS
                                           (
                                            TIPO_SANCION
                                           ,DESCRIPCION
                                           ,CODIGO_COBRO
                                            ,PLAZO_MAXIMO
                                           ,ACTIVO
                                           ,REGISTRO_FECHA
                                           ,REGISTRO_USUARIO
                                           )
                                     VALUES
                                           (
                                           '{tipo_sancion.tipo_sancion}'
                                           ,'{tipo_sancion.descripcion}'
                                           ,'{tipo_sancion.codigo_cobro}'
                                            ,'{tipo_sancion.plazo_maximo}'
                                           ,{activo}
                                           ,getdate()
                                           ,'{tipo_sancion.registro_usuario}'
                                            )";
                        resp.Code = connection.Execute(query, tipo_sancion);
                    }
                    else
                    {
                        resp = AfBeneTipoSancion_Actualizar(CodCliente, tipo_sancion);
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

        /// <summary>
        /// Metodo para actualizar un tipo de sancion
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="tipo_sancion"></param>
        /// <returns></returns>
        public ErrorDTO AfBeneTipoSancion_Actualizar(int CodCliente, AfTipoSancionesDTO tipo_sancion)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    int activo = tipo_sancion.activo ? 1 : 0;
                    var query = $@"UPDATE AFI_BENE_SANCIONES_TIPOS
                                   SET [DESCRIPCION] = '{tipo_sancion.descripcion}'
                                      ,[CODIGO_COBRO] = '{tipo_sancion.codigo_cobro}'
                                        ,[PLAZO_MAXIMO] = '{tipo_sancion.plazo_maximo}'
                                      ,[REGISTRO_USUARIO] = '{tipo_sancion.registro_usuario}'
                                      ,[ACTIVO] = {activo}
                                      ,[MODIFICA_FECHA] = getdate()
                                      ,[MODIFICA_USUARIO] = '{tipo_sancion.modifica_usuario}'
                                 WHERE TIPO_SANCION = {tipo_sancion.tipo_sancion} ";
                    resp.Code = connection.Execute(query, tipo_sancion);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Metodo para eliminar un tipo de sancion
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="tipo_sancion"></param>
        /// <returns></returns>
        public ErrorDTO AfBeneTipoSancion_Eliminar(int CodCliente, int tipo_sancion)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"DELETE FROM AFI_BENE_SANCIONES_TIPOS WHERE TIPO_SANCION = '{tipo_sancion}' ";
                    resp.Code = connection.Execute(query, new { tipo_sancion });
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Metodo para obtener las retenciones
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDTO<List<BeneListaRetencion>> BeneRetenciones_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<BeneListaRetencion>>();
            response.Result = new List<BeneListaRetencion>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT * FROM [dbo].[vAFI_Bene_Retenciones_Catalogo]";
                    response.Result = connection.Query<BeneListaRetencion>(query).ToList();
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