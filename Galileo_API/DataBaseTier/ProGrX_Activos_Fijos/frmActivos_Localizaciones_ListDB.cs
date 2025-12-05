using System.Data;
using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosLocalizacionesListDb
    {
        private readonly int vModulo = 36; // Modulo de Tesorería
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly PortalDB _portalDB;

        private const string _codlocaliza = "COD_LOCALIZA";
        public FrmActivosLocalizacionesListDb(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene una lista de localizaciones de activos con paginación y filtros.
        /// </summary>
        public ErrorDto<ActivosLocalizacionesLista> Activos_LocalizacionesLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<ActivosLocalizacionesLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivosLocalizacionesLista()
                {
                    total = 0,
                    lista = new List<ActivosLocalizacionesData>()
                }
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                // Parámetros
                var p = new DynamicParameters();

                string? filtroLike = string.IsNullOrWhiteSpace(filtros?.filtro)
                    ? null
                    : $"%{filtros.filtro.Trim()}%";
                p.Add("@filtro", filtroLike, DbType.String);

                // Campo de orden, solo permitimos columnas conocidas
                string sortFieldNorm = (filtros?.sortField ?? _codlocaliza)
                    .Trim()
                    .ToUpperInvariant();

                string orderByCol = sortFieldNorm switch
                {
                    _codlocaliza      => _codlocaliza,
                    "DESCRIPCION"       => "DESCRIPCION",
                    "REGISTRO_USUARIO"  => "REGISTRO_USUARIO",
                    "MODIFICA_USUARIO"  => "MODIFICA_USUARIO",
                    _                   => _codlocaliza
                };

                string orderDir = (filtros?.sortOrder ?? 0) == 0 ? "DESC" : "ASC";

                int pagina = filtros?.pagina ?? 0;
                int paginacion = filtros?.paginacion ?? 50;
                p.Add("@offset", pagina, DbType.Int32);
                p.Add("@rows", paginacion, DbType.Int32);

                const string whereSql = @"
                    WHERE (@filtro IS NULL
                           OR COD_LOCALIZA     LIKE @filtro
                           OR DESCRIPCION      LIKE @filtro
                           OR REGISTRO_USUARIO LIKE @filtro
                           OR MODIFICA_USUARIO LIKE @filtro)";

                // Total (con los mismos filtros)
                string countSql = $@"
                    SELECT COUNT(COD_LOCALIZA)
                    FROM dbo.ACTIVOS_LOCALIZACIONES
                    {whereSql};";

                result.Result.total = connection.QueryFirstOrDefault<int>(countSql, p);

                // Datos paginados
                string dataSql = $@"
                    SELECT
                        COD_LOCALIZA                               AS cod_localiza,
                        DESCRIPCION                                AS descripcion,
                        CAST(ACTIVA AS bit)                        AS activo,
                        REGISTRO_USUARIO                           AS registro_usuario,
                        ISNULL(MODIFICA_USUARIO, '')               AS modifica_usuario,
                        CONVERT(varchar(19), REGISTRO_FECHA, 120)  AS registro_fecha,
                        ISNULL(CONVERT(varchar(19), MODIFICA_FECHA, 120), '') AS modifica_fecha
                    FROM dbo.ACTIVOS_LOCALIZACIONES
                    {whereSql}
                    ORDER BY {orderByCol} {orderDir}
                    OFFSET @offset ROWS
                    FETCH NEXT @rows ROWS ONLY;";

                result.Result.lista = connection
                    .Query<ActivosLocalizacionesData>(dataSql, p)
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
        /// Obtiene una lista de localizaciones de activos sin paginación, con filtros aplicados.
        /// </summary>
        public ErrorDto<List<ActivosLocalizacionesData>> Activos_Localizaciones_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<List<ActivosLocalizacionesData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosLocalizacionesData>()
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
                           OR COD_LOCALIZA     LIKE @filtro
                           OR DESCRIPCION      LIKE @filtro
                           OR REGISTRO_USUARIO LIKE @filtro
                           OR MODIFICA_USUARIO LIKE @filtro)";

                string query = $@"
                    SELECT
                        COD_LOCALIZA                               AS cod_localiza,
                        DESCRIPCION                                AS descripcion,
                        CAST(ACTIVA AS bit)                        AS activo,
                        REGISTRO_USUARIO                           AS registro_usuario,
                        ISNULL(MODIFICA_USUARIO, '')               AS modifica_usuario,
                        CONVERT(varchar(19), REGISTRO_FECHA, 120)  AS registro_fecha,
                        ISNULL(CONVERT(varchar(19), MODIFICA_FECHA, 120), '') AS modifica_fecha
                    FROM dbo.ACTIVOS_LOCALIZACIONES
                    {whereSql}
                    ORDER BY COD_LOCALIZA;";

                result.Result = connection
                    .Query<ActivosLocalizacionesData>(query, p)
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
        /// Inserta o actualiza una localizacion de activo.
        /// </summary>
        public ErrorDto Activos_Localizaciones_Guardar(int CodEmpresa, string usuario, ActivosLocalizacionesData localizacion)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                // Verifico si existe usuario (activo)
                var qUsuario = @"SELECT COUNT(Nombre) 
                             FROM usuarios 
                             WHERE estado = 'A' AND UPPER(Nombre) LIKE '%' + @usr + '%'";
                int existeuser = connection.QueryFirstOrDefault<int>(qUsuario, new { usr = usuario.ToUpper() });
                if (existeuser == 0)
                {
                    result.Code = -2;
                    result.Description = $"El usuario {usuario.ToUpper()} no existe o no está activo.";
                    return result;
                }

                // Verifico si existe localización por código
                var qExiste = @"SELECT ISNULL(COUNT(*),0) 
                            FROM dbo.ACTIVOS_LOCALIZACIONES  
                            WHERE UPPER(COD_LOCALIZA) = @cod";
                var existe = connection.QueryFirstOrDefault<int>(qExiste, new { cod = localizacion.cod_localiza.ToUpper() });

                if (localizacion.isNew)
                {
                    if (existe > 0)
                    {
                        result.Code = -2;
                        result.Description = $"La Localización con el código {localizacion.cod_localiza} ya existe.";
                    }
                    else
                    {
                        result = Activos_Localizaciones_Insertar(CodEmpresa, usuario, localizacion);
                    }
                }
                else if (existe == 0 && !localizacion.isNew)
                {
                    result.Code = -2;
                    result.Description = $"La Localización con el código {localizacion.cod_localiza} no existe.";
                }
                else
                {
                    result = Activos_Localizaciones_Actualizar(CodEmpresa, usuario, localizacion);
                }

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        private ErrorDto Activos_Localizaciones_Actualizar(int CodEmpresa, string usuario, ActivosLocalizacionesData localizacion)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = @"
                                UPDATE dbo.ACTIVOS_LOCALIZACIONES
                                SET DESCRIPCION      = @descripcion,
                                    ACTIVA           = @activo,
                                    MODIFICA_USUARIO = @modifica_usuario,
                                    MODIFICA_FECHA   = SYSDATETIME()
                                WHERE COD_LOCALIZA     = @cod_localiza";

                connection.Execute(query, new
                {
                    cod_localiza = localizacion.cod_localiza.ToUpper(),
                    descripcion = localizacion.descripcion?.ToUpper(),
                    activo = localizacion.activo,
                    modifica_usuario = usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Localización: {localizacion.cod_localiza} - {localizacion.descripcion}",
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

        private ErrorDto Activos_Localizaciones_Insertar(int CodEmpresa, string usuario, ActivosLocalizacionesData localizacion)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = @"
                                INSERT INTO dbo.ACTIVOS_LOCALIZACIONES
                                    (COD_LOCALIZA, DESCRIPCION, ACTIVA, REGISTRO_USUARIO, REGISTRO_FECHA, MODIFICA_USUARIO, MODIFICA_FECHA)
                                VALUES
                                    (@cod_localiza, @descripcion, @activo, @registro_usuario, SYSDATETIME(), NULL, NULL)";

                connection.Execute(query, new
                {
                    cod_localiza = localizacion.cod_localiza.ToUpper(),
                    descripcion = localizacion.descripcion?.ToUpper(),
                    activo = localizacion.activo,
                    registro_usuario = usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Localización: {localizacion.cod_localiza} - {localizacion.descripcion}",
                    Movimiento = "Registra - WEB",
                    Modulo = vModulo // 36
                });

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        public ErrorDto Activos_Localizaciones_Eliminar(int CodEmpresa, string usuario, string cod_localiza)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = @"DELETE FROM dbo.ACTIVOS_LOCALIZACIONES WHERE COD_LOCALIZA = @cod_localiza";
                connection.Execute(query, new { cod_localiza = cod_localiza.ToUpper() });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Localización: {cod_localiza}",
                    Movimiento = "Elimina - WEB",
                    Modulo = vModulo // 36
                });

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        public ErrorDto Activos_Localizaciones_Valida(int CodEmpresa, string cod_localiza)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                var query = @"SELECT COUNT(COD_LOCALIZA)
                          FROM dbo.ACTIVOS_LOCALIZACIONES
                          WHERE UPPER(COD_LOCALIZA) = @cod_localiza";
                var existe = connection.QueryFirstOrDefault<int>(query, new { cod_localiza = cod_localiza.ToUpper() });

                if (existe > 0)
                {
                    result.Code = -1;
                    result.Description = "El código de localización ya existe.";
                }
                else
                {
                    result.Code = 0;
                    result.Description = "El código de localización es válido.";
                }

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