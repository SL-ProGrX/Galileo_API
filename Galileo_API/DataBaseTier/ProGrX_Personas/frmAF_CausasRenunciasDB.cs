using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Personas
{
    public class FrmAFCausasRenunciasDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 1;
        private readonly MSecurityMainDb _Security_MainDB;

        public FrmAFCausasRenunciasDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de causas de renuncia con filtros, orden y paginación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<AfCausasRenunciasData>> AF_CausasRenuncias_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de causas de renuncia son requeridos.", -2, new List<AfCausasRenunciasData>());
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var filtroTexto = filtros.filtro?.Trim();
                var sortField = ObtenerSortFieldCausas(filtros.sortField);
                var sortDirection = ObtenerSortDirectionCausas(filtros.sortOrder);
                var offsetRows = filtros.pagina;
                var fetchRows = filtros.paginacion;

                var sql = @"
                    SELECT Id_Causa AS id_causa,
                           Descripcion AS descripcion,
                           Tipo_Apl AS tipo_apl,
                           mortalidad,
                           AJUSTE_TASAS AS ajuste_tasas,
                           liq_alterna,
                           tasa_planilla,
                           tasa_ventanilla,
                           institucion,
                           cod_Plan AS cod_plan,
                           activo
                    FROM vAFI_Causas_Renuncias
                    WHERE (
                        @Filtro IS NULL
                        OR descripcion LIKE @Filtro
                        OR cod_plan LIKE @Filtro
                    )
                    ORDER BY
                        CASE WHEN @SortField = 'id_causa' AND @SortDirection = 'ASC' THEN Id_Causa END ASC,
                        CASE WHEN @SortField = 'id_causa' AND @SortDirection = 'DESC' THEN Id_Causa END DESC,
                        CASE WHEN @SortField = 'descripcion' AND @SortDirection = 'ASC' THEN Descripcion END ASC,
                        CASE WHEN @SortField = 'descripcion' AND @SortDirection = 'DESC' THEN Descripcion END DESC,
                        CASE WHEN @SortField = 'tipo_apl' AND @SortDirection = 'ASC' THEN Tipo_Apl END ASC,
                        CASE WHEN @SortField = 'tipo_apl' AND @SortDirection = 'DESC' THEN Tipo_Apl END DESC,
                        CASE WHEN @SortField = 'mortalidad' AND @SortDirection = 'ASC' THEN mortalidad END ASC,
                        CASE WHEN @SortField = 'mortalidad' AND @SortDirection = 'DESC' THEN mortalidad END DESC,
                        CASE WHEN @SortField = 'ajuste_tasas' AND @SortDirection = 'ASC' THEN AJUSTE_TASAS END ASC,
                        CASE WHEN @SortField = 'ajuste_tasas' AND @SortDirection = 'DESC' THEN AJUSTE_TASAS END DESC,
                        CASE WHEN @SortField = 'liq_alterna' AND @SortDirection = 'ASC' THEN liq_alterna END ASC,
                        CASE WHEN @SortField = 'liq_alterna' AND @SortDirection = 'DESC' THEN liq_alterna END DESC,
                        CASE WHEN @SortField = 'tasa_planilla' AND @SortDirection = 'ASC' THEN tasa_planilla END ASC,
                        CASE WHEN @SortField = 'tasa_planilla' AND @SortDirection = 'DESC' THEN tasa_planilla END DESC,
                        CASE WHEN @SortField = 'tasa_ventanilla' AND @SortDirection = 'ASC' THEN tasa_ventanilla END ASC,
                        CASE WHEN @SortField = 'tasa_ventanilla' AND @SortDirection = 'DESC' THEN tasa_ventanilla END DESC,
                        CASE WHEN @SortField = 'institucion' AND @SortDirection = 'ASC' THEN institucion END ASC,
                        CASE WHEN @SortField = 'institucion' AND @SortDirection = 'DESC' THEN institucion END DESC,
                        CASE WHEN @SortField = 'cod_plan' AND @SortDirection = 'ASC' THEN cod_Plan END ASC,
                        CASE WHEN @SortField = 'cod_plan' AND @SortDirection = 'DESC' THEN cod_Plan END DESC,
                        CASE WHEN @SortField = 'activo' AND @SortDirection = 'ASC' THEN activo END ASC,
                        CASE WHEN @SortField = 'activo' AND @SortDirection = 'DESC' THEN activo END DESC,
                        Id_Causa ASC";

                if (fetchRows > 0)
                {
                    sql += " OFFSET @OffsetRows ROWS FETCH NEXT @FetchRows ROWS ONLY";
                }

                var parametros = new DynamicParameters();
                parametros.Add("Filtro", string.IsNullOrWhiteSpace(filtroTexto) ? null : $"%{filtroTexto}%");
                parametros.Add("SortField", sortField);
                parametros.Add("SortDirection", sortDirection);
                parametros.Add("OffsetRows", offsetRows);
                parametros.Add("FetchRows", fetchRows);

                return connection.Query<AfCausasRenunciasData>(sql, parametros).ToList();
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<AfCausasRenunciasData>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener causas de renuncia.", result.Code.GetValueOrDefault(-1), new List<AfCausasRenunciasData>());
        }

        /// <summary>
        /// Guarda (inserta o actualiza) una causa de renuncia.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="causa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_CausasRenuncias_Guardar(int CodEmpresa, AfCausasRenunciasData causa, string usuario)
        {
            if (causa is null)
            {
                return DbHelper.ErrorResponse("Los datos de la causa de renuncia son requeridos.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var queryExiste = "SELECT COUNT(*) FROM causas_renuncias WHERE id_causa = @id_causa";
                var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { id_causa = causa.id_causa });

                return existe > 0
                    ? AF_CausasRenuncias_Actualizar(connection, causa, CodEmpresa, usuario)
                    : AF_CausasRenuncias_Insertar(connection, causa, CodEmpresa, usuario);
            });

            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar causa de renuncia.", result.Code.GetValueOrDefault(-1));
        }

        private ErrorDto AF_CausasRenuncias_Insertar(SqlConnection connection, AfCausasRenunciasData causa, int CodEmpresa, string usuario)
        {
            int newId = connection.QuerySingle<int>(
                @"INSERT INTO causas_renuncias (
                        descripcion, Tipo_Apl, mortalidad, AJUSTE_TASAS, liq_alterna,
                        tasa_planilla, tasa_ventanilla, institucion, Cod_Plan, Activo)
                  VALUES (
                        @descripcion, @tipo_apl, @mortalidad, @ajuste_tasas, @liq_alterna,
                        @tasa_planilla, @tasa_ventanilla, @institucion, @cod_plan, @activo);
                  SELECT CAST(SCOPE_IDENTITY() AS INT) AS new_id;",
                new
                {
                    causa.descripcion,
                    tipo_apl = causa.tipo_apl[0],
                    causa.mortalidad,
                    causa.ajuste_tasas,
                    causa.liq_alterna,
                    causa.tasa_planilla,
                    causa.tasa_ventanilla,
                    causa.institucion,
                    causa.cod_plan,
                    causa.activo
                });

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Causa de Renuncia: {newId} - {causa.descripcion}",
                "Registra - WEB");

            return DbHelper.OkResponse($"Insertado correctamente (Id: {newId})");
        }

        private ErrorDto AF_CausasRenuncias_Actualizar(SqlConnection connection, AfCausasRenunciasData causa, int CodEmpresa, string usuario)
        {
            connection.Execute(
                @"UPDATE causas_renuncias SET
                        descripcion     = @descripcion,
                        Tipo_Apl        = @tipo_apl,
                        mortalidad      = @mortalidad,
                        AJUSTE_TASAS    = @ajuste_tasas,
                        liq_alterna     = @liq_alterna,
                        tasa_planilla   = @tasa_planilla,
                        tasa_ventanilla = @tasa_ventanilla,
                        institucion     = @institucion,
                        Cod_Plan        = @cod_plan,
                        Activo          = @activo
                  WHERE id_causa  = @id_causa",
                new
                {
                    causa.id_causa,
                    causa.descripcion,
                    tipo_apl = causa.tipo_apl[0],
                    causa.mortalidad,
                    causa.ajuste_tasas,
                    causa.liq_alterna,
                    causa.tasa_planilla,
                    causa.tasa_ventanilla,
                    causa.institucion,
                    causa.cod_plan,
                    causa.activo
                });

            RegistrarBitacora(
                CodEmpresa,
                usuario,
                $"Causa de Renuncia: {causa.id_causa} - {causa.descripcion}",
                "Modifica - WEB");

            return DbHelper.OkResponse("Actualizado correctamente");
        }

        /// <summary>
        /// Elimina una causa de renuncia por su identificador.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="id_causa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_CausasRenuncias_Eliminar(int CodEmpresa, int id_causa, string usuario)
        {
            var result = DbHelper.ExecuteNonQueryWithResult(
                CreatePortalDb(),
                CodEmpresa,
                "DELETE FROM causas_renuncias WHERE id_causa = @id_causa",
                new { id_causa });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al eliminar causa de renuncia.", result.Code.GetValueOrDefault(-1));
            }

            if (result.Result > 0)
            {
                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    $"Causa de Renuncia: {id_causa}",
                    "Elimina - WEB");

                return DbHelper.OkResponse("Eliminado correctamente");
            }

            return new ErrorDto
            {
                Code = 1,
                Description = "No se encontró el registro"
            };
        }

        private static string ObtenerSortFieldCausas(string? sortField)
        {
            return sortField switch
            {
                "id_causa" => "id_causa",
                "descripcion" => "descripcion",
                "tipo_apl" => "tipo_apl",
                "mortalidad" => "mortalidad",
                "ajuste_tasas" => "ajuste_tasas",
                "liq_alterna" => "liq_alterna",
                "tasa_planilla" => "tasa_planilla",
                "tasa_ventanilla" => "tasa_ventanilla",
                "institucion" => "institucion",
                "cod_plan" => "cod_plan",
                "activo" => "activo",
                _ => "id_causa"
            };
        }

        private static string ObtenerSortDirectionCausas(int sortOrder)
        {
            return sortOrder == 0 ? "DESC" : "ASC";
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalleMovimiento, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalleMovimiento,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }

        private PortalDB CreatePortalDb() => new(_config);
    }
}
