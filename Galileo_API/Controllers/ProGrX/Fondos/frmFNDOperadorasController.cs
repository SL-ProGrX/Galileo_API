using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndOperadorasController : ControllerBase
    {
        private readonly FrmFndOperadorasBL _BL;

        public FrmFndOperadorasController(IConfiguration? config)
        {
            _BL = new FrmFndOperadorasBL(config);
        }

        [Authorize]
        [HttpGet("AF_Operadora_Obtener")]
        public ErrorDto<FndOperadoraDto> AF_Operadora_Obtener(int CodEmpresa, int cod_operadora)
        {
            return _BL.AF_Operadora_Obtener(CodEmpresa, cod_operadora);
        }

        [Authorize]
        [HttpGet("AF_Operadoras_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_Operadoras_Obtener(int CodEmpresa)
        {
            return _BL.AF_Operadoras_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("AF_Operadora_Guardar")]
        public ErrorDto AF_Operadora_Guardar(int codEmpresa, FndOperadoraDto request)
        {
            return _BL.AF_Operadora_Guardar(codEmpresa, request);
        }

        [Authorize]
        [HttpGet("FND_OperadoraPlanes_Obtener")]
        public ErrorDto<List<OperadoraPlanDto>> FND_OperadoraPlanes_Obtener(int CodEmpresa, int cod_operadora)
        {
            return _BL.FND_OperadoraPlanes_Obtener(CodEmpresa, cod_operadora);
        }

        [Authorize]
        [HttpDelete("AF_Operadora_Eliminar")]
        public ErrorDto AF_Operadora_Eliminar(int codEmpresa, int cod_operadora)
        {
            return _BL.AF_Operadora_Eliminar(codEmpresa, cod_operadora);
        }

        [Authorize]
        [HttpGet("AF_Operadora_Scroll_Obtener")]
        public ErrorDto<FndOperadoraDto> AF_Operadora_Scroll_Obtener(int CodEmpresa, int cod_operadora, int scrollCode)
        {
            return _BL.AF_Operadora_Scroll_Obtener(CodEmpresa, cod_operadora, scrollCode);
        }
    }
}