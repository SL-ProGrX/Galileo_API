using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrPrendasBl
    {
        private readonly FrmCrPrendasDb _db;

        public FrmCrPrendasBl(IConfiguration config)
        {
            _db = new FrmCrPrendasDb(config);
        }

        public ErrorDto<List<CrPrendaListaData>> CR_Prendas_Obtener(
            int codEmpresa,
            long operacion,
            string expediente)
        {
            return _db.CrPrendas_Obtener(codEmpresa, operacion, expediente);
        }

        public ErrorDto<CrPrendaDetalleData> CR_Prendas_ObtenerDetalle(int codEmpresa, long prendaId)
        {
            return _db.CrPrendas_ObtenerDetalle(codEmpresa, prendaId);
        }

        public ErrorDto<List<CrPrendaTipoListaData>> CR_Prendas_TiposActivos(int codEmpresa)
        {
            return _db.CrPrendas_TiposActivos(codEmpresa);
        }

        public ErrorDto<List<CrPrendaTipoListaData>> CR_Prendas_CatalogoLista(int codEmpresa, string tipoCatalogo)
        {
            return _db.CrPrendas_CatalogoLista(codEmpresa, tipoCatalogo);
        }

        public ErrorDto<List<CrPrendaTipoListaData>> CR_Prendas_UnidadesLista(int codEmpresa, string aplicacion)
        {
            return _db.CrPrendas_UnidadesLista(codEmpresa, aplicacion);
        }

        public ErrorDto<List<CrPrendaTipoListaData>> CR_Prendas_ParentescosLista(int codEmpresa)
        {
            return _db.CrPrendas_ParentescosLista(codEmpresa);
        }

        public ErrorDto<List<CrPrendaTipoListaData>> CR_Prendas_TiposIdentificacionLista(int codEmpresa)
        {
            return _db.CrPrendas_TiposIdentificacionLista(codEmpresa);
        }

        public ErrorDto<List<CrPrendaAnotacionData>> CR_Prendas_AnotacionesLista(int codEmpresa, long prendaId)
        {
            return _db.CrPrendas_AnotacionesLista(codEmpresa, prendaId);
        }

        public ErrorDto<List<CrPrendaPolizaCoberturaData>> CR_Prendas_PolizasList(
            int codEmpresa,
            string tipoPrenda,
            long prendaId)
        {
            return _db.CrPrendas_PolizasList(codEmpresa, tipoPrenda, prendaId);
        }

        public ErrorDto<List<CrPrendaHistoricoAvaluoData>> CR_Prendas_AvaluosLista(int codEmpresa, long prendaId)
        {
            return _db.CrPrendas_AvaluosLista(codEmpresa, prendaId);
        }

        public ErrorDto<string> CR_Prendas_AvaluoGuardar(int codEmpresa, CrPrendaAvaluoGuardarRequest request)
        {
            return _db.CrPrendas_AvaluoGuardar(codEmpresa, request);
        }

        public ErrorDto<string> CR_Prendas_NotariadoGuardar(int codEmpresa, CrPrendaNotariadoGuardarRequest request)
        {
            return _db.CrPrendas_NotariadoGuardar(codEmpresa, request);
        }

        public ErrorDto<string> CR_Prendas_NotaGuardar(int codEmpresa, CrPrendaNotaGuardarRequest request)
        {
            return _db.CrPrendas_NotaGuardar(codEmpresa, request);
        }

        public ErrorDto<string> CR_Prendas_PolizaCoberturaGuardar(
            int codEmpresa,
            CrPrendaPolizaCoberturaGuardarRequest request)
        {
            return _db.CrPrendas_PolizaCoberturaGuardar(codEmpresa, request);
        }

        public ErrorDto<List<CrPrendaHistoricoPolizaData>> CR_Prendas_PolizasExternasLista(int codEmpresa, long prendaId)
        {
            return _db.CrPrendas_PolizasExternasLista(codEmpresa, prendaId);
        }

        public ErrorDto<CrPrendaDetalleData> CR_Prendas_PolizaExternaLoad(int codEmpresa, long prendaId, int polizaExtId)
        {
            return _db.CrPrendas_PolizaExternaLoad(codEmpresa, prendaId, polizaExtId);
        }

        public ErrorDto<long> CR_Prendas_Guardar(int codEmpresa, CrPrendaGuardarCompletaRequest request)
        {
            return _db.CrPrendas_Guardar(codEmpresa, request);
        }

        public ErrorDto<string> CR_Prendas_PolizaExternaGuardar(
            int codEmpresa,
            CrPrendaPolizaExternaGuardarRequest request)
        {
            return _db.CrPrendas_PolizaExternaGuardar(codEmpresa, request);
        }

        public ErrorDto CR_Prendas_Eliminar(int codEmpresa, CrPrendasEliminarRequest request)
        {
            return _db.CrPrendas_Eliminar(codEmpresa, request);
        }
    }
}
