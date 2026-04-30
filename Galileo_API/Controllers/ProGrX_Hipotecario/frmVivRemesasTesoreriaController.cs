using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX_Hipotecario
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmVivRemesasTesoreriaController : ControllerBase
    {
        private readonly FrmVivRemesasTesoreriaBL _bl;

        public FrmVivRemesasTesoreriaController(IConfiguration config)
        {
            _bl = new FrmVivRemesasTesoreriaBL(config);
        }

        [HttpGet("RemesasTesoreria_Obtener")]
        public ActionResult<ErrorDto<List<RemesasTesoreriaObtenerDto>>> RemesasTesoreria_Obtener([FromQuery] int codEmpresa)
            => _bl.RemesasTesoreria_Obtener(codEmpresa);

        [HttpPost("RemesasTesoreria_Insertar")]
        public ActionResult<ErrorDto<int>> RemesasTesoreria_Insertar(
            [FromQuery] int codEmpresa,
            [FromBody] RemesaTesoreriaUpsertDto dto)
            => _bl.RemesasTesoreria_Insertar(codEmpresa, dto);

        [HttpPut("RemesasTesoreria_Actualizar")]
        public ActionResult<ErrorDto<bool>> RemesasTesoreria_Actualizar(
            [FromQuery] int codEmpresa,
            [FromBody] RemesaTesoreriaUpsertDto dto)
            => _bl.RemesasTesoreria_Actualizar(codEmpresa, dto);

        [HttpDelete("RemesasTesoreriaDetalle_Eliminar")]
        public ActionResult<ErrorDto<bool>> RemesasTesoreriaDetalle_Eliminar(
            [FromQuery] int codEmpresa,
            [FromQuery] int remesa)
            => _bl.RemesasTesoreriaDetalle_Eliminar(codEmpresa, remesa);
    }
}
