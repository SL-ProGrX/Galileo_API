using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Personas;
using PgxAPI.Models.Security;

namespace PgxAPI.DataBaseTier.ProGrX_Personas
{
    public class frmAF_ZonasDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 1;
        private readonly MSecurityMainDb _Security_MainDB;

        public frmAF_ZonasDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene una lista de zonas con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<ZonasLista> AF_ZonasLista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<ZonasLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new ZonasLista
                {
                    Total = 0,
                    Lista = new List<ZonasData>()
                }
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    query = @"SELECT COUNT(cod_zona) FROM afi_zonas";
                    result.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (!string.IsNullOrEmpty(filtros.filtro))
                    {
                        filtros.filtro = " WHERE ( cod_zona LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (string.IsNullOrEmpty(filtros.sortField))
                        filtros.sortField = "cod_zona";

                    query = $@"SELECT cod_zona, descripcion, activa, registro_usuario, registro_fecha 
                               FROM afi_zonas
                               {filtros.filtro}
                               ORDER BY {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")}
                               OFFSET {filtros.pagina} ROWS 
                               FETCH NEXT {filtros.paginacion} ROWS ONLY";
                    result.Result.Lista = connection.Query<ZonasData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.Total = 0;
                result.Result.Lista = null;
            }
            return result;
        }

        /// <summary>
        /// Obtiene una lista de zonas sin paginación, con filtros aplicados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<ZonasData>> AF_Zonas_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<ZonasData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ZonasData>()
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    if (!string.IsNullOrEmpty(filtros.filtro))
                    {
                        filtros.filtro = " WHERE ( cod_zona LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' ) ";
                    }
                    query = $@"SELECT cod_zona, descripcion, activa, registro_usuario, registro_fecha 
                               FROM afi_zonas
                               {filtros.filtro}
                               ORDER BY cod_zona";
                    result.Result = connection.Query<ZonasData>(query).ToList();
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
        /// Inserta o actualiza una zona.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="zona"></param>
        /// <returns></returns>
        public ErrorDto AF_Zonas_Guardar(int CodEmpresa, string usuario, ZonasData zona)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    // Verifico si existe zona
                    var query = @"SELECT COALESCE(COUNT(*), 0) FROM afi_zonas WHERE cod_zona = @cod_zona";
                    int existe = connection.QueryFirstOrDefault<int>(query, new { cod_zona = zona.Cod_Zona });

                    if (zona.isNew)
                    {
                        if (existe > 0)
                        {
                            result.Code = -2;
                            result.Description = $"La Zona con el código {zona.Cod_Zona} ya existe.";
                        }
                        else
                        {
                            result = AF_Zonas_Insertar(CodEmpresa, usuario, zona);
                        }
                    }
                    else if (existe == 0 && !zona.isNew)
                    {
                        result.Code = -2;
                        result.Description = $"La Zona con el código {zona.Cod_Zona} no existe.";
                    }
                    else
                    {
                        result = AF_Zonas_Actualizar(CodEmpresa, usuario, zona);
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
        /// Actualiza una zona existente.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="zona"></param>
        /// <returns></returns>
        private ErrorDto AF_Zonas_Actualizar(int CodEmpresa, string usuario, ZonasData zona)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"UPDATE afi_zonas
                                  SET descripcion = @descripcion,
                                      activa = @activa
                                  WHERE cod_zona = @cod_zona";
                    connection.Execute(query, new
                    {
                        cod_zona = zona.Cod_Zona,
                        descripcion = zona.Descripcion,
                        activa = zona.Activa ? 1 : 0
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Zona Doc.: {zona.Cod_Zona} - {zona.Descripcion}",
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
        /// Inserta una nueva zona.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="zona"></param>
        /// <returns></returns>
        private ErrorDto AF_Zonas_Insertar(int CodEmpresa, string usuario, ZonasData zona)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"INSERT INTO afi_zonas (cod_zona, descripcion, activa, registro_fecha, registro_usuario)
                                  VALUES (@cod_zona, @descripcion, @activa, dbo.mygetdate(), @registro_usuario)";
                    connection.Execute(query, new
                    {
                        cod_zona = zona.Cod_Zona,
                        descripcion = zona.Descripcion,
                        activa = zona.Activa ? 1 : 0,
                        registro_usuario = usuario
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Zona Doc.: {zona.Cod_Zona} - {zona.Descripcion}",
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
        /// Elimina una zona por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="codZona"></param>
        /// <returns></returns>
        public ErrorDto AF_Zonas_Eliminar(int CodEmpresa, string usuario, string codZona)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"DELETE FROM afi_zonas WHERE cod_zona = @cod_zona";
                    connection.Execute(query, new { cod_zona = codZona });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Zona Doc.: {codZona}",
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
        /// Valida si un código de zona ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codZona"></param>
        /// <returns></returns>
        public ErrorDto AF_Zonas_Valida(int CodEmpresa, string codZona)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"SELECT COALESCE(COUNT(*), 0) AS Existe FROM afi_zonas WHERE cod_zona = @cod_zona";
                    var existe = connection.QueryFirstOrDefault<int>(query, new { cod_zona = codZona });

                    if (existe > 0)
                    {
                        result.Code = -1;
                        result.Description = "El código de zona ya existe.";
                    }
                    else
                    {
                        result.Code = 0;
                        result.Description = "El código de zona es válido.";
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
        /// Obtiene la lista de usuarios asignados a una zona.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="zonaId"></param>
        /// <returns></returns>
        public ErrorDto<List<ZonaUsuarioAsignadoData>> AF_Zonas_UsuariosAsignados_Obtener(int CodEmpresa, string zonaId)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<ZonaUsuarioAsignadoData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ZonaUsuarioAsignadoData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "spAfi_Zonas_Usuario_Asigna_Consulta";
                    result.Result = connection.Query<ZonaUsuarioAsignadoData>(
                        query,
                        new { Zona = zonaId },
                        commandType: System.Data.CommandType.StoredProcedure
                    ).ToList();
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
        /// Obtiene la lista de instituciones asignadas a una zona.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="zonaId"></param>
        /// <returns></returns>
        public ErrorDto<List<ZonaInstitucionAsignadaData>> AF_Zonas_InstitucionesAsignadas_Obtener(int CodEmpresa, string zonaId)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<ZonaInstitucionAsignadaData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ZonaInstitucionAsignadaData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "spAfi_Zonas_Inst_Asigna_Consulta";
                    result.Result = connection.Query<ZonaInstitucionAsignadaData>(
                        query,
                        new { Zona = zonaId },
                        commandType: System.Data.CommandType.StoredProcedure
                    ).ToList();
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
        /// Registra o elimina la asignación de una institución a una zona.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="zonaId"></param>
        /// <param name="institucionId"></param>
        /// <param name="usuario"></param>
        /// <param name="movimiento"></param>
        /// <returns></returns>
        public ErrorDto AF_Zonas_InstitucionAsignar_Registrar(int CodEmpresa, string zonaId, int institucionId, string usuario, string movimiento)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "spAfi_Zonas_Inst_Asigna_Registra";
                    connection.Execute(
                        query,
                        new { Zona = zonaId, Codigo = institucionId, Usuario = usuario, Movimiento = movimiento },
                        commandType: System.Data.CommandType.StoredProcedure
                    );
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
        /// Registra o elimina la asignación de un usuario a una zona.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="zonaId"></param>
        /// <param name="usuarioId"></param>
        /// <param name="usuario"></param>
        /// <param name="movimiento"></param>
        /// <returns></returns>
        public ErrorDto AF_Zonas_UsuarioAsignar_Registrar(int CodEmpresa, string zonaId, string usuarioId, string usuario, string movimiento)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "spAfi_Zonas_Usuario_Asigna_Registra";
                    connection.Execute(
                        query,
                        new { Zona = zonaId, Codigo = usuarioId, Usuario = usuario, Movimiento = movimiento },
                        commandType: System.Data.CommandType.StoredProcedure
                    );
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
