using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.Models.ProGrX.CuentasPorCobrar;
using Galileo.Models.Security;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasPorCobrar
{
    public class FrmCxCClientesClasificaDB
    {

        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly int vModulo = 31;
        private const string Tabla = "dbo.CxC_Categoria_Clientes";

        public FrmCxCClientesClasificaDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config!);
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
                Modulo = vModulo
            });
        }
 


        private static (string orderByField, string direction) OrderByFrom(string? sortField, int? sortOrder)
        {
            var field = (sortField ?? string.Empty).Trim().ToLowerInvariant();
            var orderByField = field switch
            {
                "cod_categoria" => "cod_categoria",
                "descripcion" => "descripcion",
                "activa" => "activa",
                _ => "cod_categoria"
            };
            var direction = sortOrder == 1 ? "DESC" : "ASC";
            return (orderByField, direction);
        }

        private static string BuildWhereClause() => @"
            WHERE
                (@filtro IS NULL)
                OR (CAST(cod_categoria AS NVARCHAR(50)) LIKE @like)
                OR (descripcion LIKE @like)
            ";

        private static string BuildPaging(bool usarPaginacion) =>
            usarPaginacion ? " OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY " : string.Empty;
#endregion


        /// <summary>
        /// Obtiene la lista de clasificación de clientes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <param name="esExportar"></param>
        /// <returns></returns>
        public ErrorDto<CxCClientesClasificaLista> CxCClientesClasificaLista_Obtener(int codEmpresa, FiltrosLazyLoadData filtros, bool esExportar)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, (SqlConnection conn) =>
            {
                filtros ??= new FiltrosLazyLoadData();

                var texto = filtros.filtro?.Trim();
                var hasFiltro = !string.IsNullOrWhiteSpace(texto);
                var @params = new
                {
                    filtro = hasFiltro ? texto : null,
                    like = hasFiltro ? $"%{texto}%" : null,
                    offset = filtros.pagina,
                    fetch = filtros.paginacion 
                };
                bool usarPaginacion = (filtros.paginacion) > 0 && !esExportar;

                var (orderByField, direction) = OrderByFrom(filtros.sortField, filtros.sortOrder);
                var where = BuildWhereClause();

                var sqlCount = $@"SELECT COUNT(1) FROM {Tabla} {where};";
                var sqlList = $@"
                                SELECT
                                    cod_categoria,
                                    descripcion,
                                    activa AS Activo
                                FROM {Tabla}
                                {where}
                                ORDER BY {orderByField} {direction}
                                {BuildPaging(usarPaginacion)}
                                ;";

                var total = conn.QuerySingle<int>(sqlCount, @params);
                var lista = conn.Query<CxCClientesClasificaData>(sqlList, @params).ToList();

                return new CxCClientesClasificaLista { total = total, lista = lista };
            });
        }

        /// <summary>
        /// Guarda o actualiza una clasificación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto CxCClientesClasifica_Guardar(int codEmpresa, string usuario, CxCClientesClasificaData datos)
        {
            if (datos is null) return DbHelper.ErrorResponse("Datos requeridos.");
            if (string.IsNullOrWhiteSpace(datos.Cod_categoria)) return DbHelper.ErrorResponse("El campo 'cod_categoria' es requerido.");
            if (string.IsNullOrWhiteSpace(usuario)) return DbHelper.ErrorResponse("El usuario es requerido.");

            // Batch T-SQL: UPDATE; si no hay filas afectadas → INSERT. Retorna 'update' o 'insert'
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

            var upsert = DbHelper.ExecuteSingleQuery<string>(_portalDB, codEmpresa, sqlUpsert, defaultValue: "", parameters: new
            {
                datos.Cod_categoria,
                datos.Descripcion,
                datos.Activo,
                usuario
            });

            if (upsert.Code != 0)
                return DbHelper.ErrorResponse("No fue posible guardar la clasificación de CxC.");

            var accion = (upsert.Result ?? "").ToLowerInvariant();
            if (accion == "insert")
            {
                LogBitacora(codEmpresa, usuario, $"Categoria de Cliente : {datos.Cod_categoria}", "Registra - WEB");
                return DbHelper.OkResponse("Categoria de Cliente insertado correctamente.");
            }
            else if (accion == "update")
            {
                LogBitacora(codEmpresa, usuario, $"Categoria de Cliente : {datos.Cod_categoria}", "MODIFICA - WEB");
                return DbHelper.OkResponse("Categoria de Cliente actualizado correctamente.");
            }

             
            return DbHelper.OkResponse("Operación completada.");
        }
 
        /// <summary>
        /// Elimina una clasificación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="CodCargo"></param>
        /// <returns></returns>
        public ErrorDto CxCClientesClasifica_Eliminar(int codEmpresa, string usuario, string codCategoria)
        {

            if (string.IsNullOrWhiteSpace(codCategoria))
                return DbHelper.ErrorResponse("El 'cod_categoria' es requerido.");

            const string sql = @"DELETE FROM CxC_Categoria_Clientes WHERE cod_categoria = @CodCategoria;";
            var result = DbHelper.ExecuteNonQueryWithResult(_portalDB, codEmpresa, sql, new { CodCategoria = codCategoria });

            if (result.Code != 0)
                return DbHelper.ErrorResponse("No fue posible eliminar la clasificación de CxC.");

            // Si afectó filas, registramos bitácora
            if (result.Result > 0)
            {
                LogBitacora(
                    empresaId: codEmpresa,
                    usuario: usuario,
                    detalle: $"Categoria de Cliente : {codCategoria}",
                    movimiento: "ELIMINAR - WEB"
                );
                return DbHelper.OkResponse("Categoria de Cliente eliminado correctamente.");
            }

            // No existía el registro; devolvemos OK para mantener idempotencia (puedes cambiar el mensaje si prefieres).
            return DbHelper.OkResponse("No se encontró la categoría solicitada.");
        }

    }
}
