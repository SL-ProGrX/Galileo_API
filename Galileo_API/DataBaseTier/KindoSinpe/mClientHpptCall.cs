using Newtonsoft.Json;
using System.Text;

namespace Galileo_API.DataBaseTier.KindoSinpe
{
    public class MClientHpptCall
    {
        private readonly HttpClient _client;

        public MClientHpptCall(HttpClient? client = null)
        {
            _client = client ?? new HttpClient();
        }

        public async Task<TOut> PostJsonAsync<TReq, TServiceRes, TOut>(
          string baseUrl,
          string endpoint,
          TReq request,
          Func<TServiceRes, TOut> mapOk,
          Func<int, string, TOut> errorFactory,
          string operationName)
        {
            try
            {
                var url = $"{baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";
                var json = JsonConvert.SerializeObject(request);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var response = await _client.PostAsync(url, content).ConfigureAwait(false);
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                    return errorFactory((int)response.StatusCode,
                        $"Error HTTP {(int)response.StatusCode} en {operationName}. Body: {body}");

                var serviceRes = JsonConvert.DeserializeObject<TServiceRes>(body);
                if (serviceRes is null)
                    return errorFactory(-2, $"Respuesta nula/deserialización fallida en {operationName}");

                return mapOk(serviceRes);
            }
            catch (Exception ex)
            {
                return errorFactory(-1, $"Error en {operationName}: {ex.Message}");
            }
        }

        public async Task<TOut> JsonGetApiAsync<TServiceRes, TOut>(
            string baseUrl,
            string endpoint,
            Func<TServiceRes, TOut> mapOk,
            Func<int, string, TOut> errorFactory,
            string operationName)
        {
            try
            {
                var url = $"{baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";

                using var response = await _client.GetAsync(url).ConfigureAwait(false);
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                    return errorFactory(
                        (int)response.StatusCode,
                        $"Error HTTP {(int)response.StatusCode} en {operationName}. Body: {body}");

                var serviceRes = JsonConvert.DeserializeObject<TServiceRes>(body);
                if (serviceRes is null)
                    return errorFactory(-2, $"Respuesta nula en {operationName}");

                return mapOk(serviceRes);
            }
            catch (Exception ex)
            {
                return errorFactory(-1, $"Error en {operationName}: {ex.Message}");
            }
        }


    }
}
