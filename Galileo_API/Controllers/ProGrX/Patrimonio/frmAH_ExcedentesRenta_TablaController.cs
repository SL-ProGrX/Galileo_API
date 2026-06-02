using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Patrimonio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Patrimonio
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmAhExcedentesRentaTablaController : ControllerBase
    {
        private readonly FrmAhExcedentesRentaTablaBL _bl;

        public FrmAhExcedentesRentaTablaController(IConfiguration config)
        {
            _bl = new FrmAhExcedentesRentaTablaBL(config);
        }

        [HttpGet("AH_ExcedentesRentaTabla_Obtener")]
        public ErrorDto<List<RentaExcedenteDto>> AH_ExcedentesRentaTabla_Obtener(
            [FromQuery] int codEmpresa)
        {
            return _bl.AH_ExcedentesRentaTabla_Obtener(codEmpresa);
        }

        [HttpPost("AH_ExcedentesRentaTabla_Guardar")]
        public ErrorDto AH_ExcedentesRentaTabla_Guardar(
            [FromQuery] int codEmpresa,
            [FromQuery] string usuario,
            [FromBody] RentaExcedenteDto request)
        {
            return _bl.AH_ExcedentesRentaTabla_Guardar(codEmpresa, usuario, request);
        }

        [HttpDelete("AH_ExcedentesRentaTabla_Eliminar")]
        public ErrorDto AH_ExcedentesRentaTabla_Eliminar(
            [FromQuery] int codEmpresa,
            [FromQuery] int idRenta,
            [FromQuery] string usuario)
        {
            return _bl.AH_ExcedentesRentaTabla_Eliminar(codEmpresa, idRenta, usuario);
        }
    }
}
