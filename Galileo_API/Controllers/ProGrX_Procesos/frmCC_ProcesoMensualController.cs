
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Procesos.frmCC_ProcesoMensualBL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualCargaArchivos;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualEstadoModels;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.Controllers.ProGrX_Procesos
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmCcProcesoMensualController : ControllerBase
    {
        private const int MaxCargaDeduccionesBytes = 30_000_000;

        private readonly CcProcesoMensualBL _bl;

        public FrmCcProcesoMensualController(IConfiguration config)
        {
            _bl = new CcProcesoMensualBL(config);
        }

        [HttpGet("CcProcesoMensual_Inicial_Obtener")]
        public ErrorDto<CcProcesoMensualInicialResponse> CcProcesoMensual_Inicial_Obtener(
            int codEmpresa,
            int gInstitucion)
        {
            return _bl.CcProcesoMensual_Inicial_Obtener(
                codEmpresa,
                gInstitucion);
        }

        [HttpGet("CcProcesoMensual_Bitacora_Obtener")]
        public ErrorDto<List<CcProcesoMensualBitacoraDbModel>> CcProcesoMensual_Bitacora_Obtener(
            int codEmpresa,
            int gInstitucion,
            decimal proceso)
        {
            return _bl.CcProcesoMensual_Bitacora_Obtener(
                codEmpresa,
                gInstitucion,
                proceso);
        }

        [HttpGet("CcProcesoMensual_ValidaPaso")]
        public ErrorDto<CcProcesoMensualValidaPasoResponse> CcProcesoMensual_ValidaPaso(
            int codEmpresa,
            int codInstitucion,
            decimal fechaProceso,
            string transaccion = "08")
        {
            return _bl.CcProcesoMensual_ValidaPaso(
                codEmpresa,
                codInstitucion,
                fechaProceso,
                transaccion);
        }

        [HttpPost("CcProcesoMensual_CargarDeducciones")]
        [RequestSizeLimit(MaxCargaDeduccionesBytes)]
        public ErrorDto<CcProcesoMensualCargaDeduccionesResponse> CcProcesoMensual_CargarDeducciones(
            [FromBody] CcProcesoMensualCargaDeduccionesRequest request)
        {
            return _bl.CcProcesoMensual_CargarDeducciones(request);
        }


        [HttpPost("CcProcesoMensual_CambiarFechaProceso_Ejecutar")]
        public ErrorDto<CcProcesoMensualCambiarFechaResponse> CcProcesoMensual_CambiarFechaProceso_Ejecutar(int codEmpresa, [FromBody] CcProcesoMensualCambiarFechaRequest request)
        {
            return _bl.CcProcesoMensual_CambiarFechaProceso_Ejecutar(codEmpresa, request);
        }

        [HttpGet("CcProcesoMensual_EstadoActualProceso_Obtener")]
        public ErrorDto<CcProcesoMensualEstadoResponse> CcProcesoMensual_EstadoActualProceso_Obtener(
            int codEmpresa,
            int gInstitucion,
            string? pasoEjecutado = null)
        {
            return _bl.CcProcesoMensual_EstadoActualProceso_Obtener(
                codEmpresa,
                gInstitucion,
                pasoEjecutado);
        }

        [HttpGet("CcProcesoMensual_DatosInstitucion_Obtener")]
        public ErrorDto<CcProcesoMensualCargaConfigDbModel> CcProcesoMensual_DatosInstitucion_Obtener(
            int codEmpresa,
            int codInstitucion)
        {
            return _bl.CcProcesoMensual_DatosInstitucion_Obtener(
                codEmpresa,
                codInstitucion);
        }

        [HttpPost("CcProcesoMensual_DesglosarPlanilla_Ejecutar")]
        public ErrorDto<CcProcesoMensualDesglosePlanillaResponse> CcProcesoMensual_DesglosarPlanilla_Ejecutar([FromBody] CcProcesoMensualDesgloseRequest request)
        {
            return _bl.CcProcesoMensual_DesglosarPlanilla_Ejecutar(request);
        }

        [HttpPost("CcProcesoMensual_Ahorros_Aplicar")]
        public ErrorDto<CcProcesoMensualAhorros> CcProcesoMensual_Ahorros_Aplicar(int codEmpresa, int codInstitucion, decimal fechaProceso, string usuario)
        {
            return _bl.CcProcesoMensual_Ahorros_Aplicar(codEmpresa, codInstitucion, fechaProceso, usuario);
        }

        [HttpPost("CcProcesoMensual_AhorrosInconsistencias_Aplicar")]
        public ErrorDto<CcProcesoMensualAhorros> CcProcesoMensual_AhorrosInconsistencias_Aplicar(int codEmpresa, int codInstitucion, decimal fechaProceso, string usuario)
        {
            return _bl.CcProcesoMensual_AhorrosInconsistencias_Aplicar(codEmpresa, codInstitucion, fechaProceso, usuario);
        }

        [HttpPost("CcProcesoMensual_AhorrosDevoluciones_Aplicar")]
        public ErrorDto<CcProcesoMensualAhorros> CcProcesoMensual_AhorrosDevoluciones_Aplicar(int codEmpresa, int codInstitucion, decimal fechaProceso, string usuario)
        {
            return _bl.CcProcesoMensual_AhorrosDevoluciones_Aplicar(codEmpresa, codInstitucion, fechaProceso, usuario);
        }

        [HttpGet("CcProcesoMensual_ParametrosAhorroReporte_Obtener")]
        public ErrorDto<CcProcesoMensualAhorroReporteModel> CcProcesoMensual_ParametrosAhorroReporte_Obtener(int codEmpresa, int codInstitucion)
        {
            return _bl.CcProcesoMensual_ParametrosAhorroReporte_Obtener(codEmpresa, codInstitucion);
        }

        [HttpPost("CcProcesoMensual_CrAbonos_Aplicar")]
        public ErrorDto<CcProcesoMensualCreditosAplicacionResponse> CcProcesoMensual_CrAbonos_Aplicar(int codEmpresa, int codInstitucion, decimal fechaProceso, DateTime fechaSistema, string usuario)
        {
            return _bl.CcProcesoMensual_CrAbonos_Aplicar(codEmpresa, codInstitucion, fechaProceso, fechaSistema, usuario);
        }

        [HttpPost("CcProcesoMensual_CrdReporteInconsistencia_Aplicar")]
        public ErrorDto<CcProcesoMensualCreditosAplicacionResponse> CcProcesoMensual_CrdReporteInconsistencia_Aplicar(int codEmpresa, int codInstitucion, decimal fechaProceso, string usuario)
        {
            return _bl.CcProcesoMensual_CrdReporteInconsistencia_Aplicar(codEmpresa, codInstitucion, fechaProceso, usuario);
        }

        [HttpPost("CcProcesoMensual_CrdCalculoInteresesMoratorios_Aplicar")]
        public ErrorDto<CcProcesoMensualCreditosAplicacionResponse> CcProcesoMensual_CrdCalculoInteresesMoratorios_Aplicar(int codEmpresa, int codInstitucion, decimal fechaProceso, string usuario)
        {
            return _bl.CcProcesoMensual_CrdCalculoInteresesMoratorios_Aplicar(codEmpresa, codInstitucion, fechaProceso, usuario);
        }

        [HttpPost("CcProcesoMensual_CrdRecalculoSaldoMes_Aplicar")]
        public ErrorDto<CcProcesoMensualCreditosAplicacionResponse> CcProcesoMensual_CrdRecalculoSaldoMes_Aplicar(int codEmpresa, int codInstitucion, decimal fechaProceso, string usuario)
        {
            return _bl.CcProcesoMensual_CrdRecalculoSaldoMes_Aplicar(codEmpresa, codInstitucion, fechaProceso, usuario);
        }
    }
}
