using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Credito
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrCatalogoGarantiasController : ControllerBase
    {
        private readonly FrmCrCatalogoGarantiasBl _bl;

        public FrmCrCatalogoGarantiasController(IConfiguration config)
        {
            _bl = new FrmCrCatalogoGarantiasBl(config);
        }

        [HttpGet("CrGarantiaTipos_Obtener")]
        public ErrorDto<List<CrGarantiaTiposData>> CrGarantiaTipos_Obtener(int codEmpresa)
        {
            return _bl.CrGarantiaTipos_Obtener(codEmpresa);
        }
        
        [HttpPost("CrGarantiaTipos_Guardar")]
        public ErrorDto CrGarantiaTipos_Guardar(int codEmpresa, string usuario, CrGarantiaTiposData request)
        {
            return _bl.CrGarantiaTipos_Guardar(codEmpresa, usuario, request);
        }

        [HttpDelete("CrGarantiaTipos_Eliminar")]
        public ErrorDto CrGarantiaTipos_Eliminar(int codEmpresa, string garantia, string usuario)
        {
            return _bl.CrGarantiaTipos_Eliminar(codEmpresa, garantia, usuario);
        }
    }
}
