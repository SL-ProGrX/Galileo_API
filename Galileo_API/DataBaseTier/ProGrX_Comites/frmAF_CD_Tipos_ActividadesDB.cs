using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using static Galileo_API.Models.ProGrX_Comites.FrmAfCdTiposActividades;

namespace Galileo_API.DataBaseTier.ProGrX_Comites
{
    public class FrmAfCdTiposActividadesDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;

        private const string MovRegistra = "REGISTRA - WEB";
        private const string MovModifica = "MODIFICA - WEB";
        private const string MovElimina = "ELIMINAR - WEB";
        private const int ModuloCxC = 40;

        public FrmAfCdTiposActividadesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config!);
        }

        /// <summary>
        /// Registra un movimiento en la bitácora de seguridad con información relevante del tipo de actividad afectado y la acción realizada.
        /// </summary>
        /// <param name="empresaId"></param>
        /// <param name="usuario"></param>
        /// <param name="detalle"></param>
        /// <param name="movimiento"></param>
        private void LogBitacora(int empresaId, string usuario, string detalle, string movimiento)
        {

            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = empresaId,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = ModuloCxC
            });
        }
        private static ErrorDto Ok() => DbHelper.CreateOkResponse();
        private static ErrorDto Error(string msg) => DbHelper.ErrorResponse(msg);

        /// <summary>
        /// Valida y sanitiza los parámetros de ordenamiento recibidos desde la interfaz para prevenir inyección SQL y asegurar un ordenamiento predecible.
        /// </summary>
        /// <param name="sortField"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        private static (string OrderBy, bool Desc) SanitizeOrderBy(string? sortField, int? sortOrder)
        {
            var field = (sortField ?? string.Empty).Trim().ToLowerInvariant();
            var orderBy = field switch
            {
                "CodTipoActividad" => "CodTipoActividad",
                "NombreTipoActividad" => "NombreTipoActividad",
                "Activo" => "Activo",
                _ => "CodTipoActividad"
            };
            var desc = sortOrder == 1; // 1 = DESC; cualquier otro = ASC
            return (orderBy, desc);
        }

        /// <summary>
        /// Consulta la lista de tipos de actividades desde la base de datos aplicando filtros, ordenamiento y paginación según los parámetros recibidos.
        /// </summary>
        private const string SqlListAsc = @"
                    SELECT
                        CodTipoActividad,
                        NombreTipoActividad,
                        Activo AS Activo
                    FROM dbo.AFI_CD_TIPO_ACTIVIDAD
                    " + WhereClause + @"
                    ORDER BY
                        CASE WHEN @orderBy = 'CodTipoActividad' THEN CodTipoActividad END,
                        CASE WHEN @orderBy = 'NombreTipoActividad'   THEN NombreTipoActividad   END,
                        CASE WHEN @orderBy = 'Activo'        THEN Activo        END
                    ";

        /// <summary>
        /// Consulta la lista de tipos de actividades desde la base de datos aplicando filtros, ordenamiento descendente y paginación según los parámetros recibidos.
        /// </summary>
        private const string SqlListDesc = @"
                    SELECT
                        CodTipoActividad,
                        NombreTipoActividad,
                        Activo AS Activo
                    FROM dbo.AFI_CD_TIPO_ACTIVIDAD
                    " + WhereClause + @"
                    ORDER BY
                        CASE WHEN @orderBy = 'CodTipoActividad' THEN CodTipoActividad END DESC,
                        CASE WHEN @orderBy = 'NombreTipoActividad'   THEN NombreTipoActividad   END DESC,
                        CASE WHEN @orderBy = 'Activo'        THEN Activo        END DESC
                    ";

        /// <summary>
        /// Consulta el total de registros de tipos de actividades que cumplen con el filtro aplicado, para fines de paginación en la interfaz.
        /// </summary>
        private const string SqlCount = "SELECT COUNT(1) FROM dbo.AFI_CD_TIPO_ACTIVIDAD " + WhereClause + ";";

        /// <summary>
        /// Cláusula WHERE común utilizada en las consultas de lista y conteo, que aplica un filtro de búsqueda sobre los campos CodTipoActividad y NombreTipoActividad.
        /// </summary>
        private const string WhereClause = @"
                        WHERE
                            (@filtro IS NULL)
                            OR (CAST(CodTipoActividad AS NVARCHAR(50)) LIKE @like)
                            OR (NombreTipoActividad LIKE @like)
                        ";

        /// <summary>
        /// Obtiene la lista de tipos de actividades desde la base de datos aplicando filtros de búsqueda, ordenamiento y paginación según los parámetros recibidos desde la interfaz.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="esExportar"></param>
        /// <returns></returns>
        public ErrorDto<CDTiposActividadesLista> AfCdTiposActividadesLista_Obtener(int codEmpresa, FiltrosLazyLoadData filtros, bool esExportar)
        {

            return DbHelper.WithConn(_portalDB, codEmpresa, (SqlConnection conn) =>
            {
                filtros ??= new FiltrosLazyLoadData();

                var texto = filtros.filtro?.Trim();
                var hasFiltro = !string.IsNullOrWhiteSpace(texto);

                var offset = filtros.pagina;
                var fetch = filtros.paginacion;
                var usarPaginacion = fetch > 0 && !esExportar;

                var (orderBy, desc) = SanitizeOrderBy(filtros.sortField, filtros.sortOrder);

                var baseParams = new
                {
                    filtro = hasFiltro ? texto : null,
                    like = hasFiltro ? $"%{texto}%" : null,
                    orderBy,
                    offset,
                    fetch
                };

                var total = conn.QuerySingle<int>(SqlCount, baseParams);


                var sqlListCore = desc ? SqlListDesc : SqlListAsc;
                var sqlList = usarPaginacion
                    ? sqlListCore + " OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;"
                    : sqlListCore + ";";

                var lista = conn.Query<CDTiposActividadesData>(sqlList, baseParams).ToList();

                return new CDTiposActividadesLista
                {
                    total = total,
                    lista = lista
                };
            });

        }

        /// <summary>
        /// Guarda un tipo de actividad en la base de datos, realizando una operación de inserción o actualización según si el código de actividad ya existe o no. Valida los datos recibidos desde la interfaz y registra un movimiento en la bitácora de seguridad con la acción realizada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto AfCdTiposActividades_Guardar(int codEmpresa, string usuario, CDTiposActividadesData datos)
        {
            if (datos is null) return Error("Datos requeridos.");
            if (string.IsNullOrWhiteSpace(datos.CodTipoActividad)) return Error("El campo 'CodTipoActividad' es requerido.");
            if (string.IsNullOrWhiteSpace(usuario)) return Error("El usuario es requerido.");

            const string sqlUpsert = @"
                        DECLARE @accion NVARCHAR(10);

                        UPDATE AFI_CD_TIPO_ACTIVIDAD
                        SET NombreTipoActividad = @NombreTipoActividad,
                            Activo      = @Activo
                        WHERE CodTipoActividad = @CodTipoActividad;

                        IF @@ROWCOUNT = 0
                        BEGIN
                            INSERT INTO AFI_CD_TIPO_ACTIVIDAD
                                (CodTipoActividad, NombreTipoActividad, Activo, RegistroFecha, RegistroUsuario)
                            VALUES
                                (UPPER(@CodTipoActividad), @NombreTipoActividad, @Activo, dbo.MyGetdate(), @usuario);
                            SET @accion = N'insert';
                        END
                        ELSE
                        BEGIN
                            SET @accion = N'update';
                        END

                        SELECT @accion AS accion;
                        ";


            var upsert = DbHelper.ExecuteSingleQuery<string>(
                _portalDB, codEmpresa, sqlUpsert, defaultValue: "", parameters: new
                {
                    datos.CodTipoActividad,
                    datos.NombreTipoActividad,
                    datos.Activo,
                    usuario
                });

            if (upsert.Code != 0)
                return Error("No fue posible guardar la el tipo de actividad.");

            var accion = (upsert.Result ?? "").ToLowerInvariant();
            if (accion == "insert")
            {
                LogBitacora(codEmpresa, usuario, $"Tipos de Actividad Id: {datos.CodTipoActividad}", MovRegistra);
                return Ok();
            }
            if (accion == "update")
            {
                LogBitacora(codEmpresa, usuario, $"Tipos de Actividad Id: {datos.CodTipoActividad}", MovModifica);
                return Ok();
            }

            return Ok();
        }

        /// <summary>
        /// Elimina un tipo de actividad de la base de datos según el código recibido desde la interfaz. Valida el código recibido y registra un movimiento en la bitácora de seguridad con la acción realizada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="codTipoActividad"></param>
        /// <returns></returns>
        public ErrorDto AfCdTiposActividades_Eliminar(int codEmpresa, string usuario, string codTipoActividad)
        {
            if (string.IsNullOrWhiteSpace(codTipoActividad))
                return Error("El 'codTipoActividad' es requerido.");

            const string sql = @"DELETE FROM AFI_CD_TIPO_ACTIVIDAD WHERE CodTipoActividad = @codTipoActividad;";
            var result = DbHelper.ExecuteNonQueryWithResult(_portalDB, codEmpresa, sql, new { CodTipoActividad = codTipoActividad });

            if (result.Code != 0)
                return Error("No fue posible eliminar la clasificación de CxC.");

            if (result.Result > 0)
            {
                LogBitacora(codEmpresa, usuario, $"Tipos de Actividad Id: {codTipoActividad}", MovElimina);
                return Ok();
            }

            return Ok();
        }

    }
}
