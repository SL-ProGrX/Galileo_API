

using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoAplFndContratosInformesModels;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmCoAplFndContratosInformesController : ControllerBase
    {
        private readonly FrmCoAplFndContratosInformesBL _bl;

        public FrmCoAplFndContratosInformesController(IConfiguration config) => _bl = new FrmCoAplFndContratosInformesBL(config);

        [Authorize]
        [HttpGet("Co_AplFnd_ContratosInformes_Personas_Obtener")]
        public ErrorDto<List<CoAplFndContratosInformesPersonasResult>> Co_AplFnd_ContratosInformes_Personas_Obtener(int codEmpresa)
                => _bl.Co_AplFnd_ContratosInformes_Personas_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("Co_AplFnd_ContratosInformes_Aplicaciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Co_AplFnd_ContratosInformes_Aplicaciones_Obtener(int codEmpresa)
              => _bl.Co_AplFnd_ContratosInformes_Aplicaciones_Obtener(codEmpresa);

        [Authorize]
        [HttpPost("Co_AplFnd_ContratosInformes_Bitacora_Registrar")]
        public ErrorDto Co_AplFnd_ContratosInformes_Bitacora_Registrar(int codEmpresa, string usuario, string strTipoMovimiento, string strDetalleMovimiento)
           => _bl.Co_AplFnd_ContratosInformes_Bitacora_Registrar(codEmpresa, usuario, strTipoMovimiento, strDetalleMovimiento);
    }
}
