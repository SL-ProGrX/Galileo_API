using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.DataBaseTier.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndExclusionMultaController : ControllerBase
    {
        private readonly FrmFndExclusionMultaDb _BL;

        public FrmFndExclusionMultaController(IConfiguration config)
        {
            _BL = new FrmFndExclusionMultaDb(config);
        }

        [Authorize]
        [HttpGet("FND_Operadoras_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_Operadoras_Obtener(int CodEmpresa)
        {
            return _BL.FND_Operadoras_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("FND_Planes_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> FND_Planes_Obtener(int CodEmpresa, string cod_operadora)
        {
            return _BL.FND_Planes_Obtener(CodEmpresa, cod_operadora);
        }

        [Authorize]
        [HttpGet("FND_Contratos_Obtener")]
        public ErrorDto<List<FndContratoDto>> FND_Contratos_Obtener(int CodEmpresa, string cod_operadora, string cod_plan)
        {
            return _BL.FND_Contratos_Obtener(CodEmpresa, cod_operadora, cod_plan);
        }

        [Authorize]
        [HttpPost("FND_Exclusion_Multas_List")]
        public ErrorDto<List<FndExclusionMultaDto>> FND_Exclusion_Multas_List(int CodEmpresa, FiltrosBuscarExclusionDto filtros)
        {
            return _BL.FND_Exclusion_Multas_List(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("FND_Exclusion_Multas_Add")]
        public ErrorDto FND_Exclusion_Multas_Add(int CodEmpresa, RegistrarExclusionDto request)
        {
            return _BL.FND_Exclusion_Multas_Add(CodEmpresa, request);
        }
    }
}
