using Galileo.DataBaseTier.ProGrX_Personas;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using Galileo.Models;

namespace Galileo.BusinessLogic.ProGrX_Personas
{
    public class FrmAFCrLiquidacionMasivaBL
    {
        private readonly FrmAFCrLiquidacionMasivaDB _db;

        public FrmAFCrLiquidacionMasivaBL(IConfiguration config)
        {
            _db = new FrmAFCrLiquidacionMasivaDB(config);
        }

        public ErrorDto<List<AfLiquidacionMasiva>> AF_LiquidacionMasiva_Obtener(int CodEmpresa, AfLiquidacionMasivaFiltros Filtro)
        {
            return _db.AF_LiquidacionMasiva_Obtener(CodEmpresa, Filtro);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_LiquidacionMasiva_Obtener_Causas(int CodEmpresa, string? tipoApl = null, DateTime? inicio = null, DateTime? corte = null)
        {
            return _db.AF_LiquidacionMasiva_Obtener_Causas(CodEmpresa, tipoApl, inicio, corte);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_LiquidacionMasiva_Obtener_Instituciones(int CodEmpresa)
        {
            return _db.AF_LiquidacionMasiva_Obtener_Instituciones(CodEmpresa);
        }

        public ErrorDto AF_LiquidacionMasiva(int CodEmpresa, int RenunciaId, string Usuario, short S06 = 1)
        {
            return _db.AF_LiquidacionMasiva(CodEmpresa, RenunciaId, Usuario, S06);
        }

        public ErrorDto<AfLiqMasivaProgreso> AF_LiquidacionMasiva_Proceso_Iniciar(int CodEmpresa, AfLiqMasivaIniciarRequest request)
        {
            return _db.AF_LiquidacionMasiva_Proceso_Iniciar(CodEmpresa, request);
        }

        public ErrorDto<AfLiqMasivaProgreso> AF_LiquidacionMasiva_Proceso_ProcesarLote(int CodEmpresa, Guid ProcesoId, int Tamano)
        {
            return _db.AF_LiquidacionMasiva_Proceso_ProcesarLote(CodEmpresa, ProcesoId, Tamano);
        }

        public ErrorDto<AfLiqMasivaProgreso> AF_LiquidacionMasiva_Proceso_Estado_Obtener(int CodEmpresa, Guid ProcesoId)
        {
            return _db.AF_LiquidacionMasiva_Proceso_Estado_Obtener(CodEmpresa, ProcesoId);
        }

        public ErrorDto<AfLiqMasivaProgreso> AF_LiquidacionMasiva_Proceso_Activo_Obtener(int CodEmpresa, string Usuario)
        {
            return _db.AF_LiquidacionMasiva_Proceso_Activo_Obtener(CodEmpresa, Usuario);
        }
    }
}
