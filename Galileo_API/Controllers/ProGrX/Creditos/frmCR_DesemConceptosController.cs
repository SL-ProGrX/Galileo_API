using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrDesemConceptosController : ControllerBase
    {
        private readonly FrmCrDesemConceptosBl _bl;

        public FrmCrDesemConceptosController(IConfiguration config)
        {
            _bl = new FrmCrDesemConceptosBl(config);
        }

        [HttpGet("CrDesembConceptos_Obtener")]
        public ErrorDto<List<CrConceptoDesembData>> CrDesembConceptos_Obtener(int codEmpresa)
        {
            return _bl.CrDesembConceptos_Obtener(codEmpresa);
        }

        [HttpPost("CrDesembConcepto_Guardar")]
        public ErrorDto CrDesembConcepto_Guardar(int codEmpresa, string usuario, int codConta, CrConceptoDesembData request)
        {
            return _bl.CrDesembConcepto_Guardar(codEmpresa, usuario, codConta, request);
        }

        [HttpDelete("CrDesembConcepto_Eliminar")]
        public ErrorDto CrDesembConcepto_Eliminar(int codEmpresa, int codCondeb, string usuario)
        {
            return _bl.CrDesembConcepto_Eliminar(codEmpresa, codCondeb, usuario);
        }
    }
}
