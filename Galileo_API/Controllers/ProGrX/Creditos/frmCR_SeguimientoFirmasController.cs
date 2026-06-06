using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Credito;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Credito;

namespace Galileo.Controllers.ProGrX.Credito
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCRSeguimientoFirmasController : ControllerBase
    {
        private readonly FrmCRSeguimientoFirmasBL _bl;

        public FrmCRSeguimientoFirmasController(IConfiguration config)
        {
            _bl = new FrmCRSeguimientoFirmasBL(config);
        }

        [Authorize]
        [HttpGet("CR_SeguimientoFirmas_Obtener")]
        public ErrorDto<List<CRSeguimientoFirmasData>> CR_SeguimientoFirmas_Obtener(int CodEmpresa, int operacion)
        {
            return _bl.CR_SeguimientoFirmas_Obtener(CodEmpresa, operacion);
        }

        [Authorize]
        [HttpPost("CR_SeguimientoFirmas_Guardar")]
        public ErrorDto CR_SeguimientoFirmas_Guardar(int CodEmpresa, CRSeguimientoFirmasData firmasData)
        {
            return _bl.CR_SeguimientoFirmas_Guardar(CodEmpresa, firmasData);
        }
    }
}