using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public sealed partial class FrmFslComiteDB
    {
        private const int ModuloFosol = 22;
        private const int CodigoValidacion = -2;

        private const string MensajeComiteRequerido =
            "El c&oacute;digo del comit&eacute; es requerido.";

        private const string MensajeCedulaRequerida =
            "La c&eacute;dula del miembro es requerida.";

        private const string MensajeUsuarioRequerido =
            "El usuario es requerido.";

        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmFslComiteDB(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);
            _securityMainDb =
                new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene los comit&eacute;s de FOSOL con filtro,
        /// ordenamiento y paginaci&oacute;n.
        /// </summary>
        /// <param name="CodEmpresa">C&oacute;digo de empresa.</param>
        /// <param name="filtros">Filtros de la consulta.</param>
        /// <returns>Lista paginada de comit&eacute;s.</returns>
        public ErrorDto<
            FslListaPaginadaDto<FslComiteDto>>
            FSL_Comite_Comites_Obtener(
                int CodEmpresa,
                FslComitesFiltros filtros)
        {
            ArgumentNullException.ThrowIfNull(
                filtros);

            var filtro = filtros.filtro.Trim();

            var like =
                string.IsNullOrWhiteSpace(filtro)
                    ? null
                    : $"%{filtro}%";

            var offset =
                Math.Max(filtros.pagina, 0);

            var fetch =
                Math.Clamp(
                    filtros.paginacion,
                    1,
                    500);

            var sortField =
                FSL_Comite_Comites_Orden_Campo_Obtener(
                    filtros.sort_field);

            var sortOrder =
                filtros.sort_order == -1
                    ? -1
                    : 1;

            const string sql = """
                SELECT COUNT(*)
                FROM FSL_COMITES C
                WHERE (
                    @like IS NULL
                    OR C.COD_COMITE LIKE @like
                    OR C.DESCRIPCION LIKE @like
                    OR CONVERT(
                        VARCHAR(20),
                        C.NUMERO_RESOLUTORES
                    ) LIKE @like
                    OR CASE
                        WHEN C.ACTIVO = 1
                        THEN 'ACTIVO'
                        ELSE 'INACTIVO'
                    END LIKE @like
                );

                SELECT
                    LTRIM(RTRIM(
                        ISNULL(C.COD_COMITE, '')
                    )) AS cod_comite,
                    LTRIM(RTRIM(
                        ISNULL(C.DESCRIPCION, '')
                    )) AS descripcion,
                    ISNULL(
                        C.NUMERO_RESOLUTORES,
                        0
                    ) AS numero_resolutores,
                    CONVERT(
                        BIT,
                        ISNULL(C.ACTIVO, 0)
                    ) AS activo,
                    LTRIM(RTRIM(
                        ISNULL(C.REGISTRO_USUARIO, '')
                    )) AS registro_usuario,
                    C.REGISTRO_FECHA AS registro_fecha
                FROM FSL_COMITES C
                WHERE (
                    @like IS NULL
                    OR C.COD_COMITE LIKE @like
                    OR C.DESCRIPCION LIKE @like
                    OR CONVERT(
                        VARCHAR(20),
                        C.NUMERO_RESOLUTORES
                    ) LIKE @like
                    OR CASE
                        WHEN C.ACTIVO = 1
                        THEN 'ACTIVO'
                        ELSE 'INACTIVO'
                    END LIKE @like
                )
                ORDER BY
                    CASE
                        WHEN @sortField = 'cod_comite'
                         AND @sortOrder = 1
                        THEN C.COD_COMITE
                    END ASC,
                    CASE
                        WHEN @sortField = 'cod_comite'
                         AND @sortOrder = -1
                        THEN C.COD_COMITE
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
                        WHEN @sortField =
                            'numero_resolutores'
                         AND @sortOrder = 1
                        THEN C.NUMERO_RESOLUTORES
                    END ASC,
                    CASE
                        WHEN @sortField =
                            'numero_resolutores'
                         AND @sortOrder = -1
                        THEN C.NUMERO_RESOLUTORES
                    END DESC,
                    CASE
                        WHEN @sortField = 'activo'
                         AND @sortOrder = 1
                        THEN C.ACTIVO
                    END ASC,
                    CASE
                        WHEN @sortField = 'activo'
                         AND @sortOrder = -1
                        THEN C.ACTIVO
                    END DESC,
                    C.COD_COMITE ASC
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

                    return new FslListaPaginadaDto<
                        FslComiteDto>
                    {
                        total =
                            result.ReadFirstOrDefault<
                                int>(),
                        lista =
                            result.Read<FslComiteDto>()
                                .ToList()
                    };
                });
        }

        /// <summary>
        /// Obtiene los comit&eacute;s activos para el selector
        /// de miembros.
        /// </summary>
        /// <param name="CodEmpresa">C&oacute;digo de empresa.</param>
        /// <returns>Comit&eacute;s activos.</returns>
        public ErrorDto<
            List<DropDownListaGenericaModel>>
            FSL_Comite_ComitesActivos_Obtener(
                int CodEmpresa)
        {
            const string sql = """
                SELECT
                    LTRIM(RTRIM(COD_COMITE)) AS item,
                    CONCAT(
                        LTRIM(RTRIM(COD_COMITE)),
                        ' - ',
                        LTRIM(RTRIM(
                            ISNULL(DESCRIPCION, '')
                        ))
                    ) AS descripcion
                FROM FSL_COMITES
                WHERE ACTIVO = 1
                ORDER BY COD_COMITE;
                """;

            return DbHelper.ExecuteListQuery<
                DropDownListaGenericaModel>(
                    _portalDb,
                    CodEmpresa,
                    sql);
        }

        /// <summary>
        /// Obtiene los miembros de un comit&eacute; con filtro,
        /// ordenamiento y paginaci&oacute;n.
        /// </summary>
        /// <param name="CodEmpresa">C&oacute;digo de empresa.</param>
        /// <param name="filtros">Filtros de la consulta.</param>
        /// <returns>Lista paginada de miembros.</returns>
        public ErrorDto<
            FslListaPaginadaDto<FslComiteMiembroDto>>
            FSL_Comite_Miembros_Obtener(
                int CodEmpresa,
                FslComiteMiembrosFiltros filtros)
        {
            ArgumentNullException.ThrowIfNull(
                filtros);

            var codComite =
                filtros.cod_comite.Trim();

            if (string.IsNullOrWhiteSpace(
                codComite))
            {
                return DbHelper.CreateErrorResponse(
                    MensajeComiteRequerido,
                    CodigoValidacion,
                    new FslListaPaginadaDto<
                        FslComiteMiembroDto>());
            }

            var filtro = filtros.filtro.Trim();

            var like =
                string.IsNullOrWhiteSpace(filtro)
                    ? null
                    : $"%{filtro}%";

            var offset =
                Math.Max(filtros.pagina, 0);

            var fetch =
                Math.Clamp(
                    filtros.paginacion,
                    1,
                    500);

            var sortField =
                FSL_Comite_Miembros_Orden_Campo_Obtener(
                    filtros.sort_field);

            var sortOrder =
                filtros.sort_order == -1
                    ? -1
                    : 1;

            const string sql = """
                SELECT COUNT(*)
                FROM FSL_COMITES_MIEMBROS M
                WHERE M.COD_COMITE = @codComite
                  AND (
                      @like IS NULL
                      OR M.CEDULA LIKE @like
                      OR M.NOMBRE LIKE @like
                      OR M.USUARIO_VINCULADO LIKE @like
                      OR M.REGISTRO_USUARIO LIKE @like
                      OR M.SALIDA_USUARIO LIKE @like
                      OR CASE
                          WHEN M.ACTIVO = 1
                          THEN 'ACTIVO'
                          ELSE 'INACTIVO'
                      END LIKE @like
                  );

                SELECT
                    LTRIM(RTRIM(
                        ISNULL(M.CEDULA, '')
                    )) AS cedula,
                    LTRIM(RTRIM(
                        ISNULL(M.NOMBRE, '')
                    )) AS nombre,
                    LTRIM(RTRIM(
                        ISNULL(M.USUARIO_VINCULADO, '')
                    )) AS usuario_vinculado,
                    LTRIM(RTRIM(
                        ISNULL(M.COD_COMITE, '')
                    )) AS cod_comite,
                    M.REGISTRO_FECHA AS registro_fecha,
                    LTRIM(RTRIM(
                        ISNULL(M.REGISTRO_USUARIO, '')
                    )) AS registro_usuario,
                    M.SALIDA_FECHA AS salida_fecha,
                    LTRIM(RTRIM(
                        ISNULL(M.SALIDA_USUARIO, '')
                    )) AS salida_usuario,
                    CONVERT(
                        BIT,
                        ISNULL(M.ACTIVO, 0)
                    ) AS activo
                FROM FSL_COMITES_MIEMBROS M
                WHERE M.COD_COMITE = @codComite
                  AND (
                      @like IS NULL
                      OR M.CEDULA LIKE @like
                      OR M.NOMBRE LIKE @like
                      OR M.USUARIO_VINCULADO LIKE @like
                      OR M.REGISTRO_USUARIO LIKE @like
                      OR M.SALIDA_USUARIO LIKE @like
                      OR CASE
                          WHEN M.ACTIVO = 1
                          THEN 'ACTIVO'
                          ELSE 'INACTIVO'
                      END LIKE @like
                  )
                ORDER BY
                    CASE
                        WHEN @sortField = 'cedula'
                         AND @sortOrder = 1
                        THEN M.CEDULA
                    END ASC,
                    CASE
                        WHEN @sortField = 'cedula'
                         AND @sortOrder = -1
                        THEN M.CEDULA
                    END DESC,
                    CASE
                        WHEN @sortField = 'nombre'
                         AND @sortOrder = 1
                        THEN M.NOMBRE
                    END ASC,
                    CASE
                        WHEN @sortField = 'nombre'
                         AND @sortOrder = -1
                        THEN M.NOMBRE
                    END DESC,
                    CASE
                        WHEN @sortField =
                            'usuario_vinculado'
                         AND @sortOrder = 1
                        THEN M.USUARIO_VINCULADO
                    END ASC,
                    CASE
                        WHEN @sortField =
                            'usuario_vinculado'
                         AND @sortOrder = -1
                        THEN M.USUARIO_VINCULADO
                    END DESC,
                    CASE
                        WHEN @sortField = 'activo'
                         AND @sortOrder = 1
                        THEN M.ACTIVO
                    END ASC,
                    CASE
                        WHEN @sortField = 'activo'
                         AND @sortOrder = -1
                        THEN M.ACTIVO
                    END DESC,
                    M.ACTIVO DESC,
                    M.CEDULA ASC
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
                                codComite,
                                like,
                                sortField,
                                sortOrder,
                                offset,
                                fetch
                            });

                    return new FslListaPaginadaDto<
                        FslComiteMiembroDto>
                    {
                        total =
                            result.ReadFirstOrDefault<
                                int>(),
                        lista =
                            result
                                .Read<FslComiteMiembroDto>()
                                .ToList()
                    };
                });
        }

        /// <summary>
        /// Obtiene el campo permitido para ordenar comit&eacute;s.
        /// </summary>
        /// <param name="sortField">Campo recibido.</param>
        /// <returns>Campo permitido.</returns>
        private static string
            FSL_Comite_Comites_Orden_Campo_Obtener(
                string sortField)
        {
            return sortField
                .Trim()
                .ToLowerInvariant()
                switch
            {
                "descripcion" =>
                    "descripcion",
                "numero_resolutores" =>
                    "numero_resolutores",
                "activo" =>
                    "activo",
                _ =>
                    "cod_comite"
            };
        }

        /// <summary>
        /// Obtiene el campo permitido para ordenar miembros.
        /// </summary>
        /// <param name="sortField">Campo recibido.</param>
        /// <returns>Campo permitido.</returns>
        private static string
            FSL_Comite_Miembros_Orden_Campo_Obtener(
                string sortField)
        {
            return sortField
                .Trim()
                .ToLowerInvariant()
                switch
            {
                "nombre" =>
                    "nombre",
                "usuario_vinculado" =>
                    "usuario_vinculado",
                "activo" =>
                    "activo",
                _ =>
                    "cedula"
            };
        }

        /// <summary>
        /// Registra un movimiento del formulario en la
        /// bit&aacute;cora general.
        /// </summary>
        /// <param name="CodEmpresa">C&oacute;digo de empresa.</param>
        /// <param name="usuario">Usuario responsable.</param>
        /// <param name="movimiento">Movimiento realizado.</param>
        /// <param name="detalle">Detalle del movimiento.</param>
        private void FSL_Comite_Bitacora_Registrar(
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
    }
}