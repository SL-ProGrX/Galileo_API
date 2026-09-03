using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;

namespace Galileo.DataBaseTier
{
    public sealed class FrmInvOrdenesAutorizacionDb
    {
        private const int CodigoValidacion = -2;

        private const string TipoEntrada = "E";
        private const string TipoSalida = "S";
        private const string TipoTraspaso = "T";
        private const string TipoRequisicion = "R";

        private const string EstadoAutorizado = "A";
        private const string EstadoRechazado = "R";

        private const string MensajeEmpresaRequerida =
            "El c&oacute;digo de la empresa es requerido.";

        private const string MensajeFiltrosRequeridos =
            "Los filtros de consulta son requeridos.";

        private const string MensajeTipoRequerido =
            "El tipo de transacci&oacute;n es requerido.";

        private const string MensajeTipoInvalido =
            "El tipo de transacci&oacute;n indicado no es v&aacute;lido.";

        private const string MensajeUsuarioRequerido =
            "El usuario es requerido.";

        private const string MensajeFechaInicioRequerida =
            "La fecha inicial es requerida.";

        private const string MensajeFechaCorteRequerida =
            "La fecha de corte es requerida.";

        private const string MensajeRangoFechasInvalido =
            "La fecha inicial no puede ser mayor que la fecha de corte.";

        private const string MensajeSolicitudRequerida =
            "La informaci&oacute;n de las &oacute;rdenes es requerida.";

        private const string MensajeOrdenesRequeridas =
            "Debe seleccionar al menos una orden.";

        private const string MensajeCodigoOrdenRequerido =
            "El c&oacute;digo de la orden es requerido.";

        private const string MensajeOrdenDuplicada =
            "No se permite incluir la misma orden m&aacute;s de una vez.";

        private const string MensajeOrdenNoDisponible =
            "Una o m&aacute;s &oacute;rdenes no pudieron ser actualizadas.";

        private const string MensajeConsultaError =
            "Ocurri&oacute; un error al consultar las &oacute;rdenes pendientes.";

        private const string MensajeAutorizarError =
            "Ocurri&oacute; un error al autorizar las &oacute;rdenes seleccionadas.";

        private const string MensajeRechazarError =
            "Ocurri&oacute; un error al rechazar las &oacute;rdenes seleccionadas.";

        private const string MensajeAutorizacionExitosa =
            "Solicitudes autorizadas satisfactoriamente.";

        private const string MensajeRechazoExitoso =
            "Solicitudes rechazadas satisfactoriamente.";

        private readonly PortalDB _portalDb;

        public FrmInvOrdenesAutorizacionDb(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene las órdenes pendientes de autorización o rechazo.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros aplicados a la consulta.</param>
        /// <returns>Listado de órdenes pendientes.</returns>
        public ErrorDto<List<ResolucionTransaccionDto>>
            INV_OrdenesAutorizacion_Ordenes_Obtener(
                int CodEmpresa,
                InvOrdenesAutorizacionFiltros filtros)
        {
            var lista = new List<ResolucionTransaccionDto>();

            if (CodEmpresa <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeEmpresaRequerida,
                    CodigoValidacion,
                    lista);
            }

            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeFiltrosRequeridos,
                    CodigoValidacion,
                    lista);
            }

            string validacion =
                INV_OrdenesAutorizacion_Filtros_Validar(
                    filtros);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.CreateErrorResponse(
                    validacion,
                    CodigoValidacion,
                    lista);
            }

            const string QueryOrdenesInventario = """
            SELECT
                O.boleta AS cod_orden,
                O.tipo AS tipo_orden,
                O.total,
                O.genera_user AS user_solicita,
                O.genera_fecha AS fecha,
                C.descripcion AS causa,
                O.notas AS nota,
                'P' AS proceso
            FROM pv_InvTranSac O
            INNER JOIN pv_entrada_salida C
                ON O.cod_entsal = C.cod_entsal
            WHERE O.autoriza_fecha IS NULL
              AND O.estado = 'S'
              AND O.tipo = @Tipo
              AND O.genera_user IN
              (
                  SELECT usuario_asignado
                  FROM pv_orden_autousers
                  WHERE usuario = @Usuario
              )
              AND
              (
                  @TodasFechas = 1
                  OR O.genera_fecha BETWEEN
                      @FechaInicio AND @FechaCorte
              )
            ORDER BY
                O.genera_fecha,
                O.boleta;
            """;

            const string QueryRequisiciones = """
                SELECT
                    R.cod_requisicion AS cod_orden,
                    'R' AS tipo_orden,
                    0 AS total,
                    R.Genera_User AS user_solicita,
                    R.Genera_Fecha AS fecha,
                    C.descripcion AS causa,
                    R.notas AS nota,
                    'P' AS proceso
                FROM pv_requisiciones R
                INNER JOIN pv_entrada_salida C
                    ON R.cod_entsal = C.cod_entsal
                WHERE R.autoriza_fecha IS NULL
                  AND R.Genera_User IN
                  (
                      SELECT usuario_asignado
                      FROM pv_orden_autousers
                      WHERE usuario = @Usuario
                  )
                  AND
                  (
                      @TodasFechas = 1
                      OR R.Genera_Fecha BETWEEN
                          @FechaInicio AND @FechaCorte
                  )
                ORDER BY
                    R.Genera_Fecha,
                    R.cod_requisicion;
                """;

            string tipo = filtros.tipo
                .Trim()
                .ToUpperInvariant();

            var parametros =
                INV_OrdenesAutorizacion_Filtros_Parametros_Obtener(
                    filtros,
                    tipo);

            ErrorDto<List<ResolucionTransaccionDto>> resultado;

            string query = tipo == TipoRequisicion
                ? QueryRequisiciones
                : QueryOrdenesInventario;

            resultado =
                DbHelper.ExecuteListQuery<ResolucionTransaccionDto>(
                    _portalDb,
                    CodEmpresa,
                    query,
                    parametros);

            return resultado.Code == 0
                ? DbHelper.CreateOkResponse(
                    resultado.Result ?? lista)
                : DbHelper.CreateErrorResponse(
                    resultado.Description ??
                    MensajeConsultaError,
                    resultado.Code.GetValueOrDefault(-1),
                    lista);
        }

        /// <summary>
        /// Autoriza las órdenes seleccionadas.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Información de las órdenes seleccionadas.</param>
        /// <returns>Resultado del proceso.</returns>
        public ErrorDto
            INV_OrdenesAutorizacion_Ordenes_Autorizar(
                int CodEmpresa,
                InvOrdenesAutorizacionProcesarRequest request)
        {
            return INV_OrdenesAutorizacion_Ordenes_Procesar(
                CodEmpresa,
                request,
                true);
        }

        /// <summary>
        /// Rechaza las órdenes seleccionadas.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Información de las órdenes seleccionadas.</param>
        /// <returns>Resultado del proceso.</returns>
        public ErrorDto
            INV_OrdenesAutorizacion_Ordenes_Rechazar(
                int CodEmpresa,
                InvOrdenesAutorizacionProcesarRequest request)
        {
            return INV_OrdenesAutorizacion_Ordenes_Procesar(
                CodEmpresa,
                request,
                false);
        }

        /// <summary>
        /// Ejecuta la autorización o rechazo de las órdenes seleccionadas.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Información de las órdenes seleccionadas.</param>
        /// <param name="autorizar">Indica si las órdenes deben autorizarse.</param>
        /// <returns>Resultado del proceso.</returns>
        private ErrorDto
            INV_OrdenesAutorizacion_Ordenes_Procesar(
                int CodEmpresa,
                InvOrdenesAutorizacionProcesarRequest? request,
                bool autorizar)
        {
            if (CodEmpresa <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeEmpresaRequerida,
                    CodigoValidacion);
            }

            if (request is null)
            {
                return DbHelper.ErrorResponse(
                    MensajeSolicitudRequerida,
                    CodigoValidacion);
            }

            string validacion =
                INV_OrdenesAutorizacion_Procesamiento_Validar(
                    request);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            string usuario = request.usuario.Trim();

            List<ResolucionTransaccionDto> ordenes =
                INV_OrdenesAutorizacion_Ordenes_Normalizar(
                    request.ordenes);

            string estado = autorizar
                ? EstadoAutorizado
                : EstadoRechazado;

            string mensajeExito = autorizar
                ? MensajeAutorizacionExitosa
                : MensajeRechazoExitoso;

            string mensajeError = autorizar
                ? MensajeAutorizarError
                : MensajeRechazarError;

            var resultado = DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                {
                    connection.Open();

                    using var transaction =
                        connection.BeginTransaction();

                    try
                    {
                        int registrosActualizados =
                            INV_OrdenesAutorizacion_Ordenes_Actualizar(
                                connection,
                                transaction,
                                ordenes,
                                usuario,
                                estado);

                        if (registrosActualizados != ordenes.Count)
                        {
                            transaction.Rollback();

                            return DbHelper.ErrorResponse(
                                MensajeOrdenNoDisponible,
                                CodigoValidacion);
                        }

                        transaction.Commit();

                        return DbHelper.OkResponse(
                            mensajeExito);
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                });

            return INV_OrdenesAutorizacion_Resultado_Obtener(
                resultado,
                mensajeError);
        }

        /// <summary>
        /// Actualiza todas las órdenes seleccionadas.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="ordenes">Órdenes seleccionadas.</param>
        /// <param name="usuario">Usuario que ejecuta el proceso.</param>
        /// <param name="estado">Estado que se aplicará.</param>
        /// <returns>Cantidad de registros actualizados.</returns>
        private static int
            INV_OrdenesAutorizacion_Ordenes_Actualizar(
                IDbConnection connection,
                IDbTransaction transaction,
                IEnumerable<ResolucionTransaccionDto> ordenes,
                string usuario,
                string estado)
        {
            int registrosActualizados = 0;

            foreach (var orden in ordenes)
            {
                registrosActualizados +=
                    INV_OrdenesAutorizacion_Orden_Actualizar(
                        connection,
                        transaction,
                        orden,
                        usuario,
                        estado);
            }

            return registrosActualizados;
        }

        /// <summary>
        /// Actualiza una orden de inventario o una requisición.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="orden">Orden que se actualizará.</param>
        /// <param name="usuario">Usuario que ejecuta el proceso.</param>
        /// <param name="estado">Estado que se aplicará.</param>
        /// <returns>Cantidad de registros actualizados.</returns>
        private static int
            INV_OrdenesAutorizacion_Orden_Actualizar(
                IDbConnection connection,
                IDbTransaction transaction,
                ResolucionTransaccionDto orden,
                string usuario,
                string estado)
        {
            const string QueryOrdenInventarioActualizar = """
                UPDATE pv_InvTranSac
                SET
                    autoriza_fecha = GetDate(),
                    autoriza_user = @Usuario,
                    estado = @Estado
                WHERE boleta = @CodOrden
                  AND tipo = @TipoOrden;
                """;

            const string QueryRequisicionActualizar = """
                UPDATE pv_requisiciones
                SET
                    autoriza_fecha = GetDate(),
                    autoriza_user = @Usuario,
                    estado = @Estado
                WHERE cod_requisicion = @CodOrden;
                """;

            var parametros = new
            {
                CodOrden = orden.cod_orden,
                TipoOrden = orden.tipo_orden,
                Usuario = usuario,
                Estado = estado
            };

            if (orden.tipo_orden == TipoRequisicion)
            {
                return connection.Execute(
                    QueryRequisicionActualizar,
                    parametros,
                    transaction);
            }

            return connection.Execute(
                QueryOrdenInventarioActualizar,
                parametros,
                transaction);
        }

        /// <summary>
        /// Valida los filtros utilizados para consultar las órdenes.
        /// </summary>
        /// <param name="filtros">Filtros recibidos.</param>
        /// <returns>Mensaje de validación o una cadena vacía.</returns>
        private static string
            INV_OrdenesAutorizacion_Filtros_Validar(
                InvOrdenesAutorizacionFiltros filtros)
        {
            if (string.IsNullOrWhiteSpace(filtros.tipo))
            {
                return MensajeTipoRequerido;
            }

            if (!INV_OrdenesAutorizacion_Tipo_EsValido(
                    filtros.tipo))
            {
                return MensajeTipoInvalido;
            }

            if (string.IsNullOrWhiteSpace(filtros.usuario))
            {
                return MensajeUsuarioRequerido;
            }

            return INV_OrdenesAutorizacion_Fechas_Validar(
                filtros);
        }

        /// <summary>
        /// Valida las fechas utilizadas en la consulta.
        /// </summary>
        /// <param name="filtros">Filtros recibidos.</param>
        /// <returns>Mensaje de validación o una cadena vacía.</returns>
        private static string
            INV_OrdenesAutorizacion_Fechas_Validar(
                InvOrdenesAutorizacionFiltros filtros)
        {
            if (filtros.todas_fechas)
            {
                return string.Empty;
            }

            if (!filtros.fecha_inicio.HasValue)
            {
                return MensajeFechaInicioRequerida;
            }

            if (!filtros.fecha_corte.HasValue)
            {
                return MensajeFechaCorteRequerida;
            }

            return filtros.fecha_inicio.Value >
                   filtros.fecha_corte.Value
                ? MensajeRangoFechasInvalido
                : string.Empty;
        }

        /// <summary>
        /// Valida la información utilizada para procesar las órdenes.
        /// </summary>
        /// <param name="request">Información recibida.</param>
        /// <returns>Mensaje de validación o una cadena vacía.</returns>
        private static string
            INV_OrdenesAutorizacion_Procesamiento_Validar(
                InvOrdenesAutorizacionProcesarRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return MensajeUsuarioRequerido;
            }

            List<ResolucionTransaccionDto> seleccionadas =
                request.ordenes?
                    .Where(orden =>
                        orden is not null &&
                        orden.seleccionado)
                    .ToList() ?? [];

            if (seleccionadas.Count == 0)
            {
                return MensajeOrdenesRequeridas;
            }

            return INV_OrdenesAutorizacion_Ordenes_Validar(
                seleccionadas);
        }

        /// <summary>
        /// Valida las órdenes seleccionadas.
        /// </summary>
        /// <param name="ordenes">Órdenes seleccionadas.</param>
        /// <returns>Mensaje de validación o una cadena vacía.</returns>
        private static string
            INV_OrdenesAutorizacion_Ordenes_Validar(
                List<ResolucionTransaccionDto> ordenes)
        {
            if (ordenes.Any(orden =>
                    string.IsNullOrWhiteSpace(
                        orden.cod_orden)))
            {
                return MensajeCodigoOrdenRequerido;
            }

            if (ordenes.Any(orden =>
                    !INV_OrdenesAutorizacion_Tipo_EsValido(
                        orden.tipo_orden)))
            {
                return MensajeTipoInvalido;
            }

            int ordenesDistintas = ordenes
                .Select(orden =>
                    string.Concat(
                        orden.tipo_orden
                            .Trim()
                            .ToUpperInvariant(),
                        "|",
                        orden.cod_orden.Trim()))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            return ordenesDistintas == ordenes.Count
                ? string.Empty
                : MensajeOrdenDuplicada;
        }

        /// <summary>
        /// Normaliza las órdenes seleccionadas.
        /// </summary>
        /// <param name="ordenes">Órdenes recibidas.</param>
        /// <returns>Órdenes seleccionadas y normalizadas.</returns>
        private static List<ResolucionTransaccionDto>
            INV_OrdenesAutorizacion_Ordenes_Normalizar(
                IEnumerable<ResolucionTransaccionDto> ordenes)
        {
            return ordenes
                .Where(orden => orden.seleccionado)
                .Select(orden =>
                    new ResolucionTransaccionDto
                    {
                        cod_orden =
                            orden.cod_orden.Trim(),
                        tipo_orden =
                            orden.tipo_orden
                                .Trim()
                                .ToUpperInvariant(),
                        seleccionado = true
                    })
                .ToList();
        }

        /// <summary>
        /// Obtiene los parámetros utilizados para consultar las órdenes.
        /// </summary>
        /// <param name="filtros">Filtros recibidos.</param>
        /// <param name="tipo">Tipo normalizado.</param>
        /// <returns>Parámetros utilizados por la consulta.</returns>
        private static object
            INV_OrdenesAutorizacion_Filtros_Parametros_Obtener(
                InvOrdenesAutorizacionFiltros filtros,
                string tipo)
        {
            DateTime fechaPredeterminada =
                DateTime.Today;

            return new
            {
                Tipo = tipo,
                Usuario = filtros.usuario.Trim(),
                TodasFechas = filtros.todas_fechas,
                FechaInicio =
                    filtros.fecha_inicio ??
                    fechaPredeterminada,
                FechaCorte =
                    filtros.fecha_corte ??
                    fechaPredeterminada
            };
        }

        /// <summary>
        /// Indica si el tipo de orden recibido es válido.
        /// </summary>
        /// <param name="tipo">Tipo de orden.</param>
        /// <returns>Verdadero cuando el tipo está permitido.</returns>
        private static bool
            INV_OrdenesAutorizacion_Tipo_EsValido(
                string? tipo)
        {
            string tipoNormalizado =
                tipo?.Trim().ToUpperInvariant() ??
                string.Empty;

            return tipoNormalizado is
                TipoEntrada or
                TipoSalida or
                TipoTraspaso or
                TipoRequisicion;
        }

        /// <summary>
        /// Convierte el resultado interno en la respuesta final.
        /// </summary>
        /// <param name="resultado">Resultado generado por DbHelper.</param>
        /// <param name="mensajeError">Mensaje de error predeterminado.</param>
        /// <returns>Resultado final del proceso.</returns>
        private static ErrorDto
            INV_OrdenesAutorizacion_Resultado_Obtener(
                ErrorDto<ErrorDto> resultado,
                string mensajeError)
        {
            return resultado.Code == 0 &&
                   resultado.Result is not null
                ? resultado.Result
                : DbHelper.ErrorResponse(
                    resultado.Description ??
                    mensajeError,
                    resultado.Code.GetValueOrDefault(-1));
        }
    }
}