using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers.ProGrX.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfPadronSalariosController : ControllerBase
    {
        private readonly FrmAfPadronSalariosBL _bl;

        public FrmAfPadronSalariosController(IConfiguration config)
        {
            _bl = new FrmAfPadronSalariosBL(config);
        }

        [Authorize]
        [HttpGet("AF_PadronSalariosInstituciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_PadronSalariosInstituciones_Obtener(int CodEmpresa)
        {
            return _bl.AF_PadronSalariosInstituciones_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("AF_PadronSalarios_Padron_Procesar")]
        public ErrorDto AF_PadronSalarios_Padron_Procesar(int CodEmpresa, string institucion, string usuario, List<AfPadronData> padron)
        {
            return _bl.AF_PadronSalarios_Padron_Procesar(CodEmpresa, institucion, usuario, padron);
        }

        [Authorize]
        [HttpPost("AF_PadronSalarios_Salario_Procesar")]
        public ErrorDto AF_PadronSalarios_Salario_Procesar(int CodEmpresa, string institucion, string usuario, List<AfSalarioData> salario)
        {
            return _bl.AF_PadronSalarios_Salario_Procesar(CodEmpresa, institucion, usuario, salario);
        }

    }
}