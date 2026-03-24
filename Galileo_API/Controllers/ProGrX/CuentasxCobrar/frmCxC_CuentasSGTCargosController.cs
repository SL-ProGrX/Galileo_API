using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.CuentasxCobrar
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCxCCuentasCargosController : ControllerBase
    {
        private readonly FrmCxCCuentasCargosBL BL;

        public FrmCxCCuentasCargosController(IConfiguration config)
        {
            BL = new FrmCxCCuentasCargosBL(config);
        }

        [Authorize]
        [HttpGet("CxC_Cuentas_Cargos_Operacion_Obtener")]
        public ErrorDto<CxCCuentasCargoOperacionDto> CxC_Cuentas_Cargos_Operacion_Obtener(int CodEmpresa, int operacion)
        {
            return BL.CxC_Cuentas_Cargos_Operacion_Obtener(CodEmpresa, operacion);
        }

        [Authorize]
        [HttpGet("CxC_Cuentas_Cargos_Operacion_Export")]
        public ErrorDto<CxCCuentasCargosListaResult> CxC_Cuentas_Cargos_Operacion_Export(int CodEmpresa, int operacion)
        {
            return BL.CxC_Cuentas_Cargos_Operacion_Export(CodEmpresa, operacion);
        }

        [Authorize]
        [HttpGet("CxC_Cuentas_Cargos_Disponibles_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_Cuentas_Cargos_Disponibles_Obtener(int CodEmpresa, int operacion, string? filtro)
        {
            return BL.CxC_Cuentas_Cargos_Disponibles_Obtener(CodEmpresa, operacion, filtro);
        }

        [Authorize]
        [HttpGet("CxC_Cuentas_Cargos_Scroll_Obtener")]
        public ErrorDto<CxCCuentasCargoDisponibleDto> CxC_Cuentas_Cargos_Scroll_Obtener(int CodEmpresa, int operacion, int scrollCode, string? cargoActual)
        {
            return BL.CxC_Cuentas_Cargos_Scroll_Obtener(CodEmpresa, operacion, scrollCode, cargoActual);
        }

        [Authorize]
        [HttpPost("CxC_Cuentas_Cargos_Guardar")]
        public ErrorDto CxC_Cuentas_Cargos_Guardar(int CodEmpresa, string usuario, [FromBody] CxCCuentasCargoGuardarRequest req)
        {
            return BL.CxC_Cuentas_Cargos_Guardar(CodEmpresa, usuario, req.cargo);
        }
        [Authorize]
        [HttpPost("CxC_Cuentas_Cargos_Eliminar")]
        public ErrorDto CxC_Cuentas_Cargos_Eliminar(int CodEmpresa, string usuario, [FromBody] CxCCuentasCargoEliminarRequest req)
        {
            var operacion = req.operacion ?? 0;
            return BL.CxC_Cuentas_Cargos_Eliminar(CodEmpresa, usuario, operacion, req.cod_cargo);
        }
    }
}