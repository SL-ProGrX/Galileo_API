using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using System.Data;
using System.Text;
using System.Data.Common;
using Galileo.Models.ProGrX.Cobros;
using System.Diagnostics;
using static Org.BouncyCastle.Math.EC.ECCurve;

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
            int codEmpresa,
            CoAdvertenciasControlFiltros filtros,
            bool esExportar)
        {
            // 1) Validaciones rápidas (guard clauses)
            if (filtros is null)
                return DbHelper.CreateErrorResponse<CoAdvertenciasControlLista>("El parámetro 'filtros' es obligatorio.");
            if (!filtros.fecha_inicio.HasValue || !filtros.fecha_corte.HasValue)
                return DbHelper.CreateErrorResponse<CoAdvertenciasControlLista>("El rango de fechas es obligatorio (fecha_inicio/fecha_corte).");

            // 2) Normalizaciones de datos
            NormalizeDates(filtros, out var fi, out var fc);
            var (advertencias, estadosP) = ParseLists(filtros);
            var (sortField, sortOrder) = GetSorting(filtros);
            var (offset, pageSize) = GetPaging(filtros, esExportar);

            // 3) WHERE seguro (parametrizado)
            var whereSql = BuildWhereSql(filtros, advertencias, estadosP);

            // 4) SQL (base + count + list)
            const string selectBase = @"
        SELECT
            Adv.Linea,
            Adv.COD_ADVERTENCIA,
            RTRIM(Adv.CEDULA) AS cedula,
            RTRIM(Soc.NOMBRE) AS nombre,
            CASE
                WHEN Adv.ESTADO = 'A' THEN 'Activa'
                WHEN Adv.ESTADO = 'R' THEN 'Resuelta'
                WHEN Adv.ESTADO = 'D' THEN 'Descargada'
                ELSE 'Activa'
            END AS estado,
            Tip.DESCRIPCION AS AdvTipo,
            Adv.FECHA_VENCE,
            Adv.REGISTRO_FECHA,
            Adv.REGISTRO_USUARIO,
            Adv.RESOLUCION_FECHA,
            Adv.RESOLUCION_USUARIO
        FROM CBR_ADVERTENCIAS_CASOS Adv
        INNER JOIN Socios Soc ON Adv.CEDULA = Soc.Cedula
        INNER JOIN CBR_ADVERTENCIAS_TIPO Tip ON Adv.COD_ADVERTENCIA = Tip.COD_ADVERTENCIA
        INNER JOIN AFI_ESTADOS_PERSONA Est ON Soc.ESTADOACTUAL = Est.COD_ESTADO";

            var orderBySql = $" ORDER BY {sortField} {sortOrder}";
            var pagingSql = esExportar ? string.Empty : " OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

            var listSql = $"{selectBase}{whereSql}{orderBySql}{pagingSql}";
            var countSql = $@"SELECT COUNT(1)
                      FROM CBR_ADVERTENCIAS_CASOS Adv
                      INNER JOIN Socios Soc ON Adv.CEDULA = Soc.Cedula
                      INNER JOIN CBR_ADVERTENCIAS_TIPO Tip ON Adv.COD_ADVERTENCIA = Tip.COD_ADVERTENCIA
                      INNER JOIN AFI_ESTADOS_PERSONA Est ON Soc.ESTADOACTUAL = Est.COD_ESTADO
                      {whereSql}";


            var ctx = new QueryContext(
                Filtros: filtros,
                FechaInicio: fi,
                FechaCorte: fc,
                Advertencias: advertencias,
                EstadosPersona: estadosP,
                EsExportar: esExportar,
                Offset: offset,
                PageSize: pageSize
            );
             
            var p = BuildParameters(ctx);

            // 6) Ejecutar con DbHelper (usa tu PortalDB que depende de _config)


            var totalResp = DbHelper.ExecuteSingleQuery<int>(_portalDB, codEmpresa, countSql, defaultValue: 0, parameters: p);


            var listResp = DbHelper.ExecuteListQuery<CoAdvertenciasControlData>(_portalDB, codEmpresa, listSql, parameters: p);


            // 7) Mapear respuesta final
            var result = new CoAdvertenciasControlLista
            {
                totales = new CoAdvertenciasControlTotales
                {
                    total = totalResp.Result,
                    montototal = 0 // tu SELECT no devuelve 'monto'; ajústalo si corresponde
                },
                lista = listResp.Result ?? new List<CoAdvertenciasControlData>()
            };

            return DbHelper.CreateOkResponse(result);
        }


        private static void NormalizeDates(
            CoAdvertenciasControlFiltros filtros,
            out DateTime fi,
            out DateTime fc)
        {

            fi = DateTime.SpecifyKind(filtros.fecha_inicio!.Value.Date, DateTimeKind.Unspecified);
            fc = DateTime.SpecifyKind(filtros.fecha_corte!.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Unspecified);
        }

        private static (string[] advertencias, string[] estadosP) ParseLists(CoAdvertenciasControlFiltros filtros)
        {
            var advertencias = (filtros.lista_advertencias ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var estadosP = (filtros.lista_estados_personas ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return (advertencias, estadosP);
        }

        private static (string sortField, string sortOrder) GetSorting(CoAdvertenciasControlFiltros filtros)
        {
            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Linea","COD_ADVERTENCIA","cedula","nombre","estado","AdvTipo",
        "FECHA_VENCE","REGISTRO_FECHA","REGISTRO_USUARIO","RESOLUCION_FECHA","RESOLUCION_USUARIO"
    };

            var sortFieldRaw = string.IsNullOrWhiteSpace(filtros.sortField) ? "Linea" : filtros.sortField.Trim();
            var sortField = allowed.Contains(sortFieldRaw) ? sortFieldRaw : "Linea";
            var sortOrder = filtros.sortOrder == 0 ? "DESC" : "ASC";
            return (sortField, sortOrder);
        }

        private static (int offset, int pageSize) GetPaging(CoAdvertenciasControlFiltros filtros, bool esExportar)
        {
            if (esExportar) return (0, 0);
            var pageSize = Math.Max(1, filtros.paginacion.GetValueOrDefault(30));
            var offsetRaw = filtros.pagina.GetValueOrDefault(0);
            var offset = Math.Max(0, offsetRaw);

            return (offset, pageSize);
        }

        private static string BuildWhereSql(CoAdvertenciasControlFiltros filtros, string[] advertencias, string[] estadosP)
        {
            var where = new List<string>();


            if (!string.IsNullOrWhiteSpace(filtros.cedula))
                where.Add("Soc.Cedula LIKE '%' + @cedula + '%'");

            if (!string.IsNullOrWhiteSpace(filtros.nombre))
                where.Add("Soc.Nombre LIKE '%' + @nombre + '%'");
            if (!string.IsNullOrWhiteSpace(filtros.usuario))
                where.Add("Adv.Registro_Usuario LIKE '%' + @usuario + '%'");
            if (!string.IsNullOrWhiteSpace(filtros.filtro))
                where.Add("(Adv.COD_ADVERTENCIA LIKE '%' + @term + '%' OR Soc.Cedula LIKE '%' + @term + '%' OR Soc.Nombre LIKE '%' + @term + '%' OR Tip.DESCRIPCION LIKE '%' + @term + '%')");

            if (advertencias.Length > 0)
                where.Add("Adv.Cod_Advertencia IN @lista_advertencias");
            if (estadosP.Length > 0)
                where.Add("Soc.EstadoActual IN @ListaEstadosP");

            switch (filtros.estado)
            {
                case "Activa":
                    where.Add("Adv.Estado = 'A'");
                    where.Add("Adv.Registro_Fecha BETWEEN @fecha_inicio AND @fecha_corte");
                    break;
                case "Resuelta":
                    where.Add("Adv.Estado = 'R'");
                    where.Add("Adv.RESOLUCION_FECHA BETWEEN @fecha_inicio AND @fecha_corte");
                    break;
                case "Descartada":
                    where.Add("Adv.Estado = 'D'");
                    where.Add("Adv.RESOLUCION_FECHA BETWEEN @fecha_inicio AND @fecha_corte");
                    break;
                default:
                    where.Add("Adv.Registro_Fecha BETWEEN @fecha_inicio AND @fecha_corte");
                    break;
            }

            return " WHERE " + string.Join(" AND ", where) ;
        }

        private sealed record QueryContext(
            CoAdvertenciasControlFiltros Filtros,
            DateTime FechaInicio,
            DateTime FechaCorte,
            string[] Advertencias,
            string[] EstadosPersona,
            bool EsExportar,
            int Offset,
            int PageSize
        );


        private static DynamicParameters BuildParameters(QueryContext ctx)
        {
            var p = new Dapper.DynamicParameters();

            p.Add("@cedula", ctx.Filtros.cedula ?? string.Empty);
            p.Add("@nombre", ctx.Filtros.nombre ?? string.Empty);
            p.Add("@usuario", ctx.Filtros.usuario ?? string.Empty);
            p.Add("@term", ctx.Filtros.filtro ?? string.Empty);

            if (ctx.Advertencias.Length > 0)
                p.Add("@lista_advertencias", ctx.Advertencias);

            if (ctx.EstadosPersona.Length > 0)
                p.Add("@ListaEstadosP", ctx.EstadosPersona);

            p.Add("@fecha_inicio", ctx.FechaInicio);
            p.Add("@fecha_corte", ctx.FechaCorte);

            if (!ctx.EsExportar)
            {
                p.Add("@offset", ctx.Offset);
                p.Add("@pageSize", ctx.PageSize);
            }

            return p;
        }


    }


}