using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Microsoft.Data.SqlClient;


namespace Galileo_API.DataBaseTier.ProGrX.Bancos
{
    public class FrmTesAutorizaChKeyDB
    {
        private readonly IConfiguration? _config;
        private readonly PortalDB _portalDB;

        public FrmTesAutorizaChKeyDB(IConfiguration config)
        {
            _config = config;
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Método para cambiar la clave de autorización de cheques
        /// </summary>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Tes_AutorizaChKey_Cambiar(AutorizaChKeyData usuario)
        {
            var connection = DbHelper.OpenConnection(_portalDB, usuario.CodEmpresa);
            try
            {
                
                //Verifica si la clave actual es correcta
                var  query = @"SELECT isnull(count(*),0) as existe from tes_autorizaciones 
                                where nombre = @usuario and clave = @claveActual ";
                var existe = connection.QueryFirstOrDefault<int>(query, new
                {
                    usuario = usuario.usuarioLogin,
                    claveActual = usuario.claveActual
                });

                if (existe == 0)
                {
                    return DbHelper.ErrorResponse($"La clave actual no es valida o no fue localizada..."); 
                }

                // Verifica si la clave nueva y la confirmación son iguales
                if (usuario.claveNueva != usuario.claveConfirmar)
                {
                    return DbHelper.ErrorResponse($"La clave nueva y la confirmación no coinciden."); 
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
                    return DbHelper.ErrorResponse($"No se pudo cambiar la clave. Por favor, inténtelo de nuevo");
                }
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error al cambiar la clave: {ex.Message}");
            }
            return DbHelper.CreateOkResponse(); 
        }

      

    }
}
