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

        [HttpGet("AH_ExcedentesMensuales_Periodos_Lista")]
        public ErrorDto<List<ExcPeriodosDto>> AH_ExcedentesMensuales_Periodos_Lista(
            [FromQuery] int codEmpresa)
        {
            return _bl.AH_ExcedentesMensuales_Periodos_Lista(codEmpresa);
        }

        [HttpGet("AH_ExcedentesMensuales_Cortes_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> AH_ExcedentesMensuales_Cortes_Lista(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId)
        {
            return _bl.AH_ExcedentesMensuales_Cortes_Lista(codEmpresa, periodoId);
        }

        [HttpGet("AH_ExcedentesMensuales_Resumen_Lista")]
        public ErrorDto<List<ResumenExcedenteMDto>> AH_ExcedentesMensuales_Resumen_Lista(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId)
        {
            return _bl.AH_ExcedentesMensuales_Resumen_Lista(codEmpresa, periodoId);
        }

        [HttpGet("AH_ExcedentesMensuales_Mensual_Monto_Obtener")]
        public ErrorDto<decimal?> AH_ExcedentesMensuales_Mensual_Monto_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId,
            [FromQuery] DateTime corte,
            [FromQuery] string tipoAplicacion)
        {
            return _bl.AH_ExcedentesMensuales_Mensual_Monto_Obtener(
                codEmpresa,
                periodoId,
                corte,
                tipoAplicacion);
        }

        [HttpGet("AH_ExcedentesMensuales_Mensual_UtilidadContable_Obtener")]
        public ErrorDto<decimal?> AH_ExcedentesMensuales_Mensual_UtilidadContable_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] int anio,
            [FromQuery] int mes,
            [FromQuery] int enlace)
        {
            return _bl.AH_ExcedentesMensuales_Mensual_UtilidadContable_Obtener(
                codEmpresa,
                anio,
                mes,
                enlace);
        }

        [HttpGet("AH_ExcedentesMensuales_Mensual_Periodo_Obtener")]
        public ErrorDto<FrmAhExcedentesMensualesMensualPeriodoDto?> AH_ExcedentesMensuales_Mensual_Periodo_Obtener(
    [FromQuery] int codEmpresa,
    [FromQuery] int periodoId)
        {
            return _bl.AH_ExcedentesMensuales_Mensual_Periodo_Obtener(
                codEmpresa,
                periodoId);
        }

        [HttpPut("AH_ExcedentesMensuales_Mensual_BaseAplicacion_Actualizar")]
        public ErrorDto AH_ExcedentesMensuales_Mensual_BaseAplicacion_Actualizar(
        [FromQuery] int codEmpresa,
        [FromBody] FrmAhExcedentesMensualesBaseAplicacionRequest request)
            {
                return _bl.AH_ExcedentesMensuales_Mensual_BaseAplicacion_Actualizar(
                    codEmpresa,
                    request);
            }


        [HttpGet("AH_ExcedentesMensuales_Mensual_Valida")]
        public ErrorDto<string?> AH_ExcedentesMensuales_Mensual_Valida(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId,
            [FromQuery] DateTime corte)
        {
            return _bl.AH_ExcedentesMensuales_Mensual_Valida(codEmpresa, periodoId, corte);
        }

        [HttpPost("AH_ExcedentesMensuales_Mensual_Aplicar")]
        public ErrorDto<FrmAhExcedentesMensualesMensualResultadoDto?> AH_ExcedentesMensuales_Mensual_Aplicar(
    [FromQuery] int codEmpresa,
    [FromBody] FrmAhExcedentesMensualesMensualAplicarRequest request)
        {
            return _bl.AH_ExcedentesMensuales_Mensual_Aplicar(
                codEmpresa,
                request);
        }

        [HttpGet("AH_ExcedentesMensuales_Cierre_Periodo_Obtener")]
        public ErrorDto<ExcedentePeriodoDto?> AH_ExcedentesMensuales_Cierre_Periodo_Obtener(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId)
        {
            return _bl.AH_ExcedentesMensuales_Cierre_Periodo_Obtener(codEmpresa, periodoId);
        }

        [HttpGet("AH_ExcedentesMensuales_Cierre_Renta_Lista")]
        public ErrorDto<List<RentaExcedenteDto>> AH_ExcedentesMensuales_Cierre_Renta_Lista(
            [FromQuery] int codEmpresa)
        {
            return _bl.AH_ExcedentesMensuales_Cierre_Renta_Lista(codEmpresa);
        }

        [HttpGet("AH_ExcedentesMensuales_Cierre_Valida")]
        public ErrorDto<string?> AH_ExcedentesMensuales_Cierre_Valida(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId)
        {
            return _bl.AH_ExcedentesMensuales_Cierre_Valida(codEmpresa, periodoId);
        }

        [HttpPost("AH_ExcedentesMensuales_Cierre_Aplicar")]
        public ErrorDto AH_ExcedentesMensuales_Cierre_Aplicar(
    [FromQuery] int codEmpresa,
    [FromBody] FrmAhExcedentesMensualesCierreAplicarRequest request)
        {
            return _bl.AH_ExcedentesMensuales_Cierre_Aplicar(codEmpresa, request);
        }

        [HttpGet("AH_ExcedentesMensuales_Aplicaciones_UltimoPeriodoCerrado_Obtener")]
        public ErrorDto<ExcPeriodosDto?> AH_ExcedentesMensuales_Aplicaciones_UltimoPeriodoCerrado_Obtener(
            [FromQuery] int codEmpresa)
        {
            return _bl.AH_ExcedentesMensuales_Aplicaciones_UltimoPeriodoCerrado_Obtener(codEmpresa);
        }

        [HttpGet("AH_ExcedentesMensuales_Aplicaciones_Log_Lista")]
        public ErrorDto<List<BitacoraExcedenteDto>> AH_ExcedentesMensuales_Aplicaciones_Log_Lista(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId)
        {
            return _bl.AH_ExcedentesMensuales_Aplicaciones_Log_Lista(codEmpresa, periodoId);
        }

        [HttpGet("AH_ExcedentesMensuales_Aplicaciones_ProcesosPendientes_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> AH_ExcedentesMensuales_Aplicaciones_ProcesosPendientes_Lista(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId)
        {
            return _bl.AH_ExcedentesMensuales_Aplicaciones_ProcesosPendientes_Lista(codEmpresa, periodoId);
        }

        [HttpPost("AH_ExcedentesMensuales_Aplicaciones_Salidas_Separa")]
        public ErrorDto AH_ExcedentesMensuales_Aplicaciones_Salidas_Separa(
    [FromQuery] int codEmpresa,
    [FromBody] FrmAhExcedentesMensualesSalidasSeparaRequest request)
        {
            return _bl.AH_ExcedentesMensuales_Aplicaciones_Salidas_Separa(codEmpresa, request);
        }

        [HttpGet("AH_ExcedentesMensuales_Aplicaciones_SalidasPendientes_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> AH_ExcedentesMensuales_Aplicaciones_SalidasPendientes_Lista(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId)
        {
            return _bl.AH_ExcedentesMensuales_Aplicaciones_SalidasPendientes_Lista(codEmpresa, periodoId);
        }

        [HttpPost("AH_ExcedentesMensuales_Aplicaciones_Salidas_Fondos")]
        public ErrorDto AH_ExcedentesMensuales_Aplicaciones_Salidas_Fondos(
    [FromQuery] int codEmpresa,
    [FromBody] FrmAhExcedentesMensualesSalidasFondosRequest request)
        {
            return _bl.AH_ExcedentesMensuales_Aplicaciones_Salidas_Fondos(
                codEmpresa,
                request);
        }

        [HttpGet("AH_ExcedentesMensuales_Parametros_Lista")]
        public ErrorDto<List<ExcParametrosDto>> AH_ExcedentesMensuales_Parametros_Lista(
            [FromQuery] int codEmpresa)
        {
            return _bl.AH_ExcedentesMensuales_Parametros_Lista(codEmpresa);
        }

        [HttpGet("AH_ExcedentesMensuales_Bitacora_Valida")]
        public ErrorDto<bool> AH_ExcedentesMensuales_Bitacora_Valida(
            [FromQuery] int codEmpresa,
            [FromQuery] int periodoId,
            [FromQuery] string codProceso,
            [FromQuery] string detalle)
        {
            return _bl.AH_ExcedentesMensuales_Bitacora_Valida(
                codEmpresa,
                periodoId,
                codProceso,
                detalle);
        }

        [HttpPost("AH_ExcedentesMensuales_Bitacora_Registrar")]
        public ErrorDto AH_ExcedentesMensuales_Bitacora_Registrar(
    [FromQuery] int codEmpresa,
    [FromBody] FrmAhExcedentesMensualesBitacoraRegistrarRequest request)
        {
            return _bl.AH_ExcedentesMensuales_Bitacora_Registrar(
                codEmpresa,
                request);
        }
    }
}
