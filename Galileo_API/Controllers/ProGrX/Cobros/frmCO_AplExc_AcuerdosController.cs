using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCoAplExcAcuerdosController : ControllerBase
    {
        private readonly FrmCoAplExcAcuerdosBl _bl;

        public FrmCoAplExcAcuerdosController(IConfiguration config) => 
            _bl = new FrmCoAplExcAcuerdosBl(config);

        
        [HttpGet("CoAplExcAcuerdos_Obtener")]
        public ErrorDto<CoAplExcAcuerdosData?> CoAplExcAcuerdos_Obtener(int codEmpresa, int idAcuerdo)
        {
            return _bl.CoAplExcAcuerdos_Obtener(codEmpresa, idAcuerdo);
        }

        [HttpGet("CoAplExcAcuerdos_Lista_Obtener")]
        public ErrorDto<List<CoAplExcAcuerdosData>> CoAplExcAcuerdos_Lista_Obtener(int codEmpresa, string filtro, string estado)
        {
            return _bl.CoAplExcAcuerdos_Lista_Obtener(codEmpresa, filtro, estado);
        }

        [HttpPost("CoAplExcAcuerdos_Guardar")]
        public ErrorDto CoAplExcAcuerdos_Guardar(int codEmpresa, CoAplExcAcuerdosData request)
        {
            return _bl.CoAplExcAcuerdos_Guardar(codEmpresa, request);
        }

    }
}


