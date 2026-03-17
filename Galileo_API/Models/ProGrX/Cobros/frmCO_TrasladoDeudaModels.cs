namespace Galileo.Models.ProGrX.Cobros
{
    public class CoTrasladoDeudaDetalleDto
    {
        public string tipo { get; set; } = "";
        public string identificacion { get; set; } = "";
        public string nombre { get; set; } = "";
        public decimal porcentaje { get; set; }
        public decimal monto { get; set; }
        public decimal cuota { get; set; }
        public string estado { get; set; } = "";
        public decimal recuperado { get; set; }
        public long id_retencion { get; set; }
        public string estado_codigo { get; set; } = "";
        public int opex { get; set; }
    }

    public class CoTrasladoDeudaObtenerDto
    {
        public long id_solicitud { get; set; }

        public string operacion { get; set; } = "";
        public string linea { get; set; } = "";
        public string linea_descripcion { get; set; } = "";
        public string identificacion { get; set; } = "";
        public string nombre { get; set; } = "";
        public string proceso { get; set; } = "";
        public string opex { get; set; } = "";
        public string divisa { get; set; } = "";
        public decimal saldo { get; set; }
        public decimal intereses { get; set; }
        public decimal cargos_registrados { get; set; }
        public decimal polizas_atrasadas { get; set; }
        public decimal total_deuda { get; set; }
        public decimal total_recuperado { get; set; }
        public string linea_cobro { get; set; } = "";
        public string linea_cobro_descripcion { get; set; } = "";
        public int plazo { get; set; }
        public decimal tasa { get; set; }
        public decimal tasa_original { get; set; }
        public int plazo_original { get; set; }
        public decimal porcentaje_asignado { get; set; }
        public decimal interes_corriente { get; set; }
        public decimal interes_moratorio { get; set; }
        public decimal interes_pendiente { get; set; }
        public decimal principal_mora { get; set; }
        public decimal tbp_puntos_add { get; set; }
        public int liq_tasa { get; set; }
        public string tasa_label { get; set; } = "";

        public List<CoTrasladoDeudaDetalleDto> detalle { get; set; } = new();
    }

    public class CoTrasladoDeudaCalcularRequest
    {
        public long id_solicitud { get; set; }
        public int plazo { get; set; }
        public decimal tasa { get; set; }
        public decimal total_deuda { get; set; }
        public List<CoTrasladoDeudaDetalleDto> detalle { get; set; } = new();
    }

    public class CoTrasladoDeudaCalcularResponse
    {
        public decimal porcentaje_asignado { get; set; }
        public decimal total_deuda { get; set; }
        public decimal total_recuperado { get; set; }
        public List<CoTrasladoDeudaDetalleDto> detalle { get; set; } = new();
    }

    public class CoTrasladoDeudaAplicarRequest
    {
        public long id_solicitud { get; set; }
        public int plazo { get; set; }
        public decimal tasa { get; set; }
        public string? notas { get; set; }
        public string? usuario { get; set; }
        public List<CoTrasladoDeudaDetalleDto> detalle { get; set; } = new();
    }

    public class CoTrasladoDeudaAplicarResponse
    {
        public string tipo_documento { get; set; } = "";
        public string documento { get; set; } = "";
        public string mensaje { get; set; } = "";
        public long id_solicitud { get; set; }
        public decimal total_aplicado { get; set; }
    }

    public class CoTrasladoDeudaExportRequest
    {
        public long id_solicitud { get; set; }
        public int plazo { get; set; }
        public decimal tasa { get; set; }
        public decimal total_deuda { get; set; }
        public List<CoTrasladoDeudaDetalleDto> detalle { get; set; } = new();
    }

    public class CoTrasladoDeudaExportResponse
    {
        public long id_solicitud { get; set; }
        public string operacion { get; set; } = "";
        public string linea { get; set; } = "";
        public string identificacion { get; set; } = "";
        public string nombre { get; set; } = "";
        public decimal porcentaje_asignado { get; set; }
        public decimal total_deuda { get; set; }
        public decimal total_recuperado { get; set; }
        public List<CoTrasladoDeudaDetalleDto> detalle { get; set; } = new();
    }
}