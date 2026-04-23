namespace Galileo_API.Models.ProGrX_Hipotecario
{
    public class VivDesembolsoHeaderDto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;

        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;

        public decimal? Bruto { get; set; }
        public decimal? IntAcumulado { get; set; }
        public decimal? IntSDisponible { get; set; }
        public decimal? GiroMaximo { get; set; }

        public decimal? monto_girado { get; set; }
    }

    public class VivDesembolsoDto
    {

        public int codigo { get; set; }
        public string beneficiario { get; set; } = string.Empty;

        public decimal monto { get; set; }
        public decimal disponible { get; set; }

        public DateTime? fechacorte { get; set; }

        public string usuario { get; set; } = string.Empty;
    }

    public class VivDesembolsoPendienteDto
    {
        public string linea { get; set; } = string.Empty;
        public int? idcontacto { get; set; }
        public int? garantia { get; set; }
        public string cedula { get; set; } = string.Empty;

        public string concepto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string destipo { get; set; } = string.Empty;

        public string beneficiario { get; set; } = string.Empty;

        public decimal monto { get; set; }
        public decimal descuento { get; set; }
        public decimal montogiro { get; set; }

        public string cuenta { get; set; } = string.Empty;
        public string aplicainteres { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;

        public DateTime? fecha { get; set; }
    }

    public class ConceptoApiDto
    {
        public string item { get; set; } = "";
        public string descripcion { get; set; } = "";
        public int aplicaIntereses { get; set; }
    }

    public class ActivarDesembolsoPendienteRequestDto
    {
        public int contacto { get; set; }
        public int garantia { get; set; }
        public string tipo { get; set; } = string.Empty;
        public int linea { get; set; }
        public string usuario { get; set; } = string.Empty;

        public decimal descuento { get; set; }
        public decimal disponible { get; set; }

        public string fechaCorte { get; set; } = string.Empty;
        public string fechaUltimoCorte { get; set; } = string.Empty;

        public int diasActuales { get; set; }
        public decimal interesesActuales { get; set; }

        public int diasAcumulados { get; set; }
        public decimal interesesAcumulados { get; set; }
    }


    public class CambioPendienteResponseDto
    {
        public int Pass { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string Movimiento { get; set; } = string.Empty;
    }

    public class CambioPendienteRequestDto
    {
        public int garantia { get; set; }
        public string tipo { get; set; } = string.Empty;
        public int linea { get; set; }
        public int contacto { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class AgregarPendienteRequestDto
    {
        public int contacto { get; set; }
        public int garantia { get; set; }
        public string tipoDesembolso { get; set; } = string.Empty;
        public string beneficiario { get; set; } = string.Empty;

        public decimal monto { get; set; }
        public int aplicaIntereses { get; set; }

        public string usuario { get; set; } = string.Empty;
        public string fecha { get; set; } = string.Empty;

        public decimal disponible { get; set; }
    }

    public class DesembolsoDetalleDto
    {
        public int codigodesembolso { get; set; }
        public string codigo { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class ViviendaDesembolsoRequestDto
    {
        public int codigodesembolso { get; set; }
        public int numeroOperacion { get; set; }
        public string registroUsuario { get; set; } = string.Empty;
        public string beneficiario { get; set; } = string.Empty;

        public decimal monto { get; set; }
        public string? detalle { get; set; }
        public decimal disponible { get; set; }

        public int aplicaIntereses { get; set; }

        public string fechaCorte { get; set; } = string.Empty;
        public string fechaUltimoCorte { get; set; } = string.Empty;

        public int interesesActualDias { get; set; }
        public decimal interesesActualMonto { get; set; }

        public int interesesAcumDias { get; set; }
        public decimal interesesAcumMonto { get; set; }

        public int bancoCodigo { get; set; }
        public string bancoEmitir { get; set; } = string.Empty;
        public string bancoCuenta { get; set; } = string.Empty;
        public string identificacion { get; set; } = string.Empty;

        public List<DesembolsoDetalleDto> detalles { get; set; } = new();
    }
}
