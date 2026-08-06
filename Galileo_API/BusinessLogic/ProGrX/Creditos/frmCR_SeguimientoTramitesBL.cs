using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrSeguimientoTramitesBl
    {
        private readonly FrmCrSeguimientoTramitesDb _db;

        public FrmCrSeguimientoTramitesBl(IConfiguration config)
        {
            _db = new FrmCrSeguimientoTramitesDb(config);
        }

        public ErrorDto<CrSeguimientoTramitesInicializarData> Cr_SeguimientoTramites_Inicializar(
            int codEmpresa,
            string usuario)
            => _db.Cr_SeguimientoTramites_Inicializar(codEmpresa, usuario);

        public ErrorDto<List<CrSeguimientoTramitesBusquedaItem>> Cr_SeguimientoTramites_Buscar(
            int codEmpresa,
            string? cedula,
            string? nombre)
            => _db.Cr_SeguimientoTramites_Buscar(codEmpresa, cedula, nombre);

        public ErrorDto<CrSeguimientoTramitesOperacionData> Cr_SeguimientoTramites_Operacion_Obtener(
            int codEmpresa,
            int operacion)
            => _db.Cr_SeguimientoTramites_Operacion_Obtener(codEmpresa, operacion);

        /// <summary>
        /// Buscador paginado de operaciones para la ventana de búsqueda.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesOperacionBusquedaLista>
            Cr_SeguimientoTramites_Operaciones_Buscar(
                int codEmpresa,
                FiltrosLazyLoadData? filtros)
            => _db.Cr_SeguimientoTramites_Operaciones_Buscar(codEmpresa, filtros);

        /// <summary>
        /// Obtiene la operación anterior o siguiente respecto de la actual.
        /// </summary>
        public ErrorDto<int> Cr_SeguimientoTramites_Operacion_Navegar(
            int codEmpresa,
            int operacion,
            string direccion)
            => _db.Cr_SeguimientoTramites_Operacion_Navegar(codEmpresa, operacion, direccion);

        public ErrorDto<CrSeguimientoTramitesRecepcionGuardarResult>
            Cr_SeguimientoTramites_Recepcion_Guardar(
                int codEmpresa,
                CrSeguimientoTramitesRecepcionGuardarRequest request)
            => _db.Cr_SeguimientoTramites_Recepcion_Guardar(codEmpresa, request);

        /// <summary>
        /// Busca socios disponibles para la recepción de trámites.
        /// </summary>
        public ErrorDto<List<CrSeguimientoTramitesRecepcionSocioItem>>
            Cr_SeguimientoTramites_Recepcion_Socios_Buscar(
                int codEmpresa,
                string? filtro)
            => _db.Cr_SeguimientoTramites_Recepcion_Socios_Buscar(codEmpresa, filtro);

        /// <summary>
        /// Busca líneas de crédito disponibles para la recepción de trámites.
        /// </summary>
        public ErrorDto<List<CrSeguimientoTramitesRecepcionLineaItem>>
            Cr_SeguimientoTramites_Recepcion_Lineas_Buscar(
                int codEmpresa,
                string? filtro)
            => _db.Cr_SeguimientoTramites_Recepcion_Lineas_Buscar(codEmpresa, filtro);

        /// <summary>
        /// Busca promotores disponibles para la recepción de trámites.
        /// </summary>
        public ErrorDto<List<CrSeguimientoTramitesRecepcionPromotorItem>>
            Cr_SeguimientoTramites_Recepcion_Promotores_Buscar(
                int codEmpresa,
                string? filtro)
            => _db.Cr_SeguimientoTramites_Recepcion_Promotores_Buscar(codEmpresa, filtro);

        /// <summary>
        /// Busca proveedores disponibles para la recepción de trámites.
        /// </summary>
        public ErrorDto<List<CrSeguimientoTramitesRecepcionProveedorItem>>
            Cr_SeguimientoTramites_Recepcion_Proveedores_Buscar(
                int codEmpresa,
                string? filtro)
            => _db.Cr_SeguimientoTramites_Recepcion_Proveedores_Buscar(codEmpresa, filtro);

        /// <summary>
        /// Obtiene el contexto dependiente de persona y línea de crédito.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesRecepcionLineaContextoData>
            Cr_SeguimientoTramites_Recepcion_Linea_Contexto_Obtener(
                int codEmpresa,
                CrSeguimientoTramitesRecepcionLineaContextoRequest request)
            => _db.Cr_SeguimientoTramites_Recepcion_Linea_Contexto_Obtener(codEmpresa, request);

        /// <summary>
        /// Obtiene cálculos y reglas dependientes de la garantía.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesRecepcionGarantiaContextoData>
            Cr_SeguimientoTramites_Recepcion_Garantia_Contexto_Obtener(
                int codEmpresa,
                CrSeguimientoTramitesRecepcionGarantiaContextoRequest request)
            => _db.Cr_SeguimientoTramites_Recepcion_Garantia_Contexto_Obtener(codEmpresa, request);

        /// <summary>
        /// Obtiene las cuentas bancarias de la persona para el banco seleccionado.
        /// </summary>
        public ErrorDto<List<CrSeguimientoTramitesOpcionItem>>
            Cr_SeguimientoTramites_Recepcion_Banco_Cuentas_Obtener(
                int codEmpresa,
                CrSeguimientoTramitesRecepcionBancoCuentasRequest request)
            => _db.Cr_SeguimientoTramites_Recepcion_Banco_Cuentas_Obtener(codEmpresa, request);

        /// <summary>
        /// Obtiene contratos y cálculos del fondo de garantía.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesRecepcionFondoContextoData>
            Cr_SeguimientoTramites_Recepcion_Fondo_Contexto_Obtener(
                int codEmpresa,
                CrSeguimientoTramitesRecepcionFondoContextoRequest request)
            => _db.Cr_SeguimientoTramites_Recepcion_Fondo_Contexto_Obtener(codEmpresa, request);

        /// <summary>
        /// Obtiene la frecuencia de pago y la primer deducción sugerida de la deductora.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesFormalizacionDeductoraContextoData>
            Cr_SeguimientoTramites_Formalizacion_Deductora_Contexto_Obtener(
                int codEmpresa,
                CrSeguimientoTramitesFormalizacionDeductoraContextoRequest request)
            => _db.Cr_SeguimientoTramites_Formalizacion_Deductora_Contexto_Obtener(
                codEmpresa,
                request);

        /// <summary>
        /// Obtiene el disponible del recurso a la fecha de desembolso indicada.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesFormalizacionRecursoDisponibleData>
            Cr_SeguimientoTramites_Formalizacion_Recurso_Disponible_Obtener(
                int codEmpresa,
                CrSeguimientoTramitesFormalizacionRecursoDisponibleRequest request)
            => _db.Cr_SeguimientoTramites_Formalizacion_Recurso_Disponible_Obtener(
                codEmpresa,
                request);

        /// <summary>
        /// Obtiene los pasos previos a la formalización que requieren interacción del usuario.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesFormalizacionPrevalidacionData>
            Cr_SeguimientoTramites_Formalizacion_Prevalidacion_Obtener(
                int codEmpresa,
                CrSeguimientoTramitesFormalizacionPrevalidacionRequest request)
            => _db.Cr_SeguimientoTramites_Formalizacion_Prevalidacion_Obtener(codEmpresa, request);

        /// <summary>
        /// Recalcula el monto del crédito a partir de rebajos, intereses, póliza y cargos.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesRecepcionMontoCalculadoData>
            Cr_SeguimientoTramites_Recepcion_Monto_Calcular(
                int codEmpresa,
                CrSeguimientoTramitesRecepcionMontoCalcularRequest request)
            => _db.Cr_SeguimientoTramites_Recepcion_Monto_Calcular(codEmpresa, request);

        /// <summary>
        /// Actualiza el monto no gravable de la operación.
        /// </summary>
        public ErrorDto Cr_SeguimientoTramites_MontoNoGravable_Actualizar(
            int codEmpresa,
            CrSeguimientoTramitesMontoNoGravableRequest request)
            => _db.Cr_SeguimientoTramites_MontoNoGravable_Actualizar(codEmpresa, request);

        /// <summary>
        /// Obtiene las operaciones a refundir de la formalización.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesFormalizacionDetalleLista<
            CrSeguimientoTramitesFormalizacionRefundicionItem>>
            Cr_SeguimientoTramites_Formalizacion_Refundiciones_Obtener(
                int codEmpresa,
                int operacion)
            => _db.Cr_SeguimientoTramites_Formalizacion_Refundiciones_Obtener(codEmpresa, operacion);

        /// <summary>
        /// Obtiene los desembolsos y rebajos de la formalización.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesFormalizacionDetalleLista<
            CrSeguimientoTramitesFormalizacionDesembolsoItem>>
            Cr_SeguimientoTramites_Formalizacion_Desembolsos_Obtener(
                int codEmpresa,
                int operacion)
            => _db.Cr_SeguimientoTramites_Formalizacion_Desembolsos_Obtener(codEmpresa, operacion);

        /// <summary>
        /// Obtiene las retenciones a refundir de la formalización.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesFormalizacionDetalleLista<
            CrSeguimientoTramitesFormalizacionRefundeRetencionItem>>
            Cr_SeguimientoTramites_Formalizacion_RefundeRetenciones_Obtener(
                int codEmpresa,
                int operacion)
            => _db.Cr_SeguimientoTramites_Formalizacion_RefundeRetenciones_Obtener(
                codEmpresa,
                operacion);

        /// <summary>
        /// Obtiene el deudor y fiadores con su estado de firma.
        /// </summary>
        public ErrorDto<List<CrSeguimientoTramitesFormalizacionFirmaItem>>
            Cr_SeguimientoTramites_Formalizacion_Firmas_Obtener(
                int codEmpresa,
                int operacion)
            => _db.Cr_SeguimientoTramites_Formalizacion_Firmas_Obtener(codEmpresa, operacion);

        /// <summary>
        /// Obtiene los requisitos de la operación.
        /// </summary>
        public ErrorDto<List<CrSeguimientoTramitesFormalizacionRequisitoItem>>
            Cr_SeguimientoTramites_Formalizacion_Requisitos_Obtener(
                int codEmpresa,
                int operacion)
            => _db.Cr_SeguimientoTramites_Formalizacion_Requisitos_Obtener(codEmpresa, operacion);

        /// <summary>
        /// Obtiene los cargos adicionales de la operación.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesFormalizacionDetalleLista<
            CrSeguimientoTramitesFormalizacionCargoItem>>
            Cr_SeguimientoTramites_Formalizacion_Cargos_Obtener(
                int codEmpresa,
                int operacion)
            => _db.Cr_SeguimientoTramites_Formalizacion_Cargos_Obtener(codEmpresa, operacion);

        /// <summary>
        /// Obtiene los fiadores y co-deudores de la operación.
        /// </summary>
        public ErrorDto<List<CrSeguimientoTramitesFormalizacionFiadorItem>>
            Cr_SeguimientoTramites_Formalizacion_Fiadores_Obtener(
                int codEmpresa,
                int operacion)
            => _db.Cr_SeguimientoTramites_Formalizacion_Fiadores_Obtener(codEmpresa, operacion);

        /// <summary>
        /// Obtiene el impacto en liquidez de la operación.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesFormalizacionImpactoLiquidezData>
            Cr_SeguimientoTramites_Formalizacion_ImpactoLiquidez_Obtener(
                int codEmpresa,
                int operacion)
            => _db.Cr_SeguimientoTramites_Formalizacion_ImpactoLiquidez_Obtener(
                codEmpresa,
                operacion);

        /// <summary>
        /// Registra o retira la firma del deudor o de un fiador.
        /// </summary>
        public ErrorDto Cr_SeguimientoTramites_Formalizacion_Firma_Actualizar(
            int codEmpresa,
            CrSeguimientoTramitesFormalizacionFirmaActualizarRequest request)
            => _db.Cr_SeguimientoTramites_Formalizacion_Firma_Actualizar(codEmpresa, request);

        /// <summary>
        /// Obtiene el resumen de la operación con el detalle de rebajos y el monto a girar.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesFormalizacionResumenData>
            Cr_SeguimientoTramites_Formalizacion_Resumen_Obtener(
                int codEmpresa,
                CrSeguimientoTramitesFormalizacionResumenRequest request)
            => _db.Cr_SeguimientoTramites_Formalizacion_Resumen_Obtener(codEmpresa, request);

        /// <summary>
        /// Valida y aplica la formalización de la operación.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesFormalizacionResult>
            Cr_SeguimientoTramites_Formalizacion_Aplicar(
                int codEmpresa,
                CrSeguimientoTramitesFormalizacionAplicarRequest request)
            => _db.Cr_SeguimientoTramites_Formalizacion_Aplicar(codEmpresa, request);

        /// <summary>
        /// Valida y anula la formalización de la operación.
        /// </summary>
        public ErrorDto<CrSeguimientoTramitesFormalizacionResult>
            Cr_SeguimientoTramites_Formalizacion_Anular(
                int codEmpresa,
                CrSeguimientoTramitesFormalizacionAnularRequest request)
            => _db.Cr_SeguimientoTramites_Formalizacion_Anular(codEmpresa, request);

        /// <summary>
        /// Actualiza la fecha de formalización y la fecha de desembolso de la operación.
        /// </summary>
        public ErrorDto Cr_SeguimientoTramites_Formalizacion_Fechas_Actualizar(
            int codEmpresa,
            CrSeguimientoTramitesFormalizacionFechasRequest request)
            => _db.Cr_SeguimientoTramites_Formalizacion_Fechas_Actualizar(codEmpresa, request);

        /// <summary>
        /// Actualiza los indicadores de primer cuota y traslado de salario de la operación.
        /// </summary>
        public ErrorDto Cr_SeguimientoTramites_Formalizacion_Indicadores_Actualizar(
            int codEmpresa,
            CrSeguimientoTramitesFormalizacionIndicadoresRequest request)
            => _db.Cr_SeguimientoTramites_Formalizacion_Indicadores_Actualizar(codEmpresa, request);
    }
}
