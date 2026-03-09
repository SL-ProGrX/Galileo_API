using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models;
using Dapper;
using static Galileo_API.Models.ProGrX_Polizas.FrmPolizaFacturaVerModels;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmPolizaFacturaVerDB
    {
        private readonly PortalDB _portalDb;


        public FrmPolizaFacturaVerDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene listado de divisas.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> CrdPolizaFacturaVer_Divisas_Obtener(int CodEmpresa, int codContabilidad)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"
                    SELECT
                        RTRIM(cod_divisa) AS item,
                        RTRIM(descripcion) AS descripcion
                    FROM CntX_Divisas
                    WHERE cod_contabilidad = @codContabilidad
                    ORDER BY divisa_local DESC, cod_divisa";

                return conn.Query<DropDownListaGenericaModel>(query, new { codContabilidad }).ToList();
            });
        }

        /// <summary>
        /// Obtiene la divisa local del sistema.
        /// </summary>
        public ErrorDto<CrdPolizaFacturaVerDivisaLocalModel> CrdPolizaFacturaVer_DivisaLocal_Obtener(int CodEmpresa,int codContabilidad)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            try
            {
                const string query = @"
            SELECT
                RTRIM(cod_divisa) AS Divisa,
                RTRIM(descripcion) AS DivisaLocal
            FROM CntX_Divisas
            WHERE cod_contabilidad = @codContabilidad
              AND Divisa_Local = 1";

                var result = connection.QueryFirstOrDefault<CrdPolizaFacturaVerDivisaLocalModel>(
                    query,
                    new {codContabilidad });

                if (result is null)
                {
                    return DbHelper.CreateOkResponse(new CrdPolizaFacturaVerDivisaLocalModel());
                }

                return DbHelper.CreateOkResponse(new CrdPolizaFacturaVerDivisaLocalModel());
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<CrdPolizaFacturaVerDivisaLocalModel>(
                    "Error al obtener la divisa local.",
                    -1,
                    new CrdPolizaFacturaVerDivisaLocalModel());
            }
        }

        /// <summary>
        /// Obtiene información principal de la factura.
        /// </summary>
        public ErrorDto<CrdPolizaFacturaVerFacturaResponse> CrdPolizaFacturaVer_Factura_Obtener(
            int codEmpresa,
            int proveedor,
            string factura)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                const string query = @"
            SELECT
                F.cod_factura AS Factura,
                F.cod_proveedor AS ProveedorCodigo,
                P.descripcion AS ProveedorNombre,
                F.estado AS Estado,
                F.fecha AS Fecha,
                F.vence AS FechaVence,
                F.cod_divisa AS DivisaFactura,
                P.cod_divisa AS DivisaProveedor,
                F.tipo_cambio AS TipoCambio,
                F.total AS Total,
                F.importe_divisa_real AS TotalDivisaReal,
                F.impuesto_ventas AS Impuesto,
                F.CREACION_USER,
		        F.CREACION_FECHA,
                dbo.fxCxPSaldoFacturaCorte(
                    F.cod_proveedor,
                    F.cod_factura,
                    dbo.MyGetdate()
                ) AS Saldo,
                F.cod_forma_pago AS FormaPago,
                F.notas AS Notas
            FROM cxp_facturas F
            INNER JOIN cxp_proveedores P
                ON F.cod_proveedor = P.cod_proveedor
            WHERE F.cod_factura = @factura
              AND F.cod_proveedor = @proveedor";

                var result = connection.QueryFirstOrDefault<CrdPolizaFacturaVerFacturaResponse>(
                    query,
                    new
                    {
                        proveedor,
                        factura
                    });

                if (result is null)
                {
                    return DbHelper.CreateErrorResponse<CrdPolizaFacturaVerFacturaResponse>(
                        "No se encontró la factura consultada.",
                        -2,
                        new CrdPolizaFacturaVerFacturaResponse());
                }

                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<CrdPolizaFacturaVerFacturaResponse>(
                    "Error al consultar la factura.",
                    -1,
                    new CrdPolizaFacturaVerFacturaResponse());
            }
        }

        /// <summary>
        /// Obtiene líneas contables de la factura.
        /// </summary>
        public ErrorDto<CrdPolizaFacturaVerAsientosResponse> CrdPolizaFacturaVer_Asientos_Obtener(
         int codEmpresa,
         int proveedor,
         string factura)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                const string query = @"
            SELECT
                  C.cod_cuenta_mask AS CuentaMask,	 
	                D.cod_unidad AS CodUnidad,
	                D.cod_centro_costo AS CodCentroCosto,
                    C.descripcion AS CuentaDescripcion,
                    U.descripcion AS Unidad,
                    X.descripcion AS CentroCosto,
                    Div.cod_divisa AS CodDivisa,
	                Div.Descripcion as Divisa,
                    D.tipo_cambio AS TipoCambio,
                    CASE WHEN D.debehaber = 'D' THEN D.monto ELSE 0 END AS Debito,
                    CASE WHEN D.debehaber = 'C' THEN D.monto ELSE 0 END AS Credito
            FROM CXP_FACTURAS_DETALLE D
            INNER JOIN CXP_FACTURAS Ch
                ON D.cod_factura = Ch.cod_factura
               AND D.cod_proveedor = Ch.cod_proveedor
            INNER JOIN CntX_Cuentas C
                ON D.cod_cuenta = C.cod_cuenta
               AND D.cod_contabilidad = C.cod_contabilidad
            INNER JOIN CntX_Divisas Div
                ON D.cod_divisa = Div.cod_divisa
               AND D.cod_contabilidad = Div.cod_contabilidad
            LEFT JOIN CntX_unidades U
                ON D.cod_unidad = U.cod_unidad
               AND U.cod_contabilidad = D.cod_contabilidad
            LEFT JOIN CNTX_CENTRO_COSTOS X
                ON D.cod_centro_costo = X.cod_centro_costo
               AND X.cod_contabilidad = @CodEmpresa
            WHERE D.cod_factura = @factura
              AND D.cod_proveedor = @proveedor
            ORDER BY D.linea;";

                var lineas = connection.Query<CrdPolizaFacturaVerAsientoModel>(
                    query,
                    new
                    {
                        CodEmpresa = codEmpresa,
                        proveedor,
                        factura
                    }).ToList();

                var response = new CrdPolizaFacturaVerAsientosResponse
                {
                    Lineas = lineas,
                    Totales = new CrdPolizaFacturaVerTotalesModel
                    {
                        Debito = lineas.Sum(x => x.Debito),
                        Credito = lineas.Sum(x => x.Credito),
                        Diferencia = lineas.Sum(x => x.Debito) - lineas.Sum(x => x.Credito)
                    }
                };

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<CrdPolizaFacturaVerAsientosResponse>(
                    "Error al obtener los asientos.",
                    -1,
                    new CrdPolizaFacturaVerAsientosResponse());
            }
        }

    }
}
