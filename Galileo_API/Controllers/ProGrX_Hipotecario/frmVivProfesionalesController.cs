using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Galileo_API.Controllers.ProGrX_Hipotecario
{
    [ApiController]
    [Route("api/[controller]")]
    public class FrmVivProfesionalesController : ControllerBase
    {
        private readonly FrmVivProfesionalesBL _bl;

        public FrmVivProfesionalesController(IConfiguration config)
        {
            _bl = new FrmVivProfesionalesBL(config);
        }

        [HttpPost("VivContactos_Lista")]
        public ActionResult<ErrorDto<List<VivContactoDto>>> VivContactos_Lista(
            [FromQuery] int codEmpresa,
            [FromBody] VivContactoFiltroParams filtro)
            => _bl.VivContactos_Lista(codEmpresa, filtro);

        [HttpGet("VivTiposId_Lista")]
        public ActionResult<ErrorDto<List<DropDownListaGenericaModel>>> VivTiposId_Lista([FromQuery] int codEmpresa)
            => _bl.VivTiposId_Lista(codEmpresa);

        [HttpPost("CrdSgtBancos_Lista")]
        public ActionResult<ErrorDto<List<CrdSgtBancoDto>>> CrdSgtBancos_Lista(
            [FromQuery] int codEmpresa,
            [FromBody] CrdSgtBancoParams param)
            => _bl.CrdSgtBancos_Lista(codEmpresa, param);

        [HttpGet("VivCuentasBancarias_Lista")]
        public ActionResult<ErrorDto<List<VivCuentaBancariaDto>>> VivCuentasBancarias_Lista(
            [FromQuery] int codEmpresa,
            [FromQuery] string identificacion)
            => _bl.VivCuentasBancarias_Lista(codEmpresa, identificacion);

        [HttpGet("CrdVivContacto_Consulta")]
        public ActionResult<ErrorDto<CrdVivContactoConsultaDto>> CrdVivContacto_Consulta(
            [FromQuery] int codEmpresa,
            [FromQuery] int idContacto)
            => _bl.CrdVivContacto_Consulta(codEmpresa, idContacto);
    }
}
