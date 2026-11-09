using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public partial class FrmArfOperacionesDb
    {
        /// <summary>
        /// Calcula la fecha de finalización a partir de la fecha inicial y el plazo en meses.
        /// </summary>
        /// <param name="fechaInicio">Fecha en que inicia la operación.</param>
        /// <param name="plazo">Cantidad de meses de vigencia.</param>
        /// <returns>Fecha calculada o la fecha inicial cuando no hay un plazo válido.</returns>
        private static DateTime? CalcularFechaFinaliza(DateTime? fechaInicio, short? plazo)
        {
            if (!fechaInicio.HasValue || plazo.GetValueOrDefault() <= 0)
            {
                return fechaInicio;
            }

            return fechaInicio.Value.AddMonths(plazo.GetValueOrDefault());
        }

        /// <summary>
        /// Ejecuta las validaciones requeridas antes de guardar una operación.
        /// </summary>
        /// <param name="request">Datos de la operación que se desea guardar.</param>
        /// <param name="estadoActual">Estado actual registrado para la operación.</param>
        private static void ValidarGuardarRequest(ArfOperacionGuardarRequestDto request, string estadoActual)
        {
            ValidarCamposObligatorios(request);
            ValidarRangosPrincipales(request);
            ValidarIncrementos(request);
            ValidarFechas(request);
            ValidarEstadoEditable(estadoActual);
        }

        /// <summary>
        /// Valida que la operación consultada cumpla las mismas reglas del formulario original antes de activarse.
        /// </summary>
        /// <param name="registro">Datos actuales de la operación que se desea activar.</param>
        private static void ValidarActivacion(ArfOperacionRegistroDto? registro)
        {
            if (registro is null || registro.operacion.GetValueOrDefault() <= 0)
                throw new ArgumentException("No se encontró la operación que se desea activar.", nameof(registro));

            if (!string.Equals(registro.estado, "R", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("La operación debe encontrarse en estado recibido para ser activada.");

            var request = new ArfOperacionGuardarRequestDto
            {
                operacion = registro.operacion,
                cod_acreedor = registro.cod_acreedor,
                cod_local = registro.cod_local,
                tasa_descuento = registro.tasa_descuento,
                tasa_interes = registro.tasa_interes,
                periodicidad = registro.periodicidad,
                cuota = registro.cuota,
                plazo = registro.plazo,
                fecha_inicio = registro.fecha_inicio,
                fecha_finaliza = registro.fecha_finaliza,
                deposito_garantia_monto = registro.deposito_garantia_monto,
                deposito_garantia_ind = registro.deposito_garantia_ind,
                incremento_tipo = registro.incremento_tipo,
                incremento_valor = registro.incremento_valor,
                cod_divisa = registro.cod_divisa
            };

            ValidarCamposObligatorios(request);
            ValidarRangosPrincipales(request);
            ValidarIncrementos(request);
            ValidarFechas(request);
        }

        /// <summary>
        /// Valida los campos obligatorios del arrendamiento.
        /// </summary>
        /// <param name="request">Datos de la operación que se desea guardar.</param>
        private static void ValidarCamposObligatorios(ArfOperacionGuardarRequestDto request)
        {
            if (request.cod_acreedor.GetValueOrDefault() <= 0)
                throw new ArgumentException("No se ha especificado un Arrendador.", nameof(request));

            if (string.IsNullOrWhiteSpace(request.cod_local))
                throw new ArgumentException("No se ha especificado una Unidad/Local.", nameof(request));
        }

        /// <summary>
        /// Valida los montos, tasas y plazos principales de la operación.
        /// </summary>
        /// <param name="request">Datos de la operación que se desea guardar.</param>
        private static void ValidarRangosPrincipales(ArfOperacionGuardarRequestDto request)
        {
            if (request.cuota.GetValueOrDefault() <= 0)
                throw new ArgumentOutOfRangeException(nameof(request), "El Monto no es válido.");

            if (!EstaEntreCeroYCien(request.tasa_descuento))
                throw new ArgumentOutOfRangeException(nameof(request), "La Tasa Descuento no es válida.");

            if (!EstaEntreCeroYCien(request.tasa_interes))
                throw new ArgumentOutOfRangeException(nameof(request), "La Tasa de Interés no es válida.");

            if (request.plazo.GetValueOrDefault() <= 0)
                throw new ArgumentOutOfRangeException(nameof(request), "El Plazo no es válido.");

            if (request.deposito_garantia_monto.GetValueOrDefault() < 0)
                throw new ArgumentOutOfRangeException(nameof(request), "El dato del depósito de garantía no es válido.");
        }

        /// <summary>
        /// Valida el valor del incremento según su tipo.
        /// </summary>
        /// <param name="request">Datos de la operación que se desea guardar.</param>
        private static void ValidarIncrementos(ArfOperacionGuardarRequestDto request)
        {
            if (string.Equals(request.incremento_tipo, "P", StringComparison.OrdinalIgnoreCase) &&
                !EstaEntreCeroYCien(request.incremento_valor))
            {
                throw new ArgumentOutOfRangeException(nameof(request), "El Porcentaje de Incremento Anual no es válido.");
            }

            if (string.Equals(request.incremento_tipo, "M", StringComparison.OrdinalIgnoreCase) &&
                request.incremento_valor.GetValueOrDefault() < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(request), "El Monto del Incremento Anual no es válido.");
            }
        }

        /// <summary>
        /// Valida el rango de vigencia de la operación.
        /// </summary>
        /// <param name="request">Datos de la operación que se desea guardar.</param>
        private static void ValidarFechas(ArfOperacionGuardarRequestDto request)
        {
            if (request.fecha_inicio >= request.fecha_finaliza)
                throw new ArgumentException("Rango de Fechas Erróneo, verificar.", nameof(request));
        }

        /// <summary>
        /// Verifica que el estado actual permita modificar la operación.
        /// </summary>
        /// <param name="estadoActual">Estado actual registrado para la operación.</param>
        private static void ValidarEstadoEditable(string estadoActual)
        {
            var esRecibida = string.Equals(estadoActual, "R", StringComparison.OrdinalIgnoreCase);
            var esPendiente = string.Equals(estadoActual, "P", StringComparison.OrdinalIgnoreCase);

            if (!esRecibida && !esPendiente)
            {
                throw new InvalidOperationException("Esta Operación no puede ser modificada porque no se encuentra en estado de recibido.");
            }
        }

        /// <summary>
        /// Determina si un valor está dentro del rango porcentual permitido.
        /// </summary>
        /// <param name="valor">Valor que se desea validar.</param>
        /// <returns>Verdadero cuando el valor está entre cero y cien.</returns>
        private static bool EstaEntreCeroYCien(decimal? valor)
        {
            return valor.HasValue && valor.Value >= 0 && valor.Value <= 100;
        }
    }
}
