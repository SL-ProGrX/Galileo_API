using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCprRechazoMotivosController : ControllerBase
    {
        private readonly FrmCprRechazoMotivosBL _bl;
        public FrmCprRechazoMotivosController(IConfiguration config)
        {
            _bl = new FrmCprRechazoMotivosBL(config);
        }

        [Authorize]
        [HttpGet("cprRechazoMotivoLista_Obtener")]
        public ErrorDto<CprRechazosMotivosLista> CprRechazoMotivoLista_Obtener(int CodCliente, string vFiltros)
        {
            return _bl.CprRechazoMotivoLista_Obtener(CodCliente, vFiltros);
        }

        [Authorize]
        [HttpPost("cprRechazoMotivo_Guardar")]
        public ErrorDto CprRechazoMotivo_Guardar(int CodCliente, CprRechazosMotivosDto motivo)
        {
            return _bl.CprRechazoMotivo_Guardar(CodCliente, motivo);
        }

        [Authorize]
        [HttpDelete("cprRechazoMotivo_Eliminar")]
        public ErrorDto CprRechazoMotivo_Eliminar(int CodCliente, string cod_rechazo)
        {
            return _bl.CprRechazoMotivo_Eliminar(CodCliente, cod_rechazo);
        }
    }
}