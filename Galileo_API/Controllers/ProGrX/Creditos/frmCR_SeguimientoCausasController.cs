
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using static Galileo_API.Models.ProGrX.Creditos.FrmCRSeguimientoCausasModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCRSeguimientoCausasController : ControllerBase
    {
        private readonly FrmCRSeguimientoCausasBL _bl;

        public FrmCRSeguimientoCausasController(IConfiguration config)
        {
            _bl = new FrmCRSeguimientoCausasBL(config);
        }


        [HttpPost("CR_SeguimientoCausas_Obtener")]
        public ErrorDto<List<CrSeguimientoCausasData>> CR_SeguimientoCausas_Obtener(int codEmpresa, [FromBody] CrSeguimientoCausasObtenerRequest request)
            => _bl.CR_SeguimientoCausas_Obtener(codEmpresa, request);


        [HttpPost("CR_SeguimientoCausas_Actualizar")]
        public ErrorDto<bool> CR_SeguimientoCausas_Actualizar(int codEmpresa, [FromBody] CrSeguimientoCausasActualizarRequest request)
             => _bl.CR_SeguimientoCausas_Actualizar(codEmpresa, request);
    }
}
