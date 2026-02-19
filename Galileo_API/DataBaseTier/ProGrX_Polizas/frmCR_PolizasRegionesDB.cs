using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Polizas;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmCrPolizasRegionesDB
    {
        private readonly PortalDB _portalDb;

        public FrmCrPolizasRegionesDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        #region Regiones

        /// <summary>
        /// Lista las regiones configuradas para una póliza.
        /// VB6: sbCargaRegiones()
        /// </summary>
        public ErrorDto<List<CrdPolizasRegionDto>> Crd_Polizas_Region_Obtener(int CodEmpresa, string cod_poliza)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"
                            SELECT
                                COD_REGION,
                                MONTO_COMERCIAL,
                                MONTO_PERSONAL,
                                MODIFICA_FECHA,
                                MODIFICA_USUARIO,
                                REGISTRO_USUARIO,
                                REGISTRO_FECHA
                            FROM CRD_POLIZAS_REGION
                            WHERE COD_POLIZA = @cod_poliza
                            ORDER BY COD_REGION";

                return conn.Query<CrdPolizasRegionDto>(
                            query,
                            new { cod_poliza = cod_poliza?.Trim() }
                       ).ToList();
            });
        }

        /// <summary>
        /// Inserta o actualiza una región de póliza.
        /// Basado en VB6: fxGuardar()
        /// - Si cod_region es null => inserta con (max(cod_region)+1) por póliza.
        /// - Si cod_region tiene valor => actualiza montos y auditoría.
        /// </summary>
        public ErrorDto Crd_Polizas_Region_Guardar(int CodEmpresa, string usuario, CrdPolizasRegionGuardarDto dto)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var codPoliza = (dto.cod_poliza ?? "").Trim();
            var validationError = Validate(dto, codPoliza);
            if (validationError != null) return validationError;

            // Determina si es insert (como VB6: cuando no hay cod_region)
            var esInsert = !dto.cod_region.HasValue || dto.cod_region.Value <= 0;

            int codRegion;

            if (esInsert)
            {
                // VB6: select isnull(max(COD_REGION),0)+1 ... where COD_POLIZA = '...'
                codRegion = conn.ExecuteScalar<int>(@"
            SELECT ISNULL(MAX(COD_REGION), 0) + 1
            FROM CRD_POLIZAS_REGION
            WHERE COD_POLIZA = @cod_poliza;",
                    new { cod_poliza = codPoliza }
                );
            }
            else
            {
                codRegion = dto.cod_region!.Value;

                // Para update: confirmar que existe (VB6 asumía que sí porque venía del grid)
                var existe = conn.ExecuteScalar<int>(@"
            SELECT COUNT(1)
            FROM CRD_POLIZAS_REGION
            WHERE COD_POLIZA = @cod_poliza
              AND COD_REGION = @cod_region;",
                    new { cod_poliza = codPoliza, cod_region = codRegion }
                ) > 0;

                if (!existe)
                    return DbHelper.ErrorResponse("No se encontró la región para actualizar.");
            }

            var sql = esInsert ? InsertSql : UpdateSql;
            var parametros = BuildParams(dto, codPoliza, codRegion, usuario);

            var rows = conn.Execute(sql, parametros);

            var successMessage = esInsert
                ? "Región creada correctamente."
                : "Región actualizada correctamente.";

            var errorMessage = esInsert
                ? "No se pudo crear la región."
                : "No se pudo actualizar la región.";

            return rows > 0
                ? DbHelper.OkResponse(successMessage)
                : DbHelper.ErrorResponse(errorMessage);
        }

        private static ErrorDto? Validate(CrdPolizasRegionGuardarDto dto, string codPoliza)
        {
            if (string.IsNullOrWhiteSpace(codPoliza))
                return DbHelper.ErrorResponse("Debe indicar el código de la póliza.");

            if (dto.monto_comercial is null)
                return DbHelper.ErrorResponse("Debe completar el monto comercial.");

            if (dto.monto_personal is null)
                return DbHelper.ErrorResponse("Debe completar el monto personal.");

            return null;
        }

        private static object BuildParams(CrdPolizasRegionGuardarDto dto, string codPoliza, int codRegion, string usuario)
        {
            return new
            {
                cod_poliza = codPoliza,
                cod_region = codRegion,
                monto_comercial = dto.monto_comercial ?? 0m,
                monto_personal = dto.monto_personal ?? 0m,

                // según el SQL escogido se usa uno u otro
                registro_usuario = usuario,
                modifica_usuario = usuario
            };
        }

        private const string InsertSql = @"
    INSERT INTO CRD_POLIZAS_REGION
    (
      COD_POLIZA,
      COD_REGION,
      MONTO_COMERCIAL,
      MONTO_PERSONAL,
      REGISTRO_USUARIO,
      REGISTRO_FECHA
    )
    VALUES
    (
      @cod_poliza,
      @cod_region,
      @monto_comercial,
      @monto_personal,
      @registro_usuario,
      dbo.MyGetdate()
    );";

        private const string UpdateSql = @"
    UPDATE CRD_POLIZAS_REGION
    SET
      MONTO_COMERCIAL = @monto_comercial,
      MONTO_PERSONAL  = @monto_personal,
      MODIFICA_USUARIO = @modifica_usuario,
      MODIFICA_FECHA   = dbo.MyGetdate()
    WHERE
      COD_POLIZA = @cod_poliza
      AND COD_REGION = @cod_region;";

        /// <summary>
        /// Método para eliminar una región de póliza, solo si no tiene cantones asignados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_poliza"></param>
        /// <param name="cod_region"></param>
        /// <returns></returns>
        public ErrorDto Crd_Polizas_Region_Eliminar(int CodEmpresa, string cod_poliza, int cod_region)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            cod_poliza = (cod_poliza ?? "").Trim();

            if (string.IsNullOrWhiteSpace(cod_poliza))
                return DbHelper.ErrorResponse("Debe indicar el código de la póliza.");

            if (cod_region <= 0)
                return DbHelper.ErrorResponse("Debe indicar la región a eliminar.");

            // 1️⃣ Validar si tiene cantones asignados
            var cantidad = conn.ExecuteScalar<int>(@"
        SELECT COUNT(1)
        FROM CRD_POLIZAS_REGION_DETALLE
        WHERE COD_POLIZA = @cod_poliza
          AND COD_REGION = @cod_region;",
                new { cod_poliza, cod_region });

            if (cantidad > 0)
                return DbHelper.ErrorResponse("No se puede borrar la región, tiene cantones asignados.");

            // 2️⃣ Ejecutar delete
            var rows = conn.Execute(@"
        DELETE FROM CRD_POLIZAS_REGION
        WHERE COD_POLIZA = @cod_poliza
          AND COD_REGION = @cod_region;",
                new { cod_poliza, cod_region });

            return rows > 0
                ? DbHelper.OkResponse("Región eliminada correctamente.")
                : DbHelper.ErrorResponse("No se pudo eliminar la región.");
        }


        #endregion


        #region Asignacion

        #endregion

    }
}
