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
        private readonly PortalDB _portalDb;
        private readonly MProGrXAuxiliarDB _AuxiliarDB;

        public FrmCajasSesionDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _AuxiliarDB = new MProGrXAuxiliarDB(config);
        }

        private static CajasSesionDto? ConsultarSesion(
            SqlConnection connection,
            int sesionId,
            string caja,
            string usuario,
            int apertura,
            string identificacion)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@SesionId", sesionId, DbType.Int32);
            parameters.Add("@Caja", caja, DbType.String);
            parameters.Add("@Usuario", usuario, DbType.String);
            parameters.Add("@Apertura", apertura, DbType.Int32);

            var result = connection.QueryFirstOrDefault<CajasSesionDto>(
                "spCajas_Sesion_Info",
                parameters,
                commandType: CommandType.StoredProcedure);

            if (result == null && !string.IsNullOrWhiteSpace(identificacion))
            {
                const string sql = @"
                    SELECT TOP 1 *
                    FROM CAJAS_SESION
                    WHERE cod_usuario = @Usuario
                      AND estado = 1
                      AND identificacion = @Identificacion";

                result = connection.QueryFirstOrDefault<CajasSesionDto>(
                    sql,
                    new { Usuario = usuario, Identificacion = identificacion });
            }

            return result;
        }

        /// <summary>
        /// Obtiene los datos de la sesión de cajas.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="sesionId"></param>
        /// <param name="caja"></param>
        /// <param name="usuario"></param>
        /// <param name="apertura"></param>
        /// <param name="identificacion"></param>
        /// <returns></returns>
        public ErrorDto<CajasSesionDto> Cajas_Sesion_Obtener(
            int codEmpresa,
            int sesionId,
            string caja,
            string usuario,
            int apertura,
            string identificacion)
        {
            var response = DbHelper.WithConn(_portalDb, codEmpresa, connection =>
            {
                return ConsultarSesion(connection, sesionId, caja, usuario, apertura, identificacion);
            });

            if (response.Code == 0 && response.Result == null)
            {
                response.Code = -2;
                response.Description = "No se encontró sesión activa.";
            }

            return new ErrorDto<CajasSesionDto>
            {
                Code = response.Code,
                Description = response.Description,
                Result = response.Result!
            };
        }

        private static int ObtenerAperturaActiva(SqlConnection connection, string caja)
        {
            const string sqlApertura = @"
                SELECT TOP 1 cod_apertura
                FROM cajas_aperturas_main
                WHERE cod_caja = @Caja
                  AND estado = 'A'";

            return connection.QueryFirstOrDefault<int>(
                sqlApertura,
                new { Caja = caja }
            );
        }

        private static int IniciarSesionCaja(
            SqlConnection connection,
            string caja,
            string usuario,
            int apertura,
            int tipoId,
            string cedula,
            string nombre)
        {
            int aperturaActiva = apertura > 0 ? apertura : ObtenerAperturaActiva(connection, caja);

            var parameters = new DynamicParameters();
            parameters.Add("@Caja", caja, DbType.String);
            parameters.Add("@Usuario", usuario, DbType.String);
            parameters.Add("@Apertura", aperturaActiva, DbType.Int32);
            parameters.Add("@TipoId", tipoId, DbType.Int32);
            parameters.Add("@Identificacion", cedula, DbType.String);
            parameters.Add("@Nombre", nombre, DbType.String);

            return connection.QueryFirstOrDefault<int>(
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
        public ErrorDto Cajas_Sesion_Inicia(int codEmpresa, string caja, string usuario, int apertura, int tipoId, string cedula, string nombre)
        {
            var response = DbHelper.WithConn(_portalDb, codEmpresa, connection =>
            {
                return IniciarSesionCaja(connection, caja, usuario, apertura, tipoId, cedula, nombre);
            });

            if (response.Code == 0)
            {
                return new ErrorDto { Code = response.Result, Description = response.Description };
            }

            return new ErrorDto { Code = response.Code, Description = response.Description };
        }

        private static CajasSesionFinalizaResultDto EjecutarFinalizacionSesion(SqlConnection connection, int sesionId, string usuario)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@SesionId", sesionId, DbType.Int32);
            parameters.Add("@Usuario", usuario, DbType.String);

            return connection.QueryFirstOrDefault<CajasSesionFinalizaResultDto>(
                "spCajas_Sesion_Finaliza",
                parameters,
                commandType: CommandType.StoredProcedure
            ) ?? new CajasSesionFinalizaResultDto();
        }

        /// <summary>
        /// Finaliza la sesión de la caja.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="sesionId"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CajasSesionFinalizaResultDto> Cajas_Sesion_Finaliza(int codEmpresa, int sesionId, string usuario)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, connection =>
            {
                return EjecutarFinalizacionSesion(connection, sesionId, usuario);
            });
        }

        /// <summary>
        /// Obtiene los movimientos registrados en la sesión de caja.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="sesionId"></param>
        /// <returns></returns>
        public ErrorDto<List<CajasSesionMovimientosDto>> Cajas_Sesion_Movimientos(int codEmpresa, int sesionId)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@SesionId", sesionId, DbType.Int32);

                return connection.Query<CajasSesionMovimientosDto>(
                    "spCajas_Sesion_Aplicaciones",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();
            });
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
    }

}
