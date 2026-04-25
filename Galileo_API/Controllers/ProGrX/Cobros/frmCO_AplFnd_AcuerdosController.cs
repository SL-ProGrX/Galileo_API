using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoAplFndAcuerdosModels;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmCoAplFndAcuerdosController : ControllerBase
    {
        private readonly FrmCoAplFndAcuerdosBL _bl;
        public FrmCoAplFndAcuerdosController(IConfiguration config)
   => _bl = new FrmCoAplFndAcuerdosBL(config);

        [Authorize]
        [HttpGet("Co_AplFnd_Acuerdos_Consultar")]
        public ErrorDto<CoAplFndAcuerdosDetalleResponse> Co_AplFnd_Acuerdos_Consultar(int codEmpresa, int idAcuerdo)
                => _bl.Co_AplFnd_Acuerdos_Consultar(codEmpresa, idAcuerdo);
       
        [Authorize]
        [HttpPost("Co_AplFnd_Acuerdos_Guardar")]
        public ErrorDto<CoAplFndAcuerdosGuardarResponse> Co_AplFnd_Acuerdos_Guardar(int codEmpresa, [FromBody] CoAplFndAcuerdosDetalleResponse request)
                        => _bl.Co_AplFnd_Acuerdos_Guardar(codEmpresa, request);
        [Authorize]
        [HttpPost("Co_AplFnd_Acuerdos_Listar")]
        public ErrorDto<List<CoAplFndAcuerdosGridResponse>> Co_AplFnd_Acuerdos_Listar(int codEmpresa, [FromBody] CoAplFndAcuerdosFiltroRequest request)
                => _bl.Co_AplFnd_Acuerdos_Listar(codEmpresa, request);
       
        [Authorize]
        [HttpPost("Co_AplFnd_Acuerdos_CargaMasiva")]
        public ErrorDto<CoAplFndAcuerdosCargaMasivaResponse> Co_AplFnd_Acuerdos_CargaMasiva(
            int codEmpresa, [FromBody] CoAplFndAcuerdosCargaMasivaRequest request)
              => _bl.Co_AplFnd_Acuerdos_CargaMasiva(codEmpresa, request);
       
        [Authorize]
        [HttpGet("Co_AplFnd_Socios_Obtener")]
        public ErrorDto<List<CoAplFndAcuerdosSocioResult>> Co_AplFnd_Socios_Obtener(int codEmpresa)
               => _bl.Co_AplFnd_Socios_Obtener(codEmpresa);

      

    }
}
