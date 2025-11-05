namespace PgxAPI.DataBaseTier.ProGrX.Credito
{
    public class frmCR_SeguimientoTramitesDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 3; // Modulo de Créditos

        public frmCR_SeguimientoTramitesDB(IConfiguration? config)
        {
            _config = config;
        }
    }
}
