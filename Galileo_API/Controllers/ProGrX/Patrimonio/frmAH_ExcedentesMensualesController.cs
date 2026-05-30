using Galileo.Models;
using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Patrimonio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Patrimonio
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmAhExcedentesMensualesController : ControllerBase
    {
        private readonly FrmAhExcedentesMensualesBL _bl;

        public FrmAhExcedentesMensualesController(IConfiguration config)
        {
            _bl = new FrmAhExcedentesMensualesBL(config);
        }

        [HttpGet("Patrimonio_frmAH_ExcedentesMensuales_Periodos_Lista")]
        public ErrorDto<List<ExcPeriodosDto>> Patrimonio_frmAH_ExcedentesMensuales_Periodos_Lista(
            [FromQuery] int codEmpresa)
        {
            return _bl.Patrimonio_frmAH_ExcedentesMensuales_Periodos_Lista(codEmpresa);
        }

        [HttpGet("Patrimonio_frmAH_ExcedentesMensuales_Cortes_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Patrimonio_frmAH_ExcedentesMensuales_Cortes_Lista(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId)
        {
            return _bl.Patrimonio_frmAH_ExcedentesMensuales_Cortes_Lista(codEmpresa, periodoId);
        }

        [HttpGet("Patrimonio_frmAH_ExcedentesMensuales_Resumen_Lista")]
        public ErrorDto<List<ResumenExcedenteMDto>> Patrimonio_frmAH_ExcedentesMensuales_Resumen_Lista(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId)
        {
            return _bl.Patrimonio_frmAH_ExcedentesMensuales_Resumen_Lista(codEmpresa, periodoId);
        }

        [HttpGet("Patrimonio_frmAH_ExcedentesMensuales_Mensual_Monto_Obtener")]
        public ErrorDto<decimal?> Patrimonio_frmAH_ExcedentesMensuales_Mensual_Monto_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId,
            [FromQuery] DateTime corte,
            [FromQuery] string tipoAplicacion)
        {
            return _bl.Patrimonio_frmAH_ExcedentesMensuales_Mensual_Monto_Obtener(
                codEmpresa,
                periodoId,
                corte,
                tipoAplicacion);
        }

        [HttpGet("Patrimonio_frmAH_ExcedentesMensuales_Mensual_UtilidadContable_Obtener")]
        public ErrorDto<decimal?> Patrimonio_frmAH_ExcedentesMensuales_Mensual_UtilidadContable_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] int anio,
            [FromQuery] int mes,
            [FromQuery] int enlace)
        {
            return _bl.Patrimonio_frmAH_ExcedentesMensuales_Mensual_UtilidadContable_Obtener(
                codEmpresa,
                anio,
                mes,
                enlace);
        }

        [HttpPut("Patrimonio_frmAH_ExcedentesMensuales_Mensual_BaseAplicacion_Actualizar")]
        public ErrorDto Patrimonio_frmAH_ExcedentesMensuales_Mensual_BaseAplicacion_Actualizar(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId,
            [FromQuery] string tipoAplicacion,
            [FromQuery] string usuario)
        {
            return _bl.Patrimonio_frmAH_ExcedentesMensuales_Mensual_BaseAplicacion_Actualizar(
                codEmpresa,
                periodoId,
                tipoAplicacion,
                usuario);
        }

        [HttpGet("Patrimonio_frmAH_ExcedentesMensuales_Mensual_Valida")]
        public ErrorDto<string?> Patrimonio_frmAH_ExcedentesMensuales_Mensual_Valida(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId,
            [FromQuery] DateTime corte)
        {
            return _bl.Patrimonio_frmAH_ExcedentesMensuales_Mensual_Valida(codEmpresa, periodoId, corte);
        }

        [HttpPost("Patrimonio_frmAH_ExcedentesMensuales_Mensual_Aplicar")]
        public ErrorDto Patrimonio_frmAH_ExcedentesMensuales_Mensual_Aplicar(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId,
            [FromQuery] DateTime corte,
            [FromQuery] decimal monto,
            [FromQuery] string tipoAplicacion,
            [FromQuery] string usuario)
        {
            return _bl.Patrimonio_frmAH_ExcedentesMensuales_Mensual_Aplicar(
                codEmpresa,
                periodoId,
                corte,
                monto,
                tipoAplicacion,
                usuario);
        }

        [HttpGet("Patrimonio_frmAH_ExcedentesMensuales_Cierre_Periodo_Obtener")]
        public ErrorDto<ExcedentePeriodoDto?> Patrimonio_frmAH_ExcedentesMensuales_Cierre_Periodo_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId)
        {
            return _bl.Patrimonio_frmAH_ExcedentesMensuales_Cierre_Periodo_Obtener(codEmpresa, periodoId);
        }

        [HttpGet("Patrimonio_frmAH_ExcedentesMensuales_Cierre_Renta_Lista")]
        public ErrorDto<List<RentaExcedenteDto>> Patrimonio_frmAH_ExcedentesMensuales_Cierre_Renta_Lista(
            [FromQuery] int codEmpresa)
        {
            return _bl.Patrimonio_frmAH_ExcedentesMensuales_Cierre_Renta_Lista(codEmpresa);
        }

        [HttpGet("Patrimonio_frmAH_ExcedentesMensuales_Cierre_Valida")]
        public ErrorDto<string?> Patrimonio_frmAH_ExcedentesMensuales_Cierre_Valida(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId)
        {
            return _bl.Patrimonio_frmAH_ExcedentesMensuales_Cierre_Valida(codEmpresa, periodoId);
        }

        [HttpPost("Patrimonio_frmAH_ExcedentesMensuales_Cierre_Aplicar")]
        public ErrorDto Patrimonio_frmAH_ExcedentesMensuales_Cierre_Aplicar(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId,
            [FromQuery] string usuario)
        {
            return _bl.Patrimonio_frmAH_ExcedentesMensuales_Cierre_Aplicar(codEmpresa, periodoId, usuario);
        }

        [HttpGet("Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_UltimoPeriodoCerrado_Obtener")]
        public ErrorDto<ExcPeriodosDto?> Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_UltimoPeriodoCerrado_Obtener(
            [FromQuery] int codEmpresa)
        {
            return _bl.Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_UltimoPeriodoCerrado_Obtener(codEmpresa);
        }

        [HttpGet("Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_Log_Lista")]
        public ErrorDto<List<BitacoraExcedenteDto>> Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_Log_Lista(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId)
        {
            return _bl.Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_Log_Lista(codEmpresa, periodoId);
        }

        [HttpGet("Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_ProcesosPendientes_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_ProcesosPendientes_Lista(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId)
        {
            return _bl.Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_ProcesosPendientes_Lista(codEmpresa, periodoId);
        }

        [HttpPost("Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_Salidas_Separa")]
        public ErrorDto Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_Salidas_Separa(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId,
            [FromQuery] string usuario)
        {
            return _bl.Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_Salidas_Separa(codEmpresa, periodoId, usuario);
        }

        [HttpGet("Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_SalidasPendientes_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_SalidasPendientes_Lista(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId)
        {
            return _bl.Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_SalidasPendientes_Lista(codEmpresa, periodoId);
        }

        [HttpPost("Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_Salidas_Fondos")]
        public ErrorDto Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_Salidas_Fondos(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId,
            [FromQuery] string salida,
            [FromQuery] string usuario)
        {
            return _bl.Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_Salidas_Fondos(
                codEmpresa,
                periodoId,
                salida,
                usuario);
        }

        [HttpGet("Patrimonio_frmAH_ExcedentesMensuales_Parametros_Lista")]
        public ErrorDto<List<ExcParametrosDto>> Patrimonio_frmAH_ExcedentesMensuales_Parametros_Lista(
            [FromQuery] int codEmpresa)
        {
            return _bl.Patrimonio_frmAH_ExcedentesMensuales_Parametros_Lista(codEmpresa);
        }

        [HttpGet("Patrimonio_frmAH_ExcedentesMensuales_Bitacora_Valida")]
        public ErrorDto<bool> Patrimonio_frmAH_ExcedentesMensuales_Bitacora_Valida(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId,
            [FromQuery] string codProceso,
            [FromQuery] string detalle)
        {
            return _bl.Patrimonio_frmAH_ExcedentesMensuales_Bitacora_Valida(
                codEmpresa,
                periodoId,
                codProceso,
                detalle);
        }

        [HttpPost("Patrimonio_frmAH_ExcedentesMensuales_Bitacora_Registrar")]
        public ErrorDto Patrimonio_frmAH_ExcedentesMensuales_Bitacora_Registrar(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId,
            [FromQuery] string codProceso,
            [FromQuery] string detalle,
            [FromQuery] string usuario,
            [FromQuery] string tipoDocumento = "",
            [FromQuery] string codTransaccion = "")
        {
            return _bl.Patrimonio_frmAH_ExcedentesMensuales_Bitacora_Registrar(
                codEmpresa,
                periodoId,
                codProceso,
                detalle,
                usuario,
                tipoDocumento,
                codTransaccion);
        }
    }
}
