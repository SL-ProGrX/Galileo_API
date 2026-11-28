using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Extensions;
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

        [HttpGet(
            "CR_frmCR_Categorias_Credito_CargaProbabilidades_Obtener")]
        public ErrorDto<List<
            CrCategoriasCreditoCargaProbabilidadData>>
            CR_frmCR_Categorias_Credito_CargaProbabilidades_Obtener(
                int codEmpresa)
        {
            return _bl
                .CR_frmCR_Categorias_Credito_CargaProbabilidades_Obtener(
                    codEmpresa);
        }

        [HttpPost(
            "CR_frmCR_Categorias_Credito_CargaProbabilidades_Procesar")]
        [ValidateAntiForgeryToken]
        public ErrorDto
            CR_frmCR_Categorias_Credito_CargaProbabilidades_Procesar(
                int codEmpresa,
                [FromBody]
                CrCategoriasCreditoCargaProbabilidadRequest request)
        {
            return _bl
                .CR_frmCR_Categorias_Credito_CargaProbabilidades_Procesar(
                    codEmpresa,
                    User.ObtenerUsuario(),
                    request);
        }

        [HttpPost("CR_frmCR_Categorias_Credito_Guardar")]
        [ValidateAntiForgeryToken]
        public ErrorDto CR_frmCR_Categorias_Credito_Guardar(
            int codEmpresa,
            string tipo,
            [FromBody] CrCategoriasCreditoCrudRequest request)
        {
            return _bl.CR_frmCR_Categorias_Credito_Guardar(
                codEmpresa,
                User.ObtenerUsuario(),
                tipo,
                request);
        }

        [HttpDelete("CR_frmCR_Categorias_Credito_Eliminar")]
        [ValidateAntiForgeryToken]
        public ErrorDto CR_frmCR_Categorias_Credito_Eliminar(
            int codEmpresa,
            string tipo,
            int id)
        {
            return _bl.CR_frmCR_Categorias_Credito_Eliminar(
                codEmpresa,
                User.ObtenerUsuario(),
                tipo,
                id);
        }
    }
}
