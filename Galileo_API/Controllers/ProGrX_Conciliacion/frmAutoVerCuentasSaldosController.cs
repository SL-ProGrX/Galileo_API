using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Conciliacion;
using Galileo_API.Models.ProGrX_Conciliacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Conciliacion
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class FrmAutoVerCuentasSaldosController :
        ControllerBase
    {
        private readonly FrmAutoVerCuentasSaldosBL _bl;

        public FrmAutoVerCuentasSaldosController(
            IConfiguration config)
        {
            _bl = new FrmAutoVerCuentasSaldosBL(config);
        }

        [HttpGet(
            "Conciliacion_AutoVerCuentasSaldos_Periodos_Obtener")]
        public ErrorDto<List<AutoVerCuentasSaldosPeriodoData>>
            Conciliacion_AutoVerCuentasSaldos_Periodos_Obtener(
                int codEmpresa)
        {
            return _bl
                .Conciliacion_AutoVerCuentasSaldos_Periodos_Obtener(
                    codEmpresa);
        }

        [HttpGet(
            "Conciliacion_AutoVerCuentasSaldos_Resumen_Obtener")]
        public ErrorDto<List<AutoVerCuentasSaldosResumenData>>
            Conciliacion_AutoVerCuentasSaldos_Resumen_Obtener(
                int codEmpresa,
                string Request)
        {
            return _bl
                .Conciliacion_AutoVerCuentasSaldos_Resumen_Obtener(
                    codEmpresa,
                    Request);
        }

        [HttpGet(
            "Conciliacion_AutoVerCuentasSaldos_Tendencia_Obtener")]
        public ErrorDto<List<AutoVerCuentasSaldosTendenciaData>>
            Conciliacion_AutoVerCuentasSaldos_Tendencia_Obtener(
                int codEmpresa,
                string Request)
        {
            return _bl
                .Conciliacion_AutoVerCuentasSaldos_Tendencia_Obtener(
                    codEmpresa,
                    Request);
        }

        [HttpGet(
            "Conciliacion_AutoVerCuentasSaldos_Asignacion_Obtener")]
        public ErrorDto<List<AutoVerCuentasSaldosAsignacionData>>
            Conciliacion_AutoVerCuentasSaldos_Asignacion_Obtener(
                int codEmpresa,
                string Request)
        {
            return _bl
                .Conciliacion_AutoVerCuentasSaldos_Asignacion_Obtener(
                    codEmpresa,
                    Request);
        }

        [HttpGet(
            "Conciliacion_AutoVerCuentasSaldos_FormaPago_Obtener")]
        public ErrorDto<List<AutoVerCuentasSaldosFormaPagoData>>
            Conciliacion_AutoVerCuentasSaldos_FormaPago_Obtener(
                int codEmpresa,
                string Request)
        {
            return _bl
                .Conciliacion_AutoVerCuentasSaldos_FormaPago_Obtener(
                    codEmpresa,
                    Request);
        }

        [HttpGet(
            "Conciliacion_AutoVerCuentasSaldos_RevisionContable_Obtener")]
        public ErrorDto<List<AutoVerCuentasSaldosRevisionContableData>>
            Conciliacion_AutoVerCuentasSaldos_RevisionContable_Obtener(
                int codEmpresa,
                string request)
        {
            return _bl
                .Conciliacion_AutoVerCuentasSaldos_RevisionContable_Obtener(
                    codEmpresa,
                    request);
        }

        [HttpGet(
            "Conciliacion_AutoVerCuentasSaldos_NoContabilizados_Obtener")]
        public ErrorDto<List<AutoVerCuentasSaldosNoContabilizadoData>>
            Conciliacion_AutoVerCuentasSaldos_NoContabilizados_Obtener(
                int codEmpresa,
                string Request)
        {
            return _bl
                .Conciliacion_AutoVerCuentasSaldos_NoContabilizados_Obtener(
                    codEmpresa,
                    Request);
        }

        [HttpGet(
            "Conciliacion_AutoVerCuentasSaldos_Cambios_Obtener")]
        public ErrorDto<List<AutoVerCuentasSaldosCambioData>>
            Conciliacion_AutoVerCuentasSaldos_Cambios_Obtener(
                int codEmpresa,
                string Request)
        {
            return _bl
                .Conciliacion_AutoVerCuentasSaldos_Cambios_Obtener(
                    codEmpresa,
                    Request);
        }

        [HttpGet(
            "Conciliacion_AutoVerCuentasSaldos_Analitico_Obtener")]
        public ErrorDto<List<AutoVerCuentasSaldosAnaliticoData>>
            Conciliacion_AutoVerCuentasSaldos_Analitico_Obtener(
                int codEmpresa,
                string Request)
        {
            return _bl
                .Conciliacion_AutoVerCuentasSaldos_Analitico_Obtener(
                    codEmpresa,
                    Request);
        }

        [HttpGet(
            "Conciliacion_AutoVerCuentasSaldos_ConciliaMovimientos_Obtener")]
        public ErrorDto<List<AutoVerCuentasSaldosConciliaData>>
            Conciliacion_AutoVerCuentasSaldos_ConciliaMovimientos_Obtener(
                int codEmpresa,
                string Request)
        {
            return _bl
                .Conciliacion_AutoVerCuentasSaldos_ConciliaMovimientos_Obtener(
                    codEmpresa,
                    Request);
        }
    }
}