using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_Beneficios_MotivosDB
    {
        private readonly IConfiguration _config;

        public frmAF_Beneficios_MotivosDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene lista lazy
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pagina"></param>
        /// <param name="paginacion"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDTO<BENE_MOTIVOSDataLista> BeneficiosMotivos_Obtener(int CodEmpresa, int? pagina, int? paginacion, string? filtro)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<BENE_MOTIVOSDataLista>();
            response.Result = new BENE_MOTIVOSDataLista();
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = "SELECT COUNT(*) FROM AFI_BENE_MOTIVOS";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " WHERE DESCRIPCION LIKE '%" + filtro + "%' OR COD_MOTIVO LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"SELECT * FROM AFI_BENE_MOTIVOS
                                         {filtro} 
                                        ORDER BY COD_MOTIVO
                                        {paginaActual}
                                        {paginacionActual} ";

                    response.Result.Lista = connection.Query<BENE_MOTIVOS>(query).ToList();

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
        /// Actualiza el detalle del motivo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDTO BeneficiosMotivos_Actualizar(int CodEmpresa, BENE_MOTIVOS request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    int activo = request.activo ? 1 : 0;
                    var query = $@"UPDATE AFI_BENE_MOTIVOS 
                                SET descripcion = '{request.descripcion}', activo = {activo}, 
                                modifica_fecha = GETDATE(), modifica_usuario = '{request.modifica_usuario}'
                                WHERE cod_motivo = '{request.cod_motivo}'";

                    resp.Code = connection.ExecuteAsync(query).Result;
                    resp.Description = "Ok";
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
        public ErrorDTO BeneficiosMotivos_Agregar(int CodEmpresa, BENE_MOTIVOS request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $"select COUNT(*) FROM AFI_BENE_MOTIVOS WHERE COD_MOTIVO = '{request.cod_motivo}'";
                    int existe = connection.Query<int>(query).FirstOrDefault();

                    if (existe > 0)
                    {
                        resp.Code = -1;
                        resp.Description = "Ya existe un motivo con el código: " + request.cod_motivo + ", por favor verifique";
                    }
                    else
                    {
                        int activo = request.activo ? 1 : 0;
                        query = $@"INSERT INTO AFI_BENE_MOTIVOS(cod_motivo,descripcion,activo, registro_fecha, registro_usuario)
                        values('{request.cod_motivo}','{request.descripcion}', {activo}, getdate(),'{request.registro_usuario}')";

                        resp.Code = connection.ExecuteAsync(query).Result;
                        resp.Description = "Ok";
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
        /// Elimina categoría apremiante
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ErrorDTO BeneficiosMotivos_Eliminar(int CodEmpresa, string id)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDTO resp = new ErrorDTO();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"DELETE AFI_BENE_MOTIVOS where COD_MOTIVO = '{id}'";

                    resp.Code = connection.ExecuteAsync(query).Result;
                    resp.Description = "Ok";
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