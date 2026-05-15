using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndAutorizacionMovController : ControllerBase
    {
       private readonly FrmFndAutorizacionMovBL _bl;

       public FrmFndAutorizacionMovController(IConfiguration config)
       {
           _bl = new FrmFndAutorizacionMovBL(config);
       }

        [Authorize]
        [HttpGet("Fnd_Autorizacion_Mov_Obtener")]
        public ErrorDto<TablasListaGenericaModel> Fnd_Autorizacion_Mov_Obtener(int CodEmpresa, bool exporta, string data, string filtros)
        {
            return _bl.Fnd_Autorizacion_Mov_Obtener(CodEmpresa, exporta, data, filtros);
        }

        [Authorize]
        [HttpPost("Fnd_Autorizacion_Mov_Autoriza")]
        public ErrorDto Fnd_Autorizacion_Mov_Autoriza(int CodEmpresa, string pGestion, string pAutorizador, List<FndAutorizacionMovData> movimiento)
        {
            return _bl.Fnd_Autorizacion_Mov_Autoriza(CodEmpresa, pGestion, pAutorizador, movimiento);
        }
    }
}