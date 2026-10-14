using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmCcReportesAlCorteDb
    {
        private const string MensajePeriodoInvalido =
            "El per&iacute;odo indicado no es v&aacute;lido.";

        private const string MensajeReporteInvalido =
            "El reporte solicitado no es v&aacute;lido.";

        private const string SpAnalisisComparativo =
            "spCbr_W_AnalisisMorosidadCompartivo";

        private const string SpAnalisisMorosidad =
            "spCbr_W_AnalisisMorosidad";

        private const string SpAnalisisMorosidadTipo =
            "spCbr_W_AnalisisMorosidadxTipo";

        private const string SpAnalisisAntiguedad =
            "spCbr_W_AnalisisCorteAntiguedadDiasReal";

        private const string SpEstimacionResumen =
            "spCbr_Estimacion_Resumen";

        private const string SpEstimacionDetalle =
            "spCbr_Estimacion_Detalle";

        private const string SqlInstituciones = """
            SELECT
                CONVERT(varchar(20), COD_INSTITUCION) AS idx,
                RTRIM(descripcion) AS itmx
            FROM INSTITUCIONES
            ORDER BY descripcion;
            """;

        private const string SqlProfesiones = """
            SELECT
                CONVERT(varchar(20), COD_PROFESION) AS idx,
                RTRIM(descripcion) AS itmx
            FROM AFI_PROFESIONES
            ORDER BY descripcion;
            """;

        private const string SqlSectores = """
            SELECT
                CONVERT(varchar(20), COD_SECTOR) AS idx,
                RTRIM(descripcion) AS itmx
            FROM AFI_SECTORES
            ORDER BY descripcion;
            """;

        private const string SqlZonas = """
            SELECT
                CONVERT(varchar(20), COD_ZONA) AS idx,
                RTRIM(descripcion) AS itmx
            FROM AFI_ZONAS
            ORDER BY descripcion;
            """;

        private const string SqlEstadosPersona = """
            SELECT
                RTRIM(cod_estado) AS idx,
                RTRIM(descripcion) AS itmx
            FROM AFI_ESTADOS_PERSONA
            ORDER BY descripcion;
            """;

        private const string SqlGarantias = """
            SELECT
                RTRIM(GARANTIA) AS idx,
                RTRIM(descripcion) AS itmx
            FROM CRD_GARANTIA_TIPOS
            ORDER BY GARANTIA;
            """;

        private const string SqlCarteras = """
            SELECT
                RTRIM(cod_clasificacion) AS idx,
                RTRIM(descripcion) AS itmx
            FROM CBR_CLASIFICACION_CARTERA
            ORDER BY cod_clasificacion;
            """;

        private const string SqlOficinas = """
            SELECT
                RTRIM(cod_oficina) AS idx,
                RTRIM(descripcion) AS itmx
            FROM SIF_OFICINAS
            ORDER BY cod_oficina;
            """;

        private const string SqlEstadosCiviles = """
            SELECT
                CONVERT(varchar(20), Estado_Civil) AS idx,
                RTRIM(Descripcion) AS itmx
            FROM SYS_ESTADO_CIVIL
            WHERE Activo = 1
            ORDER BY Descripcion;
            """;

        private const string SqlEstadosLaborales = """
            SELECT
                CONVERT(varchar(20), ESTADO_LABORAL) AS idx,
                RTRIM(Descripcion) AS itmx
            FROM AFI_ESTADO_LABORAL
            WHERE Activo = 1
            ORDER BY Descripcion;
            """;

        private const string SqlProvincias = """
            SELECT
                CONVERT(varchar(20), Provincia) AS idx,
                RTRIM(Descripcion) AS itmx
            FROM Provincias
            ORDER BY Descripcion;
            """;

        private const string SqlCatalogo = """
            SELECT
                RTRIM(CODIGO) AS idx,
                RTRIM(DESCRIPCION) AS itmx
            FROM CATALOGO
            ORDER BY CODIGO;
            """;

        private readonly PortalDB _portalDb;

        public FrmCcReportesAlCorteDb(
            IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene en una sola operación los catálogos requeridos
        /// al abrir el formulario de reportes al corte.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Catálogos iniciales del formulario.</returns>
        public ErrorDto<CcReportesAlCorteInicializacionData>
            CC_ReportesAlCorte_Inicializar(
                int codEmpresa)
        {
            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                    new CcReportesAlCorteInicializacionData
                    {
                        periodos =
                            CC_ReportesAlCorte_Periodos_Consultar(
                                connection),
                        instituciones =
                            CC_ReportesAlCorte_Lista_Consultar<CCGenericList>(
                                connection,
                                SqlInstituciones),
                        profesiones =
                            CC_ReportesAlCorte_Lista_Consultar<CCGenericList>(
                                connection,
                                SqlProfesiones),
                        sectores =
                            CC_ReportesAlCorte_Lista_Consultar<CCGenericList>(
                                connection,
                                SqlSectores),
                        zonas =
                            CC_ReportesAlCorte_Lista_Consultar<CCGenericList>(
                                connection,
                                SqlZonas),
                        estados_persona =
                            CC_ReportesAlCorte_Lista_Consultar<CCGenericList>(
                                connection,
                                SqlEstadosPersona),
                        estados_civiles =
                            CC_ReportesAlCorte_Lista_Consultar<CCGenericList>(
                                connection,
                                SqlEstadosCiviles),
                        estados_laborales =
                            CC_ReportesAlCorte_Lista_Consultar<CCGenericList>(
                                connection,
                                SqlEstadosLaborales)
                    });
        }

        /// <summary>
        /// Obtiene los períodos históricos.
        /// Mantiene el contrato utilizado por consumidores existentes.
        /// </summary>
        public List<CCGenericList>
            CC_Periodos_Obtener(
                int CodEmpresa)
        {
            return CC_ReportesAlCorte_Compatibilidad_Obtener(
                CodEmpresa,
                CC_ReportesAlCorte_Periodos_Consultar);
        }

        /// <summary>
        /// Obtiene las instituciones.
        /// Mantiene el contrato utilizado por consumidores existentes.
        /// </summary>
        public List<CCGenericList>
            CC_Instituciones_Obtener(
                int CodEmpresa)
        {
            return CC_ReportesAlCorte_Lista_Compatibilidad_Obtener(
                CodEmpresa,
                SqlInstituciones);
        }

        /// <summary>
        /// Obtiene las profesiones.
        /// Mantiene el contrato utilizado por consumidores existentes.
        /// </summary>
        public List<CCGenericList>
            CC_Profesiones_Obtener(
                int CodEmpresa)
        {
            return CC_ReportesAlCorte_Lista_Compatibilidad_Obtener(
                CodEmpresa,
                SqlProfesiones);
        }

        /// <summary>
        /// Obtiene los sectores.
        /// Mantiene el contrato utilizado por consumidores existentes.
        /// </summary>
        public List<CCGenericList>
            CC_Sectores_Obtener(
                int CodEmpresa)
        {
            return CC_ReportesAlCorte_Lista_Compatibilidad_Obtener(
                CodEmpresa,
                SqlSectores);
        }

        /// <summary>
        /// Obtiene las zonas.
        /// Mantiene el contrato utilizado por consumidores existentes.
        /// </summary>
        public List<CCGenericList>
            CC_Zonas_Obtener(
                int CodEmpresa)
        {
            return CC_ReportesAlCorte_Lista_Compatibilidad_Obtener(
                CodEmpresa,
                SqlZonas);
        }

        /// <summary>
        /// Obtiene los estados de persona.
        /// Mantiene el contrato utilizado por consumidores existentes.
        /// </summary>
        public List<CCGenericList>
            CC_Estados_Persona_Obtener(
                int CodEmpresa)
        {
            return CC_ReportesAlCorte_Lista_Compatibilidad_Obtener(
                CodEmpresa,
                SqlEstadosPersona);
        }

        /// <summary>
        /// Obtiene los tipos de garantía.
        /// Mantiene el contrato utilizado por consumidores existentes.
        /// </summary>
        public List<CCGenericList>
            CC_Garantias_Obtener(
                int CodEmpresa)
        {
            return CC_ReportesAlCorte_Lista_Compatibilidad_Obtener(
                CodEmpresa,
                SqlGarantias);
        }

        /// <summary>
        /// Obtiene las clasificaciones de cartera.
        /// Mantiene el contrato utilizado por consumidores existentes.
        /// </summary>
        public List<CCGenericList>
            CC_Carteras_Obtener(
                int CodEmpresa)
        {
            return CC_ReportesAlCorte_Lista_Compatibilidad_Obtener(
                CodEmpresa,
                SqlCarteras);
        }

        /// <summary>
        /// Obtiene las oficinas.
        /// Mantiene el contrato utilizado por consumidores existentes.
        /// </summary>
        public List<CCGenericList>
            CC_Oficinas_Obtener(
                int CodEmpresa)
        {
            return CC_ReportesAlCorte_Lista_Compatibilidad_Obtener(
                CodEmpresa,
                SqlOficinas);
        }

        /// <summary>
        /// Obtiene los estados civiles activos.
        /// Mantiene el contrato utilizado por consumidores existentes.
        /// </summary>
        public List<CCGenericList>
            CC_Estados_Civiles_Obtener(
                int CodEmpresa)
        {
            return CC_ReportesAlCorte_Lista_Compatibilidad_Obtener(
                CodEmpresa,
                SqlEstadosCiviles);
        }

        /// <summary>
        /// Obtiene los estados laborales activos.
        /// Mantiene el contrato utilizado por consumidores existentes.
        /// </summary>
        public List<CCGenericList>
            CC_Estados_Laborales_Obtener(
                int CodEmpresa)
        {
            return CC_ReportesAlCorte_Lista_Compatibilidad_Obtener(
                CodEmpresa,
                SqlEstadosLaborales);
        }

        /// <summary>
        /// Obtiene las provincias.
        /// Este método es utilizado también por Beneficios Integral
        /// y Juzgados; su contrato no debe modificarse.
        /// </summary>
        public List<CCGenericList>
            CC_Provincias_Obtener(
                int CodEmpresa)
        {
            return CC_ReportesAlCorte_Lista_Compatibilidad_Obtener(
                CodEmpresa,
                SqlProvincias);
        }

        /// <summary>
        /// Obtiene los cantones de una provincia.
        /// Este método es utilizado también por Beneficios Integral
        /// y Juzgados; su contrato no debe modificarse.
        /// </summary>
        public List<CCGenericList>
            CC_Cantones_Obtener(
                int CodEmpresa,
                int Provincia)
        {
            const string sql = """
                SELECT
                    CONVERT(varchar(20), Canton) AS idx,
                    RTRIM(Descripcion) AS itmx
                FROM Cantones
                WHERE Provincia = @Provincia
                ORDER BY Descripcion;
                """;

            return CC_ReportesAlCorte_Lista_Compatibilidad_Obtener(
                CodEmpresa,
                sql,
                new
                {
                    Provincia
                });
        }

        /// <summary>
        /// Obtiene los distritos de un cantón.
        /// Este método es utilizado también por Beneficios Integral
        /// y Juzgados; su contrato no debe modificarse.
        /// </summary>
        public List<CCGenericList>
            CC_Distritos_Obtener(
                int CodEmpresa,
                int Provincia,
                int Canton)
        {
            const string sql = """
                SELECT
                    CONVERT(varchar(20), Distrito) AS idx,
                    RTRIM(Descripcion) AS itmx
                FROM Distritos
                WHERE Provincia = @Provincia
                  AND Canton = @Canton
                ORDER BY Descripcion;
                """;

            return CC_ReportesAlCorte_Lista_Compatibilidad_Obtener(
                CodEmpresa,
                sql,
                new
                {
                    Provincia,
                    Canton
                });
        }

        /// <summary>
        /// Obtiene las líneas del catálogo.
        /// Mantiene el contrato utilizado por consumidores existentes.
        /// </summary>
        public List<CCGenericList>
            CC_Catalogo_Obtener(
                int CodEmpresa)
        {
            return CC_ReportesAlCorte_Lista_Compatibilidad_Obtener(
                CodEmpresa,
                SqlCatalogo);
        }

        /// <summary>
        /// Obtiene los destinos asociados a una línea o todos los destinos.
        /// Mantiene el contrato utilizado por consumidores existentes.
        /// </summary>
        public List<CCGenericList>
            CC_Catalogo_Destinos_Obtener(
                int CodEmpresa,
                string? CodCatalgo)
        {
            const string sqlFiltrado = """
                SELECT
                    RTRIM(destino.cod_destino) AS idx,
                    RTRIM(destino.descripcion) AS itmx
                FROM catalogo_destinos destino
                INNER JOIN catalogo_destinosAsg asignacion
                    ON destino.cod_destino = asignacion.cod_destino
                WHERE asignacion.codigo = @CodCatalgo
                ORDER BY destino.descripcion;
                """;

            const string sqlTodos = """
                SELECT
                    RTRIM(cod_destino) AS idx,
                    RTRIM(descripcion) AS itmx
                FROM catalogo_destinos
                ORDER BY descripcion;
                """;

            return string.IsNullOrWhiteSpace(
                CodCatalgo)
                ? CC_ReportesAlCorte_Lista_Compatibilidad_Obtener(
                    CodEmpresa,
                    sqlTodos)
                : CC_ReportesAlCorte_Lista_Compatibilidad_Obtener(
                    CodEmpresa,
                    sqlFiltrado,
                    new
                    {
                        CodCatalgo =
                            CodCatalgo.Trim()
                    });
        }

        /// <summary>
        /// Obtiene los grupos asociados a una línea o todos los grupos.
        /// Mantiene el contrato utilizado por consumidores existentes.
        /// </summary>
        public List<CCGenericList>
            CC_Catalogo_Grupos_Obtener(
                int CodEmpresa,
                string? CodCatalgo)
        {
            const string sqlFiltrado = """
                SELECT
                    RTRIM(grupo.cod_grupo) AS idx,
                    RTRIM(grupo.descripcion) AS itmx
                FROM catalogo_grupos grupo
                INNER JOIN catalogo_AsignaGrp asignacion
                    ON grupo.cod_grupo = asignacion.cod_grupo
                WHERE asignacion.codigo = @CodCatalgo
                ORDER BY grupo.descripcion;
                """;

            const string sqlTodos = """
                SELECT
                    RTRIM(cod_grupo) AS idx,
                    RTRIM(descripcion) AS itmx
                FROM catalogo_grupos
                ORDER BY descripcion;
                """;

            return string.IsNullOrWhiteSpace(
                CodCatalgo)
                ? CC_ReportesAlCorte_Lista_Compatibilidad_Obtener(
                    CodEmpresa,
                    sqlTodos)
                : CC_ReportesAlCorte_Lista_Compatibilidad_Obtener(
                    CodEmpresa,
                    sqlFiltrado,
                    new
                    {
                        CodCatalgo =
                            CodCatalgo.Trim()
                    });
        }

        /// <summary>
        /// Obtiene los departamentos asociados a una institución
        /// o todos los departamentos.
        /// Mantiene el contrato utilizado por consumidores existentes.
        /// </summary>
        public List<CCGenericList>
            CC_Departamentos_Obtener(
                int CodEmpresa,
                string? CodInstitucion)
        {
            const string sqlFiltrado = """
                SELECT
                    RTRIM(cod_departamento) AS idx,
                    RTRIM(descripcion) AS itmx
                FROM afDepartamentos
                WHERE cod_institucion = @CodInstitucion
                ORDER BY descripcion;
                """;

            const string sqlTodos = """
                SELECT
                    RTRIM(cod_departamento) AS idx,
                    RTRIM(descripcion) AS itmx
                FROM afDepartamentos
                ORDER BY descripcion;
                """;

            return string.IsNullOrWhiteSpace(
                CodInstitucion)
                ? CC_ReportesAlCorte_Lista_Compatibilidad_Obtener(
                    CodEmpresa,
                    sqlTodos)
                : CC_ReportesAlCorte_Lista_Compatibilidad_Obtener(
                    CodEmpresa,
                    sqlFiltrado,
                    new
                    {
                        CodInstitucion =
                            CodInstitucion.Trim()
                    });
        }

        /// <summary>
        /// Obtiene las secciones asociadas a una institución
        /// y departamento o todas las secciones.
        /// Mantiene el contrato utilizado por consumidores existentes.
        /// </summary>
        public List<CCGenericList>
            CC_Secciones_Obtener(
                int CodEmpresa,
                string? CodInstitucion,
                string? CodDepartamento)
        {
            const string sqlFiltrado = """
                SELECT
                    RTRIM(cod_seccion) AS idx,
                    RTRIM(descripcion) AS itmx
                FROM afSecciones
                WHERE cod_institucion = @CodInstitucion
                  AND cod_departamento = @CodDepartamento
                ORDER BY descripcion;
                """;

            const string sqlTodos = """
                SELECT
                    RTRIM(cod_seccion) AS idx,
                    RTRIM(descripcion) AS itmx
                FROM afSecciones
                ORDER BY descripcion;
                """;

            var tieneFiltros =
                !string.IsNullOrWhiteSpace(
                    CodInstitucion) &&
                !string.IsNullOrWhiteSpace(
                    CodDepartamento);

            return tieneFiltros
                ? CC_ReportesAlCorte_Lista_Compatibilidad_Obtener(
                    CodEmpresa,
                    sqlFiltrado,
                    new
                    {
                        CodInstitucion =
                            CodInstitucion!.Trim(),
                        CodDepartamento =
                            CodDepartamento!.Trim()
                    })
                : CC_ReportesAlCorte_Lista_Compatibilidad_Obtener(
                    CodEmpresa,
                    sqlTodos);
        }

        /// <summary>
        /// Obtiene la información de un reporte de análisis de cubos.
        /// El procedimiento se resuelve internamente a partir del código
        /// funcional del reporte.
        /// </summary>
        public ErrorDto<List<CbrAnalisisCubosData>>
            CC_ReportesAlCorte_AnalisisCubos_Obtener(
                int codEmpresa,
                CcReportesAlCorteProcesoRequest request)
        {
            var validacion =
                CC_ReportesAlCorte_Proceso_Validar(
                    request);

            if (validacion != null)
            {
                return DbHelper.CreateErrorResponse(
                    validacion,
                    -2,
                    new List<CbrAnalisisCubosData>());
            }

            var procedimiento =
                CC_ReportesAlCorte_Analisis_Procedimiento_Resolver(
                    request.codigo_reporte);

            if (procedimiento == null)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeReporteInvalido,
                    -2,
                    new List<CbrAnalisisCubosData>());
            }

            return CC_ReportesAlCorte_Procedimiento_Ejecutar
                <CbrAnalisisCubosData>(
                    codEmpresa,
                    procedimiento,
                    request);
        }

        /// <summary>
        /// Obtiene la información de un reporte de estimación.
        /// El procedimiento se resuelve internamente a partir del código
        /// funcional del reporte.
        /// </summary>
        public ErrorDto<List<CbrEstimacionData>>
            CC_ReportesAlCorte_Estimacion_Obtener(
                int codEmpresa,
                CcReportesAlCorteProcesoRequest request)
        {
            var validacion =
                CC_ReportesAlCorte_Proceso_Validar(
                    request);

            if (validacion != null)
            {
                return DbHelper.CreateErrorResponse(
                    validacion,
                    -2,
                    new List<CbrEstimacionData>());
            }

            var procedimiento =
                CC_ReportesAlCorte_Estimacion_Procedimiento_Resolver(
                    request.codigo_reporte);

            if (procedimiento == null)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeReporteInvalido,
                    -2,
                    new List<CbrEstimacionData>());
            }

            return CC_ReportesAlCorte_Procedimiento_Ejecutar
                <CbrEstimacionData>(
                    codEmpresa,
                    procedimiento,
                    request);
        }

        /// <summary>
        /// Ejecuta un procedimiento de análisis recibido por el contrato
        /// anterior. Solamente permite procedimientos conocidos.
        /// </summary>
        public List<CbrAnalisisCubosData>
            CbrAnalisis_Cubos_SP(
                int CodEmpresa,
                string nombreSP,
                int Anio,
                int Mes)
        {
            var procedimiento =
                CC_ReportesAlCorte_Analisis_Nombre_Resolver(
                    nombreSP);

            if (procedimiento == null)
            {
                throw new ArgumentException(
                    MensajeReporteInvalido,
                    nameof(nombreSP));
            }

            var request =
                new CcReportesAlCorteProcesoRequest
                {
                    codigo_reporte =
                        procedimiento,
                    anio =
                        Anio,
                    mes =
                        Mes
                };

            var validacion =
                CC_ReportesAlCorte_Proceso_Validar(
                    request);

            if (validacion != null)
            {
                throw new ArgumentException(
                    validacion,
                    nameof(Anio));
            }

            var response =
                CC_ReportesAlCorte_Procedimiento_Ejecutar
                    <CbrAnalisisCubosData>(
                        CodEmpresa,
                        procedimiento,
                        request);

            return CC_ReportesAlCorte_Compatibilidad_Resultado_Obtener(
                response);
        }

        /// <summary>
        /// Ejecuta un procedimiento de estimación recibido por el contrato
        /// anterior. Solamente permite procedimientos conocidos.
        /// </summary>
        public List<CbrEstimacionData>
            CbrEstimacion_SP(
                int CodEmpresa,
                string nombreSP,
                int Anio,
                int Mes)
        {
            var procedimiento =
                CC_ReportesAlCorte_Estimacion_Nombre_Resolver(
                    nombreSP);

            if (procedimiento == null)
            {
                throw new ArgumentException(
                    MensajeReporteInvalido,
                    nameof(nombreSP));
            }

            var request =
                new CcReportesAlCorteProcesoRequest
                {
                    codigo_reporte =
                        procedimiento,
                    anio =
                        Anio,
                    mes =
                        Mes
                };

            var validacion =
                CC_ReportesAlCorte_Proceso_Validar(
                    request);

            if (validacion != null)
            {
                throw new ArgumentException(
                    validacion,
                    nameof(Anio));
            }

            var response =
                CC_ReportesAlCorte_Procedimiento_Ejecutar
                    <CbrEstimacionData>(
                        CodEmpresa,
                        procedimiento,
                        request);

            return CC_ReportesAlCorte_Compatibilidad_Resultado_Obtener(
                response);
        }

        /// <summary>
        /// Ejecuta una consulta de lista para un endpoint compatible.
        /// </summary>
        private List<CCGenericList>
            CC_ReportesAlCorte_Lista_Compatibilidad_Obtener(
                int codEmpresa,
                string sql,
                object? parameters = null)
        {
            var response =
                DbHelper.ExecuteListQuery<CCGenericList>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    parameters);

            return CC_ReportesAlCorte_Compatibilidad_Resultado_Obtener(
                response);
        }

        /// <summary>
        /// Ejecuta una consulta compatible que necesita una transformación.
        /// </summary>
        private T
            CC_ReportesAlCorte_Compatibilidad_Obtener<T>(
                int codEmpresa,
                Func<SqlConnection, T> action)
        {
            var response =
                DbHelper.WithConn(
                    _portalDb,
                    codEmpresa,
                    action);

            return CC_ReportesAlCorte_Compatibilidad_Resultado_Obtener(
                response);
        }

        /// <summary>
        /// Convierte una respuesta estándar al contrato utilizado
        /// por los endpoints compartidos anteriores.
        /// </summary>
        private static T
            CC_ReportesAlCorte_Compatibilidad_Resultado_Obtener<T>(
                ErrorDto<T> response)
        {
            if (
                response.Code != 0 ||
                response.Result is null)
            {
                throw new InvalidOperationException(
                    response.Description);
            }

            return response.Result;
        }

        /// <summary>
        /// Consulta una lista utilizando una conexión ya resuelta.
        /// </summary>
        private static List<T>
            CC_ReportesAlCorte_Lista_Consultar<T>(
                SqlConnection connection,
                string sql,
                object? parameters = null)
        {
            return connection
                .Query<T>(
                    sql,
                    parameters)
                .ToList();
        }

        /// <summary>
        /// Consulta y formatea los períodos históricos.
        /// </summary>
        private static List<CCGenericList>
            CC_ReportesAlCorte_Periodos_Consultar(
                SqlConnection connection)
        {
            const string sql = """
                SELECT
                    id_per_historico,
                    Mes,
                    Anio
                FROM ase_per_historico
                ORDER BY Anio DESC, Mes DESC;
                """;

            return connection
                .Query<CcReportesAlCortePeriodoDbData>(
                    sql)
                .Select(periodo =>
                    new CCGenericList
                    {
                        idx =
                            periodo.id_per_historico,
                        itmx =
                            $"{periodo.anio} - " +
                            MColaboradorDB.ConvierteMes(
                                periodo.mes)
                    })
                .ToList();
        }

        /// <summary>
        /// Valida el código del reporte y el período solicitado.
        /// </summary>
        private static string?
            CC_ReportesAlCorte_Proceso_Validar(
                CcReportesAlCorteProcesoRequest? request)
        {
            if (
                request == null ||
                string.IsNullOrWhiteSpace(
                    request.codigo_reporte))
            {
                return MensajeReporteInvalido;
            }

            return request.anio <= 0 ||
                   request.mes is < 1 or > 12
                ? MensajePeriodoInvalido
                : null;
        }

        /// <summary>
        /// Resuelve el procedimiento de análisis a partir
        /// del código funcional del reporte.
        /// </summary>
        private static string?
            CC_ReportesAlCorte_Analisis_Procedimiento_Resolver(
                string codigoReporte)
        {
            return codigoReporte
                .Trim()
                .ToUpperInvariant() switch
            {
                "J04" =>
                    SpAnalisisComparativo,
                "J05" =>
                    SpAnalisisMorosidad,
                "J06" =>
                    SpAnalisisMorosidadTipo,
                "J07" =>
                    SpAnalisisAntiguedad,
                _ =>
                    null
            };
        }

        /// <summary>
        /// Resuelve el procedimiento de estimación a partir
        /// del código funcional del reporte.
        /// </summary>
        private static string?
            CC_ReportesAlCorte_Estimacion_Procedimiento_Resolver(
                string codigoReporte)
        {
            return codigoReporte
                .Trim()
                .ToUpperInvariant() switch
            {
                "J10" =>
                    SpEstimacionResumen,
                "J10D" =>
                    SpEstimacionDetalle,
                _ =>
                    null
            };
        }

        /// <summary>
        /// Valida un nombre de procedimiento de análisis
        /// recibido por el contrato anterior.
        /// </summary>
        private static string?
            CC_ReportesAlCorte_Analisis_Nombre_Resolver(
                string? nombreSp)
        {
            if (string.IsNullOrWhiteSpace(
                nombreSp))
            {
                return null;
            }

            if (
                nombreSp.Equals(
                    SpAnalisisComparativo,
                    StringComparison.OrdinalIgnoreCase))
            {
                return SpAnalisisComparativo;
            }

            if (
                nombreSp.Equals(
                    SpAnalisisMorosidad,
                    StringComparison.OrdinalIgnoreCase))
            {
                return SpAnalisisMorosidad;
            }

            if (
                nombreSp.Equals(
                    SpAnalisisMorosidadTipo,
                    StringComparison.OrdinalIgnoreCase))
            {
                return SpAnalisisMorosidadTipo;
            }

            return nombreSp.Equals(
                SpAnalisisAntiguedad,
                StringComparison.OrdinalIgnoreCase)
                ? SpAnalisisAntiguedad
                : null;
        }

        /// <summary>
        /// Valida un nombre de procedimiento de estimación
        /// recibido por el contrato anterior.
        /// </summary>
        private static string?
            CC_ReportesAlCorte_Estimacion_Nombre_Resolver(
                string? nombreSp)
        {
            if (string.IsNullOrWhiteSpace(
                nombreSp))
            {
                return null;
            }

            if (
                nombreSp.Equals(
                    SpEstimacionResumen,
                    StringComparison.OrdinalIgnoreCase))
            {
                return SpEstimacionResumen;
            }

            return nombreSp.Equals(
                SpEstimacionDetalle,
                StringComparison.OrdinalIgnoreCase)
                ? SpEstimacionDetalle
                : null;
        }

        /// <summary>
        /// Ejecuta uno de los procedimientos almacenados
        /// previamente validados.
        /// </summary>
        private ErrorDto<List<T>>
            CC_ReportesAlCorte_Procedimiento_Ejecutar<T>(
                int codEmpresa,
                string procedimiento,
                CcReportesAlCorteProcesoRequest request)
        {
            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                    connection
                        .Query<T>(
                            procedimiento,
                            new
                            {
                                Anio =
                                    request.anio,
                                Mes =
                                    request.mes
                            },
                            commandType:
                                CommandType.StoredProcedure,
                            commandTimeout:
                                0)
                        .ToList());
        }

        private sealed class CcReportesAlCortePeriodoDbData
        {
            public string id_per_historico { get; set; } =
                string.Empty;

            public int mes { get; set; }

            public int anio { get; set; }
        }
    }
}