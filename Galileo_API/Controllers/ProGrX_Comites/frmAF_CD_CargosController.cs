using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Comites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX_Comites.FrmAfCdCargos;

namespace Galileo_API.Controllers.ProGrX_Comites
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfCdCargosController : ControllerBase
    {

        private readonly FrmAfCdCargosBL _bl;

        public FrmAfCdCargosController(IConfiguration config)
            => _bl = new FrmAfCdCargosBL(config);


        [Authorize]
        [HttpGet("CdCargosLista_Obtener")]
        public ErrorDto<CdCargosLista> CdCargosLista_Obtener(int codEmpresa, string filtros, bool esExportar)
          => _bl.CdCargosLista_Obtener(codEmpresa, filtros, esExportar);

        [Authorize]
        [HttpPost("AfCdCargos_Guardar")]
        public ErrorDto AfCdTiposProcesos_Guardar(int codEmpresa, string usuario, [FromBody] CdCargosData datos)
                 => _bl.AfCdCargos_Guardar(codEmpresa, usuario, datos);

        [Authorize]
        [HttpDelete("AfCdCargos_Eliminar")]
        public ErrorDto AfCdTiposProcesAfCdCargos_Eliminaros_Eliminar(int codEmpresa, string usuario, string codCargo)
                  => _bl.AfCdCargos_Eliminar(codEmpresa, usuario, codCargo);

    }
}
