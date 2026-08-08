using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.General;

namespace Galileo_API.DataBaseTier.ProGrX.General
{
    public class FrmCcAnomaliasDB
    {
        private readonly PortalDB _portalDb;

        public FrmCcAnomaliasDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene operaciones activas con saldo menor o igual al monto indicado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<CcAnomaliaCreditoItemDto>> CcAnomaliasSaldosMenores_Obtener(int codEmpresa, CcAnomaliaFiltroDto filtro)
        {
            const string sql = @"
                SELECT
                    R.codigo AS Codigo,
                    R.id_solicitud AS Id_Solicitud,
                    R.cedula AS Cedula,
                    S.nombre AS Nombre,
                    R.saldo AS Saldo,
                    R.opex AS Opex,
                    R.proceso AS Proceso,
                    R.estado AS Estado,
                    R.estadosol AS Estadosol,
                    I.descripcion AS Institucion,
                    C.descripcion AS LineaDesc,
                    ISNULL(D.descripcion, '') AS Destino
                FROM reg_creditos R
                INNER JOIN catalogo C
                    ON R.codigo = C.codigo
                   AND C.retencion = 'N'
                   AND C.poliza = 'N'
                INNER JOIN socios S
                    ON R.cedula = S.cedula
                INNER JOIN instituciones I
                    ON S.cod_institucion = I.cod_institucion
                LEFT JOIN catalogo_destinos D
                    ON R.cod_destino = D.cod_destino
                WHERE R.estado = 'A'
                  AND R.proceso = 'N'
                  AND R.saldo BETWEEN 0 AND @Monto
                  AND R.cod_divisa = 'COL'
                  AND (@Linea IS NULL OR R.codigo = @Linea)
                  AND (@Destino IS NULL OR R.cod_destino = @Destino)
                  AND (@Institucion IS NULL OR S.cod_institucion = @Institucion)
                ORDER BY
                    R.codigo,
                    R.id_solicitud;";

            return DbHelper.ExecuteListQuery<CcAnomaliaCreditoItemDto>(
                _portalDb,
                codEmpresa,
                sql,
                CrearParametrosAnomalia(filtro));
        }

        /// <summary>
        /// Obtiene el catálogo de créditos para filtros de anomalías.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CcAnomaliasCreditos_Obtener(int codEmpresa)
        {
            const string sql = @"
                SELECT
                    CODIGO AS item,
                    DESCRIPCION AS descripcion
                FROM CATALOGO
                WHERE LINEA_INTERNA = 1
                  AND RETENCION = 'N'
                  AND POLIZA = 'N'
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Obtiene el catálogo de destinos para filtros de anomalías.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CcAnomaliasDestinos_Obtener(int codEmpresa)
        {
            const string sql = @"
                SELECT
                    COD_DESTINO AS item,
                    DESCRIPCION AS descripcion
                FROM CATALOGO_DESTINOS
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Obtiene el catálogo de instituciones para filtros de anomalías.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CcAnomaliasInstituciones_Obtener(int codEmpresa)
        {
            const string sql = @"
                SELECT
                    COD_INSTITUCION AS item,
                    DESCRIPCION AS descripcion
                FROM INSTITUCIONES
                order by descripcion;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Obtiene operaciones con saldo negativo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<CcAnomaliaCreditoItemDto>> CcAnomaliasSaldosNegativos_Obtener(int codEmpresa, CcAnomaliaFiltroDto filtro)
        {
            const string sql = @"
                SELECT
                    R.codigo AS Codigo,
                    R.id_solicitud AS Id_Solicitud,
                    R.cedula AS Cedula,
                    S.nombre AS Nombre,
                    R.saldo AS Saldo,
                    R.opex AS Opex,
                    R.proceso AS Proceso,
                    R.estado AS Estado,
                    R.estadosol AS Estadosol,
                    I.descripcion AS Institucion,
                    C.descripcion AS LineaDesc,
                    ISNULL(D.descripcion, '') AS Destino
                FROM reg_creditos R
                INNER JOIN catalogo C
                    ON R.codigo = C.codigo
                   AND C.retencion = 'N'
                   AND C.poliza = 'N'
                INNER JOIN socios S
                    ON R.cedula = S.cedula
                INNER JOIN instituciones I
                    ON S.cod_institucion = I.cod_institucion
                LEFT JOIN catalogo_destinos D
                    ON R.cod_destino = D.cod_destino
                WHERE R.estado IN ('A', 'C')
                  AND R.proceso = 'N'
                  AND R.saldo < 0
                  AND R.cod_divisa = 'COL'
                  AND (@Linea IS NULL OR R.codigo = @Linea)
                  AND (@Destino IS NULL OR R.cod_destino = @Destino)
                  AND (@Institucion IS NULL OR S.cod_institucion = @Institucion)
                ORDER BY
                    R.codigo,
                    R.id_solicitud;";

            return DbHelper.ExecuteListQuery<CcAnomaliaCreditoItemDto>(
                _portalDb,
                codEmpresa,
                sql,
                CrearParametrosAnomalia(filtro));
        }

        /// <summary>
        /// Obtiene operaciones con mora financiera menor o igual al monto indicado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<CcAnomaliaCreditoItemDto>> CcAnomaliasMoraMenor_Obtener(int codEmpresa, CcAnomaliaFiltroDto filtro)
        {
            const string sql = @"
                SELECT
                    R.codigo AS Codigo,
                    R.id_solicitud AS Id_Solicitud,
                    R.cedula AS Cedula,
                    S.nombre AS Nombre,
                    R.saldo AS Saldo,
                    R.opex AS Opex,
                    R.proceso AS Proceso,
                    R.estado AS Estado,
                    NULL AS Estadosol,
                    (
                        M.intc
                        + M.intm
                        + M.amortiza
                        + M.cargo
                    ) AS MoraFinanciera,
                    I.descripcion AS Institucion,
                    C.descripcion AS LineaDesc,
                    ISNULL(D.descripcion, '') AS Destino
                FROM reg_creditos R
                INNER JOIN catalogo C
                    ON R.codigo = C.codigo
                   AND C.retencion = 'N'
                   AND C.poliza = 'N'
                INNER JOIN morosidad M
                    ON R.id_solicitud = M.id_solicitud
                   AND M.estado = 'A'
                   AND R.cod_divisa = 'COL'
                INNER JOIN socios S
                    ON R.cedula = S.cedula
                INNER JOIN instituciones I
                    ON S.cod_institucion = I.cod_institucion
                LEFT JOIN catalogo_destinos D
                    ON R.cod_destino = D.cod_destino
                WHERE R.estado = 'A'
                  AND R.proceso <> 'J'
                  AND (
                        M.intc
                        + M.intm
                        + M.amortiza
                        + M.cargo
                      ) BETWEEN 0 AND @Monto
                  AND (@Linea IS NULL OR R.codigo = @Linea)
                  AND (@Destino IS NULL OR R.cod_destino = @Destino)
                  AND (@Institucion IS NULL OR S.cod_institucion = @Institucion)
                ORDER BY
                    R.codigo,
                    R.id_solicitud;";

            return DbHelper.ExecuteListQuery<CcAnomaliaCreditoItemDto>(
                _portalDb,
                codEmpresa,
                sql,
                CrearParametrosAnomalia(filtro));
        }

        /// <summary>
        /// Obtiene operaciones con cuenta derivada menor al monto indicado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<CcAnomaliaCtaDerivadaItemDto>> CcAnomaliasCtaDerivadaMenor_Obtener(int codEmpresa, CcAnomaliaCtaDerivadaFiltroDto filtro)
        {
            const string sql = @"
                SELECT
                    V.id_solicitud AS Id_Solicitud,
                    V.codigo AS Codigo,
                    R.cedula AS Cedula,
                    S.nombre AS Nombre,
                    R.saldo AS Saldo,
                    V.num_cuota AS Num_Cuota,
                    (
                        V.intcor
                        + V.intmor
                        + V.cargos
                        + V.poliza
                        + V.principal
                    ) AS Monto,
                    C.descripcion AS Descripcion
                FROM crd_operacion_transac V
                INNER JOIN catalogo C
                    ON V.codigo = C.codigo
                   AND C.retencion = 'N'
                   AND C.poliza = 'N'
                   AND C.linea_interna = 1
                INNER JOIN reg_creditos R
                    ON V.id_solicitud = R.id_solicitud
                INNER JOIN crd_garantia_tipos GT
                    ON R.garantia = GT.garantia
                INNER JOIN socios S
                    ON R.cedula = S.cedula
                WHERE V.num_cuota_madre > 0
                  AND R.estado = 'A'
                  AND R.proceso <> 'J'
                  AND V.estado = 'A'
                  AND V.num_cuota <> 0
                  AND (
                        V.intcor
                        + V.intmor
                        + V.cargos
                        + V.poliza
                        + V.principal
                      ) < @Monto
                  AND R.cod_divisa = 'COL'
                ORDER BY
                    V.id_solicitud;";

            return DbHelper.ExecuteListQuery<CcAnomaliaCtaDerivadaItemDto>(
                _portalDb,
                codEmpresa,
                sql,
                new { Monto = filtro?.Monto ?? 0m });
        }

        private static object CrearParametrosAnomalia(CcAnomaliaFiltroDto? filtro)
        {
            return new
            {
                Monto = filtro?.Monto ?? 0m,
                Linea = string.IsNullOrWhiteSpace(filtro?.Linea) ? null : filtro.Linea.Trim(),
                Destino = string.IsNullOrWhiteSpace(filtro?.Destino) ? null : filtro.Destino.Trim(),
                filtro?.Institucion
            };
        }
    }
}
