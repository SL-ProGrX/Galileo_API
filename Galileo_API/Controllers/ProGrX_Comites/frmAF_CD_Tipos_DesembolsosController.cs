using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.BusinessLogic.ProGrX_Comites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX_Comites.FrmAfCdTiposDesembolsos;

namespace Galileo_API.Controllers.ProGrX_Comites
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfCdTiposDesembolsosController : ControllerBase
    {

        private readonly FrmAfCdTiposDesembolsosBL _bl;

        public FrmAfCdTiposDesembolsosController(IConfiguration config)
            => _bl = new FrmAfCdTiposDesembolsosBL(config);


        [Authorize]
        [HttpGet("AfCdTiposDesembolsosLista_Obtener")]
        public ErrorDto<CdTiposDesembolsosLista> AfCdTiposDesembolsosLista_Obtener(int codEmpresa, string filtros, bool esExportar)
          => _bl.AfCdTiposDesembolsosLista_Obtener(codEmpresa, filtros, esExportar);

        [Authorize]
        [HttpPost("AfCdTiposDesembolsos_Guardar")]
        public ErrorDto AfCdTiposDesembolsos_Guardar(int codEmpresa, string usuario, [FromBody] CdTiposDesembolsosData datos)
                 => _bl.AfCdTiposDesembolsos_Guardar(codEmpresa, usuario, datos);

        [Authorize]
        [HttpDelete("AfCdTiposDesembolsos_Eliminar")]
        public ErrorDto AfCdTiposDesembolsos_Eliminar(int codEmpresa, string usuario, string codTipoCuenta)
                  => _bl.AfCdTiposDesembolsos_Eliminar(codEmpresa, usuario, codTipoCuenta);

    }
}
