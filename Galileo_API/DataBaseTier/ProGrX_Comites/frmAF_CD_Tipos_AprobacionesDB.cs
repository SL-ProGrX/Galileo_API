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
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;

        private const string MovRegistra = "REGISTRA - WEB";
        private const string MovModifica = "MODIFICA - WEB";
        private const string MovElimina = "ELIMINAR - WEB";
        private const int ModuloCxC = 40;

        private const string OrderByCodTipoAprobacion = "CodTipoAprobacion";
        private const string OrderByNombreTipoAprobacion = "NombreTipoAprobacion";
        private const string OrderByActivo = "Activo";
        private const string BitacoraDetalleFormato = "Tipos de Aprobación Id: {0}";

        private const string WhereClause = @"
            WHERE
                (@filtro IS NULL)
                OR (CAST(CodTipoAprobacion AS NVARCHAR(50)) LIKE @like)
                OR (NombreTipoAprobacion LIKE @like)";

        private const string SqlCount = @"
            SELECT COUNT(1)
            FROM dbo.AFI_CD_TIPO_APROBACION
        " + WhereClause + ";";

        private const string SqlListBase = @"
            SELECT
                CodTipoAprobacion,
                NombreTipoAprobacion,
                Activo
            FROM dbo.AFI_CD_TIPO_APROBACION
        " + WhereClause;

        public FrmAfCdTiposAprobacionesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        private static ErrorDto Ok() => DbHelper.CreateOkResponse();

        private static ErrorDto Error(string message) => DbHelper.ErrorResponse(message);

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

        private void RegistrarBitacoraTipoAprobacion(
            int codEmpresa,
            string usuario,
            string codTipoAprobacion,
            string movimiento)
        {
            if (string.IsNullOrWhiteSpace(movimiento))
            {
                return;
            }

            LogBitacora(
                codEmpresa,
                usuario,
                string.Format(BitacoraDetalleFormato, codTipoAprobacion),
                movimiento);
        }

        private static (string OrderBy, string Direction) SanitizeOrderBy(string? sortField, int? sortOrder)
        {
            var field = (sortField ?? string.Empty).Trim().ToLowerInvariant();

            var orderBy = field switch
            {
                "codtipoaprobacion" => OrderByCodTipoAprobacion,
                "nombretipoaprobacion" => OrderByNombreTipoAprobacion,
                "activo" => OrderByActivo,
                _ => OrderByCodTipoAprobacion
            };

            var direction = sortOrder == 1 ? "DESC" : "ASC";
            return (orderBy, direction);
        }

        private static string BuildOrderByClause(string direction)
        {
            return $@"
            ORDER BY
                CASE WHEN @orderBy = '{OrderByCodTipoAprobacion}' THEN CodTipoAprobacion END {direction},
                CASE WHEN @orderBy = '{OrderByNombreTipoAprobacion}' THEN NombreTipoAprobacion END {direction},
                CASE WHEN @orderBy = '{OrderByActivo}' THEN Activo END {direction}";
        }

        private ErrorDto EjecutarGuardadoTipoAprobacion(
            int codEmpresa,
            string usuario,
            CdTiposAprobacionesData datos,
            out string movimiento)
        {
            movimiento = string.Empty;

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
                return Error("No fue posible guardar el tipo de aprobación.");
            }

            var accion = (upsert.Result ?? string.Empty).Trim().ToLowerInvariant();
            movimiento = GetMovimientoByAccion(accion);

            return Ok();
        }

        private ErrorDto EjecutarEliminacionTipoAprobacion(
            int codEmpresa,
            string codTipoAprobacion,
            out bool eliminado)
        {
            eliminado = false;

            const string sql = @"
                DELETE FROM dbo.AFI_CD_TIPO_APROBACION
                WHERE CodTipoAprobacion = @CodTipoAprobacion;";

            var result = DbHelper.ExecuteNonQueryWithResult(
                _portalDB,
                codEmpresa,
                sql,
                new { CodTipoAprobacion = codTipoAprobacion });

            if (result.Code != 0)
            {
                return Error("No fue posible eliminar el tipo de aprobación.");
            }

            eliminado = result.Result > 0;
            return Ok();
        }

        public ErrorDto<CdTiposAprobacionesLista> AfCdTiposAprobacionesLista_Obtener(
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
            if (datos == null)
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

            string movimiento;
            var response = EjecutarGuardadoTipoAprobacion(
                codEmpresa,
                usuario,
                datos,
                out movimiento);

            if (response.Code != 0)
            {
                return response;
            }

            RegistrarBitacoraTipoAprobacion(
                codEmpresa,
                usuario,
                datos.CodTipoAprobacion,
                movimiento);

            return response;
        }

        public ErrorDto AfCdTiposAprobaciones_Eliminar(
            int codEmpresa,
            string usuario,
            string codTipoAprobacion)
        {
            if (string.IsNullOrWhiteSpace(codTipoAprobacion))
            {
                return Error("El campo 'CodTipoAprobacion' es requerido.");
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Error("El usuario es requerido.");
            }

            bool eliminado;
            var response = EjecutarEliminacionTipoAprobacion(
                codEmpresa,
                codTipoAprobacion,
                out eliminado);

            if (response.Code != 0)
            {
                return response;
            }

            if (eliminado)
            {
                RegistrarBitacoraTipoAprobacion(
                    codEmpresa,
                    usuario,
                    codTipoAprobacion,
                    MovElimina);
            }

            return response;
        }

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