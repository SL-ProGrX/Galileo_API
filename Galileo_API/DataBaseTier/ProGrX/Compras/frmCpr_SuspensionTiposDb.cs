using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
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

        private sealed class TiposSuspensionRow
        {
            // Populated by Dapper mapping
            public string COD_SUSPENSION = string.Empty;
            public string descripcion = string.Empty;
            public int ACTIVA = 0;
            public int Total = 0;
        }

        public ErrorDto<TiposSuspensionDtoList> TiposSuspension_ObtenerTodos(int codEmpresa, int? pagina, int? paginacion, string? filtro)
        {
            var like = string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro.Trim()}%";
            var offset = (pagina is int p && p >= 0) ? p : 0;
            var fetch = (paginacion is int t && t > 0) ? t : int.MaxValue;

            const string sql = @"SELECT COD_SUSPENSION,
       descripcion,
       ACTIVA,
       COUNT(*) OVER() AS Total
  FROM CXP_SUSPENSION_TIPOS
 WHERE (@F IS NULL OR COD_SUSPENSION LIKE @F OR descripcion LIKE @F)
 ORDER BY COD_SUSPENSION
 OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY;";

            var rowsResp = DbHelper.ExecuteListQuery<TiposSuspensionRow>(
                _portalDb,
                codEmpresa,
                sql,
                new { F = like, Offset = offset, Fetch = fetch }
            );

            if (rowsResp.Code != 0)
                return new ErrorDto<TiposSuspensionDtoList>
                {
                    Code = rowsResp.Code is int c ? c : -1,
                    Description = rowsResp.Description ?? "Error",
                    Result = null
                };

            var rows = rowsResp.Result ?? new List<TiposSuspensionRow>();
            var total = rows.Count == 0 ? 0 : rows[0].Total;

            var dto = new TiposSuspensionDtoList
            {
                Total = total,
                Suspensiones = rows.Select(r => new TiposSuspensionDto
                {
                    Cod_Suspension = r.COD_SUSPENSION,
                    Descripcion = r.descripcion,
                    // Mantiene el contrato del DTO: Activa como bool
                    Activa = r.ACTIVA == 1,
                    Registro_Fecha = DateTime.UtcNow // Set to current UTC time or replace as needed
                }).ToList()
            };

            return DbHelper.CreateOkResponse(dto);
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
            const string mergeSql = @"MERGE CXP_SUSPENSION_TIPOS AS T
USING (SELECT @Cod AS COD_SUSPENSION) AS S
ON (T.COD_SUSPENSION = S.COD_SUSPENSION)
WHEN MATCHED THEN
    UPDATE SET descripcion = @Descripcion,
               ACTIVA = @Activa
WHEN NOT MATCHED THEN
    INSERT (COD_SUSPENSION, descripcion, ACTIVA, REGISTRO_FECHA, REGISTRO_USUARIO)
    VALUES (@Cod, @Descripcion, @Activa, GETDATE(), @Usuario);";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                mergeSql,
                new
                {
                    Cod = dto.Cod_Suspension,
                    Descripcion = dto.Descripcion,
                    Activa = dto.Activa ? 1 : 0,
                    Usuario = dto.Registro_Usuario
                }
            );
        }
    }
}