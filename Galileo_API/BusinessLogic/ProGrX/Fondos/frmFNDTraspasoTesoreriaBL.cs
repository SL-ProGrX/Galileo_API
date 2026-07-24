using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.BusinessLogic.ProGrX.Fondos
{
    public class FrmFndTraspasoTesoreriaBl
    {
        private readonly FrmFndTraspasoTesoreriaDb _db;

        public FrmFndTraspasoTesoreriaBl(IConfiguration config)
        {
            _db = new FrmFndTraspasoTesoreriaDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TraspasoTesoreria_Bancos_Obtener(int codEmpresa)
        {
            return _db.TraspasoTesoreria_Bancos_Obtener(codEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TraspasoTesoreria_ConceptosRetencion_Obtener(int codEmpresa)
        {
            return _db.TraspasoTesoreria_ConceptosRetencion_Obtener(codEmpresa);
        }

        public ErrorDto<List<TesTokenConsultaResult>> Tes_Token_Consulta(TesTokenConsultaParams param)
        {
            return _db.Tes_Token_Consulta(param);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TraspasoTesoreria_LiquidacionBancos_Obtener(FndTraspasoTesoreriaFiltroParams param)
        {
            return _db.TraspasoTesoreria_LiquidacionBancos_Obtener(param);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TraspasoTesoreria_LiquidacionUsuarios_Obtener(FndTraspasoTesoreriaFiltroParams param)
        {
            return _db.TraspasoTesoreria_LiquidacionUsuarios_Obtener(param);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TraspasoTesoreria_LiquidacionSistemas_Obtener(FndTraspasoTesoreriaFiltroParams param)
        {
            return _db.TraspasoTesoreria_LiquidacionSistemas_Obtener(param);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TraspasoTesoreria_LiquidacionTokens_Obtener(FndTraspasoTesoreriaFiltroParams param)
        {
            return _db.TraspasoTesoreria_LiquidacionTokens_Obtener(param);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> TraspasoTesoreria_LiquidacionOficinas_Obtener(FndTraspasoTesoreriaFiltroParams param)
        {
            return _db.TraspasoTesoreria_LiquidacionOficinas_Obtener(param);
        }

        public ErrorDto<TesTokenNewResult> Tes_Token_New(TesTokenNewParams param)
        {
            return _db.Tes_Token_New(param);
        }

        public ErrorDto<FndTraspasoTesoreriaFixResult> TraspasoTesoreria_Fix(int codEmpresa)
        {
            return _db.TraspasoTesoreria_Fix(codEmpresa);
        }

        public ErrorDto<string> TraspasoTesoreria_ParametroValor_Obtener(int codEmpresa, string codigo)
        {
            return _db.TraspasoTesoreria_ParametroValor_Obtener(codEmpresa, codigo);
        }

        public ErrorDto<List<FndTraspasoTesoreriaLiquidacionConsultaResult>> TraspasoTesoreria_LiquidacionConsulta(FndTraspasoTesoreriaLiquidacionConsultaParams param)
        {
            return _db.TraspasoTesoreria_LiquidacionConsulta(param);
        }

        public ErrorDto<List<FndTraspasoTesoreriaDuplicadosResult>> RevisaDuplicadosEnLaRemesa(FndTraspasoTesoreriaDuplicadosParams param)
        {
            return _db.RevisaDuplicadosEnLaRemesa(param);
        }

        public ErrorDto<bool> RetLiqTesoreria(FndRetLiqTesoreriaParams param)
        {
            return _db.RetLiqTesoreria(param);
        }

        public ErrorDto<bool> TraspasoTesoreria_Update(FndTraspasoTesoreriaUpdateParams param)
        {
            return _db.TraspasoTesoreria_Update(param);
        }

        public ErrorDto<bool> RetLiqTesoreria_Unificado(FndRetLiqTesoreriaUnificadoParams param)
        {
            return _db.RetLiqTesoreria_Unificado(param);
        }

        public ErrorDto<List<FndTraspasoTesoreriaLiquidacionConsultaResult>> TraspasoTesoreria_LiquidacionDetalle(FndTraspasoTesoreriaDetalleParams param)
        {
            return _db.TraspasoTesoreria_LiquidacionDetalle(param);
        }

        public ErrorDto<FndTraspasoTesoreriaProcesarLoteResult> FND_TraspasoTesoreria_ProcesarLote(
            FndTraspasoTesoreriaProcesarLoteRequest request)
        {
            return _db.FND_TraspasoTesoreria_ProcesarLote(request);
        }

        /// <summary>
        /// Inicializa el proceso persistente de traspaso de tesorería.
        /// </summary>
        public ErrorDto<FndTraspasoTesoreriaProcesoResult> FND_TraspasoTesoreria_Proceso_Iniciar(
            int codEmpresa,
            FndTraspasoTesoreriaProcesoIniciarRequest request)
        {
            return _db.FND_TraspasoTesoreria_Proceso_Iniciar(codEmpresa, request);
        }

        /// <summary>
        /// Ejecuta el siguiente lote pendiente del traspaso de tesorería.
        /// </summary>
        public ErrorDto<FndTraspasoTesoreriaProcesoResult> FND_TraspasoTesoreria_Proceso_Continuar(
            int codEmpresa,
            FndTraspasoTesoreriaProcesoContinuarRequest request)
        {
            return _db.FND_TraspasoTesoreria_Proceso_Continuar(codEmpresa, request);
        }
    }
}
