using System.Data;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using BodegaDto = Galileo.Models.CPR.BodegaDto;

namespace Galileo.DataBaseTier
{
    public class FrmCprCompraDevDB
    {
        private const string DefaultError = "Error";

        private readonly PortalDB _portalDb;

        private readonly MProGrXAuxiliarDB _auxInv;
        private readonly MSecurityMainDb _dbBitacora;
        private readonly EnvioCorreoDB _envioCorreoDB;

        private readonly string _sendEmail;

        public FrmCprCompraDevDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);

            _dbBitacora = new MSecurityMainDb(config);
            _envioCorreoDB = new EnvioCorreoDB(config);
            _auxInv = new MProGrXAuxiliarDB(config);

            _sendEmail = config.GetSection("AppSettings").GetSection("EnviaEmail").Value?.ToString() ?? "N";
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data) => _dbBitacora.Bitacora(data);

        public ErrorDto<List<FacturasDto>> ObtenerListaFacturas(int codEmpresa, int codProveedor)
        {
            return DbHelper.ExecuteListQuery<FacturasDto>(
                _portalDb,
                codEmpresa,
                @"SELECT E.cod_factura,
                         P.descripcion AS Proveedor,
                         E.total
                  FROM cpr_compras E
                  INNER JOIN cxp_Proveedores P ON E.cod_proveedor = P.cod_proveedor
                  WHERE E.cod_proveedor = @CodProveedor",
                new { CodProveedor = codProveedor }
            );
        }

        public ErrorDto<FacturaDto?> ObtenerFactura(int codEmpresa, string codFactura, int codProveedor)
        {
            return DbHelper.ExecuteSingleQuery<FacturaDto>(
                _portalDb,
                codEmpresa,
                @"SELECT E.*,
                         P.descripcion AS Proveedor
                  FROM cpr_compras E
                  INNER JOIN cxp_Proveedores P ON E.cod_proveedor = P.cod_proveedor
                  WHERE E.cod_factura = @CodFactura
                    AND E.cod_proveedor = @CodProveedor",
                defaultValue: default,
                parameters: new { CodFactura = codFactura, CodProveedor = codProveedor }
            );
        }

        public ErrorDto<List<FacturaDetalleDto>> ObtenerFacturaDetalle(int codEmpresa, string codFactura, int codProveedor)
        {
            return DbHelper.ExecuteListQuery<FacturaDetalleDto>(
                _portalDb,
                codEmpresa,
                @"SELECT D.cod_producto,
                         P.descripcion,
                         (D.cantidad - ISNULL(D.cantidad_devuelta,0)) AS Cantidad,
                         D.cod_bodega,
                         D.precio,
                         D.imp_ventas,
                         (((D.cantidad - ISNULL(D.cantidad_devuelta,0)) * D.precio) * ((D.imp_ventas / 100) + 1)) AS Total
                  FROM cpr_compras_detalle D
                  INNER JOIN pv_productos P ON D.cod_producto = P.cod_producto
                  WHERE D.cod_factura = @CodFactura
                    AND D.cod_proveedor = @CodProveedor
                  ORDER BY D.Linea",
                new { CodFactura = codFactura, CodProveedor = codProveedor }
            );
        }

        public ErrorDto<List<BodegaDto>> ObtenerBodegas(int codEmpresa)
        {
            return DbHelper.ExecuteListQuery<BodegaDto>(
                _portalDb,
                codEmpresa,
                @"SELECT cod_bodega, descripcion
                  FROM pv_bodegas
                  WHERE permite_salidas = 1"
            );
        }

        /// <summary>
        /// Mantengo tu semántica: Code=1 por defecto; si NO encuentra (o está anulada), Code=0 con mensaje.
        /// </summary>
        public ErrorDto VerificaFactura(int codEmpresa, string codFactura, int codProveedor)
        {
            var r = DbHelper.ExecuteSingleQuery<string>(
                _portalDb,
                codEmpresa,
                @"SELECT estado
                  FROM cpr_compras
                  WHERE cod_factura = @CodFactura
                    AND cod_proveedor = @CodProveedor
                    AND estado IN ('P','D')",
                defaultValue: default,
                parameters: new { CodFactura = codFactura, CodProveedor = codProveedor }
            );

            if (r.Code != 0)
                return DbHelper.ErrorResponse(r.Description ?? DefaultError, r.Code ?? -1);

            // si no existe / no cumple estado
            if (string.IsNullOrEmpty(r.Result))
            {
                return new ErrorDto
                {
                    Code = 0,
                    Description = " - No se encontró registro de la factura, o se encuentra Anulada, verifique..."
                };
            }

            return new ErrorDto { Code = 1, Description = "" };
        }

        public ErrorDto<DevolucionData?> Devolucion_Obtener(int codEmpresa, string codDevolucion)
        {
            return DbHelper.ExecuteSingleQuery<DevolucionData>(
                _portalDb,
                codEmpresa,
                @"SELECT D.*,
                         P.descripcion AS Proveedor,
                         RTRIM(C.cod_cargo) + ' - ' + RTRIM(C.descripcion) AS CargoX
                  FROM cpr_compras_dev D
                  INNER JOIN cxp_Proveedores P ON D.cod_proveedor = P.cod_proveedor
                  INNER JOIN cxp_cargos C ON D.cod_cargo = C.cod_cargo
                  WHERE D.cod_compra_dev = @CodDevolucion",
                defaultValue: default,
                parameters: new { CodDevolucion = codDevolucion }
            );
        }

        public ErrorDto<List<FacturaDetalleDto>> ObtenerDevolucionDetalle(int codEmpresa, string codDevolucion)
        {
            return DbHelper.ExecuteListQuery<FacturaDetalleDto>(
                _portalDb,
                codEmpresa,
                @"SELECT D.cod_producto,
                         P.descripcion,
                         D.cantidad,
                         D.cod_bodega,
                         D.precio,
                         D.imp_ventas,
                         (D.cantidad * D.precio) + (D.cantidad * D.precio * (D.imp_ventas / 100)) AS Total
                  FROM cpr_compra_devDet D
                  INNER JOIN pv_productos P ON D.cod_producto = P.cod_producto
                  WHERE D.cod_compra_dev = @CodDevolucion
                  ORDER BY D.Linea",
                new { CodDevolucion = codDevolucion }
            );
        }

        public ErrorDto<FacturaDto?> ObtenerOrdenCompraDev(int codEmpresa, string codFactura, int codProveedor)
        {
            return DbHelper.ExecuteSingleQuery<FacturaDto>(
                _portalDb,
                codEmpresa,
                @"SELECT cod_orden, cod_compra, fecha
                  FROM cpr_compras
                  WHERE cod_factura = @CodFactura
                    AND cod_proveedor = @CodProveedor",
                defaultValue: default,
                parameters: new { CodFactura = codFactura, CodProveedor = codProveedor }
            );
        }

        // ----------------------------- NUEVO: GUARDAR DEVOLUCION (TX) -----------------------------

        public ErrorDto Devolucion_Guardar(int codEmpresa, DevolucionInsert orden)
        {
            // 1) Validación (sin pegar a BD 50 veces)
            var valid = ValidarDevolucion(codEmpresa, orden);
            if (valid.Code != 0) return valid;

            // 2) Proceso en transacción
            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                if (conn.State != ConnectionState.Open) conn.Open();
                using var tx = conn.BeginTransaction();

                try
                {
                    var codDev = GenerarCodigoDevolucion(conn, tx);
                    InsertarCabeceraDevolucion(conn, tx, codDev, orden);
                    RegistrarBitacora(codEmpresa, orden, codDev);

                    ReemplazarDetalleDevolucion(codEmpresa,conn, tx, codDev, orden);
                    ActualizarEstadoFactura(conn, tx, orden);

                    tx.Commit();

                    // Email fuera de tx (pero en mismo flujo)
                    TryEnviarCorreo(codEmpresa, orden);

                    return DbHelper.CreateOkResponse(codDev);
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            });

            if (r.Code != 0)
                return DbHelper.ErrorResponse(r.Description ?? DefaultError, r.Code ?? -1);

            // guardo el código en Description como tu lógica original
            return new ErrorDto { Code = 0, Description = r.Result?.Result ?? "" };
        }

        private ErrorDto ValidarDevolucion(int codEmpresa, DevolucionInsert orden)
        {
            if (orden == null)
                return DbHelper.ErrorResponse(" - Devolucion no puede ser nula", -1);

            if (string.IsNullOrWhiteSpace(orden.cod_factura))
                return DbHelper.ErrorResponse(" - Devolucion no puede ser nula", -1);

            if (orden.cod_proveedor <= 0)
                return DbHelper.ErrorResponse(" - El codigo del Proveedor no es válido, verifique...", -1);

            const double epsilon = 0.00001;
            if (Math.Abs(orden.total) < epsilon)
                return DbHelper.ErrorResponse(" - El Total de la Devolución no puede ser 0...", -1);

            if (orden.lineas == null || orden.lineas.Count == 0)
                return DbHelper.ErrorResponse("No hay productos en la orden", -1);

            if (_auxInv.fxInvPeriodos(codEmpresa, orden.fecha))
                return DbHelper.ErrorResponse(" - El Periodo del Movimiento no es válido ...", -1);

            // Validaciones con BD en una sola conexión
            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                // Factura debe existir y estar en P/D
                var estado = conn.QueryFirstOrDefault<string>(
                    @"SELECT estado
                      FROM cpr_compras
                      WHERE cod_factura = @CodFactura
                        AND cod_proveedor = @CodProveedor
                        AND estado IN ('P','D')",
                    new { CodFactura = orden.cod_factura, CodProveedor = orden.cod_proveedor }
                );

                if (estado == null)
                    return DbHelper.ErrorResponse(" - No se encontró registro de la factura, o se encuentra Anulada, verifique...", -1);

                // Verificar productos + bodegas
                var msgProdBod = ValidarProductosYBodegas(conn, orden.lineas, "S");
                if (!string.IsNullOrEmpty(msgProdBod))
                    return DbHelper.ErrorResponse(msgProdBod, -1);

                // Verificar cantidades no mayores al remanente (la lógica original compara con cantidad)
                var msgCant = ValidarCantidades(conn, orden);
                if (!string.IsNullOrEmpty(msgCant))
                    return DbHelper.ErrorResponse(msgCant, -1);

                return DbHelper.CreateOkResponse();
            });

            return r.Code == 0 ? DbHelper.CreateOkResponse() : DbHelper.ErrorResponse(r.Description ?? DefaultError, r.Code ?? -1);
        }

        private static string GenerarCodigoDevolucion(IDbConnection conn, IDbTransaction tx)
        {
            // Lock para evitar duplicados por concurrencia (MAX+1)
            var next = conn.QueryFirstOrDefault<string>(
                @"SELECT RIGHT(REPLICATE('0',10) + CAST(ISNULL(MAX(CAST(cod_compra_dev AS INT)),0) + 1 AS VARCHAR(10)), 10)
                  FROM cpr_compras_dev WITH (UPDLOCK, HOLDLOCK)",
                transaction: tx
            );

            return next ?? "0000000001";
        }

        private static void InsertarCabeceraDevolucion(IDbConnection conn, IDbTransaction tx, string codDev, DevolucionInsert orden)
        {
            conn.Execute(
                @"INSERT cpr_compras_dev
                  (cod_compra_dev,cod_factura,cod_proveedor,fecha,sub_total,descuento,imp_ventas,imp_consumo,total,notas,asiento_estado,genera_user,genera_fecha,cod_cargo)
                  VALUES
                  (@CodDev,@CodFactura,@CodProveedor,GETDATE(),@SubTotal,@Descuento,@ImpVentas,0,@Total,@Notas,'P',@Usuario,GETDATE(),@Cargo)",
                new
                {
                    CodDev = codDev,
                    CodFactura = orden.cod_factura,
                    CodProveedor = orden.cod_proveedor,
                    SubTotal = orden.sub_total,
                    Descuento = orden.descuento,
                    ImpVentas = orden.imp_ventas,
                    Total = orden.total,
                    Notas = orden.notas,
                    Usuario = orden.usuario,
                    Cargo = orden.cargo
                },
                transaction: tx
            );
        }

        private void RegistrarBitacora(int codEmpresa, DevolucionInsert orden, string codDev)
        {
            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = orden.usuario,
                DetalleMovimiento = $"Registra, Devolucion Fact Compra:{orden.cod_factura} Dev {codDev}",
                Movimiento = "Registra - WEB",
                Modulo = 35
            });
        }

        private void ReemplazarDetalleDevolucion(int codEmpresa, IDbConnection conn, IDbTransaction tx, string codDev, DevolucionInsert orden)
        {
            // Limpia detalle anterior
            conn.Execute(
                @"DELETE cpr_compra_devDet WHERE cod_compra_dev = @CodDev",
                new { CodDev = codDev },
                transaction: tx
            );

            var linea = 0;

            foreach (var item in orden.lineas)
            {
                linea++;

                InsertarDetalle(conn, tx, codDev, linea, item);
                ActualizarDevueltaFactura(conn, tx, orden, linea, item);

                // Inventario
                var invDto = new CompraInventarioDto
                {
                    CodProducto = item.cod_producto,
                    Cantidad = item.cantidad,
                    CodBodega = item.cod_bodega,
                    CodTipo = codDev,
                    Origen = "Compra.Dev",
                    Fecha = orden.fecha,
                    Precio = Convert.ToDecimal(item.precio),
                    ImpConsumo = 0,
                    ImpVentas = Convert.ToDecimal(item.imp_ventas),
                    TipoMov = "D",
                    Usuario = orden.usuario
                };

                var inv = _auxInv.sbInvInventario(codEmpresa, invDto); // si no existe cod_empresa en orden, ajusta aquí
                if (inv.Code == -1)
                    throw new InvalidOperationException(inv.Description ?? "Error inventario");
            }

            CrearCargoFlotanteYActualizarSaldo(conn, tx, orden);
        }


        private static void InsertarDetalle(IDbConnection conn, IDbTransaction tx, string codDev, int linea, FacturaDetalleDto item)
        {
            conn.Execute(
                @"INSERT cpr_compra_devDet(linea,cod_compra_dev,cod_producto,cantidad,cod_bodega,precio,imp_ventas,imp_consumo)
                  VALUES(@Linea,@CodDev,@CodProducto,@Cantidad,@CodBodega,@Precio,@ImpVentas,0)",
                new
                {
                    Linea = linea,
                    CodDev = codDev,
                    CodProducto = item.cod_producto,
                    Cantidad = item.cantidad,
                    CodBodega = item.cod_bodega,
                    Precio = item.precio,
                    ImpVentas = item.imp_ventas
                },
                transaction: tx
            );
        }

        private static void ActualizarDevueltaFactura(IDbConnection conn, IDbTransaction tx, DevolucionInsert orden, int linea, FacturaDetalleDto item)
        {
            conn.Execute(
                @"UPDATE cpr_compras_detalle
                  SET cantidad_devuelta = ISNULL(cantidad_devuelta,0) + @Cantidad
                  WHERE linea = @Linea
                    AND cod_factura = @CodFactura
                    AND cod_proveedor = @CodProveedor",
                new
                {
                    Cantidad = item.cantidad,
                    Linea = linea,
                    CodFactura = orden.cod_factura,
                    CodProveedor = orden.cod_proveedor
                },
                transaction: tx
            );
        }

        private static void CrearCargoFlotanteYActualizarSaldo(IDbConnection conn, IDbTransaction tx, DevolucionInsert orden)
        {
            // id siguiente (lock)
            var ultimo = conn.QueryFirstOrDefault<int>(
                @"SELECT ISNULL(MAX(ID),0)
                  FROM cxp_cargosper WITH (UPDLOCK, HOLDLOCK)
                  WHERE cod_proveedor = @CodProveedor",
                new { CodProveedor = orden.cod_proveedor },
                transaction: tx
            );

            var id = ultimo + 1;
            var detalle = $"FACTURA : {orden.cod_factura}\nUSUARIO : {orden.usuario}";

            conn.Execute(
                @"INSERT cxp_cargosper(id,cod_proveedor,cod_cargo,tipo,valor,vence,saldo,concepto,detalle,recaudado)
                  VALUES(@Id,@CodProveedor,@CodCargo,'M',@Valor,GETDATE(),@Saldo,'DEVOLUCION MERCADERIA - FACTURA DE COMPRA',@Detalle,0)",
                new
                {
                    Id = id,
                    CodProveedor = orden.cod_proveedor,
                    CodCargo = orden.cargo,
                    Valor = orden.total,
                    Saldo = orden.total,
                    Detalle = detalle
                },
                transaction: tx
            );

            conn.Execute(
                @"UPDATE cxp_proveedores
                  SET saldo = ISNULL(saldo,0) - @Monto
                  WHERE cod_proveedor = @CodProveedor",
                new { Monto = orden.total, CodProveedor = orden.cod_proveedor },
                transaction: tx
            );
        }

        private static void ActualizarEstadoFactura(IDbConnection conn, IDbTransaction tx, DevolucionInsert orden)
        {
            conn.Execute(
                @"UPDATE cpr_compras
                  SET cxp_estado = 'D'
                  WHERE cod_factura = @CodFactura
                    AND cod_proveedor = @CodProveedor",
                new { CodFactura = orden.cod_factura, CodProveedor = orden.cod_proveedor },
                transaction: tx
            );
        }

        private void TryEnviarCorreo(int codEmpresa, DevolucionInsert orden)
        {
            try
            {
                CorreoNotificacionDevolucion_Enviar(codEmpresa, orden).GetAwaiter().GetResult();
            }
            catch
            {
                // no rompas el guardado por un fallo de correo
            }
        }

        // ----------------------------- VALIDACIONES AUX -----------------------------

        private static string ValidarProductosYBodegas(IDbConnection conn,IReadOnlyCollection<FacturaDetalleDto> lineas,string mov)
        {
            foreach (var linea in lineas)
            {
                var error = ValidarLinea(conn, linea, mov);
                if (!string.IsNullOrEmpty(error))
                    return error;
            }

            return string.Empty;
        }

        private static string? ValidarLinea(IDbConnection conn, FacturaDetalleDto item, string mov)
        {
            if (item is null) return "Línea inválida";
            if (item.cantidad <= 0) return null;

            var errorProd = ValidarProducto(conn, item.cod_producto);
            if (!string.IsNullOrEmpty(errorProd)) return errorProd;

            return ValidarBodega(conn, item.cod_bodega, mov);
        }

        private static string? ValidarProducto(IDbConnection conn, string? codProducto)
        {
            if (string.IsNullOrWhiteSpace(codProducto))
                return "Código de producto inválido";

            var estadoProd = conn.QueryFirstOrDefault<string>(
                @"SELECT estado
                FROM pv_productos
                WHERE cod_producto = @CodProducto",
                new { CodProducto = codProducto }
            );

            if (estadoProd is null)
                return $"El producto {codProducto} no existe";

            if (estadoProd == "I")
                return $"El producto {codProducto} no esta activo";

            return null;
        }

        private static string? ValidarBodega(IDbConnection conn, string? codBodega, string mov)
        {
            if (string.IsNullOrWhiteSpace(codBodega))
                return null;

            var bodega = conn.QueryFirstOrDefault<Models.BodegaDto>(
                @"SELECT permite_entradas, permite_salidas, estado
                FROM pv_bodegas
                WHERE cod_bodega = @CodBodega",
                new { CodBodega = codBodega }
            );

            if (bodega is null)
                return $"La bodega {codBodega} - No existe";

            if (bodega.estado == "I")
                return $"La bodega {codBodega} - Se encuentra Inactiva";

            var (requiereEntrada, requiereSalida) = RequerimientosMovimiento(mov);

            if (requiereEntrada && bodega.permite_entradas != "1")
                return $"La bodega {codBodega} - No permite Entradas";

            if (requiereSalida && bodega.permite_salidas != "1")
                return $"La bodega {codBodega} - No permite Salidas";

            return null;
        }

        private static (bool requiereEntrada, bool requiereSalida) RequerimientosMovimiento(string mov)
        {
            // Si mov viene raro, no forzamos permisos (lo deja pasar) o podrías decidir devolver error aquí.
            return mov switch
            {
                "E" => (true,  false),
                "S" => (false, true),
                "R" => (false, true),
                "T" => (true,  true),
                _   => (false, false)
            };
        }

        private static string ValidarCantidades(IDbConnection conn, DevolucionInsert orden)
        {
            var linea = 0;

            foreach (var l in orden.lineas)
            {
                linea++;

                // lógica original: compara con "cantidad" (no con remanente). Mantengo tu regla.
                var comparacion = conn.QueryFirstOrDefault<string>(
                    @"SELECT CASE
                              WHEN cantidad > @CantidadDev THEN 'Menor'
                              WHEN cantidad < @CantidadDev THEN 'Mayor'
                              ELSE 'Igual'
                            END
                      FROM cpr_compras_detalle
                      WHERE cod_factura = @CodFactura
                        AND cod_proveedor = @CodProveedor
                        AND cod_producto = @CodProducto
                        AND linea = @Linea",
                    new
                    {
                        CantidadDev = l.cantidad,
                        CodFactura = orden.cod_factura,
                        CodProveedor = orden.cod_proveedor,
                        CodProducto = l.cod_producto,
                        Linea = linea
                    }
                );

                if (comparacion == "Mayor")
                    return $" - Las Cantidad devoluciones en la Linea {linea}, es mayor al remanente...";
            }

            return "";
        }

        // ----------------------------- CORREO -----------------------------

        public async Task<ErrorDto> CorreoNotificacionDevolucion_Enviar(int codEmpresa, DevolucionInsert orden)
        {
            var info = DbHelper.CreateOkResponse();

            try
            {
                var correoConfigResult = _envioCorreoDB.CorreoConfig(codEmpresa, "1");
                if (correoConfigResult.Code != 0 || correoConfigResult.Result == null)
                    return DbHelper.ErrorResponse(correoConfigResult.Description ?? "Error configuración correo", -1);

                var eConfig = correoConfigResult.Result;

                var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                {
                    var proveedor = conn.QueryFirstOrDefault<string>(
                        @"SELECT DESCRIPCION FROM CXP_PROVEEDORES WHERE COD_PROVEEDOR = @CodProveedor",
                        new { CodProveedor = orden.cod_proveedor }
                    ) ?? "";

                    var email = conn.QueryFirstOrDefault<string>(
                        @"SELECT email FROM CXP_PROVEEDORES WHERE COD_PROVEEDOR = @CodProveedor",
                        new { CodProveedor = orden.cod_proveedor }
                    ) ?? "";

                    return new { proveedor, email };
                });

                if (r.Code != 0 || r.Result == null)
                    return DbHelper.ErrorResponse(r.Description ?? DefaultError, r.Code ?? -1);

                var body = @$"<html lang=""es"">
<head><meta charset=""UTF-8""></head>
<body>
  <p>Estimado Proveedor: {r.Result.proveedor}</p>
  <p>Se le comunica la devolución de la mercadería de la factura #{orden.cod_factura}</p>
  <p>Debido a {orden.notas}</p>
  <p>ASECCSS</p>
</body>
</html>";

                if (_sendEmail == "Y" && !string.IsNullOrWhiteSpace(r.Result.email))
                {
                    var emailRequest = new EmailRequest
                    {
                        To = r.Result.email,
                        From = eConfig.User,
                        Subject = "Devolución de Solicitud de Compra",
                        Body = body,
                        Attachments = new List<IFormFile>()
                    };

                    await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, info);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }

            return info;
        }
    }
}