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
        public ErrorDto<List<ExcPeriodosDto>> Patrimonio_frmAH_ExcedentesMensuales_Periodos_Lista(int codEmpresa)
        {
            return _db.Patrimonio_frmAH_ExcedentesMensuales_Periodos_Lista(codEmpresa);
        }

        /// <summary>
        /// Obtiene la lista de cortes del periodo.
        /// Método compartido entre tabs.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Patrimonio_frmAH_ExcedentesMensuales_Cortes_Lista(
            int codEmpresa,
            int periodoId)
        {
            return _db.Patrimonio_frmAH_ExcedentesMensuales_Cortes_Lista(codEmpresa, periodoId);
        }

        /// <summary>
        /// Obtiene el resumen de cortes del periodo para el tab Resumen.
        /// </summary>
        public ErrorDto<List<ResumenExcedenteMDto>> Patrimonio_frmAH_ExcedentesMensuales_Resumen_Lista(
            int codEmpresa,
            int periodoId)
        {
            return _db.Patrimonio_frmAH_ExcedentesMensuales_Resumen_Lista(codEmpresa, periodoId);
        }

        /// <summary>
        /// Obtiene el monto sugerido para la distribución mensual del periodo y corte indicados.
        /// </summary>
        public ErrorDto<decimal?> Patrimonio_frmAH_ExcedentesMensuales_Mensual_Monto_Obtener(
            int codEmpresa,
            int periodoId,
            DateTime corte,
            string tipoAplicacion)
        {
            return _db.Patrimonio_frmAH_ExcedentesMensuales_Mensual_Monto_Obtener(
                codEmpresa,
                periodoId,
                corte,
                tipoAplicacion);
        }

        /// <summary>
        /// Obtiene la utilidad contable del mes para base de aplicación real contable.
        /// </summary>
        public ErrorDto<decimal?> Patrimonio_frmAH_ExcedentesMensuales_Mensual_UtilidadContable_Obtener(
            int codEmpresa,
            int anio,
            int mes,
            int enlace)
        {
            return _db.Patrimonio_frmAH_ExcedentesMensuales_Mensual_UtilidadContable_Obtener(
                codEmpresa,
                anio,
                mes,
                enlace);
        }

        /// <summary>
        /// Actualiza el modo base de aplicación del periodo.
        /// </summary>
        public ErrorDto Patrimonio_frmAH_ExcedentesMensuales_Mensual_BaseAplicacion_Actualizar(
            int codEmpresa,
            int periodoId,
            string tipoAplicacion,
            string usuario)
        {
            return _db.Patrimonio_frmAH_ExcedentesMensuales_Mensual_BaseAplicacion_Actualizar(
                codEmpresa,
                periodoId,
                tipoAplicacion,
                usuario);
        }

        /// <summary>
        /// Valida si el corte ya fue aplicado para el periodo indicado.
        /// </summary>
        public ErrorDto<string?> Patrimonio_frmAH_ExcedentesMensuales_Mensual_Valida(
            int codEmpresa,
            int periodoId,
            DateTime corte)
        {
            return _db.Patrimonio_frmAH_ExcedentesMensuales_Mensual_Valida(codEmpresa, periodoId, corte);
        }

        /// <summary>
        /// Ejecuta la aplicación mensual de excedentes para el periodo y corte indicados.
        /// </summary>
        public ErrorDto Patrimonio_frmAH_ExcedentesMensuales_Mensual_Aplicar(
            int codEmpresa,
            int periodoId,
            DateTime corte,
            decimal monto,
            string tipoAplicacion,
            string usuario)
        {
            return _db.Patrimonio_frmAH_ExcedentesMensuales_Mensual_Aplicar(
                codEmpresa,
                periodoId,
                corte,
                monto,
                tipoAplicacion,
                usuario);
        }

        /// <summary>
        /// Obtiene la información base del periodo para el tab Cierre.
        /// </summary>
        public ErrorDto<ExcedentePeriodoDto?> Patrimonio_frmAH_ExcedentesMensuales_Cierre_Periodo_Obtener(
            int codEmpresa,
            int periodoId)
        {
            return _db.Patrimonio_frmAH_ExcedentesMensuales_Cierre_Periodo_Obtener(codEmpresa, periodoId);
        }

        /// <summary>
        /// Obtiene la tabla de renta utilizada por el cierre.
        /// </summary>
        public ErrorDto<List<RentaExcedenteDto>> Patrimonio_frmAH_ExcedentesMensuales_Cierre_Renta_Lista(int codEmpresa)
        {
            return _db.Patrimonio_frmAH_ExcedentesMensuales_Cierre_Renta_Lista(codEmpresa);
        }

        /// <summary>
        /// Valida si el periodo puede cerrarse.
        /// </summary>
        public ErrorDto<string?> Patrimonio_frmAH_ExcedentesMensuales_Cierre_Valida(
            int codEmpresa,
            int periodoId)
        {
            return _db.Patrimonio_frmAH_ExcedentesMensuales_Cierre_Valida(codEmpresa, periodoId);
        }

        /// <summary>
        /// Ejecuta el cierre de excedentes del periodo.
        /// </summary>
        public ErrorDto Patrimonio_frmAH_ExcedentesMensuales_Cierre_Aplicar(
            int codEmpresa,
            int periodoId,
            string usuario)
        {
            return _db.Patrimonio_frmAH_ExcedentesMensuales_Cierre_Aplicar(codEmpresa, periodoId, usuario);
        }

        /// <summary>
        /// Obtiene el último periodo cerrado para el tab Aplicaciones.
        /// </summary>
        public ErrorDto<ExcPeriodosDto?> Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_UltimoPeriodoCerrado_Obtener(int codEmpresa)
        {
            return _db.Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_UltimoPeriodoCerrado_Obtener(codEmpresa);
        }

        /// <summary>
        /// Obtiene la bitácora del periodo en etapa de cierre/aplicaciones.
        /// </summary>
        public ErrorDto<List<BitacoraExcedenteDto>> Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_Log_Lista(
            int codEmpresa,
            int periodoId)
        {
            return _db.Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_Log_Lista(codEmpresa, periodoId);
        }

        /// <summary>
        /// Obtiene la lista de procesos pendientes de aplicaciones para el periodo.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_ProcesosPendientes_Lista(
            int codEmpresa,
            int periodoId)
        {
            return _db.Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_ProcesosPendientes_Lista(codEmpresa, periodoId);
        }

        /// <summary>
        /// Ejecuta la separación de salidas del periodo.
        /// </summary>
        public ErrorDto Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_Salidas_Separa(
            int codEmpresa,
            int periodoId,
            string usuario)
        {
            return _db.Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_Salidas_Separa(codEmpresa, periodoId, usuario);
        }

        /// <summary>
        /// Obtiene las salidas pendientes de traslado a fondos del periodo.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_SalidasPendientes_Lista(
            int codEmpresa,
            int periodoId)
        {
            return _db.Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_SalidasPendientes_Lista(codEmpresa, periodoId);
        }

        /// <summary>
        /// Ejecuta el traslado a fondos de una salida del periodo.
        /// </summary>
        public ErrorDto Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_Salidas_Fondos(
            int codEmpresa,
            int periodoId,
            string salida,
            string usuario)
        {
            return _db.Patrimonio_frmAH_ExcedentesMensuales_Aplicaciones_Salidas_Fondos(
                codEmpresa,
                periodoId,
                salida,
                usuario);
        }

        /// <summary>
        /// Obtiene la lista de parámetros de excedentes.
        /// </summary>
        public ErrorDto<List<ExcParametrosDto>> Patrimonio_frmAH_ExcedentesMensuales_Parametros_Lista(int codEmpresa)
        {
            return _db.Patrimonio_frmAH_ExcedentesMensuales_Parametros_Lista(codEmpresa);
        }

        /// <summary>
        /// Valida si ya existe una bitácora previa para el proceso y detalle indicados.
        /// </summary>
        public ErrorDto<bool> Patrimonio_frmAH_ExcedentesMensuales_Bitacora_Valida(
            int codEmpresa,
            int periodoId,
            string codProceso,
            string detalle)
        {
            return _db.Patrimonio_frmAH_ExcedentesMensuales_Bitacora_Valida(
                codEmpresa,
                periodoId,
                codProceso,
                detalle);
        }

        /// <summary>
        /// Registra una línea de bitácora de excedentes.
        /// </summary>
        public ErrorDto Patrimonio_frmAH_ExcedentesMensuales_Bitacora_Registrar(
            int codEmpresa,
            int periodoId,
            string codProceso,
            string detalle,
            string usuario,
            string tipoDocumento = "",
            string codTransaccion = "")
        {
            return _db.Patrimonio_frmAH_ExcedentesMensuales_Bitacora_Registrar(
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
