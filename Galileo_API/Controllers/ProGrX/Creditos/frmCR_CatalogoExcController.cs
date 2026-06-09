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
    public class FrmCrCatalogoExcController : ControllerBase
    {
        private readonly FrmCrCatalogoExcBL _bl;

        public FrmCrCatalogoExcController(IConfiguration config)
        {
            _bl = new FrmCrCatalogoExcBL(config);
        }

        [Authorize]
        [HttpGet("CrCatalogoExc_Disponible_Obtener")]
        public ErrorDto<List<CrCatalogoExcDisponibleModel>> CrCatalogoExc_Disponible_Obtener(int CodEmpresa)
        {
            return _bl.CrCatalogoExc_Disponible_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("CrCatalogoExc_Disponible_Guardar")]
        public ErrorDto CrCatalogoExc_Disponible_Guardar(int CodEmpresa, CrCatalogoExcDisponibleGuardarRequest request)
        {
            return _bl.CrCatalogoExc_Disponible_Guardar(CodEmpresa, request);
        }

        [Authorize]
        [HttpDelete("CrCatalogoExc_Disponible_Eliminar")]
        public ErrorDto CrCatalogoExc_Disponible_Eliminar(int CodEmpresa, CrCatalogoExcDisponibleEliminarRequest request)
        {
            return _bl.CrCatalogoExc_Disponible_Eliminar(CodEmpresa, request);
        }
    }
}
