using Galileo.DataBaseTier;


namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public partial class FrmCntXExploradorContableDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityDb;

        private sealed record AsientoBorrarInfo(
            byte[]? ts,
            DateTime? fecha_aplicado,
            string? modulo,
            DateTime? fecha_autoriza,
            int anio,
            int mes);

        private static string? NormalizarCuenta(string? cuenta)
        {
            if (string.IsNullOrWhiteSpace(cuenta))
            {
                return null;
            }

            return cuenta.Replace("-", "").Replace(".", "").Replace(" ", "").Trim();
        }

        public FrmCntXExploradorContableDb(IConfiguration config)
            : this(new PortalDB(config), new MSecurityMainDb(config)) { }

        public FrmCntXExploradorContableDb(
            PortalDB portalDb,
            MSecurityMainDb securityDb)
        {
            _portalDb = portalDb;
            _securityDb = securityDb;
        }

      
    }
}
