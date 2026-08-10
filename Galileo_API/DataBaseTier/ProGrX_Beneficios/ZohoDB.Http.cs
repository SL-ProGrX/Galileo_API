using Galileo.Models.AF;
using Galileo_Externo.Models.NewFolder;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class ZohoDB
    {
        private const string ZohoOrgIdHeader = "orgId";
        private const string ZohoOrgId = "691715214";
        private const string ZohoOAuthUrlConfigKey = "Zoho:oauth_token_url";
        private const string ZohoDeskApiBaseUrlConfigKey = "Zoho:desk_api_base_url";
        private const string CampoCedulaZoho = "cf_numero_de_cedula";
        private const string MensajeCedulaRequerida = "Cédula no puede ser nula...";

        /// <summary>
        /// Obtiene un ticket puntual de Zoho Desk por su identificador numérico.
        /// </summary>
        private Ticket? ObtenerTicketPorId(HttpClient httpClient, string token, string ticketId)
        {
            try
            {
                var ticketIdNormalizado = NormalizarTicketId(ticketId);
                var deskApiBaseUrl = ObtenerEndpointZoho(ZohoDeskApiBaseUrlConfigKey);
                if (ticketIdNormalizado is null || deskApiBaseUrl is null)
                {
                    return null;
                }

                ConfigurarHeadersZoho(httpClient, token);
                var requestUri = new Uri(deskApiBaseUrl, $"tickets/{ticketIdNormalizado}");
                var response = httpClient.GetAsync(requestUri).Result;

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var body = response.Content.ReadAsStringAsync().Result;
                return JsonSerializer.Deserialize<Ticket>(body, _jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Normaliza el identificador de ticket a un segmento formado únicamente por dígitos.
        /// </summary>
        private static string? NormalizarTicketId(string ticketId)
        {
            return ulong.TryParse(
                ticketId.Trim(),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var ticketIdNumerico)
                ? ticketIdNumerico.ToString(CultureInfo.InvariantCulture)
                : null;
        }

        /// <summary>
        /// Obtiene un token de acceso a la API de Zoho Desk usando credenciales externas a appsettings.
        /// </summary>
        private string? ObtenerTokenZoho(HttpClient httpClient, out string? errorDetail)
        {
            errorDetail = null;
            try
            {
                var refreshToken = _config["Zoho:refresh_token"] ?? string.Empty;
                var clientId = _config["Zoho:client_id"] ?? string.Empty;
                var clientSecret = _config["Zoho:client_secret"] ?? string.Empty;
                var grantType = _config["Zoho:grant_type"] ?? "refresh_token";
                var oauthUrl = ObtenerEndpointZoho(ZohoOAuthUrlConfigKey);

                if (string.IsNullOrEmpty(refreshToken) ||
                    string.IsNullOrEmpty(clientId) ||
                    string.IsNullOrEmpty(clientSecret) ||
                    oauthUrl is null)
                {
                    errorDetail = "Falta configurar las credenciales o el endpoint OAuth de Zoho";
                    return null;
                }

                var formData = new Dictionary<string, string>
                {
                    { "refresh_token", refreshToken },
                    { "client_id", clientId },
                    { "client_secret", clientSecret },
                    { "grant_type", grantType }
                };

                using var content = new FormUrlEncodedContent(formData);
                var response = httpClient.PostAsync(oauthUrl, content).Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
                var zohoAuth = JsonSerializer.Deserialize<ZohoModel>(responseString);

                if (!string.IsNullOrEmpty(zohoAuth?.error))
                {
                    errorDetail = $"Zoho devolvió error '{zohoAuth.error}' (HTTP {(int)response.StatusCode})";
                    return null;
                }

                if (string.IsNullOrEmpty(zohoAuth?.access_token))
                {
                    errorDetail = $"Zoho respondió sin access_token (HTTP {(int)response.StatusCode}): {responseString}";
                    return null;
                }

                return zohoAuth.access_token;
            }
            catch (Exception ex)
            {
                errorDetail = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Obtiene tickets de Zoho Desk por departamento y rango de fechas.
        /// </summary>
        private List<AfiBeneTicketsDatos> ObtenerTicketsZoho(
            HttpClient httpClient,
            ZohoTicketsConsultaRequest request)
        {
            var tickets = new List<AfiBeneTicketsDatos>();
            var deskApiBaseUrl = ObtenerEndpointZoho(ZohoDeskApiBaseUrlConfigKey);
            if (deskApiBaseUrl is null)
            {
                return tickets;
            }

            const int pageSize = 10;
            ConfigurarHeadersZoho(httpClient, request.Token);

            var fechaInicioIso = request.FechaInicio.ToUniversalTime()
                .ToString("yyyy-MM-ddT00:00:00.000Z", CultureInfo.InvariantCulture);
            var fechaCorteIso = request.FechaCorte.ToUniversalTime()
                .ToString("yyyy-MM-ddT11:59:59.000Z", CultureInfo.InvariantCulture);
            var searchUrl = new Uri(deskApiBaseUrl, "tickets/search");
            var baseQuery = $"departmentId={Uri.EscapeDataString(request.DepartamentoId)}" +
                $"&createdTimeRange={fechaInicioIso},{fechaCorteIso}" +
                "&customField1=cf_productos_servicio_al_asociado:Beneficios solidarios";

            var initialResponse = httpClient.GetAsync($"{searchUrl}?{baseQuery}").Result;
            if (!initialResponse.IsSuccessStatusCode)
            {
                return tickets;
            }

            var initialBody = initialResponse.Content.ReadAsStringAsync().Result;
            var initialData = JsonSerializer.Deserialize<DataModel>(initialBody, _jsonOptions);
            var totalPages = initialData is null
                ? 0
                : initialData.count == pageSize
                    ? 1
                    : (int)Math.Ceiling((double)initialData.count / pageSize);

            for (var page = 1; page <= totalPages; page++)
            {
                var paginaActual = totalPages > 1 ? $"&from={page}" : string.Empty;
                var response = httpClient.GetAsync($"{searchUrl}?{baseQuery}{paginaActual}").Result;
                if (!response.IsSuccessStatusCode)
                {
                    continue;
                }

                var body = response.Content.ReadAsStringAsync().Result;
                var dataModel = JsonSerializer.Deserialize<DataModel>(body, _jsonOptions);
                ProcesarTicketsZoho(dataModel, tickets);
            }

            return tickets;
        }

        /// <summary>
        /// Obtiene y valida un endpoint HTTPS configurado para Zoho.
        /// </summary>
        private Uri? ObtenerEndpointZoho(string configKey)
        {
            var configuredUrl = _config[configKey];
            return Uri.TryCreate(configuredUrl, UriKind.Absolute, out var endpoint) &&
                   endpoint.Scheme == Uri.UriSchemeHttps
                ? endpoint
                : null;
        }

        /// <summary>
        /// Configura los encabezados comunes de Zoho Desk.
        /// </summary>
        private static void ConfigurarHeadersZoho(HttpClient httpClient, string token)
        {
            httpClient.DefaultRequestHeaders.Remove(ZohoOrgIdHeader);
            httpClient.DefaultRequestHeaders.Add(ZohoOrgIdHeader, ZohoOrgId);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Zoho-oauthtoken", token);
        }

        private sealed class ZohoTicketsConsultaRequest
        {
            public string Token { get; init; } = string.Empty;
            public string DepartamentoId { get; init; } = string.Empty;
            public DateTime FechaInicio { get; init; }
            public DateTime FechaCorte { get; init; }
        }
    }
}
