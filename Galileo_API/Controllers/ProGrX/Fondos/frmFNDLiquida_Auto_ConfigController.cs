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
    public class FrmFndLiquidaAutoConfigController : ControllerBase
    {
        private readonly FrmFndLiquidaAutoConfigBl _BL;

        public FrmFndLiquidaAutoConfigController(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _BL = new FrmFndLiquidaAutoConfigBl(config);
        }

        [Authorize]
        [HttpGet("Parametros_Lista")]
        public ErrorDto<List<FndLiqAutoParametroDto>> Parametros_Lista(int CodEmpresa)
        {
            return _BL.Parametros_Lista(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Planes_Lista")]
        public ErrorDto<List<FndLiqAutoPlanesDto>> Planes_Lista(int CodEmpresa)
        {
            return _BL.Planes_Lista(CodEmpresa);
        }
        
        [Authorize]
        [HttpGet("PlanesPatronal_Lista")]
        public ErrorDto<List<FndLiqAutoPlanesPatronalDto>> PlanesPatronal_Lista(int CodEmpresa)
        {
            return _BL.LiqAuto_Planes_Patronal_Lista(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Reportes_Lista")]
        public ErrorDto<List<FndLiqAutoReporteDto>> Reportes_Lista(int CodEmpresa, int Anio, int Mes)
        {
            return _BL.LiqAuto_Reportes_Lista(CodEmpresa, Anio, Mes);
        }

        [Authorize]
        [HttpGet("Operadoras_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Operadoras_Lista(int CodEmpresa)
        {
            return _BL.Operadoras_Lista(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Procesos_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Procesos_Lista(int CodEmpresa)
        {
            return _BL.Procesos_Lista(CodEmpresa);
        }

        [Authorize]
        [HttpGet("PlanesReporte_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> PlanesReporte_Lista(int CodEmpresa)
        {
            return _BL.PlanesReporte_Lista(CodEmpresa);
        }

        [Authorize]
        [HttpPost("Planes_Guardar")]
        public ErrorDto<bool> Planes_Guardar(int CodEmpresa, FndLiqAutoPlanesAddRequestDto dto)
        {
            return _BL.Planes_Guardar(CodEmpresa, dto);
        }
    }
}