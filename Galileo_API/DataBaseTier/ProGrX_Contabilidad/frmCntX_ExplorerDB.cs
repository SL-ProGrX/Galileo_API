using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad.Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public partial class FrmCntXExploradorContableDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityDb;

        private sealed class AsientoBorrarInfo
        {
            public byte[]? ts { get; set; }
            public DateTime? fecha_aplicado { get; set; }
            public string? modulo { get; set; }
            public DateTime? fecha_autoriza { get; set; }
            public int anio { get; set; }
            public int mes { get; set; }
        }

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
