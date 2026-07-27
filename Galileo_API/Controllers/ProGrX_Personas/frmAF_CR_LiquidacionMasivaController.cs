using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Personas;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using Galileo.Models;

namespace Galileo.Controllers.ProGrX_Personas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFCrLiquidacionMasivaController : ControllerBase
    {
        private readonly FrmAFCrLiquidacionMasivaBL _bl;

        public FrmAFCrLiquidacionMasivaController(IConfiguration config)
        {
            _bl = new FrmAFCrLiquidacionMasivaBL(config);
        }

        [Authorize]
        [HttpPost("AF_LiquidacionMasiva_Obtener")]
        public ErrorDto<List<AfLiquidacionMasiva>> AF_LiquidacionMasiva_Obtener(int CodEmpresa, [FromBody] AfLiquidacionMasivaFiltros Filtro)
        {
            return _bl.AF_LiquidacionMasiva_Obtener(CodEmpresa, Filtro);
        }

        [Authorize]
        [HttpGet("AF_LiquidacionMasiva_Obtener_Causas")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_LiquidacionMasiva_Obtener_Causas(
            int CodEmpresa,
            string? tipoApl = null,
            DateTime? inicio = null,
            DateTime? corte = null)
        {
            return _bl.AF_LiquidacionMasiva_Obtener_Causas(CodEmpresa, tipoApl, inicio, corte);
        }

        [Authorize]
        [HttpGet("AF_LiquidacionMasiva_Obtener_Instituciones")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_LiquidacionMasiva_Obtener_Instituciones(int CodEmpresa)
        {
            return _bl.AF_LiquidacionMasiva_Obtener_Instituciones(CodEmpresa);
        }

        [Authorize]
        [HttpPost("AF_LiquidacionMasiva")]
        public ErrorDto AF_LiquidacionMasiva(int CodEmpresa, int RenunciaId, string Usuario, short S06 = 1)
        {
            return _bl.AF_LiquidacionMasiva(CodEmpresa, RenunciaId, Usuario, S06);
        }

        [Authorize]
        [HttpPost("AF_LiquidacionMasiva_Proceso_Iniciar")]
        public ErrorDto<AfLiqMasivaProgreso> AF_LiquidacionMasiva_Proceso_Iniciar(int CodEmpresa, [FromBody] AfLiqMasivaIniciarRequest request)
        {
            return _bl.AF_LiquidacionMasiva_Proceso_Iniciar(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("AF_LiquidacionMasiva_Proceso_ProcesarLote")]
        public ErrorDto<AfLiqMasivaProgreso> AF_LiquidacionMasiva_Proceso_ProcesarLote(int CodEmpresa, Guid ProcesoId, int Tamano)
        {
            return _bl.AF_LiquidacionMasiva_Proceso_ProcesarLote(CodEmpresa, ProcesoId, Tamano);
        }

        [Authorize]
        [HttpGet("AF_LiquidacionMasiva_Proceso_Estado_Obtener")]
        public ErrorDto<AfLiqMasivaProgreso> AF_LiquidacionMasiva_Proceso_Estado_Obtener(int CodEmpresa, Guid ProcesoId)
        {
            return _bl.AF_LiquidacionMasiva_Proceso_Estado_Obtener(CodEmpresa, ProcesoId);
        }

        [Authorize]
        [HttpGet("AF_LiquidacionMasiva_Proceso_Activo_Obtener")]
        public ErrorDto<AfLiqMasivaProgreso> AF_LiquidacionMasiva_Proceso_Activo_Obtener(int CodEmpresa, string Usuario)
        {
            return _bl.AF_LiquidacionMasiva_Proceso_Activo_Obtener(CodEmpresa, Usuario);
        }
    }
}
