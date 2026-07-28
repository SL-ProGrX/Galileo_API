using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del Pago de Productos de Beneficios (frmAF_BeneProdPago).
    /// Consultas aquí; proceso de entrega/actualización en el parcial .Guardar.
    /// </summary>
    public partial class FrmAfBeneProdPagoDB
    {
        private readonly IConfiguration _config;
        private readonly MBeneficiosDB _mBeneficiosDB;

        /// <summary>
        /// Inicializa el acceso a datos y la bitácora de beneficios con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneProdPagoDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mBeneficiosDB = new MBeneficiosDB(config);
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene la lista de productos asignados pendientes de entrega para un beneficio.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_beneficio">Código del beneficio.</param>
        /// <param name="pagina">Offset de paginación.</param>
        /// <param name="paginacion">Cantidad de registros por página.</param>
        /// <param name="filtro">Filtro por consecutivo, cédula o nombre.</param>
        /// <returns>Lista de productos asignados y total.</returns>
        public ErrorDto<AfiBeneProdAsgDataList> AfiBeneProdAsgLista_Obtener(int CodCliente, string cod_beneficio, int? pagina, int? paginacion, string? filtro)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new AfiBeneProdAsgDataList();

                const string sqlCount = @"SELECT COUNT(A.cod_beneficio)
                                          FROM afi_bene_prodasg A
                                          LEFT JOIN AFI_BENE_PAGO P ON A.cod_beneficio = P.cod_Beneficio AND A.Consec = P.Consec
                                          LEFT JOIN afi_bene_otorga O ON A.cod_beneficio = O.cod_Beneficio AND A.Consec = O.Consec
                                          LEFT JOIN Socios S ON O.cedula = S.cedula
                                          WHERE O.estado IN (SELECT COD_ESTADO FROM AFI_BENE_ESTADOS WHERE P_FINALIZA = 1 AND PROCESO IN ('A'))
                                            AND A.cod_beneficio = @cod_beneficio AND P.ESTADO != 'E'";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount, new { cod_beneficio });

                var like = string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro}%";
                var offset = pagina ?? 0;
                var fetch = paginacion ?? 10;

                const string sql = @"SELECT
                                        ROW_NUMBER() OVER (ORDER BY A.REGISTRO_FECHA DESC) AS linea,
                                        CONCAT(FORMAT(O.ID_BENEFICIO, '00000'), TRIM(A.COD_BENEFICIO), FORMAT(A.consec, '00000')) AS expediente,
                                        A.COD_PRODUCTO AS Cod_Producto, A.cod_beneficio, O.cedula AS Cedula, ISNULL(S.nombre, '') AS Nombre,
                                        1 AS cantidad, A.costo_unidad AS Monto, A.REGISTRO_FECHA AS registro_fecha, A.CONSEC AS Consec,
                                        O.ID_BENEFICIO AS id_beneficio,
                                        (SELECT DESCRIPCION FROM AFI_BENE_PRODUCTOS WHERE COD_PRODUCTO = A.COD_PRODUCTO) AS ProductoDesc,
                                        (SELECT TARJETA_REGALO FROM AFI_BENE_PRODUCTOS WHERE COD_PRODUCTO = A.COD_PRODUCTO) AS tarjeta,
                                        P.ID_PAGO AS id_pago,
                                        COALESCE((SELECT NO_TARJETA FROM AFI_BENE_TARJETAS_REGALO
                                                  WHERE COD_PRODUCTO = P.COD_PRODUCTO AND COD_BENEFICIO = P.COD_BENEFICIO
                                                    AND CONSEC = P.CONSEC AND ID_PAGO = P.ID_PAGO), NULL) AS noTarjeta
                                     FROM afi_bene_prodasg A
                                     LEFT JOIN AFI_BENE_PAGO P ON A.cod_beneficio = P.cod_Beneficio AND A.Consec = P.Consec
                                     LEFT JOIN afi_bene_otorga O ON A.cod_beneficio = O.cod_Beneficio AND A.Consec = O.Consec
                                     LEFT JOIN Socios S ON O.cedula = S.cedula
                                     WHERE O.estado IN (SELECT COD_ESTADO FROM AFI_BENE_ESTADOS WHERE P_FINALIZA = 1 AND PROCESO IN ('A'))
                                       AND A.cod_beneficio = @cod_beneficio AND P.ESTADO != 'E'
                                       AND (@like IS NULL OR A.consec LIKE @like OR O.cedula LIKE @like OR S.nombre LIKE @like)
                                     ORDER BY A.cod_beneficio
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.Beneficios = connection.Query<AfiBeneProdAsgData>(sql, new { cod_beneficio, like, offset, fetch }).ToList();
                return response;
            });
        }

        /// <summary>
        /// Obtiene los beneficios que tienen productos asignados pendientes de pago.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de beneficios.</returns>
        public ErrorDto<List<AfiBeneProdData>> AfiBeneficios_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT RTRIM(cod_Beneficio) AS cod_Beneficio, RTRIM(descripcion) AS descripcion
                                     FROM afi_beneficios
                                     WHERE cod_beneficio IN (SELECT cod_beneficio FROM afi_bene_prodasg)
                                       AND cod_beneficio IN (SELECT COD_BENEFICIO FROM AFI_BENE_PAGO WHERE ESTADO != 'E' AND TIPO = 'P')";
                return connection.Query<AfiBeneProdData>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene el detalle de productos asignados a un beneficio y consecutivo.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="consec">Consecutivo del beneficio.</param>
        /// <param name="cod_beneficio">Código del beneficio.</param>
        /// <returns>Detalle de productos asignados.</returns>
        public ErrorDto<List<AfiBeneProdAsgData>> AfiBeneProdAsg_Obtener(int CodCliente, string consec, string cod_beneficio)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT B.*, P.descripcion AS ProductoDesc
                                     FROM afi_bene_prodasg B
                                     INNER JOIN afi_bene_productos P ON B.cod_producto = P.cod_Producto
                                     WHERE consec = @consec AND cod_beneficio = @cod_beneficio";
                return connection.Query<AfiBeneProdAsgData>(sql, new { consec, cod_beneficio }).ToList();
            });
        }
    }
}
