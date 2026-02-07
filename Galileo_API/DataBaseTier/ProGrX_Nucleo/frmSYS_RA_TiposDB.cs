using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSysRaTiposDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 10;
        private readonly MSecurityMainDb _Security_MainDB;
        public FrmSysRaTiposDB(IConfiguration config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
        }


        /// <summary>
        /// Consulta de lista de tipos de accesos registrados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<SysRaTiposLista> Sys_RaTiposLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<SysRaTiposLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new SysRaTiposLista()
                {
                    total = 0,
                    lista = new List<SysRaTiposData>()
                }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                // Total (mantiene comportamiento anterior: total sin filtro)
                const string sqlTotal = @"select COUNT(TIPO_ID) from SYS_EXP_TIPOS";
                result.Result.total = connection.Query<int>(sqlTotal).FirstOrDefault();

                var raw = (filtros?.filtro ?? string.Empty).Trim();
                string? q = string.IsNullOrWhiteSpace(raw) ? null : $"%{raw}%";

                var sortField = (filtros?.sortField ?? string.Empty).Trim().ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(sortField))
                {
                    sortField = "tipo_id";
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
                        TIPO_ID,
                        descripcion,
                        activo,
                        Registro_Fecha,
                        Registro_Usuario
                    from SYS_EXP_TIPOS
                    where (@q is null or (
                        TIPO_ID like @q
                        or descripcion like @q
                        or Registro_Usuario like @q
                    ))
                    order by
                        -- ASC
                        case when @sortOrder = 1 and (@sortField = 'tipo_id' or @sortField = 'tipo') then TIPO_ID end asc,
                        case when @sortOrder = 1 and @sortField = 'descripcion' then descripcion end asc,
                        case when @sortOrder = 1 and @sortField = 'activo' then convert(int, activo) end asc,
                        case when @sortOrder = 1 and @sortField = 'registro_fecha' then Registro_Fecha end asc,
                        case when @sortOrder = 1 and @sortField = 'registro_usuario' then Registro_Usuario end asc,

                        -- DESC
                        case when @sortOrder = 0 and (@sortField = 'tipo_id' or @sortField = 'tipo') then TIPO_ID end desc,
                        case when @sortOrder = 0 and @sortField = 'descripcion' then descripcion end desc,
                        case when @sortOrder = 0 and @sortField = 'activo' then convert(int, activo) end desc,
                        case when @sortOrder = 0 and @sortField = 'registro_fecha' then Registro_Fecha end desc,
                        case when @sortOrder = 0 and @sortField = 'registro_usuario' then Registro_Usuario end desc,

                        -- Fallback
                        TIPO_ID asc
                    offset @offset rows fetch next @fetch rows only;";

                result.Result.lista = connection.Query<SysRaTiposData>(sql, new
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
                result.Result.lista = new List<SysRaTiposData>();
            }
            return result;
        }


        /// <summary>
        /// Inserta o modifica un tipo de acceso registrado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto Sys_RaTipos_Guardar(int CodEmpresa, string usuario, SysRaTiposData tipo)
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
                //verifico si existe tipo
                var query = $@"select isnull(count(*),0) as Existe from SYS_EXP_TIPOS  where UPPER(TIPO_ID) = @tipoId ";
                var tipoIdUpper = tipo?.tipo_id?.ToUpper() ?? string.Empty;
                var existe = connection.QueryFirstOrDefault<int>(query, new { tipoId = tipoIdUpper });

                if (tipo != null && tipo.isNew)
                {
                    if (existe > 0)
                    {
                        result.Code = -2;
                        result.Description = $"El tipo con el código {tipo.tipo_id} ya existe.";
                    }
                    else
                    {
                        result = Sys_RaTipos_Insertar(CodEmpresa, usuario, tipo);
                    }
                }
                else if (existe == 0 && tipo != null && !tipo.isNew)
                {
                    result.Code = -2;
                    result.Description = $"El tipo con el código {tipo.tipo_id} no existe.";
                }
                else
                {
                    if (tipo == null)
                    {
                        result.Code = -2;
                        result.Description = "El parámetro 'tipo' no puede ser nulo.";
                    }
                    else
                    {
                        result = Sys_RaTipos_Actualizar(CodEmpresa, usuario, tipo);
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
        /// Actualiza un tipo de acceso registrado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private ErrorDto Sys_RaTipos_Actualizar(int CodEmpresa, string usuario, SysRaTiposData tipo)
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
                var query = $@"UPDATE SYS_EXP_TIPOS
                                    SET descripcion = @descripcion,
                                        activo = @activo
                                    WHERE TIPO_ID = @tipo_id";
                connection.Execute(query, new
                {
                    tipo_id = tipo.tipo_id,
                    descripcion = tipo.descripcion,
                    activo = tipo.activob ? 1 : 0
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"RA Tipos: {tipo.tipo_id} - {tipo.descripcion}",
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
        /// Inserta  un tipo de acceso registrado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private ErrorDto Sys_RaTipos_Insertar(int CodEmpresa, string usuario, SysRaTiposData tipo)
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
                var query = $@"INSERT INTO SYS_EXP_TIPOS (TIPO_ID,descripcion,activo,registro_fecha,registro_usuario)
                                    VALUES (@tipo_id, @descripcion, @activo, Getdate(), @registro_usuario)";
                connection.Execute(query, new
                {
                    tipo_id = tipo.tipo_id,
                    descripcion = tipo.descripcion,
                    activo = tipo.activob ? 1 : 0,
                    registro_usuario = usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"RA Tipos: {tipo.tipo_id} - {tipo.descripcion}",
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
        /// Elimina un tipo de acceso registrado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="codtipo"></param>
        /// <returns></returns>
        public ErrorDto Sys_RaTipos_Eliminar(int CodEmpresa, string usuario, string codtipo)
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
                var query = $@"DELETE FROM SYS_EXP_TIPOS WHERE TIPO_ID = @tipo_id";
                connection.Execute(query, new { tipo_id = codtipo.ToUpper() });
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"RA Tipos: {codtipo}",
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