using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Personas;

namespace PgxAPI.DataBaseTier.ProGrX_Personas
{
    public class frmAF_Canales_TiposDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 1; // Modulo de Tesorería
        private readonly mSecurityMainDb _Security_MainDB;

        public frmAF_Canales_TiposDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de tipos de canales con filtros y paginación.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de búsqueda, orden y paginación</param>
        /// <returns></returns>
        public ErrorDTO<CanalTipoLista> AF_CanalesTipos_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDTO<CanalTipoLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new CanalTipoLista()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                // Total
                var queryTotal = @"select COUNT(CANAL_TIPO) from AFI_CANALES_TIPOS";
                result.Result.Total = connection.Query<int>(queryTotal).FirstOrDefault();

                // Filtros
                string where = "";
                if (!string.IsNullOrEmpty(filtros.filtro))
                {
                    where = " WHERE ( CANAL_TIPO LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                }

                // Orden
                string sortField = string.IsNullOrEmpty(filtros.sortField) ? "CANAL_TIPO" : filtros.sortField;
                string sortOrder = filtros.sortOrder == 0 ? "DESC" : "ASC";

                // Paginación
                int pagina = filtros.pagina ?? 0;
                int paginacion = filtros.paginacion ?? 10;

                var query = $@"select CANAL_TIPO, descripcion, activo, Registro_Fecha, Registro_Usuario
                               from AFI_CANALES_TIPOS
                               {where}
                               order by {sortField} {sortOrder}
                               OFFSET {pagina} ROWS 
                               FETCH NEXT {paginacion} ROWS ONLY";

                result.Result.Lista = connection.Query<CanalTipoData>(query).ToList();
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
        /// Inserta o actualiza un tipo de canal.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="canalTipo">Datos del tipo de canal</param>
        /// <returns></returns>
        public ErrorDTO AF_CanalesTipos_Guardar(int CodEmpresa, string usuario, CanalTipoData canalTipo)
        {
            var result = new ErrorDTO { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                // Verifico si existe el tipo de canal
                var queryExiste = @"SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_CANALES_TIPOS WHERE CANAL_TIPO = @CANAL_TIPO";
                var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { CANAL_TIPO = canalTipo.Canal_Tipo.ToUpper() });

                if (existe == 0)
                {
                    // Insertar
                    result = AF_CanalesTipos_Insertar(CodEmpresa, usuario, canalTipo);
                }
                else
                {
                    // Actualizar
                    result = AF_CanalesTipos_Actualizar(CodEmpresa, usuario, canalTipo);
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
        /// Inserta un nuevo tipo de canal.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="canalTipo">Datos del tipo de canal a insertar</param>
        /// <returns></returns>
        private ErrorDTO AF_CanalesTipos_Insertar(int CodEmpresa, string usuario, CanalTipoData canalTipo)
        {
            var result = new ErrorDTO { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"INSERT INTO AFI_CANALES_TIPOS (CANAL_TIPO, Descripcion, activo, registro_fecha, registro_usuario)
                              VALUES (@CANAL_TIPO, @Descripcion, @activo, GETDATE(), @Usuario)";
                connection.Execute(query, new
                {
                    CANAL_TIPO = canalTipo.Canal_Tipo.ToUpper(),
                    Descripcion = canalTipo.Descripcion,
                    activo = canalTipo.Activo,
                    Usuario = usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipo de Canal : {canalTipo.Canal_Tipo} - {canalTipo.Descripcion}",
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
        /// Actualiza un tipo de canal existente.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="canalTipo">Datos del tipo de canal a actualizar</param>
        /// <returns></returns>
        private ErrorDTO AF_CanalesTipos_Actualizar(int CodEmpresa, string usuario, CanalTipoData canalTipo)
        {
            var result = new ErrorDTO { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"UPDATE AFI_CANALES_TIPOS
                              SET Descripcion = @Descripcion,
                                  activo = @activo
                              WHERE CANAL_TIPO = @CANAL_TIPO";
                connection.Execute(query, new
                {
                    CANAL_TIPO = canalTipo.Canal_Tipo.ToUpper(),
                    Descripcion = canalTipo.Descripcion,
                    activo = canalTipo.Activo
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipo de Canal : {canalTipo.Canal_Tipo} - {canalTipo.Descripcion}",
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
        /// Elimina un tipo de canal por su código.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="canalTipo">Código del tipo de canal a eliminar</param>
        /// <returns></returns>
        public ErrorDTO AF_CanalesTipos_Eliminar(int CodEmpresa, string usuario, string canalTipo)
        {
            var result = new ErrorDTO { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"DELETE FROM AFI_CANALES_TIPOS WHERE CANAL_TIPO = @CANAL_TIPO";
                connection.Execute(query, new { CANAL_TIPO = canalTipo.ToUpper() });

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipo de Canal : {canalTipo}",
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
        /// Valida si un tipo de canal ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="canalTipo">Código del tipo de canal a validar</param>
        /// <returns></returns>
        public ErrorDTO AF_CanalesTipos_Valida(int CodEmpresa, string canalTipo)
        {
            var result = new ErrorDTO { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_CANALES_TIPOS WHERE CANAL_TIPO = @CANAL_TIPO";
                var existe = connection.QueryFirstOrDefault<int>(query, new { CANAL_TIPO = canalTipo.ToUpper() });
                if (existe > 0)
                {
                    result.Code = -1;
                    result.Description = "El tipo de canal ya existe.";
                }
                else
                {
                    result.Code = 0;
                    result.Description = "El tipo de canal es válido.";
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
        /// Obtiene la lista de tipos de canales sin paginación, con filtros aplicados.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de búsqueda</param>
        /// <returns></returns>
        public ErrorDTO<List<CanalTipoData>> AF_CanalesTipos_Exportar(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDTO<List<CanalTipoData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CanalTipoData>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string where = "";
                if (!string.IsNullOrEmpty(filtros.filtro))
                {
                    where = " WHERE ( CANAL_TIPO LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                }

                var query = $@"select CANAL_TIPO, descripcion, activo, Registro_Fecha, Registro_Usuario
                               from AFI_CANALES_TIPOS
                               {where}
                               order by CANAL_TIPO";

                result.Result = connection.Query<CanalTipoData>(query).ToList();
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
