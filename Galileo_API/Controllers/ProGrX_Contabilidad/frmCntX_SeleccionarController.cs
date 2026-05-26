using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCntXSeleccionarController : ControllerBase
    {
        private readonly FrmCntXSeleccionarBl _bl;

        public FrmCntXSeleccionarController(IConfiguration config) => _bl = new FrmCntXSeleccionarBl(config);

        [HttpGet("CntX_Seleccionar_CargaInicial")]
        public ErrorDto<CntXSeleccionarCargaResponse> CntX_Seleccionar_CargaInicial(int codEmpresa, string usuario, bool muestraTodas)
        {
            return _bl.CntX_Seleccionar_CargaInicial(codEmpresa, usuario, muestraTodas);
        }

        [HttpGet("CntX_Seleccionar_Buscar")]
        public ErrorDto<List<CntXSeleccionarContabilidadItem>> CntX_Seleccionar_Buscar(int codEmpresa, string usuario, string filtro = "")
        {
            return _bl.CntX_Seleccionar_Buscar(codEmpresa, usuario, filtro);
        }

        [HttpPost("CntX_Seleccionar_Seleccionar")]
        public ErrorDto<CntXParametrosDto> CntX_Seleccionar_Seleccionar(int codEmpresa, string usuario, int codContabilidad)
        {
            return _bl.CntX_Seleccionar_Seleccionar(codEmpresa, usuario, codContabilidad);
        }
    }
}
