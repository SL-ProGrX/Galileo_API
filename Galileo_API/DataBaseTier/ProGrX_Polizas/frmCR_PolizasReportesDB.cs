using Dapper;
using Galileo.BusinessLogic;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;
using System.Text;
using static Galileo_API.Models.ProGrX_Polizas.FrmCRPolizasReportesModels;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{

    public class FrmCRPolizasReportesDB
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain mProGrxDll;

        public FrmCRPolizasReportesDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            mProGrxDll = new MProGrxMain(config);
        }

        /// <summary>
        /// Consulta de líneas de pólizas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrdPolizasLineaModel>> Cr_PolizasReportes_Lineas_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"
            SELECT
                RTRIM(Codigo) AS Codigo,
                RTRIM(Descripcion) AS Descripcion,
                RTRIM(ISNULL(Poliza, '')) AS Poliza,
                RTRIM(ISNULL(Retencion, '')) AS Retencion
            FROM Catalogo
            ORDER BY Descripcion;";

                return conn.Query<CrdPolizasLineaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Departamentos / Unidad Programática
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cr_PolizasReportes_Departamentos_Obtene(int codEmpresa, string usuario, int codContabilidad)
        {
            var globales = mProGrxDll.sbSifParametrosInicializa(codEmpresa, usuario, codContabilidad)?.Result
                              ?? new Globales();

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var query = globales.SysASEVersion
                    ? @"
                SELECT
                    RTRIM(codigo) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM uprogramatica
                ORDER BY codigo;"
                    : @"
                SELECT
                    RTRIM(cod_departamento) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM AFDepartamentos
                ORDER BY cod_departamento;";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Secciones / Unidad de Trabajo
        ///

        public ErrorDto<List<DropDownListaGenericaModel>> Crd_PolizasReportes_Secciones_Obtener(int codEmpresa, string usuario, int codContabilidad, string? departamentoCodigo)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
            var globales = mProGrxDll.sbSifParametrosInicializa(codEmpresa, usuario, codContabilidad)?.Result
                             ?? new Globales();
            try
            {
                if (globales.SysASEVersion)
                {
                    const string queryAse = @"
                SELECT
                    RTRIM(ut_codigo) AS item,
                    RTRIM(ut_descripcion) AS descripcion
                FROM UTRABAJO
                ORDER BY ut_codigo;";

                    var dataAse = connection.Query<DropDownListaGenericaModel>(queryAse).ToList();
                    return DbHelper.CreateOkResponse(dataAse);
                }

                const string query = @"
            SELECT
                RTRIM(cod_seccion) AS item,
                RTRIM(descripcion) AS descripcion
            FROM AFSecciones
            WHERE (@departamentoCodigo IS NULL OR @departamentoCodigo = '' OR cod_departamento = @departamentoCodigo)
            ORDER BY cod_seccion;";

                var data = connection.Query<DropDownListaGenericaModel>(
                    query,
                    new { departamentoCodigo }).ToList();

                return DbHelper.CreateOkResponse(data);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    "No fue posible obtener las secciones.",
                    -1);
            }
        }

        /// <summary>
        /// Consulta de listadod de personas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrdPolizasReportesSocioModel>> Cr_PolizasReportes_Socios_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"
            SELECT
                RTRIM(ISNULL(cedula, '')) AS Cedula,
                RTRIM(ISNULL(cedular, '')) AS CedulaAlterna,
                RTRIM(ISNULL(nombre, '')) AS Nombre
            FROM SOCIOS
            ORDER BY nombre;";

                return conn.Query<CrdPolizasReportesSocioModel>(query).ToList();
            });
        }

        /// <summary>
        /// Consulta de cantones por provincia
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="provincia"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_PolizasReportes_Cantones_Obtener(int CodEmpresa, string provincia)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"
                        SELECT
                            RTRIM(Canton) AS item,
                            RTRIM(Descripcion) AS descripcion
                        FROM Cantones
                        WHERE Provincia = @provincia
                        ORDER BY Descripcion;";

                return conn.Query<DropDownListaGenericaModel>(query, new { provincia }).ToList();
            });
        }

        /// <summary>
        /// Consulta de distritos por provincia y cantón
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="provincia"></param>
        /// <param name="canton"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_PolizasReportes_Distritos_Obtener(int CodEmpresa, string provincia, string canton)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"
            SELECT
                RTRIM(Distrito) AS item,
                RTRIM(Descripcion) AS descripcion
            FROM Distritos
            WHERE Provincia = @provincia
              AND Canton = @canton
            ORDER BY Descripcion;";

                var data = conn.Query<DropDownListaGenericaModel>(query, new { provincia, canton }).ToList();

                data.Add(new DropDownListaGenericaModel
                {
                    item = string.Empty,
                    descripcion = " "
                });

                return data;
            });
        }

        /// <summary>
        /// Carga de datos iniciales para pantalla de reportes de pólizas (listas desplegables, labels dinámicos, etc)
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<CrdPolizasReportesInicializarResponse> Cr_PolizasReportes_Inicializar(int codEmpresa, string usuario, int codContabilidad)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {

                var globales = mProGrxDll.sbSifParametrosInicializa(codEmpresa, usuario, codContabilidad)?.Result
                               ?? new Globales();

                var response = new CrdPolizasReportesInicializarResponse
                {
                    FechaServidor = DateTime.Today,
                    LabelDepartamento = globales.SysASEVersion ? "Unidad Programatica" : "Departamento",
                    LabelSeccion = globales.SysASEVersion ? "Unidad de Trabajo" : "Sección",
                    Reportes =
                        [
                            new() { CodigoReporte = "R001", Descripcion = "Listado General de Pólizas" },
                            new() { CodigoReporte = "R002", Descripcion = "Movimientos a Póliza por Operación" },
                            new() { CodigoReporte = "R003", Descripcion = "Estado de Pago de Pólizas" }
                        ],
                    FechasTipo =
                        [
                            new() { item = "ENVIO", descripcion = "Envío" },
                            new() { item = "RECEPCION", descripcion = "Recepción" },
                            new() { item = "PAGO", descripcion = "Pago" }
                        ],
                    Sexos =
                        [
                            new() { item = "TODOS", descripcion = "Todos" },
                            new() { item = "M", descripcion = "Masculino" },
                            new() { item = "F", descripcion = "Femenino" }
                        ],
                    EsAseVersion = globales.SysASEVersion,
                    Instituciones = connection.Query<DropDownListaGenericaModel>(@"
            SELECT
                RTRIM(cod_institucion) AS item,
                RTRIM(descripcion) AS descripcion
            FROM instituciones
            ORDER BY descripcion;").ToList(),

                    Nacionalidades = connection.Query<DropDownListaGenericaModel>(@"
                    SELECT
                        RTRIM(cod_nacionalidad) AS item,
                        RTRIM(Descripcion) AS descripcion
                    FROM sys_nacionalidades
                    WHERE Activo = 1
                    ORDER BY Omision DESC, Descripcion ASC;").ToList(),

                    EstadosCiviles = connection.Query<DropDownListaGenericaModel>(@"
            SELECT
                RTRIM(Estado_Civil) AS item,
                RTRIM(Descripcion) AS descripcion
            FROM SYS_ESTADO_CIVIL
            WHERE Activo = 1
            ORDER BY Descripcion ASC;").ToList(),

                    TiposId = connection.Query<DropDownListaGenericaModel>(@"
            SELECT
                RTRIM(TIPO_ID) AS item,
                RTRIM(Descripcion) AS descripcion
            FROM AFI_TIPOS_IDS
            ORDER BY Tipo_Id;").ToList(),

                    Provincias = connection.Query<DropDownListaGenericaModel>(@"
            SELECT
                RTRIM(Provincia) AS item,
                RTRIM(Descripcion) AS descripcion
            FROM Provincias
            ORDER BY Descripcion;").ToList(),

                    Divisas = connection.Query<DropDownListaGenericaModel>(@"
            SELECT
                RTRIM(COD_DIVISA) AS item,
                RTRIM(Descripcion) AS descripcion
            FROM vSys_Divisas
            ORDER BY Descripcion;").ToList(),

                    Polizas = connection.Query<DropDownListaGenericaModel>(@"
            SELECT
                RTRIM(COD_POLIZA) AS item,
                RTRIM(DESCRIPCION) AS descripcion
            FROM CRD_CATALOGO_POLIZAS
            ORDER BY DESCRIPCION;").ToList(),

                    EstadosPersona = connection.Query<DropDownListaGenericaModel>(@"
            SELECT
                RTRIM(COD_ESTADO) AS item,
                RTRIM(Descripcion) AS descripcion
            FROM AFI_ESTADOS_PERSONA
            WHERE Activo = 1
            ORDER BY DESCRIPCION;").ToList(),

                    EstadosLaborales = connection.Query<DropDownListaGenericaModel>(@"
            SELECT
                RTRIM(Estado_Laboral) AS item,
                RTRIM(Descripcion) AS descripcion
            FROM AFI_ESTADO_LABORAL
            WHERE Activo = 1
            ORDER BY Descripcion ASC;").ToList()
                };

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse(
                    "No fue posible inicializar la pantalla de reportes de pólizas.",
                    -1,
                    new CrdPolizasReportesInicializarResponse());
            }
        }

        /// <summary>
        /// construlcción de configuración de reporte (nombre de reporte, fórmula de selección, títulos, etc) a partir de filtros seleccionados por el usuario y metadata de la póliza consultada
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <param name="nombreEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<CrdPolizasReporteConfigResponse> Crd_PolizasReportes_ReporteConfig_Obtener(int CodEmpresa, CrdPolizasReportesRequest request, string usuario, string nombreEmpresa)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            try
            {


                var metadata = ObtenerMetadataPolizaInterna(connection, request.PolizaCodigo);
                var config = ConstruirConfiguracionReporte(request, metadata, usuario, nombreEmpresa, DateTime.Today);

                return DbHelper.CreateOkResponse(config);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse(
                    "No fue posible construir la configuración del reporte de pólizas.",
                    -1,
                    new CrdPolizasReporteConfigResponse());
            }
        }

        /// <summary>
        /// Obtiene metadata de la póliza seleccionada por el usuario, necesaria para construcción de filtros y configuración del reporte
        /// </summary>
        /// <param name="request"></param>
        /// <param name="metadata"></param>
        /// <param name="usuario"></param>
        /// <param name="nombreEmpresa"></param>
        /// <param name="fechaServidor"></param>
        /// <returns></returns>
        private static CrdPolizasReporteConfigResponse ConstruirConfiguracionReporte(CrdPolizasReportesRequest request, CrdPolizaReporteMetadataModel metadata, string usuario, string nombreEmpresa, DateTime fechaServidor)
        {
            var (reporteNombre, reporteTitulo) = ObtenerDatosReporte(request.CodigoReporte, metadata.Prendaria);

            var subTituloBuilder = new StringBuilder();
            var selectionFormula = ConstruirFiltros(request, metadata.CodigoRetencion, subTituloBuilder);
            var subTitulo = subTituloBuilder.ToString();

            if (request.Resumen)
            {
                reporteNombre += "_Resumen";
            }

            return new CrdPolizasReporteConfigResponse
            {
                ReporteNombre = reporteNombre,
                ReporteTitulo = reporteTitulo,
                SubTitulo = subTitulo.Length > 250 ? subTitulo[..250] : subTitulo,
                SelectionFormula = selectionFormula,
                FormulaFecha = $"FECHA: {fechaServidor:dd/MM/yyyy}",
                FormulaEmpresa = nombreEmpresa,
                FormulaUsuario = $"USER: {usuario}",
                FormulaTitulo = reporteTitulo,
                FormulaSubTitulo = subTitulo.Length > 250 ? subTitulo[..250] : subTitulo,
                EsResumen = request.Resumen
            };
        }

        /// <summary>
        /// Construcción de fórmula de selección del reporte a partir de filtros seleccionados por el usuario y metadata de la póliza consultada. Se construyen fórmulas diferentes para reporte de estado de pago (R003) y los reportes generales (R001 y R002), debido a diferencias en las tablas consultadas por cada reporte.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="codigoRetencion"></param>
        /// <param name="subTitulo"></param>
        /// <returns></returns>
        private static string ConstruirFiltros(CrdPolizasReportesRequest request, string codigoRetencion, StringBuilder subTitulo)
        {
            return request.CodigoReporte == CrdPolizasReportesConstantes.ReporteEstadoPago
                ? ConstruirFiltrosEstadoPago(request, codigoRetencion, subTitulo)
                : ConstruirFiltrosGenerales(request, codigoRetencion, subTitulo);
        }

        /// <summary>
        /// Construcción de fórmula de selección para reporte de estado de pago (R003), a partir de filtros seleccionados por el usuario y metadata de la póliza consultada. Este reporte consulta la vista vPolizas_Balance_Estado, que tiene una estructura diferente a las tablas consultadas por los reportes generales, por lo que requiere una construcción de fórmula de selección diferente. Además, este reporte tiene un número limitado de filtros disponibles para el usuario, por lo que la construcción de la fórmula es más sencilla que en los reportes generales.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="codigoRetencion"></param>
        /// <param name="subTitulo"></param>
        /// <returns></returns>
        private static string ConstruirFiltrosEstadoPago(CrdPolizasReportesRequest request, string codigoRetencion, StringBuilder subTitulo)
        {
            var filtros = new List<string>();

            if (!string.Equals(request.PolizaCodigo, CrdPolizasReportesConstantes.Todos, StringComparison.OrdinalIgnoreCase))
            {
                filtros.Add($"vPolizas_Balance_Estado.COD_POLIZA = '{CrdPolizasReportesHelper.EscaparCrystal(codigoRetencion)}'");
            }
            else
            {
                filtros.Add($"vPolizas_Balance_Estado.COD_POLIZA <> '{CrdPolizasReportesHelper.EscaparCrystal(codigoRetencion)}'");
            }

            subTitulo.Append($"Póliza: {request.PolizaCodigo}");

            if (CrdPolizasReportesHelper.TieneValor(request.Cedula))
            {
                filtros.Add($"vPolizas_Balance_Estado.CEDULA = '{CrdPolizasReportesHelper.EscaparCrystal(request.Cedula)}'");
                subTitulo.Append($"¦ Céd.: {request.Cedula}");
            }

            if (request.FiltrarOperacion && request.Operacion.HasValue && request.Operacion.Value > 0)
            {
                filtros.Add($"vPolizas_Balance_Estado.ID_SOLICITUD = {request.Operacion.Value}");
                subTitulo.Append($"¦ Op.: {request.Operacion.Value}");
            }

            if (request.FiltrarProceso && request.FechaProceso.HasValue)
            {
                filtros.Add($"vPolizas_Balance_Estado.FECHA_PROCESO = {request.FechaProceso.Value:yyyyMM}");
                subTitulo.Append($"¦ Proceso.: {request.FechaProceso.Value:yyyy-MM}");
            }

            return string.Join(" AND ", filtros);
        }

        /// <summary>
        /// Construcción de fórmula de selección para reportes generales (R001 y R002), a partir de filtros seleccionados por el usuario y metadata de la póliza consultada. Estos reportes consultan la vista vPoliza_Informe_Main, que tiene una estructura diferente a la vista consultada por el reporte de estado de pago, por lo que requiere una construcción de fórmula de selección diferente. Además, estos reportes tienen un número mayor de filtros disponibles para el usuario, por lo que la construcción de la fórmula es más compleja que en el reporte de estado de pago.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="codigoRetencion"></param>
        /// <param name="subTitulo"></param>
        /// <returns></returns>
        private static string ConstruirFiltrosGenerales(CrdPolizasReportesRequest request, string codigoRetencion, StringBuilder subTitulo)
        {
            var filtros = new List<string>();

            AgregarFiltroPoliza(request, codigoRetencion, filtros, subTitulo);
            AgregarFiltroTexto(request.LineaCodigo, "vPoliza_Informe_Main.ORIGEN_CODIGO", "Línea", filtros, subTitulo);
            AgregarFiltroTexto(request.Cedula, "vPoliza_Informe_Main.CEDULA", "Céd.", filtros, subTitulo);
            AgregarFiltroOperacion(request, filtros, subTitulo);
            AgregarFiltroProceso(request, filtros, subTitulo);

            AgregarFiltroRangoFecha(
                request.FiltrarFechasMovimiento,
                request.FechaMovimientoInicio,
                request.FechaMovimientoFin,
                "vCRD_Polizas_ReporteMovPolizasDetalle.FECHAS",
                "Fec.Mov.",
                filtros,
                subTitulo);

            AgregarFiltroRangoFecha(
                request.FiltrarCoberturaVence,
                request.CoberturaVenceInicio,
                request.CoberturaVenceFin,
                "vPoliza_Informe_Main.COBERTURA_VENCE",
                "Cob.Vence",
                filtros,
                subTitulo);

            AgregarFiltroCatalogo(request.EstadoPersona, "vPoliza_Informe_Main.ESTADOACTUAL", "Est.Per.", filtros, subTitulo);
            AgregarFiltroCatalogo(request.EstadoCivil, "vPoliza_Informe_Main.ESTADOCIVIL", "Est.Civil", filtros, subTitulo);
            AgregarFiltroCatalogo(request.EstadoLaboral, "vPoliza_Informe_Main.ESTADOLABORAL", "Est.Laboral", filtros, subTitulo);
            AgregarFiltroCatalogo(request.Institucion, "vPoliza_Informe_Main.COD_INSTITUCION", "Inst.", filtros, subTitulo);

            AgregarFiltroDepartamento(request, filtros, subTitulo);
            AgregarFiltroSeccion(request, filtros, subTitulo);

            AgregarFiltroCatalogo(request.TipoId, "vPoliza_Informe_Main.TIPO_ID", "Tipo Id.", filtros, subTitulo);
            AgregarFiltroCatalogo(request.Divisa, "vPoliza_Informe_Main.ORIGEN_DIVISA", "Divisa", filtros, subTitulo);
            AgregarFiltroCatalogo(request.Nacionalidad, "vPoliza_Informe_Main.COD_NACIONALIDAD", "Nacional.", filtros, subTitulo);

            AgregarFiltroRangoFecha(
                request.FiltrarNacimiento,
                request.FechaNacimientoInicio,
                request.FechaNacimientoFin,
                "vPoliza_Informe_Main.FECHA_NAC",
                "Fec.Nac.",
                filtros,
                subTitulo);

            AgregarFiltroTextoCondicional(request.FiltrarProvincia, request.Provincia, "vPoliza_Informe_Main.PROVINCIA", "Provincia", filtros, subTitulo);
            AgregarFiltroTextoCondicional(request.FiltrarCanton, request.Canton, "vPoliza_Informe_Main.CANTON", "Cantón", filtros, subTitulo);
            AgregarFiltroTextoCondicional(request.FiltrarDistrito, request.Distrito, "vPoliza_Informe_Main.DISTRITO", "Distrito", filtros, subTitulo);

            AgregarFiltroSexo(request, filtros, subTitulo);

            return string.Join(" AND ", filtros);
        }

        /// <summary>
        /// Agrega filtro de sexo a la fórmula de selección, si el usuario seleccionó un sexo específico (masculino o femenino) en lugar de "todos". Este filtro se construye de manera condicional, ya que el campo de sexo en la vista vPoliza_Informe_Main puede contener valores nulos o vacíos, por lo que no se puede aplicar un filtro directo sin considerar estos casos. La función CrdPolizasReportesHelper.ConstruirFiltroSexoCrystal construye una fórmula de selección que incluye los casos de valores nulos o vacíos, para asegurar que el filtro se aplique correctamente sin excluir registros que podrían ser relevantes para el reporte.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="codigoRetencion"></param>
        /// <param name="filtros"></param>
        /// <param name="subTitulo"></param>
        private static void AgregarFiltroPoliza(
            CrdPolizasReportesRequest request,
            string codigoRetencion,
            List<string> filtros,
            StringBuilder subTitulo)
        {
            var codigo = CrdPolizasReportesHelper.EscaparCrystal(codigoRetencion);

            if (!string.Equals(request.PolizaCodigo, CrdPolizasReportesConstantes.Todos, StringComparison.OrdinalIgnoreCase))
            {
                filtros.Add($"vPoliza_Informe_Main.CODIGO = '{codigo}'");
            }
            else
            {
                filtros.Add($"vPoliza_Informe_Main.CODIGO <> '{codigo}'");
            }

            subTitulo.Append($"Póliza: {request.PolizaCodigo}");
        }

        /// <summary>
        /// Agrega filtro de operación a la fórmula de selección, si el usuario seleccionó filtrar por operación y proporcionó un número de operación válido. Este filtro se aplica directamente sobre el campo ORIGEN_ID de la vista vPoliza_Informe_Main, que corresponde al número de operación asociado a cada movimiento de póliza. Al agregar este filtro, el reporte se limitará a mostrar únicamente los movimientos relacionados con la operación especificada por el usuario, lo que puede ser útil para analizar el comportamiento de pólizas asociadas a operaciones específicas o para realizar auditorías detalladas sobre ciertas transacciones.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filtros"></param>
        /// <param name="subTitulo"></param>
        private static void AgregarFiltroOperacion(
            CrdPolizasReportesRequest request,
            List<string> filtros,
            StringBuilder subTitulo)
        {
            if (!request.FiltrarOperacion || !request.Operacion.HasValue || request.Operacion.Value <= 0)
            {
                return;
            }

            filtros.Add($"vPoliza_Informe_Main.ORIGEN_ID = {request.Operacion.Value}");
            subTitulo.Append($"¦ Op.: {request.Operacion.Value}");
        }

        /// <summary>
        /// Agrega filtro de proceso a la fórmula de selección, si el usuario seleccionó filtrar por proceso y proporcionó una fecha de proceso válida. Este filtro se aplica directamente sobre el campo FECHAP de la vista vCRD_Polizas_ReporteMovPolizasDetalle, que corresponde al período de proceso asociado a cada movimiento de póliza. Al agregar este filtro, el reporte se limitará a mostrar únicamente los movimientos relacionados con el período de proceso especificado por el usuario, lo que puede ser útil para analizar el comportamiento de pólizas durante períodos específicos o para realizar auditorías detalladas sobre transacciones procesadas en ciertos períodos.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filtros"></param>
        /// <param name="subTitulo"></param>
        private static void AgregarFiltroProceso(
            CrdPolizasReportesRequest request,
            List<string> filtros,
            StringBuilder subTitulo)
        {
            if (!request.FiltrarProceso || !request.FechaProceso.HasValue)
            {
                return;
            }

            filtros.Add($"vCRD_Polizas_ReporteMovPolizasDetalle.FECHAP = {request.FechaProceso.Value:yyyyMM}");
            subTitulo.Append($"¦ Proceso.: {request.FechaProceso.Value:yyyy-MM}");
        }

        /// <summary>
        /// Agrega filtro de rango de fechas a la fórmula de selección, si el usuario seleccionó filtrar por un rango de fechas específico y proporcionó fechas de inicio y fin válidas. Este filtro se construye utilizando la función CrdPolizasReportesHelper.ConstruirRangoFechaCrystal, que genera una fórmula de selección compatible con Crystal Reports para filtrar registros dentro del rango de fechas especificado. Al agregar este filtro, el reporte se limitará a mostrar únicamente los movimientos que ocurrieron dentro del rango de fechas seleccionado por el usuario, lo que puede ser útil para analizar el comportamiento de pólizas durante períodos específicos o para realizar auditorías detalladas sobre transacciones ocurridas en ciertos intervalos de tiempo.
        /// </summary>
        /// <param name="aplicarFiltro"></param>
        /// <param name="fechaInicio"></param>
        /// <param name="fechaFin"></param>
        /// <param name="campoCrystal"></param>
        /// <param name="etiqueta"></param>
        /// <param name="filtros"></param>
        /// <param name="subTitulo"></param>
        private static void AgregarFiltroRangoFecha(
            bool aplicarFiltro,
            DateTime? fechaInicio,
            DateTime? fechaFin,
            string campoCrystal,
            string etiqueta,
            List<string> filtros,
            StringBuilder subTitulo)
        {
            if (!aplicarFiltro || !fechaInicio.HasValue || !fechaFin.HasValue)
            {
                return;
            }

            filtros.Add(
                "{" + campoCrystal + "}" +
                CrdPolizasReportesHelper.ConstruirRangoFechaCrystal(fechaInicio.Value, fechaFin.Value));

            subTitulo.Append($"¦ {etiqueta}: {fechaInicio:dd-MM-yyyy} a {fechaFin:dd-MM-yyyy}");
        }

        /// <summary>
        /// Agrega filtro de catálogo a la fórmula de selección, si el usuario seleccionó un valor específico en un filtro de catálogo (por ejemplo, estado civil, estado laboral, institución, etc) en lugar de "todos". Este filtro se construye aplicando una comparación directa entre el campo correspondiente en la vista vPoliza_Informe_Main y el valor seleccionado por el usuario, escapado para evitar problemas con caracteres especiales en Crystal Reports. Al agregar este filtro, el reporte se limitará a mostrar únicamente los movimientos que coincidan con el valor seleccionado por el usuario para ese campo específico, lo que puede ser útil para analizar el comportamiento de pólizas asociadas a ciertas características de los asegurados o para realizar auditorías detalladas sobre transacciones relacionadas con ciertos criterios.
        /// </summary>
        /// <param name="valor"></param>
        /// <param name="campo"></param>
        /// <param name="etiqueta"></param>
        /// <param name="filtros"></param>
        /// <param name="subTitulo"></param>
        private static void AgregarFiltroCatalogo(
            string? valor,
            string campo,
            string etiqueta,
            List<string> filtros,
            StringBuilder subTitulo)
        {
            if (!TieneValorFiltrable(valor))
            {
                return;
            }

            filtros.Add($"{campo} = '{CrdPolizasReportesHelper.EscaparCrystal(valor)}'");
            subTitulo.Append($"¦ {etiqueta}: {valor}");
        }

        /// <summary>
        /// Agrega filtro de texto a la fórmula de selección, si el usuario proporcionó un valor para ese filtro. Este filtro se construye aplicando una comparación directa entre el campo correspondiente en la vista vPoliza_Informe_Main y el valor ingresado por el usuario, escapado para evitar problemas con caracteres especiales en Crystal Reports. Al agregar este filtro, el reporte se limitará a mostrar únicamente los movimientos que coincidan exactamente con el valor ingresado por el usuario para ese campo específico, lo que puede ser útil para analizar el comportamiento de pólizas asociadas a ciertos criterios específicos o para realizar auditorías detalladas sobre transacciones relacionadas con valores particulares.
        /// </summary>
        /// <param name="valor"></param>
        /// <param name="campo"></param>
        /// <param name="etiqueta"></param>
        /// <param name="filtros"></param>
        /// <param name="subTitulo"></param>
        private static void AgregarFiltroTexto(
            string? valor,
            string campo,
            string etiqueta,
            List<string> filtros,
            StringBuilder subTitulo)
        {
            if (!CrdPolizasReportesHelper.TieneValor(valor))
            {
                return;
            }

            filtros.Add($"{campo} = '{CrdPolizasReportesHelper.EscaparCrystal(valor)}'");
            subTitulo.Append($"¦ {etiqueta}: {valor}");
        }

        /// <summary>
        /// Agrega filtro de texto de manera condicional a la fórmula de selección, si el usuario seleccionó aplicar ese filtro y proporcionó un valor para el mismo. Este filtro se construye aplicando una comparación directa entre el campo correspondiente en la vista vPoliza_Informe_Main y el valor ingresado por el usuario, escapado para evitar problemas con caracteres especiales en Crystal Reports. Al agregar este filtro, el reporte se limitará a mostrar únicamente los movimientos que coincidan exactamente con el valor ingresado por el usuario para ese campo específico, lo que puede ser útil para analizar el comportamiento de pólizas asociadas a ciertos criterios específicos o para realizar auditorías detalladas sobre transacciones relacionadas con valores particulares. La diferencia con la función AgregarFiltroTexto es que esta función solo agrega el filtro si el usuario indicó explícitamente que desea aplicar ese filtro, lo que permite una mayor flexibilidad en la construcción de la fórmula de selección del reporte.
        /// </summary>
        /// <param name="aplicarFiltro"></param>
        /// <param name="valor"></param>
        /// <param name="campo"></param>
        /// <param name="etiqueta"></param>
        /// <param name="filtros"></param>
        /// <param name="subTitulo"></param>
        private static void AgregarFiltroTextoCondicional(
            bool aplicarFiltro,
            string? valor,
            string campo,
            string etiqueta,
            List<string> filtros,
            StringBuilder subTitulo)
        {
            if (!aplicarFiltro || !CrdPolizasReportesHelper.TieneValor(valor))
            {
                return;
            }

            filtros.Add($"{campo} = '{CrdPolizasReportesHelper.EscaparCrystal(valor)}'");
            subTitulo.Append($"¦ {etiqueta}: {valor}");
        }


        /// <summary>
        /// Agrega filtro de departamento o unidad programática a la fórmula de selección, dependiendo de la versión del sistema (ASE o no ASE) y si el usuario proporcionó un valor para ese filtro. Este filtro se construye aplicando una comparación directa entre el campo correspondiente en la vista vPoliza_Informe_Main (que puede ser COD_DEPARTAMENTO o UP, dependiendo de la versión del sistema) y el valor ingresado por el usuario, escapado para evitar problemas con caracteres especiales en Crystal Reports. Al agregar este filtro, el reporte se limitará a mostrar únicamente los movimientos que coincidan exactamente con el valor ingresado por el usuario para ese campo específico, lo que puede ser útil para analizar el comportamiento de pólizas asociadas a ciertos departamentos o unidades programáticas, o para realizar auditorías detalladas sobre transacciones relacionadas con esos criterios.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filtros"></param>
        /// <param name="subTitulo"></param>
        private static void AgregarFiltroDepartamento(
            CrdPolizasReportesRequest request,
            List<string> filtros,
            StringBuilder subTitulo)
        {
            if (!CrdPolizasReportesHelper.TieneValor(request.DepartamentoCodigo))
            {
                return;
            }

            var campo = request.EsAseVersion
                ? "{vPoliza_Informe_Main.UP}"
                : "{vPoliza_Informe_Main.COD_DEPARTAMENTO}";

            filtros.Add($"{campo} = '{CrdPolizasReportesHelper.EscaparCrystal(request.DepartamentoCodigo)}'");
            subTitulo.Append(request.EsAseVersion
                ? $"¦ UP: {request.DepartamentoCodigo}"
                : $"¦ Dept: {request.DepartamentoCodigo}");
        }

        /// <summary>
        /// Agrega filtro de sección o unidad de trabajo a la fórmula de selección, dependiendo de la versión del sistema (ASE o no ASE) y si el usuario proporcionó un valor para ese filtro. Este filtro se construye aplicando una comparación directa entre el campo correspondiente en la vista vPoliza_Informe_Main (que puede ser COD_SECCION o UT, dependiendo de la versión del sistema) y el valor ingresado por el usuario, escapado para evitar problemas con caracteres especiales en Crystal Reports. Al agregar este filtro, el reporte se limitará a mostrar únicamente los movimientos que coincidan exactamente con el valor ingresado por el usuario para ese campo específico, lo que puede ser útil para analizar el comportamiento de pólizas asociadas a ciertas secciones o unidades de trabajo, o para realizar auditorías detalladas sobre transacciones relacionadas con esos criterios.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filtros"></param>
        /// <param name="subTitulo"></param>
        private static void AgregarFiltroSeccion(
            CrdPolizasReportesRequest request,
            List<string> filtros,
            StringBuilder subTitulo)
        {
            if (!CrdPolizasReportesHelper.TieneValor(request.SeccionCodigo))
            {
                return;
            }

            var campo = request.EsAseVersion
                ? "{vPoliza_Informe_Main.UT}"
                : "{vPoliza_Informe_Main.COD_SECCION}";

            filtros.Add($"{campo} = '{CrdPolizasReportesHelper.EscaparCrystal(request.SeccionCodigo)}'");
            subTitulo.Append(request.EsAseVersion
                ? $"¦ UT: {request.SeccionCodigo}"
                : $"¦ Sección: {request.SeccionCodigo}");
        }

        /// <summary>
        /// Agrega filtro de sexo a la fórmula de selección, si el usuario seleccionó un sexo específico (masculino o femenino) en lugar de "todos". Este filtro se construye de manera condicional, ya que el campo de sexo en la vista vPoliza_Informe_Main puede contener valores nulos o vacíos, por lo que no se puede aplicar un filtro directo sin considerar estos casos. La función CrdPolizasReportesHelper.ConstruirFiltroSexoCrystal construye una fórmula de selección que incluye los casos de valores nulos o vacíos, para asegurar que el filtro se aplique correctamente sin excluir registros que podrían ser relevantes para el reporte.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filtros"></param>
        /// <param name="subTitulo"></param>
        private static void AgregarFiltroSexo(
            CrdPolizasReportesRequest request,
            List<string> filtros,
            StringBuilder subTitulo)
        {
            if (!TieneValorFiltrable(request.Sexo))
            {
                return;
            }

            var sexo = request.Sexo.Trim().StartsWith("F", StringComparison.OrdinalIgnoreCase) ? "F" : "M";
            filtros.Add($"vPoliza_Informe_Main.SEXO = '{sexo}'");
            subTitulo.Append($"¦ Sexo: {request.Sexo}");
        }

        /// <summary>
        /// Determina si un valor seleccionado por el usuario en un filtro de catálogo es un valor específico que se debe filtrar, o si es el valor "todos" que indica que no se debe aplicar un filtro para ese campo. Esta función verifica que el valor tenga contenido (no sea nulo, vacío o solo espacios) y que no sea igual a "todos" (ignorando mayúsculas/minúsculas). Si el valor es filtrable, se devuelve true para indicar que se debe agregar un filtro a la fórmula de selección del reporte; de lo contrario, se devuelve false para indicar que no se debe aplicar un filtro para ese campo.
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
        private static bool TieneValorFiltrable(string? valor)
        {
            return CrdPolizasReportesHelper.TieneValor(valor) &&
                   !string.Equals(valor, CrdPolizasReportesConstantes.Todos, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Obtiene el nombre del reporte y el título del reporte a partir del código de reporte seleccionado por el usuario y si la póliza es prendaria o no. Esta función utiliza una estructura de switch para determinar el nombre y título del reporte según el código de reporte (R001, R002 o R003) y si la póliza es prendaria (Prendaria = 1) o no (Prendaria = 0). El nombre del reporte se utiliza para cargar el archivo de reporte correspondiente en Crystal Reports, mientras que el título del reporte se muestra en la parte superior del reporte generado para proporcionar contexto al usuario sobre el contenido del mismo.
        /// </summary>
        /// <param name="codigoReporte"></param>
        /// <param name="prendaria"></param>
        /// <returns></returns>
        private static (string ReporteNombre, string ReporteTitulo) ObtenerDatosReporte(string codigoReporte, int prendaria)
        {
            return codigoReporte switch
            {
                CrdPolizasReportesConstantes.ReporteListadoGeneral when prendaria == 1
                    => ("Credito_Polizas_Listado_General_Prendario",
                        "Reporte de Listado General de Movimientos a Pólizas: Especial Prendario"),

                CrdPolizasReportesConstantes.ReporteListadoGeneral
                    => ("Credito_Polizas_Listado_General_Todas",
                        "Reporte de Listado General de Movimientos a Pólizas"),

                CrdPolizasReportesConstantes.ReporteMovimientosOperacion
                    => ("Credito_Polizas_Movimientos",
                        "Reporte de Movimientos a Póliza por Operación"),

                CrdPolizasReportesConstantes.ReporteEstadoPago
                    => ("Credito_Polizas_Pago_Estado",
                        "Reporte de Estado de Envío de Pólizas"),

                _ => (string.Empty, string.Empty)
            };
        }

        /// <summary>
        /// Obtiene metadata de la póliza seleccionada por el usuario, necesaria para construcción de filtros y configuración del reporte. Si el usuario seleccionó "todos" en el filtro de póliza, se devuelve una metadata con valores predeterminados que indican que no se debe aplicar un filtro específico de póliza en la construcción de la fórmula de selección del reporte. Si el usuario seleccionó una póliza específica, se consulta la base de datos para obtener su metadata, incluyendo el código de retención asociado (si existe), si es prendaria o no, y su descripción. Esta información es crucial para construir correctamente los filtros y configurar el reporte según las características de la póliza seleccionada.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="codigoPoliza"></param>
        /// <returns></returns>
        private static CrdPolizaReporteMetadataModel ObtenerMetadataPolizaInterna(IDbConnection connection, string codigoPoliza)
        {
            if (string.Equals(codigoPoliza, CrdPolizasReportesConstantes.Todos, StringComparison.OrdinalIgnoreCase))
            {
                return new CrdPolizaReporteMetadataModel
                {
                    CodigoPoliza = CrdPolizasReportesConstantes.Todos,
                    CodigoRetencion = string.Empty,
                    Prendaria = 0,
                    Descripcion = CrdPolizasReportesConstantes.Todos
                };
            }

            const string query = @"
        SELECT
            RTRIM(P.COD_POLIZA) AS CodigoPoliza,
            RTRIM(P.DESCRIPCION) AS Descripcion,
            RTRIM(ISNULL(P.CODIGO_RETENCION, '')) AS CodigoRetencion,
            CASE
                WHEN ISNULL(Pg.TIPO_APLICACION, 'GEN') IN ('PPC', 'PVID') THEN 1
                ELSE 0
            END AS Prendaria
        FROM CRD_CATALOGO_POLIZAS P
        LEFT JOIN POLIZAS_GRUPO Pg
            ON P.ID_POLIZA_GRUPO = Pg.ID_POLIZA_GRUPO
        WHERE P.COD_POLIZA = @codigoPoliza;";

            return connection.QueryFirstOrDefault<CrdPolizaReporteMetadataModel>(query, new { codigoPoliza })
                   ?? new CrdPolizaReporteMetadataModel();
        }

    }


    public static class CrdPolizasReportesConstantes
    {
        public const string ReporteListadoGeneral = "R001";
        public const string ReporteMovimientosOperacion = "R002";
        public const string ReporteEstadoPago = "R003";

        public const string Todos = "TODOS";
        public const string SexoMasculino = "Masculino";
        public const string SexoFemenino = "Femenino";
    }
    public static class CrdPolizasReportesHelper
    {
        public static string ConstruirRangoFechaCrystal(DateTime inicio, DateTime fin)
        {
            return $" in Date({inicio:yyyy,MM,dd}) to Date({fin:yyyy,MM,dd})";
        }

        public static string EscaparCrystal(string valor)
        {
            return (valor ?? string.Empty).Replace("'", "''");
        }

        public static bool TieneValor(string? valor)
        {
            return !string.IsNullOrWhiteSpace(valor);
        }
    }

}
