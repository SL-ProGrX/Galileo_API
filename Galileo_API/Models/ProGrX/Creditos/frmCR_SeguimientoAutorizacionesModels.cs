namespace Galileo_API.Models.ProGrX.Credito
{
    public class CrSeguimientoAutorizacionesDetalleData
    {
        public int operacion { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public string linea_desc { get; set; } = string.Empty;
        public string cod_destino { get; set; } = string.Empty;
        public string destino_desc { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal montoapr { get; set; } = 0;
        public int plazo { get; set; } = 0;
        public decimal tasa { get; set; } = 0;
        public decimal cuota { get; set; } = 0;
        public string garantia { get; set; } = string.Empty;
        public DateTime? fecha_sol { get; set; }
        public string user_rec { get; set; } = string.Empty;
        public string observacion { get; set; } = string.Empty;
        public bool puede_autorizar { get; set; } = false;
    }

    public class CrSeguimientoAutorizacionesAutorizarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public int operacion { get; set; } = 0;
        public string notas { get; set; } = string.Empty;
    }
}