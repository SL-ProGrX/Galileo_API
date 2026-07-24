using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del Traslado de Beneficios a Tesorería (frmAF_BeneficiosTraslado).
    /// Consultas de remesas aquí; cargas, traslado, informes y notificaciones en parciales.
    /// </summary>
    public partial class FrmAfBeneficiosTrasladoDB
    {
        private const string FechaFormat = "yyyy/MM/dd";

        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _bitacoraDb;
        private readonly MBeneficiosDB _mBeneficiosDB;
        private readonly MTesFuncionesDb _mTes;
        private readonly EnvioCorreoDB _envioCorreoDB;
        private readonly MProGrXAuxiliarDB _mAuxiliarDB;
        private readonly MTesoreria _mTesoreria;
        private readonly string _sendEmail;
        private readonly string _testMail;
        private readonly string _notificaciones;
        private readonly string _codComision;
        private readonly string _ctaComision;

        /// <summary>
        /// Inicializa el acceso a datos y sus dependencias con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficiosTrasladoDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _bitacoraDb = new MSecurityMainDb(_config);
            _mBeneficiosDB = new MBeneficiosDB(_config);
            _mTes = new MTesFuncionesDb(_config);
            _envioCorreoDB = new EnvioCorreoDB(_config);
            _mAuxiliarDB = new MProGrXAuxiliarDB(_config);
            _mTesoreria = new MTesoreria(_config);
            _sendEmail = _config.GetSection("AppSettings").GetSection("EnviaEmail").Value ?? string.Empty;
            _testMail = _config.GetSection("AppSettings").GetSection("TestEmail").Value ?? string.Empty;
            _notificaciones = _config.GetSection("AppSettings").GetSection("Notificaciones").Value ?? string.Empty;
            _codComision = _config.GetSection("AFI_Beneficios").GetSection("CodComision").Value ?? string.Empty;
            _ctaComision = _config.GetSection("AFI_Beneficios").GetSection("CtaComisionBeneficios").Value ?? string.Empty;
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>Registra un movimiento en la bitácora de seguridad.</summary>
        public ErrorDto Bitacora(BitacoraInsertarDto data) => _bitacoraDb.Bitacora(data);

        /// <summary>
        /// Obtiene la lista de remesas de traslado con paginación y filtro.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtros">JSON con filtro de búsqueda y paginación.</param>
        /// <returns>Lista de remesas y total.</returns>
        public ErrorDto<AfiBeneficiosRemesasDtoLista> AfiRemesas_Obtener(int CodCliente, string filtros)
        {
            var filtro = string.IsNullOrWhiteSpace(filtros)
                ? new AfiRemesasFiltros()
                : JsonConvert.DeserializeObject<AfiRemesasFiltros>(filtros) ?? new AfiRemesasFiltros();
            var filtroTexto = filtro.filtro?.Trim() ?? string.Empty;
            var aplicaFiltro = filtroTexto.Length > 0;
            var offset = filtro.pagina ?? 0;
            var fetch = filtro.pagina.HasValue ? filtro.paginacion ?? 10 : int.MaxValue;
            var parametros = new
            {
                aplicaFiltro,
                filtroLike = aplicaFiltro ? $"%{filtroTexto}%" : string.Empty,
                offset,
                fetch
            };

            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new AfiBeneficiosRemesasDtoLista();

                const string sqlCount = @"SELECT COUNT(*)
                                          FROM AFI_BENEFICIOS_REMESAS
                                          WHERE @aplicaFiltro = 0
                                             OR CONVERT(VARCHAR(20), cod_remesa) LIKE @filtroLike
                                             OR usuario LIKE @filtroLike
                                             OR estado LIKE @filtroLike
                                             OR CONVERT(VARCHAR(19), fecha, 120) LIKE @filtroLike";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount, parametros);

                const string sql = @"SELECT *
                                     FROM AFI_BENEFICIOS_REMESAS
                                     WHERE @aplicaFiltro = 0
                                        OR CONVERT(VARCHAR(20), cod_remesa) LIKE @filtroLike
                                        OR usuario LIKE @filtroLike
                                        OR estado LIKE @filtroLike
                                        OR CONVERT(VARCHAR(19), fecha, 120) LIKE @filtroLike
                                     ORDER BY fecha DESC
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";
                response.Beneficios = connection.Query<AfiBeneficiosRemesasDto>(sql, parametros).ToList();
                return response;
            });
        }

        /// <summary>
        /// Obtiene una remesa por su código.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_remesa">Código de la remesa.</param>
        /// <returns>Datos de la remesa.</returns>
        public ErrorDto<AfiBeneficiosRemesasDto> AfiRemesa_Obtener(int CodCliente, int cod_remesa)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = "SELECT * FROM AFI_BENEFICIOS_REMESAS WHERE Cod_Remesa = @cod_remesa";
                return connection.QueryFirstOrDefault<AfiBeneficiosRemesasDto>(sql, new { cod_remesa }) ?? new AfiBeneficiosRemesasDto();
            });
        }

        /// <summary>
        /// Obtiene las oficinas con créditos pendientes de traslado en un rango de fechas.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="inicio">Fecha inicial (yyyy-MM-dd).</param>
        /// <param name="corte">Fecha de corte (yyyy-MM-dd).</param>
        /// <returns>Lista de oficinas.</returns>
        public ErrorDto<List<AfiBeneTrasladoOpciones>> AfiRemesaOficinasFechas_Obtener(int CodCliente, string inicio, string corte)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT RTRIM(cod_oficina) AS item, RTRIM(descripcion) AS descripcion
                                     FROM SIF_Oficinas
                                     WHERE cod_oficina IN (
                                        SELECT R.cod_oficina_R
                                        FROM reg_creditos R
                                        INNER JOIN Catalogo C ON R.codigo = C.codigo AND C.retencion = 'N' AND C.poliza = 'N'
                                        WHERE R.estadosol = 'F'
                                          AND R.fechaforp BETWEEN @inicio AND @corte
                                          AND R.tesoreria IS NULL AND R.estado IN ('A','C')
                                        GROUP BY R.cod_oficina_R)
                                     ORDER BY cod_oficina";

                var inicioFecha = inicio.Replace("-", "/") + " 00:00:00";
                var corteFecha = corte.Replace("-", "/") + " 23:59:59";
                return connection.Query<AfiBeneTrasladoOpciones>(sql, new { inicio = inicioFecha, corte = corteFecha }).ToList();
            });
        }

        /// <summary>
        /// Obtiene el catálogo completo de oficinas.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de oficinas.</returns>
        public ErrorDto<List<AfiBeneTrasladoOpciones>> AfiRemesaOficinas_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT RTRIM(cod_oficina) AS item, RTRIM(descripcion) AS descripcion
                                     FROM SIF_Oficinas ORDER BY cod_oficina";
                return connection.Query<AfiBeneTrasladoOpciones>(sql).ToList();
            });
        }
    }
}
