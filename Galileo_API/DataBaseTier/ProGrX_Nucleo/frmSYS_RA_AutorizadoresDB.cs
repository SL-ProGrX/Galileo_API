using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.DataBaseTier
{
    public class FrmSysRaAutorizadoresDB
    {
        private readonly PortalDB _portalDb;

        public FrmSysRaAutorizadoresDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Metodo para consultar el consecutivo ascendente o descendente de los autorizadores
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="consecutivo"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<int> Frm_Sys_Ra_Autorizadores_ConsultaAscDesc(int codEmpresa, int consecutivo, string tipo)
        {
            var connectionString = _portalDb.ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<int>
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };

            const string baseQuery = "SELECT TOP 1 AUTORIZADOR_ID FROM SYS_EXP_AUTORIZADORES";
            string query;

            if (tipo.Equals("desc", StringComparison.OrdinalIgnoreCase))
            {
                query = consecutivo == 0
                    ? $"{baseQuery} ORDER BY AUTORIZADOR_ID DESC"
                    : $"{baseQuery} WHERE AUTORIZADOR_ID < @Consecutivo ORDER BY AUTORIZADOR_ID DESC";
            }
            else
            {
                query = $"{baseQuery} WHERE AUTORIZADOR_ID > @Consecutivo ORDER BY AUTORIZADOR_ID ASC";
            }

            try
            {
                using var connection = new SqlConnection(connectionString);
                var result = connection.QueryFirstOrDefault<int>(query, new { Consecutivo = consecutivo });

                response.Result = result == 0 || result == consecutivo ? consecutivo : result;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Metodo para obtener un autorizador por su ID
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codAutorizador"></param>
        /// <returns></returns>
        public ErrorDto<AutorizadoresExpDto> Frm_Sys_Ra_Autorizadores_Obtener(int codEmpresa, int codAutorizador)
        {
            var connectionString = _portalDb.ObtenerDbConnStringEmpresa(codEmpresa);
            const string query = "SELECT * FROM SYS_EXP_AUTORIZADORES WHERE AUTORIZADOR_ID = @Id";

            var response = new ErrorDto<AutorizadoresExpDto>
            {
                Code = 0,
                Description = "Ok",
                Result = new AutorizadoresExpDto { autorizador_id = codAutorizador }
            };

            try
            {
                using var connection = new SqlConnection(connectionString);
                response.Result = connection.QueryFirstOrDefault<AutorizadoresExpDto>(query, new { Id = codAutorizador });
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Metodo para insertar un nuevo autorizador
        /// </summary>
        /// <param name="codCliente"></param>
        /// <param name="autorizador"></param>
        /// <returns></returns>
        public ErrorDto Frm_Sys_Ra_Autorizadores_Insertar(int codCliente, AutorizadoresExpDto autorizador)
        {
            var connectionString = _portalDb.ObtenerDbConnStringEmpresa(codCliente);
            var response = new ErrorDto { Code = 0, Description = "Ok" };

            const string getNextIdQuery = "SELECT ISNULL(MAX(AUTORIZADOR_ID), 0) + 1 FROM SYS_EXP_AUTORIZADORES";
            const string insertQuery = @"
                INSERT INTO SYS_EXP_AUTORIZADORES (
                    AUTORIZADOR_ID, AUT_USUARIO, AUT_CLAVE, NOTAS, ESTADO,
                    REGISTRO_FECHA, REGISTRO_USUARIO
                )
                VALUES (
                    @AutorizadorId, @AutUsuario, @AutClave, @Notas, @Estado,
                    GETDATE(), 'PEDRO'
                )";

            try
            {
                using var connection = new SqlConnection(connectionString);
                autorizador.autorizador_id = connection.QueryFirst<int>(getNextIdQuery);

                connection.Execute(insertQuery, new
                {
                    AutorizadorId = autorizador.autorizador_id,
                    AutUsuario = autorizador.aut_usuario,
                    AutClave = autorizador.aut_clave,
                    autorizador.notas,
                    autorizador.estado
                });
            }
            catch (SqlException ex) when (ex.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase))
            {
                response.Code = -2;
                response.Description = "El código de autorizador ya existe";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Metodo para actualizar un autorizador existente
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Frm_Sys_Ra_Autorizadores_Actualizar(int codEmpresa, AutorizadoresExpDto request)
        {
            var connectionString = _portalDb.ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto { Code = 0, Description = "Ok" };

            const string updateQuery = @"
                UPDATE SYS_EXP_AUTORIZADORES
                SET
                    AUT_USUARIO = @AutUsuario,
                    AUT_CLAVE = @AutClave,
                    NOTAS = @Notas,
                    ESTADO = @Estado
                WHERE AUTORIZADOR_ID = @AutorizadorId";

            try
            {
                using var connection = new SqlConnection(connectionString);
                var rows = connection.Execute(updateQuery, new
                {
                    AutorizadorId = request.autorizador_id,
                    AutUsuario = request.aut_usuario,
                    AutClave = request.aut_clave,
                    request.notas,
                    request.estado
                });

                response.Code = rows;
                response.Description = "Actualización correcta";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Método para obtener la lista de todos los juzgados.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<AutorizadoresExpDto>> Frm_Sys_Ra_AutorizadoresLista_Obtener(int codEmpresa)
        {
            var connectionString = _portalDb.ObtenerDbConnStringEmpresa(codEmpresa);
            var result = new ErrorDto<List<AutorizadoresExpDto>>();
            const string query = "SELECT * FROM SYS_EXP_AUTORIZADORES ORDER BY AUT_USUARIO ASC";
            try
            {
                using var connection = new SqlConnection(connectionString);
                result.Result = connection.Query<AutorizadoresExpDto>(query).AsList();
                result.Code = 0;
                result.Description = "Ok";
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