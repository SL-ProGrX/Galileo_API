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
    public class FrmCprSolicitudCotizacionController : ControllerBase
    {
        private readonly FrmCprSolicitudCotizacionBL _bl;
        public FrmCprSolicitudCotizacionController(IConfiguration config)
        {
            _bl = new FrmCprSolicitudCotizacionBL(config);
        }

        [HttpPost("CprSolicitudContizacionBs_Guardar")]
        public ErrorDto CprSolicitudContizacionBs_Guardar(int CodEmpresa, bool editaBs, CprSolicitusCotizacionGuardar datos)
        {
            return _bl.CprSolicitudContizacionBs_Guardar(CodEmpresa, datos);
        }

        [HttpDelete("CprSolicitudCotizacionBs_Eliminar")]
        public ErrorDto CprSolicitudCotizacionBs_Eliminar(int CodEmpresa, int id_cotizacion_linea)
        {
            return _bl.CprSolicitudCotizacionBs_Eliminar(CodEmpresa, id_cotizacion_linea);
        }

        [HttpGet("CprSolicitudContizacionLista_Obtener")]
        public ErrorDto<List<CprSolicitudProvCotiza>> CprSolicitudContizacionLista_Obtener(int CodEmpresa, int cpr_id, string cod_proveedor)
        {
            return _bl.CprSolicitudContizacionLista_Obtener(CodEmpresa, cpr_id, cod_proveedor);
        }
    }
}