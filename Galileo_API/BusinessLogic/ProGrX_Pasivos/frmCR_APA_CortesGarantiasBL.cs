using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Pasivos;
using Galileo_API.Models.ProGrX_Pasivos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_Pasivos
{
    public class FrmCrApaCortesGarantiasBL
    {
        private readonly FrmCrApaCortesGarantiasDB _db;

        public FrmCrApaCortesGarantiasBL(IConfiguration config)
        {
            _db = new FrmCrApaCortesGarantiasDB(config);
        }

        public ErrorDto<List<FrmCrApaCortesGarantiasCatalogoDto>> CR_APA_CortesGarantias_Acreedores_Obtener(int codEmpresa)
        {
            return _db.CR_APA_CortesGarantias_Acreedores_Obtener(codEmpresa);
        }

        public ErrorDto<List<FrmCrApaCortesGarantiasCatalogoDto>> CR_APA_CortesGarantias_Operaciones_Obtener(int codEmpresa, string cod_acreedor)
        {
            return _db.CR_APA_CortesGarantias_Operaciones_Obtener(codEmpresa, cod_acreedor);
        }

        public ErrorDto<FrmCrApaCortesGarantiasEncabezadoDto?> CR_APA_CortesGarantias_Encabezado_Obtener(int codEmpresa, string operacion)
        {
            return _db.CR_APA_CortesGarantias_Encabezado_Obtener(codEmpresa, operacion);
        }

        public ErrorDto<List<FrmCrApaCortesGarantiasCatalogoDto>> CR_APA_CortesGarantias_Catalogo_Obtener(int codEmpresa, string tipo)
        {
            return _db.CR_APA_CortesGarantias_Catalogo_Obtener(codEmpresa, tipo);
        }

        public ErrorDto<List<FrmCrApaCortesGarantiasCorteDto>> CR_APA_CortesGarantias_Historico_Obtener(int codEmpresa, string cod_acreedor, string operacion)
        {
            return _db.CR_APA_CortesGarantias_Historico_Obtener(codEmpresa, cod_acreedor, operacion);
        }

        public ErrorDto<FrmCrApaCortesGarantiasCorteDatosDto?> CR_APA_CortesGarantias_Corte_Obtener(int codEmpresa, string request)
        {
            return _db.CR_APA_CortesGarantias_Corte_Obtener(codEmpresa, Deserializar<FrmCrApaCortesGarantiasClaveRequest>(request));
        }

        public ErrorDto<List<FrmCrApaCortesGarantiasDetalleDto>> CR_APA_CortesGarantias_Detalle_Obtener(int codEmpresa, string request)
        {
            return _db.CR_APA_CortesGarantias_Detalle_Obtener(codEmpresa, Deserializar<FrmCrApaCortesGarantiasConsultaRequest>(request));
        }

        public ErrorDto<List<FrmCrApaCortesGarantiasDetalleDto>> CR_APA_CortesGarantias_Inclusiones_Obtener(int codEmpresa, string request)
        {
            return _db.CR_APA_CortesGarantias_Inclusiones_Obtener(codEmpresa, Deserializar<FrmCrApaCortesGarantiasConsultaRequest>(request));
        }

        public ErrorDto<FrmCrApaCortesGarantiasTotalesDto?> CR_APA_CortesGarantias_Totales_Obtener(int codEmpresa, string request)
        {
            return _db.CR_APA_CortesGarantias_Totales_Obtener(codEmpresa, Deserializar<FrmCrApaCortesGarantiasClaveRequest>(request));
        }

        public ErrorDto CR_APA_CortesGarantias_Guardar(int codEmpresa, FrmCrApaCortesGarantiasGuardarRequest request)
        {
            return _db.CR_APA_CortesGarantias_Guardar(codEmpresa, request);
        }

        public ErrorDto CR_APA_CortesGarantias_Cerrar(int codEmpresa, FrmCrApaCortesGarantiasClaveRequest request)
        {
            return _db.CR_APA_CortesGarantias_Cerrar(codEmpresa, request);
        }

        public ErrorDto CR_APA_CortesGarantias_Actualizar(int codEmpresa, FrmCrApaCortesGarantiasClaveRequest request)
        {
            return _db.CR_APA_CortesGarantias_Actualizar(codEmpresa, request);
        }

        public ErrorDto CR_APA_CortesGarantias_Excluir(int codEmpresa, FrmCrApaCortesGarantiasExcluirRequest request)
        {
            return _db.CR_APA_CortesGarantias_Excluir(codEmpresa, request);
        }

        public ErrorDto CR_APA_CortesGarantias_Incluir(int codEmpresa, FrmCrApaCortesGarantiasIncluirRequest request)
        {
            return _db.CR_APA_CortesGarantias_Incluir(codEmpresa, request);
        }

        private static T Deserializar<T>(string request) where T : new()
        {
            return JsonConvert.DeserializeObject<T>(request) ?? new T();
        }
    }
}
