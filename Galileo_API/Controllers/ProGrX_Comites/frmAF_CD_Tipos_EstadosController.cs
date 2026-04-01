using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.BusinessLogic.ProGrX_Comites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX_Comites.FrmAfCdTiposEstados;

namespace Galileo_API.Controllers.ProGrX_Comites
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfCdTiposEstadosController : ControllerBase
    {

        private readonly FrmAfCdTiposEstadosBL _bl;

        public FrmAfCdTiposEstadosController(IConfiguration config)
            => _bl = new FrmAfCdTiposEstadosBL(config);


        [Authorize]
        [HttpGet("AfCdTiposEstadosLista_Obtener")]
        public ErrorDto<CdTiposEstadosLista> AfCdTiposEstadosLista_Obtener(int codEmpresa, string filtros, bool esExportar)
          => _bl.AfCdTiposEstadosLista_Obtener(codEmpresa, filtros, esExportar);

        [Authorize]
        [HttpPost("AfCdTiposEstados_Guardar")]
        public ErrorDto AfCdTiposEstados_Guardar(int codEmpresa, string usuario, [FromBody] CdTiposEstadosData datos)
                 => _bl.AfCdTiposEstados_Guardar(codEmpresa, usuario, datos);

        [Authorize]
        [HttpDelete("AfCdTiposEstados_Eliminar")]
        public ErrorDto AfCdTiposEstados_Eliminar(int codEmpresa, string usuario, string codTipoCuenta)
                  => _bl.AfCdTiposEstados_Eliminar(codEmpresa, usuario, codTipoCuenta);

    }
}
