using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Polizas;
using static Galileo_API.Models.ProGrX_Polizas.FrmCrPolizaProcRecepcionModels;

namespace Galileo_API.BusinessLogic.ProGrX_Polizas
{
    public class FrmCrPolizaProcRecepcionBL
    {
        private readonly FrmCrPolizaProcRecepcionDB _db;

        public FrmCrPolizaProcRecepcionBL(IConfiguration config)
        {
            _db = new FrmCrPolizaProcRecepcionDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Crd_PolizaProcRecepcion_Polizas_Listar(int codEmpresa)
            => _db.Crd_PolizaProcRecepcion_Polizas_Listar(codEmpresa);

        public ErrorDto<List<DropDownListaGenericaModel>> Crd_PolizaProcRecepcion_Unidades_Listar(int codEmpresa, int codContabilidad)
            => _db.Crd_PolizaProcRecepcion_Unidades_Listar(codEmpresa, codContabilidad);

        public ErrorDto<List<DropDownListaGenericaModel>> Crd_PolizaProcRecepcion_CentrosCosto_Listar(int codEmpresa, int codContabilidad, string codUnidad)
            => _db.Crd_PolizaProcRecepcion_CentrosCosto_Listar(codEmpresa, codContabilidad, codUnidad);

        public ErrorDto<List<DropDownListaGenericaModel>> Crd_PolizaProcRecepcion_Divisas_Listar(int codEmpresa, int codContabilidad)
            => _db.Crd_PolizaProcRecepcion_Divisas_Listar(codEmpresa, codContabilidad);

        public ErrorDto<DropDownListaGenericaModel?> Crd_PolizaProcRecepcion_DivisaLocal_Obtener(int codEmpresa, int codContabilidad)
            => _db.Crd_PolizaProcRecepcion_DivisaLocal_Obtener(codEmpresa, codContabilidad);

        public ErrorDto<decimal> Crd_PolizaProcRecepcion_TipoCambio_Obtener(int codEmpresa, int codContabilidad, string divisa)
            => _db.Crd_PolizaProcRecepcion_TipoCambio_Obtener(codEmpresa, codContabilidad, divisa);

        public ErrorDto<PolizaDatos?> Crd_PolizaProcRecepcion_Poliza_Datos_Obtener(int codEmpresa, string codPoliza)
            => _db.Crd_PolizaProcRecepcion_Poliza_Datos_Obtener(codEmpresa, codPoliza);

        public ErrorDto<PolizaAseguradoraCorteConsulta?> Crd_PolizaProcRecepcion_Corte_Consultar(int codEmpresa, DateTime corte, string codPoliza)
            => _db.Crd_PolizaProcRecepcion_Corte_Consultar(codEmpresa, corte, codPoliza);

        public ErrorDto<List<PolizaAseguradoraCorteDetalleConsulta>> Crd_PolizaProcRecepcion_Corte_Detalle_Consultar(int codEmpresa, DateTime corte, string codPoliza, int idFactura)
            => _db.Crd_PolizaProcRecepcion_Corte_Detalle_Consultar(codEmpresa, corte, codPoliza, idFactura);

        public ErrorDto<PolizaAseguradoraCorte?> Crd_PolizaProcRecepcion_Corte_Validar(int codEmpresa, DateTime corte, string codPoliza, int idFactura)
            => _db.Crd_PolizaProcRecepcion_Corte_Validar(codEmpresa, corte, codPoliza, idFactura);

        public ErrorDto<PolizaAseguradoraCorte?> Crd_PolizaProcRecepcion_Archivo_Cargar(int codEmpresa, string usuario, PolizaArchivoCargarRequest request)
            => _db.Crd_PolizaProcRecepcion_Archivo_Cargar(codEmpresa, usuario, request);

        public ErrorDto<string> Crd_PolizaProcRecepcion_Detalle_Eliminar(int codEmpresa, string usuario, PolizaDetalleEliminarRequest request)
            => _db.Crd_PolizaProcRecepcion_Detalle_Eliminar(codEmpresa, usuario, request);

        public ErrorDto<PolizaAseguradoraCorte?> Crd_PolizaProcRecepcion_Factura_Generar(int codEmpresa, string usuario, PolizaAseguradoraCorteData datos)
            => _db.Crd_PolizaProcRecepcion_Factura_Generar(codEmpresa, usuario, datos);
    }
}
