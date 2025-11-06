using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using static PgxAPI.Models.ProGrX_Nucleo.FrmSysGestionesBitacoraModels;
namespace PgxAPI.DataBaseTier.ProGrX_Nucleo
{
    public class frmSYS_Gestiones_BitacoraDB
    {
        private readonly IConfiguration? _config;
        public frmSYS_Gestiones_BitacoraDB(IConfiguration? config)
        {
            _config = config;
        }
        /// <summary>
        /// Obtiene una lista de gestiones de bitacora con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cliente_Buscar"></param>
        /// <param name="gestion_Cod"></param>
        /// <param name="usuario_Buscar"></param>
        /// <param name="fecha_Inicio"></param>
        /// <param name="fecha_Fin"></param>
        /// <param name="todasFechas"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<SysGestionesBitacorasLista> Sys_Gestiones_Bitacoras_Lista_Obtener(int CodEmpresa,string cliente_Buscar,string gestion_Cod,string usuario_Buscar,string fecha_Inicio,string fecha_Fin,bool todasFechas,FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<SysGestionesBitacorasLista>
            {
                Code = 0,
                Description = "Ok",
                Result = new SysGestionesBitacorasLista { total = 0, lista = new List<SysGestionesBitacorasData>() }
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);
                string fromSql = " FROM vSys_Bitacora_Operaciones v LEFT JOIN SOCIOS s ON s.CEDULA = v.CEDULA ";

                string where = "";
                if (!todasFechas && !string.IsNullOrWhiteSpace(fecha_Inicio) && !string.IsNullOrWhiteSpace(fecha_Fin))
                {
                    where += (where == "" ? " WHERE " : " AND ")
                          + $"v.[REGISTRO_FECHA] BETWEEN '{fecha_Inicio} 00:00:00' AND '{fecha_Fin} 23:59:59'";
                }

                if (!string.IsNullOrWhiteSpace(usuario_Buscar))
                {
                    var q = usuario_Buscar.Trim().Replace("'", "''");
                    where += (where == "" ? " WHERE " : " AND ") + $"v.[REGISTRO_USUARIO] LIKE '%{q}%'";
                }

                if (!string.IsNullOrWhiteSpace(cliente_Buscar))
                {
                    var q = cliente_Buscar.Trim().Replace("'", "''");
                    where += (where == "" ? " WHERE " : " AND ")
                         + $"(v.[CEDULA] LIKE '%{q}%' OR s.[CEDULAR] LIKE '%{q}%' OR s.[NOMBRE] LIKE '%{q}%')";
                }
                if (!string.IsNullOrWhiteSpace(gestion_Cod) && !gestion_Cod.Equals("TODOS", StringComparison.OrdinalIgnoreCase))
                {
                    var q = gestion_Cod.Trim().Replace("'", "''");
                    where += (where == "" ? " WHERE " : " AND ") + $"v.[COD_GESTION] = '{q}'";
                }
                if (!string.IsNullOrWhiteSpace(filtros?.filtro))
                {
                    var q = filtros.filtro.Trim().Replace("'", "''");
                    where += (where == "" ? " WHERE " : " AND ")
                         + $"(v.[CEDULA] LIKE '%{q}%' OR s.[CEDULAR] LIKE '%{q}%' OR s.[NOMBRE] LIKE '%{q}%' " +
                           $"OR v.[REGISTRO_USUARIO] LIKE '%{q}%' OR v.[DESCRIPCION] LIKE '%{q}%' OR v.[NOTAS] LIKE '%{q}%')";
                }

                string sort = (filtros?.sortField ?? "").Trim().ToLowerInvariant();
                string sortFieldSql = sort switch
                {
                    "identificacion" => "v.[CEDULA]",
                    "nombre" => "s.[NOMBRE]",
                    "fecha" => "v.[REGISTRO_FECHA]",
                    "usuario" => "v.[REGISTRO_USUARIO]",
                    "gestion" => "v.[DESCRIPCION]",
                    "notas" => "v.[NOTAS]",
                    _ => "v.[REGISTRO_FECHA]"
                };
                string sortDir = (filtros?.sortOrder ?? 1) == 0 ? "DESC" : "ASC";

                int pagina = Math.Max(0, filtros?.pagina ?? 0);
                int paginacion = Math.Max(1, filtros?.paginacion ?? 30);
                query = $@"SELECT COUNT(*) {fromSql} {where}";
                result.Result.total = connection.Query<int>(query).FirstOrDefault();

                query = $@"
            SELECT
                v.[CEDULA]            AS Cedula,
                s.[NOMBRE]            AS Nombre,
                v.[REGISTRO_FECHA]    AS Registro_Fecha,
                v.[REGISTRO_USUARIO]  AS Registro_Usuario,
                v.[DESCRIPCION]       AS Descripcion,
                v.[NOTAS]             AS Notas,
                v.[COD_GESTION]       AS Cod_Gestion
            {fromSql}
            {where}
            ORDER BY {sortFieldSql} {sortDir}
            OFFSET {pagina} ROWS
            FETCH NEXT {paginacion} ROWS ONLY";
                result.Result.lista = connection.Query<SysGestionesBitacorasData>(query).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = null;
            }

            return result;
        }

        /// <summary>
        /// Obtiene una lista de gestiones de bitacora sin paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cliente_Buscar"></param>
        /// <param name="gestion_Cod"></param>
        /// <param name="usuario_Buscar"></param>
        /// <param name="fecha_Inicio"></param>
        /// <param name="fecha_Fin"></param>
        /// <param name="todasFechas"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<SysGestionesBitacorasData>> Sys_Gestiones_Bitacoras_Obtener(int CodEmpresa, string cliente_Buscar, string gestion_Cod, string usuario_Buscar, string fecha_Inicio, string fecha_Fin, bool todasFechas, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<SysGestionesBitacorasData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SysGestionesBitacorasData>()
            };

            try
            {
                var query = "";
                using var connection = new SqlConnection(stringConn);

                string fromSql = " FROM vSys_Bitacora_Operaciones v LEFT JOIN SOCIOS s ON s.CEDULA = v.CEDULA ";
                string where = "";

                if (!todasFechas && !string.IsNullOrWhiteSpace(fecha_Inicio) && !string.IsNullOrWhiteSpace(fecha_Fin))
                {
                    where += (where == "" ? " WHERE " : " AND ")
                          + $"v.[REGISTRO_FECHA] BETWEEN '{fecha_Inicio} 00:00:00' AND '{fecha_Fin} 23:59:59'";
                }
                if (!string.IsNullOrWhiteSpace(usuario_Buscar))
                {
                    var q = usuario_Buscar.Trim().Replace("'", "''");
                    where += (where == "" ? " WHERE " : " AND ") + $"v.[REGISTRO_USUARIO] LIKE '%{q}%'";
                }
                if (!string.IsNullOrWhiteSpace(cliente_Buscar))
                {
                    var q = cliente_Buscar.Trim().Replace("'", "''");
                    where += (where == "" ? " WHERE " : " AND ")
                         + $"(v.[CEDULA] LIKE '%{q}%' OR s.[CEDULAR] LIKE '%{q}%' OR s.[NOMBRE] LIKE '%{q}%')";
                }
                if (!string.IsNullOrWhiteSpace(gestion_Cod) && !gestion_Cod.Equals("TODOS", StringComparison.OrdinalIgnoreCase))
                {
                    var q = gestion_Cod.Trim().Replace("'", "''");
                    where += (where == "" ? " WHERE " : " AND ") + $"v.[COD_GESTION] = '{q}'";
                }
                if (!string.IsNullOrWhiteSpace(filtros?.filtro))
                {
                    var q = filtros.filtro.Trim().Replace("'", "''");
                    where += (where == "" ? " WHERE " : " AND ")
                         + $"(v.[CEDULA] LIKE '%{q}%' OR s.[CEDULAR] LIKE '%{q}%' OR s.[NOMBRE] LIKE '%{q}%' " +
                           $"OR v.[REGISTRO_USUARIO] LIKE '%{q}%' OR v.[DESCRIPCION] LIKE '%{q}%' OR v.[NOTAS] LIKE '%{q}%')";
                }

                query = $@"
            SELECT
                v.[CEDULA]            AS Cedula,
                s.[NOMBRE]            AS Nombre,
                v.[REGISTRO_FECHA]    AS Registro_Fecha,
                v.[REGISTRO_USUARIO]  AS Registro_Usuario,
                v.[DESCRIPCION]       AS Descripcion,
                v.[NOTAS]             AS Notas,
                v.[COD_GESTION]       AS Cod_Gestion
            {fromSql}
            {where}
            ORDER BY v.[REGISTRO_FECHA] DESC";
                result.Result = connection.Query<SysGestionesBitacorasData>(query).ToList();
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
        /// Obtiene una lista de gestiones para gestiones de bitacora.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Sys_Gestiones_Tipos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = @"
                    SELECT cod_gestion AS item,
                           RTRIM(descripcion) AS descripcion
                    FROM SYS_GESTIONES_TIPOS
                    WHERE ACTIVA = 1
                    ORDER BY descripcion";

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
        /// Obtiene una lista de socios de gestiones de bitacora con paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<SociosLookupLista> Sys_Socios_Buscar_Lista_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<SociosLookupLista>()
            {
                Code = 0,
                Description = "Ok",
                Result = new SociosLookupLista() { total = 0, lista = new List<SociosLookupData>() }
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                string condicion = "";
                var p = new DynamicParameters();
                if (!string.IsNullOrWhiteSpace(filtros?.filtro))
                {
                    condicion = " WHERE (CEDULA LIKE @filtro_busqueda OR CEDULAR LIKE @filtro_busqueda OR NOMBRE LIKE @filtro_busqueda) ";
                    p.Add("@filtro_busqueda", "%" + filtros.filtro.Trim() + "%");
                }

                // Total
                var query = $@"SELECT COUNT(*) FROM SOCIOS {condicion}";
                result.Result.total = connection.Query<int>(query, p).FirstOrDefault();

                // Orden
                string campoOrden = (filtros?.sortField ?? "").Trim().ToLowerInvariant();
                string ordenSql = (filtros?.sortOrder == 0) ? "DESC" : "ASC";
                string campoOrdenSql = campoOrden switch
                {
                    "cedula" => "CEDULA",
                    "cedular" => "CEDULAR",
                    "nombre" => "NOMBRE",
                    _ => "NOMBRE"
                };

                int offset = Math.Max(0, filtros?.pagina ?? 0);
                int take = Math.Max(1, filtros?.paginacion ?? 30);

                // Lista paginada
                query = $@"
                    SELECT CEDULA, CEDULAR, NOMBRE
                    FROM SOCIOS
                    {condicion}
                    ORDER BY {campoOrdenSql} {ordenSql}
                    OFFSET {offset} ROWS FETCH NEXT {take} ROWS ONLY";
                result.Result.lista = connection.Query<SociosLookupData>(query, p).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result.total = 0;
                result.Result.lista = null;
            }
            return result;
        }
    }
}
