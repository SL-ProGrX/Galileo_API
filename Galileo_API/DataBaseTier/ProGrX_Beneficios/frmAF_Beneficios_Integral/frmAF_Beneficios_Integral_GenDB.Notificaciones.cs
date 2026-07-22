using Microsoft.AspNetCore.Http;
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
                var codCategoria = ObtenerCodSmtpCategoria(codCliente, primero.id_beneficio);
                var eConfig = ObtenerCorreoConfig(codCliente, codCategoria);

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

                var body = ArmarBodyHtml("Notificación de resolucion", primero.body);
                var attachments = ConstruirAdjuntos(parametros);

                if (_sendEmail == "Y" && eConfig != null)
                {
                    var emailRequest = new EmailRequest
                    {
                        To = correo.email,
                        CopyHide = eConfig.User,
                        From = eConfig.User,
                        Subject = "Notificación de Resolución",
                        Body = body,
                        Attachments = attachments
                    };

                    await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, info);
                }

                RegistrarBitacora(codCliente, primero.cod_beneficio ?? string.Empty, primero.consec, "Notifica",
                    $"Notificación de Resolución de Solicitud enviada a {correo.email}", primero.usuario ?? string.Empty);
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
                var codCategoria = ObtenerCodSmtpCategoria(CodCliente, parametros.id_beneficio);
                var eConfig = ObtenerCorreoConfig(CodCliente, codCategoria);
                var emailCobros = ObtenerEmailCobros(CodCliente);

                var expediente = (parametros.id_beneficio ?? 0).ToString().PadLeft(5, '0')
                               + parametros.cod_beneficio
                               + (parametros.consec ?? 0).ToString().PadLeft(5, '0');

                if (string.IsNullOrWhiteSpace(parametros.body))
                {
                    parametros.body = "Estimados compañeros del Departamento de Cobros, se adjunta boleta para la aplicación del cobro al asociado con la cédula: "
                                    + parametros.cedula + ".";
                }

                var body = ArmarBodyHtml("Boleta para aplicación del beneficio al Departamento de Cobros", parametros.body);
                var attachments = ConstruirAdjuntos(new List<DocArchivoBeneIntegralDto> { parametros });

                if (_sendEmail == "Y" && eConfig != null)
                {
                    var emailRequest = new EmailRequest
                    {
                        To = emailCobros ?? string.Empty,
                        From = eConfig.User,
                        Subject = "Aplicación de beneficio - expediente " + expediente,
                        Body = body,
                        Attachments = attachments
                    };

                    await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, info);
                }

                RegistrarBitacora(CodCliente, parametros.cod_beneficio ?? string.Empty, parametros.consec, "Notifica",
                    $"Notificación Cobro de Mora enviada a {emailCobros}", parametros.usuario ?? string.Empty);
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
        /// Resuelve la configuración de correo (SMTP) para la categoría indicada.
        /// </summary>
        private EnvioCorreoModels? ObtenerCorreoConfig(int codCliente, string? codCategoria)
        {
            var resultado = _envioCorreoDB.CorreoConfig(codCliente, codCategoria ?? string.Empty);
            return (resultado != null && resultado.Code == 0) ? resultado.Result : null;
        }

        /// <summary>
        /// Arma el cuerpo HTML del correo con el título y contenido indicados.
        /// </summary>
        private static string ArmarBodyHtml(string titulo, string? contenido)
            => $@"<html lang=""es""><head><meta charset=""UTF-8"">
                  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                  <title>{titulo}</title></head>
                  <body><p>{contenido}</p><p>ASECCSS</p></body></html>";

        /// <summary>
        /// Construye la lista de adjuntos a partir de los archivos en base64 recibidos.
        /// </summary>
        private static List<IFormFile> ConstruirAdjuntos(List<DocArchivoBeneIntegralDto> archivos)
        {
            var attachments = new List<IFormFile>();

            foreach (var archivo in archivos)
            {
                if (string.IsNullOrEmpty(archivo.filecontent))
                {
                    continue;
                }

                var bytes = Convert.FromBase64String(archivo.filecontent);
                attachments.AddRange(ConvertByteArrayToIFormFileList(bytes, archivo.filename ?? "archivo"));
            }

            return attachments;
        }

        /// <summary>
        /// Convierte un arreglo de bytes en una lista de IFormFile para adjuntar al correo.
        /// </summary>
        private static List<IFormFile> ConvertByteArrayToIFormFileList(byte[] byteArray, string fileName)
        {
            var formFiles = new List<IFormFile>();

            if (byteArray == null || byteArray.Length == 0)
            {
                return formFiles;
            }

            var stream = new MemoryStream(byteArray);
            var formFile = new FormFile(stream, 0, byteArray.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/octet-stream"
            };

            formFiles.Add(formFile);
            return formFiles;
        }
    }
}
