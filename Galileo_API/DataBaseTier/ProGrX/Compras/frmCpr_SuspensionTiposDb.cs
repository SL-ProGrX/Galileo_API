using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.CPR;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmCpr_SuspensionTiposDB
    {
        private readonly IConfiguration _config;

        public frmCpr_SuspensionTiposDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<TiposSuspensionDtoList> TiposSuspension_ObtenerTodos(int CodEmpresa, int? pagina, int? paginacion, string? filtro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TiposSuspensionDtoList>();
            response.Result = new TiposSuspensionDtoList();
            response.Code = 0;
            try
            {
                var query = "";
                string where = " ", paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(stringConn);
                {
                    if (filtro != null)
                    {
                        where = "where COD_SUSPENSION LIKE '%" + filtro + "%' OR descripcion LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $"select COUNT(*) from CXP_SUSPENSION_TIPOS {where}";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    query = $"select COD_SUSPENSION,descripcion,ACTIVA from CXP_SUSPENSION_TIPOS {where} order by COD_SUSPENSION {paginaActual} {paginacionActual}";
                    response.Result.Suspensiones = connection.Query<TiposSuspensionDto>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.Suspensiones = null;
                response.Result.Total = 0;
            }
            return response;
        }

        private ErrorDto TiposSuspension_Agregar(int CodEmpresa, TiposSuspensionDto tiposSuspensionDto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    int activo = tiposSuspensionDto.Activa == true ? 1 : 0;
                    var query = $@"insert into CXP_SUSPENSION_TIPOS(COD_SUSPENSION,descripcion,ACTIVA, REGISTRO_FECHA, REGISTRO_USUARIO) 
                                values('{tiposSuspensionDto.Cod_Suspension}', '{tiposSuspensionDto.Descripcion}', {activo}, getDate(), '{tiposSuspensionDto.Registro_Usuario}')";

                    connection.Execute(query);
                    resp.Description = "Registro agregado satisfactoriamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        private ErrorDto TiposSuspension_Actualizar(int CodEmpresa, TiposSuspensionDto tiposSuspensionDto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    int activa = tiposSuspensionDto.Activa == true ? 1 : 0;
                    var query = $@"update CXP_SUSPENSION_TIPOS set descripcion = '{tiposSuspensionDto.Descripcion}', 
                                    ACTIVA = {activa} where COD_SUSPENSION = '{tiposSuspensionDto.Cod_Suspension}' ";
                    connection.Execute(query);
                    resp.Description = "Registro actualizado satisfactoriamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto TiposSuspension_Eliminar(int CodEmpresa, string codSuspension)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"delete CXP_SUSPENSION_TIPOS where COD_SUSPENSION = '{codSuspension}' ";

                    connection.Execute(query);
                    resp.Description = "Registro eliminado satisfactoriamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto TiposSuspension_Guardar(int CodEmpresa, TiposSuspensionDto tiposSuspensionDto)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new()
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Valido que no exista el codigo de suspension
                    var query = $@"select count(*) from CXP_SUSPENSION_TIPOS where COD_SUSPENSION = '{tiposSuspensionDto.Cod_Suspension}'";
                    int count = connection.Query<int>(query).FirstOrDefault();
                    if (count == 0)
                    {
                        resp = TiposSuspension_Agregar(CodEmpresa, tiposSuspensionDto);
                    }
                    else
                    {
                        resp = TiposSuspension_Actualizar(CodEmpresa, tiposSuspensionDto);
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
    }
}
