using Dapper;
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
            var r = DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var (sqlWhere, prms) = BuildFiltro(filtro);

                var total = conn.QueryFirstOrDefault<int>(
                    $"SELECT COUNT(*) FROM CXP_SUSPENSION_TIPOS {sqlWhere}",
                    prms
                );

                var (sqlPaging, pagingParams) = BuildPaging(pagina, paginacion);
                var finalParams = MergeParams(prms, pagingParams);

                var rows = conn.Query<TiposSuspensionDto>(
                    $@"SELECT COD_SUSPENSION, descripcion, ACTIVA
                       FROM CXP_SUSPENSION_TIPOS
                       {sqlWhere}
                       ORDER BY COD_SUSPENSION
                       {sqlPaging}",
                    finalParams
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

        private static void Insertar(System.Data.IDbConnection conn, TiposSuspensionDto dto)
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

        private static void Actualizar(System.Data.IDbConnection conn, TiposSuspensionDto dto)
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

        private static (string whereSql, object parameters) BuildFiltro(string? filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return (string.Empty, new { });

            // LIKE parametrizado (sin injection)
            var like = $"%{filtro.Trim()}%";
            return ("WHERE COD_SUSPENSION LIKE @F OR descripcion LIKE @F", new { F = like });
        }

        private static (string pagingSql, object parameters) BuildPaging(int? pagina, int? paginacion)
        {
            if (pagina is null || paginacion is null || pagina < 0 || paginacion <= 0)
                return (string.Empty, new { });

            // Aquí asumimos que "pagina" ya viene como OFFSET (igual que tu implementación original).
            // Si en realidad "pagina" era número de página, cambia a: offset = (pagina.Value - 1) * paginacion.Value
            return ("OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY",
                new { Offset = pagina.Value, Fetch = paginacion.Value });
        }

        private static object MergeParams(object a, object b)
        {
            // Dapper acepta DynamicParameters para combinar cómodo
            var p = new DynamicParameters(a);
            p.AddDynamicParams(b);
            return p;
        }
    }
}