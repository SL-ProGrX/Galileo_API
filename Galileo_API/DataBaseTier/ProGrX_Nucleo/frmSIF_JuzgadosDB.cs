using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.SIF;

namespace Galileo.DataBaseTier
{
    public class FrmSifJuzgadosDB
    {
        private readonly PortalDB _portalDb;

        public FrmSifJuzgadosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Método para insertar un juzgado en la base de datos del cliente.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="juzgado"></param>
        /// <returns></returns>
        public ErrorDto SIF_Juzgados_Insertar(int CodEmpresa, JuzgadosDto juzgado)
        {
            var connectionString = _portalDb.ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto { Code = 0 };

            const string query = @"
                INSERT INTO sif_juzgados (
                    cod_juzgado, nombre, telefono_01, telefono_02, tel_fax, 
                    email_01, email_02, apto_postal, direccion, nombre_contacto, 
                    sitio_web, provincia, canton, distrito, activo, registro_fecha, registro_usuario
                )
                VALUES (
                    @CodJuzgado, @Nombre, @Telefono01, @Telefono02, @TelFax,
                    @Email01, @Email02, @AptoPostal, @Direccion, @NombreContacto,
                    @SitioWeb, @Provincia, @Canton, @Distrito, 1, GETDATE(), 'PEDRO'
                )";

            try
            {
                using var connection = new SqlConnection(connectionString);
                connection.Execute(query, new
                {
                    CodJuzgado = juzgado.cod_juzgado,
                    juzgado.nombre,
                    Telefono01 = juzgado.telefono_01,
                    Telefono02 = juzgado.telefono_02,
                    TelFax = juzgado.tel_fax,
                    Email01 = juzgado.email_01,
                    Email02 = juzgado.email_02,
                    AptoPostal = juzgado.apto_postal,
                    juzgado.direccion,
                    NombreContacto = juzgado.nombre_contacto,
                    SitioWeb = juzgado.sitio_web,
                    juzgado.provincia,
                    juzgado.canton,
                    juzgado.distrito
                });
            }
            catch (SqlException ex) when (ex.Message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase))
            {
                result.Code = -2;
                result.Description = "El código de juzgado ya existe";
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }


        /// <summary>
        /// Método para consultar el siguiente o anterior código de juzgado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="consecutivo"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<string> SIF_Juzgados_ConsultaAscDesc(int codEmpresa, string consecutivo, string tipo)
        {
            var connectionString = _portalDb.ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto<string>()
            {
                Code = 0,
                Description = "Ok",
                Result = string.Empty
            };
            const string baseQuery = "SELECT TOP 1 COD_JUZGADO FROM sif_juzgados";

            string query;

            if (tipo.Equals("desc", StringComparison.OrdinalIgnoreCase))
            {
                query = string.IsNullOrEmpty(consecutivo)
                    ? $"{baseQuery} ORDER BY COD_JUZGADO DESC"
                    : $"{baseQuery} WHERE COD_JUZGADO < @Codigo ORDER BY COD_JUZGADO DESC";
            }
            else
            {
                query = string.IsNullOrEmpty(consecutivo)
                    ? $"{baseQuery} ORDER BY COD_JUZGADO ASC"
                    : $"{baseQuery} WHERE COD_JUZGADO > @Codigo ORDER BY COD_JUZGADO ASC";
            }

            try
            {
                using var connection = new SqlConnection(connectionString);
                var result = connection.QueryFirstOrDefault<string>(query, new { Codigo = consecutivo });
                // Si no hay anterior/siguiente, se mantiene el actual
                response.Result = string.IsNullOrEmpty(result) ? consecutivo : result;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = consecutivo;
            }
            return response;
        }


        /// <summary>
        /// Método para obtener los datos de un juzgado por su código.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codJuzgado"></param>
        /// <returns></returns>
        public ErrorDto<JuzgadosDto> SIF_Juzgados_Obtener(int codEmpresa, string codJuzgado)
        {
            var connectionString = _portalDb.ObtenerDbConnStringEmpresa(codEmpresa);

            var response = new ErrorDto<JuzgadosDto>
            {
                Code = 0,
                Description = "Ok",
                Result = new JuzgadosDto
                {
                    activo = false, // o true según valor por defecto deseado
                    provincia = 1 // o el valor por defecto adecuado
                }
            };

            const string query = "SELECT * FROM sif_juzgados WHERE COD_JUZGADO = @CodJuzgado";

            try
            {
                using var connection = new SqlConnection(connectionString);
                response.Result = connection.QueryFirstOrDefault<JuzgadosDto>(query, new { CodJuzgado = codJuzgado });
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }


        /// <summary>
        /// Método para actualizar los datos de un juzgado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto SIF_Juzgados_Actualizar(int codEmpresa, JuzgadosDto request)
        {
            var connectionString = _portalDb.ObtenerDbConnStringEmpresa(codEmpresa);
            var result = new ErrorDto();

            const string query = @"
                UPDATE sif_juzgados SET
                    nombre = @Nombre,
                    telefono_01 = @Telefono01,
                    telefono_02 = @Telefono02,
                    tel_fax = @TelFax,
                    email_01 = @Email01,
                    email_02 = @Email02,
                    apto_postal = @AptoPostal,
                    direccion = @Direccion,
                    nombre_contacto = @NombreContacto,
                    sitio_web = @SitioWeb,
                    provincia = @Provincia,
                    canton = @Canton,
                    distrito = @Distrito,
                    activo = @Activo
                WHERE cod_juzgado = @CodJuzgado";

            try
            {
                using var connection = new SqlConnection(connectionString);
                var rows = connection.Execute(query, new
                {
                    CodJuzgado = request.cod_juzgado,
                    request.nombre,
                    Telefono01 = request.telefono_01,
                    Telefono02 = request.telefono_02,
                    TelFax = request.tel_fax,
                    Email01 = request.email_01,
                    Email02 = request.email_02,
                    AptoPostal = request.apto_postal,
                    request.direccion,
                    NombreContacto = request.nombre_contacto,
                    SitioWeb = request.sitio_web,
                    request.provincia,
                    request.canton,
                    request.distrito,
                    Activo = request.activo ? 1 : 0
                });

                result.Code = rows;
                result.Description = "Ok";
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }

            return result;
        }


        /// <summary>
        /// Método para eliminar un juzgado por su código.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codJuzgado"></param>
        /// <returns></returns>
        public ErrorDto SIF_Juzgados_Eliminar(int codEmpresa, string codJuzgado)
        {
            var connectionString = _portalDb.ObtenerDbConnStringEmpresa(codEmpresa);
            var result = new ErrorDto();
            const string query = "DELETE FROM sif_juzgados WHERE cod_juzgado = @CodJuzgado";
            try
            {
                using var connection = new SqlConnection(connectionString);
                var rows = connection.Execute(query, new { CodJuzgado = codJuzgado });
                result.Code = rows;
                result.Description = "Ok";
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;

        }


        /// <summary>
        /// Método para obtener la lista de todos los juzgados.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<JuzgadosDto>> SIF_JuzgadosLista_Obtener(int codEmpresa)
        {
            var connectionString = _portalDb.ObtenerDbConnStringEmpresa(codEmpresa);
            var result = new ErrorDto<List<JuzgadosDto>>();
            const string query = "SELECT * FROM sif_juzgados ORDER BY cod_juzgado ASC";
            try
            {
                using var connection = new SqlConnection(connectionString);
                result.Result = connection.Query<JuzgadosDto>(query).AsList();
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