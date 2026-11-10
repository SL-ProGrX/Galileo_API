using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.Procesos;
using Galileo_API.Models.Procesos;
using Galileo_API.Models.ProGrX.Cobros;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoAplExcProcesaDb
    {
        private readonly PortalDB _portalDB;
        private readonly GenProcesoDb _procesoDb;

        public FrmCoAplExcProcesaDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmCoAplExcProcesaDb(PortalDB portalDB)
        {
            _portalDB = portalDB;
            _procesoDb = new GenProcesoDb(portalDB);
        }

        /// <summary>
        /// Obtener información de excedentes a procesar
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CoAplExcProcInformacionData>> CO_AplExcProc_Informacion_Obtener(int codEmpresa)
        {
            using var connection = _portalDB.CreateConnection(codEmpresa);

            var response = connection.Query<CoAplExcProcInformacionData>(
                sql: "exec spCBR_Excedente_Apl_Proceso_Carga_Informacion",
                commandTimeout: 0
            ).ToList();

            return DbHelper.CreateOkResponse(response);

        }

        /// <summary>
        /// Ejecuta la aplicación de excedentes de un proceso encolado. Procesa todos los casos,
        /// registra el avance en GEN_PROCESO y devuelve los casos con error al final.
        /// </summary>
        public ErrorDto<CoAplExcProcesadosResult> CO_AplExc_Procesa_Ejecutar(int CodEmpresa, Guid procesoId, ExcedenteAplicarRequest request)
        {
            try
            {
                var usuario = (request.Usuario ?? string.Empty).Trim().ToUpper();
                var cedulas = (request.Seleccionados ?? new List<CoAplExcProcInformacionData>())
                    .Where(x => !string.IsNullOrWhiteSpace(x.cedula))
                    .Select(x => x.cedula.Trim())
                    .ToList();

                if (cedulas.Count == 0)
                {
                    return RegistrarError(CodEmpresa, procesoId, "No hay casos a procesar");
                }

                _procesoDb.Actualizar(CodEmpresa, procesoId, GenProcesoEstados.EnProceso, 0, 0);

                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string guiaSql = "exec spCBR_Excedente_Apl_Proceso_Guia_Aplicacion @Usuario;";

                var idAplicacion = connection.QuerySingle<int>(
                    guiaSql,
                    new { Usuario = usuario },
                    commandTimeout: 0
                );

                var resultado = ProcesarCasos(CodEmpresa, procesoId, connection, usuario, idAplicacion, cedulas);

                return FinalizarProceso(CodEmpresa, procesoId, idAplicacion, cedulas.Count, resultado);
            }
            catch (Exception ex)
            {
                return RegistrarError(CodEmpresa, procesoId, ex.Message);
            }
        }

        private ResultadoCasos ProcesarCasos(
            int codEmpresa, Guid procesoId, IDbConnection connection,
            string usuario, int idAplicacion, List<string> cedulas)
        {
            int validos = 0;
            int procesados = 0;
            var errores = new List<string>();

            foreach (var cedula in cedulas)
            {
                procesados++;

                try
                {
                    if (AplicarExcedente(connection, usuario, cedula, idAplicacion))
                    {
                        validos++;
                    }
                    else
                    {
                        errores.Add(cedula);
                    }
                }
                catch (Exception ex)
                {
                    errores.Add($"{cedula} ({ex.Message})");
                }

                _procesoDb.Actualizar(codEmpresa, procesoId, GenProcesoEstados.EnProceso, procesados, errores.Count);
            }

            return new ResultadoCasos(validos, errores);
        }

        private ErrorDto<CoAplExcProcesadosResult> FinalizarProceso(
            int codEmpresa, Guid procesoId, int idAplicacion, int totalCasos, ResultadoCasos resultado)
        {
            var response = new ErrorDto<CoAplExcProcesadosResult>
            {
                Code = 0,
                Description = "Proceso concluido satisfactoriamente",
                Result = new CoAplExcProcesadosResult
                {
                    aplicados = resultado.Validos,
                    pendientes = totalCasos - resultado.Validos
                }
            };

            if (resultado.Errores.Count > 0)
            {
                response.Code = -1;
                response.Description =
                    $"Proceso concluido con {resultado.Errores.Count} caso(s) con error. " +
                    $"Aplicación: {idAplicacion}. " +
                    $"Procesados: {resultado.Validos} de {totalCasos}. " +
                    $"Casos no procesados: {string.Join(", ", resultado.Errores)}.";
            }

            _procesoDb.Actualizar(
                codEmpresa,
                procesoId,
                GenProcesoEstados.Terminado,
                totalCasos,
                resultado.Errores.Count,
                response.Description);

            return response;
        }

        private ErrorDto<CoAplExcProcesadosResult> RegistrarError(int codEmpresa, Guid procesoId, string mensaje)
        {
            _procesoDb.Actualizar(codEmpresa, procesoId, GenProcesoEstados.Error, 0, 0, mensaje);

            return new ErrorDto<CoAplExcProcesadosResult>
            {
                Code = -1,
                Description = mensaje,
                Result = new CoAplExcProcesadosResult
                {
                    aplicados = 0,
                    pendientes = 0
                }
            };
        }

        /// <summary>
        /// Aplica un excedente y valida su resultado.
        /// </summary>
        /// <returns>true si el caso quedó válido; false si la validación no lo aprobó.</returns>
        private static bool AplicarExcedente(IDbConnection connection, string usuario, string cedula, int idAplicacion)
        {
            const string aplicaSql =
                @"exec spCBR_Excedente_Apl_Proceso_Aplicacion
                    @Usuario, @Cedula, @AplicacionId;";

            connection.Execute(aplicaSql, new
            {
                Usuario = usuario,
                Cedula = cedula,
                AplicacionId = idAplicacion
            }, commandTimeout: 0);

            const string validaSql =
                @"select dbo.fxCBR_Excedente_Apl_Proceso_Valida(
                        @Cedula, @AplicacionId
                      ) as Resultado;";

            return connection.QuerySingle<bool>(validaSql, new
            {
                Cedula = cedula,
                AplicacionId = idAplicacion
            }, commandTimeout: 0);
        }

        private sealed record ResultadoCasos(int Validos, List<string> Errores);
    }
}
