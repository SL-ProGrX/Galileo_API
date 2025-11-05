
using Dapper;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using Microsoft.Data.SqlClient;
using static PgxAPI.Models.ProGrX_Nucleo.frmSYS_Origen_RecursosModels;
 

namespace PgxAPI.DataBaseTier.ProGrX_Nucleo
{
    public class frmSYS_Origen_RecursosDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 10; // Modulo de Tesorería
        private readonly mSecurityMainDb _Security_MainDB;

        public frmSYS_Origen_RecursosDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene una lista de origen recurso
        /// </summary>
        /// <param name="CodEmpresa"></param> 
        /// <returns></returns>
        public ErrorDTO<SysOrigen_RecursosLista> Sys_OrigenRecursosLista_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO<SysOrigen_RecursosLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new SysOrigen_RecursosLista()
                {
                    total = 0,
                    lista = new List<SysOrigen_RecursosData>()
                }
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {

                    //Busco Total
                    query = $@"select COUNT(COD_ORIGEN_RECURSOS) from SIF_ORIGEN_RECURSOS";
                    result.Result.total = connection.Query<int>(query).FirstOrDefault();
                    query = $@"select COD_ORIGEN_RECURSOS, Descripcion, Activa, Registro_Fecha, Registro_Usuario from SIF_ORIGEN_RECURSOS ";
                    result.Result.lista = connection.Query<SysOrigen_RecursosData>(query).ToList();
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
        /// Inserta o actualiza un origen recursos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="OrigenRecursos"></param>
        /// <returns></returns>
        public ErrorDTO Sys_OrigenRecursos_Guardar(int CodEmpresa, SysOrigen_RecursosData OrigenRecursos)
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
                 
                    //verifico si existe el recurso
                    var query = $@"select isnull(count(*),0) as Existe from SIF_ORIGEN_RECURSOS where UPPER(COD_ORIGEN_RECURSOS) = @OrigenRecursos ";
                    var existe = connection.QueryFirstOrDefault<int>(query, new { OrigenRecursos = OrigenRecursos.cod_origen_recursos.ToUpper() });

                    if (OrigenRecursos.isNew)
                    {
                        if (existe > 0)
                        {
                            result.Code = -2;
                            result.Description = $"El  origen recursos con el código {OrigenRecursos.cod_origen_recursos} ya existe.";
                        }
                        else
                        {
                            result = Sys_OrigenRecursos_Insertar(CodEmpresa, OrigenRecursos);
                        }
                    }
                    else if (existe == 0 && !OrigenRecursos.isNew)
                    {
                        result.Code = -2;
                        result.Description = $"El  origen recursos con el código {OrigenRecursos.cod_origen_recursos} no existe.";
                    }
                    else
                    {
                        result = Sys_OrigenRecursos_Actualizar(CodEmpresa, OrigenRecursos);
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
        /// Actualiza un  origen recursos existente.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="OrigenRecursos"></param>
        /// <returns></returns>
        private ErrorDTO Sys_OrigenRecursos_Actualizar(int CodEmpresa, SysOrigen_RecursosData OrigenRecursos)
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
                    var query = $@"update SIF_ORIGEN_RECURSOS
                                    set Descripcion = @descripcion,
                                        Activa = @estado,
                                        ACTUALIZA_FECHA = dbo.MyGetDate(),
                                        ACTUALIZA_USUARIO = @usuario
                                    WHERE COD_ORIGEN_RECURSOS = @cod_Origen_Recursos";
                    connection.Execute(query, new
                    {
                        cod_Origen_Recursos = OrigenRecursos.cod_origen_recursos.ToUpper(),
                        descripcion = OrigenRecursos.descripcion,
                        estado = OrigenRecursos.activa,
                        usuario= OrigenRecursos.actualiza_usuario
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = OrigenRecursos.registro_usuario,
                        DetalleMovimiento = $"Origen de Recursos:  {OrigenRecursos.cod_origen_recursos} - {OrigenRecursos.descripcion}",
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
        ///  Inserta un nuevo origen recursos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="OrigenRecursos"></param>
        /// <returns></returns>
        private ErrorDTO Sys_OrigenRecursos_Insertar(int CodEmpresa, SysOrigen_RecursosData OrigenRecursos)
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
                    var query = $@"insert into SIF_ORIGEN_RECURSOS(COD_ORIGEN_RECURSOS, Descripcion, Activa, Registro_fecha, Registro_usuario)
                                    VALUES (@cod_Origen_Recursos, @descripcion, @estado,dbo.MyGetdate(), @usuario)";
                    connection.Execute(query, new
                    {
                        cod_Origen_Recursos = OrigenRecursos.cod_origen_recursos.ToUpper(),
                        descripcion = OrigenRecursos.descripcion,
                        estado = OrigenRecursos.activa,
                        usuario = OrigenRecursos.registro_usuario
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = OrigenRecursos.registro_usuario,
                        DetalleMovimiento = $" Origen de Recursos: {OrigenRecursos.cod_origen_recursos} - {OrigenRecursos.descripcion}",
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
        /// Elimina un origen recursos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="OrigenRecursos"></param>
        /// <returns></returns>
        public ErrorDTO Sys_OrigenRecursos_Eliminar(int CodEmpresa, string usuario, string OrigenRecursos)
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
                    var query = $@"delete SIF_ORIGEN_RECURSOS where COD_ORIGEN_RECURSOS = @OrigenRecursos";
                    connection.Execute(query, new { OrigenRecursos = OrigenRecursos.Trim().ToUpper() });
                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Origen de Recursos: {OrigenRecursos}",
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
