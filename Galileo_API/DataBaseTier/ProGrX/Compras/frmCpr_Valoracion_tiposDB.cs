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
            const string totalSql = @"SELECT COUNT(*)
                                FROM CPR_VALORA_ESQUEMA
                                WHERE (@F IS NULL OR VAL_ID LIKE @F OR descripcion LIKE @F);";

            const string listSql = @"SELECT VAL_ID, descripcion, Activo
                                FROM CPR_VALORA_ESQUEMA
                                WHERE (@F IS NULL OR VAL_ID LIKE @F OR descripcion LIKE @F)
                                ORDER BY VAL_ID DESC
                                OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

            return PagedQuery<CprValoraEsquemaDto, CprValoraEsquemaDtoList>(
                codEmpresa,
                new PagedQuerySpec<CprValoraEsquemaDto, CprValoraEsquemaDtoList>
                {
                    Filtro = filtro,
                    Pagina = pagina,
                    Paginacion = paginacion,
                    ExtraParams = null,
                    CountSql = totalSql,
                    ListSql = listSql,
                    Build = (total, rows) => new CprValoraEsquemaDtoList { Total = total, esquemas = rows },
                    EmptyFactory = () => new CprValoraEsquemaDtoList { Total = 0, esquemas = new List<CprValoraEsquemaDto>() }
                }
            );
        }

        public ErrorDto<CprValoraItemsDtoList> ValoracionItems_Obtener(int codEmpresa, string val_id, int? pagina, int? paginacion, string? filtro)
        {
            const string totalSql = @"SELECT COUNT(*)
                    FROM CPR_VALORA_ITEMS
                    WHERE VAL_ID = @ValId
                    AND (@F IS NULL OR VAL_ITEM LIKE @F OR descripcion LIKE @F);";

            const string listSql = @"SELECT VAL_ITEM, descripcion, Peso
                    FROM CPR_VALORA_ITEMS
                    WHERE VAL_ID = @ValId
                    AND (@F IS NULL OR VAL_ITEM LIKE @F OR descripcion LIKE @F)
                    ORDER BY VAL_ITEM
                    OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

            return PagedQuery<CprValoraItemsDto, CprValoraItemsDtoList>(
                codEmpresa,
                new PagedQuerySpec<CprValoraItemsDto, CprValoraItemsDtoList>
                {
                    Filtro = filtro,
                    Pagina = pagina,
                    Paginacion = paginacion,
                    ExtraParams = new { ValId = val_id },
                    CountSql = totalSql,
                    ListSql = listSql,
                    Build = (total, rows) => new CprValoraItemsDtoList { Total = total, items = rows },
                    EmptyFactory = () => new CprValoraItemsDtoList { Total = 0, items = new List<CprValoraItemsDto>() }
                }
            );
        }

        public ErrorDto EsquemaValoracion_Upsert(int codEmpresa, string usuario, CprValoraEsquemaDto request)
        {
            var activo = request.activo ? 1 : 0;

            const string existsSql = @"SELECT ISNULL(COUNT(*),0)
            FROM CPR_VALORA_ESQUEMA
            WHERE VAL_ID = @ValId";

            const string insertSql = @"INSERT INTO CPR_VALORA_ESQUEMA
            (VAL_ID, descripcion, Activo, Registro_Fecha, Registro_Usuario)
            VALUES
            (@ValId, @Descripcion, @Activo, GETDATE(), @Usuario)";

            const string updateSql = @"UPDATE CPR_VALORA_ESQUEMA
            SET descripcion = @Descripcion,
                Activo = @Activo,
                Modifica_Fecha = GETDATE(),
                Modifica_Usuario = @Usuario
            WHERE VAL_ID = @ValId";

            var p = new
            {
                ValId = request.val_id,
                Descripcion = request.descripcion,
                Activo = activo,
                Usuario = usuario
            };

            return UpsertMessage(
                codEmpresa,
                new UpsertSpec
                {
                    ExistsSql = existsSql,
                    ExistsParams = new { ValId = request.val_id },
                    InsertSql = insertSql,
                    InsertParams = p,
                    InsertMsg = "Esquema agregado satisfactoriamente",
                    UpdateSql = updateSql,
                    UpdateParams = p,
                    UpdateMsg = "Esquema actualizado satisfactoriamente"
                }
            );
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

            return OkOrError(r, "Esquema eliminado satisfactoriamente", ErrorMessage);
        }

        public ErrorDto ValoracionItems_Upsert(int codEmpresa, string usuario, string val_id, CprValoraItemsDto request)
        {
            const string existsSql = @"SELECT ISNULL(COUNT(*),0)
            FROM CPR_VALORA_ITEMS
            WHERE VAL_ID = @Esquema AND VAL_ITEM = @Item";

            const string insertSql = @"INSERT INTO CPR_VALORA_ITEMS
            (VAL_ID, VAL_ITEM, descripcion, Peso, Registro_Fecha, Registro_Usuario)
            VALUES
            (@Esquema, @Item, @Descripcion, @Peso, GETDATE(), @Usuario)";

            const string updateSql = @"UPDATE CPR_VALORA_ITEMS
            SET descripcion = @Descripcion,
                Peso = @Peso,
                Modifica_Fecha = GETDATE(),
                Modifica_Usuario = @Usuario
            WHERE VAL_ID = @Esquema AND VAL_ITEM = @Item";

            var p = new
            {
                Esquema = val_id,
                Item = request.val_item,
                Descripcion = request.descripcion,
                Peso = request.peso,
                Usuario = usuario
            };

            return UpsertMessage(
                codEmpresa,
                new UpsertSpec
                {
                    ExistsSql = existsSql,
                    ExistsParams = new { Esquema = val_id, Item = request.val_item },
                    InsertSql = insertSql,
                    InsertParams = p,
                    InsertMsg = "Item agregado satisfactoriamente",
                    UpdateSql = updateSql,
                    UpdateParams = p,
                    UpdateMsg = "Item actualizado satisfactoriamente"
                }
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

            return OkOrError(r, "Item eliminado satisfactoriamente", "Error eliminando item");
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

        // ---------------- Reuse helpers (anti-duplication) ----------------

        private sealed class PagedQuerySpec<TItem, TList> where TList : class
        {
            public string? Filtro { get; init; }
            public int? Pagina { get; init; }
            public int? Paginacion { get; init; }
            public object? ExtraParams { get; init; }
            public string CountSql { get; init; } = string.Empty;
            public string ListSql { get; init; } = string.Empty;
            public Func<int, List<TItem>, TList> Build { get; init; } = (_, __) => throw new InvalidOperationException("Build is required.");
            public Func<TList> EmptyFactory { get; init; } = () => throw new InvalidOperationException("EmptyFactory is required.");
        }

        private sealed class UpsertSpec
        {
            public string ExistsSql { get; init; } = string.Empty;
            public object ExistsParams { get; init; } = new { };
            public string InsertSql { get; init; } = string.Empty;
            public object InsertParams { get; init; } = new { };
            public string InsertMsg { get; init; } = string.Empty;
            public string UpdateSql { get; init; } = string.Empty;
            public object UpdateParams { get; init; } = new { };
            public string UpdateMsg { get; init; } = string.Empty;
        }

        private ErrorDto<TList> PagedQuery<TItem, TList>(int codEmpresa, PagedQuerySpec<TItem, TList> spec)
            where TList : class
        {
            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var like = NormalizeLike(spec.Filtro);
                var (offset, fetch) = NormalizePaging(spec.Pagina, spec.Paginacion);

                var dp = spec.ExtraParams == null ? new DynamicParameters() : new DynamicParameters(spec.ExtraParams);
                dp.Add("F", like);
                dp.Add("Offset", offset);
                dp.Add("Fetch", fetch);

                var total = conn.QueryFirstOrDefault<int>(spec.CountSql, dp);
                var rows = conn.Query<TItem>(spec.ListSql, dp).ToList();
                return spec.Build(total, rows);
            });

            var code = r.Code is int c ? c : -1;
            if (code != 0)
                return new ErrorDto<TList>
                {
                    Code = code,
                    Description = r.Description ?? ErrorMessage,
                    Result = null
                };

            return DbHelper.CreateOkResponse(r.Result ?? spec.EmptyFactory());
        }

        private ErrorDto UpsertMessage(int codEmpresa, UpsertSpec spec)
        {
            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var existe = conn.QueryFirstOrDefault<int>(spec.ExistsSql, spec.ExistsParams) > 0;

                if (!existe)
                {
                    conn.Execute(spec.InsertSql, spec.InsertParams);
                    return spec.InsertMsg;
                }

                conn.Execute(spec.UpdateSql, spec.UpdateParams);
                return spec.UpdateMsg;
            });

            var code = r.Code is int c ? c : -1;
            return code == 0
                ? DbHelper.OkResponse(r.Result ?? "OK")
                : DbHelper.ErrorResponse(r.Description ?? ErrorMessage, code);
        }

        private static ErrorDto OkOrError<T>(ErrorDto<T> r, string okMsg, string errMsg)
        {
            var code = r.Code is int c ? c : -1;
            return code == 0
                ? DbHelper.OkResponse(okMsg)
                : DbHelper.ErrorResponse(r.Description ?? errMsg, code);
        }

        private static ErrorDto OkOrError(ErrorDto r, string okMsg, string errMsg)
        {
            var code = r.Code is int c ? c : -1;
            return code == 0
                ? DbHelper.OkResponse(okMsg)
                : DbHelper.ErrorResponse(r.Description ?? errMsg, code);
        }
    }
}