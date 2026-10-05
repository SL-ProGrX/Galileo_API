using System.Data;
using System.Globalization;
using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;

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
            _securityMainDb =
                new MSecurityMainDb(config);
            _mBeneficiosDb =
                new MBeneficiosDB(config);
            _mTesFuncionesDb =
                new MTesFuncionesDb(config);
        }

        /// <summary>
        /// Obtiene una remesa de tesorer&#237;a por c&#243;digo.
        /// </summary>
        /// <param name="CodEmpresa">C&#243;digo de empresa.</param>
        /// <param name="codRemesa">C&#243;digo de la remesa.</param>
        /// <returns>Informaci&#243;n de la remesa.</returns>
        public ErrorDto<FslRemesaDto?>
            FSL_RemesasPago_Remesa_Obtener(
                int CodEmpresa,
                long codRemesa)
        {
            if (codRemesa <= 0)
            {
                return DbHelper.CreateErrorResponse<
                    FslRemesaDto?>(
                        MensajeRemesaRequerida,
                        CodigoValidacion);
            }

            return DbHelper.WithConn<FslRemesaDto?>(
                _portalDb,
                CodEmpresa,
                connection =>
                {
                    var resultado =
                        FSL_RemesasPago_Remesas_Consultar(
                            connection,
                            new FslRemesasConsulta
                            {
                                cod_remesa = codRemesa,
                                offset = 0,
                                fetch = 1,
                                sort_field =
                                    "tesoreria_remesa",
                                sort_order = 1
                            });

                    return resultado.lista
                        .FirstOrDefault();
                });
        }

        /// <summary>
        /// Obtiene las remesas de tesorer&#237;a con filtro y paginaci&#243;n.
        /// </summary>
        /// <param name="CodEmpresa">C&#243;digo de empresa.</param>
        /// <param name="filtros">Filtros de la consulta.</param>
        /// <returns>Lista paginada de remesas.</returns>
        public ErrorDto<
            FslListaPaginadaDto<FslRemesaDto>>
            FSL_RemesasPago_Remesas_Obtener(
                int CodEmpresa,
                FslRemesasFiltros filtros)
        {
            ArgumentNullException.ThrowIfNull(
                filtros);

            var filtro =
                filtros.filtro.Trim();

            var consulta =
                new FslRemesasConsulta
                {
                    like =
                        string.IsNullOrWhiteSpace(
                            filtro)
                            ? null
                            : $"%{filtro}%",
                    offset =
                        Math.Max(
                            filtros.pagina,
                            0),
                    fetch =
                        Math.Clamp(
                            filtros.paginacion,
                            1,
                            500),
                    sort_field =
                        FSL_RemesasPago_Remesas_Orden_Campo_Obtener(
                            filtros.sort_field),
                    sort_order =
                        filtros.sort_order == 1
                            ? 1
                            : -1
                };

            return DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                    FSL_RemesasPago_Remesas_Consultar(
                        connection,
                        consulta));
        }

        /// <summary>
        /// Ejecuta la consulta centralizada de remesas.
        /// </summary>
        /// <param name="connection">Conexi&#243;n activa.</param>
        /// <param name="consulta">Par&#225;metros de consulta.</param>
        /// <returns>Remesas y cantidad total.</returns>
        private static
            FslListaPaginadaDto<FslRemesaDto>
            FSL_RemesasPago_Remesas_Consultar(
                SqlConnection connection,
                FslRemesasConsulta consulta)
        {
            const string sql = """
                SELECT COUNT(*)
                FROM FSL_REMESAS_TESORERIA R
                WHERE (
                    @cod_remesa IS NULL
                    OR R.TESORERIA_REMESA = @cod_remesa
                )
                AND (
                    @like IS NULL
                    OR CONVERT(
                        VARCHAR(30),
                        R.TESORERIA_REMESA
                    ) LIKE @like
                    OR R.REGISTRO_USUARIO LIKE @like
                    OR CONVERT(
                        VARCHAR(10),
                        R.REGISTRO_FECHA,
                        103
                    ) LIKE @like
                    OR CONVERT(
                        VARCHAR(10),
                        R.FECHA_INICIO,
                        103
                    ) LIKE @like
                    OR CONVERT(
                        VARCHAR(10),
                        R.FECHA_CORTE,
                        103
                    ) LIKE @like
                    OR R.NOTAS LIKE @like
                    OR R.ESTADO LIKE @like
                );

                SELECT
                    R.TESORERIA_REMESA AS tesoreria_remesa,
                    ISNULL(
                        R.REGISTRO_USUARIO,
                        ''
                    ) AS registro_usuario,
                    R.REGISTRO_FECHA AS registro_fecha,
                    R.FECHA_INICIO AS fecha_inicio,
                    R.FECHA_CORTE AS fecha_corte,
                    ISNULL(R.NOTAS, '') AS notas,
                    ISNULL(R.ESTADO, '') AS estado
                FROM FSL_REMESAS_TESORERIA R
                WHERE (
                    @cod_remesa IS NULL
                    OR R.TESORERIA_REMESA = @cod_remesa
                )
                AND (
                    @like IS NULL
                    OR CONVERT(
                        VARCHAR(30),
                        R.TESORERIA_REMESA
                    ) LIKE @like
                    OR R.REGISTRO_USUARIO LIKE @like
                    OR CONVERT(
                        VARCHAR(10),
                        R.REGISTRO_FECHA,
                        103
                    ) LIKE @like
                    OR CONVERT(
                        VARCHAR(10),
                        R.FECHA_INICIO,
                        103
                    ) LIKE @like
                    OR CONVERT(
                        VARCHAR(10),
                        R.FECHA_CORTE,
                        103
                    ) LIKE @like
                    OR R.NOTAS LIKE @like
                    OR R.ESTADO LIKE @like
                )
                ORDER BY
                    CASE
                        WHEN @sort_field =
                            'tesoreria_remesa'
                         AND @sort_order = 1
                        THEN R.TESORERIA_REMESA
                    END ASC,
                    CASE
                        WHEN @sort_field =
                            'tesoreria_remesa'
                         AND @sort_order = -1
                        THEN R.TESORERIA_REMESA
                    END DESC,
                    CASE
                        WHEN @sort_field =
                            'registro_usuario'
                         AND @sort_order = 1
                        THEN R.REGISTRO_USUARIO
                    END ASC,
                    CASE
                        WHEN @sort_field =
                            'registro_usuario'
                         AND @sort_order = -1
                        THEN R.REGISTRO_USUARIO
                    END DESC,
                    CASE
                        WHEN @sort_field =
                            'registro_fecha'
                         AND @sort_order = 1
                        THEN R.REGISTRO_FECHA
                    END ASC,
                    CASE
                        WHEN @sort_field =
                            'registro_fecha'
                         AND @sort_order = -1
                        THEN R.REGISTRO_FECHA
                    END DESC,
                    CASE
                        WHEN @sort_field =
                            'fecha_inicio'
                         AND @sort_order = 1
                        THEN R.FECHA_INICIO
                    END ASC,
                    CASE
                        WHEN @sort_field =
                            'fecha_inicio'
                         AND @sort_order = -1
                        THEN R.FECHA_INICIO
                    END DESC,
                    CASE
                        WHEN @sort_field =
                            'fecha_corte'
                         AND @sort_order = 1
                        THEN R.FECHA_CORTE
                    END ASC,
                    CASE
                        WHEN @sort_field =
                            'fecha_corte'
                         AND @sort_order = -1
                        THEN R.FECHA_CORTE
                    END DESC,
                    CASE
                        WHEN @sort_field = 'estado'
                         AND @sort_order = 1
                        THEN R.ESTADO
                    END ASC,
                    CASE
                        WHEN @sort_field = 'estado'
                         AND @sort_order = -1
                        THEN R.ESTADO
                    END DESC,
                    R.REGISTRO_FECHA DESC
                OFFSET @offset ROWS
                FETCH NEXT @fetch ROWS ONLY;
                """;

            if (connection.State !=
                ConnectionState.Open)
            {
                connection.Open();
            }

            using var result =
                connection.QueryMultiple(
                    sql,
                    consulta);

            var total =
                result.ReadFirstOrDefault<int>();

            var remesas =
                result.Read<FslRemesaDto>()
                    .ToList();

            foreach (var remesa in remesas)
            {
                FSL_RemesasPago_Remesa_Normalizar(
                    remesa);
            }

            return new FslListaPaginadaDto<
                FslRemesaDto>
            {
                total = total,
                lista = remesas
            };
        }

        /// <summary>
        /// Normaliza los valores y descripciones de una remesa.
        /// </summary>
        /// <param name="remesa">Remesa consultada.</param>
        private static void
            FSL_RemesasPago_Remesa_Normalizar(
                FslRemesaDto remesa)
        {
            remesa.registro_usuario =
                remesa.registro_usuario.Trim();

            remesa.notas =
                remesa.notas.Trim();

            remesa.estado =
                remesa.estado.Trim()
                    .ToUpperInvariant();

            remesa.estado_descripcion =
                FSL_RemesasPago_Estado_Descripcion_Obtener(
                    remesa.estado);

            remesa.descripcion =
                FSL_RemesasPago_Remesa_Descripcion_Crear(
                    remesa);
        }

        /// <summary>
        /// Obtiene la descripci&#243;n correspondiente al estado.
        /// </summary>
        /// <param name="estado">Estado de la remesa.</param>
        /// <returns>Descripci&#243;n del estado.</returns>
        private static string
            FSL_RemesasPago_Estado_Descripcion_Obtener(
                string estado)
        {
            return estado switch
            {
                "A" => "Remesa Abierta",
                "C" => "Remesa Cerrada",
                "T" => "Remesa Trasladada",
                _ => string.Empty
            };
        }

        /// <summary>
        /// Construye la descripci&#243;n utilizada en los selectores.
        /// </summary>
        /// <param name="remesa">Informaci&#243;n de la remesa.</param>
        /// <returns>Descripci&#243;n para mostrar.</returns>
        private static string
            FSL_RemesasPago_Remesa_Descripcion_Crear(
                FslRemesaDto remesa)
        {
            var registroFecha =
                FSL_RemesasPago_Fecha_Texto_Obtener(
                    remesa.registro_fecha,
                    "yyyy-MM-dd HH:mm:ss");

            var fechaInicio =
                FSL_RemesasPago_Fecha_Texto_Obtener(
                    remesa.fecha_inicio,
                    "dd/MM/yyyy");

            var fechaCorte =
                FSL_RemesasPago_Fecha_Texto_Obtener(
                    remesa.fecha_corte,
                    "dd/MM/yyyy");

            return string.Create(
                CultureInfo.InvariantCulture,
                $"{remesa.tesoreria_remesa:0000}...{remesa.registro_usuario}...{registroFecha} I:{fechaInicio} C:{fechaCorte}");
        }

        /// <summary>
        /// Convierte una fecha opcional en texto.
        /// </summary>
        /// <param name="fecha">Fecha a convertir.</param>
        /// <param name="formato">Formato requerido.</param>
        /// <returns>Fecha formateada o texto vac&#237;o.</returns>
        private static string
            FSL_RemesasPago_Fecha_Texto_Obtener(
                DateTime? fecha,
                string formato)
        {
            return fecha?.ToString(
                formato,
                CultureInfo.InvariantCulture) ??
                string.Empty;
        }

        /// <summary>
        /// Valida el campo solicitado para ordenar las remesas.
        /// </summary>
        /// <param name="sortField">Campo recibido.</param>
        /// <returns>Campo de ordenamiento permitido.</returns>
        private static string
            FSL_RemesasPago_Remesas_Orden_Campo_Obtener(
                string sortField)
        {
            return sortField
                .Trim()
                .ToLowerInvariant()
                switch
            {
                "tesoreria_remesa" =>
                    "tesoreria_remesa",
                "registro_usuario" =>
                    "registro_usuario",
                "fecha_inicio" =>
                    "fecha_inicio",
                "fecha_corte" =>
                    "fecha_corte",
                "estado" =>
                    "estado",
                "estado_descripcion" =>
                    "estado",
                _ =>
                    "registro_fecha"
            };
        }

        /// <summary>
        /// Registra un movimiento del formulario en la bit&#225;cora.
        /// </summary>
        /// <param name="CodEmpresa">C&#243;digo de empresa.</param>
        /// <param name="usuario">Usuario responsable.</param>
        /// <param name="movimiento">Movimiento realizado.</param>
        /// <param name="detalle">Detalle del movimiento.</param>
        private void
            FSL_RemesasPago_Bitacora_Registrar(
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
                        usuario.Trim()
                            .ToUpperInvariant(),
                    Modulo = ModuloFosol,
                    Movimiento = movimiento,
                    DetalleMovimiento = detalle
                });
        }

        private sealed class FslRemesasConsulta
        {
            public long? cod_remesa { get; set; }
            public string? like { get; set; }
            public int offset { get; set; } = 0;
            public int fetch { get; set; } = 30;
            public string sort_field { get; set; } =
                "registro_fecha";
            public int sort_order { get; set; } = -1;
        }
    }
}