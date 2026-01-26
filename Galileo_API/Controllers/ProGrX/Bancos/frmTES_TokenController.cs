using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Galileo_API.BusinessLogic.TES;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmTesTokenController : ControllerBase
    {
        private readonly  FrmTesTokenBL TokenBL;

        public FrmTesTokenController(IConfiguration config)
        {
            TokenBL = new FrmTesTokenBL(config);
        }

        [Authorize]
        [HttpGet("TES_Token_Top_Obtener")]
        public ErrorDto<List<TesTokenDto>> TES_Token_Top_Obtener(int CodEmpresa)
        {
            return TokenBL.TES_Token_Top_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPut("TES_Token_Cerrar")]
        public ErrorDto TES_Token_Cerrar(int CodEmpresa, string Id)
        {
            return TokenBL.TES_Token_Cerrar(CodEmpresa, Id);
        }

        [Authorize]
        [HttpGet("TES_Token_Pen_Obtener")]
        public ErrorDto<List<TesTokenSolicitudesData>> TES_Token_Pen_Obtener(int CodEmpresa)
        {
            return TokenBL.TES_Token_Pen_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("TES_Token_Pen_Incluir")]
        public ErrorDto TES_Token_Pen_Incluir(int CodEmpresa, string token, List<string> solicitudes)
        {
            return TokenBL.TES_Token_Pen_Incluir(CodEmpresa, token, solicitudes);
        }

        [Authorize]
        [HttpPost("TES_Token_Crear")]
        public ErrorDto TES_Token_Crear(int CodEmpresa, string Usuario)
        {
            return TokenBL.TES_Token_Crear(CodEmpresa, Usuario);
        }
    }
}