using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo.Models.Security; 
using Galileo_API.Models.ProGrX.CuentasxCobrar;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCClientesClasificaDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;

        private const int ModuloCxC = 31;
        private const string Tabla = "dbo.CxC_Categoria_Clientes";

       
        private const string MovRegistra = "REGISTRA - WEB";
        private const string MovModifica = "MODIFICA - WEB";
        private const string MovElimina = "ELIMINAR - WEB";

        public FrmCxCClientesClasificaDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        #region Helpers DRY

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
               
        private static ErrorDto Ok() => DbHelper.CreateOkResponse();
        private static ErrorDto Error(string msg) => DbHelper.ErrorResponse(msg);

        private const string WhereClause = @"
                        WHERE
                            (@filtro IS NULL)
                            OR (CAST(cod_categoria AS NVARCHAR(50)) LIKE @like)
                            OR (descripcion LIKE @like)
                        ";


        private const string SqlListAsc = @"
                    SELECT
                        cod_categoria,
                        descripcion,
                        activa AS Activo
                    FROM dbo.CxC_Categoria_Clientes
                    " + WhereClause + @"
                    ORDER BY
                        CASE WHEN @orderBy = 'cod_categoria' THEN cod_categoria END,
                        CASE WHEN @orderBy = 'descripcion'   THEN descripcion   END,
                        CASE WHEN @orderBy = 'activa'        THEN activa        END
                    ";


        private const string SqlListDesc = @"
                    SELECT
                        cod_categoria,
                        descripcion,
                        activa AS Activo
                    FROM dbo.CxC_Categoria_Clientes
                    " + WhereClause + @"
                    ORDER BY
                        CASE WHEN @orderBy = 'cod_categoria' THEN cod_categoria END DESC,
                        CASE WHEN @orderBy = 'descripcion'   THEN descripcion   END DESC,
                        CASE WHEN @orderBy = 'activa'        THEN activa        END DESC
                    ";

        private const string SqlCount = "SELECT COUNT(1) FROM dbo.CxC_Categoria_Clientes " + WhereClause + ";";

        private static (string OrderBy, bool Desc) SanitizeOrderBy(string? sortField, int? sortOrder)
        {
            var field = (sortField ?? string.Empty).Trim().ToLowerInvariant();
            var orderBy = field switch
            {
                "cod_categoria" => "cod_categoria",
                "descripcion" => "descripcion",
                "activa" => "activa",
                _ => "cod_categoria"
            };
            var desc = sortOrder == 1; // 1 = DESC; cualquier otro = ASC
            return (orderBy, desc);
        }


        #endregion

        /// <summary>
        /// Lista clasificación (filtros, orden, paginación).
        /// </summary>
        public ErrorDto<CxCClientesClasificaLista> CxCClientesClasificaLista_Obtener(
            int codEmpresa, FiltrosLazyLoadData filtros, bool esExportar)
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

                var lista = conn.Query<CxCClientesClasificaData>(sqlList, baseParams).ToList();

                return new CxCClientesClasificaLista
                {
                    total = total,
                    lista = lista
                };
            });

        }

        /// <summary>
        /// Upsert (inserta si no existe; actualiza si existe).
        /// </summary>
        public ErrorDto CxCClientesClasifica_Guardar(int codEmpresa, string usuario, CxCClientesClasificaData datos)
        {
            if (datos is null) return Error("Datos requeridos.");
            if (string.IsNullOrWhiteSpace(datos.Cod_categoria)) return Error("El campo 'cod_categoria' es requerido.");
            if (string.IsNullOrWhiteSpace(usuario)) return Error("El usuario es requerido.");

            const string sqlUpsert = @"
                        DECLARE @accion NVARCHAR(10);

                        UPDATE CxC_Categoria_Clientes
                        SET descripcion = @Descripcion,
                            activa      = @Activo
                        WHERE cod_categoria = @Cod_categoria;

                        IF @@ROWCOUNT = 0
                        BEGIN
                            INSERT INTO CxC_Categoria_Clientes
                                (cod_categoria, descripcion, activa, registro_fecha, registro_usuario)
                            VALUES
                                (@Cod_categoria, @Descripcion, @Activo, dbo.MyGetdate(), @usuario);
                            SET @accion = N'insert';
                        END
                        ELSE
                        BEGIN
                            SET @accion = N'update';
                        END

                        SELECT @accion AS accion;
                        ";

        
            var upsert = DbHelper.ExecuteSingleQuery<string>(
                _portalDB, codEmpresa, sqlUpsert, defaultValue: "", parameters: new
                {
                    datos.Cod_categoria,
                    datos.Descripcion,
                    datos.Activo,
                    usuario
                });

            if (upsert.Code != 0)
                return Error("No fue posible guardar la clasificación de CxC.");

            var accion = (upsert.Result ?? "").ToLowerInvariant();
            if (accion == "insert")
            {
                LogBitacora(codEmpresa, usuario, $"Categoria de Cliente : {datos.Cod_categoria}", MovRegistra);
                return Ok();
            }
            if (accion == "update")
            {
                LogBitacora(codEmpresa, usuario, $"Categoria de Cliente : {datos.Cod_categoria}", MovModifica);
                return Ok();
            }

            return Ok(); 
        }

        /// <summary>
        /// Elimina una clasificación.
        /// </summary>
        public ErrorDto CxCClientesClasifica_Eliminar(int codEmpresa, string usuario, string codCategoria)
        {
            if (string.IsNullOrWhiteSpace(codCategoria))
                return Error("El 'cod_categoria' es requerido.");

            const string sql = @"DELETE FROM CxC_Categoria_Clientes WHERE cod_categoria = @CodCategoria;";
            var result = DbHelper.ExecuteNonQueryWithResult(_portalDB, codEmpresa, sql, new { CodCategoria = codCategoria });

            if (result.Code != 0)
                return Error("No fue posible eliminar la clasificación de CxC.");

            if (result.Result > 0)
            {
                LogBitacora(codEmpresa, usuario, $"Categoria de Cliente : {codCategoria}", MovElimina);
                return Ok();
            }

            return Ok();
        }
    }
}