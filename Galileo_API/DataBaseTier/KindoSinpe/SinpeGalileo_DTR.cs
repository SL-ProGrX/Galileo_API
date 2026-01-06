using Newtonsoft.Json;
using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;
using System.Net.Http;
using System.Text;

namespace Galileo_API.DataBaseTier
{
    public class SinpeGalileoDtr
    {
        private readonly HttpClient _client;
        private const string MediaTypeJson = "application/json";
        private const string MsjError = "Respuesta inválida del servicio.";

        public SinpeGalileoDtr(IConfiguration config, HttpClient? client = null)
        {
            _client = client ?? new HttpClient();
        }

        // --------------------------------------------------------------------
        // Helpers de creación de errores
        // --------------------------------------------------------------------
        private static TRes BuildHttpError<TRes>(int code, string msg)
            where TRes : ResBase, new()
        {
            return new TRes
            {
                IsSuccessful = false,
                Errors = new[] { new Error { Code = code, Message = msg } }
            };
        }

        private static TRes BuildDeserializeError<TRes>(string msg)
            where TRes : ResBase, new()
        {
            return new TRes
            {
                IsSuccessful = false,
                Errors = new[] { new Error { Code = -1, Message = msg } }
            };
        }

        private static TRes BuildExceptionError<TRes>(string operation, Exception ex)
            where TRes : ResBase, new()
        {
            return new TRes
            {
                IsSuccessful = false,
                Errors = new[] { new Error { Code = -1, Message = $"Error en {operation}: {ex.Message}" } }
            };
        }

        private static bool GuardUrlOrError<TRes>(
            string url,
            string operation,
            out TRes errorResult)
            where TRes : ResBase, new()
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                errorResult = BuildDeserializeError<TRes>($"URL requerida para {operation} (vacía o nula).");
                return false;
            }

            errorResult = default!;
            return true;
        }

        // --------------------------------------------------------------------
        // Helper genérico para llamadas JSON
        // --------------------------------------------------------------------
        private async Task<TRes> PostJsonAsync<TReq, TRes>(
            string baseUrl,
            string endpoint,
            TReq data,
            Func<int, string, TRes> onHttpError,
            Func<string, TRes> onDeserializeNull,
            Func<Exception, TRes> onException)
        {
            try
            {
                // baseUrl ya viene validado por GuardUrlOrError
                var json = JsonConvert.SerializeObject(data);
                using var content = new StringContent(json, Encoding.UTF8, MediaTypeJson);

                using var response = await _client
                    .PostAsync(CombineUrl(baseUrl, endpoint), content)
                    .ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                    return onHttpError((int)response.StatusCode, response.ReasonPhrase ?? "HTTP Error");

                var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                // Si el servicio devuelve vacío, controlamos acá también.
                if (string.IsNullOrWhiteSpace(jsonResponse))
                    return onDeserializeNull(jsonResponse);

                var result = JsonConvert.DeserializeObject<TRes>(jsonResponse);
                return result ?? onDeserializeNull(jsonResponse);
            }
            catch (Exception ex)
            {
                return onException(ex);
            }
        }

        private static string CombineUrl(string baseUrl, string endpoint)
            => $"{baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";

        // --------------------------------------------------------------------
        // MÉTODOS DTR
        // --------------------------------------------------------------------

        public Task<ResServiceAvailable> IsServiceAvailableAsync(string url, ReqBase ctx)
        {
            if (!GuardUrlOrError(url, "IsServiceAvailable", out ResServiceAvailable err))
                return Task.FromResult(err);

            return PostJsonAsync<ReqBase, ResServiceAvailable>(
                url,
                "/IsServiceAvailable",
                ctx,
                (code, _) => BuildHttpError<ResServiceAvailable>(code, "El servicio no está disponible."),
                _ => BuildDeserializeError<ResServiceAvailable>(MsjError),
                ex => BuildExceptionError<ResServiceAvailable>("IsServiceAvailable", ex)
            );
        }

        public ResServiceAvailable IsServiceAvailable(string url, ReqBase ctx)
            => IsServiceAvailableAsync(url, ctx).GetAwaiter().GetResult();

        // --------------------------------------------------------------

        public Task<ResAccountInfo> GetAccountInfoAsync(string url, ReqAccountInfo data)
        {
            if (!GuardUrlOrError(url, "GetAccountInfo", out ResAccountInfo err))
                return Task.FromResult(err);

            return PostJsonAsync<ReqAccountInfo, ResAccountInfo>(
                url,
                "/GetAccountInfo",
                data,
                (code, _) => BuildHttpError<ResAccountInfo>(code, "No se pudo obtener la información de la cuenta."),
                _ => BuildDeserializeError<ResAccountInfo>(MsjError),
                ex => BuildExceptionError<ResAccountInfo>("GetAccountInfo", ex)
            );
        }

        public ResAccountInfo GetAccountInfo(string url, ReqAccountInfo data)
            => GetAccountInfoAsync(url, data).GetAwaiter().GetResult();

        // --------------------------------------------------------------

        public Task<ResDTRSending> SendDebitAsync(string url, ReqDTRSending data)
        {
            if (!GuardUrlOrError(url, "SendDebit", out ResDTRSending err))
                return Task.FromResult(err);

            return PostJsonAsync<ReqDTRSending, ResDTRSending>(
                url,
                "/SendDebit",
                data,
                (code, _) => BuildHttpError<ResDTRSending>(code, "No se pudo enviar el débito."),
                _ => BuildDeserializeError<ResDTRSending>(MsjError),
                ex => BuildExceptionError<ResDTRSending>("SendDebit", ex)
            );
        }

        public ResDTRSending SendDebit(string url, ReqDTRSending d)
            => SendDebitAsync(url, d).GetAwaiter().GetResult();

        // --------------------------------------------------------------

        public Task<ResDTRSending> GetDebitResultAsync(string url, ReqDTRInfoChannelRef data)
        {
            if (!GuardUrlOrError(url, "GetDebitResult", out ResDTRSending err))
                return Task.FromResult(err);

            return PostJsonAsync<ReqDTRInfoChannelRef, ResDTRSending>(
                url,
                "/GetDebitResult",
                data,
                (code, _) => BuildHttpError<ResDTRSending>(code, "No se pudo obtener el resultado del DTR."),
                _ => BuildDeserializeError<ResDTRSending>(MsjError),
                ex => BuildExceptionError<ResDTRSending>("GetDebitResult", ex)
            );
        }

        public ResDTRSending GetDebitResult(string url, ReqDTRInfoChannelRef d)
            => GetDebitResultAsync(url, d).GetAwaiter().GetResult();

        // --------------------------------------------------------------

        public Task<ResDTRInfo> GetDebitDataByChannelRefAsync(string url, ReqDTRInfoChannelRef data)
        {
            if (!GuardUrlOrError(url, "GetDebitDataByChannelRef", out ResDTRInfo err))
                return Task.FromResult(err);

            return PostJsonAsync<ReqDTRInfoChannelRef, ResDTRInfo>(
                url,
                "/GetDebitDataByChannelRef",
                data,
                (code, _) => BuildHttpError<ResDTRInfo>(code, "No se pudo consultar el DTR por referencia de canal."),
                _ => BuildDeserializeError<ResDTRInfo>(MsjError),
                ex => BuildExceptionError<ResDTRInfo>("GetDebitDataByChannelRef", ex)
            );
        }

        public ResDTRInfo GetDebitDataByChannelRef(string url, ReqDTRInfoChannelRef d)
            => GetDebitDataByChannelRefAsync(url, d).GetAwaiter().GetResult();

        // --------------------------------------------------------------

        public Task<ResDTRInfo> GetDebitDataBySINPERefAsync(string url, ReqDTRInfoSINPERef data)
        {
            if (!GuardUrlOrError(url, "GetDebitDataBySINPERef", out ResDTRInfo err))
                return Task.FromResult(err);

            return PostJsonAsync<ReqDTRInfoSINPERef, ResDTRInfo>(
                url,
                "/GetDebitDataBySINPERef",
                data,
                (code, _) => BuildHttpError<ResDTRInfo>(code, "No se pudo consultar el DTR por referencia SINPE."),
                _ => BuildDeserializeError<ResDTRInfo>(MsjError),
                ex => BuildExceptionError<ResDTRInfo>("GetDebitDataBySINPERef", ex)
            );
        }

        public ResDTRInfo GetDebitDataBySINPERef(string url, ReqDTRInfoSINPERef d)
            => GetDebitDataBySINPERefAsync(url, d).GetAwaiter().GetResult();

        // --------------------------------------------------------------

        public Task<ResBatchSending> SendBatchAsync(string url, ReqBatchSending data)
        {
            if (!GuardUrlOrError(url, "SendBatch", out ResBatchSending err))
                return Task.FromResult(err);

            return PostJsonAsync<ReqBatchSending, ResBatchSending>(
                url,
                "/SendBatch",
                data,
                (code, _) => BuildHttpError<ResBatchSending>(code, "Error al enviar el lote de DTRs."),
                _ => BuildDeserializeError<ResBatchSending>(MsjError),
                ex => BuildExceptionError<ResBatchSending>("SendBatch", ex)
            );
        }

        public ResBatchSending SendBatch(string url, ReqBatchSending d)
            => SendBatchAsync(url, d).GetAwaiter().GetResult();

        // --------------------------------------------------------------

        public Task<ResBatchState> GetBatchStateAsync(string url, ReqBatchState data)
        {
            if (!GuardUrlOrError(url, "GetBatchState", out ResBatchState err))
                return Task.FromResult(err);

            return PostJsonAsync<ReqBatchState, ResBatchState>(
                url,
                "/GetBatchState",
                data,
                (code, _) => BuildHttpError<ResBatchState>(code, "No se pudo obtener el estado del lote."),
                _ => BuildDeserializeError<ResBatchState>(MsjError),
                ex => BuildExceptionError<ResBatchState>("GetBatchState", ex)
            );
        }

        public ResBatchState GetBatchState(string url, ReqBatchState d)
            => GetBatchStateAsync(url, d).GetAwaiter().GetResult();

        // --------------------------------------------------------------

        public Task<ResCustomerDebits> GetCustomerDebitsAsync(string url, ReqCustomerDebits data)
        {
            if (!GuardUrlOrError(url, "GetCustomerDebits", out ResCustomerDebits err))
                return Task.FromResult(err);

            return PostJsonAsync<ReqCustomerDebits, ResCustomerDebits>(
                url,
                "/GetCustomerDebits",
                data,
                (code, _) => BuildHttpError<ResCustomerDebits>(code, "No se pudo obtener la lista de débitos del cliente."),
                _ => BuildDeserializeError<ResCustomerDebits>(MsjError),
                ex => BuildExceptionError<ResCustomerDebits>("GetCustomerDebits", ex)
            );
        }

        public ResCustomerDebits GetCustomerDebits(string url, ReqCustomerDebits d)
            => GetCustomerDebitsAsync(url, d).GetAwaiter().GetResult();

        // --------------------------------------------------------------

        public Task<ResAllDebits> GetAllDebitsAsync(string url, ReqAllDebits data)
        {
            if (!GuardUrlOrError(url, "GetAllDebits", out ResAllDebits err))
                return Task.FromResult(err);

            return PostJsonAsync<ReqAllDebits, ResAllDebits>(
                url,
                "/GetAllDebits",
                data,
                (code, _) => BuildHttpError<ResAllDebits>(code, "No se pudo obtener la lista de todos los débitos."),
                _ => BuildDeserializeError<ResAllDebits>(MsjError),
                ex => BuildExceptionError<ResAllDebits>("GetAllDebits", ex)
            );
        }

        public ResAllDebits GetAllDebits(string url, ReqAllDebits d)
            => GetAllDebitsAsync(url, d).GetAwaiter().GetResult();

        // --------------------------------------------------------------

        public Task<ResBase> RegisterAuthorizationAsync(string url, ReqCustomerServiceAuthorization data)
        {
            if (!GuardUrlOrError(url, "RegisterAuthorization", out ResBase err))
                return Task.FromResult(err);

            return PostJsonAsync<ReqCustomerServiceAuthorization, ResBase>(
                url,
                "/RegisterAuthorization",
                data,
                (code, _) => BuildHttpError<ResBase>(code, "No se pudo registrar la autorización del cliente."),
                _ => BuildDeserializeError<ResBase>(MsjError),
                ex => BuildExceptionError<ResBase>("RegisterAuthorization", ex)
            );
        }

        public ResBase RegisterAuthorization(string url, ReqCustomerServiceAuthorization d)
            => RegisterAuthorizationAsync(url, d).GetAwaiter().GetResult();

        // --------------------------------------------------------------

        public Task<ResBase> InactivateAuthorizationAsync(string url, ReqCustomerServiceAuthorization data)
        {
            if (!GuardUrlOrError(url, "InactivateAuthorization", out ResBase err))
                return Task.FromResult(err);

            return PostJsonAsync<ReqCustomerServiceAuthorization, ResBase>(
                url,
                "/InactivateAuthorization",
                data,
                (code, _) => BuildHttpError<ResBase>(code, "No se pudo inactivar la autorización del cliente."),
                _ => BuildDeserializeError<ResBase>(MsjError),
                ex => BuildExceptionError<ResBase>("InactivateAuthorization", ex)
            );
        }

        public ResBase InactivateAuthorization(string url, ReqCustomerServiceAuthorization d)
            => InactivateAuthorizationAsync(url, d).GetAwaiter().GetResult();

        // --------------------------------------------------------------

        public Task<ResCustomerServiceAuthorization> GetStateAuthorizationAsync(string url, ReqCustomerServiceAuthorization data)
        {
            if (!GuardUrlOrError(url, "GetStateAuthorization", out ResCustomerServiceAuthorization err))
                return Task.FromResult(err);

            return PostJsonAsync<ReqCustomerServiceAuthorization, ResCustomerServiceAuthorization>(
                url,
                "/GetStateAuthorization",
                data,
                (code, _) => BuildHttpError<ResCustomerServiceAuthorization>(code, "No se pudo obtener el estado de autorización del cliente."),
                _ => BuildDeserializeError<ResCustomerServiceAuthorization>(MsjError),
                ex => BuildExceptionError<ResCustomerServiceAuthorization>("GetStateAuthorization", ex)
            );
        }

        public ResCustomerServiceAuthorization GetStateAuthorization(string url, ReqCustomerServiceAuthorization d)
            => GetStateAuthorizationAsync(url, d).GetAwaiter().GetResult();
    }
}
