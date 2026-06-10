namespace Galileo_API.Controllers.ProGrX.Creditos
{
    using Galileo.Models.ERROR;
    using Galileo_API.BusinessLogic.ProGrX.Creditos;
    using Galileo_API.Models.ProGrX.Creditos;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrCatalogoErroresAnalistasController : ControllerBase
    {
        private readonly FrmCrCatalogoErroresAnalistasBL _bl;

        public FrmCrCatalogoErroresAnalistasController(IConfiguration config)
        {
            _bl = new FrmCrCatalogoErroresAnalistasBL(config);
        }

        [Authorize]
        [HttpGet("CrCatalogoErroresAnalistas_Obtener")]
        public ErrorDto<List<CrCatalogoErroresAnalistasModel>> CrCatalogoErroresAnalistas_Obtener(int CodEmpresa)
        {
            return _bl.CrCatalogoErroresAnalistas_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("CrCatalogoErroresAnalistas_Guardar")]
        public ErrorDto CrCatalogoErroresAnalistas_Guardar(int CodEmpresa, CrCatalogoErroresAnalistasGuardarRequest request)
        {
            return _bl.CrCatalogoErroresAnalistas_Guardar(CodEmpresa, request);
        }

        [Authorize]
        [HttpDelete("CrCatalogoErroresAnalistas_Eliminar")]
        public ErrorDto CrCatalogoErroresAnalistas_Eliminar(int CodEmpresa, CrCatalogoErroresAnalistasEliminarRequest request)
        {
            return _bl.CrCatalogoErroresAnalistas_Eliminar(CodEmpresa, request);
        }
    }
}
