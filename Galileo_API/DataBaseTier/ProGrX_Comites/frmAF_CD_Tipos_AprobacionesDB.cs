using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using static Galileo_API.Models.ProGrX_Comites.FrmAfCdTiposAprobaciones;
namespace Galileo_API.DataBaseTier.ProGrX_Comites
{
    public class FrmAfCdTiposAprobacionesDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;

        private const string MovRegistra = "REGISTRA - WEB";
        private const string MovModifica = "MODIFICA - WEB";
        private const string MovElimina = "ELIMINAR - WEB";
        private const int ModuloCxC = 40;

        private const string OrderByCodTipoAprobacion = "CodTipoAprobacion";
        private const string OrderByNombreTipoAprobacion = "NombreTipoAprobacion";
        private const string OrderByActivo = "Activo";


        public FrmAfCdTiposAprobacionesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Cláusula WHERE reutilizable para filtrar por código o nombre de tipo de aprobacion, usando parámetros para evitar SQL injection.
        /// </summary>
        private const string WhereClause = @"
            WHERE
                (@filtro IS NULL)
                OR (CAST(CodTipoAprobacion AS NVARCHAR(50)) LIKE @like)
                OR (NombreTipoAprobacion LIKE @like)";

        /// <summary>
        /// Consulta SQL para contar el total de registros que cumplen con el filtro, reutilizando la cláusula WHERE para consistencia y seguridad.
        /// </summary>
        private const string SqlCount = @"
            SELECT COUNT(1)
            FROM dbo.AFI_CD_TIPO_APROBACION
        " + WhereClause + ";";

        /// <summary>
        /// Consulta SQL base para obtener la lista de tipos de aprovacion, reutilizando la cláusula WHERE y dejando espacio para agregar dinámicamente el ORDER BY según los parámetros de ordenamiento.
        /// </summary>
        private const string SqlListBase = @"
            SELECT
                CodTipoAprobacion,
                NombreTipoAprobacion,
                Activo
            FROM dbo.AFI_CD_TIPO_APROBACION
        " + WhereClause;
            
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

        /// <summary>
        /// Valida y sanitiza los parámetros de ordenamiento recibidos desde la entrada, asignando un campo de ordenamiento predeterminado si el campo no es reconocido y estableciendo la dirección de ordenamiento según el valor del sortOrder. 
        /// </summary>
        /// <param name="sortField"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        private static (string OrderBy, string Direction) SanitizeOrderBy(string? sortField, int? sortOrder)
        {
            var field = (sortField ?? string.Empty).Trim().ToLowerInvariant();

            var orderBy = field switch
            {
                "CodTipoAprobacion" => OrderByCodTipoAprobacion,
                "NombreTipoAprobacion" => OrderByNombreTipoAprobacion,
                "activo" => OrderByActivo,
                _ => OrderByCodTipoAprobacion
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
                CASE WHEN @orderBy = '{OrderByCodTipoAprobacion}' THEN CodTipoAprobacion END {direction},
                CASE WHEN @orderBy = '{OrderByNombreTipoAprobacion}' THEN NombreTipoAprobacion END {direction},
                CASE WHEN @orderBy = '{OrderByActivo}' THEN Activo END {direction}";
        }

        /// <summary>
        /// Obtiene la lista de tipos de aprovaciones para filtrado, ordenamiento y paginación
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="esExportar"></param>
        /// <returns></returns>
        public ErrorDto<CdTiposAprobacionesLista> AfCdTiposAprobacionesLista_Obtener(int codEmpresa,FiltrosLazyLoadData filtros,bool esExportar)
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

                var lista = conn.Query<CdTiposAprobacionesData>(sqlList, parameters).ToList();

                return new CdTiposAprobacionesLista
                {
                    Total = total,
                    lista = lista
                };
            });
        }

        /// <summary>
        /// Guarda un tipo de aprobacion, realizando un UPSERT para insertar o actualizar según la existencia del código. Valida los datos de entrada y registra el movimiento en bitácora con el detalle correspondiente.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto AfCdTiposAprobaciones_Guardar(int codEmpresa, string usuario, CdTiposAprobacionesData datos)
        {
            if (datos is null)
            {
                return Error("Datos requeridos.");
            }

            if (string.IsNullOrWhiteSpace(datos.CodTipoAprobacion))
            {
                return Error("El campo 'CodTipoAprobacion' es requerido.");
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Error("El usuario es requerido.");
            }

            const string sqlUpsert = @"
                DECLARE @accion NVARCHAR(10);

                UPDATE dbo.AFI_CD_TIPO_APROBACION
                SET
                    NombreTipoAprobacion = @NombreTipoAprobacion,
                    Activo = @Activo
                WHERE CodTipoAprobacion = @CodTipoAprobacion;

                IF @@ROWCOUNT = 0
                BEGIN
                    INSERT INTO dbo.AFI_CD_TIPO_APROBACION
                    (
                        CodTipoAprobacion,
                        NombreTipoAprobacion,
                        Activo,
                        RegistroFecha,
                        RegistroUsuario
                    )
                    VALUES
                    (
                        UPPER(@CodTipoAprobacion),
                        @NombreTipoAprobacion,
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
                    datos.CodTipoAprobacion,
                    datos.NombreTipoAprobacion,
                    datos.Activo,
                    usuario
                });

            if (upsert.Code != 0)
            {
                return Error("No fue posible guardar el tipo de aprobacion.");
            }

            var accion = (upsert.Result ?? string.Empty).ToLowerInvariant();
            var movimiento = GetMovimientoByAccion(accion);

            if (!string.IsNullOrWhiteSpace(movimiento))
            {
                LogBitacora(
                    codEmpresa,
                    usuario,
                    $"Tipos de Aprobación Id: {datos.CodTipoAprobacion}",
                    movimiento);
            }

            return Ok();
        }

        /// <summary>
        /// Elimina un tipo de aprobacion por su código, validando que el código y el usuario sean proporcionados. Registra el movimiento en bitácora si la eliminación fue exitosa. Utiliza una consulta parametrizada para garantizar la seguridad de la operación.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="CodTipoAprobacion"></param>
        /// <returns></returns>
        public ErrorDto AfCdTiposAprobaciones_Eliminar(int codEmpresa, string usuario, string CodTipoAprobacion)
        {
            if (string.IsNullOrWhiteSpace(CodTipoAprobacion))
            {
                return Error("El 'CodTipoAprobacion' es requerido.");
            }

            const string sql = @"
                DELETE FROM dbo.AFI_CD_TIPO_APROBACION
                WHERE CodTipoAprobacion = @CodTipoAprobacion;";

            var result = DbHelper.ExecuteNonQueryWithResult(
                _portalDB,
                codEmpresa,
                sql,
                new { CodTipoAprobacion = CodTipoAprobacion });

            if (result.Code != 0)
            {
                return Error("No fue posible eliminar el tipo de aprovacion.");
            }

            if (result.Result > 0)
            {
                LogBitacora(
                    codEmpresa,
                    usuario,
                    $"Tipos de Aprobación Id: {CodTipoAprobacion}",
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