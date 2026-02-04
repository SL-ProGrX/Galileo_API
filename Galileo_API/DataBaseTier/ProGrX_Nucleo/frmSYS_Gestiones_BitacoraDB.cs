using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSysGestionesBitacoraDB
    {
        private readonly IConfiguration _config;
        private const string WhereKeyword = " WHERE ";
        private const string AndKeyword = " AND ";

        public FrmSysGestionesBitacoraDB(IConfiguration config)
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
        public ErrorDto<SysGestionesBitacorasLista> Sys_Gestiones_Bitacoras_Lista_Obtener(int CodEmpresa, SysGestionesBitacoraFiltro filtro)
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
                using var connection = new SqlConnection(stringConn);
                string fromSql = " FROM vSys_Bitacora_Operaciones v LEFT JOIN SOCIOS s ON s.CEDULA = v.CEDULA ";
                string where = ConstruirWhereBitacora(filtro);
                (string sortFieldSql, string sortDir) = ObtenerOrdenBitacora(filtro.Filtros ?? new FiltrosLazyLoadData());

                int pagina = Math.Max(0, filtro.Filtros?.pagina ?? 0);
                int paginacion = Math.Max(1, filtro.Filtros?.paginacion ?? 30);

                // Total
                var query = $@"SELECT COUNT(*) {fromSql} {where}";
                result.Result.total = connection.Query<int>(query).FirstOrDefault();

                // Lista paginada
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
        /// Devuelve el campo y dirección de ordenamiento para la bitácora.
        /// </summary>
        private static (string, string) ObtenerOrdenBitacora(FiltrosLazyLoadData filtros)
        {
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
            return (sortFieldSql, sortDir);
        }


        /// <summary>
        /// Filtros para la búsqueda de gestiones de bitácora.
        /// </summary>
        public class SysGestionesBitacoraFiltro
        {
            public string? ClienteBuscar { get; set; }
            public string? GestionCod { get; set; }
            public string? UsuarioBuscar { get; set; }
            public string? FechaInicio { get; set; }
            public string? FechaFin { get; set; }
            public bool TodasFechas { get; set; }
            public FiltrosLazyLoadData? Filtros { get; set; }
        }


        /// <summary>
        /// Construye la cláusula WHERE para la consulta de gestiones de bitácora.
        /// </summary>
        /// <param name="filtro"></param>
        /// <returns></returns>
        private static string ConstruirWhereBitacora(SysGestionesBitacoraFiltro filtro)
        {
            var conditions = new List<string>();

            if (!filtro.TodasFechas && !string.IsNullOrWhiteSpace(filtro.FechaInicio) && !string.IsNullOrWhiteSpace(filtro.FechaFin))
            {
                conditions.Add($"v.[REGISTRO_FECHA] BETWEEN '{filtro.FechaInicio} 00:00:00' AND '{filtro.FechaFin} 23:59:59'");
            }

            AddLikeCondition(conditions, filtro.UsuarioBuscar, "v.[REGISTRO_USUARIO]");
            AddClienteBuscarCondition(conditions, filtro.ClienteBuscar ?? string.Empty);
            AddGestionCodCondition(conditions, filtro.GestionCod ?? string.Empty);

            if (!string.IsNullOrWhiteSpace(filtro.Filtros?.filtro))
            {
                var q = filtro.Filtros.filtro.Trim().Replace("'", "''");
                conditions.Add(
                    $"(v.[CEDULA] LIKE '%{q}%' OR s.[CEDULAR] LIKE '%{q}%' OR s.[NOMBRE] LIKE '%{q}%' " +
                    $"OR v.[REGISTRO_USUARIO] LIKE '%{q}%' OR v.[DESCRIPCION] LIKE '%{q}%' OR v.[NOTAS] LIKE '%{q}%')");
            }

            if (conditions.Count == 0)
                return "";

            return WhereKeyword + string.Join(AndKeyword, conditions);
        }


        /// <summary>
        /// Agrega una condición LIKE a la lista de condiciones si el valor no es nulo o vacío.
        /// </summary>
        /// <param name="conditions"></param>
        /// <param name="value"></param>
        /// <param name="field"></param>
        private static void AddLikeCondition(List<string> conditions, string? value, string field)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                var q = value.Trim().Replace("'", "''");
                conditions.Add($"{field} LIKE '%{q}%'");
            }
        }


        /// <summary>
        /// Agrega una condición de búsqueda para clienteBuscar en cédula, cédular y nombre.
        /// </summary>
        /// <param name="conditions"></param>
        /// <param name="clienteBuscar"></param>
        private static void AddClienteBuscarCondition(List<string> conditions, string clienteBuscar)
        {
            if (!string.IsNullOrWhiteSpace(clienteBuscar))
            {
                var q = clienteBuscar.Trim().Replace("'", "''");
                conditions.Add($"(v.[CEDULA] LIKE '%{q}%' OR s.[CEDULAR] LIKE '%{q}%' OR s.[NOMBRE] LIKE '%{q}%')");
            }
        }


        /// <summary>
        /// Agrega una condición para el código de gestión si no es nulo, vacío o "TODOS".
        /// </summary>
        /// <param name="conditions"></param>
        /// <param name="gestionCod"></param>
        private static void AddGestionCodCondition(List<string> conditions, string gestionCod)
        {
            if (!string.IsNullOrWhiteSpace(gestionCod) && !gestionCod.Equals("TODOS", StringComparison.OrdinalIgnoreCase))
            {
                var q = gestionCod.Trim().Replace("'", "''");
                conditions.Add($"v.[COD_GESTION] = '{q}'");
            }
        }


        /// <summary>
        /// Obtiene una lista de gestiones de bitacora sin paginación y filtros.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<SysGestionesBitacorasData>> Sys_Gestiones_Bitacoras_Obtener(int CodEmpresa, SysGestionesBitacoraFiltro filtro)
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
                string where = ConstruirWhereBitacora(filtro);

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