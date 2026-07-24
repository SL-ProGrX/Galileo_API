using Dapper;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneRecargaTarjetaDB
    {
        /// <summary>
        /// Recarga las tarjetas de regalo: genera la tesorería por proveedor, actualiza pagos, otorgamientos,
        /// crea el detalle contable, actualiza el estado de la tarjeta y cierra la remesa.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="tarjetas">JSON con la remesa, usuario y tarjetas a recargar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfiTarjetasRegalo_Recargar(int CodCliente, string tarjetas)
        {
            var info = JsonConvert.DeserializeObject<AfiBeneTarjetasRecargaData>(tarjetas) ?? new AfiBeneTarjetasRecargaData();

            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                var vToken = ObtenerToken(connection, CodCliente, info.usuario);

                foreach (var item in info.tarjetas)
                {
                    ProcesarRecargaTarjeta(connection, CodCliente, info, item, vToken);
                }

                const string sqlRemesa = "UPDATE AFI_BENE_TARJETAS_REMESAS SET Estado = 'C' WHERE cod_remesa_tr = @cod_remesa_tr";
                connection.Execute(sqlRemesa, new { info.cod_remesa_tr });

                return new ErrorDto { Code = 0 };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene un token de tesorería activo o genera uno nuevo si no existe.
        /// </summary>
        private string ObtenerToken(SqlConnection connection, int CodCliente, string usuario)
        {
            const string sql = "SELECT TOP 1 id_token FROM tes_tokens WHERE estado = 'A' ORDER BY registro_fecha";
            var existe = connection.QueryFirstOrDefault<string>(sql);

            return existe ?? _mTes.fxTesToken(CodCliente, usuario);
        }

        /// <summary>
        /// Procesa la recarga de una tarjeta: tesorería, pago/otorga, detalle contable y estado de la tarjeta.
        /// </summary>
        private void ProcesarRecargaTarjeta(SqlConnection connection, int CodCliente, AfiBeneTarjetasRecargaData info, AfiBeneTarjetasData item, string vToken)
        {
            var beneficio = ObtenerBeneficio(connection, item.cod_beneficio);
            var proveedor = ObtenerProveedor(connection, item.cod_producto);

            var vTesoreria = _mTes.fxgTesoreriaMaestro(CodCliente, info.usuario, new TesoreriaMaestroModel
            {
                vTipoDocumento = proveedor.tipo_pago,
                vBanco = proveedor.cod_banco,
                vMonto = item.monto,
                vBeneficiario = proveedor.descripcion,
                vCodigo = proveedor.cedjur,
                vOP = 0,
                vDetalle1 = item.cod_beneficio,
                vReferencia = 0,
                vDetalle2 = beneficio.descripcion,
                vCuenta = proveedor.cuenta,
                vFecha = DateTime.Now.ToString(FechaFormat),
                vRemesa = Convert.ToInt32(info.cod_remesa_tr)
            });

            if (item.id_pago != null)
            {
                ActualizarPagoYOtorga(connection, info, item, vTesoreria, vToken);
            }

            CrearDetallesTesoreria(CodCliente, item, proveedor.cuenta, beneficio.cod_cuenta, vTesoreria);
            ActualizarEstadoTarjeta(connection, CodCliente, info, item, vTesoreria);
        }

        /// <summary>
        /// Obtiene la descripción y cuenta contable del beneficio.
        /// </summary>
        private static AfiBeneficiosTraslado ObtenerBeneficio(SqlConnection connection, string cod_beneficio)
        {
            const string sql = "SELECT descripcion, cod_cuenta FROM afi_beneficios WHERE cod_beneficio = @cod_beneficio";
            return connection.QueryFirstOrDefault<AfiBeneficiosTraslado>(sql, new { cod_beneficio }) ?? new AfiBeneficiosTraslado();
        }

        /// <summary>
        /// Obtiene los datos del proveedor asociado al producto de inventario de la tarjeta.
        /// </summary>
        private static AfiBeneProveedorData ObtenerProveedor(SqlConnection connection, string cod_producto)
        {
            const string sql = @"SELECT COD_PROVEEDOR, TIPO_PAGO AS tipo_pago, COD_BANCO AS cod_banco, CEDJUR AS cedjur,
                                        DESCRIPCION AS descripcion, COD_CUENTA AS cuenta
                                 FROM CXP_PROVEEDORES P
                                 WHERE COD_PROVEEDOR = (SELECT TOP 1 COD_PROVEEDOR FROM pv_producto_prov
                                                        WHERE COD_PRODUCTO = (SELECT TOP 1 COD_PRODUCTO_INV FROM AFI_BENE_PRODUCTOS
                                                                              WHERE COD_PRODUCTO = @cod_producto))";
            return connection.QueryFirstOrDefault<AfiBeneProveedorData>(sql, new { cod_producto }) ?? new AfiBeneProveedorData();
        }

        /// <summary>
        /// Actualiza el pago con la tesorería y el token, y el otorgamiento si aún no tiene remesa.
        /// </summary>
        private static void ActualizarPagoYOtorga(SqlConnection connection, AfiBeneTarjetasRecargaData info, AfiBeneTarjetasData item, long vTesoreria, string vToken)
        {
            const string sqlPago = @"UPDATE afi_bene_pago
                                     SET tesoreria = @vTesoreria, tes_supervision_usuario = @usuario, tes_supervision_fecha = GETDATE(),
                                         ID_TOKEN = @vToken, justificacion = 'Recarga de tarjeta', cod_remesa = @cod_remesa_tr
                                     WHERE cedula = @cedula AND id_pago = @id_pago
                                       AND cod_beneficio = @cod_beneficio AND consec = @consec";
            connection.Execute(sqlPago, new
            {
                vTesoreria,
                info.usuario,
                vToken,
                info.cod_remesa_tr,
                item.cedula,
                item.id_pago,
                item.cod_beneficio,
                item.consec
            });

            const string sqlRemesaOtorga = @"SELECT COALESCE((SELECT COD_REMESA FROM afi_bene_otorga
                                                              WHERE cedula = @cedula AND cod_beneficio = @cod_beneficio AND consec = @consec), 0)";
            var existeRemesa = connection.QueryFirstOrDefault<int>(sqlRemesaOtorga, new { item.cedula, item.cod_beneficio, item.consec });

            if (existeRemesa == 0)
            {
                const string sqlOtorga = @"UPDATE afi_bene_otorga
                                           SET estado = 'A', autoriza_user = @usuario, autoriza_fecha = GETDATE(), cod_remesa = @cod_remesa_tr
                                           WHERE cedula = @cedula AND cod_beneficio = @cod_beneficio AND consec = @consec";
                connection.Execute(sqlOtorga, new { info.usuario, info.cod_remesa_tr, item.cedula, item.cod_beneficio, item.consec });
            }
        }

        /// <summary>
        /// Crea los detalles contables (Haber a proveedor, Débito a beneficio) de la tesorería.
        /// </summary>
        private void CrearDetallesTesoreria(int CodCliente, AfiBeneTarjetasData item, string ctaProveedor, string ctaBeneficio, long vTesoreria)
        {
            _mTes.sbgTesoreriaDetalle(CodCliente, new TesoreriaDetalleModel
            {
                vSolicitud = vTesoreria,
                vCtaConta = ctaProveedor,
                vMonto = item.monto,
                vDH = "H",
                vLinea = 1
            });

            _mTes.sbgTesoreriaDetalle(CodCliente, new TesoreriaDetalleModel
            {
                vSolicitud = vTesoreria,
                vCtaConta = ctaBeneficio,
                vMonto = item.monto,
                vDH = "D",
                vLinea = 2
            });
        }

        /// <summary>
        /// Actualiza el estado de la tarjeta: 'D' si no tiene pago asignado, o 'E' con bitácora si sí lo tiene.
        /// </summary>
        private void ActualizarEstadoTarjeta(SqlConnection connection, int CodCliente, AfiBeneTarjetasRecargaData info, AfiBeneTarjetasData item, long vTesoreria)
        {
            if (item.id_pago == null)
            {
                const string sql = @"UPDATE AFI_BENE_TARJETAS_REGALO
                                     SET estado = 'D', activa_usuario = @usuario, activa_fecha = GETDATE(), cod_remesa_tr = @cod_remesa_tr
                                     WHERE cod_producto = @cod_producto AND cod_beneficio = @cod_beneficio AND no_tarjeta = @no_tarjeta";
                connection.Execute(sql, new { info.usuario, info.cod_remesa_tr, item.cod_producto, item.cod_beneficio, item.no_tarjeta });
                return;
            }

            const string sqlE = @"UPDATE AFI_BENE_TARJETAS_REGALO
                                  SET estado = 'E', activa_usuario = @usuario, activa_fecha = GETDATE(), cod_remesa_tr = @cod_remesa_tr
                                  WHERE cod_producto = @cod_producto AND cod_beneficio = @cod_beneficio AND consec = @consec AND id_pago = @id_pago";
            connection.Execute(sqlE, new { info.usuario, info.cod_remesa_tr, item.cod_producto, item.cod_beneficio, item.consec, item.id_pago });

            _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDto
            {
                EmpresaId = CodCliente,
                cod_beneficio = item.cod_beneficio,
                consec = item.consec,
                movimiento = "Actualiza",
                detalle = $"Envio recarga de tarjeta a tesoreria Cod.Remesa.TR: [{info.cod_remesa_tr}]",
                registro_usuario = info.usuario
            });
        }
    }
}
