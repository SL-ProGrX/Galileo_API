namespace PgxAPI.DataBaseTier.ProGrX.Credito
{
    public class frmCR_VerificaDatosPersonalesDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 3; // Modulo de Créditos

        public frmCR_VerificaDatosPersonalesDB(IConfiguration? config)
        {
            _config = config;
        }
    }
}
