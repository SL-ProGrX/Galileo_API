using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using static Galileo_API.Models.ProGrX_Comites.FrmAfCdTiposAprobaciones;

namespace Galileo_API.DataBaseTier.ProGrX_Comites
{
    public class FrmAfCdTiposAprobacionesDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;

        private const string MovRegistra = "REGISTRA - WEB";
        private const string MovModifica = "MODIFICA - WEB";
        private const string MovElimina = "ELIMINAR - WEB";
        private const int ModuloCxC = 40;

        private const string CampoCodigo = "CodTipoAprobacion";
        private const string CampoNombre = "NombreTipoAprobacion";
        private const string CampoActivo = "Activo";

        private const string ErrorGuardar = "No fue posible guardar el tipo de aprobación.";
        private const string ErrorEliminar = "No fue posible eliminar el tipo de aprobación.";
        private const string ErrorUsuarioRequerido = "El usuario es requerido.";
        private const string ErrorDatosRequeridos = "Datos requeridos.";
        private const string ErrorCodigoRequerido = "El campo 'CodTipoAprobacion' es requerido.";
        private const string BitacoraDetalle = "Tipos de Aprobación Id: {0}";

        private const string SqlCount = @"
            SELECT COUNT(1)
            FROM dbo.AFI_CD_TIPO_APROBACION
            WHERE
                (@filtro IS NULL)
                OR (CAST(CodTipoAprobacion AS NVARCHAR(50)) LIKE @like)
                OR (NombreTipoAprobacion LIKE @like);";

        private const string SqlListBase = @"
            SELECT
                CodTipoAprobacion,
                NombreTipoAprobacion,
                Activo
            FROM dbo.AFI_CD_TIPO_APROBACION
            WHERE
                (@filtro IS NULL)
                OR (CAST(CodTipoAprobacion AS NVARCHAR(50)) LIKE @like)
                OR (NombreTipoAprobacion LIKE @like)";

        private const string SqlUpsert = @"
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

        private const string SqlDelete = @"
            DELETE FROM dbo.AFI_CD_TIPO_APROBACION
            WHERE CodTipoAprobacion = @CodTipoAprobacion;";

        public FrmAfCdTiposAprobacionesDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        public ErrorDto<CdTiposAprobacionesLista> AfCdTiposAprobacionesLista_Obtener(
            int codEmpresa,
            FiltrosLazyLoadData filtros,
            bool esExportar)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, (SqlConnection conn) =>
            {
                filtros ??= new FiltrosLazyLoadData();

                var filtroTexto = filtros.filtro?.Trim();
                var usarFiltro = !string.IsNullOrWhiteSpace(filtroTexto);
                var usarPaginacion = filtros.paginacion > 0 && !esExportar;
                var (orderBy, direction) = ResolveOrder(filtros.sortField, filtros.sortOrder);

                var parameters = new
                {
                    filtro = usarFiltro ? filtroTexto : null,
                    like = usarFiltro ? $"%{filtroTexto}%" : null,
                    orderBy,
                    offset = filtros.pagina,
                    fetch = filtros.paginacion
                };

                var total = conn.QuerySingle<int>(SqlCount, parameters);
                var sqlList = BuildSqlList(direction, usarPaginacion);
                var lista = conn.Query<CdTiposAprobacionesData>(sqlList, parameters).ToList();

                return new CdTiposAprobacionesLista
                {
                    Total = total,
                    lista = lista
                };
            });
        }

        public ErrorDto AfCdTiposAprobaciones_Guardar(
            int codEmpresa,
            string usuario,
            CdTiposAprobacionesData datos)
        {
            var validationError = ValidateGuardar(usuario, datos);
            if (validationError != null)
            {
                return validationError;
            }

            var upsert = DbHelper.ExecuteSingleQuery<string>(
                _portalDb,
                codEmpresa,
                SqlUpsert,
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
                return Error(ErrorGuardar);
            }

            var movimiento = ResolveMovimiento((upsert.Result ?? string.Empty).Trim());

            if (!string.IsNullOrWhiteSpace(movimiento))
            {
                RegistrarBitacora(codEmpresa, usuario, datos.CodTipoAprobacion, movimiento);
            }

            return Ok();
        }

        public ErrorDto AfCdTiposAprobaciones_Eliminar(
            int codEmpresa,
            string usuario,
            string codTipoAprobacion)
        {
            var validationError = ValidateEliminar(usuario, codTipoAprobacion);
            if (validationError != null)
            {
                return validationError;
            }

            var result = DbHelper.ExecuteNonQueryWithResult(
                _portalDb,
                codEmpresa,
                SqlDelete,
                new { CodTipoAprobacion = codTipoAprobacion });

            if (result.Code != 0)
            {
                return Error(ErrorEliminar);
            }

            if (result.Result > 0)
            {
                RegistrarBitacora(codEmpresa, usuario, codTipoAprobacion, MovElimina);
            }

            return Ok();
        }

        private void RegistrarBitacora(int empresaId, string usuario, string codigo, string movimiento)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = empresaId,
                Usuario = usuario,
                DetalleMovimiento = string.Format(BitacoraDetalle, codigo),
                Movimiento = movimiento,
                Modulo = ModuloCxC
            });
        }

        private static (string OrderBy, string Direction) ResolveOrder(string? sortField, int? sortOrder)
        {
            var normalizedField = (sortField ?? string.Empty).Trim().ToLowerInvariant();

            var orderBy = normalizedField switch
            {
                "codtipoaprobacion" => CampoCodigo,
                "nombretipoaprobacion" => CampoNombre,
                "activo" => CampoActivo,
                _ => CampoCodigo
            };

            var direction = sortOrder == 1 ? "DESC" : "ASC";
            return (orderBy, direction);
        }

        private static string BuildSqlList(string direction, bool usarPaginacion)
        {
            var sql = $@"
{SqlListBase}
ORDER BY
    CASE WHEN @orderBy = '{CampoCodigo}' THEN CodTipoAprobacion END {direction},
    CASE WHEN @orderBy = '{CampoNombre}' THEN NombreTipoAprobacion END {direction},
    CASE WHEN @orderBy = '{CampoActivo}' THEN Activo END {direction}";

            return usarPaginacion
                ? sql + " OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;"
                : sql + ";";
        }

        private static string ResolveMovimiento(string accion)
        {
            return accion.ToLowerInvariant() switch
            {
                "insert" => MovRegistra,
                "update" => MovModifica,
                _ => string.Empty
            };
        }

        private static ErrorDto? ValidateGuardar(string usuario, CdTiposAprobacionesData datos)
        {
            if (datos == null)
            {
                return Error(ErrorDatosRequeridos);
            }

            if (string.IsNullOrWhiteSpace(datos.CodTipoAprobacion))
            {
                return Error(ErrorCodigoRequerido);
            }

            return string.IsNullOrWhiteSpace(usuario)
                ? Error(ErrorUsuarioRequerido)
                : null;
        }

        private static ErrorDto? ValidateEliminar(string usuario, string codTipoAprobacion)
        {
            if (string.IsNullOrWhiteSpace(codTipoAprobacion))
            {
                return Error(ErrorCodigoRequerido);
            }

            return string.IsNullOrWhiteSpace(usuario)
                ? Error(ErrorUsuarioRequerido)
                : null;
        }

        private static ErrorDto Ok()
        {
            return DbHelper.CreateOkResponse();
        }

        private static ErrorDto Error(string message)
        {
            return DbHelper.ErrorResponse(message);
        }
    }
}