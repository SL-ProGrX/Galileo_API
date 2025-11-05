using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX_Personas;

namespace PgxAPI.DataBaseTier.ProGrX_Personas
{
    public class frmAF_CRGestionesDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 1;
        private readonly mSecurityMainDb _Security_MainDB;

        public frmAF_CRGestionesDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new mSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de gestiones con filtros, orden y paginación.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<List<AF_CRGestionesData>> AF_CRGestiones_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            var result = new ErrorDTO<List<AF_CRGestionesData>>()
            {
                Code = 0,
                Description = "OK",
                Result = new List<AF_CRGestionesData>()
            };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                // Construye el filtro de búsqueda
                string where = "";
                object param = new { };
                if (!string.IsNullOrWhiteSpace(filtros.filtro))
                {
                    where = " WHERE cod_gestion LIKE @filtro OR descripcion LIKE @filtro ";
                    param = new { filtro = $"%{filtros.filtro}%" };
                }

                // Campo de ordenamiento por defecto
                string sortField = string.IsNullOrEmpty(filtros.sortField) ? "cod_gestion" : filtros.sortField;
                // Orden ascendente o descendente (0: DESC, 1: ASC)
                string sortOrder = filtros.sortOrder == 1 ? "ASC" : "DESC";

                // Paginación
                string paginacion = "";
                if (filtros.paginacion > 0)
                {
                    paginacion = $" OFFSET {filtros.pagina} ROWS FETCH NEXT {filtros.paginacion} ROWS ONLY ";
                }

                // Query final con ordenamiento y paginación
                string query = $@"
                    SELECT cod_gestion, descripcion
                    FROM afi_cr_gestiones
                    {where}
                    ORDER BY {sortField} {sortOrder}
                    {paginacion}";

                result.Result = connection.Query<AF_CRGestionesData>(query, param).ToList();
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
        /// Guarda (inserta o actualiza) una gestión.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="gestion"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO AF_CRGestiones_Guardar(int CodEmpresa, AF_CRGestionesData gestion, string usuario)
        {
            var result = new ErrorDTO()
            {
                Code = 0,
                Description = "OK"
            };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);

                var queryExiste = "select count(*) from afi_cr_gestiones where cod_gestion = @cod_gestion";
                var existe = connection.QueryFirstOrDefault<int>(queryExiste, new { cod_gestion = gestion.cod_gestion.ToUpper() });

                if (existe > 0)
                {
                    var queryUpdate = "update afi_cr_gestiones set descripcion = @descripcion where cod_gestion = @cod_gestion";
                    connection.Execute(queryUpdate, new
                    {
                        cod_gestion = gestion.cod_gestion.ToUpper(),
                        descripcion = gestion.descripcion.ToUpper()
                    });
                    result.Description = "Actualizado correctamente";

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Control Renuncia/Gestion: {gestion.cod_gestion} - {gestion.descripcion}",
                        Movimiento = "Modifica - WEB",
                        Modulo = vModulo
                    });
                }
                else
                {
                    var queryInsert = "insert into afi_cr_gestiones(cod_gestion, descripcion) values(@cod_gestion, @descripcion)";
                    connection.Execute(queryInsert, new
                    {
                        cod_gestion = gestion.cod_gestion.ToUpper(),
                        descripcion = gestion.descripcion.ToUpper()
                    });
                    result.Description = "Insertado correctamente";

                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Control Renuncia/Gestion: {gestion.cod_gestion} - {gestion.descripcion}",
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
        /// Elimina una gestión por su código.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cod_gestion"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO AF_CRGestiones_Eliminar(int CodEmpresa, string cod_gestion, string usuario)
        {
            var result = new ErrorDTO()
            {
                Code = 0,
                Description = "OK"
            };
            try
            {
                string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
                using var connection = new SqlConnection(stringConn);
                var queryDelete = "delete afi_cr_gestiones where cod_gestion = @cod_gestion";
                int rows = connection.Execute(queryDelete, new { cod_gestion = cod_gestion.ToUpper() });
                if (rows > 0)
                {
                    result.Description = "Eliminado correctamente";
                    _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Control Renuncia/Gestion: {cod_gestion}",
                        Movimiento = "Elimina - WEB",
                        Modulo = vModulo
                    });
                }
                else
                {
                    result.Code = 1;
                    result.Description = "No se encontró el registro";
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
