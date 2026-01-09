using Dapper;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmCprSuspensionTiposDB
    {
        private readonly PortalDB _portalDb;

        public FrmCprSuspensionTiposDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        public ErrorDto<TiposSuspensionDtoList> TiposSuspension_ObtenerTodos(int codEmpresa, int? pagina, int? paginacion, string? filtro)
        {
            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                // DbHelper.WithConn should provide an open connection, but be defensive
                if (conn.State != ConnectionState.Open) conn.Open();

                var like = NormalizeLike(filtro);
                var (offset, fetch) = NormalizePaging(pagina, paginacion);

                const string sqlCount = @"SELECT COUNT(*)
FROM CXP_SUSPENSION_TIPOS
WHERE (@F IS NULL OR COD_SUSPENSION LIKE @F OR descripcion LIKE @F);";

                var total = conn.QueryFirstOrDefault<int>(
                    sqlCount,
                    new { F = like }
                );

                const string sqlSelect = @"SELECT COD_SUSPENSION, descripcion, ACTIVA
FROM CXP_SUSPENSION_TIPOS
WHERE (@F IS NULL OR COD_SUSPENSION LIKE @F OR descripcion LIKE @F)
ORDER BY COD_SUSPENSION
OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

                var rows = conn.Query<TiposSuspensionDto>(
                    sqlSelect,
                    new { F = like, Offset = offset, Fetch = fetch }
                ).ToList();

                return new TiposSuspensionDtoList
                {
                    Total = total,
                    Suspensiones = rows
                };
            });

            if (r.Code != 0)
                return DbHelper.CreateErrorResponse<TiposSuspensionDtoList>(r.Description ?? "Error", r.Code ?? -1, null!);

            return DbHelper.CreateOkResponse(
                r.Result ?? new TiposSuspensionDtoList { Total = 0, Suspensiones = new List<TiposSuspensionDto>() }
            );
        }

        public ErrorDto TiposSuspension_Eliminar(int codEmpresa, string codSuspension)
        {
            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                @"DELETE FROM CXP_SUSPENSION_TIPOS WHERE COD_SUSPENSION = @Cod",
                new { Cod = codSuspension }
            );
        }

        public ErrorDto TiposSuspension_Guardar(int codEmpresa, TiposSuspensionDto dto)
        {
            // Upsert sin SQL injection y sin abrir conexiones duplicadas.
            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var existe = conn.QueryFirstOrDefault<int>(
                    @"SELECT COUNT(*)
                      FROM CXP_SUSPENSION_TIPOS
                      WHERE COD_SUSPENSION = @Cod",
                    new { Cod = dto.Cod_Suspension }
                ) > 0;

                if (!existe)
                    Insertar(conn, dto);
                else
                    Actualizar(conn, dto);

                return true;
            });

            var okMessage = !string.IsNullOrWhiteSpace(r.Description) ? r.Description : "Ok";
            return r.Code == 0
                ? DbHelper.OkResponse(okMessage)
                : DbHelper.ErrorResponse(r.Description ?? "Error", r.Code ?? -1);
        }

        // ----------------- Helpers -----------------

        private static void Insertar(IDbConnection conn, TiposSuspensionDto dto)
        {
            conn.Execute(
                @"INSERT INTO CXP_SUSPENSION_TIPOS
                  (COD_SUSPENSION, descripcion, ACTIVA, REGISTRO_FECHA, REGISTRO_USUARIO)
                  VALUES
                  (@Cod, @Descripcion, @Activa, GETDATE(), @Usuario)",
                new
                {
                    Cod = dto.Cod_Suspension,
                    Descripcion = dto.Descripcion,
                    Activa = dto.Activa ? 1 : 0,
                    Usuario = dto.Registro_Usuario
                }
            );
        }

        private static void Actualizar(IDbConnection conn, TiposSuspensionDto dto)
        {
            conn.Execute(
                @"UPDATE CXP_SUSPENSION_TIPOS
                  SET descripcion = @Descripcion,
                      ACTIVA = @Activa
                  WHERE COD_SUSPENSION = @Cod",
                new
                {
                    Cod = dto.Cod_Suspension,
                    Descripcion = dto.Descripcion,
                    Activa = dto.Activa ? 1 : 0
                }
            );
        }

        private static string? NormalizeLike(string? filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return null;

            var f = filtro.Trim();
            return f.Length == 0 ? null : $"%{f}%";
        }

        private static (int Offset, int Fetch) NormalizePaging(int? pagina, int? paginacion)
        {
            // Keep existing meaning: `pagina` is treated as OFFSET.
            if (pagina is null || paginacion is null || pagina < 0 || paginacion <= 0)
                return (0, int.MaxValue);

            return (pagina.Value, paginacion.Value);
        }
    }
}