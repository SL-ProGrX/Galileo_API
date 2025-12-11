    using CoreInterno;
    using Sinpe_CCD;
    using Sinpe_PIN;
    using System.ComponentModel;


    namespace Galileo.Models.KindoSinpe
    {
    #pragma warning disable S101
    #pragma warning disable S2342
        public class ClienteDataService
        {
            public string id_empresa { get; set; }
            public string clase_verificador { get; set; }
        }

        #region Tipos de datos
        //Tipos de datos utilizados seccion 9 de Interfaces de integración con los Sistemas Internos de su Entidad
        public enum E_Resultado
        {
            [Description("Ejecución realizada exitosamente.")]
            Exitoso = 0,
            [Description("La solicitud fue rechazada por el sistema interno.")]
            Rechazo = 1,
            [Description("Se registró un error durante el procesamiento.")]
            Error = 2
        }

        public enum E_Estado
        {
            [Description("Cuenta Cliente se encuentra activa.")]
            Activa = 0,

            [Description("Cuenta Cliente se encuentra cerrada.")]
            Cerrada = 1,

            [Description("Cuenta Cliente se encuentra bloqueada.")]
            Bloqueada = 2,

            [Description("Cuenta Cliente se encuentra no activa.")]
            NoActiva = 3
        }

        public enum E_ResultadoActualizacion
        {
            [Description("Actualización de la transacción exitosa.")]
            Exitoso = 0,

            [Description("La transacción a actualizar no se encuentra registrada.")]
            NoExiste = 1,

            [Description("Los datos enviados a actualizar no coinciden con los registrados para la transacción.")]
            NoPermitido = 2,

            [Description("Se presentó un error no controlado en su implementación.")]
            Error = 3
        }

        public enum E_Preferencia
        {
            [Description("Notificar vía correo electrónico.")]
            Correo = 1,

            [Description("Notificar vía mensaje de texto SMS.")]
            SMS = 2,

            [Description("Notificar vía mensaje de texto SMS y correo electrónico.")]
            Ambos = 3,

            [Description("No realizar ninguna notificación al cliente.")]
            Ninguno = 4
        }

        public enum E_MovimientosPermitidosProducto
        {
            [Description("Únicamente permite acreditaciones.")]
            Acreditar = 0,

            [Description("Únicamente permite débitos.")]
            Debitar = 1,

            [Description("Permite débitos y créditos.")]
            Ambos = 2,

            [Description("No permite débitos ni créditos.")]
            Ninguno = 3
        }
        #endregion

        #region Estados

        public enum E_EstadosIBAN
        {
            [Description("Activa")]
            Activa = 1,
            [Description("Cerrada")]
            Cerrada = 23,
            [Description("No Existe")]
            No_Existe = 28,
            [Description("Bloqueada")]
            Bloqueada = 31
        }

        public static class E_CodigoMonedaISO4217
        {
            public const string Colones = "CRC";
            public const string Dolares = "USD";
            public const string Euros = "EUR";
        }

        public enum E_CodigoServicioSINPE
        {
            [Description("Pagos Inmediatos (PIN)")]
            Pago_Inmediatos = 22
        }

        public enum E_ModalidadTransaccionSINPE
        {
            Saliente = 1,
            Entrante = 2
        }

        public static class E_TipoMovimiento
        {
            public const string Débito = "DB";
            public const string Crédito = "CR";
        }

        public enum E_EstadoAuthorizationSINPE
        {
            Autorizada = 1,
            Autorizada_Fondos_Congelados = 2,
            Rechazada = 3
        }

        #endregion

        public class ValidaTransRequest
        {
            public SI_Rastro Rastro { get; set; }
            public CL_DatosTransaccion[] Transacciones { get; set; }
        }

        public class ValidaActualizaTransaccionRequest
        {
            public SI_Rastro Rastro { get; set; }
            public CL_ActualizaTransaccion[] Transacciones { get; set; }
        }

        public class ValidaTransaccionRequest
        {
            public SI_Rastro Rastro { get; set; }
            public CL_Transaccion[] Transacciones { get; set; }
        }

        public class ValidaTransaccionRechazoRequest
        {
            public SI_Rastro Rastro { get; set; }
            public TransaccionRechazada[] Transacciones { get; set; }
        }

        public class CL_ActualizaFechaRequest
        {
            public int? ComprobanteCGP { get; set; }
            public string? DocumentoSistemaInterno { get; set; }
            public int? ServicioSINPE { get; set; }
            public DateTime? FechaCiclo { get; set; }
            public string? CodigoReferenciaAnterior { get; set; }
            public string? CodigoReferenciaNuevo { get; set; }
        }

        public class CLCierraCiclo
        {
            public int?[] EntidadesAplazadas { get; set; }
            public int? ServicioSINPE { get; set; }
            public string? Modalidad { get; set; }
            public DateTime? FechaCiclo { get; set; }
        }

        #region Clases DTR

        #region 7.1.1 ReqBase
        /// <summary>
        /// Clase base que transporta los datos comunes requeridos en todos los request de los métodos DTR.
        /// Todas las clases request de DTR deben heredar de esta clase.
        /// </summary>
        public class ReqBase
        {
            public string HostId { get; set; }            // Máx. 128 caracteres
            public string OperationId { get; set; }       // Opcional
            public string ClientIPAddress { get; set; }   // Opcional, máx. 40 caracteres
            public string CultureCode { get; set; }       // 5 caracteres (ej: "ES-CR")
            public string UserCode { get; set; }          // Opcional, máx. 50 caracteres
        }

        public class ParametrosSinpe
        {
            public string vHost { get; set; } = Environment.MachineName;
            public string vHostPin { get; set; }
            public string vIpHost { get; set; }
            public string vUserCGP { get; set; }
            public int vCanalCGP { get; set; } = 5;

            public string? UrlCGP_DTR { get; set; }
            public string? UrlCGP_PIN { get; set; }

            public string ServiciosSinpe { get; set; }

            public string vUsuarioLog { get; set; }
        }
        #endregion

        #region 7.1.2 ResBase
        /// <summary>
        /// Clase base que transporta los datos comunes requeridos en todas las respuestas (response)
        /// de los métodos DTR. Todas las clases de respuesta deben heredar de esta clase.
        /// </summary>
        public class ResBase
        {
            public bool IsSuccessful { get; set; }
            public string OperationId { get; set; }
            public Error[] Errors { get; set; }
        }
        #endregion

        #region 7.1.3 CustomField
        /// <summary>
        /// Clase para transportar datos de campos adicionales personalizados.
        /// </summary>
        public class CustomField
        {
            public string Name { get; set; }   // Máx. 50 caracteres
            public string Value { get; set; }  // Máx. 255 caracteres
        }
        #endregion

        #region 7.1.4 Error
        /// <summary>
        /// Clase para transportar el detalle de un error ocurrido durante el procesamiento de una solicitud DTR.
        /// </summary>
        public class Error
        {
            public int Code { get; set; }
            public int Type { get; set; }      // Según codificación sección 8.4
            public string Message { get; set; } // Máx. 255 caracteres
        }
        #endregion

        #region 7.1.5 OriginCustomer
        /// <summary>
        /// Datos personales y financieros del cliente origen.
        /// </summary>
        public class OriginCustomer
        {
            public string Id { get; set; }                 // 5–30 caracteres
            public string Name { get; set; }               // 8–150 caracteres
            public string IBAN1 { get; set; }              // Opcional, 22 caracteres
            public bool CreditIBAN { get; set; }           // True = acredita cuenta
            public string Email { get; set; }              // Opcional, 320 caracteres
        }
        #endregion

        #region 7.1.6 DestinationCustomer
        /// <summary>
        /// Datos personales y financieros del cliente destino.
        /// </summary>
        public class DestinationCustomer
        {
            public string Id { get; set; }                 // 5–30 caracteres
            public string Name { get; set; }               // 8–150 caracteres
            public string IBAN { get; set; }               // 22 caracteres
        }
        #endregion

        #region 7.1.7 Account
        /// <summary>
        /// Datos básicos de una cuenta.
        /// </summary>
        public class Account
        {
            public string AccountNumber { get; set; }      // 14–34 caracteres
            public string HolderId { get; set; }           // 30 caracteres
            public int HolderIdType { get; set; }          // según sección 8.1
            public string Holder { get; set; }             // 100 caracteres
            public string CurrencyCode { get; set; }       // 3 caracteres (ISO 4217)
            public string EntityCode { get; set; }         // 4 caracteres
            public string EntityName { get; set; }         // 255 caracteres
            public int RejectCode { get; set; }
            public string RejectDescription { get; set; }  // 255 caracteres
        }
        #endregion

        #region 7.1.8 DTR
        /// <summary>
        /// Datos de un Débito en Tiempo Real (DTR).
        /// </summary>
        public class DTR
        {
            public string ChannelRefNumber { get; set; }      // 1–25 dígitos
            public OriginCustomer OriginCustomer { get; set; }
            public DestinationCustomer DestinationCustomer { get; set; }
            public string Service { get; set; }               // 1–20 caracteres
            public decimal Amount { get; set; }               // > 0.00
            public string CurrencyCode { get; set; }          // 3 caracteres
            public byte[] SignedDocument { get; set; }        // opcional
            public string Description { get; set; }           // 15–255 caracteres
        }
        #endregion

        #region 7.1.9 DTRInfo
        /// <summary>
        /// Datos y resultado de envío de un DTR.
        /// </summary>
        public class DTRInfo
        {
            public short Type { get; set; }
            public DTR DebitData { get; set; }
            public DTRSendingResult DebitResult { get; set; }
        }
        #endregion

        #region 7.1.10 DebitData
        /// <summary>
        /// Datos detallados de un débito.
        /// </summary>
        public class DebitData
        {
            public string ChannelRefNumber { get; set; }
            public string SINPERefNumber { get; set; }
            public int Type { get; set; }
            public OriginCustomer OriginCustomer { get; set; }
            public DestinationCustomer DestinationCustomer { get; set; }
            public string Service { get; set; }
            public decimal Amount { get; set; }
            public string CurrencyCode { get; set; }
            public string Description { get; set; }
            public string State { get; set; }
            public short RejectCode { get; set; }
            public string RejectDescription { get; set; }
            public DateTime RegistrationDate { get; set; }
            public DateTime ProcessingDate { get; set; }
        }
        #endregion

        #region 7.1.11 DTRSendingResult
        /// <summary>
        /// Resultado del envío de un DTR.
        /// </summary>
        public class DTRSendingResult
        {
            public string ChannelRefNumber { get; set; }
            public long CGPRefNumber { get; set; }
            public string SINPERefNumber { get; set; }
            public string CBTrxNumber { get; set; }
            public decimal DebitedAmount { get; set; }
            public decimal ComissionAmount { get; set; }
            public string CurrencyComissionAmount { get; set; }
            public decimal ExchangeRate { get; set; }
            public string DebitCurrencyCode { get; set; }
            public List<CustomField> CustomData { get; set; }
            public string State { get; set; }
            public short RejectCode { get; set; }
            public string RejectDescription { get; set; }
            public DateTime RegistrationDate { get; set; }
            public DateTime ProcessingDate { get; set; }
        }
        #endregion

        #region 7.1.12 DebitInfo
        /// <summary>
        /// Información resumida de un débito.
        /// </summary>
        public class DebitInfo
        {
            public string ChannelRefNumber { get; set; }
            public string SINPERefNumber { get; set; }
            public int Type { get; set; }
            public OriginCustomer OriginCustomer { get; set; }
            public DestinationCustomer DestinationCustomer { get; set; }
            public decimal Amount { get; set; }
            public string CurrencyCode { get; set; }
            public string Description { get; set; }
            public short State { get; set; }
            public short RejectCode { get; set; }
            public string RejectDescription { get; set; }
            public DateTime RegistrationDate { get; set; }
            public DateTime ProcessingDate { get; set; }
        }
        #endregion

        #region 7.1.13 ReqDTRInfoChannelRef
        public class ReqDTRInfoChannelRef : ReqBase
        {
            public string ChannelRefNumber { get; set; }
        }
        #endregion

        #region 7.1.14 ReqDTRInfoSINPERef
        public class ReqDTRInfoSINPERef : ReqBase
        {
            public string SINPERefNumber { get; set; }
        }
        #endregion

        #region 7.1.15 ResDTRInfo
        public class ResDTRInfo : ResBase
        {
            public DTR DebitData { get; set; }
            public DTRSendingResult DebitResult { get; set; }
        }
        #endregion

        #region 7.1.16 ReqBatchSending
        public class ReqBatchSending : ReqBase
        {
            public string ChannelBatchNumber { get; set; }
            public string Description { get; set; }
            public int CoreIntegrationPoint { get; set; }
            public int CostCenter { get; set; }
            public List<DTR> Debits { get; set; }
            public List<CustomField> CustomData { get; set; }
        }
        #endregion

        #region 7.1.17 ResBatchSending
        public class ResBatchSending : ResBase
        {
            public string ChannelBatchNumber { get; set; }
            public int KINDOBatchNumber { get; set; }
            public bool Accepted { get; set; }
        }
        #endregion

        #region 7.1.18 ReqBatchState
        public class ReqBatchState : ReqBase
        {
            public string ChannelBatchNumber { get; set; }
        }
        #endregion

        #region 7.1.19 ResBatchState
        public class ResBatchState : ResBase
        {
            public BatchSendingResult BatchStateInfo { get; set; }
        }
        #endregion

        #region 7.1.20 BatchSendingResult
        public class BatchSendingResult
        {
            public string ChannelBatchNumber { get; set; }
            public int KINDOBatchNumber { get; set; }
            public bool BatchState { get; set; }
            public int ProcessedTransfers { get; set; }
            public int ProcessingProgress { get; set; }
            public int ConfirmedTransfers { get; set; }
            public int RejectedTransfers { get; set; }
            public int OnHoldTransfers { get; set; }
            public List<DTRSendingResult> DebitsResult { get; set; }
        }
        #endregion

        #region 7.1.21 ResServiceAvailable
        public class ResServiceAvailable : ResBase
        {
            public bool ServiceAvailable { get; set; }
        }
        #endregion

        #region 7.1.22 ReqCustomerServiceAuthorization
        public class ReqCustomerServiceAuthorization : ReqBase
        {
            public string CustomerIdentification { get; set; }
        }
        #endregion

        #region 7.1.23 ResCustomerServiceAuthorization
        public class ResCustomerServiceAuthorization : ResBase
        {
            public bool State { get; set; }
        }
        #endregion

        #region 7.1.24 ReqAccountInfo
        public class ReqAccountInfo : ReqBase
        {
            public string Id { get; set; }
            public string AccountNumber { get; set; }
        }
        #endregion

        #region 7.1.25 ResAccountInfo
        public class ResAccountInfo : ResBase
        {
            public Account Account { get; set; }
        }
        #endregion

        #region 7.1.26 ReqDTRSending
        public class ReqDTRSending : ReqBase
        {
            public int CoreIntegrationPoint { get; set; }
            public int CostCenter { get; set; }
            public DTR Debit { get; set; }
            public List<CustomField> CustomData { get; set; }
        }
        #endregion

        #region 7.1.27 ResDTRSending
        public class ResDTRSending : ResBase
        {
            public DTRSendingResult DTRSendingResult { get; set; }
        }
        #endregion

        #region 7.1.28 ReqCustomerDebits
        public class ReqCustomerDebits : ReqBase
        {
            public string CustomerId { get; set; }
            public DateTime InitialDate { get; set; }
            public DateTime FinalDate { get; set; }
            public short Type { get; set; }
            public short State { get; set; }
        }
        #endregion

        #region 7.1.29 ResCustomerDebits
        public class ResCustomerDebits : ResBase
        {
            public List<DTRInfo> Debits { get; set; }
        }
        #endregion

        #region 7.1.30 ReqAllDebits
        public class ReqAllDebits : ReqBase
        {
            public DateTime InitialDate { get; set; }
            public DateTime FinalDate { get; set; }
            public short Type { get; set; }
            public short State { get; set; }
            public int Page { get; set; }
        }
        #endregion

        #region 7.1.31 ResAllDebits
        public class ResAllDebits : ResBase
        {
            public List<DebitInfo> Debits { get; set; }
            public int PagesQty { get; set; }
            public int TransfersQty { get; set; }
        }
        #endregion

        #endregion

        #region Clases PIN

        /// <summary>
        /// Clase para transportar los datos personales y financieros del cliente receptor de la transacción.
        /// </summary>
        public class DestinationCustomerPIN
        {
            /// <summary>
            /// Número de cuenta IBAN del cliente receptor en su Entidad Financiera.
            /// </summary>
            public string IBAN2 { get; set; } // 22 caracteres

            /// <summary>
            /// Dirección de correo electrónico del cliente receptor (opcional).
            /// </summary>
            public string Email { get; set; } // Opcional, 320 caracteres
        }

        /// <summary>
        /// Clase que representa la información de una cuenta IBAN consultada.
        /// </summary>
        public class AccountPIN
        {
            /// <summary>
            /// Número de cuenta IBAN.
            /// </summary>
            public string IBAN { get; set; } // 22 caracteres

            /// <summary>
            /// Nombre del titular de la cuenta.
            /// </summary>
            public string AccountName { get; set; } // Máx. 150 caracteres

            /// <summary>
            /// Identificación del titular.
            /// </summary>
            public string CustomerId { get; set; } // 5–30 caracteres

            /// <summary>
            /// Código de la Entidad Financiera a la que pertenece la cuenta.
            /// </summary>
            public string EntityCode { get; set; } // Máx. 20 caracteres
        }

        /// <summary>
        /// Clase para transportar la información de una transacción PIN entre Entidades Financieras.
        /// </summary>
        public class PINTransfer
        {
            /// <summary>
            /// Referencia de la transacción asignada por el canal.
            /// </summary>
            public string ChannelReference { get; set; }

            /// <summary>
            /// Monto de la transacción.
            /// </summary>
            public decimal Amount { get; set; }

            /// <summary>
            /// Fecha y hora en que se generó la transacción.
            /// </summary>
            public DateTime TransactionDate { get; set; }

            /// <summary>
            /// Información del cliente que origina la transacción.
            /// </summary>
            public OriginCustomer Origin { get; set; }

            /// <summary>
            /// Información del cliente que recibe la transacción.
            /// </summary>
            public DestinationCustomer Destination { get; set; }

            /// <summary>
            /// Campos personalizados adicionales asociados a la transacción.
            /// </summary>
            public List<CustomField> CustomFields { get; set; }
        }

        /// <summary>
        /// Clase que representa el resultado de una solicitud de envío de PIN.
        /// </summary>
        public class PINSendingResult
        {
            /// <summary>
            /// Identificador único de la operación asignado por KINDO.
            /// </summary>
            public string OperationId { get; set; }

            /// <summary>
            /// Código de referencia de SINPE asignado a la transacción.
            /// </summary>
            public string SINPEReference { get; set; }

            /// <summary>
            /// Fecha y hora en que se procesó la transacción.
            /// </summary>
            public DateTime ProcessDate { get; set; }

            /// <summary>
            /// Indica si la transacción fue exitosa.
            /// </summary>
            public bool IsApproved { get; set; }

            /// <summary>
            /// Mensaje o detalle del resultado.
            /// </summary>
            public string Message { get; set; }
        }

        /// <summary>
        /// Request para solicitar el envío de una transacción PIN.
        /// </summary>
        public class ReqPINSending : ReqBase
        {
            /// <summary>
            /// Información de la transacción PIN a enviar.
            /// </summary>
            public PINTransfer PINData { get; set; }
        }

        /// <summary>
        /// Response para el envío de una transacción PIN.
        /// </summary>
        public class ResPINSending : ResBase
        {
            /// <summary>
            /// Resultado del envío de la transacción PIN.
            /// </summary>
            public PINSendingResult PINSendingResult { get; set; }
        }

        /// <summary>
        /// Clase que representa la información detallada de una transferencia PIN.
        /// </summary>
        public class TransferInfo
        {
            /// <summary>
            /// Referencia de la transacción asignada por el canal.
            /// </summary>
            public string ChannelReference { get; set; }

            /// <summary>
            /// Referencia de la transacción asignada por SINPE.
            /// </summary>
            public string SINPEReference { get; set; }

            /// <summary>
            /// Estado actual de la transacción.
            /// </summary>
            public string Status { get; set; }

            /// <summary>
            /// Fecha y hora en que se procesó la transacción.
            /// </summary>
            public DateTime ProcessDate { get; set; }

            /// <summary>
            /// Monto de la transacción.
            /// </summary>
            public decimal Amount { get; set; }

            /// <summary>
            /// Cliente que origina la transacción.
            /// </summary>
            public OriginCustomer Origin { get; set; }

            /// <summary>
            /// Cliente que recibe la transacción.
            /// </summary>
            public DestinationCustomer Destination { get; set; }
        }

        /// <summary>
        /// Clase que representa la información resumida de un lote de transferencias.
        /// </summary>
        public class TransfersBatch
        {
            /// <summary>
            /// Identificador único del lote.
            /// </summary>
            public string BatchId { get; set; }

            /// <summary>
            /// Cantidad total de transferencias incluidas en el lote.
            /// </summary>
            public int TransferCount { get; set; }

            /// <summary>
            /// Monto total del lote.
            /// </summary>
            public decimal TotalAmount { get; set; }

            /// <summary>
            /// Fecha y hora de creación del lote.
            /// </summary>
            public DateTime CreatedAt { get; set; }

            /// <summary>
            /// Estado actual del lote.
            /// </summary>
            public string Status { get; set; }
        }

        /// <summary>
        /// Representa el resultado de una operación de envío de lote.
        /// </summary>
        public class BatchSendingResultPIN
        {
            /// <summary>
            /// Identificador del lote asignado por KINDO.
            /// </summary>
            public string BatchId { get; set; }

            /// <summary>
            /// Indica si el envío fue exitoso.
            /// </summary>
            public bool IsSuccessful { get; set; }

            /// <summary>
            /// Fecha y hora en que se procesó el envío del lote.
            /// </summary>
            public DateTime ProcessDate { get; set; }

            /// <summary>
            /// Mensaje descriptivo del resultado.
            /// </summary>
            public string Message { get; set; }
        }

        /// <summary>
        /// Request para consultar información de una transferencia por referencia de canal.
        /// </summary>
        public class ReqTransferInfoChannelRef : ReqBase
        {
            /// <summary>
            /// Referencia de la transacción generada por el canal.
            /// </summary>
            public string ChannelReference { get; set; }
        }

        /// <summary>
        /// Request para consultar información de una transferencia por referencia SINPE.
        /// </summary>
        public class ReqTransferInfoSINPERef : ReqBase
        {
            /// <summary>
            /// Referencia SINPE de la transacción.
            /// </summary>
            public string SINPEReference { get; set; }
        }

        /// <summary>
        /// Respuesta a la consulta de información de una transferencia.
        /// </summary>
        public class ResTransferInfo : ResBase
        {
            /// <summary>
            /// Información de la transferencia consultada.
            /// </summary>
            public TransferInfo Transfer { get; set; }
        }

        /// <summary>
        /// Request para enviar un lote de transferencias PIN.
        /// </summary>
        public class ReqBatchSendingPIN : ReqBase
        {
            /// <summary>
            /// Lista de transferencias incluidas en el lote.
            /// </summary>
            public List<PINTransfer> Transfers { get; set; }

            /// <summary>
            /// Identificador opcional del lote asignado por el canal.
            /// </summary>
            public string ChannelBatchId { get; set; }
        }

        /// <summary>
        /// Response del envío de un lote de transferencias.
        /// </summary>
        public class ResBatchSendingPIN : ResBase
        {
            /// <summary>
            /// Resultado del envío del lote.
            /// </summary>
            public BatchSendingResult BatchResult { get; set; }
        }

        /// <summary>
        /// Request para consultar el estado de un lote de transferencias.
        /// </summary>
        public class ReqBatchStatePIN : ReqBase
        {
            /// <summary>
            /// Identificador del lote asignado por KINDO.
            /// </summary>
            public string BatchId { get; set; }
        }

        /// <summary>
        /// Response con el estado de un lote de transferencias.
        /// </summary>
        public class ResBatchStatePIN : ResBase
        {
            /// <summary>
            /// Estado actual del lote de transferencias.
            /// </summary>
            public string BatchStatus { get; set; }

            /// <summary>
            /// Fecha y hora del último cambio de estado.
            /// </summary>
            public DateTime LastUpdate { get; set; }
        }

        /// <summary>
        /// Request para consultar todas las transferencias realizadas por un cliente.
        /// </summary>
        public class ReqCustomerTransfers : ReqBase
        {
            /// <summary>
            /// Identificación del cliente a consultar.
            /// </summary>
            public string CustomerId { get; set; }

            /// <summary>
            /// Fecha de inicio del rango de búsqueda.
            /// </summary>
            public DateTime FromDate { get; set; }

            /// <summary>
            /// Fecha de fin del rango de búsqueda.
            /// </summary>
            public DateTime ToDate { get; set; }
        }

        /// <summary>
        /// Response que contiene todas las transferencias realizadas por un cliente.
        /// </summary>
        public class ResCustomerTransfers : ResBase
        {
            /// <summary>
            /// Lista de transferencias asociadas al cliente.
            /// </summary>
            public List<TransferInfo> Transfers { get; set; }
        }

        /// <summary>
        /// Request para obtener todas las transferencias procesadas por la entidad.
        /// </summary>
        public class ReqAllTransfers : ReqBase
        {
            /// <summary>
            /// Fecha inicial del rango de consulta.
            /// </summary>
            public DateTime FromDate { get; set; }

            /// <summary>
            /// Fecha final del rango de consulta.
            /// </summary>
            public DateTime ToDate { get; set; }

            /// <summary>
            /// Número de página a consultar (paginación).
            /// </summary>
            public int PageNumber { get; set; }

            /// <summary>
            /// Cantidad de registros por página.
            /// </summary>
            public int PageSize { get; set; }
        }

        /// <summary>
        /// Response con todas las transferencias procesadas en el rango solicitado.
        /// </summary>
        public class ResAllTransfers : ResBase
        {
            /// <summary>
            /// Lista de transferencias incluidas en el resultado.
            /// </summary>
            public List<TransferInfo> Transfers { get; set; }

            /// <summary>
            /// Total de registros encontrados.
            /// </summary>
            public int TotalCount { get; set; }
        }

        /// <summary>
        /// Request para importar un lote de transferencias a KINDO.
        /// </summary>
        public class ReqBatchImport : ReqBase
        {
            /// <summary>
            /// Lote de transferencias a importar.
            /// </summary>
            public TransfersBatch Batch { get; set; }
        }

        /// <summary>
        /// Response para la importación de un lote.
        /// </summary>
        public class ResBatchImport : ResBase
        {
            /// <summary>
            /// Resultado del proceso de importación.
            /// </summary>
            public BatchSendingResult ImportResult { get; set; }
        }

        /// <summary>
        /// Request para revertir la importación de un lote.
        /// </summary>
        public class ReqReverseBatchImport : ReqBase
        {
            /// <summary>
            /// Identificador del lote a revertir.
            /// </summary>
            public string BatchId { get; set; }

            /// <summary>
            /// Motivo de la reversión.
            /// </summary>
            public string Reason { get; set; }
        }

        /// <summary>
        /// Response para la reversión de un lote importado.
        /// </summary>
        public class ResReverseBatchImport : ResBase
        {
            /// <summary>
            /// Indica si la reversión fue exitosa.
            /// </summary>
            public bool Reversed { get; set; }

            /// <summary>
            /// Fecha y hora en que se completó la reversión.
            /// </summary>
            public DateTime ReversedAt { get; set; }
        }

        #endregion


        #region Consulta SINPE

        public class InfoSinpeRequest
        {
            public ParametrosSinpe _paramertrosSinpe { get; set; }
            public vInfoSinpe vInfo { get; set; }
            public RequestInfo requestInfo { get; set; }
        }

        public class ResponseData
        {
            public bool IsSuccessful { get; set; }
            public string OperationId { get; set; }
            public List<Errores> Errors { get; set; } = new List<Errores>();
            public Sinpe_PIN.Account Account { get; set; } = new Sinpe_PIN.Account();
        }

        public class RequestInfo
        {
            public string HostId { get; set; } = string.Empty;
            public string OperationId { get; set; } = string.Empty;
            public string ClientIPAddress { get; set; } = string.Empty;
            public string CultureCode { get; set; } = string.Empty;
            public string UserCode { get; set; } = string.Empty;
        }

    

        public class InfoSinpeData
        {
            public string Cedula { get; set; } = string.Empty;
            public string Cuenta { get; set; } = string.Empty;
            public int tipoID { get; set; } = 0;
            public string? cod_divisa { get; set; } = string.Empty;

        }

        public class TesTransaccion
        {
            public int NumeroSolicitud { get; set; } = 0;
            public Nullable<DateTime> FechaEmision { get; set; } = null;
            public Nullable<DateTime> FechaTraslado { get; set; } = null;
            public string? UsuarioGenera { get; set; } = null;
            public bool? estadoSinpe { get; set; }
            public int? IdMotivoRechazo { get; set; }
            public string? CodigoReferencia { get; set; } = null;
            public string? DocumentoBase { get; set; } = null;
            public string contador { get; set; } = "0";

            public string? Detalle1 { get; set; } = null;
            public string? Detalle2 { get; set; } = null;
            public string? Detalle3 { get; set; } = null;
            public string? Detalle4 { get; set; } = null;
            public string? Detalle5 { get; set; } = null;

            public string? Divisa { get; set; } = null;
            public decimal tipoCambio { get; set; } = 0;
            public decimal Monto { get; set; } = 0;

            public string? CorreoNotifica { get; set; } = null;
            public string? CedulaOrigen { get; set; } = null;
            public string? NombreOrigen { get; set; } = null;
            public string? CuentaOrigen { get; set; } = null;
            public E_TipoIdentificacion tipoCedOrigen { get; set; }

            public string? Codigo { get; set; } = null;
            public string? Beneficiario { get; set; } = null;
            public string? Cuenta { get; set; } = null;
            public E_TipoIdentificacion tipoCedDestino { get; set; }

            public string? NDocumento { get; set; } = null;
        }

        public class vInfoSinpe
        {
            public string? Cedula { get; set; } = null;
            public string? CuentaIBAN { get; set; } = null;
            public int tipoID { get; set; } = 0;
            public string? cod_divisa { get; set; } = string.Empty;

        }
        #endregion


        #region Factura Electronica

        public class FE_ParametrosEncabezado
        {
            public byte CantDeci { get; set; } = 0;
            public short Sucursal { get; set; } = 0;
            public long CodigoActividad { get; set; } = 0;
            public int Terminal { get; set; } = 0;
        }

        public class FE_Receptor
        {
            public string Nombre { get; set; } = string.Empty;
            public string Correo { get; set; } = string.Empty;
            public byte? TipoIdent { get; set; }
            public string Identificacion { get; set; } = string.Empty;
        }

        public class FE_Detalles
        {
            public short NumeroLinea { get; set; }
            public object? Codigo { get; set; }
            public object? Nombre { get; set; }
            public List<object>? CodProdServ { get; set; }
            public List<object>? CodTipo { get; set; }
            public object? Cantidad { get; set; }
            public object? UnidadMedida { get; set; }
            public object? UnidadComercial { get; set; }
            public object? Descripcion { get; set; }
            public object? PrecioUnitario { get; set; }
            public List<FactElectronica.FE_JsonDescuentos>? Descuentos { get; set; }
            public List<FactElectronica.FE_JsonImpuestos>? Impuestos { get; set; }
        }

        public class FE_Descuentos
        {
            public float MontoDescuento { get; set; } = 0;
        }

        //Valores fijos
        public enum E_SituacionEnvio : short
        {
            Normal = 1,
            Contingencia = 2,
            Sin_Internet = 3
        }

        public enum E_Moneda : short
        {
            Colones = 1,
            Dollar = 2,
            Euro = 3
        }

        public enum E_CondicionVenta : short
        {
            Contado = 1,
            Credito = 2,
            Consignacion = 3
        }

        #endregion


        public partial class RespuestaRegistro : Respuesta
        {
            [System.Runtime.Serialization.DataMemberAttribute(IsRequired = true)]
            public int ReferenciaBanco { get; set; }

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false)]
            public string ReferenciaSistemaInterno { get; set; }

            [System.Runtime.Serialization.DataMemberAttribute(IsRequired = true, Order = 2)]
            public decimal MontoDebito { get; set; }

            [System.Runtime.Serialization.DataMemberAttribute(IsRequired = true, Order = 3)]
            public decimal MontoComision { get; set; }

            [System.Runtime.Serialization.DataMemberAttribute(IsRequired = true, Order = 4)]
            public decimal TipoCambio { get; set; }

            [System.Runtime.Serialization.DataMemberAttribute(IsRequired = true, Order = 5)]
            public decimal MontoTotal { get; set; }

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 6)]
            public string RequestId { get; set; }

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 7)]
            public string CodigoReferencia { get; set; }
        }

        public partial class RegistrarDebitoCuentaResponseBody 
        {
            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false)]
            public RespuestaRegistro[] RegistrarDebitoCuentaResult { get; set; }
        }

        public partial class RegistrarDebitoCuentaRequestBody
        {
            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false)]
            public SI_Rastro Rastro { get; set; }

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false)]
            public TransferenciaAS400[] Transacciones { get; set; }
        }

        public partial class TransferenciaAS400 
        {
            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false)]
            public Transaccion DatosTransaccion { get; set; }

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 1)]
            public ClienteAS400 ClienteDestino { get; set; }

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 2)]
            public ClienteAS400 ClienteOrigen { get; set; }

            [System.Runtime.Serialization.DataMemberAttribute(IsRequired = true, Order = 3)]
            public ServiciosSINPE TipoTransaccion { get; set; }

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 4)]
            public string IdRequest { get; set; }
        }

        public partial class Transaccion 
        {
            [System.Runtime.Serialization.DataMemberAttribute(IsRequired = true)]
            public E_Monedas Moneda { get; set; }

            [System.Runtime.Serialization.DataMemberAttribute(IsRequired = true)]
            public decimal Monto { get; set; }

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 2)]
            public string Descripcion { get; set; }

            [System.Runtime.Serialization.DataMemberAttribute(IsRequired = true, Order = 3)]
            public int EntidadOrigen { get; set; }

            [System.Runtime.Serialization.DataMemberAttribute(IsRequired = true, Order = 4)]
            public int CentroCosto { get; set; }

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 5)]
            public string Servicio { get; set; }

            [System.Runtime.Serialization.DataMemberAttribute(IsRequired = true, Order = 6)]
            public int PuntoIntegracion { get; set; }

            [System.Runtime.Serialization.DataMemberAttribute(IsRequired = true, Order = 7)]
            public int CodigoConcepto { get; set; }

            [System.Runtime.Serialization.DataMemberAttribute(IsRequired = true, Order = 8)]
            public bool FirmaDigital { get; set; }

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 9)]
            public string eMAIL { get; set; }

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 10)]
            public string IDCorrelation { get; set; }
        }

        public partial class ClienteAS400 
        {
            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false)]
            public string Identificacion { get; set; }

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false)]
            public string Nombre { get; set; }

            [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue = false, Order = 2)]
            public string IBAN { get; set; }

            [System.Runtime.Serialization.DataMemberAttribute(IsRequired = true, Order = 3)]
            public int TipoCedula { get; set; }
        }

        public partial class RegistrarDebitoCuentaBody 
        {
        [System.Runtime.Serialization.DataMemberAttribute()]
        public RegistrarDebitoCuentaRequestBody body { get; set; }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public IModelosRastroSIF rastro { get; set; }
    }

        public partial class IModelosRastroSIF 
        {
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Equipo { get; set; }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string IP { get; set; }

        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Usuario { get; set; }
    }

    }
