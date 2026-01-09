using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
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
            var like = LikeOrNull(filtro);
            var (offset, fetch) = PagingOrAll(pagina, paginacion);

            const string sql = @"SELECT VAL_ID AS val_id,
       descripcion,
       Activo AS activo,
       COUNT(*) OVER() AS Total
  FROM CPR_VALORA_ESQUEMA
 WHERE (@F IS NULL OR VAL_ID LIKE @F OR descripcion LIKE @F)
 ORDER BY VAL_ID DESC
 OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

            return QueryPagedOver<CprValoraEsquemaDto, CprValoraEsquemaDtoList>(
                codEmpresa,
                sql,
                new { F = like, Offset = offset, Fetch = fetch },
                (total, rows) => new CprValoraEsquemaDtoList { Total = total, esquemas = rows },
                () => new CprValoraEsquemaDtoList { Total = 0, esquemas = new List<CprValoraEsquemaDto>() }
            );
        }

        public ErrorDto<CprValoraItemsDtoList> ValoracionItems_Obtener(int codEmpresa, string val_id, int? pagina, int? paginacion, string? filtro)
        {
            var like = LikeOrNull(filtro);
            var (offset, fetch) = PagingOrAll(pagina, paginacion);

            const string sql = @"SELECT VAL_ITEM AS val_item,
       descripcion,
       Peso AS peso,
       COUNT(*) OVER() AS Total
  FROM CPR_VALORA_ITEMS
 WHERE VAL_ID = @ValId
   AND (@F IS NULL OR VAL_ITEM LIKE @F OR descripcion LIKE @F)
 ORDER BY VAL_ITEM
 OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

            return QueryPagedOver<CprValoraItemsDto, CprValoraItemsDtoList>(
                codEmpresa,
                sql,
                new { ValId = val_id, F = like, Offset = offset, Fetch = fetch },
                (total, rows) => new CprValoraItemsDtoList { Total = total, items = rows },
                () => new CprValoraItemsDtoList { Total = 0, items = new List<CprValoraItemsDto>() }
            );
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

            var r = DbHelper.ExecuteSingleQuery<string>(_portalDb, codEmpresa, mergeSql, "", p);
            var code = r.Code is int c ? c : -1;
            if (code != 0)
                return DbHelper.ErrorResponse(r.Description ?? ErrorMessage, code);

            var action = (r.Result ?? string.Empty).Trim();
            var msg = action.Equals("INSERT", StringComparison.OrdinalIgnoreCase)
                ? "Esquema agregado satisfactoriamente"
                : "Esquema actualizado satisfactoriamente";

            return DbHelper.OkResponse(msg);
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

            var r = DbHelper.ExecuteSingleQuery<string>(_portalDb, codEmpresa, mergeSql, "", p);
            var code = r.Code is int c ? c : -1;
            if (code != 0)
                return DbHelper.ErrorResponse(r.Description ?? ErrorMessage, code);

            var action = (r.Result ?? string.Empty).Trim();
            var msg = action.Equals("INSERT", StringComparison.OrdinalIgnoreCase)
                ? "Item agregado satisfactoriamente"
                : "Item actualizado satisfactoriamente";

            return DbHelper.OkResponse(msg);
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


        private static string? LikeOrNull(string? value)
        {
            var v = (value ?? string.Empty).Trim();
            return v.Length == 0 ? null : string.Concat("%", v, "%");
        }

        private static (int Offset, int Fetch) PagingOrAll(int? pagina, int? paginacion)
        {
            var off = pagina.GetValueOrDefault();
            if (off < 0) off = 0;

            var take = paginacion.GetValueOrDefault(int.MaxValue);
            if (take <= 0) take = int.MaxValue;

            return (off, take);
        }

        private ErrorDto<TList> QueryPagedOver<TDto, TList>(
            int codEmpresa,
            string sql,
            object param,
            Func<int, List<TDto>, TList> build,
            Func<TList> emptyFactory)
            where TList : class
        {
            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                if (conn.State != ConnectionState.Open) conn.Open();

                var total = 0;
                var rows = conn.Query<TDto, int, TDto>(
                    sql,
                    (dto, t) =>
                    {
                        total = t;
                        return dto;
                    },
                    param,
                    splitOn: "Total").ToList();

                if (rows.Count == 0)
                    total = 0;

                return build(total, rows);
            });

            var code = r.Code is int c ? c : -1;
            if (code != 0)
                return new ErrorDto<TList>
                {
                    Code = code,
                    Description = r.Description ?? ErrorMessage,
                    Result = null
                };

            return DbHelper.CreateOkResponse(r.Result ?? emptyFactory());
        }
    }
}