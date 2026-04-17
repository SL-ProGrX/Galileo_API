using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoCobroJudicialMasivoModels;


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
        public ErrorDto<CoCobroJudicialMasivoCargaResponse> Co_CobroJudicialMasivo_CargarOperaciones(
                    int codEmpresa, [FromBody] CoCobroJudicialMasivoCargaRequest request)
    => _bl.Co_CobroJudicialMasivo_CargarOperaciones(
        codEmpresa,
        request.Operaciones,
        request.Usuario);


        [Authorize]
        [HttpPost("Co_CobroJudicialMasivo_Procesar")]
        public ErrorDto<bool> Co_CobroJudicialMasivo_Procesar(int codEmpresa,  string usuario,string notas )
              => _bl.Co_CobroJudicialMasivo_Procesar(codEmpresa, notas, usuario);

    }
}
