using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmInvEtiquetasController : ControllerBase
    {
        private readonly FrmInvEtiquetasBL _bl;
        public FrmInvEtiquetasController(IConfiguration config)
        {
            _bl = new FrmInvEtiquetasBL(config);
        }

        [HttpPost("GenerateSato")]
        public ErrorDto<List<ProductData>> GenerateSato(int CodEmpresa, GenerateSatoRequest request)
        {
            return _bl.GenerateSato(CodEmpresa, request);
        }
    }
}