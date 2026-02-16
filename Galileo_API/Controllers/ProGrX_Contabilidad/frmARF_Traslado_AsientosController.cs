using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Arrendamientos;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Activos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Arrendamientos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmArfTrasladoAsientosController : ControllerBase
    {
        private readonly FrmArfTrasladoAsientosBl _bl;

        public FrmArfTrasladoAsientosController(IConfiguration config)
        {
            _bl = new FrmArfTrasladoAsientosBl(config);
        }

        [Authorize]
        [HttpPost("Buscar")]
        public ErrorDto<List<ArfTrasladoTablaDto>> Buscar(int codEmpresa,[FromBody] ArfTrasladoFiltroDto filtros)
        {
            return _bl.Buscar(codEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("Trasladar")]
        public ErrorDto<bool> Trasladar(int codEmpresa, [FromBody] List<ArfTrasladoRequestDto> asientos)
        {
            return _bl.Trasladar(codEmpresa, asientos);
        }
    }
}
