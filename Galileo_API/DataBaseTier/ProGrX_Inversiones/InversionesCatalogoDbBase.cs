using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Inversiones
{
    /// <summary>
    /// Base común de acceso al portal, validación y resultado con bitácora de los catálogos de Inversiones.
    /// Cada catálogo conserva sus consultas, parámetros y mensajes específicos.
    /// </summary>
    public abstract class InversionesCatalogoDbBase
    {
        protected const int CodigoMensajeControlado = -2;
        protected readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly string _mensajeCodigoRequerido;
        private readonly string _detalleMovimiento;

        /// <summary>
        /// Configura los mensajes propios del catálogo y su acceso a la bitácora de Inversiones.
        /// </summary>
        /// <param name="config">Configuración de acceso al portal y la bitácora.</param>
        /// <param name="mensajeCodigoRequerido">Mensaje de validación del identificador.</param>
        /// <param name="detalleMovimiento">Prefijo del detalle de bitácora, incluido su separador.</param>
        protected InversionesCatalogoDbBase(
            IConfiguration config,
            string mensajeCodigoRequerido,
            string detalleMovimiento)
        {
            ArgumentNullException.ThrowIfNull(config);
            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
            _mensajeCodigoRequerido = mensajeCodigoRequerido;
            _detalleMovimiento = detalleMovimiento;
        }

        /// <summary>
        /// Valida el código y el usuario antes de ejecutar una modificación del catálogo.
        /// </summary>
        /// <param name="codigo">Identificador del registro.</param>
        /// <param name="usuario">Usuario que realiza el movimiento.</param>
        /// <returns>Error controlado si falta un dato requerido; nulo cuando ambos son válidos.</returns>
        protected ErrorDto? ValidarIdentificacion(string? codigo, string? usuario)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return DbHelper.ErrorResponse(
                    _mensajeCodigoRequerido, CodigoMensajeControlado);
            }
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.ErrorResponse(
                    "El usuario es requerido.", CodigoMensajeControlado);
            }
            return null;
        }

        /// <summary>
        /// Conserva los errores de ejecución y registra la bitácora únicamente si hubo un movimiento exitoso.
        /// </summary>
        /// <param name="codEmpresa">Empresa donde se realizó el movimiento.</param>
        /// <param name="resultado">Resultado y cantidad de filas afectadas por la operación.</param>
        /// <param name="usuario">Usuario previamente validado.</param>
        /// <param name="movimiento">Acción que debe registrarse en la bitácora.</param>
        /// <param name="codigo">Código del registro afectado.</param>
        /// <param name="mensajeSinCambios">Mensaje propio del catálogo cuando no se afectaron filas.</param>
        /// <returns>Resultado controlado del movimiento, con el detalle técnico cuando existe.</returns>
        protected ErrorDto ResultadoMovimiento(
            int codEmpresa,
            ErrorDto<int> resultado,
            string usuario,
            string movimiento,
            string codigo,
            string mensajeSinCambios)
        {
            if (resultado.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    resultado.Description ?? "No fue posible completar el proceso.");
            }
            if (resultado.Result == 0)
            {
                return DbHelper.ErrorResponse(mensajeSinCambios, CodigoMensajeControlado);
            }

            _ = _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario.Trim().ToUpperInvariant(),
                Modulo = 22,
                Movimiento = movimiento,
                DetalleMovimiento = $"{_detalleMovimiento}{codigo.Trim()}"
            });

            return DbHelper.OkResponse("Proceso concluido satisfactoriamente.");
        }
    }
}
