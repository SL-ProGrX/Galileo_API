namespace Galileo_API.Controllers.ProGrX_Hipotecario
{
    using Galileo.Models.ERROR;
    using Galileo_API.BusinessLogic.ProGrX_Hipotecario;
    using Galileo_API.Models.ProGrX_Hipotecario;
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;

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
    }
}
