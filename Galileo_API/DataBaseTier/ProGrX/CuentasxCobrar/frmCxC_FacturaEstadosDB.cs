
using Dapper;
using Microsoft.Data.SqlClient; // Necesario para WithConn
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo.Models;
using System.Text;
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
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

        private const string SqlWhereFilters = @"
            WHERE
                (@filtro IS NULL)
                OR (CAST(Factura_Estado AS NVARCHAR(50)) LIKE @like)
                OR (descripcion LIKE @like)
                OR (proceso LIKE @like)
                OR (accion LIKE @like)
        ";

        private const string SqlSelectBase = @"
            SELECT Factura_Estado, descripcion, Proceso, Accion, Activo
            FROM dbo.CXC_FACTURAS_ESTADOS
        " + SqlWhereFilters;

        private const string SqlCount = @"
            SELECT COUNT(1)
            FROM dbo.CXC_FACTURAS_ESTADOS
        " + SqlWhereFilters + ";";

        private static readonly IReadOnlyDictionary<string, string> OrderMap =
           new Dictionary<string, string>
           {
               ["factura_estado"] = "Factura_Estado",
               ["descripcion"] = "descripcion",
               ["proceso"] = "Proceso",
               ["accion"] = "Accion",
               ["activo"] = "Activo"
           };

        private static string BuildSafeOrderBy(string? sortField, int? sortOrder)
        {
            var key = (sortField ?? string.Empty).Trim().ToLowerInvariant();
            var column = OrderMap.TryGetValue(key, out var mapped) ? mapped : "Factura_Estado";
            var direction = (sortOrder == 1) ? "DESC" : "ASC"; // Ajusta si tu UI usa otro convenio
            return $" ORDER BY {column} {direction}";
        }


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

                var orderClause = BuildSafeOrderBy(filtros.sortField, filtros.sortOrder);

                var baseParams = new
                {
                    filtro = hasFiltro ? texto : null,
                    like = hasFiltro ? $"%{texto}%" : null,
                    offset,
                    fetch
                };

              
                var total = conn.QuerySingle<int>(SqlCount, baseParams);

                var sqlList = new StringBuilder(SqlSelectBase)
                    .Append(orderClause)
                    .Append(usarPaginacion ? " OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;" : ";")
                    .ToString();

                var lista = conn.Query<CxCFacturaEstadosData>(sqlList, baseParams).ToList();

                return new CxCFacturaEstadosLista { total = total, lista = lista };
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
            if (datos is null) return DbHelper.ErrorResponse("Datos requeridos.");
            if (string.IsNullOrWhiteSpace(datos.Factura_Estado)) return DbHelper.ErrorResponse("El campo 'Factura_Estado' es requerido.");
            if (string.IsNullOrWhiteSpace(usuario)) return DbHelper.ErrorResponse("El usuario es requerido.");

            const string sqlUpsert = @"
                DECLARE @accion_result NVARCHAR(10);

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
                    SET @accion_result = N'insert';
                END
                ELSE
                BEGIN
                    SET @accion_result = N'update';
                END

                SELECT @accion_result AS accion;
            ";

            var upsert = DbHelper.ExecuteSingleQuery<string>(
                _portalDB, codEmpresa, sqlUpsert, defaultValue: "",
                parameters: new
                {
                    datos.Factura_Estado,
                    datos.Descripcion,
                    datos.Proceso,
                    datos.Accion,
                    datos.Activo,
                    usuario
                });

            if (upsert.Code != 0)
                return DbHelper.ErrorResponse("No fue posible guardar el estado de factura.");

            var accion = (upsert.Result ?? string.Empty).ToLowerInvariant();
            if (accion == "insert")
                LogBitacora(codEmpresa, usuario, datos.Factura_Estado, MovRegistra);
            else if (accion == "update")
                LogBitacora(codEmpresa, usuario, datos.Factura_Estado, MovModifica);

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
                return DbHelper.ErrorResponse("El 'Factura_Estado' es requerido.");

            const string sql = @"DELETE FROM dbo.CXC_FACTURAS_ESTADOS WHERE Factura_Estado = @CodFactura;";
            var result = DbHelper.ExecuteNonQueryWithResult(_portalDB, codEmpresa, sql, new { CodFactura = codFactura });

            if (result.Code != 0)
                return DbHelper.ErrorResponse("No fue posible eliminar el estado de factura.");

            if (result.Result > 0)
                LogBitacora(codEmpresa, usuario, codFactura, MovElimina);

            return DbHelper.CreateOkResponse();
        }
    }
}
 
