using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrCatalogoCreditosBl
    {
        private readonly FrmCrCatalogoCreditosDb _db;

        public FrmCrCatalogoCreditosBl(IConfiguration config)
            => _db = new FrmCrCatalogoCreditosDb(config);

        public ErrorDto<List<CrCatalogoCreditoData>> CrCatalogoCreditos_Obtener(int codEmpresa, bool soloAutoGestion)
        {
            return _db.CrCatalogoCreditos_Obtener(codEmpresa, soloAutoGestion);
        }

        public ErrorDto<CrCatalogoCreditoData?> CrCatalogoCreditos_Consultar(int codEmpresa, string codigo)
        {
            return _db.CrCatalogoCreditos_Consultar(codEmpresa, codigo);
        }

        public ErrorDto<List<CrCatalogoCreditoCuentaData>> CrCatalogoCreditos_Cuentas_Obtener(int codEmpresa, string codigo)
        {
            return _db.CrCatalogoCreditos_Cuentas_Obtener(codEmpresa, codigo);
        }

        public ErrorDto<CrCatalogoCreditoAsignacionesData> CrCatalogoCreditos_Asignaciones_Obtener(int codEmpresa, string codigo)
        {
            return _db.CrCatalogoCreditos_Asignaciones_Obtener(codEmpresa, codigo);
        }

        public ErrorDto<List<CrCatalogoCreditoAdjuntoData>> CrCatalogoCreditos_Adjuntos_Obtener(int codEmpresa, string codigo)
        {
            return _db.CrCatalogoCreditos_Adjuntos_Obtener(codEmpresa, codigo);
        }

        public ErrorDto<List<CrCatalogoCreditoBusquedaData>> CrCatalogoCreditos_Oficinas_Obtener(int codEmpresa)
        {
            return _db.CrCatalogoCreditos_Oficinas_Obtener(codEmpresa);
        }

        public ErrorDto<List<CrCatalogoCreditoBusquedaData>> CrCatalogoCreditos_Planes_Obtener(int codEmpresa)
        {
            return _db.CrCatalogoCreditos_Planes_Obtener(codEmpresa);
        }

        public ErrorDto<List<CrCatalogoCreditoBusquedaData>> CrCatalogoCreditos_Divisas_Obtener(int codEmpresa)
        {
            return _db.CrCatalogoCreditos_Divisas_Obtener(codEmpresa);
        }

        public ErrorDto CrCatalogoCreditos_Asignacion_Guardar(int codEmpresa, CrCatalogoCreditoAsignacionGuardarRequest request)
        {
            return _db.CrCatalogoCreditos_Asignacion_Guardar(codEmpresa, request);
        }

        public ErrorDto<CrCatalogoCreditoRangosBaseData> CrCatalogoCreditos_RangosBase_Obtener(int codEmpresa, string codigo)
        {
            return _db.CrCatalogoCreditos_RangosBase_Obtener(codEmpresa, codigo);
        }

        public ErrorDto<int> CrCatalogoCreditos_RangoBase_Guardar(int codEmpresa, CrCatalogoCreditoRangoBaseGuardarRequest request)
        {
            return _db.CrCatalogoCreditos_RangoBase_Guardar(codEmpresa, request);
        }

        public ErrorDto<int> CrCatalogoCreditos_RangoPlazo_Guardar(int codEmpresa, CrCatalogoCreditoRangoPlazoGuardarRequest request)
        {
            return _db.CrCatalogoCreditos_RangoPlazo_Guardar(codEmpresa, request);
        }

        public ErrorDto CrCatalogoCreditos_RangoGarantia_Guardar(int codEmpresa, CrCatalogoCreditoRangoGarantiaGuardarRequest request)
        {
            return _db.CrCatalogoCreditos_RangoGarantia_Guardar(codEmpresa, request);
        }

        public ErrorDto<CrCatalogoCreditoRangosLiquidezData> CrCatalogoCreditos_RangosLiquidez_Obtener(int codEmpresa, string codigo)
        {
            return _db.CrCatalogoCreditos_RangosLiquidez_Obtener(codEmpresa, codigo);
        }

        public ErrorDto CrCatalogoCreditos_LiquidezBono_Guardar(int codEmpresa, CrCatalogoCreditoLiquidezBonoGuardarRequest request)
        {
            return _db.CrCatalogoCreditos_LiquidezBono_Guardar(codEmpresa, request);
        }

        public ErrorDto CrCatalogoCreditos_LiquidezCapacidad_Guardar(int codEmpresa, CrCatalogoCreditoLiquidezCapacidadGuardarRequest request)
        {
            return _db.CrCatalogoCreditos_LiquidezCapacidad_Guardar(codEmpresa, request);
        }

        public ErrorDto<List<CrCatalogoCreditoComiteEstudioData>> CrCatalogoCreditos_ComitesEstudio_Obtener(int codEmpresa, string codigo)
        {
            return _db.CrCatalogoCreditos_ComitesEstudio_Obtener(codEmpresa, codigo);
        }

        public ErrorDto<int> CrCatalogoCreditos_ComiteEstudio_Guardar(int codEmpresa, CrCatalogoCreditoComiteEstudioGuardarRequest request)
        {
            return _db.CrCatalogoCreditos_ComiteEstudio_Guardar(codEmpresa, request);
        }

        public ErrorDto CrCatalogoCreditos_Guardar(int codEmpresa, CrCatalogoCreditoGuardarRequest request)
        {
            return _db.CrCatalogoCreditos_Guardar(codEmpresa, request);
        }

        public ErrorDto CrCatalogoCreditos_PeL_Guardar(int codEmpresa, CrCatalogoCreditoPeLGuardarRequest request)
        {
            return _db.CrCatalogoCreditos_PeL_Guardar(codEmpresa, request);
        }

        public ErrorDto CrCatalogoCreditos_Eliminar(int codEmpresa, string codigo, string usuario)
        {
            return _db.CrCatalogoCreditos_Eliminar(codEmpresa, codigo, usuario);
        }
    }
}
