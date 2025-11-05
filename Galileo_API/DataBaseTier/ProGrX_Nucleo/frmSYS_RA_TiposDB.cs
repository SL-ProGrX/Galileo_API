using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Nucleo;



namespace PgxAPI.DataBaseTier.ProGrX_Nucleo
{
    public class frmSYS_RA_TiposDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 10; 
        private readonly mSecurityMainDb _Security_MainDB;

        public frmSYS_RA_TiposDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }

        
        /// <summary>
        /// Consulta de lista de tipos de accesos registrados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<SysRaTiposLista> Sys_RaTiposLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO<SysRaTiposLista>()
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
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {

                    //Busco Total
                    query = $@"select COUNT(TIPO_ID) from SYS_EXP_TIPOS";
                    result.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( TIPO_ID LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "TIPO_ID";
                    }

                    query = $@"select TIPO_ID,descripcion,activo, Registro_Fecha,Registro_Usuario from SYS_EXP_TIPOS
                                        {filtros.filtro} 
                                     order by {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                         OFFSET {filtros.pagina} ROWS 
                                         FETCH NEXT { filtros.paginacion } ROWS ONLY ";
                    result.Result.lista = connection.Query<SysRaTiposData>(query).ToList();
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
        /// Inserta o modifica un tipo de acceso registrado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDTO Sys_RaTipos_Guardar(int CodEmpresa, string usuario, SysRaTiposData tipo)
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
              
                    //verifico si existe tipo
                    var query = $@"select isnull(count(*),0) as Existe from SYS_EXP_TIPOS  where UPPER(TIPO_ID) = @tipoId ";
                    var existe = connection.QueryFirstOrDefault<int>(query, new { tipoId = tipo.tipo_id.ToUpper() });

                    if (tipo.isNew)
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
                    else if (existe == 0 && !tipo.isNew)
                    {
                        result.Code = -2;
                        result.Description = $"El tipo con el código {tipo.tipo_id} no existe.";
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
        private ErrorDTO Sys_RaTipos_Actualizar(int CodEmpresa, string usuario, SysRaTiposData tipo)
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

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"RA Tipos: {tipo.tipo_id} - {tipo.descripcion}",
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
        /// Inserta  un tipo de acceso registrado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private ErrorDTO Sys_RaTipos_Insertar(int CodEmpresa, string usuario, SysRaTiposData tipo)
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
                    var query = $@"INSERT INTO SYS_EXP_TIPOS (TIPO_ID,descripcion,activo,registro_fecha,registro_usuario)
                                    VALUES (@tipo_id, @descripcion, @activo, Getdate(), @registro_usuario)";
                    connection.Execute(query, new
                    {
                        tipo_id = tipo.tipo_id,
                        descripcion = tipo.descripcion,
                        activo = tipo.activob ? 1 : 0,                        
                        registro_usuario = usuario
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"RA Tipos: {tipo.tipo_id} - {tipo.descripcion}",
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
        /// Elimina un tipo de acceso registrado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="codtipo"></param>
        /// <returns></returns>
        public ErrorDTO Sys_RaTipos_Eliminar(int CodEmpresa, string usuario, string codtipo)
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
                    var query = $@"DELETE FROM SYS_EXP_TIPOS WHERE TIPO_ID = @tipo_id";
                    connection.Execute(query, new { tipo_id = codtipo.ToUpper() });
                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"RA Tipos: {codtipo}",
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


    }
}
