using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Acceso a datos de Remesas de Pago Fosol (frmFSL_RemesasPago).
    /// Consultas de remesas aquí; remesas, cargas y traslados en parciales.
    /// </summary>
    public partial class FrmFslRemesasPagoDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _bitacoraDb;
        private readonly MBeneficiosDB _mBeneficiosDB;

        /// <summary>
        /// Inicializa el acceso a datos y sus dependencias con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmFslRemesasPagoDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _bitacoraDb = new MSecurityMainDb(_config);
            _mBeneficiosDB = new MBeneficiosDB(_config);
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>Registra un movimiento en la bitácora de seguridad.</summary>
        public ErrorDto Bitacora(BitacoraInsertarDto data) => _bitacoraDb.Bitacora(data);

        /// <summary>
        /// Obtiene las fechas de una remesa de tesorería.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="cod_remesa">Código de la remesa.</param>
        /// <returns>Fechas de la remesa.</returns>
        public ErrorDto<List<FslRemesasListaDatos>> FslFechas_Obtener(int CodEmpresa, int cod_remesa)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                const string sql = "SELECT fecha_inicio, fecha_corte FROM FSL_REMESAS_TESORERIA WHERE TESORERIA_REMESA = @cod_remesa";
                return connection.Query<FslRemesasListaDatos>(sql, new { cod_remesa }).ToList();
            });
        }

        /// <summary>
        /// Obtiene la lista de remesas de tesorería con paginación y filtro.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtro">Filtro de búsqueda.</param>
        /// <param name="pagina">Offset de paginación.</param>
        /// <param name="paginacion">Cantidad de registros por página.</param>
        /// <returns>Lista de remesas y total.</returns>
        public ErrorDto<FslRemesasLista> FslRemesas_Obtener(int CodEmpresa, string? filtro, int? pagina, int? paginacion)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var response = new FslRemesasLista();

                var like = string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro}%";
                var whereClause = @"WHERE (@like IS NULL OR TESORERIA_REMESA LIKE @like OR REGISTRO_USUARIO LIKE @like
                                       OR REGISTRO_FECHA LIKE @like OR notas LIKE @like)";

                var sqlCount = $"SELECT COUNT(*) FROM FSL_REMESAS_TESORERIA {whereClause}";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount, new { like });

                var offset = pagina ?? 0;
                var fetch = paginacion ?? 10;
                var sql = $@"SELECT * FROM FSL_REMESAS_TESORERIA {whereClause}
                             ORDER BY registro_fecha DESC
                             OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.Lista = connection.Query<FslRemesasListaDatos>(sql, new { like, offset, fetch }).ToList();
                return response;
            });
        }
    }
}
