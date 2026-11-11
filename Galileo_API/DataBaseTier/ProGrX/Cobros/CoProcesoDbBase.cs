using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.Procesos;
using Galileo_API.Models.Procesos;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    /// <summary>
    /// Base de las DB de procesos de aplicación de Cobros: conexión al portal, avance en GEN_PROCESO
    /// y respuesta de error uniforme.
    /// </summary>
    public abstract class CoProcesoDbBase
    {
        protected readonly PortalDB _portalDB;
        protected readonly GenProcesoDb _procesoDb;

        protected CoProcesoDbBase(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        protected CoProcesoDbBase(PortalDB portalDB)
        {
            _portalDB = portalDB;
            _procesoDb = new GenProcesoDb(portalDB);
        }

        /// <summary>
        /// Registra el error en GEN_PROCESO y devuelve la respuesta de error con el resultado indicado.
        /// </summary>
        protected ErrorDto<TResultado> RespuestaError<TResultado>(
            int codEmpresa, Guid procesoId, string mensaje, TResultado resultadoVacio)
        {
            _procesoDb.Actualizar(codEmpresa, procesoId, GenProcesoEstados.Error, 0, 0, mensaje);

            return new ErrorDto<TResultado>
            {
                Code = -1,
                Description = mensaje,
                Result = resultadoVacio
            };
        }

        /// <summary>
        /// Ejecuta la aplicación de un caso y devuelve si la validación posterior lo aprobó.
        /// </summary>
        protected static bool AplicarYValidarCaso(
            IDbConnection connection, string usuario, string cedula, int idAplicacion, string aplicacionSql, string validaSql)
        {
            connection.Execute(aplicacionSql, new
            {
                Usuario = usuario,
                Cedula = cedula,
                AplicacionId = idAplicacion
            }, commandTimeout: 0);

            return connection.QuerySingle<bool>(validaSql, new
            {
                Cedula = cedula,
                AplicacionId = idAplicacion
            }, commandTimeout: 0);
        }
    }
}
