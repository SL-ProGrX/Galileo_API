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
    public class FrmCoAplFndContratosAplicacionDb : CoProcesoDbBase
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

            public List<CoAplFndContrProcesoErrorDto> Errores { get; } = new();

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

        public FrmCoAplFndContratosAplicacionDb(IConfiguration config)
            : base(config)
        {
        }

        public FrmCoAplFndContratosAplicacionDb(PortalDB portalDB)
            : base(portalDB)
        {
        }

        /// <summary>
        /// Obtiene los contratos disponibles para aplicar fondos.
        /// </summary>
        public ErrorDto<List<CoAplFndContrAplInformacionData>>
            CO_AplFndContrApl_Informacion_Obtener(int codEmpresa)
        {
            return DbHelper.ExecuteListQuery<CoAplFndContrAplInformacionData>(
                _portalDB,
                codEmpresa,
                "exec dbo.spCBR_Fondos_Apl_Contratos_Proceso_Carga_Informacion",
                commandTimeout: 0);
        }

        /// <summary>
        /// Ejecuta todos los casos posibles, registra avance y acumula
        /// los motivos de error individuales.
        /// </summary>
        public ErrorDto<CoAplFndContrAplicadosResult>
            CO_AplFndContrApl_Procesa_Ejecutar(
                int codEmpresa,
                Guid procesoId,
                ContratosAplicarRequest request)
        {
            var items = request.Seleccionados ??
                new List<CoAplFndContrAplInformacionData>();

            var ejecucion = new Ejecucion
            {
                ProcesoId = procesoId,
                Usuario = (request.Usuario ?? string.Empty)
                    .Trim()
                    .ToUpperInvariant(),
                Total = items.Count
            };

            var estado = new CoAplFndContrProcesoEstadoResult
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
                    codEmpresa,
                    procesoId,
                    GenProcesoEstados.EnProceso,
                    0,
                    0,
                    "Preparando la aplicación de fondos.");

                ejecucion.AplicacionId =
                    CO_AplFndContrApl_Guia_Crear(codEmpresa, ejecucion);

                foreach (var item in items)
                {
                    ejecucion.Etapa =
                        $"Ejecutar caso {ejecucion.Procesados + 1}";

                    var error = CO_AplFndContrApl_Caso_Ejecutar(
                        codEmpresa,
                        ejecucion,
                        item);

                    ejecucion.Procesados++;

                    if (error is not null)
                    {
                        ejecucion.Errores.Add(error);
                    }

                    ejecucion.Etapa = "Registrar avance en GEN_PROCESO";

                    _procesoDb.Actualizar(
                        codEmpresa,
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
                // Una falla general conserva los contadores ya alcanzados.
                estado.Estado = GenProcesoEstados.Error;
                estado.Mensaje =
                    $"Etapa: {ejecucion.Etapa}. " +
                    CO_AplFndContrApl_Error_Describir(ex);
            }

            return CO_AplFndContrApl_Proceso_Finalizar(
                codEmpresa,
                ejecucion,
                estado);
        }

        /// <summary>
        /// Ejecuta la guía con @pUsuario y lee Aplicacion.
        /// </summary>
        private int CO_AplFndContrApl_Guia_Crear(
            int codEmpresa,
            Ejecucion ejecucion)
        {
            ejecucion.Etapa = "Abrir conexión para crear la guía";

            using var connection =
                DbHelper.OpenConnection(_portalDB, codEmpresa);

            connection.Open();

            ejecucion.Etapa =
                "Ejecutar spCBR_Fondos_Apl_Contratos_Proceso_Guia_Aplicacion";

            var filas = connection.Query<GuiaResultado>(
                "dbo.spCBR_Fondos_Apl_Contratos_Proceso_Guia_Aplicacion",
                new { pUsuario = ejecucion.Usuario },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 0
            ).ToList();

            ejecucion.Etapa = "Verificar transacción de la guía";
            CO_AplFndContrApl_Transaccion_Verificar(connection);

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
        /// Ejecuta un caso en su propia conexión. Lee Resultado y después
        /// confirma la aplicación mediante la función existente.
        /// </summary>
        private CoAplFndContrProcesoErrorDto?
            CO_AplFndContrApl_Caso_Ejecutar(
                int codEmpresa,
                Ejecucion ejecucion,
                CoAplFndContrAplInformacionData? item)
        {
            var cedula = item?.cedula?.Trim() ?? string.Empty;
            var etapa = "Validar identificación";
            string mensaje;

            try
            {
                // El SP declara @pCedula VARCHAR(15).
                if (cedula.Length == 0 || cedula.Length > 15)
                {
                    throw new ResultadoException(
                        "La identificación es obligatoria y admite hasta 15 caracteres.");
                }

                etapa = "Abrir conexión del caso";

                using var connection =
                    DbHelper.OpenConnection(_portalDB, codEmpresa);

                connection.Open();

                etapa =
                    "Ejecutar spCBR_Fondos_Apl_Contratos_Proceso_Aplicacion";

                var filas = connection.Query<CasoResultado>(
                    "dbo.spCBR_Fondos_Apl_Contratos_Proceso_Aplicacion",
                    new
                    {
                        pUsuario = ejecucion.Usuario,
                        pCedula = cedula,
                        pIdAplicacion = ejecucion.AplicacionId
                    },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 0
                ).ToList();
                
                CO_AplFndContrApl_Transaccion_Verificar(connection);

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
                        SELECT dbo.fxCBR_Fondos_Apl_Contratos_Proceso_Valida(
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
                    CO_AplFndContrApl_Error_Describir(ex) +
                    " Revise los movimientos antes de reintentar.";
            }

            return new CoAplFndContrProcesoErrorDto
            {
                numero_caso = ejecucion.Procesados + 1,
                cedula = cedula,
                nombre = item?.nombre ?? string.Empty,
                operaciones = item?.operaciones ?? string.Empty,
                mensaje = mensaje
            };
        }

        /// <summary>
        /// Detecta transacciones abiertas por un SP en una conexión
        /// que el API abrió sin transacción propia.
        /// </summary>
        private static void CO_AplFndContrApl_Transaccion_Verificar(
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
        private static string CO_AplFndContrApl_Error_Describir(
            Exception exception)
        {
            var error = exception.GetBaseException();
            var sql = exception as SqlException ?? error as SqlException;

            if (sql is not null)
            {
                return string.Join(
                    " | ",
                    sql.Errors.Cast<SqlError>()
                        .Select(item =>
                            $"Error SQL {item.Number}, estado {item.State}, " +
                            $"procedimiento {item.Procedure}, línea {item.LineNumber}.")
                        .Distinct());
            }

            if (error is ResultadoException)
            {
                return error.Message;
            }

            return
                $"No fue posible completar la operación. " +
                $"Tipo: {error.GetType().Name}.";
        }

        /// <summary>
        /// Conserva el detalle temporal y registra los contadores finales.
        /// </summary>
        private ErrorDto<CoAplFndContrAplicadosResult>
            CO_AplFndContrApl_Proceso_Finalizar(
                int codEmpresa,
                Ejecucion ejecucion,
                CoAplFndContrProcesoEstadoResult estado)
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

            CO_AplFndContrApl_Resultado_Guardar(codEmpresa, estado);

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
                    CO_AplFndContrApl_Error_Describir(ex);

                CO_AplFndContrApl_Resultado_Guardar(codEmpresa, estado);
            }

            var resultado = new CoAplFndContrAplicadosResult
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
        /// Conserva una copia del resultado final durante dos horas.
        /// </summary>
        private static void CO_AplFndContrApl_Resultado_Guardar(
            int codEmpresa,
            CoAplFndContrProcesoEstadoResult estado)
        {
            ResultadosTemporales.Set(
                (codEmpresa, estado.ProcesoId),
                JsonSerializer.Serialize(estado),
                TimeSpan.FromHours(2));
        }

        /// <summary>
        /// Devuelve el resultado temporal o el avance registrado.
        /// </summary>
        public ErrorDto<CoAplFndContrProcesoEstadoResult>
            CO_AplFndContrApl_Procesa_Estado(
                int codEmpresa,
                Guid procesoId)
        {
            try
            {
                if (ResultadosTemporales.TryGetValue<string>(
                        (codEmpresa, procesoId),
                        out var json) &&
                    !string.IsNullOrWhiteSpace(json))
                {
                    var resultado =
                        JsonSerializer.Deserialize<CoAplFndContrProcesoEstadoResult>(
                            json);

                    if (resultado is not null)
                    {
                        return DbHelper.CreateOkResponse(resultado);
                    }
                }

                var estado = _procesoDb.Estado_Obtener(
                    codEmpresa,
                    procesoId);

                if (estado is null)
                {
                    return DbHelper.CreateErrorResponse(
                        "Proceso no encontrado.",
                        -1,
                        new CoAplFndContrProcesoEstadoResult());
                }

                return DbHelper.CreateOkResponse(
                    new CoAplFndContrProcesoEstadoResult
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
                    CO_AplFndContrApl_Error_Describir(ex),
                    -1,
                    new CoAplFndContrProcesoEstadoResult());
            }
        }
    }
}