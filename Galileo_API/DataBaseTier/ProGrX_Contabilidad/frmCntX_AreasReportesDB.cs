using System.Globalization;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public sealed class FrmCntXAreasReportesDb
    {
        private const string FolderReportes =
            "Contabilidad";

        private const int LongitudMaximaUsuario = 30;

        private readonly PortalDB _portalDb;
        private readonly MCntLinkDB _cntLinkDb;
        private readonly MCntXAreasDb _cntXAreasDb;
        private readonly MProGrxMain _proGrxMain;

        public FrmCntXAreasReportesDb(
            IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _cntLinkDb = new MCntLinkDB(config);
            _cntXAreasDb = new MCntXAreasDb(config);
            _proGrxMain = new MProGrxMain(config);
        }

        /// <summary>
        /// Obtiene la contabilidad enlazada, el período actual y la
        /// máscara contable utilizados por frmCntX_AreasReportes.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <returns>Parámetros contables iniciales.</returns>
        public ErrorDto<DefMascarasDto>
            CntX_frmCntX_AreasReportes_Parametros_Obtener(
                int codEmpresa)
        {
            var response =
                _cntLinkDb.sbgCntParametros(codEmpresa);

            if (response.Code.GetValueOrDefault() < 0 ||
                response.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    response.Description ??
                    "No fue posible obtener los parámetros contables.",
                    response.Code.GetValueOrDefault(-1),
                    new DefMascarasDto());
            }

            if (response.Result.gEnlace <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "La contabilidad enlazada no es válida.",
                    -2,
                    new DefMascarasDto());
            }

            if (!response.Result.gPeriodoAnio.HasValue ||
                !response.Result.gPeriodoMes.HasValue ||
                !CntX_frmCntX_AreasReportes_Periodo_Valido(
                    response.Result.gPeriodoAnio.Value,
                    response.Result.gPeriodoMes.Value))
            {
                return DbHelper.CreateErrorResponse(
                    "El período contable actual no es válido.",
                    -2,
                    new DefMascarasDto());
            }

            return DbHelper.CreateOkResponse(
                response.Result);
        }

        /// <summary>
        /// Obtiene las áreas configuradas en la contabilidad enlazada.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <returns>Lista de áreas ordenada por código.</returns>
        public ErrorDto<List<CntXAreasReportesAreaData>>
            CntX_frmCntX_AreasReportes_Areas_Obtener(
                int codEmpresa)
        {
            var parametros =
                CntX_frmCntX_AreasReportes_Parametros_Obtener(
                    codEmpresa);

            if (parametros.Code.GetValueOrDefault() < 0 ||
                parametros.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    parametros.Description ??
                    "No fue posible obtener los parámetros contables.",
                    parametros.Code.GetValueOrDefault(-1),
                    new List<CntXAreasReportesAreaData>());
            }

            const string sql = """
                select
                    cod_area,
                    rtrim(isnull(descripcion, '')) as descripcion
                from CntX_Area_Definicion
                where cod_contabilidad = @cod_contabilidad
                order by cod_area;
                """;

            return DbHelper
                .ExecuteListQuery<CntXAreasReportesAreaData>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        cod_contabilidad =
                            parametros.Result.gEnlace
                    });
        }

        /// <summary>
        /// Obtiene un área específica de la contabilidad enlazada.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="codArea">
        /// Código del área.
        /// </param>
        /// <returns>Información del área encontrada.</returns>
        public ErrorDto<CntXAreasReportesAreaData?>
            CntX_frmCntX_AreasReportes_Area_Obtener(
                int codEmpresa,
                int codArea)
        {
            if (codArea <= 0)
            {
                return DbHelper.CreateErrorResponse<
                    CntXAreasReportesAreaData?>(
                    "El área es requerida.",
                    -2);
            }

            var parametros =
                CntX_frmCntX_AreasReportes_Parametros_Obtener(
                    codEmpresa);

            if (parametros.Code.GetValueOrDefault() < 0 ||
                parametros.Result is null)
            {
                return DbHelper.CreateErrorResponse<
                    CntXAreasReportesAreaData?>(
                    parametros.Description ??
                    "No fue posible obtener los parámetros contables.",
                    parametros.Code.GetValueOrDefault(-1));
            }

            return CntX_frmCntX_AreasReportes_Area_Consultar(
                codEmpresa,
                parametros.Result.gEnlace,
                codArea);
        }

        /// <summary>
        /// Procesa los saldos temporales del área y devuelve la
        /// configuración necesaria para solicitar el reporte.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="request">
        /// Datos seleccionados para generar el reporte.
        /// </param>
        /// <returns>Configuración y parámetros del reporte.</returns>
        public ErrorDto<CntXAreasReportesGenerarData>
            CntX_frmCntX_AreasReportes_Generar(
                int codEmpresa,
                CntXAreasReportesGenerarRequest? request)
        {
            var mensajeValidacion =
                CntX_frmCntX_AreasReportes_Solicitud_Validar(
                    request);

            if (!string.IsNullOrWhiteSpace(
                    mensajeValidacion))
            {
                return DbHelper.CreateErrorResponse<
                    CntXAreasReportesGenerarData>(
                    mensajeValidacion,
                    -2);
            }

            var solicitud = request!;

            var parametrosResponse =
                CntX_frmCntX_AreasReportes_Parametros_Obtener(
                    codEmpresa);

            if (parametrosResponse.Code.GetValueOrDefault() < 0 ||
                parametrosResponse.Result is null)
            {
                return DbHelper.CreateErrorResponse<
                    CntXAreasReportesGenerarData>(
                    parametrosResponse.Description ??
                    "No fue posible obtener los parámetros contables.",
                    parametrosResponse.Code
                        .GetValueOrDefault(-1));
            }

            var configuracion =
                CntX_frmCntX_AreasReportes_Configuracion_Obtener(
                    solicitud.tipo_reporte,
                    solicitud.comparar);

            if (configuracion is null)
            {
                return DbHelper.CreateErrorResponse<
                    CntXAreasReportesGenerarData>(
                    "El tipo de reporte seleccionado no es válido.",
                    -2);
            }

            var parametrosContables =
                parametrosResponse.Result;

            var areaResponse =
                CntX_frmCntX_AreasReportes_Area_Consultar(
                    codEmpresa,
                    parametrosContables.gEnlace,
                    solicitud.cod_area);

            if (areaResponse.Code.GetValueOrDefault() < 0)
            {
                return DbHelper.CreateErrorResponse<
                    CntXAreasReportesGenerarData>(
                    areaResponse.Description ??
                    "No fue posible consultar el área.",
                    areaResponse.Code.GetValueOrDefault(-1));
            }

            if (areaResponse.Result is null)
            {
                return DbHelper.CreateErrorResponse<
                    CntXAreasReportesGenerarData>(
                    "No se encontró el área indicada.",
                    -2);
            }

            var preparacionResponse =
                CntX_frmCntX_AreasReportes_Preparacion_Ejecutar(
                    codEmpresa,
                    parametrosContables.gEnlace,
                    solicitud);

            if (!CntX_frmCntX_AreasReportes_Operacion_Correcta(
                    preparacionResponse))
            {
                return DbHelper.CreateErrorResponse<
                    CntXAreasReportesGenerarData>(
                    preparacionResponse.Description ??
                    "No fue posible preparar la información del reporte.",
                    preparacionResponse.Code
                        .GetValueOrDefault(-1));
            }

            var utilidadResponse =
                CntX_frmCntX_AreasReportes_Utilidad_Resolver(
                    codEmpresa,
                    parametrosContables.gEnlace,
                    solicitud,
                    configuracion);

            if (utilidadResponse.Code.GetValueOrDefault() < 0 ||
                utilidadResponse.Result is null)
            {
                return DbHelper.CreateErrorResponse<
                    CntXAreasReportesGenerarData>(
                    utilidadResponse.Description ??
                    "No fue posible calcular la utilidad del área.",
                    utilidadResponse.Code
                        .GetValueOrDefault(-1));
            }

            var fechaResponse =
                CntX_frmCntX_AreasReportes_FechaServidor_Obtener(
                    codEmpresa);

            if (fechaResponse.Code.GetValueOrDefault() < 0 ||
                fechaResponse.Result == default)
            {
                return DbHelper.CreateErrorResponse<
                    CntXAreasReportesGenerarData>(
                    fechaResponse.Description ??
                    "No fue posible obtener la fecha del servidor.",
                    fechaResponse.Code
                        .GetValueOrDefault(-1));
            }

            var contexto = new ReporteContexto
            {
                ParametrosContables = parametrosContables,
                Area = areaResponse.Result,
                Configuracion = configuracion,
                Utilidad = utilidadResponse.Result,
                FechaServidor = fechaResponse.Result
            };

            var resultado =
                CntX_frmCntX_AreasReportes_Respuesta_Crear(
                    solicitud,
                    contexto);

            return DbHelper.CreateOkResponse(
                resultado,
                "Reporte preparado correctamente.");
        }

        /// <summary>
        /// Consulta un área utilizando una contabilidad previamente
        /// validada.
        /// </summary>
        private ErrorDto<CntXAreasReportesAreaData?>
            CntX_frmCntX_AreasReportes_Area_Consultar(
                int codEmpresa,
                int codContabilidad,
                int codArea)
        {
            const string sql = """
                select
                    cod_area,
                    rtrim(isnull(descripcion, '')) as descripcion
                from CntX_Area_Definicion
                where cod_contabilidad = @cod_contabilidad
                  and cod_area = @cod_area;
                """;

            return DbHelper
                .ExecuteSingleQuery<CntXAreasReportesAreaData>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    default,
                    new
                    {
                        cod_contabilidad = codContabilidad,
                        cod_area = codArea
                    });
        }

        /// <summary>
        /// Ejecuta, en el orden definido por VB6, la creación del
        /// balance base, comparación opcional y mayorización.
        /// </summary>
        private ErrorDto<bool>
            CntX_frmCntX_AreasReportes_Preparacion_Ejecutar(
                int codEmpresa,
                int codContabilidad,
                CntXAreasReportesGenerarRequest request)
        {
            var usuario = request.usuario.Trim();

            var baseResponse =
                _cntXAreasDb
                    .sbCntX_Areas_Balance_Comprobacion(
                        codEmpresa,
                        codContabilidad,
                        request.periodo_anio,
                        request.periodo_mes,
                        request.cod_area,
                        usuario);

            var baseNormalizada =
                CntX_frmCntX_AreasReportes_Operacion_Normalizar(
                    baseResponse,
                    "No fue posible procesar el período base.");

            if (!CntX_frmCntX_AreasReportes_Operacion_Correcta(
                    baseNormalizada))
            {
                return baseNormalizada;
            }

            if (request.comparar)
            {
                var comparativoResponse =
                    _cntXAreasDb
                        .sbCntX_Areas_Balance_Compara(
                            codEmpresa,
                            codContabilidad,
                            request.periodo_comparativo_anio,
                            request.periodo_comparativo_mes,
                            request.cod_area,
                            usuario);

                var comparativoNormalizado =
                    CntX_frmCntX_AreasReportes_Operacion_Normalizar(
                        comparativoResponse,
                        "No fue posible procesar el período comparativo.");

                if (!CntX_frmCntX_AreasReportes_Operacion_Correcta(
                        comparativoNormalizado))
                {
                    return comparativoNormalizado;
                }
            }

            var mayorizarResponse =
                _cntXAreasDb.sbCntX_Areas_Mayorizar(
                    codEmpresa,
                    codContabilidad,
                    request.cod_area,
                    usuario);

            return CntX_frmCntX_AreasReportes_Operacion_Normalizar(
                mayorizarResponse,
                "No fue posible mayorizar los resultados.");
        }

        /// <summary>
        /// Obtiene la utilidad únicamente para los reportes de
        /// resultados y balance general.
        /// </summary>
        private ErrorDto<CntXAreaUtilidadDto>
            CntX_frmCntX_AreasReportes_Utilidad_Resolver(
                int codEmpresa,
                int codContabilidad,
                CntXAreasReportesGenerarRequest request,
                ReporteConfiguracion configuracion)
        {
            if (!configuracion.RequiereUtilidad)
            {
                return DbHelper.CreateOkResponse(
                    new CntXAreaUtilidadDto());
            }

            return _cntXAreasDb.sbCntX_Areas_Utilidad(
                codEmpresa,
                codContabilidad,
                request.cod_area,
                request.usuario.Trim());
        }

        /// <summary>
        /// Obtiene la fecha definida por el servidor de la empresa.
        /// </summary>
        private ErrorDto<DateTime>
            CntX_frmCntX_AreasReportes_FechaServidor_Obtener(
                int codEmpresa)
        {
            try
            {
                var fecha =
                    _proGrxMain.fxFechaServidor(
                        codEmpresa,
                        0);

                if (fecha == default)
                {
                    return DbHelper.CreateErrorResponse(
                        "No fue posible obtener la fecha del servidor.",
                        -1,
                        default(DateTime));
                }

                return DbHelper.CreateOkResponse(fecha);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    default(DateTime));
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    default(DateTime));
            }
        }

        /// <summary>
        /// Determina si una operación compartida finalizó correctamente.
        /// </summary>
        private static bool
            CntX_frmCntX_AreasReportes_Operacion_Correcta(
                ErrorDto<bool> response)
        {
            return response.Code.GetValueOrDefault() == 0 &&
                   response.Result;
        }

        /// <summary>
        /// Normaliza los errores devueltos por los procesos compartidos.
        /// </summary>
        private static ErrorDto<bool>
            CntX_frmCntX_AreasReportes_Operacion_Normalizar(
                ErrorDto<bool> response,
                string mensajePredeterminado)
        {
            if (CntX_frmCntX_AreasReportes_Operacion_Correcta(
                    response))
            {
                return response;
            }

            var codigo =
                response.Code.GetValueOrDefault(-1);

            if (codigo >= 0)
            {
                codigo = -1;
            }

            return DbHelper.CreateErrorResponse(
                response.Description ??
                mensajePredeterminado,
                codigo,
                false);
        }

        /// <summary>
        /// Valida los datos recibidos antes de ejecutar el proceso.
        /// </summary>
        private static string
            CntX_frmCntX_AreasReportes_Solicitud_Validar(
                CntXAreasReportesGenerarRequest? request)
        {
            if (request is null)
            {
                return "Los datos del reporte son requeridos.";
            }

            if (request.cod_area <= 0)
            {
                return "El área es requerida.";
            }

            if (!CntX_frmCntX_AreasReportes_Periodo_Valido(
                    request.periodo_anio,
                    request.periodo_mes))
            {
                return "El período base no es válido.";
            }

            if (request.comparar &&
                !CntX_frmCntX_AreasReportes_Periodo_Valido(
                    request.periodo_comparativo_anio,
                    request.periodo_comparativo_mes))
            {
                return "El período comparativo no es válido.";
            }

            if (!CntX_frmCntX_AreasReportes_Usuario_Valido(
                    request.usuario))
            {
                return "El usuario indicado no es válido.";
            }

            return CntX_frmCntX_AreasReportes_Configuracion_Obtener(
                       request.tipo_reporte,
                       request.comparar) is null
                ? "El tipo de reporte seleccionado no es válido."
                : string.Empty;
        }

        /// <summary>
        /// Valida un período contable, incluyendo el mes de cierre
        /// fiscal número 13.
        /// </summary>
        private static bool
            CntX_frmCntX_AreasReportes_Periodo_Valido(
                int anio,
                int mes)
        {
            return anio is >= 1900 and <= 9999 &&
                   mes is >= 1 and <= 13;
        }

        /// <summary>
        /// Valida el usuario antes de utilizarlo en los procesos
        /// temporales y en el filtro del reporte.
        /// </summary>
        private static bool
            CntX_frmCntX_AreasReportes_Usuario_Valido(
                string? usuario)
        {
            var valor = (usuario ?? string.Empty).Trim();

            if (valor.Length == 0 ||
                valor.Length > LongitudMaximaUsuario)
            {
                return false;
            }

            foreach (var caracter in valor)
            {
                if (char.IsLetterOrDigit(caracter) ||
                    caracter is '.' or '_' or '-' or '@' or '\\')
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Normaliza el tipo de reporte al código de dos posiciones
        /// utilizado por VB6.
        /// </summary>
        private static string
            CntX_frmCntX_AreasReportes_Tipo_Normalizar(
                string? tipoReporte)
        {
            var tipo =
                (tipoReporte ?? string.Empty).Trim();

            return tipo.Length == 1
                ? tipo.PadLeft(2, '0')
                : tipo;
        }

        /// <summary>
        /// Resuelve el archivo físico y comportamiento del reporte.
        /// </summary>
        private static ReporteConfiguracion?
            CntX_frmCntX_AreasReportes_Configuracion_Obtener(
                string? tipoReporte,
                bool comparar)
        {
            var codigo =
                CntX_frmCntX_AreasReportes_Tipo_Normalizar(
                    tipoReporte);

            return codigo switch
            {
                "01" => new ReporteConfiguracion
                {
                    Titulo =
                        "BALANCE DE COMPROBACIÓN",
                    NombreReporte =
                        "AreaBalanceComprobacion"
                },
                "02" => new ReporteConfiguracion
                {
                    Titulo =
                        "BALANCE DE RESULTADOS",
                    NombreReporte = comparar
                        ? "AreaBalanceResultadosCMP"
                        : "AreaBalanceResultados",
                    RequiereUtilidad = true
                },
                "03" => new ReporteConfiguracion
                {
                    Titulo =
                        "BALANCE GENERAL",
                    NombreReporte = comparar
                        ? "AreaBalanceGeneralCMP"
                        : "AreaBalanceGeneral",
                    RequiereUtilidad = true
                },
                "04" => new ReporteConfiguracion
                {
                    Titulo =
                        "BALANCE DE SITUACIÓN",
                    NombreReporte =
                        "AreaBalanceSituacion"
                },
                "05" => new ReporteConfiguracion
                {
                    Titulo =
                        "COSTOS/GASTOS",
                    NombreReporte = comparar
                        ? "AreaGastosIngresosCMP"
                        : "AreaGastosIngresos",
                    Clasificacion = "G"
                },
                "06" => new ReporteConfiguracion
                {
                    Titulo =
                        "VENTAS/INGRESOS",
                    NombreReporte = comparar
                        ? "AreaGastosIngresosCMP"
                        : "AreaGastosIngresos",
                    Clasificacion = "I"
                },
                _ => null
            };
        }

        /// <summary>
        /// Construye la respuesta mínima que Angular utilizará para
        /// solicitar el reporte.
        /// </summary>
        private static CntXAreasReportesGenerarData
            CntX_frmCntX_AreasReportes_Respuesta_Crear(
                CntXAreasReportesGenerarRequest request,
                ReporteContexto contexto)
        {
            var periodo =
                MCntXCalculosDb.FxCntX_PeriodoDesc(
                    request.periodo_anio,
                    request.periodo_mes);

            var periodoComparativo = request.comparar
                ? MCntXCalculosDb.FxCntX_PeriodoDesc(
                    request.periodo_comparativo_anio,
                    request.periodo_comparativo_mes)
                : string.Empty;

            var subtitulo = request.comparar
                ? $"{periodo} vrs {periodoComparativo}"
                : periodo;

            return new CntXAreasReportesGenerarData
            {
                nombre_reporte =
                    contexto.Configuracion.NombreReporte,
                folder = FolderReportes,
                parametros =
                    CntX_frmCntX_AreasReportes_ParametrosReporte_Crear(
                        request,
                        contexto,
                        subtitulo)
            };
        }

        /// <summary>
        /// Construye los parámetros de selección y fórmulas del reporte.
        /// </summary>
        private static Dictionary<string, object?>
            CntX_frmCntX_AreasReportes_ParametrosReporte_Crear(
                CntXAreasReportesGenerarRequest request,
                ReporteContexto contexto,
                string subtitulo)
        {
            var parametros =
                new Dictionary<string, object?>
                {
                    ["filtros"] =
                        CntX_frmCntX_AreasReportes_Filtro_Crear(
                            request,
                            contexto),
                    ["Fecha"] =
                        contexto.FechaServidor.ToString(
                            "dd/MM/yyyy",
                            CultureInfo.InvariantCulture),
                    ["Empresa"] = null,
                    ["Usuario"] =
                        request.usuario.Trim(),
                    ["Mascara"] =
                        contexto.ParametrosContables
                            .gstrMascara,
                    ["Area"] =
                        contexto.Area.descripcion,
                    ["Titulo"] =
                        contexto.Configuracion.Titulo,
                    ["SubTitulo"] = subtitulo
                };

            if (!contexto.Configuracion.RequiereUtilidad)
            {
                return parametros;
            }

            parametros["UT_MES"] =
                contexto.Utilidad.AreaUTMes;

            parametros["UT_ACUMULADA"] =
                contexto.Utilidad.AreaUTAcumulada;

            if (request.comparar)
            {
                parametros["UTC_MES"] =
                    contexto.Utilidad.AreaUTCMes;

                parametros["UTC_ACUMULADA"] =
                    contexto.Utilidad.AreaUTCAcumulada;
            }

            return parametros;
        }

        /// <summary>
        /// Construye el filtro requerido por los reportes migrados.
        /// Todos los valores se validan antes de incorporarlos.
        /// </summary>
        private static string
            CntX_frmCntX_AreasReportes_Filtro_Crear(
                CntXAreasReportesGenerarRequest request,
                ReporteContexto contexto)
        {
            // El motor de reportes recibe una expresión de selección.
            // El usuario se limita a caracteres permitidos y los códigos
            // utilizados son enteros previamente validados.
            var usuarioSeguro =
                request.usuario
                    .Trim()
                    .Replace(
                        "'",
                        "''",
                        StringComparison.Ordinal);

            var filtro = string.Format(
                CultureInfo.InvariantCulture,
                " WHERE CntX_Acceso_Historico.USUARIO = '{0}'" +
                " AND CntX_Acceso_Historico.cod_contabilidad = {1}" +
                " AND CntX_Acceso_Historico.COD_AREA = {2}",
                usuarioSeguro,
                contexto.ParametrosContables.gEnlace,
                request.cod_area);

            if (!string.IsNullOrWhiteSpace(
                    contexto.Configuracion.Clasificacion))
            {
                filtro += string.Format(
                    CultureInfo.InvariantCulture,
                    " AND CntX_Tipos_Cuentas.CLASIFICACION = '{0}'",
                    contexto.Configuracion.Clasificacion);
            }

            return filtro;
        }

        private sealed class ReporteConfiguracion
        {
            public string Titulo { get; init; } =
                string.Empty;

            public string NombreReporte { get; init; } =
                string.Empty;

            public string Clasificacion { get; init; } =
                string.Empty;

            public bool RequiereUtilidad { get; init; }
        }

        private sealed class ReporteContexto
        {
            public required DefMascarasDto
                ParametrosContables
            {
                get;
                init;
            }

            public required CntXAreasReportesAreaData Area
            {
                get;
                init;
            }

            public required ReporteConfiguracion Configuracion
            {
                get;
                init;
            }

            public required CntXAreaUtilidadDto Utilidad
            {
                get;
                init;
            }

            public DateTime FechaServidor { get; init; }
        }
    }
}