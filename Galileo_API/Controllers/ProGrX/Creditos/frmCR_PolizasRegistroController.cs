using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FrmCrPolizasRegistroController : ControllerBase
    {
        private readonly FrmCrPolizasRegistroBl _bl;

        public FrmCrPolizasRegistroController(IConfiguration config)
        {
            _bl = new FrmCrPolizasRegistroBl(config);
        }

        [HttpGet("CrPolizasRegistro_PolizaLinea_Obtener")]
        public ErrorDto<List<CrPolizasRegistroPolizaLineaItem>> CrPolizasRegistro_PolizaLinea_Obtener(int codEmpresa)
            => _bl.CrPolizasRegistro_PolizaLinea_Obtener(codEmpresa);

        [HttpGet("CrPolizasRegistro_Operacion_Navegar_Obtener")]
        public ErrorDto<int> CrPolizasRegistro_Operacion_Navegar_Obtener(
            int codEmpresa,
            int operacion,
            int direccion)
            => _bl.CrPolizasRegistro_Operacion_Navegar_Obtener(codEmpresa, operacion, direccion);

        [HttpGet("CrPolizasRegistro_Operacion_Obtener")]
        public ErrorDto<CrPolizasRegistroOperacionData> CrPolizasRegistro_Operacion_Obtener(
            int codEmpresa,
            int operacion)
            => _bl.CrPolizasRegistro_Operacion_Obtener(codEmpresa, operacion);

        [HttpGet("CrPolizasRegistro_Lista_Obtener")]
        public ErrorDto<List<CrPolizasRegistroListadoItem>> CrPolizasRegistro_Lista_Obtener(
            int codEmpresa,
            int operacion)
            => _bl.CrPolizasRegistro_Lista_Obtener(codEmpresa, operacion);

        [HttpGet("CrPolizasRegistro_Detalle_Obtener")]
        public ErrorDto<CrPolizasRegistroFormData> CrPolizasRegistro_Detalle_Obtener(
            int codEmpresa,
            int operacion,
            int num_poliza)
            => _bl.CrPolizasRegistro_Detalle_Obtener(codEmpresa, operacion, num_poliza);

        [HttpGet("CrPolizasRegistro_OperacionPoliza_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrPolizasRegistro_OperacionPoliza_Obtener(
            int codEmpresa,
            int operacion)
            => _bl.CrPolizasRegistro_OperacionPoliza_Obtener(codEmpresa, operacion);

        [HttpGet("CrPolizasRegistro_Pagos_Obtener")]
        public ErrorDto<List<CrPolizasRegistroPagoItem>> CrPolizasRegistro_Pagos_Obtener(
            int codEmpresa,
            int operacion,
            int num_poliza)
            => _bl.CrPolizasRegistro_Pagos_Obtener(codEmpresa, operacion, num_poliza);

        [HttpGet("CrPolizasRegistro_Recaudacion_Obtener")]
        public ErrorDto<List<CrPolizasRegistroRecaudacionItem>> CrPolizasRegistro_Recaudacion_Obtener(
            int codEmpresa,
            int operacion,
            int num_poliza)
            => _bl.CrPolizasRegistro_Recaudacion_Obtener(codEmpresa, operacion, num_poliza);

        [HttpGet("CrPolizasRegistro_Acreedores_Obtener")]
        public ErrorDto<List<CrPolizasRegistroAcreedorItem>> CrPolizasRegistro_Acreedores_Obtener(
            int codEmpresa,
            int operacion,
            int num_poliza)
            => _bl.CrPolizasRegistro_Acreedores_Obtener(codEmpresa, operacion, num_poliza);

        [HttpGet("CrPolizasRegistro_PlanPagos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrPolizasRegistro_PlanPagos_Obtener(
            int codEmpresa,
            int operacion)
            => _bl.CrPolizasRegistro_PlanPagos_Obtener(codEmpresa, operacion);

        [HttpGet("CrPolizasRegistro_Beneficiarios_Obtener")]
        public ErrorDto<List<CrPolizasRegistroBeneficiarioItem>> CrPolizasRegistro_Beneficiarios_Obtener(
            int codEmpresa,
            int operacion,
            int numPoliza)
            => _bl.CrPolizasRegistro_Beneficiarios_Obtener(codEmpresa, operacion, numPoliza);

        [HttpPost("CrPolizasRegistro_Acreedor_Aplicar")]
        public ErrorDto<bool> CrPolizasRegistro_Acreedor_Aplicar(
            int codEmpresa,
            CrPolizasRegistroAcreedorAplicarRequest request)
            => _bl.CrPolizasRegistro_Acreedor_Aplicar(codEmpresa, request);

        [HttpGet("CrPolizasRegistro_PlanPago_Detalle_Obtener")]
        public ErrorDto<CrPolizasRegistroPlanPagoDetalleData> CrPolizasRegistro_PlanPago_Detalle_Obtener(
            int codEmpresa,
            string request)
            => _bl.CrPolizasRegistro_PlanPago_Detalle_Obtener(codEmpresa, request);

        [HttpPost("CrPolizasRegistro_PolizaIntegrada_Guardar")]
        public ErrorDto<int> CrPolizasRegistro_PolizaIntegrada_Guardar(
            int codEmpresa,
            CrPolizasRegistroPolizaIntegradaGuardarRequest request)
            => _bl.CrPolizasRegistro_PolizaIntegrada_Guardar(codEmpresa, request);

        [HttpPost("CrPolizasRegistro_PolizaRetencion_Guardar")]
        public ErrorDto<int> CrPolizasRegistro_PolizaRetencion_Guardar(
            int codEmpresa,
            CrPolizasRegistroPolizaRetencionGuardarRequest request)
            => _bl.CrPolizasRegistro_PolizaRetencion_Guardar(codEmpresa, request);
    }
}