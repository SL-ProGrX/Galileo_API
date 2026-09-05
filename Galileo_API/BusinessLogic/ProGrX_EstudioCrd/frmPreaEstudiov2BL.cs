using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public class FrmPreaEstudiov2BL
    {
        private readonly FrmPreaEstudiov2DB _db;
        private const string MensajeCodigoExpedienteRequerido = "Debe indicar el código del expediente.";

        public FrmPreaEstudiov2BL(IConfiguration config)
        {
            _db = new FrmPreaEstudiov2DB(config);
        }

        /// <summary>
        /// Carga la información completa de un expediente de estudio de crédito.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2ScrollResponse> Prea_frmPreaEstudiov2_Scroll(
            int codEmpresa,
            FrmPreaEstudiov2ScrollRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2ScrollResponse>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = new FrmPreaEstudiov2ScrollResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Scroll(codEmpresa, request);
        }

        /// <summary>
        /// Recalcula Cuota/Pólizas/Compromiso. VB6: sbCalcularCuota, chkPolizaX_Click.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2CreditoRecalculoResponse> Prea_frmPreaEstudiov2_Credito_Recalcular(
            int codEmpresa,
            FrmPreaEstudiov2CreditoRecalcularRequest request)
        {
            if (request is null)
            {
                return new ErrorDto<FrmPreaEstudiov2CreditoRecalculoResponse>
                {
                    Code = -1,
                    Description = "Debe indicar la información del crédito.",
                    Result = new FrmPreaEstudiov2CreditoRecalculoResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Credito_Recalcular(codEmpresa, request);
        }

        /// <summary>
        /// Recarga destino/garantía en cascada cuando cambia la Línea seleccionada
        /// (equivalente a txtLinea_LostFocus en VB6).
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2DestinosGarantiasResponse> Prea_frmPreaEstudiov2_DestinosGarantias_Consultar(
            int codEmpresa, string linea)
        {
            return _db.Prea_frmPreaEstudiov2_DestinosGarantias_Consultar(codEmpresa, linea);
        }

        /// <summary>
        /// Catálogos estáticos del formulario (VB6: los combos que se llenan en Form_Load).
        /// Angular los pide una sola vez al abrir la pantalla.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2CatalogosResponse> Prea_frmPreaEstudiov2_Catalogos_Consultar(int codEmpresa)
        {
            return _db.Prea_frmPreaEstudiov2_Catalogos_Consultar(codEmpresa);
        }

        /// <summary>
        /// Detalle de la pestaña Salarios de un expediente, sin recargarlo completo.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2SalariosDto> Prea_frmPreaEstudiov2_Salarios_Consultar(
            int codEmpresa, string codPreanalisis)
        {
            if (string.IsNullOrWhiteSpace(codPreanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2SalariosDto>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = new FrmPreaEstudiov2SalariosDto()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Salarios_Consultar(codEmpresa, codPreanalisis);
        }

        /// <summary>
        /// Actualiza m_NumPagos cuando cambia la fecha de corte de colilla. VB6:
        /// dtpCorte_Change -&gt; sbNumPagos_Update -&gt; EXEC spCrd_Prea_NumPagos.
        /// </summary>
        public ErrorDto<int> Prea_frmPreaEstudiov2_NumPagos_Obtener(
            int codEmpresa, string cedula, DateTime fechaCorte)
        {
            return _db.Prea_frmPreaEstudiov2_NumPagos_Obtener(codEmpresa, cedula, fechaCorte);
        }

        /// <summary>
        /// Indica si el par Línea/Destino aplica Primera Cuota. VB6: sbAplicaPrimeraCta
        /// -&gt; EXEC spCRDPreaDestinos_TXAplicaPrimCta.
        /// </summary>
        public ErrorDto<bool> Prea_frmPreaEstudiov2_Destino_PrimeraCuota(
            int codEmpresa, string linea, string destino)
        {
            return _db.Prea_frmPreaEstudiov2_Destino_PrimeraCuota(codEmpresa, linea, destino);
        }

        /// <summary>
        /// Valida si la cédula requiere abrir Verificación de Datos Personales. VB6:
        /// txtCedula_LostFocus -> dbo.fxCrdPrea_Persona_Datos_Valida.
        /// </summary>
        public ErrorDto<int> Prea_frmPreaEstudiov2_Persona_ValidarDatos(int codEmpresa, string cedula)
        {
            return _db.Prea_frmPreaEstudiov2_Persona_ValidarDatos(codEmpresa, cedula);
        }

        /// <summary>
        /// Obtiene nombre, estado, edad y clasificación al confirmar la identificación.
        /// VB6: txtCedula_LostFocus.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2EncabezadoDto> Prea_frmPreaEstudiov2_Persona_Datos_Obtener(
            int codEmpresa,
            string cedula,
            string codPreanalisis,
            string estado,
            int plazo)
        {
            return _db.Prea_frmPreaEstudiov2_Persona_Datos_Obtener(codEmpresa, cedula, codPreanalisis, estado, plazo);
        }

        /// <summary>
        /// Calcula el Monto según el FORMULARIO de la Garantía (F01/F06). VB6: cboGarantia_Click.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2GarantiaMontoResponse> Prea_frmPreaEstudiov2_Garantia_Monto(
            int codEmpresa, FrmPreaEstudiov2GarantiaMontoRequest request)
        {
            return _db.Prea_frmPreaEstudiov2_Garantia_Monto(codEmpresa, request);
        }

        /// <summary>
        /// Calcula Monto/Tasa/Plazo disponibles de un Fondo de Ahorros (y su lista de
        /// contratos si es cambio de Fondo). VB6: cboFondo_Click / cboFondoContrato_Click.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2FondoCalcularResponse> Prea_frmPreaEstudiov2_Fondo_Calcular(
            int codEmpresa, FrmPreaEstudiov2FondoCalcularRequest request)
        {
            return _db.Prea_frmPreaEstudiov2_Fondo_Calcular(codEmpresa, request);
        }

        /// <summary>
        /// Lista de sub-expedientes (fiadores) ligados a un expediente principal.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2SubExpedientesResponse> Prea_frmPreaEstudiov2_SubExpedientes_Consultar(
            int codEmpresa, string expedientePadre)
        {
            return _db.Prea_frmPreaEstudiov2_SubExpedientes_Consultar(codEmpresa, expedientePadre);
        }

        /// <summary>
        /// VB6: fxExistenFiadores. Cuenta sub-expedientes (fiadores) registrados
        /// para un expediente principal.
        /// </summary>
        public ErrorDto<int> Prea_frmPreaEstudiov2_Fiadores_Contar(
            int codEmpresa, string expedientePadre)
        {
            return _db.Prea_frmPreaEstudiov2_Fiadores_Contar(codEmpresa, expedientePadre);
        }

        /// <summary>
        /// Genera/valida el número de un nuevo sub-expediente (fiador).
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2SubExpedienteGenerarResponse> Prea_frmPreaEstudiov2_SubExpediente_Generar(
            int codEmpresa, string expediente)
        {
            return _db.Prea_frmPreaEstudiov2_SubExpediente_Generar(codEmpresa, expediente);
        }

        /// <summary>
        /// Borra un expediente. Solo permitido en modo edición (equivalente a
        /// m_ventanaEnModo = ModificarRegistro en VB6) — validación replicada aquí.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2GuardarResponse> Prea_frmPreaEstudiov2_Borrar(
            int codEmpresa, string codPreanalisis, string codPreanalisisRef)
        {
            if (string.IsNullOrWhiteSpace(codPreanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2GuardarResponse>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = new FrmPreaEstudiov2GuardarResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Borrar(codEmpresa, codPreanalisis, codPreanalisisRef);
        }

        /// <summary>
        /// Guarda la grilla "Tabla de Salarios" (equivalente a sbSalarios_Guardar en VB6).
        /// </summary>
        public ErrorDto<List<FrmPreaEstudiov2SalarioDetalleDto>> Prea_frmPreaEstudiov2_TablaSalarios_Guardar(
            int codEmpresa, FrmPreaEstudiov2TablaSalariosGuardarRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<List<FrmPreaEstudiov2SalarioDetalleDto>>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = []
                };
            }

            return _db.Prea_frmPreaEstudiov2_TablaSalarios_Guardar(codEmpresa, request);
        }

        /// <summary>
        /// Cambia oficina y ejecutivo colocador (equivalente a btnOficinaCambia_Click en VB6).
        /// </summary>
        public ErrorDto<string> Prea_frmPreaEstudiov2_OficinaEjecutivo_Cambiar(
            int codEmpresa, FrmPreaEstudiov2OficinaEjecutivoCambiarRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<string>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = string.Empty
                };
            }

            return _db.Prea_frmPreaEstudiov2_OficinaEjecutivo_Cambiar(codEmpresa, request);
        }

        public ErrorDto<FrmPreaEstudiov2CargaResponse> Prea_frmPreaEstudiov2_Cargar(
            int codEmpresa,
            FrmPreaEstudiov2CargaRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2CargaResponse>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = new FrmPreaEstudiov2CargaResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Cargar(codEmpresa, request);
        }

        /// <summary>
        /// Suma el avalúo Factor CFIA del expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2HipotecarioResponse> Prea_frmPreaEstudiov2_Hipotecario_SumarAvaluoCfia(
            int codEmpresa,
            FrmPreaEstudiov2HipotecarioSumarAvaluoRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2HipotecarioResponse>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = new FrmPreaEstudiov2HipotecarioResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Hipotecario_SumarAvaluoCfia(codEmpresa, request);
        }

        /// <summary>
        /// Cambia el estado hipotecario del expediente (comité + estado hipotecario aprobado).
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2HipotecarioResponse> Prea_frmPreaEstudiov2_Hipotecario_CambiarEstado(
            int codEmpresa,
            FrmPreaEstudiov2HipotecarioCambiarEstadoRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2HipotecarioResponse>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = new FrmPreaEstudiov2HipotecarioResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Hipotecario_CambiarEstado(codEmpresa, request);
        }

        /// <summary>
        /// Cambia el expediente a estado abandonado.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2AbandonarResponse> Prea_frmPreaEstudiov2_Abandonar(
            int codEmpresa,
            FrmPreaEstudiov2AbandonarRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2AbandonarResponse>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = new FrmPreaEstudiov2AbandonarResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Abandonar(codEmpresa, request);
        }

        /// <summary>
        /// Guarda el preanálisis (nuevo o modificado).
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2GuardarResponse> Prea_frmPreaEstudiov2_Guardar(
            int codEmpresa,
            FrmPreaEstudiov2GuardarRequest request)
        {
            if (request is null)
            {
                return new ErrorDto<FrmPreaEstudiov2GuardarResponse>
                {
                    Code = -1,
                    Description = "Debe indicar la información del preanálisis.",
                    Result = new FrmPreaEstudiov2GuardarResponse()
                };
            }

            if (string.IsNullOrWhiteSpace(request.cedula))
            {
                return new ErrorDto<FrmPreaEstudiov2GuardarResponse>
                {
                    Code = -1,
                    Description = "Debe indicar la cédula del solicitante.",
                    Result = new FrmPreaEstudiov2GuardarResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Guardar(codEmpresa, request);
        }

        /// <summary>
        /// Consulta las deducciones del expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2DeduccionesResponse> Prea_frmPreaEstudiov2_Deducciones_Consultar(
            int codEmpresa,
            string cod_preanalisis)
        {
            if (string.IsNullOrWhiteSpace(cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2DeduccionesResponse>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = new FrmPreaEstudiov2DeduccionesResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Deducciones_Consultar(codEmpresa, cod_preanalisis);
        }

        /// <summary>
        /// Agrega una deducción al expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2DeduccionesResponse> Prea_frmPreaEstudiov2_Deducciones_Agregar(
            int codEmpresa,
            FrmPreaEstudiov2DeduccionesAgregarRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2DeduccionesResponse>
                {
                    Code = -1,
                    Description = "Debe indicar el expediente y la deducción.",
                    Result = new FrmPreaEstudiov2DeduccionesResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Deducciones_Agregar(codEmpresa, request);
        }

        /// <summary>
        /// Borra una deducción del expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2DeduccionesResponse> Prea_frmPreaEstudiov2_Deducciones_Borrar(
            int codEmpresa,
            FrmPreaEstudiov2DeduccionesBorrarRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis) || string.IsNullOrWhiteSpace(request.id_x))
            {
                return new ErrorDto<FrmPreaEstudiov2DeduccionesResponse>
                {
                    Code = -1,
                    Description = "Debe indicar el expediente y la deducción a borrar.",
                    Result = new FrmPreaEstudiov2DeduccionesResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Deducciones_Borrar(codEmpresa, request);
        }

        /// <summary>
        /// Consulta los créditos en tránsito del expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2CreditosResponse> Prea_frmPreaEstudiov2_Creditos_Consultar(
            int codEmpresa,
            string cod_preanalisis)
        {
            if (string.IsNullOrWhiteSpace(cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2CreditosResponse>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = new FrmPreaEstudiov2CreditosResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Creditos_Consultar(codEmpresa, cod_preanalisis);
        }

        /// <summary>
        /// Registra una cuota en tránsito (Cancelada o Por Cobrar).
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2CreditosResponse> Prea_frmPreaEstudiov2_Creditos_Registrar(
            int codEmpresa,
            FrmPreaEstudiov2CreditoTransitoRegistrarRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2CreditosResponse>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = new FrmPreaEstudiov2CreditosResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Creditos_Registrar(codEmpresa, request);
        }

        /// <summary>
        /// Elimina todas las cuotas en tránsito del tipo indicado (Cancelada o Por Cobrar).
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2CreditosResponse> Prea_frmPreaEstudiov2_Creditos_Borrar(
            int codEmpresa,
            FrmPreaEstudiov2CreditoTransitoBorrarRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2CreditosResponse>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = new FrmPreaEstudiov2CreditosResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Creditos_Borrar(codEmpresa, request);
        }

        /// <summary>
        /// Elimina una cuota en tránsito individual por id_solicitud.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2CreditosResponse> Prea_frmPreaEstudiov2_Creditos_BorrarFila(
            int codEmpresa,
            FrmPreaEstudiov2CreditoTransitoBorrarFilaRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2CreditosResponse>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = new FrmPreaEstudiov2CreditosResponse()
                };
            }

            if (request.id_solicitud <= 0)
            {
                return new ErrorDto<FrmPreaEstudiov2CreditosResponse>
                {
                    Code = -1,
                    Description = "Debe indicar la cuota en tránsito a eliminar.",
                    Result = new FrmPreaEstudiov2CreditosResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Creditos_BorrarFila(codEmpresa, request);
        }

        /// <summary>
        /// Consulta las refundiciones del expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2RefundicionesResponse> Prea_frmPreaEstudiov2_Refundiciones_Consultar(
            int codEmpresa,
            string cod_preanalisis,
            string cod_garantia,
            DateTime? fechaFormaliza = null)
        {
            if (string.IsNullOrWhiteSpace(cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2RefundicionesResponse>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = new FrmPreaEstudiov2RefundicionesResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Refundiciones_Consultar(codEmpresa, cod_preanalisis, cod_garantia, fechaFormaliza);
        }

        /// <summary>
        /// Actualiza (recalcula) las refundiciones del expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2RefundicionesResponse> Prea_frmPreaEstudiov2_Refundiciones_Actualizar(
            int codEmpresa,
            FrmPreaEstudiov2RefundicionesActualizarRequest request,
            string cod_garantia,
            DateTime? fechaFormaliza = null)
        {
            if (string.IsNullOrWhiteSpace(request?.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2RefundicionesResponse>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = new FrmPreaEstudiov2RefundicionesResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Refundiciones_Actualizar(codEmpresa, request, cod_garantia, fechaFormaliza);
        }

        /// <summary>
        /// Actualiza los checkboxes Aplica / Apl_Mora de una fila de refundición.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2RefundicionesResponse> Prea_frmPreaEstudiov2_Refundiciones_ToggleAplica(
            int codEmpresa,
            FrmPreaEstudiov2RefundicionToggleRequest request,
            string cod_garantia)
        {
            if (string.IsNullOrWhiteSpace(request?.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2RefundicionesResponse>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = new FrmPreaEstudiov2RefundicionesResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Refundiciones_ToggleAplica(codEmpresa, request, cod_garantia);
        }

        /// <summary>
        /// Consulta las fianzas del expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2FianzasResponse> Prea_frmPreaEstudiov2_Fianzas_Consultar(
            int codEmpresa,
            string cod_preanalisis)
        {
            if (string.IsNullOrWhiteSpace(cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2FianzasResponse>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = new FrmPreaEstudiov2FianzasResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Fianzas_Consultar(codEmpresa, cod_preanalisis);
        }

        /// <summary>
        /// Actualiza (recalcula) las fianzas del expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2FianzasResponse> Prea_frmPreaEstudiov2_Fianzas_Actualizar(
            int codEmpresa,
            FrmPreaEstudiov2FianzasActualizarRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2FianzasResponse>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = new FrmPreaEstudiov2FianzasResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Fianzas_Actualizar(codEmpresa, request);
        }

        /// <summary>
        /// Actualiza los checkboxes Aplica / Cancela_Mora de una fila de fianza.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2FianzasResponse> Prea_frmPreaEstudiov2_Fianzas_ToggleAplica(
            int codEmpresa,
            FrmPreaEstudiov2FianzaToggleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2FianzasResponse>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = new FrmPreaEstudiov2FianzasResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Fianzas_ToggleAplica(codEmpresa, request);
        }

        /// <summary>
        /// Consulta los desembolsos del expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2DesembolsosResponse> Prea_frmPreaEstudiov2_Desembolsos_Consultar(
            int codEmpresa,
            string cod_preanalisis,
            string usuario)
        {
            if (string.IsNullOrWhiteSpace(cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2DesembolsosResponse>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = new FrmPreaEstudiov2DesembolsosResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Desembolsos_Consultar(codEmpresa, cod_preanalisis, usuario);
        }

        /// <summary>
        /// Consulta acreedores/conceptos para la lista de selección del tab Desembolsos.
        /// </summary>
        public ErrorDto<List<FrmPreaEstudiov2DesembolsoAcreedorDto>> Prea_frmPreaEstudiov2_Desembolsos_Acreedores_Consultar(
            int codEmpresa,
            bool ordinario,
            string? filtro)
        {
            return _db.Prea_frmPreaEstudiov2_Desembolsos_Acreedores_Consultar(codEmpresa, ordinario, filtro);
        }

        /// <summary>
        /// Consulta cuentas bancarias para el tab Desembolsos.
        /// </summary>
        public ErrorDto<List<FrmPreaEstudiov2DropdownDto>> Prea_frmPreaEstudiov2_Desembolsos_Cuentas_Consultar(
            int codEmpresa,
            string identificacion,
            string banco)
        {
            return _db.Prea_frmPreaEstudiov2_Desembolsos_Cuentas_Consultar(codEmpresa, identificacion, banco);
        }

        /// <summary>
        /// Guarda un desembolso del expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2DesembolsosResponse> Prea_frmPreaEstudiov2_Desembolsos_Guardar(
            int codEmpresa,
            FrmPreaEstudiov2DesembolsoGuardarRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2DesembolsosResponse>
                {
                    Code = -1,
                    Description = "Debe indicar el expediente y el desembolso.",
                    Result = new FrmPreaEstudiov2DesembolsosResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Desembolsos_Guardar(codEmpresa, request);
        }

        /// <summary>
        /// Elimina un desembolso del expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2DesembolsosResponse> Prea_frmPreaEstudiov2_Desembolsos_Eliminar(
            int codEmpresa,
            string cod_preanalisis,
            int id_desembolso,
            string usuario)
        {
            if (string.IsNullOrWhiteSpace(cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2DesembolsosResponse>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = new FrmPreaEstudiov2DesembolsosResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Desembolsos_Eliminar(codEmpresa, cod_preanalisis, id_desembolso, usuario);
        }

        /// <summary>
        /// Consulta el historial del expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2HistorialResponse> Prea_frmPreaEstudiov2_Historial_Consultar(
            int codEmpresa,
            string cod_preanalisis)
        {
            if (string.IsNullOrWhiteSpace(cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2HistorialResponse>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = new FrmPreaEstudiov2HistorialResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Historial_Consultar(codEmpresa, cod_preanalisis);
        }

        /// <summary>
        /// Agrega una etiqueta de seguimiento con nota al expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2HistorialResponse> Prea_frmPreaEstudiov2_Etiqueta_Agregar(
            int codEmpresa,
            FrmPreaEstudiov2EtiquetaAgregarRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2HistorialResponse>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = new FrmPreaEstudiov2HistorialResponse()
                };
            }

            if (string.IsNullOrWhiteSpace(request.cod_etiqueta))
            {
                return new ErrorDto<FrmPreaEstudiov2HistorialResponse>
                {
                    Code = -1,
                    Description = "Debe seleccionar una etiqueta.",
                    Result = new FrmPreaEstudiov2HistorialResponse()
                };
            }

            if ((request.nota ?? string.Empty).Trim().Length < 50)
            {
                return new ErrorDto<FrmPreaEstudiov2HistorialResponse>
                {
                    Code = -1,
                    Description = "Indique una observación válida, tiene que ser de al menos 50 caracteres.",
                    Result = new FrmPreaEstudiov2HistorialResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Etiqueta_Agregar(codEmpresa, request);
        }

        /// <summary>
        /// Consulta los adjuntos del expediente.
        /// </summary>
        public ErrorDto<List<FrmPreaEstudiov2AdjuntoDto>> Prea_frmPreaEstudiov2_Adjuntos_Consultar(
            int codEmpresa,
            string cod_preanalisis)
        {
            if (string.IsNullOrWhiteSpace(cod_preanalisis))
            {
                return new ErrorDto<List<FrmPreaEstudiov2AdjuntoDto>>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = []
                };
            }

            return _db.Prea_frmPreaEstudiov2_Adjuntos_Consultar(codEmpresa, cod_preanalisis);
        }

        /// <summary>
        /// VB6: btnAdjunto_Guardar_Click. Guarda un archivo adjunto al expediente.
        /// </summary>
        public ErrorDto<string> Prea_frmPreaEstudiov2_Adjunto_Guardar(
            int codEmpresa,
            FrmPreaEstudiov2AdjuntoGuardarRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<string> { Code = -1, Description = MensajeCodigoExpedienteRequerido, Result = string.Empty };
            }

            if (string.IsNullOrWhiteSpace(request.nombre_archivo))
            {
                return new ErrorDto<string> { Code = -1, Description = "Debe indicar el nombre del archivo.", Result = string.Empty };
            }

            byte[] contenido;
            try
            {
                contenido = Convert.FromBase64String(request.contenido_base64 ?? string.Empty);
            }
            catch (FormatException)
            {
                return new ErrorDto<string> { Code = -1, Description = "El contenido del archivo no es válido.", Result = string.Empty };
            }

            if (contenido.Length == 0)
            {
                return new ErrorDto<string> { Code = -1, Description = "El archivo no contiene datos.", Result = string.Empty };
            }

            return _db.Prea_frmPreaEstudiov2_Adjunto_Guardar(
                codEmpresa,
                request.usuario,
                request.cod_preanalisis,
                request.nombre_archivo,
                contenido);
        }

        /// <summary>
        /// VB6: lswArchivos_DblClick. Obtiene el contenido binario de un adjunto.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2AdjuntoDescargaDto> Prea_frmPreaEstudiov2_Adjunto_Descargar(
            int codEmpresa,
            string cod_preanalisis,
            int id_adjunto)
        {
            if (string.IsNullOrWhiteSpace(cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2AdjuntoDescargaDto>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = new FrmPreaEstudiov2AdjuntoDescargaDto()
                };
            }

            if (id_adjunto <= 0)
            {
                return new ErrorDto<FrmPreaEstudiov2AdjuntoDescargaDto>
                {
                    Code = -1,
                    Description = "Debe indicar el adjunto a descargar.",
                    Result = new FrmPreaEstudiov2AdjuntoDescargaDto()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Adjunto_Descargar(codEmpresa, cod_preanalisis, id_adjunto);
        }

        /// <summary>
        /// VB6: btnAdjunto_Elimina_Click. Elimina un archivo adjunto del expediente.
        /// </summary>
        public ErrorDto<string> Prea_frmPreaEstudiov2_Adjunto_Eliminar(
            int codEmpresa,
            FrmPreaEstudiov2AdjuntoEliminarRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<string> { Code = -1, Description = MensajeCodigoExpedienteRequerido, Result = string.Empty };
            }

            var idsAdjuntos = ObtenerAdjuntosEliminar(request);
            if (idsAdjuntos.Count == 0)
            {
                return new ErrorDto<string> { Code = -1, Description = "Debe indicar los adjuntos a eliminar.", Result = string.Empty };
            }

            return _db.Prea_frmPreaEstudiov2_Adjunto_Eliminar(codEmpresa, request.cod_preanalisis, idsAdjuntos);
        }

        private static List<int> ObtenerAdjuntosEliminar(FrmPreaEstudiov2AdjuntoEliminarRequest request)
        {
            var ids = request.ids_adjuntos?
                .Where(id => id > 0)
                .Distinct()
                .ToList() ?? [];

            if (ids.Count == 0 && request.id_adjunto > 0)
            {
                ids.Add(request.id_adjunto);
            }

            return ids;
        }

        /// <summary>
        /// Consulta la resolución del expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2ResolucionResponse> Prea_frmPreaEstudiov2_Resolucion_Consultar(
            int codEmpresa,
            string cod_preanalisis,
            string tipo)
        {
            if (string.IsNullOrWhiteSpace(cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2ResolucionResponse>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = new FrmPreaEstudiov2ResolucionResponse()
                };
            }

            var tipoNormalizado = (tipo ?? string.Empty).Trim().ToUpperInvariant();
            if (tipoNormalizado is not ("RES" or "AUT" or "ASI"))
            {
                return new ErrorDto<FrmPreaEstudiov2ResolucionResponse>
                {
                    Code = -1,
                    Description = "El tipo de resolución indicado no es válido.",
                    Result = new FrmPreaEstudiov2ResolucionResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Resolucion_Consultar(codEmpresa, cod_preanalisis, tipoNormalizado);
        }

        /// <summary>
        /// Guarda observaciones del expediente.
        /// </summary>
        public ErrorDto<string> Prea_frmPreaEstudiov2_Observaciones_Guardar(
            int codEmpresa,
            FrmPreaEstudiov2ObservacionesRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<string>
                {
                    Code = -1,
                    Description = "Debe indicar el expediente y las observaciones.",
                    Result = string.Empty
                };
            }

            return _db.Prea_frmPreaEstudiov2_Observaciones_Guardar(codEmpresa, request);
        }

        /// <summary>
        /// Asigna comité resolutivo al expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2ComiteAsignarResponse> Prea_frmPreaEstudiov2_Comite_Asignar(
            int codEmpresa,
            FrmPreaEstudiov2ComiteAsignarRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2ComiteAsignarResponse>
                {
                    Code = -1,
                    Description = "Debe indicar el expediente y el comité.",
                    Result = new FrmPreaEstudiov2ComiteAsignarResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Comite_Asignar(codEmpresa, request);
        }

        /// <summary>
        /// Copia un expediente existente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2CopiarResponse> Prea_frmPreaEstudiov2_Copiar(
            int codEmpresa,
            FrmPreaEstudiov2CopiarRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis_origen))
            {
                return new ErrorDto<FrmPreaEstudiov2CopiarResponse>
                {
                    Code = -1,
                    Description = "Debe indicar el expediente origen.",
                    Result = new FrmPreaEstudiov2CopiarResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Copiar(codEmpresa, request);
        }

        /// <summary>
        /// Solicita el estado del expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2SolicitarResponse> Prea_frmPreaEstudiov2_Solicitar(
            int codEmpresa,
            FrmPreaEstudiov2SolicitarRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2SolicitarResponse>
                {
                    Code = -1,
                    Description = "Debe indicar el expediente.",
                    Result = new FrmPreaEstudiov2SolicitarResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Solicitar(codEmpresa, request);
        }

        /// <summary>
        /// Guarda la lista de incapacidades del expediente (sbIncapacidades_Guardar en VB6).
        /// </summary>
        public ErrorDto<List<FrmPreaEstudiov2IncapacidadDto>> Prea_frmPreaEstudiov2_Incapacidades_Guardar(
            int codEmpresa,
            FrmPreaEstudiov2IncapacidadGuardarRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<List<FrmPreaEstudiov2IncapacidadDto>>
                {
                    Code = -1,
                    Description = "Debe indicar el expediente y las incapacidades.",
                    Result = []
                };
            }

            return _db.Prea_frmPreaEstudiov2_Incapacidades_Guardar(codEmpresa, request);
        }

        /// <summary>
        /// Elimina incapacidades del expediente.
        /// </summary>
        public ErrorDto<string> Prea_frmPreaEstudiov2_Incapacidades_Eliminar(
            int codEmpresa,
            string cod_preanalisis)
        {
            if (string.IsNullOrWhiteSpace(cod_preanalisis))
            {
                return new ErrorDto<string>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = string.Empty
                };
            }

            return _db.Prea_frmPreaEstudiov2_Incapacidades_Eliminar(codEmpresa, cod_preanalisis);
        }

        /// <summary>
        /// Guarda un extra del expediente.
        /// </summary>
        public ErrorDto<string> Prea_frmPreaEstudiov2_Extras_Guardar(
            int codEmpresa,
            FrmPreaEstudiov2ExtraGuardarRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<string>
                {
                    Code = -1,
                    Description = "Debe indicar el expediente y el extra.",
                    Result = string.Empty
                };
            }

            return _db.Prea_frmPreaEstudiov2_Extras_Guardar(codEmpresa, request);
        }

        /// <summary>
        /// Elimina un extra del expediente.
        /// </summary>
        public ErrorDto<string> Prea_frmPreaEstudiov2_Extras_Borrar(
            int codEmpresa,
            FrmPreaEstudiov2ExtraBorrarRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<string>
                {
                    Code = -1,
                    Description = "Debe indicar el expediente y el extra.",
                    Result = string.Empty
                };
            }

            return _db.Prea_frmPreaEstudiov2_Extras_Borrar(codEmpresa, request);
        }

        /// <summary>
        /// Consulta las causas de seguimiento del expediente.
        /// </summary>
        public ErrorDto<List<FrmPreaEstudiov2CausaDto>> Prea_frmPreaEstudiov2_Causas_Consultar(
            int codEmpresa,
            string cod_preanalisis,
            string tipo)
        {
            if (string.IsNullOrWhiteSpace(cod_preanalisis))
            {
                return new ErrorDto<List<FrmPreaEstudiov2CausaDto>>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = []
                };
            }

            return _db.Prea_frmPreaEstudiov2_Causas_Consultar(codEmpresa, cod_preanalisis, tipo);
        }

        /// <summary>
        /// Consulta el tab Prendario: log de exámenes y datos de la prenda.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2PrendarioConsultarResponse> Prea_frmPreaEstudiov2_Prendario_Consultar(
            int codEmpresa,
            string cod_preanalisis)
        {
            if (string.IsNullOrWhiteSpace(cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2PrendarioConsultarResponse>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = new FrmPreaEstudiov2PrendarioConsultarResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Prendario_Consultar(codEmpresa, cod_preanalisis);
        }

        /// <summary>
        /// Aplica un estado a los exámenes de prenda del expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2PrendarioEstadoResponse> Prea_frmPreaEstudiov2_Prendario_Estado(
            int codEmpresa,
            FrmPreaEstudiov2PrendarioEstadoRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<FrmPreaEstudiov2PrendarioEstadoResponse>
                {
                    Code = -1,
                    Description = MensajeCodigoExpedienteRequerido,
                    Result = new FrmPreaEstudiov2PrendarioEstadoResponse()
                };
            }

            if (string.IsNullOrWhiteSpace(request.estado))
            {
                return new ErrorDto<FrmPreaEstudiov2PrendarioEstadoResponse>
                {
                    Code = -1,
                    Description = "Debe indicar el estado de los exámenes.",
                    Result = new FrmPreaEstudiov2PrendarioEstadoResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Prendario_Estado(codEmpresa, request);
        }
    }
}
