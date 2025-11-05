using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_Beneficios_Integral_GenDB
    {
        private readonly IConfiguration _config;
        private readonly frmAF_BeneficioAsgDB frmAF_BeneficioAsgDB;
        private readonly EnvioCorreoDB _envioCorreoDB;
        private bool bAplicaParcial = false;
        mProGrx_Main mProGrx_Main;
        private readonly mBeneficiosDB _mBeneficiosDB;
        private mProGrX_AuxiliarDB mAuxiliarDB;
        public string sendEmail = "";
        public string nofiticacionCobros = "";

        public frmAF_Beneficios_Integral_GenDB(IConfiguration config)
        {
            _config = config;
            frmAF_BeneficioAsgDB = new frmAF_BeneficioAsgDB(_config);
            mProGrx_Main = new mProGrx_Main(_config);
            _envioCorreoDB = new EnvioCorreoDB(_config);
            _mBeneficiosDB = new mBeneficiosDB(config);
            mAuxiliarDB = new mProGrX_AuxiliarDB(_config);
            sendEmail = _config.GetSection("AppSettings").GetSection("EnviaEmail").Value.ToString();
            nofiticacionCobros = _config.GetSection("AFI_Beneficios").GetSection("NotificacionCobros").Value.ToString();
        }

        public ErrorDto SbSIFRegistraTags(SIFRegistraTagsRequestDTO data)
        {
            return mProGrx_Main.SbSIFRegistraTags(data);
        }

        /// <summary>
        /// Obtengo la lista de Beneficios por categoria
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="categoria"></param>
        /// <returns></returns>
        public ErrorDto<List<BeneficiosLista>> BeneficiosLista_Obtener(int CodCliente, string categoria)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<BeneficiosLista>>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT B.COD_BENEFICIO as item
                                        ,B.DESCRIPCION, B.TIPO , 
	                                    (SELECT MONTO FROM AFI_BENE_GRUPOS G WHERE G.COD_GRUPO =  B.COD_GRUPO) MONTO
                                         FROM AFI_BENEFICIOS B
                                  WHERE COD_CATEGORIA = '{categoria}'";
                    response.Result = connection.Query<BeneficiosLista>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "BeneficiosLista_Obtener - " + ex.Message;
                response.Result = null;
            }
            return response;
        }

        #region Notificaciones 
        /// <summary>
        /// Envia correo de solicitud de beneficio nuevo
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="socio"></param>
        /// <returns></returns>
        private async Task CorreoNotificacionSolicitud_Enviar(
        int CodCliente,
            AfiBeneDatosCorreo socio, string cod_beneficio, string consec, int id_beneficio, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto response = new ErrorDto();
            EnvioCorreoModels eConfig = new();
            string codCategoria = "";
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var queryCodBene = @$"SELECT C.COD_SMTP FROM AFI_BENE_CATEGORIAS C
                                            WHERE C.COD_CATEGORIA IN (
                                            SELECT B.COD_CATEGORIA FROM AFI_BENEFICIOS B
                                            WHERE B.COD_BENEFICIO IN (
                                            SELECT DISTINCT H.COD_BENEFICIO FROM AFI_BENE_OTORGA H
                                            WHERE H.ID_BENEFICIO = {id_beneficio}
                                            )
                                            )";
                    codCategoria = connection.Query<string>(queryCodBene).FirstOrDefault();

                    eConfig = _envioCorreoDB.CorreoConfig(CodCliente, codCategoria);
                }

                string body = "";

                if(codCategoria == "B_PSICO")
                {
                    body = @$"<html lang=""es"">
                                <head>
                                    <meta charset=""UTF-8"">
                                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                    <title>Confirmación de solicitud por Beneficio de {socio.beneficio} {socio.expediente} </title>
                                </head>
                                <body>
                                    <p>Estimado (a) asociado (a):</p>

                                    <p>Nos complace informarle que la solicitud de {socio.beneficio} fue registrada en nuestro sistema. </p>

                                    <p>Este servicio de atención psicológica en ASECCSS consta de 3 sesiones aproximadamente con la intención de brindar un acompañamiento emocional en el momento que te encuentres enfrentando actualmente. </p>
                                    <p>Pronto nuestro equipo se pondrá en contacto con usted para brindarle más detalles y las indicaciones del proceso y así como fecha de inicio de la primera sesión.</p>

                                    <p>En la Gerencia de Bienestar Social y Sostenibilidad de ASECCSS estamos para servirle. ¡Nuestro compromiso solidario es con el asociado/a y su familia!</p>
                                    <p>ASECCSS</p>
                                </body>
                                </html>";
                }
                else
                {
                    body = @$" <html lang=""es"">
                                <head>
                                    <meta charset=""UTF-8"">
                                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                    <title>Confirmación de solicitud por Beneficio de {socio.beneficio} {socio.expediente} </title>
                                </head>
                                <body>
                                    <p>Estimado (a) asociado (a):</p>

                                    <p>Nos complace informarle que la solicitud de {socio.beneficio} fue registrada en nuestro sistema. </p>

                                    <p>Le recordamos algunos puntos fundamentales: </p>
                                    <p>1.De la manera más atenta, aclaramos que el llenado del formulario de solicitud y la presentación de los requisitos adjuntos no garantiza la aprobación del beneficio.</p>
                                    <p>2. Todas las solicitudes deben ser analizadas por el equipo de Bienestar Social y Sostenibilidad.</p>
                                    <p>3. Para finalizar, queremos informarle que las resoluciones se estarán brindando luego de la verificación de los requisitos completos de acuerdo con los plazos estipulados en el Reglamento del PROBESOL.</p>
                                    <p>Podes consultar el Reglamento PROBESOL y toda la información de nuestros beneficios solidarios en nuestro sitio web: https://aseccss.com/beneficios-solidarios/</p>
                                    <p>En la Gerencia de Bienestar Social y Sostenibilidad de ASECCSS estamos para servirle. ¡Nuestro compromiso solidario es con el asociado/a y su familia!</p>
                                    <p>ASECCSS</p>
                                </body>
                                </html>";
                }

                //string body = @$"<html lang=""es"">
                //                <head>
                //                    <meta charset=""UTF-8"">
                //                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                //                    <title>Notificaci�n de Solicitud</title>
                //                </head>
                //                <body>
                //                    <p>Estimado/a <strong>{socio.nombre}</strong>, c�dula <strong>{socio.cedula}</strong>,</p>

                //                    <p>Reciba un cordial saludo de parte de la Gerencia de Bienestar Social y Sostenibilidad de ASECCSS.</p>

                //                    <p>De la manera m�s atenta le comunicamos que su persona registr� de forma efectiva su solicitud por el Beneficio Solidario de <strong>{socio.beneficio}</strong>.</p>

                //                    <p><em>Todos los beneficios solidarios de ASECCSS conllevan un proceso de revisi�n de requisitos y an�lisis.</em></p>

                //                    <p>Le recomendamos mantenerse al tanto de su correo electr�nico, ya que es el medio que se utilizar� para notificarle la resoluci�n.</p>

                //                    <p>-Fecha de solicitud: {DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year} </p>

                //                    <p>-N� de expediente: {socio.expediente} </p>

                //                    <p>-Estado: Pendiente de Valoracion</p>

                //                    <p>Saludos cordiales,</p>

                //                    <p>Gerencia de Bienestar Social y Sostenibilidad</p>";


                List<IFormFile> Attachments = new List<IFormFile>();

                EmailRequest emailRequest = new EmailRequest();

                if (eConfig != null)
                {
                    if (sendEmail == "Y")
                    {
                        emailRequest.To = socio.email;
                        emailRequest.From = eConfig.User;
                        emailRequest.Subject = "Notificación de Solicitud";
                        emailRequest.Body = body;
                        emailRequest.Attachments = Attachments;

                        await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, response);
                    }

                    _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                    {
                        EmpresaId = CodCliente,
                        cod_beneficio = cod_beneficio,
                        consec = int.Parse(consec),
                        movimiento = "Notifica",
                        detalle = $@"Notificación Solicitud de Beneficio enviada a {socio.email}",
                        registro_usuario = usuario
                    });

                }


            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
        }

        /// <summary>
        /// Envia correo de notificacion de resolucion de beneficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public async Task<ErrorDto> BeneficioNotificaResolucion_Enviar(List<DocArchivoBeneIntegralDTO> parametros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(Convert.ToInt32(parametros[0].codCliente));
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            EnvioCorreoModels eConfig = new();

            if (parametros == null || parametros.Count == 0)
            {
                info.Code = -1;
                info.Description = "No se recibieron parámetros para el envío de correo";
                return info;
            }

            try
            {

                using var connection = new SqlConnection(clienteConnString);
                {
                    var queryCodBene = @$"SELECT C.COD_SMTP FROM AFI_BENE_CATEGORIAS C
                                            WHERE C.COD_CATEGORIA IN (
                                            SELECT B.COD_CATEGORIA FROM AFI_BENEFICIOS B
                                            WHERE B.COD_BENEFICIO IN (
                                            SELECT DISTINCT H.COD_BENEFICIO FROM AFI_BENE_OTORGA H
                                            WHERE H.ID_BENEFICIO = '{parametros[0].id_beneficio}'
                                            )
                                            )";
                    string codCategoria = connection.Query<string>(queryCodBene).FirstOrDefault();

                    eConfig = _envioCorreoDB.CorreoConfig(Convert.ToInt32(parametros[0].codCliente), codCategoria);
                }

                //Busco el correo del socio
                AfiBeneDatosCorreo correo = _envioCorreoDB.BuscoDatosSocioBeneficio(Convert.ToInt32(parametros[0].codCliente), parametros[0].cedula, parametros[0].cod_beneficio);

                if(string.IsNullOrEmpty(correo.email))
                {
                    info.Code = -1;
                    info.Description = "El asociado no tiene un correo electrónico registrado en Datos Persona";
                    return info;
                }

                AfiBeneDatosCorreo email = new AfiBeneDatosCorreo
                {
                    nombre = correo.nombre,
                    cedula = correo.cedula,
                    email = correo.email,
                    beneficio = correo.beneficio,
                    expediente = parametros[0].id_beneficio.ToString().PadLeft(5, '0') + parametros[0].cod_beneficio + parametros[0].consec.ToString().PadLeft(5, '0')
                };

                if (parametros[0].body.Trim() == "")
                {
                    parametros[0].body = "Estimado asociado, se le notifica el estado de la resolucion de su socilitud. Por favor, revise el archivo adjunto para más detalles.";
                }

                string body = @$"<html lang=""es"">
                                    <head>
                                        <meta charset=""UTF-8"">
                                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                        <title>Notificación de resolucion</title>
                                    </head>
                                    <body>
                                        <p>{parametros[0].body}</p>
                                        <p>ASECCSS</p>
                                    </body>
                                    </html>";

                List<IFormFile> Attachments = new List<IFormFile>();
                var file = new List<IFormFile>();
                foreach (DocArchivoBeneIntegralDTO fileE in parametros)
                {
                    //Convierto de base64 a byte[]
                    byte[] fileContent = Convert.FromBase64String(fileE.filecontent);

                    file = ConvertByteArrayToIFormFileList(fileContent, fileE.filename);
                    Attachments.AddRange(file);
                }

                if (sendEmail == "Y")
                {
                    EmailRequest emailRequest = new EmailRequest();

                    emailRequest.To = correo.email;
                    emailRequest.CopyHide = eConfig.User;
                    emailRequest.From = eConfig.User;
                    emailRequest.Subject = "Notificación de Resolución";
                    emailRequest.Body = body;
                    emailRequest.Attachments = Attachments;

                    if (eConfig != null)
                    {
                        await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, info);
                    }

                }

                _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                {
                    EmpresaId = Convert.ToInt32(parametros[0].codCliente),
                    cod_beneficio = parametros[0].cod_beneficio,
                    consec = parametros[0].consec,
                    movimiento = "Notifica",
                    detalle = $@"Notificación de Resolución de Solicitud enviada a {correo.email}",
                    registro_usuario = parametros[0].usuario
                });


            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = "BeneficioNotificaResolucion_Enviar - " + ex.Message;
            }
            return info;
        }

        /// <summary>
        /// Convierte un arreglo de bytes a una lista de IFormFile
        /// </summary>
        /// <param name="byteArray"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private List<IFormFile> ConvertByteArrayToIFormFileList(byte[] byteArray, string fileName)
        {
            var formFiles = new List<IFormFile>();

            if (byteArray == null || byteArray.Length == 0)
                return formFiles;

            // Crear un stream a partir del arreglo de bytes
            var stream = new MemoryStream(byteArray);

            // Crear una instancia de FormFile con el stream
            var formFile = new FormFile(stream, 0, byteArray.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/octet-stream" // Puedes especificar el tipo de contenido si lo conoces
            };

            // Agregar el FormFile a la lista
            formFiles.Add(formFile);

            return formFiles;
        }

        #endregion

        /// <summary>
        /// Obtengo la informacion del benficio seleccionado.
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="id_beneficio"></param>
        /// <returns></returns>
        public ErrorDto<BeneficioGeneral> BeneficioIntegralGeneral_Obtener(int CodCliente, int? id_beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<BeneficioGeneral>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT
                                    A.ID_BENEFICIO,
                                    A.CONSEC, 
                                    A.COD_BENEFICIO,
                                    A.TIPO,
                                    A.NOTAS,
                                    A.CRECE_GRUPO,
                                    A.CEDULA, 
                                    A.SOLICITA,
                                    A.NOMBRE,
                                    A.MONTO, 
                                    A.MONTO_APLICADO,
                                    A.MODIFICA_MONTO,
                                    B.NOTAS AS observaciones_monto,
                                    A.ESTADO,  
                                    C.NOTAS AS estadoObservaciones,
                                    A.FENA_NOMBRE AS desa_nombre, 
                                    A.FENA_DESCRIPCION AS desa_descripcion, 
                                    A.SEPELIO_IDENTIFICACION, 
                                    A.SEPELIO_NOMBRE, 
                                    A.SEPELIO_FECHA_FALLECIMIENTO,
                                    D.COD_MOTIVO,
                                    A.REGISTRA_FECHA,
                                    A.REGISTRA_USER,
                                    A.MODIFICA_USUARIO,
                                    A.MODIFICA_FECHA,
                                    A.ID_PROFESIONAL, A.ID_APT_CATEGORIA,
                                    A.REQUIERE_JUSTIFICACION, 
                                    (SELECT TOP 1 PAGOS_MULTIPLES FROM AFI_BENEFICIOS WHERE COD_BENEFICIO = A.COD_BENEFICIO ) AS PAGOS_MULTIPLES, 
                                    A.APLICA_MORA, 
                                    A.APLICA_PAGO_MASIVO 
                                    FROM AFI_BENE_OTORGA A
                                    LEFT JOIN AFI_BENE_REGISTRO_MONTOS B ON A.COD_BENEFICIO = B.COD_BENEFICIO AND A.CONSEC = B.CONSEC
                                    LEFT JOIN AFI_BENE_REGISTRO_ESTADOS C ON A.COD_BENEFICIO = C.COD_BENEFICIO AND A.CONSEC = C.CONSEC
									LEFT JOIN AFI_BENE_REGISTRO_MOTIVOS D ON A.COD_BENEFICIO = D.COD_BENEFICIO AND A.CONSEC = D.CONSEC
                                    WHERE ID_BENEFICIO = '{id_beneficio}' ";

                    response.Result = connection.Query<BeneficioGeneral>(query).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "DatosPersona_Obtener - " + ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtengo informacion de los productos del beneficio seleccionado
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="consec"></param>
        /// <param name="cod_beneficio"></param>
        /// <returns></returns>
        public ErrorDto<List<AfiBenProductoDTO>> BeneIntegralGenProductos_Obtener(int CodCliente, int consec, string cod_beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfiBenProductoDTO>>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT [CONSEC]
                                          ,[COD_BENEFICIO]
                                          ,A.[COD_PRODUCTO]
	                                      ,B.DESCRIPCION AS prodDesc
                                          ,[CANTIDAD]
                                          ,A.[COSTO_UNIDAD]
                                      FROM [AFI_BENE_PRODASG] A left join AFI_BENE_PRODUCTOS B
                                      ON A.COD_PRODUCTO = B.COD_PRODUCTO WHERE CONSEC = {consec} AND COD_BENEFICIO = '{cod_beneficio}' ";
                    response.Result = connection.Query<AfiBenProductoDTO>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "BeneIntegralGenProductos_Obtener - " + ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Guarda el beneficio general
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="fuente"></param>
        /// <param name="beneficioGeneral"></param>
        /// <returns></returns>
        public async Task<ErrorDto<BeneficioGeneralDatos>> BeneficioIntegralGeneral_Guardar(int CodCliente, string fuente, BeneficioGeneralDatos beneficioGeneral)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<BeneficioGeneralDatos>();

            response.Code = 0;
            
            //primer filtro de validaciones
            if (beneficioGeneral.cedula == "" || beneficioGeneral.cedula == null)
            {
                response.Code = -1;
                response.Description = "Cédula no puede ser nula";
                return response;
            }

            //Valida los permisos del usuario
            using var connection = new SqlConnection(clienteConnString);
            {
                var permisos = $@"select dbo.fxBeneficio_ValidacionPermisos( '{beneficioGeneral.modifica_usuario}', '{beneficioGeneral.cod_categoria}', '{beneficioGeneral.estado.item}' ) ";
                var valida = connection.Query<string>(permisos).FirstOrDefault();

                if (valida != "")
                {
                    response.Code = -1;
                    response.Description += "\n " + valida;
                    return response;
                }
            }

            //segundo filtro de validaciones
            var perError = _mBeneficiosDB.ValidarPersona(CodCliente, beneficioGeneral.cedula.Trim(), beneficioGeneral.cod_beneficio.item);
            if (perError.Code == -1)
            {
                response.Code = -1;
                response.Description += perError.Description;
                return response;
            }


            //Valida si el beneficio aplica parcial
            AfiBeneficiosDTO afiBeneficios = frmAF_BeneficioAsgDB.AfiBeneficioDTO_Obtener(CodCliente, beneficioGeneral.cod_beneficio.item).Result;
            bAplicaParcial = afiBeneficios.aplica_parcial == 1 ? true : false;

            //valida requisitos
            if(beneficioGeneral.consec != null)
            {
                var respreq = _mBeneficiosDB.ValidaRequisitos(CodCliente, beneficioGeneral.estado.item, beneficioGeneral.cod_beneficio.item, (int)beneficioGeneral.consec);
                if (respreq.Code == -1)
                {
                    response.Code = respreq.Code;
                    response.Description = respreq.Description;
                    return response;
                }

            }

            //Valida Beneficios datos
            var errBene = _mBeneficiosDB.ValidarBeneficioDato(CodCliente, beneficioGeneral);
            if (errBene.Code == -1)
            {
                response.Code = errBene.Code;
                response.Description = errBene.Description;
                return response;
            }

            //Si el beneficio es monetario y si es nuevo
            if (beneficioGeneral.id_beneficio == 0)
            {
                response = await Guarda_Beneficio(CodCliente, beneficioGeneral, "S", fuente);
            }
            else
            {
                response = Actualiza_Beneficio(CodCliente, beneficioGeneral, "S", fuente);
            }

            //if (response.Code != -1)
            //{
            //    response.Description = response.Description;
            //}

            return response;

        }

        /// <summary>
        /// Guarda el beneficio neuvo en la tabla de beneficios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="beneficio"></param>
        /// <param name="modificaMonto"></param>
        /// <param name="fuente"></param>
        /// <returns></returns>
        public async Task<ErrorDto<BeneficioGeneralDatos>> Guarda_Beneficio(int CodCliente, BeneficioGeneralDatos beneficio, string modificaMonto, string fuente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<BeneficioGeneralDatos>();

            //tercer filtro de validaciones
            var justError = _mBeneficiosDB.ValidarBeneficioJustificaDato(CodCliente, beneficio, beneficio.requiere_justificacion);
            if (justError.Code == -1)
            {
                response.Code = justError.Code;
                response.Description = justError.Description;
                return response;
            }

            //busco COD de la oficina donde se realiza el registro.
            List<SIFOficinasUsuarioResultDTO> empresa = frmAF_BeneficioAsgDB.CargaOficinas(CodCliente, beneficio.registra_user).Result;
            try
            {
                //Busco el consecutivo del beneficio (este depende de la categoria)
                long vBeneConsec = _mBeneficiosDB.fxConsec(CodCliente, beneficio.cod_beneficio.item);
                response.Description = vBeneConsec.ToString();
                beneficio.consec = Convert.ToInt32(vBeneConsec);

                string sepelio_fecha_fallecimiento = mAuxiliarDB.validaFechaGlobal(beneficio.sepelio_fecha_fallecimiento);

                using var connection = new SqlConnection(clienteConnString);
                {
                    //valida beneficios crece
                    //var tallerCrece = $@"select COUNT(*) FROM AFI_BENE_OTORGA WHERE TRIM(CEDULA) = '{beneficio.cedula.Trim()}' AND COD_BENEFICIO IN 
	                   //                     (select COD_BENEFICIO FROM AFI_BENEFICIOS WHERE COD_CATEGORIA = 'B_CRECE')
	                   //                     AND REGISTRA_FECHA between DATEADD(YEAR, -1, GETDATE()) and GETDATE() ";
                    //var talleres = connection.Query<int>(tallerCrece).FirstOrDefault();
                    //if (talleres > 0)
                    //{
                    //    response.Code = -1;
                    //    response.Description = "...El Asociado se encuentra cursando un Taller de educacion financiera ";
                    //    response.Result = null;
                    //    return response;
                    //}

                    //valida estado inicial
                    var estadoInicial = $@"select TOP 1 E.COD_ESTADO from AFI_BENE_ESTADOS E WHERE E.P_INICIA = 1 AND E.PROCESO = 'T' AND E.COD_ESTADO IN ( 
                                            SELECT G.COD_ESTADO FROM AFI_BENE_GRUPO_ESTADOS G WHERE G.COD_GRUPO IN ( 
                                            SELECT B.COD_GRUPO FROM AFI_GRUPO_BENEFICIO B WHERE COD_BENEFICIO = '{beneficio.cod_beneficio.item}'))
                                            ORDER BY E.COD_ESTADO DESC";
                    var estado = connection.Query<string>(estadoInicial).FirstOrDefault();

                    modificaMonto = beneficio.tipo.item == "P" ? "N" : "S";

                    var query = $@"insert afi_bene_otorga (
                                            consec,
                                            cod_beneficio,
                                            cedula,
                                            monto,
                                            modifica_monto,
                                            registra_user,
                                            registra_fecha,
                                            estado,
                                            notas,
                                            Solicita,
                                            nombre,
                                            tipo,
                                            cod_oficina, 
                                            MONTO_APLICADO, 
                                            FENA_NOMBRE, 
                                            FENA_DESCRIPCION,
                                            SEPELIO_IDENTIFICACION,
                                            SEPELIO_NOMBRE, 
                                            SEPELIO_FECHA_FALLECIMIENTO, 
                                            CRECE_GRUPO, ID_PROFESIONAL,ID_APT_CATEGORIA,
                                            REQUIERE_JUSTIFICACION,
                                            APLICA_MORA,
                                            APLICA_PAGO_MASIVO
                                )values(
                                            {vBeneConsec},
                                            '{beneficio.cod_beneficio.item}',
                                            '{beneficio.cedula.Trim()}',
                                            {beneficio.monto},
                                            '{modificaMonto}',
                                            '{beneficio.registra_user.ToUpper()}',
                                              Getdate(),
                                            '{estado}',
                                            '{beneficio.notas}',
                                            '{beneficio.cedula.Trim()}',
                                            '{beneficio.nombre}',
                                            '{beneficio.tipo.item}',
                                            '{empresa[0].Titular}', 
                                             {beneficio.monto_aplicado}, 
                                            '{beneficio.desa_nombre}',
                                            '{beneficio.desa_descripcion}',
                                            '{beneficio.sepelio_identificacion}',
                                            '{beneficio.sepelio_nombre}', 
                                            '{sepelio_fecha_fallecimiento}', 
                                            '{beneficio.crece_grupo}', 
                                            {((beneficio.id_profesional == null) ? 0 : beneficio.id_profesional.item)} ,
                                            {((beneficio.id_apt_categoria == null) ? 0 : beneficio.id_apt_categoria.item)},
                                            {((beneficio.requiere_justificacion == true) ? 1 : 0)},
                                            {((beneficio.aplica_mora == true) ? 1 : 0)},
                                            {((beneficio.aplica_pago_masivo == true) ? 1 : 0)}
                                            )";


                    int resp = connection.Execute(query);

                    //devuelve el id del beneficio
                    query = "SELECT IDENT_CURRENT('afi_bene_otorga') as 'id'";
                    var id = connection.Query<int>(query).FirstOrDefault();
                    beneficio.id_beneficio = id;


                    if (beneficio.tipo.item != "M" && beneficio.productos != null)
                    {
                        foreach (var prod in beneficio.productos)
                        {
                            query = $@"insert afi_bene_prodasg(consec,cod_beneficio,cod_producto,cantidad,costo_unidad,REGISTRO_FECHA, REGISTRO_USUARIO )
                                 	values({beneficio.consec},'{beneficio.cod_beneficio.item}','{prod.cod_producto}',{prod.cantidad},{prod.costo_unidad}, getDate(), '{beneficio.registra_user}')";
                            resp = connection.Execute(query);

                            //Insert tarjeta regalo
                            //query = $@"select COUNT(*) from afi_bene_productos where tarjeta_regalo = 1 and COD_PRODUCTO = '{prod.cod_producto}'";
                            //var tipoTarjeta = connection.Query<int>(query).FirstOrDefault();
                            //if (tipoTarjeta > 0)
                            //{
                            //    query = $@"insert AFI_BENE_TARJETAS_REGALO(COD_PRODUCTO,REGISTRO_FECHA, REGISTRO_USUARIO, COD_BENEFICIO, CONSEC, CEDULA, ID_BENEFICIO, ESTADO )
                            //     	values('{prod.cod_producto}', getDate(), '{beneficio.registra_user}', '{beneficio.cod_beneficio.item}', {beneficio.consec}, '{beneficio.cedula}', {beneficio.id_beneficio}, 'P' )";
                            //    connection.Execute(query);
                            //}
                        }
                    }

                    string tipo = beneficio.tipo.item == "P" ? "Producto" : beneficio.tipo.item == "M" ? "Monetario" : "Mixto";

                    _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                    {
                        EmpresaId = CodCliente,
                        cod_beneficio = beneficio.cod_beneficio.item,
                        consec = vBeneConsec,
                        movimiento = "Inserta",
                        detalle = $@"Inserta Datos Generales - Beneficio {tipo}: [{id + " " + beneficio.cod_beneficio.item + " " + vBeneConsec}]",
                        registro_usuario = beneficio.registra_user
                    });

                    if (beneficio.cod_motivo != null)
                    {
                        InsertarActualizarMotivos(CodCliente, beneficio);
                    }
                    InsertarActualizarMontos(CodCliente, beneficio, modificaMonto);
                    // InsertarActualizarEstados(CodCliente, beneficio);

                    response.Description = id.ToString() + "@" + beneficio.consec;

                    //Insertar en pagos
                    //Insertar_Pagos(CodCliente, beneficio);

                    //Busco el correo del socio
                    AfiBeneDatosCorreo correo = _envioCorreoDB.BuscoDatosSocioBeneficio(CodCliente, beneficio.cedula, beneficio.cod_beneficio.item);

                    if (string.IsNullOrEmpty(correo.email))
                    {
                        response.Code = -1;
                        response.Description = "El asociado no tiene un correo electrónico registrado en Datos Persona";
                        return response;
                    }

                    AfiBeneDatosCorreo email = new AfiBeneDatosCorreo
                    {
                        nombre = correo.nombre,
                        cedula = correo.cedula,
                        email = correo.email,
                        beneficio = correo.beneficio,
                        expediente = id.ToString().PadLeft(5, '0') + beneficio.cod_beneficio.item.Trim() + vBeneConsec.ToString().PadLeft(5, '0')
                    };

                    query = $@"SELECT COD_CATEGORIA FROM AFI_BENEFICIOS Where COD_BENEFICIO = '{beneficio.cod_beneficio.item}'";
                    string categoria = connection.Query<string>(query).FirstOrDefault();
                    if (categoria != "B_CRECE")
                    {
                        //Envio Correo
                        await CorreoNotificacionSolicitud_Enviar(CodCliente, email,
                            beneficio.cod_beneficio.item.Trim(), beneficio.consec.ToString(), id, beneficio.registra_user);
                    }

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "Guarda_Beneficio - " + ex.Message;
                response.Result = null;
            }
            return response;

        }

        /// <summary>
        /// Actualiza la informacion del beneficio en la tabla de beneficios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="beneficio"></param>
        /// <param name="modificaMonto"></param>
        /// <param name="fuente"></param>
        /// <returns></returns>
        private ErrorDto<BeneficioGeneralDatos> Actualiza_Beneficio(int CodCliente, BeneficioGeneralDatos beneficio, string modificaMonto, string fuente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<BeneficioGeneralDatos>();
            ErrorDto error = new ErrorDto();
            try
            {

                //tercer filtro de validaciones
                //response = _mBeneficiosDB.ValidaBeneficioActualizar(CodCliente, beneficio);
                //if (response.Code == -1)
                //{
                //    return response;
                //}

                using var connection = new SqlConnection(clienteConnString);
                {
                    if (beneficio.cod_motivo != null)
                    {
                        InsertarActualizarMotivos(CodCliente, beneficio);
                    }

                    var query = $@"update afi_bene_otorga set 
                                            notas= '{beneficio.notas}',
                                            Solicita= '{beneficio.cedula.Trim()}',
                                            nombre= '{beneficio.nombre}',
                                            FENA_NOMBRE = '{beneficio.desa_nombre}', 
                                            FENA_DESCRIPCION = '{beneficio.desa_descripcion}',
                                            SEPELIO_IDENTIFICACION = '{beneficio.sepelio_identificacion}',
                                            SEPELIO_NOMBRE = '{beneficio.sepelio_nombre}', 
                                            SEPELIO_FECHA_FALLECIMIENTO = '{beneficio.sepelio_fecha_fallecimiento}', 
                                            CRECE_GRUPO = '{beneficio.crece_grupo}',
                                            TIPO = '{beneficio.tipo.item}' ,
                                            ID_PROFESIONAL = {((beneficio.id_profesional == null) ? 0 : beneficio.id_profesional.item)} ,
                                            ID_APT_CATEGORIA = {((beneficio.id_apt_categoria == null) ? 0 : beneficio.id_apt_categoria.item)}, 
                                            APLICA_MORA = {((beneficio.aplica_mora == true) ? 1 : 0)},
                                            APLICA_PAGO_MASIVO = {((beneficio.aplica_pago_masivo == true) ? 1 : 0)} ,
                                            modifica_usuario = '{beneficio.modifica_usuario}', 
                                            modifica_fecha = GETDATE() 
                                        where id_beneficio = {beneficio.id_beneficio} ";

                    response.Code = connection.Execute(query);

                    
                    string tipo = beneficio.tipo.item == "P" ? "Producto" : beneficio.tipo.item == "M" ? "Monetario" : "Mixto";

                    switch (fuente)
                    {
                        case "DB":
                            _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                            {
                                EmpresaId = CodCliente,
                                cod_beneficio = beneficio.cod_beneficio.item,
                                consec = beneficio.consec,
                                movimiento = "Actualiza",
                                detalle = $@"Actualiza Datos Generales - Beneficio {tipo}: [{beneficio.id_beneficio + " " + beneficio.cod_beneficio.item + " " + beneficio.consec}]",
                                registro_usuario = beneficio.registra_user
                            });
                            break;
                        case "M":
                            error = InsertarActualizarMontos(CodCliente, beneficio, modificaMonto);
                            if (error.Code == -1) {
                                response.Code = error.Code;
                                response.Description = error.Description;
                                return response;
                            }
                            break;
                        case "E":
                            error = InsertarActualizarEstados(CodCliente, beneficio);
                            if (error.Code == -1)
                            {
                                response.Code = error.Code;
                                response.Description = error.Description;
                                return response;
                            }
                            break;
                        default:
                            break;
                    }
                }

                response.Description = beneficio.id_beneficio.ToString() + "@" + beneficio.consec;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "Actualiza_Beneficio - " + ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Actualiza o inserta en Afi_Bene_Registra_Montos del beneficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="beneficio"></param>
        /// <param name="modificaMonto"></param>
        /// <returns></returns>
        private ErrorDto InsertarActualizarMontos(int CodCliente, BeneficioGeneralDatos beneficio, string modificaMonto)
        {
            //BeneficioGeneral beneficio = JsonConvert.DeserializeObject<BeneficioGeneral>(beneficioGeneral);

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    if (ExisteRegitro(CodCliente, beneficio.consec, beneficio.cod_beneficio.item, "MON"))
                    {
                        var querySelect = $"SELECT * FROM afi_bene_otorga WHERE id_beneficio = {beneficio.id_beneficio}";
                        BeneConsultaDatos currentData = connection.QueryFirstOrDefault<BeneConsultaDatos>(querySelect);

                        //Revisa campo monto nuevo 
                        var queryU = $@"SELECT MONTO_NUEVO FROM AFI_BENE_REGISTRO_MONTOS WHERE CONSEC = {beneficio.consec} AND
                                    COD_BENEFICIO = '{beneficio.cod_beneficio.item}' ";
                        var monto = connection.Query<float>(queryU).FirstOrDefault();

                        queryU = @$"UPDATE [dbo].[AFI_BENE_REGISTRO_MONTOS]
                                   SET
                                        [MONTO_NUEVO]  = {beneficio.monto_aplicado}
                                       ,[MONTO_ANTERIOR] = {monto}
                                       ,[NOTAS] = '{beneficio.observaciones_monto}'
                                       ,[REGISTRO_FECHA] = GETDATE()
                                       ,[REGISTRO_USUARIO] ='{beneficio.registra_user}'
                                        WHERE CONSEC = {beneficio.consec} AND [COD_BENEFICIO] = '{beneficio.cod_beneficio.item}' ";

                        connection.Execute(queryU);

                        if (monto != beneficio.monto_aplicado)
                        {

                            _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                            {
                                EmpresaId = CodCliente,
                                cod_beneficio = beneficio.cod_beneficio.item,
                                consec = beneficio.consec,
                                movimiento = "Actualiza",
                                detalle = $@"Actualiza Monto de {monto} a {beneficio.monto_aplicado} ",
                                registro_usuario = beneficio.registra_user
                            });
                        }
                    }
                    else
                    {

                        var queryI = @$"INSERT INTO[dbo].[AFI_BENE_REGISTRO_MONTOS]
                                        ([COD_BENEFICIO]
                                       , [CONSEC]
                                       , [MONTO_NUEVO]
                                       , [MONTO_ANTERIOR]
                                       , [NOTAS]
                                       , [REGISTRO_FECHA]
                                       , [REGISTRO_USUARIO])
                                VALUES
                                       ('{beneficio.cod_beneficio.item}',
                                       {beneficio.consec},
                                       {beneficio.monto_aplicado},
                                       0,
                                       '{beneficio.observaciones_monto}',
                                       GETDATE(),
                                       '{beneficio.registra_user}')";

                        connection.Execute(queryI);

                        _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                        {
                            EmpresaId = CodCliente,
                            cod_beneficio = beneficio.cod_beneficio.item,
                            consec = beneficio.consec,
                            movimiento = "Inserta",
                            detalle = $@"Inserta Monto {beneficio.monto_aplicado}",
                            registro_usuario = beneficio.registra_user
                        });

                    }

                    var query = $@"update afi_bene_otorga set 
                                            monto = {beneficio.monto},
                                            modifica_monto= '{modificaMonto}',
                                            MONTO_APLICADO = '{beneficio.monto_aplicado}' 
                                where id_beneficio = {beneficio.id_beneficio} ";
                    connection.Execute(query);

                    if (beneficio.tipo.item != "M" && beneficio.productos != null)
                    {
                        var QueryExist = $@"DELETE FROM afi_bene_prodasg WHERE cod_beneficio = '{beneficio.cod_beneficio.item}' and consec = {beneficio.consec} ";
                        connection.Execute(QueryExist);

                        //Eliminar tarjetas regalo
                        //QueryExist = $@"DELETE FROM AFI_BENE_TARJETAS_REGALO WHERE cod_beneficio = '{beneficio.cod_beneficio.item}' and consec = {beneficio.consec} ";
                        //connection.Execute(QueryExist);

                        foreach (var prod in beneficio.productos)
                        {
                            query = $@"insert afi_bene_prodasg(consec,cod_beneficio,cod_producto,cantidad,costo_unidad,REGISTRO_FECHA, REGISTRO_USUARIO )
                               	values({beneficio.consec},'{beneficio.cod_beneficio.item}','{prod.cod_producto}',{prod.cantidad},{prod.costo_unidad}, getDate(), '{beneficio.registra_user}')";
                            connection.Execute(query);

                            //Insert tarjeta regalo
                            //query = $@"select COUNT(*) from afi_bene_productos where tarjeta_regalo = 1 and COD_PRODUCTO = '{prod.cod_producto}'";
                            //var tipoTarjeta = connection.Query<int>(query).FirstOrDefault();
                            //if (tipoTarjeta > 0)
                            //{
                            //    query = $@"insert AFI_BENE_TARJETAS_REGALO(COD_PRODUCTO,REGISTRO_FECHA, REGISTRO_USUARIO, COD_BENEFICIO, CONSEC, CEDULA, ID_BENEFICIO, ESTADO )
                            //     	values('{prod.cod_producto}', getDate(), '{beneficio.registra_user}', '{beneficio.cod_beneficio.item}', {beneficio.consec}, '{beneficio.cedula}', {beneficio.id_beneficio}, 'P' )";
                            //    connection.Execute(query);
                            //}
                        }

                        if (beneficio.tipo.item == "P")
                        {
                            //sumar los montos de los productos
                            query = $@"SELECT ISNULL(SUM(ISNULL(CANTIDAD, 0) * COSTO_UNIDAD),0) AS MONTO FROM AFI_BENE_PRODASG WHERE CONSEC = {beneficio.consec} AND COD_BENEFICIO = '{beneficio.cod_beneficio.item}' ";
                            var montoTotalProd = connection.Query<decimal>(query).FirstOrDefault();

                            query = $@"update afi_bene_otorga set MONTO_APLICADO = {montoTotalProd} where id_beneficio = {beneficio.id_beneficio} ";
                            connection.Execute(query);
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = "InsertarActualizarMontos - " + ex.Message;
            }
            return info;
        }

        /// <summary>
        /// Actualiza o inserta en Afi_Bene_Registra_Estados del benficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="beneficio"></param>
        /// <returns></returns>
        private ErrorDto InsertarActualizarEstados(int CodCliente, BeneficioGeneralDatos beneficio)
        {
            // BeneficioGeneral beneficio = JsonConvert.DeserializeObject<BeneficioGeneral>(beneficioGeneral);

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //Estado Anterior
                    var query = $@"SELECT COD_ESTADO FROM AFI_BENE_REGISTRO_ESTADOS WHERE CONSEC = {beneficio.consec} AND COD_BENEFICIO = '{beneficio.cod_beneficio.item}' ";
                    var cod_estado = connection.Query<string>(query).FirstOrDefault();

                    //Obtengo nombre de estado anterior:
                    query = $@"SELECT [DESCRIPCION] FROM [AFI_BENE_ESTADOS] WHERE COD_ESTADO ='{cod_estado}' ";
                    var estadoAnterior = connection.Query<string>(query).FirstOrDefault();

                    //Obtengo nombre de estado actual:
                    query = $@"SELECT [DESCRIPCION] FROM [AFI_BENE_ESTADOS] WHERE COD_ESTADO ='{beneficio.estado.item}' ";
                    var estado = connection.Query<string>(query).FirstOrDefault();

                    //valido si el expediente ya tiene un id_pago en afi_bene_pago 
                    query = $@"SELECT COUNT(ID_PAGO) FROM AFI_BENE_PAGO WHERE COD_BENEFICIO = '{beneficio.cod_beneficio.item}' 
                                AND CONSEC = '{beneficio.consec}' AND CEDULA = '{beneficio.cedula}' AND ESTADO = 'P'";
                    var ordenPagada = connection.Query<int>(query).FirstOrDefault();

                    estado = $@"SELECT COD_ESTADO
                                      FROM [dbo].[AFI_BENE_ESTADOS]
                                      WHERE COD_ESTADO = '{beneficio.estado.item}' AND P_FINALIZA = 1 AND PROCESO = 'A'";
                    string estadoAprob = connection.Query<string>(estado).FirstOrDefault();

                    if (ordenPagada > 0 && estadoAprob == null)
                    {
                        info.Code = -1;
                        info.Description = "No se permite cambiar al estado indicado debido a que este expediente ya tiene un registro de solicitud de pago";
                        return info;
                    }

                    if (ExisteRegitro(CodCliente, beneficio.consec, beneficio.cod_beneficio.item, "E"))
                    {

                        query = @$"UPDATE [dbo].[AFI_BENE_REGISTRO_ESTADOS]
                               SET
                                   [COD_ESTADO]  =  '{beneficio.estado.item}'
                                   ,[NOTAS] = '{beneficio.estadoObservaciones}'
                                   ,[REGISTRO_FECHA] = GETDATE()
                                   ,[REGISTRO_USUARIO] ='{beneficio.registra_user}'
                             WHERE CONSEC = {beneficio.consec} AND  [COD_BENEFICIO] = '{beneficio.cod_beneficio.item}' ";

                        connection.Execute(query);

                        
                        //cuando el estado es aprobado se dispara el siguiente SP
                        if (beneficio.estado.item == "A" || beneficio.estado.descripcion.ToUpper() == "APROBADO")
                        {
                            TrdDocumentosModel trdDocumentos = new TrdDocumentosModel();
                            trdDocumentos.CodDocumento = "10";
                            trdDocumentos.Consecutivo = beneficio.id_beneficio.ToString();
                            trdDocumentos.IdSobre = null;
                            trdDocumentos.IdEstado = 1;
                            trdDocumentos.ConfirmaRecepcion = 2;
                            trdDocumentos.FechaActualiza = null;
                            trdDocumentos.UsuarioActualiza = null;
                            trdDocumentos.FechaInserta = DateTime.Now;
                            trdDocumentos.UsuarioInserta = beneficio.registra_user;
                            trdDocumentos.CodBarras = beneficio.id_beneficio.ToString();
                            trdDocumentos.Descripcion = null;


                            var values = new
                            {
                                CodDocumento = trdDocumentos.CodDocumento,
                                Consecutivo = trdDocumentos.Consecutivo,
                                IdSobre = trdDocumentos.IdSobre,
                                IdEstado = trdDocumentos.IdEstado,
                                ConfirmaRecepcion = trdDocumentos.ConfirmaRecepcion,
                                FechaActualiza = trdDocumentos.FechaActualiza,
                                UsuarioActualiza = trdDocumentos.UsuarioActualiza,
                                FechaInserta = trdDocumentos.FechaInserta,
                                UsuarioInserta = trdDocumentos.UsuarioInserta,
                                CodBarras = trdDocumentos.CodBarras,
                                Descripcion = trdDocumentos.Descripcion,
                                Resultado = 0
                            };

                            var sp = connection.ExecuteAsync("dbo.spTrdDocumentosIns", values, commandType: System.Data.CommandType.StoredProcedure);
                        }


                        if (cod_estado != beneficio.estado.item)
                        {
                            _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                            {
                                EmpresaId = CodCliente,
                                cod_beneficio = beneficio.cod_beneficio.item,
                                consec = beneficio.consec,
                                movimiento = "Actualiza",
                                detalle = $@"Actualiza Estado de {estadoAnterior} a {estado}",
                                registro_usuario = beneficio.registra_user
                            });

                        }

                    }
                    else
                    {

                        query = @$"INSERT INTO[dbo].[AFI_BENE_REGISTRO_ESTADOS]
                                        ([COD_BENEFICIO]
                                       , [CONSEC]
                                       , [COD_ESTADO]
                                       , [NOTAS]
                                       , [REGISTRO_FECHA]
                                       , [REGISTRO_USUARIO])
                                VALUES
                                       ('{beneficio.cod_beneficio.item}',
                                       {beneficio.consec},
                                       '{beneficio.estado.item}',
                                       '{beneficio.estadoObservaciones}',
                                       GETDATE(),
                                       '{beneficio.registra_user}')";

                        connection.Execute(query);

                        _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                        {
                            EmpresaId = CodCliente,
                            cod_beneficio = beneficio.cod_beneficio.item,
                            consec = beneficio.consec,
                            movimiento = "Inserta",
                            detalle = $@"Inserta Estado {estado}",
                            registro_usuario = beneficio.registra_user
                        });

                    }

                    query = $@"update afi_bene_otorga set 
                                            estado= '{beneficio.estado.item}' 
                                        where id_beneficio = {beneficio.id_beneficio} ";
                    connection.Execute(query);

                    estado = $@"SELECT COD_ESTADO
                                      FROM [dbo].[AFI_BENE_ESTADOS]
                                      WHERE COD_ESTADO = '{beneficio.estado.item}' AND P_FINALIZA = 1";
                    string finaliza = connection.Query<string>(estado).FirstOrDefault();


                    if (finaliza != null)
                    {
                        query = $@"update afi_bene_otorga set autoriza_fecha = getDate(), autoriza_user = '{beneficio.modifica_usuario}' where id_beneficio = {beneficio.id_beneficio} ";
                        connection.Execute(query);
                    }

                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = "InsertarActualizarEstados - " + ex.Message;
            }
            return info;
        }

        /// <summary>
        /// Actualiza o inserta en Afi_Bene_Registra_Motivos del benficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="beneficio"></param>
        /// <returns></returns>
        private ErrorDto InsertarActualizarMotivos(int CodCliente, BeneficioGeneralDatos beneficio)
        {
            //BeneficioGeneral beneficio = JsonConvert.DeserializeObject<BeneficioGeneral>(beneficioGeneral);

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {

                using var connection = new SqlConnection(clienteConnString);
                {
                    if (ExisteRegitro(CodCliente, beneficio.consec, beneficio.cod_beneficio.item, "MOT"))
                    {
                        //Motivo Anterior
                        var query = $@"SELECT COD_MOTIVO FROM AFI_BENE_REGISTRO_MOTIVOS WHERE CONSEC = {beneficio.consec} AND COD_BENEFICIO = '{beneficio.cod_beneficio.item}' ";
                        var cod_motivo = connection.Query<string>(query).FirstOrDefault();


                        query = @$"UPDATE [dbo].[AFI_BENE_REGISTRO_MOTIVOS]
                               SET
                                    [COD_MOTIVO]  =  '{beneficio.cod_motivo.item}'
                                   ,[REGISTRO_FECHA] = GETDATE()
                                   ,[REGISTRO_USUARIO] ='{beneficio.registra_user}'
                             WHERE CONSEC = {beneficio.consec} AND [COD_BENEFICIO] = '{beneficio.cod_beneficio.item}' ";

                        connection.Execute(query);

                        if (cod_motivo != beneficio.cod_motivo.item)
                        {
                            _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                            {
                                EmpresaId = CodCliente,
                                cod_beneficio = beneficio.cod_beneficio.item,
                                consec = beneficio.consec,
                                movimiento = "Actualiza",
                                detalle = $@"Actualiza Motivo de {cod_motivo}a {beneficio.cod_motivo.item}",
                                registro_usuario = beneficio.registra_user
                            });
                        }
                    }
                    else
                    {
                        var query = @$"INSERT INTO[dbo].[AFI_BENE_REGISTRO_MOTIVOS]
                                        ([COD_BENEFICIO]
                                       , [CONSEC]
                                       , [COD_MOTIVO]
                                       , [REGISTRO_FECHA]
                                       , [REGISTRO_USUARIO])
                                VALUES
                                       ('{beneficio.cod_beneficio.item}',
                                       {beneficio.consec},
                                       '{beneficio.cod_motivo.item}',
                                       GETDATE(),
                                       '{beneficio.registra_user}')";

                        connection.Execute(query);

                        _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                        {
                            EmpresaId = CodCliente,
                            cod_beneficio = beneficio.cod_beneficio.item,
                            consec = beneficio.consec,
                            movimiento = "Inserta",
                            detalle = $@"Inserta Motivo {beneficio.cod_motivo.descripcion}",
                            registro_usuario = beneficio.registra_user
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = "InsertarActualizarMotivos - " + ex.Message;
            }
            return info;
        }

        /// <summary>
        /// Busca si existe un registro en la tabla de beneficios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="consec"></param>
        /// <param name="cod_beneficio"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        private bool ExisteRegitro(int CodCliente, int? consec, string cod_beneficio, string tipo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            bool resp = false;
            try
            {

                // NOTA: tipo = MOT (MONETARIO), MON (MONTO), E (ESTADO) de AFI_BENE_REGISTRO_*

                using (IDbConnection db = new SqlConnection(clienteConnString))
                {
                    var query = $@"exec spAFI_Bene_ExisteRegistros {consec}, '{cod_beneficio}', '{tipo}' ";
                    var lista = db.Query<int>(query).FirstOrDefault();
                    if (lista > 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                resp = false;
            }
            return resp;
        }

        /// <summary>
        /// Valida si el socio ya se encuentra registrado en el programa Crece
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto ValidaProgramaCrece(int CodCliente, string cedula)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                using (IDbConnection db = new SqlConnection(clienteConnString))
                {
                    var query = $@"SELECT COUNT(*) FROM AFI_BENE_OTORGA
                                    WHERE CEDULA = '{cedula}' AND COD_BENEFICIO IN (
                                    SELECT B.COD_BENEFICIO FROM AFI_BENEFICIOS B
                                    WHERE COD_CATEGORIA = 'B_CRECE')
		                            AND DATEDIFF(MONTH,REGISTRA_FECHA, GETDATE()) <
                                    ( SELECT MAX(A.VIGENCIA_MESES) FROM AFI_BENEFICIOS A 
		                            WHERE COD_CATEGORIA = 'B_CRECE' )  ";
                    var lista = db.Query<int>(query).FirstOrDefault();
                    if (lista > 0)
                    {
                        info.Code = -1;
                        info.Description = "El socio ya se encuentra registrado en el programa Crece, ¿Desea registrarlo nuevamente?";
                    }
                }
            }
            catch (Exception)
            {
                info.Code = -1;
                info.Description = "Error al validar el programa";
            }
            return info;
        }

        /// <summary>
        /// Valida si el estado seleccionado es de resolución del expediente
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="estado"></param>
        /// <param name="categoria"></param>
        /// <returns></returns>
        public ErrorDto ValidaEstadoExpediente(int CodCliente, string estado, string categoria)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                using (IDbConnection db = new SqlConnection(clienteConnString))
                {
                    var query = $@"SELECT COUNT(*)
                                      FROM [AFI_BENE_ESTADOS]
                                      WHERE COD_ESTADO IN (SELECT COD_ESTADO
                                      FROM [AFI_BENE_GRUPO_ESTADOS]
                                      WHERE COD_GRUPO IN (
	                                      SELECT COD_GRUPO
	                                      FROM [AFI_BENE_GRUPOS]
	                                      WHERE COD_CATEGORIA = '{categoria}'
                                      ) )
                                      AND P_FINALIZA = 1
                                      AND COD_ESTADO = '{estado}' AND PROCESO IN ('A', 'D')";
                    var lista = db.Query<int>(query).FirstOrDefault();

                    if (lista == 0)
                    {
                        info.Code = -1;
                        info.Description = "El estado seleccionado no es de resolución del expediente";
                    }

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        /// <summary>
        /// Obtiene la lista de profesionales
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<List<BeneApreLista>> AfiBeneProfesionales_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<BeneApreLista>>();

            try
            {
                using (IDbConnection db = new SqlConnection(clienteConnString))
                {
                    var query = $@"SELECT  [ID_PROFESIONAL] as item
                                      ,CONCAT(IDENTIFICACION, ' ',[NOMBRE]) descripcion
                                  FROM AFI_BENE_APT_PROFESIONALES WHERE ACTIVO = 1 ";
                    response.Result = db.Query<BeneApreLista>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Obtiene la lista de categorias
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<List<BeneApreLista>> AfiBeneCategorias_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<BeneApreLista>>();

            try
            {
                using (IDbConnection db = new SqlConnection(clienteConnString))
                {
                    var query = $@"SELECT [ID_APT_CATEGORIA] as item
                                          ,[DESCRIPCION]
                                      FROM AFI_BENE_APT_CATEGORIAS WHERE ACTIVO = 1 ";
                    response.Result = db.Query<BeneApreLista>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Valida si el beneficio requiere justificacion
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cedula"></param>
        /// <param name="beneficio"></param>
        /// <returns></returns>
        public ErrorDto ValidaRequiereJustificacion(int CodCliente, string cedula, string beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto();
            response.Description = "";
            try
            {
                using (var connection = new SqlConnection(clienteConnString))
                {
                    var query = @$"SELECT * FROM AFI_BENE_VALIDACIONES abv WHERE COD_VAL IN (
                                    select COD_VAL FROM AFI_BENE_VALIDA_CATEGORIA
                                    WHERE COD_CATEGORIA = 
                                    (
	                                    SELECT ab.COD_CATEGORIA FROM AFI_BENEFICIOS ab WHERE ab.COD_BENEFICIO = '{beneficio}'
                                    ) AND ESTADO = 1 AND REGISTRO_JUSTIFICA = 1 )";
                    var validaciones = connection.Query<afiBeneCalidaciones>(query).ToList();

                    foreach (var validacion in validaciones)
                    {
                        query = validacion.query_val.Replace("@cedula", cedula).Replace("@cod_beneficio", beneficio);
                        var result = connection.Query<int>(query).FirstOrDefault();

                        if (result == validacion.resultado_val)
                        {
                            response.Code = -1;
                            response.Description += validacion.msj_val + "...\n";
                        }
                    }

                    ////Valida Membrecia Socio
                    //var Query = $@"select case when ESTADOACTUAL = 'S' 
                    //                then DATEDIFF(d, FECHAINGRESO , getDate() ) else 0 end 
                    //          from SOCIOS 
                    //          where CEDULA = '{cedula}' ";
                    //var dias = db.Query<int>(Query).FirstOrDefault();

                    //Query = $@"Select MAXIMO_OTORGA, MODIFICA_MONTO, MODIFICA_DIFERENCIA , VIGENCIA_MESES 
                    //          from AFI_BENEFICIOS 
                    //          where COD_BENEFICIO = '{beneficio}' ";
                    //var parametrosBene = db.Query<ValidaMetodoRequiere>(Query).FirstOrDefault();

                    //Query = $@"select isnull(MAX(Monto),0)
                    //           from AFI_BENEFICIO_MONTOS
                    //           Where COD_BENEFICIO = '{beneficio}' 
                    //             and {dias} between INICIO and CORTE ";

                    //var membresia = db.Query<int>(Query).FirstOrDefault();

                    //if (membresia == 0)
                    //{
                    //    response.Description = "El asociado no cumple con los requisitos de la membresía";
                    //}

                    //var result = db.ExecuteScalar<string>("SELECT dbo.fxBeneficio_Persona_Validacion(@Cedula)", new
                    //{
                    //    Cedula = cedula
                    //});

                    //response.Description += result;

                    //if (response.Description != "")
                    //{
                    //    response.Code = -1;
                    //}
                }


            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;

        }

        /// <summary>
        /// Valida el tipo de beneficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_beneficio"></param>
        /// <returns></returns>
        public ErrorDto<string> ValidaTipoBeneficio(int CodCliente, string? cod_beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<string>();

            try
            {
                if (cod_beneficio == null)
                {
                    response.Code = 0;
                    response.Description = "El código de beneficio no puede ser nulo";
                    response.Result = "M";
                }

                using (IDbConnection db = new SqlConnection(clienteConnString))
                {
                    var query = $@"SELECT TIPO FROM AFI_BENEFICIOS
                                     WHERE COD_BENEFICIO = '{cod_beneficio}'";
                    response.Result = db.Query<string>(query).FirstOrDefault();
                    response.Result = response.Result == null ? "M" : response.Result;
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = "M";
            }
            return response;
        }

        /// <summary>
        /// Genera la boleta de registro
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_categoria"></param>
        /// <param name="cedula"></param>
        /// <param name="consec"></param>
        /// <param name="titulo"></param>
        /// <param name="id_beneficio"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private async Task<IFormFile> BoletaRegistro(
            int CodCliente,
            string cod_categoria,
            string cedula,
            string consec, string titulo, int id_beneficio, string usuario)
        {

            string RepServer = _config.GetSection("ReporteSrv").GetSection("ReportServer").Value.ToString();
            // Definir la URL con parámetros (ajusta según sea necesario)
            string baseUrl = RepServer + "/frmAF_BeneficioIntegral/BoletaRegistro";
            string parametros = @"?CodEmpresa=61&cod_beneficio=" +
                cod_categoria + "&cedula=" + cedula + "&consec=" + consec + "&parametros={'Titulo':'" + titulo + "','Cedula':'" + cedula + "','Cod_Beneficio':'" + cod_categoria + "','Id_beneficio':" + id_beneficio + ",'Usuario':'" + usuario + "','Consec_Bene':" + id_beneficio + "}";
            string fullUrl = $"{baseUrl}{parametros}";

            // Crear cliente HTTP
            using (HttpClient client = new HttpClient())
            {
                IFormFile formFile;
                try
                {
                    HttpResponseMessage response = await client.GetAsync(fullUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        // Obtener la cadena Base64 del cuerpo de la respuesta
                        string base64String = await response.Content.ReadAsStringAsync();

                        // Decodificar la cadena Base64 a bytes
                        byte[] fileBytes = Convert.FromBase64String(base64String);

                        // Convertir los bytes en un Stream
                        MemoryStream stream = new MemoryStream(fileBytes);

                        // Crear el objeto IFormFile
                        formFile = new FormFile(stream, 0, fileBytes.Length, "file", "BoletaRegistro.pdf");

                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Excepción: {ex.Message}");
                    return null;
                }

                return formFile;
            }
        }

        /// <summary>
        /// Obtiene el registro de mora del beneficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="consec"></param>
        /// <param name="beneficio"></param>
        /// <returns></returns>
        public ErrorDto<BeneRegistroMoraDTO> BeneRegistroMora_Obtener(int CodCliente, int consec, string beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<BeneRegistroMoraDTO>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select ID_MORA, ACUERDO, ACUERDO_FECHA, 
                        CANCELACION_MORA, MES_CANCELACION, 
                        ADELANTO_CUOTA, MES_ADELANTO, 
                        CANCELACION_TOTAL_OPERACION, NUMERO_OPERACION
                        from AFI_BENE_REGISTRO_MORA 
	                    where CONSEC = {consec} and COD_BENEFICIO = '{beneficio}'";

                    response.Result = connection.Query<BeneRegistroMoraDTO>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Guarda el registro de mora del beneficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cobroMora"></param>
        /// <returns></returns>
        public ErrorDto BeneRegistroMora_Guardar(int CodCliente, BeneRegistroMoraGuardar cobroMora)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT COUNT(*) FROM AFI_BENE_REGISTRO_MORA
                    WHERE COD_BENEFICIO = '{cobroMora.cod_beneficio}' AND CONSEC = {cobroMora.consec} AND CEDULA = '{cobroMora.cedula}'";
                    int existe = connection.Query<int>(query).FirstOrDefault(); ;

                    if (existe > 0)
                    {
                        var updateQuery = $@"UPDATE AFI_BENE_REGISTRO_MORA
                        SET  
                            ACUERDO = '{cobroMora.acuerdo}',  
                            ACUERDO_FECHA = '{cobroMora.acuerdo_fecha}' ";

                        updateQuery += ", CANCELACION_MORA = " + (cobroMora.cancelacion_mora.HasValue ? cobroMora.cancelacion_mora.Value.ToString("F2") : "NULL") 
                            + ", MES_CANCELACION = ";
                        updateQuery += string.IsNullOrEmpty(cobroMora.mes_cancelacion) ? "NULL" : $"'{cobroMora.mes_cancelacion}'";

                        updateQuery += ", ADELANTO_CUOTA = " + (cobroMora.adelanto_cuota.HasValue ? cobroMora.adelanto_cuota.Value.ToString("F2") : "NULL")
                            + ", MES_ADELANTO = ";
                        updateQuery += string.IsNullOrEmpty(cobroMora.mes_adelanto) ? "NULL" : $"'{cobroMora.mes_adelanto}'";

                        updateQuery += ", CANCELACION_TOTAL_OPERACION = " + (cobroMora.cancelacion_total_operacion.HasValue ? cobroMora.cancelacion_total_operacion.Value.ToString("F2") : "NULL")
                            + ", NUMERO_OPERACION = ";
                        updateQuery += string.IsNullOrEmpty(cobroMora.numero_operacion) ? "NULL" : $"'{cobroMora.numero_operacion}'";

                        updateQuery += $@" WHERE COD_BENEFICIO = '{cobroMora.cod_beneficio}' AND CONSEC = {cobroMora.consec} AND CEDULA = '{cobroMora.cedula}'";

                        response.Code = connection.Execute(updateQuery);

                        _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                        {
                            EmpresaId = CodCliente,
                            cod_beneficio = cobroMora.cod_beneficio,
                            consec = cobroMora.consec,
                            movimiento = "Inserta",
                            detalle = $@"Inserta Cobro de Mora del asociado {cobroMora.cedula}",
                            registro_usuario = cobroMora.registro_usuario
                        });
                    }
                    else
                    {
                        var insertQuery = $@"INSERT INTO AFI_BENE_REGISTRO_MORA (
                            COD_BENEFICIO,
                            CONSEC,
                            CEDULA,
                            ACUERDO,
                            ACUERDO_FECHA,
                            REGISTRO_FECHA,
                            REGISTRO_USUARIO,
                            CANCELACION_MORA,
                            MES_CANCELACION,
                            ADELANTO_CUOTA,
                            MES_ADELANTO,
                            CANCELACION_TOTAL_OPERACION,
                            NUMERO_OPERACION
                        ) 
                        VALUES (
                            '{cobroMora.cod_beneficio}', 
                            {cobroMora.consec},  
                            '{cobroMora.cedula}',
                            '{cobroMora.acuerdo}',  
                            '{cobroMora.acuerdo_fecha}',
                            GETDATE(), 
                            '{cobroMora.registro_usuario}',";
                        insertQuery += $"{(cobroMora.cancelacion_mora.HasValue ? cobroMora.cancelacion_mora.Value.ToString("F2") : "NULL")},";
                        insertQuery += string.IsNullOrEmpty(cobroMora.mes_cancelacion) ? "NULL," : $"'{cobroMora.mes_cancelacion}',";
                        insertQuery += $"{(cobroMora.adelanto_cuota.HasValue ? cobroMora.adelanto_cuota.Value.ToString("F2") : "NULL")},";
                        insertQuery += string.IsNullOrEmpty(cobroMora.mes_adelanto) ? "NULL," : $"'{cobroMora.mes_adelanto}',";
                        insertQuery += $"{(cobroMora.cancelacion_total_operacion.HasValue ? cobroMora.cancelacion_total_operacion.Value.ToString("F2") : "NULL")},";
                        insertQuery += string.IsNullOrEmpty(cobroMora.numero_operacion) ? "NULL" : $"'{cobroMora.numero_operacion}'";
                        insertQuery += ")";

                        response.Code = connection.Execute(insertQuery);

                        _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                        {
                            EmpresaId = CodCliente,
                            cod_beneficio = cobroMora.cod_beneficio,
                            consec = cobroMora.consec,
                            movimiento = "Actualiza",
                            detalle = $@"Actualiza Cobro de Mora del asociado {cobroMora.cedula}",
                            registro_usuario = cobroMora.registro_usuario
                        });
                    }
                    response.Description = "Registro para cobro de mora guardado correctamente";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Envía la boleta de cobro de mora al departamento de cobros
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public async Task<ErrorDto> BeneRegistroMora_Enviar(int CodCliente, DocArchivoBeneIntegralDTO parametros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            EnvioCorreoModels eConfig = new();
            string emailCobros = "";

            if(parametros == null)
            {
                info.Code = -1;
                info.Description = "Los parámetros no pueden ser nulos";
                return info;
            }

            try
            {

                using var connection = new SqlConnection(clienteConnString);
                {
                    var queryCodBene = @$"SELECT C.COD_SMTP FROM AFI_BENE_CATEGORIAS C
                                            WHERE C.COD_CATEGORIA IN (
                                            SELECT B.COD_CATEGORIA FROM AFI_BENEFICIOS B
                                            WHERE B.COD_BENEFICIO IN (
                                            SELECT DISTINCT H.COD_BENEFICIO FROM AFI_BENE_OTORGA H
                                            WHERE H.ID_BENEFICIO = {parametros.id_beneficio}
                                            )
                                            )";
                    string codCategoria = connection.Query<string>(queryCodBene).FirstOrDefault();

                    eConfig = _envioCorreoDB.CorreoConfig(CodCliente, codCategoria);

                    var queryEmailCobros = @$"select VALOR from SIF_PARAMETROS where COD_PARAMETRO = '{nofiticacionCobros}'";
                    emailCobros = connection.Query<string>(queryEmailCobros).FirstOrDefault();
                }


                string expediente = parametros.id_beneficio.ToString().PadLeft(5, '0') + parametros.cod_beneficio + parametros.consec.ToString().PadLeft(5, '0');

                if (parametros.body.Trim() == "")
                {
                    parametros.body = "Estimados compa�eros del Departamento de Cobros, se adjunta boleta para la aplicaci�n del cobro al asociado con la c�dula: " + parametros.cedula + ".";
                }

                string body = @$"<html lang=""es"">
                                    <head>
                                        <meta charset=""UTF-8"">
                                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                                        <title>Boleta para aplicaci�n del beneficio al Departamento de Cobros</title>
                                    </head>
                                    <body>
                                        <p>{parametros.body}</p>
                                        <p>ASECCSS</p>
                                    </body>
                                    </html>";

                List<IFormFile> Attachments = new List<IFormFile>();
                if(parametros.filecontent != null)
                {
                    //Convierto de base64 a byte[]
                    byte[] fileContent = Convert.FromBase64String(parametros.filecontent);

                    var file = ConvertByteArrayToIFormFileList(fileContent, parametros.filename);

                    Attachments.AddRange(file);
                }
                

                if (sendEmail == "Y")
                {
                    EmailRequest emailRequest = new EmailRequest();

                    emailRequest.To = emailCobros;
                    emailRequest.From = eConfig.User;
                    emailRequest.Subject = "Aplicación de beneficio - expediente " + expediente;
                    emailRequest.Body = body;
                    emailRequest.Attachments = Attachments;

                    if (eConfig != null)
                    {
                        await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, info);
                    }

                }

                _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                {
                    EmpresaId = CodCliente,
                    cod_beneficio = parametros.cod_beneficio,
                    consec = parametros.consec,
                    movimiento = "Notifica",
                    detalle = $@"Notificación Cobro de Mora enviada a {emailCobros}",
                    registro_usuario = parametros.usuario
                });

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        public ErrorDto ValidaFallecido(int CodCliente, string cedulafallecido)
        {
            return _mBeneficiosDB.ValidaFallecido(CodCliente, cedulafallecido);
        }



    }
}