using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.Procesos;
using Galileo_API.Models.ProGrX.Cobros;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoAplExcContratosAplicacionDb : CoProcesoDbBase
    {
        public FrmCoAplExcContratosAplicacionDb(IConfiguration config)
            : base(config)
        {
        }

        public FrmCoAplExcContratosAplicacionDb(PortalDB portalDB)
            : base(portalDB)
        {
        }

        /// <summary>
        /// Obtiene la información de los contratos para aplicar excedentes a mora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CoAplExcContrAplInformacionData>> CO_AplExcContrApl_Informacion_Obtener(int codEmpresa)
        {

            using var connection = _portalDB.CreateConnection(codEmpresa);

            var response = connection.Query<CoAplExcContrAplInformacionData> (
                sql: "exec spCBR_Excedente_Apl_Contratos_Proceso_Carga_Informacion",
                commandTimeout: 0
            ).ToList();

            return DbHelper.CreateOkResponse(response);
        }

        /// <summary>
        /// Ejecuta la aplicación de excedentes a mora de un proceso encolado. Registra el avance en GEN_PROCESO
        /// y se detiene en el primer caso que no valida (igual que el VB6).
        /// </summary>
        public ErrorDto<CoAplExcContrAplicadosResult> CO_AplExcContrApl_Procesa_Ejecutar(int CodEmpresa, Guid procesoId, ExcContratosAplicarRequest request)
        {
            try
            {
                var usuario = (request.Usuario ?? string.Empty).Trim().ToUpper();
                var items = request.Seleccionados
                    ?.Where(x => !string.IsNullOrWhiteSpace(x.cedula))
                    .ToList() ?? new List<CoAplExcContrAplInformacionData>();

                if (items.Count == 0)
                {
                    return RegistrarError(CodEmpresa, procesoId, "No hay casos a procesar");
                }

                _procesoDb.Actualizar(CodEmpresa, procesoId, GenProcesoEstados.EnProceso, 0, 0);

                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string guiaAplSql = "exec spCBR_Excedente_Apl_Contratos_Proceso_Guia_Aplicacion @Usuario;";

                var idAplicacion = connection.QuerySingle<int>(
                    guiaAplSql,
                    new { Usuario = usuario },
                    commandTimeout: 0
                );

                var cedulas = items.Select(x => x.cedula.Trim()).ToList();

                return ProcesarCasos(CodEmpresa, procesoId, connection, usuario, idAplicacion, cedulas);
            }
            catch (Exception ex)
            {
                return RegistrarError(CodEmpresa, procesoId, ex.Message);
            }
        }

        private ErrorDto<CoAplExcContrAplicadosResult> ProcesarCasos(
            int codEmpresa, Guid procesoId, IDbConnection connection,
            string usuario, int idAplicacion, List<string> cedulas)
        {
            int contValidos = 0;
            int totalCasos = cedulas.Count;

            foreach (var cedula in cedulas)
            {
                if (!AplicarExcedenteContrato(connection, usuario, cedula, idAplicacion))
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

        private ErrorDto<CoAplExcContrAplicadosResult> FinalizarOk(
            int codEmpresa, Guid procesoId, int contValidos, int totalCasos)
        {
            const string mensaje = "Proceso concluido satisfactoriamente";

            _procesoDb.Actualizar(codEmpresa, procesoId, GenProcesoEstados.Terminado, totalCasos, 0, mensaje);

            return new ErrorDto<CoAplExcContrAplicadosResult>
            {
                Code = 0,
                Description = mensaje,
                Result = new CoAplExcContrAplicadosResult
                {
                    aplicados = contValidos,
                    pendientes = totalCasos - contValidos
                }
            };
        }

        private ErrorDto<CoAplExcContrAplicadosResult> FinalizarConError(
            int codEmpresa, Guid procesoId, int contValidos, int totalCasos, string mensaje)
        {
            _procesoDb.Actualizar(codEmpresa, procesoId, GenProcesoEstados.Error, contValidos, 1, mensaje);

            return new ErrorDto<CoAplExcContrAplicadosResult>
            {
                Code = -1,
                Description = mensaje,
                Result = new CoAplExcContrAplicadosResult
                {
                    aplicados = contValidos,
                    pendientes = totalCasos - contValidos
                }
            };
        }

        private ErrorDto<CoAplExcContrAplicadosResult> RegistrarError(int codEmpresa, Guid procesoId, string mensaje) =>
            RespuestaError(codEmpresa, procesoId, mensaje, new CoAplExcContrAplicadosResult { aplicados = 0, pendientes = 0 });

        /// <summary>
        /// Aplica los excedentes de un contrato y valida su resultado.
        /// </summary>
        /// <returns>true si el caso quedó válido; false si la validación no lo aprobó.</returns>
        private static bool AplicarExcedenteContrato(IDbConnection connection, string usuario, string cedula, int idAplicacion)
        {
            const string aplicSql =
                @"exec spCBR_Excedente_Apl_Contratos_Proceso_Aplicacion @Usuario, @Cedula, @AplicacionId;";

            const string validaSql =
                @"select dbo.fxCBR_Excedente_Apl_Contratos_Proceso_Valida(
                        @Cedula, @AplicacionId
                      ) as Resultado;";

            return AplicarYValidarCaso(connection, usuario, cedula, idAplicacion, aplicSql, validaSql);
        }
    }
}
