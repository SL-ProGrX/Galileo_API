using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.BusinessLogic.ProGrX_Comites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static Galileo_API.Models.ProGrX_Comites.FrmAfCdTiposActividades; 

namespace Galileo_API.Controllers.ProGrX_Comites
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfCdTiposActividadesController : ControllerBase
    {

        private readonly FrmAfCdTiposActividadesBL _bl;

        public FrmAfCdTiposActividadesController(IConfiguration config)
            => _bl = new FrmAfCdTiposActividadesBL(config);


        [Authorize]
        [HttpGet("AfCdTiposActividadesLista_Obtener")]
        public ErrorDto<CDTiposActividadesLista> AfCdTiposActividadesLista_Obtener(int codEmpresa, string filtros, bool esExportar)
          => _bl.AfCdTiposActividadesLista_Obtener(codEmpresa, filtros, esExportar);

        [Authorize]
        [HttpPost("AfCdTiposActividades_Guardar")]
        public ErrorDto AfCdTiposActividades_Guardar(int codEmpresa, string usuario, [FromBody] CDTiposActividadesData datos)
                 => _bl.AfCdTiposActividades_Guardar(codEmpresa, usuario, datos);

        [Authorize]
        [HttpDelete("AfCdTiposActividades_Eliminar")]
        public ErrorDto AfCdTiposActividades_Eliminar(int codEmpresa, string usuario, string codTipoActividad)
                  => _bl.AfCdTiposActividades_Eliminar(codEmpresa, usuario, codTipoActividad);

    }
}
