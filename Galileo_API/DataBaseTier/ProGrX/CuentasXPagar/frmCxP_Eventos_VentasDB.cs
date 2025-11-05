using PgxAPI.Models;
using PgxAPI.Models.CxP;
using System.Data;
using Dapper;
using PgxAPI.Models.ERROR;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using PgxAPI.BusinessLogic;
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

        public ErrorDto<List<cxpEventosDTO>> Eventos_Obtener(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<cxpEventosDTO>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $"Select Cod_Evento as 'IdX', Descripcion as 'ItmX' from CxP_Eventos order by Fecha_Inicio desc";

                    response.Result = connection.Query<cxpEventosDTO>(query).ToList();
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

        public ErrorDto<List<cxpEventos_VentasDTO>> Eventos_Ventas_Obtener(int CodEmpresa, string parametros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var info = JsonConvert.DeserializeObject<cxpEventos_VentasFiltros>(parametros);
            var response = new ErrorDto<List<cxpEventos_VentasDTO>>
            {
                Code = 0,
                Result = new List<cxpEventos_VentasDTO>()
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

                    response.Result = connection.Query<cxpEventos_VentasDTO>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
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