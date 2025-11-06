using PgxAPI.Models.CxP;
using System.Data;
using Dapper;
using PgxAPI.Models.ERROR;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;

namespace PgxAPI.DataBaseTier
{
    public class frmCxP_Eventos_VentasDB
    {
        private readonly IConfiguration? _config;

        public frmCxP_Eventos_VentasDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<List<CxpEventosDto>> Eventos_Obtener(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CxpEventosDto>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $"Select Cod_Evento as 'IdX', Descripcion as 'ItmX' from CxP_Eventos order by Fecha_Inicio desc";

                    response.Result = connection.Query<CxpEventosDto>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        public ErrorDto<List<CxpEventosVentasDto>> Eventos_Ventas_Obtener(int CodEmpresa, string parametros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var info = JsonConvert.DeserializeObject<CxpEventosVentasFiltros>(parametros);
            var response = new ErrorDto<List<CxpEventosVentasDto>>
            {
                Code = 0,
                Result = new List<CxpEventosVentasDto>()
            };
            try
            {
                if (info.id_venta == 0) 
                {
                    info.id_venta = null;
                }
                using var connection = new SqlConnection(clienteConnString);
                {
                    var procedure = $@"[spCxP_Eventos_Ventas]";
                    var values = new
                    {
                        id_venta = info.id_venta,
                        inicio = info.inicio,
                        corte = info.corte,
                        proveedorId = info.proveedorId,
                        proveedorNombre = info.proveedorNombre,
                        cedula = info.cedula,
                        nombre = info.nombre,
                        usuario = info.usuario,
                        appcod = info.appcod
                    };

                    response.Result = connection.Query<CxpEventosVentasDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }
    }
}