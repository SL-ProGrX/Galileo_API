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
        // Helpers de creación de errores (elimina 70% de duplicación)
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

        // --------------------------------------------------------------------
        // Helper genérico para llamadas JSON (ya existía)
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
                var json = JsonConvert.SerializeObject(data);
                using var content = new StringContent(json, Encoding.UTF8, MediaTypeJson);

                using var response = await _client.PostAsync(
                    CombineUrl(baseUrl, endpoint),
                    content
                ).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                    return onHttpError((int)response.StatusCode, response.ReasonPhrase ?? "HTTP Error");

                var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var result = JsonConvert.DeserializeObject<TRes>(jsonResponse);

                return result ?? onDeserializeNull(jsonResponse);
            }
            catch (Exception ex)
            {
                return onException(ex);
            }
        }

        private static string CombineUrl(string baseUrl, string endpoint)
            => $"{baseUrl?.TrimEnd('/')}/{endpoint?.TrimStart('/')}";

        // --------------------------------------------------------------------
        // MÉTODOS DTR REFACTORIZADOS
        // --------------------------------------------------------------------

        public Task<ResServiceAvailable> IsServiceAvailableAsync(string url, ReqBase ctx)
        {
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

        public Task<ResDTRSending> SendDebitAsync(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqDTRSending data)
        {
            var url = parametros.Result.Item1.UrlCGP_DTR;

            return PostJsonAsync<ReqDTRSending, ResDTRSending>(
                url,
                "/SendDebit",
                data,
                (code, _) => BuildHttpError<ResDTRSending>(code, "No se pudo enviar el débito."),
                _ => BuildDeserializeError<ResDTRSending>(MsjError),
                ex => BuildExceptionError<ResDTRSending>("SendDebit", ex)
            );
        }

        public ResDTRSending SendDebit(ErrorDto<(ParametrosSinpe, HttpClient)> p, ReqDTRSending d)
            => SendDebitAsync(p, d).GetAwaiter().GetResult();

        // --------------------------------------------------------------

        public Task<ResDTRSending> GetDebitResultAsync(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqDTRInfoChannelRef data)
        {
            var url = parametros.Result.Item1.UrlCGP_DTR;

            return PostJsonAsync<ReqDTRInfoChannelRef, ResDTRSending>(
                url,
                "/GetDebitResult",
                data,
                (code, _) => BuildHttpError<ResDTRSending>(code, "No se pudo obtener el resultado del DTR."),
                _ => BuildDeserializeError<ResDTRSending>(MsjError),
                ex => BuildExceptionError<ResDTRSending>("GetDebitResult", ex)
            );
        }

        public ResDTRSending GetDebitResult(ErrorDto<(ParametrosSinpe, HttpClient)> p, ReqDTRInfoChannelRef d)
            => GetDebitResultAsync(p, d).GetAwaiter().GetResult();

        // --------------------------------------------------------------

        public Task<ResDTRInfo> GetDebitDataByChannelRefAsync(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqDTRInfoChannelRef data)
        {
            var url = parametros.Result.Item1.UrlCGP_DTR;

            return PostJsonAsync<ReqDTRInfoChannelRef, ResDTRInfo>(
                url,
                "/GetDebitDataByChannelRef",
                data,
                (code, _) => BuildHttpError<ResDTRInfo>(code, "No se pudo consultar el DTR por referencia de canal."),
                _ => BuildDeserializeError<ResDTRInfo>(MsjError),
                ex => BuildExceptionError<ResDTRInfo>("GetDebitDataByChannelRef", ex)
            );
        }

        public ResDTRInfo GetDebitDataByChannelRef(ErrorDto<(ParametrosSinpe, HttpClient)> p, ReqDTRInfoChannelRef d)
            => GetDebitDataByChannelRefAsync(p, d).GetAwaiter().GetResult();

        // --------------------------------------------------------------

        public Task<ResDTRInfo> GetDebitDataBySINPERefAsync(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqDTRInfoSINPERef data)
        {
            var url = parametros.Result.Item1.UrlCGP_DTR;

            return PostJsonAsync<ReqDTRInfoSINPERef, ResDTRInfo>(
                url,
                "/GetDebitDataBySINPERef",
                data,
            (code, _) => BuildHttpError<ResDTRInfo>(code, "No se pudo consultar el DTR por referencia SINPE."),
                _ => BuildDeserializeError<ResDTRInfo>(MsjError),
                ex => BuildExceptionError<ResDTRInfo>("GetDebitDataBySINPERef", ex)
            );
        }

        public ResDTRInfo GetDebitDataBySINPERef(ErrorDto<(ParametrosSinpe, HttpClient)> p, ReqDTRInfoSINPERef d)
            => GetDebitDataBySINPERefAsync(p, d).GetAwaiter().GetResult();

        // --------------------------------------------------------------

        public Task<ResBatchSending> SendBatchAsync(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqBatchSending data)
        {
            var url = parametros.Result.Item1.UrlCGP_DTR;

            return PostJsonAsync<ReqBatchSending, ResBatchSending>(
                url,
                "/SendBatch",
                data,
                (code, _) => BuildHttpError<ResBatchSending>(code, "Error al enviar el lote de DTRs."),
                _ => BuildDeserializeError<ResBatchSending>(MsjError),
                ex => BuildExceptionError<ResBatchSending>("SendBatch", ex)
            );
        }

        public ResBatchSending SendBatch(ErrorDto<(ParametrosSinpe, HttpClient)> p, ReqBatchSending d)
            => SendBatchAsync(p, d).GetAwaiter().GetResult();

        // --------------------------------------------------------------

        public Task<ResBatchState> GetBatchStateAsync(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqBatchState data)
        {
            var url = parametros.Result.Item1.UrlCGP_DTR;

            return PostJsonAsync<ReqBatchState, ResBatchState>(
                url,
                "/GetBatchState",
                data,
                (code, _) => BuildHttpError<ResBatchState>(code, "No se pudo obtener el estado del lote."),
                _ => BuildDeserializeError<ResBatchState>(MsjError),
                ex => BuildExceptionError<ResBatchState>("GetBatchState", ex)
            );
        }

        public ResBatchState GetBatchState(ErrorDto<(ParametrosSinpe, HttpClient)> p, ReqBatchState d)
            => GetBatchStateAsync(p, d).GetAwaiter().GetResult();

        // --------------------------------------------------------------

        public Task<ResCustomerDebits> GetCustomerDebitsAsync(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqCustomerDebits data)
        {
            var url = parametros.Result.Item1.UrlCGP_DTR;

            return PostJsonAsync<ReqCustomerDebits, ResCustomerDebits>(
                url,
                "/GetCustomerDebits",
                data,
                (code, _) => BuildHttpError<ResCustomerDebits>(code, "No se pudo obtener la lista de débitos del cliente."),
                _ => BuildDeserializeError<ResCustomerDebits>(MsjError),
                ex => BuildExceptionError<ResCustomerDebits>("GetCustomerDebits", ex)
            );
        }

        public ResCustomerDebits GetCustomerDebits(ErrorDto<(ParametrosSinpe, HttpClient)> p, ReqCustomerDebits d)
            => GetCustomerDebitsAsync(p, d).GetAwaiter().GetResult();

        // --------------------------------------------------------------

        public Task<ResAllDebits> GetAllDebitsAsync(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqAllDebits data)
        {
            var url = parametros.Result.Item1.UrlCGP_DTR;

            return PostJsonAsync<ReqAllDebits, ResAllDebits>(
                url,
                "/GetAllDebits",
                data,
                (code, _) => BuildHttpError<ResAllDebits>(code, "No se pudo obtener la lista de todos los débitos."),
                _ => BuildDeserializeError<ResAllDebits>(MsjError),
                ex => BuildExceptionError<ResAllDebits>("GetAllDebits", ex)
            );
        }

        public ResAllDebits GetAllDebits(ErrorDto<(ParametrosSinpe, HttpClient)> p, ReqAllDebits d)
            => GetAllDebitsAsync(p, d).GetAwaiter().GetResult();

        // --------------------------------------------------------------

        public Task<ResBase> RegisterAuthorizationAsync(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqCustomerServiceAuthorization data)
        {
            var url = parametros.Result.Item1.UrlCGP_DTR;

            return PostJsonAsync<ReqCustomerServiceAuthorization, ResBase>(
                url,
                "/RegisterAuthorization",
                data,
                (code, _) => BuildHttpError<ResBase>(code, "No se pudo registrar la autorización del cliente."),
                _ => BuildDeserializeError<ResBase>(MsjError),
                ex => BuildExceptionError<ResBase>("RegisterAuthorization", ex)
            );
        }

        public ResBase RegisterAuthorization(ErrorDto<(ParametrosSinpe, HttpClient)> p, ReqCustomerServiceAuthorization d)
            => RegisterAuthorizationAsync(p, d).GetAwaiter().GetResult();

        // --------------------------------------------------------------

        public Task<ResBase> InactivateAuthorizationAsync(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqCustomerServiceAuthorization data)
        {
            var url = parametros.Result.Item1.UrlCGP_DTR;

            return PostJsonAsync<ReqCustomerServiceAuthorization, ResBase>(
                url,
                "/InactivateAuthorization",
                data,
                (code, _) => BuildHttpError<ResBase>(code, "No se pudo inactivar la autorización del cliente."),
                _ => BuildDeserializeError<ResBase>(MsjError),
                ex => BuildExceptionError<ResBase>("InactivateAuthorization", ex)
            );
        }

        public ResBase InactivateAuthorization(ErrorDto<(ParametrosSinpe, HttpClient)> p, ReqCustomerServiceAuthorization d)
            => InactivateAuthorizationAsync(p, d).GetAwaiter().GetResult();

        // --------------------------------------------------------------

        public Task<ResCustomerServiceAuthorization> GetStateAuthorizationAsync(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqCustomerServiceAuthorization data)
        {
            var url = parametros.Result.Item1.UrlCGP_DTR;

            return PostJsonAsync<ReqCustomerServiceAuthorization, ResCustomerServiceAuthorization>(
                url,
                "/GetStateAuthorization",
                data,
                (code, _) => BuildHttpError<ResCustomerServiceAuthorization>(code, "No se pudo obtener el estado de autorización del cliente."),
                _ => BuildDeserializeError<ResCustomerServiceAuthorization>(MsjError),
                ex => BuildExceptionError<ResCustomerServiceAuthorization>("GetStateAuthorization", ex)
            );
        }

        public ResCustomerServiceAuthorization GetStateAuthorization(ErrorDto<(ParametrosSinpe, HttpClient)> p, ReqCustomerServiceAuthorization d)
            => GetStateAuthorizationAsync(p, d).GetAwaiter().GetResult();
    }
}
