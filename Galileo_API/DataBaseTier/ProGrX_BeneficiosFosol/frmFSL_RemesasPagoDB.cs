using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public sealed partial class FrmFslRemesasPagoDB
    {
        private const int ModuloFosol = 22;
        private const int CodigoValidacion = -2;

        private const string MensajeRemesaRequerida =
            "El c&oacute;digo de la remesa es requerido.";

        private const string MensajeUsuarioRequerido =
            "El usuario es requerido.";

        private const string MensajeRemesaNoExiste =
            "La remesa indicada no existe.";

        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly MBeneficiosDB _mBeneficiosDb;
        private readonly MTesFuncionesDb _mTesFuncionesDb;

        public FrmFslRemesasPagoDB(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
            _mBeneficiosDb = new MBeneficiosDB(config);
            _mTesFuncionesDb = new MTesFuncionesDb(config);
        }

        /// <summary>
        /// Obtiene una remesa de tesorería por código.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="codRemesa">Código de la remesa.</param>
        /// <returns>Información de la remesa.</returns>
        public ErrorDto<FslRemesaDto?>
            FSL_RemesasPago_Remesa_Obtener(
                int CodEmpresa,
                long codRemesa)
        {
            if (codRemesa <= 0)
            {
                return DbHelper.CreateErrorResponse<FslRemesaDto?>(
                    MensajeRemesaRequerida,
                    CodigoValidacion);
            }

            const string sql = """
                SELECT
                    R.TESORERIA_REMESA AS tesoreria_remesa,
                    LTRIM(RTRIM(ISNULL(R.REGISTRO_USUARIO, '')))
                        AS registro_usuario,
                    R.REGISTRO_FECHA AS registro_fecha,
                    R.FECHA_INICIO AS fecha_inicio,
                    R.FECHA_CORTE AS fecha_corte,
                    LTRIM(RTRIM(ISNULL(R.NOTAS, ''))) AS notas,
                    LTRIM(RTRIM(ISNULL(R.ESTADO, ''))) AS estado,
                    CASE LTRIM(RTRIM(ISNULL(R.ESTADO, '')))
                        WHEN 'A' THEN 'Remesa Abierta'
                        WHEN 'C' THEN 'Remesa Cerrada'
                        WHEN 'T' THEN 'Remesa Trasladada'
                        ELSE ''
                    END AS estado_descripcion
                FROM FSL_REMESAS_TESORERIA R
                WHERE R.TESORERIA_REMESA = @codRemesa;
                """;

            return DbHelper.ExecuteSingleQuery<FslRemesaDto>(
                _portalDb,
                CodEmpresa,
                sql,
                defaultValue: null,
                parameters: new
                {
                    codRemesa
                });
        }

        /// <summary>
        /// Obtiene las remesas de tesorería con filtro y paginación.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros de la consulta.</param>
        /// <returns>Lista paginada de remesas.</returns>
        public ErrorDto<
            FslListaPaginadaDto<FslRemesaDto>>
            FSL_RemesasPago_Remesas_Obtener(
                int CodEmpresa,
                FslRemesasFiltros filtros)
        {
            ArgumentNullException.ThrowIfNull(filtros);

            var filtro = filtros.filtro.Trim();
            var like = string.IsNullOrWhiteSpace(filtro)
                ? null
                : $"%{filtro}%";

            var offset = Math.Max(filtros.pagina, 0);
            var fetch = Math.Clamp(
                filtros.paginacion,
                1,
                500);

            var sortField =
                filtros.sort_field.Trim().ToLowerInvariant()
                switch
                {
                    "tesoreria_remesa" => "tesoreria_remesa",
                    "registro_usuario" => "registro_usuario",
                    "fecha_inicio" => "fecha_inicio",
                    "fecha_corte" => "fecha_corte",
                    "estado" => "estado",
                    _ => "registro_fecha"
                };

            var sortOrder =
                filtros.sort_order == 1 ? 1 : -1;

            const string sql = """
                SELECT COUNT(*)
                FROM FSL_REMESAS_TESORERIA R
                WHERE (
                    @like IS NULL
                    OR CONVERT(VARCHAR(30), R.TESORERIA_REMESA) LIKE @like
                    OR R.REGISTRO_USUARIO LIKE @like
                    OR CONVERT(VARCHAR(10), R.REGISTRO_FECHA, 103) LIKE @like
                    OR CONVERT(VARCHAR(10), R.FECHA_INICIO, 103) LIKE @like
                    OR CONVERT(VARCHAR(10), R.FECHA_CORTE, 103) LIKE @like
                    OR R.NOTAS LIKE @like
                    OR R.ESTADO LIKE @like
                );

                SELECT
                    R.TESORERIA_REMESA AS tesoreria_remesa,
                    LTRIM(RTRIM(ISNULL(R.REGISTRO_USUARIO, '')))
                        AS registro_usuario,
                    R.REGISTRO_FECHA AS registro_fecha,
                    R.FECHA_INICIO AS fecha_inicio,
                    R.FECHA_CORTE AS fecha_corte,
                    LTRIM(RTRIM(ISNULL(R.NOTAS, ''))) AS notas,
                    LTRIM(RTRIM(ISNULL(R.ESTADO, ''))) AS estado,
                    CASE LTRIM(RTRIM(ISNULL(R.ESTADO, '')))
                        WHEN 'A' THEN 'Remesa Abierta'
                        WHEN 'C' THEN 'Remesa Cerrada'
                        WHEN 'T' THEN 'Remesa Trasladada'
                        ELSE ''
                    END AS estado_descripcion,
                    CONCAT(
                        RIGHT(
                            '0000' +
                            CONVERT(VARCHAR(20), R.TESORERIA_REMESA),
                            4
                        ),
                        '...',
                        LTRIM(RTRIM(ISNULL(R.REGISTRO_USUARIO, ''))),
                        '...',
                        CONVERT(VARCHAR(19), R.REGISTRO_FECHA, 120),
                        ' I:',
                        CONVERT(VARCHAR(10), R.FECHA_INICIO, 103),
                        ' C:',
                        CONVERT(VARCHAR(10), R.FECHA_CORTE, 103)
                    ) AS descripcion
                FROM FSL_REMESAS_TESORERIA R
                WHERE (
                    @like IS NULL
                    OR CONVERT(VARCHAR(30), R.TESORERIA_REMESA) LIKE @like
                    OR R.REGISTRO_USUARIO LIKE @like
                    OR CONVERT(VARCHAR(10), R.REGISTRO_FECHA, 103) LIKE @like
                    OR CONVERT(VARCHAR(10), R.FECHA_INICIO, 103) LIKE @like
                    OR CONVERT(VARCHAR(10), R.FECHA_CORTE, 103) LIKE @like
                    OR R.NOTAS LIKE @like
                    OR R.ESTADO LIKE @like
                )
                ORDER BY
                    CASE
                        WHEN @sortField = 'tesoreria_remesa'
                         AND @sortOrder = 1
                        THEN R.TESORERIA_REMESA
                    END ASC,
                    CASE
                        WHEN @sortField = 'tesoreria_remesa'
                         AND @sortOrder = -1
                        THEN R.TESORERIA_REMESA
                    END DESC,
                    CASE
                        WHEN @sortField = 'registro_usuario'
                         AND @sortOrder = 1
                        THEN R.REGISTRO_USUARIO
                    END ASC,
                    CASE
                        WHEN @sortField = 'registro_usuario'
                         AND @sortOrder = -1
                        THEN R.REGISTRO_USUARIO
                    END DESC,
                    CASE
                        WHEN @sortField = 'registro_fecha'
                         AND @sortOrder = 1
                        THEN R.REGISTRO_FECHA
                    END ASC,
                    CASE
                        WHEN @sortField = 'registro_fecha'
                         AND @sortOrder = -1
                        THEN R.REGISTRO_FECHA
                    END DESC,
                    CASE
                        WHEN @sortField = 'fecha_inicio'
                         AND @sortOrder = 1
                        THEN R.FECHA_INICIO
                    END ASC,
                    CASE
                        WHEN @sortField = 'fecha_inicio'
                         AND @sortOrder = -1
                        THEN R.FECHA_INICIO
                    END DESC,
                    CASE
                        WHEN @sortField = 'fecha_corte'
                         AND @sortOrder = 1
                        THEN R.FECHA_CORTE
                    END ASC,
                    CASE
                        WHEN @sortField = 'fecha_corte'
                         AND @sortOrder = -1
                        THEN R.FECHA_CORTE
                    END DESC,
                    CASE
                        WHEN @sortField = 'estado'
                         AND @sortOrder = 1
                        THEN R.ESTADO
                    END ASC,
                    CASE
                        WHEN @sortField = 'estado'
                         AND @sortOrder = -1
                        THEN R.ESTADO
                    END DESC,
                    R.REGISTRO_FECHA DESC
                OFFSET @offset ROWS
                FETCH NEXT @fetch ROWS ONLY;
                """;

            return DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                {
                    using var result =
                        connection.QueryMultiple(
                            sql,
                            new
                            {
                                like,
                                sortField,
                                sortOrder,
                                offset,
                                fetch
                            });

                    return new FslListaPaginadaDto<FslRemesaDto>
                    {
                        total =
                            result.ReadFirstOrDefault<int>(),
                        lista =
                            result.Read<FslRemesaDto>()
                                .ToList()
                    };
                });
        }

        /// <summary>
        /// Registra un movimiento del formulario en la bitácora.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario responsable.</param>
        /// <param name="movimiento">Movimiento realizado.</param>
        /// <param name="detalle">Detalle del movimiento.</param>
        private void FSL_RemesasPago_Bitacora_Registrar(
            int CodEmpresa,
            string usuario,
            string movimiento,
            string detalle)
        {
            _ = _securityMainDb.Bitacora(
                new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario =
                        usuario.Trim().ToUpperInvariant(),
                    Modulo = ModuloFosol,
                    Movimiento = movimiento,
                    DetalleMovimiento = detalle
                });
        }
    }
}