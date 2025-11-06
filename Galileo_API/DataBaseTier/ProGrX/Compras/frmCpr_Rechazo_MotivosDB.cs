using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.CPR;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;


namespace PgxAPI.DataBaseTier
{
    public class frmCpr_Rechazo_MotivosDB
    {
        private readonly IConfiguration _config;

        public frmCpr_Rechazo_MotivosDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene la lista de motivos de rechazo
        /// </summary>
        /// <param name="CodCliente">Código del cliente</param>
        /// <param name="vFiltros">Filtros en formato JSON</param>
        /// <returns>Lista de motivos de rechazo</returns>
        public ErrorDto<CprRechazosMotivosLista> cprRechazoMotivoLista_Obtener(int CodCliente, string vFiltros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            FiltrosLazyLoadData filtro = JsonConvert.DeserializeObject<FiltrosLazyLoadData>(vFiltros);
            var response = new ErrorDto<CprRechazosMotivosLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new CprRechazosMotivosLista()
                {
                    total = 0,
                    lista = new List<CprRechazosMotivosDto>()
                }
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    //Busco Total
                    query = $@"select COUNT(COD_RECHAZO) from CPR_RECHAZO_TIPOS ";
                    response.Result.total = connection.QueryFirstOrDefault<int>(query);

                    if (filtro.filtro != null)
                    {
                        filtro.filtro = " WHERE COD_RECHAZO LIKE '%" + filtro.filtro + "%' OR descripcion LIKE '%" + filtro.filtro + "%' ";
                    }

                    if (filtro.sortField == "" || filtro.sortField == null)
                    {
                        filtro.sortField = "COD_RECHAZO";
                    }

                    query = $@"select COD_RECHAZO, descripcion, Activo from CPR_RECHAZO_TIPOS 
                                         {filtro.filtro} 
                                        ORDER BY {filtro.sortField} {(filtro.sortOrder == 0 ? "DESC" : "ASC")}
                                        OFFSET {filtro.pagina} ROWS 
                                         FETCH NEXT {filtro.paginacion} ROWS ONLY  ";

                    response.Result.lista = connection.Query<CprRechazosMotivosDto>(query).ToList();
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
        /// Guarda un motivo de rechazo
        /// </summary>
        /// <param name="CodCliente">Código del cliente</param>
        /// <param name="motivo">Motivo de rechazo a guardar</param>
        /// <returns>Resultado de la operación</returns>    
        public ErrorDto cprRechazoMotivo_Guardar(int CodCliente, CprRechazosMotivosDto motivo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //Verifico si el motivo ya existe
                    var query = $@"select COUNT(COD_RECHAZO) from CPR_RECHAZO_TIPOS where UPPER(COD_RECHAZO) = @motivo ";
                    var count = connection.QueryFirstOrDefault<int>(query, new { motivo = motivo.cod_rechazo.ToUpper() });

                    if (motivo.isNew)
                    {
                        if (count > 0)
                        {
                            result.Code = -2;
                            result.Description = "El motivo de rechazo con el código " + motivo.cod_rechazo + " ya existe.";
                        }
                        else
                        {
                            result = cprRechazoMotivo_Insertar(CodCliente, motivo);
                        }

                    }
                    else if (count == 0 && !motivo.isNew)
                    {
                        result.Code = -3;
                        result.Description = "El motivo de rechazo con el código " + motivo.cod_rechazo + " no existe.";
                    }
                    else
                    {
                        result = cprRechazoMotivo_Actualizar(CodCliente, motivo);
                    }

                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }


        /// <summary>
        /// Inserta un nuevo motivo de rechazo
        /// </summary>
        /// <param name="CodCliente">Código del cliente</param>
        /// <param name="motivo">Motivo de rechazo a insertar</param>
        /// <returns>Resultado de la operación</returns>
        private ErrorDto cprRechazoMotivo_Insertar(int CodCliente, CprRechazosMotivosDto motivo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto error = new ErrorDto();
            error.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    int activo = motivo.activo == true ? 1 : 0;
                    var query = $@"INSERT INTO [dbo].[CPR_RECHAZO_TIPOS]
                                       ([COD_RECHAZO]
                                       ,[DESCRIPCION]
                                       ,[ACTIVO]
                                       ,[REGISTRO_FECHA]
                                       ,[REGISTRO_USUARIO]
                                       )
                                 VALUES
                                       ('{motivo.cod_rechazo}'
                                       ,'{motivo.descripcion}'
                                       ,{activo}
                                       ,getDate()
                                       ,'{motivo.modifica_usuario}'
                                      )";
                    connection.Execute(query);


                }
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }

            return error;
        }


        /// <summary>
        /// Actualiza un motivo de rechazo existente
        /// </summary>
        /// <param name="CodCliente">Código del cliente</param>
        /// <param name="motivo">Motivo de rechazo a actualizar</param>
        /// <returns>Resultado de la operación</returns>    
        private ErrorDto cprRechazoMotivo_Actualizar(int CodCliente, CprRechazosMotivosDto motivo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto error = new ErrorDto();
            error.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    int activo = motivo.activo == true ? 1 : 0;
                    var query = $@"UPDATE [dbo].[CPR_RECHAZO_TIPOS]
                                   SET [DESCRIPCION] = '{motivo.descripcion}'
                                      ,[ACTIVO] = {activo}
                                      ,[MODIFICA_FECHA] = getDate()
                                      ,[MODIFICA_USUARIO] = '{motivo.modifica_usuario}'
                                 WHERE [COD_RECHAZO] = '{motivo.cod_rechazo}' ";
                    connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }

            return error;
        }


        /// <summary>
        /// Elimina un motivo de rechazo
        /// </summary>
        /// <param name="CodCliente">Código del cliente</param>
        /// <param name="cod_rechazo">Código del motivo de rechazo a eliminar</param>
        /// <returns>Resultado de la operación</returns>
        public ErrorDto cprRechazoMotivo_Eliminar(int CodCliente, string cod_rechazo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto error = new ErrorDto();
            error.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"delete from CPR_RECHAZO_TIPOS where COD_RECHAZO = '{cod_rechazo}' ";
                    connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }

            return error;
        }

    }
}