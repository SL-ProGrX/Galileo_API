using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_EstudioCrd
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmPreaTablaPagosController : ControllerBase
    {
        private readonly FrmPreaTablaPagosBl _bl;

        public FrmPreaTablaPagosController(IConfiguration config) =>
            _bl = new FrmPreaTablaPagosBl(config);

        [HttpGet("CrPreaTablaPagos_ObtenerInstituciones")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrPreaTablaPagos_ObtenerInstituciones(int codEmpresa)
        {
            return _bl.CrPreaTablaPagos_ObtenerInstituciones(codEmpresa);
        }

        [HttpGet("CrPreaTablaPagos_Obtener")]
        public ErrorDto<List<CrdPreaTablaPagosData>> CrPreaTablaPagos_Obtener(int codEmpresa, int codInstitucion)
        {
            return _bl.CrPreaTablaPagos_Obtener(codEmpresa, codInstitucion);
        }

        [HttpPost("CrPreaTablaPagos_Guardar")]
        public ErrorDto CrPreaTablaPagos_Guardar(int codEmpresa, string usuario, CrdPreaTablaPagosData request)
        {
            return _bl.CrPreaTablaPagos_Guardar(codEmpresa, usuario, request);
        }

        [HttpDelete("CrPreaTablaPagos_Eliminar")]
        public ErrorDto CrPreaTablaPagos_Eliminar(int codEmpresa, int idx, string usuario)
        {
            return _bl.CrPreaTablaPagos_Eliminar(codEmpresa, idx, usuario);
        }
    }
}
