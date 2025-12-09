using System.Data;
using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosObrasTipoDesemDb
    {
        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly PortalDB _portalDB;

        private const string CodDesembolsoCol = "cod_desembolso";

        // SELECT base común
        private const string BaseSelectSql = @"
            SELECT cod_desembolso,
                   descripcion,
                   activo,
                   registro_usuario,
                   registro_fecha,
                   modifica_usuario,
                   modifica_fecha
            FROM   Activos_obras_tdesem";

        // WHERE común para filtro de texto
        private const string WhereFilterSql = @"
            WHERE (@filtro IS NULL
                   OR cod_desembolso LIKE @filtro
                   OR descripcion    LIKE @filtro)";

        public FrmActivosObrasTipoDesemDb(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _portalDB        = new PortalDB(config);
        }

        /// <summary>
        /// Método para consultar la lista de tipos de desembolsos (paginado).
        /// </summary>
        public ErrorDto<ActivosObrasTipoDesemDataLista> Activos_ObrasTipoDesem_Consultar(
            int CodEmpresa,
            FiltrosLazyLoadData filtros)
        {
            var result = DbHelper.CreateOkResponse(
                new ActivosObrasTipoDesemDataLista
                {
                    total = 0,
                    lista = new List<ActivosObrasTipoDesemData>()
                });

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                // Total sin filtro (igual que tu versión original)
                const string countSql = @"SELECT COUNT(cod_desembolso) FROM Activos_obras_tdesem";
                if (result.Result != null)
                {
                    result.Result.total = connection.QueryFirstOrDefault<int?>(countSql) ?? 0;
                }

                // Parámetros
                var p = new DynamicParameters();

                string? filtroLike = string.IsNullOrWhiteSpace(filtros?.filtro)
                    ? null
                    : $"%{filtros.filtro.Trim()}%";
                p.Add("@filtro", filtroLike, DbType.String);

                int pagina     = filtros?.pagina     ?? 0;
                int paginacion = filtros?.paginacion ?? 50;
                p.Add("@offset", pagina,     DbType.Int32);
                p.Add("@rows",   paginacion, DbType.Int32);

                // Sort seguro con índice (para evitar S2077)
                var  sortFieldRaw  = (filtros?.sortField ?? CodDesembolsoCol).Trim();
                var  sortFieldNorm = sortFieldRaw.ToLowerInvariant();
                int  sortIndex     = sortFieldNorm switch
                {
                    CodDesembolsoCol => 1,
                    "descripcion"    => 2,
                    "activo"         => 3,
                    _                => 1
                };
                int sortDir = (filtros?.sortOrder ?? 0) == 0 ? 0 : 1; // 0 = DESC, 1 = ASC
                p.Add("@sortIndex", sortIndex, DbType.Int32);
                p.Add("@sortDir",   sortDir,   DbType.Int32);

                // ORDER BY usando CASE en lugar de interpolar columna/dirección
                const string dataSql = BaseSelectSql + @"
                    " + WhereFilterSql + @"
                    ORDER BY
                        -- ASC
                        CASE @sortDir WHEN 1 THEN
                            CASE @sortIndex
                                WHEN 1 THEN cod_desembolso
                                WHEN 2 THEN descripcion
                                WHEN 3 THEN CAST(activo AS varchar(10))
                            END
                        END ASC,
                        -- DESC
                        CASE @sortDir WHEN 0 THEN
                            CASE @sortIndex
                                WHEN 1 THEN cod_desembolso
                                WHEN 2 THEN descripcion
                                WHEN 3 THEN CAST(activo AS varchar(10))
                            END
                        END DESC
                    OFFSET @offset ROWS 
                    FETCH NEXT @rows ROWS ONLY;";

                if (result.Result != null)
                {
                    result.Result.lista = connection
                        .Query<ActivosObrasTipoDesemData>(dataSql, p)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                result.Code           = -1;
                result.Description    = ex.Message;
                if (result.Result != null)
                {
                    result.Result.total = 0;
                    result.Result.lista = [];
                }
            }

            return result;
        }

        /// <summary>
        /// Método para consultar lista de tipos de desembolsos a exportar (sin paginar).
        /// </summary>
        public ErrorDto<List<ActivosObrasTipoDesemData>> Activos_ObrasTipoDesem_Obtener(
            int CodEmpresa,
            FiltrosLazyLoadData filtros)
        {
            var p = new DynamicParameters();

            string? filtroLike = string.IsNullOrWhiteSpace(filtros?.filtro)
                ? null
                : $"%{filtros.filtro.Trim()}%";
            p.Add("@filtro", filtroLike, DbType.String);

            const string query = BaseSelectSql + @"
                " + WhereFilterSql + @"
                ORDER BY cod_desembolso;";

            return DbHelper.ExecuteListQuery<ActivosObrasTipoDesemData>(
                _portalDB,
                CodEmpresa,
                query,
                p);
        }

        /// <summary>
        /// Método para actualizar o insertar un nuevo tipo de desembolso
        /// </summary>
        public ErrorDto Activos_ObrasTipoDesem_Guardar(
            int CodEmpresa,
            string usuario,
            ActivosObrasTipoDesemData datos)
        {
            var result = DbHelper.CreateOkResponse();

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                const string queryExiste = @"
                    SELECT COALESCE(COUNT(*),0) AS Existe
                    FROM   Activos_obras_tdesem
                    WHERE  cod_desembolso = @codigo";

                var existe = connection.QueryFirstOrDefault<int>(
                    queryExiste,
                    new { codigo = datos.cod_desembolso });

                if (datos.isNew)
                {
                    if (existe > 0)
                    {
                        result.Code        = -2;
                        result.Description =
                            $"El Tipo de Desembolso con el código {datos.cod_desembolso} ya existe.";
                    }
                    else
                    {
                        result = Activos_ObrasTipoDesem_Insertar(CodEmpresa, usuario, datos);
                    }
                }
                else if (existe == 0)
                {
                    result.Code        = -2;
                    result.Description =
                        $"El Tipo de Desembolso con el código {datos.cod_desembolso} no existe.";
                }
                else
                {
                    result = Activos_ObrasTipoDesem_Actualizar(CodEmpresa, usuario, datos);
                }
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Método para actualizar un tipo de desembolso
        /// </summary>
        private ErrorDto Activos_ObrasTipoDesem_Actualizar(
            int CodEmpresa,
            string usuario,
            ActivosObrasTipoDesemData datos)
        {
            const string query = @"
                UPDATE Activos_obras_tdesem
                   SET descripcion      = @descripcion,
                       activo           = @activo,
                       modifica_usuario = @usuario,
                       modifica_fecha   = GETDATE()
                 WHERE cod_desembolso   = @cod_desembolso";

            var result = DbHelper.ExecuteNonQuery(
                _portalDB,
                CodEmpresa,
                query,
                new
                {
                    datos.cod_desembolso,
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
                    DetalleMovimiento =
                        $"Tipo de Desem. para Obra en Proceso : {datos.cod_desembolso}",
                    Movimiento        = "Modifica - WEB",
                    Modulo            = vModulo
                });
            }

            return result;
        }

        /// <summary>
        /// Método para insertar un nuevo tipo de desembolso
        /// </summary>
        private ErrorDto Activos_ObrasTipoDesem_Insertar(
            int CodEmpresa,
            string usuario,
            ActivosObrasTipoDesemData datos)
        {
            const string query = @"
                INSERT INTO Activos_obras_tdesem
                    (cod_desembolso, descripcion, activo, registro_usuario, registro_fecha)
                VALUES
                    (@cod_desembolso, @descripcion, @activo, @usuario, GETDATE())";

            var result = DbHelper.ExecuteNonQuery(
                _portalDB,
                CodEmpresa,
                query,
                new
                {
                    datos.cod_desembolso,
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
                    DetalleMovimiento =
                        $"Tipo de Desem. para Obra en Proceso : {datos.cod_desembolso}",
                    Movimiento        = "Registra - WEB",
                    Modulo            = vModulo
                });
            }

            return result;
        }

        /// <summary>
        /// Método para eliminar un tipo de desembolso
        /// </summary>
        public ErrorDto Activos_ObrasTipoDesem_Eliminar(
            int CodEmpresa,
            string usuario,
            string cod_desembolso)
        {
            const string query = @"DELETE FROM Activos_obras_tdesem WHERE cod_desembolso = @cod_desembolso";

            var result = DbHelper.ExecuteNonQuery(
                _portalDB,
                CodEmpresa,
                query,
                new { cod_desembolso });

            if (result.Code == 0)
            {
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId         = CodEmpresa,
                    Usuario           = usuario,
                    DetalleMovimiento =
                        $"Tipo de Desem. para Obra en Proceso : {cod_desembolso}",
                    Movimiento        = "Elimina - WEB",
                    Modulo            = vModulo
                });
            }

            return result;
        }
    }
}