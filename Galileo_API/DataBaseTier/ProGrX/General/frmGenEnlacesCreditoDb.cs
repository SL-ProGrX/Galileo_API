using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier
{
    public class FrmGenEnlacesCreditoDb
    {
        private readonly PortalDB _portalDB;

        public FrmGenEnlacesCreditoDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        // ==========================
        // Helpers
        // ==========================

        private const string SqlEnlacesSelect = @"
SELECT
    I.cod_institucion,
    I.descripcion,
    ISNULL(P.cod_credito, '') AS cod_credito
";

        private const string SqlEnlacesFrom = @"
FROM instituciones I
INNER JOIN PV_PARINSTITUCIONES P
    ON I.cod_institucion = P.cod_institucion
WHERE
    (@filtro IS NULL)
 OR (CONVERT(varchar(20), I.cod_institucion) LIKE @like)
 OR (I.descripcion LIKE @like)
 OR (P.cod_credito LIKE @like)
";

        private static (string? filtro, string? like) BuildFiltroLike(FiltrosLazyLoadData? filtros)
        {
            var texto = filtros?.filtro?.Trim();
            if (string.IsNullOrWhiteSpace(texto))
                return (null, null);

            return (texto, $"%{texto}%");
        }

        private static (string sortField, string sortOrder) ResolveSort(FiltrosLazyLoadData? filtros)
        {
            // ORDER BY seguro (whitelist)
            string sortField = (filtros?.sortField ?? "").Trim().ToLowerInvariant() switch
            {
                "descripcion" => "I.descripcion",
                "cod_credito" => "P.cod_credito",
                _ => "I.cod_institucion"
            };

            string sortOrder = filtros?.sortOrder == 0 ? "DESC" : "ASC";
            return (sortField, sortOrder);
        }

        private static List<GenEnlacesCreditoData> QueryEnlaces(
            SqlConnection conn,
            FiltrosLazyLoadData? filtros,
            bool usarPaginacion,
            out int total)
        {
            var (filtro, like) = BuildFiltroLike(filtros);
            var (sortField, sortOrder) = ResolveSort(filtros);

            const string sqlCount = "SELECT COUNT(1) " + SqlEnlacesFrom + ";";
            total = conn.QuerySingle<int>(sqlCount, new { filtro, like });

            var sqlList = SqlEnlacesSelect + SqlEnlacesFrom + $"\nORDER BY {sortField} {sortOrder}";

            int offset = filtros?.pagina ?? 0;
            int fetch = filtros?.paginacion ?? 0;

            sqlList += usarPaginacion && fetch > 0
                ? "\nOFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;"
                : ";";

            return conn.Query<GenEnlacesCreditoData>(sqlList, new { filtro, like, offset, fetch }).ToList();
        }

        // ==========================
        // Públicos
        // ==========================

        /// <summary>
        /// Registra en PV_PARINSTITUCIONES las instituciones nuevas o no configuradas.
        /// Equivale al INSERT del Form_Load de frmGenEnlacesCredito.
        /// </summary>
        public ErrorDto Gen_EnlacesCredito_Sincronizar(int CodEmpresa)
        {
            const string sql = @"
INSERT INTO PV_PARINSTITUCIONES (COD_INSTITUCION, COD_CREDITO)
SELECT I.COD_INSTITUCION, ''
FROM INSTITUCIONES I
WHERE NOT EXISTS (SELECT 1 FROM PV_PARINSTITUCIONES P WHERE P.COD_INSTITUCION = I.COD_INSTITUCION);";

            return DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, sql);
        }

        /// <summary>
        /// Obtiene la lista paginada de enlaces institución - crédito.
        /// Equivale a sbCargaLsw de frmGenEnlacesCredito.
        /// </summary>
        public ErrorDto<GenEnlacesCreditoLista> Gen_EnlacesCreditoLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                bool usarPaginacion = (filtros?.paginacion ?? 0) > 0;
                var lista = QueryEnlaces(conn, filtros, usarPaginacion, out int total);

                return DbHelper.CreateOkResponse(new GenEnlacesCreditoLista { total = total, lista = lista });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<GenEnlacesCreditoLista>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene los enlaces sin paginación (exportación a PDF y Excel).
        /// </summary>
        public ErrorDto<List<GenEnlacesCreditoData>> Gen_EnlacesCredito_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var lista = QueryEnlaces(conn, filtros, usarPaginacion: false, out _);
                return DbHelper.CreateOkResponse(lista);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<List<GenEnlacesCreditoData>>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene las líneas de crédito del catálogo para una institución.
        /// Equivale a la búsqueda F4 de txtCodCredito.
        /// catalogo.cod_institucion es varchar (ver frmCR_CatalogoCreditosDB), por lo que se
        /// compara de forma segura para no depender de ceros a la izquierda ni del tipo.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Gen_EnlacesCreditoCatalogo_Obtener(int CodEmpresa, int cod_institucion)
        {
            const string sql = @"
SELECT codigo AS item, descripcion
FROM catalogo
WHERE TRY_CAST(cod_institucion AS int) = @cod_institucion
ORDER BY descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDB, CodEmpresa, sql, new { cod_institucion });
        }

        /// <summary>
        /// Actualiza la línea de crédito asignada a una institución.
        /// Equivale al Enter de txtCodCredito (permite código vacío, igual que VB6).
        /// </summary>
        public ErrorDto Gen_EnlacesCredito_Actualizar(int CodEmpresa, GenEnlacesCreditoData enlace)
        {
            if (enlace == null)
                return DbHelper.ErrorResponse("El enlace es requerido.", -2);

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
UPDATE PV_PARINSTITUCIONES
SET cod_credito = @cod_credito
WHERE cod_institucion = @cod_institucion;";

                int filas = conn.Execute(sql, new
                {
                    cod_credito = (enlace.cod_credito ?? string.Empty).Trim(),
                    enlace.cod_institucion
                });

                return filas == 0
                    ? DbHelper.ErrorResponse($"La institución {enlace.cod_institucion} no existe en los enlaces.", -2)
                    : DbHelper.OkResponse("Enlace actualizado correctamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
    }
}
