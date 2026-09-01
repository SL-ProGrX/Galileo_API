using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB
{
    /// <summary>
    /// Gestión del estado del proceso resiliente para proceso mensual.
    /// Maneja la tabla PRM_PROCESO_MENSUAL_PROCESO y sus operaciones CRUD.
    /// </summary>
    public sealed class CcProcesoMensualProcesoDb
    {
        private readonly PortalDB _portalDb;

        public CcProcesoMensualProcesoDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Calcula el hash SHA256 de los parámetros para deduplicación de procesos.
        /// </summary>
        public static string CalcularHash(int codEmpresa, int codInstitucion, decimal fechaProceso, string tipoProceso, string propietario)
        {
            var raw = $"{codEmpresa}|{codInstitucion}|{fechaProceso}|{tipoProceso}|{propietario}";
            var bytes = Encoding.UTF8.GetBytes(raw);
            var hashBytes = SHA256.HashData(bytes);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        /// <summary>
        /// Inicia un nuevo proceso o retorna uno existente si ya está activo con el mismo hash.
        /// </summary>
        public ErrorDto<CcProcesoMensualProcesoResultado> Proceso_Iniciar(
            int codEmpresa, string propietario, CcProcesoMensualProcesoIniciarRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
            connection.Open();

            var hash = CalcularHash(codEmpresa, request.CodInstitucion, request.FechaProceso, request.TipoProceso, propietario);

            var resultado = connection.QueryFirstOrDefault<CcProcesoMensualProcesoResultado>(
                "spPRM_ProcesoMensual_Proceso_Iniciar",
                new
                {
                    CodEmpresa = codEmpresa,
                    CodInstitucion = request.CodInstitucion,
                    FechaProceso = request.FechaProceso,
                    TipoProceso = request.TipoProceso,
                    Propietario = propietario,
                    Hash = hash,
                    ContextoJson = request.ContextoJson
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 0);

            if (resultado is null)
            {
                return DbHelper.CreateErrorResponse<CcProcesoMensualProcesoResultado>(
                    "No se pudo crear el proceso.", -1, new CcProcesoMensualProcesoResultado());
            }

            return DbHelper.CreateOkResponse(resultado);
        }

        /// <summary>
        /// Obtiene el estado actual del proceso (para polling).
        /// </summary>
        public ErrorDto<CcProcesoMensualProcesoResultado> Proceso_Estado_Obtener(
            int codEmpresa, Guid procesoId, string propietario)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
            connection.Open();

            var resultado = connection.QueryFirstOrDefault<CcProcesoMensualProcesoResultado>(
                "spPRM_ProcesoMensual_Proceso_Estado",
                new { ProcesoId = procesoId, Propietario = propietario },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 0);

            if (resultado is null)
            {
                return DbHelper.CreateErrorResponse<CcProcesoMensualProcesoResultado>(
                    "Proceso no encontrado.", -1, new CcProcesoMensualProcesoResultado());
            }

            return DbHelper.CreateOkResponse(resultado);
        }

        /// <summary>
        /// Adquiere un proceso Pendiente (Worker lo toma para procesar).
        /// </summary>
        public CcProcesoMensualProcesoResultado? Proceso_Adquirir(int codEmpresa, Guid procesoId)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
            connection.Open();

            return connection.QueryFirstOrDefault<CcProcesoMensualProcesoResultado>(
                "spPRM_ProcesoMensual_Proceso_Adquirir",
                new { ProcesoId = procesoId },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 0);
        }

        /// <summary>
        /// Actualiza el avance del proceso (total, procesadas, exitosas, errores).
        /// </summary>
        public void Proceso_Avance_Actualizar(
            int codEmpresa, Guid procesoId,
            int? total = null, int? procesadas = null, int? exitosas = null, int? errores = null,
            string? mensaje = null)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
            connection.Open();

            connection.Execute(
                "spPRM_ProcesoMensual_Proceso_Avance",
                new
                {
                    ProcesoId = procesoId,
                    Total = total,
                    Procesadas = procesadas,
                    Exitosas = exitosas,
                    Errores = errores,
                    Mensaje = mensaje
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 0);
        }

        /// <summary>
        /// Finaliza el proceso exitosamente.
        /// </summary>
        public void Proceso_Finalizar(int codEmpresa, Guid procesoId, string mensaje = "Completado")
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
            connection.Open();

            connection.Execute(
                "spPRM_ProcesoMensual_Proceso_Finalizar",
                new { ProcesoId = procesoId, Mensaje = mensaje },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 0);
        }

        /// <summary>
        /// Registra un error fatal en el proceso.
        /// </summary>
        public void Proceso_Error_Registrar(int codEmpresa, Guid procesoId, string mensaje)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
            connection.Open();

            connection.Execute(
                "spPRM_ProcesoMensual_Proceso_Error",
                new { ProcesoId = procesoId, Mensaje = mensaje },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 0);
        }

        /// <summary>
        /// Registra un error individual de un registro procesado.
        /// </summary>
        public void Proceso_ErrorRegistro_Registrar(
            int codEmpresa, Guid procesoId, int registroNumero, int codigo, string descripcion)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
            connection.Open();

            connection.Execute(
                "spPRM_ProcesoMensual_Proceso_ErrorRegistro",
                new
                {
                    ProcesoId = procesoId,
                    RegistroNumero = registroNumero,
                    Codigo = codigo,
                    Descripcion = descripcion
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 0);
        }

        /// <summary>
        /// Obtiene los errores individuales de un proceso.
        /// </summary>
        public List<CcProcesoMensualProcesoError> Proceso_Errores_Obtener(int codEmpresa, Guid procesoId)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
            connection.Open();

            return connection.Query<CcProcesoMensualProcesoError>(
                "spPRM_ProcesoMensual_Proceso_Errores",
                new { ProcesoId = procesoId },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 0).ToList();
        }

        /// <summary>
        /// Obtiene el contexto serializado de un proceso.
        /// </summary>
        public string? Proceso_Contexto_Obtener(int codEmpresa, Guid procesoId)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
            connection.Open();

            return connection.QueryFirstOrDefault<string>(
                "SELECT ContextoJson FROM PRM_PROCESO_MENSUAL_PROCESO WHERE ProcesoId = @ProcesoId",
                new { ProcesoId = procesoId },
                commandTimeout: 0);
        }

        /// <summary>
        /// Recupera procesos stuck en estado Procesando por más de 5 minutos.
        /// </summary>
        public int Proceso_Recuperar(int codEmpresa)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
            connection.Open();

            return connection.Execute(
                "spPRM_ProcesoMensual_Proceso_Recuperar",
                commandType: CommandType.StoredProcedure,
                commandTimeout: 0);
        }
    }
}
