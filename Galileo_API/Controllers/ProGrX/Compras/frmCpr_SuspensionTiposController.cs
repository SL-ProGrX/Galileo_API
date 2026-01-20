using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmCprSuspensionTiposController : ControllerBase
    {
        private readonly FrmCprSuspensionTiposBL _bl;

        public FrmCprSuspensionTiposController(IConfiguration config)
        {
            _bl = new FrmCprSuspensionTiposBL(config);
        }

        [HttpGet("TiposSuspension_ObtenerTodos")]
        public ErrorDto<TiposSuspensionDtoList> TiposSuspension_ObtenerTodos(int CodEmpresa, int? pagina, int? paginacion, string? filtro)
        {
            return _bl.TiposSuspension_ObtenerTodos(CodEmpresa, pagina, paginacion, filtro);
        }

        [HttpPost("TiposSuspension_Guardar")]
        public ErrorDto TiposSuspension_Guardar(int CodEmpresa, TiposSuspensionDto tiposSuspensionDto)
        {
            return _bl.TiposSuspension_Guardar(CodEmpresa, tiposSuspensionDto);
        }

        [HttpDelete("TiposSuspension_Eliminar")]
        public ErrorDto TiposSuspension_Eliminar(int CodEmpresa, string codSuspension)
        {
            return _bl.TiposSuspension_Eliminar(CodEmpresa, codSuspension);
        }
    }
}
