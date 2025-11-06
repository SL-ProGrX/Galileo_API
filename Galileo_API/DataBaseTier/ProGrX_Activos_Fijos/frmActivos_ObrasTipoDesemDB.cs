using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Activos_Fijos;
using PgxAPI.Models.Security;

namespace PgxAPI.DataBaseTier.ProGrX_Activos_Fijos
{
    public class frmActivos_ObrasTipoDesemDB
    {

        private readonly IConfiguration? _config;
        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _Security_MainDB;

        public frmActivos_ObrasTipoDesemDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
        }
        /// <summary>
        /// Metodo para consultar la lista de tipos de desembolsos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<ActivosObrasTipoDesemDataLista> Activos_ObrasTipoDesem_Consultar(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<ActivosObrasTipoDesemDataLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivosObrasTipoDesemDataLista()
                {
                    total = 0,
                    lista = new List<ActivosObrasTipoDesemData>()
                }
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {

                    //Busco Total
                    query = $@"select COUNT(cod_desembolso) from Activos_obras_tdesem";
                    result.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( cod_desembolso LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%'  ) ";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "cod_desembolso";
                    }

                    query = $@"select * from Activos_obras_tdesem  
                                        {filtros.filtro} 
                                     order by {filtros.sortField}   {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                         OFFSET {filtros.pagina} ROWS 
                                         FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                    result.Result.lista = connection.Query<ActivosObrasTipoDesemData>(query).ToList();

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
        /// Metodo para consultar lista de tipos de desembolsos a exportar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<ActivosObrasTipoDesemData>> Activos_ObrasTipoDesem_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<ActivosObrasTipoDesemData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosObrasTipoDesemData>()
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( cod_desembolso LIKE '%" + filtros.filtro + "%' " +
                           " OR descripcion LIKE '%" + filtros.filtro + "%'  ) ";

                    }
                    query = $@"select * from Activos_obras_tdesem 
                                        {filtros.filtro} 
                                     order by cod_desembolso";
                    result.Result = connection.Query<ActivosObrasTipoDesemData>(query).ToList();
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
        /// Metodo para actualizar o insertar un nuevo tipo de desembolso
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto Activos_ObrasTipoDesem_Guardar(int CodEmpresa, string usuario, ActivosObrasTipoDesemData datos)
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

                    //verifico si existe 
                    var query = $@"select coalesce(count(*),0) as Existe from Activos_obras_tdesem where cod_desembolso = @codigo ";
                    var existe = connection.QueryFirstOrDefault<int>(query, new { codigo = datos.cod_desembolso });

                    if (datos.isNew)
                    {
                        if (existe > 0)
                        {
                            result.Code = -2;
                            result.Description = $"El Tipo de Desembolso con el código {datos.cod_desembolso} ya existe.";
                        }
                        else
                        {
                            result = Activos_ObrasTipoDesem_Insertar(CodEmpresa, usuario, datos);
                        }
                    }
                    else if (existe == 0 && !datos.isNew)
                    {
                        result.Code = -2;
                        result.Description = $"El Tipo de Desembolso con el código {datos.cod_desembolso} no existe.";
                    }
                    else
                    {
                        result = Activos_ObrasTipoDesem_Actualizar(CodEmpresa, usuario, datos);
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
        /// Metodo para actualizar un nuevo tipo de desembolso
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDto Activos_ObrasTipoDesem_Actualizar(int CodEmpresa, string usuario, ActivosObrasTipoDesemData datos)
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
                    var query = $@"UPDATE  Activos_obras_tdesem
                                    SET descripcion = @descripcion,
                                        activo = @activo,
                                        modifica_usuario = @usuario,
                                        modifica_fecha = getdate()
                                    WHERE cod_desembolso = @cod_desembolso";
                    connection.Execute(query, new
                    {
                        datos.cod_desembolso,
                        datos.descripcion,
                        datos.activo,
                        usuario
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Tipo de Desem. para Obra en Proceso : {datos.cod_desembolso}",
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
        /// Metodo para insertar un nuevo tipo de desembolso
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDto Activos_ObrasTipoDesem_Insertar(int CodEmpresa, string usuario, ActivosObrasTipoDesemData datos)
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
                    var query = $@"INSERT INTO Activos_obras_tdesem(cod_desembolso,descripcion,activo,registro_usuario,registro_fecha)
                                    VALUES (@cod_desembolso, @descripcion, @activo, @usuario,getdate())";
                    connection.Execute(query, new
                    {
                        datos.cod_desembolso,
                        datos.descripcion,
                        datos.activo,
                        usuario
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Tipo de Desem. para Obra en Proceso : {datos.cod_desembolso}",
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
        /// Metodo para eliminar un tipo de desembolso
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_desembolso"></param>
        /// <returns></returns>
        public ErrorDto Activos_ObrasTipoDesem_Eliminar(int CodEmpresa, string usuario, string cod_desembolso)
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
                    var query = $@"DELETE FROM Activos_obras_tdesem WHERE cod_desembolso = @cod_desembolso";
                    connection.Execute(query, new { cod_desembolso });
                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Tipo de Desem. para Obra en Proceso : {cod_desembolso}",
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
