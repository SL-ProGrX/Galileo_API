using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoProcesosMasivoModels;


namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmCoCobroJudicialReversaMasivoController : ControllerBase
    {
        private readonly FrmCoCobroJudicialReversaMasivoBL _bl;
        public FrmCoCobroJudicialReversaMasivoController(IConfiguration config)
   => _bl = new FrmCoCobroJudicialReversaMasivoBL(config);

        [Authorize]
        [HttpPost("Co_CobroJudicialRevMasivo_CargarOperaciones")]
        public ErrorDto<CoProcesosMasivoCargaResponse> Co_CobroJudicialRevMasivo_CargarOperaciones(
                    int codEmpresa, [FromBody] CoProcesosMasivoCargaRequest request, string modulo)
    => _bl.Co_CobroJudicialRevMasivo_CargarOperaciones(
        codEmpresa,
        request.Operaciones,
        request.Usuario,
       modulo  );


        [Authorize]
        [HttpPost("Co_CobroJudicialRevMasivo_Procesar")]
        public ErrorDto<bool> Co_CobroJudicialRevMasivo_Procesar(int codEmpresa,  string usuario,string notas, string modulo )
              => _bl.Co_CobroJudicialRevMasivo_Procesar(codEmpresa, notas, usuario, modulo);

    }
}
