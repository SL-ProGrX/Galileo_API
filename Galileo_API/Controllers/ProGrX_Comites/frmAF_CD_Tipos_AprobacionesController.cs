using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.BusinessLogic.ProGrX_Comites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX_Comites.FrmAfCdTiposAprobaciones;

namespace Galileo_API.Controllers.ProGrX_Comites
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfCdTiposAprobacionesController : ControllerBase
    {

        private readonly FrmAfCdTiposAprobacionesBL _bl;

        public FrmAfCdTiposAprobacionesController(IConfiguration config)
            => _bl = new FrmAfCdTiposAprobacionesBL(config);


        [Authorize]
        [HttpGet("AfCdTiposAprobacionesLista_Obtener")]
        public ErrorDto<CdTiposAprobacionesLista> AfCdTiposAprobacionesLista_Obtener(int codEmpresa, string filtros, bool esExportar)
          => _bl.AfCdTiposAprobacionesLista_Obtener(codEmpresa, filtros, esExportar);

        [Authorize]
        [HttpPost("AfCdTiposAprobaciones_Guardar")]
        public ErrorDto AfCdTiposAprobaciones_Guardar(int codEmpresa, string usuario, [FromBody] CdTiposAprobacionesData datos)
                 => _bl.AfCdTiposAprobaciones_Guardar(codEmpresa, usuario, datos);

        [Authorize]
        [HttpDelete("AfCdTiposAprobaciones_Eliminar")]
        public ErrorDto AfCdTiposAprobaciones_Eliminar(int codEmpresa, string usuario, string codTipoAprobacion)
                  => _bl.AfCdTiposAprobaciones_Eliminar(codEmpresa, usuario, codTipoAprobacion);

    }
}
