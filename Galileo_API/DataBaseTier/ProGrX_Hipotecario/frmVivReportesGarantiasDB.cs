using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using System.Globalization;
using static Galileo_API.Models.ProGrX_Hipotecario.FrmVivReportesGarantiasModels;

namespace Galileo_API.DataBaseTier.ProGrX_Hipotecario
{
    public class FrmVivReportesGarantiasDB
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain mProGrxDll;

        public FrmVivReportesGarantiasDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            mProGrxDll = new MProGrxMain(config);
        }


        private const string FechaFormatoPantalla = "dd/MM/yyyy";
        private const string FechaHoraFinSql = "yyyy-MM-dd 23:59:59.000";
        private const string WindowTitleReportes = "Reportes Admin Créditos Hipotecarios";

        private static VivReporteGarantiasResponse CrearRespuestaBase(VivReporteGarantiasRequest request)
        {
            return new VivReporteGarantiasResponse
            {
                FechaDesde = request.FechaInicio,
                FechaCorte = request.FechaCorte,
                Empresa = request.NombreEmpresa,
                Fecha = DateTime.Now.ToString(FechaFormatoPantalla, CultureInfo.InvariantCulture),
                Usuario = request.Usuario.ToUpper(),
            };
        }

        /// <summary>
        ///  Contruye metadata para reporte de duración de trámites por fecha.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private static void ConfigurarReporteGarantiasPorFecha(
            VivReporteGarantiasRequest request,
            VivReporteGarantiasResponse response)
        {
            response.Titulo = "Listado de Duración de Trámites";
            response.SubTitulo = "Tiempo en Horas";
            response.Reporte = "Credito_Hipotecario_DuracionGarantias";
            response.SelectionFormula = CrearFiltroFecha(
                "VISTA_ViviendaDuracionGarantiasTotal",
                "RegistroFechaVG",
                request.FechaInicio,
                request.FechaCorte);
        }

        /// <summary>
        /// Construye metadata para reportes por contacto, incluyendo duración de trámites, montos y trámites pendientes.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private static void ConfigurarReportePorContacto(
            VivReporteGarantiasRequest request,
            VivReporteGarantiasResponse response)
        {
            switch (request.OpcionReporte)
            {
                case "DURACION_TRAMITES_CONTACTO":
                    response.Titulo = "Listado de Duración de Trámites";
                    response.SubTitulo = "Duración por Contacto en Horas";
                    response.Reporte = "Credito_Hipotecario_DuracionGarantiasContacto";
                    response.SelectionFormula = CrearFiltroContactoDuracion(request);
                    break; ;

                case "MONTO_CREDITOS_CONTACTO":
                    response.Titulo = "Listado de Montos en Trámites";
                    response.SubTitulo = request.ContactosDetallado
                        ? "Montos por Contacto Detallado"
                        : "Montos por Contacto Resumido";
                    response.Reporte = request.ContactosDetallado
                        ? "Credito_Hipotecario_MontosContactoDet"
                        : "Credito_Hipotecario_MontosContactoRes";
                    response.SelectionFormula = CrearFiltroContactoMontos(request);
                    break;

                case "TRAMITES_PENDIENTES_CONTACTO":
                    response.Titulo = "Listado de Trámites Pendientes";
                    response.SubTitulo = "Trámites por Contacto";
                    response.Reporte = "Credito_Hipotecario_TramitesPendientesContacto";
                    response.SelectionFormula = request.TramitesPendientesTodos
                        ? string.Empty
                        : CrearFiltroContactoPendientes(request);
                    break;
            }
        }

        /// <summary>
        /// Construye metadata para reportes por zona, incluyendo duración de trámites y montos por zona, con opciones detallado y resumido.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private static void ConfigurarReportePorZona(
            VivReporteGarantiasRequest request,
            VivReporteGarantiasResponse response)
        {
            if (request.OpcionReporte == "DURACION_TRAMITES_ZONA")
            {
                response.Titulo = "Listado de Duración de Trámites";
                response.SubTitulo = "Tiempo en Horas por Zona";
                response.Reporte = "Credito_Hipotecario_DuracionGarantiasZona";

                var filtros = new List<string>
        {
            CrearFiltroFecha(
                "VISTA_ViviendaDuracionTramitesZona",
                "RegistroFecha",
                request.FechaInicio,
                request.FechaCorte)
        };

                AgregarFiltroEntero(filtros, "VISTA_ViviendaDuracionTramitesZona", "IdZona", request.IdZona);
                response.SelectionFormula = string.Join(" and ", filtros);
                return;
            }

            response.Titulo = "Listado de Monto de Trámites";
            response.SubTitulo = request.ZonasDetallado
                ? "Montos por Zona Detallado"
                : "Montos por Zona Resumido";
            response.Reporte = request.ZonasDetallado
                ? "Credito_Hipotecario_MontosZonaDet"
                : "Credito_Hipotecario_MontosZonaRes";

            var filtrosMontos = new List<string>
    {
        CrearFiltroFecha(
            "VISTA_ViviendaMontosZonas",
            "RegistroFecha",
            request.FechaInicio,
            request.FechaCorte)
    };

            AgregarFiltroEntero(filtrosMontos, "VISTA_ViviendaMontosZonas", "IdZona", request.IdZona);
            response.SelectionFormula = string.Join(" and ", filtrosMontos);
        }

        /// <summary>
        /// Construye metadata para reportes de desembolsos, incluyendo fechas de desembolso, formalización, disponibles, auxiliares y concluidos.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private static void ConfigurarReporteDesembolsos(
            VivReporteGarantiasRequest request,
            VivReporteGarantiasResponse response)
        {
            switch (request.OpcionReporte)
            {
                case "FECHA_DESEMBOLSO":
                    response.Titulo = "Listado de Desembolsos";
                    response.SubTitulo = "Desembolsos por Fechas de Desembolso";
                    response.Reporte = "Credito_Hipotecario_Desembolsos";
                    response.SelectionFormula = CrearFiltroFecha(
                        "VISTA_ViviendaDesembolsos",
                        "RegistroFecha",
                        request.FechaInicio,
                        request.FechaCorte);
                    break;

                case "FECHA_FORMALIZACION":
                    response.Titulo = "Listado de Desembolso";
                    response.SubTitulo = "Desembolsos por Fechas de Formalización";
                    response.Reporte = "Credito_Hipotecario_Desembolsos";
                    response.SelectionFormula = CrearFiltroFecha(
                        "VISTA_ViviendaDesembolsos",
                        "FechaForp",
                        request.FechaInicio,
                        request.FechaCorte);
                    break;

                case "DESEMBOLSOS_DISPONIBLES":
                    response.Titulo = "Listado de Desembolsos";
                    response.SubTitulo = "Desembolsos Disponibles";
                    response.Reporte = "Credito_Hipotecario_DesembolsosDisponible";
                    response.SelectionFormula = request.IncluirTodos
                        ? string.Empty
                        : CrearFiltroFecha(
                            "VISTA_ViviendaDesembolsoDisponible",
                            "FechaForp",
                            request.FechaInicio,
                            request.FechaCorte);
                    break;

                case "AUXILIAR_DESEMBOLSOS":
                    response.Titulo = "Listado Auxiliar de Desembolsos";
                    response.SubTitulo = "Operaciones con Pendientes al Corte";
                    response.Reporte = "Credito_Hipotecario_AuxiliarDesembolsos";
                    response.StoredProcParams.Add(
                        request.FechaCorte);
                    break;

                case "DESEMBOLSOS_CONCLUIDOS":
                    response.Titulo = "Listado de Desembolsos Concluidos";
                    response.SubTitulo = "Operaciones Disponible en Cero del Periodo";
                    response.Reporte = "Credito_Hipotecario_DesembolsosConcluidos";

                    response.StoredProcParams.Add(
                        request.FechaInicio);
                    response.StoredProcParams.Add(
                        request.FechaCorte);
                    break;
            }
        }

        /// <summary>
        /// Construye metadata para reporte de desembolsos pendientes, con filtros por fecha, tipo de profesional, estado y contacto específico.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        private static void ConfigurarReporteDesembolsosPendientes(
            VivReporteGarantiasRequest request,
            VivReporteGarantiasResponse response)
        {
            response.Titulo = "Listado de Desembolsos";
            response.SubTitulo = "Desembolsos Pendientes";
            response.Reporte = "Credito_Hipotecario_DesembolsosPendientes";

            var filtros = new List<string>
    {
        CrearFiltroFecha(
            "VISTA_ViviendaDesembolsoPendientes",
            "Fecha",
            request.FechaInicio,
            request.FechaCorte)
    };

            AgregarFiltroTexto(filtros, "VISTA_ViviendaDesembolsoPendientes", "TipoProfesional", request.TipoContacto);
            AgregarFiltroTexto(filtros, "VISTA_ViviendaDesembolsoPendientes", "Estado", request.Estado);
            AgregarFiltroEntero(filtros, "VISTA_ViviendaDesembolsoPendientes", "IdContacto", request.IdContacto);

            response.SelectionFormula = string.Join(" and ", filtros);
        }

        /// <summary>
        /// Genera una cadena de filtro SQL para contactos basada en fechas, tipo, contacto y empresa especificados en la solicitud. Este filtro se utiliza para reportes de duración de trámites por contacto.
        /// </summary>       
        private static string CrearFiltroContactoDuracion(VivReporteGarantiasRequest request)
        {
            var filtros = new List<string>
    {
        CrearFiltroFecha(
            "VISTA_ViviendaDuracionTramitesContacto",
            "AsignacionFecha",
            request.FechaInicio,
            request.FechaCorte)
    };

            AgregarFiltroTexto(filtros, "VISTA_ViviendaDuracionTramitesContacto", "IdTipo", request.Tipo);
            AgregarFiltroEntero(filtros, "VISTA_ViviendaDuracionTramitesContacto", "IdContacto", request.IdContacto);
            AgregarFiltroEntero(filtros, "VISTA_ViviendaDuracionTramitesContacto", "IdEmpresa", request.IdEmpresa);

            return string.Join(" and ", filtros);
        }

        /// <summary>
        ///  Genera una cadena de filtro SQL para contactos basada en fechas, tipo, contacto y empresa especificados en la solicitud. Este filtro se utiliza para reportes de montos por contacto, tanto detallado como resumido.
        /// </summary>
        /// <param name="request">Solicitud que contiene los parámetros de filtrado, incluyendo fechas, tipo, contacto y empresa.</param>
        /// <returns>Cadena de filtro SQL combinada con condiciones AND.</returns>
        private static string CrearFiltroContactoMontos(VivReporteGarantiasRequest request)
        {
            var filtros = new List<string>
    {
        CrearFiltroFecha(
            "VISTA_ViviendaMontosContactos",
            "AsignacionFecha",
            request.FechaInicio,
            request.FechaCorte)
    };

            AgregarFiltroTexto(filtros, "VISTA_ViviendaMontosContactos", "TipoProfesional", request.Tipo);
            AgregarFiltroEntero(filtros, "VISTA_ViviendaMontosContactos", "IdContacto", request.IdContacto);
            AgregarFiltroEntero(filtros, "VISTA_ViviendaMontosContactos", "IdEmpresa", request.IdEmpresa);

            return string.Join(" and ", filtros);
        }

        /// <summary>
        /// Genera una cadena de filtro SQL para trámites pendientes por contacto, basada en fechas, tipo, contacto y empresa especificados en la solicitud. Este filtro se utiliza para reportes de trámites pendientes por contacto, a menos que se indique que se deben incluir todos los trámites pendientes, en cuyo caso no se aplica ningún filtro adicional.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string CrearFiltroContactoPendientes(VivReporteGarantiasRequest request)
        {
            var filtros = new List<string>
    {
        CrearFiltroFecha(
            "VISTA_ViviendaTramitesPContacto",
            "AsignacionFecha",
            request.FechaInicio,
            request.FechaCorte)
    };

            AgregarFiltroTexto(filtros, "VISTA_ViviendaTramitesPContacto", "TipoProfesional", request.Tipo);
            AgregarFiltroEntero(filtros, "VISTA_ViviendaTramitesPContacto", "IdContacto", request.IdContacto);
            AgregarFiltroEntero(filtros, "VISTA_ViviendaTramitesPContacto", "IdEmpresa", request.IdEmpresa);

            return string.Join(" and ", filtros);
        }

        /// <summary>
        /// Genera metadata del reporte auxiliar de producción acumulada.
        /// Equivalente VB6: btnProdAcum_Click.
        /// </summary>
        public ErrorDto<VivReporteGarantiasResponse> FrmVivReportesGarantias_ProdAcum_Generar(int CodEmpresa, VivReporteGarantiasProdAcumRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            try
            {
                _ = connection;

                var response = new VivReporteGarantiasResponse
                {
                    Reporte = "Credito_Hipotecario_AuxiliarProdAcum",
                    FechaCorte = request.FechaCorte.ToString(FechaFormatoPantalla, CultureInfo.InvariantCulture),
                    Empresa = request.NombreEmpresa,
                    Titulo = WindowTitleReportes,
                    Fecha = DateTime.Now.ToString(FechaFormatoPantalla, CultureInfo.InvariantCulture),
                    Usuario = request.Usuario.ToUpper(),
                    StoredProcParams =
            {
                request.FechaCorte.ToString(FechaHoraFinSql, CultureInfo.InvariantCulture)
            }
                };

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<VivReporteGarantiasResponse>(
                    "Ocurrió un error al generar el reporte auxiliar de producción acumulada.",
                    -1,
                    null);
            }
        }
        private static string CrearFiltroFecha(
            string vista,
            string campo,
            string fechaInicio,
            string fechaCorte)
        {
            return $"{vista}.{campo} BETWEEN '{fechaInicio}' " +
                   $"and '{fechaCorte}'";
        }

        /// <summary>
        ///  Genera un filtro SQL para un campo entero, agregándolo a la lista de filtros solo si el valor proporcionado es válido (mayor que cero). Este método ayuda a construir dinámicamente la cláusula WHERE de una consulta SQL basada en los parámetros de entrada.
        /// </summary>
        /// <param name="filtros"></param>
        /// <param name="vista"></param>
        /// <param name="campo"></param>
        /// <param name="valor"></param>
        private static void AgregarFiltroEntero(
            List<string> filtros,
            string vista,
            string campo,
            int? valor)
        {
            if (!valor.HasValue || valor.Value <= 0)
            {
                return;
            }

            filtros.Add($"{vista}.{campo} = {valor.Value}");
        }

        /// <summary>
        /// Genera un filtro SQL para un campo de texto, agregándolo a la lista de filtros solo si el valor proporcionado no es nulo, vacío o solo espacios en blanco. El valor se limpia y se escapan las comillas simples para evitar errores de sintaxis en la consulta SQL. Este método ayuda a construir dinámicamente la cláusula WHERE de una consulta SQL basada en los parámetros de entrada.
        /// </summary>
        /// <param name="filtros"></param>
        /// <param name="vista"></param>
        /// <param name="campo"></param>
        /// <param name="valor"></param>
        private static void AgregarFiltroTexto(
            List<string> filtros,
            string vista,
            string campo,
            string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return;
            }

            var valorSeguro = valor.Trim().Replace("'", "''", StringComparison.Ordinal);

            filtros.Add($"{vista}.{campo} = '{valorSeguro}'");
        }
       
        /// <summary>
        /// Obtiene datos para combos de reportes de garantías de vivienda.
        /// Equivalente VB6: sbCargaCbo.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> FrmVivReportesGarantias_Combo_Obtener(int CodEmpresa, string tipo)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                string query = tipo?.Trim().ToUpperInvariant() switch
                {
                    "CONTACTOS" => @"
                SELECT
                    IdContacto AS item,
                    ISNULL(Nombre, '') AS descripcion
                FROM ViviendaContactos
                WHERE TipoContacto <> 'E'
                ORDER BY Nombre",

                    "EMPRESAS" => @"
                SELECT
                    IdEmpresa AS item,
                    ISNULL(Nombre, '') AS descripcion
                FROM ViviendaContactos
                WHERE TipoContacto = 'E'
                ORDER BY Nombre",

                    "ZONAS" => @"
                SELECT
                    IdZona AS item,
                    ISNULL(Descripcion, '') AS descripcion
                FROM ViviendaZonas
                ORDER BY Descripcion",

                    _ => string.Empty
                };


                var lista = conn.Query<DropDownListaGenericaModel>(query).ToList();



                return lista;
            });
        }

        /// <summary>
        /// Genera metadata para reportes de garantías de vivienda.
        /// Equivalente VB6: sbImprimir.
        /// </summary>
        public ErrorDto<VivReporteGarantiasResponse> FrmVivReportesGarantias_Reporte_Generar(int CodEmpresa, VivReporteGarantiasRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            try
            {
                _ = connection;

                var response = CrearRespuestaBase(request);

                switch (request.TabIndex)
                {
                    case 0:
                        ConfigurarReporteGarantiasPorFecha(request, response);
                        break;

                    case 1:
                        ConfigurarReportePorContacto(request, response);
                        break;

                    case 2:
                        ConfigurarReportePorZona(request, response);
                        break;

                    case 3:
                        ConfigurarReporteDesembolsos(request, response);
                        break;

                    case 4:
                        ConfigurarReporteDesembolsosPendientes(request, response);
                        break;

                    default:
                        return DbHelper.CreateErrorResponse<VivReporteGarantiasResponse>(
                            "Tipo de reporte no válido.",
                            -1,
                            null);
                }

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<VivReporteGarantiasResponse>(
                    "Ocurrió un error al generar el reporte de garantías.",
                    -1,
                    null);
            }
        }

    }
}
