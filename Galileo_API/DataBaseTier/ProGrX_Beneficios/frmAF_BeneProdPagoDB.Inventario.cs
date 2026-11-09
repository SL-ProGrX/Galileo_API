using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using System.Globalization;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneProdPagoDB
    {
        private const int LoteConsecutivos = 1000;
        private const int MaximoNotasBoleta = 200;
        private const string BodegaBeneficios = "Beneficios Solidarios";

        /// <summary>
        /// Obtiene los productos asignados a los beneficios seleccionados.
        /// </summary>
        private static List<AfiBeneProdEntregaProducto>
            AF_BeneProdPago_Productos_Obtener(
                SqlConnection connection,
                SqlTransaction transaction,
                List<EntregaClave> claves)
        {
            const string sql = """
                SELECT
                    B.cod_beneficio,
                    B.consec,
                    B.cod_producto,
                    B.cantidad,
                    B.costo_unidad,
                    P.cod_producto_inv,
                    CONCAT(
                        FORMAT(O.id_beneficio, '00000'),
                        TRIM(B.cod_beneficio),
                        FORMAT(B.consec, '00000')) AS expediente
                FROM afi_bene_prodasg B
                LEFT JOIN afi_bene_productos P
                    ON B.cod_producto = P.cod_producto
                LEFT JOIN afi_bene_otorga O
                    ON B.cod_beneficio = O.cod_beneficio
                    AND B.consec = O.consec
                WHERE B.cod_beneficio = @codBeneficio
                    AND B.consec IN @consecs
                """;

            var productos = new List<AfiBeneProdEntregaProducto>();

            foreach (var grupo in claves.GroupBy(
                clave => clave.CodBeneficio))
            {
                foreach (var lote in grupo
                    .Select(clave => clave.Consec)
                    .Chunk(LoteConsecutivos))
                {
                    productos.AddRange(
                        connection.Query<AfiBeneProdEntregaProducto>(
                            sql,
                            new
                            {
                                codBeneficio = grupo.Key,
                                consecs = lote
                            },
                            transaction));
                }
            }

            return productos;
        }

        /// <summary>
        /// Indica si el producto descuenta inventario.
        /// </summary>
        private static bool AF_BeneProdPago_Producto_Inventariable(
            AfiBeneProdEntregaProducto producto)
        {
            return !string.IsNullOrWhiteSpace(
                producto.cod_producto_inv);
        }

        /// <summary>
        /// Valida que exista inventario suficiente para la entrega.
        /// La cantidad se acumula por producto de inventario.
        /// </summary>
        /// <returns>Error de validación o null cuando hay existencia.</returns>
        private static ErrorDto? AF_BeneProdPago_Existencia_Validar(
            SqlConnection connection,
            SqlTransaction transaction,
            List<AfiBeneProdEntregaProducto> productos)
        {
            const string sql = """
                SELECT TOP 1 EXISTENCIA
                FROM PV_INVENTARIO
                WHERE cod_producto = @codProductoInv
                ORDER BY ANIO DESC, MES DESC, ENTRADA_FECHA DESC
                """;

            var insuficientes = new List<string>();

            var requeridos = productos
                .Where(AF_BeneProdPago_Producto_Inventariable)
                .GroupBy(producto => producto.cod_producto_inv!.Trim());

            foreach (var requerido in requeridos)
            {
                var cantidad = requerido.Sum(
                    producto => producto.cantidad);

                var existencia = connection
                    .QueryFirstOrDefault<decimal?>(
                        sql,
                        new { codProductoInv = requerido.Key },
                        transaction) ?? 0m;

                if (existencia >= cantidad) continue;

                var codigos = string.Join(
                    ", ",
                    requerido
                        .Select(producto => producto.cod_producto)
                        .Distinct());

                insuficientes.Add(
                    $"{codigos} (requerido: " +
                    $"{cantidad.ToString("0.##", CultureInfo.InvariantCulture)}, " +
                    $"disponible: " +
                    $"{existencia.ToString("0.##", CultureInfo.InvariantCulture)})");
            }

            if (insuficientes.Count == 0) return null;

            return DbHelper.ErrorResponse(
                "No hay suficiente existencia para el producto: " +
                string.Join("; ", insuficientes),
                CodigoValidacion);
        }

        /// <summary>
        /// Crea la boleta de salida de inventario con su detalle.
        /// No hace nada cuando ningún producto descuenta inventario.
        /// </summary>
        /// <returns>Error de validación o null cuando se creó la boleta.</returns>
        private static ErrorDto? AF_BeneProdPago_Boleta_Crear(
            SqlConnection connection,
            SqlTransaction transaction,
            string usuario,
            List<AfiBeneProdEntregaProducto> productos,
            EntregaResultado resultado)
        {
            var lineas = productos
                .Where(AF_BeneProdPago_Producto_Inventariable)
                .ToList();

            if (lineas.Count == 0) return null;

            var codBodega = connection.QueryFirstOrDefault<string>(
                """
                SELECT TOP 1 COD_BODEGA
                FROM PV_BODEGAS
                WHERE DESCRIPCION = @bodega
                """,
                new { bodega = BodegaBeneficios },
                transaction);

            if (string.IsNullOrWhiteSpace(codBodega))
            {
                return DbHelper.ErrorResponse(
                    $"No existe la bodega '{BodegaBeneficios}' " +
                    "para generar la salida de inventario.",
                    CodigoValidacion);
            }

            var boleta = AF_BeneProdPago_Boleta_Siguiente(
                connection,
                transaction);

            AF_BeneProdPago_Boleta_Encabezado_Insertar(
                connection,
                transaction,
                boleta,
                usuario,
                lineas);

            AF_BeneProdPago_Boleta_Detalle_Insertar(
                connection,
                transaction,
                boleta,
                codBodega.Trim(),
                lineas);

            resultado.Boleta = boleta;

            return null;
        }

        /// <summary>
        /// Obtiene el siguiente consecutivo de boletas de salida.
        /// </summary>
        private static string AF_BeneProdPago_Boleta_Siguiente(
            SqlConnection connection,
            SqlTransaction transaction)
        {
            const string sql = """
                SELECT CONVERT(bigint, ISNULL(MAX(boleta), '0')) + 1
                FROM pv_InvTranSac WITH (UPDLOCK, HOLDLOCK)
                WHERE tipo = 'S'
                """;

            var consecutivo = connection.QuerySingle<long>(
                sql,
                transaction: transaction);

            return consecutivo.ToString(
                "D10",
                CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Inserta el encabezado de la boleta de salida.
        /// </summary>
        private static void AF_BeneProdPago_Boleta_Encabezado_Insertar(
            SqlConnection connection,
            SqlTransaction transaction,
            string boleta,
            string usuario,
            List<AfiBeneProdEntregaProducto> lineas)
        {
            const string sql = """
                INSERT INTO PV_INVTRANSAC
                    (BOLETA, TIPO, COD_ENTSAL, FECHA, ESTADO,
                     FECHA_SISTEMA, NOTAS, DOCUMENTO, PLANTILLA,
                     GENERA_USER, GENERA_FECHA,
                     AUTORIZA_USER, AUTORIZA_FECHA,
                     PROCESA_USER, PROCESA_FECHA, TOTAL)
                VALUES
                    (@boleta, 'S', 'S', GETDATE(), 'A',
                     GETDATE(), @notas, 'ENTREGA BENEFICIO', 0,
                     @usuario, GETDATE(),
                     @usuario, GETDATE(),
                     @usuario, GETDATE(), @total)
                """;

            connection.Execute(
                sql,
                new
                {
                    boleta,
                    notas = AF_BeneProdPago_Boleta_Notas_Crear(lineas),
                    usuario,
                    total = lineas.Sum(
                        linea => linea.cantidad * linea.costo_unidad)
                },
                transaction);
        }

        /// <summary>
        /// Inserta el detalle de la boleta de salida.
        /// </summary>
        private static void AF_BeneProdPago_Boleta_Detalle_Insertar(
            SqlConnection connection,
            SqlTransaction transaction,
            string boleta,
            string codBodega,
            List<AfiBeneProdEntregaProducto> lineas)
        {
            const string sql = """
                INSERT INTO PV_INVTRADET
                    (LINEA, BOLETA, TIPO, COD_BODEGA, COD_PRODUCTO,
                     CANTIDAD, PRECIO, DESPACHO)
                VALUES
                    (@linea, @boleta, 'S', @codBodega, @codProducto,
                     @cantidad, @precio, @cantidad)
                """;

            var parametros = lineas.Select(
                (linea, indice) => new
                {
                    linea = indice + 1,
                    boleta,
                    codBodega,
                    codProducto = linea.cod_producto_inv!.Trim(),
                    cantidad = linea.cantidad,
                    precio = linea.costo_unidad
                });

            connection.Execute(sql, parametros, transaction);
        }

        /// <summary>
        /// Crea las notas de la boleta con los expedientes entregados.
        /// </summary>
        private static string AF_BeneProdPago_Boleta_Notas_Crear(
            List<AfiBeneProdEntregaProducto> lineas)
        {
            var expedientes = lineas
                .Select(linea => linea.expediente)
                .Where(expediente => !string.IsNullOrWhiteSpace(expediente))
                .Distinct()
                .ToList();

            var notas = "Expedientes: " + string.Join(", ", expedientes);

            return notas.Length <= MaximoNotasBoleta
                ? notas
                : $"Expedientes: {expedientes.Count} entregas " +
                  "(ver bitácora de beneficios)";
        }
    }
}
