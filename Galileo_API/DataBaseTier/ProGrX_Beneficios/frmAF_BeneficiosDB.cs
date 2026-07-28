using Dapper;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>
    /// Acceso a datos del Mantenimiento de Beneficios (frmAF_Beneficios).
    /// Consultas y bitácora aquí; guardado en .Guardar y montos/fechas en .Montos.
    /// </summary>
    public partial class FrmAfBeneficiosDB
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _bitacoraDb;
        private readonly MBeneficiosDB _mBeneficiosDB;

        /// <summary>
        /// Inicializa el acceso a datos y las dependencias (bitácora) con la configuración inyectada.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmAfBeneficiosDB(IConfiguration config)
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
        /// Obtiene el primer código de beneficio para navegación (scroll).
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="Scroll">1 = siguiente, otro = anterior.</param>
        /// <param name="Cod_Beneficio">Código actual de referencia.</param>
        /// <returns>Código de beneficio en Description.</returns>
        public ErrorDto Top1Beneficio_Obtener(int CodCliente, int Scroll, string Cod_Beneficio)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                if (string.IsNullOrEmpty(Cod_Beneficio))
                {
                    return connection.QueryFirstOrDefault<string>("SELECT TOP 1 Cod_Beneficio FROM afi_beneficios");
                }

                var sql = Scroll == 1
                    ? "SELECT TOP 1 Cod_Beneficio FROM afi_beneficios WHERE Cod_Beneficio > @cod ORDER BY Cod_Beneficio ASC"
                    : "SELECT TOP 1 Cod_Beneficio FROM afi_beneficios WHERE Cod_Beneficio < @cod ORDER BY Cod_Beneficio DESC";

                return connection.QueryFirstOrDefault<string>(sql, new { cod = Cod_Beneficio });
            });

            return new ErrorDto { Code = result.Code, Description = result.Result ?? string.Empty };
        }

        /// <summary>
        /// Obtiene el detalle de un beneficio.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="Cod_Beneficio">Código del beneficio.</param>
        /// <returns>Datos del beneficio.</returns>
        public ErrorDto<AfiBeneficiosDto> AfiBeneficioDTO_Obtener(int CodCliente, string Cod_Beneficio)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = "SELECT * FROM afi_beneficios WHERE Cod_Beneficio = @Cod_Beneficio";
                return connection.QueryFirstOrDefault<AfiBeneficiosDto>(sql, new { Cod_Beneficio }) ?? new AfiBeneficiosDto();
            });
        }

        /// <summary>
        /// Obtiene los montos configurados de un beneficio.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="Cod_Beneficio">Código del beneficio.</param>
        /// <returns>Lista de montos.</returns>
        public ErrorDto<List<AfiBeneficioMontoData>> AfiBeneficioMontos_Obtener(int CodCliente, string Cod_Beneficio)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT id_bene, inicio, corte, monto FROM afi_beneficio_montos
                                     WHERE cod_beneficio = @Cod_Beneficio";
                return connection.Query<AfiBeneficioMontoData>(sql, new { Cod_Beneficio }).ToList();
            });
        }

        /// <summary>
        /// Obtiene los grupos y su marca de asignación a un beneficio.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="Cod_Beneficio">Código del beneficio.</param>
        /// <returns>Lista de grupos.</returns>
        public ErrorDto<List<AfiBeneficioGruposData>> AfiBeneficioGrupos_Obtener(int CodCliente, string Cod_Beneficio)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT B.cod_grupo AS Grupo, B.descripcion,
                                        CASE WHEN A.cod_grupo IS NOT NULL THEN 1 ELSE 0 END AS cod_grupo
                                     FROM AFI_BENEFICIO_GRUPOS B
                                     LEFT JOIN AFI_BENE_GRUPOSB A ON B.cod_grupo = A.cod_grupo AND A.cod_beneficio = @Cod_Beneficio
                                     ORDER BY A.cod_grupo DESC, B.descripcion ASC";
                return connection.Query<AfiBeneficioGruposData>(sql, new { Cod_Beneficio }).ToList();
            });
        }

        /// <summary>
        /// Obtiene el nombre de una cuenta contable.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cuenta">Código de la cuenta.</param>
        /// <returns>Descripción de la cuenta en Description.</returns>
        public ErrorDto NombreCuenta_Obtener(int CodCliente, string cuenta)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = "SELECT descripcion FROM CntX_Cuentas WHERE COD_CUENTA = @cuenta ORDER BY cod_cuenta";
                return connection.QueryFirstOrDefault<string>(sql, new { cuenta });
            });

            return new ErrorDto { Code = result.Code, Description = result.Result ?? string.Empty };
        }

        /// <summary>
        /// Obtiene el catálogo de categorías de beneficios activas.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <returns>Lista de categorías.</returns>
        public ErrorDto<List<AfiBeneListas>> AfiBeneCategoria_Obtener(int CodCliente)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT cod_categoria AS item, descripcion FROM afi_bene_categorias
                                     WHERE Activo = 1 ORDER BY descripcion";
                return connection.Query<AfiBeneListas>(sql).ToList();
            });
        }

        /// <summary>
        /// Obtiene los grupos de una categoría de beneficios.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="categoria">Código de la categoría.</param>
        /// <returns>Lista de grupos.</returns>
        public ErrorDto<List<AfiBeneListas>> AfiBeneGrupos_Obtener(int CodCliente, string categoria)
        {
            return DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                const string sql = @"SELECT COD_GRUPO AS item, DESCRIPCION AS descripcion FROM AFI_BENE_GRUPOS
                                     WHERE Cod_Categoria = @categoria AND Estado = 1 ORDER BY DESCRIPCION";
                return connection.Query<AfiBeneListas>(sql, new { categoria }).ToList();
            });
        }

        /// <summary>
        /// Obtiene la bitácora de un beneficio (por beneficio, grupo y categoría).
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Cod_Beneficio">Código del beneficio.</param>
        /// <param name="Consec">Consecutivo (no usado en el filtro actual).</param>
        /// <param name="cod_grupo">Código del grupo.</param>
        /// <param name="cod_categoria">Código de la categoría.</param>
        /// <returns>Lista de movimientos de bitácora.</returns>
        public ErrorDto<List<BitacoraBeneficioDto>> BitacoraBeneficio_Obtener(int CodEmpresa, string Cod_Beneficio, int Consec, string? cod_grupo, string? cod_categoria)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                const string sql = @"SELECT * FROM (
                                        SELECT B.ID_BITACORA, B.CONSEC, B.REGISTRO_FECHA, B.COD_BENEFICIO, B.REGISTRO_USUARIO, B.DETALLE, B.MOVIMIENTO
                                        FROM AFI_BENE_REGISTRO_BITACORA B WHERE B.COD_BENEFICIO = @cod_grupo AND B.CONSEC = -2
                                        UNION ALL
                                        SELECT B.ID_BITACORA, B.CONSEC, B.REGISTRO_FECHA, B.COD_BENEFICIO, B.REGISTRO_USUARIO, B.DETALLE, B.MOVIMIENTO
                                        FROM AFI_BENE_REGISTRO_BITACORA B WHERE B.COD_BENEFICIO = @Cod_Beneficio AND B.CONSEC = -1
                                        UNION ALL
                                        SELECT B.ID_BITACORA, B.CONSEC, B.REGISTRO_FECHA, B.COD_BENEFICIO, B.REGISTRO_USUARIO, B.DETALLE, B.MOVIMIENTO
                                        FROM AFI_BENE_REGISTRO_BITACORA B WHERE B.COD_BENEFICIO = @cod_categoria AND B.CONSEC = -2
                                     ) T
                                     ORDER BY T.REGISTRO_FECHA DESC";
                return connection.Query<BitacoraBeneficioDto>(sql, new { cod_grupo, Cod_Beneficio, cod_categoria }).ToList();
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<List<BitacoraBeneficioDto>>("BitacoraBeneficio_Obtener: " + result.Description);
            }

            return result;
        }

        /// <summary>
        /// Registra un movimiento en la bitácora de beneficios (consec = -1).
        /// </summary>
        private void RegistrarBitacora(int CodCliente, string movimiento, string detalle, string codBeneficio, string registraUser)
        {
            _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDto
            {
                EmpresaId = CodCliente,
                cod_beneficio = codBeneficio,
                consec = -1,
                movimiento = movimiento,
                detalle = detalle,
                registro_usuario = registraUser
            });
        }
    }
}
