using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient; 
using static Galileo_API.Models.ProGrX_Comites.FrmAfCdTiposEstados;

namespace Galileo_API.DataBaseTier.ProGrX_Comites
{
    public class FrmAfCdTiposEstadosDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;

        private const string MovRegistra = "REGISTRA - WEB";
        private const string MovModifica = "MODIFICA - WEB";
        private const string MovElimina = "ELIMINAR - WEB";
        private const int ModuloCxC = 40;

        private const string OrderByCodEstado = "CodEstado";
        private const string OrderByNombreEstado = "NombreEstado";
        private const string OrderByActivo = "Activo";
        private const string BitacoraDetalleFormato = "Tipos de Estado Id: {0}";

        public FrmAfCdTiposEstadosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }


        /// <summary>
        /// Cláusula WHERE común para las consultas de conteo y listado, utilizando parámetros para filtro dinámico.
        /// </summary>
        private const string WhereClause = @"
            WHERE
                (@filtro IS NULL)
                OR (CAST(CodEstado AS NVARCHAR(50)) LIKE @like)
                OR (NombreEstado LIKE @like)";

        /// <summary>
        /// Consulta SQL para obtener el conteo total de registros que cumplen con el filtro, reutilizando la cláusula WHERE parametrizada.
        /// </summary>
        private const string SqlCount = @"
            SELECT COUNT(1)
            FROM dbo.AFI_CD_TIPOS_ESTADOS_CUENTAS
        " + WhereClause + ";";

        /// <summary>
        /// Consulta SQL base para obtener la lista de tipos de estados, reutilizando la cláusula WHERE parametrizada y dejando espacio para la cláusula ORDER BY dinámica.
        /// </summary>
        private const string SqlListBase = @"
            SELECT
                CodEstado,
                NombreEstado,
                Activo
            FROM dbo.AFI_CD_TIPOS_ESTADOS_CUENTAS
        " + WhereClause;

        /// <summary>
        /// Genera una respuesta de éxito genérica sin datos, para operaciones de guardado o eliminación que no requieren retornar información específica.
        /// </summary>
        /// <returns></returns>
        private static ErrorDto Ok() => DbHelper.CreateOkResponse();

        /// <summary>
        /// Genera una respuesta de error con un mensaje específico, utilizando un método auxiliar para mantener consistencia en el formato de las respuestas de error en toda la aplicación.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static ErrorDto Error(string message) => DbHelper.ErrorResponse(message);

        /// <summary>
        /// Registra un movimiento en la bitácora de seguridad utilizando el módulo específico para Cuentas por Cobrar, con detalles formateados que incluyen el código del estado afectado y el tipo de movimiento realizado (registro, modificación o eliminación).
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
        /// Registra un movimiento específico relacionado con los tipos de estados en la bitácora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="CodEstado"></param>
        /// <param name="movimiento"></param>
        private void RegistrarBitacoraTipoEstado(
            int codEmpresa,
            string usuario,
            string CodEstado,
            string movimiento)
        {
            if (string.IsNullOrWhiteSpace(movimiento))
            {
                return;
            }

            LogBitacora(
                codEmpresa,
                usuario,
                string.Format(BitacoraDetalleFormato, CodEstado),
                movimiento);
        }

        /// <summary>
        /// Valida y sanitiza los parámetros de ordenamiento recibidos desde la interfaz, asegurando que el campo por el cual se ordena sea uno de los permitidos y que la dirección de ordenamiento sea válida, para prevenir inyección SQL y garantizar un comportamiento predecible en las consultas de listado.
        /// </summary>
        /// <param name="sortField"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        private static (string OrderBy, string Direction) SanitizeOrderBy(string? sortField, int? sortOrder)
        {
            var field = (sortField ?? string.Empty).Trim().ToLowerInvariant();

            var orderBy = field switch
            {
                "CodEstado" => OrderByCodEstado,
                "NombreEstado" => OrderByNombreEstado,
                "activo" => OrderByActivo,
                _ => OrderByCodEstado
            };

            var direction = sortOrder == 1 ? "DESC" : "ASC";
            return (orderBy, direction);
        }

        /// <summary>
        /// Construye dinámicamente la cláusula ORDER BY para la consulta de listado, utilizando CASE para aplicar el ordenamiento solo al campo seleccionado, y aplicando la dirección de ordenamiento validada
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        private static string BuildOrderByClause(string direction)
        {
            return $@"
            ORDER BY
                CASE WHEN @orderBy = '{OrderByCodEstado}' THEN CodEstado END {direction},
                CASE WHEN @orderBy = '{OrderByNombreEstado}' THEN NombreEstado END {direction},
                CASE WHEN @orderBy = '{OrderByActivo}' THEN Activo END {direction}";
        }

        /// <summary>
        /// Ejecuta la operación de guardado (inserción o actualización) de un tipo de estado
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <param name="movimiento"></param>
        /// <returns></returns>
        private ErrorDto EjecutarGuardadoTipoEstado(
            int codEmpresa,
            string usuario,
            CdTiposEstadosData datos,
            out string movimiento)
        {
            movimiento = string.Empty;

            var codigo = datos.CodEstado!.Trim();

            const string sqlUpsert = @"
                DECLARE @accion NVARCHAR(10);

                UPDATE dbo.AFI_CD_TIPOS_ESTADOS_CUENTAS
                SET
                    NombreEstado = @NombreEstado,
                    Activo = @Activo,
                    Modifica_Fecha= dbo.MyGetdate(),
                    Modifica_Usuario=@usuario
                WHERE CodEstado = @CodEstado;

                IF @@ROWCOUNT = 0
                BEGIN
                    INSERT INTO dbo.AFI_CD_TIPOS_ESTADOS_CUENTAS
                    (
                        CodEstado,
                        NombreEstado,
                        Activo,
                        RegistroFecha,
                        RegistroUsuario
                    )
                    VALUES
                    (
                        UPPER(@CodEstado),
                        @NombreEstado,
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
                    CodEstado = codigo,
                    datos.NombreEstado,
                    datos.Activo,
                    usuario
                });

            if (upsert.Code != 0)
            {
                return Error("No fue posible guardar el tipo de estado.");
            }

            var accion = (upsert.Result ?? string.Empty).Trim().ToLowerInvariant();
            movimiento = GetMovimientoByAccion(accion);

            return Ok();
        }

        /// <summary>
        /// Ejecuta la operación de eliminación de un tipo de estado
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="CodEstado"></param>
        /// <param name="eliminado"></param>
        /// <returns></returns>
        private ErrorDto EjecutarEliminacionTipoEstado(
            int codEmpresa,
            string CodEstado,
            out bool eliminado)
        {
            eliminado = false;

            const string sql = @"
                DELETE FROM dbo.AFI_CD_TIPOS_ESTADOS_CUENTAS
                WHERE CodEstado = @CodEstado;";

            var result = DbHelper.ExecuteNonQueryWithResult(
                _portalDB,
                codEmpresa,
                sql,
                new {  CodEstado });

            if (result.Code != 0)
            {
                return Error("No fue posible eliminar el tipo de estado.");
            }

            eliminado = result.Result > 0;
            return Ok();
        }

        /// <summary>
        /// Obtiene la lista de tipos de estados con soporte para filtrado, ordenamiento y paginación, utilizando consultas parametrizadas para garantizar seguridad y eficiencia en el acceso a datos, y retornando un objeto que incluye tanto el total de registros que cumplen con el filtro como la lista de resultados para la página solicitada. El método también considera un escenario de exportación donde se omite la paginación para retornar todos los registros que cumplen con el filtro.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="esExportar"></param>
        /// <returns></returns>
        public ErrorDto<CdTiposEstadosLista> AfCdTiposEstadosLista_Obtener(
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

                var lista = conn.Query<CdTiposEstadosData>(sqlList, parameters).ToList();

                return new CdTiposEstadosLista
                {
                    Total = total,
                    lista = lista
                };
            });
        }

        /// <summary>
        /// Valida los datos de entrada para la operación de guardado de un tipo de estado, asegurando que se proporcionen los campos requeridos y que el usuario esté presente, antes de ejecutar la operación de inserción o actualización. Luego de realizar la operación, registra un movimiento en la bitácora indicando si se trató de un registro nuevo o una modificación, utilizando el código del estado como parte del detalle del movimiento para mantener un historial claro de las acciones realizadas sobre los tipos de estados. El método retorna una respuesta genérica de éxito o error dependiendo del resultado de la operación de guardado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto AfCdTiposEstados_Guardar(
            int codEmpresa,
            string usuario,
            CdTiposEstadosData datos)
        {
            if (datos == null)
            {
                return Error("Datos requeridos.");
            }

            if (string.IsNullOrWhiteSpace(datos.CodEstado))
            {
                return Error("El campo 'CodEstado' es requerido.");
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Error("El usuario es requerido.");
            }

      
            var response = EjecutarGuardadoTipoEstado(
                codEmpresa,
                usuario,
                datos,
                out var movimiento);

            if (response.Code != 0)
            {
                return response;
            }

            RegistrarBitacoraTipoEstado(
                codEmpresa,
                usuario,
                datos.CodEstado.Trim(),
                movimiento);

            return response;
        }

        /// <summary>
        /// Valida el código del estado a eliminar y el usuario que realiza la acción, asegurando que se proporcionen los datos necesarios antes de ejecutar la operación de eliminación. Luego de intentar eliminar el registro, si la operación fue exitosa y se eliminó un registro, registra un movimiento en la bitácora indicando la eliminación del tipo de estado, utilizando el código del estado como parte del detalle del movimiento para mantener un historial claro de las acciones realizadas sobre los tipos de estados. El método retorna una respuesta genérica de éxito o error dependiendo del resultado de la operación de eliminación.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="CodEstado"></param>
        /// <returns></returns>
        public ErrorDto AfCdTiposEstados_Eliminar(
            int codEmpresa,
            string usuario,
            string CodEstado)
        {
            if (string.IsNullOrWhiteSpace(CodEstado))
            {
                return Error("El campo 'CodEstado' es requerido.");
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Error("El usuario es requerido.");
            }

            var codigo = CodEstado.Trim();

            var response = EjecutarEliminacionTipoEstado(
             codEmpresa,
             codigo,
             out var eliminado);

            if (response.Code != 0)
            {
                return response;
            }

            if (eliminado)
            {
                RegistrarBitacoraTipoEstado(
                    codEmpresa,
                    usuario,
                    codigo,
                    MovElimina);
            }

            return response;
        }

        /// <summary>
        /// Determina el tipo de movimiento a registrar en la bitácora basado en la acción realizada (inserción o actualización), retornando una cadena descriptiva que indica si se trató de un registro nuevo o una modificación, para mantener un historial claro y consistente de las acciones realizadas sobre los tipos de estados. Si la acción no es reconocida, retorna una cadena vacía, lo que indica que no se registrará ningún movimiento en la bitácora.
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