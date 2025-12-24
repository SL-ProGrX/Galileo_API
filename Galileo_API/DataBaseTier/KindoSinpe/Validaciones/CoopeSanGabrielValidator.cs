using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;
using Galileo.Models.Security;
using Sinpe_PIN;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Galileo_API.DataBaseTier
{
#pragma warning disable S125 // Quitar despues de implementar resto de metodos con CSG
#pragma warning disable S2589 // Quitar despues de implementar resto de metodos con CSG
    public class CoopeSanGabrielValidator
    {

        private readonly InfoSinpeRequest _infoSinpe = new InfoSinpeRequest();
        private readonly SinpeGalileoPin _sinpePIN;

        private readonly MKindoServiceDb _mKindo;
        private readonly MTesoreria _mTesoreria;


        public CoopeSanGabrielValidator(IConfiguration config)
        {
            _mKindo = new MKindoServiceDb(config);
            _sinpePIN = new SinpeGalileoPin(config);
            _mTesoreria = new MTesoreria(config);
        }

        #region Validación de Solicitud SINPE
        public ErrorDto fxValidacionSinpe(int CodEmpresa, string solicitud, string usuario)
        {
            var response = OkResponse();

            var parametrosSinpe = _mKindo.GetUriEmpresa(CodEmpresa, usuario);

            try
            {
                var infoSinpeResult = CargarInfoSinpe(CodEmpresa, solicitud);
                if (infoSinpeResult.Code == -1)
                    return ErrorResponse(infoSinpeResult.Description);

                _infoSinpe.vInfo = infoSinpeResult.Result;

                if (!TieneDatosMinimos(_infoSinpe.vInfo))
                    return response; // mismo comportamiento que tu código: si no hay datos, regresa Ok

                if (!MKindoServiceDb.IsValidCostaRicaIBAN(_infoSinpe.vInfo.CuentaIBAN))
                    return ErrorResponse("Cuenta IBAN no Valida");

                var context = CrearContexto(parametrosSinpe);

                var servicio = _sinpePIN.IsServiceAvailable(parametrosSinpe.Result.UrlCGP_PIN, context);
                if (!servicio.IsSuccessful)
                    return ErrorResponse(servicio.Errors?[0]?.Message ?? "Servicio no disponible");

                var cuenta = ConsultarCuenta(parametrosSinpe, context, _infoSinpe.vInfo.CuentaIBAN);
                if (!cuenta.IsSuccessful)
                {
                    // tu código no hace nada aquí (comentado), así que mantenemos el comportamiento.
                    return response;
                }

                // aquí irían validaciones futuras (divisa, cédula, etc.)
                return response;
            }
            catch (Exception ex)
            {
                return ErrorResponse("Ocurrió un problema con la validación. - " + ex.Message);
            }
        }

        private static ErrorDto OkResponse() =>
    new ErrorDto { Code = 0, Description = "Ok" };

        private static ErrorDto ErrorResponse(string description) =>
            new ErrorDto { Code = -1, Description = description };

        private ErrorDto<vInfoSinpe> CargarInfoSinpe(int CodEmpresa, string solicitud)
        {
            _infoSinpe.vInfo = new vInfoSinpe();

            var cntInfoSinpe = _mKindo.fxTesConsultaInfoSinpe(CodEmpresa, solicitud);
            return cntInfoSinpe;
        }

        private static bool TieneDatosMinimos(vInfoSinpe info) =>
            !string.IsNullOrEmpty(info?.Cedula) &&
            !string.IsNullOrEmpty(info?.CuentaIBAN);

        private static ReqBase CrearContexto(ErrorDto<ParametrosSinpe> parametrosSinpe) =>
            new ReqBase
            {
                HostId = parametrosSinpe.Result.vHostPin,
                OperationId = Guid.NewGuid().ToString(), // o usa tu OperationId de campo si es requerido por negocio
                ClientIPAddress = parametrosSinpe.Result.vIpHost,
                CultureCode = "ES-CR",
                UserCode = parametrosSinpe.Result.vUsuarioLog,
            };

        private Galileo.Models.KindoSinpe.ResAccountInfo ConsultarCuenta(
            ErrorDto<ParametrosSinpe> parametrosSinpe,
            ReqBase context,
            string cuentaIban)
        {
            var accountData = new Galileo.Models.KindoSinpe.ReqAccountInfo
            {
                HostId = context.HostId,
                OperationId = context.OperationId,
                ClientIPAddress = context.ClientIPAddress,
                CultureCode = context.CultureCode,
                UserCode = context.UserCode,
                Id = null,
                AccountNumber = cuentaIban
            };

            return _sinpePIN.GetAccountInfo(parametrosSinpe.Result.UrlCGP_PIN, accountData);
        }


        /// <summary>
        /// Realiza el proceso de emisión de una transferencia SINPE Crédito Directo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Nsolicitud"></param>
        /// <param name="vfecha"></param>
        /// <param name="vUsuario"></param>
        /// <param name="doc_base"></param>
        /// <param name="contador"></param>
        /// <returns></returns>
        public ErrorDto fxTesEmisionSinpeCreditoDirecto(int CodEmpresa,
            int Nsolicitud, DateTime vfecha, string vUsuario, int doc_base, int contador)
        {
            //(Realiza el proceso de envio y recibido de SINPE)
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            var respuesta = new RespuestaRegistro();
            var datos = new TesTransaccion();
            bool estadoSinpe = true;
            int idRechazo = 0;
            string rechazo = "";
            try
            {
                if (Nsolicitud > 0)
                {
                    var servicioDisponible = fxValidacionSinpe(CodEmpresa, Nsolicitud.ToString(), vUsuario);
                    if (servicioDisponible.Code == -1)
                    {
                        estadoSinpe = false;
                        idRechazo = 83;
                        rechazo = _mKindo.fxTesConsultaMotivo(CodEmpresa, idRechazo).Result;
                    }
                    else
                    {
                        respuesta = fxTesEnvioSinpeCreditoDirecto(CodEmpresa, Nsolicitud, vUsuario).Result;

                        if (respuesta.MotivoError != 0)
                        {
                            estadoSinpe = false;
                            idRechazo = 83;
                            rechazo = _mKindo.fxTesConsultaMotivo(CodEmpresa, idRechazo).Result;
                        }
                        else
                        {
                            estadoSinpe = true;
                        }

                    }

                    //Guardar la respuesta en la transacción
                    datos.NumeroSolicitud = Nsolicitud;
                    datos.FechaEmision = vfecha;
                    datos.FechaTraslado = vfecha;
                    datos.UsuarioGenera = vUsuario;
                    datos.estadoSinpe = estadoSinpe;
                    datos.IdMotivoRechazo = idRechazo;
                    datos.CodigoReferencia = respuesta.CodigoReferencia;
                    datos.DocumentoBase = doc_base.ToString();
                    datos.contador = contador.ToString();

                    if (!_mKindo.fxTesRespuestaSinpe(CodEmpresa, datos).Result)
                    {
                        _mTesoreria.sbTesBitacoraEspecial(CodEmpresa, Nsolicitud, "10", "Se produjo un error al actualizar la transacción", vUsuario);
                    }

                    if (estadoSinpe)
                    {
                        _mTesoreria.sbTesBitacoraEspecial(CodEmpresa, Nsolicitud, "10", "Emisión Transferencia Sinpe: Exitosa", vUsuario);
                    }
                    else
                    {
                        _mTesoreria.sbTesBitacoraEspecial(CodEmpresa, Nsolicitud, "10", $"Transferencia Sinpe rechazada: {rechazo} ", vUsuario);
                    }
                }
                else
                {
                    response.Code = -1;
                    response.Description = "No se ha indicado una solicitud válida.";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        private ErrorDto<RespuestaRegistro> fxTesEnvioSinpeCreditoDirecto(int CodEmpresa, int Nsolicitud, string vUsuario)
        {

            var resp = new ErrorDto<RespuestaRegistro>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };


            
            var pinData = new Galileo.Models.KindoSinpe.ReqPINSending();

            try
            {
                var parametrosSinpe = _mKindo.GetUriEmpresa(CodEmpresa, vUsuario);
                // var response = new ResPINSending();
                var solicitud = _mKindo.fxTesConsultaSolicitud(CodEmpresa, Nsolicitud).Result;

                var context = CrearContexto(parametrosSinpe);

                pinData.HostId = context.HostId;
                pinData.OperationId = context.OperationId;
                pinData.ClientIPAddress = context.ClientIPAddress;
                pinData.CultureCode = context.CultureCode;
                pinData.UserCode = context.UserCode;
                pinData.PINData = new Galileo.Models.KindoSinpe.PINTransfer()
                {
                    ChannelReference = solicitud.NDocumento.PadLeft(10, '0'),
                    Amount = solicitud.Monto,
                    TransactionDate = DateTime.Now,
                    Origin = new Galileo.Models.KindoSinpe.OriginCustomer()
                    {
                        Id = solicitud.CedulaOrigen.Replace("-", ""),
                        Name = solicitud.NombreOrigen,
                        IBAN1 = solicitud.CuentaOrigen,
                        CreditIBAN = false,
                        Email = solicitud.CorreoNotifica?.Trim()
                    },
                    Destination = new Galileo.Models.KindoSinpe.DestinationCustomer()
                    {
                        Id = solicitud.Codigo.Replace("-", ""),
                        Name = solicitud.Beneficiario,
                        IBAN = solicitud.Cuenta
                    },
                    CustomFields = new List<Galileo.Models.KindoSinpe.CustomField>()
                    {
                        new Galileo.Models.KindoSinpe.CustomField()
                        {
                            Name = "Email",
                            Value = solicitud.CorreoNotifica?.Trim()
                        },
                        new Galileo.Models.KindoSinpe.CustomField()
                        {
                            Name = "Servicio",
                            Value = "CCD"
                        }
                    }
                };

                var response = _sinpePIN.SendPIN(parametrosSinpe.Result.UrlCGP_PIN, pinData);
                if (response.IsSuccessful)
                {
                    var updateNSolicitud = _mKindo.RegistraDibitoCuenta(CodEmpresa, Nsolicitud, response).Result;

                    if (updateNSolicitud)
                    {
                        return new ErrorDto<RespuestaRegistro>
                        {
                            Code = 0,
                            Description = "Ok",
                            Result = new RespuestaRegistro
                            {
                                MotivoError = 0,
                                CodigoReferencia = response.PINSendingResult.SINPEReference
                            }
                        };
                    }
                    else
                    {
                        return new ErrorDto<RespuestaRegistro>
                        {
                            Code = -1,
                            Description = "Error al actualizar el número de solicitud con la respuesta de SINPE.",
                            Result = new RespuestaRegistro
                            {
                                MotivoError = -1,
                                CodigoReferencia = ""
                            }
                        };
                    } 
                }
                else
                {
                    return new ErrorDto<RespuestaRegistro>
                    {
                        Code = -1,
                        Description = "Error al enviar PIN",
                        Result = new RespuestaRegistro
                        {
                            MotivoError = response.Errors[0].Code,
                            CodigoReferencia = "",
                            MotivoErrorInterno = response.Errors[0].Message
                        }
                    };
                }
            }
            catch (Exception)
            {
                resp.Code = -1;
                resp.Description = "Error al enviar la solicitud de crédito directo a SINPE.";
                resp.Result = null;
            }

            return resp;
        }


        #endregion
    }
}
