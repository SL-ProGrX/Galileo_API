using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Activos;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmArfMonitorController : ControllerBase
    {
        private readonly FrmArfMonitorBl _bl;

        public FrmArfMonitorController(IConfiguration config)
        {
            _bl = new FrmArfMonitorBl(config);
        }

        [Authorize]
        [HttpPost("Buscar")]
        public ErrorDto<List<ARFMonitorTablaDto>> Buscar(int codEmpresa,[FromBody] ARFMonitorFiltroDto filtros
        )
        {
            return _bl.Buscar(codEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("Unidades_Buscar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Unidades_Buscar(int codEmpresa
        )
        {
            return _bl.Unidades_Buscar(codEmpresa);
        }

        [Authorize]
        [HttpGet("Arrendadores_Buscar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Arrendadores_Buscar(int codEmpresa
        )
        {
            return _bl.Arrendadores_Buscar(codEmpresa);
        }


    }
}
