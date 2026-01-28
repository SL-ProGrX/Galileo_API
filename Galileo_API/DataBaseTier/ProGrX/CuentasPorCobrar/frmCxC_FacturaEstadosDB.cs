
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

        public FrmCxCFacturaEstadosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config!);
        }


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

        private const string WhereClause = @"
            WHERE
                (@filtro IS NULL)
                OR (CAST(Factura_Estado AS NVARCHAR(50)) LIKE @like)
                OR (descripcion LIKE @like)
                OR (proceso LIKE @like)
                OR (accion LIKE @like)
            ";

        private const string SqlListAsc = @"
            SELECT Factura_Estado, descripcion, Proceso, Accion, activo
            FROM dbo.CXC_FACTURAS_ESTADOS
            " + WhereClause + @"
            ORDER BY
                CASE WHEN @orderBy = 'Factura_Estado' THEN Factura_Estado END,
                CASE WHEN @orderBy = 'descripcion'    THEN descripcion    END,
                CASE WHEN @orderBy = 'proceso'        THEN Proceso        END,
                CASE WHEN @orderBy = 'accion'         THEN Accion         END,
                CASE WHEN @orderBy = 'activo'         THEN activo         END
            ";
        private const string SqlListDesc = @"
                SELECT Factura_Estado, descripcion, Proceso, Accion, activo
                FROM dbo.CXC_FACTURAS_ESTADOS
                " + WhereClause + @"
                ORDER BY
                    CASE WHEN @orderBy = 'Factura_Estado' THEN Factura_Estado END DESC,
                    CASE WHEN @orderBy = 'descripcion'    THEN descripcion    END DESC,
                    CASE WHEN @orderBy = 'proceso'        THEN Proceso        END DESC,
                    CASE WHEN @orderBy = 'accion'         THEN Accion         END DESC,
                    CASE WHEN @orderBy = 'activo'         THEN activo         END DESC
                ";

        private const string SqlCount = @"
                SELECT COUNT(1)
                FROM dbo.CXC_FACTURAS_ESTADOS
                " + WhereClause + @";
                ";

        private static (string OrderBy, bool Desc) SanitizeOrderBy(string? sortField, int? sortOrder)
        {
            var field = (sortField ?? string.Empty).Trim().ToLowerInvariant();
            var orderBy = field switch
            {
                "Factura_Estado" => "Factura_Estado",
                "descripcion" => "descripcion",
                "proceso" => "proceso",
                "accion" => "accion",
                "activa" => "activa",
                _ => "Factura_Estado"
            };
            var desc = sortOrder == 1; // 1 = DESC; cualquier otro = ASC
            return (orderBy, desc);
        }
 
        private static ErrorDto Error(string msg) => DbHelper.ErrorResponse(msg);
        private static ErrorDto Ok() => DbHelper.CreateOkResponse();

        /// <summary>
        /// Consulta de listado de estado de factura
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="esExportar"></param>
        /// <returns></returns>
        public ErrorDto<CxCFacturaEstadosLista> CxCFacturaEstadosLista_Obtener(int codEmpresa, FiltrosLazyLoadData filtros, bool esExportar)
        {

            return DbHelper.WithConn(_portalDB, codEmpresa, (SqlConnection conn) =>
            {
                filtros ??= new FiltrosLazyLoadData();

                var texto = filtros.filtro?.Trim();
                var hasFiltro = !string.IsNullOrWhiteSpace(texto);

                var offset = filtros.pagina;
                var fetch = filtros.paginacion;
                var usarPaginacion = fetch > 0 && !esExportar;

                var (orderBy, desc) = SanitizeOrderBy(filtros.sortField, filtros.sortOrder);

                var baseParams = new
                {
                    filtro = hasFiltro ? texto : null,
                    like = hasFiltro ? $"%{texto}%" : null,
                    orderBy,
                    offset,
                    fetch
                };

                var total = conn.QuerySingle<int>(SqlCount, baseParams);


                var sqlListCore = desc ? SqlListDesc : SqlListAsc;
                var sqlList = usarPaginacion
                    ? sqlListCore + " OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;"
                    : sqlListCore + ";";

                var lista = conn.Query<CxCFacturaEstadosData>(sqlList, baseParams).ToList();

                return new CxCFacturaEstadosLista
                {
                    total = total,
                    lista = lista
                };
            });

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
                LogBitacora(codEmpresa, usuario, $"Estado de Factura CxC: {datos.Factura_Estado}", MovRegistra);
                return Ok();
 
            }

            if (accion == "update")
            {
                LogBitacora(codEmpresa, usuario, $"Estado de Factura CxC: {datos.Factura_Estado}", MovModifica);
                return Ok();
               
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
                LogBitacora(codEmpresa, usuario, $"Estado de Factura CxC: {codFactura}", MovElimina);
                return Ok(); 
            }

            return Ok();
        }
    }
}
 
