using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Extensions;
using Galileo_API.Models.ProGrX.Credito;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrDigitalCatalogosController : ControllerBase
    {
        private readonly FrmCrDigitalCatalogosBl _bl;

        public FrmCrDigitalCatalogosController(IConfiguration config)
        {
            _bl = new FrmCrDigitalCatalogosBl(config);
        }

        [HttpGet("CrDigitalCatalogos_Obtener")]
        public ErrorDto<List<CrDigitalCatalogoData>> CrDigitalCatalogos_Obtener(
            int codEmpresa,
            string tipo)
        {
            return _bl.CrDigitalCatalogos_Obtener(codEmpresa, tipo);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("CrDigitalCatalogos_Guardar")]
        public ErrorDto CrDigitalCatalogos_Guardar(
            int codEmpresa,
            string tipo,
            CrDigitalCatalogoData request)
        {
            return _bl.CrDigitalCatalogos_Guardar(
                codEmpresa,
                User.ObtenerUsuario(),
                tipo,
                request);
        }

        [ValidateAntiForgeryToken]
        [HttpDelete("CrDigitalCatalogos_Eliminar")]
        public ErrorDto CrDigitalCatalogos_Eliminar(
            int codEmpresa,
            string tipo,
            short id)
        {
            return _bl.CrDigitalCatalogos_Eliminar(
                codEmpresa,
                User.ObtenerUsuario(),
                tipo,
                id);
        }
    }
}
