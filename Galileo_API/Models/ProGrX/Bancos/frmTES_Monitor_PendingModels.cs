namespace PgxAPI.Models.TES
{
    public class TesMonitorPending
    {
        public required string modulo { get; set; }
        public required string modulo_desc { get; set; }
        public decimal casos { get; set; }
        public decimal monto { get; set; }
    }
}