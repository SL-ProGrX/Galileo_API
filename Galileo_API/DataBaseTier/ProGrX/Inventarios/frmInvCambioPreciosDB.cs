using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;

namespace PgxAPI.DataBaseTier
{
    public class frmInvCambioPreciosDB
    {
        private readonly IConfiguration _config;

        public frmInvCambioPreciosDB(IConfiguration config)
        {
            _config = config;
        }


        public ErrorDto<List<FacturaPrecioDetalleDTO>> OrdenesDetalle_Obtener(int CodEmpresa, string CodFactura, int? CodProveedor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<FacturaPrecioDetalleDTO>>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select D.Cod_factura,D.cod_producto,P.descripcion,D.cantidad,D.cod_bodega,D.precio,isnull(D.descuento,0) as descuento,D.imp_ventas,0 as Total
                                          from cpr_Compras_detalle D inner join pv_productos P on D.cod_producto = P.cod_producto
                                          where D.cod_factura = '{CodFactura}' and D.cod_proveedor = {CodProveedor}
                                          order by D.Linea";
                    response.Result = connection.Query<FacturaPrecioDetalleDTO>(query).ToList();
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

        public ErrorDto PreciosFactura_Actualiza(int CodEmpresa, FacturaPrecioDetalleDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //float precio = request.nuevo_precio == 0 ? request.precio : request.nuevo_precio;
                    if (request.nuevo_precio > 0 )
                    {
                        var query = $@"UPDATE cpr_Compras_detalle SET PRECIO = {request.nuevo_precio}
						WHERE COD_FACTURA = '{request.cod_factura}' AND cod_producto = '{request.cod_producto}' ";

                        resp.Code = connection.Query<int>(query).FirstOrDefault();
                        resp.Description = "Ok";
                    }
                 
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto CambiosPrecios_Actualizar(int CodEmpresa, PrecioExcelDTO precio)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string tipo_producto = "P";

                    if (precio.no_existe.ToUpper() == "NO")
                    {
                        if (precio.cod_producto == "0")
                        {
                            var queryConsecutivo = $@"select FORMAT(NEXT VALUE FOR SeqPv_Productos, '000000')";
                            precio.cod_producto = connection.Query<string>(queryConsecutivo).FirstOrDefault();

                        }

                        if (precio.unidad_medida.ToUpper() == "SP")
                        {
                            tipo_producto = "S";
                        }

                        if (precio.familia.ToUpper() == "ACTIVOS")
                        {
                            tipo_producto = "A";
                        }

                        var queryProdClas = $@"SELECT COD_PRODCLAS FROM PV_PROD_CLASIFICA WHERE UPPER(DESCRIPCION) = UPPER('{precio.categoria}')";
                        precio.categoria = connection.Query<string>(queryProdClas).FirstOrDefault();

                        var queryFamilia = $@"SELECT COD_LINEA_SUB FROM PV_PROD_CLASIFICA_SUB
                                            WHERE UPPER(DESCRIPCION) = UPPER('{precio.familia}') AND COD_PRODCLAS = '{precio.categoria}' AND COD_LINEA_SUB_MADRE IS NOT NULL";
                        precio.familia = connection.Query<string>(queryFamilia).FirstOrDefault();


                        var queryNuevo = $@"INSERT INTO PV_PRODUCTOS (COD_PRODUCTO, COD_MARCA, COD_UNIDAD, COD_PRODCLAS, COD_BARRAS, LOTES, DESCRIPCION, TIPO_PRODUCTO, ESTADO, MODELO, OBSERVACION, COSTO_REGULAR, PRECIO_REGULAR, DIR_FOTOGRAFIA, COD_FABRICANTE, COMISION_MONTO, COMISION_UNIDAD, IMPUESTO_VENTAS, IMPUESTO_CONSUMO, INVENTARIO_CALCULA, INVENTARIO_MINIMO, INVENTARIO_MAXIMO, FRACCIONES, PRECIO_COMPRA, DESCUENTO_TIPO, DESCUENTO_VALOR, COD_CUENTA, EXISTENCIA, USER_CREA, USER_MODIFICA, ULTIMA_MODIFICACION, PORC_UTILIDAD, TIPO_CAMBIO, SIMILAR, COD_LINEA_SUB, CABYS, FE_SINC_FECHA, FE_SINC_USER, I_STOCK, I_VENTAENLINEA, REGISTRO_FECHA, VENTA_FREQ_DIAS, VENTA_QTY_MAX, TIPO_ACTIVO, I_FILTRADO, PUNTO_REORDEN, TIEMPO_ENTREGA_DIAS
                        )VALUES ('{precio.cod_producto}', '01', '{precio.unidad_medida}', {precio.categoria}, 0, 0, '{precio.descripcion}', '{tipo_producto}', 'A', '', '{precio.notas}', 0,{precio.precio_nuevo}, '', '', 0, 0, 0, 0, 'N', 0, 0, 1, NULL, NULL, NULL, NULL, NULL, 'demo', NULL, NULL, 0, 0, NULL, '{precio.familia}', 1, 0, NULL, NULL, 1, 0, NULL, 1, 99, 0, 0, 0
                        );";


                        resp.Code = connection.Query<int>(queryNuevo).FirstOrDefault();
                        resp.Description = "Ok";
                    }
                    else
                    {
                        var queryExistente = $@"UPDATE PV_PRODUCTOS SET PRECIO_REGULAR = {precio.precio_nuevo}
						WHERE COD_PRODUCTO = '{precio.cod_producto}'";

                        resp.Code = connection.Query<int>(queryExistente).FirstOrDefault();
                        resp.Description = "Ok";
                    }


                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }


    }
}