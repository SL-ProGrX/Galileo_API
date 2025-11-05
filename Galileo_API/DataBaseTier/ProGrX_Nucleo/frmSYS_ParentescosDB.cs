using Dapper;
using MimeKit;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Nucleo;
using System.Data;
using Microsoft.Data.SqlClient;

namespace PgxAPI.DataBaseTier.ProGrX_Nucleo
{
    public class frmSYS_ParentescosDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 10;
        private readonly mSecurityMainDb _Security_MainDB;
        public frmSYS_ParentescosDB(IConfiguration config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
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
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {

                    //Busco Total
                    query = $@"select COUNT(COD_PARENTESCO) from SYS_PARENTESCOS";
                    result.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE (COD_PARENTESCO LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "COD_PARENTESCO";
                    }

                    query = $@"
                            select
                                COD_PARENTESCO      as cod_parentesco,
                                descripcion         as descripcion,
                                activo              as activo,
                                Registro_Fecha      as registro_fecha,
                                Registro_Usuario    as registro_usuario
                            from SYS_PARENTESCOS
                                        {filtros.filtro} 
                                     order by {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                         OFFSET {filtros.pagina} ROWS 
                                         FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                    result.Result.lista = connection.Query<SysParentescosData>(query).ToList();
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
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( COD_PARENTESCO LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                    }
                    query = $@"select   COD_PARENTESCO      as cod_parentesco,
                                descripcion         as descripcion,
                                activo              as activo,
                                Registro_Fecha      as registro_fecha,
                                Registro_Usuario    as registro_usuario
                            from SYS_PARENTESCOS
                                        {filtros.filtro} 
                                     order by COD_PARENTESCO";
                    result.Result = connection.Query<SysParentescosData>(query).ToList();
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
                {
                    var query = @"DELETE FROM SYS_PARENTESCOS WHERE COD_PARENTESCO = @cod_parentesco";
                    connection.Execute(query, new { cod_parentesco = (cod_parentesco ?? string.Empty).ToUpper() });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Parentesco : {cod_parentesco}",
                        Movimiento = "Elimina - WEB",
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
                {
                    //Verifico si existe usuario
                    var qUsuario = $@"select count(Nombre) from usuarios where estado = 'A' and UPPER(Nombre) like '%{parentesco.registro_usuario.ToUpper()}%' ";
                    int existeuser = connection.QueryFirstOrDefault<int>(qUsuario);
                    if (existeuser == 0)
                    {
                        result.Code = -2;
                        result.Description = $"El usuario {parentesco.registro_usuario.ToUpper()} no existe o no está activo.";
                        return result;
                    }

                    //verifico si existe parentesco
                    var query = $@"select isnull(count(*),0) as Existe 
                           from SYS_PARENTESCOS  
                           where UPPER(COD_PARENTESCO) = '{parentesco.cod_parentesco.ToUpper()}' ";
                    var existe = connection.QueryFirstOrDefault<int>(query);

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
                {
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

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Parentesco : {parentesco.cod_parentesco} - {parentesco.descripcion}",
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
                {
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

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Parentesco : {parentesco.cod_parentesco} - {parentesco.descripcion}",
                        Movimiento = "Registra - WEB",
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
                {
                    var query = $@"SELECT count(COD_PARENTESCO) FROM SYS_PARENTESCOS WHERE UPPER(COD_PARENTESCO) = @COD_PARENTESCO";
                    var existe = connection.QueryFirstOrDefault<int>(query, new { cod_parentesco = cod_parentesco.ToUpper() });

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