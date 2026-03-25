using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Credito
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrCatalogoDestinosController : ControllerBase
    {
        private readonly FrmCrCatalogoDestinosBl _bl;

        public FrmCrCatalogoDestinosController(IConfiguration config)
        {
            _bl = new FrmCrCatalogoDestinosBl(config);
        }

        [HttpGet("CrCatalogoDestinos_Obtener")]
        public ErrorDto<List<CrCatalogoDestinoData>> CrCatalogoDestinos_Obtener(int codEmpresa)
        {
            return _bl.CrCatalogoDestinos_Obtener(codEmpresa);
        }

        [HttpGet("CrCatalogos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrCatalogos_Obtener(int codEmpresa, string tipo)
        {
            return _bl.CrCatalogos_Obtener(codEmpresa, tipo);
        }

        [HttpGet("CrCatalogoDestinos_Asignados_Obtener")]
        public ErrorDto<List<CrCatalogoDestinoData>> CrCatalogoDestinos_Asignados_Obtener(int codEmpresa, string codigo)
        {
            return _bl.CrCatalogoDestinos_Asignados_Obtener(codEmpresa, codigo);
        }

        [HttpPost("CrCatalogoDestinos_Asignar")]
        public ErrorDto CrCatalogoDestinos_Asignar(int codEmpresa, string codDestino, string catalogo, bool isChecked)
        {
            return _bl.CrCatalogoDestinos_Asignar(codEmpresa, codDestino, catalogo, isChecked);
        }

        [HttpPost("CrCatalogoDestinos_Guardar")]
        public ErrorDto CrCatalogoDestinos_Guardar(int codEmpresa, string usuario, CrCatalogoDestinoData request)
        {
            return _bl.CrCatalogoDestinos_Guardar(codEmpresa, usuario, request);
        }

        [HttpDelete("CrCatalogoDestinos_Eliminar")]
        public ErrorDto CrCatalogoDestinos_Eliminar(int codEmpresa, string codDestino, string usuario)
        {
            return _bl.CrCatalogoDestinos_Eliminar(codEmpresa, codDestino, usuario);
        }
    }
}
