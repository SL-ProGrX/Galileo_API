using System.Data;
using Dapper;
using Galileo.Models;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public class FrmCprAnulacionCompraDB
    {
        private readonly PortalDB _portalDb;
        private readonly IConfiguration _config;

        public FrmCprAnulacionCompraDB(IConfiguration config)
        {
            _config = config;
            _portalDb = new PortalDB(config);
        }

        public ErrorDto<List<CompraDto>> Compras_Obtener(int codEmpresa, string filtro)
        {
            var like = $"%{(filtro ?? string.Empty).Trim()}%";

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.Query<CompraDto>(
                    @"SELECT TOP 30
                          (E.Cod_Factura +' - ' + CONVERT(VARCHAR(10), E.Cod_Proveedor)) AS dataKey,
                          E.cod_compra,
                          E.cod_orden,
                          E.cod_factura,
                          P.descripcion AS Proveedor
                      FROM cpr_compras E
                      INNER JOIN cxp_proveedores P ON E.cod_proveedor = P.cod_proveedor
                      WHERE E.cod_compra  LIKE @F
                         OR E.cod_orden   LIKE @F
                         OR E.cod_factura LIKE @F
                         OR P.descripcion LIKE @F",
                    new { F = like }
                ).ToList()
            );
        }

        public ErrorDto<CompraAnulacionDatosDto> Compra_Datos_Obtener(int codEmpresa, string cod_Compra)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.QueryFirstOrDefault<CompraAnulacionDatosDto>(
                    @"SELECT
                          E.Cod_Factura, E.Cod_Proveedor, E.Cod_Orden, E.Cod_Compra, E.Estado, E.Forma_Pago, E.Fecha,
                          E.genera_user, E.ANULA_FECHA, E.ANULA_FEC_AFECTA,
                          E.Sub_Total, E.Notas, E.Descuento, E.Imp_Ventas, E.Imp_Consumo, E.Total,
                          E.Cxp_Estado, E.Asiento_Estado, E.Asiento_Fecha,
                          (RTRIM(C.Tipo_Orden) + ' - ' + C.descripcion) AS Causa,
                          P.descripcion AS Proveedor,
                          O.nota
                      FROM cpr_ordenes O
                      INNER JOIN cpr_Tipo_Orden C ON O.Tipo_Orden = C.Tipo_Orden
                      INNER JOIN cpr_compras E    ON O.cod_orden  = E.cod_orden
                      INNER JOIN cxp_proveedores P ON E.cod_proveedor = P.cod_proveedor
                      WHERE E.cod_compra = @CodCompra",
                    new { CodCompra = cod_Compra }
                ) ?? new CompraAnulacionDatosDto()
            );
        }

        public ErrorDto<List<CompraDetalleDto>> CompraDetalles_Obtener(int codEmpresa, string cod_Factura)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.Query<CompraDetalleDto>(
                    @"SELECT D.*,
                             B.DESCRIPCION AS BODEGA,
                             P.DESCRIPCION AS PRODUCTO
                      FROM cpr_compras_detalle D
                      INNER JOIN PV_BODEGAS  B ON B.COD_BODEGA  = D.COD_BODEGA
                      INNER JOIN PV_PRODUCTOS P ON P.COD_PRODUCTO = D.COD_PRODUCTO
                      WHERE D.cod_factura = @CodFactura",
                    new { CodFactura = cod_Factura }
                ).ToList()
            );
        }

        public ErrorDto<CompraAnulacionDto> Compra_Obtener(int codEmpresa, string codCompra)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.QueryFirstOrDefault<CompraAnulacionDto>(
                    @"SELECT Cod_Factura, Cod_Proveedor, Cod_Orden, Cod_Compra, Estado, Forma_Pago, Fecha,
                             Sub_Total, Notas, Descuento, Imp_Ventas, Imp_Consumo, Total,
                             Cxp_Estado, Asiento_Estado, Asiento_Fecha
                      FROM cpr_compras
                      WHERE cod_compra = @CodCompra",
                    new { CodCompra = codCompra }
                ) ?? new CompraAnulacionDto()
            );
        }

        public ErrorDto<CompraAnulacionDatosDto> Compra_Anulacion_Datos_Obtener(int codEmpresa, CompraAnulacionDatosRequestDto req)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var sql = @"SELECT
                                E.Cod_Factura, E.Cod_Proveedor, E.Cod_Orden, E.Cod_Compra, E.Estado, E.Forma_Pago, E.Fecha,
                                E.Sub_Total, E.Notas, E.Descuento, E.Imp_Ventas, E.Imp_Consumo, E.Total,
                                E.Cxp_Estado, E.Asiento_Estado, E.Asiento_Fecha,
                                (RTRIM(C.Tipo_Orden) + ' - ' + C.descripcion) AS Causa,
                                P.descripcion AS Proveedor,
                                O.nota
                            FROM cpr_ordenes O
                            INNER JOIN cpr_Tipo_Orden C ON O.Tipo_Orden = C.Tipo_Orden
                            INNER JOIN cpr_compras E    ON O.cod_orden  = E.cod_orden
                            INNER JOIN cxp_proveedores P ON E.cod_proveedor = P.cod_proveedor
                            WHERE E.cod_compra = @CodCompra ";

                var p = new DynamicParameters();
                p.Add("CodCompra", req.codigoCompra, DbType.String);

                if (!string.IsNullOrWhiteSpace(req.codigoOrden))
                {
                    sql += " AND E.cod_orden = @CodOrden ";
                    p.Add("CodOrden", req.codigoOrden, DbType.String);
                }
                else if (!string.IsNullOrWhiteSpace(req.codigoProveedor))
                {
                    sql += " AND E.cod_proveedor = @CodProveedor ";
                    p.Add("CodProveedor", req.codigoProveedor, DbType.String);
                }

                return conn.QueryFirstOrDefault<CompraAnulacionDatosDto>(sql, p) ?? new CompraAnulacionDatosDto();
            });
        }

        public ErrorDto Compra_Anular(int codEmpresa, CompraAnulacionDto compraDto)
        {
            // transacción porque toca muchas tablas
            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                if (conn.State != ConnectionState.Open) conn.Open();
                using var tx = conn.BeginTransaction();

                try
                {
                    // 1) Si CR: ajustar saldo del proveedor
                    if (compraDto.Forma_Pago == "CR")
                    {
                        AjustarSaldoProveedor(conn, tx, compraDto);
                    }

                    // 2) Marcar compra anulada
                    MarcarCompraAnulada(conn, tx, compraDto);

                    // 3) Si CXP generado: cargo periódico + eliminar programación pendiente
                    if (compraDto.Cxp_Estado == "G")
                    {
                        ProcesarCxpAnulacion(conn, tx, compraDto);
                    }

                    // 4) Reversar inventario por detalle
                    var detalle = ObtenerDetalleCompra(conn, tx, compraDto);
                    var invErr = ReversarInventario(codEmpresa, compraDto, detalle);
                    if (invErr.Code != 0) // error de negocio
                    {
                        tx.Rollback();
                        return invErr; // devolvemos ErrorDto (pero ojo: WithConn<T> envuelve => aquí T=ErrorDto)
                    }

                    tx.Commit();
                    return DbHelper.CreateOkResponse();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            });

            // r aquí es ErrorDto<ErrorDto>
            if (r.Code != 0 || r.Result == null)
                return DbHelper.ErrorResponse(r.Description ?? "Error", r.Code ?? -1);

            return r.Result;
        }

        // ----------------- Helpers -----------------

        private static void AjustarSaldoProveedor(System.Data.IDbConnection conn, System.Data.IDbTransaction tx, CompraAnulacionDto dto)
        {
            // En tu código original usabas @Total pero NO lo mandabas. Aquí sí.
            conn.Execute(
                @"UPDATE cxp_proveedores
                  SET saldo = ISNULL(saldo, 0) - @Total
                  WHERE cod_proveedor = @CodProv",
                new
                {
                    CodProv = dto.Cod_Proveedor,
                    Total = dto.Total
                },
                transaction: tx
            );
        }

        private static void MarcarCompraAnulada(System.Data.IDbConnection conn, System.Data.IDbTransaction tx, CompraAnulacionDto dto)
        {
            conn.Execute(
                @"UPDATE cpr_compras
                  SET estado = 'A',
                      anula_fecha = GETDATE(),
                      anula_fec_afecta = @Fecha,
                      anula_user = @Usuario
                  WHERE cod_compra = @CodCompra",
                new
                {
                    Fecha = dto.Fecha,
                    Usuario = string.Empty, // en tu código era ""
                    CodCompra = dto.Cod_Compra
                },
                transaction: tx
            );
        }

        private static void ProcesarCxpAnulacion(System.Data.IDbConnection conn, System.Data.IDbTransaction tx, CompraAnulacionDto dto)
        {
            var montoPagado = conn.QueryFirstOrDefault<decimal>(
                @"SELECT ISNULL(SUM(monto), 0)
                  FROM cxp_pagoProv
                  WHERE cod_proveedor = @CodProveedor
                    AND cod_factura = @CodFactura
                    AND tesoreria IS NOT NULL",
                new { CodProveedor = dto.Cod_Proveedor, CodFactura = dto.Cod_Factura },
                transaction: tx
            );

            if (dto.Forma_Pago == "CR" && montoPagado > 0)
            {
                var ultimo = conn.QueryFirstOrDefault<int>(
                    @"SELECT ISNULL(MAX(ID), 0)
                      FROM cxp_cargosper
                      WHERE cod_proveedor = @CodProveedor",
                    new { CodProveedor = dto.Cod_Proveedor },
                    transaction: tx
                ) + 1;

                conn.Execute(
                    @"INSERT INTO cxp_cargosper
                      (id, cod_proveedor, cod_cargo, tipo, valor, vence, saldo, concepto, detalle, recaudado)
                      VALUES
                      (@Id, @CodProveedor, @CodCargo, @Tipo, @Valor, @Vence, @Saldo, @Concepto, @Detalle, @Recaudado)",
                    new
                    {
                        Id = ultimo,
                        CodProveedor = dto.Cod_Proveedor,
                        CodCargo = string.Empty,
                        Tipo = "M",
                        Valor = montoPagado,
                        Vence = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Unspecified),
                        Saldo = montoPagado,
                        Concepto = "ANULACION DE FACTURA DE COMPRA",
                        Detalle = "FACTURA : " + dto.Cod_Factura,
                        Recaudado = 0
                    },
                    transaction: tx
                );
            }

            conn.Execute(
                @"DELETE cxp_pagoProv
                  WHERE cod_proveedor = @CodProveedor
                    AND cod_factura = @CodFactura
                    AND tesoreria IS NULL",
                new { CodProveedor = dto.Cod_Proveedor, CodFactura = dto.Cod_Factura },
                transaction: tx
            );
        }

        private static List<CompraDetalleDto> ObtenerDetalleCompra(System.Data.IDbConnection conn, System.Data.IDbTransaction tx, CompraAnulacionDto dto)
        {
            return conn.Query<CompraDetalleDto>(
                @"SELECT *
                  FROM cpr_compras_detalle
                  WHERE cod_factura = @CodFactura
                    AND cod_proveedor = @CodProveedor",
                new { CodProveedor = dto.Cod_Proveedor, CodFactura = dto.Cod_Factura },
                transaction: tx
            ).ToList();
        }

        private ErrorDto ReversarInventario(int codEmpresa, CompraAnulacionDto compraDto, List<CompraDetalleDto> detalle)
        {
            var aux = new MProGrXAuxiliarDB(_config);

            foreach (var item in detalle)
            {
                var comp = new CompraInventarioDto
                {
                    CodProducto = item.Cod_Producto,
                    Cantidad = item.Cantidad,
                    CodBodega = item.Cod_Bodega,
                    CodTipo = compraDto.Cod_Compra,
                    Origen = "Compra.Anu",
                    Fecha = compraDto.Fecha.ToString(),
                    Precio = item.Precio,
                    ImpConsumo = item.Imp_Consumo,
                    ImpVentas = item.Imp_Ventas,
                    Usuario = "S"
                };

                var r = aux.sbInvInventario(codEmpresa, comp);
                if (r.Code != 0)
                    return DbHelper.ErrorResponse(r.Description ?? "Error reversando inventario", r.Code ?? -1);
            }

            return DbHelper.CreateOkResponse();
        }
    }
}