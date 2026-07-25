using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneProdPagoDB
    {
        private const string BodegaBeneficios = "Beneficios Solidarios";

        /// <summary>
        /// Procesa la entrega de productos: valida existencias, crea la boleta de salida de inventario,
        /// gestiona tarjetas de regalo, registra el detalle, actualiza el pago y procesa la boleta.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="beneficio">JSON con la lista de productos a entregar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfiBeneOtorga_Actualiza(int CodCliente, string beneficio)
        {
            var items = JsonConvert.DeserializeObject<List<AfiBeneProdAsgData>>(beneficio) ?? new List<AfiBeneProdAsgData>();

            if (items.Count == 0)
            {
                return new ErrorDto { Code = 2, Description = "No se encontraron registros para actualizar" };
            }

            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                var (errorExistencia, expediente) = ValidarExistencias(connection, items);
                if (errorExistencia != null)
                {
                    return errorExistencia;
                }

                var ultimaBoleta = CrearBoletaSalida(connection, items, expediente);

                foreach (var item in items)
                {
                    ProcesarEntregaItem(connection, CodCliente, item, ultimaBoleta);
                }

                ProcesarBoleta(CodCliente, ultimaBoleta, items[0].autoriza_user);

                return DbHelper.OkResponse("Registro Actualizado");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Valida la existencia de inventario de cada producto y arma la cadena de expedientes.
        /// </summary>
        private static (ErrorDto? error, string expediente) ValidarExistencias(SqlConnection connection, List<AfiBeneProdAsgData> items)
        {
            var expedientes = new List<string>();

            foreach (var item in items)
            {
                var codProdInv = ObtenerCodProductoInv(connection, item.Cod_Producto);
                if (codProdInv == null)
                {
                    continue;
                }

                const string sqlExistencia = @"SELECT TOP 1 EXISTENCIA FROM PV_INVENTARIO
                                               WHERE cod_producto = @codProdInv
                                               ORDER BY ANIO DESC, MES DESC, ENTRADA_FECHA DESC";
                var existencia = connection.QueryFirstOrDefault<int>(sqlExistencia, new { codProdInv });

                if (existencia < item.Cantidad)
                {
                    return (DbHelper.ErrorResponse("No hay suficiente existencia para el producto " + item.Cod_Producto), string.Empty);
                }

                expedientes.Add(item.expediente);
            }

            return (null, string.Join(" ,", expedientes));
        }

        /// <summary>
        /// Crea la boleta de salida de inventario (PV_INVTRANSAC) si hay expedientes a entregar.
        /// </summary>
        private static string CrearBoletaSalida(SqlConnection connection, List<AfiBeneProdAsgData> items, string expediente)
        {
            const string sqlConsec = "SELECT ISNULL(MAX(Boleta), 0) + 1 AS Ultimo FROM pv_InvTranSac WHERE Tipo = 'S'";
            var ultimaBoleta = (connection.QueryFirstOrDefault<string>(sqlConsec) ?? "0").PadLeft(10, '0');

            if (expediente.Length == 0)
            {
                return ultimaBoleta;
            }

            const string sqlInsert = @"INSERT INTO [dbo].[PV_INVTRANSAC]
                                        (BOLETA, TIPO, COD_ENTSAL, FECHA, ESTADO, FECHA_SISTEMA, NOTAS, DOCUMENTO, PLANTILLA,
                                         GENERA_USER, GENERA_FECHA, AUTORIZA_USER, AUTORIZA_FECHA, PROCESA_USER, PROCESA_FECHA, TOTAL)
                                       VALUES
                                        (@ultimaBoleta, 'S', 'S', GETDATE(), 'A', GETDATE(), @notas, 'ENTREGA BENEFICIO', 0,
                                         @usuario, GETDATE(), @usuario, GETDATE(), @usuario, GETDATE(), @total)";

            connection.Execute(sqlInsert, new
            {
                ultimaBoleta,
                notas = "Expedientes: " + expediente,
                usuario = items[0].autoriza_user,
                total = items.Sum(x => x.Monto)
            });

            return ultimaBoleta;
        }

        /// <summary>
        /// Procesa la entrega de un ítem: tarjeta de regalo, detalle de boleta, actualización de pago y bitácora.
        /// </summary>
        private void ProcesarEntregaItem(SqlConnection connection, int CodCliente, AfiBeneProdAsgData item, string ultimaBoleta)
        {
            var existeTarjeta = item.tarjeta ? ManejarTarjetaRegalo(connection, item) : 0;

            var codProdInv = ObtenerCodProductoInv(connection, item.Cod_Producto);
            if (existeTarjeta == 0 && codProdInv != null)
            {
                InsertarDetalleBoleta(connection, item, ultimaBoleta, codProdInv);
            }

            ActualizarPago(connection, item);
            RegistrarBitacora(CodCliente, item);
        }

        /// <summary>
        /// Gestiona la tarjeta de regalo: reutiliza una disponible ('D') o inserta una nueva pendiente.
        /// </summary>
        private static int ManejarTarjetaRegalo(SqlConnection connection, AfiBeneProdAsgData item)
        {
            const string sqlExiste = @"SELECT COUNT(*) FROM AFI_BENE_TARJETAS_REGALO
                                       WHERE COD_PRODUCTO = @Cod_Producto AND cod_beneficio = @cod_beneficio AND consec = @Consec";
            var existeTarjeta = connection.QueryFirstOrDefault<int>(sqlExiste,
                new { item.Cod_Producto, item.cod_beneficio, item.Consec });

            const string sqlEstado = @"SELECT estado FROM AFI_BENE_TARJETAS_REGALO
                                       WHERE COD_PRODUCTO = @Cod_Producto AND cod_beneficio = @cod_beneficio AND no_tarjeta = @noTarjeta";
            var estadoTarjeta = connection.QueryFirstOrDefault<string>(sqlEstado,
                new { item.Cod_Producto, item.cod_beneficio, item.noTarjeta });

            if (estadoTarjeta == "D")
            {
                const string sqlUpdate = @"UPDATE AFI_BENE_TARJETAS_REGALO
                                           SET estado = 'E', consec = @Consec, id_beneficio = @id_beneficio,
                                               cedula = @Cedula, id_pago = @id_pago
                                           WHERE cod_producto = @Cod_Producto AND cod_beneficio = @cod_beneficio AND no_tarjeta = @noTarjeta";
                connection.Execute(sqlUpdate, new
                {
                    item.Consec, item.id_beneficio, item.Cedula, item.id_pago, item.Cod_Producto, item.cod_beneficio, item.noTarjeta
                });
            }
            else
            {
                const string sqlInsert = @"INSERT AFI_BENE_TARJETAS_REGALO
                                            (COD_PRODUCTO, REGISTRO_FECHA, REGISTRO_USUARIO, COD_BENEFICIO, CONSEC, CEDULA,
                                             ID_BENEFICIO, ESTADO, NO_TARJETA, MONTO, ID_PAGO)
                                           VALUES
                                            (@Cod_Producto, GETDATE(), @autoriza_user, @cod_beneficio, @Consec, @Cedula,
                                             @id_beneficio, 'P', @noTarjeta, @Monto, @id_pago)";
                connection.Execute(sqlInsert, new
                {
                    item.Cod_Producto, item.autoriza_user, item.cod_beneficio, item.Consec, item.Cedula,
                    item.id_beneficio, item.noTarjeta, item.Monto, item.id_pago
                });
            }

            return existeTarjeta;
        }

        /// <summary>
        /// Inserta el detalle de la boleta de salida (PV_INVTRADET) para el producto de inventario.
        /// </summary>
        private static void InsertarDetalleBoleta(SqlConnection connection, AfiBeneProdAsgData item, string ultimaBoleta, string codProdInv)
        {
            const string sql = @"INSERT INTO [dbo].[PV_INVTRADET]
                                    (LINEA, BOLETA, TIPO, COD_BODEGA, COD_PRODUCTO, CANTIDAD, PRECIO, DESPACHO)
                                 VALUES
                                    (@linea, @ultimaBoleta, 'S',
                                     (SELECT COD_BODEGA FROM PV_BODEGAS WHERE DESCRIPCION = @bodega),
                                     @codProdInv, @Cantidad, @Monto, @Cantidad)";

            connection.Execute(sql, new
            {
                item.linea,
                ultimaBoleta,
                bodega = BodegaBeneficios,
                codProdInv,
                item.Cantidad,
                item.Monto
            });
        }

        /// <summary>
        /// Actualiza el pago del beneficio a estado 'E' (Entregado).
        /// </summary>
        private static void ActualizarPago(SqlConnection connection, AfiBeneProdAsgData item)
        {
            const string sql = @"UPDATE afi_bene_pago
                                 SET estado = 'E', envio_user = @autoriza_user, envio_fecha = GETDATE()
                                 WHERE cedula = @Cedula AND cod_beneficio = @cod_beneficio AND consec = @Consec
                                   AND COD_PRODUCTO = @Cod_Producto";
            connection.Execute(sql, new { item.autoriza_user, item.Cedula, item.cod_beneficio, item.Consec, item.Cod_Producto });
        }

        /// <summary>
        /// Registra el movimiento de entrega en la bitácora de beneficios.
        /// </summary>
        private void RegistrarBitacora(int CodCliente, AfiBeneProdAsgData item)
        {
            _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDto
            {
                EmpresaId = CodCliente,
                cod_beneficio = item.cod_beneficio,
                consec = item.Consec,
                movimiento = "Actualiza",
                detalle = $"Entrega de Producto COD: [{item.Cod_Producto}], Monto: [{item.Monto}]",
                registro_usuario = item.autoriza_user.ToUpper()
            });
        }

        /// <summary>
        /// Procesa la boleta de salida de inventario mediante el SP correspondiente.
        /// </summary>
        private void ProcesarBoleta(int CodCliente, string ultimaBoleta, string usuario)
        {
            var procesaBoleta = new FrmInvTransacProcesaDB(_config);
            procesaBoleta.InvTransacProcesa_SP(CodCliente, new InvTransacProcesa
            {
                Tipo = "S",
                Boleta = ultimaBoleta,
                Usuario = usuario
            });
        }

        /// <summary>
        /// Obtiene el código de producto de inventario asociado a un producto de beneficio.
        /// </summary>
        private static string? ObtenerCodProductoInv(SqlConnection connection, string cod_producto)
        {
            const string sql = "SELECT cod_producto_inv FROM AFI_BENE_PRODUCTOS WHERE cod_producto = @cod_producto";
            return connection.QueryFirstOrDefault<string>(sql, new { cod_producto });
        }
    }
}
