using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models.ERROR;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndActualizacionSifController : ControllerBase
    {
        private readonly FrmFndActualizacionSifBl _bl;

        public FrmFndActualizacionSifController(IConfiguration config)
        {
            _bl = new FrmFndActualizacionSifBl(config);
        }

        [Authorize]
        [HttpPost("Fnd_ActualizacionSif_Aplicar")]
        public ErrorDto Fnd_ActualizacionSif_Aplicar(int CodEmpresa, string Usuario)
        {
            return _bl.Fnd_ActualizacionSif_Aplicar(CodEmpresa, Usuario);
        }
    }
}