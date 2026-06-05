using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrCreditosCargaLoteDB
    {
        private readonly PortalDB _portalDb;

        public FrmCrCreditosCargaLoteDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la lista de clientes para carga de lote de créditos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrCreditosCargaLote_Cliente_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string sqlQuery = @"
                    SELECT
                        RTRIM(codigo) AS item,
                        RTRIM(descripcion) + '  [' + RTRIM(codigo) + ']' AS descripcion
                    FROM catalogo
                    WHERE retencion = 'N'
                      AND activo = 1
                      AND codigo NOT IN (
                          SELECT codigo_ase
                          FROM fnd_planes
                      )
                    ORDER BY codigo;";

                return conn.Query<DropDownListaGenericaModel>(sqlQuery).ToList();
            });
        }

        /// <summary>
        /// Obtiene la lista de destinos asociados a una línea de crédito.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrCreditosCargaLote_Destinos_Obtener(int CodEmpresa, string codigo)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string sqlQuery = @"
                    select
                        rtrim(D.cod_Destino) as item,
                        rtrim(D.descripcion) as descripcion
                    from catalogo_destinos D
                    inner join catalogo_destinosASG C
                        on D.cod_destino = C.cod_destino
                    where C.codigo = @Codigo
                    order by D.prioridad asc;";

                return conn.Query<DropDownListaGenericaModel>(
                    sqlQuery,
                    new { Codigo = (codigo ?? string.Empty).Trim() }).ToList();
            });
        }

        /// <summary>
        /// Obtiene la lista de conceptos de desembolso activos que retienen.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrCreditosCargaLote_ConceptosDesembolso_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string sqlQuery = @"
                    SELECT
                        COD_CONDEB AS item,
                        DESCRIPCION AS descripcion
                    FROM CONCEPTO_DESEMB
                    WHERE ACTIVO = 1
                      AND RETIENE = 1;";

                return conn.Query<DropDownListaGenericaModel>(sqlQuery).ToList();
            });
        }
    }
}
