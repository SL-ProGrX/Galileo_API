using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using System.Net.NetworkInformation;

namespace PgxAPI.DataBaseTier
{
    public class mProGrX_Security_MainDB
    {
        private readonly IConfiguration _config;

        private mProGrX_Security_MainDB(IConfiguration config)
        {
            _config = config;
        }

        private ErrorDto Bitacora(MProGrXSecurityMainBitacora bitacora)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(bitacora.CodEmpresa);
            var response = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //aseguro que strDetalleMovimiento sea de maximo 500
                    if (bitacora.strDetalleMovimiento.Length > 500)
                    {
                        bitacora.strDetalleMovimiento = bitacora.strDetalleMovimiento.Substring(0, 500);
                    }

                    string nombreMaquina = Environment.MachineName;

                    if (nombreMaquina.Length > 500)
                    {
                        nombreMaquina = nombreMaquina.Substring(0, 100);
                    }

                    string macAddress = NetworkInterface
                                        .GetAllNetworkInterfaces()
                                        .Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
                                                      nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                                        .Select(nic => nic.GetPhysicalAddress().ToString())
                                        .FirstOrDefault();

                    string query = $@"exec spSEG_Bitacora_Add  
                                                {bitacora.pCliente} ,
                                                '{bitacora.usuario}',
                                                {bitacora.vModulo},
                                                '{bitacora.strTipoMovimiento.ToUpper()}',
                                                '{bitacora.strDetalleMovimiento}',
                                                '{bitacora.AppName}',
                                                '{bitacora.AppVersion}',
                                                '{nombreMaquina}',
                                                '',
                                                '{macAddress}'";
                    
                }
                response.Code = 0;
                response.Description = "Bitácora registrada correctamente.";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }
    }
}
