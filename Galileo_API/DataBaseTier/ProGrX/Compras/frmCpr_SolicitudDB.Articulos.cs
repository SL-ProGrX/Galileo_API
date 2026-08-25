using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public partial class FrmCprSolicitudDB
    {
        // ===========================
        //  ARTICULOS PLAN
        // ===========================

        public ErrorDto<ArticuloDataLista> Articulos_Obtener(int codEmpresa, int? pagina, int? paginacion, string? filtro, string? cod_unidad)
        {
            var codUnidad = (cod_unidad ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(codUnidad))
                return DbHelper.CreateErrorResponse<ArticuloDataLista>("Debe indicar la unidad solicitante para consultar artículos del plan de compras");

            var parametros = CrearParametrosArticulos(filtro, pagina, paginacion);
            parametros.Add("CodUnidad", codUnidad);

            const string consultaBase = @"
                            SELECT DISTINCT
                                D.COD_PRODUCTO,
                                P.DESCRIPCION,
                                P.COSTO_REGULAR,
                                P.EXISTENCIA,
                                P.COD_BARRAS,
                                P.CABYS,
                                P.PRECIO_REGULAR,
                                P.IMPUESTO_VENTAS,
                                P.COD_FABRICANTE,
                                P.I_STOCK
                            FROM CPR_PLAN_DT D
                            INNER JOIN CPR_PLAN_COMPRAS C ON D.ID_PC = C.ID_PC
                                AND (RTRIM(LTRIM(C.COD_UNIDAD)) = @CodUnidad OR RTRIM(LTRIM(C.COD_UNIDAD_DESTINO)) = @CodUnidad)
                            INNER JOIN CPR_PLAN_DT_CORTES S ON D.ID_PLAN = S.ID_PLAN
                            INNER JOIN PV_PRODUCTOS P ON D.COD_PRODUCTO = P.COD_PRODUCTO
                            WHERE S.CORTE >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)
                              AND (
                                    NULLIF(@Filtro, '') IS NULL
                                    OR D.COD_PRODUCTO LIKE '%' + @Filtro + '%'
                                    OR P.DESCRIPCION LIKE '%' + @Filtro + '%'
                                  )";
            const string qCount = "SELECT COUNT(*) FROM (" + consultaBase + ") T;";
            const string qList = consultaBase + @"
                            ORDER BY D.COD_PRODUCTO
                            OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

            return CprSolicitud_ArticulosLista_Obtener(codEmpresa, parametros, qCount, qList);
        }

        /// <summary>
        /// Obtiene artículos generales desde PV_PRODUCTOS para búsqueda reutilizable.
        /// </summary>
        public ErrorDto<ArticuloDataLista> CprSolicitud_ArticulosGenerales_Obtener(int codEmpresa, int? pagina, int? paginacion, string? filtro)
        {
            var parametros = CrearParametrosArticulos(filtro, pagina, paginacion);

            const string consultaBase = @"
                            SELECT
                                P.COD_PRODUCTO,
                                P.CABYS,
                                P.DESCRIPCION,
                                P.COD_BARRAS,
                                P.TIPO_PRODUCTO AS TIPO,
                                P.PRECIO_COMPRA AS PRECIO_REGULAR,
                                P.EXISTENCIA,
                                CAST(0 AS DECIMAL(18,2)) AS IMPUESTO_VENTAS,
                                ISNULL(P.COD_UNIDAD, '') AS UNIDAD,
                                CAST('' AS VARCHAR(50)) AS CODIGO,
                                CAST('' AS VARCHAR(50)) AS COD_FABRICANTE,
                                CAST(0 AS BIT) AS I_STOCK
                            FROM PV_PRODUCTOS P
                            WHERE
                                NULLIF(@Filtro, '') IS NULL
                                OR P.COD_PRODUCTO LIKE '%' + @Filtro + '%'
                                OR P.DESCRIPCION LIKE '%' + @Filtro + '%'
                                OR P.COD_BARRAS LIKE '%' + @Filtro + '%'";
            const string qCount = "SELECT COUNT(*) FROM (" + consultaBase + ") T;";
            const string qList = consultaBase + @"
                            ORDER BY P.COD_PRODUCTO
                            OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

            return CprSolicitud_ArticulosLista_Obtener(codEmpresa, parametros, qCount, qList);
        }

        private ErrorDto<ArticuloDataLista> CprSolicitud_ArticulosLista_Obtener(
            int codEmpresa,
            DynamicParameters parametros,
            string consultaTotal,
            string consultaLista)
        {
            var r = DbHelper.WithConn<ArticuloDataLista>(_portalDb, codEmpresa, conn =>
            {
                EnsureOpen(conn);

                return new ArticuloDataLista
                {
                    Total = conn.Query<int>(consultaTotal, parametros).FirstOrDefault(),
                    Articulos = conn.Query<ArticuloData>(consultaLista, parametros).ToList()
                };
            });

            return WrapRequired(r);
        }

        private static DynamicParameters CrearParametrosArticulos(string? filtro, int? pagina, int? paginacion)
        {
            var paging = GetPaging(pagina, paginacion);
            var parametros = new DynamicParameters();
            parametros.Add("Filtro", (filtro ?? string.Empty).Trim());
            parametros.Add("Offset", paging.Offset);
            parametros.Add("Fetch", paging.Fetch);
            return parametros;
        }
    }
}
