using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Conciliacion;
using Galileo_API.Models.ProGrX_Conciliacion;
using Newtonsoft.Json;

namespace Galileo_API.BusinessLogic.ProGrX_Conciliacion
{
    public sealed class FrmAutoVerCuentasSaldosBL
    {
        private readonly FrmAutoVerCuentasSaldosDB _db;

        public FrmAutoVerCuentasSaldosBL(
            IConfiguration config)
        {
            _db = new FrmAutoVerCuentasSaldosDB(config);
        }

        public ErrorDto<List<AutoVerCuentasSaldosPeriodoData>>
            Conciliacion_AutoVerCuentasSaldos_Periodos_Obtener(
                int codEmpresa)
        {
            return _db
                .Conciliacion_AutoVerCuentasSaldos_Periodos_Obtener(
                    codEmpresa);
        }

        public ErrorDto<List<AutoVerCuentasSaldosResumenData>>
            Conciliacion_AutoVerCuentasSaldos_Resumen_Obtener(
                int codEmpresa,
                string Request)
        {
            AutoVerCuentasSaldosResumenQuery? request =
                JsonConvert.DeserializeObject<
                    AutoVerCuentasSaldosResumenQuery
                >(Request);

            return _db
                .Conciliacion_AutoVerCuentasSaldos_Resumen_Obtener(
                    codEmpresa,
                    request);
        }

        public ErrorDto<List<AutoVerCuentasSaldosTendenciaData>>
            Conciliacion_AutoVerCuentasSaldos_Tendencia_Obtener(
                int codEmpresa,
                string Request)
        {
            AutoVerCuentasSaldosTendenciaRequest? request =
                JsonConvert.DeserializeObject<
                    AutoVerCuentasSaldosTendenciaRequest
                >(Request);

            return _db
                .Conciliacion_AutoVerCuentasSaldos_Tendencia_Obtener(
                    codEmpresa,
                    request,
                    request?.auxiliar);
        }

        public ErrorDto<List<AutoVerCuentasSaldosAsignacionData>>
            Conciliacion_AutoVerCuentasSaldos_Asignacion_Obtener(
                int codEmpresa,
                string Request)
        {
            AutoVerCuentasSaldosCuentaQuery? request =
                JsonConvert.DeserializeObject<
                    AutoVerCuentasSaldosCuentaQuery
                >(Request);

            return _db
                .Conciliacion_AutoVerCuentasSaldos_Asignacion_Obtener(
                    codEmpresa,
                    request);
        }

        public ErrorDto<List<AutoVerCuentasSaldosFormaPagoData>>
            Conciliacion_AutoVerCuentasSaldos_FormaPago_Obtener(
                int codEmpresa,
                string Request)
        {
            AutoVerCuentasSaldosPeriodoQuery? request =
                JsonConvert.DeserializeObject<
                    AutoVerCuentasSaldosPeriodoQuery
                >(Request);

            return _db
                .Conciliacion_AutoVerCuentasSaldos_FormaPago_Obtener(
                    codEmpresa,
                    request);
        }

        public ErrorDto<List<AutoVerCuentasSaldosRevisionContableData>>
            Conciliacion_AutoVerCuentasSaldos_RevisionContable_Obtener(
                int codEmpresa,
                string Request)
        {
            AutoVerCuentasSaldosCuentaQuery? request =
                JsonConvert.DeserializeObject<
                    AutoVerCuentasSaldosCuentaQuery
                >(Request);

            return _db
                .Conciliacion_AutoVerCuentasSaldos_RevisionContable_Obtener(
                    codEmpresa,
                    request);
        }

        public ErrorDto<List<AutoVerCuentasSaldosNoContabilizadoData>>
            Conciliacion_AutoVerCuentasSaldos_NoContabilizados_Obtener(
                int codEmpresa,
                string Request)
        {
            AutoVerCuentasSaldosCuentaQuery? request =
                JsonConvert.DeserializeObject<
                    AutoVerCuentasSaldosCuentaQuery
                >(Request);

            return _db
                .Conciliacion_AutoVerCuentasSaldos_NoContabilizados_Obtener(
                    codEmpresa,
                    request);
        }

        public ErrorDto<List<AutoVerCuentasSaldosCambioData>>
            Conciliacion_AutoVerCuentasSaldos_Cambios_Obtener(
                int codEmpresa,
                string Request)
        {
            AutoVerCuentasSaldosPeriodoQuery? request =
                JsonConvert.DeserializeObject<
                    AutoVerCuentasSaldosPeriodoQuery
                >(Request);

            return _db
                .Conciliacion_AutoVerCuentasSaldos_Cambios_Obtener(
                    codEmpresa,
                    request);
        }

        public ErrorDto<List<AutoVerCuentasSaldosAnaliticoData>>
            Conciliacion_AutoVerCuentasSaldos_Analitico_Obtener(
                int codEmpresa,
                string Request)
        {
            AutoVerCuentasSaldosAnaliticoQuery? request =
                JsonConvert.DeserializeObject<
                    AutoVerCuentasSaldosAnaliticoQuery
                >(Request);

            return _db
                .Conciliacion_AutoVerCuentasSaldos_Analitico_Obtener(
                    codEmpresa,
                    request);
        }

        public ErrorDto<List<AutoVerCuentasSaldosConciliaData>>
            Conciliacion_AutoVerCuentasSaldos_ConciliaMovimientos_Obtener(
                int codEmpresa,
                string Request)
        {
            AutoVerCuentasSaldosConciliaQuery? request =
                JsonConvert.DeserializeObject<
                    AutoVerCuentasSaldosConciliaQuery
                >(Request);

            return _db
                .Conciliacion_AutoVerCuentasSaldos_ConciliaMovimientos_Obtener(
                    codEmpresa,
                    request);
        }

        private sealed class AutoVerCuentasSaldosTendenciaRequest :
            AutoVerCuentasSaldosCuentaQuery
        {
            public string auxiliar { get; set; } =
                string.Empty;
        }
    }
}