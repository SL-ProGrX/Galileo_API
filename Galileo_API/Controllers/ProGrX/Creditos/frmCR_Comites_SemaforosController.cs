using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Creditos;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCrComitesSemaforoController : ControllerBase
    {
        private readonly FrmCrComitesSemaforoBL BL;

        public FrmCrComitesSemaforoController(IConfiguration config)
        {
            BL = new FrmCrComitesSemaforoBL(config);
        }

        [Authorize]
        [HttpGet("CR_ComitesSemaforo_Comites_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ComitesSemaforo_Comites_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CR_ComitesSemaforo_Comites_Dropdown_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CR_ComitesSemaforo_Obtener")]
        public ErrorDto<CrComitesSemaforoData> CR_ComitesSemaforo_Obtener(int CodEmpresa, int idComite)
        {
            return BL.CR_ComitesSemaforo_Obtener(CodEmpresa, idComite);
        }

        [Authorize]
        [HttpPost("CR_ComitesSemaforo_Guardar")]
        public ErrorDto CR_ComitesSemaforo_Guardar(int CodEmpresa, [FromBody] CrComitesSemaforoGuardarRequest request)
        {
            return BL.CR_ComitesSemaforo_Guardar(CodEmpresa, request);
        }

        [Authorize]
        [HttpGet("CR_ComitesSemaforo_Email_Lista_Obtener")]
        public ErrorDto<CrComitesSemaforoEmailLista> CR_ComitesSemaforo_Email_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return BL.CR_ComitesSemaforo_Email_Lista_Obtener(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpGet("CR_ComitesSemaforo_Email_Lista_Export")]
        public ErrorDto<CrComitesSemaforoEmailLista> CR_ComitesSemaforo_Email_Lista_Export(int CodEmpresa, string parametros)
        {
            return BL.CR_ComitesSemaforo_Email_Lista_Export(CodEmpresa, parametros);
        }

        [Authorize]
        [HttpPost("CR_ComitesSemaforo_Email_Agregar")]
        public ErrorDto CR_ComitesSemaforo_Email_Agregar(int CodEmpresa, [FromBody] CrComitesSemaforoEmailAgregarRequest request)
        {
            return BL.CR_ComitesSemaforo_Email_Agregar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("CR_ComitesSemaforo_Email_Eliminar")]
        public ErrorDto CR_ComitesSemaforo_Email_Eliminar(int CodEmpresa, [FromBody] CrComitesSemaforoEmailEliminarRequest request)
        {
            return BL.CR_ComitesSemaforo_Email_Eliminar(CodEmpresa, request);
        }
    }
}