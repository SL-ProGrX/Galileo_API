using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficioProdDB
    {
        /// <summary>
        /// Guarda un producto: inserta si no existe, o actualiza si ya existe.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="producto">Datos del producto.</param>
        /// <param name="usuario">Usuario que realiza la operación.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfiBeneficioProd_Producto_Guardar(int CodCliente, ProductoData producto, string usuario)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                return Producto_Existe(connection, producto.cod_producto)
                    ? Producto_Actualizar(connection, producto)
                    : Producto_Insertar(connection, producto, usuario);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Inserta un producto en el catálogo con su indicador de tarjeta regalo.
        /// </summary>
        private static ErrorDto Producto_Insertar(SqlConnection connection, ProductoData producto, string usuario)
        {
            var existeTarjeta = EsTarjetaRegalo(connection, producto.cod_producto_inv);
            var codProdInv = string.IsNullOrEmpty(producto.cod_producto_inv) ? null : producto.cod_producto_inv;

            const string sql = @"INSERT INTO afi_bene_productos
                                    (cod_producto, descripcion, costo_unidad, tarjeta_regalo, registro_fecha, registro_usuario, cod_producto_inv)
                                 VALUES
                                    (@cod_producto, @descripcion, @costo_unidad, @existeTarjeta, GETDATE(), @usuario, @codProdInv)";

            connection.Execute(sql, new
            {
                producto.cod_producto,
                producto.descripcion,
                producto.costo_unidad,
                existeTarjeta,
                usuario,
                codProdInv
            });

            return DbHelper.OkResponse("Producto insertado satisfactoriamente!");
        }

        /// <summary>
        /// Actualiza un producto existente y su indicador de tarjeta regalo.
        /// </summary>
        private static ErrorDto Producto_Actualizar(SqlConnection connection, ProductoData producto)
        {
            var existeTarjeta = EsTarjetaRegalo(connection, producto.cod_producto_inv);
            var codProdInv = string.IsNullOrEmpty(producto.cod_producto_inv) ? null : producto.cod_producto_inv;

            const string sql = @"UPDATE afi_bene_productos
                                 SET descripcion = @descripcion, costo_unidad = @costo_unidad,
                                     tarjeta_regalo = @existeTarjeta, cod_producto_inv = @codProdInv
                                 WHERE cod_producto = @cod_producto";

            connection.Execute(sql, new
            {
                producto.descripcion,
                producto.costo_unidad,
                existeTarjeta,
                codProdInv,
                producto.cod_producto
            });

            return DbHelper.OkResponse("Producto actualizado satisfactoriamente!");
        }

        /// <summary>
        /// Elimina un producto del catálogo.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_producto">Código del producto a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfiBeneficioProd_Producto_Eliminar(int CodCliente, string cod_producto)
        {
            const string sql = "DELETE FROM afi_bene_productos WHERE cod_producto = @cod_producto";

            var result = DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new { cod_producto });

            if (result.Code == 0)
            {
                result.Description = "Producto eliminado satisfactoriamente!";
            }

            return result;
        }
    }
}
