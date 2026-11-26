using System.Net;
using Dapper;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    /// <summary>Códigos de SYS_NOTIFICACIONES_PLANTILLAS usados por Beneficios Solidarios.</summary>
    public static class BeneficiosPlantillas
    {
        public const string Solicitud = "BEN_SOL01";
        public const string SolicitudPsicologia = "BEN_SOL02";
        public const string Resolucion = "BEN_RES01";
        public const string CobroMora = "BEN_MORA01";
        public const string SolicitudBloqueo = "BEN_BLQ01";
        public const string RecargaTarjetas = "BEN_TAR01";
        public const string Deposito = "BEN_DEP01";
    }

    /// <summary>Archivo adjunto de un correo de Beneficios.</summary>
    public sealed record AdjuntoBeneficio(string Nombre, string ContenidoBase64);

    /// <summary>Datos de un correo de Beneficios que se va a encolar.</summary>
    public sealed record CorreoBeneficio
    {
        public required int CodEmpresa { get; init; }
        public required string CodPlantilla { get; init; }
        public required string CodSmtp { get; init; }
        public required string Para { get; init; }
        public string Usuario { get; init; } = "Sistema";
        public string Cc { get; init; } = string.Empty;
        public IReadOnlyDictionary<string, string> Datos { get; init; } = new Dictionary<string, string>();
        public IReadOnlyList<AdjuntoBeneficio> Adjuntos { get; init; } = [];
    }

    /// <summary>
    /// Registra los correos de Beneficios en la cola de envío (SYS_MAIL_SERVICE); los dispara un servicio aparte.
    /// Sin adjuntos usa spCpr_EncolarCorreos, igual que Compras. Con adjuntos usa el formato de MailService 2.0
    /// (SYS_MAIL_SERVICE.I_ADJUNTOS = 1 y un registro por archivo en SYS_MAIL_SERVICE_ADJUNTOSV2),
    /// para que todos los archivos lleguen en un mismo correo.
    /// </summary>
    public class BeneficiosCorreoDB
    {
        private const int LargoUsuario = 30;
        private const int LargoAsunto = 150;
        private const int LargoNombreArchivo = 100;
        private const int LargoCodNotificacion = 10;

        private const string SqlCorreo = @"
            INSERT INTO dbo.SYS_MAIL_SERVICE
                (COD_SMTP, PARA, CC, CUERPO, ASUNTO, PRIORIDAD, ESTADO, FECHA, COD_EMPRESA, USUARIO, COD_NOTIFICACION, I_ADJUNTOS)
            VALUES
                (@codSmtp, @para, @cc, @cuerpo, @asunto, 0, 'P', GETDATE(), @codEmpresa, @usuario, @codNotificacion, 1);
            SELECT CAST(SCOPE_IDENTITY() AS bigint);";

        private const string SqlAdjunto = @"
            INSERT INTO dbo.SYS_MAIL_SERVICE_ADJUNTOSV2 (ID_EMAIL, CUERPO, GA_NOMBRE_ARCHIVO, GA_INDICA)
            VALUES (@idEmail, @contenido, @nombre, 0);";

        private readonly IConfiguration _config;
        private readonly EnvioCorreoDB _envioCorreoDB;

        public BeneficiosCorreoDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _envioCorreoDB = new EnvioCorreoDB(_config);
        }

        /// <summary>Cuenta SMTP configurada en la categoría del beneficio.</summary>
        public string CodSmtpPorBeneficio(int codEmpresa, string codBeneficio)
        {
            const string sql = @"
                SELECT C.COD_SMTP FROM AFI_BENE_CATEGORIAS C
                INNER JOIN AFI_BENEFICIOS B ON B.COD_CATEGORIA = C.COD_CATEGORIA
                WHERE B.COD_BENEFICIO = @codBeneficio";

            return ObtenerCodSmtp(codEmpresa, sql, new { codBeneficio });
        }

        /// <summary>Cuenta SMTP configurada en la categoría indicada.</summary>
        public string CodSmtpPorCategoria(int codEmpresa, string codCategoria)
        {
            const string sql = "SELECT COD_SMTP FROM AFI_BENE_CATEGORIAS WHERE COD_CATEGORIA = @codCategoria";

            return ObtenerCodSmtp(codEmpresa, sql, new { codCategoria });
        }

        private string ObtenerCodSmtp(int codEmpresa, string sql, object parametros)
        {
            var result = DbHelper.ExecuteSingleQuery<string>(new PortalDB(_config), codEmpresa, sql, null, parametros);
            return result.Result?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Encola el correo en estado P con el destinatario real. No depende de AppSettings:EnviaEmail
        /// ni redirige a TestEmail: el envío lo controla el servicio de correo.
        /// </summary>
        public async Task<ErrorDto> EncolarAsync(CorreoBeneficio correo)
        {
            if (string.IsNullOrWhiteSpace(correo.CodSmtp))
            {
                return Error("La categoría del beneficio no tiene una cuenta de correo configurada.");
            }

            if (string.IsNullOrWhiteSpace(correo.Para))
            {
                return Error("No hay un correo electrónico de destino.");
            }

            return correo.Adjuntos.Count == 0
                ? await _envioCorreoDB.QueuePlantillaDestinoRealAsync(
                    correo.CodEmpresa, correo.CodSmtp, correo.CodPlantilla, correo.Para.Trim(), correo.Datos, correo.Usuario)
                : await EncolarConAdjuntosAsync(correo);
        }

        private async Task<ErrorDto> EncolarConAdjuntosAsync(CorreoBeneficio correo)
        {
            // El servicio de correo solo genera archivos PDF; cualquier otro tipo llegaría dañado.
            var noPdf = correo.Adjuntos.FirstOrDefault(a => !EsPdf(a.ContenidoBase64));
            if (noPdf != null)
            {
                return Error($"Solo se pueden enviar archivos PDF. Revise el archivo: {noPdf.Nombre}");
            }

            var plantilla = await _envioCorreoDB.ObtenerPlantillaAsync(correo.CodEmpresa, correo.CodPlantilla);
            if (plantilla == null)
            {
                return Error($"No existe la plantilla activa {correo.CodPlantilla} en SYS_NOTIFICACIONES_PLANTILLAS.");
            }

            var baseConn = _config.GetConnectionString("BaseConnString");
            if (string.IsNullOrWhiteSpace(baseConn))
            {
                return Error("BaseConnString no está configurada para encolar el correo.");
            }

            try
            {
                await using var connection = new SqlConnection(baseConn);
                await connection.OpenAsync();
                await using var transaction = await connection.BeginTransactionAsync();

                var idEmail = await connection.QuerySingleAsync<long>(SqlCorreo, new
                {
                    codSmtp = correo.CodSmtp,
                    para = correo.Para.Trim(),
                    cc = correo.Cc,
                    cuerpo = Reemplazar(plantilla.Value.Cuerpo, correo.Datos, WebUtility.HtmlEncode),
                    asunto = Recortar(Reemplazar(plantilla.Value.Asunto, correo.Datos, valor => valor), LargoAsunto),
                    codEmpresa = correo.CodEmpresa,
                    usuario = Recortar(correo.Usuario, LargoUsuario),
                    codNotificacion = Recortar(correo.CodPlantilla, LargoCodNotificacion)
                }, transaction);

                foreach (var adjunto in correo.Adjuntos)
                {
                    await connection.ExecuteAsync(SqlAdjunto, new
                    {
                        idEmail,
                        contenido = adjunto.ContenidoBase64,
                        nombre = RecortarNombre(adjunto.Nombre)
                    }, transaction);
                }

                await transaction.CommitAsync();
                return new ErrorDto { Code = 0, Description = "Correo agregado a la cola de envío." };
            }
            catch (Exception ex)
            {
                return Error("No fue posible encolar el correo: " + ex.Message);
            }
        }

        private static ErrorDto Error(string mensaje) => new() { Code = -1, Description = mensaje };

        // "JVBER" es el inicio en base64 de la firma "%PDF".
        private static bool EsPdf(string contenidoBase64) =>
            contenidoBase64.StartsWith("JVBER", StringComparison.Ordinal);

        private static string Reemplazar(string texto, IReadOnlyDictionary<string, string> datos, Func<string, string> codificar) =>
            datos.Aggregate(texto, (actual, dato) => actual.Replace($"[[{dato.Key}]]", codificar(dato.Value ?? string.Empty)));

        private static string Recortar(string? valor, int largo)
        {
            var texto = valor ?? string.Empty;
            return texto.Length <= largo ? texto : texto[..largo];
        }

        private static string RecortarNombre(string nombre)
        {
            var extension = Path.GetExtension(nombre);
            return Recortar(Path.GetFileNameWithoutExtension(nombre), LargoNombreArchivo - extension.Length) + extension;
        }
    }
}
