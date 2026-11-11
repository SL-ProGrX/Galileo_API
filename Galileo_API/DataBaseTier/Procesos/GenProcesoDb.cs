using Dapper;
using Galileo.DataBaseTier;
using Galileo_API.Models.Procesos;
using System.Data;

namespace Galileo_API.DataBaseTier.Procesos
{
    /// <summary>
    /// Acceso a la tabla general de procesos (GEN_PROCESO) para registrar avance y tiempos.
    /// </summary>
    public sealed class GenProcesoDb
    {
        private const int MensajeMaxLength = 4000;
        private readonly PortalDB _portalDb;

        public GenProcesoDb(PortalDB portalDb)
        {
            _portalDb = portalDb;
        }

        public void Iniciar(int codEmpresa, Guid procesoId, string tipoProceso, string usuario, int totalItems)
        {
            using var connection = _portalDb.CreateConnection(codEmpresa);

            connection.Execute(
                "spGEN_Proceso_Iniciar",
                new
                {
                    ProcesoId = procesoId,
                    CodEmpresa = codEmpresa,
                    TipoProceso = tipoProceso,
                    Usuario = usuario,
                    TotalItems = totalItems
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 0);
        }

        public void Actualizar(int codEmpresa, Guid procesoId, string estado, int itemsProcesados, int itemsConError, string? mensaje = null)
        {
            using var connection = _portalDb.CreateConnection(codEmpresa);

            connection.Execute(
                "spGEN_Proceso_Actualizar",
                new
                {
                    ProcesoId = procesoId,
                    Estado = estado,
                    ItemsProcesados = itemsProcesados,
                    ItemsConError = itemsConError,
                    Mensaje = Truncar(mensaje)
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 0);
        }

        public GenProcesoEstadoResult? Estado_Obtener(int codEmpresa, Guid procesoId)
        {
            using var connection = _portalDb.CreateConnection(codEmpresa);

            return connection.QueryFirstOrDefault<GenProcesoEstadoResult>(
                "spGEN_Proceso_Estado",
                new { ProcesoId = procesoId, CodEmpresa = codEmpresa },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 0);
        }

        private static string? Truncar(string? mensaje) =>
            mensaje is null || mensaje.Length <= MensajeMaxLength
                ? mensaje
                : mensaje[..MensajeMaxLength];
    }
}
