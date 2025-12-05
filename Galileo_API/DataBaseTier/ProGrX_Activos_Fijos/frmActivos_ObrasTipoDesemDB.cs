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
        private const string _coddesembolso = "cod_desembolso";

        public FrmActivosObrasTipoDesemDb(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Método para consultar la lista de tipos de desembolsos (paginado).
        /// </summary>
        public ErrorDto<ActivosObrasTipoDesemDataLista> Activos_ObrasTipoDesem_Consultar(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<ActivosObrasTipoDesemDataLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivosObrasTipoDesemDataLista()
                {
                    total = 0,
                    lista = new List<ActivosObrasTipoDesemData>()
                }
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                // 1) Total (como en tu código original: sin filtro)
                const string countSql = @"SELECT COUNT(cod_desembolso) FROM Activos_obras_tdesem";
                result.Result.total = connection.QueryFirstOrDefault<int>(countSql);

                // 2) Parámetros de filtro / paginación
                var p = new DynamicParameters();

                string? filtroLike = string.IsNullOrWhiteSpace(filtros?.filtro)
                    ? null
                    : $"%{filtros.filtro.Trim()}%";
                p.Add("@filtro", filtroLike, DbType.String);

                int pagina = filtros?.pagina ?? 0;
                int paginacion = filtros?.paginacion ?? 50;
                p.Add("@offset", pagina, DbType.Int32);
                p.Add("@rows", paginacion, DbType.Int32);

                // 3) Sort seguro (whitelist de columnas)
                var sortFieldRaw = (filtros?.sortField ?? _coddesembolso).Trim();
                var sortFieldNorm = sortFieldRaw.ToLowerInvariant();

                string orderByCol = sortFieldNorm switch
                {
                    _coddesembolso => _coddesembolso,
                    "descripcion"    => "descripcion",
                    "activo"         => "activo",
                    _                => _coddesembolso
                };

                string orderDir = (filtros?.sortOrder ?? 0) == 0 ? "DESC" : "ASC";

                const string whereSql = @"
                    WHERE (@filtro IS NULL
                           OR cod_desembolso LIKE @filtro
                           OR descripcion    LIKE @filtro)";

                string dataSql = $@"
                    SELECT cod_desembolso,
                           descripcion,
                           activo,
                           registro_usuario,
                           registro_fecha,
                           modifica_usuario,
                           modifica_fecha
                    FROM   Activos_obras_tdesem
                    {whereSql}
                    ORDER BY {orderByCol} {orderDir}
                    OFFSET @offset ROWS 
                    FETCH NEXT @rows ROWS ONLY;";

                result.Result.lista = connection
                    .Query<ActivosObrasTipoDesemData>(dataSql, p)
                    .ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = [];
            }
            return result;
        }

        /// <summary>
        /// Método para consultar lista de tipos de desembolsos a exportar (sin paginar).
        /// </summary>
        public ErrorDto<List<ActivosObrasTipoDesemData>> Activos_ObrasTipoDesem_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<List<ActivosObrasTipoDesemData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosObrasTipoDesemData>()
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var p = new DynamicParameters();
                string? filtroLike = string.IsNullOrWhiteSpace(filtros?.filtro)
                    ? null
                    : $"%{filtros.filtro.Trim()}%";
                p.Add("@filtro", filtroLike, DbType.String);

                const string whereSql = @"
                    WHERE (@filtro IS NULL
                           OR cod_desembolso LIKE @filtro
                           OR descripcion    LIKE @filtro)";

                string query = $@"
                    SELECT cod_desembolso,
                           descripcion,
                           activo,
                           registro_usuario,
                           registro_fecha,
                           modifica_usuario,
                           modifica_fecha
                    FROM   Activos_obras_tdesem
                    {whereSql}
                    ORDER BY cod_desembolso;";

                result.Result = connection
                    .Query<ActivosObrasTipoDesemData>(query, p)
                    .ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Método para actualizar o insertar un nuevo tipo de desembolso
        /// </summary>
        public ErrorDto Activos_ObrasTipoDesem_Guardar(int CodEmpresa, string usuario, ActivosObrasTipoDesemData datos)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                const string query = @"
                    SELECT COALESCE(COUNT(*),0) AS Existe
                    FROM   Activos_obras_tdesem
                    WHERE  cod_desembolso = @codigo";
                var existe = connection.QueryFirstOrDefault<int>(query, new { codigo = datos.cod_desembolso });

                if (datos.isNew)
                {
                    if (existe > 0)
                    {
                        result.Code = -2;
                        result.Description = $"El Tipo de Desembolso con el código {datos.cod_desembolso} ya existe.";
                    }
                    else
                    {
                        result = Activos_ObrasTipoDesem_Insertar(CodEmpresa, usuario, datos);
                    }
                }
                else if (existe == 0 && !datos.isNew)
                {
                    result.Code = -2;
                    result.Description = $"El Tipo de Desembolso con el código {datos.cod_desembolso} no existe.";
                }
                else
                {
                    result = Activos_ObrasTipoDesem_Actualizar(CodEmpresa, usuario, datos);
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Método para actualizar un tipo de desembolso
        /// </summary>
        private ErrorDto Activos_ObrasTipoDesem_Actualizar(int CodEmpresa, string usuario, ActivosObrasTipoDesemData datos)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                const string query = @"
                    UPDATE Activos_obras_tdesem
                       SET descripcion      = @descripcion,
                           activo           = @activo,
                           modifica_usuario = @usuario,
                           modifica_fecha   = GETDATE()
                     WHERE cod_desembolso   = @cod_desembolso";

                connection.Execute(query, new
                {
                    datos.cod_desembolso,
                    datos.descripcion,
                    datos.activo,
                    usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipo de Desem. para Obra en Proceso : {datos.cod_desembolso}",
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Método para insertar un nuevo tipo de desembolso
        /// </summary>
        private ErrorDto Activos_ObrasTipoDesem_Insertar(int CodEmpresa, string usuario, ActivosObrasTipoDesemData datos)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                const string query = @"
                    INSERT INTO Activos_obras_tdesem
                        (cod_desembolso, descripcion, activo, registro_usuario, registro_fecha)
                    VALUES
                        (@cod_desembolso, @descripcion, @activo, @usuario, GETDATE())";

                connection.Execute(query, new
                {
                    datos.cod_desembolso,
                    datos.descripcion,
                    datos.activo,
                    usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipo de Desem. para Obra en Proceso : {datos.cod_desembolso}",
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Método para eliminar un tipo de desembolso
        /// </summary>
        public ErrorDto Activos_ObrasTipoDesem_Eliminar(int CodEmpresa, string usuario, string cod_desembolso)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                const string query = @"DELETE FROM Activos_obras_tdesem WHERE cod_desembolso = @cod_desembolso";
                connection.Execute(query, new { cod_desembolso });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipo de Desem. para Obra en Proceso : {cod_desembolso}",
                    Movimiento = "Elimina - WEB",
                    Modulo = vModulo
                });
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }
    }
}