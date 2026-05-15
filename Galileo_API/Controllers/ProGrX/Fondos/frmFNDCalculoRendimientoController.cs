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
    public class FrmFndCalculoRendimientoController : ControllerBase
    {
        private readonly FrmFndCalculoRendimientoBL _BL;

        public FrmFndCalculoRendimientoController(IConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _BL = new FrmFndCalculoRendimientoBL(config);
        }

        [Authorize]
        [HttpGet("Operadoras_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Operadoras_Lista(int CodEmpresa)
        {
            return _BL.Operadoras_Lista(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Plan_Obtener")]
        public ErrorDto<FndPlanDatosDto> Plan_Obtener(int CodEmpresa, int CodOperadora, string CodPlan)
        {
            return _BL.Plan_Obtener(CodEmpresa, CodOperadora, CodPlan);
        }

        [Authorize]
        [HttpGet("Plan_Scroll")]
        public ErrorDto<FndPlanDatosDto> Plan_Scroll(int CodEmpresa, int CodOperadora, string? CodPlan, int ScrollCode)
        {
            return _BL.Plan_Scroll(CodEmpresa, CodOperadora, CodPlan, ScrollCode);
        }

        [Authorize]
        [HttpPost("AplicarRendimientos")]
        public ErrorDto<FndRendimientoResultadoDto> AplicarRendimientos(int CodEmpresa, FndRendimientoRequestDto dto)
        {
            return _BL.AplicarRendimientos(CodEmpresa, dto);
        }

        [Authorize]
        [HttpGet("HistorialRend_Lista")]
        public ErrorDto<List<FndHistorialRendDto>> HistorialRend_Lista(int CodEmpresa, int CodOperadora, string CodPlan)
        {
            return _BL.HistorialRend_Lista(CodEmpresa, CodOperadora, CodPlan);
        }

        [Authorize]
        [HttpGet("Planes_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Planes_Lista(int CodEmpresa, int CodOperadora)
        {
            return _BL.Planes_Lista(CodEmpresa, CodOperadora);
        }

        [Authorize]
        [HttpGet("FechaServidor_Obtener")]
        public ErrorDto FechaServidor_Obtener(int CodEmpresa)
        {
            return _BL.FechaServidor_Obtener(CodEmpresa);
        }
    }
}