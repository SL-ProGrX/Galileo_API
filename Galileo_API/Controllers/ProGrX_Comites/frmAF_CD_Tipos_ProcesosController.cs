using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Comites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX_Comites.FrmAfCdTiposProcesos;

namespace Galileo_API.Controllers.ProGrX_Comites
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfCdTiposProcesosController : ControllerBase
    {

        private readonly FrmAfCdTiposProcesosBL _bl;

        public FrmAfCdTiposProcesosController(IConfiguration config)
            => _bl = new FrmAfCdTiposProcesosBL(config);


        [Authorize]
        [HttpGet("AfCdTiposProcesosLista_Obtener")]
        public ErrorDto<CdTiposProcesosLista> AfCdTiposProcesosLista_Obtener(int codEmpresa, string filtros, bool esExportar)
          => _bl.AfCdTiposProcesosLista_Obtener(codEmpresa, filtros, esExportar);

        [Authorize]
        [HttpPost("AfCdTiposProcesos_Guardar")]
        public ErrorDto AfCdTiposProcesos_Guardar(int codEmpresa, string usuario, [FromBody] CdTiposProcesosData datos)
                 => _bl.AfCdTiposProcesos_Guardar(codEmpresa, usuario, datos);

        [Authorize]
        [HttpDelete("AfCdTiposProcesos_Eliminar")]
        public ErrorDto AfCdTiposProcesos_Eliminar(int codEmpresa, string usuario, string codTipoCuenta)
                  => _bl.AfCdTiposProcesos_Eliminar(codEmpresa, usuario, codTipoCuenta);

    }
}
