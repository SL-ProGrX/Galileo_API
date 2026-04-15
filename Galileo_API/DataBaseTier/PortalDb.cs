
using Galileo.Models;
using Microsoft.Data.SqlClient;
using System.Security;
using System.Text.RegularExpressions;

namespace Galileo.DataBaseTier
{
    public class PortalDB
    {
        private readonly IConfiguration _config;
        public PortalDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene la cadena de conexión de la empresa luego de validar y normalizar el código recibido.
        /// </summary>
        public string ObtenerDbConnStringEmpresa(int CodEmpresa)
        {
            var codEmpresaSeguro = NormalizarCodEmpresa(CodEmpresa);
            var seguridadPortal = new SeguridadPortalDb(_config);
            var pgxClienteDto = seguridadPortal.SeleccionarPgxClientePorCodEmpresa(codEmpresaSeguro);

            ValidarClientePortal(pgxClienteDto);

            return ConstruirConnectionStringSegura(pgxClienteDto);
        }

        /// <summary>
        /// Crea una conexión SQL usando una cadena de conexión segura asociada a la empresa validada.
        /// </summary>
        public SqlConnection CreateConnection(int codEmpresa)
        {
            var connectionString = ObtenerDbConnStringEmpresa(codEmpresa);
            return new SqlConnection(connectionString);
        }

        private static readonly Regex SafeSqlNameRegex = new(
            @"^[A-Za-z0-9_\.\-\\]+$",
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(250));

         /// <summary>
        /// Valida y normaliza el código de empresa recibido desde entrada no confiable.
        /// </summary>
        private static int NormalizarCodEmpresa(int codEmpresa)
        {
            if (codEmpresa <= 0 || codEmpresa > 999999)
            {
                throw new SecurityException("El código de empresa no es válido.");
            }

            return codEmpresa;
        }

        /// <summary>
        /// Valida que los datos de conexión recuperados desde portal sean aptos para construir
        /// una cadena de conexión controlada y sin formato libre.
        /// </summary>
        private static void ValidarClientePortal(PgxClienteDto cliente)
        {
            if (cliente == null)
            {
                throw new SecurityException("No fue posible resolver una empresa válida.");
            }

            var server = (cliente.PGX_CORE_SERVER ?? string.Empty).Trim();
            var database = (cliente.PGX_CORE_DB ?? string.Empty).Trim();
            var user = (cliente.PGX_CORE_USER ?? string.Empty).Trim();
            var secret = (cliente.PGX_CORE_KEY ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(server) ||
                string.IsNullOrWhiteSpace(database) ||
                string.IsNullOrWhiteSpace(user) ||
                string.IsNullOrWhiteSpace(secret))
            {
                throw new SecurityException("La configuración de conexión de la empresa está incompleta.");
            }

            if (!SafeSqlNameRegex.IsMatch(server) ||
                !SafeSqlNameRegex.IsMatch(database))
            {
                throw new SecurityException("La configuración de conexión contiene caracteres no permitidos.");
            }
        }

        /// <summary>
        /// Construye una cadena de conexión segura a partir de valores previamente validados.
        /// </summary>
        private static string ConstruirConnectionStringSegura(PgxClienteDto cliente)
        {
            var server = (cliente.PGX_CORE_SERVER ?? string.Empty).Trim();
            var database = (cliente.PGX_CORE_DB ?? string.Empty).Trim();
            var user = (cliente.PGX_CORE_USER ?? string.Empty).Trim();
            var secret = (cliente.PGX_CORE_KEY ?? string.Empty).Trim();

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = database,
                UserID = user,
                Password = secret,
                Encrypt = true,
                TrustServerCertificate = true,
                ApplicationName = "PGX_CORE_Access"
            };

            return builder.ConnectionString;
        }
    }
}