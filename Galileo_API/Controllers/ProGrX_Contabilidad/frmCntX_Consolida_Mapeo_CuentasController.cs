using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmCntXConsolidaMapeoCuentasController : ControllerBase
    {
        private readonly FrmCntXConsolidaMapeoCuentasBL _bl;

        public FrmCntXConsolidaMapeoCuentasController(IConfiguration config)
        {
            _bl = new FrmCntXConsolidaMapeoCuentasBL(config);
        }

        [HttpGet("ConsolidaMapeoCuentas_ObtenerUnidades")]
        public ActionResult<ErrorDto<List<DropDownListaGenericaModel>>> ConsolidaMapeoCuentas_ObtenerUnidades(
            [FromQuery] int codEmpresa, 
            [FromQuery] int mContabilidad)
            => _bl.ConsolidaMapeoCuentas_ObtenerUnidades(codEmpresa, mContabilidad);
    }
}
