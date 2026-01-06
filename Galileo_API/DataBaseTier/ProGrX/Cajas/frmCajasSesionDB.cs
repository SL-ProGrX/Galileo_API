using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using Microsoft.Data.SqlClient;
using System.Data;
namespace Galileo_API.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasSesionDb
    {
        private readonly IConfiguration _config;
        private readonly MProGrXAuxiliarDB _AuxiliarDB;

        public FrmCajasSesionDb(IConfiguration config)
        {
            _config = config;
            _AuxiliarDB = new MProGrXAuxiliarDB(config);
        }

        // Método extraído para la consulta de sesión
        private ErrorDto<CajasSesionDto> ConsultarSesion(SqlConnection connection, string usuario, string identificacion)
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
            {
                return new ErrorDto<CajasSesionDto>
                {
                    Code = 0,
                    Description = "Ok",
                    Result = result
                };
            }
            else
            {
                return new ErrorDto<CajasSesionDto>
                {
                    Code = -2,
                    Description = "No se encontró sesión activa.",
                    Result = null
                };
            }
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
                EjecutarConConexion(stringConn, connection =>
                {
                    response = ConsultarSesion(connection, usuario, identificacion);
                });
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        // Nuevo método extraído para obtener la apertura activa
        private int ObtenerAperturaActiva(SqlConnection connection, string caja)
        {
            string sqlApertura = @"SELECT TOP 1 cod_apertura FROM cajas_aperturas_main WHERE cod_caja = @Caja AND estado = 'A'";
            return connection.QueryFirstOrDefault<int>(
                sqlApertura,
                new { Caja = caja }
            );
        }

        // Nuevo método extraído para iniciar la sesión de la caja
        private void IniciarSesionCaja(SqlConnection connection, string caja, string usuario, int tipoId, string cedula, string nombre)
        {
            int aperturaActiva = ObtenerAperturaActiva(connection, caja);

            var parameters = new DynamicParameters();
            parameters.Add("@Caja", caja, DbType.String);
            parameters.Add("@Usuario", usuario, DbType.String);
            parameters.Add("@Apertura", aperturaActiva, DbType.Int32);
            parameters.Add("@TipoId", tipoId, DbType.Int32);
            parameters.Add("@Identificacion", cedula, DbType.String);
            parameters.Add("@Nombre", nombre, DbType.String);

            connection.QueryFirstOrDefault<CajaSesionDto>(
                "spCajas_Sesion_Inicia",
                parameters,
                commandType: CommandType.StoredProcedure);
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
        public ErrorDto Cajas_Sesion_Inicia(int codEmpresa, string caja, string usuario, int tipoId, string cedula, string nombre)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok",
            };

            try
            {
                EjecutarConConexion(stringConn, connection =>
                {
                    IniciarSesionCaja(connection, caja, usuario, tipoId, cedula, nombre);
                });
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        // Nuevo método extraído para ejecutar el procedimiento de finalización de sesión
        private static void EjecutarFinalizacionSesion(SqlConnection connection, int sesionId, string usuario)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@SesionId", sesionId, DbType.Int32);
            parameters.Add("@Usuario", usuario, DbType.String);

            connection.Execute(
                "spCajas_Sesion_Finaliza",
                parameters,
                commandType: CommandType.StoredProcedure
            );
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
                EjecutarConConexion(stringConn, connection =>
                {
                    EjecutarFinalizacionSesion(connection, sesionId, usuario);
                });
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
                EjecutarConConexion(stringConn, connection =>
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@SesionId", sesionId, DbType.Int32);

                    response.Result = connection.Query<CajasSesionMovimientosDto>(
                        "spCajas_Sesion_Aplicaciones",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    ).ToList();
                });
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Método para obtener los tipos de identificacion
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TiposIdentificacion_Obtener(int CodCliente)
        {
            return _AuxiliarDB.TiposIdentificacion_Obtener(CodCliente);
        }

        // Cambia el método a static
        private static void EjecutarConConexion(string stringConn, Action<SqlConnection> accion)
        {
            using var connection = new SqlConnection(stringConn);
            accion(connection);
        }


    }

}