using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Dapper;
using static Galileo_API.Models.ProGrX.Creditos.FrmCRSeguimientoCausasModels;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCRSeguimientoCausasDB
    {
        private readonly PortalDB _portalDb;   
        private readonly IConfiguration _config;

        public FrmCRSeguimientoCausasDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _portalDb = new PortalDB(config);  
        }

        private PortalDB CreatePortalDb() => new(_config);
     
        /// <summary>
        /// Consulta todas las causas de un seguimiento
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CrSeguimientoCausasData>> CR_SeguimientoCausas_Obtener(int codEmpresa, CrSeguimientoCausasObtenerRequest request)
        {
            const string query = """
                 SELECT 
                 RTRIM(C.cod_causas) AS CodCausa, 
                 RTRIM(C.descripcion) AS Descripcion,
                 CONVERT( bit, CASE WHEN EXISTS ( SELECT 1 FROM operacion_gestion AS G 
                 WHERE G.cod_causas = C.cod_causas AND G.tipo = C.tipo AND G.id_solicitud = @IdSolicitud ) 
                 THEN 1 ELSE 0 END ) AS Seleccionado
                 FROM operacion_causas AS C 
                 WHERE C.estado = 1 AND C.tipo = @Tipo ORDER BY C.cod_causas; 
                """;
            return DbHelper.ExecuteListQuery<CrSeguimientoCausasData>(CreatePortalDb(), codEmpresa, query, new { request.IdSolicitud, request.Tipo });
        }
       
        /// <summary>
        /// insertar una causa
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private static int RegistrarCausa(IDbConnection connection, CrSeguimientoCausasActualizarRequest request)
        {
            const string query = @"
                  INSERT INTO operacion_gestion ( cod_causas, tipo, id_solicitud, codigo )
                 VALUES ( @CodCausa, @Tipo, @IdSolicitud, @Codigo );
               ";
            return connection.Execute(query, new { request.CodCausa, request.Tipo, request.IdSolicitud, request.Codigo });
        }
       
        /// <summary>
        /// Elimina una causa
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private static int EliminarCausa(IDbConnection connection, CrSeguimientoCausasActualizarRequest request)
        {
            const string query = """
                 DELETE FROM operacion_gestion 
                WHERE cod_causas = @CodCausa AND tipo = @Tipo AND id_solicitud = @IdSolicitud; 
                """;
            return connection.Execute(query, new { request.CodCausa, request.Tipo, request.IdSolicitud });
        }
      
        /// <summary>
        ///  Elimina o insetarta una nueva causa
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<bool> CR_SeguimientoCausas_Actualizar(int codEmpresa, CrSeguimientoCausasActualizarRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
            try
            {
                var affectedRows = request.Seleccionado ? RegistrarCausa(connection, request) : EliminarCausa(connection, request);
                return DbHelper.CreateOkResponse(affectedRows >= 0);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse("Error al actualizar causa.", -1, false);
            }
        }


    }
}
