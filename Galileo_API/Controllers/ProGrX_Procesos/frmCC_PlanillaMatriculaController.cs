using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Procesos;
using Galileo_API.Models.ProGrX_Procesos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Procesos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCCPlanillaMatriculaController : ControllerBase
    {
        private readonly FrmCCPlanillaMatriculaBL BL;

        public FrmCCPlanillaMatriculaController(IConfiguration config)
        {
            BL = new FrmCCPlanillaMatriculaBL(config);
        }
        [Authorize]
        [HttpGet("CC_PlanillaMatricula_Instituciones_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CC_PlanillaMatricula_Instituciones_Dropdown_Obtener(int CodEmpresa)
        {
            return BL.CC_PlanillaMatricula_Instituciones_Dropdown_Obtener(CodEmpresa);
        }
        [Authorize]
        [HttpGet("CC_PlanillaMatricula_Lista_Obtener")]
        public ErrorDto<CcPlanillaMatriculaListaResultDto> CC_PlanillaMatricula_Lista_Obtener(int CodEmpresa, string parametros)
        {
            return BL.CC_PlanillaMatricula_Lista_Obtener(CodEmpresa, parametros);
        }
        [Authorize]
        [HttpGet("CC_PlanillaMatricula_Lista_Export")]
        public ErrorDto<CcPlanillaMatriculaListaResultDto> CC_PlanillaMatricula_Lista_Export(int CodEmpresa, string parametros)
        {
            return BL.CC_PlanillaMatricula_Lista_Export(CodEmpresa, parametros);
        }
        [Authorize]
        [HttpPost("CC_PlanillaMatricula_Bloquear")]
        public ErrorDto CC_PlanillaMatricula_Bloquear(int CodEmpresa,string usuario,[FromBody] CcPlanillaMatriculaBloquearRequest request)
        {
            if (request == null)
            {
                return new ErrorDto
                {
                    Code = -2,
                    Description = "Debe indicar la información a procesar."
                };
            }

            return BL.CC_PlanillaMatricula_Bloquear(CodEmpresa, usuario, request);
        }
        [Authorize]
        [HttpPost("CC_PlanillaMatricula_BloqueoMasivo")]
        public ErrorDto<CcPlanillaMatriculaBloqueoMasivoResultDto> CC_PlanillaMatricula_BloqueoMasivo(int CodEmpresa,string usuario,[FromBody] CcPlanillaMatriculaBloqueoMasivoRequest request)
        {
            if (request == null)
            {
                return new ErrorDto<CcPlanillaMatriculaBloqueoMasivoResultDto>
                {
                    Code = -2,
                    Description = "Debe indicar la información a procesar.",
                    Result = new CcPlanillaMatriculaBloqueoMasivoResultDto()
                };
            }

            return BL.CC_PlanillaMatricula_BloqueoMasivo(CodEmpresa, usuario, request);
        }
        [Authorize]
        [HttpPost("CC_PlanillaMatricula_ArchivoTotal_Generar")]
        public ErrorDto<CcPlanillaMatriculaArchivoTotalDto> CC_PlanillaMatricula_ArchivoTotal_Generar(int CodEmpresa,[FromBody] CcPlanillaMatriculaArchivoTotalRequest request)
        {
            if (request == null)
            {
                return new ErrorDto<CcPlanillaMatriculaArchivoTotalDto>
                {
                    Code = -2,
                    Description = "Debe indicar la información a procesar.",
                    Result = new CcPlanillaMatriculaArchivoTotalDto()
                };
            }

            return BL.CC_PlanillaMatricula_ArchivoTotal_Generar(CodEmpresa, request);
        }
    }
}