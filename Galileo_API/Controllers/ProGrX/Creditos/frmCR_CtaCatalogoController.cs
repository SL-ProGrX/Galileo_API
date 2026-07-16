using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrCtaCatalogoController : ControllerBase
    {
        private readonly FrmCrCtaCatalogoBl _bl;

        public FrmCrCtaCatalogoController(IConfiguration config)
        {
            _bl = new FrmCrCtaCatalogoBl(config);
        }

        [Authorize]
        [HttpGet("CrCtaCatalogo_Cuentas_Obtener")]
        public ErrorDto<CrCtaCatalogoCuenta?> CrCtaCatalogo_Cuentas_Obtener(int codEmpresa, string codigo)
        {
            return _bl.CrCtaCatalogo_Cuentas_Obtener(codEmpresa, codigo);
        }

        [Authorize]
        [HttpPost("CrCtaCatalogo_Cuentas_Guardar")]
        public ErrorDto CrCtaCatalogo_Cuentas_Guardar(int codEmpresa, CrCtaCatalogoCuentasGuardarRequest request)
        {
            return _bl.CrCtaCatalogo_Cuentas_Guardar(codEmpresa, request);
        }
    }
}
