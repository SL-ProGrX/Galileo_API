using Dapper;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using System.Data;
using System.Web;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class FrmCprOrdenesDB
    {
        private const string ErrorLiteral = "Error";
        private readonly IConfiguration _config;

        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _DBBitacora;
        private readonly EnvioCorreoDB _envioCorreoDB;
        private readonly string _sendEmail;
        private readonly string _notificaciones;

        public FrmCprOrdenesDB(IConfiguration config)
        {
            _config = config;
            _portalDb = new PortalDB(_config);
            _DBBitacora = new MSecurityMainDb(_config);
            _envioCorreoDB = new EnvioCorreoDB(_config);

            _sendEmail = _config.GetSection("AppSettings").GetSection("EnviaEmail").Value?.ToString() ?? "N";
            _notificaciones = _config.GetSection("AppSettings").GetSection("Notificaciones").Value?.ToString() ?? "";
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data) => _DBBitacora.Bitacora(data);

        /// <summary>
        /// Se obtiene la orden seleccionada (con validación de rangos por UEN)
        /// </summary>
        public ErrorDto<OrdenDto> OrdenesSeleccionada(int CodEmpresa, string CodOrden, string usuario)
        {
            const string sql = @"
                SELECT
                    O.*,
                    RTRIM(C.Tipo_Orden)              AS Causa_Id,
                    RTRIM(C.descripcion)             AS Causa_Desc,
                    ISNULL(Prov.Descripcion,'')      AS Proveedor_Desc,
                    Prov.CEDJUR                      AS cedula_proveedor,
                    Prov.TELEFONO                    AS telefono_proveedor,
                    Prov.DIRECCION                   AS direccion_proveedor,
                    RIGHT(REPLICATE('0', 10) + CAST(sp.CPR_ID AS VARCHAR), 10) AS cod_solicitud,
                    s.cod_unidad,
                    s.DIVISA
                FROM cpr_ordenes O
                INNER JOIN cpr_Tipo_Orden C ON O.Tipo_Orden = C.Tipo_Orden
                LEFT JOIN CXP_Proveedores Prov ON O.cod_Proveedor = Prov.Cod_Proveedor
                LEFT JOIN CPR_SOLICITUD_PROV sp ON sp.ADJUDICA_ORDEN  = O.COD_ORDEN AND sp.PROVEEDOR_CODIGO = O.COD_PROVEEDOR 
                LEFT JOIN CPR_SOLICITUD s ON s.CPR_ID = sp.CPR_ID
                WHERE O.cod_orden = @CodOrden;";

            var ordenR = DbHelper.ExecuteSingleQuery(_portalDb, CodEmpresa, sql, new OrdenDto(), new { CodOrden });
            if (ordenR.Code != 0)
                return new ErrorDto<OrdenDto>
                {
                    Code = ordenR.Code,
                    Description = ordenR.Description,
                    Result = ordenR.Result ?? new OrdenDto()
                };
            if (ordenR.Result == null)
                return DbHelper.CreateErrorResponse("No se encontró la orden", -1, new OrdenDto());

            var validaR = DbHelper.WithConn<ErrorDto>(_portalDb, CodEmpresa, conn =>
            {
                if (conn.State != ConnectionState.Open) conn.Open();
                return ValidarRangoVisualizacion(conn, ordenR.Result, usuario);
            });

            // WithConn<T> devuelve ErrorDto<T>. Aquí T es ErrorDto.
            if (validaR.Code != 0 || validaR.Result == null)
                return DbHelper.CreateErrorResponse(validaR.Description ?? ErrorLiteral, validaR.Code ?? -1, ordenR.Result);

            if (validaR.Result.Code != 0)
                return DbHelper.CreateErrorResponse(validaR.Result.Description ?? "No autorizado", validaR.Result.Code ?? -1, ordenR.Result);

            return new ErrorDto<OrdenDto>
            {
                Code = ordenR.Code,
                Description = ordenR.Description,
                Result = ordenR.Result
            };
        }

        private ErrorDto ValidarRangoVisualizacion(IDbConnection conn, OrdenDto orden, string usuario)
        {
            if (string.IsNullOrWhiteSpace(orden.cod_unidad))
                return DbHelper.CreateOkResponse();

            var montoUsdR = CalcularMontoUsd(conn, orden);
            if (montoUsdR.Code != 0) return DbHelper.ErrorResponse(montoUsdR.Description ?? ErrorLiteral, montoUsdR.Code ?? -1);

            var rangosR = ObtenerRangosUsuario(conn, usuario, orden.cod_unidad);
            if (rangosR.Code != 0) return DbHelper.ErrorResponse(rangosR.Description ?? ErrorLiteral, rangosR.Code ?? -1);

            var rangos = rangosR.Result ?? new List<(decimal MONTO_MINIMO, decimal MONTO_MAXIMO)>();
            bool dentro = rangos.Any(r => montoUsdR.Result >= r.MONTO_MINIMO && montoUsdR.Result <= r.MONTO_MAXIMO);

            if (!dentro)
                return DbHelper.ErrorResponse($"No tiene permisos para visualizar la orden #{orden.cod_orden}", -2);

            return DbHelper.CreateOkResponse();
        }

        private static ErrorDto<decimal> CalcularMontoUsd(IDbConnection conn, OrdenDto orden)
        {
            try
            {
                decimal total = Convert.ToDecimal(orden.total);

                if (string.Equals(orden.divisa, "USD", StringComparison.OrdinalIgnoreCase))
                    return DbHelper.CreateOkResponse(total);

                var tc = conn.QueryFirstOrDefault<decimal>(
                    @"SELECT VALOR FROM SIF_PARAMETROS WHERE COD_PARAMETRO = 'TC';"
                );

                if (tc <= 0)
                    return DbHelper.CreateErrorResponse("Tipo de cambio inválido", -1, 0m);

                return DbHelper.CreateOkResponse(total / tc);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, 0m);
            }
        }

        private ErrorDto<List<(decimal MONTO_MINIMO, decimal MONTO_MAXIMO)>> ObtenerRangosUsuario(IDbConnection conn, string usuario, string uen)
        {
            const string sql = @"
                SELECT r.MONTO_MINIMO, r.MONTO_MAXIMO
                FROM cpr_orden_rangos r
                INNER JOIN CPR_RANGO_USUARIO u ON r.cod_rango = u.cod_rango
                WHERE u.USUARIO = @Usuario AND u.UEN = @UEN AND u.ACTIVO = 1;";

            try
            {
                var list = conn.Query<(decimal MONTO_MINIMO, decimal MONTO_MAXIMO)>(
                    sql,
                    new { Usuario = usuario, UEN = uen }
                ).ToList();

                return DbHelper.CreateOkResponse(list);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, new List<(decimal, decimal)>());
            }
        }

        /// <summary>
        /// Obtiene las lineas de la orden
        /// </summary>
        public ErrorDto<OrdenLineasData> OrdenLineasObtener(int CodEmpresa, string jfiltros)
        {
            var filtros = JsonConvert.DeserializeObject<OrderLineaTablaFiltros>(jfiltros) ?? new OrderLineaTablaFiltros();
            if (string.IsNullOrWhiteSpace(filtros.CodOrden))
                return DbHelper.CreateErrorResponse("Debe indicar CodOrden", -1, new OrdenLineasData());

            var totalR = DbHelper.ExecuteSingleQuery(_portalDb, CodEmpresa,
                @"SELECT COUNT(D.cod_producto)
                  FROM cpr_ordenes_detalle D
                  INNER JOIN pv_productos P ON D.cod_producto = P.cod_producto
                  WHERE D.cod_orden = @CodOrden;",
                0,
                new { CodOrden = filtros.CodOrden });

            if (totalR.Code != 0)
                return DbHelper.CreateErrorResponse(totalR.Description ?? ErrorLiteral, totalR.Code ?? -1, new OrdenLineasData());

            long cantidad = 0;
            if (totalR.Result > 0)
            {
                var cantR = DbHelper.ExecuteSingleQuery(_portalDb, CodEmpresa,
                    @"SELECT ISNULL(SUM(D.cantidad),0)
                      FROM cpr_ordenes_detalle D
                      INNER JOIN pv_productos P ON D.cod_producto = P.cod_producto
                      WHERE D.cod_orden = @CodOrden;",
                    0L,
                    new { CodOrden = filtros.CodOrden });

                if (cantR.Code != 0)
                    return DbHelper.CreateErrorResponse(cantR.Description ?? ErrorLiteral, cantR.Code ?? -1, new OrdenLineasData());

                cantidad = cantR.Result;
            }

            const string qLineas = @"
                SELECT
                    D.cod_producto,
                    P.descripcion,
                    D.cantidad,
                    D.precio,
                    ISNULL(D.descuento,0) AS Descuento,
                    D.imp_ventas,
                    0 AS Total,
                    CASE WHEN (
                        SELECT U.COD_PRODUCTO
                        FROM CPR_ORDENES_UENS U
                        WHERE U.COD_PRODUCTO = D.cod_producto
                          AND U.COD_ORDEN = D.cod_orden
                        GROUP BY U.COD_ORDEN, U.COD_PRODUCTO
                    ) IS NOT NULL THEN 1 ELSE 0 END AS i_existe,
                    CASE WHEN (
                        SELECT COALESCE((
                            SELECT SUM(U.CANTIDAD)
                            FROM CPR_ORDENES_UENS U
                            WHERE U.COD_PRODUCTO = D.cod_producto
                              AND U.COD_ORDEN = D.cod_orden
                            GROUP BY U.COD_ORDEN, U.COD_PRODUCTO
                        ), 0)
                    ) < D.cantidad THEN 0 ELSE 1 END AS i_completo
                FROM cpr_ordenes_detalle D
                INNER JOIN pv_productos P ON D.cod_producto = P.cod_producto
                WHERE D.cod_orden = @CodOrden
                ORDER BY D.cod_producto;";

            var lineasR = DbHelper.ExecuteListQuery<OrdenLineas>(_portalDb, CodEmpresa, qLineas, new { CodOrden = filtros.CodOrden });
            if (lineasR.Code != 0)
                return DbHelper.CreateErrorResponse(lineasR.Description ?? ErrorLiteral, lineasR.Code ?? -1, new OrdenLineasData());

            var data = new OrdenLineasData
            {
                total = totalR.Result,
                cantidad = cantidad,
                lineas = lineasR.Result ?? new List<OrdenLineas>()
            };

            return DbHelper.CreateOkResponse(data);
        }

        /// <summary>
        /// Orden Scroll
        /// </summary>
        public ErrorDto<OrdenesData> Orden_scroll(int CodEmpresa, int scrollValue, string? cod_Orden)
        {
            var sql = scrollValue == 1
                ? @"SELECT TOP 1 cod_orden FROM cpr_ordenes WHERE cod_orden > @CodOrden ORDER BY cod_orden ASC;"
                : @"SELECT TOP 1 cod_orden FROM cpr_ordenes WHERE cod_orden < @CodOrden ORDER BY cod_orden DESC;";

            var r = DbHelper.ExecuteSingleQuery(_portalDb, CodEmpresa, sql, "", new { CodOrden = cod_Orden ?? "" });

            var resp = new ErrorDto<OrdenesData>();
            if (r.Code != 0)
            {
                resp.Code = r.Code;
                resp.Description = r.Description;
                return resp;
            }

            if (r.Result == null)
            {
                resp.Code = -2;
                resp.Description = "No se encontraron mas ordenes";
                return resp;
            }

            resp.Code = 0;
            resp.Description = r.Result;
            return resp;
        }

        public ErrorDto Orden_Insertar(int CodEmpresa, object jOrdenes)
        {
            try
            {
                var jsonString = jOrdenes?.ToString() ?? "{}";
                var ordenes = JsonConvert.DeserializeObject<OrdenDatosAcciones>(jsonString) ?? new OrdenDatosAcciones { edita = false };
                return OrdenesGuardar(CodEmpresa, ordenes);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, 1);
            }
        }

        public ErrorDto Orden_Actualiza(int CodEmpresa, OrdenDatosAcciones jOrdenes)
            => OrdenesGuardar(CodEmpresa, jOrdenes);


        private ErrorDto OrdenesGuardar(int codEmpresa, OrdenDatosAcciones ordenes)
        {
            var msg = ValidarOrdenBasica(ordenes);
            if (!string.IsNullOrEmpty(msg))
                return DbHelper.ErrorResponse(msg, 1);

            var prod = ValidarProductosActivosDb(codEmpresa, ordenes.lineas);
            var prodMsg = UnwrapOrError(prod, out var prodValidationError);
            if (prodValidationError != null) return prodValidationError;
            if (!string.IsNullOrWhiteSpace(prodMsg))
                return DbHelper.ErrorResponse(prodMsg, 1);

            var totales = CalcularTotales(ordenes.lineas);

            var persist = GuardarOrdenEnTransaccionDb(codEmpresa, ordenes, totales);
            return UnwrapResult(persist);
        }

        private ErrorDto<string> ValidarProductosActivosDb(int codEmpresa, List<OrdenLineas> lineas)
        {
            return DbHelper.WithConn<string>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);
                return ValidarProductosActivos(conn, lineas); // retorna "" si ok, o mensaje si falla
            });
        }

        private static void EnsureOpen(IDbConnection conn)
        {
            if (conn.State != ConnectionState.Open) conn.Open();
        }

        private static string UnwrapOrError(ErrorDto<string> r, out ErrorDto? error)
        {
            error = null;

            if (r.Code != 0)
            {
                error = DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, r.Code ?? -1);
                return "";
            }

            return r.Result ?? "";
        }

        private static OrdenTotales CalcularTotales(List<OrdenLineas> lineas)
        {
            CalcularTotalesOrden(lineas, out var sub, out var desc, out var iv, out _, out var total);
            return new OrdenTotales(sub, desc, iv, total);
        }

        private ErrorDto<ErrorDto> GuardarOrdenEnTransaccionDb(int codEmpresa, OrdenDatosAcciones ordenes, OrdenTotales totales)
        {
            return DbHelper.WithConn<ErrorDto>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);
                using var tx = conn.BeginTransaction();

                try
                {
                    var ctx = new OrdenPersistContext
                    {
                        CodEmpresa = codEmpresa,
                        Orden = ordenes,
                        Totales = totales
                    };

                    var head = GuardarEncabezado(conn, tx, ctx);
                    if (head.Code != 0) return Rollback(tx, head);

                    var det = GuardarDetalle(conn, tx, ordenes);
                    if (det.Code != 0) return Rollback(tx, det);

                    tx.Commit();
                    return DbHelper.OkResponse(ordenes.cod_orden ?? "");
                }
                catch (Exception ex)
                {
                    return Rollback(tx, DbHelper.ErrorResponse(ex.Message, -1));
                }
            });
        }

        private static ErrorDto Rollback(IDbTransaction tx, ErrorDto dto)
        {
            tx.Rollback();
            return dto;
        }

        private ErrorDto GuardarEncabezado(IDbConnection conn, IDbTransaction tx, OrdenPersistContext ctx)
        {
            return ctx.Orden.edita
                ? ActualizarOrden(conn, tx, ctx)
                : InsertarOrden(conn, tx, ctx);
        }

        private static ErrorDto UnwrapResult(ErrorDto<ErrorDto> r)
        {
            if (r.Code != 0 || r.Result == null)
                return DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, r.Code ?? -1);

            return r.Result;
        }
        
        private static string? ValidarOrdenBasica(OrdenDatosAcciones ordenes)
        {
            if (ordenes == null) return "Orden inválida";
            if (string.IsNullOrWhiteSpace(ordenes.usuario)) return "Debe indicar usuario";
            if (string.IsNullOrWhiteSpace(ordenes.tipo_orden)) return "Debe indicar tipo de orden";
            if (ordenes.lineas == null || ordenes.lineas.Count == 0) return "No hay productos en la orden";
            return null;
        }

        private static string ValidarProductosActivos(IDbConnection conn, List<OrdenLineas> lineas)
        {
            foreach (var item in lineas)
            {
                if (item.cantidad <= 0) continue;

                var estado = conn.QueryFirstOrDefault<string>(
                    @"SELECT estado FROM pv_productos WHERE cod_producto = @CodProducto;",
                    new { CodProducto = item.cod_producto }
                );

                if (estado == null) return $"El producto {item.cod_producto} no existe";
                if (estado == "I") return $"El producto {item.cod_producto} no esta activo";
            }

            return "";
        }

        private readonly record struct OrdenTotales(
            float SubTotal,
            float Descuento,
            float ImpVentas,
            float Total
        );

        private sealed class OrdenPersistContext
        {
            public required int CodEmpresa { get; init; }
            public required OrdenDatosAcciones Orden { get; init; }
            public required OrdenTotales Totales { get; init; }
        }

        private ErrorDto ActualizarOrden(IDbConnection conn, IDbTransaction tx, OrdenPersistContext ctx)
        {
            var o = ctx.Orden;
            var t = ctx.Totales;

            if (!string.Equals(o.estado, "S", StringComparison.OrdinalIgnoreCase))
                return DbHelper.ErrorResponse("No puede Modificar esta Orden, ya que no se encuentra Solicitada...", 1);

            conn.Execute(@"
                UPDATE cpr_ordenes SET
                    nota = @Nota,
                    descuento = @Descuento,
                    subtotal = @SubTotal,
                    imp_ventas = @ImpVentas,
                    total = @Total,
                    plazo_entrega = @PlazoEntrega,
                    horario_recepcion = @HorarioRecepcion,
                    plazo_pago = @PlazoPago,
                    direccion_entrega = @DireccionEntrega,
                    garantia = @Garantia,
                    terminos_condiciones = @Terminos,
                    multa = @Multa
                WHERE cod_orden = @CodOrden
                AND tipo_orden = @TipoOrden;",
                new
                {
                    Nota = o.nota ?? "",
                    Descuento = t.Descuento,
                    SubTotal = t.SubTotal,
                    ImpVentas = t.ImpVentas,
                    Total = t.Total,
                    PlazoEntrega = o.plazo_entrega ?? "",
                    HorarioRecepcion = o.horario_recepcion ?? "",
                    PlazoPago = o.plazo_pago ?? "",
                    DireccionEntrega = o.direccion_entrega ?? "",
                    Garantia = o.garantia ?? "",
                    Terminos = o.terminos_condiciones ?? "",
                    Multa = o.multa ?? "",
                    CodOrden = o.cod_orden ?? "",
                    TipoOrden = o.tipo_orden ?? ""
                },
                transaction: tx
            );

            _ = Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = ctx.CodEmpresa,
                Usuario = o.usuario ?? "",
                DetalleMovimiento = "Modifica, Orden Compra:" + o.cod_orden,
                Movimiento = "Modifica - WEB",
                Modulo = 35
            });

            return DbHelper.CreateOkResponse();
        }

        private ErrorDto InsertarOrden(IDbConnection conn, IDbTransaction tx, OrdenPersistContext ctx)
        {
            var o = ctx.Orden;
            var t = ctx.Totales;

            var consecutivo = GenerarConsecutivoOrden(conn, tx);
            o.cod_orden = consecutivo;

            conn.Execute(@"
                INSERT INTO cpr_ordenes
                (
                    cod_orden, tipo_orden, estado, genera_fecha, nota, genera_user,
                    subtotal, descuento, imp_ventas, total,
                    pin_autorizacion, pin_entrada, proceso,
                    plazo_entrega, garantia, plazo_pago, direccion_entrega,
                    horario_recepcion, terminos_condiciones, multa
                )
                VALUES
                (
                    @CodOrden, @TipoOrden, 'S', GETDATE(), @Nota, @Usuario,
                    @SubTotal, @Descuento, @ImpVentas, @Total,
                    0, '', 'P',
                    @PlazoEntrega, @Garantia, @PlazoPago, @DireccionEntrega,
                    @HorarioRecepcion, @Terminos, @Multa
                );",
                new
                {
                    CodOrden = consecutivo,
                    TipoOrden = o.tipo_orden ?? "",
                    Nota = o.nota ?? "",
                    Usuario = o.usuario ?? "",
                    SubTotal = t.SubTotal,
                    Descuento = t.Descuento,
                    ImpVentas = t.ImpVentas,
                    Total = t.Total,
                    PlazoEntrega = o.plazo_entrega ?? "",
                    Garantia = o.garantia ?? "",
                    PlazoPago = o.plazo_pago ?? "",
                    DireccionEntrega = o.direccion_entrega ?? "",
                    HorarioRecepcion = o.horario_recepcion ?? "",
                    Terminos = o.terminos_condiciones ?? "",
                    Multa = o.multa ?? ""
                },
                transaction: tx
            );

            _ = Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = ctx.CodEmpresa,
                Usuario = o.usuario ?? "",
                DetalleMovimiento = "Registra, Orden Compra:" + consecutivo,
                Movimiento = "Registra - WEB",
                Modulo = 35
            });

            return DbHelper.CreateOkResponse();
        }

        private static ErrorDto GuardarDetalle(IDbConnection conn, IDbTransaction tx, OrdenDatosAcciones ordenes)
        {
            conn.Execute(
                @"DELETE cpr_ordenes_detalle WHERE cod_orden = @CodOrden;",
                new { CodOrden = ordenes.cod_orden ?? "" },
                transaction: tx
            );

            int linea = 0;
            foreach (var item in ordenes.lineas)
            {
                linea++;
                conn.Execute(@"
                    INSERT INTO cpr_ordenes_detalle
                    (
                        linea, cod_orden, cod_producto, cantidad, estado,
                        precio, descuento, imp_ventas
                    )
                    VALUES
                    (
                        @Linea, @CodOrden, @CodProducto, @Cantidad, 'P',
                        @Precio, @Descuento, @ImpVentas
                    );",
                    new
                    {
                        Linea = linea,
                        CodOrden = ordenes.cod_orden ?? "",
                        CodProducto = item.cod_producto,
                        Cantidad = item.cantidad,
                        Precio = item.precio,
                        Descuento = item.descuento,
                        ImpVentas = item.imp_ventas
                    },
                    transaction: tx
                );
            }

            return DbHelper.CreateOkResponse();
        }

        private static string GenerarConsecutivoOrden(IDbConnection conn, IDbTransaction tx)
        {
            var next = conn.QueryFirstOrDefault<int>(
                @"SELECT ISNULL(MAX(CAST(cod_orden AS INT)), 0) + 1 FROM cpr_ordenes;",
                transaction: tx
            );

            return next.ToString().PadLeft(10, '0');
        }

        private static void CalcularTotalesOrden(
            List<OrdenLineas> lineas,
            out float curSubTotal,
            out float curDescuento,
            out float curIV,
            out float curCantidad,
            out float curTotal)
        {
            curSubTotal = 0;
            curDescuento = 0;
            curIV = 0;
            curCantidad = 0;

            foreach (var item in lineas)
            {
                curSubTotal += item.cantidad * item.precio;

                float tmpDesc = (item.cantidad * item.precio) * (item.descuento / 100);
                curDescuento += tmpDesc;

                float tmpIV = ((item.cantidad * item.precio) - tmpDesc) * (item.imp_ventas / 100);
                curIV += tmpIV;

                item.total = (item.cantidad * item.precio) - tmpDesc + tmpIV;
                curCantidad += item.cantidad;
            }

            curTotal = curSubTotal + curIV - curDescuento;
        }

        public ErrorDto<List<OrdenesUensData>> OrdenesUENs_Obtener(int CodEmpresa, string CodOrden, string CodProducto)
        {
            var existeR = DbHelper.ExecuteSingleQuery(_portalDb, CodEmpresa,
                @"SELECT COUNT(*) FROM CPR_ORDENES_UENS WHERE COD_ORDEN = @CodOrden AND COD_PRODUCTO = @CodProducto;",
                0,
                new { CodOrden, CodProducto });

            if (existeR.Code != 0)
                return DbHelper.CreateErrorResponse(existeR.Description ?? ErrorLiteral, existeR.Code ?? -1, new List<OrdenesUensData>());

            string sql = existeR.Result > 0
                ? @"SELECT COD_ORDEN, COD_PRODUCTO, COD_UNIDAD, CANTIDAD, TIPO_PRODUCTO, REGISTRO_USUARIO, REGISTRO_FECHA
                    FROM CPR_ORDENES_UENS
                    WHERE COD_ORDEN = @CodOrden AND COD_PRODUCTO = @CodProducto;"
                : @"SELECT O.COD_ORDEN, D.COD_PRODUCTO, BS.COD_UNIDAD, BS.CANTIDAD, P.TIPO_PRODUCTO
                    FROM cpr_ordenes O
                    LEFT JOIN CPR_ORDENES_DETALLE D ON D.COD_ORDEN = O.COD_ORDEN
                    LEFT JOIN PV_PRODUCTOS P ON P.COD_PRODUCTO = D.COD_PRODUCTO
                    LEFT JOIN CPR_SOLICITUD S ON S.ADJUDICA_PROVEEDOR = O.COD_PROVEEDOR AND S.ADJUDICA_ORDEN = O.COD_ORDEN
                    LEFT JOIN CPR_SOLICITUD_BS BS ON BS.CPR_ID = S.CPR_ID AND BS.COD_PRODUCTO = D.COD_PRODUCTO
                    WHERE O.COD_ORDEN = @CodOrden AND D.COD_PRODUCTO = @CodProducto;";

            return DbHelper.ExecuteListQuery<OrdenesUensData>(_portalDb, CodEmpresa, sql, new { CodOrden, CodProducto });
        }

        public ErrorDto OrdenesUENs_Guardar(int CodEmpresa, List<OrdenesUensData> lista)
        {
            var r = DbHelper.WithConn<ErrorDto>(_portalDb, CodEmpresa, conn =>
            {
                if (conn.State != ConnectionState.Open) conn.Open();

                foreach (var item in lista)
                {
                    const string procedure = "[spCPR_Ordenes_UENS_Upsert]";
                    var values = new
                    {
                        cod_orden = item.cod_orden,
                        cod_producto = item.cod_producto,
                        cod_unidad = item.cod_unidad,
                        cantidad = item.cantidad,
                        usuario = item.registro_usuario
                    };

                    var code = conn.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    if (code < 0) return DbHelper.ErrorResponse("Error al guardar registros", -1);
                }

                return DbHelper.OkResponse("Registros agregados correctamente");
            });

            if (r.Code != 0 || r.Result == null)
                return DbHelper.ErrorResponse(r.Description ?? ErrorLiteral, r.Code ?? -1);

            return r.Result;
        }

        public ErrorDto OrdenesUENs_Eliminar(int CodEmpresa, string cod_orden, string cod_producto, string cod_unidad)
        {
            return DbHelper.ExecuteNonQuery(_portalDb, CodEmpresa,
                @"DELETE FROM CPR_ORDENES_UENS
                  WHERE COD_ORDEN = @CodOrden AND COD_PRODUCTO = @CodProducto AND COD_UNIDAD = @CodUnidad;",
                new { CodOrden = cod_orden, CodProducto = cod_producto, CodUnidad = cod_unidad });
        }

        public ErrorDto<List<CprHorarioLista>> horarios_Obtener(int CodEmpresa, string usuario)
        {
            const string sql = @"SELECT catalogo_id item, DESCRIPCION
                                 FROM CPR_CATALOGOS_ORDENES
                                 WHERE tipo_id = 1;";
            return DbHelper.ExecuteListQuery<CprHorarioLista>(_portalDb, CodEmpresa, sql, null);
        }

        public ErrorDto<List<CprFormaPago>> formapago_Obtener(int CodEmpresa, string usuario)
        {
            const string sql = @"SELECT catalogo_id item, DESCRIPCION
                                 FROM CPR_CATALOGOS_ORDENES
                                 WHERE tipo_id = 2;";
            return DbHelper.ExecuteListQuery<CprFormaPago>(_portalDb, CodEmpresa, sql, null);
        }

        public ErrorDto CorreoNotificaOrdenCompra(int CodEmpresa, string cod_orden, string proveedor, string cod_proveedor)
        {
            try
            {
                CorreoNotificaOrdenCompra_Enviar(CodEmpresa, cod_orden, proveedor, cod_proveedor)
                    .GetAwaiter().GetResult();

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message, -1);
            }
        }

        private async Task CorreoNotificaOrdenCompra_Enviar(int CodEmpresa, string cod_orden, string proveedor, string cod_proveedor)
        {
            var emailProveedor = ObtenerEmailProveedor(CodEmpresa, cod_proveedor);
            if (string.IsNullOrWhiteSpace(emailProveedor))
            {
                throw new InvalidOperationException("El proveedor no tiene email registrado.");
            }

            var correoConfigResult = _envioCorreoDB.CorreoConfig(CodEmpresa, _notificaciones);
            var eConfig = correoConfigResult.Result;
            if (eConfig == null)
                throw new InvalidOperationException("No hay configuración de correo.");

            var body = ConstruirBodyCorreo(cod_orden, proveedor);

            var attachments = new List<IFormFile>();
            var fileBoleta = await BoletaRegistro(CodEmpresa, cod_orden);
            if (fileBoleta != null) attachments.Add(fileBoleta);

            if (_sendEmail != "Y") return;

            var resp = new ErrorDto { Code = 0 };

            var emailRequest = new EmailRequest
            {
                To = emailProveedor,
                From = eConfig.User,
                Subject = "Notificación de Orden de Compra",
                Body = body,
                Attachments = attachments
            };

            await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, resp);

            if (resp.Code != 0)
                throw new InvalidOperationException(resp.Description ?? $"{ErrorLiteral} enviando correo");
        }

        private string ObtenerEmailProveedor(int CodEmpresa, string cod_proveedor)
        {
            // Parametrizado SIEMPRE
            var r = DbHelper.ExecuteSingleQuery(_portalDb, CodEmpresa,
                @"SELECT EMAIL FROM CXP_PROVEEDORES WHERE COD_PROVEEDOR = @CodProveedor;",
                "",
                new { CodProveedor = cod_proveedor });

            return r.Code == 0 ? (r.Result ?? "") : "";
        }

        private static string ConstruirBodyCorreo(string cod_orden, string proveedor)
        {
            return @$"<html lang=""es"">
<head><meta charset=""UTF-8""></head>
<body>
  <h3><strong>Notificación Orden de Compra</strong></h3>
  <p>No. Orden de Compras <strong>{cod_orden}</strong></p>
  <p>Proveedor: {proveedor}</p>
  <p>Se le adjunta la Orden de Compra</p>
</body>
</html>";
        }

        private async Task<IFormFile?> BoletaRegistro(int CodEmpresa, string cod_orden)
        {
            string repServer = _config.GetSection("ReporteSrv").GetSection("ReportServer").Value?.ToString() ?? "";
            string baseUrl = repServer + "/frmCprOrdenes/Compras_OrdenesBoleta";

            string jsonParam = @"{""Usuario"":""Usuario de Demostración""}";
            string jsonParamEncoded = HttpUtility.UrlEncode(jsonParam);

            // antes estaba fijo CodEmpresa=61 -> ahora usa el parámetro real
            string parametros = $"?CodEmpresa={CodEmpresa}&nombreRepotre=Compras_OrdenesBoleta&parametros={jsonParamEncoded}&cod_orden={cod_orden}";
            string fullUrl = $"{baseUrl}{parametros}";

            using var client = new HttpClient();

            try
            {
                var response = await client.GetAsync(fullUrl);
                if (!response.IsSuccessStatusCode) return null;

                var base64String = await response.Content.ReadAsStringAsync();
                var fileBytes = Convert.FromBase64String(base64String);

                using var stream = new MemoryStream(fileBytes);
                // Create a new MemoryStream for the returned FormFile to avoid leaking the disposable flagged by CodeQL
                var fileStream = new MemoryStream(stream.ToArray());
                return new FormFile(fileStream, 0, fileBytes.Length, "file", $"OrdenCompra_{cod_orden}.pdf");
            }
            catch
            {
                return null;
            }
        }
    }
}