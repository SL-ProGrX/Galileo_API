using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSysParentescosDB
    {
        private readonly int vModulo = 10;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly PortalDB _portalDB;
        private const string ErrorInesperado = "Error inesperado";
        public FrmSysParentescosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
        }

        private static string? BuildSearchLike(FiltrosLazyLoadData? filtros)
        {
            var search = filtros?.filtro?.Trim();
            return string.IsNullOrWhiteSpace(search) ? null : $"%{search}%";
        }

        private static string NormalizeUpper(string? value)
            => (value ?? string.Empty).Trim().ToUpperInvariant();

        private const string BaseSelectParentescos = @"
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
                    ))";

        private ErrorDto LogBitacora(int codEmpresa, string usuario, string detalleMovimiento, string movimiento)
        {
            try
            {
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = (usuario ?? string.Empty).ToUpperInvariant(),
                    DetalleMovimiento = detalleMovimiento,
                    Movimiento = movimiento,
                    Modulo = vModulo
                });

                return DbHelper.CreateOkResponse();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message ?? ErrorInesperado);
            }
        }

        private ErrorDto ExecuteActualizar(SqlConnection connection, int codEmpresa, string usuario, SysParentescosData parentesco)
        {
            const string query = @"UPDATE SYS_PARENTESCOS
                               SET descripcion       = @descripcion,
                                   activo            = @activo,
                                   Registro_Fecha    = GETDATE(),
                                   Registro_Usuario  = @registro_usuario
                             WHERE COD_PARENTESCO    = @cod_parentesco;";

            connection.Execute(query, new
            {
                cod_parentesco = NormalizeUpper(parentesco.cod_parentesco),
                descripcion = NormalizeUpper(parentesco.descripcion),
                activo = parentesco.activo,
                registro_usuario = usuario
            });

            var bit = LogBitacora(codEmpresa, usuario, $"Parentesco : {parentesco.cod_parentesco} - {parentesco.descripcion}", "Modifica - WEB");
            return (bit.Code ?? -1) == 0 ? DbHelper.CreateOkResponse() : bit;
        }

        private ErrorDto ExecuteInsertar(SqlConnection connection, int codEmpresa, string usuario, SysParentescosData parentesco)
        {
            const string query = @"INSERT INTO SYS_PARENTESCOS
                                    (COD_PARENTESCO, descripcion, activo, Registro_Fecha, Registro_Usuario)
                                VALUES
                                    (@cod_parentesco, @descripcion, @activo, GETDATE(), @registro_usuario);";

            connection.Execute(query, new
            {
                cod_parentesco = NormalizeUpper(parentesco.cod_parentesco),
                descripcion = NormalizeUpper(parentesco.descripcion),
                activo = parentesco.activo,
                registro_usuario = usuario
            });

            var bit = LogBitacora(codEmpresa, usuario, $"Parentesco : {parentesco.cod_parentesco} - {parentesco.descripcion}", "Registra - WEB");
            return (bit.Code ?? -1) == 0 ? DbHelper.CreateOkResponse() : bit;
        }

        /// <summary>
        /// Lista los parentescos existentes con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<SysParentescosLista> SYS_ParentescosLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, connection =>
            {
                // Total (mantiene comportamiento anterior: total sin filtro)
                const string sqlTotal = @"select COUNT(COD_PARENTESCO) from SYS_PARENTESCOS";
                var total = connection.Query<int>(sqlTotal).FirstOrDefault();

                var q = BuildSearchLike(filtros);

                var sortField = (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(sortField))
                    sortField = "cod_parentesco";

                var sortOrder = filtros?.sortOrder ?? 1; // 0=DESC, 1=ASC

                var offset = Math.Max(0, filtros?.pagina ?? 0);
                var fetch = filtros?.paginacion ?? 0;
                if (fetch <= 0)
                    fetch = 30;

                var sql = $@"{BaseSelectParentescos}
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

                var lista = connection.Query<SysParentescosData>(sql, new
                {
                    q,
                    sortField,
                    sortOrder,
                    offset,
                    fetch
                }).ToList();

                return new SysParentescosLista
                {
                    total = total,
                    lista = lista
                };
            });
        }


        /// <summary>
        /// Obtiene una lista de parentescos  sin paginación, con filtros aplicados. Para exportar.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<SysParentescosData>> SYS_Parentescos_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, connection =>
            {
                var q = BuildSearchLike(filtros);

                var sql = $@"{BaseSelectParentescos}
                             order by COD_PARENTESCO";

                return connection.Query<SysParentescosData>(sql, new { q }).ToList();
            });
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
            var query = @"DELETE FROM SYS_PARENTESCOS WHERE COD_PARENTESCO = @cod_parentesco";
            var res = DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, query, new { cod_parentesco = NormalizeUpper(cod_parentesco) });

            if ((res.Code ?? -1) != 0)
                return res;

            var bit = LogBitacora(CodEmpresa, usuario, $"Parentesco : {cod_parentesco}", "Elimina - WEB");
            return (bit.Code ?? -1) == 0 ? res : bit;
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
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                // Verifico si existe usuario
                const string qUsuario = @"select count(Nombre)
                                         from usuarios
                                         where estado = 'A'
                                           and UPPER(Nombre) like @usr";

                var usrLike = $"%{NormalizeUpper(parentesco.registro_usuario)}%";
                var existeuser = connection.QueryFirstOrDefault<int>(qUsuario, new { usr = usrLike });

                if (existeuser == 0)
                {
                    return new ErrorDto
                    {
                        Code = -2,
                        Description = $"El usuario {NormalizeUpper(parentesco.registro_usuario)} no existe o no está activo."
                    };
                }

                // Verifico si existe parentesco
                const string queryExiste = @"select isnull(count(*),0) as Existe
                                            from SYS_PARENTESCOS
                                            where UPPER(COD_PARENTESCO) = @cod";

                var cod = NormalizeUpper(parentesco.cod_parentesco);
                var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { cod });

                if (parentesco.isNew)
                {
                    if (existe > 0)
                    {
                        return new ErrorDto
                        {
                            Code = -2,
                            Description = $"El parentesco con el código {parentesco.cod_parentesco} ya existe."
                        };
                    }

                    return ExecuteInsertar(connection, CodEmpresa, usuario, parentesco);
                }

                if (existe == 0)
                {
                    return new ErrorDto
                    {
                        Code = -2,
                        Description = $"El parentesco con el código {parentesco.cod_parentesco} no existe."
                    };
                }

                return ExecuteActualizar(connection, CodEmpresa, usuario, parentesco);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message ?? ErrorInesperado);
            }
        }


        /// <summary>
        /// Valida si un código de parentesco ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_parentesco"></param>
        /// <returns></returns>
        public ErrorDto SYS_Parentescos_Valida(int CodEmpresa, string cod_parentesco)
        {
            var query = @"SELECT count(COD_PARENTESCO)
                                       FROM SYS_PARENTESCOS
                                       WHERE UPPER(COD_PARENTESCO) = @cod_parentesco";

            var res = DbHelper.ExecuteSingleQuery<int>(_portalDB, CodEmpresa, query, 0, new { cod_parentesco = NormalizeUpper(cod_parentesco) });

            if ((res.Code ?? -1) != 0)
                return DbHelper.ErrorResponse(res.Description ?? "Error", res.Code ?? -1);

            var existe = res.Result;

            if (existe > 0)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "El código de parentesco ya existe."
                };
            }

            return new ErrorDto
            {
                Code = 0,
                Description = "El código de parentesco es válido."
            };
        }
    }
}