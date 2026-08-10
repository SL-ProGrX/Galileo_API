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
        /// Obtiene la información del tab Hipotecario.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2HipotecarioResponse> Prea_frmPreaEstudiov2_Hipotecario_Obtener(
            int codEmpresa,
            FrmPreaEstudiov2HipotecarioRequest request)
        {
            if (request is null)
            {
                return new ErrorDto<FrmPreaEstudiov2HipotecarioResponse>
                {
                    Code = -1,
                    Description = "Debe indicar la información del estudio.",
                    Result = new FrmPreaEstudiov2HipotecarioResponse()
                };
            }

            if (string.IsNullOrWhiteSpace(request.cod_preanalisis) && request.id_solicitud <= 0)
            {
                return new ErrorDto<FrmPreaEstudiov2HipotecarioResponse>
                {
                    Code = -1,
                    Description = "Debe indicar un expediente o una solicitud válida.",
                    Result = new FrmPreaEstudiov2HipotecarioResponse()
                };
            }

            return _db.Prea_frmPreaEstudiov2_Hipotecario_Obtener(codEmpresa, request);
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
        /// Consulta las refundiciones del expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2RefundicionesResponse> Prea_frmPreaEstudiov2_Refundiciones_Consultar(
            int codEmpresa,
            string cod_preanalisis)
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

            return _db.Prea_frmPreaEstudiov2_Refundiciones_Consultar(codEmpresa, cod_preanalisis);
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
        /// Consulta los desembolsos del expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2DesembolsosResponse> Prea_frmPreaEstudiov2_Desembolsos_Consultar(
            int codEmpresa,
            string cod_preanalisis)
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

            return _db.Prea_frmPreaEstudiov2_Desembolsos_Consultar(codEmpresa, cod_preanalisis);
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
            int id_desembolso)
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

            return _db.Prea_frmPreaEstudiov2_Desembolsos_Eliminar(codEmpresa, cod_preanalisis, id_desembolso);
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
        /// Consulta la resolución del expediente.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2ResolucionResponse> Prea_frmPreaEstudiov2_Resolucion_Consultar(
            int codEmpresa,
            string cod_preanalisis)
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

            return _db.Prea_frmPreaEstudiov2_Resolucion_Consultar(codEmpresa, cod_preanalisis);
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
        /// Guarda una incapacidad del expediente.
        /// </summary>
        public ErrorDto<string> Prea_frmPreaEstudiov2_Incapacidades_Guardar(
            int codEmpresa,
            FrmPreaEstudiov2IncapacidadGuardarRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<string>
                {
                    Code = -1,
                    Description = "Debe indicar el expediente y la incapacidad.",
                    Result = string.Empty
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
        /// Guarda observaciones de una causa del expediente.
        /// </summary>
        public ErrorDto<string> Prea_frmPreaEstudiov2_Causas_Guardar(
            int codEmpresa,
            FrmPreaEstudiov2CausasGuardarRequest request)
        {
            if (request is null || string.IsNullOrWhiteSpace(request.cod_preanalisis))
            {
                return new ErrorDto<string>
                {
                    Code = -1,
                    Description = "Debe indicar el expediente y la causa.",
                    Result = string.Empty
                };
            }

            return _db.Prea_frmPreaEstudiov2_Causas_Guardar(codEmpresa, request);
        }
    }
}
