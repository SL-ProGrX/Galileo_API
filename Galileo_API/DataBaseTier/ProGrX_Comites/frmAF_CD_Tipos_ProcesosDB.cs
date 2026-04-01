using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
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
        private const string MensajeGuardarError = "No fue posible guardar el tipo de proceso.";
        private const string MensajeEliminarError = "No fue posible eliminar el tipo de proceso.";
        private const string FormatoDetalleBitacora = "Tipos de Proceso Id: {0}";

        private const string SqlWhere = @"
            WHERE
                (@filtro IS NULL)
                OR (CAST(CodTipoProceso AS NVARCHAR(50)) LIKE @like)
                OR (NombreTipoProceso LIKE @like)";

        private const string SqlCount = @"
            SELECT COUNT(1)
            FROM dbo.AFI_CD_TIPO_PROCESO
        " + SqlWhere + ";";

        private const string SqlListBase = @"
            SELECT
                CodTipoProceso,
                NombreTipoProceso,
                Activo
            FROM dbo.AFI_CD_TIPO_PROCESO
        " + SqlWhere;

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

        private const string SqlDelete = @"
            DELETE FROM dbo.AFI_CD_TIPO_PROCESO
            WHERE CodTipoProceso = @CodTipoProceso;";

        // Inicializa las dependencias de acceso a datos.
        public FrmAfCdTiposProcesosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        // Obtiene la lista filtrada, ordenada y paginada.
        public ErrorDto<CdTiposProcesosLista> AfCdTiposProcesosLista_Obtener(
            int codEmpresa,
            FiltrosLazyLoadData filtros,
            bool esExportar)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, (SqlConnection conn) =>
            {
                var request = CreateListRequest(filtros, esExportar);
                var total = conn.QuerySingle<int>(SqlCount, request.Parameters);
                var lista = conn.Query<CdTiposProcesosData>(request.Sql, request.Parameters).ToList();

                return new CdTiposProcesosLista
                {
                    Total = total,
                    lista = lista
                };
            });
        }

        // Guarda o actualiza el tipo de proceso.
        public ErrorDto AfCdTiposProcesos_Guardar(int codEmpresa, string usuario, CdTiposProcesosData datos)
        {
            var error = ValidateRequiredData(usuario, datos?.CodTipoProceso);
            if (error != null || datos == null)
            {
                return error ?? Fail(MensajeDatosRequeridos);
            }

            var codigo = datos.CodTipoProceso.Trim();
            var response = DbHelper.ExecuteSingleQuery<string>(
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

            if (response.Code != 0)
            {
                return Fail(MensajeGuardarError);
            }

            RegisterAuditIfNeeded(
                codEmpresa,
                usuario,
                codigo,
                ResolveMovement(response.Result));

            return Ok();
        }

        // Elimina el registro y registra bitácora si aplica.
        public ErrorDto AfCdTiposProcesos_Eliminar(int codEmpresa, string usuario, string codTipoProceso)
        {
            var error = ValidateRequiredData(usuario, codTipoProceso);
            if (error != null)
            {
                return error;
            }

            var codigo = codTipoProceso.Trim();
            var response = DbHelper.ExecuteNonQueryWithResult(
                _portalDb,
                codEmpresa,
                SqlDelete,
                new { CodTipoProceso = codigo });

            if (response.Code != 0)
            {
                return Fail(MensajeEliminarError);
            }

            if (response.Result > 0)
            {
                RegisterAuditIfNeeded(codEmpresa, usuario, codigo, MovElimina);
            }

            return Ok();
        }

        // Registra el movimiento en bitácora cuando corresponde.
        private void RegisterAuditIfNeeded(int empresaId, string usuario, string codigo, string movimiento)
        {
            if (string.IsNullOrWhiteSpace(movimiento))
            {
                return;
            }

            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = empresaId,
                Usuario = usuario,
                DetalleMovimiento = string.Format(FormatoDetalleBitacora, codigo),
                Movimiento = movimiento,
                Modulo = ModuloCxC
            });
        }

        // Valida usuario y código requeridos.
        private static ErrorDto? ValidateRequiredData(string usuario, string? codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return Fail(MensajeCodigoRequerido);
            }

            return string.IsNullOrWhiteSpace(usuario)
                ? Fail(MensajeUsuarioRequerido)
                : null;
        }

        // Resuelve el campo y dirección de ordenamiento.
        private static (string OrderBy, string Direction) ResolveOrder(string? sortField, int? sortOrder)
        {
            var normalized = (sortField ?? string.Empty).Trim().ToLowerInvariant();

            var orderBy = normalized switch
            {
                "codtipoproceso" => CampoCodigo,
                "nombretipoproceso" => CampoNombre,
                "activo" => CampoActivo,
                _ => CampoCodigo
            };

            return (orderBy, sortOrder == 1 ? "DESC" : "ASC");
        }

        // Construye el SQL de lista y sus parámetros.
        private static ListRequest CreateListRequest(FiltrosLazyLoadData filtros, bool esExportar)
        {
            filtros ??= new FiltrosLazyLoadData();

            var texto = filtros.filtro?.Trim();
            var hasFilter = !string.IsNullOrWhiteSpace(texto);
            var (orderBy, direction) = ResolveOrder(filtros.sortField, filtros.sortOrder);

            var parameters = new
            {
                filtro = hasFilter ? texto : null,
                like = hasFilter ? $"%{texto}%" : null,
                orderBy,
                offset = filtros.pagina,
                fetch = filtros.paginacion
            };

            var sql = SqlListBase + ComposeOrderBy(direction);
            sql += filtros.paginacion > 0 && !esExportar
                ? " OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;"
                : ";";

            return new ListRequest(sql, parameters);
        }

        // Genera la cláusula ORDER BY segura.
        private static string ComposeOrderBy(string direction)
        {
            return $@"
            ORDER BY
                CASE WHEN @orderBy = '{CampoCodigo}' THEN CodTipoProceso END {direction},
                CASE WHEN @orderBy = '{CampoNombre}' THEN NombreTipoProceso END {direction},
                CASE WHEN @orderBy = '{CampoActivo}' THEN Activo END {direction}";
        }

        // Traduce la acción SQL a movimiento de bitácora.
        private static string ResolveMovement(string? accion)
        {
            return (accion ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "insert" => MovRegistra,
                "update" => MovModifica,
                _ => string.Empty
            };
        }

        // Devuelve una respuesta exitosa estándar.
        private static ErrorDto Ok() => DbHelper.CreateOkResponse();

        // Devuelve una respuesta de error estándar.
        private static ErrorDto Fail(string message) => DbHelper.ErrorResponse(message);

        // Encapsula el SQL y parámetros de la consulta de lista.
        private sealed record ListRequest(string Sql, object Parameters);
    }
}