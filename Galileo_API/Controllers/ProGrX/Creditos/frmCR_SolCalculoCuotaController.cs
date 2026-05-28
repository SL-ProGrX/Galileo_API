using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrSolCalculoCuotaController : ControllerBase
    {
        private readonly FrmCrSolCalculoCuotaBl _bl;

        public FrmCrSolCalculoCuotaController(IConfiguration config)
        {
            _bl = new FrmCrSolCalculoCuotaBl(config);
        }

        [HttpGet("CrSolCalculoCuota_Pantalla_Obtener")]
        public ErrorDto<CrSolCalculoCuotaPantallaData> CrSolCalculoCuota_Pantalla_Obtener(int codEmpresa)
            => _bl.CrSolCalculoCuota_Pantalla_Obtener(codEmpresa);

        [HttpPost("CrSolCalculoCuota_Factor_Obtener")]
        public ErrorDto<CrSolCalculoCuotaFactorData> CrSolCalculoCuota_Factor_Obtener(
            int codEmpresa, CrSolCalculoCuotaFactorRequest request)
        => _bl.CrSolCalculoCuota_Factor_Obtener(codEmpresa, request);

        [HttpPost("CrSolCalculoCuota_Cuota_Calcular")]
        public ErrorDto<CrSolCalculoCuotaCalcularCuotaData> CrSolCalculoCuota_Cuota_Calcular(
            int codEmpresa, CrSolCalculoCuotaCalcularCuotaRequest request)
            => _bl.CrSolCalculoCuota_Cuota_Calcular(codEmpresa, request);

        [HttpPost("CrSolCalculoCuota_Nivelada_Calcular")]
        public ErrorDto<CrSolCalculoCuotaNiveladaData> CrSolCalculoCuota_Nivelada_Calcular(
            int codEmpresa, CrSolCalculoCuotaNiveladaRequest request)
            => _bl.CrSolCalculoCuota_Nivelada_Calcular(codEmpresa, request);

        [HttpPost("CrSolCalculoCuota_DiasMes_Obtener")]
        public ErrorDto<CrSolCalculoCuotaDiasMesData> CrSolCalculoCuota_DiasMes_Obtener(
            int codEmpresa, CrSolCalculoCuotaDiasMesRequest request)
            => _bl.CrSolCalculoCuota_DiasMes_Obtener(codEmpresa, request);
    }
}