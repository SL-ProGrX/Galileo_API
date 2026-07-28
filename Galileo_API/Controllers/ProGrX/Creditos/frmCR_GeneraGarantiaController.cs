using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrGeneraGarantiaController : ControllerBase
    {
        private readonly FrmCrGeneraGarantiaBl _bl;

        public FrmCrGeneraGarantiaController(IConfiguration config)
        {
            _bl = new FrmCrGeneraGarantiaBl(config);
        }

        [HttpPost("CR_GeneraGarantia_Pagare_Preparar")]
        public ErrorDto<CrGeneraGarantiaPagareDto> CR_GeneraGarantia_Pagare_Preparar(
            int codEmpresa,
            CrGeneraGarantiaOperacionRequest request) =>
            _bl.CR_GeneraGarantia_Pagare_Preparar(codEmpresa, request);

        [HttpPost("CR_GeneraGarantia_Contrato_Preparar")]
        public ErrorDto<CrGeneraGarantiaContratoDto> CR_GeneraGarantia_Contrato_Preparar(
            int codEmpresa,
            string usuario,
            CrGeneraGarantiaOperacionRequest request) =>
            _bl.CR_GeneraGarantia_Contrato_Preparar(codEmpresa, usuario, request);

        [HttpPost("CR_GeneraGarantia_PagareEmail_Enviar")]
        public ErrorDto<CrGeneraGarantiaEmailDto> CR_GeneraGarantia_PagareEmail_Enviar(
            int codEmpresa,
            CrGeneraGarantiaOperacionRequest request) =>
            _bl.CR_GeneraGarantia_PagareEmail_Enviar(codEmpresa, request);

        [HttpPost("CR_GeneraGarantia_Letras_Obtener")]
        public ErrorDto<List<CrGeneraGarantiaLetraDto>> CR_GeneraGarantia_Letras_Obtener(
            int codEmpresa,
            CrGeneraGarantiaRangoRequest request) =>
            _bl.CR_GeneraGarantia_Letras_Obtener(codEmpresa, request);

        [HttpPost("CR_GeneraGarantia_PreImpreso_Preparar")]
        public ErrorDto<CrGeneraGarantiaPreImpresoDto> CR_GeneraGarantia_PreImpreso_Preparar(
            int codEmpresa,
            CrGeneraGarantiaOperacionRequest request) =>
            _bl.CR_GeneraGarantia_PreImpreso_Preparar(codEmpresa, request);
    }
}
