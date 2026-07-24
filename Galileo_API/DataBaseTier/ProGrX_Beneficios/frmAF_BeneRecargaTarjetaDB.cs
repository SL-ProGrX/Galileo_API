using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos de Recarga de Tarjetas de Beneficios (frmAF_BeneRecargaTarjeta).
    /// Constructor y consultas de remesas aquí; el resto en parciales por responsabilidad.
    /// </summary>
    public partial class FrmAfBeneRecargaTarjetaDB
    {
        private const string FechaFormat = "yyyy/MM/dd";

        private readonly IConfiguration _config;
        private readonly EnvioCorreoDB _envioCorreoDB;
        private readonly MSecurityMainDb _bitacoraDb;
        private readonly MTesFuncionesDb _mTes;
        private readonly MBeneficiosDB _mBeneficiosDB;
        private readonly string _sendEmail;

        /// <summary>
        /// Inicializa el acceso a datos y sus dependencias con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneRecargaTarjetaDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _envioCorreoDB = new EnvioCorreoDB(_config);
            _bitacoraDb = new MSecurityMainDb(_config);
            _mTes = new MTesFuncionesDb(_config);
            _mBeneficiosDB = new MBeneficiosDB(_config);
            _sendEmail = _config.GetSection("AppSettings").GetSection("EnviaEmail").Value ?? string.Empty;
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>Registra un movimiento en la bitácora de seguridad.</summary>
        public ErrorDto Bitacora(BitacoraInsertarDto data) => _bitacoraDb.Bitacora(data);

        /// <summary>
        /// Obtiene la lista de remesas de tarjetas con paginación y filtro.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="filtro">Filtro de búsqueda.</param>
        /// <param name="pagina">Offset de paginación.</param>
        /// <param name="paginacion">Cantidad de registros por página.</param>
        /// <returns>Lista de remesas y total.</returns>
        public ErrorDto<AfiBeneTarjetasRemesasDataLista> AfiTajertasRemesas_Obtener(int CodCliente, string? filtro, int? pagina, int? paginacion)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var response = new AfiBeneTarjetasRemesasDataLista();

                const string sqlCount = "SELECT COUNT(*) FROM AFI_BENE_TARJETAS_REMESAS";
                response.Total = connection.QueryFirstOrDefault<int>(sqlCount);

                var like = string.IsNullOrWhiteSpace(filtro) ? null : $"%{filtro}%";
                var offset = pagina ?? 0;
                var fetch = paginacion ?? 10;

                const string sql = @"SELECT * FROM AFI_BENE_TARJETAS_REMESAS
                                     WHERE (@like IS NULL OR cod_remesa_tr LIKE @like OR registro_usuario LIKE @like
                                            OR registro_fecha LIKE @like OR estado LIKE @like)
                                     ORDER BY registro_fecha DESC
                                     OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY";

                response.Beneficios = connection.Query<AfiBeneTarjetasRemesasData>(sql, new { like, offset, fetch }).ToList();
                return response;
            });
        }

        /// <summary>
        /// Obtiene una remesa de tarjetas por su código.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_remesa">Código de la remesa.</param>
        /// <returns>Datos de la remesa.</returns>
        public ErrorDto<AfiBeneTarjetasRemesasData> AfiTarjetasRemesa_Obtener(int CodCliente, int cod_remesa)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = "SELECT * FROM AFI_BENE_TARJETAS_REMESAS WHERE cod_remesa_tr = @cod_remesa";
                return connection.QueryFirstOrDefault<AfiBeneTarjetasRemesasData>(sql, new { cod_remesa }) ?? new AfiBeneTarjetasRemesasData();
            });

            return result;
        }

        /// <summary>
        /// Obtiene las remesas de tarjetas abiertas (estado 'A').
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de remesas abiertas.</returns>
        public ErrorDto<List<AfiBeneTarjetasRemesasData>> AfiTarjetasRemesasAbiertas_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT *, CONCAT(COD_REMESA_TR, REGISTRO_USUARIO, REGISTRO_FECHA, FECHA_INICIO, FECHA_CORTE) AS DESCRIPCION
                                     FROM AFI_BENE_TARJETAS_REMESAS WHERE estado = 'A' ORDER BY REGISTRO_FECHA DESC";
                return connection.Query<AfiBeneTarjetasRemesasData>(sql).ToList();
            });
        }
    }
}
