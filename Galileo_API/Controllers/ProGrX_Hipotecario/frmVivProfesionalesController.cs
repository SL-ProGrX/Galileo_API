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

        [HttpGet("VivContactos_EmpresaLista")]
        public ActionResult<ErrorDto<List<VivContactoEmpresaDto>>> VivContactos_EmpresaLista(
            [FromQuery] int codEmpresa,
            [FromQuery] int vCodigo)
            => _bl.VivContactos_EmpresaLista(codEmpresa, vCodigo);

        [HttpPost("CrdVivContacto_Add")]
        public ActionResult<ErrorDto<CrdVivContactoAddResult>> CrdVivContacto_Add(
            [FromQuery] int codEmpresa,
            [FromBody] CrdVivContactoAddParams param)
            => _bl.CrdVivContacto_Add(codEmpresa, param);

        [HttpDelete("VivContacto_Delete")]
        public ActionResult<ErrorDto<bool>> VivContacto_Delete(
            [FromQuery] int codEmpresa,
            [FromQuery] int idContacto)
            => _bl.VivContacto_Delete(codEmpresa, idContacto);

        [HttpGet("VivContactos_JuridicosLista")]
        public ActionResult<ErrorDto<List<VivContactoDto>>> VivContactos_JuridicosLista(
            [FromQuery] int codEmpresa,
            [FromQuery] int vCodigo)
            => _bl.VivContactos_JuridicosLista(codEmpresa, vCodigo);

        [HttpPut("VivContacto_SetEmpresa")]
        public ActionResult<ErrorDto<bool>> VivContacto_SetEmpresa(
            [FromQuery] int codEmpresa,
            [FromQuery] int vCodigo,
            [FromQuery] int? txtEmpresaId)
            => _bl.VivContacto_SetEmpresa(codEmpresa, vCodigo, txtEmpresaId);
    }
}
