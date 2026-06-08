using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

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

        /// <summary>
        /// Obtiene la lista de clientes para carga de lote de créditos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrCreditosCargaLote_ObtenerDeductoras(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string sqlQuery = @"
                    SELECT 
                        COD_INSTITUCION AS item,
                        DESCRIPCION AS descripcion
                    FROM INSTITUCIONES
                    WHERE ACTIVA = 1
                      AND DEDUCCION_PLANILLA = 1";

                return conn.Query<DropDownListaGenericaModel>(sqlQuery).ToList();
            });
        }

        /// <summary>
        /// Obtiene la lista de clientes para carga de lote de créditos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<FrecuenciaReductora>> CrCreditosCargaLote_ObtenerFrecuenciaDeductora(int CodEmpresa, string CodInstitucion)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string sqlQuery = @"
                    SELECT 
                        RTRIM(descripcion) AS Descripcion,
                        ISNULL(Frecuencia,'M') AS Frecuencia_Id
                    FROM instituciones
                    WHERE cod_institucion = @CodInstitucion";

                return conn.Query<FrecuenciaReductora>(
                    sqlQuery,
                    new { CodInstitucion = (CodInstitucion ?? string.Empty).Trim() }).ToList();
            });
        }

        /// <summary>
        /// Metodo para obtener bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrCreditosCargaLote_Banco_Obtener(int CodEmpresa, string usuario)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"EXEC spCrd_SGT_Bancos @Usuario";

                var param = new { Usuario = usuario };

                var result = conn.Query<dynamic>(query, param).ToList();                

                var lista = result.Select(x => new DropDownListaGenericaModel
                {
                    item = x.IdX,
                    descripcion = x.ItmX
                }).ToList();

                return lista;
            });
        }
    }
}
