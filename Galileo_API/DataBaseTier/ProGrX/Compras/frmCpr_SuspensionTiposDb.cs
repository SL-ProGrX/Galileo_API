using Galileo.Models.CPR;
using Galileo.Models.ERROR;

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
            var like = NormalizeLike(filtro);
            var (offset, fetch) = NormalizePaging(pagina, paginacion);

            const string sqlCount = @"SELECT COUNT(*)
FROM CXP_SUSPENSION_TIPOS
WHERE (@F IS NULL OR COD_SUSPENSION LIKE @F OR descripcion LIKE @F);";

            var totalResp = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, sqlCount, 0, new { F = like });
            if (totalResp.Code != 0)
                return DbHelper.CreateErrorResponse<TiposSuspensionDtoList>(totalResp.Description ?? "Error", totalResp.Code ?? -1, null!);

            const string sqlSelect = @"SELECT COD_SUSPENSION, descripcion, ACTIVA
FROM CXP_SUSPENSION_TIPOS
WHERE (@F IS NULL OR COD_SUSPENSION LIKE @F OR descripcion LIKE @F)
ORDER BY COD_SUSPENSION
OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

            var listResp = DbHelper.ExecuteListQuery<TiposSuspensionDto>(_portalDb, codEmpresa, sqlSelect, new { F = like, Offset = offset, Fetch = fetch });
            if (listResp.Code != 0)
                return DbHelper.CreateErrorResponse<TiposSuspensionDtoList>(listResp.Description ?? "Error", listResp.Code ?? -1, null!);

            return DbHelper.CreateOkResponse(
                new TiposSuspensionDtoList
                {
                    Total = totalResp.Result,
                    Suspensiones = listResp.Result ?? new List<TiposSuspensionDto>()
                }
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
            const string existsSql = @"SELECT COUNT(*)
FROM CXP_SUSPENSION_TIPOS
WHERE COD_SUSPENSION = @Cod";

            var existeResp = DbHelper.ExecuteSingleQuery<int>(_portalDb, codEmpresa, existsSql, 0, new { Cod = dto.Cod_Suspension });
            if (existeResp.Code != 0)
                return DbHelper.ErrorResponse(existeResp.Description ?? "Error", existeResp.Code ?? -1);

            var p = new
            {
                Cod = dto.Cod_Suspension,
                Descripcion = dto.Descripcion,
                Activa = dto.Activa ? 1 : 0,
                Usuario = dto.Registro_Usuario
            };

            if (existeResp.Result <= 0)
            {
                const string insertSql = @"INSERT INTO CXP_SUSPENSION_TIPOS
(COD_SUSPENSION, descripcion, ACTIVA, REGISTRO_FECHA, REGISTRO_USUARIO)
VALUES
(@Cod, @Descripcion, @Activa, GETDATE(), @Usuario)";

                return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, insertSql, p);
            }

            const string updateSql = @"UPDATE CXP_SUSPENSION_TIPOS
SET descripcion = @Descripcion,
    ACTIVA = @Activa
WHERE COD_SUSPENSION = @Cod";

            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, updateSql, p);
        }

        // ----------------- Helpers -----------------

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