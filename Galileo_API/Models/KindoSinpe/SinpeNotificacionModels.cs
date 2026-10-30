using System.Net;

namespace Galileo.Models.KindoSinpe
{
    public enum TipoPlantillaSinpe
    {
        TranAs,
        TranBe,
        TranPr,
        TranI,
        TranS,
        TranSs,
        TranRe
    }

    public class PlantillaNotificacionSinpe
    {
        public string TipoPlantilla { get; set; } = string.Empty;
        public string? Asunto { get; set; }
        public string? Plantilla { get; set; }
        public int? Canal { get; set; }
        public int? Prioridad { get; set; }
    }

    /// <summary>
    /// Valores que reemplazan los @@dato@@ de las plantillas de notificación SINPE.
    /// Cada propiedad corresponde a un token; las no asignadas salen vacías.
    /// </summary>
    public class SinpeNotificacionDatos
    {
        public string? NombreLogin { get; set; }
        public string? NSolicitud { get; set; }
        public string? FechaHora { get; set; }
        public string? TipodeMovimiento { get; set; }
        public string? PersonaDestino { get; set; }
        public string? CtaIBAN { get; set; }
        public string? RefTransaccion { get; set; }
        public string? MontoTransaccion { get; set; }
        public string? Moneda { get; set; }
        public string? Detalle { get; set; }
        public string? CedulaOrigen { get; set; }
        public string? CedulaDestino { get; set; }
        public string? IBAN { get; set; }
        public string? TipoTransaccion { get; set; }
        public string? NumeroInternoTransaccion { get; set; }
        public string? ComisionTransaccion { get; set; }
        public string? NombreOrigenTransaccion { get; set; }
        public string? NombreDestinoTransaccion { get; set; }
        public string? EstadoTransaccion { get; set; }
        public string? CedulaOrigenTransaccion { get; set; }

        /// <summary>
        /// Mapa token → valor HTML-codificado. Incluye todos los tokens conocidos.
        /// </summary>
        public Dictionary<string, string> ToTokens() => new()
        {
            ["NombreLogin"] = Codificar(NombreLogin),
            ["NSolicitud"] = Codificar(NSolicitud),
            ["FechaHora"] = Codificar(FechaHora),
            ["TipodeMovimiento"] = Codificar(TipodeMovimiento),
            ["PersonaDestino"] = Codificar(PersonaDestino),
            ["CtaIBAN"] = Codificar(CtaIBAN),
            ["RefTransaccion"] = Codificar(RefTransaccion),
            ["MontoTransaccion"] = Codificar(MontoTransaccion),
            ["Moneda"] = Codificar(Moneda),
            ["Detalle"] = Codificar(Detalle),
            ["CedulaOrigen"] = Codificar(CedulaOrigen),
            ["CedulaDestino"] = Codificar(CedulaDestino),
            ["IBAN"] = Codificar(IBAN),
            ["TipoTransaccion"] = Codificar(TipoTransaccion),
            ["NumeroInternoTransaccion"] = Codificar(NumeroInternoTransaccion),
            ["ComisionTransaccion"] = Codificar(ComisionTransaccion),
            ["NombreOrigenTransaccion"] = Codificar(NombreOrigenTransaccion),
            ["NombreDestinoTransaccion"] = Codificar(NombreDestinoTransaccion),
            ["EstadoTransaccion"] = Codificar(EstadoTransaccion),
            ["CedulaOrigenTransaccion"] = Codificar(CedulaOrigenTransaccion)
        };

        private static string Codificar(string? valor) => WebUtility.HtmlEncode(valor ?? string.Empty);
    }
}
