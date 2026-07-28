using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.AH;
using Galileo.Models.ERROR;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public class FrmAhReportesRangosFechaDB
    {
        private readonly PortalDB _portalDb;
        private const string Todos = "TODOS";

        public FrmAhReportesRangosFechaDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene los combos y catálogo base requeridos por frmAH_ReportesRangosFecha.
        /// </summary>
        public ErrorDto<FrmAhReportesRangosFechaFiltrosDto> AH_ReportesRangosFecha_Filtros_Obtener(int codEmpresa)
        {
            const string sqlEstados = @"
select
    rtrim(cod_estado) as item,
    rtrim(descripcion) as descripcion
from AFI_ESTADOS_PERSONA
where activo = 1
order by descripcion;";

            const string sqlInstituciones = @"
select
    convert(varchar(20), cod_institucion) as item,
    rtrim(descripcion) as descripcion
from INSTITUCIONES
where activa = 1
order by descripcion;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var response = new FrmAhReportesRangosFechaFiltrosDto
                {
                    estados_persona = AH_ReportesRangosFecha_AgregarTodos(
                        conn.Query<DropDownListaGenericaModel>(sqlEstados).ToList()),
                    instituciones = AH_ReportesRangosFecha_AgregarTodos(
                        conn.Query<DropDownListaGenericaModel>(sqlInstituciones).ToList()),
                    reportes = AH_ReportesRangosFecha_ObtenerCatalogoReportes()
                };

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new FrmAhReportesRangosFechaFiltrosDto());
            }
        }

        /// <summary>
        /// Construye la metadata del reporte equivalente al flujo VB6 de frmAH_ReportesRangosFecha.
        /// </summary>
        public ErrorDto<FrmAhReportesRangosFechaReporteResponse> AH_ReportesRangosFecha_Reporte_Obtener(
            int codEmpresa,
            FrmAhReportesRangosFechaReporteRequest? request)
        {
            var validacion = AH_ReportesRangosFecha_ValidarRequest(request);
            if (validacion.Code < 0)
            {
                return validacion;
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var reporte = AH_ReportesRangosFecha_ResolverReporte(request.codigo_reporte);
                if (reporte == null)
                {
                    return DbHelper.CreateErrorResponse(
                        "El reporte indicado no es válido.",
                        -2,
                        new FrmAhReportesRangosFechaReporteResponse());
                }

                var codEstado = AH_ReportesRangosFecha_NormalizarCodigo(request.cod_estado);
                var codInstitucion = AH_ReportesRangosFecha_NormalizarCodigo(request.cod_institucion);

                var estadoDescripcion = AH_ReportesRangosFecha_ObtenerEstadoDescripcion(conn, codEstado);
                var institucionDescripcion = AH_ReportesRangosFecha_ObtenerInstitucionDescripcion(conn, codInstitucion);

                var filtros = AH_ReportesRangosFecha_ConstruirFiltros(
                    reporte.Value.codigo,
                    codEstado,
                    codInstitucion);

                var response = new FrmAhReportesRangosFechaReporteResponse
                {
                    nombre_reporte = reporte.Value.nombreReporte,
                    titulo = reporte.Value.descripcion,
                    sub_titulo = $"Estados.: {estadoDescripcion}  -  Institucion.: {institucionDescripcion}",
                    filtros = filtros,
                    usuario = (request.usuario ?? string.Empty).Trim().ToUpperInvariant(),
                    empresa = (request.nombre_empresa ?? string.Empty).Trim()
                };

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new FrmAhReportesRangosFechaReporteResponse());
            }
        }

        private static ErrorDto<FrmAhReportesRangosFechaReporteResponse> AH_ReportesRangosFecha_ValidarRequest(
            FrmAhReportesRangosFechaReporteRequest? request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse(
                    "La solicitud es requerida.",
                    -2,
                    new FrmAhReportesRangosFechaReporteResponse());
            }

            if (string.IsNullOrWhiteSpace(request.codigo_reporte))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el reporte.",
                    -2,
                    new FrmAhReportesRangosFechaReporteResponse());
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el usuario.",
                    -2,
                    new FrmAhReportesRangosFechaReporteResponse());
            }

            if (string.IsNullOrWhiteSpace(request.nombre_empresa))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el nombre de la empresa.",
                    -2,
                    new FrmAhReportesRangosFechaReporteResponse());
            }

            return DbHelper.CreateOkResponse(new FrmAhReportesRangosFechaReporteResponse());
        }

        private static List<DropDownListaGenericaModel> AH_ReportesRangosFecha_AgregarTodos(
            List<DropDownListaGenericaModel> lista)
        {
            lista.Insert(0, new DropDownListaGenericaModel
            {
                item = Todos,
                descripcion = Todos
            });

            return lista;
        }

        private static List<FrmAhReportesRangosFechaReporteDto> AH_ReportesRangosFecha_ObtenerCatalogoReportes()
        {
            return
            [
                new FrmAhReportesRangosFechaReporteDto
                {
                    codigo = "1",
                    descripcion = "Informe Consolidado Detallado"
                },
                new FrmAhReportesRangosFechaReporteDto
                {
                    codigo = "2",
                    descripcion = "Resumen por Estado de la Persona"
                },
                new FrmAhReportesRangosFechaReporteDto
                {
                    codigo = "3",
                    descripcion = "Resumen por Institucion"
                },
                new FrmAhReportesRangosFechaReporteDto
                {
                    codigo = "4",
                    descripcion = "Informe de Aporte Patronal en Custodia"
                }
            ];
        }

        private static string AH_ReportesRangosFecha_NormalizarCodigo(string? valor)
        {
            var codigo = (valor ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(codigo) ||
                codigo.Equals(Todos, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            return codigo;
        }

        private static string AH_ReportesRangosFecha_ConstruirFiltros(
            string codigoReporte,
            string codEstado,
            string codInstitucion)
        {
            var filtros = new List<string>();

            filtros.Add("WHERE 1 = 1 ");

            if (!string.IsNullOrWhiteSpace(codEstado))
            {
                filtros.Add($"{{SOCIOS.ESTADOACTUAL}} = '{codEstado}'");
            }

            if (!string.IsNullOrWhiteSpace(codInstitucion) && int.TryParse(codInstitucion, out var institucion))
            {
                filtros.Add($"{{SOCIOS.COD_INSTITUCION}} = {institucion}");
            }

            if (codigoReporte == "4")
            {
                filtros.Add("{vPAT_Consolidado.CUSTODIA} > 0");
            }

            return string.Join(" AND ", filtros);
        }

        private static (string codigo, string descripcion, string nombreReporte)? AH_ReportesRangosFecha_ResolverReporte(
            string codigoReporte)
        {
            return codigoReporte.Trim() switch
            {
                "1" => ("1", "Informe Consolidado Detallado", "Patrimonio_Consolidado"),
                "2" => ("2", "Resumen por Estado de la Persona", "Patrimonio_Consolidado_Estado"),
                "3" => ("3", "Resumen por Institucion", "Patrimonio_Consolidado_Institucion"),
                "4" => ("4", "Informe de Aporte Patronal en Custodia", "Patrimonio_AporteEnCustodia"),
                _ => null
            };
        }

        private static string AH_ReportesRangosFecha_ObtenerEstadoDescripcion(
            System.Data.IDbConnection conn,
            string codEstado)
        {
            if (string.IsNullOrWhiteSpace(codEstado))
            {
                return Todos;
            }

            const string sql = @"
select top 1 rtrim(descripcion)
from AFI_ESTADOS_PERSONA
where cod_estado = @cod_estado;";

            return conn.QueryFirstOrDefault<string>(sql, new { cod_estado = codEstado })?.Trim() ?? Todos;
        }

        private static string AH_ReportesRangosFecha_ObtenerInstitucionDescripcion(
            System.Data.IDbConnection conn,
            string codInstitucion)
        {
            if (string.IsNullOrWhiteSpace(codInstitucion))
            {
                return Todos;
            }

            const string sql = @"
select top 1 rtrim(descripcion)
from INSTITUCIONES
where cod_institucion = @cod_institucion;";

            return conn.QueryFirstOrDefault<string>(sql, new { cod_institucion = codInstitucion })?.Trim() ?? Todos;
        }
    }
}
