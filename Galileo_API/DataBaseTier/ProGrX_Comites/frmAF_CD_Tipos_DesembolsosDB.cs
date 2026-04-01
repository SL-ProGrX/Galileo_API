using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
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

        /// <summary>
        /// Obtiene una lista de tipos de desembolso desde la base de datos, aplicando filtros de búsqueda, ordenamiento dinámico y paginación según los parámetros proporcionados, y devuelve el resultado junto con el total de registros para su consumo en el cliente.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="esExportar"></param>
        /// <returns></returns>
        public ErrorDto<CdTiposDesembolsosLista> AfCdTiposDesembolsosLista_Obtener(
      int codEmpresa,
      FiltrosLazyLoadData filtros,
      bool esExportar)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, (SqlConnection conn) =>
            {
                filtros ??= new FiltrosLazyLoadData();

                var texto = filtros.filtro?.Trim();
                var hasFiltro = !string.IsNullOrWhiteSpace(texto);
                var usarPaginacion = filtros.paginacion > 0 && !esExportar;
                var (orderBy, direction) = ResolveSort(filtros.sortField, filtros.sortOrder);

                var parameters = new
                {
                    filtro = hasFiltro ? texto : null,
                    like = hasFiltro ? $"%{texto}%" : null,
                    orderBy,
                    offset = filtros.pagina,
                    fetch = filtros.paginacion
                };

                var total = conn.QuerySingle<int>(SqlContar, parameters);

                var sql = ComposeListSql(direction, usarPaginacion);
                var registros = conn.Query<CdTiposDesembolsosData>(sql, parameters).ToList();

                return new CdTiposDesembolsosLista
                {
                    Total = total,
                    lista = registros
                };
            });
        }

        /// <summary>
        /// Guarda un tipo de desembolso, realizando una inserción o actualización según corresponda, y registra la acción en bitácora.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Elimina un tipo de desembolso por su código, y registra la acción en bitácora.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="codTipoCuenta"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Registra un movimiento en la bitácora de seguridad para auditoría de cambios en tipos de desembolso.
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
                DetalleMovimiento = BuildLogDetail(codigo),
                Movimiento = movimiento,
                Modulo = ModuloBitacora
            });
        }

        /// <summary>
        /// Construye el detalle del movimiento para la bitácora, incluyendo el código del tipo de desembolso afectado.
        /// </summary>
        /// <param name="codigo"></param>
        /// <returns></returns>
        private static string BuildLogDetail(string codigo)
        {
            return $"Tipos de Desembolso Id: {codigo}";
        }

        /// <summary>
        /// Valida los datos de entrada para la operación de guardado, asegurando que se proporcionen los campos requeridos y que el usuario esté presente.
        /// </summary>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Valida los datos de entrada para la operación de eliminación, asegurando que se proporcione el código del tipo de desembolso a eliminar y que el usuario esté presente.
        /// </summary>
        /// <param name="usuario"></param>
        /// <param name="codTipoCuenta"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Compone la consulta SQL para obtener la lista de tipos de desembolso, incluyendo cláusulas de ordenamiento dinámico y paginación según los parámetros proporcionados.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="usePagination"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Resuelve el tipo de movimiento para la bitácora basado en la acción realizada (inserción o actualización), utilizando los resultados devueltos por la consulta de guardado.
        /// </summary>
        /// <param name="accion"></param>
        /// <returns></returns>
        private static string ResolveMovement(string? accion)
        {
            return (accion ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "insert" => MovimientoRegistro,
                "update" => MovimientoModificacion,
                _ => string.Empty
            };
        }

        /// <summary>
        /// Construye la respuesta para la lista de tipos de desembolso, incluyendo el total de registros y la lista de datos obtenida, para ser consumida por el cliente.
        /// </summary>
        /// <param name="total"></param>
        /// <param name="registros"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Construye los parámetros de consulta para la obtención de la lista de tipos de desembolso.
        /// </summary>
        /// <param name="filtros"></param>
        /// <param name="esExportar"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Construye una respuesta de éxito genérica para operaciones que no requieren devolver datos específicos, indicando que la operación se realizó correctamente.
        /// </summary>
        /// <returns></returns>
        private static ErrorDto Success()
        {
            return DbHelper.CreateOkResponse();
        }

        /// <summary>
        /// Construye una respuesta de error genérica con un mensaje específico, para ser devuelta en caso de que ocurra un error durante las operaciones de guardado o eliminación.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private static ErrorDto Fail(string message)
        {
            return DbHelper.ErrorResponse(message);
        }

        /// <summary>
        /// Clase auxiliar para encapsular los parámetros de consulta utilizados en la obtención de la lista de tipos de desembolso, incluyendo filtros, ordenamiento y paginación, para simplificar la construcción de consultas dinámicas.
        /// </summary>
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