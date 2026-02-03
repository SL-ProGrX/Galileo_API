using Galileo.Models.KindoSinpe;
using Galileo_API.DataBaseTier.KindoSinpe;
using Newtonsoft.Json;
using System.Text;

namespace Galileo_API.DataBaseTier
{
    public class SinpeGalileoDtr
    {
        private readonly MClientHpptCall mClient;

        public SinpeGalileoDtr(IConfiguration config, HttpClient? client = null)
        {
            mClient = new MClientHpptCall();
        }

        public ResServiceAvailable IsServiceAvailable(string UrlCGP_DTR, ReqBase ctx)
        {
            return mClient.PostJsonAsync<ReqBase, ResServiceAvailable, ResServiceAvailable>(
                  baseUrl: UrlCGP_DTR,
                  endpoint: "/IsServiceAvailable",
                  request: ctx,
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

        public ResAccountInfo GetAccountInfo(string UrlCGP_DTR, ReqAccountInfo data)
        {
            return mClient.PostJsonAsync<ReqBase, ResAccountInfo, ResAccountInfo>(
                 baseUrl: UrlCGP_DTR,
                 endpoint: "/GetAccountInfo",
                 request: data,
                 mapOk: serviceRes => new ResAccountInfo
                 {
                     IsSuccessful = serviceRes.IsSuccessful,
                     OperationId = serviceRes.OperationId,
                     Account = serviceRes.Account,
                     Errors = serviceRes.Errors
                 },
                 errorFactory: (code, msg) => new ResAccountInfo
                 {
                     IsSuccessful = false,
                     Errors = new[] { new Error { Code = code, Message = msg } }
                 },
                 operationName: nameof(GetAccountInfo)
             ).Result;
        }


        public ResDTRSending SendDebit(string UrlCGP_DTR, ReqDTRSending data)
        {
            return mClient.PostJsonAsync<ReqDTRSending, ResDTRSending, ResDTRSending>(
                 baseUrl: UrlCGP_DTR,
                 endpoint: "/SendDebit",
                 request: data,
                 mapOk: serviceRes => new ResDTRSending
                 {
                     IsSuccessful = serviceRes.IsSuccessful,
                     OperationId = serviceRes.OperationId,
                     DTRSendingResult = serviceRes.DTRSendingResult,
                     Errors = serviceRes.Errors
                 },
                 errorFactory: (code, msg) => new ResDTRSending
                 {
                     IsSuccessful = false,
                     Errors = new[] { new Error { Code = code, Message = msg } }
                 },
                 operationName: nameof(SendDebit)
             ).Result;
        }

      
        public ResDTRSending GetDebitResult(string UrlCGP_DTR, ReqDTRInfoChannelRef data)
        {
            return mClient.PostJsonAsync<ReqDTRInfoChannelRef, ResDTRSending, ResDTRSending>(
                 baseUrl: UrlCGP_DTR,
                 endpoint: "/GetDebitResult",
                 request: data,
                 mapOk: serviceRes => new ResDTRSending
                 {
                     IsSuccessful = serviceRes.IsSuccessful,
                     OperationId = serviceRes.OperationId,
                     DTRSendingResult = serviceRes.DTRSendingResult,
                     Errors = serviceRes.Errors
                 },
                 errorFactory: (code, msg) => new ResDTRSending
                 {
                     IsSuccessful = false,
                     Errors = new[] { new Error { Code = code, Message = msg } }
                 },
                 operationName: nameof(GetDebitResult)
             ).Result;
        }

     

        public ResDTRInfo GetDebitDataByChannelRef(string UrlCGP_DTR, ReqDTRInfoChannelRef data)
        {
            return mClient.PostJsonAsync<ReqDTRInfoChannelRef, ResDTRInfo, ResDTRInfo>(
                baseUrl: UrlCGP_DTR,
                endpoint: "/GetDebitDataByChannelRef",
                request: data,
                mapOk: serviceRes => new ResDTRInfo
                {
                    IsSuccessful = serviceRes.IsSuccessful,
                    OperationId = serviceRes.OperationId,
                    DebitData = serviceRes.DebitData,
                    DebitResult = serviceRes.DebitResult,
                    Errors = serviceRes.Errors
                },
                errorFactory: (code, msg) => new ResDTRInfo
                {
                    IsSuccessful = false,
                    Errors = new[] { new Error { Code = code, Message = msg } }
                },
                operationName: nameof(GetDebitDataByChannelRef)
            ).Result;
        }

      

        public ResDTRInfo GetDebitDataBySINPERef(string UrlCGP_DTR, ReqDTRInfoSINPERef data)
        {
            return mClient.PostJsonAsync<ReqDTRInfoSINPERef, ResDTRInfo, ResDTRInfo>(
               baseUrl: UrlCGP_DTR,
               endpoint: "/GetDebitDataBySINPERef",
               request: data,
               mapOk: serviceRes => new ResDTRInfo
               {
                   IsSuccessful = serviceRes.IsSuccessful,
                   OperationId = serviceRes.OperationId,
                   DebitData = serviceRes.DebitData,
                   DebitResult = serviceRes.DebitResult,
                   Errors = serviceRes.Errors
                   
               },
               errorFactory: (code, msg) => new ResDTRInfo
               {
                   IsSuccessful = false,
                   Errors = new[] { new Error { Code = code, Message = msg } }
               },
               operationName: nameof(GetDebitDataBySINPERef)
           ).Result;
        }

     

        public ResBatchSending SendBatch(string UrlCGP_DTR, ReqBatchSending data)
        {
            return mClient.PostJsonAsync<ReqBatchSending, ResBatchSending, ResBatchSending>(
               baseUrl: UrlCGP_DTR,
               endpoint: "/SendBatch",
               request: data,
               mapOk: serviceRes => new ResBatchSending
               {
                   IsSuccessful = serviceRes.IsSuccessful,
                   OperationId = serviceRes.OperationId,
                   Errors = serviceRes.Errors,
                   Accepted = serviceRes.Accepted,
                   ChannelBatchNumber = serviceRes.ChannelBatchNumber,
                   KINDOBatchNumber = serviceRes.KINDOBatchNumber,
               },
               errorFactory: (code, msg) => new ResBatchSending
               {
                   IsSuccessful = false,
                   Errors = new[] { new Error { Code = code, Message = msg } }
               },
               operationName: nameof(SendBatch)
           ).Result;

        }

        public ResBatchState GetBatchState(string UrlCGP_DTR, ReqBatchState data)
        {
            return mClient.PostJsonAsync<ReqBatchState, ResBatchState, ResBatchState>(
               baseUrl: UrlCGP_DTR,
               endpoint: "/GetBatchState",
               request: data,
               mapOk: serviceRes => new ResBatchState
               {
                   IsSuccessful = serviceRes.IsSuccessful,
                   OperationId = serviceRes.OperationId,
                   Errors = serviceRes.Errors,
                   BatchStateInfo = serviceRes.BatchStateInfo,
               },
               errorFactory: (code, msg) => new ResBatchState
               {
                   IsSuccessful = false,
                   Errors = new[] { new Error { Code = code, Message = msg } }
               },
               operationName: nameof(GetBatchState)
           ).Result;
        }

       
        public ResCustomerDebits GetCustomerDebits(string UrlCGP_DTR, ReqCustomerDebits data)
        {
            return mClient.PostJsonAsync<ReqCustomerDebits, ResCustomerDebits, ResCustomerDebits>(
               baseUrl: UrlCGP_DTR,
               endpoint: "/GetCustomerDebits",
               request: data,
               mapOk: serviceRes => new ResCustomerDebits
               {
                   IsSuccessful = serviceRes.IsSuccessful,
                   OperationId = serviceRes.OperationId,
                   Errors = serviceRes.Errors,
                   Debits = serviceRes.Debits,
               },
               errorFactory: (code, msg) => new ResCustomerDebits
               {
                   IsSuccessful = false,
                   Errors = new[] { new Error { Code = code, Message = msg } }
               },
               operationName: nameof(GetCustomerDebits)
           ).Result;
        }

     
        public ResAllDebits GetAllDebits(string UrlCGP_DTR, ReqAllDebits data)
        {
            return mClient.PostJsonAsync<ReqAllDebits, ResAllDebits, ResAllDebits>(
              baseUrl: UrlCGP_DTR,
              endpoint: "/GetAllDebits",
              request: data,
              mapOk: serviceRes => new ResAllDebits
              {
                  IsSuccessful = serviceRes.IsSuccessful,
                  OperationId = serviceRes.OperationId,
                  Errors = serviceRes.Errors,
                  Debits = serviceRes.Debits,
                  PagesQty = serviceRes.PagesQty,
                  TransfersQty = serviceRes.TransfersQty
              },
              errorFactory: (code, msg) => new ResAllDebits
              {
                  IsSuccessful = false,
                  Errors = new[] { new Error { Code = code, Message = msg } }
              },
              operationName: nameof(GetAllDebits)
          ).Result;
        }

     
        public ResBase RegisterAuthorization(string UrlCGP_DTR, ReqCustomerServiceAuthorization data)
        {
            return mClient.PostJsonAsync<ReqCustomerServiceAuthorization, ResBase, ResBase>(
              baseUrl: UrlCGP_DTR,
              endpoint: "/RegisterAuthorization",
              request: data,
              mapOk: serviceRes => new ResBase
              {
                  IsSuccessful = serviceRes.IsSuccessful,
                  OperationId = serviceRes.OperationId,
                  Errors = serviceRes.Errors,
              },
              errorFactory: (code, msg) => new ResBase
              {
                  IsSuccessful = false,
                  Errors = new[] { new Error { Code = code, Message = msg } }
              },
              operationName: nameof(RegisterAuthorization)
          ).Result;
        }

  
        public ResBase InactivateAuthorization(string UrlCGP_DTR, ReqCustomerServiceAuthorization data)
        {
            return mClient.PostJsonAsync<ReqCustomerServiceAuthorization, ResBase, ResBase>(
              baseUrl: UrlCGP_DTR,
              endpoint: "/InactivateAuthorization",
              request: data,
              mapOk: serviceRes => new ResBase
              {
                  IsSuccessful = serviceRes.IsSuccessful,
                  OperationId = serviceRes.OperationId,
                  Errors = serviceRes.Errors,
              },
              errorFactory: (code, msg) => new ResBase
              {
                  IsSuccessful = false,
                  Errors = new[] { new Error { Code = code, Message = msg } }
              },
              operationName: nameof(InactivateAuthorization)
          ).Result;
        }

  
        public ResCustomerServiceAuthorization GetStateAuthorization(string UrlCGP_DTR, ReqCustomerServiceAuthorization data)
        {
            return mClient.PostJsonAsync<ReqCustomerServiceAuthorization, ResCustomerServiceAuthorization, ResCustomerServiceAuthorization>(
              baseUrl: UrlCGP_DTR,
              endpoint: "/GetStateAuthorization",
              request: data,
              mapOk: serviceRes => new ResCustomerServiceAuthorization
              {
                  IsSuccessful = serviceRes.IsSuccessful,
                  OperationId = serviceRes.OperationId,
                  Errors = serviceRes.Errors,
                  State = serviceRes.State,
              },
              errorFactory: (code, msg) => new ResCustomerServiceAuthorization
              {
                  IsSuccessful = false,
                  Errors = new[] { new Error { Code = code, Message = msg } }
              },
              operationName: nameof(GetStateAuthorization)
          ).Result;
        }

    }
}
