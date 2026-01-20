using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmCprSolicitudCotizacionValidacionController : ControllerBase
    {
        private readonly FrmCprSolicitudCotizacionValidacionBL _bl;
        public FrmCprSolicitudCotizacionValidacionController(IConfiguration config)
        {
            _bl = new FrmCprSolicitudCotizacionValidacionBL(config);
        }

        [HttpGet("CprValidarCotizacionBs_Obtener")]
        public ErrorDto<CprSolicitudCotizacionPrvBsLista> CprValidarCotizacionBs_Obtener(int CodEmpresa, int? cpr_id, int? cod_unidad)
        {
            return _bl.CprValidarCotizacionBs_Obtener(CodEmpresa, cpr_id, cod_unidad);
        }

        [HttpPost("CprValidarContizacionBs_Guardar")]
        public ErrorDto CprValidarContizacionBs_Guardar(int CodEmpresa, bool editaBs, CprSolicitusCotizacionGuardar datos)
        {
            return _bl.CprValidarContizacionBs_Guardar(CodEmpresa, datos);
        }

        [HttpDelete("CprValidacionCotizacionBs_Eliminar")]
        public ErrorDto CprValidacionCotizacionBs_Eliminar(int CodEmpresa, int cpr_id ,string codigo, string cod_producto)
        {
            return _bl.CprValidacionCotizacionBs_Eliminar(CodEmpresa, cpr_id,codigo, cod_producto);
        }
    }
}