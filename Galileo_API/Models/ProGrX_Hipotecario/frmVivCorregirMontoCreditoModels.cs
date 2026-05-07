namespace Galileo_API.Models.ProGrX_Hipotecario
{
    public class FrmVivCorregirMontoCreditoGuardarContexto
    {
        public int plazo { get; set; } = 0;
        public decimal tasa { get; set; } = 0;
        public decimal cuota_actual { get; set; } = 0;
    }

    public class FrmVivCorregirMontoCreditoResponse
    {
        public long numero_operacion { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal monto_credito { get; set; } = 0;
        public decimal monto_no_gravable { get; set; } = 0;
        public int plazo { get; set; } = 0;
        public decimal tasa { get; set; } = 0;
        public decimal cuota { get; set; } = 0;
        public string estado_operacion { get; set; } = string.Empty;
    }

    public class FrmVivCorregirMontoCreditoGuardarRequest
    {
        public long numero_operacion { get; set; } = 0;
        public decimal monto_credito { get; set; } = 0;
        public decimal monto_no_gravable { get; set; } = 0;
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class FrmVivCorregirMontoCreditoGuardarResponse
    {
        public decimal cuota { get; set; } = 0;
        public string mensaje { get; set; } = string.Empty;
    }
}
