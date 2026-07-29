using Microsoft.Data.SqlClient;
using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using System.Net.NetworkInformation;

namespace Galileo.DataBaseTier
{
    public class MProGrXSecurityMainDb
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "DefaultConnString";
        public MProGrXSecurityMainDb(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto Bitacora(MProGrXSecurityMainBitacora bitacora)
        {
            var response = new ErrorDto();

            try
            {
                var connection = new SqlConnection(_config.GetConnectionString(connectionStringName));


                // Normalizar / truncar strings
                var detalle = Trunc(bitacora.strDetalleMovimiento, 500);
                var usuario = Trunc(bitacora.usuario, 100);              // ajusta si tu SP soporta más
                var tipoMov = (bitacora.strTipoMovimiento ?? "").ToUpperInvariant();
                tipoMov = Trunc(tipoMov, 50);

                var appName = Trunc(bitacora.AppName, 100);
                var appVersion = Trunc(bitacora.AppVersion, 50);

                var nombreMaquina = Trunc(Environment.MachineName, 100); // aquí tu código tenía un if mal (500 vs 100)

                var macAddress =
                    NetworkInterface.GetAllNetworkInterfaces()
                        .Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
                                      nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                        .Select(nic => nic.GetPhysicalAddress().ToString())
                        .FirstOrDefault() ?? string.Empty;

                macAddress = Trunc(macAddress, 50);

                // Ejecutar SP con parámetros (sin SQL dinámico)
                const string sp = "spSEG_Bitacora_Add";

                var args = new
                {
                    Cliente = bitacora.CodEmpresa,
                    Usuario = usuario,
                    Modulo = bitacora.vModulo,
                    Movimiento = tipoMov,
                    Detalle = detalle,
                    AppName = appName,
                    AppVersion = appVersion,
                    LogEquipo = nombreMaquina,
                    LogIP = "",              // estabas pasando '' fijo
                    LogEquipoMac = macAddress
                };

                connection.Execute(sp, args, commandType: System.Data.CommandType.StoredProcedure);

                response.Code = 0;
                response.Description = "Bitácora registrada correctamente.";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;

            static string Trunc(string? value, int maxLen)
            {
                if (string.IsNullOrEmpty(value)) return string.Empty;
                return value.Length > maxLen ? value.Substring(0, maxLen) : value;
            }
        }
    }
}