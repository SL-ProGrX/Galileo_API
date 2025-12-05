
using Galileo.Models;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier
{
    public class PortalDB
    {
        private readonly IConfiguration _config;
        public PortalDB(IConfiguration config)
        {
            _config = config;
        }

        public string ObtenerDbConnStringEmpresa(int CodEmpresa)
        {
            SeguridadPortalDb seguridadPortal = new SeguridadPortalDb(_config);
            PgxClienteDto pgxClienteDto;
            string connectionString = string.Empty;
            try
            {
                pgxClienteDto = seguridadPortal.SeleccionarPgxClientePorCodEmpresa(CodEmpresa);
                connectionString = $"Server={pgxClienteDto.PGX_CORE_SERVER};Database={pgxClienteDto.PGX_CORE_DB};" +
                                          $"User Id={pgxClienteDto.PGX_CORE_USER};Password={pgxClienteDto.PGX_CORE_KEY};" +
                                          "Encrypt=True;TrustServerCertificate=True;Application Name=PGX_CORE_Access;";
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return connectionString;
        }

        public SqlConnection CreateConnection(int codEmpresa)
        {
            var stringConn = ObtenerDbConnStringEmpresa(codEmpresa);
            return new SqlConnection(stringConn);
        }
    }
}