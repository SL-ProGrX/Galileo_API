using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosIntegralGenDB
    {
        /// <summary>
        /// Envía por correo la notificación de resolución al asociado, adjuntando los archivos recibidos.
        /// Los documentos llegan desde Angular en base64 (no se generan en el API).
        /// </summary>
        public async Task<ErrorDto> BeneficioNotificaResolucion_Enviar(List<DocArchivoBeneIntegralDto> parametros)
        {
            var info = new ErrorDto { Code = 0 };

            if (parametros == null || parametros.Count == 0)
            {
                return new ErrorDto { Code = -1, Description = "No se recibieron parámetros para el envío de correo" };
            }

            var primero = parametros[0];
            var codCliente = Convert.ToInt32(primero.codCliente);

            try
            {
                var codSmtp = ObtenerCodSmtpCategoria(codCliente, primero.id_beneficio)?.Trim() ?? string.Empty;

                var correoResult = _envioCorreoDB.BuscoDatosSocioBeneficio(
                    codCliente, primero.cedula ?? string.Empty, primero.cod_beneficio ?? string.Empty);
                var correo = correoResult?.Result;

                if (correo == null || string.IsNullOrEmpty(correo.email))
                {
                    return new ErrorDto { Code = -1, Description = "El asociado no tiene un correo electrónico registrado en Datos Persona" };
                }

                if (string.IsNullOrWhiteSpace(primero.body))
                {
                    primero.body = "Estimado asociado, se le notifica el estado de la resolucion de su socilitud. Por favor, revise el archivo adjunto para más detalles.";
                }

                info = await _beneCorreo.EncolarAsync(new CorreoBeneficio
                {
                    CodEmpresa = codCliente,
                    CodPlantilla = BeneficiosPlantillas.Resolucion,
                    CodSmtp = codSmtp,
                    Para = correo.email,
                    Cc = CuentaRemitente(codCliente, codSmtp),
                    Usuario = primero.usuario ?? string.Empty,
                    Datos = new Dictionary<string, string> { ["MENSAJE"] = primero.body },
                    Adjuntos = ConstruirAdjuntos(parametros)
                });

                if (info.Code == 0)
                {
                    RegistrarBitacora(codCliente, primero.cod_beneficio ?? string.Empty, primero.consec, "Notifica",
                        $"Notificación de Resolución de Solicitud enviada a {correo.email}", primero.usuario ?? string.Empty);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = "BeneficioNotificaResolucion_Enviar - " + ex.Message;
            }

            return info;
        }

        /// <summary>
        /// Envía por correo la boleta de cobro de mora al Departamento de Cobros, adjuntando el archivo recibido.
        /// El documento llega desde Angular en base64 (no se genera en el API).
        /// </summary>
        public async Task<ErrorDto> BeneRegistroMora_Enviar(int CodCliente, DocArchivoBeneIntegralDto parametros)
        {
            var info = new ErrorDto { Code = 0 };

            if (parametros == null)
            {
                return new ErrorDto { Code = -1, Description = "Los parámetros no pueden ser nulos" };
            }

            try
            {
                var emailCobros = ObtenerEmailCobros(CodCliente);

                var expediente = (parametros.id_beneficio ?? 0).ToString().PadLeft(5, '0')
                               + parametros.cod_beneficio
                               + (parametros.consec ?? 0).ToString().PadLeft(5, '0');

                if (string.IsNullOrWhiteSpace(parametros.body))
                {
                    parametros.body = "Estimados compañeros del Departamento de Cobros, se adjunta boleta para la aplicación del cobro al asociado con la cédula: "
                                    + parametros.cedula + ".";
                }

                info = await _beneCorreo.EncolarAsync(new CorreoBeneficio
                {
                    CodEmpresa = CodCliente,
                    CodPlantilla = BeneficiosPlantillas.CobroMora,
                    CodSmtp = ObtenerCodSmtpCategoria(CodCliente, parametros.id_beneficio)?.Trim() ?? string.Empty,
                    Para = emailCobros ?? string.Empty,
                    Usuario = parametros.usuario ?? string.Empty,
                    Datos = new Dictionary<string, string> { ["MENSAJE"] = parametros.body, ["EXPEDIENTE"] = expediente },
                    Adjuntos = ConstruirAdjuntos(new List<DocArchivoBeneIntegralDto> { parametros })
                });

                if (info.Code == 0)
                {
                    RegistrarBitacora(CodCliente, parametros.cod_beneficio ?? string.Empty, parametros.consec, "Notifica",
                        $"Notificación Cobro de Mora enviada a {emailCobros}", parametros.usuario ?? string.Empty);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }

            return info;
        }

        // ==================== Helpers de notificación ====================

        /// <summary>
        /// Obtiene el código SMTP de la categoría asociada al beneficio otorgado.
        /// </summary>
        private string? ObtenerCodSmtpCategoria(int codCliente, int? idBeneficio)
        {
            const string sql = @"
                SELECT C.COD_SMTP FROM AFI_BENE_CATEGORIAS C
                WHERE C.COD_CATEGORIA IN (
                    SELECT B.COD_CATEGORIA FROM AFI_BENEFICIOS B WHERE B.COD_BENEFICIO IN (
                        SELECT DISTINCT H.COD_BENEFICIO FROM AFI_BENE_OTORGA H WHERE H.ID_BENEFICIO = @idBeneficio))";

            var result = DbHelper.ExecuteSingleQuery<string>(CreatePortalDb(), codCliente, sql, null, new { idBeneficio });
            return result.Result;
        }

        /// <summary>
        /// Obtiene el correo del Departamento de Cobros configurado en parámetros.
        /// </summary>
        private string? ObtenerEmailCobros(int codCliente)
        {
            const string sql = "SELECT VALOR FROM SIF_PARAMETROS WHERE COD_PARAMETRO = @codParametro";
            var result = DbHelper.ExecuteSingleQuery<string>(CreatePortalDb(), codCliente, sql, null, new { codParametro = _notificacionCobros });
            return result.Result;
        }

        /// <summary>
        /// Usuario de la cuenta SMTP, para dejarle copia del correo.
        /// </summary>
        private string CuentaRemitente(int codCliente, string codSmtp)
        {
            if (string.IsNullOrWhiteSpace(codSmtp))
            {
                return string.Empty;
            }

            return _envioCorreoDB.CorreoConfig(codCliente, codSmtp).Result?.User ?? string.Empty;
        }

        /// <summary>
        /// Construye la lista de adjuntos a partir de los archivos en base64 recibidos.
        /// </summary>
        private static List<AdjuntoBeneficio> ConstruirAdjuntos(List<DocArchivoBeneIntegralDto> archivos)
            => archivos
                .Where(a => !string.IsNullOrEmpty(a.filecontent))
                .Select(a => new AdjuntoBeneficio(a.filename ?? "archivo.pdf", a.filecontent!))
                .ToList();
    }
}
