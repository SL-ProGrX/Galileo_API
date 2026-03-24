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
    public class FrmCntXUtilVerificaAsientosController : ControllerBase
    {
        private readonly FrmCntXUtilVerificaAsientosBl _bl;

        public FrmCntXUtilVerificaAsientosController(IConfiguration config) 
            => _bl = new FrmCntXUtilVerificaAsientosBl(config);

        [HttpPost("CntXAsientos_Verificar")]
        public ErrorDto CntXAsientos_Verificar(int codEmpresa, CntXAsientosVerificarRequest request)
        {
            return _bl.CntXAsientos_Verificar(codEmpresa, request);
        }
    }
}