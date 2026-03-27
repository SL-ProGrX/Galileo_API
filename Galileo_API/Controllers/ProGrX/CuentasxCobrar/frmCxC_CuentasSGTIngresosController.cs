using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_CxC;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_CxC
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCxCCuentasSgtIngresosController : ControllerBase
    {
        private readonly FrmCxCCuentasSgtIngresosBl _bl;

        public FrmCxCCuentasSgtIngresosController(IConfiguration config)
        {
            _bl = new FrmCxCCuentasSgtIngresosBl(config);
        }

        [HttpGet("ListarRegistrosIngresos")]
        public ErrorDto<List<CxCIngresoDto>> ListarRegistrosIngresos(int codEmpresa, int operacion)
        {
            return _bl.ListarRegistrosIngresos(codEmpresa, operacion);
        }

        [HttpPost("GuardarRegistrosIngresos")]
        public ErrorDto<bool> GuardarRegistrosIngresos(int codEmpresa, CxCIngresoGuardarDto dto)
        {
            return _bl.GuardarRegistrosIngresos(codEmpresa, dto);
        }

        [HttpDelete("EliminarRegistroIngresos")]
        public ErrorDto<bool> EliminarRegistroIngresos(int codEmpresa, int operacion, int linea, string codCargo)
        {
            return _bl.EliminarRegistroIngresos(codEmpresa, operacion, linea, codCargo);
        }

        [HttpPost("ActualizarRegistroingreso")]
        public ErrorDto<bool> ActualizarRegistroingreso(int codEmpresa, int operacion, string usuario)
        {
            return _bl.ActualizarRegistroingreso(codEmpresa, operacion, usuario);
        }

        [HttpGet("Scroll")]
        public ErrorDto<CxCIngresoDto> Scroll(int codEmpresa,int operacion,string? codCargo,string direccion)
        {
            return _bl.Scroll(codEmpresa, operacion, codCargo, direccion);
        }

        [HttpGet("IngresosListar")]
        public ErrorDto<List<CxCIngresoDto>> IngresosListar(int codEmpresa)
        {
            return _bl.IngresosListar(codEmpresa);
        }
    }
}