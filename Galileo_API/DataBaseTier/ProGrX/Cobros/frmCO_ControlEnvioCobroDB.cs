using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;

namespace Galileo.DataBaseTier.ProGrX.Cobros
{
    public sealed class FrmCOControlEnvioCobroDB
    {
        private readonly PortalDB _portalDb;

        public FrmCOControlEnvioCobroDB(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene el catalogo de gestiones utilizado por el formulario
        /// de control de envio al cobro.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            Co_ControlEnvioCobro_Gestiones_Obtener(int codEmpresa)
        {
            const string query = """
                SELECT
                    RTRIM(cod_gestion) AS item,
                    RTRIM(ISNULL(descripcion, '')) AS descripcion
                FROM cbr_gestiones
                ORDER BY cod_gestion;
                """;
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                query);
        }

        /// <summary>
        /// Obtiene las gestiones pendientes de envio al cobro.
        /// Cuando todos es falso, filtra por el codigo de gestion indicado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="todos"></param>
        /// <param name="codGestion"></param>
        /// <returns></returns>
        public ErrorDto<List<CoControlEnvioCobroPendienteData>>
            Co_ControlEnvioCobro_Pendientes_Obtener(
                int codEmpresa,
                bool todos,
                string? codGestion)
        {
            var codigoGestion = (codGestion ?? string.Empty).Trim();

            if (!todos && string.IsNullOrWhiteSpace(codigoGestion))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar la gestion que desea consultar.",
                    -2,
                    new List<CoControlEnvioCobroPendienteData>());
            }

            const string query = """
                SELECT
                    X.cod_seg AS cod_seg,
                    X.fecha AS fecha,
                    RTRIM(ISNULL(X.cedula, '')) AS cedula,
                    RTRIM(ISNULL(S.nombre, '')) AS nombre,
                    ISNULL(X.monto, 0) AS monto,
                    RTRIM(ISNULL(X.cod_gestion, '')) AS cod_gestion,
                    RTRIM(ISNULL(G.descripcion, '')) AS gestion_x,
                    RTRIM(ISNULL(X.usuario, '')) AS usuario,
                    RTRIM(
                        ISNULL(
                            CONVERT(varchar(50), G.codigo_referencia),
                            ''
                        )
                    ) AS codigo_referencia
                FROM Socios AS S
                INNER JOIN cbr_seguimiento AS X
                    ON S.cedula = X.cedula
                INNER JOIN cbr_gestiones AS G
                    ON X.cod_gestion = G.cod_gestion
                WHERE X.estado = 0 AND X.operacion_credito IS NULL
                 AND (@todos = 1 OR G.cod_gestion = @codGestion);
                """;

            return DbHelper.ExecuteListQuery<CoControlEnvioCobroPendienteData>(
                _portalDb,
                codEmpresa,
                query,
                new
                {
                    todos,
                    codGestion = codigoGestion
                });
        }
    }
}