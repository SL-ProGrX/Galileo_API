using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;


namespace PgxAPI.DataBaseTier
{
    public class frmAF_BeneficioProdDB
    {
        private readonly IConfiguration _config;

        public frmAF_BeneficioProdDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Metodo para obtener la lista de productos
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="pagina"></param>
        /// <param name="paginacion"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDTO<ProductoDataLista> ProductoLista_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var response = new ErrorDTO<ProductoDataLista>();
            response.Result = new ProductoDataLista();
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = "SELECT COUNT(*) from afi_bene_productos ";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " where cod_producto LIKE '%" + filtro + "%' OR descripcion LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"select cod_producto,descripcion,costo_unidad, cod_producto_inv from afi_bene_productos 
                                         {filtro} 
                                        order by cod_producto
                                        {paginaActual}
                                        {paginacionActual} ";


                    response.Result.productos = connection.Query<ProductoData>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.Total = 0;
            }
            return response;
        }

        /// <summary>
        /// Metodo para verificar si el producto existe
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_producto"></param>
        /// <returns></returns>
        private bool Producto_Existe(int CodCliente, string cod_producto)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            bool existe = false;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT COUNT(*) from afi_bene_productos where cod_producto = '{cod_producto}' ";
                    existe = connection.Query<int>(query).FirstOrDefault() > 0;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return existe;
        }

        /// <summary>
        /// Metodo para insertar un producto
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="producto"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private ErrorDTO Producto_Insertar(int CodCliente, ProductoData producto, string usuario)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;
            string codProdInv = "";
            try
            {

                if (!Producto_Existe(CodCliente, producto.cod_producto))
                {
                    using var connection = new SqlConnection(stringConn);
                    {
                        var query = $@"SELECT CASE  
                            WHEN EXISTS (
                                SELECT 1 FROM PV_PRODUCTOS  
                                WHERE COD_PRODCLAS = (SELECT COD_PRODCLAS FROM PV_PROD_CLASIFICA WHERE DESCRIPCION = 'Beneficios Solidarios')  
		                        AND COD_PRODUCTO = '{producto.cod_producto_inv}'
                            ) THEN 1  
                            ELSE 0  
                        END AS Existe;";
                        int existeTarjeta = connection.Query<int>(query).FirstOrDefault();

                        if (producto.cod_producto_inv != null && producto.cod_producto_inv != "")
                        {
                            codProdInv = $"'{producto.cod_producto_inv}'";
                        }
                        else
                        {
                            codProdInv = "NULL";
                        }

                        query = $@"insert into afi_bene_productos(cod_producto,descripcion,costo_unidad, tarjeta_regalo, registro_fecha, registro_usuario, cod_producto_inv)  
                                    values ('{producto.cod_producto}', '{producto.descripcion}', '{producto.costo_unidad}', {existeTarjeta}, getDate(), '{usuario}', {codProdInv})";
                        var result = connection.Execute(query);
                    }

                    info.Description = "Producto insertado satisfactoriamente!";
                }
                else
                {
                    info.Code = -1;
                    info.Description = "Producto " + producto.cod_producto + " ya Existe!";
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;

        }

        /// <summary>
        /// Metodo para actualizar un producto
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="producto"></param>
        /// <returns></returns>
        private ErrorDTO Producto_Actualizar(int CodCliente, ProductoData producto)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;
            string codProdInv = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT CASE  
                            WHEN EXISTS (
                                SELECT 1 FROM PV_PRODUCTOS  
                                WHERE COD_PRODCLAS = (SELECT COD_PRODCLAS FROM PV_PROD_CLASIFICA WHERE DESCRIPCION = 'Beneficios Solidarios')  
		                        AND COD_PRODUCTO = '{producto.cod_producto_inv}'
                            ) THEN 1  
                            ELSE 0  
                        END AS Existe;";
                    int existeTarjeta = connection.Query<int>(query).FirstOrDefault();

                    if (producto.cod_producto_inv != null && producto.cod_producto_inv != "")
                    {
                        codProdInv = $"'{producto.cod_producto_inv}'";
                    }
                    else
                    {
                        codProdInv = "NULL";
                    }

                    query = $@"update afi_bene_productos set descripcion = '{producto.descripcion}', costo_unidad = '{producto.costo_unidad}', 
                    tarjeta_regalo = {existeTarjeta}, cod_producto_inv = {codProdInv} where cod_producto = '{producto.cod_producto}' ";
                    var result = connection.Execute(query);
                }

                info.Description = "Producto actualizado satisfactoriamente!";
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        /// <summary>
        /// Metodo para eliminar un producto
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_producto"></param>
        /// <returns></returns>
        public ErrorDTO Producto_Eliminar(int CodCliente, string cod_producto)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"delete from afi_bene_productos where cod_producto = '{cod_producto}' ";
                    var result = connection.Execute(query);
                }

                info.Description = "Producto eliminado satisfactoriamente!";
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        /// <summary>
        /// Metodo para exportar la lista de productos
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDTO<List<ProductoData>> Producto_Exportar(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<ProductoData>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select cod_producto,descripcion,costo_unidad from afi_bene_productos order by cod_producto";
                    response.Result = connection.Query<ProductoData>(query).ToList();
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

        /// <summary>
        /// Metodo para guardar un producto valdiando si existe o no
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="producto"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO Producto_Guardar(int CodCliente, ProductoData producto, string usuario)
        {
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;
            if (!Producto_Existe(CodCliente, producto.cod_producto))
            {
                info = Producto_Insertar(CodCliente, producto, usuario);
            }
            else
            {
                info = Producto_Actualizar(CodCliente, producto);
            }
            return info;
        }
    }
}