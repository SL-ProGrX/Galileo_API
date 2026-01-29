
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.CuentasxCobrar
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCxCCargosTiposController : ControllerBase
    {
        private readonly FrmCxCCargosTiposBL _bl;

        public FrmCxCCargosTiposController(IConfiguration config)
            => _bl = new FrmCxCCargosTiposBL(config);

        [Authorize]
        [HttpGet("CxCCargosTiposLista_Obtener")]
        public ErrorDto<CxCCargosTiposLista> CxCCargosTiposLista_Obtener(int CodEmpresa, string filtros, bool esExportar)
        {
            return _bl.CxCCargosTiposLista_Obtener(CodEmpresa, filtros, esExportar);
        }

        [Authorize]
        [HttpPost("CxCCargosTipos_Guardar")]
        public ErrorDto CxCCargosTipos_Guardar(int CodEmpresa, string usuario, [FromBody] CxCCargosTiposData datos)
        {
            return _bl.CxCCargosTipos_Guardar(CodEmpresa, usuario, datos);
        }

        [Authorize]
        [HttpDelete("CxCCargosTipos_Eliminar")]
        public ErrorDto CxCCargosTipos_Eliminar(int CodEmpresa, string usuario, string CodCargo)
        {
            return _bl.CxCCargosTipos_Eliminar(CodEmpresa, usuario, CodCargo);
        }
    }
}