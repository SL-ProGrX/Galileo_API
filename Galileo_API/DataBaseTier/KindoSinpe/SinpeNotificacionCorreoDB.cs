using System.Globalization;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;

namespace Galileo_API.DataBaseTier
{
    /// <summary>
    /// Arma la notificación SINPE por correo a partir de PSL_PLANTILLAS_NOTIFICACIONES
    /// y la guarda en PSL_EMAIL. El envío lo realiza otro proceso.
    /// </summary>
    public class SinpeNotificacionCorreoDB
    {
        private const int TipoBeneficiarioPersona = 1;
        private const int TipoBeneficiarioProveedor = 3;
        private const int TipoBeneficiarioDesembolso = 8;
        private const string GrupoBancoInterno = "INT";
        public const int CodigoSinCorreo = 2;
        public const string MensajeSinCorreo = "Notificación no enviada: la solicitud no tiene correo electrónico.";
        public const string SinPlantillaAplicable = "Sin plantilla de notificación aplicable a esta transferencia.";
        private static readonly CultureInfo CulturaCr = new("es-CR");

        private readonly IConfiguration _config;
        private readonly PortalDB _portalDb;

        public SinpeNotificacionCorreoDB(IConfiguration config)
        {
            _config = config;
            _portalDb = new PortalDB(config);
        }

        public ErrorDto GuardarNotificacionSinpe(
            int codEmpresa,
            TipoPlantillaSinpe tipo,
            string? para,
            SinpeNotificacionDatos datos)
        {
            if (string.IsNullOrWhiteSpace(para))
                return new ErrorDto { Code = -1, Description = "La solicitud no tiene correo para la notificación." };

            var plantillaResult = ObtenerPlantilla(codEmpresa, tipo);
            var plantilla = plantillaResult.Result;
            if (plantillaResult.Code != 0 || plantilla is null || string.IsNullOrWhiteSpace(plantilla.Plantilla))
                return new ErrorDto
                {
                    Code = -1,
                    Description = plantillaResult.Description ?? $"No existe la plantilla {CodigoPlantilla(tipo)}."
                };

            var cuerpo = ReemplazarTokens(plantilla.Plantilla ?? string.Empty, datos);
            return InsertarSifEmail(codEmpresa, para, plantilla, cuerpo);
        }

        /// <summary>
        /// Punto único de notificación de una transferencia SINPE. Si falló usa TRAN_RE;
        /// si no, resuelve la plantilla según la solicitud. El destinatario es el correo de la
        /// solicitud cuando AppSettings:EnviaEmail es "Y"; en cualquier otro caso, AppSettings:TestEmail.
        /// </summary>
        public ErrorDto NotificarTransferencia(
            int codEmpresa,
            TesTransaccion solicitud,
            bool fallo,
            string? referencia,
            string? motivo)
        {
            return Ejecutar(() =>
            {
                var tipo = fallo
                    ? Resultado(TipoPlantillaSinpe.TranRe)
                    : ResolverTipoPlantilla(solicitud);

                return Notificar(codEmpresa, solicitud, tipo, fallo, referencia, motivo);
            });
        }

        /// <summary>
        /// Notificación de una transferencia TE (banco, sin respuesta SINPE). Solo aplica cuando el
        /// destino es proveedor, desembolso a asociado o cuenta interna del asociado; en los demás
        /// casos no hay correo y devuelve Code 0 sin mensaje. Un fallo usa TRAN_RE solo si aplica.
        /// </summary>
        public ErrorDto NotificarTransferenciaTE(
            int codEmpresa,
            TesTransaccion solicitud,
            bool fallo,
            string? referencia,
            string? motivo)
        {
            return Ejecutar(() =>
            {
                var aplica = ResolverTipoPlantillaTE(codEmpresa, solicitud);
                if (aplica.Code != 0)
                    return new ErrorDto { Code = -1, Description = aplica.Description ?? string.Empty };

                if (aplica.Result is null)
                    return new ErrorDto { Code = 0, Description = SinPlantillaAplicable };

                var tipo = fallo ? Resultado(TipoPlantillaSinpe.TranRe) : aplica;
                return Notificar(codEmpresa, solicitud, tipo, fallo, referencia, motivo);
            });
        }

        /// <summary>
        /// Convierte el resultado de una notificación en el mensaje para la bitácora. Null cuando no hay nada que registrar.
        /// </summary>
        public static string? MensajeBitacora(ErrorDto resultado) => resultado.Code switch
        {
            0 => null,
            CodigoSinCorreo => MensajeSinCorreo,
            _ => $"Notificación por correo no guardada: {resultado.Description}"
        };

        private ErrorDto Notificar(
            int codEmpresa,
            TesTransaccion solicitud,
            ErrorDto<TipoPlantillaSinpe?> tipo,
            bool fallo,
            string? referencia,
            string? motivo)
        {
            if (tipo.Code != 0 || tipo.Result is null)
                return new ErrorDto { Code = -1, Description = tipo.Description ?? string.Empty };

            var destino = DestinoNotificacion(codEmpresa, solicitud);
            if (destino.Code != 0)
                return new ErrorDto { Code = destino.Code, Description = destino.Description ?? string.Empty };

            return GuardarNotificacionSinpe(
                codEmpresa,
                tipo.Result.Value,
                destino.Result,
                ArmarDatos(solicitud, tipo.Result.Value, fallo, referencia, motivo));
        }

        private static ErrorDto Ejecutar(Func<ErrorDto> notificacion)
        {
            try
            {
                return notificacion();
            }
            catch (Exception ex)
            {
                return new ErrorDto { Code = -1, Description = ex.Message };
            }
        }

        /// <summary>
        /// Usa el correo de la solicitud y, si no tiene, lo busca en el maestro (SOCIOS o cxp_proveedores).
        /// Sin correo, devuelve CodigoSinCorreo. AppSettings:EnviaEmail no aplica: la fila se guarda en
        /// PSL_EMAIL y otro proceso la envía.
        /// </summary>
        private ErrorDto<string?> DestinoNotificacion(int codEmpresa, TesTransaccion solicitud)
        {
            var correo = solicitud.CorreoNotifica?.Trim();
            if (string.IsNullOrWhiteSpace(correo))
                correo = ObtenerCorreoMaestro(codEmpresa, solicitud);

            if (string.IsNullOrWhiteSpace(correo))
                return new ErrorDto<string?> { Code = CodigoSinCorreo, Description = MensajeSinCorreo, Result = null };

            return new ErrorDto<string?> { Code = 0, Description = "Ok", Result = correo };
        }

        private string? ObtenerCorreoMaestro(int codEmpresa, TesTransaccion solicitud)
        {
            var codigo = solicitud.Codigo?.Trim() ?? string.Empty;
            var tipo = (int?)solicitud.TIPO_BENEFICIARIO;

            var sql = tipo switch
            {
                TipoBeneficiarioProveedor => "SELECT TOP 1 RTRIM(ISNULL(email, '')) FROM cxp_proveedores WHERE CEDJUR = @Codigo;",
                TipoBeneficiarioPersona or TipoBeneficiarioDesembolso => "SELECT TOP 1 RTRIM(ISNULL(af_email, '')) FROM socios WHERE cedula = @Codigo;",
                _ => null
            };
            if (sql is null || string.IsNullOrWhiteSpace(codigo))
                return null;

            var correo = DbHelper.ExecuteSingleQuery<string>(
                _portalDb,
                codEmpresa,
                sql,
                defaultValue: string.Empty,
                parameters: new { Codigo = codigo }
            ).Result;

            return string.IsNullOrWhiteSpace(correo) ? null : correo.Trim();
        }

        private static SinpeNotificacionDatos ArmarDatos(
            TesTransaccion solicitud,
            TipoPlantillaSinpe tipo,
            bool fallo,
            string? referencia,
            string? motivo) => new()
        {
            NombreLogin = tipo == TipoPlantillaSinpe.TranAs ? solicitud.Beneficiario : solicitud.NombreOrigen,
            NSolicitud = solicitud.NumeroSolicitud.ToString(),
            FechaHora = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss", CulturaCr),
            PersonaDestino = solicitud.Beneficiario,
            CtaIBAN = solicitud.Cuenta,
            RefTransaccion = referencia,
            MontoTransaccion = solicitud.Monto.ToString("N2", CulturaCr),
            Moneda = solicitud.Divisa is "USD" or "DOL" ? "Dólares" : "Colones",
            Detalle = string.Concat(solicitud.Detalle1, solicitud.Detalle2, solicitud.Detalle3, solicitud.Detalle4, solicitud.Detalle5),
            CedulaOrigen = solicitud.CedulaOrigen,
            CedulaDestino = solicitud.Codigo,
            IBAN = solicitud.Cuenta,
            NombreOrigenTransaccion = solicitud.NombreOrigen,
            NombreDestinoTransaccion = solicitud.Beneficiario,
            EstadoTransaccion = fallo ? motivo : "Exitosa",
            CedulaOrigenTransaccion = solicitud.CedulaOrigen
        };

        /// <summary>
        /// Determina la plantilla TS según TIPO_BENEFICIARIO de la solicitud:
        /// 3 proveedor (TRAN-PR); 8 desembolso (TRAN-BE); 1 persona (TRAN_S).
        /// </summary>
        public ErrorDto<TipoPlantillaSinpe?> ResolverTipoPlantilla(TesTransaccion solicitud)
        {
            var tipo = (int?)solicitud.TIPO_BENEFICIARIO;

            if (tipo == TipoBeneficiarioProveedor)
                return Resultado(TipoPlantillaSinpe.TranPr);

            if (tipo == TipoBeneficiarioDesembolso)
                return Resultado(TipoPlantillaSinpe.TranBe);

            if (tipo == TipoBeneficiarioPersona)
                return Resultado(TipoPlantillaSinpe.TranS);

            return Error($"Tipo de beneficiario {tipo?.ToString() ?? "(vacío)"} no soportado para la notificación SINPE.");
        }

        /// <summary>
        /// Determina la plantilla TE: 3 proveedor (TRAN-PR); 8 desembolso a asociado (TRAN-AS);
        /// 1 persona a cuenta interna (TRAN_I). Cualquier otro caso devuelve Result null (no aplica).
        /// </summary>
        public ErrorDto<TipoPlantillaSinpe?> ResolverTipoPlantillaTE(int codEmpresa, TesTransaccion solicitud)
        {
            var tipo = (int?)solicitud.TIPO_BENEFICIARIO;

            if (tipo == TipoBeneficiarioProveedor)
                return Resultado(TipoPlantillaSinpe.TranPr);

            if (tipo == TipoBeneficiarioDesembolso)
            {
                var asociado = EsAsociado(codEmpresa, solicitud.Codigo);
                if (asociado.Code != 0)
                    return Error(asociado.Description);

                return Resultado(asociado.Result ? TipoPlantillaSinpe.TranAs : null);
            }

            if (tipo == TipoBeneficiarioPersona)
            {
                var interna = EsCuentaInterna(codEmpresa, solicitud.ID_BANCO_DESTINO);
                if (interna.Code != 0)
                    return Error(interna.Description);

                return Resultado(interna.Result ? TipoPlantillaSinpe.TranI : null);
            }

            return Resultado(null);
        }

        private ErrorDto<bool> EsAsociado(int codEmpresa, string? codigo)
        {
            const string sql = "SELECT COUNT(1) FROM SOCIOS WHERE CEDULA = @Cedula;";

            var conteo = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sql,
                defaultValue: 0,
                parameters: new { Cedula = codigo?.Trim() ?? string.Empty }
            );

            return new ErrorDto<bool>
            {
                Code = conteo.Code,
                Description = conteo.Description,
                Result = conteo.Result > 0
            };
        }

        private ErrorDto<bool> EsCuentaInterna(int codEmpresa, string? idBancoDestino)
        {
            if (string.IsNullOrWhiteSpace(idBancoDestino))
                return new ErrorDto<bool> { Code = 0, Description = "Ok", Result = false };

            const string sql = @"
SELECT COUNT(1)
FROM dbo.TES_BANCOS B
INNER JOIN dbo.TES_BANCOS_GRUPOS G
    ON B.cod_Grupo = G.cod_grupo
WHERE B.id_banco = @IdBanco
  AND G.cod_grupo = @GrupoInterno;";

            var conteo = DbHelper.ExecuteSingleQuery<int>(
                _portalDb,
                codEmpresa,
                sql,
                defaultValue: 0,
                parameters: new { IdBanco = idBancoDestino.Trim(), GrupoInterno = GrupoBancoInterno }
            );

            return new ErrorDto<bool>
            {
                Code = conteo.Code,
                Description = conteo.Description,
                Result = conteo.Result > 0
            };
        }

        private static ErrorDto<TipoPlantillaSinpe?> Resultado(TipoPlantillaSinpe? tipo) =>
            new() { Code = 0, Description = "Ok", Result = tipo };

        private static ErrorDto<TipoPlantillaSinpe?> Error(string? descripcion) =>
            new() { Code = -1, Description = descripcion ?? "No fue posible determinar la plantilla SINPE.", Result = null };

        private ErrorDto<PlantillaNotificacionSinpe?> ObtenerPlantilla(int codEmpresa, TipoPlantillaSinpe tipo)
        {
            const string sql = @"
SELECT
    TIPO_PLANTILLA AS TipoPlantilla,
    ASUNTO         AS Asunto,
    PLANTILLA      AS Plantilla,
    CANAL          AS Canal,
    PRIORIDAD      AS Prioridad
FROM dbo.PSL_PLANTILLAS_NOTIFICACIONES
WHERE TIPO_PLANTILLA = @TipoPlantilla;";

            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                sql,
                defaultValue: new PlantillaNotificacionSinpe(),
                parameters: new { TipoPlantilla = CodigoPlantilla(tipo) }
            );
        }

        private ErrorDto InsertarSifEmail(int codEmpresa, string para, PlantillaNotificacionSinpe plantilla, string cuerpo)
        {
            const string sql = @"
INSERT INTO dbo.PSL_EMAIL (PARA, CUERPO, ASUNTO, PRIORIDAD, ESTADO, FECHA)
VALUES (@Para, @Cuerpo, @Asunto, @Prioridad, 'P', GETDATE());";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    Para = para,
                    Cuerpo = cuerpo,
                    Asunto = plantilla.Asunto is { Length: > 100 } asunto ? asunto[..100] : plantilla.Asunto,
                    Prioridad = plantilla.Prioridad
                }
            );
        }

        private static string ReemplazarTokens(string plantilla, SinpeNotificacionDatos datos)
        {
            var cuerpo = plantilla;
            foreach (var token in datos.ToTokens())
                cuerpo = cuerpo.Replace($"@@{token.Key}@@", token.Value, StringComparison.Ordinal);

            return cuerpo;
        }

        private static string CodigoPlantilla(TipoPlantillaSinpe tipo) => tipo switch
        {
            TipoPlantillaSinpe.TranAs => "TRAN-AS",
            TipoPlantillaSinpe.TranBe => "TRAN-BE",
            TipoPlantillaSinpe.TranPr => "TRAN-PR",
            TipoPlantillaSinpe.TranI => "TRAN_I",
            TipoPlantillaSinpe.TranS => "TRAN_S",
            TipoPlantillaSinpe.TranSs => "TRAN_SS",
            TipoPlantillaSinpe.TranRe => "TRAN_RE",
            _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Tipo de plantilla SINPE no soportado.")
        };
    }
}
