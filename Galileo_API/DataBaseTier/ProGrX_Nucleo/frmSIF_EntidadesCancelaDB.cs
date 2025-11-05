using Dapper;
using MimeKit;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Nucleo;
using System.Data;
using Microsoft.Data.SqlClient;

namespace PgxAPI.DataBaseTier.ProGrX_Nucleo
{
    public class frmSIF_EntidadesCancelaDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 10;
        private readonly mSecurityMainDb _Security_MainDB;
        public frmSIF_EntidadesCancelaDB(IConfiguration config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }

        /// <summary>
        /// Lista las entidades pagadoras existentes con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<SIFEntidadesCancelaLista> SIF_EntidadesCancelaLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO<SIFEntidadesCancelaLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new SIFEntidadesCancelaLista()
                {
                    total = 0,
                    lista = new List<SIFEntidadesCancelaData>()
                }
            };
            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {

                    //Busco Total
                    query = $@"select COUNT(COD_ENTIDAD_PAGO) from SIF_ENTIDADES_PAGO";
                    result.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE (COD_ENTIDAD_PAGO LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "COD_ENTIDAD_PAGO";
                    }

                    query = $@"
                            select
                                COD_ENTIDAD_PAGO      as cod_entidad_pago,
                                descripcion         as descripcion,
                                activa              as activa,
                                Registro_Fecha      as registro_fecha,
                                Registro_Usuario    as registro_usuario
                            from SIF_ENTIDADES_PAGO
                                        {filtros.filtro} 
                                     order by {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                         OFFSET {filtros.pagina} ROWS 
                                         FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                    result.Result.lista = connection.Query<SIFEntidadesCancelaData>(query).ToList();
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
        /// Obtiene una lista de entidades pagadoras  sin paginación, con filtros aplicados. Para exportar.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<List<SIFEntidadesCancelaData>> SIF_EntidadesCancela_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO<List<SIFEntidadesCancelaData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SIFEntidadesCancelaData>()
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( COD_ENTIDAD_PAGO LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                    }
                    query = $@"select   COD_ENTIDAD_PAGO      as cod_entidad_pago,
                                descripcion         as descripcion,
                                activa              as activa,
                                Registro_Fecha      as registro_fecha,
                                Registro_Usuario    as registro_usuario
                            from SIF_ENTIDADES_PAGO
                                        {filtros.filtro} 
                                     order by COD_ENTIDAD_PAGO";
                    result.Result = connection.Query<SIFEntidadesCancelaData>(query).ToList();
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
        /// Elimina una entidad pagadora por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_entidad_pago"></param>
        /// <returns></returns>

        public ErrorDTO SIF_EntidadesCancela_Eliminar(int CodEmpresa, string usuario, string cod_entidad_pago)
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
                    var query = @"DELETE FROM SIF_ENTIDADES_PAGO WHERE COD_ENTIDAD_PAGO = @cod_entidad_pago";
                    connection.Execute(query, new { cod_entidad_pago = (cod_entidad_pago ?? string.Empty).ToUpper() });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Entidad Pagadora : {cod_entidad_pago}",
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
        /// Inserta o actualiza una entidad pagadora.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="entidad"></param>
        /// <returns></returns>
        /// 
        public ErrorDTO SIF_EntidadesCancela_Guardar(int CodEmpresa, string usuario, SIFEntidadesCancelaData entidad)
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
                    //Verifico si existe usuario
                    var qUsuario = $@"select count(Nombre) from usuarios where estado = 'A' and UPPER(Nombre) like '%{entidad.registro_usuario.ToUpper()}%' ";
                    int existeuser = connection.QueryFirstOrDefault<int>(qUsuario);
                    if (existeuser == 0)
                    {
                        result.Code = -2;
                        result.Description = $"El usuario {entidad.registro_usuario.ToUpper()} no existe o no está activo.";
                        return result;
                    }

                    //verifico si existe parentesco
                    var query = $@"select isnull(count(*),0) as Existe 
                           from SIF_ENTIDADES_PAGO  
                           where UPPER(COD_ENTIDAD_PAGO) = '{entidad.cod_entidad_pago.ToUpper()}' ";
                    var existe = connection.QueryFirstOrDefault<int>(query);

                    if (entidad.isNew)
                    {
                        if (existe > 0)
                        {
                            result.Code = -2;
                            result.Description = $"La Entidad pagadora con el código {entidad.cod_entidad_pago} ya existe.";
                        }
                        else
                        {
                            result = SIF_EntidadesCancela_Insertar(CodEmpresa, usuario, entidad);
                        }
                    }
                    else if (existe == 0 && !entidad.isNew)
                    {
                        result.Code = -2;
                        result.Description = $"La Entidad pagadora con el código {entidad.cod_entidad_pago} no existe.";
                    }
                    else
                    {
                        result = SIF_EntidadesCancela_Actualizar(CodEmpresa, usuario, entidad);
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
        /// <param name="entidad"></param>
        /// <returns></returns>
        private ErrorDTO SIF_EntidadesCancela_Actualizar(int CodEmpresa, string usuario, SIFEntidadesCancelaData entidad)
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
                    var query = $@"UPDATE SIF_ENTIDADES_PAGO
                               SET descripcion       = @descripcion,
                                   activa            = @activa,
                                   Registro_Fecha    = GETDATE(),
                                   Registro_Usuario  = @registro_usuario
                             WHERE COD_ENTIDAD_PAGO    = @cod_entidad_pago;";
                    connection.Execute(query, new
                    {
                        cod_entidad_pago = (entidad.cod_entidad_pago ?? string.Empty).ToUpper(),
                        descripcion = (entidad.descripcion ?? string.Empty).ToUpper(),
                        activa = entidad.activa,
                        registro_usuario = usuario
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Entidad Pagadora : {entidad.cod_entidad_pago} - {entidad.descripcion}",
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
        /// <param name="entidad"></param>
        /// <returns></returns>
        private ErrorDTO SIF_EntidadesCancela_Insertar(int CodEmpresa, string usuario, SIFEntidadesCancelaData entidad)
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
                    var query = $@"INSERT INTO SIF_ENTIDADES_PAGO
                                    (COD_ENTIDAD_PAGO, descripcion, activA, Registro_Fecha, Registro_Usuario)
                                VALUES
                                    (@cod_entidad_pago, @descripcion, @activa, GETDATE(), @registro_usuario);";
                    connection.Execute(query, new
                    {
                        cod_entidad_pago = (entidad.cod_entidad_pago ?? string.Empty).ToUpper(),
                        descripcion = (entidad.descripcion ?? string.Empty).ToUpper(),
                        activa = entidad.activa,
                        registro_usuario = usuario
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Entidad pagadora : {entidad.cod_entidad_pago} - {entidad.descripcion}",
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
        /// Valida si un código de entidad pagadora ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_entidad_pago"></param>
        /// <returns></returns>
        public ErrorDTO SIF_EntidadesCancela_Valida(int CodEmpresa, string cod_entidad_pago)
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
                    var query = $@"SELECT count(COD_ENTIDAD_PAGO) FROM SIF_ENTIDADES_PAGO WHERE UPPER(COD_ENTIDAD_PAGO) = @COD_ENTIDAD_PAGO";
                    var existe = connection.QueryFirstOrDefault<int>(query, new { cod_entidad_pago = cod_entidad_pago.ToUpper() });

                    if (existe > 0)
                    {
                        result.Code = -1;
                        result.Description = "El código de entidad ya existe.";
                    }
                    else
                    {
                        result.Code = 0;
                        result.Description = "El código de entidad es válido.";

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
