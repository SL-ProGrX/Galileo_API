using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_EstudioCrd
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmPreaAcredoresController : ControllerBase
    {
        private readonly FrmPreaAcredoresBl _bl;

        public FrmPreaAcredoresController(IConfiguration config) =>
            _bl = new FrmPreaAcredoresBl(config);

        [HttpGet("PreaAcredores_ObtenerLista")]
        public ErrorDto<List<CrdPreaAcredoresData>> PreaAcredores_ObtenerLista(int codEmpresa)
        {
            return _bl.CrPreaAcredores_ObtenerLista(codEmpresa);
        }

        [HttpPost("PreaAcredores_Guardar")]
        public ErrorDto PreaAcredores_Guardar(int codEmpresa, string usuario, CrdPreaAcredoresData request)
        {
            return _bl.CrPreaAcredores_Guardar(codEmpresa, usuario, request);
        }

        [HttpDelete("PreaAcredores_Borrar")]
        public ErrorDto PreaAcredores_Borrar(int codEmpresa, string usuario, string codAcredor)
        {
            return _bl.CrPreaAcredores_Borrar(codEmpresa, usuario, codAcredor);
        }
    }
}
