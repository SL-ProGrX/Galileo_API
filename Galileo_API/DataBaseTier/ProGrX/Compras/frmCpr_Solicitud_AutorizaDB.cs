using Dapper;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier
{
    public class FrmCprSolicitudAutorizaDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _dbBitacora;
        private readonly FrmCprSolicitudDB _solicitudDB;
        private readonly FrmCprCompraDirectaDB compraDirectaDB;

        private const string MsgOk = "Ok";
        private const string MsgSolicitudNoExiste = "La solicitud no existe.";

        private const string SqlSolicitudById = "SELECT * FROM CPR_SOLICITUD WHERE CPR_ID = @cpr_id;";
        private const string SqlRecomendacionById = "SELECT RECOMENDACION FROM CPR_SOLICITUD WHERE CPR_ID = @cpr_id;";
        private const string SqlContratoById = "SELECT COD_CONTRATO FROM CPR_SOLICITUD WHERE CPR_ID = @cpr_id;";
        private const string MsgAdjudicacionCerrada = "Proceso de adjudicación cerrado satisfactoriamente!";

        public FrmCprSolicitudAutorizaDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _dbBitacora = new MSecurityMainDb(config);
            _solicitudDB = new FrmCprSolicitudDB(config);
            compraDirectaDB = new FrmCprCompraDirectaDB(config);
        }

        // ---- Helpers ----
        private ErrorDto<T> Ok<T>(T result, string desc = MsgOk)
            => new ErrorDto<T> { Code = 0, Description = desc, Result = result };

        private ErrorDto<T> Fail<T>(Exception ex)
            => new ErrorDto<T> { Code = -1, Description = ex.Message, Result = default! };

        private ErrorDto OkNoResult(string desc = MsgOk)
            => new ErrorDto { Code = 0, Description = desc };

        private ErrorDto FailNoResult(Exception ex)
            => new ErrorDto { Code = -1, Description = ex.Message };

     

        public ErrorDto Bitacora(BitacoraInsertarDto data) => _dbBitacora.Bitacora(data);

        // -------------------- CONSULTAS --------------------

        public ErrorDto<List<CprSolicitudAdjudicaConsulta>> CprSolicitudAdjudica_Consultar(int CodEmpresa, int cpr_id)
        {
            try
            {
                return DbHelper.WithConn(_portalDb, CodEmpresa,  conn =>
                {
                    const string sql = "EXEC spCprSolicitudProveedoresLista_Obtener @cpr_id;";
                    return conn.Query<CprSolicitudAdjudicaConsulta>(sql, new { cpr_id }).ToList();
                });
            }
            catch (Exception ex)
            {
                return Fail<List<CprSolicitudAdjudicaConsulta>>(ex);
            }
        }

        public ErrorDto<List<CprSolicitudAdjudicaProductosDto>> CprSolicitudAdjudicaProductos_Consultar(
            int CodEmpresa, int cpr_id, int proveedor, string? cotizacion)
        {
            try
            {
                return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
                {
                    var solicitud = conn.QueryFirstOrDefault<CprSolicitudDto>(SqlSolicitudById, new { cpr_id });
                    if (solicitud == null)
                        throw new InvalidOperationException(MsgSolicitudNoExiste);

                    var tipoGm = _solicitudDB.CprSolicitud_TipoExcepcionGM(CodEmpresa)?.Description ?? string.Empty;
                    var tipoEx = _solicitudDB.CprSolicitud_TipoExcepcion(CodEmpresa)?.Description ?? string.Empty;

                    var sql = GetSqlProductosAdjudicados(solicitud.tipo_orden ?? string.Empty, tipoGm, tipoEx);

                    return conn.Query<CprSolicitudAdjudicaProductosDto>(sql, new
                    {
                        cpr_id,
                        proveedor,
                        cotizacion = cotizacion ?? string.Empty
                    }).ToList();
                });
            }
            catch (Exception ex)
            {
                return Fail<List<CprSolicitudAdjudicaProductosDto>>(ex);
            }
        }

        public ErrorDto<string> CprSolicitudRecomendacion_Obtener(int CodEmpresa, int cpr_id)
        {
            try
            {
                return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
                    conn.QueryFirstOrDefault<string>(SqlRecomendacionById, new { cpr_id }) ?? string.Empty
                );
            }
            catch (Exception ex)
            {
                return Fail<string>(ex);
            }
        }

        public ErrorDto<string> CprSolicitudNumContrato_Obtener(int CodEmpresa, int cpr_id)
        {
            try
            {
                return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
                    conn.QueryFirstOrDefault<string>(SqlContratoById, new { cpr_id }) ?? string.Empty
                );
            }
            catch (Exception ex)
            {
                return Fail<string>(ex);
            }
        }

        // -------------------- GUARDAR / UPSERT --------------------

        public ErrorDto CprSolicitudAdjudicaProv_Upsert(int CodEmpresa, string adjudica)
        {
            try
            {
                var datos = ParseAdjudica(adjudica);

                var resp = DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
                {
                    var solicitud = GetSolicitudOrThrow(conn, datos.cpr_id);
                    ValidateMontoIfGM(conn, CodEmpresa, solicitud, datos);

                    UpsertLineasAdjudicacion(conn, datos);

                    var adjudicado = ExisteAdjudicacionProveedor(conn, datos.cpr_id, datos.proveedor!.proveedor_codigo);
                    SetEstadoProveedor(conn, datos, adjudicado);

                    return adjudicado
                        ? $"Proveedor {datos.proveedor!.descripcion} adjudicado satisfactoriamente!"
                        : $"Proveedor {datos.proveedor!.descripcion} desadjudicado satisfactoriamente!";
                });

                return resp.Code == -1
                    ? new ErrorDto { Code = -1, Description = resp.Description }
                    : new ErrorDto { Code = 0, Description = resp.Result ?? MsgOk };
            }
            catch (Exception ex)
            {
                return FailNoResult(ex);
            }
        }

// ---------------- helpers ----------------

        private static CprSolicitudAdjudicaGuardar ParseAdjudica(string adjudica)
        {
            var datos = JsonConvert.DeserializeObject<CprSolicitudAdjudicaGuardar>(adjudica)
                        ?? throw new InvalidOperationException("Parámetros inválidos.");

            if (datos.proveedor == null)
                throw new InvalidOperationException("Proveedor inválido.");

            return datos;
        }

        private static CprSolicitudDto GetSolicitudOrThrow(SqlConnection conn, int cprId)
        {
            var solicitud = conn.QueryFirstOrDefault<CprSolicitudDto>(SqlSolicitudById, new { cpr_id = cprId });
            if (solicitud == null)
                throw new InvalidOperationException(MsgSolicitudNoExiste);

            return solicitud;
        }

        private void ValidateMontoIfGM(SqlConnection conn, int codEmpresa, CprSolicitudDto solicitud, CprSolicitudAdjudicaGuardar datos)
        {
            var tipoGm = _solicitudDB.CprSolicitud_TipoExcepcionGM(codEmpresa)?.Description ?? string.Empty;
            if ((solicitud.tipo_orden ?? string.Empty) != tipoGm) return;

            var montos = GetMontosSolicitudGM(conn, datos.cpr_id, datos.proveedor!.proveedor_codigo);
            var montoOrden = montos.MontoOrden ?? 0f;
            var montoConOrden = montos.MontoAdjudicado + montoOrden;

            if (montoConOrden > montos.MontoMaximo)
                throw new InvalidOperationException("El monto de la compra sobrepasa el valor permitido para la solicitud");
        }

        private static void UpsertLineasAdjudicacion(SqlConnection conn, CprSolicitudAdjudicaGuardar datos)
        {
            const string whereBase = "WHERE CPR_ID = @cpr_id AND PROVEEDOR_CODIGO = @proveedor";

            var productos = datos.productos ?? new List<CprSolicitudAdjudicaProductosDto>();
            foreach (var item in productos)
            {
                var sql = BuildSqlUpsertLinea(whereBase, item.adjudica_ind == true);

                conn.Execute(sql, new
                {
                    cpr_id = datos.cpr_id,
                    proveedor = datos.proveedor!.proveedor_codigo,
                    cod_producto = item.cod_producto,
                    no_cotizacion = datos.proveedor!.no_cotizacion ?? string.Empty
                });
            }
        }

        private static string BuildSqlUpsertLinea(string whereBase, bool adjudica)
        {
            return adjudica
                ? $@"UPDATE CPR_SOLICITUD_PROV_BS
                    SET ADJUDICA_IND = 1, ESTADO = 'F'
                    {whereBase}
                    AND COD_PRODUCTO = @cod_producto
                    AND NO_COTIZACION = @no_cotizacion;"
                : $@"UPDATE CPR_SOLICITUD_PROV_BS
                    SET ADJUDICA_IND = 0, ESTADO = 'V'
                    {whereBase}
                    AND COD_PRODUCTO = @cod_producto
                    AND NO_COTIZACION = @no_cotizacion;";
        }

        private static bool ExisteAdjudicacionProveedor(SqlConnection conn, int cprId, int proveedorCodigo)
        {
            const string whereBase = "WHERE CPR_ID = @cpr_id AND PROVEEDOR_CODIGO = @proveedor";

            var existe = conn.QueryFirstOrDefault<int>(
                $@"SELECT COUNT(*) FROM CPR_SOLICITUD_PROV_BS {whereBase} AND ADJUDICA_IND = 1;",
                new { cpr_id = cprId, proveedor = proveedorCodigo }
            );

            return existe > 0;
        }

        private static void SetEstadoProveedor(SqlConnection conn, CprSolicitudAdjudicaGuardar datos, bool adjudicado)
        {
            const string whereBase = "WHERE CPR_ID = @cpr_id AND PROVEEDOR_CODIGO = @proveedor";

            if (adjudicado)
            {
                conn.Execute(
                    $@"UPDATE CPR_SOLICITUD_PROV
                        SET ADJUDICA_IND = 1,
                            ESTADO = 'F',
                            ADJUDICA_USUARIO = @usuario,
                            ADJUDICA_FECHA = GETDATE()
                        {whereBase};",
                    new
                    {
                        cpr_id = datos.cpr_id,
                        proveedor = datos.proveedor!.proveedor_codigo,
                        usuario = datos.usuario
                    }
                );
                return;
            }

            conn.Execute(
                $@"UPDATE CPR_SOLICITUD_PROV
                    SET ADJUDICA_IND = 0,
                        ESTADO = 'V',
                        ADJUDICA_USUARIO = NULL,
                        ADJUDICA_FECHA = NULL
                    {whereBase};",
                new { cpr_id = datos.cpr_id, proveedor = datos.proveedor!.proveedor_codigo }
            );
        }
        
        public ErrorDto CprSolicitudRecomendacion_Guardar(int CodEmpresa, int cpr_id, string recomendacion, string? cod_contrato, bool requiereContrato)
        {
            try
            {
                var resp = DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
                {
                    if (requiereContrato)
                    {
                        const string sql = @"
UPDATE CPR_SOLICITUD
   SET RECOMENDACION = @recomendacion,
       COD_CONTRATO = @cod_contrato,
       I_CONTRATO_REQUIERE = 1
 WHERE CPR_ID = @cpr_id;";
                        return conn.Execute(sql, new { recomendacion, cod_contrato = cod_contrato ?? string.Empty, cpr_id });
                    }
                    else
                    {
                        const string sql = @"
UPDATE CPR_SOLICITUD
   SET RECOMENDACION = @recomendacion,
       COD_CONTRATO = NULL,
       I_CONTRATO_REQUIERE = 0
 WHERE CPR_ID = @cpr_id;";
                        return conn.Execute(sql, new { recomendacion, cpr_id });
                    }
                });

                if (resp.Code == -1) return new ErrorDto { Code = -1, Description = resp.Description };

                return requiereContrato
                    ? OkNoResult("Recomendación y número de contrato guardados satisfactoriamente!")
                    : OkNoResult("Recomendación guardada satisfactoriamente!");
            }
            catch (Exception ex)
            {
                return FailNoResult(ex);
            }
        }


        // -------------------- INTERNOS --------------------

        public ErrorDto CprSolicitudAdjudicacion_Cerrar(int CodEmpresa, int cpr_id, string usuario)
        {
            try
            {
                var resp = DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
                {
                    EnsureNoProductosSinAdjudicar(conn, cpr_id);

                    var solicitud = GetSolicitudOrThrow(conn, cpr_id);

                    if (IsCompraDirecta(CodEmpresa, solicitud))
                        return CerrarCompraDirecta(conn, CodEmpresa, cpr_id, usuario);

                    CerrarOrdenesProveedor(conn, CodEmpresa, solicitud, cpr_id, usuario);

                    return MsgAdjudicacionCerrada;
                });

                return resp.Code == -1
                    ? new ErrorDto { Code = -1, Description = resp.Description }
                    : new ErrorDto { Code = 0, Description = resp.Result ?? MsgAdjudicacionCerrada };
            }
            catch (Exception ex)
            {
                return FailNoResult(ex);
            }
        }

        // ---------------- helpers ----------------

        private static void EnsureNoProductosSinAdjudicar(SqlConnection conn, int cprId)
        {
            const string sql = @"
        SELECT COUNT(*)
        FROM (
            SELECT CASE
                WHEN EXISTS (
                    SELECT 1
                    FROM CPR_SOLICITUD_PROV_BS
                    WHERE CPR_ID = P.CPR_ID
                    AND COD_PRODUCTO = P.COD_PRODUCTO
                    AND ADJUDICA_IND = 1
                ) THEN 1 ELSE 0 END AS OCUPADO
            FROM CPR_SOLICITUD_BS P
            WHERE P.CPR_ID = @cpr_id
        ) X
        WHERE OCUPADO = 0;";

            var sinAdjudicar = conn.QueryFirstOrDefault<int>(sql, new { cpr_id = cprId });
            if (sinAdjudicar > 0)
                throw new InvalidOperationException("Existen productos sin adjudicar, por favor verifique!");
        }

        private bool IsCompraDirecta(int codEmpresa, CprSolicitudDto solicitud)
        {
            var tipoEx = _solicitudDB.CprSolicitud_TipoExcepcion(codEmpresa)?.Description ?? string.Empty;
            return (solicitud.tipo_orden ?? string.Empty) == tipoEx;
        }

        private string CerrarCompraDirecta(SqlConnection conn, int codEmpresa, int cprId, string usuario)
        {
            var solicitud = conn.QueryFirstOrDefault<CprSolicitudDto>(SqlSolicitudById, new { cpr_id = cprId });
            if (solicitud == null)
                throw new InvalidOperationException(MsgSolicitudNoExiste);

            if (string.IsNullOrWhiteSpace(solicitud.documento) || solicitud.documento.Trim() == "0")
                throw new InvalidOperationException("Se requiere el número de factura del proveedor seleccionado para la compra directa.");

            var cd = CompraDirectaSolicitud_Autorizar(conn, codEmpresa, cprId, usuario);
            if (cd.Code == -1)
                throw new InvalidOperationException(cd.Description);

            return cd.Description ?? MsgAdjudicacionCerrada;
        }

        private void CerrarOrdenesProveedor(SqlConnection conn, int codEmpresa, CprSolicitudDto solicitud, int cprId, string usuario)
        {
            var proveedores = GetProveedoresAdjudicadosSinOrden(conn, cprId);

            var orden = BuildOrdenBase(solicitud, usuario);

            foreach (var prov in proveedores)
            {
                orden.lineas = GetLineasAdjudicadas(conn, cprId, prov);
                var r = OrdenesGuardar(conn, codEmpresa, orden, prov, cprId);
                if (r.Code == -1)
                    throw new InvalidOperationException(r.Description);
            }
        }

        private static List<string> GetProveedoresAdjudicadosSinOrden(SqlConnection conn, int cprId)
        {
            const string sqlProv = @"
        SELECT PROVEEDOR_CODIGO
        FROM CPR_SOLICITUD_PROV
        WHERE CPR_ID = @cpr_id
        AND ADJUDICA_IND = 1
        AND COD_ORDEN IS NULL;";

            return conn.Query<string>(sqlProv, new { cpr_id = cprId }).ToList();
        }

        private static OrdenDatosAcciones BuildOrdenBase(CprSolicitudDto solicitud, string usuario)
        {
            return new OrdenDatosAcciones
            {
                usuario = usuario,
                tipo_orden = solicitud.tipo_orden ?? string.Empty,
                nota = solicitud.detalle ?? string.Empty,
                edita = false,
                estado = "S",
                cod_orden = ""
            };
        }

        private static List<OrdenLineas> GetLineasAdjudicadas(SqlConnection conn, int cprId, string proveedor)
        {
            const string sqlLineas = @"
        SELECT  [COD_PRODUCTO] AS COD_PRODUCTO,
                '' AS DESCRIPCION,
                [CANTIDAD] AS CANTIDAD,
                [MONTO] AS PRECIO,
                [DESC_PORC] AS DESCUENTO,
                [IVA_PORC] AS IMP_VENTAS,
                [TOTAL] AS TOTAL
        FROM [dbo].[CPR_SOLICITUD_PROV_BS]
        WHERE CPR_ID = @cpr_id
        AND ADJUDICA_IND = 1
        AND PROVEEDOR_CODIGO = @proveedor;";

            return conn.Query<OrdenLineas>(sqlLineas, new { cpr_id = cprId, proveedor }).ToList();
        }


        private static string GetSqlProductosAdjudicados(string tipoSolicitud, string tipoGm, string tipoEx)
        {
            if (tipoSolicitud == tipoGm)
            {
                return @"
SELECT DISTINCT
    SP.NO_COTIZACION,
    C.COD_PRODUCTO,
    P.DESCRIPCION + '-' + C.MODELO AS DESCRIPCION,
    C.MONTO,
    SP.ADJUDICA_IND,
    C.CANTIDAD,
    C.DESC_MONTO,
    C.IVA_MONTO,
    C.TOTAL,
    (
        SELECT CASE
            WHEN (
                (SELECT MONTO FROM CPR_SOLICITUD WHERE CPR_ID = PP.CPR_ID) -
                ISNULL((SELECT SUM(TOTAL) FROM CPR_SOLICITUD_PROV_BS WHERE CPR_ID = PP.CPR_ID AND ADJUDICA_IND = 1),0)
            ) <= 0 THEN 1 ELSE 0 END
    ) AS OCUPADO
FROM CPR_SOLICITUD_PROV_COTIZA_LINEAS C
LEFT JOIN CPR_SOLICITUD_PROV_COTIZA spc ON spc.ID_COTIZACION = C.ID_COTIZACION
LEFT JOIN PV_PRODUCTOS P ON C.COD_PRODUCTO = P.COD_PRODUCTO
LEFT JOIN CPR_SOLICITUD_PROV_BS SP ON C.COD_PRODUCTO = P.COD_PRODUCTO
LEFT JOIN CPR_SOLICITUD_PROV PP ON PP.CPR_ID = SP.CPR_ID
WHERE SP.PROVEEDOR_CODIGO = @proveedor
  AND PP.CPR_ID = @cpr_id
  AND spc.CPR_ID = SP.CPR_ID
  AND spc.PROVEEDOR_CODIGO = SP.PROVEEDOR_CODIGO
  AND SP.NO_COTIZACION = @cotizacion;";
            }

            if (tipoSolicitud == tipoEx)
            {
                return @"
SELECT DISTINCT
    SP.NO_COTIZACION,
    C.COD_PRODUCTO,
    P.DESCRIPCION + '-' + C.MODELO AS DESCRIPCION,
    C.MONTO,
    SP.ADJUDICA_IND,
    C.CANTIDAD,
    C.DESC_MONTO,
    C.IVA_MONTO,
    C.TOTAL,
    (SELECT CASE WHEN EXISTS (
        SELECT 1 FROM CPR_SOLICITUD_PROV_BS
        WHERE CPR_ID = SP.CPR_ID AND COD_PRODUCTO = C.COD_PRODUCTO AND ADJUDICA_IND = 1
    ) THEN 1 ELSE 0 END) AS OCUPADO
FROM CPR_SOLICITUD_PROV_COTIZA_LINEAS C
LEFT JOIN CPR_SOLICITUD_PROV_COTIZA spc ON spc.ID_COTIZACION = C.ID_COTIZACION
LEFT JOIN PV_PRODUCTOS P ON C.COD_PRODUCTO = P.COD_PRODUCTO
LEFT JOIN CPR_SOLICITUD_PROV_BS SP ON C.COD_PRODUCTO = P.COD_PRODUCTO
LEFT JOIN CPR_SOLICITUD_PROV PP ON PP.CPR_ID = SP.CPR_ID
WHERE SP.PROVEEDOR_CODIGO = @proveedor
  AND PP.CPR_ID = @cpr_id
  AND spc.CPR_ID = SP.CPR_ID
  AND spc.PROVEEDOR_CODIGO = SP.PROVEEDOR_CODIGO
  AND SP.NO_COTIZACION = @cotizacion;";
            }

            return @"
SELECT DISTINCT
    C.COD_PRODUCTO,
    P.DESCRIPCION,
    C.MONTO,
    C.ADJUDICA_IND,
    (SELECT TOP 1 CANTIDAD FROM CPR_SOLICITUD_BS WHERE CPR_ID = C.CPR_ID AND COD_PRODUCTO = C.COD_PRODUCTO) AS CANTIDAD,
    C.DESC_MONTO,
    C.IVA_MONTO,
    (C.MONTO + C.IVA_MONTO - C.DESC_MONTO) *
    (SELECT TOP 1 CANTIDAD FROM CPR_SOLICITUD_BS WHERE CPR_ID = C.CPR_ID AND COD_PRODUCTO = C.COD_PRODUCTO) AS TOTAL,
    (SELECT CASE WHEN EXISTS (
        SELECT 1 FROM CPR_SOLICITUD_PROV_BS
        WHERE CPR_ID = C.CPR_ID AND COD_PRODUCTO = C.COD_PRODUCTO AND ADJUDICA_IND = 1
    ) THEN 1 ELSE 0 END) AS OCUPADO
FROM CPR_SOLICITUD_PROV_BS C
LEFT JOIN PV_PRODUCTOS P ON C.COD_PRODUCTO = P.COD_PRODUCTO
WHERE C.CPR_ID = @cpr_id
  AND C.PROVEEDOR_CODIGO = @proveedor
  AND C.NO_COTIZACION = @cotizacion;";
        }

        private static SolicitudMontosDto GetMontosSolicitudGM(SqlConnection conn, int cpr_id, int proveedorCodigo)
        {
            const string sql = @"
SELECT
    s.MONTO AS MontoMaximo,
    ISNULL((
        SELECT SUM(TOTAL)
        FROM CPR_SOLICITUD_PROV_BS spbs1
        WHERE spbs1.CPR_ID = s.CPR_ID AND ADJUDICA_IND = 1
    ), 0) AS MontoAdjudicado,
    (
        SELECT TOP 1 TOTAL
        FROM CPR_SOLICITUD_PROV_BS spbs2
        WHERE spbs2.CPR_ID = s.CPR_ID
          AND spbs2.ADJUDICA_IND IS NULL
          AND spbs2.PROVEEDOR_CODIGO = @proveedorCodigo
    ) AS MontoOrden
FROM CPR_SOLICITUD s
WHERE s.CPR_ID = @cpr_id;";

            return conn.QueryFirstOrDefault<SolicitudMontosDto>(sql, new { cpr_id, proveedorCodigo })
                   ?? new SolicitudMontosDto();
        }

        private ErrorDto OrdenesGuardar(SqlConnection conn, int CodEmpresa, OrdenDatosAcciones ordenes, string proveedor, int cpr_id)
        {
            var valida = fxInvVerificaLineaDetalle(conn, ordenes.lineas ?? new List<OrdenLineas>(), "E");
            if (valida.Code == -1) return valida;

            try
            {
                float curSubTotal = 0;
                float curDescuento = 0;
                float curIV = 0;
                float curCantidad = 0;
                float curTotal = 0;

                var lineasCalc = sbCalculaTotales(
                    ordenes.lineas ?? new List<OrdenLineas>(),
                    ref curSubTotal,
                    ref curDescuento,
                    ref curIV,
                    ref curCantidad,
                    ref curTotal
                );

                if (lineasCalc.Code == -1)
                    return new ErrorDto { Code = -1, Description = lineasCalc.Description };

                ordenes.lineas = lineasCalc.Result ?? new List<OrdenLineas>();

                // consecutivo
                var vConsecutivo = conn.QueryFirstOrDefault<string>(
                    "SELECT ISNULL(MAX(cod_orden),0) + 1 AS Ultimo FROM cpr_Ordenes;"
                ) ?? "1";
                vConsecutivo = vConsecutivo.PadLeft(10, '0');

                const string sqlCab = @"
INSERT INTO cpr_ordenes (
    cod_orden, tipo_orden, estado, genera_fecha, nota, genera_user,
    subtotal, descuento, imp_ventas, total, pin_autorizacion, pin_entrada,
    proceso, cod_proveedor
)
VALUES (
    @CodOrden, @TipoOrden, 'S', GETDATE(), @Nota, @Usuario,
    @SubTotal, @Descuento, @ImpVentas, @Total, 0, '', 'P', @CodProveedor
);";

                conn.Execute(sqlCab, new
                {
                    CodOrden = vConsecutivo,
                    TipoOrden = ordenes.tipo_orden,
                    Nota = ordenes.nota,
                    Usuario = ordenes.usuario,
                    SubTotal = curSubTotal,
                    Descuento = curDescuento,
                    ImpVentas = curIV,
                    Total = curTotal,
                    CodProveedor = proveedor
                });

                // Bitácora
                Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = ordenes.usuario,
                    DetalleMovimiento = "Registra, Orden Compra:" + vConsecutivo,
                    Movimiento = "Registra - WEB",
                    Modulo = 35
                });

                // Detalle
                conn.Execute("DELETE cpr_ordenes_detalle WHERE cod_orden = @cod_orden;", new { cod_orden = vConsecutivo });

                int linea = 0;
                const string sqlDet = @"
INSERT cpr_ordenes_detalle(
    linea, cod_orden, cod_producto, cantidad, estado, precio, descuento, imp_ventas
)
VALUES (
    @linea, @cod_orden, @cod_producto, @cantidad, 'A', @precio, @descuento, @imp_ventas
);";

                foreach (var item in ordenes.lineas ?? new List<OrdenLineas>())
                {
                    linea++;
                    conn.Execute(sqlDet, new
                    {
                        linea,
                        cod_orden = vConsecutivo,
                        cod_producto = item.Cod_Producto,
                        cantidad = item.Cantidad,
                        precio = item.Precio,
                        descuento = item.Descuento,
                        imp_ventas = item.Imp_Ventas
                    });
                }

                // Actualizo solicitud
                const string sqlSol = @"
UPDATE CPR_SOLICITUD
   SET ESTADO = 'F',
       ADJUDICA_ORDEN = @cod_orden,
       ADJUDICA_USUARIO = @usuario,
       ADJUDICA_FECHA = GETDATE(),
       ADJUDICA_PROVEEDOR = @proveedor
 WHERE CPR_ID = @cpr_id;";
                conn.Execute(sqlSol, new { cod_orden = vConsecutivo, usuario = ordenes.usuario, proveedor, cpr_id });

                // Actualizo proveedor
                const string sqlProv = @"
UPDATE CPR_SOLICITUD_PROV
   SET ESTADO = 'F',
       ADJUDICA_ORDEN = @cod_orden
 WHERE CPR_ID = @cpr_id
   AND PROVEEDOR_CODIGO = @proveedor;";
                conn.Execute(sqlProv, new { cod_orden = vConsecutivo, cpr_id, proveedor });

                return new ErrorDto { Code = 0, Description = MsgAdjudicacionCerrada };
            }
            catch (Exception ex)
            {
                return FailNoResult(ex);
            }
        }

        private ErrorDto fxInvVerificaLineaDetalle(
            SqlConnection conn,
            List<OrdenLineas> vGrid,
            string vMov,
            int? ColBod1 = 0,
            int ColBod2 = 0)
        {
            try
            {
                var gridErr = ValidateGrid(vGrid);
                if (gridErr != null) return gridErr;

                foreach (var item in vGrid)
                {
                    var err = ValidateLinea(conn, item, vMov, ColBod1, ColBod2);
                    if (err != null) return err;
                }

                return new ErrorDto { Code = 0, Description = MsgOk };
            }
            catch (Exception ex)
            {
                return FailNoResult(ex);
            }
        }

        private static ErrorDto? ValidateGrid(List<OrdenLineas>? vGrid)
        {
            if (vGrid == null || vGrid.Count == 0)
                return new ErrorDto { Code = -1, Description = "No hay productos en la orden" };

            return null;
        }

        private static ErrorDto? ValidateLinea(
            SqlConnection conn,
            OrdenLineas item,
            string vMov,
            int? colBod1,
            int colBod2)
        {
            var err = ValidateProducto(conn, item);
            if (err != null) return err;

            err = ValidaBodega(conn, colBod1 ?? 0, vMov);
            if (err != null) return err;

            err = ValidaBodega(conn, colBod2, vMov);
            if (err != null) return err;

            return null;
        }

        private static ErrorDto? ValidateProducto(SqlConnection conn, OrdenLineas item)
        {
            if (item.Cantidad <= 0) return null;

            const string sql = "SELECT estado FROM pv_productos WHERE cod_producto = @cod;";
            var estado = conn.QueryFirstOrDefault<string>(sql, new { cod = item.Cod_Producto });

            if (estado == null)
                return new ErrorDto { Code = -1, Description = $"El producto {item.Cod_Producto} no existe" };

            if (estado == "I")
                return new ErrorDto { Code = -1, Description = $"El producto {item.Cod_Producto} no está activo" };

            return null;
        }

        private static ErrorDto? ValidaBodega(SqlConnection conn, int codBodega, string vMov)
        {
            var bodega = conn.QueryFirstOrDefault<Models.BodegaDto>(
                "SELECT permite_entradas, permite_salidas, estado FROM pv_bodegas WHERE cod_bodega = @cod;",
                new { cod = codBodega.ToString() }
            );

            if (bodega == null) return new ErrorDto { Code = -1, Description = $"La bodega {codBodega} - No existe" };
            if (bodega.estado == "I") return new ErrorDto { Code = -1, Description = $"La bodega {codBodega} - Se encuentra Inactiva" };

            switch (vMov)
            {
                case "E":
                    if (bodega.permite_entradas != "1")
                        return new ErrorDto { Code = -1, Description = $"La bodega {codBodega} - No permite Entradas" };
                    break;

                case "S":
                case "R":
                case "T":
                    if (bodega.permite_salidas != "1")
                        return new ErrorDto { Code = -1, Description = $"La bodega {codBodega} - No permite Salidas" };
                    break;
            }

            return null;
        }

        private ErrorDto<List<OrdenLineas>> sbCalculaTotales(
            List<OrdenLineas> vGrid,
            ref float curSubTotal,
            ref float curDescuento,
            ref float curIV,
            ref float curCantidad,
            ref float curTotal)
        {
            try
            {
                foreach (var item in vGrid)
                {
                    curSubTotal += (item.Cantidad * item.Precio);
                    var curTmpDesc = ((item.Cantidad * item.Precio) * (item.Descuento / 100f));
                    curDescuento += curTmpDesc;

                    var curTmpIV = (((item.Cantidad * item.Precio) - curTmpDesc) * (item.Imp_Ventas / 100f));
                    curIV += curTmpIV;

                    item.Total = (item.Cantidad * item.Precio) - curTmpDesc + curTmpIV;
                    curCantidad += item.Cantidad;
                }

                curTotal = curSubTotal + curIV - curDescuento;
                return Ok(vGrid);
            }
            catch (Exception ex)
            {
                return Fail<List<OrdenLineas>>(ex);
            }
        }

        private ErrorDto CompraDirectaSolicitud_Autorizar(SqlConnection conn, int CodEmpresa, int CPR_ID, string usuario)
        {
            try
            {
                var solicitud = conn.QueryFirstOrDefault<CprSolicitudDto>(SqlSolicitudById, new { cpr_id = CPR_ID });
                if (solicitud == null)
                    return new ErrorDto { Code = -1, Description = MsgSolicitudNoExiste };

                const string sqlProv = @"
SELECT TOP 1
    PROVEEDOR_CODIGO as com_dir_cod_proveedor,
    cp.DESCRIPCION as com_dir_des_proveedor
FROM CPR_SOLICITUD_PROV P
LEFT JOIN CXP_PROVEEDORES cp ON cp.COD_PROVEEDOR = P.PROVEEDOR_CODIGO
WHERE CPR_ID = @cpr_id;";

                var prov = conn.QueryFirstOrDefault<CprSolicitudDto>(sqlProv, new { cpr_id = CPR_ID });
                if (prov != null)
                {
                    solicitud.com_dir_cod_proveedor = prov.com_dir_cod_proveedor;
                    solicitud.com_dir_des_proveedor = prov.com_dir_des_proveedor;
                }

                const string sqlDet = @"
SELECT
    BS.COD_PRODUCTO, BS.CANTIDAD, BS.MONTO, S.COD_BODEGA,
    BS.IVA_PORC, BS.IVA_MONTO, BS.DESC_PORC, BS.DESC_MONTO, BS.TOTAL
FROM CPR_SOLICITUD_PROV_BS BS
LEFT JOIN CPR_SOLICITUD_BS S ON S.CPR_ID = BS.CPR_ID
WHERE BS.CPR_ID = @cpr_id;";

                var detalle = conn.Query<CprSolicitudBsDto>(sqlDet, new { cpr_id = CPR_ID }).ToList();

                float impVenta = 0;
                float descuento = 0;
                var lineas = new List<CompraDirectaDetalle>();

                foreach (var item in detalle)
                {
                    lineas.Add(new CompraDirectaDetalle
                    {
                        cod_producto = item.cod_producto,
                        cantidad = item.cantidad,
                        precio = item.monto,
                        cod_bodega = string.IsNullOrEmpty(item.cod_bodega) ? "0" : item.cod_bodega,
                        imp_ventas = item.iva_porc ?? 0,
                        descuento = item.desc_porc ?? 0,
                        total = item.total
                    });

                    impVenta += item.iva_monto ?? 0;
                    descuento += item.desc_monto ?? 0;
                }

                var directaInsert = new CompraDirectaInsert
                {
                    cod_factura = solicitud.documento ?? string.Empty,
                    fecha = MProGrXAuxiliarDB.validaFechaGlobal(DateTime.Now, "yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd"),
                    usuario = usuario,
                    causa = solicitud.tipo_orden ?? string.Empty,
                    notas = solicitud.detalle,
                    cod_proveedor = solicitud.com_dir_cod_proveedor ?? 0,
                    forma_pago = string.IsNullOrEmpty(solicitud.int_tipo_pago) ? "0" : solicitud.int_tipo_pago,
                    divisa = solicitud.divisa ?? string.Empty,
                    tipo_pago = string.IsNullOrEmpty(solicitud.int_forma_pago) ? "0" : solicitud.int_forma_pago,
                    lineas = lineas,
                    sub_total = lineas.Sum(x => x.precio * x.cantidad),
                    imp_ventas = impVenta,
                    descuento = descuento,
                    total = lineas.Sum(x => x.total)
                };

                var info = compraDirectaDB.CompraDirecta_Insertar(CodEmpresa, directaInsert);

                if (info.Code != -1)
                {
                    var ordenCompra = (info.Description ?? string.Empty).Split('-');
                    var codOrden = ordenCompra.Length > 0 ? ordenCompra[0] : string.Empty;

                    const string updProv = @"
UPDATE CPR_SOLICITUD_PROV SET
    ESTADO = 'F',
    ADJUDICA_IND = 1,
    ADJUDICA_ORDEN = @codOrden,
    ADJUDICA_USUARIO = @usuario,
    ADJUDICA_FECHA = GETDATE()
WHERE CPR_ID = @cpr_id;";
                    conn.Execute(updProv, new { codOrden, usuario, cpr_id = CPR_ID });

                    const string updSol = @"
UPDATE CPR_SOLICITUD SET
    ESTADO = 'F',
    ADJUDICA_ORDEN = @codOrden,
    ADJUDICA_USUARIO = @usuario,
    ADJUDICA_FECHA = GETDATE()
WHERE CPR_ID = @cpr_id;";
                    conn.Execute(updSol, new { codOrden, usuario, cpr_id = CPR_ID });
                }

                return info;
            }
            catch (Exception ex)
            {
                return FailNoResult(ex);
            }
        }
    }
}