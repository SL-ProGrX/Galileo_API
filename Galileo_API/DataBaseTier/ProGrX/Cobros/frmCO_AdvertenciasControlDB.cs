using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using System.Data;
using System.Text;
using System.Data.Common;
using Galileo.Models.ProGrX.Cobros;
namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoAdvertenciasControlDB
    {
        private readonly PortalDB _portalDB;


        public FrmCoAdvertenciasControlDB(IConfiguration config)
        {

            _portalDB = new PortalDB(config);

        }

        public ErrorDto<List<DropDownListaGenericaModel>> TiposAdvertiencia_Consultar(int CodEmpresa)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                const string sql = @"
                        SELECT 
                            Cod_Advertencia AS item,
                            Descripcion     AS descripcion
                        FROM CBR_ADVERTENCIAS_TIPO
                        WHERE Activa = 1
                        ORDER BY Cod_Advertencia;";


                var lista = conn.Query<DropDownListaGenericaModel>(sql).ToList();

                return new ErrorDto<List<DropDownListaGenericaModel>>
                {
                    Code = 0,
                    Description = "Ok",
                    Result = lista
                };


            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(ex.Message);
            }

        }



        public ErrorDto<List<DropDownListaGenericaModel>> EstadosPersonas_Consultar(int CodEmpresa)
        {
            const string sql = @"
                SELECT 
                    cod_estado   AS item,
                    descripcion  AS descripcion
                FROM AFI_ESTADOS_PERSONA
                ORDER BY descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDB, CodEmpresa, sql);
        }


        public ErrorDto<CoAdvertenciasControlLista> CoAdvertenciasControlBuscar(
            int CodEmpresa,
            CoAdvertenciasControlFiltros filtros,
            bool esExportar)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                NormalizarEntradas(
                    filtros,
                    out var sortField,
                    out var sortDir,
                    out var offset,
                    out var fetch,
                    out var fechaInicio,
                    out var fechaCorte);


                var p = new DynamicParameters();
                var whereSql = ConstruirWhere(filtros, fechaInicio, fechaCorte, p);


                var fromSql = ConstruirFrom(whereSql);
                var selectSql = ConstruirSelect(fromSql);



                var sqlDatos = new StringBuilder()
                    .Append(selectSql)
                    .Append(FormarOrderBy(sortField, sortDir));

                if (!esExportar)
                {
                    p.Add("@offset", offset);
                    p.Add("@fetch", fetch);
                    sqlDatos.Append(" OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY");
                }

                var sqlCount = new StringBuilder()
                    .Append("SELECT COUNT(1) ")
                    .Append(fromSql);

                if (!string.IsNullOrWhiteSpace(whereSql))
                    sqlCount.Append(' ').Append(whereSql);

                var totalRegistros = conn.ExecuteScalar<int>(sqlCount.ToString(), p);

                var montoTotal = 0m;

                var lista = conn.Query<CoAdvertenciasControlData>(sqlDatos.ToString(), p).ToList();


                return new ErrorDto<CoAdvertenciasControlLista>
                {
                    Code = 0,
                    Description = "OK",
                    Result = new CoAdvertenciasControlLista
                    {
                        totales = new CoAdvertenciasControlTotales
                        {
                            total = totalRegistros,
                            montototal = montoTotal
                        },
                        lista = lista
                    }
                };
            }
            catch (DbException dbEx)
            {

                return DbHelper.CreateErrorResponse<CoAdvertenciasControlLista>(
                    $"Error de base de datos al buscar control de advertencias: {dbEx.Message}"
                );
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CoAdvertenciasControlLista>(
                    $"Error al buscar control de advertencias: {ex.Message}"
                );
            }
        }

        /* ===========================
         *      Helpers privados
         * =========================== */

        private static void NormalizarEntradas(
            CoAdvertenciasControlFiltros filtros,
            out string sortField,
            out string sortDir,
            out int offset,
            out int fetch,
            out DateTime fechaInicio,
            out DateTime fechaCorte)
        {
         
            sortField = string.IsNullOrWhiteSpace(filtros.sortField) ? "Linea" : filtros.sortField;
            var allowedSort = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Linea", "COD_ADVERTENCIA", "cedula", "nombre", "estado",
            "AdvTipo", "FECHA_VENCE", "REGISTRO_FECHA", "REGISTRO_USUARIO",
            "RESOLUCION_FECHA", "RESOLUCION_USUARIO"
        };
            if (!allowedSort.Contains(sortField))
                sortField = "Linea";

            sortDir = filtros.sortOrder == 0 ? "DESC" : "ASC";

            offset = Math.Max(0, filtros.pagina ?? 0);
            fetch = Math.Max(1, filtros.paginacion ?? 50);

            var inicioUser = filtros.fecha_inicio?.Date ?? DateTime.Today;
            var corteUser = filtros.fecha_corte?.Date ?? DateTime.Today;

            fechaCorte = new DateTime(corteUser.Year, corteUser.Month, corteUser.Day, 23, 59, 59, DateTimeKind.Local);
            fechaInicio = new DateTime(inicioUser.Year, inicioUser.Month, inicioUser.Day, 0, 0, 0, DateTimeKind.Local);
        }

        private static string ConstruirWhere(
            CoAdvertenciasControlFiltros filtros,
            DateTime fechaInicio,
            DateTime fechaCorte,
            DynamicParameters p)
        {
            var where = new List<string>();

            p.Add("@cedula", filtros.cedula ?? string.Empty);
            where.Add("Soc.Cedula LIKE '%' + @cedula + '%'");

            if (!string.IsNullOrWhiteSpace(filtros.nombre))
            {
                p.Add("@nombre", filtros.nombre);
                where.Add("Soc.Nombre LIKE '%' + @nombre + '%'");
            }

            if (!string.IsNullOrWhiteSpace(filtros.usuario))
            {
                p.Add("@usuario", filtros.usuario);
                where.Add("Adv.Registro_Usuario LIKE '%' + @usuario + '%'");
            }

            if (!string.IsNullOrWhiteSpace(filtros.lista_advertencias))
                AgregarInList(filtros.lista_advertencias, "@lista_advertencias", "Adv.Cod_Advertencia", where, p);

            if (!string.IsNullOrWhiteSpace(filtros.lista_estados_personas))
                AgregarInList(filtros.lista_estados_personas, "@ListaEstadosP", "Soc.EstadoActual", where, p);

            // Fechas por estado
            p.Add("@fecha_inicio", fechaInicio);
            p.Add("@fecha_corte", fechaCorte);

            where.Add(WherePorEstado(filtros.estado));

            if (!string.IsNullOrWhiteSpace(filtros.filtro))
            {
                p.Add("@q", filtros.filtro);
                where.Add(@"(
                Adv.COD_ADVERTENCIA LIKE '%' + @q + '%'
                OR RTRIM(Adv.CEDULA) LIKE '%' + @q + '%'
                OR RTRIM(Soc.NOMBRE) LIKE '%' + @q + '%'
                OR Tip.DESCRIPCION   LIKE '%' + @q + '%'
                    )");
            }

            return ToWhereClause(where);
        }

        private static void AgregarInList(
            string listaCsv,
            string paramName,
            string columnName,
            List<string> where,
            DynamicParameters p)
        {
            var arr = listaCsv
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Distinct()
                .ToArray();

            if (arr.Length == 0) return;

            p.Add(paramName, arr);
            where.Add($"{columnName} IN {paramName}"); 
        }

        private static string WherePorEstado(string? estado)
        {
       
            return estado switch
            {
                "Activa" => "Adv.Estado = 'A' AND Adv.Registro_Fecha    BETWEEN @fecha_inicio AND @fecha_corte",
                "Resuelta" => "Adv.Estado = 'R' AND Adv.RESOLUCION_FECHA   BETWEEN @fecha_inicio AND @fecha_corte",
                "Descartada" => "Adv.Estado = 'D' AND Adv.RESOLUCION_FECHA   BETWEEN @fecha_inicio AND @fecha_corte",
                _ => "Adv.Registro_Fecha BETWEEN @fecha_inicio AND @fecha_corte"
            };
        }

        private static string ConstruirFrom(string whereSql) => $@"
        FROM CBR_ADVERTENCIAS_CASOS Adv
        INNER JOIN Socios                Soc ON Adv.CEDULA = Soc.Cedula
        INNER JOIN CBR_ADVERTENCIAS_TIPO Tip ON Adv.COD_ADVERTENCIA = Tip.COD_ADVERTENCIA
        INNER JOIN AFI_ESTADOS_PERSONA   Est ON Soc.ESTADOACTUAL = Est.COD_ESTADO
        {whereSql}";

        private static string ConstruirSelect(string fromSql) => $@"
        SELECT
            Adv.Linea,
            Adv.COD_ADVERTENCIA,
            RTRIM(Adv.CEDULA)           AS cedula,
            RTRIM(Soc.NOMBRE)           AS nombre,
            CASE 
                WHEN Adv.ESTADO = 'A' THEN 'Activa'
                WHEN Adv.ESTADO = 'R' THEN 'Resuelta'
                WHEN Adv.ESTADO = 'D' THEN 'Descargada'
                ELSE 'Activa'
            END                          AS estado,
            Tip.DESCRIPCION              AS AdvTipo,
            Adv.FECHA_VENCE,
            Adv.REGISTRO_FECHA,
            Adv.REGISTRO_USUARIO,
            Adv.RESOLUCION_FECHA,
            Adv.RESOLUCION_USUARIO
        {fromSql}";

        private static string FormarOrderBy(string sortField, string sortDir)
            => $" ORDER BY {sortField} {sortDir}";

        private static string ToWhereClause(List<string> conditions)
        {
            return conditions.Count == 0 ? string.Empty : "WHERE " + string.Join(" AND ", conditions);
        }


    }


}