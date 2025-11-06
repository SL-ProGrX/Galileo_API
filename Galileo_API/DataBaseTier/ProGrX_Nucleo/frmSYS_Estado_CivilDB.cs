
using Dapper;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using Microsoft.Data.SqlClient;
using static PgxAPI.Models.ProGrX_Nucleo.FrmSysEstadoCivilModels;
using PgxAPI.Models.Security;


namespace PgxAPI.DataBaseTier.ProGrX_Nucleo
{
    public class frmSYS_Estado_CivilDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 10; // Modulo de Tesorería
        private readonly MSecurityMainDb _Security_MainDB;

        public frmSYS_Estado_CivilDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene una lista de estados civil sin paginación, con filtros aplicados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<SysEstadoCivilLista> Sys_EstadoCivilLista_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<SysEstadoCivilLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new SysEstadoCivilLista()
                {
                    total = 0,
                    lista = new List<SysEstadoCivilData>()
                }
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {

                    //Busco Total
                    query = $@"select COUNT(ESTADO_CIVIL) from SYS_ESTADO_CIVIL";
                    result.Result.total = connection.Query<int>(query).FirstOrDefault();


                    query = $@"select ESTADO_CIVIL as cod_estado_civil,descripcion,Registro_Fecha as registro_fecha,Registro_Usuario ,ACTIVO from SYS_ESTADO_CIVIL order by ESTADO_CIVIL";
                    result.Result.lista = connection.Query<SysEstadoCivilData>(query).ToList();
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
        /// Inserta o actualiza un estado civil
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="estadoCivil"></param>
        /// <returns></returns>
        public ErrorDto Sys_EstadoCivil_Guardar(int CodEmpresa, SysEstadoCivilData estadoCivil)
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
                    //verifico si existe el estado civil 
                    var query = $@"select isnull(count(*),0) as Existe from SYS_ESTADO_CIVIL  where UPPER(ESTADO_CIVIL) = @estadoCivil ";
                    var existe = connection.QueryFirstOrDefault<int>(query, new { estadoCivil = estadoCivil.cod_estado_civil.ToUpper() });

                    if (estadoCivil.isNew)
                    {
                        if (existe > 0)
                        {
                            result.Code = -2;
                            result.Description = $"El estado civil con el código {estadoCivil.cod_estado_civil} ya existe.";
                        }
                        else
                        {
                            result = Sys_EstadoCivil_Insertar(CodEmpresa, estadoCivil);
                        }
                    }
                    else if (existe == 0 && !estadoCivil.isNew)
                    {
                        result.Code = -2;
                        result.Description = $"El estado civil con el código {estadoCivil.cod_estado_civil} no existe.";
                    }
                    else
                    {
                        result = Sys_EstadoCivil_Actualizar(CodEmpresa, estadoCivil);
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
        /// Actualiza un estado civil existente.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="estadoCivil"></param>
        /// <returns></returns>
        private ErrorDto Sys_EstadoCivil_Actualizar(int CodEmpresa, SysEstadoCivilData estadoCivil)
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
                    var query = $@"UPDATE SYS_ESTADO_CIVIL
                                    SET Descripcion = @descripcion,
                                        activo = @estado
                                    WHERE ESTADO_CIVIL = @cod_estado_civil";
                    connection.Execute(query, new
                    {
                        cod_estado_civil = estadoCivil.cod_estado_civil.ToUpper(),
                        descripcion = estadoCivil.descripcion,
                        estado = estadoCivil.activo 
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = estadoCivil.registro_usuario,
                        DetalleMovimiento = $"Estado Civil:  {estadoCivil.cod_estado_civil} - {estadoCivil.descripcion}",
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
        ///  Inserta un nuevo estado civil
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="estadoCivil"></param>
        /// <returns></returns>
        private ErrorDto Sys_EstadoCivil_Insertar(int CodEmpresa, SysEstadoCivilData estadoCivil)
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
                    var query = $@"INSERT INTO SYS_ESTADO_CIVIL (ESTADO_CIVIL,descripcion,activo,registro_fecha,registro_usuario)
                                    VALUES (@cod_ubicacion, @descripcion, @estado,Getdate(), @usuario)";
                    connection.Execute(query, new
                    {
                        cod_ubicacion = estadoCivil.cod_estado_civil.ToUpper(),
                        descripcion = estadoCivil.descripcion,
                        estado = estadoCivil.activo,
                        usuario = estadoCivil.registro_usuario
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = estadoCivil.registro_usuario,
                        DetalleMovimiento = $"Estado Civil: {estadoCivil.cod_estado_civil} - {estadoCivil.descripcion}",
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
        /// Elimina un estado civil
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="estadoCivil"></param>
        /// <returns></returns>
        public ErrorDto Sys_EstadoCivil_Eliminar(int CodEmpresa, string usuario, string estadoCivil)
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
                    var query = $@"DELETE FROM SYS_ESTADO_CIVIL WHERE ESTADO_CIVIL = @estadoCivil";
                    connection.Execute(query, new { estadoCivil = estadoCivil.ToUpper() });
                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Estado Civil: {estadoCivil}",
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
