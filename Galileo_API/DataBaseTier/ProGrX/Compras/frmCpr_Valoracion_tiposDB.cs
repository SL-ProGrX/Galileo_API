using System.Data;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public class FrmCprValoracionTiposDB
    {
        private const string ErrorMessage = "Error";
        private static readonly IReadOnlySet<string> EsquemaSortFields =
            new HashSet<string>(new[] { "val_id", "descripcion", "activo" }, StringComparer.OrdinalIgnoreCase);
        private static readonly IReadOnlySet<string> ItemSortFields =
            new HashSet<string>(new[] { "val_item", "descripcion", "peso" }, StringComparer.OrdinalIgnoreCase);
        private readonly PortalDB _portalDb;

        public FrmCprValoracionTiposDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        private sealed class EsquemaRow
        {
            // Populated by Dapper inside DbHelper
            public string val_id { get; set; } = string.Empty;
            public string descripcion { get; set; } = string.Empty;
            public int activo { get; set; } = 0;
            public int Total { get; set; } = 0;
        }

        private sealed class ItemRow
        {
            // Populated by Dapper inside DbHelper
            public string val_item { get; set; } = string.Empty;
            public string descripcion { get; set; } = string.Empty;
            public decimal peso { get; set; } = 0m;
            public int Total { get; set; } = 0;
        }

        private sealed class SearchOptions
        {
            public string? Like { get; init; }
            public int Offset { get; init; }
            public int Fetch { get; init; }
            public string SortField { get; init; } = string.Empty;
            public int SortOrder { get; init; }
        }

        /// <summary>
        /// Normaliza el filtro, la paginación y el orden solicitado contra una lista permitida.
        /// </summary>
        private static SearchOptions BuildSearch(
            CprValoraConsultaRequest? request,
            string defaultSortField,
            IReadOnlySet<string> allowedSortFields)
        {
            request ??= new CprValoraConsultaRequest();
            var requestedSortField = request.SortField ?? string.Empty;
            var sortField = allowedSortFields.Contains(requestedSortField)
                ? requestedSortField.ToLowerInvariant()
                : defaultSortField;

            var fetch = request.Paginacion.GetValueOrDefault(int.MaxValue);
            if (fetch <= 0) fetch = int.MaxValue;

            return new SearchOptions
            {
                Like = string.IsNullOrWhiteSpace(request.Filtro) ? null : $"%{request.Filtro.Trim()}%",
                Offset = Math.Max(request.Pagina.GetValueOrDefault(), 0),
                Fetch = fetch,
                SortField = sortField,
                SortOrder = request.SortOrder == 0 ? 0 : 1
            };
        }

        private ErrorDto MergeUpsert(int codEmpresa, string mergeSql, object param, string insertMsg, string updateMsg)
        {
            var r = DbHelper.ExecuteSingleQuery<string>(_portalDb, codEmpresa, mergeSql, string.Empty, param);
            var code = r.Code is int c ? c : -1;
            if (code != 0)
                return DbHelper.ErrorResponse(r.Description ?? ErrorMessage, code);

            var action = (r.Result ?? string.Empty).Trim();
            var msg = action.Equals("INSERT", StringComparison.OrdinalIgnoreCase) ? insertMsg : updateMsg;
            return DbHelper.OkResponse(msg);
        }

        /// <summary>
        /// Obtiene los esquemas de valoración filtrados, ordenados y paginados.
        /// </summary>
        public ErrorDto<CprValoraEsquemaDtoList> CPR_frmCpr_Valoracion_Tipos_EsquemaValoracion_Obtener(
            int codEmpresa,
            CprValoraConsultaRequest request)
        {
            var search = BuildSearch(request, "val_id", EsquemaSortFields);

            const string sql = @"SELECT VAL_ID AS val_id,
       descripcion,
       Activo AS activo,
       COUNT(*) OVER() AS Total
  FROM CPR_VALORA_ESQUEMA
 WHERE (@F IS NULL OR VAL_ID LIKE @F OR descripcion LIKE @F)
 ORDER BY
       CASE WHEN @SortField = 'val_id' AND @SortOrder = 1 THEN VAL_ID END ASC,
       CASE WHEN @SortField = 'val_id' AND @SortOrder = 0 THEN VAL_ID END DESC,
       CASE WHEN @SortField = 'descripcion' AND @SortOrder = 1 THEN descripcion END ASC,
       CASE WHEN @SortField = 'descripcion' AND @SortOrder = 0 THEN descripcion END DESC,
       CASE WHEN @SortField = 'activo' AND @SortOrder = 1 THEN Activo END ASC,
       CASE WHEN @SortField = 'activo' AND @SortOrder = 0 THEN Activo END DESC,
       VAL_ID ASC
 OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

            var rowsResp = DbHelper.ExecuteListQuery<EsquemaRow>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    F = search.Like,
                    search.Offset,
                    search.Fetch,
                    search.SortField,
                    search.SortOrder
                }
            );

            var code = rowsResp.Code is int c ? c : -1;
            if (code != 0)
                return DbHelper.CreateErrorResponse<CprValoraEsquemaDtoList>(rowsResp.Description ?? ErrorMessage, code, null!);

            var rows = rowsResp.Result ?? new List<EsquemaRow>();
            var total = rows.Count == 0 ? 0 : rows[0].Total;

            var dtoRows = rows.Select(r => new CprValoraEsquemaDto
            {
                val_id = r.val_id,
                descripcion = r.descripcion,
                activo = r.activo == 1
            }).ToList();

            return DbHelper.CreateOkResponse(new CprValoraEsquemaDtoList
            {
                Total = total,
                esquemas = dtoRows
            });
        }

        /// <summary>
        /// Obtiene los ítems de un esquema de valoración filtrados, ordenados y paginados.
        /// </summary>
        public ErrorDto<CprValoraItemsDtoList> CPR_frmCpr_Valoracion_Tipos_ValoracionItems_Obtener(
            int codEmpresa,
            string val_id,
            CprValoraConsultaRequest request)
        {
            var search = BuildSearch(request, "val_item", ItemSortFields);

            const string sql = @"SELECT VAL_ITEM AS val_item,
       descripcion,
       Peso AS peso,
       COUNT(*) OVER() AS Total
  FROM CPR_VALORA_ITEMS
 WHERE VAL_ID = @ValId
   AND (@F IS NULL OR VAL_ITEM LIKE @F OR descripcion LIKE @F)
 ORDER BY
       CASE WHEN @SortField = 'val_item' AND @SortOrder = 1 THEN VAL_ITEM END ASC,
       CASE WHEN @SortField = 'val_item' AND @SortOrder = 0 THEN VAL_ITEM END DESC,
       CASE WHEN @SortField = 'descripcion' AND @SortOrder = 1 THEN descripcion END ASC,
       CASE WHEN @SortField = 'descripcion' AND @SortOrder = 0 THEN descripcion END DESC,
       CASE WHEN @SortField = 'peso' AND @SortOrder = 1 THEN Peso END ASC,
       CASE WHEN @SortField = 'peso' AND @SortOrder = 0 THEN Peso END DESC,
       VAL_ITEM ASC
 OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

            var rowsResp = DbHelper.ExecuteListQuery<ItemRow>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    ValId = val_id,
                    F = search.Like,
                    search.Offset,
                    search.Fetch,
                    search.SortField,
                    search.SortOrder
                }
            );

            var code = rowsResp.Code is int c ? c : -1;
            if (code != 0)
                return DbHelper.CreateErrorResponse<CprValoraItemsDtoList>(rowsResp.Description ?? ErrorMessage, code, null!);

            var rows = rowsResp.Result ?? new List<ItemRow>();
            var total = rows.Count == 0 ? 0 : rows[0].Total;

            var dtoRows = rows.Select(r => new CprValoraItemsDto
            {
                val_item = r.val_item,
                descripcion = r.descripcion,
                peso = r.peso
            }).ToList();

            return DbHelper.CreateOkResponse(new CprValoraItemsDtoList
            {
                Total = total,
                items = dtoRows
            });
        }

        public ErrorDto EsquemaValoracion_Upsert(int codEmpresa, string usuario, CprValoraEsquemaDto request)
        {
            var p = new
            {
                ValId = request.val_id,
                Descripcion = request.descripcion,
                Activo = request.activo ? 1 : 0,
                Usuario = usuario
            };

            const string mergeSql = @"MERGE CPR_VALORA_ESQUEMA AS T
USING (SELECT @ValId AS VAL_ID) AS S
ON (T.VAL_ID = S.VAL_ID)
WHEN MATCHED THEN
    UPDATE SET descripcion = @Descripcion,
               Activo = @Activo,
               Modifica_Fecha = GETDATE(),
               Modifica_Usuario = @Usuario
WHEN NOT MATCHED THEN
    INSERT (VAL_ID, descripcion, Activo, Registro_Fecha, Registro_Usuario)
    VALUES (@ValId, @Descripcion, @Activo, GETDATE(), @Usuario)
OUTPUT $action;";

            return MergeUpsert(
                codEmpresa,
                mergeSql,
                p,
                "Esquema agregado satisfactoriamente",
                "Esquema actualizado satisfactoriamente"
            );
        }

        public ErrorDto EsquemaValoracion_Delete(int codEmpresa, string val_id)
        {
            const string sql = @"BEGIN TRY
    BEGIN TRAN;
    DELETE FROM CPR_VALORA_ITEMS WHERE VAL_ID = @ValId;
    DELETE FROM CPR_VALORA_ESQUEMA WHERE VAL_ID = @ValId;
    COMMIT TRAN;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    THROW;
END CATCH";

            var r = DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, new { ValId = val_id });
            var code = r.Code is int c ? c : -1;
            return code == 0
                ? DbHelper.OkResponse("Esquema eliminado satisfactoriamente")
                : DbHelper.ErrorResponse(r.Description ?? ErrorMessage, code);
        }

        public ErrorDto ValoracionItems_Upsert(int codEmpresa, string usuario, string val_id, CprValoraItemsDto request)
        {
            var p = new
            {
                Esquema = val_id,
                Item = request.val_item,
                Descripcion = request.descripcion,
                Peso = request.peso,
                Usuario = usuario
            };

            const string mergeSql = @"MERGE CPR_VALORA_ITEMS AS T
USING (SELECT @Esquema AS VAL_ID, @Item AS VAL_ITEM) AS S
ON (T.VAL_ID = S.VAL_ID AND T.VAL_ITEM = S.VAL_ITEM)
WHEN MATCHED THEN
    UPDATE SET descripcion = @Descripcion,
               Peso = @Peso,
               Modifica_Fecha = GETDATE(),
               Modifica_Usuario = @Usuario
WHEN NOT MATCHED THEN
    INSERT (VAL_ID, VAL_ITEM, descripcion, Peso, Registro_Fecha, Registro_Usuario)
    VALUES (@Esquema, @Item, @Descripcion, @Peso, GETDATE(), @Usuario)
OUTPUT $action;";

            return MergeUpsert(
                codEmpresa,
                mergeSql,
                p,
                "Item agregado satisfactoriamente",
                "Item actualizado satisfactoriamente"
            );
        }

        public ErrorDto ValoracionItems_Delete(int codEmpresa, string val_id, string val_item)
        {
            var r = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                @"DELETE FROM CPR_VALORA_ITEMS
                  WHERE VAL_ID = @ValId AND VAL_ITEM = @ValItem",
                new { ValId = val_id, ValItem = val_item }
            );

            var code = r.Code is int c ? c : -1;
            return code == 0
                ? DbHelper.OkResponse("Item eliminado satisfactoriamente")
                : DbHelper.ErrorResponse(r.Description ?? "Error eliminando item", code);
        }
    }
}
