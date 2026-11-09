using Dapper;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneProdPagoDB
    {
        private const int CodigoValidacion = -2;
        private const string MensajeCodigoBeneficioRequerido =
            "El c&oacute;digo del beneficio es requerido.";
        private const string MensajeConsecutivoRequerido =
            "El consecutivo del beneficio es requerido.";

        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos de frmAF_BeneProdPago.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneProdPagoDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene los beneficios que poseen productos asignados.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Catálogo de beneficios.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            AF_BeneProdPago_Beneficios_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(
                AF_BeneProdPago_Portal_Crear(),
                CodEmpresa,
                connection =>
                {
                    const string sql = """
                        SELECT
                            RTRIM(cod_Beneficio) AS item,
                            RTRIM(descripcion) AS descripcion
                        FROM afi_beneficios
                        WHERE cod_beneficio IN
                        (
                            SELECT cod_beneficio
                            FROM afi_bene_prodasg
                        )
                        """;

                    return connection
                        .Query<DropDownListaGenericaModel>(sql)
                        .ToList();
                });
        }

        /// <summary>
        /// Obtiene los beneficios asignados pendientes de entrega.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Filtros y paginación de la consulta.</param>
        /// <returns>Beneficios asignados y total de registros.</returns>
        public ErrorDto<AfiBeneProdAsgDataList>
            AF_BeneProdPago_Lista_Obtener(
                int CodEmpresa,
                AfiBeneProdPagoListaRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.cod_beneficio))
            {
                return DbHelper.CreateErrorResponse(
                    MensajeCodigoBeneficioRequerido,
                    CodigoValidacion,
                    new AfiBeneProdAsgDataList());
            }

            var codBeneficio = request.cod_beneficio.Trim();
            var pagina = Math.Max(request.pagina, 0);
            var paginacion = request.paginacion <= 0
                ? 30
                : Math.Min(request.paginacion, 500);

            var filtro = string.IsNullOrWhiteSpace(request.filtro)
                ? null
                : $"%{request.filtro.Trim()}%";

            var ordenSql = AF_BeneProdPago_Orden_Crear(
                request.sortField,
                request.sortOrder);

            return DbHelper.WithConn(
                AF_BeneProdPago_Portal_Crear(),
                CodEmpresa,
                connection =>
                {
                    const string sqlBase = """
                        SELECT
                            A.consec,
                            A.cod_beneficio,
                            O.cedula,
                            ISNULL(S.nombre, '') AS nombre,
                            SUM(A.cantidad) AS cantidad,
                            SUM(A.costo_unidad * A.cantidad) AS monto
                        INTO #beneficios
                        FROM afi_bene_prodasg A
                        LEFT JOIN afi_bene_otorga O
                            ON A.cod_beneficio = O.cod_Beneficio
                            AND A.consec = O.consec
                        LEFT JOIN socios S
                            ON O.cedula = S.cedula
                        WHERE O.estado = 'S'
                            AND A.cod_beneficio = @codBeneficio
                        GROUP BY
                            A.consec,
                            A.cod_beneficio,
                            O.cedula,
                            S.nombre;

                        SELECT COUNT(*)
                        FROM #beneficios
                        WHERE
                            @filtro IS NULL
                            OR CONVERT(VARCHAR(20), consec) LIKE @filtro
                            OR cod_beneficio LIKE @filtro
                            OR cedula LIKE @filtro
                            OR nombre LIKE @filtro;

                        SELECT
                            consec,
                            cod_beneficio,
                            cedula,
                            nombre,
                            cantidad,
                            monto
                        FROM #beneficios
                        WHERE
                            @filtro IS NULL
                            OR CONVERT(VARCHAR(20), consec) LIKE @filtro
                            OR cod_beneficio LIKE @filtro
                            OR cedula LIKE @filtro
                            OR nombre LIKE @filtro
                        ORDER BY {ORDEN}
                        OFFSET @pagina ROWS
                        FETCH NEXT @paginacion ROWS ONLY;
                        """;

                    var sql = sqlBase.Replace("{ORDEN}", ordenSql);

                    using var resultados = connection.QueryMultiple(
                        sql,
                        new
                        {
                            codBeneficio,
                            filtro,
                            pagina,
                            paginacion
                        });

                    return new AfiBeneProdAsgDataList
                    {
                        total = resultados.ReadFirst<int>(),
                        beneficios = resultados
                            .Read<AfiBeneProdAsgData>()
                            .ToList()
                    };
                });
        }

        /// <summary>
        /// Obtiene el detalle de productos de un beneficio asignado.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="consec">Consecutivo del beneficio.</param>
        /// <param name="cod_beneficio">Código del beneficio.</param>
        /// <returns>Detalle de productos asignados.</returns>
        public ErrorDto<List<AfiBeneProdDetalleData>>
            AF_BeneProdPago_Detalle_Obtener(
                int CodEmpresa,
                int consec,
                string cod_beneficio)
        {
            if (consec <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeConsecutivoRequerido,
                    CodigoValidacion,
                    new List<AfiBeneProdDetalleData>());
            }

            if (string.IsNullOrWhiteSpace(cod_beneficio))
            {
                return DbHelper.CreateErrorResponse(
                    MensajeCodigoBeneficioRequerido,
                    CodigoValidacion,
                    new List<AfiBeneProdDetalleData>());
            }

            var codBeneficio = cod_beneficio.Trim();

            return DbHelper.WithConn(
                AF_BeneProdPago_Portal_Crear(),
                CodEmpresa,
                connection =>
                {
                    const string sql = """
                        SELECT
                            B.consec,
                            B.cod_beneficio,
                            B.cod_producto,
                            P.descripcion AS producto_desc,
                            B.cantidad,
                            B.costo_unidad
                        FROM afi_bene_prodasg B
                        INNER JOIN afi_bene_productos P
                            ON B.cod_producto = P.cod_producto
                        WHERE B.consec = @consec
                            AND B.cod_beneficio = @codBeneficio
                        """;

                    return connection
                        .Query<AfiBeneProdDetalleData>(
                            sql,
                            new
                            {
                                consec,
                                codBeneficio
                            })
                        .ToList();
                });
        }

        /// <summary>
        /// Crea la cláusula ORDER BY a partir de una lista cerrada de columnas.
        /// </summary>
        /// <param name="sortField">Columna solicitada.</param>
        /// <param name="sortOrder">1 ascendente, -1 descendente.</param>
        /// <returns>Cláusula de ordenamiento segura.</returns>
        private static string AF_BeneProdPago_Orden_Crear(
            string? sortField,
            int sortOrder)
        {
            var columna = (sortField ?? string.Empty)
                .Trim()
                .ToLowerInvariant() switch
            {
                "cod_beneficio" => "cod_beneficio",
                "cedula" => "cedula",
                "nombre" => "nombre",
                "cantidad" => "cantidad",
                "monto" => "monto",
                _ => "consec"
            };

            var direccion = sortOrder == -1 ? "DESC" : "ASC";

            return columna == "consec"
                ? $"consec {direccion}"
                : $"{columna} {direccion}, consec ASC";
        }

        /// <summary>
        /// Crea el acceso a la conexión de empresas.
        /// </summary>
        /// <returns>Instancia de acceso al portal.</returns>
        private PortalDB AF_BeneProdPago_Portal_Crear()
        {
            return new PortalDB(_config);
        }
    }
}