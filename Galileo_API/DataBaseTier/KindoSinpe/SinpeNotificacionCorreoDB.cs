using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;

namespace Galileo_API.DataBaseTier
{
    /// <summary>
    /// Arma la notificación SINPE por correo a partir de PSL_PLANTILLAS_NOTIFICACIONES
    /// y la guarda en SIF_EMAIL. El envío lo realiza otro proceso.
    /// </summary>
    public class SinpeNotificacionCorreoDB
    {
        private const int TipoBeneficiarioPersona = 1;
        private const int TipoBeneficiarioProveedor = 3;
        private const int TipoBeneficiarioDesembolso = 8;
        private const string GrupoBancoInterno = "INT";

        private readonly PortalDB _portalDb;

        public SinpeNotificacionCorreoDB(IConfiguration config)
        {
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
        /// Determina la plantilla según TIPO_BENEFICIARIO de la solicitud:
        /// 3 proveedor; 8 desembolso (asociado o no); 1 persona (interna o externa).
        /// </summary>
        public ErrorDto<TipoPlantillaSinpe?> ResolverTipoPlantilla(int codEmpresa, TesTransaccion solicitud)
        {
            var tipo = (int?)solicitud.TIPO_BENEFICIARIO;

            if (tipo == TipoBeneficiarioProveedor)
                return Resultado(TipoPlantillaSinpe.TranPr);

            if (tipo == TipoBeneficiarioDesembolso)
            {
                var asociado = EsAsociado(codEmpresa, solicitud.Codigo);
                if (asociado.Code != 0)
                    return Error(asociado.Description);

                return Resultado(asociado.Result ? TipoPlantillaSinpe.TranAs : TipoPlantillaSinpe.TranBe);
            }

            if (tipo == TipoBeneficiarioPersona)
            {
                var interna = EsCuentaInterna(codEmpresa, solicitud.ID_BANCO_DESTINO);
                if (interna.Code != 0)
                    return Error(interna.Description);

                return Resultado(interna.Result ? TipoPlantillaSinpe.TranI : TipoPlantillaSinpe.TranS);
            }

            return Error($"Tipo de beneficiario {tipo?.ToString() ?? "(vacío)"} no soportado para la notificación SINPE.");
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

        private static ErrorDto<TipoPlantillaSinpe?> Resultado(TipoPlantillaSinpe tipo) =>
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
INSERT INTO dbo.SIF_EMAIL (PARA, CUERPO, ASUNTO, PRIORIDAD, ESTADO, FECHA)
VALUES (@Para, @Cuerpo, @Asunto, @Prioridad, 'P', GETDATE());";

            return DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    Para = para,
                    Cuerpo = cuerpo,
                    Asunto = plantilla.Asunto,
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
