using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo.Models.ERROR;
using static Galileo_API.Models.ProGrX.Creditos.FrmCRAbonoMasivo_ManualModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models;
namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class FrmCRAbonoMasivo_ManualController : ControllerBase
    {
        private readonly FrmCRAbonoMasivo_ManualBL _bl;

        public FrmCRAbonoMasivo_ManualController(IConfiguration config)
        {
            _bl = new FrmCRAbonoMasivo_ManualBL(config);
        }


        [HttpGet("CR_AbonoMasivo_Manual_Operadoras_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_AbonoMasivo_Manual_Operadoras_Obtener(int codEmpresa)
           => _bl.CR_AbonoMasivo_Manual_Operadoras_Obtener(codEmpresa);


        [HttpGet("CR_AbonoMasivo_Manual_Planes_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_AbonoMasivo_Manual_Planes_Obtener(int codEmpresa, string operadora)
           => _bl.CR_AbonoMasivo_Manual_Planes_Obtener(codEmpresa, operadora);

        [HttpPost("CR_AbonoMasivo_Manual_ProcesarAbonosMasivos")]
        public ErrorDto<CrAplicacionAbonoMasivoProcesarResponse> CR_AbonoMasivo_Manual_ProcesarAbonosMasivos(int codEmpresa, [FromBody] CrAplicacionAbonoMasivoProcesarRequest request)
              => _bl.ProcesarAbonosMasivos(codEmpresa, request);

        [HttpPost("CR_AbonoMasivo_Manual_CargaDeducciones_Procesar")]
        public ErrorDto<CrAplicacionAbonoMasivoResponse> CR_AbonoMasivo_Manual_CargaDeducciones_Procesar(int codEmpresa, [FromBody] CrAplicacionAbonoMasivoRequest request)
              => _bl.CR_AbonoMasivo_Manual_CargaDeducciones_Procesar(codEmpresa, request);



    }
}
