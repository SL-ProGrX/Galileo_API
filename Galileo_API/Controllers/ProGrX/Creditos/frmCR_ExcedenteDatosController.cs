using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class FrmCrExcedenteDatosController : ControllerBase
    {
        private readonly FrmCrExcedenteDatosBl _bl;

        public FrmCrExcedenteDatosController(
            IConfiguration config)
        {
            _bl = new FrmCrExcedenteDatosBl(config);
        }


        [HttpGet("Cr_ExcedenteDatos_Obtener")]
        public ErrorDto<MCredito.CrExcedenteDisponibleData>
            Cr_ExcedenteDatos_Obtener(
                int codEmpresa,
                string cedula)
        {
            return _bl.Cr_ExcedenteDatos_Obtener(
                codEmpresa,
                cedula);
        }
    }
}