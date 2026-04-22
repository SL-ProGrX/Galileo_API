using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoProcesosMasivoModels;


namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmCoCobroJudicialMasivoController : ControllerBase
    {
        private readonly FrmCoCobroJudicialMasivoBL _bl;
        public FrmCoCobroJudicialMasivoController(IConfiguration config)
   => _bl = new FrmCoCobroJudicialMasivoBL(config);

        [Authorize]
        [HttpPost("Co_CobroJudicialMasivo_CargarOperaciones")]
        public ErrorDto<CoProcesosMasivoCargaResponse> Co_CobroJudicialMasivo_CargarOperaciones(
                    int codEmpresa, [FromBody] CoProcesosMasivoCargaRequest request, string modulo)
    => _bl.Co_CobroJudicialMasivo_CargarOperaciones(
        codEmpresa,
        request.Operaciones,
        request.Usuario,
       modulo  );


        [Authorize]
        [HttpPost("Co_CobroJudicialMasivo_Procesar")]
        public ErrorDto<bool> Co_CobroJudicialMasivo_Procesar(int codEmpresa,  string usuario,string notas, string modulo )
              => _bl.Co_CobroJudicialMasivo_Procesar(codEmpresa, notas, usuario, modulo);

    }
}
