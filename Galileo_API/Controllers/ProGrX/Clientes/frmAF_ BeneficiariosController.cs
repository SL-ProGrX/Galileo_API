using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class frmAF_BeneficiariosController : ControllerBase
    {
        FrmAFBeneficiariosBL _bl;
        public frmAF_BeneficiariosController(IConfiguration config)
        {
            _bl = new FrmAFBeneficiariosBL(config);
        }

        [Authorize]
        [HttpGet("AF_PersonaBeneficiarios_Consulta")]
        public ErrorDto<List<PersonaBeneficiarioDto>> AF_PersonaBeneficiarios_Consulta(int CodEmpresa, string cedula, int? lineaId)
        {
            return _bl.AF_PersonaBeneficiarios_Consulta(CodEmpresa, cedula, lineaId);
        }

        [Authorize]
        [HttpPost("AF_PersonaBeneficiarios_Registro")]
        public ErrorDto<int> AF_PersonaBeneficiarios_Registro(int CodEmpresa, PersonaBeneficiarioDto dto)
        {
            return _bl.AF_PersonaBeneficiarios_Registro(CodEmpresa, dto);
        }

        [Authorize]
        [HttpGet("AF_Beneficiarios_Catalogos_Obtener")]
        public ErrorDto<BeneficiariosCatalogoDto> AF_Beneficiarios_Catalogos_Obtener(int CodEmpresa)
        {
            return _bl.AF_Beneficiarios_Catalogos_Obtener(CodEmpresa);
        }
    }
}