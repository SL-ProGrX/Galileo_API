using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Conciliacion;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Conciliacion
{
    public class FrmCcReportesEstudioDB
    {
        private const string ItemTodos = "";
        private const string DescripcionTodos = "TODOS";

        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _proGrxMain;
        private readonly MCntLinkDB _cntLinkDb;

        public FrmCcReportesEstudioDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _proGrxMain = new MProGrxMain(config);
            _cntLinkDb = new MCntLinkDB(config);
        }

        #region Carga inicial

        /// <summary>
        /// Obtiene la información inicial requerida por la pestaña Auxiliares:
        /// empresa, períodos, garantías, divisas, operadoras y grupos de fondos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<CcReportesEstudioAuxiliaresInicialDto>
            CC_ReportesEstudio_Auxiliares_Inicial_Obtener(
                int CodEmpresa,
                int codContabilidad)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(
                    _portalDb,
                    CodEmpresa);

                int contabilidad = ResolverContabilidad(
                    connection,
                    codContabilidad);

                var result = new CcReportesEstudioAuxiliaresInicialDto
                {
                    empresa_nombre_corto = ObtenerNombreCortoEmpresa(connection),
                    periodos = ObtenerPeriodos(connection),
                    garantias = AgregarTodos(ObtenerGarantias(connection)),
                    divisas = AgregarTodos(
                        ObtenerDivisas(connection, contabilidad)),
                    operadoras = ObtenerOperadoras(connection),
                    grupos_fondos = AgregarTodos(
                        ObtenerGruposFondos(connection))
                };

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper
                    .CreateErrorResponse<CcReportesEstudioAuxiliaresInicialDto>(
                        ex.Message,
                        -1,
                        CrearInicialVacio());
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper
                    .CreateErrorResponse<CcReportesEstudioAuxiliaresInicialDto>(
                        ex.Message,
                        -1,
                        CrearInicialVacio());
            }
        }

        /// <summary>
        /// Obtiene los períodos históricos disponibles para los reportes de
        /// conciliación, ordenados del más reciente al más antiguo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CcReportesEstudioPeriodoDto>>
            CC_ReportesEstudio_Periodos_Dropdown_Obtener(int CodEmpresa)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(
                    _portalDb,
                    CodEmpresa);

                return DbHelper.CreateOkResponse(
                    ObtenerPeriodos(connection));
            }
            catch (SqlException ex)
            {
                return DbHelper
                    .CreateErrorResponse<List<CcReportesEstudioPeriodoDto>>(
                        ex.Message,
                        -1,
                        new List<CcReportesEstudioPeriodoDto>());
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper
                    .CreateErrorResponse<List<CcReportesEstudioPeriodoDto>>(
                        ex.Message,
                        -1,
                        new List<CcReportesEstudioPeriodoDto>());
            }
        }

        /// <summary>
        /// Obtiene la información de un período histórico mediante su
        /// identificador de ASE_PER_HISTORICO.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="idPerHistorico"></param>
        /// <returns></returns>
        public ErrorDto<CcReportesEstudioPeriodoData>
            CC_ReportesEstudio_Periodo_Obtener(
                int CodEmpresa,
                int idPerHistorico)
        {
            if (idPerHistorico <= 0)
            {
                return DbHelper
                    .CreateErrorResponse<CcReportesEstudioPeriodoData>(
                        "Debe seleccionar un período válido.",
                        -2,
                        new CcReportesEstudioPeriodoData());
            }

            try
            {
                using var connection = DbHelper.OpenConnection(
                    _portalDb,
                    CodEmpresa);

                var periodo = ObtenerPeriodo(
                    connection,
                    idPerHistorico);

                if (periodo == null)
                {
                    return DbHelper
                        .CreateErrorResponse<CcReportesEstudioPeriodoData>(
                            "El período seleccionado no existe.",
                            -2,
                            new CcReportesEstudioPeriodoData());
                }

                return DbHelper.CreateOkResponse(periodo);
            }
            catch (SqlException ex)
            {
                return DbHelper
                    .CreateErrorResponse<CcReportesEstudioPeriodoData>(
                        ex.Message,
                        -1,
                        new CcReportesEstudioPeriodoData());
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper
                    .CreateErrorResponse<CcReportesEstudioPeriodoData>(
                        ex.Message,
                        -1,
                        new CcReportesEstudioPeriodoData());
            }
        }

        /// <summary>
        /// Obtiene la fecha actual del servidor de la empresa para utilizarla
        /// en los parámetros y encabezados de los reportes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<DateTime>
            CC_ReportesEstudio_FechaServidor_Obtener(int CodEmpresa)
        {
            try
            {
                DateTime fecha = _proGrxMain.fxFechaServidor(
                    CodEmpresa,
                    0);

                return DbHelper.CreateOkResponse(fecha);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<DateTime>(
                    ex.Message,
                    -1,
                    default);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse<DateTime>(
                    ex.Message,
                    -1,
                    default);
            }
        }

        #endregion

        #region Catálogos

        /// <summary>
        /// Obtiene las garantías disponibles para los filtros del auxiliar
        /// de crédito e incluye la opción TODOS.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            CC_ReportesEstudio_Garantias_Dropdown_Obtener(int CodEmpresa)
        {
            return EjecutarCatalogo(
                CodEmpresa,
                connection => AgregarTodos(
                    ObtenerGarantias(connection)));
        }

        /// <summary>
        /// Obtiene las divisas de la contabilidad indicada e incluye la opción
        /// TODOS.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            CC_ReportesEstudio_Divisas_Dropdown_Obtener(
                int CodEmpresa,
                int codContabilidad)
        {
            return EjecutarCatalogo(
                CodEmpresa,
                connection =>
                {
                    int contabilidad = ResolverContabilidad(
                        connection,
                        codContabilidad);

                    return AgregarTodos(
                        ObtenerDivisas(connection, contabilidad));
                });
        }

        /// <summary>
        /// Obtiene las operadoras configuradas para los auxiliares de fondos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            CC_ReportesEstudio_Operadoras_Dropdown_Obtener(int CodEmpresa)
        {
            return EjecutarCatalogo(
                CodEmpresa,
                ObtenerOperadoras);
        }

        /// <summary>
        /// Obtiene los grupos configurados para los auxiliares de fondos e
        /// incluye la opción TODOS.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            CC_ReportesEstudio_GruposFondos_Dropdown_Obtener(int CodEmpresa)
        {
            return EjecutarCatalogo(
                CodEmpresa,
                connection => AgregarTodos(
                    ObtenerGruposFondos(connection)));
        }

        #endregion

        #region Búsquedas F4

        /// <summary>
        /// Obtiene instituciones para el buscador F4, permitiendo buscar por
        /// código o descripción.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="texto"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            CC_ReportesEstudio_Instituciones_Obtener(
                int CodEmpresa,
                string? texto)
        {
            const string sql = """
                select
                    cast(COD_INSTITUCION as varchar(20)) as item,
                    rtrim(DESCRIPCION) as descripcion
                from INSTITUCIONES
                where
                    @Texto = ''
                    or cast(COD_INSTITUCION as varchar(20)) like @Like
                    or DESCRIPCION like @Like
                order by DESCRIPCION;
                """;

            return EjecutarBusqueda(
                CodEmpresa,
                sql,
                texto);
        }

        /// <summary>
        /// Obtiene códigos del catálogo de crédito para el buscador F4,
        /// permitiendo buscar por código o descripción.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="texto"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            CC_ReportesEstudio_Lineas_Obtener(
                int CodEmpresa,
                string? texto)
        {
            const string sql = """
                select
                    rtrim(CODIGO) as item,
                    rtrim(DESCRIPCION) as descripcion
                from CATALOGO
                where
                    @Texto = ''
                    or CODIGO like @Like
                    or DESCRIPCION like @Like
                order by DESCRIPCION;
                """;

            /*
             * El filtro legado:
             *
             * LINEA_INTERNA = 1
             * RETENCION = 'N'
             * POLIZA = 'N'
             *
             * se encuentra comentado en el VB6. Por compatibilidad se conserva
             * la consulta sin ese filtro.
             */

            return EjecutarBusqueda(
                CodEmpresa,
                sql,
                texto);
        }

        /// <summary>
        /// Obtiene destinos de crédito para el buscador F4, permitiendo buscar
        /// por código o descripción.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="texto"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            CC_ReportesEstudio_Destinos_Obtener(
                int CodEmpresa,
                string? texto)
        {
            const string sql = """
                select
                    rtrim(COD_DESTINO) as item,
                    rtrim(DESCRIPCION) as descripcion
                from CATALOGO_DESTINOS
                where
                    @Texto = ''
                    or COD_DESTINO like @Like
                    or DESCRIPCION like @Like
                order by DESCRIPCION;
                """;

            return EjecutarBusqueda(
                CodEmpresa,
                sql,
                texto);
        }

        /// <summary>
        /// Obtiene recursos de crédito para el buscador F4, permitiendo buscar
        /// por código o descripción.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="texto"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            CC_ReportesEstudio_Recursos_Obtener(
                int CodEmpresa,
                string? texto)
        {
            const string sql = """
                select
                    rtrim(COD_GRUPO) as item,
                    rtrim(DESCRIPCION) as descripcion
                from CATALOGO_GRUPOS
                where
                    @Texto = ''
                    or COD_GRUPO like @Like
                    or DESCRIPCION like @Like
                order by DESCRIPCION;
                """;

            return EjecutarBusqueda(
                CodEmpresa,
                sql,
                texto);
        }

        /// <summary>
        /// Obtiene planes de fondos asociados a una operadora para el buscador
        /// F4, permitiendo buscar por código o descripción.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codOperadora"></param>
        /// <param name="texto"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            CC_ReportesEstudio_Planes_Obtener(
                int CodEmpresa,
                int codOperadora,
                string? texto)
        {
            if (codOperadora <= 0)
            {
                return DbHelper
                    .CreateErrorResponse<List<DropDownListaGenericaModel>>(
                        "Debe seleccionar una operadora.",
                        -2,
                        new List<DropDownListaGenericaModel>());
            }

            const string sql = """
                select
                    rtrim(COD_PLAN) as item,
                    rtrim(DESCRIPCION) as descripcion
                from FND_PLANES
                where COD_OPERADORA = @CodOperadora
                  and (
                        @Texto = ''
                        or COD_PLAN like @Like
                        or DESCRIPCION like @Like
                      )
                order by COD_PLAN;
                """;

            string filtro = NormalizarTexto(texto);

            try
            {
                using var connection = DbHelper.OpenConnection(
                    _portalDb,
                    CodEmpresa);

                var lista = connection.Query<DropDownListaGenericaModel>(
                    sql,
                    new
                    {
                        CodOperadora = codOperadora,
                        Texto = filtro,
                        Like = CrearLike(filtro)
                    }).ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper
                    .CreateErrorResponse<List<DropDownListaGenericaModel>>(
                        ex.Message,
                        -1,
                        new List<DropDownListaGenericaModel>());
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper
                    .CreateErrorResponse<List<DropDownListaGenericaModel>>(
                        ex.Message,
                        -1,
                        new List<DropDownListaGenericaModel>());
            }
        }

        /// <summary>
        /// Obtiene la descripción de una cuenta contable utilizando el método
        /// compartido MCntLinkDB.fxgCntCuentaDesc.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cuenta"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<CcReportesEstudioCuentaDto>
            CC_ReportesEstudio_Cuenta_Descripcion_Obtener(
                int CodEmpresa,
                string? cuenta,
                int codContabilidad)
        {
            string cuentaNormalizada = NormalizarTexto(cuenta);

            if (string.IsNullOrWhiteSpace(cuentaNormalizada))
            {
                return DbHelper
                    .CreateErrorResponse<CcReportesEstudioCuentaDto>(
                        "Debe indicar una cuenta contable.",
                        -2,
                        new CcReportesEstudioCuentaDto());
            }

            if (codContabilidad <= 0)
            {
                return DbHelper
                    .CreateErrorResponse<CcReportesEstudioCuentaDto>(
                        "La contabilidad indicada no es válida.",
                        -2,
                        new CcReportesEstudioCuentaDto());
            }

            try
            {
                string descripcion = _cntLinkDb.fxgCntCuentaDesc(
                    CodEmpresa,
                    cuentaNormalizada,
                    codContabilidad);

                if (string.IsNullOrWhiteSpace(descripcion))
                {
                    return DbHelper
                        .CreateErrorResponse<CcReportesEstudioCuentaDto>(
                            "La cuenta contable no existe.",
                            -2,
                            new CcReportesEstudioCuentaDto
                            {
                                cod_cuenta = cuentaNormalizada,
                                cod_contabilidad = codContabilidad
                            });
                }

                var result = new CcReportesEstudioCuentaDto
                {
                    cod_cuenta = cuentaNormalizada,
                    cod_cuenta_mask = cuentaNormalizada,
                    descripcion = descripcion.Trim(),
                    cod_contabilidad = codContabilidad,
                    cod_divisa = string.Empty
                };

                return DbHelper.CreateOkResponse(result);
            }
            catch (SqlException ex)
            {
                return DbHelper
                    .CreateErrorResponse<CcReportesEstudioCuentaDto>(
                        ex.Message,
                        -1,
                        new CcReportesEstudioCuentaDto());
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper
                    .CreateErrorResponse<CcReportesEstudioCuentaDto>(
                        ex.Message,
                        -1,
                        new CcReportesEstudioCuentaDto());
            }
        }

        #endregion

        #region Métodos privados de carga inicial

        private static string ObtenerNombreCortoEmpresa(
            SqlConnection connection)
        {
            const string sql = """
                select top 1
                    rtrim(isnull(PAG_NOMCORTO, '')) as nombre_corto
                from SIF_EMPRESA;
                """;

            return connection.QueryFirstOrDefault<string>(sql)
                   ?? string.Empty;
        }

        private static List<CcReportesEstudioPeriodoDto> ObtenerPeriodos(
            SqlConnection connection)
        {
            const string sql = """
                select
                    ID_PER_HISTORICO as id_per_historico,
                    ANIO as anio,
                    MES as mes
                from ASE_PER_HISTORICO
                order by ANIO desc, MES desc;
                """;

            return connection
                .Query<CcReportesEstudioPeriodoData>(sql)
                .Select(MapearPeriodo)
                .ToList();
        }

        private static CcReportesEstudioPeriodoData? ObtenerPeriodo(
            SqlConnection connection,
            int idPerHistorico)
        {
            const string sql = """
                select
                    ID_PER_HISTORICO as id_per_historico,
                    ANIO as anio,
                    MES as mes
                from ASE_PER_HISTORICO
                where ID_PER_HISTORICO = @IdPerHistorico;
                """;

            var periodo = connection
                .QueryFirstOrDefault<CcReportesEstudioPeriodoData>(
                    sql,
                    new
                    {
                        IdPerHistorico = idPerHistorico
                    });

            if (periodo == null)
            {
                return null;
            }

            periodo.descripcion = ConstruirDescripcionPeriodo(
                periodo.anio,
                periodo.mes);

            return periodo;
        }

        private static CcReportesEstudioPeriodoDto MapearPeriodo(
            CcReportesEstudioPeriodoData periodo)
        {
            return new CcReportesEstudioPeriodoDto
            {
                id_per_historico = periodo.id_per_historico,
                anio = periodo.anio,
                mes = periodo.mes,
                descripcion = ConstruirDescripcionPeriodo(
                    periodo.anio,
                    periodo.mes)
            };
        }

        private static string ConstruirDescripcionPeriodo(
            int anio,
            int mes)
        {
            string descripcionMes = MConciliacionDB.fxConvierteMES(mes);

            return string.IsNullOrWhiteSpace(descripcionMes)
                ? anio.ToString()
                : $"{anio} - {descripcionMes}";
        }

        private static List<DropDownListaGenericaModel> ObtenerGarantias(
            SqlConnection connection)
        {
            const string sql = """
                select
                    rtrim(GARANTIA) as item,
                    rtrim(DESCRIPCION) as descripcion
                from CRD_GARANTIA_TIPOS
                order by DESCRIPCION;
                """;

            return connection
                .Query<DropDownListaGenericaModel>(sql)
                .ToList();
        }

        private static List<DropDownListaGenericaModel> ObtenerDivisas(
            SqlConnection connection,
            int codContabilidad)
        {
            const string sql = """
                select
                    rtrim(COD_DIVISA) as item,
                    rtrim(DESCRIPCION) as descripcion
                from CNTX_DIVISAS
                where COD_CONTABILIDAD = @CodContabilidad
                order by DESCRIPCION;
                """;

            return connection
                .Query<DropDownListaGenericaModel>(
                    sql,
                    new
                    {
                        CodContabilidad = codContabilidad
                    })
                .ToList();
        }

        private static List<DropDownListaGenericaModel> ObtenerOperadoras(
            SqlConnection connection)
        {
            const string sql = """
                select
                    cast(COD_OPERADORA as varchar(20)) as item,
                    rtrim(DESCRIPCION) as descripcion
                from FND_OPERADORAS
                order by DESCRIPCION;
                """;

            return connection
                .Query<DropDownListaGenericaModel>(sql)
                .ToList();
        }

        private static List<DropDownListaGenericaModel> ObtenerGruposFondos(
            SqlConnection connection)
        {
            const string sql = """
                select
                    rtrim(COD_GRUPO) as item,
                    rtrim(DESCRIPCION) as descripcion
                from FND_GRUPOS
                order by DESCRIPCION;
                """;

            return connection
                .Query<DropDownListaGenericaModel>(sql)
                .ToList();
        }

        private static int ResolverContabilidad(
            SqlConnection connection,
            int codContabilidad)
        {
            if (codContabilidad > 0)
            {
                return codContabilidad;
            }

            const string sql = """
                select top 1
                    isnull(COD_EMPRESA_ENLACE, 0)
                from SIF_EMPRESA;
                """;

            int contabilidad = connection
                .QueryFirstOrDefault<int>(sql);

            if (contabilidad <= 0)
            {
                throw new InvalidOperationException(
                    "No fue posible determinar la contabilidad de la empresa.");
            }

            return contabilidad;
        }

        #endregion

        #region Métodos privados comunes

        private ErrorDto<List<DropDownListaGenericaModel>> EjecutarCatalogo(
            int CodEmpresa,
            Func<SqlConnection, List<DropDownListaGenericaModel>> action)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(
                    _portalDb,
                    CodEmpresa);

                return DbHelper.CreateOkResponse(
                    action(connection));
            }
            catch (SqlException ex)
            {
                return DbHelper
                    .CreateErrorResponse<List<DropDownListaGenericaModel>>(
                        ex.Message,
                        -1,
                        new List<DropDownListaGenericaModel>());
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper
                    .CreateErrorResponse<List<DropDownListaGenericaModel>>(
                        ex.Message,
                        -1,
                        new List<DropDownListaGenericaModel>());
            }
        }

        private ErrorDto<List<DropDownListaGenericaModel>> EjecutarBusqueda(
            int CodEmpresa,
            string sql,
            string? texto)
        {
            string filtro = NormalizarTexto(texto);

            try
            {
                using var connection = DbHelper.OpenConnection(
                    _portalDb,
                    CodEmpresa);

                var lista = connection
                    .Query<DropDownListaGenericaModel>(
                        sql,
                        new
                        {
                            Texto = filtro,
                            Like = CrearLike(filtro)
                        })
                    .ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper
                    .CreateErrorResponse<List<DropDownListaGenericaModel>>(
                        ex.Message,
                        -1,
                        new List<DropDownListaGenericaModel>());
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper
                    .CreateErrorResponse<List<DropDownListaGenericaModel>>(
                        ex.Message,
                        -1,
                        new List<DropDownListaGenericaModel>());
            }
        }

        private static List<DropDownListaGenericaModel> AgregarTodos(
            IEnumerable<DropDownListaGenericaModel>? lista)
        {
            var resultado = new List<DropDownListaGenericaModel>
            {
                new()
                {
                    item = ItemTodos,
                    descripcion = DescripcionTodos
                }
            };

            if (lista != null)
            {
                resultado.AddRange(lista);
            }

            return resultado;
        }

        private static string NormalizarTexto(string? texto)
        {
            return (texto ?? string.Empty).Trim();
        }

        private static string? CrearLike(string texto)
        {
            return string.IsNullOrWhiteSpace(texto)
                ? null
                : $"%{texto}%";
        }

        private static CcReportesEstudioAuxiliaresInicialDto CrearInicialVacio()
        {
            return new CcReportesEstudioAuxiliaresInicialDto
            {
                empresa_nombre_corto = string.Empty,
                periodos = new List<CcReportesEstudioPeriodoDto>(),
                garantias = new List<DropDownListaGenericaModel>(),
                divisas = new List<DropDownListaGenericaModel>(),
                operadoras = new List<DropDownListaGenericaModel>(),
                grupos_fondos = new List<DropDownListaGenericaModel>()
            };
        }

        #endregion
        #region Generación de auxiliares

        private const string TipoPatrimonio = "P";
        private const string TipoFondos = "F";
        private const string TipoCredito = "C";
        private const string FolderConciliacion = "Conciliacion";
        private const string CodigoReporte = "P";
        private const string ParametroFiltros = "filtros";
        private const string ParametroEmpresa = "EMPRESA";
        private const string ParametroFecha = "FECHA";
        private const string ParametroUsuario = "USUARIO";
        private const string ParametroSubtitulo = "SUBTITULO";
        private const string ParametroMascara = "MASCARA";
        private const string ParametroTituloMayuscula = "TITULO";
        private const string ParametroTitulo = "Titulo";

        /// <summary>
        /// Genera la configuración requerida para ejecutar un reporte auxiliar de patrimonio, fondos o crédito.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CcReportesEstudioAuxiliarGenerarResult> CC_ReportesEstudio_Auxiliar_Generar(int CodEmpresa,CcReportesEstudioAuxiliarGenerarRequest? request)
        {
            if (request == null)
            {
                return CrearErrorGeneracion(
                    "No se recibió la información para generar el auxiliar.",
                    -2);
            }

            string validacion = ValidarAuxiliarGenerarRequest(request);

            if (!string.IsNullOrWhiteSpace(validacion))
            {
                return CrearErrorGeneracion(
                    validacion,
                    -2);
            }

            try
            {
                using var connection = DbHelper.OpenConnection(
                    _portalDb,
                    CodEmpresa);

                int idPerHistorico = request.id_per_historico ?? 0;

                var periodo = ObtenerPeriodo(
                    connection,
                    idPerHistorico);

                if (periodo == null)
                {
                    return CrearErrorGeneracion(
                        "El período seleccionado no existe.",
                        -2);
                }

                DateTime fechaServidor = _proGrxMain.fxFechaServidor(
                    CodEmpresa,
                    0);

                string tipoAuxiliar = NormalizarTexto(
                    request.tipo_auxiliar).ToUpperInvariant();

                return tipoAuxiliar switch
                {
                    TipoPatrimonio => GenerarAuxiliarPatrimonio(
                        request,
                        periodo,
                        fechaServidor),

                    TipoFondos => GenerarAuxiliarFondos(
                        request,
                        periodo,
                        fechaServidor),

                    TipoCredito => GenerarAuxiliarCredito(
                        request,
                        periodo,
                        fechaServidor),

                    _ => CrearErrorGeneracion(
                        "El tipo de auxiliar indicado no es válido.",
                        -2)
                };
            }
            catch (SqlException ex)
            {
                return CrearErrorGeneracion(
                    ex.Message,
                    -1);
            }
            catch (InvalidOperationException ex)
            {
                return CrearErrorGeneracion(
                    ex.Message,
                    -1);
            }
        }

        /// <summary>
        /// Genera la configuración de los reportes auxiliares de patrimonio.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="periodo"></param>
        /// <param name="fechaServidor"></param>
        /// <returns></returns>
        private static ErrorDto<CcReportesEstudioAuxiliarGenerarResult>GenerarAuxiliarPatrimonio( CcReportesEstudioAuxiliarGenerarRequest request, CcReportesEstudioPeriodoData periodo,DateTime fechaServidor)
        {
            string codigoInforme = NormalizarTexto(request.codigo_informe);
            bool incluyeFci = NormalizarTexto(request.codigo_filtro) == "00";

            string nombreReporte = codigoInforme switch
            {
                "01" => incluyeFci
                    ? "Sys_AuxAportesResumenFCI"
                    : "Sys_AuxAportesResumen",

                "02" => incluyeFci
                    ? "Sys_AuxAportesDetalleFCI"
                    : "Sys_AuxAportesDetalle",

                "03" => incluyeFci
                    ? "Sys_AuxAportesRsmCategoriaFCI"
                    : "Sys_AuxAportesRsmCategoria",

                "04" => "Sys_AuxAportesCuentas",

                _ => string.Empty
            };

            if (string.IsNullOrWhiteSpace(nombreReporte))
            {
                return CrearErrorGeneracion(
                    "El informe de patrimonio indicado no es válido.",
                    -2);
            }

            if (codigoInforme == "04" &&
                string.IsNullOrWhiteSpace(request.gstr_niveles))
            {
                return CrearErrorGeneracion(
                    "No se encontró la configuración de niveles contables.",
                    -2);
            }

            string titulo = ObtenerTituloPatrimonio(codigoInforme);
            string filtros = ConstruirFiltrosPatrimonio(
                request,
                periodo,
                codigoInforme);

            var parametros = CrearParametrosEncabezado(
                periodo,
                fechaServidor,
                request.usuario_sesion);

            AgregarParametro(
                parametros,
                ParametroFiltros,
                filtros);

            if (codigoInforme == "04")
            {
                AgregarParametro(
                    parametros,
                    ParametroMascara,
                    ConstruirMascaraNiveles(request.gstr_niveles));

                AgregarParametro(
                    parametros,
                    ParametroTitulo,
                    titulo);
            }

            return CrearResultadoGeneracion(
                nombreReporte,
                titulo,
                periodo,
                parametros);
        }

        /// <summary>
        /// Genera la configuración de los reportes auxiliares de fondos.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="periodo"></param>
        /// <param name="fechaServidor"></param>
        /// <returns></returns>
        private static ErrorDto<CcReportesEstudioAuxiliarGenerarResult>
            GenerarAuxiliarFondos(
                CcReportesEstudioAuxiliarGenerarRequest request,
                CcReportesEstudioPeriodoData periodo,
                DateTime fechaServidor)
        {
            string codigoInforme = NormalizarTexto(request.codigo_informe);

            string nombreReporte = codigoInforme switch
            {
                "01" => "Sys_AuxFondoResumen",
                "02" => "Sys_AuxFondoDetalle",
                _ => string.Empty
            };

            if (string.IsNullOrWhiteSpace(nombreReporte))
            {
                return CrearErrorGeneracion(
                    "El informe de fondos indicado no es válido.",
                    -2);
            }

            string titulo = codigoInforme == "01"
                ? "Auxiliar de Fondos - Resumen"
                : "Auxiliar de Fondos - Detalle";

            string filtros = ConstruirFiltrosFondos(
                request,
                periodo);

            var parametros = CrearParametrosEncabezado(
                periodo,
                fechaServidor,
                request.usuario_sesion);

            AgregarParametro(
                parametros,
                ParametroFiltros,
                filtros);

            return CrearResultadoGeneracion(
                nombreReporte,
                titulo,
                periodo,
                parametros);
        }

        /// <summary>
        /// Genera la configuración de los reportes auxiliares de crédito.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="periodo"></param>
        /// <param name="fechaServidor"></param>
        /// <returns></returns>
        private static ErrorDto<CcReportesEstudioAuxiliarGenerarResult>
            GenerarAuxiliarCredito(
                CcReportesEstudioAuxiliarGenerarRequest request,
                CcReportesEstudioPeriodoData periodo,
                DateTime fechaServidor)
        {
            string codigoInforme = NormalizarTexto(request.codigo_informe);

            if (codigoInforme == "11")
            {
                return CrearErrorGeneracion(
                    "El informe 11 - Créditos Balance (Detalle) no tiene una definición asociada en el VB6.",
                    -2);
            }

            return codigoInforme switch
            {
                "01" or "02" or "10" => GenerarCreditoResumenDetalle(
                    request,
                    periodo,
                    fechaServidor,
                    codigoInforme),

                "03" or "04" => GenerarCreditoSaldoNegativo(
                    request,
                    periodo,
                    fechaServidor,
                    codigoInforme),

                "05" => GenerarCreditoMetodoContable(
                    request,
                    periodo,
                    fechaServidor),

                "06" => GenerarCreditoGarantias(
                    request,
                    periodo,
                    fechaServidor),

                "07" => GenerarCreditoComplementario(
                    request,
                    periodo,
                    fechaServidor),

                "08" or "09" or "12" or "13" or "14" or "15" or "16" or "17"
                    => GenerarCreditoProcedimiento(
                        request,
                        periodo,
                        fechaServidor,
                        codigoInforme),

                _ => CrearErrorGeneracion(
                    "El informe de crédito indicado no es válido.",
                    -2)
            };
        }

        /// <summary>
        /// Genera los informes de crédito de resumen, detalle y balance resumen.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="periodo"></param>
        /// <param name="fechaServidor"></param>
        /// <param name="codigoInforme"></param>
        /// <returns></returns>
        private static ErrorDto<CcReportesEstudioAuxiliarGenerarResult>
            GenerarCreditoResumenDetalle(
                CcReportesEstudioAuxiliarGenerarRequest request,
                CcReportesEstudioPeriodoData periodo,
                DateTime fechaServidor,
                string codigoInforme)
        {
            bool tieneInstitucion = !string.IsNullOrWhiteSpace(
                request.comunes.cod_institucion);

            string nombreReporte = codigoInforme switch
            {
                "01" => tieneInstitucion
                    ? "Sys_AuxCreditoResumenIns"
                    : "Sys_AuxCreditoResumen",

                "02" => tieneInstitucion
                    ? "Sys_AuxCreditoDetalleIns"
                    : "Sys_AuxCreditoDetalle",

                "10" => "Sys_AuxCreditoBalanceResumen",

                _ => string.Empty
            };

            string titulo = codigoInforme switch
            {
                "01" => "AUXILIAR DE CREDITO - RESUMEN DE SALDOS",
                "02" => "AUXILIAR DE CREDITO - DETALLE DE SALDOS",
                "10" => "AUXILIAR DE CREDITO - BALANCE RESUMIDO",
                _ => string.Empty
            };

            string filtros = ConstruirFiltrosCreditoBase(
                request,
                periodo);

            var parametros = CrearParametrosEncabezadoCredito(
                periodo,
                fechaServidor);

            AgregarParametro(
                parametros,
                ParametroMascara,
                ConstruirMascaraNiveles(request.gstr_niveles));

            AgregarParametro(
                parametros,
                ParametroTituloMayuscula,
                titulo);

            AgregarParametro(
                parametros,
                ParametroFiltros,
                filtros);

            return CrearResultadoGeneracion(
                nombreReporte,
                titulo,
                periodo,
                parametros);
        }

        /// <summary>
        /// Genera los informes de crédito con saldo inicial o saldo final negativo.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="periodo"></param>
        /// <param name="fechaServidor"></param>
        /// <param name="codigoInforme"></param>
        /// <returns></returns>
        private static ErrorDto<CcReportesEstudioAuxiliarGenerarResult>GenerarCreditoSaldoNegativo(CcReportesEstudioAuxiliarGenerarRequest request,CcReportesEstudioPeriodoData periodo,DateTime fechaServidor,string codigoInforme)
        {
            bool reporteGeneral = request.credito.usar_reporte_general;

            string nombreReporte = reporteGeneral
                ? "Sys_AuxCreditoDetalle"
                : "Sys_AuxCreditoSaldosNegativos";

            string titulo = codigoInforme == "03"
                ? "AUXILIAR DE CREDITO - SALDOS INICIALES NEGATIVOS"
                : "AUXILIAR DE CREDITO - SALDOS FINALES - NEGATIVOS";

            string filtros = ConstruirFiltrosCreditoSaldoNegativo(
                periodo,
                codigoInforme);

            var parametros = CrearParametrosEncabezadoCredito(
                periodo,
                fechaServidor);

            AgregarParametro(
                parametros,
                ParametroTituloMayuscula,
                titulo);

            if (reporteGeneral)
            {
                AgregarParametro(
                    parametros,
                    ParametroMascara,
                    request.gstr_niveles);
            }

            AgregarParametro(
                parametros,
                ParametroFiltros,
                filtros);

            return CrearResultadoGeneracion(
                nombreReporte,
                titulo,
                periodo,
                parametros);
        }

        /// <summary>
        /// Genera el informe contable del auxiliar de crédito.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="periodo"></param>
        /// <param name="fechaServidor"></param>
        /// <returns></returns>
        private static ErrorDto<CcReportesEstudioAuxiliarGenerarResult> GenerarCreditoMetodoContable(CcReportesEstudioAuxiliarGenerarRequest request,CcReportesEstudioPeriodoData periodo,DateTime fechaServidor)
        {
            if (string.IsNullOrWhiteSpace(request.gstr_niveles))
            {
                return CrearErrorGeneracion(
                    "No se encontró la configuración de niveles contables.",
                    -2);
            }

            const string nombreReporte = "Sys_AuxCreditoCuentas";
            const string titulo = "Auxiliar : Balance Contable";

            string filtros = ConstruirFiltrosPeriodo(
                "ASE_PER_CUENTAS",
                periodo);

            var parametros = CrearParametrosEncabezado(
                periodo,
                fechaServidor,
                request.usuario_sesion);

            AgregarParametro(
                parametros,
                ParametroMascara,
                ConstruirMascaraNiveles(request.gstr_niveles));

            AgregarParametro(
                parametros,
                ParametroTituloMayuscula,
                titulo);

            AgregarParametro(
                parametros,
                ParametroFiltros,
                filtros);

            return CrearResultadoGeneracion(
                nombreReporte,
                titulo,
                periodo,
                parametros);
        }

        /// <summary>
        /// Genera el informe de cartera de crédito agrupado por garantía.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="periodo"></param>
        /// <param name="fechaServidor"></param>
        /// <returns></returns>
        private static ErrorDto<CcReportesEstudioAuxiliarGenerarResult>
            GenerarCreditoGarantias(
                CcReportesEstudioAuxiliarGenerarRequest request,
                CcReportesEstudioPeriodoData periodo,
                DateTime fechaServidor)
        {
            const string nombreReporte = "Sys_AuxCreditoResumeXGarantia";
            const string titulo = "Auxiliar de Crédito - Cartera por Garantía";

            string filtros = ConstruirFiltrosCreditoGarantias(
                request,
                periodo);

            var parametros = CrearParametrosEncabezadoCredito(
                periodo,
                fechaServidor);

            AgregarParametro(
                parametros,
                ParametroFiltros,
                filtros);

            return CrearResultadoGeneracion(
                nombreReporte,
                titulo,
                periodo,
                parametros);
        }

        /// <summary>
        /// Genera el informe complementario del auxiliar de crédito.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="periodo"></param>
        /// <param name="fechaServidor"></param>
        /// <returns></returns>
        private static ErrorDto<CcReportesEstudioAuxiliarGenerarResult> GenerarCreditoComplementario(CcReportesEstudioAuxiliarGenerarRequest request,CcReportesEstudioPeriodoData periodo, DateTime fechaServidor)
        {
            const string nombreReporte = "Sys_AuxCreditoComplementario";
            const string titulo = "AUXILIAR DE CREDITO - COMPLEMENTARIO";

            string filtros = ConstruirFiltrosCreditoComplementario(
                request,
                periodo);

            var parametros = CrearParametrosEncabezadoCredito(
                periodo,
                fechaServidor);

            AgregarParametro(
                parametros,
                ParametroMascara,
                ConstruirMascaraNiveles(request.gstr_niveles));

            AgregarParametro(
                parametros,
                ParametroTituloMayuscula,
                titulo);

            AgregarParametro(
                parametros,
                ParametroFiltros,
                filtros);

            return CrearResultadoGeneracion(
                nombreReporte,
                titulo,
                periodo,
                parametros);
        }

        /// <summary>
        /// Genera los informes de crédito cuyo origen de datos es un procedimiento almacenado.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="periodo"></param>
        /// <param name="fechaServidor"></param>
        /// <param name="codigoInforme"></param>
        /// <returns></returns>
        private static ErrorDto<CcReportesEstudioAuxiliarGenerarResult>GenerarCreditoProcedimiento(CcReportesEstudioAuxiliarGenerarRequest request,CcReportesEstudioPeriodoData periodo,DateTime fechaServidor, string codigoInforme)
        {
            var configuracion = ObtenerConfiguracionCreditoProcedimiento(
                codigoInforme);

            if (string.IsNullOrWhiteSpace(configuracion.nombreReporte))
            {
                return CrearErrorGeneracion(
                    "No fue posible resolver el informe de crédito.",
                    -2);
            }

            var parametros = CrearParametrosEncabezado(
                periodo,
                fechaServidor,
                request.usuario_sesion);

            AgregarParametro(
                parametros,
                ParametroTitulo,
                configuracion.titulo);

            AgregarParametro(
                parametros,
                "Anio",
                periodo.anio);

            AgregarParametro(
                parametros,
                "Mes",
                periodo.mes);

            if (configuracion.incluyeUsuario)
            {
                AgregarParametro(
                    parametros,
                    "Usuario",
                    request.usuario_sesion.Trim());

                AgregarParametro(
                    parametros,
                    "Asiento",
                    0);
            }

            return CrearResultadoGeneracion(
                configuracion.nombreReporte,
                configuracion.titulo,
                periodo,
                parametros);
        }
        /// <summary>
        /// Obtiene las clasificaciones de cartera utilizadas por los informes especiales de mora y antigüedad.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CC_ReportesEstudio_Carteras_Lista_Obtener(int CodEmpresa)
        {
            const string sql = """
                select
                    rtrim(COD_CLASIFICACION) as item,
                    rtrim(DESCRIPCION) as descripcion
                from CBR_CLASIFICACION_CARTERA
                where isnull(ESTADO, 0) = 1
                order by COD_CLASIFICACION;
                """;

            try
            {
                using var connection = DbHelper.OpenConnection(
                    _portalDb,
                    CodEmpresa);

                var lista = connection
                    .Query<DropDownListaGenericaModel>(sql)
                    .ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper
                    .CreateErrorResponse<List<DropDownListaGenericaModel>>(
                        ex.Message,
                        -1,
                        new List<DropDownListaGenericaModel>());
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper
                    .CreateErrorResponse<List<DropDownListaGenericaModel>>(
                        ex.Message,
                        -1,
                        new List<DropDownListaGenericaModel>());
            }
        }
        /// <summary>
        /// Ejecuta el informe especial seleccionado. Los tipos 0, 1, 2 y 3 devuelven datos para exportación directa a Excel; el tipo 4 devuelve la configuración del reporte de antigüedad y garantía.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CcReportesEstudioEspecialReporteDto>
    CC_ReportesEstudio_Especial_Generar(
        int CodEmpresa,
        CcReportesEstudioEspecialRequest? request)
        {
            if (request == null)
            {
                return CrearErrorEspecial(
                    "No se recibió la información para generar el informe especial.",
                    -2);
            }

            string validacion = ValidarEspecialGenerarRequest(request);

            if (!string.IsNullOrWhiteSpace(validacion))
            {
                return CrearErrorEspecial(validacion, -2);
            }

            try
            {
                if (request.tipo == 4)
                {
                    DateTime fechaServidor = _proGrxMain.fxFechaServidor(
                        CodEmpresa,
                        0);

                    return CrearResultadoEspecialReporte(
                        request,
                        fechaServidor);
                }

                using var connection = DbHelper.OpenConnection(
                    _portalDb,
                    CodEmpresa);

                return request.tipo switch
                {
                    0 => EjecutarEspecialExcel(
                        connection,
                        "spSys_Aux_Core_Full",
                        new
                        {
                            Anio = request.anio,
                            Mes = request.mes
                        },
                        $"Sys_Aux_Core_Full_{request.anio}{request.mes:00}.xlsx"),

                    1 => EjecutarEspecialExcel(
                        connection,
                        "spSys_Aux_Creditos_Base_Incobrables",
                        new
                        {
                            Anio = request.anio,
                            Mes = request.mes
                        },
                        $"Sys_Aux_Creditos_Base_Incobrables_{request.anio}{request.mes:00}.xlsx"),

                    2 => EjecutarEspecialExcel(
                        connection,
                        "spSys_Aux_Creditos_Completo",
                        new
                        {
                            Anio = request.anio,
                            Mes = request.mes
                        },
                        $"Sys_Aux_Creditos_Completo_{request.anio}{request.mes:00}.xlsx"),

                    3 => EjecutarEspecialExcel(
                        connection,
                        "spSIF_ReporteGeneralMora",
                        new
                        {
                            Anio = request.anio.ToString(),
                            Mes = request.mes.ToString(),
                            Cartera = ConstruirCarterasEspeciales(
                                request.carteras)
                        },
                        $"Informe_Mora_Especial_{request.anio}{request.mes:00}.xlsx"),

                    _ => CrearErrorEspecial(
                        "El informe especial indicado no es válido.",
                        -2)
                };
            }
            catch (SqlException ex)
            {
                return CrearErrorEspecial(ex.Message, -1);
            }
            catch (InvalidOperationException ex)
            {
                return CrearErrorEspecial(ex.Message, -1);
            }
        }
        /// <summary>
        /// Ejecuta un procedimiento almacenado de informes especiales y convierte el resultado en filas exportables a Excel.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="procedimiento"></param>
        /// <param name="parametros"></param>
        /// <param name="nombreArchivo"></param>
        /// <returns></returns>
        private static ErrorDto<CcReportesEstudioEspecialReporteDto> EjecutarEspecialExcel(
            SqlConnection connection,
            string procedimiento,
            object parametros,
            string nombreArchivo)
        {
            var registros = connection.Query(
                procedimiento,
                parametros,
                commandType: CommandType.StoredProcedure);

            var result = new CcReportesEstudioEspecialReporteDto
            {
                tipo_salida = "EXCEL",
                nombre_archivo = nombreArchivo,
                filas = registros
                    .Select(ConvertirFilaEspecial)
                    .ToList()
            };

            return DbHelper.CreateOkResponse(result);
        }

        /// <summary>
        /// Convierte una fila dinámica de Dapper en un diccionario serializable.
        /// </summary>
        /// <param name="registro"></param>
        /// <returns></returns>
        private static Dictionary<string, object?> ConvertirFilaEspecial(dynamic registro)
        {
            var fila = (IDictionary<string, object>)registro;

            return fila.ToDictionary(
                columna => columna.Key,
                columna => columna.Value is DBNull
                    ? null
                    : columna.Value);
        }
        private static string ValidarEspecialGenerarRequest(CcReportesEstudioEspecialRequest? request)
        {
            if (request == null)
            {
                return "La solicitud del informe especial es requerida.";
            }

            if (request.tipo < 0 || request.tipo > 4)
            {
                return "Debe seleccionar un informe especial válido.";
            }

            if (request.anio <= 0)
            {
                return "El año del período no es válido.";
            }

            if (request.mes < 1 || request.mes > 12)
            {
                return "El mes del período no es válido.";
            }

            if (request.tipo is 3 or 4 &&
                (request.carteras == null ||
                 request.carteras.Count == 0))
            {
                return "Debe seleccionar al menos una cartera.";
            }

            if (request.tipo == 4 &&
                string.IsNullOrWhiteSpace(request.usuario_sesion))
            {
                return "El usuario de sesión es requerido.";
            }

            return string.Empty;
        }
        private static ErrorDto<CcReportesEstudioEspecialReporteDto>CrearResultadoEspecialReporte(CcReportesEstudioEspecialRequest request,DateTime fechaServidor)
        {
            bool detallado = request.detallado ?? false;
            int anio = request.anio ?? 0;
            int mes = request.mes ?? 0;
            string usuario = request.usuario_sesion ?? string.Empty;

            string nombreReporte = detallado
                ? "Sys_AuxCorteCbrAntiguedadGarantia_Especial_Detalle"
                : "Sys_AuxCorteCbrAntiguedadGarantia_Especial";

            string titulo = "Lista de Cartera al Cierre por Garantía";

            var parametros = new List<CcReportesEstudioParametroDto>
    {
        CrearParametro(
            "Anio",
            anio),

        CrearParametro(
            "Mes",
            mes),

        CrearParametro(
            "Cartera",
            ConstruirCarterasEspeciales(request.carteras)),

        CrearParametro(
            "Detalle",
            detallado ? 1 : 0),

        CrearParametro(
            "fxFecha",
            $"FECHA: {fechaServidor:dd/MM/yyyy}"),

        CrearParametro(
            "fxEmpresa",
            null),

        CrearParametro(
            "fxUsuario",
            $"USER: {usuario.ToUpperInvariant()}"),

        CrearParametro(
            "fxTitulo",
            titulo),

        CrearParametro(
            "fxSubTitulo",
            $"Periodo: {anio} - {MConciliacionDB.fxConvierteMES(mes)}"),

        CrearParametro(
            "Empresa",
            null)
    };

            var result = new CcReportesEstudioEspecialReporteDto
            {
                tipo_salida = "REPORTE",
                nombre_reporte = nombreReporte,
                folder = "Conciliacion",
                cod_reporte = "P",
                titulo_ventana =
                    "Reportes de Auxiliares: Antigüedad por Garantías",
                nombre_archivo = nombreReporte,
                parametros = parametros
            };

            return DbHelper.CreateOkResponse(result);
        }
        private static ErrorDto<CcReportesEstudioEspecialReporteDto> CrearErrorEspecial(string mensaje, int codigo)
        {
            return DbHelper
                .CreateErrorResponse<CcReportesEstudioEspecialReporteDto>(
                    mensaje,
                    codigo,
                    new CcReportesEstudioEspecialReporteDto());
        }
        private static (string nombreReporte,string titulo,bool incluyeUsuario)ObtenerConfiguracionCreditoProcedimiento(string codigoInforme)
        {
            return codigoInforme switch
            {
                "08" => (
                    "Sys_AuxCreditoProdAcum",
                    "Auxiliar: Producto Acumulado [Detalle]",
                    false),

                "09" => (
                    "Sys_AuxCreditoProdAcumRsm",
                    "Auxiliar: Producto Acumulado [Resumen]",
                    false),

                "12" => (
                    "Sys_AuxCreditoIntCbrAdelandoRsm",
                    "Ingresos x Intereses Cobrados por Adelantado (Resumen)",
                    true),

                "13" => (
                    "Sys_AuxCreditoIntCbrAdelando",
                    "Ingresos x Intereses Cobrados por Adelantado (Detalle)",
                    true),

                "14" => (
                    "Sys_AuxCreditoProdAcumSuspenso",
                    "Auxiliar: Producto Acumulado en Suspenso [Detalle]",
                    false),

                "15" => (
                    "Sys_AuxCreditoProdAcumSuspensoRsm",
                    "Auxiliar: Producto Acumulado en Suspenso [Resumen]",
                    false),

                "16" => (
                    "Sys_AuxCreditoGastoDiferido",
                    "Cargos / Gastos Diferidos (Detalle)",
                    true),

                "17" => (
                    "Sys_AuxCreditoGastoDiferidoRsm",
                    "Cargos / Gastos Diferidos (Resumen)",
                    true),

                _ => (string.Empty, string.Empty, false)
            };
        }

        private static string ValidarAuxiliarGenerarRequest( CcReportesEstudioAuxiliarGenerarRequest? request)
        {
            if (request == null)
            {
                return "La solicitud del reporte es requerida.";
            }

            if ((request.id_per_historico ?? 0) <= 0)
            {
                return "Debe seleccionar un período válido.";
            }

            string tipoAuxiliar = NormalizarTexto(
                request.tipo_auxiliar).ToUpperInvariant();

            if (tipoAuxiliar is not TipoPatrimonio
                and not TipoFondos
                and not TipoCredito)
            {
                return "Debe seleccionar un tipo de auxiliar válido.";
            }

            if (string.IsNullOrWhiteSpace(request.codigo_informe))
            {
                return "Debe seleccionar un informe.";
            }

            return string.Empty;
        }

        private static string ObtenerTituloPatrimonio(string codigoInforme)
        {
            return codigoInforme switch
            {
                "01" => "Auxiliar de Aportes - Resumen",
                "02" => "Auxiliar de Aportes - Detalle",
                "03" => "Auxiliar de Aportes - Resumen por Categoría",
                "04" => "Auxiliar : Balance Contable",
                _ => string.Empty
            };
        }

        private static List<CcReportesEstudioParametroDto>
            CrearParametrosEncabezado(
                CcReportesEstudioPeriodoData periodo,
                DateTime fechaServidor,
                string usuario)
        {
            return new List<CcReportesEstudioParametroDto>
    {
        CrearParametro(
            ParametroSubtitulo,
            ConstruirSubtitulo(periodo)),

        CrearParametro(
            ParametroFecha,
            fechaServidor.ToString("dd/MM/yyyy")),

        CrearParametro(
            ParametroEmpresa,
            null),

        CrearParametro(
            ParametroUsuario,
            NormalizarTexto(usuario))
    };
        }

        private static List<CcReportesEstudioParametroDto>
            CrearParametrosEncabezadoCredito(
                CcReportesEstudioPeriodoData periodo,
                DateTime fechaServidor)
        {
            return new List<CcReportesEstudioParametroDto>
    {
        CrearParametro(
            ParametroSubtitulo,
            ConstruirSubtitulo(periodo)),

        CrearParametro(
            ParametroFecha,
            fechaServidor.ToString("dd/MM/yyyy")),

        CrearParametro(
            ParametroEmpresa,
            null)
    };
        }

        private static string ConstruirSubtitulo(CcReportesEstudioPeriodoData periodo)
        {
            return $"PERIODO: {periodo.anio} - " +
                   MConciliacionDB
                       .fxConvierteMES(periodo.mes)
                       .ToUpperInvariant();
        }

        private static string ConstruirFiltrosPatrimonio(
     CcReportesEstudioAuxiliarGenerarRequest request,
     CcReportesEstudioPeriodoData periodo,
     string codigoInforme)
        {
            string alias = codigoInforme == "04"
                ? "ASE_PER_CUENTAS"
                : "ASE_PER_APORTES";

            var condiciones = new List<string>
    {
        $"{alias}.ANIO = {periodo.anio}",
        $"{alias}.MES = {periodo.mes}"
    };

            if (codigoInforme != "04")
            {
                AgregarFiltroTexto(
                    condiciones,
                    "SOCIOS.COD_INSTITUCION",
                    request.comunes.cod_institucion);

                if (request.comunes.solo_lineas_con_contenido)
                {
                    condiciones.Add(
                        "(ASE_PER_APORTES.APORTE + " +
                        "ASE_PER_APORTES.AHORRO + " +
                        "ASE_PER_APORTES.CAPITALIZA + " +
                        "ASE_PER_APORTES.CUSTODIA + " +
                        "ASE_PER_APORTES.EXTRA) <> 0");
                }
            }

            return ConstruirWhere(condiciones);
        }

        private static string ConstruirFiltrosFondos(
     CcReportesEstudioAuxiliarGenerarRequest request,
     CcReportesEstudioPeriodoData periodo)
        {
            var condiciones = new List<string>
    {
        $"FND_PER_CERRADOS.ANIO = {periodo.anio}",
        $"FND_PER_CERRADOS.MES = {periodo.mes}"
    };

            string codigoFiltro = NormalizarTexto(
                request.codigo_filtro);

            if (codigoFiltro == "01")
            {
                condiciones.Add(
                    "FND_PER_CERRADOS.ESTADO = 'A'");
            }
            else if (codigoFiltro == "02")
            {
                condiciones.Add(
                    "FND_PER_CERRADOS.ESTADO = 'L'");
            }

            return ConstruirWhere(condiciones);
        }

        private static string ConstruirFiltrosCreditoBase(CcReportesEstudioAuxiliarGenerarRequest request,CcReportesEstudioPeriodoData periodo)
        {
            var condiciones = new List<string>
            {
                $"ASE_PER_CERRADOS.ANIO = {periodo.anio}",
                $"ASE_PER_CERRADOS.MES = {periodo.mes}",
                "ASE_PER_CERRADOS.ESTADO IN ('A', 'C')"
            };

            AgregarFiltroEspecialCredito(
                condiciones,
                request.codigo_filtro);

            AgregarFiltroTexto(
                condiciones,
                "REG_CREDITOS.COD_GRUPO",
                request.credito.cod_recurso);

            AgregarFiltroTexto(
                condiciones,
                "REG_CREDITOS.CODIGO",
                request.credito.codigo);

            AgregarFiltroTexto(
                condiciones,
                "REG_CREDITOS.COD_DESTINO",
                request.credito.cod_destino);

            return ConstruirWhere(condiciones);
        }
        private static string ConstruirFiltrosCreditoComplementario( CcReportesEstudioAuxiliarGenerarRequest request,CcReportesEstudioPeriodoData periodo)
        {
            var condiciones = new List<string>
            {
                $"ASE_PER_CERRADOS.ANIO = {periodo.anio}",
                $"ASE_PER_CERRADOS.MES = {periodo.mes}",
                "ASE_PER_CERRADOS.ESTADO IN ('A', 'C')"
            };

            AgregarFiltroEspecialCredito(
                condiciones,
                request.codigo_filtro);

            return ConstruirWhere(condiciones);
        }
        private static string ConstruirFiltrosCreditoSaldoNegativo(CcReportesEstudioPeriodoData periodo,string codigoInforme)
        {
            var condiciones = new List<string>
            {
                codigoInforme == "03"
                    ? "ASE_PER_CERRADOS.SALDO_INICIAL < 0"
                    : "ASE_PER_CERRADOS.SALDO_FINAL < 0",

                $"ASE_PER_CERRADOS.ANIO = {periodo.anio}",
                $"ASE_PER_CERRADOS.MES = {periodo.mes}"
            };

            return ConstruirWhere(condiciones);
        }
        private static string ConstruirMascaraNiveles(string? niveles)
        {
            return NormalizarTexto(niveles)
                .PadRight(5, '0');
        }
        private static string ConstruirFiltrosCreditoGarantias( CcReportesEstudioAuxiliarGenerarRequest request,CcReportesEstudioPeriodoData periodo)
        {
            var condiciones = new List<string>
            {
                $"vSIFAuxCorteRepCredito.ANIO = {periodo.anio}",
                $"vSIFAuxCorteRepCredito.MES = {periodo.mes}",
                "vSIFAuxCorteRepCredito.SALDO_FINAL > 0"
            };

            string codigoFiltro = NormalizarTexto(
                request.codigo_filtro);

            if (codigoFiltro == "01")
            {
                condiciones.Add(
                    "vSIFAuxCorteRepCredito.RETENCION = 'N'");

                condiciones.Add(
                    "vSIFAuxCorteRepCredito.POLIZA = 'N'");
            }
            else if (codigoFiltro == "02")
            {
                condiciones.Add(
                    "(vSIFAuxCorteRepCredito.RETENCION = 'S' OR " +
                    "vSIFAuxCorteRepCredito.POLIZA = 'S')");
            }

            return ConstruirWhere(condiciones);
        }

        private static string ConstruirFiltrosPeriodo(string tabla, CcReportesEstudioPeriodoData periodo)
        {
            var condiciones = new List<string>
            {
                $"{tabla}.ANIO = {periodo.anio}",
                $"{tabla}.MES = {periodo.mes}"
            };

            return ConstruirWhere(condiciones);
        }

        private static void AgregarFiltroEspecialCredito(ICollection<string> condiciones,string? codigoFiltro)
        {
            string filtro = NormalizarTexto(codigoFiltro);

            if (filtro == "01")
            {
                condiciones.Add(
                    "ASE_PER_CATALOGO.RETENCION = 'N'");

                condiciones.Add(
                    "ASE_PER_CATALOGO.POLIZA = 'N'");
            }
            else if (filtro == "02")
            {
                condiciones.Add(
                    "(ASE_PER_CATALOGO.RETENCION = 'S' OR " +
                    "ASE_PER_CATALOGO.POLIZA = 'S')");
            }
        }

        private static void AgregarFiltroTexto(
            ICollection<string> condiciones,
            string campo,
            string? valor)
        {
            string valorNormalizado = NormalizarTexto(valor);

            if (string.IsNullOrWhiteSpace(valorNormalizado))
            {
                return;
            }

            condiciones.Add(
                $"{campo} = '{EscaparSqlFiltro(valorNormalizado)}'");
        }

        private static string EscaparSqlFiltro(string valor)
        {
            return valor.Replace("'", "''");
        }

        private static CcReportesEstudioParametroDto CrearParametro(
            string nombre,
            object? valor)
        {
            return new CcReportesEstudioParametroDto
            {
                nombre = nombre,
                valor = valor
            };
        }

        private static void AgregarParametro(
            ICollection<CcReportesEstudioParametroDto> parametros,
            string nombre,
            object? valor)
        {
            parametros.Add(
                CrearParametro(
                    nombre,
                    valor));
        }

        private static ErrorDto<CcReportesEstudioAuxiliarGenerarResult>
            CrearResultadoGeneracion(
                string nombreReporte,
                string titulo,
                CcReportesEstudioPeriodoData periodo,
                List<CcReportesEstudioParametroDto> parametros)
        {
            var result = new CcReportesEstudioAuxiliarGenerarResult
            {
                nombre_reporte = nombreReporte,
                folder = FolderConciliacion,
                cod_reporte = CodigoReporte,
                titulo_ventana = titulo,
                nombre_archivo =
                    $"{nombreReporte}_{periodo.anio}{periodo.mes:00}",
                parametros = parametros
            };

            return DbHelper.CreateOkResponse(result);
        }
        private static string ConstruirWhere(IEnumerable<string> condiciones)
        {
            var lista = condiciones
                .Where(condicion => !string.IsNullOrWhiteSpace(condicion))
                .ToList();

            return lista.Count == 0
                ? string.Empty
                : $"WHERE {string.Join(" AND ", lista)}";
        }
        private static ErrorDto<CcReportesEstudioAuxiliarGenerarResult>
            CrearErrorGeneracion(
                string mensaje,
                int codigo)
        {
            return DbHelper
                .CreateErrorResponse<CcReportesEstudioAuxiliarGenerarResult>(
                    mensaje,
                    codigo,
                    new CcReportesEstudioAuxiliarGenerarResult());
        }
        private static string ConstruirCarterasEspeciales(
    IEnumerable<string>? carteras)
        {
            if (carteras == null)
            {
                return string.Empty;
            }

            return string.Join(
                ",",
                carteras
                    .Where(cartera => !string.IsNullOrWhiteSpace(cartera))
                    .Select(cartera => cartera.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase));
        }
        #endregion
    }
}
