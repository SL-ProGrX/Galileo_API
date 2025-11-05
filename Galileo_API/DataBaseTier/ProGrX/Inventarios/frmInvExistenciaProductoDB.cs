using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;

namespace PgxAPI.DataBaseTier
{
    public class frmInvExistenciaProductoDB
    {
        private readonly IConfiguration _config;

        public frmInvExistenciaProductoDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDTO<List<ExistenciaProductoDTO>> existenciaProducto_Obtener(int CodCliente, string filtroString)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            ExistenciaProducto_Filtros filtros = JsonConvert.DeserializeObject<ExistenciaProducto_Filtros>(filtroString);

            var response = new ErrorDTO<List<ExistenciaProductoDTO>>();
            try
            {
                string where = "";

                DateTimeOffset fecha_inicio = DateTimeOffset.Parse(filtros.fecha_inicio);
                string fechainicio = fecha_inicio.ToString("yyyy-MM-dd");

                DateTimeOffset fecha_corte = DateTimeOffset.Parse(filtros.fecha_corte);
                string fechacorte = fecha_corte.ToString("yyyy-MM-dd");


                where = $"where ip.cod_producto = '{filtros.cod_Producto}' ";


                //}


                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT b.Cod_Bodega AS Bodega, b.Descripcion AS Descripcion,SUM(
                                           (ip.existencia_inicial + ip.entradas - ip.salidas)) AS Existencia
                                    FROM pv_bodegas b
                                    JOIN pv_inventario_proceso ip ON b.Cod_Bodega = ip.cod_bodega 
                                    {where} GROUP BY b.COD_BODEGA, b.DESCRIPCION";

                    response.Result = connection.Query<ExistenciaProductoDTO>(query).ToList();
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