using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Credito;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Credito;

namespace Galileo.Controllers.ProGrX.Credito
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCRSeguimientoDocController : ControllerBase
    {
        private readonly FrmCRSeguimientoDocBL _bl;

        public FrmCRSeguimientoDocController(IConfiguration config)
        {
            _bl = new FrmCRSeguimientoDocBL(config);
        }

        [Authorize]
        [HttpPost("CR_SeguimientoDoc_Aplicar")]
        public ErrorDto CR_SeguimientoDoc_Aplicar(int CodEmpresa, FrmCRSeguimientoDocData documento)
        {
            return _bl.CR_SeguimientoDoc_Aplicar(CodEmpresa, documento);
        }
    }
}