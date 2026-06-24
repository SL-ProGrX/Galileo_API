using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrPolizasRegistroBl
    {
        private readonly FrmCrPolizasRegistroDb _db;

        public FrmCrPolizasRegistroBl(IConfiguration config)
        {
            _db = new FrmCrPolizasRegistroDb(config);
        }

        public ErrorDto<List<CrPolizasRegistroPolizaLineaItem>> CrPolizasRegistro_PolizaLinea_Obtener(int codEmpresa)
            => _db.CrPolizasRegistro_PolizaLinea_Obtener(codEmpresa);

        public ErrorDto<int> CrPolizasRegistro_Operacion_Navegar_Obtener(
            int codEmpresa,
            int operacion,
            int direccion)
            => _db.CrPolizasRegistro_Operacion_Navegar_Obtener(codEmpresa, operacion, direccion);

        public ErrorDto<CrPolizasRegistroOperacionData> CrPolizasRegistro_Operacion_Obtener(
            int codEmpresa,
            int operacion)
            => _db.CrPolizasRegistro_Operacion_Obtener(codEmpresa, operacion);

        public ErrorDto<List<CrPolizasRegistroListadoItem>> CrPolizasRegistro_Lista_Obtener(
            int codEmpresa,
            int operacion)
            => _db.CrPolizasRegistro_Lista_Obtener(codEmpresa, operacion);

        public ErrorDto<CrPolizasRegistroFormData> CrPolizasRegistro_Detalle_Obtener(
            int codEmpresa,
            int operacion,
            int num_poliza)
            => _db.CrPolizasRegistro_Detalle_Obtener(codEmpresa, operacion, num_poliza);

        public ErrorDto<List<DropDownListaGenericaModel>> CrPolizasRegistro_OperacionPoliza_Obtener(
            int codEmpresa,
            int operacion)
            => _db.CrPolizasRegistro_OperacionPoliza_Obtener(codEmpresa, operacion);

        public ErrorDto<List<CrPolizasRegistroPagoItem>> CrPolizasRegistro_Pagos_Obtener(
            int codEmpresa,
            int operacion,
            int num_poliza)
            => _db.CrPolizasRegistro_Pagos_Obtener(codEmpresa, operacion, num_poliza);

        public ErrorDto<List<CrPolizasRegistroRecaudacionItem>> CrPolizasRegistro_Recaudacion_Obtener(
            int codEmpresa,
            int operacion,
            int num_poliza)
            => _db.CrPolizasRegistro_Recaudacion_Obtener(codEmpresa, operacion, num_poliza);

        public ErrorDto<List<CrPolizasRegistroAcreedorItem>> CrPolizasRegistro_Acreedores_Obtener(
            int codEmpresa,
            int operacion,
            int num_poliza)
            => _db.CrPolizasRegistro_Acreedores_Obtener(codEmpresa, operacion, num_poliza);

        public ErrorDto<List<DropDownListaGenericaModel>> CrPolizasRegistro_PlanPagos_Obtener(
            int codEmpresa,
            int operacion)
            => _db.CrPolizasRegistro_PlanPagos_Obtener(codEmpresa, operacion);

        public ErrorDto<List<CrPolizasRegistroBeneficiarioItem>> CrPolizasRegistro_Beneficiarios_Obtener(
            int codEmpresa,
            int operacion,
            int numPoliza)
            => _db.CrPolizasRegistro_Beneficiarios_Obtener(codEmpresa, operacion, numPoliza);

        public ErrorDto<bool> CrPolizasRegistro_Acreedor_Aplicar(
            int codEmpresa,
            CrPolizasRegistroAcreedorAplicarRequest request)
            => _db.CrPolizasRegistro_Acreedor_Aplicar(codEmpresa, request);

        public ErrorDto<CrPolizasRegistroPlanPagoDetalleData> CrPolizasRegistro_PlanPago_Detalle_Obtener(
            int codEmpresa,
            string request)
        {
            var data = 
                JsonConvert.DeserializeObject<CrPolizasRegistroPlanPagoDetalleRequest>(request) ?? new CrPolizasRegistroPlanPagoDetalleRequest();
            return _db.CrPolizasRegistro_PlanPago_Detalle_Obtener(codEmpresa, data);
        }

        public ErrorDto<int> CrPolizasRegistro_PolizaIntegrada_Guardar(
            int codEmpresa,
            CrPolizasRegistroPolizaIntegradaGuardarRequest request)
            => _db.CrPolizasRegistro_PolizaIntegrada_Guardar(codEmpresa, request);

        public ErrorDto<int> CrPolizasRegistro_PolizaRetencion_Guardar(
            int codEmpresa,
            CrPolizasRegistroPolizaRetencionGuardarRequest request)
            => _db.CrPolizasRegistro_PolizaRetencion_Guardar(codEmpresa, request);
    }
}