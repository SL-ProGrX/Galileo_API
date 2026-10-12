using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Galileo.Models.Security;
using System.Data;

namespace Galileo.DataBaseTier
{
    public sealed class FrmInvVentasEnLineaSincDb
    {
        private const int CodigoValidacion = -2;
        private const int ModuloInventarios = 32;
        private const int TiempoEsperaSincronizacion = 600;

        private const string ProcesoCatalogoExistencias =
            "catalogo_existencias";

        private const string ProcesoVentasExistencias =
            "ventas_existencias";

        private const string MensajeSincronizacionExitosa =
            "Sincronizaci&oacute;n realizada satisfactoriamente.";

        private const string MensajeRequestRequerido =
            "La informaci&oacute;n de la sincronizaci&oacute;n es requerida.";

        private const string MensajeUsuarioRequerido =
            "El usuario que realiza la sincronizaci&oacute;n es requerido.";

        private const string MensajeErrorSincronizacion =
            "Ocurri&oacute; un error al realizar la sincronizaci&oacute;n. "
            + "Verifique que su empresa est&eacute; configurada para Ventas en L&iacute;nea.";

        private const string DetalleCatalogoExistencias =
            "Sincronizaci&oacute;n de Ventas en L&iacute;nea: "
            + "Cat&aacute;logos de Productos";

        private const string DetalleVentasExistencias =
            "Sincronizaci&oacute;n de Ventas en L&iacute;nea: "
            + "Ventas y Existencias";

        private const string SqlCatalogoExistencias = """
            EXEC spVentas_EnLinea_Sincroniza_Catalogo_Existencias 0;
            """;

        private const string SqlVentasExistencias = """
            EXEC spVentas_EnLinea_Sincroniza_Ventas;
            """;

        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;

        /// <summary>
        /// Inicializa el acceso a datos del proceso de sincronización
        /// de Ventas en Línea.
        /// </summary>
        /// <param name="config">
        /// Configuración de conexiones del sistema.
        /// </param>
        public FrmInvVentasEnLineaSincDb(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Sincroniza el catálogo de productos y sus existencias
        /// con el sistema de Ventas en Línea.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa que ejecuta el proceso.
        /// </param>
        /// <param name="request">
        /// Información del usuario que solicita la sincronización.
        /// </param>
        /// <returns>
        /// Resultado de la sincronización.
        /// </returns>
        public ErrorDto<InvVentasEnLineaSincData>
            INV_VentasEnLineaSinc_CatalogoExistencias_Sincronizar(
                int codEmpresa,
                InvVentasEnLineaSincRequest? request)
        {
            var proceso = new InvVentasEnLineaSincProcesoConfig(
                ProcesoCatalogoExistencias,
                SqlCatalogoExistencias,
                DetalleCatalogoExistencias);

            return INV_VentasEnLineaSinc_Proceso_Ejecutar(
                codEmpresa,
                request,
                proceso);
        }

        /// <summary>
        /// Sincroniza las ventas, la facturación y las existencias
        /// con el sistema de Ventas en Línea.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa que ejecuta el proceso.
        /// </param>
        /// <param name="request">
        /// Información del usuario que solicita la sincronización.
        /// </param>
        /// <returns>
        /// Resultado de la sincronización.
        /// </returns>
        public ErrorDto<InvVentasEnLineaSincData>
            INV_VentasEnLineaSinc_VentasExistencias_Sincronizar(
                int codEmpresa,
                InvVentasEnLineaSincRequest? request)
        {
            var proceso = new InvVentasEnLineaSincProcesoConfig(
                ProcesoVentasExistencias,
                SqlVentasExistencias,
                DetalleVentasExistencias);

            return INV_VentasEnLineaSinc_Proceso_Ejecutar(
                codEmpresa,
                request,
                proceso);
        }

        /// <summary>
        /// Valida y ejecuta uno de los procesos de sincronización
        /// de Ventas en Línea.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa que ejecuta el proceso.
        /// </param>
        /// <param name="request">
        /// Información del usuario solicitante.
        /// </param>
        /// <param name="proceso">
        /// Configuración del proceso que debe ejecutarse.
        /// </param>
        /// <returns>
        /// Resultado de la ejecución.
        /// </returns>
        private ErrorDto<InvVentasEnLineaSincData>
            INV_VentasEnLineaSinc_Proceso_Ejecutar(
                int codEmpresa,
                InvVentasEnLineaSincRequest? request,
                InvVentasEnLineaSincProcesoConfig proceso)
        {
            string validacion =
                INV_VentasEnLineaSinc_Request_Validar(request);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.CreateErrorResponse<
                    InvVentasEnLineaSincData>(
                        validacion,
                        CodigoValidacion);
            }

            string usuario = request!.usuario!.Trim();

            var ejecucion = DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                {
                    connection.Execute(
                        proceso.Sql,
                        commandTimeout:
                            TiempoEsperaSincronizacion,
                        commandType:
                            CommandType.Text);

                    return true;
                });

            if (ejecucion.Code != 0)
            {
                return DbHelper.CreateErrorResponse<
                    InvVentasEnLineaSincData>(
                        INV_VentasEnLineaSinc_Error_Obtener(
                            ejecucion.Description),
                        ejecucion.Code.GetValueOrDefault(-1));
            }

            INV_VentasEnLineaSinc_Bitacora_Registrar(
                codEmpresa,
                usuario,
                proceso.DetalleBitacora);

            var resultado = new InvVentasEnLineaSincData
            {
                proceso = proceso.Codigo,
                mensaje = MensajeSincronizacionExitosa
            };

            return DbHelper.CreateOkResponse(
                resultado,
                MensajeSincronizacionExitosa);
        }

        /// <summary>
        /// Valida la información requerida para ejecutar
        /// una sincronización.
        /// </summary>
        /// <param name="request">
        /// Información de la solicitud.
        /// </param>
        /// <returns>
        /// Mensaje de validación o una cadena vacía.
        /// </returns>
        private static string
            INV_VentasEnLineaSinc_Request_Validar(
                InvVentasEnLineaSincRequest? request)
        {
            if (request is null)
            {
                return MensajeRequestRequerido;
            }

            return string.IsNullOrWhiteSpace(request.usuario)
                ? MensajeUsuarioRequerido
                : string.Empty;
        }

        /// <summary>
        /// Construye el mensaje de error del proceso conservando
        /// el detalle técnico retornado por el proveedor SQL.
        /// </summary>
        /// <param name="detalleTecnico">
        /// Detalle técnico generado durante la ejecución.
        /// </param>
        /// <returns>
        /// Mensaje funcional con el detalle técnico disponible.
        /// </returns>
        private static string
            INV_VentasEnLineaSinc_Error_Obtener(
                string? detalleTecnico)
        {
            string detalle =
                detalleTecnico?.Trim() ?? string.Empty;

            return string.IsNullOrEmpty(detalle)
                ? MensajeErrorSincronizacion
                : $"{MensajeErrorSincronizacion} Detalle: {detalle}";
        }

        /// <summary>
        /// Registra en bitácora una sincronización finalizada
        /// correctamente.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="usuario">
        /// Usuario que ejecutó la sincronización.
        /// </param>
        /// <param name="detalle">
        /// Descripción del proceso ejecutado.
        /// </param>
        private void
            INV_VentasEnLineaSinc_Bitacora_Registrar(
                int codEmpresa,
                string usuario,
                string detalle)
        {
            _securityMainDb.Bitacora(
                new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario,
                    Movimiento = "Aplica",
                    DetalleMovimiento = detalle,
                    Modulo = ModuloInventarios
                });
        }

        private sealed record
            InvVentasEnLineaSincProcesoConfig(
                string Codigo,
                string Sql,
                string DetalleBitacora);
    }
}