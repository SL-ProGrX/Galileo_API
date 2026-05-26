namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    using Galileo.Models.ERROR;
    using Galileo_API.Models.ProGrX_Contabilidad;
    using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;

    [ApiController]
    [Route("api/[controller]")]
    public class FrmCntXConCierreController : ControllerBase
    {
        private readonly FrmCntXConCierreBL _bl;

        public FrmCntXConCierreController(IConfiguration config)
        {
            _bl = new FrmCntXConCierreBL(config);
        }

        [HttpGet("AF_CntXConCierre_Obtener")]
        [Authorize]
        public ErrorDto<FrmCntXConCierreLista> AF_CntXConCierre_Obtener(int codEmpresa)
        {
            return _bl.AF_CntXConCierre_Obtener(codEmpresa);
        }

        [HttpGet("AF_CntXConCierre_ObtenerDefinicion")]
        [Authorize]
        public ErrorDto<FrmCntXConCierreDefinicionLista> AF_CntXConCierre_ObtenerDefinicion(int codEmpresa, int codConsolida)
        {
            return _bl.AF_CntXConCierre_ObtenerDefinicion(codEmpresa, codConsolida);
        }

        [HttpGet("AF_CntXConCierre_ValidaPeriodoBase")]
        [Authorize]
        public ErrorDto AF_CntXConCierre_ValidaPeriodoBase(int codEmpresa, int mes, int anio, int codContabilidad)
        {
            return _bl.AF_CntXConCierre_ValidaPeriodoBase(codEmpresa, mes, anio, codContabilidad);
        }

        [HttpGet("AF_CntXConCierre_ValidaPeriodoLocal")]
        [Authorize]
        public ErrorDto AF_CntXConCierre_ValidaPeriodoLocal(int codEmpresa, int mes, int anio, int codConsolida)
        {
            return _bl.AF_CntXConCierre_ValidaPeriodoLocal(codEmpresa, mes, anio, codConsolida);
        }
        [HttpGet("AF_CntXConCierre_ObtenerPortales")]
        [Authorize]
        public ErrorDto<FrmCntXConCierrePortalLista> AF_CntXConCierre_ObtenerPortales(int codEmpresa, int codConsolida)
        {
            return _bl.AF_CntXConCierre_ObtenerPortales(codEmpresa, codConsolida);
        }

        [HttpGet("AF_CntXConCierre_ValidaPeriodo")]
        [Authorize]
        public ErrorDto AF_CntXConCierre_ValidaPeriodo(int codEmpresa, int mes, int anio, int codContabilidad, bool soloAbierto)
        {
            return _bl.AF_CntXConCierre_ValidaPeriodo(codEmpresa, mes, anio, codContabilidad, soloAbierto);
        }

        [HttpPost("AF_CntXConCierre_InsertarPeriodo")]        
        [Authorize]
        public ErrorDto AF_CntXConCierre_InsertarPeriodo(int codEmpresa, int anio, int mes, int codContabilidad)
        {
            return _bl.AF_CntXConCierre_InsertarPeriodo(codEmpresa, anio, mes, codContabilidad);
        }

        [HttpPost("AF_CntXConCierre_InsertarMovimientos")]
        [Authorize]
        public ErrorDto AF_CntXConCierre_InsertarMovimientos(int codEmpresa, int codConsolida, int codContabilidad, int anio, int mes, int nivel)
        {
            return _bl.AF_CntXConCierre_InsertarMovimientos(codEmpresa, codConsolida, codContabilidad, anio, mes, nivel);
        }

        [HttpPut("AF_CntXConCierre_ActualizarMovimiento")]
        [Authorize]
        public ErrorDto AF_CntXConCierre_ActualizarMovimiento(int codEmpresa, [FromBody] FrmCntXConCierreActualizarMovimientoRequest req)
        {
            return _bl.AF_CntXConCierre_ActualizarMovimiento(codEmpresa, req);
        }
    }
}
