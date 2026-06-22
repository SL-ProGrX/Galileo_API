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
        public ErrorDto<List<DropDownListaGenericaModel>> CrPolizasRegistro_PolizaLinea_Obtener(int codEmpresa)
            => _bl.CrPolizasRegistro_PolizaLinea_Obtener(codEmpresa);

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
    }
}