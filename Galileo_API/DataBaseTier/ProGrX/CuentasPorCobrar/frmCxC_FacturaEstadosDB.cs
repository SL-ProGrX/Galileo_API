
using Dapper;
using Microsoft.Data.SqlClient; // Necesario para WithConn
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.CuentasPorCobrar;
using Galileo.Models.Security;
using Galileo.Models;
using System.Text;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasPorCobrar
{
    public class FrmCxCFacturaEstadosDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;

        private const int ModuloCxC = 31;

        // Movimientos bitácora centralizados
        private const string MovRegistra = "Registra - WEB";
        private const string MovModifica = "MODIFICA - WEB";
        private const string MovElimina = "ELIMINAR - WEB";
        private const string Tabla = "dbo.CXC_FACTURAS_ESTADOS";

        public FrmCxCFacturaEstadosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config!);
        }
        internal static class SqlFragments
        {
            /// <summary>
            /// Construye un WHERE con búsqueda LIKE parametrizada sobre varias columnas.
            /// </summary>
            public static string BuildWhereLike(params string[] columns)
            {
                var sb = new StringBuilder();
                sb.AppendLine("WHERE");
                sb.AppendLine("    (@filtro IS NULL)");

                bool first = true;
                foreach (var col in columns)
                {
                    if (first)
                    {
                        // Primer columna con CAST para soportar numérico -> texto si aplica.
                        sb.AppendLine($"    OR (CAST({col} AS NVARCHAR(50)) LIKE @like)");
                        first = false;
                    }
                    else
                    {
                        sb.AppendLine($"    OR ({col} LIKE @like)");
                    }
                }
                return sb.ToString();
            }

            /// <summary>
            /// Genera ORDER BY con CASE (parametrizado por @orderBy). Sin interpolar valores del usuario.
            /// </summary>
            public static string BuildOrderByCase(string orderByParamName, bool desc, params string[] columns)
            {
                var sb = new StringBuilder();
                sb.AppendLine("ORDER BY");
                for (int i = 0; i < columns.Length; i++)
                {
                    var col = columns[i];
                    var comma = (i < columns.Length - 1) ? "," : string.Empty;
                    if (desc)
                    {
                        sb.AppendLine($"    CASE WHEN @{orderByParamName} = '{col}' THEN {col} END DESC{comma}");
                    }
                    else
                    {
                        sb.AppendLine($"    CASE WHEN @{orderByParamName} = '{col}' THEN {col} END{comma}");
                    }
                }
                return sb.ToString();
            }

            /// <summary>
            /// Construye el fragmento de paginación OFFSET/FETCH si aplica.
            /// </summary>
            public static string BuildPaging(bool usarPaginacion)
                => usarPaginacion ? " OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY" : string.Empty;
        }
        /// <summary>
        /// Sanitiza campos de ordenamiento usando whitelist.
        /// </summary>
        internal static class QuerySafeHelpers
        {
            public static (string OrderBy, bool Desc) SanitizeOrderBy(
                string? sortField, int? sortOrder, string defaultColumn, params string[] allowedColumns)
            {
                var desc = sortOrder == 1; // 1 => DESC
                var candidate = (sortField ?? string.Empty).Trim();

                foreach (var allowed in allowedColumns)
                {
                    if (candidate.Equals(allowed, StringComparison.OrdinalIgnoreCase))
                    {
                        return (allowed, desc);
                    }
                }
                return (defaultColumn, desc);
            }
        }

        internal static class BitacoraHelper
        {
            public static void Registrar(MSecurityMainDb securityDb, int empresaId, string usuario, int modulo,
                string detalle, string movimiento)
            {
                securityDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = empresaId,
                    Usuario = usuario,
                    DetalleMovimiento = detalle,
                    Movimiento = movimiento,
                    Modulo = modulo
                });
            }
        }

        private static ErrorDto Error(string msg) => DbHelper.ErrorResponse(msg);

        /// <summary>
        /// Consulta de listado de estado de factura
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="esExportar"></param>
        /// <returns></returns>
        public ErrorDto<CxCFacturaEstadosLista> CxCFacturaEstadosLista_Obtener(int codEmpresa, FiltrosLazyLoadData filtros, bool esExportar)
        {
            var dto = DbHelper.WithConn(_portalDB, codEmpresa, (SqlConnection conn) =>
            {
                filtros ??= new FiltrosLazyLoadData();

                var texto = filtros.filtro?.Trim();
                var hasFiltro = !string.IsNullOrWhiteSpace(texto);

                var offset = filtros.pagina;
                var fetch = filtros.paginacion;
                var usarPaginacion = fetch > 0 && !esExportar;

           
                var (orderBy, desc) = QuerySafeHelpers.SanitizeOrderBy(
                    filtros.sortField, filtros.sortOrder, defaultColumn: "Factura_Estado",
                    allowedColumns: new[] { "Factura_Estado", "descripcion", "proceso", "accion", "activo" }
                );

             
                var where = SqlFragments.BuildWhereLike("Factura_Estado", "descripcion", "proceso", "accion");
                var order = SqlFragments.BuildOrderByCase(orderByParamName: "orderBy", desc: desc,
                    columns: new[] { "Factura_Estado", "descripcion", "proceso", "accion", "activo" });
                var paging = SqlFragments.BuildPaging(usarPaginacion);

               
                var sqlCount = "SELECT COUNT(1) FROM " + Tabla + "\n" + where + ";";

              
                var sqlList = @"
                        SELECT
                            Factura_Estado,
                            descripcion,
                            Proceso,
                            Accion,
                            activo
                        FROM " + Tabla + "\n" + where + "\n" + order + paging + ";";

                var @params = new
                {
                    filtro = hasFiltro ? texto : null,
                    like = hasFiltro ? $"%{texto}%" : null,
                    orderBy,
                    offset,
                    fetch
                };

                var total = conn.QuerySingle<int>(sqlCount, @params);
                var lista = conn.Query<CxCFacturaEstadosData>(sqlList, @params).ToList();

                return new CxCFacturaEstadosLista { total = total, lista = lista };
            });
 
            // Si WithConn atrapó un error, devolvemos mensaje genérico y lista vacía
            if (dto.Code != 0)
            {
                dto.Description = "No fue posible consultar los datos.";
                dto.Result = new CxCFacturaEstadosLista { total = 0, lista = new List<CxCFacturaEstadosData>() };
            }
            return dto;
        }

        /// <summary>
        /// Inserta o Actualiza un registri de estado de factura
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto CxCFacturaEstados_Guardar(int codEmpresa, string usuario, CxCFacturaEstadosData datos)
        {
            if (datos is null) return Error("Datos requeridos.");
            if (string.IsNullOrWhiteSpace(datos.Factura_Estado)) return Error("El campo 'Factura_Estado' es requerido.");
            if (string.IsNullOrWhiteSpace(usuario)) return Error("El usuario es requerido.");

            const string sqlUpsert = @"
                        DECLARE @accion NVARCHAR(10);

                        UPDATE dbo.CXC_FACTURAS_ESTADOS
                        SET descripcion = @Descripcion,
                            Proceso     = @Proceso,
                            Accion      = @Accion,
                            Activo      = @Activo
                        WHERE Factura_Estado = @Factura_Estado;

                        IF @@ROWCOUNT = 0
                        BEGIN
                            INSERT INTO dbo.CXC_FACTURAS_ESTADOS
                                (Factura_Estado, descripcion, Proceso, Accion, Activo, registro_fecha, registro_usuario)
                            VALUES
                                (@Factura_Estado, @Descripcion, @Proceso, @Accion, @Activo, dbo.MyGetdate(), @usuario);
                            SET @accion = N'insert';
                        END
                        ELSE
                        BEGIN
                            SET @accion = N'update';
                        END

                        SELECT @accion AS accion;";

            var upsert = DbHelper.ExecuteSingleQuery<string>(_portalDB, codEmpresa, sqlUpsert, defaultValue: "", parameters: new
            {
                datos.Factura_Estado,
                datos.Descripcion,
                datos.Proceso,
                datos.Accion,
                datos.Activo,
                usuario
            });

            if (upsert.Code != 0)
                return Error("No fue posible guardar el estado de factura.");

            var accion = (upsert.Result ?? string.Empty).ToLowerInvariant();
            if (accion == "insert")
            {
                BitacoraHelper.Registrar(_securityMainDb, codEmpresa, usuario, ModuloCxC,
                    $"Estado de Factura CxC: {datos.Factura_Estado}", MovRegistra);
                return DbHelper.CreateOkResponse();
            }

            if (accion == "update")
            {
                BitacoraHelper.Registrar(_securityMainDb, codEmpresa, usuario, ModuloCxC,
                    $"Estado de Factura CxC: {datos.Factura_Estado}", MovModifica);
                return DbHelper.CreateOkResponse();
            }

            return DbHelper.CreateOkResponse();
        }

        /// <summary>
        /// Elimina un registro de estado de factura
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="codFactura"></param>
        /// <returns></returns>
        public ErrorDto CxCFacturaEstados_Eliminar(int codEmpresa, string usuario, string codFactura)
        {
            if (string.IsNullOrWhiteSpace(codFactura))
                return Error("El 'Factura_Estado' es requerido.");

            const string sql = @"DELETE FROM dbo.CXC_FACTURAS_ESTADOS WHERE Factura_Estado = @CodFactura;";
            var result = DbHelper.ExecuteNonQueryWithResult(_portalDB, codEmpresa, sql, new { CodFactura = codFactura });

            if (result.Code != 0)
                return Error("No fue posible eliminar el estado de factura.");

            if (result.Result > 0)
            {
                BitacoraHelper.Registrar(_securityMainDb, codEmpresa, usuario, ModuloCxC,
                    $"Estado de Factura CxC: {codFactura}", MovElimina);
                return DbHelper.CreateOkResponse();
            }

            return DbHelper.CreateOkResponse();
        }
    }
}
 
