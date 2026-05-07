using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Hipotecario;
using Galileo_API.Models.ProGrX_Hipotecario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Hipotecario
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmVivTramiteNotasController : ControllerBase
    {
        private readonly FrmVivTramiteNotasBl _bl;

        public FrmVivTramiteNotasController(IConfiguration config)
        {
            _bl = new FrmVivTramiteNotasBl(config);
        }

        [HttpGet("VivTramiteNotas_ObtenerInformacionOperacion")]
        public ErrorDto<VivTramiteNotaOperacionData?> VivTramiteNotas_ObtenerInformacionOperacion(
            int codEmpresa, string numeroOperacion, int idGarantia)
        {
            return _bl.VivTramiteNotas_ObtenerInformacionOperacion(codEmpresa, numeroOperacion, idGarantia);
        }

        [HttpGet("VivTramiteNotas_ObtenerLista")]
        public ErrorDto<List<VivTramiteNotaData>> VivTramiteNotas_ObtenerLista(
            int codEmpresa, int idGarantia, string profesional)
        {
            return _bl.VivTramiteNotas_ObtenerLista(codEmpresa, idGarantia, profesional);
        }

        [HttpPost("VivTramiteNotas_Guardar")]
        public ErrorDto VivTramiteNotas_Guardar(
            int codEmpresa, string usuario, VivTramiteNotaGuardarRequest request)
        {
            return _bl.VivTramiteNotas_Guardar(codEmpresa, usuario, request);
        }
    }
}
