using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX_Polizas.FrmCRPolizasRegistroBeneficiariosModels;

namespace Galileo_API.Controllers.ProGrX_Polizas
{

    [Route("api/[controller]")]
    [ApiController]

    public class FrmCRPolizasRegistroBeneficiariosController
    {

        private readonly FrmCRPolizasRegistroBeneficiariosBL _bl;
        public FrmCRPolizasRegistroBeneficiariosController(IConfiguration config)
        {
            _bl = new FrmCRPolizasRegistroBeneficiariosBL(config);
        }

        [Authorize]
        [HttpGet("CRPolizasRegistroBeneficiarios_Parentescos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CRPolizasRegistroBeneficiarios_Parentescos_Obtener(int codEmpresa)
                => _bl.CRPolizasRegistroBeneficiarios_Parentescos_Obtener(codEmpresa);


        [Authorize]
        [HttpGet("CRPolizasRegistroBeneficiarios_Encabezado_Obtener")]
        public ErrorDto<CRPolizasRegistroBeneficiariosEncabezadoResponse> CRPolizasRegistroBeneficiarios_Encabezado_Obtener(
           int codEmpresa,
           int IdSolicitud,
           int NumPoliza)
            => _bl.CRPolizasRegistroBeneficiarios_Encabezado_Obtener(codEmpresa, IdSolicitud, NumPoliza);

        [Authorize]
        [HttpGet("CRPolizasRegistroBeneficiarios_Beneficiarios_Obtener")]
        public ErrorDto<List<CRPolizasRegistroBeneficiariosListaItem>> CRPolizasRegistroBeneficiarios_Beneficiarios_Obtener(
              int codEmpresa,
             int IdSolicitud,
              int NumPoliza)
           => _bl.CRPolizasRegistroBeneficiarios_Beneficiarios_Obtener(codEmpresa, IdSolicitud, NumPoliza);

        [Authorize]
        [HttpGet("CRPolizasRegistroBeneficiarios_Nuevo_Obtener")]
        public ErrorDto<CRPolizasRegistroBeneficiariosNuevoResponse> CRPolizasRegistroBeneficiarios_Nuevo_Obtener(
           int codEmpresa,
           int IdSolicitud,
           int NumPoliza)
       => _bl.CRPolizasRegistroBeneficiarios_Nuevo_Obtener(codEmpresa, IdSolicitud, NumPoliza);


        [Authorize]
        [HttpGet("CRPolizasRegistroBeneficiarios_Detalle_Obtener")]
        public ErrorDto<CRPolizasRegistroBeneficiarios> CRPolizasRegistroBeneficiarios_Detalle_Obtener(
              int codEmpresa,
              int IdSolicitud,
              int NumPoliza,
              string IdBeneficiario)
          => _bl.CRPolizasRegistroBeneficiarios_Detalle_Obtener(codEmpresa, IdSolicitud, NumPoliza, IdBeneficiario);

        [Authorize]
        [HttpPost("CRPolizasRegistroBeneficiarios_Guardar")]
        public ErrorDto<CRPolizasRegistroBeneficiariosGuardarResponse> CRPolizasRegistroBeneficiarios_Guardar(
            int codEmpresa,
            string usuario,
            [FromBody] CRPolizasRegistroBeneficiarios request)
         => _bl.CRPolizasRegistroBeneficiarios_Guardar(codEmpresa, usuario, request);

        [Authorize]
        [HttpDelete("CRPolizasRegistroBeneficiarios_Eliminar")]
        public ErrorDto CRPolizasRegistroBeneficiarios_Eliminar(
             int codEmpresa,
             int IdSolicitud,
             int NumPoliza,
             string IdBeneficiario)
   => _bl.CRPolizasRegistroBeneficiarios_Eliminar(codEmpresa, IdSolicitud, NumPoliza, IdBeneficiario);
    }
}
