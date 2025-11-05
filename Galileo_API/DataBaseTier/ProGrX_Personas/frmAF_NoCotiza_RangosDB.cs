using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Personas;

namespace PgxAPI.DataBaseTier.ProGrX_Personas
{
    public class frmAF_NoCotiza_RangosDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 1;
        private readonly mSecurityMainDb _Security_MainDB;

        public frmAF_NoCotiza_RangosDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de rangos sin aportes con filtros y paginación.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de búsqueda, orden y paginación</param>
        /// <returns></returns>
        public ErrorDTO<NoCotizaRangosLista> AF_NoCotizaRangos_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDTO<NoCotizaRangosLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new NoCotizaRangosLista()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var queryTotal = @"select COUNT(Linea_Id) from AFI_SOCIOS_SIN_APORTES_RANGOS";
                result.Result.Total = connection.Query<int>(queryTotal).FirstOrDefault();

                string where = "";
                if (!string.IsNullOrEmpty(filtros.filtro))
                {
                    where = " WHERE ( " +
                        "CAST(Linea_Id AS VARCHAR) LIKE '%" + filtros.filtro + "%' OR " +
                        "CAST(Dia_Desde AS VARCHAR) LIKE '%" + filtros.filtro + "%' OR " +
                        "CAST(Dia_Hasta AS VARCHAR) LIKE '%" + filtros.filtro + "%' OR " +
                        "Descripcion LIKE '%" + filtros.filtro + "%' OR " +
                        "CONVERT(VARCHAR, Registro_Fecha, 120) LIKE '%" + filtros.filtro + "%' OR " +
                        "Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                }

                string sortField = string.IsNullOrEmpty(filtros.sortField) ? "Dia_Desde" : filtros.sortField;
                string sortOrder = filtros.sortOrder == 0 ? "DESC" : "ASC";
                int pagina = filtros.pagina ?? 0;
                int paginacion = filtros.paginacion ?? 10;

                var query = $@"SELECT Linea_Id, Dia_Desde, Dia_Hasta, Descripcion, Activo, Registro_Fecha, Registro_Usuario
                               FROM AFI_SOCIOS_SIN_APORTES_RANGOS
                               {where}
                               ORDER BY {sortField} {sortOrder}
                               OFFSET {pagina} ROWS 
                               FETCH NEXT {paginacion} ROWS ONLY";

                result.Result.Lista = connection.Query<NoCotizaRangosData>(query).ToList();
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
        /// Inserta o actualiza un rango sin aportes.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="rango">Datos del rango</param>
        /// <returns></returns>
        public ErrorDTO AF_NoCotizaRangos_Guardar(int CodEmpresa, string usuario, NoCotizaRangosData rango)
        {
            var result = new ErrorDTO { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var queryExiste = @"SELECT ISNULL(COUNT(*),0) AS Existe FROM AFI_SOCIOS_SIN_APORTES_RANGOS WHERE Linea_Id = @Linea_Id";
                var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { Linea_Id = rango.Linea_Id });

                if (rango.Linea_Id == 0 || existe == 0)
                {
                    result = AF_NoCotizaRangos_Insertar(CodEmpresa, usuario, rango);
                }
                else
                {
                    result = AF_NoCotizaRangos_Actualizar(CodEmpresa, usuario, rango);
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
        /// Inserta un nuevo rango sin aportes.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="rango">Datos del rango a insertar</param>
        /// <returns></returns>
        private ErrorDTO AF_NoCotizaRangos_Insertar(int CodEmpresa, string usuario, NoCotizaRangosData rango)
        {
            var result = new ErrorDTO { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"INSERT INTO AFI_SOCIOS_SIN_APORTES_RANGOS
                              (Dia_Desde, Dia_Hasta, Descripcion, Activo, Registro_Fecha, Registro_Usuario)
                              VALUES (@Dia_Desde, @Dia_Hasta, @Descripcion, @Activo, GETDATE(), @Usuario)";
                connection.Execute(query, new
                {
                    Dia_Desde = rango.Dia_Desde,
                    Dia_Hasta = rango.Dia_Hasta,
                    Descripcion = rango.Descripcion,
                    Activo = rango.Activo,
                    Usuario = usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"No Cotiza Rango : {rango.Descripcion} ({rango.Dia_Desde}-{rango.Dia_Hasta})",
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
        /// Actualiza un rango sin aportes existente.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="rango">Datos del rango a actualizar</param>
        /// <returns></returns>
        private ErrorDTO AF_NoCotizaRangos_Actualizar(int CodEmpresa, string usuario, NoCotizaRangosData rango)
        {
            var result = new ErrorDTO { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"UPDATE AFI_SOCIOS_SIN_APORTES_RANGOS
                              SET Dia_Desde = @Dia_Desde,
                                  Dia_Hasta = @Dia_Hasta,
                                  Descripcion = @Descripcion,
                                  Activo = @Activo,
                                  Modifica_Fecha = GETDATE(),
                                  Modifica_Usuario = @Usuario
                              WHERE Linea_Id = @Linea_Id";
                connection.Execute(query, new
                {
                    Linea_Id = rango.Linea_Id,
                    Dia_Desde = rango.Dia_Desde,
                    Dia_Hasta = rango.Dia_Hasta,
                    Descripcion = rango.Descripcion,
                    Activo = rango.Activo,
                    Usuario = usuario
                });

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"No Cotiza Rango : {rango.Descripcion} ({rango.Dia_Desde}-{rango.Dia_Hasta})",
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
        /// Elimina un rango sin aportes por su id.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="usuario">Usuario que realiza la operación</param>
        /// <param name="lineaId">Id del rango a eliminar</param>
        /// <returns></returns>
        public ErrorDTO AF_NoCotizaRangos_Eliminar(int CodEmpresa, string usuario, int lineaId)
        {
            var result = new ErrorDTO { Code = 0, Description = "Ok" };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var query = @"DELETE FROM AFI_SOCIOS_SIN_APORTES_RANGOS WHERE Linea_Id = @Linea_Id";
                connection.Execute(query, new { Linea_Id = lineaId });

                _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"No Cotiza Rango : {lineaId}",
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
        /// Obtiene la lista de rangos sin aportes sin paginación, con filtros aplicados.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa</param>
        /// <param name="filtros">Filtros de búsqueda</param>
        /// <returns></returns>
        public ErrorDTO<List<NoCotizaRangosData>> AF_NoCotizaRangos_Exportar(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDTO<List<NoCotizaRangosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<NoCotizaRangosData>()
            };

            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                string where = "";
                if (!string.IsNullOrEmpty(filtros.filtro))
                {
                    where = " WHERE ( " +
                        "CAST(Linea_Id AS VARCHAR) LIKE '%" + filtros.filtro + "%' OR " +
                        "CAST(Dia_Desde AS VARCHAR) LIKE '%" + filtros.filtro + "%' OR " +
                        "CAST(Dia_Hasta AS VARCHAR) LIKE '%" + filtros.filtro + "%' OR " +
                        "Descripcion LIKE '%" + filtros.filtro + "%' OR " +
                        "CONVERT(VARCHAR, Registro_Fecha, 120) LIKE '%" + filtros.filtro + "%' OR " +
                        "Registro_Usuario LIKE '%" + filtros.filtro + "%' ) ";
                }

                var query = $@"SELECT Linea_Id, Dia_Desde, Dia_Hasta, Descripcion, Activo, Registro_Fecha, Registro_Usuario
                               FROM AFI_SOCIOS_SIN_APORTES_RANGOS
                               {where}
                               ORDER BY Dia_Desde ASC";

                result.Result = connection.Query<NoCotizaRangosData>(query).ToList();
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
