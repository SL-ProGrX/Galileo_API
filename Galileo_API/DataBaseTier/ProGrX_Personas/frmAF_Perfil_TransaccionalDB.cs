using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Personas;

namespace PgxAPI.DataBaseTier.ProGrX_Personas
{
    public class frmAF_Perfil_TransaccionalDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 1;
        private readonly mSecurityMainDb _Security_MainDB;

        public frmAF_Perfil_TransaccionalDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de perfiles transaccionales con filtros y paginación.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de búsqueda, orden y paginación</param>
        /// <returns></returns>
        public ErrorDto<PerfilTransaccionalLista> AF_PerfilTransaccional_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<PerfilTransaccionalLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new PerfilTransaccionalLista()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var queryTotal = @"select COUNT(PT_Id) from AFI_PERFIL_TRANSACCIONAL";
                result.Result.Total = connection.Query<int>(queryTotal).FirstOrDefault();

                string where = "";
                if (!string.IsNullOrEmpty(filtros.filtro))
                {
                    where = " WHERE ( " +
                        "CAST(PT_Id AS VARCHAR) LIKE '%" + filtros.filtro + "%' OR " +
                        "CAST(Monto_Minimo AS VARCHAR) LIKE '%" + filtros.filtro + "%' OR " +
                        "CAST(Monto_Maximo AS VARCHAR) LIKE '%" + filtros.filtro + "%' OR " +
                        "Nivel LIKE '%" + filtros.filtro + "%' OR " +
                        "CONVERT(VARCHAR, Registro_Fecha, 120) LIKE '%" + filtros.filtro + "%' OR " +
                        "Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                }

                string sortField = string.IsNullOrEmpty(filtros.sortField) ? "Monto_Minimo" : filtros.sortField;
                string sortOrder = filtros.sortOrder == 0 ? "DESC" : "ASC";
                int pagina = filtros.pagina ?? 0;
                int paginacion = filtros.paginacion ?? 10;

                var query = $@"SELECT PT_Id, Monto_Minimo, Monto_Maximo, Nivel, Activo, Registro_Fecha, Registro_Usuario
                               FROM AFI_PERFIL_TRANSACCIONAL
                               {where}
                               ORDER BY {sortField} {sortOrder}
                               OFFSET {pagina} ROWS 
                               FETCH NEXT {paginacion} ROWS ONLY";

                result.Result.Lista = connection.Query<PerfilTransaccionalData>(query).ToList();
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
        /// Inserta o actualiza un perfil transaccional.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="perfil">Datos del perfil transaccional</param>
        /// <returns></returns>
        public ErrorDto AF_PerfilTransaccional_Guardar(int CodEmpresa, string usuario, PerfilTransaccionalData perfil)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var queryExiste = @"SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_PERFIL_TRANSACCIONAL WHERE PT_Id = @PT_Id";
                var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { PT_Id = perfil.PT_Id });

                if (perfil.PT_Id == 0 || existe == 0)
                {
                    result = AF_PerfilTransaccional_Insertar(CodEmpresa, usuario, perfil);
                }
                else
                {
                    result = AF_PerfilTransaccional_Actualizar(CodEmpresa, usuario, perfil);
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
        /// Inserta un nuevo perfil transaccional.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="perfil">Datos del perfil a insertar</param>
        /// <returns></returns>
        private ErrorDto AF_PerfilTransaccional_Insertar(int CodEmpresa, string usuario, PerfilTransaccionalData perfil)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"INSERT INTO AFI_PERFIL_TRANSACCIONAL
                              (Monto_Minimo, Monto_Maximo, Nivel, Activo, Registro_Fecha, Registro_Usuario)
                              VALUES (@Monto_Minimo, @Monto_Maximo, @Nivel, @Activo, GETDATE(), @Usuario)";
                connection.Execute(query, new
                {
                    Monto_Minimo = perfil.Monto_Minimo,
                    Monto_Maximo = perfil.Monto_Maximo,
                    Nivel = perfil.Nivel,
                    Activo = perfil.Activo,
                    Usuario = usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Perfil Transaccional : {perfil.Nivel} ({perfil.Monto_Minimo}-{perfil.Monto_Maximo})",
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
        /// Actualiza un perfil transaccional existente.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="perfil">Datos del perfil a actualizar</param>
        /// <returns></returns>
        private ErrorDto AF_PerfilTransaccional_Actualizar(int CodEmpresa, string usuario, PerfilTransaccionalData perfil)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"UPDATE AFI_PERFIL_TRANSACCIONAL
                              SET Monto_Minimo = @Monto_Minimo,
                                  Monto_Maximo = @Monto_Maximo,
                                  Nivel = @Nivel,
                                  Activo = @Activo,
                                  Modifica_Fecha = GETDATE(),
                                  Modifica_Usuario = @Usuario
                              WHERE PT_Id = @PT_Id";
                connection.Execute(query, new
                {
                    PT_Id = perfil.PT_Id,
                    Monto_Minimo = perfil.Monto_Minimo,
                    Monto_Maximo = perfil.Monto_Maximo,
                    Nivel = perfil.Nivel,
                    Activo = perfil.Activo,
                    Usuario = usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Perfil Transaccional : {perfil.Nivel} ({perfil.Monto_Minimo}-{perfil.Monto_Maximo})",
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
        /// Elimina un perfil transaccional por su id.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="ptId">Id del perfil a eliminar</param>
        /// <returns></returns>
        public ErrorDto AF_PerfilTransaccional_Eliminar(int CodEmpresa, string usuario, int ptId)
        {
            var result = new ErrorDto { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"DELETE FROM AFI_PERFIL_TRANSACCIONAL WHERE PT_Id = @PT_Id";
                connection.Execute(query, new { PT_Id = ptId });

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Perfil Transaccional : {ptId}",
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
        /// Obtiene la lista de perfiles transaccionales sin paginación, con filtros aplicados.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de búsqueda</param>
        /// <returns></returns>
        public ErrorDto<List<PerfilTransaccionalData>> AF_PerfilTransaccional_Exportar(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDto<List<PerfilTransaccionalData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<PerfilTransaccionalData>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string where = "";
                if (!string.IsNullOrEmpty(filtros.filtro))
                {
                    where = " WHERE ( " +
                        "CAST(PT_Id AS VARCHAR) LIKE '%" + filtros.filtro + "%' OR " +
                        "CAST(Monto_Minimo AS VARCHAR) LIKE '%" + filtros.filtro + "%' OR " +
                        "CAST(Monto_Maximo AS VARCHAR) LIKE '%" + filtros.filtro + "%' OR " +
                        "Nivel LIKE '%" + filtros.filtro + "%' OR " +
                        "CONVERT(VARCHAR, Registro_Fecha, 120) LIKE '%" + filtros.filtro + "%' OR " +
                        "Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                }

                var query = $@"SELECT PT_Id, Monto_Minimo, Monto_Maximo, Nivel, Activo, Registro_Fecha, Registro_Usuario
                               FROM AFI_PERFIL_TRANSACCIONAL
                               {where}
                               ORDER BY Monto_Minimo ASC";

                result.Result = connection.Query<PerfilTransaccionalData>(query).ToList();
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
