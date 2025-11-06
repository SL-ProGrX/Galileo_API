using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;

namespace PgxAPI.DataBaseTier.ProGrX.Bancos
{
    public class frmTES_AutorizaChKeyDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 9; // Modulo de Tesoreria
        private readonly MTesoreria MTesoreria;

        public frmTES_AutorizaChKeyDB(IConfiguration config)
        {
            _config = config;
            MTesoreria = new MTesoreria(config);
        }

        /// <summary>
        /// Metodo para cambiar la clave de autorización de cheques
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Tes_AutorizaChKey_Cambiar(AutorizaChKeyData usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(usuario.CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Clave cambiada correctamente"
            };
            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    //Verifica si la clave actual es correcta
                    query = @"SELECT isnull(count(*),0) as existe from tes_autorizaciones 
                                where nombre = @usuario and clave = @claveActual ";
                    var existe = connection.QueryFirstOrDefault<int>(query, new
                    {
                        usuario = usuario.usuarioLogin,
                        claveActual = usuario.claveActual                    });

                    if (existe == 0)
                    {
                        response.Code = -1; // Indica que la clave actual no es correcta
                        response.Description = "La clave actual no es valida o no fue localizada...";
                        return response;
                    }

                    // Verifica si la clave nueva y la confirmación son iguales
                    if (usuario.claveNueva != usuario.claveConfirmar)
                    {
                        response.Code = -1; // Indica que las claves no coinciden
                        response.Description = "La clave nueva y la confirmación no coinciden.";
                        return response;
                    }

                    // Actualiza la clave
                    query = @"UPDATE tes_autorizaciones 
                              SET clave = @claveNueva 
                              WHERE nombre = @usuario";
                    var rowsAffected = connection.Execute(query, new
                    {
                        usuario = usuario.usuarioLogin,
                        claveNueva = usuario.claveNueva
                    });

                    if (rowsAffected == 0)
                    {
                        response.Code = -1; // Indica que no se actualizó ninguna fila
                        response.Description = "No se pudo cambiar la clave. Por favor, inténtelo de nuevo.";
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1; // Indica un error
                response.Description = $"Error al cambiar la clave: {ex.Message}";
            }
            return response;
        }

      

    }
}
