using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient; 
using static Galileo_API.Models.ProGrX_Comites.FrmAfCdTiposProcesos;

namespace Galileo_API.DataBaseTier.ProGrX_Comites
{
    public class FrmAfCdTiposProcesosDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;

        private const string MovRegistra = "REGISTRA - WEB";
        private const string MovModifica = "MODIFICA - WEB";
        private const string MovElimina = "ELIMINAR - WEB";
        private const int ModuloCxC = 40;

        private const string CampoCodigo = "CodTipoProceso";
        private const string CampoNombre = "NombreTipoProceso";
        private const string CampoActivo = "Activo";

        private const string MensajeDatosRequeridos = "Datos requeridos.";
        private const string MensajeUsuarioRequerido = "El usuario es requerido.";
        private const string MensajeCodigoRequerido = "El campo 'CodTipoProceso' es requerido.";
        private const string MensajeGuardarError = "No fue posible guardar el tipo de Proceso.";
        private const string MensajeEliminarError = "No fue posible eliminar el tipo de Proceso.";
        private const string FormatoDetalleBitacora = "Tipos de Proceso Id: {0}";


        public FrmAfCdTiposProcesosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la lista de tipos de Procesos con filtrado, ordenamiento y paginación.
        /// </summary>
        private const string SqlWhere = @"
            WHERE
                (@filtro IS NULL)
                OR (CAST(CodTipoProceso AS NVARCHAR(50)) LIKE @like)
                OR (NombreTipoProceso LIKE @like)";

        /// <summary>
        /// Cuenta el total de registros que cumplen con el filtro para propósitos de paginación.
        /// </summary>
        private const string SqlCount = @"
            SELECT COUNT(1)
            FROM dbo.AFI_CD_TIPO_PROCESO
        " + SqlWhere + ";";

        /// <summary>
        /// Selecciona los registros que cumplen con el filtro, ordenados y paginados según los parámetros recibidos.
        /// </summary>
        private const string SqlListBase = @"
            SELECT
                CodTipoProceso,
                NombreTipoProceso,
                Activo
            FROM dbo.AFI_CD_TIPO_PROCESO
        " + SqlWhere;

        /// <summary>
        /// Realiza un UPSERT: intenta actualizar el registro y si no existe, lo inserta. Devuelve la acción realizada para propósitos de bitácora.
        /// </summary>
        private const string SqlUpsert = @"
            DECLARE @accion NVARCHAR(10);

            UPDATE dbo.AFI_CD_TIPO_PROCESO
            SET
                NombreTipoProceso = @NombreTipoProceso,
                Activo = @Activo,
                Modifica_Fecha = dbo.MyGetdate(),
                Modifica_Usuario = @usuario
            WHERE CodTipoProceso = @CodTipoProceso;

            IF @@ROWCOUNT = 0
            BEGIN
                INSERT INTO dbo.AFI_CD_TIPO_PROCESO
                (
                    CodTipoProceso,
                    NombreTipoProceso,
                    Activo,
                    RegistroFecha,
                    RegistroUsuario
                )
                VALUES
                (
                    UPPER(@CodTipoProceso),
                    @NombreTipoProceso,
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

        /// <summary>
        /// Elimina el registro con el código especificado. Se espera que la validación previa garantice que el código existe y que el usuario tiene permisos para eliminarlo.
        /// </summary>
        private const string SqlDelete = @"
            DELETE FROM dbo.AFI_CD_TIPO_PROCESO
            WHERE CodTipoProceso = @CodTipoProceso;";

        // Inicializa dependencias de acceso a datos.

        /// <summary>
        // Obtiene la lista filtrada, ordenada y paginada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="esExportar"></param>
        /// <returns></returns>

        public ErrorDto<CdTiposProcesosLista> AfCdTiposProcesosLista_Obtener(
            int codEmpresa,
            FiltrosLazyLoadData filtros,
            bool esExportar)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, (SqlConnection conn) =>
            {
                var query = BuildListQuery(filtros, esExportar);

                var total = conn.QuerySingle<int>(SqlCount, query.Parameters);
                var lista = conn.Query<CdTiposProcesosData>(query.Sql, query.Parameters).ToList();

                return new CdTiposProcesosLista
                {
                    Total = total,
                    lista = lista
                };
            });
        }

        /// <summary>
        // Obtiene la lista filtrada, ordenada y paginada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>

        public ErrorDto AfCdTiposProcesos_Guardar(int codEmpresa, string usuario, CdTiposProcesosData datos)
        {
            var error = ValidateSave(usuario, datos);
            if (error != null)
            {
                return error;
            }

            var codigo = datos.CodTipoProceso!.Trim();
            var resultado = DbHelper.ExecuteSingleQuery<string>(
                _portalDb,
                codEmpresa,
                SqlUpsert,
                defaultValue: string.Empty,
                parameters: new
                {
                    CodTipoProceso = codigo,
                    datos.NombreTipoProceso,
                    datos.Activo,
                    usuario
                });

            if (resultado.Code != 0)
            {
                return Fail(MensajeGuardarError);
            }

            var movimiento = ResolveMovement(resultado.Result);
            if (!string.IsNullOrWhiteSpace(movimiento))
            {
                RegistrarBitacora(codEmpresa, usuario, codigo, movimiento);
            }

            return Ok();
        }

        /// <summary>
        /// Elimina el tipo de Proceso especificado. Se espera que la validación previa garantice que el código existe y que el usuario tiene permisos para eliminarlo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="CodTipoProceso"></param>
        /// <returns></returns>
        public ErrorDto AfCdTiposProcesos_Eliminar(int codEmpresa, string usuario, string CodTipoProceso)
        {
            var error = ValidateDelete(usuario, CodTipoProceso);
            if (error != null)
            {
                return error;
            }

            var codigo = CodTipoProceso.Trim();
            var resultado = DbHelper.ExecuteNonQueryWithResult(
                _portalDb,
                codEmpresa,
                SqlDelete,
                new { CodTipoProceso = codigo });

            if (resultado.Code != 0)
            {
                return Fail(MensajeEliminarError);
            }

            if (resultado.Result > 0)
            {
                RegistrarBitacora(codEmpresa, usuario, codigo, MovElimina);
            }

            return Ok();
        }


        /// <summary>
        /// Registra un movimiento en la bitácora de seguridad con detalles del tipo de Proceso afectado. Se espera que los parámetros hayan sido validados previamente.
        /// </summary>
        /// <param name="empresaId"></param>
        /// <param name="usuario"></param>
        /// <param name="codigo"></param>
        /// <param name="movimiento"></param>
        private void RegistrarBitacora(int empresaId, string usuario, string codigo, string movimiento)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = empresaId,
                Usuario = usuario,
                DetalleMovimiento = string.Format(FormatoDetalleBitacora, codigo),
                Movimiento = movimiento,
                Modulo = ModuloCxC
            });
        }

        
        /// <summary>
        ///  Valida los datos requeridos para guardar.
        /// </summary>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private static ErrorDto? ValidateSave(string usuario, CdTiposProcesosData datos)
        {
            if (datos == null)
            {
                return Fail(MensajeDatosRequeridos);
            }

            if (string.IsNullOrWhiteSpace(datos.CodTipoProceso))
            {
                return Fail(MensajeCodigoRequerido);
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Fail(MensajeUsuarioRequerido);
            }

            return null;
        }

        /// <summary>
        /// valida los datos requeridos para eliminar.
        /// </summary>
        /// <param name="usuario"></param>
        /// <param name="CodTipoProceso"></param>
        /// <returns></returns>
        private static ErrorDto? ValidateDelete(string usuario, string CodTipoProceso)
        {
            if (string.IsNullOrWhiteSpace(CodTipoProceso))
            {
                return Fail(MensajeCodigoRequerido);
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Fail(MensajeUsuarioRequerido);
            }

            return null;
        }

        /// <summary>
        ///  Resuelve el campo y dirección de ordenamiento.
        /// </summary>
        /// <param name="sortField"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        private static (string OrderBy, string Direction) ResolveOrder(string? sortField, int? sortOrder)
        {
            var normalizedField = (sortField ?? string.Empty).Trim().ToLowerInvariant();

            var orderBy = normalizedField switch
            {
                "CodTipoProceso" => CampoCodigo,
                "NombreTipoProceso" => CampoNombre,
                "activo" => CampoActivo,
                _ => CampoCodigo
            };

            var direction = sortOrder == 1 ? "DESC" : "ASC";
            return (orderBy, direction);
        }

        /// <summary>
        ///  Construye el SQL final y sus parámetros.
        /// </summary>
        /// <param name="filtros"></param>
        /// <param name="esExportar"></param>
        /// <returns></returns>
        private static ListQuery BuildListQuery(FiltrosLazyLoadData filtros, bool esExportar)
        {
            filtros ??= new FiltrosLazyLoadData();

            var texto = filtros.filtro?.Trim();
            var hasFiltro = !string.IsNullOrWhiteSpace(texto);
            var usePagination = filtros.paginacion > 0 && !esExportar;
            var (orderBy, direction) = ResolveOrder(filtros.sortField, filtros.sortOrder);

            var parameters = new
            {
                filtro = hasFiltro ? texto : null,
                like = hasFiltro ? $"%{texto}%" : null,
                orderBy,
                offset = filtros.pagina,
                fetch = filtros.paginacion
            };

            var sql = SqlListBase + BuildOrderByClause(direction);
            sql += usePagination
                ? " OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;"
                : ";";

            return new ListQuery(sql, parameters);
        }

        /// <summary>
        ///  Genera la cláusula ORDER BY segura.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        private static string BuildOrderByClause(string direction)
        {
            return $@"
            ORDER BY
                CASE WHEN @orderBy = '{CampoCodigo}' THEN CodTipoProceso END {direction},
                CASE WHEN @orderBy = '{CampoNombre}' THEN NombreTipoProceso END {direction},
                CASE WHEN @orderBy = '{CampoActivo}' THEN Activo END {direction}";
        }

        /// <summary>
        ///  Traduce la acción devuelta por SQL al movimiento de bitácora.
        /// </summary>
        /// <param name="accion"></param>
        /// <returns></returns>
        private static string ResolveMovement(string? accion)
        {
            return (accion ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "insert" => MovRegistra,
                "update" => MovModifica,
                _ => string.Empty
            };
        }

        /// <summary>
        ///  Devuelve una respuesta exitosa estándar.
        /// </summary>
        /// <returns></returns>
        private static ErrorDto Ok() => DbHelper.CreateOkResponse();

        /// <summary>
        ///  Devuelve una respuesta de error estándar.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static ErrorDto Fail(string message) => DbHelper.ErrorResponse(message);

        /// <summary>
        ///  Encapsula el SQL final y sus parámetros.
        /// </summary>
        /// <param name="Sql"></param>
        /// <param name="Parameters"></param>
        private sealed record ListQuery(string Sql, object Parameters);
    }
}