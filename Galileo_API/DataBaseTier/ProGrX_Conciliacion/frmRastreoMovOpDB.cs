using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Conciliacion;

namespace Galileo_API.DataBaseTier.ProGrX_Conciliacion
{
    public sealed class FrmRastreoMovOpDB
    {
        private readonly PortalDB _portalDb;

        public FrmRastreoMovOpDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene los periodos historicos ordenados por anio y mes descendente.
        /// </summary>
        /// <param name="codEmpresa">Codigo de la empresa.</param>
        /// <returns>Lista de periodos historicos.</returns>
        public ErrorDto<List<RastreoMovOpPeriodoData>> RastreoMovOp_Periodos_Obtener(
            int codEmpresa)
        {
            const string sql = """
                -- @CodEmpresa: codigo de la empresa
                SELECT
                    id_per_historico AS Id_Per_Historico,
                    anio AS Anio,
                    mes AS Mes
                FROM ase_per_historico
                ORDER BY anio DESC, mes DESC;
                """;

            return DbHelper.ExecuteListQuery<RastreoMovOpPeriodoData>(
                _portalDb,
                codEmpresa,
                sql);
        }

        /// <summary>
        /// Obtiene el anio y mes del periodo historico seleccionado.
        /// </summary>
        /// <param name="codEmpresa">Codigo de la empresa.</param>
        /// <param name="idPerHistorico">Identificador del periodo historico.</param>
        /// <returns>Periodo historico.</returns>
        public ErrorDto<RastreoMovOpPeriodoData?> RastreoMovOp_Periodo_Obtener(
            int codEmpresa,
            int idPerHistorico)
        {
            const string sql = """
                -- @IdPerHistorico: identificador del periodo historico
                SELECT
                    id_per_historico AS Id_Per_Historico,
                    anio AS Anio,
                    mes AS Mes
                FROM ase_per_historico
                WHERE id_per_historico = @IdPerHistorico;
                """;

            return DbHelper.ExecuteSingleQuery<RastreoMovOpPeriodoData?>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                new { IdPerHistorico = idPerHistorico });
        }

        /// <summary>
        /// Obtiene el analisis de saldos de operaciones de credito para un periodo.
        /// </summary>
        /// <param name="codEmpresa">Codigo de la empresa.</param>
        /// <param name="anio">Anio del periodo.</param>
        /// <param name="mes">Mes del periodo.</param>
        /// <param name="lineas">Cantidad maxima de registros.</param>
        /// <param name="diferencias">Indica si solo se incluyen registros con diferencia.</param>
        /// <returns>Lista de saldos por operacion.</returns>
        public ErrorDto<List<RastreoMovOpSaldosData>> RastreoMovOp_Saldos_Obtener(
            int codEmpresa,
            int anio,
            int mes,
            int lineas,
            bool diferencias)
        {
            const string sql = """
                -- @Anio: anio del periodo
                -- @Mes: mes del periodo
                -- @Lineas: cantidad maxima de registros
                -- @Diferencias: 1 = solo diferencias mayores a 1
                SELECT TOP (@Lineas)
                    H.id_solicitud AS Operacion,
                    RTRIM(H.codigo) AS Codigo,
                    RTRIM(ISNULL(S.cedula, '')) AS Identificacion,
                    RTRIM(ISNULL(S.nombre, '')) AS Nombre,
                    ISNULL(H.saldo_inicial, 0) AS Saldo_Inicial,
                    ISNULL(H.saldo_final, 0) AS Saldo_Final,
                    ISNULL(H.total_debitos, 0) AS Debitos,
                    ISNULL(H.total_creditos, 0) AS Creditos,
                    ISNULL(H.saldo_final, 0)
                        - (
                            ISNULL(H.saldo_inicial, 0)
                            + ISNULL(H.total_debitos, 0)
                            - ISNULL(H.total_creditos, 0)
                          ) AS Diferencia
                FROM ase_per_cerrados H
                INNER JOIN reg_creditos R
                    ON H.id_solicitud = R.id_solicitud
                LEFT JOIN Socios S
                    ON R.cedula = S.cedula
                INNER JOIN ase_per_Catalogo C
                    ON C.codigo = H.codigo
                   AND C.mes = H.mes
                   AND C.anio = H.anio
                WHERE H.anio = @Anio
                  AND H.mes = @Mes
                  AND C.retencion = 'N'
                  AND C.poliza = 'N'
                  AND (
                        @Diferencias = 0
                        OR ABS(
                            H.saldo_final
                            - (H.saldo_inicial + H.total_debitos - H.total_creditos)
                        ) > 1
                      );
                """;

            return DbHelper.ExecuteListQuery<RastreoMovOpSaldosData>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    Anio = anio,
                    Mes = mes,
                    Lineas = lineas,
                    Diferencias = diferencias ? 1 : 0,
                });
        }
    }
}
