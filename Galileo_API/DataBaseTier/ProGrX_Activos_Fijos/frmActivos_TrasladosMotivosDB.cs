using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosTrasladosMotivosDb
    {

        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly PortalDB _portalDB;

        public FrmActivosTrasladosMotivosDb(IConfiguration config)
        {
            _Security_MainDB = new MSecurityMainDb(config);
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Método para consultar la lista de motivos de traslados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<ActivosTrasladosMotivosDataLista> Activos_TrasladosMotivos_Consultar(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<ActivosTrasladosMotivosDataLista>()
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
                using var connection = _portalDB.CreateConnection(CodEmpresa);
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
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = [];
            }
            return result;
        }

        /// <summary>
        /// Método para consultar datos de motivos de Traslado a exportar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<ActivosTrasladosMotivosData>> Activos_TrasladosMotivos_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<List<ActivosTrasladosMotivosData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosTrasladosMotivosData>()
            };

            try
            {
                var query = "";
                using var connection = _portalDB.CreateConnection(CodEmpresa);

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
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }


        /// <summary>
        /// Método para insertar o actualizar un Motivo de Traslado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto Activos_TrasladosMotivos_Guardar(int CodEmpresa, string usuario, ActivosTrasladosMotivosData datos)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
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
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// Método para actualizar un Motivo de Traslado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDto Activos_TrasladosMotivos_Actualizar(int CodEmpresa, string usuario, ActivosTrasladosMotivosData datos)
        {

            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
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

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Motivo de Traslado:  {datos.cod_motivo}",
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
        /// Método para insertar un Motivo de Traslado
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDto Activos_TrasladosMotivos_Insertar(int CodEmpresa, string usuario, ActivosTrasladosMotivosData datos)
        {

            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"insert into ACTIVOS_TRASLADOS_MOTIVOS(cod_motivo,descripcion,activo,registro_usuario,registro_fecha)
                                    VALUES (@cod_motivo, @descripcion, @activo, @usuario,getdate())";
                connection.Execute(query, new
                {
                    datos.cod_motivo,
                    datos.descripcion,
                    datos.activo,
                    usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Motivo de Traslado:  : {datos.cod_motivo}",
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
        /// Método para eliminar un motivo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_motivo"></param>
        /// <returns></returns>
        public ErrorDto Activos_TrasladosMotivos_Eliminar(int CodEmpresa, string usuario, string cod_motivo)
        {

            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"DELETE FROM ACTIVOS_TRASLADOS_MOTIVOS WHERE cod_motivo = @cod_motivo";
                connection.Execute(query, new { cod_motivo });
                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Motivo de Traslado:  : {cod_motivo}",
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
