using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Personas;

namespace PgxAPI.DataBaseTier.ProGrX_Personas
{
    public class frmAF_CatalogosDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 1;
        private readonly mSecurityMainDb _Security_MainDB;

        public frmAF_CatalogosDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de catálogos por tipo, con filtros aplicados a todos los campos principales.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="tipoId">Tipo de catálogo</param>
        /// <param name="filtros">Filtros de búsqueda, orden y paginación</param>
        /// <returns></returns>
        public ErrorDto<CatalogoLista> AF_Catalogos_Obtener(int CodEmpresa, int tipoId, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<CatalogoLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new CatalogoLista()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                // Total
                var queryTotal = @"SELECT COUNT(*) FROM AFI_CATALOGOS WHERE Tipo_Id = @Tipo_Id";
                result.Result.Total = connection.Query<int>(queryTotal, new { Tipo_Id = tipoId }).FirstOrDefault();

                // Filtros
                string where = " WHERE Tipo_Id = @Tipo_Id";
                if (!string.IsNullOrEmpty(filtros.filtro))
                {
                    where += " AND ( " +
                        "CAST(Linea_Id AS VARCHAR) LIKE '%" + filtros.filtro + "%' OR " +
                        "Catalogo_Id LIKE '%" + filtros.filtro + "%' OR " +
                        "Descripcion LIKE '%" + filtros.filtro + "%' OR " +
                        "CAST(Activo AS VARCHAR) LIKE '%" + filtros.filtro + "%' OR " +
                        "CAST(Tipo_Id AS VARCHAR) LIKE '%" + filtros.filtro + "%' OR " +
                        "CONVERT(VARCHAR, Registro_Fecha, 120) LIKE '%" + filtros.filtro + "%' OR " +
                        "Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                }

                // Orden
                string sortField = string.IsNullOrEmpty(filtros.sortField) ? "Catalogo_Id" : filtros.sortField;
                string sortOrder = filtros.sortOrder == 0 ? "DESC" : "ASC";
                int pagina = filtros.pagina ?? 0;
                int paginacion = filtros.paginacion ?? 10;

                var query = $@"SELECT Linea_Id, Catalogo_Id, Descripcion, Activo, Tipo_Id, Registro_Fecha, Registro_Usuario
                       FROM AFI_CATALOGOS
                       {where}
                       ORDER BY {sortField} {sortOrder}
                       OFFSET {pagina} ROWS 
                       FETCH NEXT {paginacion} ROWS ONLY";

                result.Result.Lista = connection.Query<CatalogoData>(query, new { Tipo_Id = tipoId }).ToList();
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
        /// Valida si existe un catálogo por su id y tipo.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="catalogoId">Id del catálogo</param>
        /// <param name="tipoId">Tipo de catálogo</param>
        /// <returns></returns>
        public ErrorDto<CatalogoValidate> AF_Catalogos_Valida(int CodEmpresa, string catalogoId, int tipoId)
        {
            var result = new ErrorDto<CatalogoValidate> { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"SELECT COUNT(*) AS Existe, MAX(Linea_Id) AS Linea_Id
                              FROM AFI_CATALOGOS
                              WHERE Catalogo_Id = @Catalogo_Id AND Tipo_Id = @Tipo_Id";
                var data = connection.QueryFirstOrDefault<CatalogoValidate>(query, new { Catalogo_Id = catalogoId, Tipo_Id = tipoId });
                if (data != null && data.Existe > 0)
                {
                    result.Code = -1;
                    result.Description = "El catálogo ya existe.";
                    result.Result = data;
                }
                else
                {
                    result.Code = 0;
                    result.Description = "El catálogo es válido.";
                    result.Result = data;
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
        /// Inserta o actualiza un catálogo.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="catalogo">Datos del catálogo</param>
        /// <returns></returns>
        public ErrorDto AF_Catalogos_Guardar(int CodEmpresa, string usuario, CatalogoData catalogo)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var queryExiste = @"SELECT COUNT(*) FROM AFI_CATALOGOS WHERE Linea_Id = @Linea_Id";
                var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { Linea_Id = catalogo.Linea_Id });

                if (catalogo.Linea_Id == 0 || existe == 0)
                {
                    result = AF_Catalogos_Insertar(CodEmpresa, usuario, catalogo);
                }
                else
                {
                    result = AF_Catalogos_Actualizar(CodEmpresa, usuario, catalogo);
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
        /// Inserta un nuevo catálogo.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="catalogo">Datos del catálogo a insertar</param>
        /// <returns></returns>
        private ErrorDto AF_Catalogos_Insertar(int CodEmpresa, string usuario, CatalogoData catalogo)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"INSERT INTO AFI_CATALOGOS
                              (Catalogo_Id, Descripcion, Activo, Tipo_Id, Registro_Fecha, Registro_Usuario)
                              VALUES (@Catalogo_Id, @Descripcion, @Activo, @Tipo_Id, GETDATE(), @Usuario)";
                connection.Execute(query, new
                {
                    Catalogo_Id = catalogo.Catalogo_Id,
                    Descripcion = catalogo.Descripcion,
                    Activo = catalogo.Activo,
                    Tipo_Id = catalogo.Tipo_Id,
                    Usuario = usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Catálogo : {catalogo.Catalogo_Id} - {catalogo.Descripcion}",
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
        /// Actualiza un catálogo existente.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="catalogo">Datos del catálogo a actualizar</param>
        /// <returns></returns>
        private ErrorDto AF_Catalogos_Actualizar(int CodEmpresa, string usuario, CatalogoData catalogo)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"UPDATE AFI_CATALOGOS
                              SET Descripcion = @Descripcion,
                                  Activo = @Activo,
                                  Modifica_Fecha = GETDATE(),
                                  Modifica_Usuario = @Usuario
                              WHERE Linea_Id = @Linea_Id";
                connection.Execute(query, new
                {
                    Linea_Id = catalogo.Linea_Id,
                    Descripcion = catalogo.Descripcion,
                    Activo = catalogo.Activo,
                    Usuario = usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Catálogo : {catalogo.Catalogo_Id} - {catalogo.Descripcion}",
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
        /// Elimina un catálogo por su id.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="lineaId">Id del catálogo a eliminar</param>
        /// <returns></returns>
        public ErrorDto AF_Catalogos_Eliminar(int CodEmpresa, string usuario, int lineaId)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"DELETE FROM AFI_CATALOGOS WHERE Linea_Id = @Linea_Id";
                connection.Execute(query, new { Linea_Id = lineaId });

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Catálogo : {lineaId}",
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
        /// Obtiene la lista de tipos de catálogo activos para dropdown.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Catalogos_Tipos_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var query = @"SELECT CAST(Tipo_Id AS VARCHAR) AS item, Descripcion AS descripcion
                              FROM AFI_CATALOGOS_TIPOS
                              WHERE Activo = 1
                              ORDER BY Descripcion";

                result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// Obtiene la lista completa de tipos de catálogo, con filtros aplicados a todos los campos principales.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de búsqueda, orden y paginación</param>
        /// <returns></returns>
        public ErrorDto<List<CatalogoTipoData>> AF_Catalogos_Tipos_ObtenerTodos(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<List<CatalogoTipoData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CatalogoTipoData>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string where = "";
                if (!string.IsNullOrEmpty(filtros.filtro))
                {
                    where = " WHERE ( " +
                        "CAST(Tipo_Id AS VARCHAR) LIKE '%" + filtros.filtro + "%' OR " +
                        "Descripcion LIKE '%" + filtros.filtro + "%' OR " +
                        "CAST(Activo AS VARCHAR) LIKE '%" + filtros.filtro + "%' OR " +
                        "CONVERT(VARCHAR, Registro_Fecha, 120) LIKE '%" + filtros.filtro + "%' OR " +
                        "Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                }

                // Orden y paginación opcional
                string sortField = string.IsNullOrEmpty(filtros.sortField) ? "Descripcion" : filtros.sortField;
                string sortOrder = filtros.sortOrder == 0 ? "DESC" : "ASC";
                int pagina = filtros.pagina ?? 0;
                int paginacion = filtros.paginacion ?? 1000; // o el valor que prefieras

                var query = $@"SELECT Tipo_Id, Descripcion, Activo, Registro_Fecha, Registro_Usuario
                       FROM AFI_CATALOGOS_TIPOS
                       {where}
                       ORDER BY {sortField} {sortOrder}
                       OFFSET {pagina} ROWS 
                       FETCH NEXT {paginacion} ROWS ONLY";

                result.Result = connection.Query<CatalogoTipoData>(query).ToList();
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
