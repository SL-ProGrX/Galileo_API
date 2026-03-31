using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using static Galileo_API.Models.ProGrX_Comites.FrmAfCdTiposDesembolsos;

namespace Galileo_API.DataBaseTier.ProGrX_Comites
{
    public class FrmAfCdTiposDesembolsosDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;

        private const string MovimientoRegistro = "REGISTRA - WEB";
        private const string MovimientoModificacion = "MODIFICA - WEB";
        private const string MovimientoEliminacion = "ELIMINAR - WEB";
        private const int ModuloBitacora = 40;

        private const string CampoCodigo = "CodTipoCuenta";
        private const string CampoNombre = "NombreTipoCuenta";
        private const string CampoActivo = "Activo";

        private const string MensajeDatosRequeridos = "Datos requeridos.";
        private const string MensajeUsuarioRequerido = "El usuario es requerido.";
        private const string MensajeCodigoRequerido = "El campo 'CodTipoCuenta' es requerido.";
        private const string MensajeGuardarError = "No fue posible guardar el tipo de desembolso.";
        private const string MensajeEliminarError = "No fue posible eliminar el tipo de desembolso.";

        private const string SqlWhereFiltro = @"
            WHERE
                (@filtro IS NULL)
                OR (CAST(CodTipoCuenta AS NVARCHAR(50)) LIKE @like)
                OR (NombreTipoCuenta LIKE @like)";

        private const string SqlContar = @"
            SELECT COUNT(1)
            FROM dbo.AFI_CD_TIPO_CUENTA
        " + SqlWhereFiltro + ";";

        private const string SqlSeleccionBase = @"
            SELECT
                CodTipoCuenta,
                NombreTipoCuenta,
                Activo
            FROM dbo.AFI_CD_TIPO_CUENTA
        " + SqlWhereFiltro;

        private const string SqlGuardar = @"
            DECLARE @accion NVARCHAR(10);

            UPDATE dbo.AFI_CD_TIPO_CUENTA
            SET
                NombreTipoCuenta = @NombreTipoCuenta,
                Activo = @Activo,
                Modifica_Fecha = dbo.MyGetdate(),
                Modifica_Usuario = @usuario
            WHERE CodTipoCuenta = @CodTipoCuenta;

            IF @@ROWCOUNT = 0
            BEGIN
                INSERT INTO dbo.AFI_CD_TIPO_CUENTA
                (
                    CodTipoCuenta,
                    NombreTipoCuenta,
                    Activo,
                    RegistroFecha,
                    RegistroUsuario
                )
                VALUES
                (
                    UPPER(@CodTipoCuenta),
                    @NombreTipoCuenta,
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

        private const string SqlEliminar = @"
            DELETE FROM dbo.AFI_CD_TIPO_CUENTA
            WHERE CodTipoCuenta = @CodTipoCuenta;";

        public FrmAfCdTiposDesembolsosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        public ErrorDto<CdTiposDesembolsosLista> AfCdTiposDesembolsosLista_Obtener(
            int codEmpresa,
            FiltrosLazyLoadData filtros,
            bool esExportar)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, (SqlConnection conn) =>
            {
                var filtrosNormalizados = filtros ?? new FiltrosLazyLoadData();
                var parametros = BuildListParameters(filtrosNormalizados, esExportar);
                var total = conn.QuerySingle<int>(SqlContar, parametros);

                var sql = ComposeListSql(parametros.Direction, parametros.UsePagination);
                var registros = conn.Query<CdTiposDesembolsosData>(sql, new
                {
                    filtro = parametros.Filter,
                    like = parametros.Like,
                    orderBy = parametros.OrderBy,
                    offset = parametros.Offset,
                    fetch = parametros.Fetch
                }).ToList();

                return CreateListResponse(total, registros);
            });
        }

        public ErrorDto AfCdTiposDesembolsos_Guardar(
            int codEmpresa,
            string usuario,
            CdTiposDesembolsosData datos)
        {
            var errorValidacion = ValidateSaveRequest(usuario, datos);
            if (errorValidacion != null)
            {
                return errorValidacion;
            }

            var resultado = DbHelper.ExecuteSingleQuery<string>(
                _portalDb,
                codEmpresa,
                SqlGuardar,
                defaultValue: string.Empty,
                parameters: new
                {
                    datos.CodTipoCuenta,
                    datos.NombreTipoCuenta,
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
                RegistrarBitacora(codEmpresa, usuario, datos.CodTipoCuenta!, movimiento);
            }

            return Success();
        }

        public ErrorDto AfCdTiposDesembolsos_Eliminar(
            int codEmpresa,
            string usuario,
            string codTipoCuenta)
        {
            var errorValidacion = ValidateDeleteRequest(usuario, codTipoCuenta);
            if (errorValidacion != null)
            {
                return errorValidacion;
            }

            var resultado = DbHelper.ExecuteNonQueryWithResult(
                _portalDb,
                codEmpresa,
                SqlEliminar,
                new
                {
                    CodTipoCuenta = codTipoCuenta
                });

            if (resultado.Code != 0)
            {
                return Fail(MensajeEliminarError);
            }

            if (resultado.Result > 0)
            {
                RegistrarBitacora(codEmpresa, usuario, codTipoCuenta, MovimientoEliminacion);
            }

            return Success();
        }

        private void RegistrarBitacora(int empresaId, string usuario, string codigo, string movimiento)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = empresaId,
                Usuario = usuario,
                DetalleMovimiento = BuildLogDetail(codigo),
                Movimiento = movimiento,
                Modulo = ModuloBitacora
            });
        }

        private static string BuildLogDetail(string codigo)
        {
            return $"Tipos de Desembolso Id: {codigo}";
        }

        private static ErrorDto? ValidateSaveRequest(string usuario, CdTiposDesembolsosData datos)
        {
            if (datos == null)
            {
                return Fail(MensajeDatosRequeridos);
            }

            if (string.IsNullOrWhiteSpace(datos.CodTipoCuenta))
            {
                return Fail(MensajeCodigoRequerido);
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Fail(MensajeUsuarioRequerido);
            }

            return null;
        }

        private static ErrorDto? ValidateDeleteRequest(string usuario, string codTipoCuenta)
        {
            if (string.IsNullOrWhiteSpace(codTipoCuenta))
            {
                return Fail(MensajeCodigoRequerido);
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return Fail(MensajeUsuarioRequerido);
            }

            return null;
        }

        private static (string OrderBy, string Direction) ResolveSort(string? sortField, int? sortOrder)
        {
            var normalized = (sortField ?? string.Empty).Trim().ToLowerInvariant();

            var orderBy = normalized switch
            {
                "codtipocuenta" => CampoCodigo,
                "nombretipocuenta" => CampoNombre,
                "activo" => CampoActivo,
                _ => CampoCodigo
            };

            var direction = sortOrder == 1 ? "DESC" : "ASC";
            return (orderBy, direction);
        }

        private static string ComposeListSql(string direction, bool usePagination)
        {
            var orderByClause = $@"
            ORDER BY
                CASE WHEN @orderBy = '{CampoCodigo}' THEN CodTipoCuenta END {direction},
                CASE WHEN @orderBy = '{CampoNombre}' THEN NombreTipoCuenta END {direction},
                CASE WHEN @orderBy = '{CampoActivo}' THEN Activo END {direction}";

            var pagingClause = usePagination
                ? " OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;"
                : ";";

            return SqlSeleccionBase + orderByClause + pagingClause;
        }

        private static string ResolveMovement(string? accion)
        {
            return (accion ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "insert" => MovimientoRegistro,
                "update" => MovimientoModificacion,
                _ => string.Empty
            };
        }

        private static CdTiposDesembolsosLista CreateListResponse(
            int total,
            List<CdTiposDesembolsosData> registros)
        {
            return new CdTiposDesembolsosLista
            {
                Total = total,
                lista = registros
            };
        }

        private static ListQueryParameters BuildListParameters(
            FiltrosLazyLoadData filtros,
            bool esExportar)
        {
            var textoFiltro = filtros.filtro?.Trim();
            var tieneFiltro = !string.IsNullOrWhiteSpace(textoFiltro);
            var usePagination = filtros.paginacion > 0 && !esExportar;
            var (orderBy, direction) = ResolveSort(filtros.sortField, filtros.sortOrder);

            return new ListQueryParameters
            {
                Filter = tieneFiltro ? textoFiltro : null,
                Like = tieneFiltro ? $"%{textoFiltro}%" : null,
                OrderBy = orderBy,
                Direction = direction,
                Offset = filtros.pagina,
                Fetch = filtros.paginacion,
                UsePagination = usePagination
            };
        }

        private static ErrorDto Success()
        {
            return DbHelper.CreateOkResponse();
        }

        private static ErrorDto Fail(string message)
        {
            return DbHelper.ErrorResponse(message);
        }

        private sealed class ListQueryParameters
        {
            public string? Filter { get; init; }
            public string? Like { get; init; }
            public string OrderBy { get; init; } = CampoCodigo;
            public string Direction { get; init; } = "ASC";
            public int Offset { get; init; }
            public int Fetch { get; init; }
            public bool UsePagination { get; init; }
        }
    }
}