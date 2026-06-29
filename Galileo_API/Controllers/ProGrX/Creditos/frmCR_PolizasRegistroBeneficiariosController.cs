using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCRPolizasRegistroBeneficiariosController : ControllerBase
    {
        private readonly FrmCRPolizasRegistroBeneficiariosBL _bl;

        public FrmCRPolizasRegistroBeneficiariosController(IConfiguration config)
        {
            _bl = new FrmCRPolizasRegistroBeneficiariosBL(config);
        }

        [Authorize]
        [HttpGet("CrPolizasRegistroBeneficiarios_Parentescos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrPolizasRegistroBeneficiarios_Parentescos_Obtener(int codEmpresa)
            => _bl.CrPolizasRegistroBeneficiarios_Parentescos_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("CrPolizasRegistroBeneficiarios_Encabezado_Obtener")]
        public ErrorDto<CrPolizasRegistroBeneficiariosEncabezadoData?> CrPolizasRegistroBeneficiarios_Encabezado_Obtener(
            int codEmpresa,
            int operacion,
            int numPoliza)
            => _bl.CrPolizasRegistroBeneficiarios_Encabezado_Obtener(codEmpresa, operacion, numPoliza);

        [Authorize]
        [HttpGet("CrPolizasRegistroBeneficiarios_Beneficiarios_Obtener")]
        public ErrorDto<List<CrPolizasRegistroBeneficiariosListaData>> CrPolizasRegistroBeneficiarios_Beneficiarios_Obtener(
            int codEmpresa,
            int operacion,
            int numPoliza)
            => _bl.CrPolizasRegistroBeneficiarios_Beneficiarios_Obtener(codEmpresa, operacion, numPoliza);

        [Authorize]
        [HttpGet("CrPolizasRegistroBeneficiarios_Nuevo_Obtener")]
        public ErrorDto<CrPolizasRegistroBeneficiariosNuevoData?> CrPolizasRegistroBeneficiarios_Nuevo_Obtener(
            int codEmpresa,
            int operacion,
            int numPoliza)
            => _bl.CrPolizasRegistroBeneficiarios_Nuevo_Obtener(codEmpresa, operacion, numPoliza);

        [Authorize]
        [HttpGet("CrPolizasRegistroBeneficiarios_Detalle_Obtener")]
        public ErrorDto<CrPolizasRegistroBeneficiariosDetalleData?> CrPolizasRegistroBeneficiarios_Detalle_Obtener(
            int codEmpresa,
            int operacion,
            int numPoliza,
            string idBeneficiario)
            => _bl.CrPolizasRegistroBeneficiarios_Detalle_Obtener(codEmpresa, operacion, numPoliza, idBeneficiario);

        [Authorize]
        [HttpGet("CrPolizasRegistroBeneficiarios_Busqueda_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrPolizasRegistroBeneficiarios_Busqueda_Obtener(
            int codEmpresa,
            int operacion,
            int numPoliza)
            => _bl.CrPolizasRegistroBeneficiarios_Busqueda_Obtener(codEmpresa, operacion, numPoliza);

        [Authorize]
        [HttpPost("CrPolizasRegistroBeneficiarios_Guardar")]
        public ErrorDto<CrPolizasRegistroBeneficiariosGuardarData> CrPolizasRegistroBeneficiarios_Guardar(
            int codEmpresa,
            string usuario,
            [FromBody] CrPolizasRegistroBeneficiariosGuardarRequest request)
            => _bl.CrPolizasRegistroBeneficiarios_Guardar(codEmpresa, usuario, request);

        [Authorize]
        [HttpDelete("CrPolizasRegistroBeneficiarios_Eliminar")]
        public ErrorDto CrPolizasRegistroBeneficiarios_Eliminar(
            int codEmpresa,
            int operacion,
            int numPoliza,
            string idBeneficiario)
            => _bl.CrPolizasRegistroBeneficiarios_Eliminar(codEmpresa, operacion, numPoliza, idBeneficiario);
    }
}