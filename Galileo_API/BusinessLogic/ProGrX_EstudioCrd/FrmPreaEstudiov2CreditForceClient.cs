using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Galileo_API.Models.ProGrX_EstudioCrd;

namespace Galileo_API.BusinessLogic.ProGrX_EstudioCrd
{
    public sealed class FrmPreaEstudiov2CreditForceClient
    {
        private static readonly HttpClient SharedClient = CrearCliente(false);
        private static readonly HttpClient DevelopmentClient = CrearCliente(true);
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly IConfiguration _config;
        private readonly HttpClient? _http;
        private readonly bool _esDesarrollo;

        public FrmPreaEstudiov2CreditForceClient(
            IConfiguration config, HttpClient? http = null, IHostEnvironment? environment = null)
        {
            _config = config;
            _http = http;
            _esDesarrollo = environment?.IsDevelopment() == true;
        }

        private static HttpClient CrearCliente(bool permitirNombreLocal)
        {
            var handler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                ConnectTimeout = TimeSpan.FromSeconds(10),
                AllowAutoRedirect = false
            };
            if (permitirNombreLocal)
                handler.SslOptions.RemoteCertificateValidationCallback = ValidarCertificadoDesarrollo;
            return new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan };
        }

        private static bool ValidarCertificadoDesarrollo(
            object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors errors)
        {
            return errors == SslPolicyErrors.None
                || (errors == SslPolicyErrors.RemoteCertificateNameMismatch
                    && certificate is not null
                    && string.Equals(certificate.GetCertHashString(),
                        "187ED2FCC1C73CE6F111B4D3FED9D019EFDFA6C6", StringComparison.OrdinalIgnoreCase));
        }

        private bool PermitirNombreLocal(Uri baseUri)
        {
            return _esDesarrollo
                && _config.GetValue<bool>("CreditForce:AllowCertificateNameMismatchInDevelopment")
                && string.Equals(
                    baseUri.Host,
                    _config["CreditForce:DevelopmentHost"],
                    StringComparison.OrdinalIgnoreCase)
                && baseUri.Port == 8443;
        }

        public async Task<PreaScoreCrediticioDetalleDto?> ObtenerComportamientoAsync(
            string cedula, string usuario, string ip, CancellationToken cancellationToken)
        {
            var baseUrl = _config["CreditForce:BaseUrl"];
            var entidad = _config["CreditForce:Entidad"];
            var apiKey = _config["CreditForce:ApiKey"];
            if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri)
                || baseUri.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(baseUri.UserInfo)
                || string.IsNullOrWhiteSpace(entidad) || string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "Debe configurar CreditForce:BaseUrl (HTTPS), CreditForce:Entidad y CreditForce:ApiKey en el servidor.");
            }

            var http = _http ?? (PermitirNombreLocal(baseUri) ? DevelopmentClient : SharedClient);
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(60));
            var token = timeout.Token;
            var identificacion = NormalizarIdentificacion(cedula);
            using var login = new HttpRequestMessage(HttpMethod.Post, new Uri(baseUri, "/API/Login"));
            login.Headers.Add("Entidad", entidad);
            login.Headers.Add("ApiKey", apiKey);
            login.Content = JsonContent.Create(new
            {
                usuario,
                aplicacion = _config["CreditForce:Aplicacion"] ?? "Galileo",
                ip
            });
            var autenticacion = await EnviarAsync<CreditForceRespuesta>(login, "autenticar", token, http);
            if (!EsExitosa(autenticacion) || string.IsNullOrWhiteSpace(autenticacion.Detalle)
                || autenticacion.Detalle.Any(char.IsWhiteSpace))
            {
                throw new InvalidOperationException("CreditForce no autorizó la autenticación solicitada.");
            }

            using var existeRequest = CrearSolicitud(baseUri, "ValidaClienteExiste", autenticacion.Detalle,
                new { numeroIdentificacion = identificacion });
            var existe = await EnviarAsync<CreditForceExisteResponse>(existeRequest, "validar cliente", token, http);
            ValidarRespuesta(existe.Respuesta, "validar cliente");
            var cliente = existe.ResultadoClienteExiste
                ?? throw new InvalidOperationException("CreditForce no devolvió el resultado de existencia del cliente.");
            ValidarResultado(cliente.TieneError, cliente.Error, cliente.NumeroIdentificacion, identificacion);
            if (!cliente.Existe)
                return null;

            using var scoreRequest = CrearSolicitud(baseUri, "ObtenerScoreClienteExistente", autenticacion.Detalle,
                new { numeroIdentificacion = identificacion, esClienteExistente = true });
            var score = await EnviarAsync<CreditForceScoreResponse>(scoreRequest, "obtener score", token, http);
            ValidarRespuesta(score.Respuesta, "obtener score");
            var resultado = score.ResultadoScoreClienteExiste
                ?? throw new InvalidOperationException("CreditForce no devolvió el resultado del score.");
            ValidarResultado(resultado.TieneError, resultado.Error, resultado.NumeroIdentificacion, identificacion);
            if (!resultado.ClienteExisteEnComportamiento)
                return null;
            if (!string.Equals(resultado.TipoClienteEvaluado, "existente", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("CreditForce devolvió un tipo de cliente diferente del solicitado.");

            var comportamiento = resultado.Calificaciones?.Comportamiento
                ?? throw new InvalidOperationException("CreditForce no devolvió la calificación de comportamiento.");
            return MapearComportamiento(comportamiento);
        }

        private static async Task<T> EnviarAsync<T>(
            HttpRequestMessage request, string operacion, CancellationToken token, HttpClient http)
        {
            using var response = await http.SendAsync(request, token);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"CreditForce: error HTTP {(int)response.StatusCode} al {operacion}.");
            return await response.Content.ReadFromJsonAsync<T>(JsonOptions, token)
                ?? throw new InvalidOperationException($"CreditForce devolvió una respuesta vacía al {operacion}.");
        }

        private static HttpRequestMessage CrearSolicitud<T>(Uri baseUri, string endpoint, string token, T datos)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(baseUri, "/API/CreditForce/" + endpoint));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = JsonContent.Create(datos);
            return request;
        }

        private static bool EsExitosa(CreditForceRespuesta? respuesta)
            => respuesta is not null && respuesta.Tipo == 0
                && string.Equals(respuesta.TipoRespuesta, "Y", StringComparison.OrdinalIgnoreCase);

        private static void ValidarRespuesta(CreditForceRespuesta? respuesta, string operacion)
        {
            if (!EsExitosa(respuesta))
                throw new InvalidOperationException($"CreditForce no pudo {operacion}: {respuesta?.Detalle}");
        }

        private static void ValidarResultado(bool tieneError, JsonElement? error, string recibida, string solicitada)
        {
            if (tieneError)
                throw new InvalidOperationException($"CreditForce devolvió un error: {error}");
            if (!string.Equals(NormalizarIdentificacion(recibida), solicitada, StringComparison.Ordinal))
                throw new InvalidOperationException("La identificación devuelta por CreditForce no coincide con la solicitada.");
        }

        private static string NormalizarIdentificacion(string? identificacion)
            => (identificacion ?? string.Empty).Trim().Replace("-", string.Empty).Replace(" ", string.Empty);

        private static PreaScoreCrediticioDetalleDto MapearComportamiento(CreditForceComportamiento score)
        {
            if (!string.Equals(score.Modelo, "comportamiento", StringComparison.OrdinalIgnoreCase)
                || score.Modelo.Length > 25 || score.Objetivo?.Length > 25
                || score.ProbabilidadDefecto < 0 || score.ProbabilidadDefecto > 1
                || score.UmbralAplicado < 0 || score.UmbralAplicado > 1
                || score.DecisionBinaria is not (0 or 1)
                || string.IsNullOrWhiteSpace(score.Veredicto) || score.Veredicto.Length > 100
                || string.IsNullOrWhiteSpace(score.CategoriaRiesgo) || score.CategoriaRiesgo.Length > 30)
            {
                throw new InvalidOperationException("La calificación de CreditForce contiene datos inválidos para registrar el score.");
            }
            return new PreaScoreCrediticioDetalleDto
            {
                modelo = score.Modelo,
                objetivo = score.Objetivo,
                probabilidad_defecto = score.ProbabilidadDefecto,
                decision_binaria = score.DecisionBinaria == 1,
                veredicto = score.Veredicto,
                categoria_riesgo = score.CategoriaRiesgo,
                umbral_aplicado = score.UmbralAplicado
            };
        }
    }
}
