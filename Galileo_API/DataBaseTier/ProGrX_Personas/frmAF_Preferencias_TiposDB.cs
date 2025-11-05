using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Personas;

using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Personas;

    namespace PgxAPI.DataBaseTier.ProGrX_Personas
{
    public class frmAF_Preferencias_TiposDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 1; // Modulo de Tesorería
        private readonly mSecurityMainDb _Security_MainDB;

        public frmAF_Preferencias_TiposDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de preferencias con filtros y paginación.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de búsqueda, orden y paginación</param>
        /// <returns></returns>
        public ErrorDTO<PreferenciaTipoLista> AF_Preferencias_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDTO<PreferenciaTipoLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new PreferenciaTipoLista()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                // Total
                var queryTotal = @"select COUNT(COD_PREFERENCIA) from AFI_PREFERENCIAS";
                result.Result.Total = connection.Query<int>(queryTotal).FirstOrDefault();

                // Filtros
                string where = "";
                if (!string.IsNullOrEmpty(filtros.filtro))
                {
                    where = " WHERE ( COD_PREFERENCIA LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                }

                // Orden
                string sortField = string.IsNullOrEmpty(filtros.sortField) ? "COD_PREFERENCIA" : filtros.sortField;
                string sortOrder = filtros.sortOrder == 0 ? "DESC" : "ASC";

                // Paginación
                int pagina = filtros.pagina ?? 0;
                int paginacion = filtros.paginacion ?? 10;

                var query = $@"select COD_PREFERENCIA, descripcion, ACTIVA, Registro_Fecha, Registro_Usuario
                               from AFI_PREFERENCIAS
                               {where}
                               order by {sortField} {sortOrder}
                               OFFSET {pagina} ROWS 
                               FETCH NEXT {paginacion} ROWS ONLY";

                result.Result.Lista = connection.Query<PreferenciaTipoData>(query).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.Lista = null;
            }
            return result;
        }

        /// <summary>
        /// Inserta o actualiza una preferencia.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="preferenciaTipo">Datos de la preferencia</param>
        /// <returns></returns>
        public ErrorDTO AF_Preferencias_Guardar(int CodEmpresa, string usuario, PreferenciaTipoData preferenciaTipo)
        {
            var result = new ErrorDTO { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                // Verifico si existe la preferencia
                var queryExiste = @"SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_PREFERENCIAS WHERE COD_PREFERENCIA = @COD_PREFERENCIA";
                var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { COD_PREFERENCIA = preferenciaTipo.Cod_Preferencia.ToUpper() });

                if (existe == 0)
                {
                    // Insertar
                    result = AF_Preferencias_Insertar(CodEmpresa, usuario, preferenciaTipo);
                }
                else
                {
                    // Actualizar
                    result = AF_Preferencias_Actualizar(CodEmpresa, usuario, preferenciaTipo);
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
        /// Inserta una nueva preferencia.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="preferenciaTipo">Datos de la preferencia a insertar</param>
        /// <returns></returns>
        private ErrorDTO AF_Preferencias_Insertar(int CodEmpresa, string usuario, PreferenciaTipoData preferenciaTipo)
        {
            var result = new ErrorDTO { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"INSERT INTO AFI_PREFERENCIAS (COD_PREFERENCIA, Descripcion, ACTIVA, registro_fecha, registro_usuario)
                              VALUES (@COD_PREFERENCIA, @Descripcion, @ACTIVA, GETDATE(), @Usuario)";
                connection.Execute(query, new
                {
                    COD_PREFERENCIA = preferenciaTipo.Cod_Preferencia.ToUpper(),
                    Descripcion = preferenciaTipo.Descripcion,
                    ACTIVA = preferenciaTipo.Activa,
                    Usuario = usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipo de Preferencia : {preferenciaTipo.Cod_Preferencia} - {preferenciaTipo.Descripcion}",
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
        /// Actualiza una preferencia existente.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="preferenciaTipo">Datos de la preferencia a actualizar</param>
        /// <returns></returns>
        private ErrorDTO AF_Preferencias_Actualizar(int CodEmpresa, string usuario, PreferenciaTipoData preferenciaTipo)
        {
            var result = new ErrorDTO { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"UPDATE AFI_PREFERENCIAS
                              SET Descripcion = @Descripcion,
                                  ACTIVA = @ACTIVA
                              WHERE COD_PREFERENCIA = @COD_PREFERENCIA";
                connection.Execute(query, new
                {
                    COD_PREFERENCIA = preferenciaTipo.Cod_Preferencia.ToUpper(),
                    Descripcion = preferenciaTipo.Descripcion,
                    ACTIVA = preferenciaTipo.Activa
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipo de Preferencia : {preferenciaTipo.Cod_Preferencia} - {preferenciaTipo.Descripcion}",
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
        /// Elimina una preferencia por su código.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="codPreferencia">Código de la preferencia a eliminar</param>
        /// <returns></returns>
        public ErrorDTO AF_Preferencias_Eliminar(int CodEmpresa, string usuario, string codPreferencia)
        {
            var result = new ErrorDTO { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"DELETE FROM AFI_PREFERENCIAS WHERE COD_PREFERENCIA = @COD_PREFERENCIA";
                connection.Execute(query, new { COD_PREFERENCIA = codPreferencia.ToUpper() });

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipo de Preferencia : {codPreferencia}",
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
        /// Valida si una preferencia ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="codPreferencia">Código de la preferencia a validar</param>
        /// <returns></returns>
        public ErrorDTO AF_Preferencias_Valida(int CodEmpresa, string codPreferencia)
        {
            var result = new ErrorDTO { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_PREFERENCIAS WHERE COD_PREFERENCIA = @COD_PREFERENCIA";
                var existe = connection.QueryFirstOrDefault<int>(query, new { COD_PREFERENCIA = codPreferencia.ToUpper() });
                if (existe > 0)
                {
                    result.Code = -1;
                    result.Description = "La preferencia ya existe.";
                }
                else
                {
                    result.Code = 0;
                    result.Description = "La preferencia es válida.";
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
        /// Obtiene la lista de preferencias sin paginación, con filtros aplicados.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de búsqueda</param>
        /// <returns></returns>
        public ErrorDTO<List<PreferenciaTipoData>> AF_Preferencias_Exportar(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDTO<List<PreferenciaTipoData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<PreferenciaTipoData>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string where = "";
                if (!string.IsNullOrEmpty(filtros.filtro))
                {
                    where = " WHERE ( COD_PREFERENCIA LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                }

                var query = $@"select COD_PREFERENCIA, descripcion, ACTIVA, Registro_Fecha, Registro_Usuario
                               from AFI_PREFERENCIAS
                               {where}
                               order by COD_PREFERENCIA";

                result.Result = connection.Query<PreferenciaTipoData>(query).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }
    }
}
