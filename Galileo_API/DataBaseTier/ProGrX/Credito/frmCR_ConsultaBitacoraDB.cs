namespace PgxAPI.DataBaseTier.ProGrX.Credito
{
    public class frmCR_ConsultaBitacoraDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 3; // Modulo de Créditos

        public frmCR_ConsultaBitacoraDB(IConfiguration? config)
        {
            _config = config;
        }
    }
}
