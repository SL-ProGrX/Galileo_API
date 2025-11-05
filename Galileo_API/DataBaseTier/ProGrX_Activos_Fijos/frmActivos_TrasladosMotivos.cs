using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Activos_Fijos;

namespace PgxAPI.DataBaseTier.ProGrX_Activos_Fijos
{
    public class frmActivos_TrasladosMotivosDB
    {

        private readonly IConfiguration? _config;
        private readonly int vModulo = 36;
        private readonly mSecurityMainDb _Security_MainDB;

        public frmActivos_TrasladosMotivosDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }
        /// <summary>
        /// Metodo para consultar la lista de motivos de traslados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<ActivosTrasladosMotivosDataLista> Activos_TrasladosMotivos_Consultar(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO<ActivosTrasladosMotivosDataLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivosTrasladosMotivosDataLista()
                {
                    total = 0,
                    lista = new List<ActivosTrasladosMotivosData>()
                }
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {

                    //Busco Total
                    query = $@"select COUNT(cod_motivo) from ACTIVOS_TRASLADOS_MOTIVOS";
                    result.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( cod_motivo LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%'  ) ";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "cod_motivo";
                    }

                    query = $@"select * from ACTIVOS_TRASLADOS_MOTIVOS  
                                        {filtros.filtro} 
                                     order by {filtros.sortField}  {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                         OFFSET {filtros.pagina} ROWS 
                                         FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                    result.Result.lista = connection.Query<ActivosTrasladosMotivosData>(query).ToList();

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
        /// Metodo para consultar datos de motivos de Traslado a exportar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<List<ActivosTrasladosMotivosData>> Activos_TrasladosMotivos_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO<List<ActivosTrasladosMotivosData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosTrasladosMotivosData>()
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( cod_motivo LIKE '%" + filtros.filtro + "%' " +
                           " OR descripcion LIKE '%" + filtros.filtro + "%'  ) ";

                    }
                    query = $@"select * from ACTIVOS_TRASLADOS_MOTIVOS 
                                        {filtros.filtro} 
                                     order by cod_motivo";
                    result.Result = connection.Query<ActivosTrasladosMotivosData>(query).ToList();
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
        /// Metodo para insertar o actualizar un Motivo de Traslado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDTO Activos_TrasladosMotivos_Guardar(int CodEmpresa, string usuario, ActivosTrasladosMotivosData datos)
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

                    //verifico si existe 
                    var query = $@"select coalesce(count(*),0) as Existe from ACTIVOS_TRASLADOS_MOTIVOS  where cod_motivo = @codigo ";
                    var existe = connection.QueryFirstOrDefault<int>(query, new { codigo = datos.cod_motivo });

                    if (datos.isNew)
                    {
                        if (existe > 0)
                        {
                            result.Code = -2;
                            result.Description = $"El Motivo con el código {datos.cod_motivo} ya existe.";
                        }
                        else
                        {
                            result = Activos_TrasladosMotivos_Insertar(CodEmpresa, usuario, datos);
                        }
                    }
                    else if (existe == 0 && !datos.isNew)
                    {
                        result.Code = -2;
                        result.Description = $"El Motivo con el código {datos.cod_motivo} no existe.";
                    }
                    else
                    {
                        result = Activos_TrasladosMotivos_Actualizar(CodEmpresa, usuario, datos);
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
        /// Metodo para actualizar un Motivo de Traslado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDTO Activos_TrasladosMotivos_Actualizar(int CodEmpresa, string usuario, ActivosTrasladosMotivosData datos)
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
                    var query = $@"update ACTIVOS_TRASLADOS_MOTIVOS 
                                    SET descripcion = @descripcion,
                                        activo = @activo
                                    WHERE cod_motivo = @cod_motivo";
                    connection.Execute(query, new
                    {
                        datos.cod_motivo,
                        datos.descripcion,
                        datos.activo,
                        usuario
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Motivo de Traslado:  {datos.cod_motivo}",
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
        /// Metodo para insertar un Motivo de Traslado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDTO Activos_TrasladosMotivos_Insertar(int CodEmpresa, string usuario, ActivosTrasladosMotivosData datos)
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
                    var query = $@"insert into ACTIVOS_TRASLADOS_MOTIVOS(cod_motivo,descripcion,activo,registro_usuario,registro_fecha)
                                    VALUES (@cod_motivo, @descripcion, @activo, @usuario,getdate())";
                    connection.Execute(query, new
                    {
                        datos.cod_motivo,
                        datos.descripcion,
                        datos.activo,
                        usuario
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Motivo de Traslado:  : {datos.cod_motivo}",
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
        /// Metodo para eliminar un motivo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_motivo"></param>
        /// <returns></returns>
        public ErrorDTO Activos_TrasladosMotivos_Eliminar(int CodEmpresa, string usuario, string cod_motivo)
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
                    var query = $@"DELETE FROM ACTIVOS_TRASLADOS_MOTIVOS WHERE cod_motivo = @cod_motivo";
                    connection.Execute(query, new { cod_motivo });
                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Motivo de Traslado:  : {cod_motivo}",
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
