using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using static Galileo_API.Models.ProGrX_Comites.FrmAfCdTiposEstados;

namespace Galileo_API.DataBaseTier.ProGrX_Comites
{
    public class FrmAfCdTiposEstadosDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;

        private const string MovRegistra = "REGISTRA - WEB";
        private const string MovModifica = "MODIFICA - WEB";
        private const string MovElimina = "ELIMINAR - WEB";
        private const int ModuloCxC = 40;

        private const string CampoCodigo = "CodEstado";
        private const string CampoNombre = "NombreEstado";
        private const string CampoActivo = "Activo";

        private const string MensajeDatosRequeridos = "Datos requeridos.";
        private const string MensajeUsuarioRequerido = "El usuario es requerido.";
        private const string MensajeCodigoRequerido = "El campo 'CodEstado' es requerido.";
        private const string MensajeGuardarError = "No fue posible guardar el tipo de estado.";
        private const string MensajeEliminarError = "No fue posible eliminar el tipo de estado.";
        private const string FormatoDetalleBitacora = "Tipos de Estado Id: {0}";

        private const string SqlWhere = @"
            WHERE
                (@filtro IS NULL)
                OR (CAST(CodEstado AS NVARCHAR(50)) LIKE @like)
                OR (NombreEstado LIKE @like)";

        private const string SqlCount = @"
            SELECT COUNT(1)
            FROM dbo.AFI_CD_TIPOS_ESTADOS_CUENTAS
        " + SqlWhere + ";";

        private const string SqlListBase = @"
            SELECT
                CodEstado,
                NombreEstado,
                Activo
            FROM dbo.AFI_CD_TIPOS_ESTADOS_CUENTAS
        " + SqlWhere;

        private const string SqlUpsert = @"
            DECLARE @accion NVARCHAR(10);

            UPDATE dbo.AFI_CD_TIPOS_ESTADOS_CUENTAS
            SET
                NombreEstado = @NombreEstado,
                Activo = @Activo,
                Modifica_Fecha = dbo.MyGetdate(),
                Modifica_Usuario = @usuario
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

        private const string SqlDelete = @"
            DELETE FROM dbo.AFI_CD_TIPOS_ESTADOS_CUENTAS
            WHERE CodEstado = @CodEstado;";

        // Inicializa dependencias de acceso a datos.
        public FrmAfCdTiposEstadosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        // Obtiene la lista filtrada, ordenada y paginada.
        public ErrorDto<CdTiposEstadosLista> AfCdTiposEstadosLista_Obtener(
            int codEmpresa,
            FiltrosLazyLoadData filtros,
            bool esExportar)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, (SqlConnection conn) =>
            {
                var query = BuildListQuery(filtros, esExportar);

                var total = conn.QuerySingle<int>(SqlCount, query.Parameters);
                var lista = conn.Query<CdTiposEstadosData>(query.Sql, query.Parameters).ToList();

                return new CdTiposEstadosLista
                {
                    Total = total,
                    lista = lista
                };
            });
        }

        // Guarda o actualiza el tipo de estado.
        public ErrorDto AfCdTiposEstados_Guardar(int codEmpresa, string usuario, CdTiposEstadosData datos)
        {
            var error = ValidateSave(usuario, datos);
            if (error != null)
            {
                return error;
            }

            var codigo = datos.CodEstado!.Trim();
            var resultado = DbHelper.ExecuteSingleQuery<string>(
                _portalDb,
                codEmpresa,
                SqlUpsert,
                defaultValue: string.Empty,
                parameters: new
                {
                    CodEstado = codigo,
                    datos.NombreEstado,
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

        // Elimina el registro y registra bitácora si aplica.
        public ErrorDto AfCdTiposEstados_Eliminar(int codEmpresa, string usuario, string codEstado)
        {
            var error = ValidateDelete(usuario, codEstado);
            if (error != null)
            {
                return error;
            }

            var codigo = codEstado.Trim();
            var resultado = DbHelper.ExecuteNonQueryWithResult(
                _portalDb,
                codEmpresa,
                SqlDelete,
                new { CodEstado = codigo });

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

        // Registra el movimiento en bitácora.
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

        // Valida los datos requeridos para guardar.
        private static ErrorDto? ValidateSave(string usuario, CdTiposEstadosData datos)
        {
            if (datos == null)
            {
                return Fail(MensajeDatosRequeridos);
            }

            if (string.IsNullOrWhiteSpace(datos.CodEstado))
            {
                return Fail(MensajeCodigoRequerido);
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Fail(MensajeUsuarioRequerido);
            }

            return null;
        }

        // Valida los datos requeridos para eliminar.
        private static ErrorDto? ValidateDelete(string usuario, string codEstado)
        {
            if (string.IsNullOrWhiteSpace(codEstado))
            {
                return Fail(MensajeCodigoRequerido);
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Fail(MensajeUsuarioRequerido);
            }

            return null;
        }

        // Resuelve el campo y dirección de ordenamiento.
        private static (string OrderBy, string Direction) ResolveOrder(string? sortField, int? sortOrder)
        {
            var normalizedField = (sortField ?? string.Empty).Trim().ToLowerInvariant();

            var orderBy = normalizedField switch
            {
                "codestado" => CampoCodigo,
                "nombreestado" => CampoNombre,
                "activo" => CampoActivo,
                _ => CampoCodigo
            };

            var direction = sortOrder == 1 ? "DESC" : "ASC";
            return (orderBy, direction);
        }

        // Construye el SQL final y sus parámetros.
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

        // Genera la cláusula ORDER BY segura.
        private static string BuildOrderByClause(string direction)
        {
            return $@"
            ORDER BY
                CASE WHEN @orderBy = '{CampoCodigo}' THEN CodEstado END {direction},
                CASE WHEN @orderBy = '{CampoNombre}' THEN NombreEstado END {direction},
                CASE WHEN @orderBy = '{CampoActivo}' THEN Activo END {direction}";
        }

        // Traduce la acción devuelta por SQL al movimiento de bitácora.
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

        // Encapsula el SQL final y sus parámetros.
        private sealed record ListQuery(string Sql, object Parameters);
    }
}