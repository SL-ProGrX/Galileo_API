using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public sealed partial class FrmFslTipoAplicacionDB
    {
        private const int ModuloFosol = 22;
        private const int CodigoValidacion = -2;

        private const string MovimientoRegistrar =
            "Registra";

        private const string MovimientoModificar =
            "Modifica";

        private const string MovimientoEliminar =
            "Elimina";

        private const string MensajePlanRequerido =
            "El c&oacute;digo del plan es requerido.";

        private const string MensajePlanDescripcionRequerida =
            "La descripci&oacute;n del plan es requerida.";

        private const string MensajePlanTipoInvalido =
            "El tipo de desembolso del plan no es v&aacute;lido.";

        private const string MensajePlanExistente =
            "El plan indicado ya existe.";

        private const string MensajePlanNoExiste =
            "El plan indicado no existe.";

        private const string MensajeCausaRequerida =
            "El c&oacute;digo de la causa es requerido.";

        private const string MensajeCausaDescripcionRequerida =
            "La descripci&oacute;n de la causa es requerida.";

        private const string MensajeMontoBaseInvalido =
            "El monto base de la causa no es v&aacute;lido.";

        private const string MensajeTipoTablaInvalido =
            "El tipo de tabla de la causa no es v&aacute;lido.";

        private const string MensajeCausaExistente =
            "La causa indicada ya existe para el plan seleccionado.";

        private const string MensajeCausaNoExiste =
            "La causa indicada no existe para el plan seleccionado.";

        private const string MensajeUsuarioRequerido = "El usuario es requerido.";

        private const string CampoDescripcion = "descripcion";

        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmFslTipoAplicacionDB(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);

            _securityMainDb =
                new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene los planes de aplicaci&oacute;n FOSOL con
        /// filtro, ordenamiento y paginaci&oacute;n.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="filtros">
        /// Filtros, ordenamiento y paginaci&oacute;n.
        /// </param>
        /// <returns>
        /// Lista paginada de planes.
        /// </returns>
        public ErrorDto<
            FslListaPaginadaDto<
                FslTipoAplicacionPlanDto>>
            FSL_TipoAplicacion_Planes_Lista_Obtener(
                int CodEmpresa,
                FslTipoAplicacionFiltros filtros)
        {
            ArgumentNullException.ThrowIfNull(filtros);

            var filtro =
                FSL_TipoAplicacion_Texto_Normalizar(
                    filtros.filtro);

            var like =
                string.IsNullOrWhiteSpace(filtro)
                    ? null
                    : $"%{filtro}%";

            var offset = Math.Max(
                filtros.pagina,
                0);

            var fetch = Math.Clamp(
                filtros.paginacion,
                1,
                500);

            var sortField =
                FSL_TipoAplicacion_Plan_Orden_Campo_Obtener(
                    filtros.sort_field);

            var sortOrder =
                filtros.sort_order == -1
                    ? -1
                    : 1;

            const string sql = """
                SELECT COUNT(*)
                FROM FSL_PLANES P
                WHERE
                    @like IS NULL
                    OR P.COD_PLAN LIKE @like
                    OR P.DESCRIPCION LIKE @like;

                SELECT
                    RTRIM(
                        ISNULL(P.COD_PLAN, '')
                    ) AS cod_plan,
                    RTRIM(
                        ISNULL(P.DESCRIPCION, '')
                    ) AS descripcion,
                    CASE
                        WHEN ISNULL(
                            NULLIF(
                                LTRIM(
                                    RTRIM(
                                        P.TIPO_DESEMBOLSO
                                    )
                                ),
                                ''
                            ),
                            'F'
                        ) = 'T'
                        THEN 'T'
                        ELSE 'F'
                    END AS tipo_desembolso,
                    CASE
                        WHEN ISNULL(
                            NULLIF(
                                LTRIM(
                                    RTRIM(
                                        P.TIPO_DESEMBOLSO
                                    )
                                ),
                                ''
                            ),
                            'F'
                        ) = 'T'
                        THEN 'Tesorería'
                        ELSE 'Fondos'
                    END AS tipo_desembolso_descripcion,
                    CAST(
                        ISNULL(P.ACTIVO, 0)
                        AS BIT
                    ) AS activo
                FROM FSL_PLANES P
                WHERE
                    @like IS NULL
                    OR P.COD_PLAN LIKE @like
                    OR P.DESCRIPCION LIKE @like
                ORDER BY
                    CASE
                        WHEN @sortField = 'cod_plan'
                            AND @sortOrder = 1
                        THEN P.COD_PLAN
                    END ASC,
                    CASE
                        WHEN @sortField = 'cod_plan'
                            AND @sortOrder = -1
                        THEN P.COD_PLAN
                    END DESC,
                    CASE
                        WHEN @sortField = 'descripcion'
                            AND @sortOrder = 1
                        THEN P.DESCRIPCION
                    END ASC,
                    CASE
                        WHEN @sortField = 'descripcion'
                            AND @sortOrder = -1
                        THEN P.DESCRIPCION
                    END DESC,
                    CASE
                        WHEN @sortField = 'tipo_desembolso'
                            AND @sortOrder = 1
                        THEN P.TIPO_DESEMBOLSO
                    END ASC,
                    CASE
                        WHEN @sortField = 'tipo_desembolso'
                            AND @sortOrder = -1
                        THEN P.TIPO_DESEMBOLSO
                    END DESC,
                    CASE
                        WHEN @sortField = 'activo'
                            AND @sortOrder = 1
                        THEN P.ACTIVO
                    END ASC,
                    CASE
                        WHEN @sortField = 'activo'
                            AND @sortOrder = -1
                        THEN P.ACTIVO
                    END DESC,
                    P.COD_PLAN ASC
                OFFSET @offset ROWS
                FETCH NEXT @fetch ROWS ONLY;
                """;

            return DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                {
                    using var reader =
                        connection.QueryMultiple(
                            sql,
                            new
                            {
                                like,
                                offset,
                                fetch,
                                sortField,
                                sortOrder
                            });

                    return new FslListaPaginadaDto<
                        FslTipoAplicacionPlanDto>
                    {
                        total =
                            reader.ReadSingle<int>(),
                        lista =
                            reader
                                .Read<
                                    FslTipoAplicacionPlanDto>()
                                .ToList()
                    };
                });
        }

        /// <summary>
        /// Obtiene las causas asociadas con un plan FOSOL.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="codPlan">
        /// C&oacute;digo del plan.
        /// </param>
        /// <param name="filtros">
        /// Filtros, ordenamiento y paginaci&oacute;n.
        /// </param>
        /// <returns>
        /// Lista paginada de causas.
        /// </returns>
        public ErrorDto<
            FslListaPaginadaDto<
                FslTipoAplicacionCausaDto>>
            FSL_TipoAplicacion_Causas_Lista_Obtener(
                int CodEmpresa,
                string? codPlan,
                FslTipoAplicacionFiltros filtros)
        {
            ArgumentNullException.ThrowIfNull(filtros);

            var codigoPlan =
                FSL_TipoAplicacion_Texto_Normalizar(
                    codPlan);

            if (string.IsNullOrWhiteSpace(codigoPlan))
            {
                return DbHelper.CreateErrorResponse(
                    MensajePlanRequerido,
                    CodigoValidacion,
                    new FslListaPaginadaDto<
                        FslTipoAplicacionCausaDto>());
            }

            var filtro =
                FSL_TipoAplicacion_Texto_Normalizar(
                    filtros.filtro);

            var like =
                string.IsNullOrWhiteSpace(filtro)
                    ? null
                    : $"%{filtro}%";

            var offset = Math.Max(
                filtros.pagina,
                0);

            var fetch = Math.Clamp(
                filtros.paginacion,
                1,
                500);

            var sortField =
                FSL_TipoAplicacion_Causa_Orden_Campo_Obtener(
                    filtros.sort_field);

            var sortOrder =
                filtros.sort_order == -1
                    ? -1
                    : 1;

            const string sql = """
                SELECT COUNT(*)
                FROM FSL_PLANES_CAUSAS C
                WHERE
                    C.COD_PLAN = @codigoPlan
                    AND
                    (
                        @like IS NULL
                        OR C.COD_CAUSA LIKE @like
                        OR C.DESCRIPCION LIKE @like
                    );

                SELECT
                    RTRIM(
                        ISNULL(C.COD_CAUSA, '')
                    ) AS cod_causa,
                    RTRIM(
                        ISNULL(C.COD_PLAN, '')
                    ) AS cod_plan,
                    RTRIM(
                        ISNULL(C.DESCRIPCION, '')
                    ) AS descripcion,
                    CASE
                        WHEN C.MONTO_BASE = 'F'
                        THEN 'F'
                        ELSE 'S'
                    END AS monto_base,
                    CASE
                        WHEN C.MONTO_BASE = 'F'
                        THEN 'Formalizado'
                        ELSE 'Saldo'
                    END AS monto_base_descripcion,
                    CASE
                        WHEN C.TIPO_TABLA = 'I'
                        THEN 'I'
                        WHEN C.TIPO_TABLA = 'S'
                        THEN 'S'
                        WHEN C.TIPO_TABLA = 'X'
                        THEN 'X'
                        ELSE 'F'
                    END AS tipo_tabla,
                    CASE
                        WHEN C.TIPO_TABLA = 'I'
                        THEN 'Incapacidad'
                        WHEN C.TIPO_TABLA = 'S'
                        THEN 'Suicidio'
                        WHEN C.TIPO_TABLA = 'X'
                        THEN '100 %'
                        ELSE 'Fallecimiento'
                    END AS tipo_tabla_descripcion,
                    CAST(
                        ISNULL(C.ACTIVA, 0)
                        AS BIT
                    ) AS activa
                FROM FSL_PLANES_CAUSAS C
                WHERE
                    C.COD_PLAN = @codigoPlan
                    AND
                    (
                        @like IS NULL
                        OR C.COD_CAUSA LIKE @like
                        OR C.DESCRIPCION LIKE @like
                    )
                ORDER BY
                    CASE
                        WHEN @sortField = 'cod_causa'
                            AND @sortOrder = 1
                        THEN C.COD_CAUSA
                    END ASC,
                    CASE
                        WHEN @sortField = 'cod_causa'
                            AND @sortOrder = -1
                        THEN C.COD_CAUSA
                    END DESC,
                    CASE
                        WHEN @sortField = 'descripcion'
                            AND @sortOrder = 1
                        THEN C.DESCRIPCION
                    END ASC,
                    CASE
                        WHEN @sortField = 'descripcion'
                            AND @sortOrder = -1
                        THEN C.DESCRIPCION
                    END DESC,
                    CASE
                        WHEN @sortField = 'monto_base'
                            AND @sortOrder = 1
                        THEN C.MONTO_BASE
                    END ASC,
                    CASE
                        WHEN @sortField = 'monto_base'
                            AND @sortOrder = -1
                        THEN C.MONTO_BASE
                    END DESC,
                    CASE
                        WHEN @sortField = 'tipo_tabla'
                            AND @sortOrder = 1
                        THEN C.TIPO_TABLA
                    END ASC,
                    CASE
                        WHEN @sortField = 'tipo_tabla'
                            AND @sortOrder = -1
                        THEN C.TIPO_TABLA
                    END DESC,
                    CASE
                        WHEN @sortField = 'activa'
                            AND @sortOrder = 1
                        THEN C.ACTIVA
                    END ASC,
                    CASE
                        WHEN @sortField = 'activa'
                            AND @sortOrder = -1
                        THEN C.ACTIVA
                    END DESC,
                    C.COD_CAUSA ASC
                OFFSET @offset ROWS
                FETCH NEXT @fetch ROWS ONLY;
                """;

            return DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                {
                    using var reader =
                        connection.QueryMultiple(
                            sql,
                            new
                            {
                                codigoPlan,
                                like,
                                offset,
                                fetch,
                                sortField,
                                sortOrder
                            });

                    return new FslListaPaginadaDto<
                        FslTipoAplicacionCausaDto>
                    {
                        total =
                            reader.ReadSingle<int>(),
                        lista =
                            reader
                                .Read<
                                    FslTipoAplicacionCausaDto>()
                                .ToList()
                    };
                });
        }

        /// <summary>
        /// Obtiene los planes activos utilizados por el
        /// selector de causas.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <returns>
        /// Planes activos.
        /// </returns>
        public ErrorDto<
            List<DropDownListaGenericaModel>>
            FSL_TipoAplicacion_Planes_Selector_Obtener(
                int CodEmpresa)
        {
            const string sql = """
                SELECT
                    RTRIM(
                        ISNULL(COD_PLAN, '')
                    ) AS item,
                    RTRIM(
                        ISNULL(COD_PLAN, '')
                    ) + ' - ' +
                    RTRIM(
                        ISNULL(DESCRIPCION, '')
                    ) AS descripcion
                FROM FSL_PLANES
                WHERE ACTIVO = 1
                ORDER BY COD_PLAN;
                """;

            return DbHelper.ExecuteListQuery<
                DropDownListaGenericaModel>(
                    _portalDb,
                    CodEmpresa,
                    sql);
        }

        private static string
            FSL_TipoAplicacion_Texto_Normalizar(
                string? valor)
        {
            return valor?.Trim() ??
                string.Empty;
        }

        private static string
            FSL_TipoAplicacion_Codigo_Normalizar(
                string? valor)
        {
            return FSL_TipoAplicacion_Texto_Normalizar(
                valor)
                .ToUpperInvariant();
        }

        private static string
            FSL_TipoAplicacion_Plan_Orden_Campo_Obtener(
                string? sortField)
        {
            return FSL_TipoAplicacion_Texto_Normalizar(
                sortField)
                .ToLowerInvariant() switch
            {
                CampoDescripcion =>
                    CampoDescripcion,
                "tipo_desembolso" =>
                    "tipo_desembolso",
                "activo" => "activo",
                _ => "cod_plan"
            };
        }

        private static string
            FSL_TipoAplicacion_Causa_Orden_Campo_Obtener(
                string? sortField)
        {
            return FSL_TipoAplicacion_Texto_Normalizar(
                sortField)
                .ToLowerInvariant() switch
            {
                CampoDescripcion =>
                    CampoDescripcion,
                "monto_base" => "monto_base",
                "tipo_tabla" => "tipo_tabla",
                "activa" => "activa",
                _ => "cod_causa"
            };
        }

        private void
            FSL_TipoAplicacion_Bitacora_Registrar(
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
                        FSL_TipoAplicacion_Codigo_Normalizar(
                            usuario),
                    Modulo = ModuloFosol,
                    Movimiento = movimiento,
                    DetalleMovimiento = detalle
                });
        }
    }
}