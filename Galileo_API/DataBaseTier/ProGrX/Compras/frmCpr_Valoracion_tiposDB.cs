using System.Data;
using Dapper;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public class FrmCprValoraciontiposDB
    {
        private const string ErrorMessage = "Error";
        private readonly PortalDB _portalDb;

        public FrmCprValoraciontiposDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        public ErrorDto<CprValoraEsquemaDtoList> EsquemaValoracion_Obtener(int codEmpresa, int? pagina, int? paginacion, string? filtro)
        {
            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var like = NormalizeLike(filtro);
                var (offset, fetch) = NormalizePaging(pagina, paginacion);

                const string totalSql = @"SELECT COUNT(*)
FROM CPR_VALORA_ESQUEMA
WHERE (@F IS NULL OR VAL_ID LIKE @F OR descripcion LIKE @F);";

                var total = conn.QueryFirstOrDefault<int>(
                    totalSql,
                    new { F = like }
                );

                const string listSql = @"SELECT VAL_ID, descripcion, Activo
FROM CPR_VALORA_ESQUEMA
WHERE (@F IS NULL OR VAL_ID LIKE @F OR descripcion LIKE @F)
ORDER BY VAL_ID DESC
OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

                var esquemas = conn.Query<CprValoraEsquemaDto>(
                    listSql,
                    new { F = like, Offset = offset, Fetch = fetch }
                ).ToList();

                return new CprValoraEsquemaDtoList { Total = total, esquemas = esquemas };
            });

            if (r.Code != 0)
                return DbHelper.CreateErrorResponse<CprValoraEsquemaDtoList>(r.Description ?? ErrorMessage, r.Code ?? -1, null!);

            return DbHelper.CreateOkResponse(r.Result ?? new CprValoraEsquemaDtoList { Total = 0, esquemas = new List<CprValoraEsquemaDto>() });
        }

        public ErrorDto<CprValoraItemsDtoList> ValoracionItems_Obtener(int codEmpresa, string val_id, int? pagina, int? paginacion, string? filtro)
        {
            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var like = NormalizeLike(filtro);
                var (offset, fetch) = NormalizePaging(pagina, paginacion);

                const string totalSql = @"SELECT COUNT(*)
FROM CPR_VALORA_ITEMS
WHERE VAL_ID = @ValId
  AND (@F IS NULL OR VAL_ITEM LIKE @F OR descripcion LIKE @F);";

                var total = conn.QueryFirstOrDefault<int>(
                    totalSql,
                    new { ValId = val_id, F = like }
                );

                const string listSql = @"SELECT VAL_ITEM, descripcion, Peso
FROM CPR_VALORA_ITEMS
WHERE VAL_ID = @ValId
  AND (@F IS NULL OR VAL_ITEM LIKE @F OR descripcion LIKE @F)
ORDER BY VAL_ITEM
OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

                var items = conn.Query<CprValoraItemsDto>(
                    listSql,
                    new { ValId = val_id, F = like, Offset = offset, Fetch = fetch }
                ).ToList();

                return new CprValoraItemsDtoList { Total = total, items = items };
            });

            if (r.Code != 0)
                return DbHelper.CreateErrorResponse<CprValoraItemsDtoList>(r.Description ?? ErrorMessage, r.Code ?? -1, null!);

            return DbHelper.CreateOkResponse(r.Result ?? new CprValoraItemsDtoList { Total = 0, items = new List<CprValoraItemsDto>() });
        }

        public ErrorDto EsquemaValoracion_Upsert(int codEmpresa, string usuario, CprValoraEsquemaDto request)
        {
            var activo = request.activo ? 1 : 0;

            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var existe = conn.QueryFirstOrDefault<int>(
                    @"SELECT ISNULL(COUNT(*),0)
                      FROM CPR_VALORA_ESQUEMA
                      WHERE VAL_ID = @ValId",
                    new { ValId = request.val_id }
                ) > 0;

                if (!existe)
                {
                    conn.Execute(
                        @"INSERT INTO CPR_VALORA_ESQUEMA
                          (VAL_ID, descripcion, Activo, Registro_Fecha, Registro_Usuario)
                          VALUES
                          (@ValId, @Descripcion, @Activo, GETDATE(), @Usuario)",
                        new
                        {
                            ValId = request.val_id,
                            Descripcion = request.descripcion,
                            Activo = activo,
                            Usuario = usuario
                        }
                    );
                    return "Esquema agregado satisfactoriamente";
                }

                conn.Execute(
                    @"UPDATE CPR_VALORA_ESQUEMA
                      SET descripcion = @Descripcion,
                          Activo = @Activo,
                          Modifica_Fecha = GETDATE(),
                          Modifica_Usuario = @Usuario
                      WHERE VAL_ID = @ValId",
                    new
                    {
                        ValId = request.val_id,
                        Descripcion = request.descripcion,
                        Activo = activo,
                        Usuario = usuario
                    }
                );
                return "Esquema actualizado satisfactoriamente";
            });

            return r.Code == 0
                ? DbHelper.OkResponse(r.Result ?? "OK")
                : DbHelper.ErrorResponse(r.Description ?? ErrorMessage, r.Code ?? -1);
        }

        public ErrorDto EsquemaValoracion_Delete(int codEmpresa, string val_id)
        {
            // 2 deletes => transacción (para que no queden huérfanos)
            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                if (conn.State != ConnectionState.Open) conn.Open();
                using var tx = conn.BeginTransaction();

                try
                {
                    conn.Execute(
                        @"DELETE FROM CPR_VALORA_ITEMS WHERE VAL_ID = @ValId",
                        new { ValId = val_id },
                        transaction: tx
                    );

                    conn.Execute(
                        @"DELETE FROM CPR_VALORA_ESQUEMA WHERE VAL_ID = @ValId",
                        new { ValId = val_id },
                        transaction: tx
                    );

                    tx.Commit();
                    return true;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            });

            return r.Code == 0
                ? DbHelper.OkResponse("Esquema eliminado satisfactoriamente")
                : DbHelper.ErrorResponse(r.Description ?? ErrorMessage, r.Code ?? -1);
        }

        public ErrorDto ValoracionItems_Upsert(int codEmpresa, string usuario, string val_id, CprValoraItemsDto request)
        {
            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var existe = conn.QueryFirstOrDefault<int>(
                    @"SELECT ISNULL(COUNT(*),0)
                      FROM CPR_VALORA_ITEMS
                      WHERE VAL_ID = @Esquema AND VAL_ITEM = @Item",
                    new { Esquema = val_id, Item = request.val_item }
                ) > 0;

                if (!existe)
                {
                    conn.Execute(
                        @"INSERT INTO CPR_VALORA_ITEMS
                          (VAL_ID, VAL_ITEM, descripcion, Peso, Registro_Fecha, Registro_Usuario)
                          VALUES
                          (@Esquema, @Item, @Descripcion, @Peso, GETDATE(), @Usuario)",
                        new
                        {
                            Esquema = val_id,
                            Item = request.val_item,
                            Descripcion = request.descripcion,
                            Peso = request.peso,
                            Usuario = usuario
                        }
                    );
                    return "Item agregado satisfactoriamente";
                }

                conn.Execute(
                    @"UPDATE CPR_VALORA_ITEMS
                      SET descripcion = @Descripcion,
                          Peso = @Peso,
                          Modifica_Fecha = GETDATE(),
                          Modifica_Usuario = @Usuario
                      WHERE VAL_ID = @Esquema AND VAL_ITEM = @Item",
                    new
                    {
                        Esquema = val_id,
                        Item = request.val_item,
                        Descripcion = request.descripcion,
                        Peso = request.peso,
                        Usuario = usuario
                    }
                );
                return "Item actualizado satisfactoriamente";
            });

            return r.Code == 0
                ? DbHelper.OkResponse(r.Result ?? "OK")
                : DbHelper.ErrorResponse(r.Description ?? "Error", r.Code ?? -1);
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

            return r.Code == 0
                ? DbHelper.OkResponse("Item eliminado satisfactoriamente")
                : DbHelper.ErrorResponse(r.Description ?? "Error eliminando item", r.Code ?? -1);
        }

        // ---------------- Helpers ----------------

        private static string? NormalizeLike(string? filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return null;

            var f = filtro.Trim();
            return f.Length == 0 ? null : $"%{f}%";
        }

        private static (int Offset, int Fetch) NormalizePaging(int? pagina, int? paginacion)
        {
            // Keep the existing meaning: `pagina` is treated as OFFSET.
            if (pagina is null || paginacion is null || pagina < 0 || paginacion <= 0)
                return (0, int.MaxValue);

            return (pagina.Value, paginacion.Value);
        }
    }
}