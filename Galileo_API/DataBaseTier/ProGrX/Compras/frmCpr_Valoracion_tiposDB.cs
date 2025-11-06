using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.CPR;
using PgxAPI.Models.ERROR;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmCpr_Valoracion_tiposDB
    {
        private readonly IConfiguration _config;

        public frmCpr_Valoracion_tiposDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<CprValoraEsquemaDtoList> EsquemaValoracion_Obtener(int CodEmpresa, int? pagina, int? paginacion, string? filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<CprValoraEsquemaDtoList>();
            response.Result = new CprValoraEsquemaDtoList();
            response.Code = 0;
            try
            {
                var query = "";
                string where = " ", paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(stringConn);
                {
                    if (filtro != null)
                    {
                        where = "where VAL_ID LIKE '%" + filtro + "%' OR descripcion LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $"select COUNT(*) from CPR_VALORA_ESQUEMA {where}";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    query = $"select VAL_ID, descripcion, Activo from CPR_VALORA_ESQUEMA {where} order by VAL_ID desc {paginaActual} {paginacionActual}";
                    response.Result.esquemas = connection.Query<CprValoraEsquemaDto>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.esquemas = null;
                response.Result.Total = 0;
            }
            return response;
        }

        public ErrorDto<CprValoraItemsDtoList> ValoracionItems_Obtener(int CodEmpresa, string val_id, int? pagina, int? paginacion, string? filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<CprValoraItemsDtoList>();
            response.Result = new CprValoraItemsDtoList();
            response.Code = 0;
            try
            {
                var query = "";
                string and = " ", paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(stringConn);
                {
                    if (filtro != null)
                    {
                        and = "and VAL_ITEM LIKE '%" + filtro + "%' OR descripcion LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $"select COUNT(*) from CPR_VALORA_ITEMS Where VAL_ID = '{val_id}' {and}";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    query = @$"select VAL_ITEM, descripcion, Peso from CPR_VALORA_ITEMS Where VAL_ID = '{val_id}'
                        {and} order by VAL_ITEM {paginaActual} {paginacionActual}";
                    response.Result.items = connection.Query<CprValoraItemsDto>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.items = null;
                response.Result.Total = 0;
            }
            return response;
        }

        public ErrorDto EsquemaValoracion_Upsert(int CodEmpresa, string usuario, CprValoraEsquemaDto request)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new()
            {
                Code = 0
            };
            var activo = 0;
            try
            {
                if (request.activo == true)
                {
                    activo = 1;
                }
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $"select isnull(count(*),0) as Existe from CPR_VALORA_ESQUEMA where VAL_ID = '{request.val_id}'";
                    int Existe = connection.Query<int>(query).FirstOrDefault();

                    if (Existe == 0)
                    {
                        query = @$"insert into CPR_VALORA_ESQUEMA(VAL_ID, descripcion, Activo, Registro_Fecha, Registro_Usuario) 
                            values('{request.val_id}','{request.descripcion}', {activo}, Getdate(), '{usuario}' )";
                        resp.Description = "Esquema agregado satisfactoriamente";
                    }
                    else
                    {
                        query = @$"update CPR_VALORA_ESQUEMA set descripcion = '{request.descripcion}',
                            Activo = {activo}, Modifica_Fecha = Getdate(), Modifica_Usuario = '{usuario}'
                            where VAL_ID = '{request.val_id}'";
                        resp.Description = "Esquema actualizado satisfactoriamente";
                    }
                    resp.Code = connection.ExecuteAsync(query).Result;
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto EsquemaValoracion_Delete(int CodEmpresa, string val_id)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $"delete from CPR_VALORA_ITEMS where VAL_ID = '{val_id}'";
                    resp.Code = connection.ExecuteAsync(query).Result;

                    query = $"delete from CPR_VALORA_ESQUEMA where VAL_ID = '{val_id}'";
                    resp.Code = connection.ExecuteAsync(query).Result;
                    resp.Description = "Esquema eliminado satisfactoriamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto ValoracionItems_Upsert(int CodEmpresa, string usuario, string val_id, CprValoraItemsDto request)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = @$"select isnull(count(*),0) as Existe from CPR_VALORA_ITEMS 
                        where VAL_ID = '{val_id}' and VAL_ITEM = '{request.val_item}'";
                    int Existe = connection.Query<int>(query).FirstOrDefault();

                    if (Existe == 0)
                    {
                        query = "insert into CPR_VALORA_ITEMS(VAL_ID, VAL_ITEM, descripcion, Peso, Registro_Fecha, Registro_Usuario)" +
                            "values(@Esquema,@Item,@Descripcion, @Peso, Getdate(), @Usuario )";
                        resp.Description = "Item agregado satisfactoriamente";
                    }
                    else
                    {
                        query = "update CPR_VALORA_ITEMS set descripcion = @Descripcion, " +
                            "Peso = @Peso, Modifica_Fecha = Getdate(), Modifica_Usuario = @Usuario " +
                            "where VAL_ID = @Esquema and VAL_ITEM = @Item";
                        resp.Description = "Item actualizado satisfactoriamente";
                    }
                    var parameters = new DynamicParameters();
                    parameters.Add("Item", request.val_item, DbType.String);
                    parameters.Add("Descripcion", request.descripcion, DbType.String);
                    parameters.Add("Peso", request.peso, DbType.Decimal);
                    parameters.Add("Esquema", val_id, DbType.String);
                    parameters.Add("Usuario", usuario, DbType.String);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto ValoracionItems_Delete(int CodEmpresa, string val_id, string val_item)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $"delete CPR_VALORA_ITEMS where VAL_ID = '{val_id}' and VAL_ITEM = '{val_item}'";
                    resp.Code = connection.ExecuteAsync(query).Result;
                    resp.Description = "Item eliminado satisfactoriamente";
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