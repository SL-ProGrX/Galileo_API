using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.BusinessLogic;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Bancos;

namespace PgxAPI.DataBaseTier.ProGrX.Bancos
{
    public class frmTES_UbicacionesDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 9; // Modulo de Tesorería
        private readonly mSecurityMainDb _Security_MainDB;

        public frmTES_UbicacionesDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene una lista de ubicaciones de tesorería con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<TesUbicacionesLista> Tes_UbicacionesLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO<TesUbicacionesLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new TesUbicacionesLista()
                {
                    total = 0,
                    lista = new List<TesUbicacionesData>()
                }
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {

                    //Busco Total
                    query = $@"select COUNT(cod_ubicacion) from tes_ubicaciones";
                    result.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( cod_ubicacion LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR USUARIO LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "cod_ubicacion";
                    }

                    query = $@"select cod_ubicacion,descripcion,Case when estado = 'I' then 0 Else 1 end activo, USUARIO from tes_ubicaciones
                                        {filtros.filtro} 
                                     order by {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                         OFFSET {filtros.pagina} ROWS 
                                         FETCH NEXT { filtros.paginacion } ROWS ONLY ";
                    result.Result.lista = connection.Query<TesUbicacionesData>(query).ToList();
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
        /// Obtiene una lista de ubicaciones de tesorería sin paginación, con filtros aplicados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<List<TesUbicacionesData>> Tes_Ubicaciones_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO<List<TesUbicacionesData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<TesUbicacionesData>()
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( cod_ubicacion LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR USUARIO LIKE '%" + filtros.filtro + "%' ) ";
                    }
                    query = $@"select cod_ubicacion,descripcion,Case when estado = 'I' then 0 Else 1 end activo, USUARIO from tes_ubicaciones
                                        {filtros.filtro} 
                                     order by cod_ubicacion";
                    result.Result = connection.Query<TesUbicacionesData>(query).ToList();
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
        /// Inserta o actualiza una ubicación de tesorería.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="ubicacion"></param>
        /// <returns></returns>
        public ErrorDTO Tes_Ubicaciones_Guardar(int CodEmpresa, string usuario, TesUbicacionesData ubicacion)
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
                    var qUsuario = $@"select count(Nombre) from usuarios where estado = 'A' and UPPER(Nombre) like '%{ubicacion.usuario.ToUpper()}%' ";
                    int existeuser = connection.QueryFirstOrDefault<int>(qUsuario);
                    if (existeuser == 0)
                    {
                        result.Code = -2;
                        result.Description = $"El usuario {ubicacion.usuario.ToUpper()} no existe o no está activo.";
                        return result;
                    }

                    //verifico si existe ubicacion
                    var query = $@"select isnull(count(*),0) as Existe from tes_ubicaciones  where UPPER(cod_ubicacion) = @ubicacion ";
                    var existe = connection.QueryFirstOrDefault<int>(query, new { ubicacion = ubicacion.cod_ubicacion.ToUpper() });

                    if(ubicacion.isNew)
                    {
                        if(existe > 0)
                        {
                            result.Code = -2;
                            result.Description = $"La Ubicación con el código { ubicacion.cod_ubicacion } ya existe.";
                        }
                        else
                        {
                            result = Tes_Ubicaciones_Insertar(CodEmpresa, usuario, ubicacion);
                        }
                    }
                    else if(existe == 0 && !ubicacion.isNew)
                    {
                        result.Code = -2;
                        result.Description = $"La Ubicación con el código {ubicacion.cod_ubicacion} no existe.";
                    }
                    else
                    {
                        result = Tes_Ubicaciones_Actualizar(CodEmpresa, usuario, ubicacion);
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
        /// Actualiza una ubicación de tesorería existente.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="ubicacion"></param>
        /// <returns></returns>
        private ErrorDTO Tes_Ubicaciones_Actualizar(int CodEmpresa, string usuario, TesUbicacionesData ubicacion)
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
                    var query = $@"UPDATE tes_ubicaciones
                                    SET descripcion = @descripcion,
                                        estado = @estado,
                                        usuario = @usuario
                                    WHERE cod_ubicacion = @cod_ubicacion";
                    connection.Execute(query, new
                    {
                        cod_ubicacion = ubicacion.cod_ubicacion.ToUpper(),
                        descripcion = ubicacion.descripcion?.ToUpper(),
                        estado = ubicacion.activo ? "A" : "I",
                        usuario = ubicacion.usuario
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Ubicacion Doc. : {ubicacion.cod_ubicacion} - {ubicacion.descripcion}",
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
        /// Inserta una nueva ubicación de tesorería.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="ubicacion"></param>
        /// <returns></returns>
        private ErrorDTO Tes_Ubicaciones_Insertar(int CodEmpresa, string usuario ,TesUbicacionesData ubicacion)
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
                    var query = $@"INSERT INTO tes_ubicaciones (cod_ubicacion, descripcion, estado, usuario)
                                    VALUES (@cod_ubicacion, @descripcion, @estado, @usuario)";
                    connection.Execute(query, new
                    {
                        cod_ubicacion = ubicacion.cod_ubicacion.ToUpper(),
                        descripcion = ubicacion.descripcion?.ToUpper(),
                        estado = ubicacion.activo ? "A" : "I",
                        usuario = ubicacion.usuario
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Ubicacion Doc. : {ubicacion.cod_ubicacion} - {ubicacion.descripcion}",
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
        /// Elimina una ubicación de tesorería por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="codUbicacion"></param>
        /// <returns></returns>
        public ErrorDTO Tes_Ubicaciones_Eliminar(int CodEmpresa, string usuario, string codUbicacion)
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
                    var query = $@"DELETE FROM tes_ubicaciones WHERE cod_ubicacion = @cod_ubicacion";
                    connection.Execute(query, new { cod_ubicacion = codUbicacion.ToUpper() });
                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Ubicacion Doc. : {codUbicacion}",
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
        /// Obtiene una lista de usuarios activos para ubicaciones de tesorería.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDTO<List<DropDownListaGenericaModel>> Tes_UbicacionesUsuarios_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDTO<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Nombre as 'item',descripcion from usuarios where estado = 'A' ";
                    result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// Valida si un código de ubicación ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_ubicacion"></param>
        /// <returns></returns>
        public ErrorDTO Tes_Ubicaciones_Valida(int CodEmpresa, string cod_ubicacion)
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
                    var query = $@"SELECT count(cod_ubicacion) FROM tes_ubicaciones WHERE UPPER(cod_ubicacion) = @cod_ubicacion";
                    var existe = connection.QueryFirstOrDefault<int>(query, new { cod_ubicacion = cod_ubicacion.ToUpper() });

                    if (existe > 0)
                    {
                        result.Code = -1;
                        result.Description = "El código de ubicación ya existe.";
                    }
                    else
                    {
                        result.Code = 0;
                        result.Description = "El código de ubicación es válido.";

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
