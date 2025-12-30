using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cajas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cajas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCajasAccesosController : ControllerBase
    {
        private readonly FrmCajasAccesosBl BL_Cajas_Accesos;

        public FrmCajasAccesosController(IConfiguration config)
            => BL_Cajas_Accesos = new FrmCajasAccesosBl(config);

        [Authorize]
        [HttpGet("Cajas_Apertura_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_TiposCedulasCajas_Apertura_Obtener_Obtener(int CodEmpresa, string usuario)
        {
            return BL_Cajas_Accesos.Cajas_Apertura_Obtener(CodEmpresa, usuario);
        }

        [Authorize]
        [HttpPost("Cajas_AbreCaja")]
        public ErrorDto Cajas_AbreCaja(int codEmpresa, string codCaja, string usuario, string appVersion, string clave)
        {
            return BL_Cajas_Accesos.Cajas_AbreCaja(codEmpresa, codCaja, usuario, appVersion, clave);
        }
    }
}