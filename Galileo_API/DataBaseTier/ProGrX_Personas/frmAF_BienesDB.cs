using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Personas;
using PgxAPI.Models.Security;
using System.Reflection;

namespace PgxAPI.DataBaseTier.ProGrX_Personas
{
    public class frmAF_BienesDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 1; // Modulo de Tesorería
        private readonly MSecurityMainDb _Security_MainDB;

        public frmAF_BienesDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de tipos de bienes con filtros y paginación.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de búsqueda, orden y paginación</param>
        /// <returns></returns>
        public ErrorDto<BienesTipoLista> AF_BienesTipos_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<BienesTipoLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new BienesTipoLista()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                // Total
                var queryTotal = @"select COUNT(BIEN_TIPO) from AFI_BIENES_TIPOS";
                result.Result.Total = connection.Query<int>(queryTotal).FirstOrDefault();

                // Filtros
                string where = "";
                if (!string.IsNullOrEmpty(filtros.filtro))
                {
                    where = " WHERE ( BIEN_TIPO LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                }

                // Orden
                string sortField = string.IsNullOrEmpty(filtros.sortField) ? "BIEN_TIPO" : filtros.sortField;
                string sortOrder = filtros.sortOrder == 0 ? "DESC" : "ASC";

                // Paginación
                int pagina = (filtros.pagina == 0 ? 0 : filtros.pagina);
                int paginacion = (filtros.paginacion == 0 ? 10 : filtros.paginacion);

                var query = $@"select BIEN_TIPO, descripcion, ACTIVO, Registro_Fecha, Registro_Usuario
                               from AFI_BIENES_TIPOS
                               {where}
                               order by {sortField} {sortOrder}
                               OFFSET {pagina} ROWS 
                               FETCH NEXT {paginacion} ROWS ONLY";

                result.Result.Lista = connection.Query<BienesTipoData>(query).ToList();
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
        /// Inserta o actualiza un tipo de bien.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="bienTipo">Datos del tipo de bien</param>
        /// <returns></returns>
        public ErrorDto AF_BienesTipos_Guardar(int CodEmpresa, string usuario, BienesTipoData bienTipo)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                // Verifico si existe el tipo de bien
                var queryExiste = @"SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_BIENES_TIPOS WHERE BIEN_TIPO = @BIEN_TIPO";
                var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { BIEN_TIPO = bienTipo.Bien_Tipo.ToUpper() });

                if (existe == 0)
                {
                    // Insertar
                    result = AF_BienesTipos_Insertar(CodEmpresa, usuario, bienTipo);
                }
                else
                {
                    // Actualizar
                    result = AF_BienesTipos_Actualizar(CodEmpresa, usuario, bienTipo);
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
        /// Inserta un nuevo tipo de bien.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="bienTipo">Datos del tipo de bien a insertar</param>
        /// <returns></returns>
        private ErrorDto AF_BienesTipos_Insertar(int CodEmpresa, string usuario, BienesTipoData bienTipo)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"INSERT INTO AFI_BIENES_TIPOS (BIEN_TIPO, Descripcion, ACTIVO, registro_fecha, registro_usuario)
                              VALUES (@BIEN_TIPO, @Descripcion, @ACTIVO, GETDATE(), @Usuario)";
                connection.Execute(query, new
                {
                    BIEN_TIPO = bienTipo.Bien_Tipo.ToUpper(),
                    Descripcion = bienTipo.Descripcion,
                    ACTIVO = bienTipo.Activo,
                    Usuario = usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipo de Bien : {bienTipo.Bien_Tipo} - {bienTipo.Descripcion}",
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
        /// Actualiza un tipo de bien existente.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="bienTipo">Datos del tipo de bien a actualizar</param>
        /// <returns></returns>
        private ErrorDto AF_BienesTipos_Actualizar(int CodEmpresa, string usuario, BienesTipoData bienTipo)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"UPDATE AFI_BIENES_TIPOS
                              SET Descripcion = @Descripcion,
                                  ACTIVO = @ACTIVO
                              WHERE BIEN_TIPO = @BIEN_TIPO";
                connection.Execute(query, new
                {
                    BIEN_TIPO = bienTipo.Bien_Tipo.ToUpper(),
                    Descripcion = bienTipo.Descripcion,
                    ACTIVO = bienTipo.Activo
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipo de Bien : {bienTipo.Bien_Tipo} - {bienTipo.Descripcion}",
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
        /// Elimina un tipo de bien por su código.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="bienTipo">Código del tipo de bien a eliminar</param>
        /// <returns></returns>
        public ErrorDto AF_BienesTipos_Eliminar(int CodEmpresa, string usuario, string bienTipo)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"DELETE FROM AFI_BIENES_TIPOS WHERE BIEN_TIPO = @BIEN_TIPO";
                connection.Execute(query, new { BIEN_TIPO = bienTipo.ToUpper() });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipo de Bien : {bienTipo}",
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
        /// Valida si un tipo de bien ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="bienTipo">Código del tipo de bien a validar</param>
        /// <returns></returns>
        public ErrorDto AF_BienesTipos_Valida(int CodEmpresa, string bienTipo)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_BIENES_TIPOS WHERE BIEN_TIPO = @BIEN_TIPO";
                var existe = connection.QueryFirstOrDefault<int>(query, new { BIEN_TIPO = bienTipo.ToUpper() });
                if (existe > 0)
                {
                    result.Code = -1;
                    result.Description = "El tipo de bien ya existe.";
                }
                else
                {
                    result.Code = 0;
                    result.Description = "El tipo de bien es válido.";
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
        /// Obtiene la lista de tipos de bienes sin paginación, con filtros aplicados.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de búsqueda</param>
        /// <returns></returns>
        public ErrorDto<List<BienesTipoData>> AF_BienesTipos_Exportar(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<List<BienesTipoData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<BienesTipoData>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string where = "";
                if (!string.IsNullOrEmpty(filtros.filtro))
                {
                    where = " WHERE ( BIEN_TIPO LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                }

                var query = $@"select BIEN_TIPO, descripcion, ACTIVO, Registro_Fecha, Registro_Usuario
                               from AFI_BIENES_TIPOS
                               {where}
                               order by BIEN_TIPO";

                result.Result = connection.Query<BienesTipoData>(query).ToList();
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
