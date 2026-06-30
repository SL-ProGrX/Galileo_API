namespace Galileo.Models
{
    public class MTesTransaccionDto
    {
        public decimal monto { get; set; }
        public long nSolicitud { get; set; }
        public bool doc_auto { get; set; }
        public string comprobante { get; set; } = string.Empty;
        public decimal firmas_desde { get; set; }
        public decimal firmas_hasta { get; set; }
        public string lugar_emision { get; set; } = string.Empty;
        public string tipoX { get; set; } = string.Empty;
        public DateTime fechaX { get; set; }
        public int id_banco { get; set; }
        public string tipo { get; set; } = string.Empty;
        public string modulo { get; set; } = string.Empty;
        public int? op { get; set; }
        public int? referencia { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string subModulo { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
    }

    public class TesReporteTransferenciaDto
    {
        public long registros { get; set; }
        public string montoLetras { get; set; } = string.Empty;
        public decimal totalMonto { get; set; }
        public string fxNombre { get; set; } = string.Empty;
        public string fxPuesto { get; set; } = string.Empty;
        public string fxDepartamento { get; set; } = string.Empty;
        public string letras1 { get; set; } = string.Empty;
        public string fecha { get; set; } = string.Empty;
        public string empresa { get; set; } = string.Empty;
        public string banco { get; set; } = string.Empty;
        public string transferencia { get; set; } = string.Empty;
    }

    public class ProcesarComprobanteParametros
    {
        public int codEmpresa { get; set; }
        public string? usuario { get; set; }
        public int modulo { get; set; }
        public int solicitud { get; set; }
        public string? documentoManual { get; set; }
    }

    public class SbTeBcrParametros
    {
        public int vBanco { get; set; } = 0;
        public string vTipoDoc { get; set; } = string.Empty;
        public int cantidadSolicitudes { get; set; }
        public int? solInicio { get; set; }
        public int? solCorte { get; set; }
        public DateTime? fechaInicio { get; set; }
        public DateTime? fechaCorte { get; set; }
    }
}
