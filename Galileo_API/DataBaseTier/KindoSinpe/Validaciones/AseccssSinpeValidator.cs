using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Sinpe_CCD;
using Sinpe_PIN;
using Sinpe_TFT;
using Galileo_API.Controllers.WFCSinpe;
using Galileo.DataBaseTier;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace Galileo_API.DataBaseTier
{
    #pragma warning disable S3776
    #pragma warning disable S2077
    #pragma warning disable S1450
    #pragma warning disable S2325
    #pragma warning disable S1854
    #pragma warning disable S1172
    #pragma warning disable S1764
    #pragma warning disable S3981
    #pragma warning disable S1125
    #pragma warning disable S1192
    #pragma warning disable S1481
    #pragma warning disable S1656
    #pragma warning disable S1116
    #pragma warning disable S2583
    #pragma warning disable S2559
    #pragma warning disable S3241
    #pragma warning disable S2259
    // LEGACY: código histórico, no modificar sin plan de refactor
    public class AseccssSinpeValidator : IWfcSinpe
    {
        private readonly IConfiguration _config;
        private readonly SINPE_PINClient _srvSinpePin = new SINPE_PINClient();
        private readonly SINPE_TFTClient _srvSinpeTft = new SINPE_TFTClient();
        private readonly SINPE_CCDClient _srvSinpeCcd = new SINPE_CCDClient();
        private Galileo.Models.KindoSinpe.ParametrosSinpe _parametrosSinpe;
        private readonly FactElectronica.ServicioClient _srvFactElectronica = new FactElectronica.ServicioClient();

        private readonly MKindoServiceDb _mKindo;
        private readonly MTesoreria _mTesoreria;

        public AseccssSinpeValidator(IConfiguration config)
        {
            _config = config;
            _mTesoreria = new MTesoreria(_config);
            _mKindo = new MKindoServiceDb(_config);
            _parametrosSinpe = new Galileo.Models.KindoSinpe.ParametrosSinpe();

        }

        #region Validación de Solicitud SINPE

        /// <summary>
        /// Método principal para la validación de una solicitud SINPE
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
       
        public ErrorDto fxValidacionSinpe(int CodEmpresa, string solicitud, string usuario) 
        {
            ErrorDto response = new();
            _parametrosSinpe = _mKindo.GetUriEmpresa(CodEmpresa, usuario).Result;

            string fxValidacionSinpe = "";
            Galileo.Models.KindoSinpe.vInfoSinpe? info = new();
            try
            {
                var cntInfoSinpe = _mKindo.fxTesConsultaInfoSinpe(CodEmpresa, solicitud);
                if (cntInfoSinpe.Code == -1)
                {
                    response.Code = cntInfoSinpe.Code;
                    response.Description = cntInfoSinpe.Description;
                    return response;
                }
                info = cntInfoSinpe.Result;


                if (string.IsNullOrEmpty(info.Cedula) == false && string.IsNullOrEmpty(info.CuentaIBAN) == false)
                {
                    if (ConsultarIsPINEntity(info.CuentaIBAN).Result == true)
                    {
                        if (ConsultarIsServiceAvailable(usuario).Result == false)
                        {
                            response.Code = -1;
                            response.Description = solicitud.ToString() + " - " + "No se ha podido establecer comunicación con el servidor de forma adecuada, intente de nuevo o más tarde.";
                        }
                        else
                        {
                            ResAccountInfo LaInformacionDeLaCuentaPIN = new ResAccountInfo();
                            LaInformacionDeLaCuentaPIN.Errors = new Errores[0];
                            LaInformacionDeLaCuentaPIN = ConsultarAccountInfo(info.CuentaIBAN).Result;

                            if (LaInformacionDeLaCuentaPIN.IsSuccessful == false ||
                                LaInformacionDeLaCuentaPIN.Errors.Length > 0 ||
                                LaInformacionDeLaCuentaPIN.Account.State != "1")
                            {
                                if (LaInformacionDeLaCuentaPIN.Errors.Length > 0)
                                {
                                    response.Code = -1;
                                    response.Description = LaInformacionDeLaCuentaPIN.Errors[0].Message;
                                }
                                else
                                {
                                    response.Code = -1;
                                    response.Description = "Error de estado " + LaInformacionDeLaCuentaPIN.Account.State;
                                }

                                if (!string.IsNullOrEmpty(response.Description))
                                {
                                    response.Code = -1;
                                    response.Description = solicitud.ToString() + " - " + response.Description;
                                }
                            }
                            else
                            {
                                var cedulaFormateada = fxFormatoIdentificacionSinpe(info.Cedula.Trim(), info.tipoID).Result;
                                if (cedulaFormateada != LaInformacionDeLaCuentaPIN.Account.HolderId)
                                {
                                    response.Code = -1;
                                    response.Description = "La cédula obtenida no corresponde con la cédula de la cuenta.";
                                }
                                else
                                {
                                    response.Code = 0;
                                    response.Description = $@"La cuenta IBAN {info.CuentaIBAN} registrada a 
                                        nombre de {LaInformacionDeLaCuentaPIN.Account.Holder} cédula: {LaInformacionDeLaCuentaPIN.Account.HolderId} Tipo Id: {info.tipoID} Tipo de Moneda: @moneda";
                                }
                            }

                        }
                    }
                    else
                    {

                        response.Description = fxTesValidaRechazoCuentaIBAN(CodEmpresa, info.Cedula, info.CuentaIBAN, info.tipoID).Result;
                    }
                }
                else
                {
                    response.Code = -1;
                    response.Description = solicitud.ToString() + " - Solicitud SINPE incompleta.";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                _ = ex.Message; //bandera para visualizar error técnico. 
                response.Description = "Ocurrió un problema con la validación.";
            }
            return response;
        }

        private ErrorDto<bool> ConsultarIsPINEntity(string AccountNumber)
        {
            var Elresutado = new ResPINEntity();
            ErrorDto<bool> ErrorDto = new ErrorDto<bool>();
            ErrorDto.Result = true;
            var resultado = new ReqPINEntity();
            try
            {
                // Generar un nuevo GUID para OperationId
                Guid newGuid = Guid.NewGuid();
                ReqPINEntity PINEntity = new ReqPINEntity();
                // Crear instancia del objeto de solicitud ReqPINEntity
                var obtenerIpEquipoActual = _mKindo.fxObtenerIpEquipoActual(_parametrosSinpe.vHost);
                if (obtenerIpEquipoActual.Code == -1)
                {
                    return new ErrorDto<bool>
                    {
                        Code = -1,
                        Description = obtenerIpEquipoActual.Description,
                        Result = false
                    };
                }

                PINEntity.HostId = _parametrosSinpe.vHostPin;
                PINEntity.OperationId = newGuid;
                PINEntity.ClientIpAddress = obtenerIpEquipoActual.Result;
                PINEntity.AccountNumber = AccountNumber;

                ReqPINEntityBody pINEntityBody = new ReqPINEntityBody();
                pINEntityBody.ReqPINEntity = PINEntity;

                // Llamar al método IsPINEntity con los parámetros
                Elresutado = IsPINEntity(PINEntity).Result;
                ErrorDto.Result = Elresutado.PINEntity;
            }
            catch (Exception ex)
            {
                ErrorDto.Code = -1;
                ErrorDto.Description = ex.Message;
                ErrorDto.Result = false;
            }

            // Retornar el resultado del campo PINEntity
            return ErrorDto;
        }

        private ErrorDto<string> fxTesValidaRechazoCuentaIBAN(int CodEmpresa, string ced, string iban, int tipoID)
        {
            ErrorDto<string> response = new ErrorDto<string>();
            string? fxTesValidaRechazoCuentaIBAN = "";
            try
            {
                var bodySINPE = new ObtenerInformacionCuentaBody();
                var result = new InformacionCuenta();
                string cedulaValida = "";
                var body = _srvSinpeTft.ObtenerInformacionCuentaAsync;

                if (!string.IsNullOrEmpty(ced) && !string.IsNullOrEmpty(ced))
                {
                    // Formatear la cédula
                    var valCedula = fxFormatoIdentificacionSinpe(ced.Trim(), tipoID);
                    if (valCedula.Code == -1)
                    {
                        return new ErrorDto<string>
                        {
                            Code = -1,
                            Description = valCedula.Description,
                            Result = ""
                        };
                    }

                    cedulaValida = valCedula.Result;
                    bodySINPE.body.Identificacion = cedulaValida.Trim();
                    bodySINPE.body.IBAN = iban.Trim();
                    bodySINPE.rastro = fxCrearRastroSINPESIF_TFT().Result;

                    var infoResult = fxObtenerInformacionSINPE(bodySINPE);
                    if (infoResult.Code == -1)
                    {
                        response.Code = infoResult.Code;
                        response.Description = infoResult.Description;
                        response.Result = "";
                        return response;
                    }

                    if (result.MotivoRechazo > 0)
                    {
                        var valMotivo = fxTesConsultarMotivoRechazo(CodEmpresa, result.MotivoRechazo);
                        if (valMotivo.Code == -1)
                        {
                            response.Code = valMotivo.Code;
                            response.Description = valMotivo.Description;
                            response.Result = valMotivo.Result;
                            return response;
                        }
                        fxTesValidaRechazoCuentaIBAN = valMotivo.Result;
                    }

                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = "";
            }
            return response;
        }

        private ErrorDto<string> fxFormatoIdentificacionSinpe(string cedula, int tipoID)
        {
            ErrorDto<string> ErrorDto = new();
            string response = "";
            string formato = "";
            int idSINPE = -1;

            try
            {
                if (cedula.Contains("-"))
                {
                    ErrorDto.Code = 0;
                    ErrorDto.Description = "";
                    ErrorDto.Result = cedula;
                    return ErrorDto;
                }

                cedula = cedula.Trim();
                idSINPE = setCodigoSugefEstandar(tipoID).Result;

                switch (idSINPE)
                {
                    case 0: // Cédula Nacional
                        if (cedula.Length != 9)
                        {
                            ErrorDto.Code = -1;
                            ErrorDto.Description = "Formato de cédula inválido.";
                            ErrorDto.Result = cedula;
                            return ErrorDto;
                        }

                        formato = "0{0}-{1}-{2}";
                        response = string.Format(formato, cedula.Substring(0, 1), cedula.Substring(1, 4), cedula.Substring(5));
                        break;
                    case 1: // Dimex
                    case 5: // Didi
                        if (cedula.Length != 12)
                        {
                            ErrorDto.Code = -1;
                            ErrorDto.Description = "Formato de cédula inválido.";
                            ErrorDto.Result = cedula;
                            return ErrorDto;
                        }

                        formato = "{0}";
                        response = string.Format(formato, cedula);
                        break;
                    case 3: //Jurídica
                    case 4: //Institución autónoma
                        if (cedula.Length != 10)
                        {
                            ErrorDto.Code = -1;
                            ErrorDto.Description = "Formato de cédula inválido.";
                            ErrorDto.Result = cedula;
                            return ErrorDto;
                        }

                        formato = "{0}-{1}-{2}";
                        response = string.Format(formato, cedula.Substring(0, 1), cedula.Substring(1, 3), cedula.Substring(4));
                        break;
                    case 9: // Cédula de residencia.
                        if (cedula.Length != 13)
                        {
                            ErrorDto.Code = -1;
                            ErrorDto.Description = "Formato de cédula inválido.";
                            ErrorDto.Result = cedula;
                            return ErrorDto;
                        }

                        formato = "{0}-{1}-{2}";
                        response = string.Format(
                            formato,
                            cedula.Substring(0, 3),  // Primer bloque de 3 dígitos
                            cedula.Substring(3, 5),  // Segundo bloque de 5 dígitos
                            cedula.Substring(8, 5)   // Tercer bloque de 5 dígitos
                        );

                        break;
                    case 2: // Pasaporte 
                        if (cedula.Length != 9)
                        {
                            ErrorDto.Code = -1;
                            ErrorDto.Description = "Formato de cédula inválido.";
                            ErrorDto.Result = cedula;
                            return ErrorDto;
                        }

                        response = cedula;

                        break;
                    default:
                        ErrorDto.Code = -1;
                        ErrorDto.Description = "Tipo de identificación inválido.";
                        ErrorDto.Result = cedula;
                        break;
                }
                ErrorDto.Result = response;

            }
            catch (Exception ex)
            {
                ErrorDto.Code = -1;
                ErrorDto.Description = ex.Message;
                ErrorDto.Result = "";
            }
            return ErrorDto;
        }

        private ErrorDto<string> fxTesConsultarMotivoRechazo(int CodEmpresa, int idMotivo)
        {
            ErrorDto<string> response = new ErrorDto<string>();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = @"
                            SELECT CONCAT(descripcion, ' (', cod_motivo, ')') AS rechazo
                            FROM sinpe_motivos
                            WHERE cod_motivo = @IdMotivo;";

                response.Result = connection.QueryFirstOrDefault<string>(query, new { IdMotivo = idMotivo });
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = "Rechazo desconocido.";
            }
            return response;
        }

        public ErrorDto<bool> ConsultarIsServiceAvailable(string vUsuario)
        {
            ErrorDto<bool> ErrorDto = new ErrorDto<bool>();
            try
            {
                // Generar un nuevo GUID
                Guid newGuid = Guid.NewGuid();

                var context = new BaseRequest
                {
                    HostId = _parametrosSinpe.vHostPin,
                    OperationId = newGuid,
                    ClientIpAddress = _mKindo.fxObtenerIpEquipoActual(_parametrosSinpe.vHost).Result
                };

                BaseRequestBody contextBody = new BaseRequestBody();
                contextBody.BaseRequest = context;

                var elResultado = IsServiceAvailable(context, vUsuario);
                if (!elResultado.Result.ServiceAvailable)
                {
                    ErrorDto.Code = -1;
                    ErrorDto.Description = "Servicio PIN NO Disponible RESPUESTA: " + elResultado.Result.Errors;
                    ErrorDto.Result = false;
                }
                else
                {
                    ErrorDto.Result = elResultado.Result.ServiceAvailable;
                }

            }
            catch (Exception ex)
            {
                ErrorDto.Code = -1;
                ErrorDto.Description = ex.Message;
                ErrorDto.Result = false;
            }
            return ErrorDto;
        }

        private ErrorDto<ResAccountInfo> ConsultarAccountInfo(string iban)
        {
            ErrorDto<ResAccountInfo> ErrorDto = new ErrorDto<ResAccountInfo>();
            ErrorDto = new ErrorDto<ResAccountInfo>();
            try
            {
                Guid newGuid = Guid.NewGuid();
                var accountData = new ReqAccountInfo
                {
                    HostId = _parametrosSinpe.vHostPin,
                    OperationId = newGuid,
                    ClientIpAddress = _mKindo.fxObtenerIpEquipoActual(_parametrosSinpe.vHost).Result,
                    AccountNumber = iban
                };


                var laInformacionDeLaCuentaPIN = fxCrearAccountInfo(GetAccountInfo(accountData).Result);
                ErrorDto.Result = laInformacionDeLaCuentaPIN.Result;
            }
            catch (Exception ex)
            {
                ErrorDto.Code = -1;
                ErrorDto.Description = ex.Message;
                ErrorDto.Result = new ResAccountInfo();
            }

            return ErrorDto;
        }

        private ErrorDto<ResAccountInfo> fxCrearAccountInfo(ResAccountInfo accountInfo)
        {
            ErrorDto<ResAccountInfo> ErrorDto = new ErrorDto<ResAccountInfo>();
            ErrorDto = new ErrorDto<ResAccountInfo>();

            try
            {
                var response = new  Galileo.Models.KindoSinpe.ResponseData();
                response.IsSuccessful = accountInfo.IsSuccessful;
                response.OperationId = accountInfo.OperationId;
                response.Errors = new List<Errores>();
                response.Account = new Account();

                foreach (var error in accountInfo.Errors)
                {
                    response.Errors.Add(new Errores
                    {
                        Code = error.Code,
                        Message = error.Message
                    });
                }


                response.Account = accountInfo.Account;

                ErrorDto.Result = new ResAccountInfo
                {
                    Errors = response.Errors.ToArray(),
                    OperationId = response.OperationId,
                    IsSuccessful = response.IsSuccessful,
                    Account = response.Account 
                };
            }
            catch (Exception ex)
            {
                ErrorDto.Code = -1;
                ErrorDto.Description = ex.Message;
                ErrorDto.Result = new ResAccountInfo();
            }
            return ErrorDto;
        }

        private ErrorDto<ResAccountInfo> GetAccountInfo(ReqAccountInfo AccountData)
        {
            ErrorDto<ResAccountInfo> response = new ErrorDto<ResAccountInfo>();
            var body = new ReqAccountInfoBody();
            try
            {
                body.ReqAccountInfo = AccountData;
                body.rastro = fxCrearRastroSINPESIF_PIN().Result;

                response.Result = _srvSinpePin.GetAccountInfoAsync(body).Result;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new ResAccountInfo();

            }
            return response;
        }

        private ErrorDto<Sinpe_PIN.IModelosRastroSIF> fxCrearRastroSINPESIF_PIN()
        {
            ErrorDto<Sinpe_PIN.IModelosRastroSIF> response = new ErrorDto<Sinpe_PIN.IModelosRastroSIF>();
            response.Result = new Sinpe_PIN.IModelosRastroSIF();
            try
            {
                response.Result.IP = _parametrosSinpe.vIpHost;
                response.Result.Equipo = _parametrosSinpe.vHost;
                response.Result.Usuario = _parametrosSinpe.vUserCGP;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new Sinpe_PIN.IModelosRastroSIF();
            }
            return response;
        }

        private ErrorDto<Sinpe_PIN.IModelosRastroSIF> fxCrearRastroSINPESIF_PIN(string vUsuario)
        {
            ErrorDto<Sinpe_PIN.IModelosRastroSIF> response = new ErrorDto<Sinpe_PIN.IModelosRastroSIF>();
            response.Result = new Sinpe_PIN.IModelosRastroSIF();
            try
            {
                response.Result.IP = _parametrosSinpe.vIpHost;
                response.Result.Equipo = _parametrosSinpe.vHost;
                response.Result.Usuario = vUsuario;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new Sinpe_PIN.IModelosRastroSIF();
            }
            return response;
        }

        private ErrorDto<ResPINEntity> IsPINEntity(ReqPINEntity PINEntity)
        {
            ErrorDto<ResPINEntity> response = new ErrorDto<ResPINEntity>();
            var body = new ReqPINEntityBody();

            try
            {
                body.ReqPINEntity = PINEntity;
                _parametrosSinpe.vIpHost = PINEntity.ClientIpAddress;
                _parametrosSinpe.vUserCGP = _parametrosSinpe.vUserCGP;


                body.rastro = fxCrearRastroSINPESIF_PIN().Result;
                response.Result = new ResPINEntity();
                response.Result.PINEntity = body.ReqPINEntity.Equals(PINEntity);

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new ResPINEntity();
            }
            return response;
        }

        private ErrorDto<ResServiceAvailable> IsServiceAvailable(BaseRequest Context, string vUsuario)
        {
            ErrorDto<ResServiceAvailable> response = new ErrorDto<ResServiceAvailable>();
            var body = new BaseRequestBody();
            try
            {
                body.BaseRequest = Context;
                body.rastro = fxCrearRastroSINPESIF_PIN(vUsuario).Result;

                response.Result = _srvSinpePin.IsServiceAvailableAsync(body).Result;

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new ResServiceAvailable();
            }

            return response;
        }

        private ErrorDto<Sinpe_TFT.IModelosRastroSIF> fxCrearRastroSINPESIF_TFT()
        {
            var response = new ErrorDto<Sinpe_TFT.IModelosRastroSIF>();
            try
            {
                Sinpe_TFT.IModelosRastroSIF rastro = new Sinpe_TFT.IModelosRastroSIF();
                rastro.IP = _parametrosSinpe.vIpHost;
                rastro.Equipo = _parametrosSinpe.vHost;
                rastro.Usuario = _parametrosSinpe.vUserCGP;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        private ErrorDto<ObtenerInformacionCuentaResponseBody> fxObtenerInformacionSINPE(ObtenerInformacionCuentaBody body)
        {
            var response = new ErrorDto<ObtenerInformacionCuentaResponseBody>();
            try
            {
                response.Result = _srvSinpeTft.ObtenerInformacionCuentaAsync(body).Result;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        #endregion

        #region Emisión de SINPE Credito Directo

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
            _parametrosSinpe = _mKindo.GetUriEmpresa(CodEmpresa, vUsuario).Result;


            var respuesta = new Sinpe_CCD.RespuestaRegistro();
            var datos = new Galileo.Models.KindoSinpe.TesTransaccion();
            bool estadoSinpe = true;
            int idRechazo = 0;
            string rechazo = "";
            try
            {
                if (Nsolicitud > 0)
                {
                    if (fxTesConsultarServicioDisponible(CodEmpresa, vUsuario).Result == false)
                    {
                        estadoSinpe = false;
                        idRechazo = 83;
                        rechazo = fxTesConsultaMotivo(CodEmpresa, idRechazo).Result;
                    }
                    else
                    {
                        respuesta = fxTesEnvioSinpeCreditoDirecto(CodEmpresa, Nsolicitud, vUsuario).Result;

                        if (respuesta.MotivoError != 0)
                        {
                            estadoSinpe = false;
                            idRechazo = 83;
                            rechazo = fxTesConsultaMotivo(CodEmpresa, idRechazo).Result;
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

                    if (fxTesRespuestaSinpe(CodEmpresa, datos).Result == false)
                    {
                        _mTesoreria.sbTesBitacoraEspecial(CodEmpresa, Nsolicitud, "10", "Se produjo un error al actualizar la transacción", vUsuario);
                    }

                    if (estadoSinpe == true)
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

        private ErrorDto<bool> fxTesConsultarServicioDisponible(int CodEmpresa, string vUsuario)
        {
            var response = new ErrorDto<bool>
            {
                Code = 0,
                Description = "Ok",
                Result = true
            };

            var rastro = new Sinpe_CCD.IModelosRastroSIF();

            try
            {
                rastro = fxCrearRastroSINPESIF_CCD(vUsuario).Result; 

                if (_srvSinpeCcd.ServicioDisponibleAsync(rastro).Result == false)
                {
                    response.Result = false;
                }
            }
            catch (Exception)
            {
                response.Code = -1;
                response.Description = "Error al consultar el servicio disponible de SINPE.";
                response.Result = false;
            }
            return response;
        }

        private ErrorDto<Sinpe_CCD.IModelosRastroSIF> fxCrearRastroSINPESIF_CCD(string vUsuario)
        {
            ErrorDto<Sinpe_CCD.IModelosRastroSIF> response = new ErrorDto<Sinpe_CCD.IModelosRastroSIF>();
            response.Result = new Sinpe_CCD.IModelosRastroSIF();
            try
            {
                response.Result.IP = _parametrosSinpe.vIpHost;
                response.Result.Equipo = _parametrosSinpe.vHost;
                response.Result.Usuario = vUsuario;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new Sinpe_CCD.IModelosRastroSIF();
            }
            return response;
        }

        private ErrorDto<Sinpe_CCD.RespuestaRegistro> fxTesEnvioSinpeCreditoDirecto(int CodEmpresa, int Nsolicitud, string vUsuario)
        {
            //(Realiza el proceso de emisión de la transferencia SINPE)
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<Sinpe_CCD.RespuestaRegistro>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };

            var solicitud = new Galileo.Models.KindoSinpe.TesTransaccion();

            var response = new RegistrarDebitoCuentaResponseBody();
            var body = new RegistrarDebitoCuentaRequestBody();
            var rastro = new Sinpe_CCD.Rastro();
            var transaccion = new TransferenciaAS400();
            var responseDetail = new Sinpe_CCD.RespuestaRegistro();
            var bodyWCF = new RegistrarDebitoCuentaBody();

            string detalle = "";

            try
            {
                solicitud = _mKindo.fxTesConsultaSolicitud(CodEmpresa, Nsolicitud).Result;

                body.Rastro = new Sinpe_CCD.Rastro();
                detalle = (solicitud.Detalle1 + solicitud.Detalle2 + solicitud.Detalle3 + solicitud.Detalle4 + solicitud.Detalle5)
                    .Substring(0, Math.Min(255,
                        (solicitud.Detalle1 + solicitud.Detalle2 + solicitud.Detalle3 + solicitud.Detalle4 + solicitud.Detalle5).Length));

                if (detalle.Length < 15)
                {
                    detalle = (detalle.Replace("\r\n", "") + " Transferencia SINPE")
                             .Substring(0, Math.Min(255, (detalle.Replace("\r\n", "") + " Transferencia SINPE").Length));
                }

                body.Rastro.IP = _parametrosSinpe.vIpHost;
                body.Rastro.Usuario = _parametrosSinpe.vUserCGP;
                body.Rastro.Canal = _parametrosSinpe.vCanalCGP;

                transaccion.DatosTransaccion = new Sinpe_CCD.Transaccion();
                transaccion.DatosTransaccion.Moneda = solicitud.Divisa == "DOL"
                                                    ? Sinpe_CCD.E_Monedas.Dolares
                                                    : Sinpe_CCD.E_Monedas.Colones;

                // Monto
                if (solicitud.Divisa == "DOL")
                {
                    transaccion.DatosTransaccion.Monto = solicitud.tipoCambio > 0
                        ? solicitud.Monto / solicitud.tipoCambio
                        : 0;
                    transaccion.DatosTransaccion.Monto = Math.Round(transaccion.DatosTransaccion.Monto, 4); // Redondeo para dólares
                }
                else
                {
                    transaccion.DatosTransaccion.Monto = solicitud.Monto;
                }

                // Otros campos
                transaccion.DatosTransaccion.Descripcion = detalle.Replace("\r\n", "");
                transaccion.DatosTransaccion.Servicio = "CCD";
                transaccion.DatosTransaccion.PuntoIntegracion = 1;
                transaccion.DatosTransaccion.CentroCosto = 1;
                transaccion.DatosTransaccion.CodigoConcepto = 1;
                transaccion.DatosTransaccion.EntidadOrigen = 205;
                transaccion.DatosTransaccion.FirmaDigital = false;
                transaccion.DatosTransaccion.IDCorrelation = "1";
                transaccion.DatosTransaccion.eMAIL = solicitud.CorreoNotifica?.Trim();

                transaccion.ClienteOrigen = new ClienteAS400();
                transaccion.ClienteOrigen.Identificacion = solicitud.CedulaOrigen.Replace("-", "");
                transaccion.ClienteOrigen.Nombre = solicitud.NombreOrigen;
                transaccion.ClienteOrigen.IBAN = solicitud.CuentaOrigen;
                transaccion.ClienteOrigen.TipoCedula = solicitud.tipoCedOrigen;

                transaccion.ClienteDestino = new ClienteAS400();
                transaccion.ClienteDestino.Identificacion = solicitud.Codigo.Replace("-", "");
                transaccion.ClienteDestino.Nombre = solicitud.Beneficiario;
                transaccion.ClienteDestino.IBAN = solicitud.Cuenta;
                transaccion.ClienteDestino.TipoCedula = solicitud.tipoCedDestino;

                try
                {
                    body.Transacciones = new TransferenciaAS400[] { transaccion };

                    bodyWCF.body = body;
                    bodyWCF.rastro = fxCrearRastroSINPESIF_CCD(vUsuario).Result;

                    response = _srvSinpeCcd.RegistrarDebitoCuentaAsync(bodyWCF).Result;
                    responseDetail = response.RegistrarDebitoCuentaResult[0];
                    ;

                    resp.Result = responseDetail;
                }
                catch (Exception)
                {
                    responseDetail.MotivoError = 201;

                    resp.Result = responseDetail;
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

        #region Emisión de SINPE Tiempo Real

        /// <summary>
        /// Realiza el proceso de emisión de una transferencia SINPE Tiempo Real
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Nsolicitud"></param>
        /// <param name="vfecha"></param>
        /// <param name="vUsuario"></param>
        /// <param name="doc_base"></param>
        /// <param name="contador"></param>
        /// <returns></returns>
        [SuppressMessage(
        "Major Code Smell",
        "csharpsquid:S3776",
        Justification = "Legacy: método orquestador, refactor pendiente")]
        public ErrorDto fxTesEmisionSinpeTiempoReal(int CodEmpresa, int Nsolicitud, DateTime vfecha, string vUsuario, int doc_base, int contador) 
        {
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            _parametrosSinpe = _mKindo.GetUriEmpresa(CodEmpresa, vUsuario).Result;

            var solicitud = new Galileo.Models.KindoSinpe.TesTransaccion();
            var ElResultadoDeSendTransfer = new ResPINSending();
            var respuesta = new Sinpe_TFT.RespuestaRegistro();
            var datos = new Galileo.Models.KindoSinpe.TesTransaccion();
            bool estadoSinpe = true;
            int idRechazo = 0;
            string rechazo = null;

            try
            {
                if (Nsolicitud > 0)
                {
                    solicitud = _mKindo.fxTesConsultaSolicitud(CodEmpresa, Nsolicitud).Result;

                    if (ConsultarIsPINEntity(solicitud.Cuenta).Result == true) // NOSONAR
                    {
                        if (ConsultarIsServiceAvailable(vUsuario).Result == false) // NOSONAR
                        {
                            estadoSinpe = false;
                            idRechazo = 83;
                            rechazo = fxTesConsultaMotivo(CodEmpresa, idRechazo).Result; // NOSONAR
                        }
                        else
                        {
                            //Servicio disponible
                            ElResultadoDeSendTransfer = SendTransfer(CodEmpresa, solicitud).Result;

                            if (ElResultadoDeSendTransfer == null || ElResultadoDeSendTransfer.IsSuccessful == false)
                            {
                                estadoSinpe = false;

                                if (ElResultadoDeSendTransfer.Errors.Length >= 0)
                                {
                                    idRechazo = ElResultadoDeSendTransfer.Errors[0].Code;
                                    rechazo = ElResultadoDeSendTransfer.Errors[0].Message;
                                }
                                else
                                {
                                    idRechazo = -1;
                                    rechazo = fxTesConsultaMotivo(CodEmpresa, idRechazo).Result;
                                }
                            }
                            else
                            {
                                estadoSinpe = true;
                            }
                        }

                        switch (ElResultadoDeSendTransfer.PINSendingResult.State)
                        {
                            case 32:
                                estadoSinpe = true;
                                break;
                            case 128:
                                estadoSinpe = false;
                                rechazo = "Se ha rechazado la transacción. Favor intente mas tarde.";
                                break;
                            case 256:
                                estadoSinpe = false;
                                rechazo = "La confirmación la transacción esta en espera, favor consulte sus movimientos mas tarde.";
                                break;
                        }

                        datos.NumeroSolicitud = Nsolicitud;
                        datos.FechaEmision = vfecha;
                        datos.FechaTraslado = vfecha;
                        datos.UsuarioGenera = vUsuario;
                        datos.estadoSinpe = estadoSinpe;
                        datos.IdMotivoRechazo = idRechazo;
                        datos.CodigoReferencia = ElResultadoDeSendTransfer.PINSendingResult.SINPERefNumber;
                        datos.DocumentoBase = doc_base.ToString();
                        datos.contador = contador.ToString();

                        if (fxTesRespuestaSinpe(CodEmpresa, datos).Result == false)
                        {
                            _mTesoreria.sbTesBitacoraEspecial(CodEmpresa, Nsolicitud, "10", "Se produjo un error al actualizar la transacción", vUsuario);
                        }

                        //'Actualización nueva
                        if (estadoSinpe == true)
                        {
                            _mTesoreria.sbTesBitacoraEspecial(CodEmpresa, Nsolicitud, "10", "Emisión Transferencia Sinpe: Exitosa", vUsuario);

                            //Se agrega proceso para realizar el envio de notificaciones
                            EnviaNotificacionesCajas(CodEmpresa, datos.CodigoReferencia);
                        }
                        else
                        {
                            _mTesoreria.sbTesBitacoraEspecial(CodEmpresa, Nsolicitud, "10", $"Transferencia Sinpe rechazada: {rechazo} ", vUsuario);
                        }
                    }
                    else //Uso de canal CanalTFT
                    {
                        if (fxTesConsultarServicioDisponible(CodEmpresa, vUsuario).Result == false)
                        {
                            //'Se registra el error por servicio no disponible
                            estadoSinpe = false;
                            idRechazo = 83;
                            rechazo = fxTesConsultaMotivo(CodEmpresa, idRechazo).Result;
                            _mTesoreria.sbTesBitacoraEspecial(CodEmpresa, Nsolicitud, "10", $"Emisión Transferencia Sinpe: {rechazo}", vUsuario);
                        }
                        else
                        {
                            respuesta = fxTesEnvioSinpeTiempoReal(solicitud).Result;

                            if (respuesta.MotivoError != 0)
                            {
                                estadoSinpe = false;
                                idRechazo = 83;
                                rechazo = fxTesConsultaMotivo(CodEmpresa, idRechazo).Result;
                                _mTesoreria.sbTesBitacoraEspecial(CodEmpresa, Nsolicitud, "10", $"Transferencia Sinpe rechazada: {rechazo}", vUsuario);
                            }
                            else
                            {
                                estadoSinpe = true;
                                _mTesoreria.sbTesBitacoraEspecial(CodEmpresa, Nsolicitud, "10", $"Emisión Transferencia Sinpe: Exitosa", vUsuario);
                            }

                            datos.NumeroSolicitud = Nsolicitud;
                            datos.FechaEmision = vfecha;
                            datos.FechaTraslado = vfecha;
                            datos.UsuarioGenera = vUsuario;
                            datos.estadoSinpe = estadoSinpe;
                            datos.IdMotivoRechazo = idRechazo;
                            datos.CodigoReferencia = respuesta.CodigoReferencia;
                            datos.DocumentoBase = doc_base.ToString();
                            datos.contador = contador.ToString();

                            if (estadoSinpe)
                            {
                                EnviaNotificacionesCajas(CodEmpresa, datos.CodigoReferencia);
                            }

                            if (fxTesRespuestaSinpe(CodEmpresa, datos).Result == false)
                            {
                                _mTesoreria.sbTesBitacoraEspecial(CodEmpresa, Nsolicitud, "10", $"Se produjo un error al actualizar la transacción", vUsuario);
                            }
                        }
                    }
                }
                else
                {
                    response.Code = -1;
                    response.Description = "No se ha indicado una solicitud válida.";
                }
            }
            catch (Exception)
            {
                response.Code = -1;
                response.Description = "Error al procesar la solicitud de emisión de SINPE Tiempo Real.";
            }
            return response;
        }

        private ErrorDto<ResPINSending> SendTransfer(int CodEmpresa, Galileo.Models.KindoSinpe.TesTransaccion solicitud)
        {
            var ErrorDto = new ErrorDto<ResPINSending>
            {
                Code = 0,
                Description = "Ok",
                Result = new ResPINSending()
            };
            Sinpe_PIN.ReqPINSending TransferData = new Sinpe_PIN.ReqPINSending();
            Sinpe_PIN.ResPINSending ElResultadoDeSendTransfer = new Sinpe_PIN.ResPINSending();

            string detalle = "";
            try
            {
                detalle = (solicitud.Detalle1 + solicitud.Detalle2 + solicitud.Detalle3 + solicitud.Detalle4 + solicitud.Detalle5)
                   .Substring(0, Math.Min(255,
                       (solicitud.Detalle1 + solicitud.Detalle2 + solicitud.Detalle3 + solicitud.Detalle4 + solicitud.Detalle5).Length));


                if (detalle.Length < 15)
                {
                    detalle = (detalle.Replace("\r\n", "") + " Transferencia SINPE")
                             .Substring(0, Math.Min(255, (detalle.Replace("\r\n", "") + " Transferencia SINPE").Length));
                }

                if (solicitud.Divisa == "DOL")
                {
                    solicitud.Monto = solicitud.tipoCambio > 0
                         ? solicitud.Monto / solicitud.tipoCambio
                         : 0;
                    solicitud.Monto = Math.Round(solicitud.Monto, 4); // Redondeo para dólares
                    solicitud.Divisa = "USD";
                }
                else
                {
                    solicitud.Divisa = "CRC";
                }

                Guid newGuid = Guid.NewGuid();
                TransferData.HostId = _parametrosSinpe.vHostPin;
                TransferData.OperationId = newGuid;
                TransferData.ClientIpAddress = _mKindo.fxObtenerIpEquipoActual(_parametrosSinpe.vHost).Result;
                TransferData.CoreIntegrationPoint = 1; // Punto de integración
                TransferData.CostCenter = 1; // Centro de costo
                TransferData.UserCode = _parametrosSinpe.vUserCGP;

                TransferData.Transfer = new PINTransfer();
                TransferData.Transfer.ChannelRefNumber = ConsultarConsecutivoSinpe(CodEmpresa).Result;
                TransferData.Transfer.Amount = solicitud.Monto;
                TransferData.Transfer.CurrencyCode = solicitud.Divisa;
                TransferData.Transfer.Description = detalle;
                TransferData.Transfer.OriginEntityIBAN = "";
                TransferData.Transfer.OriginCustomer = new Sinpe_PIN.OriginCustomer();
                TransferData.Transfer.OriginCustomer.Id = fxFormatoIdentificacionSinpe(solicitud.CedulaOrigen.Trim(), solicitud.tipoCedOrigen.GetHashCode()).Result;
                TransferData.Transfer.OriginCustomer.IdType = PIN_OBTENER_TIPO_IDENTIFICACION(CodEmpresa, solicitud.tipoCedOrigen.GetHashCode()).Result;
                TransferData.Transfer.OriginCustomer.Name = solicitud.NombreOrigen;
                TransferData.Transfer.OriginCustomer.IBAN = solicitud.CuentaOrigen;
                TransferData.Transfer.OriginCustomer.Email = "";
                TransferData.Transfer.OriginCustomer.DebitIBAN = true;
                TransferData.Transfer.DestinationCustomer = new DestinationCustomer();
                TransferData.Transfer.DestinationCustomer.Id = fxFormatoIdentificacionSinpe(solicitud.Codigo.Trim(), solicitud.tipoCedDestino.GetHashCode()).Result;
                TransferData.Transfer.DestinationCustomer.IdType = PIN_OBTENER_TIPO_IDENTIFICACION(CodEmpresa, solicitud.tipoCedDestino.GetHashCode()).Result;
                TransferData.Transfer.DestinationCustomer.Name = solicitud.Beneficiario;
                TransferData.Transfer.DestinationCustomer.IBAN = solicitud.Cuenta;
                TransferData.Transfer.DestinationCustomer.Email = "";

                ElResultadoDeSendTransfer = SendTransfer(CodEmpresa, TransferData, solicitud.UsuarioGenera).Result;

                ErrorDto.Result = ElResultadoDeSendTransfer;
            }
            catch (Exception)
            {
                ErrorDto.Code = -1;
                ErrorDto.Description = "Error al procesar los detalles de la solicitud.";
                ErrorDto.Result = new ResPINSending();
            }
            return ErrorDto;

        }

        private ErrorDto<ResPINSending> SendTransfer(int CodEmpresa, ReqPINSending TransferData, string vUsuario)
        {
            var res = new ErrorDto<ResPINSending>
            {
                Code = 0,
                Description = "Ok",
                Result = new ResPINSending()
            };
            try
            {
                var response = new ResPINSending();
                var body = new ReqPINSendingBody();

                body.ReqPINSending = TransferData;
                body.rastro = fxCrearRastroSINPESIF_PIN(vUsuario).Result;

                response = _srvSinpePin.SendTransferAsync(body).Result;

                res.Result = response;
            }
            catch (Exception)
            {
                res.Code = -1;
                res.Description = "Error al enviar la transferencia SINPE Tiempo Real.";
                res.Result = new ResPINSending();
            }
            return res;
        }

        private ErrorDto<int> PIN_OBTENER_TIPO_IDENTIFICACION(int CodEmpresa, int CODIGO_SUGEF)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<int>
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = @"
                            SELECT CODIGO_PIN
                            FROM AFI_TIPOS_IDS
                            WHERE CODIGO_SUGEF = @CodigoSugef;";

                response.Result = connection.QueryFirstOrDefault<int>(query, new { CodigoSugef = CODIGO_SUGEF });
            }
            catch (Exception)
            {
                response.Code = -1;
                response.Description = "Error al obtener el tipo de identificación PIN.";
                response.Result = 0;
            }

            return response;
        }

        private ErrorDto<string> ConsultarConsecutivoSinpe(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<string>
            {
                Code = 0,
                Description = "Ok",
                Result = ""
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                response.Result = connection.QueryFirstOrDefault<string>(
                           "spPSL_ConsultarConsecutivoSinpe",
                           new { CANAL = _parametrosSinpe.vCanalCGP },
                           commandType: CommandType.StoredProcedure
                       );
            }
            catch (Exception)
            {
                response.Code = -1;
                response.Description = "Error al consultar el consecutivo de SINPE.";
                response.Result = null;
            }

            return response;
        }

        private ErrorDto<Sinpe_TFT.RespuestaRegistro> fxTesEnvioSinpeTiempoReal(Galileo.Models.KindoSinpe.TesTransaccion solicitud)
        {
            var resp = new ErrorDto<Sinpe_TFT.RespuestaRegistro>
            {
                Code = 0,
                Description = "Ok",
                Result = new Sinpe_TFT.RespuestaRegistro()
            };

            var response = new EnvioDirectoDebitoCuentaResponseBody();
            var body = new EnvioDirectoDebitoCuentaRequestBody();
            var transaccion = new Transferencia();
            var responseDetail = new RespuestaTransaccion();
            var bodyWCF = new RegistrarDirectoDebitoCuentaBody();

            string detalle = "";

            try
            {
                body.Rastro = new Sinpe_TFT.Rastro();
                detalle = (solicitud.Detalle1 + solicitud.Detalle2 + solicitud.Detalle3 + solicitud.Detalle4 + solicitud.Detalle5)
                    .Substring(0, Math.Min(255,
                        (solicitud.Detalle1 + solicitud.Detalle2 + solicitud.Detalle3 + solicitud.Detalle4 + solicitud.Detalle5).Length));

                if (detalle.Length < 15)
                {
                    detalle = (detalle.Replace("\r\n", "") + " Transferencia SINPE")
                             .Substring(0, Math.Min(255, (detalle.Replace("\r\n", "") + " Transferencia SINPE").Length));
                }

                body.Rastro.IP = _parametrosSinpe.vIpHost;
                body.Rastro.Usuario = _parametrosSinpe.vUserCGP;
                body.Rastro.Canal = _parametrosSinpe.vCanalCGP;

                transaccion.DatosDebito = new Sinpe_TFT.Transaccion();
                transaccion.DatosDebito.Moneda = solicitud.Divisa == "DOL"
                                                    ? Sinpe_TFT.E_Monedas.Dolares
                                                    : Sinpe_TFT.E_Monedas.Colones;

                // Monto
                if (solicitud.Divisa == "DOL")
                {
                    transaccion.DatosDebito.Monto = solicitud.tipoCambio > 0
                        ? solicitud.Monto / solicitud.tipoCambio
                        : 0;
                    transaccion.DatosDebito.Monto = Math.Round(transaccion.DatosDebito.Monto, 4); // Redondeo para dólares
                }
                else
                {
                    transaccion.DatosDebito.Monto = solicitud.Monto;
                }

                // Otros campos
                transaccion.DatosDebito.Descripcion = detalle.Replace("\r\n", "");
                transaccion.DatosDebito.Servicio = "TFT";
                transaccion.DatosDebito.PuntoIntegracion = 1;
                transaccion.DatosDebito.CentroCosto = 1;
                transaccion.DatosDebito.CodigoConcepto = 1;
                transaccion.DatosDebito.EntidadOrigen = 205;
                transaccion.DatosDebito.FirmaDigital = false;
                transaccion.DatosDebito.IDCorrelation = "1";
                transaccion.DatosDebito.eMAIL = solicitud.CorreoNotifica.Trim();

                transaccion.ClienteOrigen = new Cliente
                {
                    Identificacion = fxFormatoIdentificacionSinpe(solicitud.CedulaOrigen.Trim(), solicitud.tipoCedOrigen.GetHashCode()).Result,
                    Nombre = solicitud.NombreOrigen,
                    IBAN = solicitud.CuentaOrigen,
                };

                transaccion.ClienteDestino = new Cliente
                {
                    Identificacion = fxFormatoIdentificacionSinpe(solicitud.Codigo.Trim(), solicitud.tipoCedDestino.GetHashCode()).Result,
                    Nombre = solicitud.Beneficiario,
                    IBAN = solicitud.Cuenta,
                };

                try
                {
                    body.Transacciones = new Transferencia[] { transaccion };
                    bodyWCF.body = body;
                    bodyWCF.rastro = fxCrearRastroSINPESIF_TFT().Result;
                    response = _srvSinpeTft.EnvioDirectoDebitoCuentaAsync(bodyWCF).Result;
                    responseDetail = response.EnvioDirectoDebitoCuentaResult[0];
                    resp.Result = responseDetail;
                }
                catch (Exception)
                {
                    responseDetail.MotivoError = 201;
                    resp.Result = responseDetail;
                }


            }
            catch (Exception)
            {
                resp.Code = -1;
                resp.Description = "Error al procesar la solicitud de emisión de SINPE Tiempo Real.";
                resp.Result = new Sinpe_TFT.RespuestaRegistro();
            }

            return resp;
        }

        #endregion

        #region Factura Electrónica
        /// <summary>
        /// Este se utiliza para el modulo de cajas al pagar IVA. 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pCedula"></param>
        /// <param name="pNumeroDocumento"></param>
        /// <param name="pTipoDoc"></param>
        /// <param name="pTipoDocEletronico"></param>
        /// <param name="pNotas"></param>
        /// <param name="pTipoTramite"></param>
        /// <returns></returns>
        public ErrorDto<bool> GenerarFacturacionElectronica(int CodEmpresa,
            string pCedula,
            string pNumeroDocumento, string pTipoDoc,
            byte pTipoDocEletronico, string pNotas, string pTipoTramite)
        {
            var response = new ErrorDto<bool>();
            _parametrosSinpe = _mKindo.GetUriEmpresa(CodEmpresa, "ProGrx").Result;

            try
            {
                bool Respuesta = false;
                var Facturacion = new FactElectronica.FE_Facturacion();

                Facturacion.Encabezado = ObtieneEncabezado(CodEmpresa, pCedula, pNumeroDocumento, pTipoDoc, pTipoDocEletronico).Result;
                Facturacion.Detalles = ObtieneDetalles(CodEmpresa, pNumeroDocumento, pTipoDoc, pTipoTramite).Result;
                Facturacion.Parametros = GenerarParametros(pNumeroDocumento, pTipoDoc, pNotas).Result;

                response.Result = _srvFactElectronica.ProcesarDocumentoAsync(Facturacion).Result;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = false;
            }
            return response;
        }

        private ErrorDto<FactElectronica.FE_JsonEncabezado> ObtieneEncabezado(int CodEmpresa, string pCedula,
            string pNumComprobante, string pTipoDoc,
            byte pTipoDocEletronico)
        {
            var response = new ErrorDto<FactElectronica.FE_JsonEncabezado>();
            try
            {
                var EncabezadoServicio = new FactElectronica.FE_JsonEncabezado();
                var parametrosEncabezado = new Galileo.Models.KindoSinpe.FE_ParametrosEncabezado();

                EncabezadoServicio.FechaFactura = DateTime.Now;
                EncabezadoServicio.TipoDoc = pTipoDocEletronico;
                EncabezadoServicio.SituacionEnvio = (byte)Galileo.Models.KindoSinpe.E_SituacionEnvio.Normal;

                parametrosEncabezado = ObtieneParametrosEncabezado(CodEmpresa).Result;
                EncabezadoServicio.CantDeci = parametrosEncabezado.CantDeci; //Cantidad de decimales
                EncabezadoServicio.Sucursal = parametrosEncabezado.Sucursal; //Sucursal asignada por GTI
                EncabezadoServicio.CodigoActividad = parametrosEncabezado.CodigoActividad; //Codigo de actividad de la empresa
                EncabezadoServicio.Terminal = parametrosEncabezado.Terminal; //Terminal asignada por GTI

                EncabezadoServicio.Moneda = (short)Galileo.Models.KindoSinpe.E_Moneda.Colones;
                EncabezadoServicio.MedioPago = ObtieneMediosPago(CodEmpresa, pNumComprobante, pTipoDoc).Result;
                EncabezadoServicio.CondicionVenta = (byte)Galileo.Models.KindoSinpe.E_CondicionVenta.Contado;

                if (pTipoDocEletronico == 1)
                {
                    EncabezadoServicio.Receptor = receptorValidado(CodEmpresa, pCedula).Result;
                }
                else
                {
                    EncabezadoServicio.Receptor = null;
                }

                if (EncabezadoServicio.Receptor == null && pTipoDocEletronico == 1)
                {
                    EncabezadoServicio = null;
                }

                response.Result = EncabezadoServicio;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        private ErrorDto<Galileo.Models.KindoSinpe.FE_ParametrosEncabezado> ObtieneParametrosEncabezado(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto<Galileo.Models.KindoSinpe.FE_ParametrosEncabezado> response = new ErrorDto<Galileo.Models.KindoSinpe.FE_ParametrosEncabezado>();
            response.Result = new Galileo.Models.KindoSinpe.FE_ParametrosEncabezado();
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = $@"exec spFE_ObtieneParametrosEncabezado ";
                response.Result = connection.Query<Galileo.Models.KindoSinpe.FE_ParametrosEncabezado>(query).FirstOrDefault();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new Galileo.Models.KindoSinpe.FE_ParametrosEncabezado();
            }

            return response;
        }

        private ErrorDto<short[]> ObtieneMediosPago(int CodEmpresa, string pNumComprobante, string pTipoDocumento)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto<short[]> response = new ErrorDto<short[]>();
            response.Result = new short[0];
            try
            {
                //Info de pruebas
                using var connection = new SqlConnection(stringConn);
                response.Result = connection.Query<short>(
                           "spFE_ObtieneMedioPagos",
                           new { pNumComprobante, pTipoDocumento },
                           commandType: CommandType.StoredProcedure
                       ).ToArray();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new short[0];
            }

            return response;
        }

        private ErrorDto<FactElectronica.FE_JsonReceptor> receptorValidado(int CodEmpresa, string pCedula)
        {
            var response = new ErrorDto<FactElectronica.FE_JsonReceptor>();
            try
            {
                var receptorValServicio = new FactElectronica.FE_JsonReceptor();
                var receptorVal = new Galileo.Models.KindoSinpe.FE_Receptor();

                receptorVal = receptorValidado(CodEmpresa, pCedula, null).Result;
                receptorValServicio.Nombre = receptorVal.Nombre;
                receptorValServicio.Correo = receptorVal.Correo;
                receptorValServicio.TipoIdent = receptorVal.TipoIdent;
                receptorValServicio.Identificacion = receptorVal.Identificacion;

                response.Result = receptorValServicio;

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        private ErrorDto<Galileo.Models.KindoSinpe.FE_Receptor> receptorValidado(int CodEmpresa, string pCedula, string? n)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<Galileo.Models.KindoSinpe.FE_Receptor>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                response.Result = connection.QueryFirstOrDefault<Galileo.Models.KindoSinpe.FE_Receptor>(
                   "spFE_ObtieneEncabezado",
                   new { pCedula },
                   commandType: CommandType.StoredProcedure
                   );
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        private ErrorDto<FactElectronica.FE_JsonLineas[]> ObtieneDetalles(int CodEmpresa, string pNumComprobante, string pTipoDoc, string pTipoTramite)
        {
            var response = new ErrorDto<FactElectronica.FE_JsonLineas[]>();
            try
            {
                var ListaLineaServicio = new FactElectronica.FE_JsonLineas[0];
                var LineaServicio = new FactElectronica.FE_JsonLineas();
                var ListaLinea = new List<Galileo.Models.KindoSinpe.FE_Detalles>();
                short Linea = 0;

                ListaLinea = ObtieneDetalles(CodEmpresa, pNumComprobante, pTipoDoc, pTipoTramite, null).Result;


            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;

            }
            return response;
        }

        private ErrorDto<List<Galileo.Models.KindoSinpe.FE_Detalles>> ObtieneDetalles(int CodEmpresa, string pNumComprobante, string pTipoDocumento, string pTipoTramite, string? n)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<Galileo.Models.KindoSinpe.FE_Detalles>>();
            try
            {
                using var connection = new SqlConnection(stringConn);
                response.Result = connection.Query<Galileo.Models.KindoSinpe.FE_Detalles>(
                            "spFE_ObtieneDetalle",
                            new { pNumComprobante, pTipoDocumento, pTipoTramite },
                            commandType: System.Data.CommandType.StoredProcedure
                        ).ToList();

                if (response.Result != null)
                {
                    foreach (var item in response.Result)
                    {
                        if (item.PrecioUnitario != "0")
                        {
                            item.Descuentos = ObtieneDescuentos(CodEmpresa, pNumComprobante, pTipoDocumento).Result;

                            item.Impuestos = ObtieneImpuestos(CodEmpresa, pNumComprobante, pTipoDocumento, pTipoTramite, item.NumeroLinea).Result;
                        }
                    }
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

        private ErrorDto<List<FactElectronica.FE_JsonDescuentos>> ObtieneDescuentos(int CodEmpresa, string pNumComprobante, string pTipoDocumento)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<FactElectronica.FE_JsonDescuentos>>();
            var ListaDescuentoServicio = new List<FactElectronica.FE_JsonDescuentos>();
            var DescuentoServicio = new FactElectronica.FE_JsonDescuentos();
            try
            {

                //'//-------------------------------------------- //
                //'   Los creditos Jaules no tienen descuento
                //'//-------------------------------------------- //
                using var connection = new SqlConnection(stringConn);
                response.Result = connection.Query<FactElectronica.FE_JsonDescuentos>(
                         "dbo.spFE_ObtieneDescuentos",
                         new { pNumComprobante, pTipoDocumento },
                         commandType: CommandType.StoredProcedure
                     ).ToList();

                response.Result = ListaDescuentoServicio;

            }
            catch (Exception ex)
            {
                DescuentoServicio.MontoDescuento = 0;
                DescuentoServicio.DetalleDescuento = "";
                ListaDescuentoServicio.Add(DescuentoServicio);

                response.Code = -1;
                response.Description = ex.Message;
                response.Result = ListaDescuentoServicio;
            }
            return response;
        }

        private ErrorDto<List<FactElectronica.FE_JsonImpuestos>> ObtieneImpuestos(int CodEmpresa, string pNumComprobante, string pTipoDocumento, string pTipoTramite, short pNumeroLinea)
        {
            var response = new ErrorDto<List<FactElectronica.FE_JsonImpuestos>>();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            try
            {
                using var connection = new SqlConnection(stringConn);
                response.Result = connection.Query<FactElectronica.FE_JsonImpuestos>(
                        "dbo.spFE_ObtieneImpuestos",
                        new { pNumComprobante, pTipoDocumento, pTipoTramite, pNumeroLinea },
                        commandType: CommandType.StoredProcedure
                    ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        private ErrorDto<FactElectronica.FE_ParametrosSistemas> GenerarParametros(string pNumeroComprobante, string pTipoDoc, string pNotas)
        {
            var response = new ErrorDto<FactElectronica.FE_ParametrosSistemas>();
            try
            {
                if (pNotas == "" && pNotas == null)
                {
                    pNotas = "Recibo de Pago ASECCSS";
                }

                response.Result.TipoDocumento = pTipoDoc;
                response.Result.SistemaSiglas = "ProGrx ASECCSS";
                response.Result.NumeroDeComprobante = pNumeroComprobante;
                response.Result.Notas = pNotas;

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        #endregion

        #region General/Compartido


        /// <summary>
        /// Consulta el motivo de rechazo de una transacción SINPE.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="idRechazo"></param>
        /// <returns></returns>
        private ErrorDto<string> fxTesConsultaMotivo(int CodEmpresa, int idRechazo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<string>
            {
                Code = 0,
                Description = "Ok",
                Result = ""
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = $@"SELECT DESCRIPCION FROM SINPE_MOTIVOS where COD_MOTIVO = @rechazo ";
                response.Result = connection.Query<string>(query, new { rechazo = idRechazo }).FirstOrDefault();
            }
            catch (Exception)
            {
                response.Code = -1;
                response.Description = "Error al consultar el motivo de rechazo.";
                response.Result = "Motivo desconocido.";
            }
            return response;
        }

        /// <summary>
        /// Actualiza el estado de una transacción SINPE a "Sinpe" o "Rechazada".
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        private ErrorDto<bool> fxTesRespuestaSinpe(int CodEmpresa, Galileo.Models.KindoSinpe.TesTransaccion datos)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<bool>
            {
                Code = 0,
                Description = "Ok",
                Result = false
            };

            string nDocumento = "";

            try
            {
                using var connection = new SqlConnection(stringConn);
                if (datos.IdMotivoRechazo != 201)
                {
                    nDocumento = (datos.DocumentoBase + "-" + datos.contador.ToString())
                                 .Substring(0, Math.Min(30, (datos.DocumentoBase + "-" + datos.contador.ToString()).Length));


                    var query = $@"Update Tes_Transacciones Set Estado='I',Fecha_Emision= @FECHAEMITE, 
                                    Ubicacion_Actual='T',FECHA_TRASLADO= @FECHATRASLADO, User_Genera = @USUARIO, 
                                    Estado_Sinpe= @ESTADOSINPE, Id_Rechazo= @RECHAZO, Referencia_Sinpe= @REFERENCIA, 
                                    Documento_Base = @DOCBASE, NDocumento = CASE WHEN USUARIO_AUTORIZA_ESPECIAL IS 
                                    NULL THEN @NDOC ELSE @REFERENCIA END where NSolicitud= @SOLICITUD";
                    var result = connection.Execute(query, new
                    {
                        FECHAEMITE = datos.FechaEmision,
                        FECHATRASLADO = datos.FechaTraslado,
                        USUARIO = datos.UsuarioGenera,
                        ESTADOSINPE = datos.estadoSinpe,
                        RECHAZO = datos.IdMotivoRechazo,
                        REFERENCIA = datos.CodigoReferencia,
                        DOCBASE = datos.DocumentoBase,
                        NDOC = nDocumento,
                        SOLICITUD = datos.NumeroSolicitud
                    });
                    if (result > 0)
                    {
                        response.Result = true;
                    }
                }
                else
                {
                    response.Result = false;
                }
            }
            catch (Exception)
            {
                response.Code = -1;
                response.Description = "Error al consultar el motivo de rechazo.";
                response.Result = false;
            }
            return response;
        }

        /// <summary>
        /// Establece el código SUGEF estándar basado en el tipo de identificación.
        /// </summary>
        /// <param name="TipoId"></param>
        /// <returns></returns>
        private ErrorDto<int> setCodigoSugefEstandar(int TipoId)
        {
            var response = new ErrorDto<int>();
            try
            {
                switch (TipoId)
                {
                    case 1: // Cédula Nacional
                        response.Result = 3;
                        break;
                    case 3: // Dimex
                        response.Result = 1;
                        break;
                    case 4: // Jurídica
                        response.Result = 5;
                        break;
                    case 5: // Institución autónoma
                        response.Result = 4;
                        break;
                    default:
                        response.Result = TipoId;
                        break;
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "Error al validar la información del código SUGEF.";
                response.Result = TipoId;
            }
            return response;
        }

        /// <summary>
        /// Envia notificaciones a las cajas para una transacción específica.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodigoReferencia"></param>
        /// <returns></returns>
        private ErrorDto<bool> EnviaNotificacionesCajas(int CodEmpresa, string CodigoReferencia)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<bool>
            {
                Code = 0,
                Description = "Ok",
                Result = false
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                var result = connection.Execute(
                             "sp_Sinpe_Notificaciones_Cajas",
                             new { CODIGO_REFERENCIA = CodigoReferencia },
                             commandType: CommandType.StoredProcedure
                         );
                if (result > 0)
                {
                    resp.Result = true;
                }
                else
                {
                    resp.Code = -1;
                    resp.Description = "No se enviaron notificaciones a las cajas.";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = $"Error al enviar notificaciones a las cajas: {ex.Message}";
            }
            return resp;
        }

        #endregion
    }
   

}
