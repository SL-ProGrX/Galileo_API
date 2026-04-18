using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Comites;
using Galileo_API.Models.ProGrX_Comites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Comites
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmAfCdAprobacionesController : ControllerBase
    {
        private readonly FrmAfCdAprobacionesBl _bl;

        public FrmAfCdAprobacionesController(IConfiguration config)
        {
            _bl = new FrmAfCdAprobacionesBl(config);
        }

        [HttpGet("Listar")]
        public ErrorDto<List<AfcdAprobacionDto>> Listar(int codEmpresa, int banco)
        {
            return _bl.Listar(codEmpresa, banco);
        }

        [HttpGet("Bancos")]
        public ErrorDto<List<DropDownListaGenericaModel>> Bancos(int codEmpresa)
        {
            return _bl.Bancos(codEmpresa);
        }

        [HttpPost("Aprobar")]
        public ErrorDto<bool> Aprobar(AfcdAprobacionRequest req)
        {
            return _bl.Aprobar(req);
        }

        [HttpPost("Rechazar")]
        public ErrorDto<bool> Rechazar(AfcdRechazoRequest req)
        {
            return _bl.Rechazar(req);
        }

        [HttpGet("OficinaUsuario")]
        public ErrorDto<OficinaUsuarioAprobacionDto> Oficina_ObtenerPorUsuario(int codEmpresa,string usuario)
        {
            return _bl.Oficina_ObtenerPorUsuario(codEmpresa, usuario);
        }
    }
}