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
    public class FrmAfCdPlanController : ControllerBase
    {
        private readonly FrmAfCdPlanBl _bl;

        public FrmAfCdPlanController(IConfiguration config)
        {
            _bl = new FrmAfCdPlanBl(config);
        }

        [HttpGet("AfCdComites_Lista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AfCdComites_Lista_Obtener(int codEmpresa)
        {
            return _bl.AfCdComites_Lista_Obtener(codEmpresa);
        }

        [HttpGet("AfCdPlanMensajes_Lista_Obtener")]
        public ErrorDto<List<AfCdPlanMensajeData>> AfCdPlanMensajes_Lista_Obtener(int codEmpresa, string codComite)
        {
            return _bl.AfCdPlanMensajes_Lista_Obtener(codEmpresa, codComite);
        }

        [HttpPost("AfCdPlanMensaje_Guardar")]
        public ErrorDto AfCdPlanMensaje_Guardar(int codEmpresa, AfCdPlanMensajeData request)
        {
            return _bl.AfCdPlanMensaje_Guardar(codEmpresa, request);
        }

        [HttpDelete("AfCdPlanMensajes_Eliminar")]
        public ErrorDto AfCdPlanMensajes_Eliminar(int codEmpresa, string codComite, int numMensaje)
        {
            return _bl.AfCdPlanMensajes_Eliminar(codEmpresa, codComite, numMensaje);
        }
    }
}