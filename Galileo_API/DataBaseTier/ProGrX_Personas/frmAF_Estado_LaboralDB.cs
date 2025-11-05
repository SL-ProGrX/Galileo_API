using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Personas;

namespace PgxAPI.DataBaseTier.ProGrX_Personas
{
    public class frmAF_Estado_LaboralDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 1; // Modulo de Tesorería
        private readonly mSecurityMainDb _Security_MainDB;

        public frmAF_Estado_LaboralDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de estados laborales con filtros y paginación.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de búsqueda, orden y paginación</param>
        /// <returns></returns>
        public ErrorDto<EstadoLaboralLista> AF_EstadoLaboral_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<EstadoLaboralLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new EstadoLaboralLista()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                // Total
                var queryTotal = @"select COUNT(ESTADO_LABORAL) from AFI_ESTADO_LABORAL";
                result.Result.Total = connection.Query<int>(queryTotal).FirstOrDefault();

                // Filtros
                string where = "";
                if (!string.IsNullOrEmpty(filtros.filtro))
                {
                    where = " WHERE ( ESTADO_LABORAL LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                }

                // Orden
                string sortField = string.IsNullOrEmpty(filtros.sortField) ? "ESTADO_LABORAL" : filtros.sortField;
                string sortOrder = filtros.sortOrder == 0 ? "DESC" : "ASC";

                // Paginación
                int pagina = filtros.pagina ?? 0;
                int paginacion = filtros.paginacion ?? 10;

                var query = $@"select ESTADO_LABORAL, descripcion, activo, Registro_Fecha, Registro_Usuario
                               from AFI_ESTADO_LABORAL
                               {where}
                               order by {sortField} {sortOrder}
                               OFFSET {pagina} ROWS 
                               FETCH NEXT {paginacion} ROWS ONLY";

                result.Result.Lista = connection.Query<EstadoLaboralData>(query).ToList();
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
        /// Inserta o actualiza un estado laboral.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="estadoLaboral">Datos del estado laboral</param>
        /// <returns></returns>
        public ErrorDto AF_EstadoLaboral_Guardar(int CodEmpresa, string usuario, EstadoLaboralData estadoLaboral)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                // Verifico si existe el estado laboral
                var queryExiste = @"SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_ESTADO_LABORAL WHERE ESTADO_LABORAL = @ESTADO_LABORAL";
                var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { ESTADO_LABORAL = estadoLaboral.Estado_Laboral.ToUpper() });

                if (existe == 0)
                {
                    // Insertar
                    result = AF_EstadoLaboral_Insertar(CodEmpresa, usuario, estadoLaboral);
                }
                else
                {
                    // Actualizar
                    result = AF_EstadoLaboral_Actualizar(CodEmpresa, usuario, estadoLaboral);
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
        /// Inserta un nuevo estado laboral.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="estadoLaboral">Datos del estado laboral a insertar</param>
        /// <returns></returns>
        private ErrorDto AF_EstadoLaboral_Insertar(int CodEmpresa, string usuario, EstadoLaboralData estadoLaboral)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"INSERT INTO AFI_ESTADO_LABORAL (ESTADO_LABORAL, Descripcion, activo, registro_fecha, registro_usuario)
                              VALUES (@ESTADO_LABORAL, @Descripcion, @activo, GETDATE(), @Usuario)";
                connection.Execute(query, new
                {
                    ESTADO_LABORAL = estadoLaboral.Estado_Laboral.ToUpper(),
                    Descripcion = estadoLaboral.Descripcion,
                    activo = estadoLaboral.Activo,
                    Usuario = usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Estado Laboral : {estadoLaboral.Estado_Laboral} - {estadoLaboral.Descripcion}",
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
        /// Actualiza un estado laboral existente.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="estadoLaboral">Datos del estado laboral a actualizar</param>
        /// <returns></returns>
        private ErrorDto AF_EstadoLaboral_Actualizar(int CodEmpresa, string usuario, EstadoLaboralData estadoLaboral)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"UPDATE AFI_ESTADO_LABORAL
                              SET Descripcion = @Descripcion,
                                  activo = @activo
                              WHERE ESTADO_LABORAL = @ESTADO_LABORAL";
                connection.Execute(query, new
                {
                    ESTADO_LABORAL = estadoLaboral.Estado_Laboral.ToUpper(),
                    Descripcion = estadoLaboral.Descripcion,
                    activo = estadoLaboral.Activo
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Estado Laboral : {estadoLaboral.Estado_Laboral} - {estadoLaboral.Descripcion}",
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
        /// Elimina un estado laboral por su código.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="estadoLaboral">Código del estado laboral a eliminar</param>
        /// <returns></returns>
        public ErrorDto AF_EstadoLaboral_Eliminar(int CodEmpresa, string usuario, string estadoLaboral)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"DELETE FROM AFI_ESTADO_LABORAL WHERE ESTADO_LABORAL = @ESTADO_LABORAL";
                connection.Execute(query, new { ESTADO_LABORAL = estadoLaboral.ToUpper() });

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Estado Laboral : {estadoLaboral}",
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
        /// Valida si un estado laboral ya existe en la base de datos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="estadoLaboral">Código del estado laboral a validar</param>
        /// <returns></returns>
        public ErrorDto AF_EstadoLaboral_Valida(int CodEmpresa, string estadoLaboral)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_ESTADO_LABORAL WHERE ESTADO_LABORAL = @ESTADO_LABORAL";
                var existe = connection.QueryFirstOrDefault<int>(query, new { ESTADO_LABORAL = estadoLaboral.ToUpper() });
                if (existe > 0)
                {
                    result.Code = -1;
                    result.Description = "El estado laboral ya existe.";
                }
                else
                {
                    result.Code = 0;
                    result.Description = "El estado laboral es válido.";
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
        /// Obtiene la lista de estados laborales sin paginación, con filtros aplicados.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de búsqueda</param>
        /// <returns></returns>
        public ErrorDto<List<EstadoLaboralData>> AF_EstadoLaboral_Exportar(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<List<EstadoLaboralData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<EstadoLaboralData>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string where = "";
                if (!string.IsNullOrEmpty(filtros.filtro))
                {
                    where = " WHERE ( ESTADO_LABORAL LIKE '%" + filtros.filtro + "%' " +
                            " OR descripcion LIKE '%" + filtros.filtro + "%' " +
                            " OR Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                }

                var query = $@"select ESTADO_LABORAL, descripcion, activo, Registro_Fecha, Registro_Usuario
                               from AFI_ESTADO_LABORAL
                               {where}
                               order by ESTADO_LABORAL";

                result.Result = connection.Query<EstadoLaboralData>(query).ToList();
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
