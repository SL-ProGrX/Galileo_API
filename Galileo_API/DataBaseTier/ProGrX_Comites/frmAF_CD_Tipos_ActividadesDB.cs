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

        private const string OrderByCodTipoActividad = "CodTipoActividad";
        private const string OrderByNombreTipoActividad = "NombreTipoActividad";
        private const string OrderByActivo = "Activo";

        /// <summary>
        /// Cláusula WHERE reutilizable para filtrar por código o nombre de tipo de actividad, usando parámetros para evitar SQL injection.
        /// </summary>
        private const string WhereClause = @"
            WHERE
                (@filtro IS NULL)
                OR (CAST(CodTipoActividad AS NVARCHAR(50)) LIKE @like)
                OR (NombreTipoActividad LIKE @like)";

        /// <summary>
        /// Consulta SQL para contar el total de registros que cumplen con el filtro, reutilizando la cláusula WHERE para consistencia y seguridad.
        /// </summary>
        private const string SqlCount = @"
            SELECT COUNT(1)
            FROM dbo.AFI_CD_TIPO_ACTIVIDAD
        " + WhereClause + ";";

        /// <summary>
        /// Consulta SQL base para obtener la lista de tipos de actividades, reutilizando la cláusula WHERE y dejando espacio para agregar dinámicamente el ORDER BY según los parámetros de ordenamiento.
        /// </summary>
        private const string SqlListBase = @"
            SELECT
                CodTipoActividad,
                NombreTipoActividad,
                Activo
            FROM dbo.AFI_CD_TIPO_ACTIVIDAD
        " + WhereClause;

        public FrmAfCdTiposActividadesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        private static ErrorDto Ok() => DbHelper.CreateOkResponse();

        private static ErrorDto Error(string message) => DbHelper.ErrorResponse(message);

        /// <summary>
        /// Registra un movimiento en bitácora utilizando el MSecurityMainDb, asegurando que los datos de entrada estén validados y normalizados antes de la inserción.
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

        private static (string OrderBy, string Direction) SanitizeOrderBy(string? sortField, int? sortOrder)
        {
            var field = (sortField ?? string.Empty).Trim().ToLowerInvariant();

            var orderBy = field switch
            {
                "codtipoactividad" => OrderByCodTipoActividad,
                "nombretipoactividad" => OrderByNombreTipoActividad,
                "activo" => OrderByActivo,
                _ => OrderByCodTipoActividad
            };

            var direction = sortOrder == 1 ? "DESC" : "ASC";
            return (orderBy, direction);
        }

        /// <summary>
        /// Construye la cláusula ORDER BY de forma segura utilizando CASE para evitar inyección SQL, permitiendo ordenar por los campos permitidos según los parámetros recibidos.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        private static string BuildOrderByClause(string direction)
        {
            return $@"
            ORDER BY
                CASE WHEN @orderBy = '{OrderByCodTipoActividad}' THEN CodTipoActividad END {direction},
                CASE WHEN @orderBy = '{OrderByNombreTipoActividad}' THEN NombreTipoActividad END {direction},
                CASE WHEN @orderBy = '{OrderByActivo}' THEN Activo END {direction}";
        }

        /// <summary>
        /// Obtiene la lista de tipos de actividades con soporte para filtrado, ordenamiento y paginación, utilizando consultas parametrizadas para garantizar la seguridad y consistencia de los datos. Permite exportar sin paginación cuando se indica.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="esExportar"></param>
        /// <returns></returns>
        public ErrorDto<CDTiposActividadesLista> AfCdTiposActividadesLista_Obtener(
            int codEmpresa,
            FiltrosLazyLoadData filtros,
            bool esExportar)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, (SqlConnection conn) =>
            {
                filtros ??= new FiltrosLazyLoadData();

                var texto = filtros.filtro?.Trim();
                var hasFiltro = !string.IsNullOrWhiteSpace(texto);
                var usarPaginacion = filtros.paginacion > 0 && !esExportar;

                var (orderBy, direction) = SanitizeOrderBy(filtros.sortField, filtros.sortOrder);

                var parameters = new
                {
                    filtro = hasFiltro ? texto : null,
                    like = hasFiltro ? $"%{texto}%" : null,
                    orderBy,
                    offset = filtros.pagina,
                    fetch = filtros.paginacion
                };

                var total = conn.QuerySingle<int>(SqlCount, parameters);

                var sqlList = SqlListBase + BuildOrderByClause(direction);

                if (usarPaginacion)
                {
                    sqlList += " OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";
                }
                else
                {
                    sqlList += ";";
                }

                var lista = conn.Query<CDTiposActividadesData>(sqlList, parameters).ToList();

                return new CDTiposActividadesLista
                {
                    total = total,
                    lista = lista
                };
            });
        }

        /// <summary>
        /// Guarda un tipo de actividad, realizando un UPSERT para insertar o actualizar según la existencia del código. Valida los datos de entrada y registra el movimiento en bitácora con el detalle correspondiente. Utiliza consultas parametrizadas para garantizar la seguridad de la operación.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto AfCdTiposActividades_Guardar(int codEmpresa, string usuario, CDTiposActividadesData datos)
        {
            if (datos is null)
            {
                return Error("Datos requeridos.");
            }

            if (string.IsNullOrWhiteSpace(datos.CodTipoActividad))
            {
                return Error("El campo 'CodTipoActividad' es requerido.");
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Error("El usuario es requerido.");
            }

            const string sqlUpsert = @"
                DECLARE @accion NVARCHAR(10);

                UPDATE dbo.AFI_CD_TIPO_ACTIVIDAD
                SET
                    NombreTipoActividad = @NombreTipoActividad,
                    Activo = @Activo
                WHERE CodTipoActividad = @CodTipoActividad;

                IF @@ROWCOUNT = 0
                BEGIN
                    INSERT INTO dbo.AFI_CD_TIPO_ACTIVIDAD
                    (
                        CodTipoActividad,
                        NombreTipoActividad,
                        Activo,
                        RegistroFecha,
                        RegistroUsuario
                    )
                    VALUES
                    (
                        UPPER(@CodTipoActividad),
                        @NombreTipoActividad,
                        @Activo,
                        dbo.MyGetdate(),
                        @usuario
                    );

                    SET @accion = N'insert';
                END
                ELSE
                BEGIN
                    SET @accion = N'update';
                END

                SELECT @accion AS accion;";

            var upsert = DbHelper.ExecuteSingleQuery<string>(
                _portalDB,
                codEmpresa,
                sqlUpsert,
                defaultValue: string.Empty,
                parameters: new
                {
                    datos.CodTipoActividad,
                    datos.NombreTipoActividad,
                    datos.Activo,
                    usuario
                });

            if (upsert.Code != 0)
            {
                return Error("No fue posible guardar el tipo de actividad.");
            }

            var accion = (upsert.Result ?? string.Empty).ToLowerInvariant();
            var movimiento = GetMovimientoByAccion(accion);

            if (!string.IsNullOrWhiteSpace(movimiento))
            {
                LogBitacora(
                    codEmpresa,
                    usuario,
                    $"Tipos de Actividad Id: {datos.CodTipoActividad}",
                    movimiento);
            }

            return Ok();
        }

        /// <summary>
        /// Elimina un tipo de actividad por su código, validando que el código y el usuario sean proporcionados. Registra el movimiento en bitácora si la eliminación fue exitosa. Utiliza una consulta parametrizada para garantizar la seguridad de la operación.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="codTipoActividad"></param>
        /// <returns></returns>
        public ErrorDto AfCdTiposActividades_Eliminar(int codEmpresa, string usuario, string codTipoActividad)
        {
            if (string.IsNullOrWhiteSpace(codTipoActividad))
            {
                return Error("El 'codTipoActividad' es requerido.");
            }

            const string sql = @"
                DELETE FROM dbo.AFI_CD_TIPO_ACTIVIDAD
                WHERE CodTipoActividad = @CodTipoActividad;";

            var result = DbHelper.ExecuteNonQueryWithResult(
                _portalDB,
                codEmpresa,
                sql,
                new { CodTipoActividad = codTipoActividad });

            if (result.Code != 0)
            {
                return Error("No fue posible eliminar el tipo de actividad.");
            }

            if (result.Result > 0)
            {
                LogBitacora(
                    codEmpresa,
                    usuario,
                    $"Tipos de Actividad Id: {codTipoActividad}",
                    MovElimina);
            }

            return Ok();
        }

        /// <summary>
        /// Obtiene el tipo de movimiento para bitácora según la acción realizada (insertar o actualizar), permitiendo centralizar la lógica de asignación de movimientos y facilitar su mantenimiento. Retorna una cadena vacía si la acción no es reconocida, lo que puede ser útil para evitar registrar movimientos no definidos.
        /// </summary>
        /// <param name="accion"></param>
        /// <returns></returns>
        private static string GetMovimientoByAccion(string accion)
        {
            return accion switch
            {
                "insert" => MovRegistra,
                "update" => MovModifica,
                _ => string.Empty
            };
        }
    }
}