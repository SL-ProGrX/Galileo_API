using Dapper;
using Galileo.Models;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;


namespace Galileo.DataBaseTier
{
    public class FrmCprComprasOrdenDB
    {
        private const string ErrorLiteral = "Ocurrió un error inesperado.";
        private const string BitacoraMovimientoRegistra = "Registra";
        private readonly PortalDB _portalDb;

        private readonly MProGrXAuxiliarDB _mProGrxAuxiliar;
        private readonly MSecurityMainDb _dbBitacora;
        private readonly MComprasDB _mComprasDb;
        private readonly EnvioCorreoDB _envioCorreoDB;

        private readonly string sendEmail = "";
        private readonly string Notificaciones = "";

        public FrmCprComprasOrdenDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);

            _dbBitacora = new MSecurityMainDb(config);
            _mProGrxAuxiliar = new MProGrXAuxiliarDB(config);
            _mComprasDb = new MComprasDB(config);

            _envioCorreoDB = new EnvioCorreoDB(config);

            sendEmail = config.GetSection("AppSettings").GetSection("EnviaEmail").Value?.ToString() ?? "";
            Notificaciones = config.GetSection("AppSettings").GetSection("Notificaciones").Value?.ToString() ?? "";
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data) => _dbBitacora.Bitacora(data);

        // ===========================
        //  CONSULTAS
        // ===========================

        public ErrorDto<OrdenCompraSinFacturaData> Orden_Obtener(int codEmpresa, string codOrden)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

            const string sql = @"
            SELECT
                O.*,
                RTRIM(C.tipo_orden) AS Causa_ID,
                RTRIM(C.descripcion) AS Causa_Desc,
                Prov.Descripcion AS Proveedor,
                RIGHT(REPLICATE('0', 10) + CAST(sp.CPR_ID AS VARCHAR), 10) AS no_solicitud
            FROM cpr_ordenes O
            INNER JOIN cpr_Tipo_Orden C ON O.tipo_orden = C.tipo_orden
            INNER JOIN CxP_Proveedores Prov ON O.cod_Proveedor = Prov.cod_proveedor
            LEFT JOIN CPR_SOLICITUD_PROV sp
                ON sp.ADJUDICA_ORDEN = O.COD_ORDEN
            AND sp.PROVEEDOR_CODIGO = O.COD_PROVEEDOR
            WHERE
                O.cod_orden = @CodOrden
                AND O.estado = 'A'
                AND O.Proceso IN ('A','D','X');";

               return QuerySingleOrNew<OrdenCompraSinFacturaData>(conn, sql, new { CodOrden = codOrden });
            });
        }

        public ErrorDto<OrdenCompraFacturaData> OrdenFactura_Obtener(int codEmpresa, string codCompra)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                const string sql = @"
                SELECT
                    E.*,
                    (RTRIM(C.Tipo_Orden) + ' - ' + C.descripcion) AS Causa,
                    P.descripcion AS Proveedor,
                    O.nota,
                    E.notas AS CompraNotas,
                    RIGHT(REPLICATE('0', 10) + CAST(sp.CPR_ID AS VARCHAR), 10) AS no_solicitud
                FROM cpr_ordenes O
                INNER JOIN cpr_Tipo_Orden C ON O.Tipo_Orden = C.Tipo_Orden
                INNER JOIN cpr_Compras E ON O.cod_orden = E.cod_orden
                LEFT JOIN CPR_SOLICITUD_PROV sp
                    ON sp.ADJUDICA_ORDEN = O.COD_ORDEN
                AND sp.PROVEEDOR_CODIGO = O.COD_PROVEEDOR
                INNER JOIN cxp_proveedores P ON E.cod_proveedor = P.cod_proveedor
                WHERE E.cod_compra = @CodCompra;";

                return QuerySingleOrNew<OrdenCompraFacturaData>(conn, sql, new { CodCompra = codCompra });
            });
        }

        public ErrorDto<CompraOrdenLineasData> OrdenesDetalleF_Obtener(int codEmpresa, CompraOrderLineaTablaFiltros filtros)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                var response = new CompraOrdenLineasData();
                var p = new { CodFactura = filtros.CodOrden, CodProveedor = filtros.CodProveedor };

                const string qTotal = @"
                            SELECT COUNT(D.cod_producto)
                            FROM cpr_Compras_detalle D
                            INNER JOIN pv_productos P ON D.cod_producto = P.cod_producto
                            WHERE D.cod_factura = @CodFactura AND D.cod_proveedor = @CodProveedor;";


                const string qCantidad = @"
                            SELECT ISNULL(SUM(D.cantidad), 0)
                            FROM cpr_Compras_detalle D
                            INNER JOIN pv_productos P ON D.cod_producto = P.cod_producto
                            WHERE D.cod_factura = @CodFactura AND D.cod_proveedor = @CodProveedor;";

                FillTotals(response, conn, qTotal, qCantidad, p);

                const string sql = @"
                            SELECT
                                D.cod_producto,
                                P.descripcion,
                                P.COD_UNIDAD AS unidad,
                                od.CANTIDAD AS qtyOrg,
                                0 AS qtyPend,
                                D.cantidad,
                                D.cod_bodega,
                                D.precio,
                                ISNULL(D.descuento,0) AS descuento,
                                D.imp_ventas,
                                0 AS Total,
                                NULL AS tipoProd,
                                ppc.DESCRIPCION AS familia
                            FROM cpr_Compras_detalle D
                            INNER JOIN pv_productos P ON D.cod_producto = P.cod_producto
                            LEFT JOIN CPR_COMPRAS C ON C.COD_FACTURA = D.COD_FACTURA
                            LEFT JOIN cpr_ordenes_detalle od
                                ON od.COD_ORDEN = C.COD_ORDEN
                            AND od.COD_PRODUCTO = D.COD_PRODUCTO
                            LEFT JOIN PV_PROD_CLASIFICA ppc ON ppc.COD_PRODCLAS = P.COD_PRODCLAS
                            WHERE D.cod_factura = @CodFactura AND D.cod_proveedor = @CodProveedor
                            ORDER BY D.cod_producto;";

                response.lineas = QueryLineas(conn, sql, p);
                return response;
            });
        }

        public ErrorDto<CompraOrdenLineasData> OrdenesDetalleO_Obtener(int codEmpresa, CompraOrderLineaTablaFiltros filtros)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                var response = new CompraOrdenLineasData();
                var p = new { CodOrden = filtros.CodOrden };

                const string qTotal = @"
                            SELECT COUNT(D.cod_producto)
                            FROM cpr_ordenes_detalle D
                            INNER JOIN pv_productos P ON D.cod_producto = P.cod_producto
                            WHERE D.cod_orden = @CodOrden;";


                const string qCantidad = @"
                            SELECT ISNULL(SUM(D.cantidad),0)
                            FROM cpr_ordenes_detalle D
                            INNER JOIN pv_productos P ON D.cod_producto = P.cod_producto
                            WHERE D.cod_orden = @CodOrden;";

                FillTotals(response, conn, qTotal, qCantidad, p);

                var like = BuildLike(filtros.filtro);
                var hasFiltro = !string.IsNullOrWhiteSpace(like);

                // Nota: el SQL es fijo; solo activamos el filtro por parámetro
                var sql = @"
                    SELECT
                        cod_producto, descripcion, unidad, qtyOrg, qtyPend, Cantidad, cod_bodega, precio, Descuento, imp_ventas, Total, tipoProd, familia
                    FROM (
                        SELECT
                            D.cod_producto,
                            P.descripcion,
                            P.COD_UNIDAD AS unidad,
                            D.cantidad AS qtyOrg,
                            (D.cantidad - SUM(ISNULL(ccd.cantidad, 0))) AS qtyPend,
                            (D.cantidad - SUM(ISNULL(ccd.cantidad, 0))) AS Cantidad,
                            '' AS cod_bodega,
                            D.precio,
                            ISNULL(D.descuento, 0) AS Descuento,
                            D.imp_ventas,
                            0 AS Total,
                            P.TIPO_PRODUCTO AS tipoProd,
                            ppc.DESCRIPCION AS familia
                        FROM cpr_ordenes_detalle D
                        INNER JOIN pv_productos P ON D.cod_producto = P.cod_producto
                        LEFT JOIN CPR_COMPRAS cc ON cc.COD_ORDEN = D.cod_orden
                        LEFT JOIN cpr_compras_detalle ccd
                            ON ccd.cod_factura = cc.cod_factura
                        AND ccd.cod_producto = D.cod_producto
                        LEFT JOIN PV_PROD_CLASIFICA ppc ON ppc.COD_PRODCLAS = P.COD_PRODCLAS
                        WHERE D.cod_orden = @CodOrden
                        AND (@HasFiltro = 0 OR (D.cod_producto LIKE @Like OR P.descripcion LIKE @Like))
                        GROUP BY
                            D.cod_producto, P.descripcion, P.COD_UNIDAD, D.cantidad,
                            D.precio, D.descuento, D.imp_ventas, P.TIPO_PRODUCTO, ppc.DESCRIPCION
                    ) T
                    ORDER BY cod_producto
                    OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

                var (offset, fetch) = NormalizePaging(filtros.pagina, filtros.paginacion, 50);


                response.lineas = QueryLineas(conn, sql, new
                {
                    CodOrden = filtros.CodOrden,
                    HasFiltro = hasFiltro ? 1 : 0,
                    Like = like,
                    Offset = offset,
                    Fetch = fetch
                });              

                return response;
            });
        }

        private ErrorDto OrdenCosto_Actualiza(int codEmpresa, string codCompra, string usuario)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                const string procedure = "spCRP_W_CostosArticulos_Actualizar";
                conn.Execute(procedure, new { Boleta = codCompra, Usuario = usuario }, commandType: CommandType.StoredProcedure);

                return DbHelper.OkResponse("ok");
            }).Result ?? DbHelper.ErrorResponse(ErrorLiteral, -1);
        }

        // ===========================
        //  GUARDAR COMPRA DESDE ORDEN
        // ===========================

        public ErrorDto ComprasOrden_Guardar(int codEmpresa, ComprasOrdenDatos orden)
        {
            var basic = ValidarFacturaNoVacia(orden);
            if (basic != null) return basic;

            var valida = ValidarCompraOrden(codEmpresa, orden);
            if (valida.Code != 0) return valida;

            var r = DbHelper.WithConn<ErrorDto>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);
                using var tx = conn.BeginTransaction();

                try
                {
                    var pinR = ValidarPinCompra(conn, tx, orden);
                    if (pinR.Code != 0) return Rollback(tx, pinR);

                    var consecutivo = GenerarConsecutivoCompra(conn, tx);

                    var totales = CalcularTotalesCompra(orden.lineas);

                    var head = InsertarCompraEncabezado(conn, tx, codEmpresa, orden, consecutivo);
                    if (head.Code != 0) return Rollback(tx, head);

                    var post = ProcesarCxP(conn, tx, orden, totales);
                    if (post.Code != 0) return Rollback(tx, post);

                    var det = GuardarCompraDetalle(conn, tx, codEmpresa, orden, consecutivo);
                    if (det.Code != 0) return Rollback(tx, det);

                    // Estados / costos / actualizaciones
                    _mComprasDb.sbCprOrdenesDespacho(codEmpresa, orden.cod_orden);
                    _ = OrdenCosto_Actualiza(codEmpresa, consecutivo, orden.genera_user);
                    _mComprasDb.FacturaOrdenes_Actualizar(codEmpresa, orden.cod_factura, orden.cod_proveedor);

                    tx.Commit();

                    // No hacemos await aquí para no cambiar la firma pública
                    _ = CorreoNotificaRegistraFactura_Enviar(codEmpresa, orden.factura, orden.genera_user, orden.cod_proveedor);

                    return DbHelper.OkResponse(consecutivo);
                }
                catch (Exception ex)
                {
                    return Rollback(tx, DbHelper.ErrorResponse(ex.Message, -1));
                }
            });

            return UnwrapResult(r);
        }

        private static ErrorDto? ValidarFacturaNoVacia(ComprasOrdenDatos orden)
        {
            if (string.IsNullOrWhiteSpace(orden.cod_factura))
                return DbHelper.ErrorResponse("El campo Factura no puede ser nulo", -1);

            return null;
        }

        private ErrorDto ValidarCompraOrden(int codEmpresa, ComprasOrdenDatos orden)
        {
            // Validaciones que requieren BD las hacemos en una sola conexión (sin inyección)
            var r = DbHelper.WithConn<string>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                var msg = "";
                msg += ValidarFacturaCampo(orden);
                msg += ValidarFacturaDuplicada(conn, orden.cod_factura, orden.cod_proveedor);
                msg += ValidarProductosYBodegas(conn, orden.lineas, "E");
                msg += ValidarTotalesFactura(conn, orden);
                msg += ValidarPeriodoInventario(codEmpresa, orden.fecha);

                return msg.Trim();
            });

            if (r.Code != 0)
                return DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, r.Code ?? -1);

            if (!string.IsNullOrWhiteSpace(r.Result))
                return DbHelper.ErrorResponse(r.Result, -1);

            return DbHelper.CreateOkResponse();
        }

        private static string ValidarFacturaCampo(ComprasOrdenDatos orden)
        {
            if (orden.factura == null) return " - Factura no puede ser nulo";
            if (orden.factura.Length == 0) return " - Factura no puede ser vacio";
            return "";
        }

        private static string ValidarFacturaDuplicada(IDbConnection conn, string codFactura, int codProveedor)
        {
            const string sql = "SELECT COUNT(*) FROM CPR_COMPRAS WHERE cod_factura = @Factura AND cod_proveedor = @Proveedor;";
            var existe = conn.Query<int>(sql, new { Factura = codFactura, Proveedor = codProveedor }).FirstOrDefault();
            return existe > 0 ? " - El número de factura ya existe para el proveedor seleccionado." : "";
        }

        private static string ValidarTotalesFactura(IDbConnection conn, ComprasOrdenDatos orden)
        {
            var r = fxVerificaTotalesFac_Conn(conn, orden);
            return r.Code == -1 ? $" - {r.Description}" : "";
        }

        private string ValidarPeriodoInventario(int codEmpresa, string fecha)
        {
            // En tu código original: si fxInvPeriodos == false => cerrado
            return !_mProGrxAuxiliar.fxInvPeriodos(codEmpresa, fecha)
                ? " - El periodo en el que desea realizar el movimiento se encuentra cerrado ..."
                : "";
        }

        private static ErrorDto Rollback(IDbTransaction tx, ErrorDto dto)
        {
            tx.Rollback();
            return dto;
        }

        private static ErrorDto UnwrapResult(ErrorDto<ErrorDto> r)
        {
            if (r.Code != 0 || r.Result == null)
                return DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, r.Code ?? -1);

            return r.Result;
        }

        private static void EnsureOpen(IDbConnection conn)
        {
            if (conn.State != ConnectionState.Open) conn.Open();
        }

        private static string BuildLike(string? filtro)
        {
            filtro = (filtro ?? "").Trim();
            return filtro.Length == 0 ? "" : $"%{filtro}%";
        }

        private static (int Offset, int Fetch) NormalizePaging(int? pagina, int? paginacion, int defaultFetch)
        {
            var offset = pagina ?? 0;
            if (offset < 0) offset = 0;

            var fetch = paginacion ?? defaultFetch;
            if (fetch <= 0) fetch = defaultFetch;

            return (offset, fetch);
        }

        private static T QuerySingleOrNew<T>(IDbConnection conn, string sql, object param) where T : class, new()
        {
            var result = conn.QueryFirstOrDefault<T>(sql, param);
            return result ?? new T();
        }

        private static void FillTotals(CompraOrdenLineasData response, IDbConnection conn, string totalSql, string cantidadSql, object param)
        {
            response.total = conn.Query<int>(totalSql, param).FirstOrDefault();
            response.cantidad = conn.Query<long>(cantidadSql, param).FirstOrDefault();
        }

        private static List<OrdenCompraDetalleData> QueryLineas(IDbConnection conn, string sql, object param)
            => conn.Query<OrdenCompraDetalleData>(sql, param).ToList();

        private static ErrorDto ValidarPinCompra(IDbConnection conn, IDbTransaction tx, ComprasOrdenDatos orden)
        {
            if (string.IsNullOrWhiteSpace(orden.pin))
                return ValidarPinRequerido(conn, tx, orden.cod_orden);

            return ValidarPinIngresado(conn, tx, orden.cod_orden, orden.pin);
        }

        private static ErrorDto ValidarPinRequerido(IDbConnection conn, IDbTransaction tx, string codOrden)
        {
            const string sql = "SELECT pin_autorizacion FROM cpr_ordenes WHERE cod_orden = @CodOrden;";
            var pin = conn.Query<int>(sql, new { CodOrden = codOrden }, transaction: tx).FirstOrDefault();

            // En tu lógica original: si pin_autorizacion == 1 => devolver Code = 2
            return pin == 1
                ? new ErrorDto { Code = 2, Description = "" }
                : DbHelper.CreateOkResponse();
        }

        private static ErrorDto ValidarPinIngresado(IDbConnection conn, IDbTransaction tx, string codOrden, string pin)
        {
            const string sql = @"
SELECT ISNULL(COUNT(*),0)
FROM cpr_ordenes
WHERE cod_orden = @CodOrden AND pin_entrada = @Pin;";

            var ok = conn.Query<int>(sql, new { CodOrden = codOrden, Pin = pin }, transaction: tx).FirstOrDefault();
            return ok == 0
                ? DbHelper.ErrorResponse("El Pin de Compra suministrado no es correcto...", -1)
                : DbHelper.CreateOkResponse();
        }

        private static string GenerarConsecutivoCompra(IDbConnection conn, IDbTransaction tx)
        {
            const string sql = "SELECT ISNULL(MAX(cod_compra),0) + 1 FROM cpr_compras;";
            var consecutivo = conn.Query<string>(sql, transaction: tx).FirstOrDefault() ?? "0";
            return consecutivo.PadLeft(10, '0');
        }

        private sealed record TotalesCompra(float SubTotal, float Descuento, float ImpVentas, float Cantidad, float Total);

        private static TotalesCompra CalcularTotalesCompra(List<OrdenCompraDetalleData> lineas)
        {
            float sub = 0, desc = 0, iv = 0, cant = 0, total = 0;

            foreach (var item in lineas)
            {
                sub += item.cantidad * item.precio;

                var tmpDesc = (item.cantidad * item.precio) * (item.descuento / 100);
                desc += tmpDesc;

                var tmpIv = ((item.cantidad * item.precio) - tmpDesc) * (item.imp_ventas / 100);
                iv += tmpIv;

                item.total = (item.cantidad * item.precio) - tmpDesc + tmpIv;
                cant += item.cantidad;
            }

            total = sub + iv - desc;
            return new TotalesCompra(sub, desc, iv, cant, total);
        }

        private ErrorDto InsertarCompraEncabezado(
            IDbConnection conn,
            IDbTransaction tx,
            int codEmpresa,
            ComprasOrdenDatos orden,
            string consecutivo
            )
        {
            var esCredito = string.Equals(orden.tipo_pago, "CR", StringComparison.OrdinalIgnoreCase);
            var cxpEstado = esCredito ? "P" : "G";

            const string sql = @"
INSERT INTO cpr_Compras
(
    estado, cod_factura, forma_pago, cod_proveedor, cod_compra, cod_orden,
    genera_user, genera_fecha, fecha,
    sub_total, descuento, imp_ventas, imp_consumo, total,
    cxp_estado, asiento_estado, notas
)
VALUES
(
    'P',
    @CodFactura,
    @FormaPago,
    @CodProveedor,
    @CodCompra,
    @CodOrden,
    @Usuario,
    @Fecha,
    @Fecha,
    @SubTotal,
    @Descuento,
    @ImpVentas,
    0,
    @Total,
    @CxpEstado,
    'P',
    @Notas
);";

            conn.Execute(sql, new
            {
                CodFactura = orden.cod_factura,
                FormaPago = orden.forma_pago,
                CodProveedor = orden.cod_proveedor,
                CodCompra = consecutivo,
                CodOrden = orden.cod_orden,
                Usuario = orden.genera_user,
                Fecha = orden.fecha,
                SubTotal = orden.sub_total,
                Descuento = orden.descuento,
                ImpVentas = orden.imp_ventas,
                Total = orden.total,
                CxpEstado = cxpEstado,
                Notas = orden.notas ?? ""
            }, transaction: tx);

            _ = Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = orden.genera_user,
                DetalleMovimiento = "Registra, Compra:" + consecutivo,
                Movimiento = "Registra - WEB",
                Modulo = 35
            });

            return DbHelper.CreateOkResponse();
        }

        private static ErrorDto ProcesarCxP(IDbConnection conn, IDbTransaction tx, ComprasOrdenDatos orden, TotalesCompra totales)
        {
            var esCredito = string.Equals(orden.tipo_pago, "CR", StringComparison.OrdinalIgnoreCase);

            return esCredito
                ? ActualizarSaldoProveedor(conn, tx, orden.cod_proveedor, totales.Total)
                : RegistrarPagoContado(conn, tx, orden, totales.Total);
        }

        private static ErrorDto ActualizarSaldoProveedor(IDbConnection conn, IDbTransaction tx, int codProveedor, float total)
        {
            const string sql = @"
UPDATE cxp_proveedores
SET saldo = ISNULL(saldo,0) + @Total
WHERE cod_proveedor = @CodProveedor;";

            conn.Execute(sql, new { Total = total, CodProveedor = codProveedor }, transaction: tx);
            return DbHelper.CreateOkResponse();
        }

        private static ErrorDto RegistrarPagoContado(IDbConnection conn, IDbTransaction tx, ComprasOrdenDatos orden, float total)
        {
            const string sql = @"
INSERT cxp_pagoProv
(
    NPago, Cod_Proveedor, Cod_Factura, Fecha_Vencimiento, Monto, Frecuencia,
    Tipo_Transac, User_TrasLada, Fecha_Traslada, Tesoreria, Pago_Tercero,
    Apl_Cargo_Flotante, Pago_Anticipado, forma_pago, IMPORTE_DIVISA_REAL
)
VALUES
(
    1, @CodProveedor, @CodFactura, GETDATE(), @Monto, 0,
    0, @Usuario, GETDATE(), 0, '', 0, 0, 'CO', @Monto
);";

            conn.Execute(sql, new
            {
                CodProveedor = orden.cod_proveedor,
                CodFactura = orden.cod_factura,
                Usuario = orden.genera_user,
                Monto = total
            }, transaction: tx);

            return DbHelper.CreateOkResponse();
        }

        private ErrorDto GuardarCompraDetalle(
            IDbConnection conn,
            IDbTransaction tx,
            int codEmpresa,
            ComprasOrdenDatos orden,
            string consecutivo)
        {
            // Limpia detalle
            const string qLimpia = @"
DELETE cpr_Compras_detalle
WHERE cod_factura = @CodFactura AND cod_proveedor = @CodProveedor;";

            conn.Execute(qLimpia, new { CodFactura = orden.cod_factura, CodProveedor = orden.cod_proveedor }, transaction: tx);

            var ctx = new ArticulosActivosContext(codEmpresa, consecutivo, orden.cod_factura, orden.genera_user, orden.cod_orden, orden.cod_proveedor);

            var linea = 0;
            foreach (var item in orden.lineas)
            {
                linea++;

                var ins = InsertarLineaCompra(conn, tx, orden, item, linea);
                if (ins.Code != 0) return ins;

                _ = ActualizarDespachoOrden(conn, tx, orden.cod_orden, item.cod_producto);

                // Activos (si aplica)
                var activos = BuscoArticulosActivos(conn, tx, ctx, item);
                if (activos.Code != 0) return activos;

                // Inventario (usa tu clase auxiliar)
                var inv = ActualizarInventario(codEmpresa, item, orden.cod_orden);
                if (inv.Code != 0) return inv;
            }

            return DbHelper.CreateOkResponse();
        }

        private static ErrorDto InsertarLineaCompra(IDbConnection conn, IDbTransaction tx, ComprasOrdenDatos orden, OrdenCompraDetalleData item, int linea)
        {
            const string sql = @"
INSERT cpr_Compras_detalle
(
    linea, cod_factura, cod_proveedor, cod_producto, cantidad, cod_bodega,
    precio, descuento, imp_ventas, imp_consumo
)
VALUES
(
    @Linea, @CodFactura, @CodProveedor, @CodProducto, @Cantidad, @CodBodega,
    @Precio, @Descuento, @ImpVentas, 0
);";

            conn.Execute(sql, new
            {
                Linea = linea,
                CodFactura = orden.cod_factura,
                CodProveedor = orden.cod_proveedor,
                CodProducto = item.cod_producto,
                Cantidad = item.cantidad,
                CodBodega = item.cod_bodega ?? "",
                Precio = item.precio,
                Descuento = item.descuento,
                ImpVentas = item.imp_ventas
            }, transaction: tx);

            return DbHelper.CreateOkResponse();
        }

        private static ErrorDto ActualizarDespachoOrden(IDbConnection conn, IDbTransaction tx, string codOrden, string codProducto)
        {
            const string sql = @"
UPDATE cpr_ordenes_detalle
SET cantidad_despachada = ISNULL(cantidad_despachada,0)
WHERE cod_producto = @CodProducto AND cod_orden = @CodOrden;";

            conn.Execute(sql, new { CodProducto = codProducto, CodOrden = codOrden }, transaction: tx);
            return DbHelper.CreateOkResponse();
        }

        private ErrorDto ActualizarInventario(int codEmpresa, OrdenCompraDetalleData item, string codOrden)
        {
            var compraInventario = new CompraInventarioDto
            {
                CodProducto = item.cod_producto,
                Cantidad = item.cantidad,
                CodBodega = item.cod_bodega,
                CodTipo = codOrden,
                Origen = "Compra",
                Fecha = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss"),
                Precio = Convert.ToDecimal(item.precio),
                ImpVentas = Convert.ToDecimal(item.imp_ventas),
                ImpConsumo = 0,
                TipoMov = "E"
            };

            // Usa el objeto ya creado en el ctor
            return _mProGrxAuxiliar.sbInvInventario(codEmpresa, compraInventario);
        }

        // ===========================
        //  VALIDACIÓN PRODUCTO/BODEGA (SIN INJECTION)
        // ===========================

        private static string ValidarProductosYBodegas(IDbConnection conn, List<OrdenCompraDetalleData> lineas, string mov)
        {
            if (lineas == null || lineas.Count == 0) return "No hay productos en la orden";

            foreach (var item in lineas)
            {
                if (item.cantidad <= 0) continue;

                var prodMsg = ValidarProductoActivo(conn, item.cod_producto);
                if (!string.IsNullOrEmpty(prodMsg)) return prodMsg;

                var bodMsg = ValidarBodega(conn, item.cod_bodega, mov);
                if (!string.IsNullOrEmpty(bodMsg)) return bodMsg;
            }

            return "";
        }

        private static string ValidarProductoActivo(IDbConnection conn, string codProducto)
        {
            const string sql = "SELECT estado FROM pv_productos WHERE cod_producto = @CodProducto;";
            var estado = conn.QueryFirstOrDefault<string>(sql, new { CodProducto = codProducto });

            if (estado == null) return $"El producto {codProducto} no existe";
            if (estado == "I") return $"El producto {codProducto} no esta activo";
            return "";
        }

        private static string ValidarBodega(IDbConnection conn, string? codBodega, string mov)
        {
            if (string.IsNullOrWhiteSpace(codBodega)) return "";

            const string sql = @"
SELECT permite_entradas, permite_salidas, estado
FROM pv_bodegas
WHERE cod_bodega = @CodBodega;";

            var bodega = conn.QueryFirstOrDefault<Models.BodegaDto>(sql, new { CodBodega = codBodega });
            if (bodega == null) return $"La bodega {codBodega} - No existe";
            if (bodega.estado == "I") return $"La bodega {codBodega} - Se encuentra Inactiva";

            var requiereEntrada = mov is "E" or "T";
            var requiereSalida = mov is "S" or "R" or "T";

            if (requiereEntrada && bodega.permite_entradas != "1") return $"La bodega {codBodega} - No permite Entradas";
            if (requiereSalida && bodega.permite_salidas != "1") return $"La bodega {codBodega} - No permite Salidas";

            return "";
        }

        // ===========================
        //  PIN / CONSECUTIVO (EXTERNOS)
        // ===========================

        public ErrorDto OrdenPin_Obtener(int codEmpresa, string codOrden)
        {
            var r = DbHelper.WithConn<string>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);
                const string sql = "SELECT pin_autorizacion FROM cpr_ordenes WHERE cod_orden = @CodOrden;";
                return conn.QueryFirstOrDefault<string>(sql, new { CodOrden = codOrden }) ?? "0";
            });

            if (r.Code != 0) return DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, r.Code ?? -1);

            var resp = new ErrorDto { Code = 1, Description = r.Result };
            if (resp.Description != "0") resp.Code = 2;
            return resp;
        }

        public ErrorDto OrdenPin_Verifica(int codEmpresa, string codOrden, string ordPin)
        {
            var r = DbHelper.WithConn<int>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);
                const string sql = @"
SELECT ISNULL(COUNT(*),0)
FROM cpr_ordenes
WHERE cod_orden = @CodOrden AND pin_entrada = @Pin;";
                return conn.Query<int>(sql, new { CodOrden = codOrden, Pin = ordPin }).FirstOrDefault();
            });

            if (r.Code != 0) return DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, r.Code ?? -1);
            return new ErrorDto { Code = 1, Description = r.Result.ToString() };
        }

        public ErrorDto OrdenConsecutivo_Obtener(int codEmpresa)
        {
            var r = DbHelper.WithConn<string>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);
                const string sql = "SELECT ISNULL(MAX(cod_orden),0) + 1 FROM cpr_compras;";
                var consecutivo = conn.Query<string>(sql).FirstOrDefault() ?? "0";
                return consecutivo.PadLeft(10, '0');
            });

            if (r.Code != 0) return DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, r.Code ?? -1);
            return new ErrorDto { Code = 1, Description = r.Result ?? "0000000000" };
        }

        // ===========================
        //  ACTIVOS (ARREGLA S107 CON CONTEXTO)
        // ===========================

        private sealed record ArticulosActivosContext(
            int CodEmpresa,
            string Consecutivo,
            string CodFactura,
            string Usuario,
            string CodOrden,
            int CodProveedor);

        private static ErrorDto BuscoArticulosActivos(
            IDbConnection conn,
            IDbTransaction tx,
            ArticulosActivosContext ctx,
            OrdenCompraDetalleData linea)
        {
            var response = new ErrorDto { Code = 0 };

            const string qTipoProd = "SELECT TIPO_PRODUCTO FROM pv_productos WHERE cod_producto = @CodProducto;";
            var tipo = conn.QueryFirstOrDefault<string>(qTipoProd, new { CodProducto = linea.cod_producto }, transaction: tx);

            if (!string.Equals(tipo, "A", StringComparison.OrdinalIgnoreCase))
                return response;

            const string qUen = @"
SELECT COD_UNIDAD
FROM CPR_ORDENES_UENS
WHERE cod_orden = @CodOrden AND COD_PRODUCTO = @CodProducto;";

            var codUen = conn.QueryFirstOrDefault<string>(qUen, new { CodOrden = ctx.CodOrden, CodProducto = linea.cod_producto }, transaction: tx);

            for (int i = 0; i < linea.cantidad; i++)
            {
                const string sp = "[spCPR_CONTROL_ACTIVOS_GUARDAR]";
                var values = new
                {
                    COD_PRODUCTO = linea.cod_producto,
                    COD_UEN = codUen,
                    COD_PROVEEDOR = ctx.CodProveedor,
                    COD_COMPRA = ctx.Consecutivo,
                    COSTO_TOTAL = linea.cantidad * linea.precio,
                    COSTO_UNITARIO = linea.precio,
                    FACTURA = ctx.CodFactura,
                    FECHA_COMPRA = DateTime.Now,
                    COD_BODEGA = linea.cod_bodega,
                    ESTADO = 'P',
                    NUMERO_PLACA = "",
                    COD_LOCALIZACION = "",
                    MARCA = "",
                    MODELO = "",
                    SERIE = "",
                    OBSERVACIONES = "",
                    ID_RESPONSABLE = "",
                    COD_REQUISICION = "",
                    ENTREGA_USUARIO = "",
                    ENTREGA_FECHA = "",
                    ACTIVO_USUARIO = "",
                    ACTIVO_FECHA = "",
                    REGISTRO_USUARIO = ctx.Usuario
                };

                response.Code = conn.Query<int>(sp, values, commandType: CommandType.StoredProcedure, transaction: tx).FirstOrDefault();
                if (response.Code != 0 && response.Code != 1) // por si SP devuelve códigos distintos
                    return response;
            }

            response.Description = "Ok";
            return response;
        }

        // ===========================
        //  TOTALES FACTURA VS ORDEN (SIN INJECTION)
        // ===========================

        public ErrorDto fxVerificaTotalesFac(int codEmpresa, ComprasOrdenDatos orden)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);
                return fxVerificaTotalesFac_Conn(conn, orden);
            }).Result ?? DbHelper.ErrorResponse(ErrorLiteral, -1);
        }

        private static ErrorDto fxVerificaTotalesFac_Conn(IDbConnection conn, ComprasOrdenDatos orden)
        {
            var result = new ErrorDto { Code = 0 };

            const string qTotalOrd = "SELECT TOTAL FROM CPR_ORDENES WHERE COD_ORDEN = @CodOrden;";
            var totalOrd = conn.Query<float>(qTotalOrd, new { CodOrden = orden.cod_orden }).FirstOrDefault();

            const string qTotalFac = "SELECT ISNULL(SUM(TOTAL), 0) FROM CPR_COMPRAS WHERE COD_ORDEN = @CodOrden;";
            var totalFac = conn.Query<float>(qTotalFac, new { CodOrden = orden.cod_orden }).FirstOrDefault();

            var totalNueva = CalcularTotalLineas(orden.lineas);

            if (totalNueva + totalFac > totalOrd)
            {
                result.Code = -1;
                result.Description = "El total de la factura no puede ser mayor al total de la orden";
            }

            return result;
        }

        private static float CalcularTotalLineas(List<OrdenCompraDetalleData> lineas)
        {
            float total = 0;

            foreach (var item in lineas)
            {
                var tmpDesc = (item.cantidad * item.precio) * (item.descuento / 100);
                var tmpIv = ((item.cantidad * item.precio) - tmpDesc) * (item.imp_ventas / 100);
                total += (item.cantidad * item.precio) - tmpDesc + tmpIv;
            }

            return total;
        }

        // ===========================
        //  FACTURAS XML (SIN INJECTION)
        // ===========================

        public ErrorDto<List<FacturasAutorizarDto>> FacturasAutorizar_Obtener(int codEmpresa, string usuario, int proveedor)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                const string sql = @"
SELECT
    f.ID,
    f.COD_DOCUMENTO,
    f.NOMBRE_PROV,
    f.MONTO_TOTAL,
    CASE f.ESTADO
        WHEN 'R' THEN 'Rechazada'
        WHEN 'P' THEN 'Pendiente'
        WHEN 'A' THEN 'Autorizada'
        WHEN 'E' THEN 'Emitida'
        ELSE 'DESCONOCIDO'
    END AS ESTADO_DESCRIPCION
FROM CPR_FACTURAS_XML f
JOIN CXP_PROVEEDORES p ON f.CED_JUR_PROV = REPLACE(p.CEDJUR, '-', '')
WHERE f.ESTADO IN ('P', 'E')
  AND p.cod_proveedor = @Proveedor;";

                return conn.Query<FacturasAutorizarDto>(sql, new { Proveedor = proveedor }).ToList();
            });
        }

        public ErrorDto Factura_AutorizarRechazar(int codEmpresa, string usuario, string cod, string cod_factura, string justificacion)
        {
            var r = DbHelper.WithConn<int>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                const string sql = @"
UPDATE CPR_FACTURAS_XML
SET estado = @Estado, JUSTIFICACION = @Justificacion
WHERE COD_DOCUMENTO = @CodDocumento;";

                return conn.Execute(sql, new { Estado = cod, Justificacion = justificacion ?? "", CodDocumento = cod_factura });
            });

            if (r.Code != 0) return DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, r.Code ?? -1);

            var info = DbHelper.CreateOkResponse();
            info.Code = r.Result;

            if (string.Equals(cod, "A", StringComparison.OrdinalIgnoreCase))
                _ = CorreoNotificaAutorizaFactura_Enviar(codEmpresa, cod_factura, usuario);

            return info;
        }

        public ErrorDto ValidaAutorizacion(int codEmpresa, string usuario, string cod_orden)
        {
            var r = DbHelper.WithConn<int?>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                const string sql = @"
SELECT 1
WHERE EXISTS (
    SELECT 1
    FROM CPR_SOLICITUD s
    JOIN CPR_ORDENES o ON s.adjudica_orden = o.cod_orden
    JOIN CORE_UENS_USUARIOS_ROLES r ON r.COD_UNIDAD = s.COD_UNIDAD
    WHERE o.COD_ORDEN = @cod_orden
      AND r.CORE_USUARIO = @usuario
      AND (r.ROL_AUTORIZA = 1)
);";

                return conn.ExecuteScalar<int?>(sql, new { cod_orden, usuario });
            });

            if (r.Code != 0) return DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, r.Code ?? -1);

            return new ErrorDto { Code = (r.Result.HasValue ? 1 : 0) };
        }

        // ===========================
        //  BITACORA CORREO (SIN INJECTION)
        // ===========================

        public ErrorDto BitacoraEnvioCorreo(BitacoraComprasInsertarDto req)
        {
            var r = DbHelper.WithConn<int>(_portalDb, req.EmpresaId, conn =>
            {
                EnsureOpen(conn);

                const string sql = @"
INSERT INTO [dbo].[BITACORA_COMPRAS]
(
    [ID_COMPRA],
    [CONSEC],
    [MOVIMIENTO],
    [DETALLE],
    [REGISTRO_FECHA],
    [REGISTRO_USUARIO]
)
VALUES
(
    @IdCompra,
    @Consec,
    @Movimiento,
    @Detalle,
    GETDATE(),
    @Usuario
);";

                return conn.Execute(sql, new
                {
                    IdCompra = req.id_bitacora,
                    Consec = req.consec,
                    Movimiento = req.movimiento ?? "",
                    Detalle = req.detalle ?? "",
                    Usuario = req.registro_usuario ?? ""
                });
            });

            if (r.Code != 0) return DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, r.Code ?? -1);

            return new ErrorDto { Code = r.Result, Description = "Ok" };
        }

        private static string BuildFacturaEmailBody(string proveedor, string codFactura, string accion)
        {
            return @$"<html lang=""es""><body>
<p>Estimado/a {proveedor} la factura número #{codFactura} se ha {accion}.</p>
</body></html>";
        }

        private ErrorDto<(string Proveedor, string Email)> ObtenerProveedorEmailPorFacturaXml(int codEmpresa, string codFactura)
        {
            return DbHelper.WithConn<(string Proveedor, string Email)>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                const string sql = @"
SELECT TOP 1
    ISNULL(f.NOMBRE_PROV, '') AS Proveedor,
    ISNULL(p.EMAIL, '') AS Email
FROM CPR_FACTURAS_XML f
LEFT JOIN CXP_PROVEEDORES p
    ON REPLACE(p.CEDJUR, '-', '') = REPLACE(f.CED_JUR_PROV, '-', '')
WHERE f.cod_documento = @Doc;";

                return conn.QueryFirstOrDefault<(string Proveedor, string Email)>(sql, new { Doc = codFactura });
            });
        }

        private ErrorDto<(string Proveedor, string Email)> ObtenerProveedorEmailPorCodProveedor(int codEmpresa, int codProveedor)
        {
            return DbHelper.WithConn<(string Proveedor, string Email)>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                const string sql = @"
SELECT TOP 1
    ISNULL(descripcion, '') AS Proveedor,
    ISNULL(EMAIL, '') AS Email
FROM CXP_PROVEEDORES
WHERE cod_proveedor = @Prov;";

                return conn.QueryFirstOrDefault<(string Proveedor, string Email)>(sql, new { Prov = codProveedor });
            });
        }

        private async Task TrySendEmailAndLog(int codEmpresa, string to, string subject, string body, string registroUsuario, string bitacoraDetalle)
        {
            if (!string.Equals(sendEmail, "Y", StringComparison.OrdinalIgnoreCase))
                return;

            if (string.IsNullOrWhiteSpace(to))
                return;

            var eConfigResult = _envioCorreoDB.CorreoConfig(codEmpresa, Notificaciones);
            if (eConfigResult == null || eConfigResult.Code != 0 || eConfigResult.Result == null)
                return;

            var eConfig = eConfigResult.Result;

            var emailRequest = new EmailRequest
            {
                To = to,
                From = eConfig.User,
                Subject = subject,
                Body = body,
                Attachments = new List<IFormFile>()
            };

            var resp = new ErrorDto();
            await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, resp);

            BitacoraEnvioCorreo(new BitacoraComprasInsertarDto
            {
                EmpresaId = codEmpresa,
                consec = 0,
                movimiento = BitacoraMovimientoRegistra,
                detalle = bitacoraDetalle,
                registro_usuario = registroUsuario
            });
        }

        // ===========================
        //  CORREOS
        // ===========================

        private async Task CorreoNotificaAutorizaFactura_Enviar(int codEmpresa, string cod_factura, string usuario)
        {
            try
            {
                var datos = ObtenerProveedorEmailPorFacturaXml(codEmpresa, cod_factura);
                if (datos.Code != 0)
                    return;

                var proveedor = datos.Result.Proveedor;
                var emailProveedor = datos.Result.Email;

                var body = BuildFacturaEmailBody(proveedor, cod_factura, "aprobado");

                await TrySendEmailAndLog(
                    codEmpresa,
                    emailProveedor,
                    "Aprobación de factura",
                    body,
                    usuario,
                    $@"Envío de correo de aprobacion de factura #{cod_factura}"
                );
            }
            catch
            {
                // si falla correo, no tumbamos el proceso principal
            }
        }

        private async Task CorreoNotificaRegistraFactura_Enviar(int codEmpresa, string cod_factura, string usuario, int cod_proveedor)
        {
            try
            {
                var datos = ObtenerProveedorEmailPorCodProveedor(codEmpresa, cod_proveedor);
                if (datos.Code != 0)
                    return;

                var proveedor = datos.Result.Proveedor;
                var emailProveedor = datos.Result.Email;

                var body = BuildFacturaEmailBody(proveedor, cod_factura, "registrado");

                await TrySendEmailAndLog(
                    codEmpresa,
                    emailProveedor,
                    "Registro de factura",
                    body,
                    usuario,
                    $@"Envío de correo de registro de factura #{cod_factura}"
                );
            }
            catch
            {
                // si falla correo, no tumbamos el proceso principal
            }
        }
    }
}