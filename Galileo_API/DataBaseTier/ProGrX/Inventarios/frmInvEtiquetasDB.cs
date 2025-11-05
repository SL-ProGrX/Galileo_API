using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;

namespace PgxAPI.DataBaseTier
{
    public class frmInvEtiquetasDB
    {
        private readonly IConfiguration _config;

        public frmInvEtiquetasDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDTO<List<ProductData>> GenerateSato(int CodEmpresa, GenerateSatoRequest request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<ProductData>>();
            CodBarrasData barData = new();
            try
            {
                decimal curRedondeo = request.Redondeo == "D" ? request.Value : request.Value * -1;

                using var connection = new SqlConnection(stringConn);
                {
                    var query = "";
                    if (request.Opcion == 0)
                    {
                        query = $@"select 1 as Cantidad,modelo,cod_barras,descripcion,cod_producto
                        ,round((precio_regular * ((impuesto_ventas / 100)+1)),{request.Value}) as Precio
                        from pv_productos where cod_producto = '{request.CodProducto}'";
                       
                    }
                    else if (request.Opcion == 1)
                    {
                        query = $@"select E.Cantidad,P.modelo,P.cod_barras,P.descripcion,P.cod_producto
                        ,round((precio_regular * ((impuesto_ventas / 100)+1)),{request.Value}) as Precio
                        from pv_productos P inner join Cpr_Compras_detalle E On P.cod_producto = E.cod_producto
                        where E.cod_proveedor = {request.CodProveedor} and cod_factura = '{request.CodFactura}'";
                    }

                    response.Result = connection.Query<ProductData>(query).ToList();

                    foreach (ProductData item in response.Result)
                    {
                        if (string.IsNullOrEmpty(item.Cod_Barras))
                        {
                            var query2 = $@"select cod_barras,cod_ProdClas from pv_productos where cod_producto = '{item.Cod_Producto}'";
                            barData = connection.Query<CodBarrasData>(query2).FirstOrDefault();

                            string xBarra = barData.Cod_Barras.Trim();
                            if (xBarra.Length < 12)
                            {
                                xBarra = "2000" + barData.Cod_ProdClas.ToString().Trim().PadLeft(3, '0').Substring(0, 3)
                                    + item.Cod_Producto.Trim().PadLeft(5, '0').Substring(0, 5);
                                var query3 = $@"update pv_productos set cod_barras = '{xBarra}' where cod_producto = '{item.Cod_Producto}'";
                                connection.Execute(query3);
                                item.Cod_Barras = xBarra;
                            }
                        }
                    }
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