using Galileo.Models;
using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Patrimonio;

namespace Galileo_API.BusinessLogic.ProGrX.Patrimonio
{
    public class FrmAhExcedentesMensualesBL
    {
        private readonly FrmAhExcedentesMensualesDB _db;

        public FrmAhExcedentesMensualesBL(IConfiguration config)
        {
            _db = new FrmAhExcedentesMensualesDB(config);
        }

        /// <summary>
        /// Obtiene los periodos de excedentes disponibles.
        /// Método compartido entre tabs.
        /// </summary>
        public ErrorDto<List<ExcPeriodosDto>> AH_ExcedentesMensuales_Periodos_Lista(int codEmpresa)
        {
            return _db.AH_ExcedentesMensuales_Periodos_Lista(codEmpresa);
        }

        /// <summary>
        /// Obtiene la lista de cortes del periodo.
        /// Método compartido entre tabs.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> AH_ExcedentesMensuales_Cortes_Lista(
            int codEmpresa,
            int periodoId)
        {
            return _db.AH_ExcedentesMensuales_Cortes_Lista(codEmpresa, periodoId);
        }

        /// <summary>
        /// Obtiene el resumen de cortes del periodo para el tab Resumen.
        /// </summary>
        public ErrorDto<List<ResumenExcedenteMDto>> AH_ExcedentesMensuales_Resumen_Lista(
            int codEmpresa,
            int periodoId)
        {
            return _db.AH_ExcedentesMensuales_Resumen_Lista(codEmpresa, periodoId);
        }

        /// <summary>
        /// Obtiene el monto sugerido para la distribución mensual del periodo y corte indicados.
        /// </summary>
        public ErrorDto<decimal?> AH_ExcedentesMensuales_Mensual_Monto_Obtener(
            int codEmpresa,
            int periodoId,
            DateTime corte,
            string tipoAplicacion)
        {
            return _db.AH_ExcedentesMensuales_Mensual_Monto_Obtener(
                codEmpresa,
                periodoId,
                corte,
                tipoAplicacion);
        }

        /// <summary>
        /// Obtiene la utilidad contable del mes para base de aplicación real contable.
        /// </summary>
        public ErrorDto<decimal?> AH_ExcedentesMensuales_Mensual_UtilidadContable_Obtener(
            int codEmpresa,
            int anio,
            int mes,
            int enlace)
        {
            return _db.AH_ExcedentesMensuales_Mensual_UtilidadContable_Obtener(
                codEmpresa,
                anio,
                mes,
                enlace);
        }

        /// <summary>
        /// Obtiene la configuración mensual del periodo seleccionado.
        /// </summary>
        public ErrorDto<FrmAhExcedentesMensualesMensualPeriodoDto?> AH_ExcedentesMensuales_Mensual_Periodo_Obtener(
            int codEmpresa,
            int periodoId)
        {
            return _db.AH_ExcedentesMensuales_Mensual_Periodo_Obtener(
                codEmpresa,
                periodoId);
        }

        /// <summary>
        /// Actualiza el modo base de aplicación del periodo.
        /// </summary>
        public ErrorDto AH_ExcedentesMensuales_Mensual_BaseAplicacion_Actualizar(
    int codEmpresa,
    FrmAhExcedentesMensualesBaseAplicacionRequest request)
        {
            return _db.AH_ExcedentesMensuales_Mensual_BaseAplicacion_Actualizar(
                codEmpresa,
                request.periodoId,
                request.tipoAplicacion,
                request.usuario);
        }

        /// <summary>
        /// Valida si el corte ya fue aplicado para el periodo indicado.
        /// </summary>
        public ErrorDto<string?> AH_ExcedentesMensuales_Mensual_Valida(
            int codEmpresa,
            int periodoId,
            DateTime corte)
        {
            return _db.AH_ExcedentesMensuales_Mensual_Valida(codEmpresa, periodoId, corte);
        }

        /// <summary>
        /// Ejecuta la aplicación mensual de excedentes para el periodo y corte indicados.
        /// </summary>
        public ErrorDto<FrmAhExcedentesMensualesMensualResultadoDto?> AH_ExcedentesMensuales_Mensual_Aplicar(
            int codEmpresa,
            FrmAhExcedentesMensualesMensualAplicarRequest request)
        {
            return _db.AH_ExcedentesMensuales_Mensual_Aplicar(
                codEmpresa,
                request.periodoId,
                request.corte,
                request.monto,
                request.tipoAplicacion,
                request.usuario);
        }

        /// <summary>
        /// Obtiene la información base del periodo para el tab Cierre.
        /// </summary>
        public ErrorDto<ExcedentePeriodoDto?> AH_ExcedentesMensuales_Cierre_Periodo_Obtener(
            int codEmpresa,
            int periodoId)
        {
            return _db.AH_ExcedentesMensuales_Cierre_Periodo_Obtener(codEmpresa, periodoId);
        }

        /// <summary>
        /// Obtiene la tabla de renta utilizada por el cierre.
        /// </summary>
        public ErrorDto<List<RentaExcedenteDto>> AH_ExcedentesMensuales_Cierre_Renta_Lista(int codEmpresa)
        {
            return _db.AH_ExcedentesMensuales_Cierre_Renta_Lista(codEmpresa);
        }

        /// <summary>
        /// Valida si el periodo puede cerrarse.
        /// </summary>
        public ErrorDto<string?> AH_ExcedentesMensuales_Cierre_Valida(
            int codEmpresa,
            int periodoId)
        {
            return _db.AH_ExcedentesMensuales_Cierre_Valida(codEmpresa, periodoId);
        }

        /// <summary>
        /// Ejecuta el cierre de excedentes del periodo.
        /// </summary>
        public ErrorDto AH_ExcedentesMensuales_Cierre_Aplicar(
    int codEmpresa,
    FrmAhExcedentesMensualesCierreAplicarRequest request)
        {
            return _db.AH_ExcedentesMensuales_Cierre_Aplicar(
                codEmpresa,
                request.periodoId,
                request.usuario);
        }

        /// <summary>
        /// Obtiene el último periodo cerrado para el tab Aplicaciones.
        /// </summary>
        public ErrorDto<ExcPeriodosDto?> AH_ExcedentesMensuales_Aplicaciones_UltimoPeriodoCerrado_Obtener(int codEmpresa)
        {
            return _db.AH_ExcedentesMensuales_Aplicaciones_UltimoPeriodoCerrado_Obtener(codEmpresa);
        }

        /// <summary>
        /// Obtiene la bitácora del periodo en etapa de cierre/aplicaciones.
        /// </summary>
        public ErrorDto<List<BitacoraExcedenteDto>> AH_ExcedentesMensuales_Aplicaciones_Log_Lista(
            int codEmpresa,
            int periodoId)
        {
            return _db.AH_ExcedentesMensuales_Aplicaciones_Log_Lista(codEmpresa, periodoId);
        }

        /// <summary>
        /// Obtiene la lista de procesos pendientes de aplicaciones para el periodo.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> AH_ExcedentesMensuales_Aplicaciones_ProcesosPendientes_Lista(
            int codEmpresa,
            int periodoId)
        {
            return _db.AH_ExcedentesMensuales_Aplicaciones_ProcesosPendientes_Lista(codEmpresa, periodoId);
        }

        /// <summary>
        /// Ejecuta la separación de salidas del periodo.
        /// </summary>
        public ErrorDto AH_ExcedentesMensuales_Aplicaciones_Salidas_Separa(
     int codEmpresa,
     FrmAhExcedentesMensualesSalidasSeparaRequest request)
        {
            return _db.AH_ExcedentesMensuales_Aplicaciones_Salidas_Separa(
                codEmpresa,
                request.periodoId,
                request.usuario);
        }

        /// <summary>
        /// Obtiene las salidas pendientes de traslado a fondos del periodo.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> AH_ExcedentesMensuales_Aplicaciones_SalidasPendientes_Lista(
            int codEmpresa,
            int periodoId)
        {
            return _db.AH_ExcedentesMensuales_Aplicaciones_SalidasPendientes_Lista(codEmpresa, periodoId);
        }

        /// <summary>
        /// Ejecuta el traslado a fondos de una salida del periodo.
        /// </summary>
        public ErrorDto AH_ExcedentesMensuales_Aplicaciones_Salidas_Fondos(
    int codEmpresa,
    FrmAhExcedentesMensualesSalidasFondosRequest request)
        {
            return _db.AH_ExcedentesMensuales_Aplicaciones_Salidas_Fondos(
                codEmpresa,
                request.periodoId,
                request.salida,
                request.usuario);
        }

        /// <summary>
        /// Obtiene la lista de parámetros de excedentes.
        /// </summary>
        public ErrorDto<List<ExcParametrosDto>> AH_ExcedentesMensuales_Parametros_Lista(int codEmpresa)
        {
            return _db.AH_ExcedentesMensuales_Parametros_Lista(codEmpresa);
        }

        /// <summary>
        /// Valida si ya existe una bitácora previa para el proceso y detalle indicados.
        /// </summary>
        public ErrorDto<bool> AH_ExcedentesMensuales_Bitacora_Valida(
            int codEmpresa,
            int periodoId,
            string codProceso,
            string detalle)
        {
            return _db.AH_ExcedentesMensuales_Bitacora_Valida(
                codEmpresa,
                periodoId,
                codProceso,
                detalle);
        }

        /// <summary>
        /// Registra una línea de bitácora de excedentes.
        /// </summary>
        public ErrorDto AH_ExcedentesMensuales_Bitacora_Registrar(
    int codEmpresa,
    FrmAhExcedentesMensualesBitacoraRegistrarRequest request)
        {
            return _db.AH_ExcedentesMensuales_Bitacora_Registrar(
                codEmpresa,
                request.periodoId,
                request.codProceso,
                request.detalle,
                request.usuario,
                request.tipoDocumento,
                request.codTransaccion);
        }

        public ErrorDto<FrmAhExcedentesMensualesAplicacionProcesoResponse?> AH_ExcedentesMensuales_Aplicaciones_Proceso_Ejecutar(
    int codEmpresa,
    FrmAhExcedentesMensualesAplicacionProcesoRequest request)
        {
            return _db.AH_ExcedentesMensuales_Aplicaciones_Proceso_Ejecutar(
                codEmpresa,
                request);
        }
    }
}
