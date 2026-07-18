using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosIntegralDB
    {
        /// <summary>Datos internos de la fecha de corte de pago automático.</summary>
        private sealed class FechaCortePago
        {
            public DateTime? fecha_corte { get; set; } = new DateTime();
            public int? monto { get; set; } = 0;
        }

        private const string SqlDatosBeneficiarioDeposito = @"
            SELECT B.MONTO, B.COD_BENEFICIO, B.TIPO, B.CONSEC, B.REGISTRA_USER AS registro_usuario,
                   B.NOMBRE_BENEFICIARIO AS t_beneficiario, S.AF_EMAIL AS t_email, A.ID_BANCO AS COD_BANCO,
                   C.CUENTA_INTERNA AS cta_bancaria, B.cedula AS t_identificacion
            FROM vBeneficios_W_Integral B
            LEFT JOIN SOCIOS S ON B.CEDULA = S.CEDULA
            LEFT JOIN SYS_CUENTAS_BANCARIAS C ON B.CEDULA = C.IDENTIFICACION
            LEFT JOIN BANCOS A ON A.COD_GRUPO = C.COD_BANCO
            WHERE B.cedula = @cedula AND B.ID_BENEFICIO = @idBeneficio";

        private const string SqlFechaCorteDeposito = @"
            SELECT TOP 1 fecha_corte, monto FROM AFI_BENE_FECHA_PAGO_AUTOMATICO
            WHERE COD_BENEFICIO = @codBeneficio AND PERIODO = YEAR(GETDATE()) AND ACTIVO = 1 AND mes = @mes
            ORDER BY FECHA_CORTE ASC";

        /// <summary>
        /// Aprueba de forma masiva los beneficios seleccionados y deja traza en bitácora.
        /// </summary>
        public ErrorDto BeneIntegral_AprobacionMasiva(int CodEmpresa, string lista)
        {
            var beneficios = JsonConvert.DeserializeObject<List<BeneficioGuadar>>(lista) ?? new List<BeneficioGuadar>();

            const string sqlOtorga = @"UPDATE afi_bene_otorga SET autoriza_fecha = GETDATE(), autoriza_user = @usuario, estado = 'A'
                                        WHERE id_beneficio = @idBeneficio";
            const string sqlEstado = @"UPDATE [dbo].[AFI_BENE_REGISTRO_ESTADOS]
                                          SET [COD_ESTADO] = 'A', [NOTAS] = @notas, [REGISTRO_FECHA] = GETDATE(), [REGISTRO_USUARIO] = @usuario
                                        WHERE CONSEC = @consec AND [COD_BENEFICIO] = @codBeneficio";

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                foreach (var b in beneficios)
                {
                    connection.Execute(sqlOtorga, new { usuario = b.usuario, idBeneficio = b.id_beneficio });
                    connection.Execute(sqlEstado, new { notas = b.estadoObservaciones, usuario = b.usuario, consec = b.consec, codBeneficio = b.cod_beneficio });
                    RegistrarBitacora(CodEmpresa, b.cod_beneficio, b.consec, "Actualizar",
                        $"Actualiza Estado del Beneficio [{b.id_beneficio}] - Nota: [{b.estadoObservaciones}]", b.usuario);
                }
                return true;
            });

            return result.Code == 0
                ? new ErrorDto { Code = 0, Description = "Beneficios Aprobados Correctamente!" }
                : new ErrorDto { Code = -1, Description = result.Description };
        }

        /// <summary>
        /// Genera las solicitudes de depósito (proyecciones de pago) para los beneficios seleccionados.
        /// </summary>
        public ErrorDto BeneSolicitudDeposito_Generar(int CodEmpresa, string lista, int mes)
        {
            var beneficios = JsonConvert.DeserializeObject<List<BeneficioGuadar>>(lista) ?? new List<BeneficioGuadar>();
            var info = new ErrorDto { Code = 0 };

            try
            {
                using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);

                foreach (var b in beneficios)
                {
                    var error = ValidarBeneficiarioDeposito(connection, b, mes);
                    if (error != null)
                    {
                        return error;
                    }
                }

                foreach (var b in beneficios)
                {
                    GenerarProyeccionDeposito(CodEmpresa, connection, b, mes);
                }

                info.Description = "Solicitudes de Deposito Generadas Correctamente";
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }

            return info;
        }

        /// <summary>
        /// Valida que el beneficiario tenga cuenta y fecha de pago activa para el mes.
        /// </summary>
        private ErrorDto? ValidarBeneficiarioDeposito(SqlConnection connection, BeneficioGuadar b, int mes)
        {
            var datos = connection.QueryFirstOrDefault<AfiBenePagoProyecta>(
                SqlDatosBeneficiarioDeposito, new { cedula = b.cedula, idBeneficio = b.id_beneficio });
            if (datos == null)
            {
                return new ErrorDto { Code = -1, Description = "Beneficiario con la cédula " + b.cedula + " no tiene una cuenta asociada, por favor verifique" };
            }

            var fechaCorte = connection.QueryFirstOrDefault<FechaCortePago>(
                SqlFechaCorteDeposito, new { codBeneficio = b.cod_beneficio, mes });
            if (fechaCorte == null || fechaCorte.monto == null)
            {
                return new ErrorDto { Code = -1, Description = "Beneficio " + b.cod_beneficio + " no tiene fecha de pago activa para el mes indicado, por favor verifique" };
            }

            return null;
        }

        /// <summary>
        /// Inserta la proyección de pago del depósito y actualiza notas/bitácora cuando la cuenta está activa.
        /// </summary>
        private void GenerarProyeccionDeposito(int CodEmpresa, SqlConnection connection, BeneficioGuadar b, int mes)
        {
            var cuentaActiva = connection.QueryFirstOrDefault<int>(
                "SELECT ACTIVA FROM SYS_CUENTAS_BANCARIAS WHERE IDENTIFICACION = @cedula", new { cedula = b.cedula });
            if (cuentaActiva != 1)
            {
                return;
            }

            var datos = connection.QueryFirstOrDefault<AfiBenePagoProyecta>(
                SqlDatosBeneficiarioDeposito, new { cedula = b.cedula, idBeneficio = b.id_beneficio });
            var fechaCorte = connection.QueryFirstOrDefault<FechaCortePago>(
                SqlFechaCorteDeposito, new { codBeneficio = b.cod_beneficio, mes });
            if (datos == null || fechaCorte == null)
            {
                return;
            }

            connection.Execute(@"
                INSERT AFI_BENE_PAGO_PROYECTA(cedula,consec,cod_beneficio,tipo,fecha_vence,monto,cod_banco,tipo_emision,
                                              cta_bancaria,estado,t_identificacion,t_beneficiario,t_email,registro_fecha,registro_usuario)
                VALUES(@cedula,@consec,@codBeneficio,@tipo,@fechaVence,@monto,@codBanco,'TE',@ctaBancaria,'P',
                       @tIdentificacion,@tBeneficiario,@tEmail,GETDATE(),@usuario)",
                new
                {
                    cedula = b.cedula.Trim(),
                    consec = b.consec,
                    codBeneficio = (b.cod_beneficio ?? string.Empty).Trim(),
                    tipo = datos.tipo,
                    fechaVence = fechaCorte.fecha_corte,
                    monto = fechaCorte.monto,
                    codBanco = datos.cod_banco,
                    ctaBancaria = datos.cta_bancaria,
                    tIdentificacion = (datos.t_identificacion ?? string.Empty).Trim(),
                    tBeneficiario = datos.t_beneficiario,
                    tEmail = datos.t_email,
                    usuario = b.usuario
                });

            var notas = connection.QueryFirstOrDefault<string>(
                "SELECT [NOTAS] FROM [dbo].[AFI_BENE_REGISTRO_ESTADOS] WHERE CONSEC = @consec AND [COD_BENEFICIO] = @codBeneficio",
                new { consec = b.consec, codBeneficio = b.cod_beneficio });
            notas = (notas ?? string.Empty) + ", " + b.notas;
            connection.Execute(
                "UPDATE [dbo].[AFI_BENE_REGISTRO_ESTADOS] SET [NOTAS] = @notas WHERE CONSEC = @consec AND [COD_BENEFICIO] = @codBeneficio",
                new { notas, consec = b.consec, codBeneficio = b.cod_beneficio });

            RegistrarBitacora(CodEmpresa, b.cod_beneficio!, b.consec, "Inserta",
                $"Autoriza Solicitud de Deposito - Nota: [{b.notas}]", b.usuario);
        }

        /// <summary>
        /// Devuelve las solicitudes de depósito (estado 'DR') y deja traza en bitácora.
        /// </summary>
        public ErrorDto BeneSolicitudDeposito_Devolver(int CodEmpresa, string lista)
        {
            var beneficios = JsonConvert.DeserializeObject<List<BeneficioGuadar>>(lista) ?? new List<BeneficioGuadar>();

            const string sqlOtorga = "UPDATE afi_bene_otorga SET estado = 'DR' WHERE id_beneficio = @idBeneficio";
            const string sqlNotas = "SELECT [NOTAS] FROM [dbo].[AFI_BENE_REGISTRO_ESTADOS] WHERE CONSEC = @consec AND COD_BENEFICIO = @codBeneficio";
            const string sqlEstado = @"UPDATE [dbo].[AFI_BENE_REGISTRO_ESTADOS]
                                          SET [COD_ESTADO] = 'DR', [NOTAS] = @notas, [REGISTRO_FECHA] = GETDATE(), [REGISTRO_USUARIO] = @usuario
                                        WHERE CONSEC = @consec AND COD_BENEFICIO = @codBeneficio";

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                foreach (var b in beneficios)
                {
                    connection.Execute(sqlOtorga, new { idBeneficio = b.id_beneficio });

                    var notas = connection.QueryFirstOrDefault<string>(sqlNotas, new { consec = b.consec, codBeneficio = b.cod_beneficio });
                    notas = (notas ?? string.Empty) + ", " + b.notas;

                    connection.Execute(sqlEstado, new { notas, usuario = b.usuario, consec = b.consec, codBeneficio = b.cod_beneficio });

                    RegistrarBitacora(CodEmpresa, b.cod_beneficio, b.consec, "Actualiza",
                        $"Devolución Solicitud Pago - Nota: [{b.notas}]", b.usuario);
                }
                return true;
            });

            return new ErrorDto { Code = result.Code, Description = result.Description };
        }

        /// <summary>
        /// Envía la solicitud de bloqueo del asociado al Departamento de Cobros y deja traza en bitácora.
        /// </summary>
        public async Task<ErrorDto> BeneSolicitudBloqueo_Enviar(int CodCliente, DocArchivoBeneIntegralDto parametros)
        {
            var info = new ErrorDto { Code = 0 };

            try
            {
                EnvioCorreoModels? eConfig;
                string? emailCobros;

                using (var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente))
                {
                    var codCategoria = await connection.QueryFirstOrDefaultAsync<string>(
                        "SELECT C.COD_SMTP FROM AFI_BENE_CATEGORIAS C WHERE C.COD_CATEGORIA = @codCategoria",
                        new { codCategoria = parametros.cod_beneficio });

                    var eConfigResult = _envioCorreoDB.CorreoConfig(CodCliente, codCategoria ?? string.Empty);
                    eConfig = (eConfigResult != null && eConfigResult.Code == 0) ? eConfigResult.Result : null;

                    emailCobros = await connection.QueryFirstOrDefaultAsync<string>(
                        "SELECT VALOR FROM SIF_PARAMETROS WHERE COD_PARAMETRO = @codParametro",
                        new { codParametro = _notificacionCobros });
                }

                var body = $@"<html lang=""es""><head><meta charset=""UTF-8"">
                              <title>Boleta para aplicación del beneficio al Departamento de Cobros</title></head>
                              <body><p>{parametros.body}</p><p>ASECCSS</p></body></html>";

                var attachments = new List<IFormFile>();
                if (parametros.filecontent != null)
                {
                    var fileContent = Convert.FromBase64String(parametros.filecontent);
                    attachments.AddRange(ConvertByteArrayToIFormFileList(fileContent, parametros.filename ?? "archivo"));
                }

                if (_sendEmail == "Y" && eConfig != null)
                {
                    var emailRequest = new EmailRequest
                    {
                        To = emailCobros ?? string.Empty,
                        From = eConfig.User,
                        Subject = "Solicitud de Bloqueo de Asociado",
                        Body = body,
                        Attachments = attachments
                    };

                    await _envioCorreoDB.SendEmailAsync(emailRequest, eConfig, info);
                }

                RegistrarBitacora(CodCliente, parametros.cod_beneficio ?? string.Empty, parametros.consec, "Notifica",
                    $"Notificación Solicitud de Bloqueo Asociado {emailCobros}", parametros.usuario ?? string.Empty);
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }

            return info;
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

            // The stream must remain open for the returned FormFile to be readable.
            // Its lifecycle is tied to the IFormFile consumer, which is responsible for disposal.
            var fileContentStream = new MemoryStream(byteArray);
            var formFile = new FormFile(fileContentStream, 0, byteArray.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/octet-stream"
            };

            formFiles.Add(formFile);
            return formFiles;
        }
    }
}
