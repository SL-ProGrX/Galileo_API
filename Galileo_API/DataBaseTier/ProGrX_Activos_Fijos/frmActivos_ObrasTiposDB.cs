using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Activos_Fijos;

namespace PgxAPI.DataBaseTier.ProGrX_Activos_Fijos
{
    public class frmActivos_ObrasTiposDB
    {

        private readonly IConfiguration? _config;
        private readonly int vModulo = 36;
        private readonly mSecurityMainDb _Security_MainDB;

        public frmActivos_ObrasTiposDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }
        /// <summary>
        /// Metodo para consultar la lista de tipos de obras en proceso
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<Activos_ObrasTipoDataLista> Activos_ObrasTipos_Consultar(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO<Activos_ObrasTipoDataLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new Activos_ObrasTipoDataLista()
                {
                    total = 0,
                    lista = new List<Activos_ObrasTipoData>()
                }
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {

                    //Busco Total
                    query = $@"select COUNT(cod_tipo) from Activos_obras_tipos";
                    result.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( cod_tipo LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%'  ) ";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "cod_tipo";
                    }

                    query = $@"select * from Activos_obras_tipos 
                                        {filtros.filtro} 
                                     order by {filtros.sortField}   {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                         OFFSET {filtros.pagina} ROWS 
                                         FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                    result.Result.lista = connection.Query<Activos_ObrasTipoData>(query).ToList();

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
        /// Metodo para consultar lista de tipos de obras en proceso a exportar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<List<Activos_ObrasTipoData>> Activos_ObrasTipos_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO<List<Activos_ObrasTipoData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<Activos_ObrasTipoData>()
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( cod_tipo LIKE '%" + filtros.filtro + "%' " +
                           " OR descripcion LIKE '%" + filtros.filtro + "%'  ) ";

                    }
                    query = $@"select * from Activos_obras_tipos 
                                        {filtros.filtro} 
                                     order by cod_tipo";
                    result.Result = connection.Query<Activos_ObrasTipoData>(query).ToList();
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
        /// Metodo para actualizar o insertar un nuevo tipo de obras en proceso
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDTO Activos_ObrasTipos_Guardar(int CodEmpresa, string usuario, Activos_ObrasTipoData datos)
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
                    var query = $@"select coalesce(count(*),0) as Existe from Activos_obras_tipos where cod_tipo = @codigo ";
                    var existe = connection.QueryFirstOrDefault<int>(query, new { codigo = datos.cod_tipo });

                    if (datos.isNew)
                    {
                        if (existe > 0)
                        {
                            result.Code = -2;
                            result.Description = $"El Tipo de obras en proceso con el código {datos.cod_tipo} ya existe.";
                        }
                        else
                        {
                            result = Activos_ObrasTipos_Insertar(CodEmpresa, usuario, datos);
                        }
                    }
                    else if (existe == 0 && !datos.isNew)
                    {
                        result.Code = -2;
                        result.Description = $"El Tipo de obras en proceso con el código {datos.cod_tipo} no existe.";
                    }
                    else
                    {
                        result = Activos_ObrasTipos_Actualizar(CodEmpresa, usuario, datos);
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
        /// Metodo para actualizar un nuevo tipo de obras en proceso
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDTO Activos_ObrasTipos_Actualizar(int CodEmpresa, string usuario, Activos_ObrasTipoData datos)
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
                    var query = $@"UPDATE  Activos_obras_tipos
                                    SET descripcion = @descripcion,
                                        activo = @activo,
                                        modifica_usuario = @usuario,
                                        modifica_fecha = getdate()
                                    WHERE cod_tipo = @cod_tipo";
                    connection.Execute(query, new
                    {
                        datos.cod_tipo,
                        datos.descripcion,
                        datos.activo,
                        usuario
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Tipo de Obra en Proceso : {datos.cod_tipo}",
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
        /// Metodo para insertar un nuevo tipo de obras en proceso
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDTO Activos_ObrasTipos_Insertar(int CodEmpresa, string usuario, Activos_ObrasTipoData datos)
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
                    var query = $@"insert into Activos_obras_tipos(cod_tipo,descripcion,activo,registro_usuario,registro_fecha)
                                    VALUES (@cod_tipo, @descripcion, @activo, @usuario,getdate())";
                    connection.Execute(query, new
                    {
                        datos.cod_tipo,
                        datos.descripcion,
                        datos.activo,
                        usuario
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Tipo de Obra en Proceso : {datos.cod_tipo}",
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
        /// Metodo para eliminar un tipo de obras en proceso
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_tipo"></param>
        /// <returns></returns>
        public ErrorDTO Activos_ObrasTipos_Eliminar(int CodEmpresa, string usuario, string cod_tipo)
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
                    var query = $@"DELETE FROM Activos_obras_tipos WHERE cod_tipo = @cod_tipo";
                    connection.Execute(query, new { cod_tipo });
                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Tipo de Obra en Proceso :  {cod_tipo}",
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
