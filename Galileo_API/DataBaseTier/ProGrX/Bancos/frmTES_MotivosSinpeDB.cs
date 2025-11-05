using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PgxAPI.DataBaseTier.ProGrX.Bancos
{
    public class frmTES_MotivosSinpeDB
    {
        private readonly IConfiguration? _config;

        public frmTES_MotivosSinpeDB(IConfiguration? config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene los motivos de SINPE para una empresa específica.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<TesMotivosSinpeLista> TES_MotivoSinpe_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<TesMotivosSinpeLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new TesMotivosSinpeLista()
                {
                    total = 0,
                    lista = new List<TesMotivosSinpeDTO>()
                }
            };
            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    //Busco Total
                    query = $@"select COUNT(cod_motivo) from SINPE_MOTIVOS";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( cod_motivo LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR usuario_registro LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "cod_motivo";
                    }

                    query = $@"select cod_motivo, descripcion, usuario_registro from SINPE_MOTIVOS 
                                     {filtros.filtro} 
                                     order by {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                         OFFSET {filtros.pagina} ROWS 
                                         FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                    response.Result.lista = connection.Query<TesMotivosSinpeDTO>(query).ToList();
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
        /// Obtiene los motivos de SINPE para exportar a Excel o CSV.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<List<TesMotivosSinpeDTO>> TES_MotivoSinpeExportar_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<TesMotivosSinpeDTO>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<TesMotivosSinpeDTO>()
            };
            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( cod_motivo LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR usuario_registro LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "cod_motivo";
                    }

                    query = $@"select cod_motivo, descripcion, usuario_registro from SINPE_MOTIVOS 
                                     {filtros.filtro} 
                                     order by {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")} ";
                    response.Result = connection.Query<TesMotivosSinpeDTO>(query).ToList();
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
        /// Método para guardar un motivo de SINPE.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="motivo"></param>
        /// <returns></returns>
        public ErrorDTO TES_MotivoSinpe_Guardar(int CodEmpresa, string usuario, TesMotivosSinpeDTO motivo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Verifico si existe usuario
                    var qUsuario = $@"select count(cod_motivo) from SINPE_MOTIVOS where cod_motivo = @cod_motivo ";
                    int existe = connection.QueryFirstOrDefault<int>(qUsuario, new { cod_motivo = motivo.cod_motivo});
             
                    if (motivo.isNew)
                    {
                        if (existe > 0)
                        {
                            result.Code = -2;
                            result.Description = $"El Motivo con el código {motivo.cod_motivo} ya existe.";
                        }
                        else
                        {
                            result = TES_MotivoSinpe_Insertar(CodEmpresa, usuario, motivo);
                        }
                    }
                    else if (existe == 0 && !motivo.isNew)
                    {
                        result.Code = -2;
                        result.Description = $"El Motivo con el código {motivo.cod_motivo} no existe.";
                    }
                    else
                    {
                        result = TES_MotivoSinpe_Actualizar(CodEmpresa, usuario, motivo);
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
        /// Inserta un nuevo motivo de SINPE en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="motivo"></param>
        /// <returns></returns>
        private ErrorDTO TES_MotivoSinpe_Insertar(int CodEmpresa, string usuario, TesMotivosSinpeDTO motivo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"INSERT INTO SINPE_MOTIVOS (cod_motivo, descripcion, usuario_registro, fecha_registro) 
                                   VALUES (@cod_motivo, @descripcion, @usuario_registro, getDate())";
                    connection.Execute(query, new { cod_motivo = motivo.cod_motivo, descripcion = motivo.descripcion, usuario_registro = usuario });
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
        /// Actualiza un motivo de SINPE existente en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="motivo"></param>
        /// <returns></returns>
        private ErrorDTO TES_MotivoSinpe_Actualizar(int CodEmpresa, string usuario, TesMotivosSinpeDTO motivo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"UPDATE SINPE_MOTIVOS 
                                   SET descripcion = @descripcion, usuario_actualiza = @usuario_actualiza, fecha_actualiza = getDate() 
                                   WHERE cod_motivo = @cod_motivo";
                    connection.Execute(query, new { cod_motivo = motivo.cod_motivo, descripcion = motivo.descripcion, usuario_actualiza = usuario });
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
        /// Elimina un motivo de SINPE de la base de datos.<!---->
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_motivo"></param>
        /// <returns></returns>
        public ErrorDTO TES_MotivoSinpe_Eliminar(int CodEmpresa, string usuario, int cod_motivo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                //NOTA: Borrar después de validar la tabla de referencias.
                if(cod_motivo != 99)
                {
                    result.Code = -1;
                    result.Description = "El Borrado aun no esta disponible para este código";
                    return result;
                }
               

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"DELETE FROM SINPE_MOTIVOS WHERE cod_motivo = @cod_motivo";
                    connection.Execute(query, new { cod_motivo });
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }
    }
}
