using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier
{
    public class FrmAFPlanillaEnviaDB
    {
        private readonly IConfiguration _config;

        private const string SqlInstituciones = @"
                    SELECT cod_institucion AS item,
                           RTRIM(descripcion) AS descripcion
                    FROM dbo.instituciones
                    WHERE activa = 1
                    ORDER BY descripcion;";

        private const string SqlPeriodosProceso = @"
                    ;WITH Periodos AS (
                        SELECT dbo.fxSIFPrmProcesoAnt(
                                   dbo.fxSIFPrmProcesoAnt(YEAR(dbo.MyGetdate()) * 100 + MONTH(dbo.MyGetdate()))
                               ) AS item,
                               0 AS Orden
                        UNION ALL
                        SELECT dbo.fxSIFPrmProcesoSig(item), Orden + 1
                        FROM Periodos
                        WHERE Orden < 6
                    )
                    SELECT item
                    FROM Periodos
                    ORDER BY item;";

        private const string SpArchivoObtener = "spPrm_Formato_PG_Soc";

        private const string SqlPlanillaEnvio = @"
                    SELECT ISNULL(PLANILLA_ENVIO, '') AS planillaEnvio,
                           LTRIM(RTRIM(ISNULL(codigo_inst_deduc, ''))) AS codigoInstDeduc
                    FROM dbo.INSTITUCIONES
                    WHERE COD_INSTITUCION = @CodInstitucion;";

        private const string SqlArchivoF15 = @"
                    SELECT S.CEDULA,
                           S.NOMBRE,
                           RIGHT('0000000000' + LTRIM(RTRIM(CONVERT(varchar(20), S.CEDULA))), 10)
                             + CHAR(9) + '463020'
                             + CHAR(9) + CONVERT(varchar(30), CONVERT(decimal(18, 2), 3.5))
                             + CHAR(9) + '1' AS cadena
                    FROM dbo.SOCIOS S
                    WHERE S.ESTADOACTUAL = 'S'
                      AND S.COD_INSTITUCION = @CodInstitucion
                      AND S.CEDULA NOT IN (
                          SELECT CEDULA
                          FROM dbo.AFI_CR_RENUNCIAS
                          WHERE ESTADO = 'T'
                            AND DATEDIFF(DAY, REGISTRO_FECHA, dbo.mygetdate()) <= 30
                      )
                    ORDER BY S.CEDULA;";

        public FrmAFPlanillaEnviaDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene las instituciones activas para envio de planillas.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa.</param>
        /// <returns>Listado de instituciones activas.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Instituciones_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlInstituciones);
        }

        /// <summary>
        /// Obtiene los periodos de proceso disponibles.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa.</param>
        /// <returns>Listado de periodos de proceso.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_PeriodosProceso_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlPeriodosProceso);
        }

        /// <summary>
        /// Obtiene el archivo de planilla PG generado para una institucion y periodo.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa.</param>
        /// <param name="codinstitucion">Codigo de institucion.</param>
        /// <param name="fechaproceso">Fecha de proceso.</param>
        /// <returns>Resultado del archivo PG generado.</returns>
        public ErrorDto<List<AfArchivoResultadoDto>> AF_Archivo_Obtener(int CodEmpresa, string codinstitucion, string fechaproceso)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                CompletarCadenasPg(connection.Query<AfArchivoResultadoDto>(
                    SpArchivoObtener,
                    new
                    {
                        Institucion = NormalizarEntero(codinstitucion),
                        Proceso = NormalizarEntero(fechaproceso)
                    },
                    commandType: System.Data.CommandType.StoredProcedure).ToList()));
        }

        /// <summary>
        /// Obtiene la planilla lista para descargar segun el formato configurado en la institucion.
        /// </summary>
        /// <param name="CodEmpresa">Codigo de empresa.</param>
        /// <param name="codinstitucion">Codigo de institucion.</param>
        /// <param name="fechaproceso">Fecha de proceso.</param>
        /// <returns>Datos del archivo, formato y nombre sugerido.</returns>
        public ErrorDto<AfArchivoPlanillaDto> AF_ArchivoPlanilla_Obtener(int CodEmpresa, string codinstitucion, string fechaproceso)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var codInstitucion = NormalizarEntero(codinstitucion);
                var fechaProceso = NormalizarTexto(fechaproceso);
                var institucion = connection.QueryFirstOrDefault<InstitucionPlanillaDto>(
                    SqlPlanillaEnvio,
                    new { CodInstitucion = codInstitucion });

                var planillaEnvio = string.IsNullOrWhiteSpace(institucion?.PlanillaEnvio)
                    ? "29"
                    : institucion.PlanillaEnvio.Trim();

                var registros = planillaEnvio == "15"
                    ? connection.Query<AfArchivoResultadoDto>(
                        SqlArchivoF15,
                        new { CodInstitucion = codInstitucion }).ToList()
                    : CompletarCadenasPg(connection.Query<AfArchivoResultadoDto>(
                        SpArchivoObtener,
                        new
                        {
                            Institucion = codInstitucion,
                            Proceso = NormalizarEntero(fechaProceso)
                        },
                        commandType: System.Data.CommandType.StoredProcedure).ToList());

                return new AfArchivoPlanillaDto
                {
                    planillaEnvio = planillaEnvio,
                    nombreArchivo = CrearNombreArchivo(planillaEnvio, institucion?.CodigoInstDeduc),
                    registros = registros
                };
            });
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuracion inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();

        private static int NormalizarEntero(string? valor)
        {
            var texto = NormalizarTexto(valor);

            if (int.TryParse(texto, out var entero))
            {
                return entero;
            }

            return decimal.TryParse(texto, out var numero)
                ? Convert.ToInt32(Math.Truncate(numero))
                : 0;
        }

        private static string CrearNombreArchivo(string planillaEnvio, string? codigoInstitucion)
        {
            var codigo = NormalizarTexto(codigoInstitucion);

            return planillaEnvio == "15"
                ? $"E-{codigo}-{DateTime.Now:yyyyMMdd}-01.CIF"
                : $"Asoc-{codigo}-{DateTime.Now:yyyyMMdd}-PG.csv";
        }

        private static List<AfArchivoResultadoDto> CompletarCadenasPg(List<AfArchivoResultadoDto> registros)
        {
            foreach (var registro in registros)
            {
                if (!string.IsNullOrWhiteSpace(registro.cadena))
                {
                    continue;
                }

                registro.cadena = string.Join(",",
                    NormalizarTexto(registro.col_01),
                    NormalizarTexto(registro.col_02),
                    NormalizarTexto(registro.col_03),
                    NormalizarTexto(registro.col_04),
                    NormalizarTexto(registro.col_05),
                    NormalizarTexto(registro.col_06),
                    NormalizarTexto(registro.col_07),
                    NormalizarTexto(registro.col_08),
                    NormalizarTexto(registro.col_09),
                    NormalizarTexto(registro.col_10),
                    NormalizarTexto(registro.col_11));
            }

            return registros;
        }

        private sealed class InstitucionPlanillaDto
        {
            public InstitucionPlanillaDto(string? planillaEnvio, string? codigoInstDeduc)
            {
                PlanillaEnvio = NormalizarTexto(planillaEnvio);
                CodigoInstDeduc = NormalizarTexto(codigoInstDeduc);
            }

            public string PlanillaEnvio { get; }
            public string CodigoInstDeduc { get; }
        }
    }
}
