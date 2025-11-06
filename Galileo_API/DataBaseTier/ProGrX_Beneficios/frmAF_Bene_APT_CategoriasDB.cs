using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_Bene_APT_CategoriasDB
    {
        private readonly IConfiguration _config;

        public frmAF_Bene_APT_CategoriasDB(IConfiguration config)
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
        public ErrorDto<AptCategoriasDataLista> CategoriasApremiante_Obtener(int CodEmpresa, int? pagina, int? paginacion, string? filtro)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<AptCategoriasDataLista>();
            response.Result = new AptCategoriasDataLista();
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = "SELECT COUNT(*) FROM AFI_BENE_APT_CATEGORIAS";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " WHERE DESCRIPCION LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"SELECT * FROM AFI_BENE_APT_CATEGORIAS
                                         {filtro} 
                                        ORDER BY ID_APT_CATEGORIA
                                        {paginaActual}
                                        {paginacionActual} ";

                    response.Result.Lista = connection.Query<AptCategorias>(query).ToList();

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
        /// Actualiza una categor�a apremiante
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CategoriasApremiante_Actualizar(int CodEmpresa, AptCategorias request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    int activo = request.activo ? 1 : 0;
                    var query = $@"UPDATE AFI_BENE_APT_CATEGORIAS 
                                SET descripcion = '{request.descripcion}', activo = {activo},
                                modifica_fecha = GETDATE(), modifica_usuario = '{request.modifica_usuario}'
                                WHERE ID_APT_CATEGORIA = {request.id_apt_categoria}";

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
        /// Agrega una categor�a apremiante
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CategoriasApremiante_Agregar(int CodEmpresa, AptCategorias request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    int activo = request.activo ? 1 : 0;
                    var query = $@"INSERT INTO AFI_BENE_APT_CATEGORIAS(descripcion,activo, registro_fecha, registro_usuario)
                        values('{request.descripcion}', {activo}, GETDATE(),'{request.registro_usuario}')";

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
        /// Elimina una categor�a apremiante
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public ErrorDto CategoriasApremiante_Eliminar(int CodEmpresa, int id)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"DELETE AFI_BENE_APT_CATEGORIAS where ID_APT_CATEGORIA = {id}";

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