using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public sealed partial class FrmFslRequisitosDB
    {
        private const int ModuloFosol = 22;
        private const int CodigoValidacion = -2;

        private const string MensajePlanRequerido =
            "El plan es requerido.";

        private const string MensajeCausaRequerida =
            "La causa es requerida.";

        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmFslRequisitosDB(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene los requisitos con filtro, ordenamiento y
        /// paginaci&oacute;n.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="filtros">
        /// Filtros de la consulta.
        /// </param>
        /// <returns>
        /// Lista paginada de requisitos.
        /// </returns>
        public ErrorDto<
            FslListaPaginadaDto<FslRequisitoDto>>
            FSL_Requisitos_Lista_Obtener(
                int CodEmpresa,
                FslRequisitosFiltros filtros)
        {
            ArgumentNullException.ThrowIfNull(filtros);

            var filtro = filtros.filtro.Trim();

            var like =
                string.IsNullOrWhiteSpace(filtro)
                    ? null
                    : $"%{filtro}%";

            var offset = Math.Max(filtros.pagina, 0);

            var fetch = Math.Clamp(
                filtros.paginacion,
                1,
                500);

            var sortField =
                FSL_Requisitos_Orden_Campo_Obtener(
                    filtros.sort_field);

            var sortOrder =
                filtros.sort_order == -1
                    ? -1
                    : 1;

            const string sql = """
                SELECT COUNT(*)
                FROM FSL_REQUISITOS R
                WHERE (
                    @like IS NULL
                    OR R.COD_REQUISITO LIKE @like
                    OR R.DESCRIPCION LIKE @like
                );

                SELECT
                    RTRIM(R.COD_REQUISITO) AS cod_requisito,
                    RTRIM(R.DESCRIPCION) AS descripcion,
                    CAST(ISNULL(R.ACTIVO, 0) AS bit) AS activo
                FROM FSL_REQUISITOS R
                WHERE (
                    @like IS NULL
                    OR R.COD_REQUISITO LIKE @like
                    OR R.DESCRIPCION LIKE @like
                )
                ORDER BY
                    CASE
                        WHEN @sortField = 'cod_requisito'
                            AND @sortOrder = 1
                        THEN R.COD_REQUISITO
                    END ASC,
                    CASE
                        WHEN @sortField = 'cod_requisito'
                            AND @sortOrder = -1
                        THEN R.COD_REQUISITO
                    END DESC,
                    CASE
                        WHEN @sortField = 'descripcion'
                            AND @sortOrder = 1
                        THEN R.DESCRIPCION
                    END ASC,
                    CASE
                        WHEN @sortField = 'descripcion'
                            AND @sortOrder = -1
                        THEN R.DESCRIPCION
                    END DESC,
                    CASE
                        WHEN @sortField = 'activo'
                            AND @sortOrder = 1
                        THEN R.ACTIVO
                    END ASC,
                    CASE
                        WHEN @sortField = 'activo'
                            AND @sortOrder = -1
                        THEN R.ACTIVO
                    END DESC,
                    R.COD_REQUISITO ASC
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
                        FslRequisitoDto>
                    {
                        total = reader.ReadFirst<int>(),
                        lista = reader
                            .Read<FslRequisitoDto>()
                            .ToList()
                    };
                });
        }

        /// <summary>
        /// Obtiene los planes activos disponibles para la
        /// asignaci&oacute;n de requisitos.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <returns>
        /// Lista de planes activos.
        /// </returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            FSL_Requisitos_Planes_Obtener(
                int CodEmpresa)
        {
            const string sql = """
                SELECT
                    RTRIM(COD_PLAN) AS item,
                    RTRIM(DESCRIPCION) AS descripcion
                FROM FSL_PLANES
                WHERE ACTIVO = 1
                ORDER BY COD_PLAN;
                """;

            return DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                    connection
                        .Query<DropDownListaGenericaModel>(
                            sql)
                        .ToList());
        }

        /// <summary>
        /// Obtiene las causas activas correspondientes a un
        /// plan.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="codPlan">
        /// C&oacute;digo del plan.
        /// </param>
        /// <returns>
        /// Lista de causas activas.
        /// </returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            FSL_Requisitos_Causas_Obtener(
                int CodEmpresa,
                string codPlan)
        {
            var plan = codPlan.Trim();

            if (string.IsNullOrWhiteSpace(plan))
            {
                return DbHelper.CreateErrorResponse<
                    List<DropDownListaGenericaModel>>(
                        MensajePlanRequerido,
                        CodigoValidacion,
                        []);
            }

            const string sql = """
                SELECT
                    RTRIM(COD_CAUSA) AS item,
                    RTRIM(DESCRIPCION) AS descripcion
                FROM FSL_PLANES_CAUSAS
                WHERE ACTIVA = 1
                  AND COD_PLAN = @plan
                ORDER BY COD_CAUSA;
                """;

            return DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                    connection
                        .Query<DropDownListaGenericaModel>(
                            sql,
                            new { plan })
                        .ToList());
        }

        /// <summary>
        /// Obtiene todos los requisitos activos y su estado de
        /// asignaci&oacute;n para un plan y una causa.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="codPlan">
        /// C&oacute;digo del plan.
        /// </param>
        /// <param name="codCausa">
        /// C&oacute;digo de la causa.
        /// </param>
        /// <returns>
        /// Requisitos activos con sus indicadores de
        /// asignaci&oacute;n y opcionalidad.
        /// </returns>
        public ErrorDto<List<FslRequisitoCausaDto>>
            FSL_Requisitos_Asignaciones_Obtener(
                int CodEmpresa,
                string codPlan,
                string codCausa)
        {
            var plan = codPlan.Trim();
            var causa = codCausa.Trim();

            if (string.IsNullOrWhiteSpace(plan))
            {
                return DbHelper.CreateErrorResponse<
                    List<FslRequisitoCausaDto>>(
                        MensajePlanRequerido,
                        CodigoValidacion,
                        []);
            }

            if (string.IsNullOrWhiteSpace(causa))
            {
                return DbHelper.CreateErrorResponse<
                    List<FslRequisitoCausaDto>>(
                        MensajeCausaRequerida,
                        CodigoValidacion,
                        []);
            }

            const string sql = """
                SELECT
                    RTRIM(RQ.COD_REQUISITO)
                        AS cod_requisito,
                    RTRIM(RQ.DESCRIPCION)
                        AS descripcion,
                    CAST(
                        ISNULL(RC.OPCIONAL, 0)
                        AS bit
                    ) AS opcional,
                    CAST(
                        ISNULL(RC.ASIGNADO, 0)
                        AS bit
                    ) AS asignado
                FROM FSL_REQUISITOS RQ
                LEFT JOIN FSL_REQUISITOS_CAUSAS RC
                    ON RQ.COD_REQUISITO =
                       RC.COD_REQUISITO
                   AND RC.COD_PLAN = @plan
                   AND RC.COD_CAUSA = @causa
                WHERE RQ.ACTIVO = 1
                ORDER BY RQ.COD_REQUISITO;
                """;

            return DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                    connection
                        .Query<FslRequisitoCausaDto>(
                            sql,
                            new
                            {
                                plan,
                                causa
                            })
                        .ToList());
        }

        /// <summary>
        /// Obtiene el campo permitido para ordenar la lista de
        /// requisitos.
        /// </summary>
        /// <param name="sortField">
        /// Campo solicitado.
        /// </param>
        /// <returns>
        /// Campo de ordenamiento permitido.
        /// </returns>
        private static string
            FSL_Requisitos_Orden_Campo_Obtener(
                string sortField)
        {
            return sortField
                .Trim()
                .ToLowerInvariant() switch
            {
                "descripcion" => "descripcion",
                "activo" => "activo",
                _ => "cod_requisito"
            };
        }

        /// <summary>
        /// Registra un movimiento del formulario en la
        /// bit&aacute;cora general.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="usuario">
        /// Usuario responsable.
        /// </param>
        /// <param name="movimiento">
        /// Movimiento realizado.
        /// </param>
        /// <param name="detalle">
        /// Detalle del movimiento.
        /// </param>
        private void FSL_Requisitos_Bitacora_Registrar(
            int CodEmpresa,
            string usuario,
            string movimiento,
            string detalle)
        {
            _ = _securityMainDb.Bitacora(
                new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario
                        .Trim()
                        .ToUpperInvariant(),
                    Modulo = ModuloFosol,
                    Movimiento = movimiento,
                    DetalleMovimiento = detalle
                });
        }
    }
}