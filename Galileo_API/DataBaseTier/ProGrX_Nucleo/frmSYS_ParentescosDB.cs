using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Microsoft.Data.SqlClient;
using System.Data;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSysParentescosDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 10;
        private readonly MSecurityMainDb _Security_MainDB;
        public FrmSysParentescosDB(IConfiguration config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Lista los parentescos existentes con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<SysParentescosLista> SYS_ParentescosLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<SysParentescosLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new SysParentescosLista()
                {
                    total = 0,
                    lista = new List<SysParentescosData>()
                }
            };
            try
            {
                using var connection = new SqlConnection(stringConn);

                // Total (mantiene comportamiento anterior: total sin filtro)
                const string sqlTotal = @"select COUNT(COD_PARENTESCO) from SYS_PARENTESCOS";
                result.Result.total = connection.Query<int>(sqlTotal).FirstOrDefault();

                var raw = (filtros?.filtro ?? string.Empty).Trim();
                string? q = string.IsNullOrWhiteSpace(raw) ? null : $"%{raw}%";

                var sortField = (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(sortField))
                {
                    sortField = "cod_parentesco";
                }

                var sortOrder = filtros?.sortOrder ?? 1; // 0=DESC, 1=ASC

                var offset = Math.Max(0, filtros?.pagina ?? 0);
                var fetch = filtros?.paginacion ?? 0;
                if (fetch <= 0)
                {
                    fetch = 30;
                }

                const string sql = @"
                    select
                        COD_PARENTESCO      as cod_parentesco,
                        descripcion         as descripcion,
                        activo              as activo,
                        Registro_Fecha      as registro_fecha,
                        Registro_Usuario    as registro_usuario
                    from SYS_PARENTESCOS
                    where (@q is null or (
                        COD_PARENTESCO like @q
                        or descripcion like @q
                        or Registro_Usuario like @q
                    ))
                    order by
                        -- ASC
                        case when @sortOrder = 1 and @sortField = 'cod_parentesco' then COD_PARENTESCO end asc,
                        case when @sortOrder = 1 and @sortField = 'descripcion' then descripcion end asc,
                        case when @sortOrder = 1 and @sortField = 'activo' then convert(int, activo) end asc,
                        case when @sortOrder = 1 and @sortField = 'registro_fecha' then Registro_Fecha end asc,
                        case when @sortOrder = 1 and @sortField = 'registro_usuario' then Registro_Usuario end asc,

                        -- DESC
                        case when @sortOrder = 0 and @sortField = 'cod_parentesco' then COD_PARENTESCO end desc,
                        case when @sortOrder = 0 and @sortField = 'descripcion' then descripcion end desc,
                        case when @sortOrder = 0 and @sortField = 'activo' then convert(int, activo) end desc,
                        case when @sortOrder = 0 and @sortField = 'registro_fecha' then Registro_Fecha end desc,
                        case when @sortOrder = 0 and @sortField = 'registro_usuario' then Registro_Usuario end desc,

                        COD_PARENTESCO asc
                    offset @offset rows fetch next @fetch rows only;";

                result.Result.lista = connection.Query<SysParentescosData>(sql, new
                {
                    q,
                    sortField,
                    sortOrder,
                    offset,
                    fetch
                }).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = new List<SysParentescosData>();
            }
            return result;
        }


        /// <summary>
        /// Obtiene una lista de parentescos  sin paginación, con filtros aplicados. Para exportar.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<SysParentescosData>> SYS_Parentescos_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SysParentescosData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SysParentescosData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                var raw = (filtros?.filtro ?? string.Empty).Trim();
                string? q = string.IsNullOrWhiteSpace(raw) ? null : $"%{raw}%";

                const string sql = @"select
                                        COD_PARENTESCO      as cod_parentesco,
                                        descripcion         as descripcion,
                                        activo              as activo,
                                        Registro_Fecha      as registro_fecha,
                                        Registro_Usuario    as registro_usuario
                                     from SYS_PARENTESCOS
                                     where (@q is null or (
                                         COD_PARENTESCO like @q
                                         or descripcion like @q
                                         or Registro_Usuario like @q
                                     ))
                                     order by COD_PARENTESCO";

                result.Result = connection.Query<SysParentescosData>(sql, new { q }).ToList();
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
        /// Elimina un parentesco por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_parentesco"></param>
        /// <returns></returns>

        public ErrorDto SYS_Parentescos_Eliminar(int CodEmpresa, string usuario, string cod_parentesco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = @"DELETE FROM SYS_PARENTESCOS WHERE COD_PARENTESCO = @cod_parentesco";
                connection.Execute(query, new { cod_parentesco = (cod_parentesco ?? string.Empty).ToUpper() });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Parentesco : {cod_parentesco}",
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


        /// <summary>
        /// Inserta o actualiza un parentesco.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="ubicacion"></param>
        /// <returns></returns>
        /// 
        public ErrorDto SYS_Parentescos_Guardar(int CodEmpresa, string usuario, SysParentescosData parentesco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                //Verifico si existe usuario
                const string qUsuario = @"select count(Nombre)
                                         from usuarios
                                         where estado = 'A'
                                           and UPPER(Nombre) like @usr";
                var usrLike = $"%{(parentesco.registro_usuario ?? string.Empty).Trim().ToUpper()}%";
                int existeuser = connection.QueryFirstOrDefault<int>(qUsuario, new { usr = usrLike });
                if (existeuser == 0)
                {
                    result.Code = -2;
                    result.Description = $"El usuario {(parentesco.registro_usuario ?? string.Empty).ToUpper()} no existe o no está activo.";
                    return result;
                }

                //verifico si existe parentesco
                const string query = @"select isnull(count(*),0) as Existe
                                       from SYS_PARENTESCOS
                                       where UPPER(COD_PARENTESCO) = @cod";
                var existe = connection.QueryFirstOrDefault<int>(query, new { cod = (parentesco.cod_parentesco ?? string.Empty).Trim().ToUpper() });

                if (parentesco.isNew)
                {
                    if (existe > 0)
                    {
                        result.Code = -2;
                        result.Description = $"El parentesco con el código {parentesco.cod_parentesco} ya existe.";
                    }
                    else
                    {
                        result = SYS_Parentescos_Insertar(CodEmpresa, usuario, parentesco);
                    }
                }
                else if (existe == 0 && !parentesco.isNew)
                {
                    result.Code = -2;
                    result.Description = $"El parentesco con el código {parentesco.cod_parentesco} no existe.";
                }
                else
                {
                    result = SYS_Parentescos_Actualizar(CodEmpresa, usuario, parentesco);
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
        /// Actualiza un parentesco existente.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="parentesco"></param>
        /// <returns></returns>
        private ErrorDto SYS_Parentescos_Actualizar(int CodEmpresa, string usuario, SysParentescosData parentesco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = $@"UPDATE SYS_PARENTESCOS
                               SET descripcion       = @descripcion,
                                   activo            = @activo,
                                   Registro_Fecha    = GETDATE(),
                                   Registro_Usuario  = @registro_usuario
                             WHERE COD_PARENTESCO    = @cod_parentesco;";
                connection.Execute(query, new
                {
                    cod_parentesco = (parentesco.cod_parentesco ?? string.Empty).ToUpper(),
                    descripcion = (parentesco.descripcion ?? string.Empty).ToUpper(),
                    activo = parentesco.activo,
                    registro_usuario = usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Parentesco : {parentesco.cod_parentesco} - {parentesco.descripcion}",
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
        /// Inserta un nuevo parentesco.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="parentesco"></param>
        /// <returns></returns>
        private ErrorDto SYS_Parentescos_Insertar(int CodEmpresa, string usuario, SysParentescosData parentesco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = $@"INSERT INTO SYS_PARENTESCOS
                                    (COD_PARENTESCO, descripcion, activo, Registro_Fecha, Registro_Usuario)
                                VALUES
                                    (@cod_parentesco, @descripcion, @activo, GETDATE(), @registro_usuario);";
                connection.Execute(query, new
                {
                    cod_parentesco = (parentesco.cod_parentesco ?? string.Empty).ToUpper(),
                    descripcion = (parentesco.descripcion ?? string.Empty).ToUpper(),
                    activo = parentesco.activo,
                    registro_usuario = usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Parentesco : {parentesco.cod_parentesco} - {parentesco.descripcion}",
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
        /// Valida si un código de parentesco ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_parentesco"></param>
        /// <returns></returns>
        public ErrorDto SYS_Parentescos_Valida(int CodEmpresa, string cod_parentesco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                const string query = @"SELECT count(COD_PARENTESCO)
                                       FROM SYS_PARENTESCOS
                                       WHERE UPPER(COD_PARENTESCO) = @cod_parentesco";
                var existe = connection.QueryFirstOrDefault<int>(query, new { cod_parentesco = (cod_parentesco ?? string.Empty).Trim().ToUpper() });

                if (existe > 0)
                {
                    result.Code = -1;
                    result.Description = "El código de parentesco ya existe.";
                }
                else
                {
                    result.Code = 0;
                    result.Description = "El código de parentesco es válido.";

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