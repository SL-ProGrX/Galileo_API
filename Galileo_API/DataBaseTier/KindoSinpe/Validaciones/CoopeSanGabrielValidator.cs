using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;
using Galileo.Models.KindoSinpe;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;
using Org.BouncyCastle.Ocsp;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Galileo_API.DataBaseTier
{
#pragma warning disable S125 // Quitar despues de implementar resto de metodos con CSG
#pragma warning disable S2589 // Quitar despues de implementar resto de metodos con CSG
    public class CoopeSanGabrielValidator
    {
        private readonly PortalDB _portalDB;
        private readonly InfoSinpeRequest _infoSinpe = new InfoSinpeRequest();
        private readonly SinpeGalileoPin _sinpePIN;

        private readonly MKindoServiceDb _mKindo;
        private readonly MTesoreria _mTesoreria;


        public CoopeSanGabrielValidator(IConfiguration config)
        {
            _mKindo = new MKindoServiceDb(config);
            _sinpePIN = new SinpeGalileoPin(config);
            _mTesoreria = new MTesoreria(config);
            _portalDB = new PortalDB(config);
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
                    return ErrorResponse(infoSinpeResult.Description!);

                _infoSinpe.vInfo = infoSinpeResult.Result!;

                if (!TieneDatosMinimos(_infoSinpe.vInfo!))
                    return response; // mismo comportamiento que tu código: si no hay datos, regresa Ok

                if (!MKindoServiceDb.IsValidCostaRicaIBAN(_infoSinpe.vInfo.CuentaIBAN!))
                    return ErrorResponse("Cuenta IBAN no Valida");

                var context = CrearContexto(parametrosSinpe);

                var servicio = _sinpePIN.IsServiceAvailable(parametrosSinpe?.Result?.UrlCGP_PIN!, context);
                if (!servicio.ServiceAvailable)
                    return ErrorResponse(servicio.Errors?[0]?.Message ?? "Servicio no disponible");

                var cuenta = ConsultarCuenta(parametrosSinpe!, context, _infoSinpe.vInfo.CuentaIBAN!);
                if (!cuenta.IsSuccessful)
                {
                    response.Code = cuenta.Errors[0].Code;
                    response.Description = cuenta.Errors[0].Message;
                    return response;
                }
                else
                {
                    if(cuenta.Account!.State == 0 || cuenta.Account!.State == 1)
                    {
                        // aquí irían validaciones futuras (divisa, cédula, etc.)
                        response.Description = $@"La cuenta IBAN {_infoSinpe.vInfo.CuentaIBAN} registrada a 
                                        nombre de {cuenta.Account!.Holder} cédula: {cuenta.Account.HolderId} Tipo Id: {_infoSinpe.vInfo.tipoID} 
                                        Tipo de Moneda: {cuenta.Account.CurrencyCode} Entidad: {cuenta.Account.EntityCode}-{cuenta.Account.EntityName}";
                        //guarda el mensaje de error de la emision:
                        fxGuardaID_RespuestaSinpe(CodEmpresa, (int)cuenta.Account.State!, solicitud);
                        return DbHelper.OkResponse(response.Description);
                    }
                    else
                    {
                        //guarda el mensaje de error de la emision:
                        fxGuardaID_RespuestaSinpe(CodEmpresa, (int)cuenta.Account.State!, solicitud);
                        var rechazo = _mKindo.fxTesConsultaMotivo(CodEmpresa, (int)cuenta.Account.State!).Result!;
                        return DbHelper.ErrorResponse(rechazo, (int)cuenta.Account.State!);
                    } 
                    
                }
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
                HostId = parametrosSinpe.Result!.vHostPin,
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
                Id = string.Empty,
                AccountNumber = cuentaIban
            };

            return _sinpePIN.GetAccountInfo(parametrosSinpe.Result?.UrlCGP_PIN!, accountData);
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
        public ErrorDto fxTesEmisionSinpeCreditoDirecto(
    int CodEmpresa,
    int Nsolicitud,
    DateTime vfecha,
    string vUsuario,
    int doc_base,
    int contador)
        {
            var response = new ErrorDto { Code = 0, Description = "Ok" };

            var respuesta = new RespuestaRegistro();
            var datos = new TesTransaccion();

            bool estadoSinpe = true;
            int idRechazo = 0;
            string rechazo = "";

            try
            {
                if (Nsolicitud <= 0)
                {
                    response.Code = -1;
                    response.Description = "No se ha indicado una solicitud válida.";
                    return response;
                }

                // 1) Validación disponibilidad SINPE
                var servicioDisponible = fxValidacionSinpe(CodEmpresa, Nsolicitud.ToString(), vUsuario);

                if (servicioDisponible.Code != 0 && servicioDisponible.Code != 1)
                {
                    estadoSinpe = false;
                    idRechazo = servicioDisponible.Code!.Value;

                    rechazo = _mKindo.fxTesConsultaMotivo(CodEmpresa, idRechazo).Result!;
                    response = DbHelper.ErrorResponse("N°: " + Nsolicitud + " - " + rechazo);

                    fxGuardaID_RespuestaSinpe(CodEmpresa, idRechazo, Nsolicitud.ToString());
                }
                else
                {
                    // 2) Envío a SINPE
                    respuesta = fxTesEnvioSinpeCreditoDirecto(CodEmpresa, Nsolicitud, vUsuario).Result;

                    if (respuesta != null && respuesta.MotivoError != 0)
                    {
                        estadoSinpe = false;
                        idRechazo = respuesta.MotivoError;

                        rechazo = _mKindo.fxTesConsultaMotivo(CodEmpresa, idRechazo).Result!;
                        response = DbHelper.ErrorResponse(rechazo);
                    }
                    else
                    {
                        estadoSinpe = true;
                    }
                }

                // 3) Guardar la respuesta en la transacción (tanto éxito como rechazo)
                datos.NumeroSolicitud = Nsolicitud;
                datos.FechaEmision = vfecha;
                datos.FechaTraslado = vfecha;
                datos.UsuarioGenera = vUsuario;
                datos.estadoSinpe = estadoSinpe;
                datos.IdMotivoRechazo = idRechazo;
                datos.CodigoReferencia = respuesta?.CodigoReferencia; // <- evita NRE si no hubo envío
                datos.DocumentoBase = doc_base.ToString();
                datos.contador = contador.ToString();

                if (!_mKindo.fxTesRespuestaSinpe(CodEmpresa, datos).Result)
                {
                    _mTesoreria.sbTesBitacoraEspecial(
                        CodEmpresa, Nsolicitud, "10",
                        "Se produjo un error al actualizar la transacción",
                        vUsuario);
                }

                // 4) Bitácora final: aquí ahora sí hay caminos donde estadoSinpe=false
                if (estadoSinpe)
                {
                    _mTesoreria.sbTesBitacoraEspecial(
                        CodEmpresa, Nsolicitud, "10",
                        "Emisión Transferencia Sinpe: Exitosa",
                        vUsuario);
                }
                else
                {
                    _mTesoreria.sbTesBitacoraEspecial(
                        CodEmpresa, Nsolicitud, "10",
                        $"Transferencia Sinpe rechazada: {rechazo} ",
                        vUsuario);
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


            
            var pinData = new ReqPINSending();

            try
            {
                var parametrosSinpe = _mKindo.GetUriEmpresa(CodEmpresa, vUsuario);
                // var response = new ResPINSending();
                var solicitud = _mKindo.fxTesConsultaSolicitud(CodEmpresa, Nsolicitud).Result;

                var context = CrearContexto(parametrosSinpe);

                var cod_referencia = _mKindo.IsValidTransactionNumber(CodEmpresa, solicitud?.CuentaOrigen!);

                pinData.HostId = context.HostId; 
                pinData.OperationId = context.OperationId;
                pinData.ClientIPAddress = context.ClientIPAddress;
                pinData.CultureCode = context.CultureCode;
                pinData.UserCode = context.UserCode;
                pinData.CoreIntegrationPoint = 1;
                pinData.CostCenter = 1;
                pinData.Transfer = new Galileo.Models.KindoSinpe.PINTransfer()
                {
                    ChannelReference = cod_referencia,
                    Amount = solicitud!.Monto,
                    CurrencyCode = _mKindo.GetCurrencyCodeDes(solicitud.Divisa!),
                    Description = solicitud.Detalle1 + solicitud.Detalle2 + solicitud.Detalle3 + solicitud.Detalle4,
                    OriginEntityIBAN = solicitud.CuentaOrigen!,
                    OriginCustomer = new Galileo.Models.KindoSinpe.OriginCustomer()
                    {
                        Id = _mKindo.MaskSinpeId(Convert.ToInt32(_mKindo.Inferir(solicitud.CedulaOrigen!.Replace("-", "")).Codigo), solicitud.CedulaOrigen!.Replace("-", ""))  ,
                        IdType = Convert.ToInt32(_mKindo.Inferir(solicitud.CedulaOrigen!.Replace("-", "")).Codigo),
                        Name = solicitud.NombreOrigen!,
                        IBAN = solicitud.CuentaOrigen!,
                        DebitIBAN = _mKindo.fxSinpe_Valida_MovimientosPermitidos(CodEmpresa, solicitud.CuentaOrigen!),
                        Email = solicitud.CorreoNotifica!.Trim()
                    },
                    DestinationCustomer = new Galileo.Models.KindoSinpe.DestinationCustomer()
                    {
                        Id = _mKindo.MaskSinpeId(Convert.ToInt32(_mKindo.Inferir(solicitud.Codigo!.Replace("-", "")).Codigo), solicitud.Codigo!.Replace("-", "")) ,
                        IdType = Convert.ToInt32(_mKindo.Inferir(solicitud.Codigo!.Replace("-", "")).Codigo),
                        Name = solicitud.Beneficiario!,
                        IBAN = solicitud.Cuenta!,
                        Email = solicitud.CorreoNotifica
                    },

                };
                pinData.CustomData = new List<Galileo.Models.KindoSinpe.CustomField>()
                    {
                        new Galileo.Models.KindoSinpe.CustomField()
                        {
                            Name = "Galileo",
                            Value = "CSG"
                        }
                    };

                var response = _sinpePIN.SendPIN(parametrosSinpe.Result?.UrlCGP_PIN!, pinData);
                if (response.IsSuccessful)
                {

                    fxGuardaID_RespuestaSinpe(CodEmpresa, response.Errors[0].Code, Nsolicitud.ToString());

                    var updateNSolicitud = _mKindo.RegistraDibitoCuenta(CodEmpresa, Nsolicitud, response).Result;

                    if (updateNSolicitud)
                    {

                        _mKindo.RegistraMovTransito(CodEmpresa, cod_referencia, context.UserCode!, response, solicitud);
                        return new ErrorDto<RespuestaRegistro>
                        {
                            Code = 0,
                            Description = "Ok",
                            Result = new RespuestaRegistro
                            {
                                MotivoError = 0,
                                CodigoReferencia = response.PINSendingResult!.SINPEReference
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
                    fxGuardaID_RespuestaSinpe(CodEmpresa, response.Errors[0].Code, Nsolicitud.ToString());

                    return new ErrorDto<RespuestaRegistro>
                    {
                        Code = -1,
                        Description = "Error al enviar PIN",
                        Result = new RespuestaRegistro
                        {
                            MotivoError = response.Errors![0].Code,
                            CodigoReferencia = "",
                            MotivoErrorInterno = response.Errors![0].Message
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

        private void fxGuardaID_RespuestaSinpe(int CodEmpresa, int codigo,string nsolicitud, string? referenciaSinpe = null)
        {
            const string query = @"UPDATE TES_TRANSACCIONES SET 
                            ID_RECHAZO = @codigo, REFERENCIA_SINPE = @referenciaSinpe 
                                WHERE NSOLICITUD = @nsolicitud";

            var parametros = new
            {
                codigo = codigo,
                referenciaSinpe = referenciaSinpe,
                nsolicitud = nsolicitud
            };

            DbHelper.ExecuteNonQuery(_portalDB, CodEmpresa, query, parametros);
        }



        #endregion
    }
}
