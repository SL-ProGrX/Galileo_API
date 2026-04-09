using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.BusinessLogic.ProGrX_Comites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;  
using static Galileo_API.Models.ProGrX_Comites.FrmAfCdPreCalculo;

namespace Galileo_API.Controllers.ProGrX_Comites
{

    [Route("api/[controller]")]
    [ApiController]
   
        public class FrmAfCdPreCalculoController: ControllerBase
        {


        private readonly FrmAfCdPreCalculoBL _bl;

        public FrmAfCdPreCalculoController(IConfiguration config)
            => _bl = new FrmAfCdPreCalculoBL(config);

        [Authorize]
        [HttpGet("CrdPreCalculo_PantallaInicial_Obtener")]
        public ErrorDto<CrdPreCalculoPantallaInicialResponse> CrdPreCalculo_PantallaInicial_Obtener(int codEmpresa)
                => _bl.CrdPreCalculo_PantallaInicial_Obtener(codEmpresa);

        [Authorize]
        [HttpPost("CrdPreCalculo_Comite_Obtener")]
        public ErrorDto<CrdPreCalculoComiteResponse> CrdPreCalculo_Comite_Obtener(int codEmpresa, [FromBody] CrdPreCalculoComiteRequest request)
                  => _bl.CrdPreCalculo_Comite_Obtener(codEmpresa, request);

        [Authorize]
        [HttpPost("CrdPreCalculo_Grid_Obtener")]
        public ErrorDto<CrdPreCalculoGridResponse> CrdPreCalculo_Grid_Obtener(int codEmpresa, [FromBody] CrdPreCalculoGridRequest request)
               => _bl.CrdPreCalculo_Grid_Obtener(codEmpresa, request);

        [Authorize]
        [HttpGet("CrdPreCalculo_ComiteDesc_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrdPreCalculo_ComiteDesc_Obtener(int codEmpresa)
                 => _bl.CrdPreCalculo_ComiteDesc_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("CrdPreCalculo_ComiteId_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrdPreCalculo_ComiteId_Obtener(int codEmpresa)
                 => _bl.CrdPreCalculo_ComiteId_Obtener(codEmpresa);

    }
}
