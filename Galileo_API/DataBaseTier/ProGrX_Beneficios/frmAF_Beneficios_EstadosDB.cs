using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_Beneficios_EstadosDB
    {
        private readonly IConfiguration _config;

        public frmAF_Beneficios_EstadosDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene la lista lazy  
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pagina"></param>
        /// <param name="paginacion"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDTO<BENE_ESTADODataLista> BeneficiosEstados_Obtener(int CodEmpresa, int? pagina, int? paginacion, string? filtro)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<BENE_ESTADODataLista>();
            response.Result = new BENE_ESTADODataLista();
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = "SELECT COUNT(*) FROM AFI_BENE_ESTADOS";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " WHERE COD_ESTADO LIKE '%" + filtro + "%' OR DESCRIPCION LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"SELECT * FROM AFI_BENE_ESTADOS
                                         {filtro} 
                                        ORDER BY COD_ESTADO
                                        {paginaActual}
                                        {paginacionActual} ";

                    response.Result.Lista = connection.Query<BENE_ESTADO>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.Total = 0;
            }
            return response;
        }


        /// <summary>
        /// Actualiza un estado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDTO BeneficiosEstados_Actualizar(int CodEmpresa, BENE_ESTADO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    int activo = request.activo ? 1 : 0;
                    int ini = request.p_inicia ? 1 : 0;
                    int fin = request.p_finaliza ? 1 : 0;

                    var query = $@"UPDATE AFI_BENE_ESTADOS 
                                SET descripcion = '{request.descripcion}', activo = {activo}, 
                                orden = '{request.orden}', p_inicia = {ini}, p_finaliza = {fin},
                                modifica_fecha = GETDATE(), modifica_usuario = '{request.modifica_usuario}', 
                                proceso = '{request.proceso}' 
                                WHERE cod_estado = '{request.cod_estado}'";

                    resp.Code = connection.ExecuteAsync(query).Result;
                    resp.Description = "Estado actualizado correctamente";
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
        /// Agrega una categoría apremiante
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDTO BeneficiosEstados_Agregar(int CodEmpresa, BENE_ESTADO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $"select COUNT(*) FROM AFI_BENE_ESTADOS WHERE COD_ESTADO = '{request.cod_estado}'";
                    int existe = connection.Query<int>(query).FirstOrDefault();

                    if (existe > 0)
                    {
                        resp.Code = -1;
                        resp.Description = "Ya existe un estado con el código: " + request.cod_estado + ", por favor verifique";
                    }
                    else
                    {
                        int activo = request.activo ? 1 : 0;
                        int ini = request.p_inicia ? 1 : 0;
                        int fin = request.p_finaliza ? 1 : 0;

                        query = $@"INSERT INTO AFI_BENE_ESTADOS(cod_estado,descripcion,activo, orden, p_inicia, p_finaliza, registro_fecha, registro_usuario, proceso)
                        values('{request.cod_estado}','{request.descripcion}', {activo}, '{request.orden}', {ini},{fin}, getdate(),'{request.registro_usuario}', '{request.proceso}')";

                        resp.Code = connection.ExecuteAsync(query).Result;
                        resp.Description = "Estado agregado correctamente";
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
        /// Elimina un estado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ErrorDTO BeneficiosEstados_Eliminar(int CodEmpresa, string id)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"DELETE AFI_BENE_ESTADOS where COD_ESTADO = '{id}'";

                    resp.Code = connection.ExecuteAsync(query).Result;
                    resp.Description = "Estado eliminado correctamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }
    }
}