using System.Data;
using System.Text;
using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class FrmCprCompraDirectaDB
    {
        private const string DefaultErrorMessage = "Error";
        private readonly PortalDB _portalDb;
        private readonly MProGrXAuxiliarDB _auxiliarDb;
        private readonly MSecurityMainDb _bitacoraDb;
        private readonly MComprasDB _comprasDb;

        public FrmCprCompraDirectaDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _bitacoraDb = new MSecurityMainDb(config);
            _auxiliarDb = new MProGrXAuxiliarDB(config);
            _comprasDb = new MComprasDB(config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data) => _bitacoraDb.Bitacora(data);

        // =========================
        //  GET: Compra Directa
        // =========================
        public ErrorDto<CompraDirectaData?> CompraDirecta_Obtener(int codEmpresa, string codCompra, string codOrden, int codProveedor)
        {
            var sql = new StringBuilder(@"
                SELECT
                    E.*,
                    RTRIM(C.descripcion) AS Causa_Desc,
                    RTRIM(C.Tipo_Orden)  AS Causa_Id,
                    P.descripcion        AS Proveedor,
                    O.nota
                FROM cpr_ordenes O
                INNER JOIN cpr_Tipo_Orden C ON O.Tipo_Orden = C.Tipo_Orden
                INNER JOIN cpr_compras E    ON O.cod_orden = E.cod_orden
                INNER JOIN cxp_proveedores P ON E.cod_proveedor = P.cod_proveedor
                WHERE E.cod_compra = @CodCompra
            ");

            var p = new DynamicParameters();
            p.Add("@CodCompra", codCompra, DbType.String);

            if (!string.Equals(codOrden, "0", StringComparison.OrdinalIgnoreCase))
            {
                sql.Append(" AND E.cod_orden = @CodOrden ");
                p.Add("@CodOrden", codOrden, DbType.String);
            }

            if (codProveedor != 0)
            {
                sql.Append(" AND E.cod_proveedor = @CodProveedor ");
                p.Add("@CodProveedor", codProveedor, DbType.Int32);
            }

            // Usa tu helper
            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                sql.ToString(),
                new CompraDirectaData(),
                p
            );
        }

        // =========================
        //  GET: Detalle Compra Directa
        // =========================
        public ErrorDto<CompraDirectaListaData> CompraDirectaDetalle_Obtener(int codEmpresa, string jFiltros, string? codFactura, int? codProveedor)
        {
            _ = JsonConvert.DeserializeObject<OrderLineaTablaFiltros>(jFiltros) ?? new OrderLineaTablaFiltros();

            var response = new ErrorDto<CompraDirectaListaData> { Result = new CompraDirectaListaData() };

            if (string.IsNullOrWhiteSpace(codFactura) || !codProveedor.HasValue)
                return DbHelper.CreateErrorResponse("Debe indicar factura y proveedor", -1, new CompraDirectaListaData());

            var p = new DynamicParameters();
            p.Add("@CodFactura", codFactura, DbType.String);
            p.Add("@CodProveedor", codProveedor.Value, DbType.Int32);

            // Total
            var totalR = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                @"SELECT COUNT(D.cod_producto)
                  FROM cpr_compras_detalle D
                  INNER JOIN pv_productos P ON D.cod_producto = P.cod_producto
                  WHERE D.cod_factura = @CodFactura
                    AND D.cod_proveedor = @CodProveedor",
                0,
                p
            );

            if (totalR.Code != 0) return DbHelper.CreateErrorResponse(totalR.Description ?? DefaultErrorMessage, totalR.Code ?? -1, new CompraDirectaListaData());

            response.Result.total = totalR.Result;

            // Cantidad (solo si hay lineas)
            if (response.Result.total > 0)
            {
                var cantR = DbHelper.ExecuteSingleQuery<long>(
                    _portalDb,
                    codEmpresa,
                    @"SELECT ISNULL(SUM(D.cantidad),0)
                      FROM cpr_compras_detalle D
                      INNER JOIN pv_productos P ON D.cod_producto = P.cod_producto
                      WHERE D.cod_factura = @CodFactura
                        AND D.cod_proveedor = @CodProveedor",
                    0,
                    p
                );

                if (cantR.Code != 0) return DbHelper.CreateErrorResponse(cantR.Description ?? DefaultErrorMessage, cantR.Code ?? -1, new CompraDirectaListaData());
                response.Result.cantidad = cantR.Result;
            }
            else
            {
                response.Result.cantidad = 0;
            }

            // Lineas
            var qLineas = @"
                SELECT
                    D.cod_producto,
                    P.descripcion,
                    D.cantidad,
                    D.cod_bodega,
                    D.precio,
                    ISNULL(D.descuento,0) AS descuento,
                    D.imp_ventas,
                    0 AS total,
                    CASE WHEN (
                        SELECT U.COD_PRODUCTO
                        FROM CPR_ORDENES_UENS U
                        WHERE U.COD_PRODUCTO = D.cod_producto
                          AND U.COD_ORDEN = C.COD_ORDEN
                        GROUP BY U.COD_ORDEN, U.COD_PRODUCTO
                    ) IS NOT NULL THEN 1 ELSE 0 END AS i_existe,
                    CASE WHEN (
                        SELECT COALESCE((
                            SELECT SUM(U.CANTIDAD)
                            FROM CPR_ORDENES_UENS U
                            WHERE U.COD_PRODUCTO = D.cod_producto
                              AND U.COD_ORDEN = C.COD_ORDEN
                            GROUP BY U.COD_ORDEN, U.COD_PRODUCTO
                        ), 0)
                    ) < D.cantidad THEN 0 ELSE 1 END AS i_completo
                FROM cpr_compras_detalle D
                INNER JOIN pv_productos P ON D.cod_producto = P.cod_producto
                LEFT JOIN cpr_compras C ON C.COD_FACTURA = D.COD_FACTURA
                WHERE D.cod_factura = @CodFactura
                  AND D.cod_proveedor = @CodProveedor
                ORDER BY D.Linea";

            var lineasR = DbHelper.ExecuteListQuery<CompraDirectaDetalle>(_portalDb, codEmpresa, qLineas, p);
            if (lineasR.Code != 0) return DbHelper.CreateErrorResponse(lineasR.Description ?? DefaultErrorMessage, lineasR.Code ?? -1, new CompraDirectaListaData());

            response.Result.lineas = lineasR.Result ?? new List<CompraDirectaDetalle>();
            response.Code = 0;
            return response;
        }

        // =========================
        //  POST: Insertar Compra Directa (TX)
        // =========================
        public ErrorDto CompraDirecta_Insertar(int codEmpresa, CompraDirectaInsert orden)
        {
            // Validación “rápida” (sin BD)
            var v0 = ValidarInputBasico(codEmpresa, orden);
            if (v0.Code != 0) return v0;

            // Validación con BD + ejecución en transacción
            var r = DbHelper.WithConn<string>(_portalDb, codEmpresa, conn =>
            {
                if (conn.State != ConnectionState.Open) conn.Open();
                using var tx = conn.BeginTransaction();

                try
                {
                    // Validación productos/bodegas (sin SQL injection)
                    var errLineas = ValidarLineasConBd(conn, tx, orden.lineas, "E");
                    if (!string.IsNullOrEmpty(errLineas))
                        return errLineas;

                    // Totales (mantiene tu lógica)
                    CalcularTotales(orden.lineas, out _, out _, out _, out _);
                    // Si querés: puedes comparar contra orden.sub_total/orden.total aquí.

                    // 1) Orden
                    var codOrden = GenerarCodigo(conn, tx, "cpr_Ordenes", "cod_orden");
                    InsertarOrden(conn, tx, codOrden, orden);

                    InsertarOrdenProceso(conn, tx, codOrden, orden);
                    InsertarDetalleOrden(conn, tx, codOrden, orden);

                    RegistrarBitacora(codEmpresa, orden.usuario, $"Registra, Orden Compra: {codOrden}");

                    // 2) Compra
                    var codCompra = GenerarCodigo(conn, tx, "cpr_compras", "cod_compra");
                    InsertarCompra(conn, tx, codCompra, codOrden, orden);

                    RegistrarBitacora(codEmpresa, orden.usuario, $"Registra, Compra Directa: {codCompra}");

                    // 3) Proveedor saldo + pago contado
                    ActualizarSaldoProveedor(conn, tx, orden.cod_proveedor, orden.total);

                    if (string.Equals(orden.tipo_pago, "CO", StringComparison.OrdinalIgnoreCase))
                        RegistrarPagoContado(conn, tx, orden);

                    // 4) Detalle compra + inventario + costo
                    InsertarDetalleCompraYActualizarInventario(codEmpresa, conn, tx, codCompra, orden);

                    // 5) Actualiza relación factura-ordenes (si este método abre su propia conexión,
                    // lo ideal es que tenga versión "conn+tx". Aquí lo dejamos como estaba.)
                    _comprasDb.FacturaOrdenes_Actualizar(codEmpresa, orden.cod_factura, orden.cod_proveedor);

                    tx.Commit();
                    return $"{codOrden}-{codCompra}";
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            });

            if (r.Code != 0)
                return DbHelper.ErrorResponse(r.Description ?? DefaultErrorMessage, r.Code ?? -1);

            // éxito
            return DbHelper.OkResponse(r.Result ?? "ok");
        }

        // =========================
        //  Helpers (VALIDACIÓN)
        // =========================
        private ErrorDto ValidarInputBasico(int codEmpresa, CompraDirectaInsert orden)
        {
            if (orden == null) return DbHelper.ErrorResponse("Orden inválida", -1);
            if (string.IsNullOrWhiteSpace(orden.cod_factura)) return DbHelper.ErrorResponse("Debe ingresar el número de factura", -1);
            if (orden.lineas == null || orden.lineas.Count == 0) return DbHelper.ErrorResponse("No hay productos en la orden", -1);

            // Periodo
            if (!_auxiliarDb.fxInvPeriodos(codEmpresa, orden.fecha))
                return DbHelper.ErrorResponse("El periodo en el que desea realizar el movimiento se encuentra cerrado ...", -1);

            return DbHelper.CreateOkResponse();
        }

        private static string ValidarLineasConBd(IDbConnection conn, IDbTransaction tx, List<CompraDirectaDetalle> lineas, string mov)
        {
            foreach (var item in lineas)
            {
                var err = ValidarLinea(conn, tx, item, mov);
                if (!string.IsNullOrEmpty(err)) return err;
            }
            return string.Empty;
        }

        private static string? ValidarLinea(IDbConnection conn, IDbTransaction tx, CompraDirectaDetalle item, string mov)
        {
            if (item == null) return "Línea inválida";
            if (item.cantidad <= 0) return null;

            var errProd = ValidarProducto(conn, tx, item.cod_producto);
            if (!string.IsNullOrEmpty(errProd)) return errProd;

            return ValidarBodega(conn, tx, item.cod_bodega, mov);
        }

        private static string? ValidarProducto(IDbConnection conn, IDbTransaction tx, string? codProducto)
        {
            if (string.IsNullOrWhiteSpace(codProducto))
                return "Código de producto inválido";

            var estado = conn.QueryFirstOrDefault<string>(
                @"SELECT estado FROM pv_productos WHERE cod_producto = @CodProducto",
                new { CodProducto = codProducto },
                transaction: tx
            );

            if (estado == null) return $"El producto {codProducto} no existe";
            if (estado == "I") return $"El producto {codProducto} no esta activo";
            return null;
        }

        private static string? ValidarBodega(IDbConnection conn, IDbTransaction tx, string? codBodega, string mov)
        {
            if (string.IsNullOrWhiteSpace(codBodega))
                return "Debe indicar bodega";

            var bodega = conn.QueryFirstOrDefault<Models.BodegaDto>(
                @"SELECT permite_entradas, permite_salidas, estado
                  FROM pv_bodegas
                  WHERE cod_bodega = @CodBodega",
                new { CodBodega = codBodega },
                transaction: tx
            );

            if (bodega == null) return $"La bodega {codBodega} - No existe";
            if (bodega.estado == "I") return $"La bodega {codBodega} - Se encuentra Inactiva";

            var (requiereEntrada, requiereSalida) = mov switch
            {
                "E" => (true, false),
                "S" => (false, true),
                "R" => (false, true),
                "T" => (true, true),
                _ => (false, false)
            };

            if (requiereEntrada && bodega.permite_entradas != "1")
                return $"La bodega {codBodega} - No permite Entradas";

            if (requiereSalida && bodega.permite_salidas != "1")
                return $"La bodega {codBodega} - No permite Salidas";

            return null;
        }

        // =========================
        //  Helpers (TOTALES)
        // =========================
        private static void CalcularTotales(
            List<CompraDirectaDetalle> lineas,
            out float descuento,
            out float iv,
            out float cantidad,
            out float total)
        {
            float subTotal = 0;
            descuento = 0;
            iv = 0;
            cantidad = 0;

            foreach (var item in lineas)
            {
                subTotal += (item.cantidad * item.precio);

                var tmpDesc = (item.cantidad * item.precio) * (item.descuento / 100);
                descuento += tmpDesc;

                var tmpIv = ((item.cantidad * item.precio) - tmpDesc) * (item.imp_ventas / 100);
                iv += tmpIv;

                item.total = (item.cantidad * item.precio) - tmpDesc + tmpIv;
                cantidad += item.cantidad;
            }

            total = subTotal + iv - descuento;
        }

        // =========================
        //  Helpers (INSERTS / UPDATES) - TX
        // =========================
        private static string GenerarCodigo(IDbConnection conn, IDbTransaction tx, string tabla, string campo)
        {
            // Mantiene tu patrón (MAX+1). Mejor reemplazar por SEQUENCE/SP si existe.
            var sql = $@"SELECT RIGHT(REPLICATE('0', 10) + CAST(ISNULL(MAX(CAST({campo} AS INT)),0) + 1 AS VARCHAR(10)), 10)
                         FROM {tabla}";
            return conn.QueryFirstOrDefault<string>(sql, transaction: tx) ?? "0000000001";
        }

        private static void InsertarOrden(IDbConnection conn, IDbTransaction tx, string codOrden, CompraDirectaInsert orden)
        {
            conn.Execute(@"
                INSERT INTO cpr_ordenes
                (
                    cod_orden, Tipo_Orden, estado, nota, genera_user, genera_fecha,
                    subtotal, descuento, imp_ventas, total,
                    autoriza_fecha, autoriza_user, pin_autorizacion, pin_entrada, proceso, cod_proveedor
                )
                VALUES
                (
                    @CodOrden, @Causa, 'A', @Notas, @Usuario, GETDATE(),
                    @SubTotal, @Descuento, @ImpVentas, @Total,
                    GETDATE(), @Usuario, 0, '', 'D', @CodProveedor
                )",
                new
                {
                    CodOrden = codOrden,
                    Causa = orden.causa,
                    Notas = orden.notas ?? string.Empty,
                    Usuario = (orden.usuario ?? string.Empty).ToUpperInvariant(),
                    SubTotal = orden.sub_total,
                    Descuento = orden.descuento,
                    ImpVentas = orden.imp_ventas,
                    Total = orden.total,
                    CodProveedor = orden.cod_proveedor
                },
                transaction: tx
            );
        }

        private static void InsertarOrdenProceso(IDbConnection conn, IDbTransaction tx, string codOrden, CompraDirectaInsert orden)
        {
            conn.Execute(@"
                INSERT INTO CPR_ORDENES_PROCESO
                (
                    COD_ORDEN, COD_PROVEEDOR, REGISTRO_FECHA, REGISTRO_USUARIO,
                    COTIZA_FECHA, COTIZA_USUARIO, ADJUDICA_FECHA, ADJUDICA_USUARIO, NOTAS
                )
                VALUES
                (
                    @CodOrden, @CodProveedor, GETDATE(), @Usuario,
                    GETDATE(), @Usuario, GETDATE(), @Usuario, 'Compra Directa!'
                )",
                new
                {
                    CodOrden = codOrden,
                    CodProveedor = orden.cod_proveedor,
                    Usuario = orden.usuario ?? string.Empty
                },
                transaction: tx
            );
        }

        private static void InsertarDetalleOrden(IDbConnection conn, IDbTransaction tx, string codOrden, CompraDirectaInsert orden)
        {
            conn.Execute(
                @"DELETE FROM cpr_ordenes_detalle WHERE cod_orden = @CodOrden",
                new { CodOrden = codOrden },
                transaction: tx
            );

            var linea = 0;
            foreach (var item in orden.lineas)
            {
                linea++;

                conn.Execute(@"
                    INSERT INTO cpr_ordenes_detalle
                    (
                        linea, cod_orden, cod_producto, cantidad,
                        estado, cantidad_despachada,
                        precio, descuento, imp_ventas, imp_consumo
                    )
                    VALUES
                    (
                        @Linea, @CodOrden, @CodProducto, @Cantidad,
                        'D', 0,
                        @Precio, @Descuento, @ImpVentas, 0
                    )",
                    new
                    {
                        Linea = linea,
                        CodOrden = codOrden,
                        CodProducto = item.cod_producto,
                        Cantidad = item.cantidad,
                        Precio = item.precio,
                        Descuento = item.descuento,
                        ImpVentas = item.imp_ventas
                    },
                    transaction: tx
                );
            }
        }

        private static void InsertarCompra(IDbConnection conn, IDbTransaction tx, string codCompra, string codOrden, CompraDirectaInsert orden)
        {
            var cxpEstado = string.Equals(orden.forma_pago, "CR", StringComparison.OrdinalIgnoreCase) ? "P" : "G";

            conn.Execute(@"
                INSERT INTO cpr_compras
                (
                    estado, cod_factura, forma_pago, cod_proveedor, cod_compra, cod_orden,
                    genera_user, genera_fecha, fecha,
                    sub_total, descuento, imp_ventas, imp_consumo,
                    total, cxp_estado, asiento_estado, divisa, tipo_pago
                )
                VALUES
                (
                    'P', @CodFactura, @TipoPago, @CodProveedor, @CodCompra, @CodOrden,
                    @Usuario, @Fecha, @Fecha,
                    @SubTotal, @Descuento, @ImpVentas, 0,
                    @Total, @CxpEstado, 'P', @Divisa, @FormaPago
                )",
                new
                {
                    CodFactura = orden.cod_factura,
                    TipoPago = orden.tipo_pago,          // respeta tu diseño original
                    CodProveedor = orden.cod_proveedor,
                    CodCompra = codCompra,
                    CodOrden = codOrden,
                    Usuario = orden.usuario ?? string.Empty,
                    Fecha = orden.fecha,
                    SubTotal = orden.sub_total,
                    Descuento = orden.descuento,
                    ImpVentas = orden.imp_ventas,
                    Total = orden.total,
                    CxpEstado = cxpEstado,
                    Divisa = orden.divisa,
                    FormaPago = orden.forma_pago
                },
                transaction: tx
            );
        }

        private static void ActualizarSaldoProveedor(IDbConnection conn, IDbTransaction tx, int codProveedor, float saldo)
        {
            conn.Execute(
                @"UPDATE cxp_proveedores
                  SET saldo = ISNULL(saldo,0) + @Saldo
                  WHERE cod_proveedor = @CodProveedor",
                new { Saldo = saldo, CodProveedor = codProveedor },
                transaction: tx
            );
        }

        private static void RegistrarPagoContado(IDbConnection conn, IDbTransaction tx, CompraDirectaInsert compra)
        {
            conn.Execute(@"
                INSERT INTO cxp_pagoProv
                (
                    NPago, Cod_Proveedor, Cod_Factura, Fecha_Vencimiento, Monto, Frecuencia, Tipo_Transac,
                    User_TrasLada, Fecha_Traslada, Tesoreria, Pago_Tercero, Apl_Cargo_Flotante,
                    Pago_Anticipado, forma_pago, IMPORTE_DIVISA_REAL
                )
                VALUES
                (
                    1, @CodProveedor, @CodFactura, GETDATE(), @Monto, 0, 0,
                    NULL, NULL, NULL, '', 0,
                    0, 'CO', @Monto
                )",
                new
                {
                    CodProveedor = compra.cod_proveedor,
                    CodFactura = compra.cod_factura,
                    Monto = compra.total
                },
                transaction: tx
            );
        }

        private void InsertarDetalleCompraYActualizarInventario(int codEmpresa, IDbConnection conn, IDbTransaction tx, string codCompra, CompraDirectaInsert orden)
        {
            conn.Execute(
                @"DELETE FROM cpr_compras_detalle
                  WHERE cod_factura = @CodFactura AND cod_proveedor = @CodProveedor",
                new { CodFactura = orden.cod_factura, CodProveedor = orden.cod_proveedor },
                transaction: tx
            );

            var linea = 0;
            foreach (var item in orden.lineas)
            {
                linea++;

                conn.Execute(@"
                    INSERT INTO cpr_compras_detalle
                    (
                        linea, cod_factura, cod_proveedor, cod_producto,
                        cantidad, cod_bodega,
                        precio, descuento, imp_ventas, imp_consumo
                    )
                    VALUES
                    (
                        @Linea, @CodFactura, @CodProveedor, @CodProducto,
                        @Cantidad, @CodBodega,
                        @Precio, @Descuento, @ImpVentas, 0
                    )",
                    new
                    {
                        Linea = linea,
                        CodFactura = orden.cod_factura,
                        CodProveedor = orden.cod_proveedor,
                        CodProducto = item.cod_producto,
                        Cantidad = item.cantidad,
                        CodBodega = item.cod_bodega,
                        Precio = item.precio,
                        Descuento = item.descuento,
                        ImpVentas = item.imp_ventas
                    },
                    transaction: tx
                );

                // Inventario (si falla => rollback)
                var invReq = new CompraInventarioDto
                {
                    CodProducto = item.cod_producto,
                    Cantidad = Decimal.Parse(item.cantidad.ToString()),
                    CodBodega = item.cod_bodega,
                    CodTipo = codCompra,
                    Origen = "Compra",
                    Fecha = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                    Precio = decimal.Parse(item.precio.ToString()),
                    ImpVentas = decimal.Parse(item.imp_ventas.ToString()),
                    ImpConsumo = 0,
                    TipoMov = "E",
                    Usuario = orden.usuario
                };

                var inv = _auxiliarDb.sbInvInventario(codEmpresa, invReq);
                if (inv.Code != 0)
                {
                    // Use InvalidOperationException for business logic errors, or handle with ErrorDto as per project convention
                    throw new InvalidOperationException(inv.Description ?? "Error en inventario");
                }

                // Costo artículos
                _ = CostoArticulos_Actualiza(codEmpresa, orden.usuario, codCompra);
            }
        }

        private void RegistrarBitacora(int empresaId, string usuario, string detalle)
        {
            _ = Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = empresaId,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = "Registra - WEB",
                Modulo = 35
            });
        }

        // =========================
        //  SP: Actualiza Costos (usa helper/parametrizado)
        // =========================
        public ErrorDto CostoArticulos_Actualiza(int codEmpresa, string usuario, string codCompra)
        {
            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                @"EXEC spCRPActualizaCts @Boleta, @Usuario",
                new { Boleta = codCompra, Usuario = usuario }
            );
        }
    }
}