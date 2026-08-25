using Galileo.Models.ERROR;
using Galileo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo_API.BusinessLogic.ProGrX_Procesos;

namespace Galileo_API.Controllers.ProGrX_Procesos
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmCcFndSolidarioController : ControllerBase
    {
        private readonly FrmCcFndSolidarioBL _bl;

        public FrmCcFndSolidarioController(IConfiguration config)
            => _bl = new FrmCcFndSolidarioBL(config);

        [Authorize]
        [HttpGet("FNDSolidario_Instituciones_Obtener")]       

        public ErrorDto<List<DropDownListaGenericaModel>> FNDSolidario_Instituciones_Obtener(int codEmpresa)
                => _bl.FNDSolidario_Instituciones_Obtener(codEmpresa);

        [Authorize]
        [HttpPost("FrmCC_FNDSolidario_Ejecutar")]
        public ErrorDto FrmCC_FNDSolidario_Ejecutar(int codEmpresa, string usuario, int codContabilidad, int codInstitucion)
                     => _bl.FrmCC_FNDSolidario_Ejecutar(codEmpresa, usuario, codContabilidad, codInstitucion);


    }
}
