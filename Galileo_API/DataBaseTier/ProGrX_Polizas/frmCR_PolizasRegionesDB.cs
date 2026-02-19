using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Polizas;
using System.Collections.Generic;

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

        /// <summary>
        /// Lista las regiones configuradas para una póliza.
        /// VB6: sbCargaRegiones()
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_Polizas_RegionLista_Obtener(int CodEmpresa, string cod_poliza)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"
                            SELECT
                                COD_REGION as item,
                                CONCAT(
                                    COD_REGION, '-',
                                    'MC: ', 
                                    FORMAT(MONTO_COMERCIAL, 'N2'),
                                    '  MP: ',
                                    FORMAT(MONTO_PERSONAL, 'N2')
                                ) AS  descripcion
                            FROM CRD_POLIZAS_REGION
                            WHERE COD_POLIZA = @cod_poliza
                            ORDER BY COD_REGION";

                return conn.Query<DropDownListaGenericaModel>(
                            query,
                            new { cod_poliza = cod_poliza?.Trim() }
                       ).ToList();
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_Provincias_Listar(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"
                        SELECT
                            LTRIM(RTRIM(PROVINCIA)) AS item,
                            RTRIM(PROVINCIA) + ' - ' + RTRIM(descripcion) AS descripcion
                        FROM PROVINCIAS
                        ORDER BY DESCRIPCION;";

                var list = conn.Query<DropDownListaGenericaModel>(query).ToList();

                // Agrega "TODOS" como primera opción
                list.Insert(0, new DropDownListaGenericaModel { item = 0, descripcion = "TODOS" });

                return list;
            });
        }


        public ErrorDto<List<CrdPolizasRegionCantonDto>> Crd_Polizas_Region_Cantones_Listar(
                int CodEmpresa,
                string cod_poliza,
                int cod_region,
                string? provincia,
                CrdCantonesModo modo)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var codPoliza = (cod_poliza ?? "").Trim();
            var provinciaTxt = (provincia ?? "TODOS").Trim();

            var validationError = ValidateCantonesListar(codPoliza, cod_region, provinciaTxt, out var provinciaInt);
            if (validationError != null)
            {
                return DbHelper.CreateErrorResponse<List<CrdPolizasRegionCantonDto>>(validationError.Description!);
            }
                

                var sql = BuildCantonesSql(modo, provinciaInt.HasValue);
            var param = BuildCantonesParams(codPoliza, cod_region, provinciaInt);

            var response = conn.Query<CrdPolizasRegionCantonDto>(sql, param).ToList();

            return DbHelper.CreateOkResponse<List<CrdPolizasRegionCantonDto>>(response);
        }

        private static ErrorDto? ValidateCantonesListar(
                string codPoliza,
                int codRegion,
                string provinciaTxt,
                out int? provinciaInt)
        {
            provinciaInt = null;

            if (string.IsNullOrWhiteSpace(codPoliza))
                return DbHelper.ErrorResponse("cod_poliza es requerido.");

            if (codRegion <= 0)
                return DbHelper.ErrorResponse("cod_region es requerido.");

            if (IsTodos(provinciaTxt))
                return null;

            if (!int.TryParse(provinciaTxt, out var p))
                return DbHelper.ErrorResponse("provincia inválida.");

            provinciaInt = p;
            return null;
        }

        private static bool IsTodos(string provinciaTxt) =>
            string.Equals(provinciaTxt, "TODOS", StringComparison.OrdinalIgnoreCase);

        private static object BuildCantonesParams(string codPoliza, int codRegion, int? provinciaInt) =>
            new { cod_poliza = codPoliza, cod_region = codRegion, provincia = provinciaInt };

        private static string BuildCantonesSql(CrdCantonesModo modo, bool filtraProvincia)
        {
            var select = @"
                SELECT
                    CASE WHEN RD.CANTON IS NULL THEN 0 ELSE 1 END AS asignado,
                    C.CANTON AS canton,
                    C.DESCRIPCION AS ncanton,
                    P.PROVINCIA AS provincia,
                    P.DESCRIPCION AS nprovincia,
                    RD.REGISTRO_FECHA AS registro_fecha,
                    RD.REGISTRO_USUARIO AS registro_usuario
                FROM CANTONES C
                INNER JOIN PROVINCIAS P
                    ON C.PROVINCIA = P.PROVINCIA
                ";

            var join = GetJoinByModo(modo);
            var where = GetWhereByModo(modo, filtraProvincia);

            return select + join + where + "\nORDER BY P.DESCRIPCION, C.DESCRIPCION;";
        }

        private static string GetJoinByModo(CrdCantonesModo modo) =>
            modo == CrdCantonesModo.solo_asignados
                ? @"
                    INNER JOIN CRD_POLIZAS_REGION_DETALLE RD
                        ON C.CANTON = RD.CANTON
                       AND C.PROVINCIA = RD.PROVINCIA
                       AND RD.COD_POLIZA = @cod_poliza
                       AND RD.COD_REGION = @cod_region
                    "
                                    : @"
                    LEFT JOIN CRD_POLIZAS_REGION_DETALLE RD
                        ON C.CANTON = RD.CANTON
                       AND C.PROVINCIA = RD.PROVINCIA
                       AND RD.COD_POLIZA = @cod_poliza
                       AND RD.COD_REGION = @cod_region
                    ";

        private static string GetWhereByModo(CrdCantonesModo modo, bool filtraProvincia)
        {
            // base: nada
            var clauses = new List<string>();

            if (modo == CrdCantonesModo.no_asignados)
                clauses.Add("RD.CANTON IS NULL");

            if (filtraProvincia)
                clauses.Add("P.PROVINCIA = @provincia");

            return clauses.Count == 0 ? "" : "\nWHERE " + string.Join(" AND ", clauses) + "\n";
        }

        public ErrorDto Crd_Polizas_Region_Canton_Asignar(
            int CodEmpresa,
            string usuario,
            CrdPolizasRegionAsignarCantonDto req)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            string msj = req.asigna ? "asignar" : "desasignar";
            try
            {
                var codPoliza = (req.cod_poliza ?? "").Trim();
                var canton = (req.canton ?? "").Trim();

                const string validaExiste = @"
                    SELECT COUNT(1)
                    FROM CRD_POLIZAS_REGION_DETALLE
                    WHERE COD_POLIZA = @cod_poliza
                      AND COD_REGION = @cod_region
                      AND CANTON = @canton
                      AND PROVINCIA = @provincia;";

                var param =  new
                {
                    cod_poliza = codPoliza,
                    cod_region = req.cod_region,
                    canton,
                    provincia = req.provincia
                };

                var existe = connection.Query<int>(validaExiste, param).FirstOrDefault();

                if (existe == 0)
                {
                    // Regla heredada del VB6:
                    // eliminar cualquier asignación previa del cantón en la póliza
                    const string deletePrevio = @"
                            DELETE CRD_POLIZAS_REGION_DETALLE
                            WHERE COD_POLIZA = @cod_poliza
                              AND CANTON = @canton
                              AND PROVINCIA = @provincia";

                    connection.Execute(deletePrevio, new
                    {
                        cod_poliza = codPoliza,
                        canton,
                        provincia = req.provincia
                    });

                    const string insertSql = @"
                        INSERT CRD_POLIZAS_REGION_DETALLE
                            (COD_POLIZA, COD_REGION, CANTON, PROVINCIA, REGISTRO_USUARIO, REGISTRO_FECHA)
                        VALUES
                            (@cod_poliza, @cod_region, @canton, @provincia, @usuario, dbo.MyGetdate())";

                    connection.Execute(insertSql, new
                    {
                        cod_poliza = codPoliza,
                        cod_region = req.cod_region,
                        canton,
                        provincia = req.provincia,
                        usuario
                    });
                }
                else
                {
                    const string deleteSql = @"
                DELETE CRD_POLIZAS_REGION_DETALLE
                WHERE COD_POLIZA = @cod_poliza
                  AND COD_REGION = @cod_region
                  AND CANTON = @canton
                  AND PROVINCIA = @provincia";

                    connection.Execute(deleteSql, new
                    {
                        cod_poliza = codPoliza,
                        cod_region = req.cod_region,
                        canton,
                        provincia = req.provincia
                    });
                }

                return DbHelper.OkResponse($"Se ha {msj} el cantón correctamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"Error asignando/desasignando cantón: {ex.Message}");
            }
        }


        #endregion

    }
}
