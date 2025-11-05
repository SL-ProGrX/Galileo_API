using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using static PgxAPI.Models.ProGrX_Activos_Fijos.frmActivos_Localizaciones_ListModels;
namespace PgxAPI.DataBaseTier.ProGrX_Activos_Fijos
{
    public class frmActivos_Localizaciones_ListDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 36; // Modulo de Tesorería
        private readonly mSecurityMainDb _Security_MainDB; 
       public frmActivos_Localizaciones_ListDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene una lista de localizaciones de activos con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<ActivosLocalizacionesLista> Activos_LocalizacionesLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO<ActivosLocalizacionesLista>()
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
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {

                    // Busco Total
                    query = @"SELECT COUNT(COD_LOCALIZA) FROM dbo.ACTIVOS_LOCALIZACIONES";
                    result.Result.total = connection.Query<int>(query).FirstOrDefault();

                    // WHERE (solo si viene filtro)
                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( COD_LOCALIZA LIKE '%" + filtros.filtro + "%' " +
                                         " OR DESCRIPCION LIKE '%" + filtros.filtro + "%' " +
                                         " OR REGISTRO_USUARIO LIKE '%" + filtros.filtro + "%' " +
                                         " OR MODIFICA_USUARIO LIKE '%" + filtros.filtro + "%' ) ";
                    }
                    if (string.IsNullOrEmpty(filtros.sortField))
                    {
                        filtros.sortField = "COD_LOCALIZA";
                    }
                    query = $@"
                    SELECT
                        COD_LOCALIZA                               AS cod_localiza,
                        DESCRIPCION                                AS descripcion,
                        CAST(ACTIVA AS bit)                        AS activo,
                        REGISTRO_USUARIO                           AS registro_usuario,
                        ISNULL(MODIFICA_USUARIO, '')               AS modifica_usuario,
                        CONVERT(varchar(19), REGISTRO_FECHA, 120)  AS registro_fecha,
                        ISNULL(CONVERT(varchar(19), MODIFICA_FECHA, 120), '') AS modifica_fecha
                    FROM dbo.ACTIVOS_LOCALIZACIONES
                    {filtros.filtro}
                    ORDER BY {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                    OFFSET {filtros.pagina} ROWS
                    FETCH NEXT {filtros.paginacion} ROWS ONLY
";
                    result.Result.lista = connection.Query<ActivosLocalizacionesData>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = null;
            }
            return result;
        }

        /// <summary>
        /// Obtiene una lista de localizaciones de activos sin paginación, con filtros aplicados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<List<ActivosLocalizacionesData>> Activos_Localizaciones_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO<List<ActivosLocalizacionesData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosLocalizacionesData>()
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( COD_LOCALIZA LIKE '%" + filtros.filtro + "%' " +
                         " OR DESCRIPCION LIKE '%" + filtros.filtro + "%' " +
                         " OR REGISTRO_USUARIO LIKE '%" + filtros.filtro + "%' " +
                         " OR MODIFICA_USUARIO LIKE '%" + filtros.filtro + "%' ) ";
                    }
                    query = $@"
                    SELECT
                        COD_LOCALIZA                               AS cod_localiza,
                        DESCRIPCION                                AS descripcion,
                        CAST(ACTIVA AS bit)                        AS activo,
                        REGISTRO_USUARIO                           AS registro_usuario,
                        ISNULL(MODIFICA_USUARIO, '')               AS modifica_usuario,
                        CONVERT(varchar(19), REGISTRO_FECHA, 120)  AS registro_fecha,
                        ISNULL(CONVERT(varchar(19), MODIFICA_FECHA, 120), '') AS modifica_fecha
                    FROM dbo.ACTIVOS_LOCALIZACIONES
                    {filtros.filtro}
                    ORDER BY COD_LOCALIZA";

                    result.Result = connection.Query<ActivosLocalizacionesData>(query).ToList();
                }
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
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="localizacion"></param>
        /// <returns></returns>
        public ErrorDTO Activos_Localizaciones_Guardar(int CodEmpresa, string usuario, ActivosLocalizacionesData localizacion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO()
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
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
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// Actualiza una localizacion de activos existente.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="localizacion"></param>
        /// <returns></returns>
        private ErrorDTO Activos_Localizaciones_Actualizar(int CodEmpresa, string usuario, ActivosLocalizacionesData localizacion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO()
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
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

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Localización: {localizacion.cod_localiza} - {localizacion.descripcion}",
                        Movimiento = "Modifica - WEB",
                        Modulo = vModulo
                    });
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
        /// Inserta una nueva localizacion de activo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="localizacion"></param>
        /// <returns></returns>
        private ErrorDTO Activos_Localizaciones_Insertar(int CodEmpresa, string usuario, ActivosLocalizacionesData localizacion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO()
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
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

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Localización: {localizacion.cod_localiza} - {localizacion.descripcion}",
                        Movimiento = "Registra - WEB",
                        Modulo = vModulo // 36
                    });
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
        /// Elimina una localización por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_localiza"></param>
        public ErrorDTO Activos_Localizaciones_Eliminar(int CodEmpresa, string usuario, string cod_localiza)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"DELETE FROM dbo.ACTIVOS_LOCALIZACIONES WHERE COD_LOCALIZA = @cod_localiza";
                    connection.Execute(query, new { cod_localiza = cod_localiza.ToUpper() });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Localización: {cod_localiza}",
                        Movimiento = "Elimina - WEB",
                        Modulo = vModulo // 36
                    });
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
        /// Valida si un código de localización ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_localiza"></param>
        public ErrorDTO Activos_Localizaciones_Valida(int CodEmpresa, string cod_localiza)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
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
