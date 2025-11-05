
using PgxAPI.Models;

namespace PgxAPI.DataBaseTier
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
            Seguridad_PortalDB seguridadPortal = new Seguridad_PortalDB(_config);
            PgxClienteDTO pgxClienteDto;
            string connectionString = string.Empty;
            try
            {
                pgxClienteDto = seguridadPortal.SeleccionarPgxClientePorCodEmpresa(CodEmpresa);
                string nombreServidorCore = pgxClienteDto.PGX_CORE_SERVER;
                string nombreBDCore = pgxClienteDto.PGX_CORE_DB;
                string userId = pgxClienteDto.PGX_CORE_USER;
                string pass = pgxClienteDto.PGX_CORE_KEY;

                //connectionString = $"Data Source={nombreServidorCore};" +
                //          $"Initial Catalog={nombreBDCore};" +
                //          $"Integrated Security=False;User Id={userId};Password={pass};";

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



    }//end class
}//end namespace
