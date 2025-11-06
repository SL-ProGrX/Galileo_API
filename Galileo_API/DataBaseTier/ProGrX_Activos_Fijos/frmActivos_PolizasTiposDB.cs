using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Activos_Fijos;
using PgxAPI.Models.Security;
namespace PgxAPI.DataBaseTier.ProGrX_Activos_Fijos
{
    public class frmActivos_PolizasTiposDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 36; // Modulo de Activos Fijos
        private readonly MSecurityMainDb _Security_MainDB;

        public frmActivos_PolizasTiposDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene una lista de tipos de póliza de activos fijos con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<ActivosPolizasTiposLista> Activos_PolizasTiposLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<ActivosPolizasTiposLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivosPolizasTiposLista()
                {
                    total = 0,
                    lista = new List<ActivosPolizasTiposData>()
                }
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {

                    //Busco Total
                    query = $@"select COUNT(tipo_poliza) from activos_polizas_tipos";
                    result.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( tipo_poliza LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%') ";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "tipo_poliza";
                    }

                    query = $@"select tipo_poliza,descripcion,CASE WHEN ISNULL(ACTIVO, 0) = 0 THEN 0 ELSE 1 END AS ACTIVO from activos_polizas_tipos
                                        {filtros.filtro} 
                                    ORDER BY {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                         OFFSET {filtros.pagina} ROWS 
                                         FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                    result.Result.lista = connection.Query<ActivosPolizasTiposData>(query).ToList();
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
        /// Obtiene una lista de tipos de pólizas de activos fijos sin paginación, con filtros aplicados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<ActivosPolizasTiposData>> Activos_PolizasTipos_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<ActivosPolizasTiposData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosPolizasTiposData>()
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( tipo_poliza LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%') ";
                    }
                    query = $@"select tipo_poliza,descripcion,CASE WHEN ISNULL(ACTIVO, 0) = 0 THEN 0 ELSE 1 END AS ACTIVO
                        FROM activos_polizas_tipos {filtros.filtro} 
                                     order by tipo_poliza";
                    result.Result = connection.Query<ActivosPolizasTiposData>(query).ToList();
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
        /// Inserta o actualiza un tipo de póliza de activos fijos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipoPoliza"></param>
        /// <returns></returns>
        public ErrorDto Activos_PolizasTipos_Guardar(int CodEmpresa, string usuario, ActivosPolizasTiposData tipoPoliza)
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
                    // Verifico si existe usuario (activo)
                    var qUsuario = @"SELECT COUNT(Nombre) 
                             FROM usuarios 
                             WHERE estado = 'A' AND UPPER(Nombre) LIKE '%' + @usr + '%'";
                    int existeuser = connection.QueryFirstOrDefault<int>(qUsuario, new { usr = usuario.ToUpper() });
                    if (existeuser == 0)
                    {
                        result.Code = -2;
                        result.Description = $"El usuario {usuario.ToUpper()} no existe o no está activo.";
                        return result;
                    }

                    //verifico si existe tipoPoliza
                    var query = $@"select isnull(count(*),0) as Existe from activos_polizas_tipos  where UPPER(tipo_poliza) = @tipoPoliza ";
                    var existe = connection.QueryFirstOrDefault<int>(query, new { tipoPoliza = tipoPoliza.tipo_poliza.ToUpper() });

                    if (tipoPoliza.isNew)
                    {
                        if (existe > 0)
                        {
                            result.Code = -2;
                            result.Description = $"El tipo de póliza con el código {tipoPoliza.tipo_poliza} ya existe.";
                        }
                        else
                        {
                            result = Activos_PolizasTipos_Insertar(CodEmpresa, usuario, tipoPoliza);
                        }
                    }
                    else if (existe == 0 && !tipoPoliza.isNew)
                    {
                        result.Code = -2;
                        result.Description = $"El tipo de póliza con el código {tipoPoliza.tipo_poliza} no existe.";
                    }
                    else
                    {
                        result = Activos_PolizasTipos_Actualizar(CodEmpresa, usuario, tipoPoliza);
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
        /// Actualiza un tipo de póliza existente.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipoPoliza"></param>
        /// <returns></returns>
        private ErrorDto Activos_PolizasTipos_Actualizar(int CodEmpresa, string usuario, ActivosPolizasTiposData tipoPoliza)
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
                    var query = $@"UPDATE activos_polizas_tipos
                                    SET descripcion = @descripcion,
                                        activo = @activo,
                                       modifica_usuario = @modifica_usuario,
                                       modifica_fecha   = SYSDATETIME()
                                    WHERE tipo_poliza = @tipo_poliza";
                    connection.Execute(query, new
                    {
                        tipo_poliza = tipoPoliza.tipo_poliza.ToUpper(),
                        descripcion = tipoPoliza.descripcion?.ToUpper(),
                        activo = tipoPoliza.activo,
                        modifica_usuario = usuario
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Tipos de Pólizas Doc. : {tipoPoliza.tipo_poliza} - {tipoPoliza.descripcion}",
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
        /// Inserta un tipo de póliza.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipoPoliza"></param>
        /// <returns></returns>
        private ErrorDto Activos_PolizasTipos_Insertar(int CodEmpresa, string usuario, ActivosPolizasTiposData tipoPoliza)
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
                    var query = $@"INSERT INTO activos_polizas_tipos (tipo_poliza, descripcion, activo, REGISTRO_USUARIO, REGISTRO_FECHA, MODIFICA_USUARIO, MODIFICA_FECHA)
                                    VALUES (@tipo_poliza, @descripcion, @activo,@registro_usuario, SYSDATETIME(), NULL, NULL)";
                    connection.Execute(query, new
                    {
                        tipo_poliza = tipoPoliza.tipo_poliza.ToUpper(),
                        descripcion = tipoPoliza.descripcion?.ToUpper(),
                        activo = tipoPoliza.activo,
                        registro_usuario = usuario
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Tipos de Pólizas Doc.. : {tipoPoliza.tipo_poliza} - {tipoPoliza.descripcion}",
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
        /// Elimina un tipo de póliza de activos fijos por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipo_poliza"></param>
        /// <returns></returns>
        public ErrorDto Activos_PolizasTipos_Eliminar(int CodEmpresa, string usuario, string tipo_poliza)
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
                    var query = $@"DELETE FROM activos_polizas_tipos WHERE tipo_poliza = @tipo_poliza";
                    connection.Execute(query, new { tipo_poliza = tipo_poliza.ToUpper() });
                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Tipos de Pólizas Doc. : {tipo_poliza}",
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
        /// Valida si un código de tipo de póliza ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="tipo_poliza"></param>
        /// <returns></returns>
        public ErrorDto Activos_PolizasTipos_Valida(int CodEmpresa, string tipo_poliza)
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
                    var query = $@"SELECT count(tipo_poliza) FROM activos_polizas_tipos WHERE UPPER(tipo_poliza) = @tipo_poliza";
                    var existe = connection.QueryFirstOrDefault<int>(query, new { tipo_poliza = tipo_poliza.ToUpper() });

                    if (existe > 0)
                    {
                        result.Code = -1;
                        result.Description = "El código de tipo de póliza ya existe.";
                    }
                    else
                    {
                        result.Code = 0;
                        result.Description = "El código de tipo de póliza es válido.";

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
