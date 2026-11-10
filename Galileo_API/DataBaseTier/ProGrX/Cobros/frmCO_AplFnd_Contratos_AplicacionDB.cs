using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.Procesos;
using Galileo_API.Models.Procesos;
using Galileo_API.Models.ProGrX.Cobros;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoAplFndContratosAplicacionDb
    {
        private readonly PortalDB _portalDB;
        private readonly GenProcesoDb _procesoDb;

        public FrmCoAplFndContratosAplicacionDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmCoAplFndContratosAplicacionDb(PortalDB portalDB)
        {
            _portalDB = portalDB;
            _procesoDb = new GenProcesoDb(portalDB);
        }

        /// <summary>
        /// Obtiene la información de los contratos con mora para aplicar fondos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CoAplFndContrAplInformacionData>> CO_AplFndContrApl_Informacion_Obtener(int codEmpresa)
        {
            using var connection = _portalDB.CreateConnection(codEmpresa);

            var response = connection.Query<CoAplFndContrAplInformacionData>(
                sql: "exec spCBR_Fondos_Apl_Contratos_Proceso_Carga_Informacion",
                commandTimeout: 0
            ).ToList();

            return DbHelper.CreateOkResponse(response);

        }

        /// <summary>
        /// Ejecuta la aplicación de fondos de un proceso encolado. Registra el avance en GEN_PROCESO
        /// y se detiene en el primer caso que no valida (igual que el VB6).
        /// </summary>
        public ErrorDto<CoAplFndContrAplicadosResult> CO_AplFndContrApl_Procesa_Ejecutar(int CodEmpresa, Guid procesoId, ContratosAplicarRequest request)
        {
            try
            {
                var usuario = (request.Usuario ?? string.Empty).Trim().ToUpper();
                var cedulas = (request.Seleccionados ?? new List<CoAplFndContrAplInformacionData>())
                    .Where(x => !string.IsNullOrWhiteSpace(x.cedula))
                    .Select(x => x.cedula.Trim())
                    .ToList();

                if (cedulas.Count == 0)
                {
                    return RegistrarError(CodEmpresa, procesoId, "No hay casos a procesar");
                }

                _procesoDb.Actualizar(CodEmpresa, procesoId, GenProcesoEstados.EnProceso, 0, 0);

                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string guiaSql = "exec spCBR_Fondos_Apl_Contratos_Proceso_Guia_Aplicacion @Usuario;";

                var idAplicacion = connection.QuerySingle<int>(
                    guiaSql,
                    new { Usuario = usuario },
                    commandTimeout: 0
                );

                return ProcesarCasos(CodEmpresa, procesoId, connection, usuario, idAplicacion, cedulas);
            }
            catch (Exception ex)
            {
                return RegistrarError(CodEmpresa, procesoId, ex.Message);
            }
        }

        private ErrorDto<CoAplFndContrAplicadosResult> ProcesarCasos(
            int codEmpresa, Guid procesoId, IDbConnection connection,
            string usuario, int idAplicacion, List<string> cedulas)
        {
            int contValidos = 0;
            int totalCasos = cedulas.Count;

            foreach (var cedula in cedulas)
            {
                if (!AplicarFondo(connection, usuario, cedula, idAplicacion))
                {
                    var mensaje =
                        $"El caso {cedula} no se procesó correctamente. " +
                        $"Aplicación: {idAplicacion}. " +
                        $"Procesados: {contValidos} de {totalCasos}.";

                    return FinalizarConError(codEmpresa, procesoId, contValidos, totalCasos, mensaje);
                }

                contValidos++;
                _procesoDb.Actualizar(codEmpresa, procesoId, GenProcesoEstados.EnProceso, contValidos, 0);
            }

            return FinalizarOk(codEmpresa, procesoId, contValidos, totalCasos);
        }

        private ErrorDto<CoAplFndContrAplicadosResult> FinalizarOk(
            int codEmpresa, Guid procesoId, int contValidos, int totalCasos)
        {
            const string mensaje = "Proceso concluido satisfactoriamente";

            _procesoDb.Actualizar(codEmpresa, procesoId, GenProcesoEstados.Terminado, totalCasos, 0, mensaje);

            return new ErrorDto<CoAplFndContrAplicadosResult>
            {
                Code = 0,
                Description = mensaje,
                Result = new CoAplFndContrAplicadosResult
                {
                    aplicados = contValidos,
                    pendientes = totalCasos - contValidos
                }
            };
        }

        private ErrorDto<CoAplFndContrAplicadosResult> FinalizarConError(
            int codEmpresa, Guid procesoId, int contValidos, int totalCasos, string mensaje)
        {
            _procesoDb.Actualizar(codEmpresa, procesoId, GenProcesoEstados.Error, contValidos, 1, mensaje);

            return new ErrorDto<CoAplFndContrAplicadosResult>
            {
                Code = -1,
                Description = mensaje,
                Result = new CoAplFndContrAplicadosResult
                {
                    aplicados = contValidos,
                    pendientes = totalCasos - contValidos
                }
            };
        }

        private ErrorDto<CoAplFndContrAplicadosResult> RegistrarError(int codEmpresa, Guid procesoId, string mensaje)
        {
            _procesoDb.Actualizar(codEmpresa, procesoId, GenProcesoEstados.Error, 0, 0, mensaje);

            return new ErrorDto<CoAplFndContrAplicadosResult>
            {
                Code = -1,
                Description = mensaje,
                Result = new CoAplFndContrAplicadosResult
                {
                    aplicados = 0,
                    pendientes = 0
                }
            };
        }

        /// <summary>
        /// Aplica los fondos de un caso y valida su resultado.
        /// </summary>
        /// <returns>true si el caso quedó válido; false si la validación no lo aprobó.</returns>
        private static bool AplicarFondo(IDbConnection connection, string usuario, string cedula, int idAplicacion)
        {
            const string aplicaSql =
                @"exec spCBR_Fondos_Apl_Contratos_Proceso_Aplicacion @Usuario, @Cedula, @AplicacionId;";

            connection.Execute(aplicaSql, new
            {
                Usuario = usuario,
                Cedula = cedula,
                AplicacionId = idAplicacion
            }, commandTimeout: 0);

            const string validaSql =
                @"select dbo.fxCBR_Fondos_Apl_Contratos_Proceso_Valida(
                        @Cedula, @AplicacionId
                      ) as Resultado;";

            return connection.QuerySingle<bool>(validaSql, new
            {
                Cedula = cedula,
                AplicacionId = idAplicacion
            }, commandTimeout: 0);
        }
    }
}
