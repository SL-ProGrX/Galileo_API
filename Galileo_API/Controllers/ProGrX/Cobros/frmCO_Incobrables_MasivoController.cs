using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoIncobrablesMasivoModels;


namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmCoIncobrablesMasivoController : ControllerBase
    {
        private readonly FrmCoIncobrablesMasivoBL _bl;
        public FrmCoIncobrablesMasivoController(IConfiguration config)
   => _bl = new FrmCoIncobrablesMasivoBL(config);

        [Authorize]
        [HttpPost("Co_IncobrablesMasivo_CargarArchivo")]
        public ErrorDto<CoIncobrablesMasivoCargaResponse> Co_IncobrablesMasivo_CargarArchivo(
                    int codEmpresa, [FromBody] CoIncobrablesMasivoCargaRequest request)
    => _bl.Co_IncobrablesMasivo_CargarArchivo(
        codEmpresa,
        request.Operaciones,
        request.Usuario);


        [Authorize]
        [HttpPost("Co_IncobrablesMasivo_Procesar")]
        public ErrorDto<bool> Co_IncobrablesMasivo_Procesar(int codEmpresa,  string usuario,string notas )
              => _bl.Co_IncobrablesMasivo_Procesar(codEmpresa, notas, usuario);

    }
}
