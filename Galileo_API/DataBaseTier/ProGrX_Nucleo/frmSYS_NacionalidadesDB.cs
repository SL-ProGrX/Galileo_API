using Dapper;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.SYS;
using Microsoft.Data.SqlClient;

namespace PgxAPI.DataBaseTier.ProGrX_Nucleo
{
    public class frmSYS_NacionalidadesDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 10; // Modulo de Tesorería
        private readonly MSecurityMainDb _Security_MainDB;

        public frmSYS_NacionalidadesDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene una lista de nacionalidades con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<SysNacionalidadesLista> Sys_NacionalidadesLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<SysNacionalidadesLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new SysNacionalidadesLista()
                {
                    total = 0,
                    lista = new List<SysNacionalidadesData>()
                }
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    // Busco Total
                    query = $@"select COUNT(COD_NACIONALIDAD) from SYS_NACIONALIDADES";
                    result.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( COD_NACIONALIDAD LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR cod_inter LIKE '%" + filtros.filtro + "%' " +
                            " OR Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (string.IsNullOrEmpty(filtros.sortField))
                    {
                        filtros.sortField = "COD_NACIONALIDAD";
                    }

                    query = $@"select COD_NACIONALIDAD, descripcion, cod_inter, omision, activo, Registro_Fecha, Registro_Usuario
                                from SYS_NACIONALIDADES
                                {filtros.filtro}
                                order by {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                                OFFSET {filtros.pagina} ROWS 
                                FETCH NEXT {filtros.paginacion} ROWS ONLY ";

                    result.Result.lista = connection.Query<SysNacionalidadesData>(query).ToList();
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
        /// Obtiene una lista de nacionalidades sin paginación, con filtros aplicados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<SysNacionalidadesData>> Sys_Nacionalidades_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SysNacionalidadesData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SysNacionalidadesData>()
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " WHERE ( COD_NACIONALIDAD LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                    }
                    query = $@"SELECT COD_NACIONALIDAD, descripcion, cod_inter, omision, activo, Registro_Fecha, Registro_Usuario
                                FROM SYS_NACIONALIDADES
                                {filtros.filtro}
                                ORDER BY COD_NACIONALIDAD";
                    result.Result = connection.Query<SysNacionalidadesData>(query).ToList();
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
        /// Inserta o actualiza una nacionalidad.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="nacionalidad"></param>
        /// <returns></returns>
        public ErrorDto Sys_Nacionalidades_Guardar(int CodEmpresa, string usuario, SysNacionalidadesData nacionalidad)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                // Reutiliza la función de validación
                var valida = Sys_Nacionalidades_Valida(CodEmpresa, nacionalidad);

                if (nacionalidad.isNew)
                {
                    if (valida.Code == -1)
                    {
                        result.Code = -2;
                        result.Description = valida.Description;
                    }
                    else
                    {
                        result = Sys_Nacionalidades_Insertar(CodEmpresa, usuario, nacionalidad);
                    }
                }
                else
                {
                    // Para actualizar, solo valida que exista por código
                    string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                    using var connection = new SqlConnection(stringConn);
                    var query = @"SELECT COUNT(*) FROM SYS_NACIONALIDADES WHERE UPPER(COD_NACIONALIDAD) = @cod_nacionalidad";
                    var existe = connection.QueryFirstOrDefault<int>(query, new { cod_nacionalidad = nacionalidad.cod_nacionalidad.ToUpper() });

                    if (existe == 0)
                    {
                        result.Code = -2;
                        result.Description = $"La nacionalidad con el código {nacionalidad.cod_nacionalidad} no existe.";
                    }
                    else
                    {
                        result = Sys_Nacionalidades_Actualizar(CodEmpresa, usuario, nacionalidad);
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
        /// Inserta una nueva nacionalidad.
        /// </summary>
        private ErrorDto Sys_Nacionalidades_Insertar(int CodEmpresa, string usuario, SysNacionalidadesData nacionalidad)
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
                var query = @"INSERT INTO SYS_NACIONALIDADES 
                    (COD_NACIONALIDAD, descripcion, cod_inter, omision, activo, Registro_Fecha, Registro_Usuario)
                    VALUES (@cod_nacionalidad, @descripcion, @cod_inter, @omision, @activo, GETDATE(), @registro_usuario)";
                connection.Execute(query, new
                {
                    cod_nacionalidad = nacionalidad.cod_nacionalidad.ToUpper(),
                    nacionalidad.descripcion,
                    nacionalidad.cod_inter,
                    nacionalidad.omision,
                    nacionalidad.activo,
                    registro_usuario = usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Nacionalidad: {nacionalidad.cod_nacionalidad} - {nacionalidad.descripcion}",
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
        /// Actualiza una nacionalidad existente.
        /// </summary>
        private ErrorDto Sys_Nacionalidades_Actualizar(int CodEmpresa, string usuario, SysNacionalidadesData nacionalidad)
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
                var query = @"UPDATE SYS_NACIONALIDADES
                    SET descripcion = @descripcion,
                        cod_inter = @cod_inter,
                        omision = @omision,
                        activo = @activo,
                        Registro_Usuario = @registro_usuario,
                        Registro_Fecha = GETDATE()
                    WHERE COD_NACIONALIDAD = @cod_nacionalidad";
                connection.Execute(query, new
                {
                    cod_nacionalidad = nacionalidad.cod_nacionalidad.ToUpper(),
                    nacionalidad.descripcion,
                    nacionalidad.cod_inter,
                    nacionalidad.omision,
                    nacionalidad.activo,
                    registro_usuario = usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Nacionalidad: {nacionalidad.cod_nacionalidad} - {nacionalidad.descripcion}",
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
        /// Elimina una nacionalidad por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_nacionalidad"></param>
        /// <returns></returns>
        public ErrorDto Sys_Nacionalidades_Eliminar(int CodEmpresa, string usuario, string cod_nacionalidad)
        {
            var result = new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };

            // Usamos la función de validación para verificar existencia
            var nacionalidad = new SysNacionalidadesData
            {
                cod_nacionalidad = cod_nacionalidad,
                descripcion = string.Empty // Solo interesa el código para eliminar
            };
            var valida = Sys_Nacionalidades_Valida(CodEmpresa, nacionalidad);

            // Si la validación indica que no existe, devolvemos error
            if (valida.Code == 0)
            {
                result.Code = -2;
                result.Description = $"La nacionalidad con el código {cod_nacionalidad} no existe.";
                return result;
            }

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"DELETE FROM SYS_NACIONALIDADES WHERE UPPER(COD_NACIONALIDAD) = @cod_nacionalidad";
                connection.Execute(query, new { cod_nacionalidad = cod_nacionalidad.ToUpper() });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Nacionalidad eliminada: {cod_nacionalidad}",
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

        /// <summary>
        /// Valida si un código o descripción de nacionalidad ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="nacionalidad"></param>
        /// <returns></returns>
        public ErrorDto Sys_Nacionalidades_Valida(int CodEmpresa, SysNacionalidadesData nacionalidad)
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
                    var query = @"SELECT COUNT(*) FROM SYS_NACIONALIDADES 
                                  WHERE UPPER(COD_NACIONALIDAD) = @cod_nacionalidad
                                     OR UPPER(descripcion) = @descripcion";
                    var existe = connection.QueryFirstOrDefault<int>(query, new
                    {
                        cod_nacionalidad = nacionalidad.cod_nacionalidad.ToUpper(),
                        descripcion = nacionalidad.descripcion.ToUpper()
                    });

                    if (existe > 0)
                    {
                        result.Code = -1;
                        result.Description = "Ya existe una nacionalidad con ese código o descripción.";
                    }
                    else
                    {
                        result.Code = 0;
                        result.Description = "El código y la descripción de nacionalidad son válidos.";
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
