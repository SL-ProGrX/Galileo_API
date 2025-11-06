using Dapper;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Nucleo;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.Security;

namespace PgxAPI.DataBaseTier.ProGrX_Nucleo
{
    public class frmSYS_EducacionDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 10; // Modulo de Tesorería
        private readonly MSecurityMainDb _Security_MainDB;


        public frmSYS_EducacionDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
        }
      
       /// <summary>
      /// Consulta la lista de centros educativos
      /// </summary>
      /// <param name="CodEmpresa"></param>
      /// <param name="tipo"></param>
      /// <param name="filtros"></param>
      /// <returns></returns>
        public ErrorDto<SysEducacionLista> Sys_EducacionlLista_Obtener(int CodEmpresa,string tipo, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<SysEducacionLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new SysEducacionLista()
                {
                    total = 0,
                    lista = new List<SysEducacionData>()
                }
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {


                    if (filtros.filtro != null && filtros.filtro.Trim()!= "")
                    {
                        filtros.filtro = $" WHERE Tipo = '{tipo}'  and ( cod_Educ LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' )" ;
                    }
                    else
                    {
                        filtros.filtro = $" WHERE  Tipo = '{tipo}'";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "cod_Educ";
                    }

                    //Busco Total
                    //query = $@"select cod_Educ, descripcion, Activa, 0 as btn from SYS_EDUCACION_CFG Where Tipo = '{tipo}' order by cod_Educ";

                    query = $@"select cod_Educ, descripcion, Activa, 0 as btn from SYS_EDUCACION_CFG  
                                        {filtros.filtro} 
                                     order by {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                         OFFSET {filtros.pagina} ROWS 
                                         FETCH NEXT {filtros.paginacion} ROWS ONLY ";


                    result.Result.lista = connection.Query<SysEducacionData>(query).ToList();
                    result.Result.total = result.Result.lista.Count;
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
        /// Actualiza o inserta datos de centros educativos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto Sys_Educacion_Guardar(int CodEmpresa, string usuario, SysEducacionData datos)
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
                    //verifico si existe dato
                    var query = $@"select isnull(count(*),0) as Existe from SYS_EDUCACION_CFG where cod_Educ ='{datos.cod_educ}' and Tipo ='{datos.tipo}' ";
                    var existe = connection.QueryFirstOrDefault<int>(query, new { dato = datos.cod_educ });

                    if (datos.isNew)
                    {
                        if (existe > 0)
                        {
                            result.Code = -2;
                            result.Description = $"El código no puede ser utilizado! Ya existe un item diferente con su uso!";
                        }
                        else
                        {
                            result = Sys_Educacion_Insertar(CodEmpresa, usuario, datos);
                        }
                    }
                    else if (existe == 0 && !datos.isNew)
                    {
                        result.Code = -2;
                        result.Description = $"El código {datos.cod_educ} no existe.";
                    }
                    else
                    {
                        result = Sys_Educacion_Actualizar(CodEmpresa, usuario, datos);
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
        /// Actualiza centros educativos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="dato"></param>
        /// <returns></returns>
        private ErrorDto Sys_Educacion_Actualizar(int CodEmpresa, string usuario, SysEducacionData dato)
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
                    var query = $@"UPDATE SYS_EDUCACION_CFG
                                    SET descripcion = @descripcion,
                                        Activa = @estado
                                    WHERE  cod_Educ = @cod_dato";
                    connection.Execute(query, new
                    {
                        cod_dato = dato.cod_educ,
                        descripcion = dato.descripcion,
                        estado = dato.activa
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Educacion Doc. : {dato.cod_educ} - {dato.descripcion}",
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
        /// Inserta centros educativos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="dato"></param>
        /// <returns></returns>
        private ErrorDto Sys_Educacion_Insertar(int CodEmpresa, string usuario, SysEducacionData dato)
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
                    var query = $@"INSERT SYS_EDUCACION_CFG(cod_Educ, Tipo, descripcion, Activa, Registro_Usuario, Registro_Fecha) 
                                    VALUES (@cod_dato,@tipo, @descripcion, @estado, @usuario, dbo.MyGetdate() )";
                    connection.Execute(query, new
                    {
                        cod_dato = dato.cod_educ,
                        tipo = dato.tipo,
                        descripcion = dato.descripcion,
                        estado = dato.activa ,
                        usuario = usuario
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Educacion Doc. : {dato.cod_educ} - {dato.descripcion}",
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
        /// Elimina centros educativos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_Educ"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto Sys_Educacion_Eliminar(int CodEmpresa, string usuario, string cod_Educ,string tipo)
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
                    var query = $@"DELETE SYS_EDUCACION_CFG where cod_Educ =  @cod_Educ and Tipo = @tipo";
                    connection.Execute(query, new { cod_Educ = cod_Educ, tipo = tipo });
                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Educacion Doc. : {cod_Educ}",
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
        /// Consulta el detalle de centros educativos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipoDetalleEduc"></param>
        /// <param name="cod_Educ"></param>
        /// <returns></returns>
        public ErrorDto<List<SysEducacionDetalleData>> Sys_EducacionDetalle_Consulta(int CodEmpresa, string tipoDetalleEduc, string cod_Educ)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SysEducacionDetalleData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SysEducacionDetalleData>()
            };

            try
            {
                
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spSys_Educacion_Asigna_Consulta '{cod_Educ.Trim()}','{tipoDetalleEduc}' ";
                    result.Result = connection.Query<SysEducacionDetalleData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new List<SysEducacionDetalleData>();
            }
            return result;
        }

        /// <summary>
        /// Asigna o des asigna detalle de centros educativos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_Educ"></param>
        /// <param name="cod_DetalleEduc"></param>
        /// <param name="accion"></param>
        /// <returns></returns>
        public ErrorDto Sys_EducacionDetalle_Asignar(int CodEmpresa, string usuario, string cod_Educ, string cod_DetalleEduc, bool accion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            string check = accion == true ? "A" : "E";
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {


                    var query = $@"exec spSys_Educacion_Asigna @cod_Educ,@cod_DetalleEduc,@usuario,@check";
                    connection.Execute(query,
                         new
                         {
                             cod_Educ = cod_Educ.Trim(),
                             cod_DetalleEduc = cod_DetalleEduc,
                             usuario = usuario,
                             check = check
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
