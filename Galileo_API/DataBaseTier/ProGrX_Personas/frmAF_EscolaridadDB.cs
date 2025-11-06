using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Personas;
using PgxAPI.Models.Security;

namespace PgxAPI.DataBaseTier.ProGrX_Personas
{
    public class frmAF_EscolaridadDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 1; // Modulo de Tesorería
        private readonly MSecurityMainDb _Security_MainDB;

        public frmAF_EscolaridadDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de tipos de escolaridad con filtros y paginación.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de búsqueda, orden y paginación</param>
        /// <returns></returns>
        public ErrorDto<NivelEscolaridadLista> AF_EscolaridadTipos_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<NivelEscolaridadLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new NivelEscolaridadLista()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                // Total
                var queryTotal = @"select COUNT(ESCOLARIDAD_TIPO) from AFI_ESCOLARIDAD_TIPOS";
                result.Result.Total = connection.Query<int>(queryTotal).FirstOrDefault();

                // Filtros
                string where = "";
                if (!string.IsNullOrEmpty(filtros.filtro))
                {
                    where = " WHERE ( ESCOLARIDAD_TIPO LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                }

                // Orden
                string sortField = string.IsNullOrEmpty(filtros.sortField) ? "ESCOLARIDAD_TIPO" : filtros.sortField;
                string sortOrder = filtros.sortOrder == 0 ? "DESC" : "ASC";

                // Paginación
                int pagina = (filtros.pagina != 0) ? filtros.pagina : 0;
                int paginacion = (filtros.paginacion != 0) ? filtros.paginacion : 10;

                var query = $@"select ESCOLARIDAD_TIPO, descripcion, ACTIVO, Registro_Fecha, Registro_Usuario
                               from AFI_ESCOLARIDAD_TIPOS
                               {where}
                               order by {sortField} {sortOrder}
                               OFFSET {pagina} ROWS 
                               FETCH NEXT {paginacion} ROWS ONLY";

                result.Result.Lista = connection.Query<NivelEscolaridadData>(query).ToList();
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
        /// Inserta o actualiza un tipo de escolaridad.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="escolaridadTipo">Datos del tipo de escolaridad</param>
        /// <returns></returns>
        public ErrorDto AF_EscolaridadTipos_Guardar(int CodEmpresa, string usuario, NivelEscolaridadData escolaridadTipo)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                // Verifico si existe el tipo de escolaridad
                var queryExiste = @"SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_ESCOLARIDAD_TIPOS WHERE ESCOLARIDAD_TIPO = @ESCOLARIDAD_TIPO";
                var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { ESCOLARIDAD_TIPO = escolaridadTipo.Escolaridad_Tipo.ToUpper() });

                if (existe == 0)
                {
                    // Insertar
                    result = AF_EscolaridadTipos_Insertar(CodEmpresa, usuario, escolaridadTipo);
                }
                else
                {
                    // Actualizar
                    result = AF_EscolaridadTipos_Actualizar(CodEmpresa, usuario, escolaridadTipo);
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
        /// Inserta un nuevo tipo de escolaridad.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="escolaridadTipo">Datos del tipo de escolaridad a insertar</param>
        /// <returns></returns>
        private ErrorDto AF_EscolaridadTipos_Insertar(int CodEmpresa, string usuario, NivelEscolaridadData escolaridadTipo)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"INSERT INTO AFI_ESCOLARIDAD_TIPOS (ESCOLARIDAD_TIPO, Descripcion, ACTIVO, registro_fecha, registro_usuario)
                              VALUES (@ESCOLARIDAD_TIPO, @Descripcion, @ACTIVO, GETDATE(), @Usuario)";
                connection.Execute(query, new
                {
                    ESCOLARIDAD_TIPO = escolaridadTipo.Escolaridad_Tipo.ToUpper(),
                    Descripcion = escolaridadTipo.Descripcion,
                    ACTIVO = escolaridadTipo.Activo,
                    Usuario = usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipo de Escolaridad : {escolaridadTipo.Escolaridad_Tipo} - {escolaridadTipo.Descripcion}",
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
        /// Actualiza un tipo de escolaridad existente.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="escolaridadTipo">Datos del tipo de escolaridad a actualizar</param>
        /// <returns></returns>
        private ErrorDto AF_EscolaridadTipos_Actualizar(int CodEmpresa, string usuario, NivelEscolaridadData escolaridadTipo)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"UPDATE AFI_ESCOLARIDAD_TIPOS
                              SET Descripcion = @Descripcion,
                                  ACTIVO = @ACTIVO
                              WHERE ESCOLARIDAD_TIPO = @ESCOLARIDAD_TIPO";
                connection.Execute(query, new
                {
                    ESCOLARIDAD_TIPO = escolaridadTipo.Escolaridad_Tipo.ToUpper(),
                    Descripcion = escolaridadTipo.Descripcion,
                    ACTIVO = escolaridadTipo.Activo
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipo de Escolaridad : {escolaridadTipo.Escolaridad_Tipo} - {escolaridadTipo.Descripcion}",
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
        /// Elimina un tipo de escolaridad por su código.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="escolaridadTipo">Código del tipo de escolaridad a eliminar</param>
        /// <returns></returns>
        public ErrorDto AF_EscolaridadTipos_Eliminar(int CodEmpresa, string usuario, string escolaridadTipo)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"DELETE FROM AFI_ESCOLARIDAD_TIPOS WHERE ESCOLARIDAD_TIPO = @ESCOLARIDAD_TIPO";
                connection.Execute(query, new { ESCOLARIDAD_TIPO = escolaridadTipo.ToUpper() });

                _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Tipo de Escolaridad : {escolaridadTipo}",
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
        /// Valida si un tipo de escolaridad ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="escolaridadTipo">Código del tipo de escolaridad a validar</param>
        /// <returns></returns>
        public ErrorDto AF_EscolaridadTipos_Valida(int CodEmpresa, string escolaridadTipo)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_ESCOLARIDAD_TIPOS WHERE ESCOLARIDAD_TIPO = @ESCOLARIDAD_TIPO";
                var existe = connection.QueryFirstOrDefault<int>(query, new { ESCOLARIDAD_TIPO = escolaridadTipo.ToUpper() });
                if (existe > 0)
                {
                    result.Code = -1;
                    result.Description = "El tipo de escolaridad ya existe.";
                }
                else
                {
                    result.Code = 0;
                    result.Description = "El tipo de escolaridad es válido.";
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
        /// Obtiene la lista de tipos de escolaridad sin paginación, con filtros aplicados.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de búsqueda</param>
        /// <returns></returns>
        public ErrorDto<List<NivelEscolaridadData>> AF_EscolaridadTipos_Exportar(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<List<NivelEscolaridadData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<NivelEscolaridadData>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string where = "";
                if (!string.IsNullOrEmpty(filtros.filtro))
                {
                    where = " WHERE ( ESCOLARIDAD_TIPO LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                }

                var query = $@"select ESCOLARIDAD_TIPO, descripcion, ACTIVO, Registro_Fecha, Registro_Usuario
                               from AFI_ESCOLARIDAD_TIPOS
                               {where}
                               order by ESCOLARIDAD_TIPO";

                result.Result = connection.Query<NivelEscolaridadData>(query).ToList();
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
