using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXRepBalanceComprobacionDb
    {
        private readonly PortalDB _portalDB;
        private readonly MCntXPreliminaresDb _mCntXPreliminaresDb;
        private readonly MCntXCalculosDb _mCntXCalculosDb;

        public FrmCntXRepBalanceComprobacionDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _mCntXPreliminaresDb = new MCntXPreliminaresDb(config);
            _mCntXCalculosDb = new MCntXCalculosDb(config);
        }

        /// <summary>
        /// Genera los movimientos temporales requeridos por el balance preliminar.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa cuya conexión se utilizará.</param>
        /// <param name="request">Parámetros del proceso preliminar.</param>
        /// <returns>Resultado del montaje de los movimientos temporales.</returns>
        public ErrorDto<bool> CntX_Preliminar_Montar(
            int codEmpresa,
            CntXPreliminarMontarRequest request)
        {
            return _mCntXPreliminaresDb.sbCntX_Preliminar_Montar(
                codEmpresa,
                request);
        }

        /// <summary>
        /// Lista las unidades de negocio de la contabilidad activa.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa cuya conexión se utilizará.</param>
        /// <param name="codContabilidad">Código de la contabilidad activa.</param>
        /// <returns>Lista de unidades ordenada por código.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CntX_Unidades_Listar(
            int codEmpresa,
            int codContabilidad)
        {
            const string sql = @"
                SELECT
                    RTRIM(cod_unidad) AS item,
                    RTRIM(descripcion) AS descripcion,
                    Nivel AS nivel,
                    unidad_omision,
                    reporta_renta,
                    activa,
                    RTRIM(Cta_Renta) AS cta_renta,
                    RTRIM(Cta_Renta_Gasto) AS cta_renta_gasto
                FROM CntX_Unidades
                WHERE COD_CONTABILIDAD = @CodContabilidad
                ORDER BY cod_unidad;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDB,
                codEmpresa,
                sql,
                new { CodContabilidad = codContabilidad });
        }

        /// <summary>
        /// Reestructura los movimientos del período antes de generar el balance.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa cuya conexión se utilizará.</param>
        /// <param name="request">Contabilidad, período y tipo de revisión solicitados.</param>
        /// <returns>Resultado de la reestructuración.</returns>
        public ErrorDto CntX_Movimientos_Restructurar(
            int codEmpresa,
            CntXCalculosRestructuraRequest request)
        {
            return _mCntXCalculosDb.SbCntX_RestructuraMovimientosRSM(
                codEmpresa,
                request);
        }
    }
}
