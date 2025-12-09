using Newtonsoft.Json;
using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;
using System.Net.Http;
using System.Text;

namespace Galileo_API.DataBaseTier
{
    /// <summary>
    /// Cliente para consumir los métodos del Servicio DTR de KINDO (Sección 6 del documento).
    /// Refactorizado para evitar duplicación Sonar.
    /// </summary>
    public class SinpeGalileoDtr
    {
        private readonly HttpClient _client;
        private const string MediaTypeJson = "application/json";
        private const string MsjError = "Respuesta inválida del servicio.";

        // Ideal: HttpClient por DI (IHttpClientFactory). Mantengo firma simple.
        public SinpeGalileoDtr(IConfiguration config, HttpClient? client = null)
        {
            _client = client ?? new HttpClient();
        }

        // -----------------------------
        // Helper genérico central
        // -----------------------------
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

        // -----------------------------
        // 6.x IsServiceAvailable
        // -----------------------------
        public Task<ResServiceAvailable> IsServiceAvailableAsync(string urlCGP_DTR, ReqBase context)
        {
            return PostJsonAsync<ReqBase, ResServiceAvailable>(
                urlCGP_DTR,
                "/IsServiceAvailable",
                context,
                onHttpError: (code, _) => new ResServiceAvailable
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = code, Message = "El servicio no está disponible." }
                    }
                },
                onDeserializeNull: _ => new ResServiceAvailable
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = -1, Message = MsjError }
                    }
                },
                onException: ex => new ResServiceAvailable
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = -1, Message = $"Error en IsServiceAvailable: {ex.Message}" }
                    }
                }
            );
        }

        // Backward compatible sync wrapper si lo necesitás:
        public ResServiceAvailable IsServiceAvailable(string urlCGP_DTR, ReqBase context)
            => IsServiceAvailableAsync(urlCGP_DTR, context).GetAwaiter().GetResult();

        // -----------------------------
        // 6.x GetAccountInfo
        // -----------------------------
        public Task<ResAccountInfo> GetAccountInfoAsync(string urlCGP_DTR, ReqAccountInfo accountData)
        {
            return PostJsonAsync<ReqAccountInfo, ResAccountInfo>(
                urlCGP_DTR,
                "/GetAccountInfo",
                accountData,
                onHttpError: (code, _) => new ResAccountInfo
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = code, Message = "No se pudo obtener la información de la cuenta." }
                    }
                },
                onDeserializeNull: _ => new ResAccountInfo
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = -1, Message = MsjError }
                    }
                },
                onException: ex => new ResAccountInfo
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = -1, Message = $"Error en GetAccountInfo: {ex.Message}" }
                    }
                }
            );
        }

        public ResAccountInfo GetAccountInfo(string urlCGP_DTR, ReqAccountInfo accountData)
            => GetAccountInfoAsync(urlCGP_DTR, accountData).GetAwaiter().GetResult();

        // -----------------------------
        // 6.x SendDebit
        // -----------------------------
        public Task<ResDTRSending> SendDebitAsync(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqDTRSending debitData)
        {
            var baseUrl = parametros.Result.Item1.UrlCGP_DTR;

            return PostJsonAsync<ReqDTRSending, ResDTRSending>(
                baseUrl,
                "/SendDebit",
                debitData,
                onHttpError: (code, _) => new ResDTRSending
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = code, Message = "No se pudo enviar el débito." }
                    }
                },
                onDeserializeNull: _ => new ResDTRSending
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = -1, Message = MsjError }
                    }
                },
                onException: ex => new ResDTRSending
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = -1, Message = $"Error en SendDebit: {ex.Message}" }
                    }
                }
            );
        }

        public ResDTRSending SendDebit(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqDTRSending debitData)
            => SendDebitAsync(parametros, debitData).GetAwaiter().GetResult();

        // -----------------------------
        // 6.x GetDebitResult
        // -----------------------------
        public Task<ResDTRSending> GetDebitResultAsync(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqDTRInfoChannelRef debitData)
        {
            var baseUrl = parametros.Result.Item1.UrlCGP_DTR;

            return PostJsonAsync<ReqDTRInfoChannelRef, ResDTRSending>(
                baseUrl,
                "/GetDebitResult",
                debitData,
                onHttpError: (code, _) => new ResDTRSending
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = code, Message = "No se pudo obtener el resultado del DTR." }
                    }
                },
                onDeserializeNull: _ => new ResDTRSending
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = -1, Message =MsjError }
                    }
                },
                onException: ex => new ResDTRSending
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = -1, Message = $"Error en GetDebitResult: {ex.Message}" }
                    }
                }
            );
        }

        public ResDTRSending GetDebitResult(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqDTRInfoChannelRef debitData)
            => GetDebitResultAsync(parametros, debitData).GetAwaiter().GetResult();

        // -----------------------------
        // 6.x GetDebitDataByChannelRef
        // -----------------------------
        public Task<ResDTRInfo> GetDebitDataByChannelRefAsync(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqDTRInfoChannelRef debitData)
        {
            var baseUrl = parametros.Result.Item1.UrlCGP_DTR;

            return PostJsonAsync<ReqDTRInfoChannelRef, ResDTRInfo>(
                baseUrl,
                "/GetDebitDataByChannelRef",
                debitData,
                onHttpError: (code, _) => new ResDTRInfo
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = code, Message = "No se pudo consultar el DTR por referencia de canal." }
                    }
                },
                onDeserializeNull: _ => new ResDTRInfo
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = -1, Message = MsjError }
                    }
                },
                onException: ex => new ResDTRInfo
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = -1, Message = $"Error en GetDebitDataByChannelRef: {ex.Message}" }
                    }
                }
            );
        }

        public ResDTRInfo GetDebitDataByChannelRef(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqDTRInfoChannelRef debitData)
            => GetDebitDataByChannelRefAsync(parametros, debitData).GetAwaiter().GetResult();

        // -----------------------------
        // 6.x GetDebitDataBySINPERef
        // -----------------------------
        public Task<ResDTRInfo> GetDebitDataBySINPERefAsync(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqDTRInfoSINPERef debitData)
        {
            var baseUrl = parametros.Result.Item1.UrlCGP_DTR;

            return PostJsonAsync<ReqDTRInfoSINPERef, ResDTRInfo>(
                baseUrl,
                "/GetDebitDataBySINPERef",
                debitData,
                onHttpError: (code, _) => new ResDTRInfo
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = code, Message = "No se pudo consultar el DTR por referencia SINPE." }
                    }
                },
                onDeserializeNull: _ => new ResDTRInfo
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = -1, Message = MsjError }
                    }
                },
                onException: ex => new ResDTRInfo
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = -1, Message = $"Error en GetDebitDataBySINPERef: {ex.Message}" }
                    }
                }
            );
        }

        public ResDTRInfo GetDebitDataBySINPERef(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqDTRInfoSINPERef debitData)
            => GetDebitDataBySINPERefAsync(parametros, debitData).GetAwaiter().GetResult();

        // -----------------------------
        // 6.x SendBatch
        // -----------------------------
        public Task<ResBatchSending> SendBatchAsync(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqBatchSending batchData)
        {
            var baseUrl = parametros.Result.Item1.UrlCGP_DTR;

            return PostJsonAsync<ReqBatchSending, ResBatchSending>(
                baseUrl,
                "/SendBatch",
                batchData,
                onHttpError: (code, _) => new ResBatchSending
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = code, Message = "Error al enviar el lote de DTRs." }
                    }
                },
                onDeserializeNull: _ => new ResBatchSending
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = -1, Message = MsjError }
                    }
                },
                onException: ex => new ResBatchSending
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = -1, Message = $"Error en SendBatch: {ex.Message}" }
                    }
                }
            );
        }

        public ResBatchSending SendBatch(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqBatchSending batchData)
            => SendBatchAsync(parametros, batchData).GetAwaiter().GetResult();

        // -----------------------------
        // 6.x GetBatchState
        // -----------------------------
        public Task<ResBatchState> GetBatchStateAsync(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqBatchState batchData)
        {
            var baseUrl = parametros.Result.Item1.UrlCGP_DTR;

            return PostJsonAsync<ReqBatchState, ResBatchState>(
                baseUrl,
                "/GetBatchState",
                batchData,
                onHttpError: (code, _) => new ResBatchState
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = code, Message = "No se pudo obtener el estado del lote." }
                    }
                },
                onDeserializeNull: _ => new ResBatchState
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = -1, Message = MsjError }
                    }
                },
                onException: ex => new ResBatchState
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = -1, Message = $"Error en GetBatchState: {ex.Message}" }
                    }
                }
            );
        }

        public ResBatchState GetBatchState(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqBatchState batchData)
            => GetBatchStateAsync(parametros, batchData).GetAwaiter().GetResult();

        // -----------------------------
        // 6.x GetCustomerDebits
        // -----------------------------
        public Task<ResCustomerDebits> GetCustomerDebitsAsync(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqCustomerDebits consultData)
        {
            var baseUrl = parametros.Result.Item1.UrlCGP_DTR;

            return PostJsonAsync<ReqCustomerDebits, ResCustomerDebits>(
                baseUrl,
                "/GetCustomerDebits",
                consultData,
                onHttpError: (code, _) => new ResCustomerDebits
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = code, Message = "No se pudo obtener la lista de débitos del cliente." }
                    }
                },
                onDeserializeNull: _ => new ResCustomerDebits
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = -1, Message =MsjError }
                    }
                },
                onException: ex => new ResCustomerDebits
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = -1, Message = $"Error en GetCustomerDebits: {ex.Message}" }
                    }
                }
            );
        }

        public ResCustomerDebits GetCustomerDebits(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqCustomerDebits consultData)
            => GetCustomerDebitsAsync(parametros, consultData).GetAwaiter().GetResult();

        // -----------------------------
        // 6.x GetAllDebits
        // -----------------------------
        public Task<ResAllDebits> GetAllDebitsAsync(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqAllDebits filterData)
        {
            var baseUrl = parametros.Result.Item1.UrlCGP_DTR;

            return PostJsonAsync<ReqAllDebits, ResAllDebits>(
                baseUrl,
                "/GetAllDebits",
                filterData,
                onHttpError: (code, _) => new ResAllDebits
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = code, Message = "No se pudo obtener la lista de todos los débitos." }
                    }
                },
                onDeserializeNull: _ => new ResAllDebits
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = -1, Message = MsjError }
                    }
                },
                onException: ex => new ResAllDebits
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = -1, Message = $"Error en GetAllDebits: {ex.Message}" }
                    }
                }
            );
        }

        public ResAllDebits GetAllDebits(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqAllDebits filterData)
            => GetAllDebitsAsync(parametros, filterData).GetAwaiter().GetResult();

        // -----------------------------
        // 6.x RegisterAuthorization
        // -----------------------------
        public Task<ResBase> RegisterAuthorizationAsync(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqCustomerServiceAuthorization authorizationData)
        {
            var baseUrl = parametros.Result.Item1.UrlCGP_DTR;

            return PostJsonAsync<ReqCustomerServiceAuthorization, ResBase>(
                baseUrl,
                "/RegisterAuthorization",
                authorizationData,
                onHttpError: (code, _) => new ResBase
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = code, Message = "No se pudo registrar la autorización del cliente." }
                    }
                },
                onDeserializeNull: _ => new ResBase
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = -1, Message = MsjError }
                    }
                },
                onException: ex => new ResBase
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = -1, Message = $"Error en RegisterAuthorization: {ex.Message}" }
                    }
                }
            );
        }

        public ResBase RegisterAuthorization(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqCustomerServiceAuthorization authorizationData)
            => RegisterAuthorizationAsync(parametros, authorizationData).GetAwaiter().GetResult();

        // -----------------------------
        // 6.x InactivateAuthorization
        // -----------------------------
        public Task<ResBase> InactivateAuthorizationAsync(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqCustomerServiceAuthorization authorizationData)
        {
            var baseUrl = parametros.Result.Item1.UrlCGP_DTR;

            return PostJsonAsync<ReqCustomerServiceAuthorization, ResBase>(
                baseUrl,
                "/InactivateAuthorization",
                authorizationData,
                onHttpError: (code, _) => new ResBase
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = code, Message = "No se pudo inactivar la autorización del cliente." }
                    }
                },
                onDeserializeNull: _ => new ResBase
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = -1, Message = MsjError }
                    }
                },
                onException: ex => new ResBase
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = -1, Message = $"Error en InactivateAuthorization: {ex.Message}" }
                    }
                }
            );
        }

        public ResBase InactivateAuthorization(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqCustomerServiceAuthorization authorizationData)
            => InactivateAuthorizationAsync(parametros, authorizationData).GetAwaiter().GetResult();

        // -----------------------------
        // 6.x GetStateAuthorization
        // -----------------------------
        public Task<ResCustomerServiceAuthorization> GetStateAuthorizationAsync(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqCustomerServiceAuthorization authorizationData)
        {
            var baseUrl = parametros.Result.Item1.UrlCGP_DTR;

            return PostJsonAsync<ReqCustomerServiceAuthorization, ResCustomerServiceAuthorization>(
                baseUrl,
                "/GetStateAuthorization",
                authorizationData,
                onHttpError: (code, _) => new ResCustomerServiceAuthorization
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = code, Message = "No se pudo obtener el estado de autorización del cliente." }
                    }
                },
                onDeserializeNull: _ => new ResCustomerServiceAuthorization
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = -1, Message = MsjError }
                    }
                },
                onException: ex => new ResCustomerServiceAuthorization
                {
                    IsSuccessful = false,
                    Errors = new[]
                    {
                        new Error { Code = -1, Message = $"Error en GetStateAuthorization: {ex.Message}" }
                    }
                }
            );
        }

        public ResCustomerServiceAuthorization GetStateAuthorization(
            ErrorDto<(ParametrosSinpe, HttpClient)> parametros,
            ReqCustomerServiceAuthorization authorizationData)
            => GetStateAuthorizationAsync(parametros, authorizationData).GetAwaiter().GetResult();
    }
}
