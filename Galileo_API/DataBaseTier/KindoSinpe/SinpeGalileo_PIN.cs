using Galileo.Models.KindoSinpe;
using Galileo_API.DataBaseTier.KindoSinpe;
using Humanizer;
using Newtonsoft.Json;
using System.Drawing.Drawing2D;
using System.Text;

namespace Galileo_API.DataBaseTier
{
#pragma warning disable S125 // Quitar despues de implementar resto de metodos con CSG
    public class SinpeGalileoPin
    {
        private readonly MClientHpptCall mClient = new MClientHpptCall();
        public SinpeGalileoPin(IConfiguration config)
        {
            mClient = new MClientHpptCall();
        }

        #region 6.x IsServiceAvailable
        /// <summary>
        /// ResServiceAvailable IsServiceAvailable (ReqBase Context)
        /// Verifica si el Servicio PIN de KINDO se encuentra disponible para procesar transacciones.
        /// Endpoint: /IsServiceAvailable
        /// </summary>
        public ResServiceAvailable IsServiceAvailable(string UrlCGP_PIN, ReqBase context)
        {
            return mClient.PostJsonAsync<ReqBase, ResServiceAvailable, ResServiceAvailable>(
                   baseUrl: UrlCGP_PIN,
                   endpoint: "/IsServiceAvailable",
                   request: context,
                   mapOk: serviceRes => new ResServiceAvailable
                   {
                       IsSuccessful = true,
                       OperationId = serviceRes.OperationId,
                       ServiceAvailable = serviceRes.ServiceAvailable
                   },
                   errorFactory: (code, msg) => new ResServiceAvailable
                   {
                       IsSuccessful = false,
                       Errors = new[] { new Error { Code = code, Message = msg } }
                   },
                   operationName: nameof(IsServiceAvailable)
               ).Result;
        }
        #endregion

        #region 6.x GetAccountInfo
        /// <summary>
        /// ResAccountInfo GetAccountInfo (ReqAccountInfo AccountData)
        /// Consulta la información de una cuenta IBAN en otra Entidad Financiera.
        /// Endpoint: /GetAccountInfo
        /// </summary>
        public ResAccountInfo GetAccountInfo(string UrlCGP_PIN, ReqAccountInfo accountData)
        {
            return mClient.PostJsonAsync<ReqAccountInfo, ResAccountInfo, ResAccountInfo>(
                   baseUrl: UrlCGP_PIN,
                   endpoint: "/GetAccountInfo",
                   request: accountData,
                   mapOk: serviceRes => new ResAccountInfo
                   {
                       IsSuccessful = serviceRes!.IsSuccessful,
                       OperationId = serviceRes.OperationId,
                       Account = serviceRes.Account,
                       Errors = serviceRes.Errors
                   },
                   errorFactory: (code, msg) => new ResAccountInfo
                   {
                       IsSuccessful = false,
                       Errors = new Error[]
                        {
                            new Error { Code = code, Message = msg}
                        }
                   },
                   operationName: nameof(GetAccountInfo)
               ).Result;
        }
        #endregion

        #region 6.x SendPIN
        /// <summary>
        /// ResPINSending SendPIN (ReqPINSending PINData)
        /// Envía una transacción PIN a una Entidad Financiera participante.
        /// Endpoint: /SendPIN
        /// </summary>
        public ResSendingDynamic SendPIN(string UrlCGP_PIN, ReqSendingDynamic pinData)
        {
            return mClient.PostJsonAsync<ReqSendingDynamic, ResSendingDynamic, ResSendingDynamic>(
                   baseUrl: UrlCGP_PIN,
                   endpoint: "/SendPIN",
                   request: pinData,
                   mapOk: serviceRes => new ResSendingDynamic
                   {
                       Errors = serviceRes.Errors,
                       IsSuccessful = serviceRes.IsSuccessful,
                       OperationId = serviceRes.OperationId,
                       PINSendingResult = serviceRes.PINSendingResult     
                   },
                   errorFactory: (code, msg) => new ResSendingDynamic
                   {
                       IsSuccessful = false,
                       Errors = new Error[]
                        {
                            new Error { Code = code, Message = msg}
                        }
                   },
                   operationName: nameof(SendPIN)
               ).Result;
        }
        #endregion

        #region 6.x GetPINResult
        /// <summary>
        /// ResPINSending GetPINResult (ReqTransferInfoChannelRef PINData)
        /// Consulta el resultado de envío de una transacción PIN usando la referencia del canal.
        /// Endpoint: /GetPINResult
        /// </summary>
        public ResSendingDynamic GetPINResult(string UrlCGP_PIN, ReqTransferInfoChannelRef pinData)
        {
            return mClient.PostJsonAsync<ReqTransferInfoChannelRef, ResSendingDynamic, ResSendingDynamic>(
                   baseUrl: UrlCGP_PIN,
                   endpoint: "/GetPINResult",
                   request: pinData,
                   mapOk: serviceRes => new ResSendingDynamic
                   {
                       Errors = serviceRes.Errors,
                       IsSuccessful = serviceRes.IsSuccessful,
                       OperationId = serviceRes.OperationId,
                       PINSendingResult = serviceRes.PINSendingResult
                   },
                   errorFactory: (code, msg) => new ResSendingDynamic
                   {
                       IsSuccessful = false,
                       Errors = new Error[]
                        {
                            new Error { Code = code, Message = msg}
                        }
                   },
                   operationName: nameof(GetPINResult)
               ).Result;
        }
        #endregion

        #region 6.x GetPINDataByChannelRef
        /// <summary>
        /// ResTransferInfo GetPINDataByChannelRef (ReqTransferInfoChannelRef PINData)
        /// Consulta datos y resultado de una transacción PIN usando la referencia interna del canal.
        /// Endpoint: /GetPINDataByChannelRef
        /// </summary>
        public ResTransferInfo GetPINDataByChannelRef(string UrlCGP_PIN, ReqTransferInfoChannelRef pinData)
        {
            return mClient.PostJsonAsync<ReqTransferInfoChannelRef, ResTransferInfo, ResTransferInfo>(
                   baseUrl: UrlCGP_PIN,
                   endpoint: "/GetPINDataByChannelRef",
                   request: pinData,
                   mapOk: serviceRes => new ResTransferInfo
                   {
                      Errors = serviceRes.Errors,
                      IsSuccessful = serviceRes.IsSuccessful,
                      OperationId = serviceRes.OperationId,
                      Transfer = serviceRes.Transfer
                      
                   },
                   errorFactory: (code, msg) => new ResTransferInfo
                   {
                       IsSuccessful = false,
                       Errors = new Error[]
                        {
                            new Error { Code = code, Message = msg}
                        }
                   },
                   operationName: nameof(GetPINDataByChannelRef)
               ).Result;
        }
        #endregion

        #region 6.x GetPINDataBySINPERef
        /// <summary>
        /// ResTransferInfo GetPINDataBySINPERef (ReqTransferInfoSINPERef PINData)
        /// Consulta datos y resultado de una transacción PIN usando el número de referencia SINPE.
        /// Endpoint: /GetPINDataBySINPERef
        /// </summary>
        public ResTransferInfo GetPINDataBySINPERef(string UrlCGP_PIN, ReqTransferInfoSINPERef pinData)
        {
            return mClient.PostJsonAsync<ReqTransferInfoSINPERef, ResTransferInfo, ResTransferInfo>(
                   baseUrl: UrlCGP_PIN,
                   endpoint: "/GetPINDataBySINPERef",
                   request: pinData,
                   mapOk: serviceRes => new ResTransferInfo
                   {
                       Errors = serviceRes.Errors,
                       IsSuccessful = serviceRes.IsSuccessful,
                       OperationId = serviceRes.OperationId,
                       Transfer = serviceRes.Transfer
                   },
                   errorFactory: (code, msg) => new ResTransferInfo
                   {
                       IsSuccessful = false,
                       Errors = new Error[]
                        {
                            new Error { Code = code, Message = msg}
                        }
                   },
                   operationName: nameof(GetPINDataBySINPERef)
               ).Result;
        }
        #endregion

        #region 6.x SendBatch
        /// <summary>
        /// ResBatchSending SendBatch (ReqBatchSending BatchData)
        /// Solicita el envío de un lote de transacciones PIN. El procesamiento puede ser asincrónico.
        /// Endpoint: /SendBatch
        /// </summary>
        public ResBatchSending SendBatch(string UrlCGP_PIN, ReqBatchSending batchData)
        {
            return mClient.PostJsonAsync<ReqBatchSending, ResBatchSending, ResBatchSending>(
                   baseUrl: UrlCGP_PIN,
                   endpoint: "/SendBatch",
                   request: batchData,
                   mapOk: serviceRes => new ResBatchSending
                   {
                       Errors = serviceRes.Errors,
                       IsSuccessful = serviceRes.IsSuccessful,
                          OperationId = serviceRes.OperationId,
                          Accepted = serviceRes.Accepted,
                          ChannelBatchNumber = serviceRes.ChannelBatchNumber,
                          KINDOBatchNumber = serviceRes.KINDOBatchNumber
                   },
                   errorFactory: (code, msg) => new ResBatchSending
                   {
                       IsSuccessful = false,
                       Errors = new Error[]
                        {
                            new Error { Code = code, Message = msg}
                        }
                   },
                   operationName: nameof(SendBatch)
               ).Result;
        }
        #endregion

        #region 6.x GetBatchState
        /// <summary>
        /// ResBatchState GetBatchState (ReqBatchState BatchData)
        /// Consulta el estado actual de procesamiento de un lote de transacciones PIN.
        /// Endpoint: /GetBatchState
        /// </summary>
        public ResBatchState GetBatchState(string UrlCGP_PIN, ReqBatchState batchData)
        {
            return mClient.PostJsonAsync<ReqBatchState, ResBatchState, ResBatchState>(
                  baseUrl: UrlCGP_PIN,
                  endpoint: "/GetBatchState",
                  request: batchData,
                  mapOk: serviceRes => new ResBatchState
                  {
                      Errors = serviceRes.Errors,
                      IsSuccessful = serviceRes.IsSuccessful,
                      OperationId = serviceRes.OperationId,
                      BatchStateInfo = serviceRes.BatchStateInfo
                      
                  },
                  errorFactory: (code, msg) => new ResBatchState
                  {
                      IsSuccessful = false,
                      Errors = new Error[]
                       {
                            new Error { Code = code, Message = msg}
                       }
                  },
                  operationName: nameof(GetBatchState)
              ).Result;
        }
        #endregion

        #region 6.x GetCustomerTransfers
        /// <summary>
        /// ResCustomerTransfers GetCustomerTransfers (ReqCustomerTransfers ConsultData)
        /// Obtiene todas las transferencias PIN que ha enviado o recibido un cliente en un rango de fechas.
        /// Endpoint: /GetCustomerTransfers
        /// </summary>
        public ResCustomerTransfers GetCustomerTransfers(string UrlCGP_PIN, ReqCustomerTransfers consultData)
        {
            return mClient.PostJsonAsync<ReqCustomerTransfers, ResCustomerTransfers, ResCustomerTransfers>(
                 baseUrl: UrlCGP_PIN,
                 endpoint: "/GetCustomerTransfers",
                 request: consultData,
                 mapOk: serviceRes => new ResCustomerTransfers
                 {
                     Errors = serviceRes.Errors,
                     IsSuccessful = serviceRes.IsSuccessful,
                     OperationId = serviceRes.OperationId,
                     Transfers = serviceRes.Transfers,
                 },
                 errorFactory: (code, msg) => new ResCustomerTransfers
                 {
                     IsSuccessful = false,
                     Errors = new Error[]
                      {
                            new Error { Code = code, Message = msg}
                      }
                 },
                 operationName: nameof(GetCustomerTransfers)
             ).Result;  
        }
        #endregion

        #region 6.x GetAllTransfers
        /// <summary>
        /// ResAllTransfers GetAllTransfers (ReqAllTransfers FilterData)
        /// Obtiene todas las transferencias PIN registradas en un rango de fechas determinado (con paginación).
        /// Endpoint: /GetAllTransfers
        /// </summary>
        public ResAllTransfers GetAllTransfers(string UrlCGP_PIN, ReqAllTransfers filterData)
        {
            return mClient.PostJsonAsync<ReqAllTransfers, ResAllTransfers, ResAllTransfers>(
                baseUrl: UrlCGP_PIN,
                endpoint: "/GetAllTransfers",
                request: filterData,
                mapOk: serviceRes => new ResAllTransfers
                {
                    Errors = serviceRes.Errors,
                    IsSuccessful = serviceRes.IsSuccessful,
                    OperationId = serviceRes.OperationId,
                    Transfers = serviceRes.Transfers,
                    TotalCount = serviceRes.TotalCount,
                    
                },
                errorFactory: (code, msg) => new ResAllTransfers
                {
                    IsSuccessful = false,
                    Errors = new Error[]
                     {
                            new Error { Code = code, Message = msg}
                     }
                },
                operationName: nameof(GetAllTransfers)
            ).Result;
        }
        #endregion
    }
}
