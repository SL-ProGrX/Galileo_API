using System.Collections.Generic;
using System.Data;
using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosObrasTiposDb
    {
        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly PortalDB _portalDB;
        private const string _codtipo = "cod_tipo";

        // SELECT base común
        private const string BaseSelectSql = @"
            SELECT cod_tipo,
                   descripcion,
                   activo,
                   registro_usuario,
                   registro_fecha,
                   modifica_usuario,
                   modifica_fecha
            FROM   Activos_obras_tipos";

        // WHERE común para filtro
        private const string WhereFilterSql = @"
            WHERE (@filtro IS NULL
                   OR cod_tipo    LIKE @filtro
                   OR descripcion LIKE @filtro)";

        // Query de datos con orden y paginación
        private const string ObrasTiposDataSql =
            BaseSelectSql + @"
            " + WhereFilterSql + @"
            ORDER BY
                -- ASC
                CASE @orderDir WHEN 1 THEN
                    CASE @orderIndex
                        WHEN 1 THEN cod_tipo
                        WHEN 2 THEN descripcion
                        WHEN 3 THEN CAST(activo AS INT)
                    END
                END ASC,
                -- DESC
                CASE @orderDir WHEN 0 THEN
                    CASE @orderIndex
                        WHEN 1 THEN cod_tipo
                        WHEN 2 THEN descripcion
                        WHEN 3 THEN CAST(activo AS INT)
                    END
                END DESC
            OFFSET @offset ROWS 
            FETCH NEXT @rows ROWS ONLY;";

        public FrmActivosObrasTiposDb(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _portalDB        = new PortalDB(config);
        }

        #region Helpers privados

        private static int GetObrasTiposTotal(IDbConnection connection)
        {
            const string countSql = @"SELECT COUNT(cod_tipo) FROM Activos_obras_tipos;";
            return connection.QueryFirstOrDefault<int?>(countSql) ?? 0;
        }

        private static int GetOrderIndex(string? sortField)
        {
            var field = (sortField ?? _codtipo).Trim().ToLowerInvariant();
            return field switch
            {
                "cod_tipo"    => 1,
                "descripcion" => 2,
                "activo"      => 3,
                _             => 1
            };
        }

        private static int GetOrderDir(int? sortOrder) =>
            (sortOrder ?? 0) == 0 ? 0 : 1; // 0 = DESC, 1 = ASC

        private static DynamicParameters BuildObrasTiposParams(FiltrosLazyLoadData filtros)
        {
            var p = new DynamicParameters();

            string? filtroLike = string.IsNullOrWhiteSpace(filtros?.filtro)
                ? null
                : $"%{filtros.filtro.Trim()}%";
            p.Add("@filtro", filtroLike, DbType.String);

            int pagina     = filtros?.pagina     ?? 0;
            int paginacion = filtros?.paginacion ?? 50;
            p.Add("@offset", pagina,     DbType.Int32);
            p.Add("@rows",   paginacion, DbType.Int32);

            p.Add("@orderIndex", GetOrderIndex(filtros?.sortField), DbType.Int32);
            p.Add("@orderDir",   GetOrderDir(filtros?.sortOrder),   DbType.Int32);

            return p;
        }

        private static List<ActivosObrasTipoData> GetObrasTiposLista(
            IDbConnection connection,
            DynamicParameters p) =>
            connection.Query<ActivosObrasTipoData>(ObrasTiposDataSql, p).ToList();

        #endregion

        /// <summary>
        /// Metodo para consultar la lista de tipos de obras en proceso (paginado).
        /// </summary>
        public ErrorDto<ActivosObrasTipoDataLista> Activos_ObrasTipos_Consultar(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = DbHelper.CreateOkResponse(
                new ActivosObrasTipoDataLista
                {
                    total = 0,
                    lista = new List<ActivosObrasTipoData>()
                });

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                if (result.Result != null)
                {
                    result.Result.total = GetObrasTiposTotal(connection);

                    var p = BuildObrasTiposParams(filtros);
                    result.Result.lista = GetObrasTiposLista(connection, p);
                }
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
                if (result.Result != null)
                {
                    result.Result.total = 0;
                    result.Result.lista = [];
                }
            }

            return result;
        }

        /// <summary>
        /// Metodo para consultar lista de tipos de obras en proceso a exportar (sin paginar).
        /// </summary>
        public ErrorDto<List<ActivosObrasTipoData>> Activos_ObrasTipos_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var p = new DynamicParameters();
            string? filtroLike = string.IsNullOrWhiteSpace(filtros?.filtro)
                ? null
                : $"%{filtros.filtro.Trim()}%";
            p.Add("@filtro", filtroLike, DbType.String);

            string query = BaseSelectSql + @"
                " + WhereFilterSql + @"
                ORDER BY cod_tipo;";

            return DbHelper.ExecuteListQuery<ActivosObrasTipoData>(
                _portalDB,
                CodEmpresa,
                query,
                p);
        }

        /// <summary>
        /// Metodo para actualizar o insertar un nuevo tipo de obras en proceso
        /// </summary>
        public ErrorDto Activos_ObrasTipos_Guardar(int CodEmpresa, string usuario, ActivosObrasTipoData datos)
        {
            var result = DbHelper.CreateOkResponse();

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                const string query = @"
                    SELECT COALESCE(COUNT(*),0) AS Existe
                    FROM   Activos_obras_tipos
                    WHERE  cod_tipo = @codigo;";
                var existe = connection.QueryFirstOrDefault<int>(query, new { codigo = datos.cod_tipo });

                if (datos.isNew)
                {
                    if (existe > 0)
                    {
                        result.Code        = -2;
                        result.Description =
                            $"El Tipo de obras en proceso con el código {datos.cod_tipo} ya existe.";
                    }
                    else
                    {
                        result = Activos_ObrasTipos_Insertar(CodEmpresa, usuario, datos);
                    }
                }
                else if (existe == 0)
                {
                    result.Code        = -2;
                    result.Description =
                        $"El Tipo de obras en proceso con el código {datos.cod_tipo} no existe.";
                }
                else
                {
                    result = Activos_ObrasTipos_Actualizar(CodEmpresa, usuario, datos);
                }
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>Metodo para actualizar un nuevo tipo de obras en proceso</summary>
        private ErrorDto Activos_ObrasTipos_Actualizar(int CodEmpresa, string usuario, ActivosObrasTipoData datos)
        {
            const string query = @"
                UPDATE Activos_obras_tipos
                   SET descripcion      = @descripcion,
                       activo           = @activo,
                       modifica_usuario = @usuario,
                       modifica_fecha   = GETDATE()
                 WHERE cod_tipo        = @cod_tipo;";

            var result = DbHelper.ExecuteNonQuery(
                _portalDB,
                CodEmpresa,
                query,
                new
                {
                    datos.cod_tipo,
                    datos.descripcion,
                    datos.activo,
                    usuario
                });

            if (result.Code == 0)
            {
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId         = CodEmpresa,
                    Usuario           = usuario,
                    DetalleMovimiento = $"Tipo de Obra en Proceso : {datos.cod_tipo}",
                    Movimiento        = "Modifica - WEB",
                    Modulo            = vModulo
                });
            }

            return result;
        }

        /// <summary>Metodo para insertar un nuevo tipo de obras en proceso</summary>
        private ErrorDto Activos_ObrasTipos_Insertar(int CodEmpresa, string usuario, ActivosObrasTipoData datos)
        {
            const string query = @"
                INSERT INTO Activos_obras_tipos
                    (cod_tipo, descripcion, activo, registro_usuario, registro_fecha)
                VALUES (@cod_tipo, @descripcion, @activo, @usuario, GETDATE());";

            var result = DbHelper.ExecuteNonQuery(
                _portalDB,
                CodEmpresa,
                query,
                new
                {
                    datos.cod_tipo,
                    datos.descripcion,
                    datos.activo,
                    usuario
                });

            if (result.Code == 0)
            {
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId         = CodEmpresa,
                    Usuario           = usuario,
                    DetalleMovimiento = $"Tipo de Obra en Proceso : {datos.cod_tipo}",
                    Movimiento        = "Registra - WEB",
                    Modulo            = vModulo
                });
            }

            return result;
        }

        /// <summary>Metodo para eliminar un tipo de obras en proceso</summary>
        public ErrorDto Activos_ObrasTipos_Eliminar(int CodEmpresa, string usuario, string cod_tipo)
        {
            const string query = @"DELETE FROM Activos_obras_tipos WHERE cod_tipo = @cod_tipo;";

            var result = DbHelper.ExecuteNonQuery(
                _portalDB,
                CodEmpresa,
                query,
                new { cod_tipo });

            if (result.Code == 0)
            {
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId         = CodEmpresa,
                    Usuario           = usuario,
                    DetalleMovimiento = $"Tipo de Obra en Proceso :  {cod_tipo}",
                    Movimiento        = "Elimina - WEB",
                    Modulo            = vModulo
                });
            }

            return result;
        }
    }
}