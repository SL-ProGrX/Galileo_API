using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.Procesos;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using System.Data;
using System.Text.Json;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoAplExcProcesaDb : CoProcesoDbBase
    {
        private static readonly MemoryCache ResultadosTemporales =
            new(new MemoryCacheOptions
            {
                ExpirationScanFrequency = TimeSpan.FromMinutes(1)
            });

        private sealed class Ejecucion
        {
            public Guid ProcesoId { get; init; }
            public string Usuario { get; init; } = string.Empty;
            public int AplicacionId { get; set; }
            public int Total { get; init; }
            public int Procesados { get; set; }
            public string Etapa { get; set; } = "Validar selección";

            public List<CoAplExcProcesoErrorDto> Errores { get; } = new();

            public int Correctos => Procesados - Errores.Count;
            public int SinIntentar => Total - Procesados;
        }

        private sealed class GuiaResultado
        {
            public int? Aplicacion { get; set; }
        }

        private sealed class CasoResultado
        {
            public string? Resultado { get; set; }
        }

        public sealed class ResultadoException : Exception
        {
            public ResultadoException(string mensaje) : base(mensaje)
            {
            }
        }

        public FrmCoAplExcProcesaDb(IConfiguration config)
            : base(config)
        {
        }

        public FrmCoAplExcProcesaDb(PortalDB portalDB)
            : base(portalDB)
        {
        }

        /// <summary>
        /// Obtiene los acuerdos disponibles para aplicar excedentes.
        /// </summary>
        public ErrorDto<List<CoAplExcProcInformacionData>>
            CO_AplExcProc_Informacion_Obtener(int codEmpresa)
        {
            return DbHelper.ExecuteListQuery<CoAplExcProcInformacionData>(
                _portalDB,
                codEmpresa,
                "exec dbo.spCBR_Excedente_Apl_Proceso_Carga_Informacion",
                commandTimeout: 0);
        }

        /// <summary>
        /// Ejecuta el lote, conserva errores individuales y publica avance.
        /// Los errores de un caso no impiden intentar el siguiente.
        /// </summary>
        public ErrorDto<CoAplExcProcesadosResult> CO_AplExc_Procesa_Ejecutar(
            int CodEmpresa,
            Guid procesoId,
            ExcedenteAplicarRequest request)
        {
            var items = request.Seleccionados ??
                new List<CoAplExcProcInformacionData>();

            var ejecucion = new Ejecucion
            {
                ProcesoId = procesoId,
                Usuario = (request.Usuario ?? string.Empty)
                    .Trim()
                    .ToUpperInvariant(),
                Total = items.Count
            };

            var estado = new CoAplExcProcesoEstadoResult
            {
                Estado = GenProcesoEstados.Terminado
            };

            try
            {
                if (items.Count == 0)
                {
                    throw new ResultadoException("No hay casos a procesar.");
                }

                if (ejecucion.Usuario.Length == 0 ||
                    ejecucion.Usuario.Length > 30)
                {
                    throw new ResultadoException(
                        "El usuario es obligatorio y admite hasta 30 caracteres.");
                }

                ejecucion.Etapa = "Registrar inicio en GEN_PROCESO";

                _procesoDb.Actualizar(
                    CodEmpresa,
                    procesoId,
                    GenProcesoEstados.EnProceso,
                    0,
                    0,
                    "Preparando la aplicación de excedentes.");

                ejecucion.AplicacionId =
                    CO_AplExc_Procesa_Guia_Crear(CodEmpresa, ejecucion);

                foreach (var item in items)
                {
                    ejecucion.Etapa =
                        $"Ejecutar caso {ejecucion.Procesados + 1}";

                    var error = CO_AplExc_Procesa_Caso_Ejecutar(
                        CodEmpresa,
                        ejecucion,
                        item);

                    ejecucion.Procesados++;

                    if (error is not null)
                    {
                        ejecucion.Errores.Add(error);
                    }

                    ejecucion.Etapa = "Registrar avance en GEN_PROCESO";

                    _procesoDb.Actualizar(
                        CodEmpresa,
                        procesoId,
                        GenProcesoEstados.EnProceso,
                        ejecucion.Procesados,
                        ejecucion.Errores.Count,
                        $"Aplicación: {ejecucion.AplicacionId}. " +
                        $"Procesados: {ejecucion.Procesados} de {ejecucion.Total}. " +
                        $"Correctos: {ejecucion.Correctos}. " +
                        $"Con error: {ejecucion.Errores.Count}.");
                }
            }
            catch (Exception ex)
            {
                // Una falla general conserva los casos ya intentados.
                estado.Estado = GenProcesoEstados.Error;
                estado.Mensaje =
                    $"Etapa: {ejecucion.Etapa}. " +
                    CO_AplExc_Procesa_Error_Describir(ex);
            }

            return CO_AplExc_Procesa_Finalizar(
                CodEmpresa,
                ejecucion,
                estado);
        }

        /// <summary>
        /// Crea la guía usando @pUsuario y lee Aplicacion.
        /// </summary>
        private int CO_AplExc_Procesa_Guia_Crear(
            int codEmpresa,
            Ejecucion ejecucion)
        {
            ejecucion.Etapa = "Abrir conexión para crear la guía";

            using var connection =
                DbHelper.OpenConnection(_portalDB, codEmpresa);

            connection.Open();

            ejecucion.Etapa =
                "Ejecutar spCBR_Excedente_Apl_Proceso_Guia_Aplicacion";

            var filas = connection.Query<GuiaResultado>(
                "dbo.spCBR_Excedente_Apl_Proceso_Guia_Aplicacion",
                new { pUsuario = ejecucion.Usuario },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 0
            ).ToList();

            CO_AplExc_Procesa_Transaccion_Verificar(connection);

            if (filas.Count != 1 ||
                !filas[0].Aplicacion.HasValue ||
                filas[0].Aplicacion.Value <= 0)
            {
                throw new ResultadoException(
                    "La guía no devolvió una única Aplicacion positiva.");
            }

            return filas[0].Aplicacion.Value;
        }

        /// <summary>
        /// Ejecuta un caso en su propia conexión y lee el motivo devuelto
        /// en Resultado. La función confirma la aplicación registrada.
        /// </summary>
        private CoAplExcProcesoErrorDto? CO_AplExc_Procesa_Caso_Ejecutar(
            int codEmpresa,
            Ejecucion ejecucion,
            CoAplExcProcInformacionData? item)
        {
            var cedula = item?.cedula?.Trim() ?? string.Empty;
            var etapa = "Validar identificación";
            string mensaje;

            try
            {
                if (cedula.Length == 0 || cedula.Length > 20)
                {
                    throw new ResultadoException(
                        "La identificación es obligatoria y admite hasta 20 caracteres.");
                }

                etapa = "Abrir conexión del caso";

                using var connection =
                    DbHelper.OpenConnection(_portalDB, codEmpresa);

                connection.Open();

                etapa = "Ejecutar spCBR_Excedente_Apl_Proceso_Aplicacion";

                var filas = connection.Query<CasoResultado>(
                    "dbo.spCBR_Excedente_Apl_Proceso_Aplicacion",
                    new
                    {
                        pUsuario = ejecucion.Usuario,
                        pCedula = cedula,
                        pIdAplicacion = ejecucion.AplicacionId
                    },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 0
                ).ToList();

                CO_AplExc_Procesa_Transaccion_Verificar(connection);

                etapa = "Leer Resultado del procedimiento";

                if (filas.Count != 1)
                {
                    throw new ResultadoException(
                        $"El procedimiento devolvió {filas.Count} filas; " +
                        "se esperaba una fila con Resultado.");
                }

                var motivo = filas[0].Resultado?.Trim();

                if (!string.IsNullOrWhiteSpace(motivo))
                {
                    mensaje =
                        $"El procedimiento informó: {motivo} " +
                        "Revise los movimientos antes de reintentar.";
                }
                else
                {
                    etapa = "Confirmar aplicación registrada";

                    var valido = connection.QuerySingle<bool?>(
                        """
                        SELECT dbo.fxCBR_Excedente_Apl_Proceso_Valida(
                            @Cedula,
                            @Aplicacion
                        );
                        """,
                        new
                        {
                            Cedula = cedula,
                            Aplicacion = ejecucion.AplicacionId
                        },
                        commandTimeout: 0);

                    if (valido == true)
                    {
                        return null;
                    }

                    mensaje =
                        "El procedimiento no informó un motivo, pero la función " +
                        "de validación no confirmó una aplicación procesada. " +
                        "Revise los movimientos antes de reintentar.";
                }
            }
            catch (Exception ex)
            {
                mensaje =
                    $"Etapa: {etapa}. " +
                    CO_AplExc_Procesa_Error_Describir(ex) +
                    " Revise los movimientos antes de reintentar.";
            }

            return new CoAplExcProcesoErrorDto
            {
                numero_caso = ejecucion.Procesados + 1,
                cedula = cedula,
                nombre = item?.nombre ?? string.Empty,
                mensaje = mensaje
            };
        }

        /// <summary>
        /// Detecta transacciones que el SP haya dejado abiertas.
        /// La conexión del caso se desecha al salir del método.
        /// </summary>
        private static void CO_AplExc_Procesa_Transaccion_Verificar(
            IDbConnection connection)
        {
            var abiertas = connection.QuerySingle<int>(
                "SELECT @@TRANCOUNT;",
                commandTimeout: 30);

            if (abiertas != 0)
            {
                throw new ResultadoException(
                    $"El procedimiento dejó {abiertas} transacciones abiertas. " +
                    "El caso no se considera confirmado.");
            }
        }

        /// <summary>
        /// Describe fallos técnicos sin devolver conexiones ni credenciales.
        /// </summary>
        private static string CO_AplExc_Procesa_Error_Describir(
            Exception exception)
        {
            var error = exception.GetBaseException();

            var sql = exception as SqlException ?? error as SqlException;

            if (sql is not null)
            {
                return string.Join(
                    " | ",
                    sql.Errors.Cast<SqlError>().Select(item =>
                        $"Error SQL {item.Number}, estado {item.State}, " +
                        $"procedimiento {item.Procedure}, línea {item.LineNumber}.")
                    .Distinct());
            }

            if (error is ResultadoException)
            {
                return error.Message;
            }

            return $"No fue posible completar la operación. Tipo: {error.GetType().Name}.";
        }

        /// <summary>
        /// Conserva el detalle temporal y registra los contadores finales.
        /// </summary>
        private ErrorDto<CoAplExcProcesadosResult> CO_AplExc_Procesa_Finalizar(
            int codEmpresa,
            Ejecucion ejecucion,
            CoAplExcProcesoEstadoResult estado)
        {
            estado.ProcesoId = ejecucion.ProcesoId;
            estado.TotalItems = ejecucion.Total;
            estado.ItemsProcesados = ejecucion.Procesados;
            estado.ItemsConError = ejecucion.Errores.Count;
            estado.DetalleDisponible = true;
            estado.Errores = ejecucion.Errores.ToList();

            var aplicacion = ejecucion.AplicacionId > 0
                ? ejecucion.AplicacionId.ToString()
                : "sin identificador confirmado";

            var resumen =
                $"Aplicación: {aplicacion}. " +
                $"Correctos: {ejecucion.Correctos}. " +
                $"Con error: {ejecucion.Errores.Count}. " +
                $"Sin intentar: {ejecucion.SinIntentar}. " +
                $"Procesados: {ejecucion.Procesados} de {ejecucion.Total}.";

            estado.Mensaje = string.IsNullOrWhiteSpace(estado.Mensaje)
                ? resumen
                : $"{estado.Mensaje} {resumen}";

            CO_AplExc_Procesa_Resultado_Guardar(codEmpresa, estado);

            try
            {
                _procesoDb.Actualizar(
                    codEmpresa,
                    ejecucion.ProcesoId,
                    estado.Estado,
                    estado.ItemsProcesados,
                    estado.ItemsConError,
                    estado.Mensaje);
            }
            catch (Exception ex)
            {
                estado.Mensaje +=
                    " No fue posible registrar el estado final en GEN_PROCESO. " +
                    CO_AplExc_Procesa_Error_Describir(ex);

                CO_AplExc_Procesa_Resultado_Guardar(codEmpresa, estado);
            }

            var resultado = new CoAplExcProcesadosResult
            {
                aplicados = ejecucion.Correctos,
                pendientes = ejecucion.Total - ejecucion.Correctos
            };

            return estado.Estado == GenProcesoEstados.Terminado
                ? DbHelper.CreateOkResponse(
                    resultado,
                    estado.Mensaje ?? string.Empty)
                : DbHelper.CreateErrorResponse(
                    estado.Mensaje ?? string.Empty,
                    -1,
                    resultado);
        }

        /// <summary>
        /// Conserva una copia independiente del resultado durante dos horas.
        /// </summary>
        private static void CO_AplExc_Procesa_Resultado_Guardar(
            int codEmpresa,
            CoAplExcProcesoEstadoResult estado)
        {
            ResultadosTemporales.Set(
                (codEmpresa, estado.ProcesoId),
                JsonSerializer.Serialize(estado),
                TimeSpan.FromHours(2));
        }

        /// <summary>
        /// Devuelve el resultado temporal o el avance persistido.
        /// </summary>
        public ErrorDto<CoAplExcProcesoEstadoResult>
            CO_AplExc_Procesa_Estado(int codEmpresa, Guid procesoId)
        {
            try
            {
                if (ResultadosTemporales.TryGetValue<string>(
                        (codEmpresa, procesoId),
                        out var json) &&
                    !string.IsNullOrWhiteSpace(json))
                {
                    var resultado =
                        JsonSerializer.Deserialize<CoAplExcProcesoEstadoResult>(json);

                    if (resultado is not null)
                    {
                        return DbHelper.CreateOkResponse(resultado);
                    }
                }

                var estado = _procesoDb.Estado_Obtener(codEmpresa, procesoId);

                if (estado is null)
                {
                    return DbHelper.CreateErrorResponse(
                        "Proceso no encontrado.",
                        -1,
                        new CoAplExcProcesoEstadoResult());
                }

                return DbHelper.CreateOkResponse(
                    new CoAplExcProcesoEstadoResult
                    {
                        ProcesoId = estado.ProcesoId,
                        Estado = estado.Estado,
                        TotalItems = estado.TotalItems,
                        ItemsProcesados = estado.ItemsProcesados,
                        ItemsConError = estado.ItemsConError,
                        Mensaje = estado.Mensaje,
                        FechaInicio = estado.FechaInicio,
                        FechaFin = estado.FechaFin,
                        DetalleDisponible = false
                    });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    "No fue posible consultar el avance. " +
                    CO_AplExc_Procesa_Error_Describir(ex),
                    -1,
                    new CoAplExcProcesoEstadoResult());
            }
        }
    }
}