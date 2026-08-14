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
    public class FrmCrCategoriasCreditoController
        : ControllerBase
    {
        private readonly FrmCrCategoriasCreditoBl _bl;

        public FrmCrCategoriasCreditoController(
            IConfiguration config)
        {
            _bl = new FrmCrCategoriasCreditoBl(
                config);
        }

        [HttpGet(
            "CR_frmCR_Categorias_Credito_ProbabilidadDefault_Obtener")]
        public ErrorDto<List<
            CrCategoriasCreditoProbabilidadDefaultData>>
            CR_frmCR_Categorias_Credito_ProbabilidadDefault_Obtener(
                int codEmpresa)
        {
            return _bl
                .CR_frmCR_Categorias_Credito_ProbabilidadDefault_Obtener(
                    codEmpresa);
        }

        [HttpGet(
            "CR_frmCR_Categorias_Credito_ProbabilidadMora_Obtener")]
        public ErrorDto<List<
            CrCategoriasCreditoProbabilidadMoraData>>
            CR_frmCR_Categorias_Credito_ProbabilidadMora_Obtener(
                int codEmpresa)
        {
            return _bl
                .CR_frmCR_Categorias_Credito_ProbabilidadMora_Obtener(
                    codEmpresa);
        }

        [HttpGet(
            "CR_frmCR_Categorias_Credito_Segmentos_Obtener")]
        public ErrorDto<List<
            CrCategoriasCreditoSegmentoData>>
            CR_frmCR_Categorias_Credito_Segmentos_Obtener(
                int codEmpresa)
        {
            return _bl
                .CR_frmCR_Categorias_Credito_Segmentos_Obtener(
                    codEmpresa);
        }
    }
}