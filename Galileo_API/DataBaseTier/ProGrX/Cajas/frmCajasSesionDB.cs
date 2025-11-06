using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Cajas;
using System.Data;
namespace PgxAPI.DataBaseTier.ProGrX.Cajas
{
    public class frmCajas_SesionDB
    {
        private readonly IConfiguration _config;
        private readonly mProGrX_AuxiliarDB _AuxiliarDB;

        public frmCajas_SesionDB(IConfiguration config)
        {
            _config = config;
            _AuxiliarDB = new mProGrX_AuxiliarDB(config);


        }
        /// <summary>
        /// Obtiene los datos de la sesion
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="identificacion"></param>
        /// <returns></returns>

        public ErrorDto<CajasSesionDto> Cajas_Sesion_Obtener(int codEmpresa, string usuario, string identificacion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto<CajasSesionDto>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var sql = @"SELECT TOP 1 *
                        FROM CAJAS_SESION
                        WHERE cod_usuario = @Usuario
                          AND estado = 1
                          AND identificacion = @Identificacion";

                    var result = connection.QueryFirstOrDefault<CajasSesionDto>(
                        sql,
                        new { Usuario = usuario, Identificacion = identificacion }
                    );

                    if (result != null)
                        response.Result = result;
                    else
                    {
                        response.Code = -2;
                        response.Description = "No se encontró sesión activa.";
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Inicia la sesion de la caja
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="caja"></param>
        /// <param name="usuario"></param>
        /// <param name="apertura"></param>
        /// <param name="tipoId"></param>
        /// <param name="cedula"></param>
        /// <param name="nombre"></param>
        /// <returns></returns>
        public ErrorDto Cajas_Sesion_Inicia(int codEmpresa, string caja, string usuario,int tipoId, string cedula, string nombre)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok",
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string sqlApertura = @"SELECT TOP 1 cod_apertura FROM cajas_aperturas_main WHERE cod_caja = @Caja AND estado = 'A'";

                    int aperturaActiva = connection.QueryFirstOrDefault<int>(
                        sqlApertura,
                        new { Caja = caja }
                    );

                    var parameters = new DynamicParameters();
                    parameters.Add("@Caja", caja, DbType.String);
                    parameters.Add("@Usuario", usuario, DbType.String);
                    parameters.Add("@Apertura", aperturaActiva, DbType.Int32);
                    parameters.Add("@TipoId", tipoId, DbType.Int32);
                    parameters.Add("@Identificacion", cedula, DbType.String);
                    parameters.Add("@Nombre", nombre, DbType.String);

                    var result = connection.QueryFirstOrDefault<CajaSesionDto>(
                        "spCajas_Sesion_Inicia",
                        parameters,
                        commandType: CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Finaliza la sesion de la caja
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="sesionId"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Cajas_Sesion_Finaliza(int codEmpresa, int sesionId, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok",
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@SesionId", sesionId, DbType.Int32);
                    parameters.Add("@Usuario", usuario, DbType.String);

                    var result = connection.Execute(
                        "spCajas_Sesion_Finaliza",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    );

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Movimientos de la Caja
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="sesionId"></param>
        /// <returns></returns>
        public ErrorDto<List<CajasSesionMovimientosDto>> Cajas_Sesion_Movimientos(int codEmpresa, int sesionId)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto<List<CajasSesionMovimientosDto>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CajasSesionMovimientosDto>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@SesionId", sesionId, DbType.Int32);

                    response.Result = connection.Query<CajasSesionMovimientosDto>(
                        "spCajas_Sesion_Aplicaciones",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    ).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Metodo para obtener los tipos de identificacion
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TiposIdentificacion_Obtener(int CodCliente)
        {
            return _AuxiliarDB.TiposIdentificacion_Obtener(CodCliente);
        }


    }

}